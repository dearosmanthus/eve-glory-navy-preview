using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EveOPreview.Services.Implementation
{
	/// <summary>
	/// One process-lifetime worker isolates foreign input queues from both the UI
	/// and mouse-hook threads. A stuck native call keeps this worker busy; it must
	/// never cause replacement workers or leave the caller waiting indefinitely.
	/// </summary>
	internal static class ForegroundWindowActivator
	{
		private static readonly Lazy<ForegroundActivationWorker> Worker = new Lazy<ForegroundActivationWorker>(
			() => new ForegroundActivationWorker(ForegroundActivationNativeCalls.CreateWindows(), 400, 320));

		internal static bool TryActivate(IntPtr target, out string reason)
		{
			try { return Worker.Value.TryActivate(target, out reason); }
			catch (SystemException exception)
			{
				reason = "activation-worker-unavailable: " + exception.GetType().Name;
				return false;
			}
		}
	}

	/// <summary>Native boundary, also injectable by the isolated regression harness.</summary>
	internal sealed class ForegroundActivationNativeCalls
	{
		internal readonly Func<IntPtr, bool> IsWindow;
		internal readonly Func<IntPtr> GetForegroundWindow;
		internal readonly Func<IntPtr, uint> GetWindowThreadId;
		internal readonly Func<uint> GetCurrentThreadId;
		internal readonly Action EnsureMessageQueue;
		internal readonly Func<IntPtr, uint, bool> PingWindow;
		internal readonly Func<IntPtr, bool> SetForegroundWindow;
		internal readonly Func<uint, uint, bool, bool> AttachThreadInput;
		internal readonly Func<IntPtr, bool> IsOwnProcessWindow;

		internal ForegroundActivationNativeCalls(Func<IntPtr, bool> isWindow, Func<IntPtr> getForegroundWindow,
			Func<IntPtr, uint> getWindowThreadId, Func<uint> getCurrentThreadId, Action ensureMessageQueue,
			Func<IntPtr, uint, bool> pingWindow, Func<IntPtr, bool> setForegroundWindow,
			Func<uint, uint, bool, bool> attachThreadInput, Func<IntPtr, bool> isOwnProcessWindow)
		{
			this.IsWindow = isWindow;
			this.GetForegroundWindow = getForegroundWindow;
			this.GetWindowThreadId = getWindowThreadId;
			this.GetCurrentThreadId = getCurrentThreadId;
			this.EnsureMessageQueue = ensureMessageQueue;
			this.PingWindow = pingWindow;
			this.SetForegroundWindow = setForegroundWindow;
			this.AttachThreadInput = attachThreadInput;
			this.IsOwnProcessWindow = isOwnProcessWindow;
		}

		internal static ForegroundActivationNativeCalls CreateWindows() => new ForegroundActivationNativeCalls(
			ForegroundActivationNativeMethods.IsWindow, ForegroundActivationNativeMethods.GetForegroundWindow,
			window => ForegroundActivationNativeMethods.GetWindowThreadProcessId(window, out uint ignored),
			ForegroundActivationNativeMethods.GetCurrentThreadId,
			() => ForegroundActivationNativeMethods.PeekMessage(out ForegroundActivationNativeMethods.NativeMessage ignored, IntPtr.Zero, 0, 0, 0),
			(window, timeout) => ForegroundActivationNativeMethods.SendMessageTimeout(window, 0, UIntPtr.Zero, IntPtr.Zero,
				0x0001 | 0x0002 | 0x0020, timeout, out UIntPtr ignored) != IntPtr.Zero,
			ForegroundActivationNativeMethods.SetForegroundWindow, ForegroundActivationNativeMethods.AttachThreadInput,
			window => { ForegroundActivationNativeMethods.GetWindowThreadProcessId(window, out uint process); return process == (uint)Environment.ProcessId; });
	}

	internal sealed class ForegroundActivationWorker : IDisposable
	{
		private readonly ForegroundActivationNativeCalls _native;
		private readonly int _callerTimeoutMilliseconds;
		private readonly int _workBudgetMilliseconds;
		private readonly AutoResetEvent _ready = new AutoResetEvent(false);
		private readonly Thread _thread;
		private Request _pending;
		private Request _current;
		private int _inFlight;
		private int _disposed;
		private int _unsafeAttachment;

		internal ForegroundActivationWorker(ForegroundActivationNativeCalls native, int callerTimeoutMilliseconds, int workBudgetMilliseconds)
		{
			this._native = native ?? throw new ArgumentNullException(nameof(native));
			if (workBudgetMilliseconds <= 0 || callerTimeoutMilliseconds < workBudgetMilliseconds)
				throw new ArgumentOutOfRangeException(nameof(workBudgetMilliseconds));
			this._callerTimeoutMilliseconds = callerTimeoutMilliseconds;
			this._workBudgetMilliseconds = workBudgetMilliseconds;
			this._thread = new Thread(this.Run) { IsBackground = true, Name = "EVE-O foreground activation worker" };
			this._thread.Start();
		}

		internal bool TryActivate(IntPtr target, out string reason)
		{
			if (target == IntPtr.Zero) { reason = "invalid-target"; return false; }
			if (Volatile.Read(ref this._disposed) != 0) { reason = "activation-worker-stopped"; return false; }
			if (Volatile.Read(ref this._unsafeAttachment) != 0) { reason = "input-detach-failed"; return false; }
			if (Interlocked.CompareExchange(ref this._inFlight, 1, 0) != 0) { reason = "activation-worker-busy"; return false; }
			var request = new Request(target, this._workBudgetMilliseconds);
			Volatile.Write(ref this._pending, request);
			try { this._ready.Set(); }
			catch (ObjectDisposedException)
			{
				request.Cancel();
				Interlocked.Exchange(ref this._pending, null);
				Volatile.Write(ref this._inFlight, 0);
				reason = "activation-worker-stopped";
				return false;
			}
			if (!request.Completion.Task.Wait(this._callerTimeoutMilliseconds))
			{
				request.Cancel();
				reason = "activation-timeout";
				return false;
			}
			ActivationResult result = request.Completion.Task.GetAwaiter().GetResult();
			reason = result.Reason;
			return result.Success;
		}

		private void Run()
		{
			while (this._ready.WaitOne())
			{
				Request request = Interlocked.Exchange(ref this._pending, null);
				if (Volatile.Read(ref this._disposed) != 0)
				{
					request?.Completion.TrySetResult(new ActivationResult(false, "activation-worker-stopped"));
					Volatile.Write(ref this._inFlight, 0);
					return;
				}
				if (request == null) continue;
				Volatile.Write(ref this._current, request);
				// Close the handoff gap where Dispose could see neither pending
				// nor current: once published, either side observes cancellation.
				if (Volatile.Read(ref this._disposed) != 0) request.Cancel();
				ActivationResult result;
				try { result = this.Execute(request); }
				catch (Exception exception) { result = new ActivationResult(false, "activation-native-error: " + exception.GetType().Name); }
				Volatile.Write(ref this._current, null);
				Volatile.Write(ref this._inFlight, 0);
				request.Completion.TrySetResult(result);
				// A failed detach must end this native thread, not park an
				// attached input queue on an event that never pumps messages.
				// Natural thread exit lets Windows release its input-queue state.
				if (Volatile.Read(ref this._disposed) != 0 || Volatile.Read(ref this._unsafeAttachment) != 0) return;
			}
		}

		private ActivationResult Execute(Request request)
		{
			if (request.Expired) return Failure("activation-timeout");
			this._native.EnsureMessageQueue();
			if (request.Expired) return Failure("activation-timeout");
			if (!this._native.IsWindow(request.Target)) return Failure("invalid-target");
			IntPtr originalForeground = this._native.GetForegroundWindow();
			if (originalForeground == request.Target) return Success();
			// Background games may deliberately pump slowly while frame-limited.
			// Plain cross-queue activation is asynchronous: do not reject it on
			// a short target WM_NULL probe before trying the normal API path.
			if (request.Expired) return Failure("activation-timeout");
			if (this._native.GetForegroundWindow() != originalForeground) return Failure("foreground-changed");
			bool accepted = this._native.SetForegroundWindow(request.Target);
			if (request.Expired) return Failure("activation-timeout");
			// A successful cross-queue request is asynchronous. Never attach or
			// issue another foreground request while that activation is pending.
			if (accepted) return this.ObserveForeground(request);
			IntPtr foreground = this._native.GetForegroundWindow();
			if (foreground == request.Target) return Success();
			if (foreground == IntPtr.Zero || foreground != originalForeground) return Failure("foreground-changed");
			if (this._native.IsOwnProcessWindow(foreground)) return Failure("own-process-input-not-attached");
			if (!this.Ping(request, foreground)) return Failure(request.Expired ? "activation-timeout" : "foreground-not-responding");
			if (request.Expired) return Failure("activation-timeout");
			uint workerThread = this._native.GetCurrentThreadId();
			uint foregroundThread = this._native.GetWindowThreadId(foreground);
			if (foregroundThread == 0 || workerThread == foregroundThread) return Failure("foreground-access-denied");
			// Do not attach to a window the user switched away from during probing.
			if (this._native.GetForegroundWindow() != foreground) return Failure("foreground-changed");
			if (request.Expired) return Failure("activation-timeout");
			if (!this._native.AttachThreadInput(workerThread, foregroundThread, true)) return Failure("input-attach-denied");
			bool attempted = false;
			try
			{
				if (!request.Expired)
				{
					attempted = true;
					accepted = this._native.SetForegroundWindow(request.Target);
				}
			}
			finally
			{
				try
				{
					if (!this._native.AttachThreadInput(workerThread, foregroundThread, false)) Volatile.Write(ref this._unsafeAttachment, 1);
				}
				catch { Volatile.Write(ref this._unsafeAttachment, 1); throw; }
			}
			if (Volatile.Read(ref this._unsafeAttachment) != 0) return Failure("input-detach-failed");
			if (request.Expired || !attempted) return Failure("activation-timeout");
			return accepted ? this.ObserveForeground(request)
				: (this._native.GetForegroundWindow() == request.Target ? Success() : Failure("foreground-access-denied"));
		}

		private bool Ping(Request request, IntPtr window)
		{
			int remaining = request.RemainingMilliseconds;
			return remaining > 0 && this._native.PingWindow(window, (uint)Math.Min(100, remaining));
		}

		private ActivationResult ObserveForeground(Request request)
		{
			while (!request.Expired)
			{
				if (this._native.GetForegroundWindow() == request.Target) return Success();
				Thread.Sleep(Math.Min(5, request.RemainingMilliseconds));
			}
			return Failure("activation-not-confirmed");
		}

		private static ActivationResult Success() => new ActivationResult(true, string.Empty);
		private static ActivationResult Failure(string reason) => new ActivationResult(false, reason);

		public void Dispose()
		{
			if (Interlocked.Exchange(ref this._disposed, 1) != 0) return;
			Volatile.Read(ref this._pending)?.Cancel();
			Volatile.Read(ref this._current)?.Cancel();
			this._ready.Set();
			// A foreign native call cannot be safely aborted. Keep that one
			// background thread/event alive until it returns or the process exits.
			if (this._thread != Thread.CurrentThread && this._thread.Join(100)) this._ready.Dispose();
		}

		private sealed class Request
		{
			internal readonly IntPtr Target;
			internal readonly TaskCompletionSource<ActivationResult> Completion = new TaskCompletionSource<ActivationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			private readonly long _deadline;
			private int _cancelled;
			internal Request(IntPtr target, int budgetMilliseconds)
			{
				this.Target = target;
				this._deadline = Stopwatch.GetTimestamp() + (long)(budgetMilliseconds * (double)Stopwatch.Frequency / 1000);
			}
			internal bool Expired => Volatile.Read(ref this._cancelled) != 0 || Stopwatch.GetTimestamp() >= this._deadline;
			internal int RemainingMilliseconds => this.Expired ? 0 : Math.Max(0, (int)Math.Ceiling((this._deadline - Stopwatch.GetTimestamp()) * 1000d / Stopwatch.Frequency));
			internal void Cancel() => Interlocked.Exchange(ref this._cancelled, 1);
		}

		private readonly struct ActivationResult
		{
			internal bool Success { get; }
			internal string Reason { get; }
			internal ActivationResult(bool success, string reason) { this.Success = success; this.Reason = reason; }
		}
	}

	internal static class ForegroundActivationNativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct NativeMessage
		{
			internal IntPtr Window;
			internal uint Message;
			internal UIntPtr WParam;
			internal IntPtr LParam;
			internal uint Time;
			internal int X;
			internal int Y;
			internal uint Private;
		}
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindow(IntPtr window);
		[DllImport("user32.dll")]
		internal static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		internal static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
		[DllImport("kernel32.dll")]
		internal static extern uint GetCurrentThreadId();
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PeekMessage(out NativeMessage message, IntPtr window, uint min, uint max, uint remove);
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SendMessageTimeout(IntPtr window, uint message, UIntPtr wParam, IntPtr lParam, uint flags, uint timeout, out UIntPtr result);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetForegroundWindow(IntPtr window);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AttachThreadInput(uint from, uint to, [MarshalAs(UnmanagedType.Bool)] bool attach);
	}
}
