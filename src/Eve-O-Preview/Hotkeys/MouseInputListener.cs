using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	/// <summary>
	/// Receives only middle/X-button gestures. The native hook has its own message loop;
	/// accepted actions are posted to the thread that constructed this listener.
	/// </summary>
	internal sealed class MouseInputListener : IDisposable
	{
		private readonly object _lifecycleGate = new object();
		private readonly object _inputGate = new object();
		private readonly Func<Keys, bool> _accepts;
		private readonly Action<Keys> _pressed;
		private readonly Control _dispatcher;
		private readonly MouseInputState _state = new MouseInputState();
		private readonly MouseInputNativeMethods.HookProc _hookProc;
		private readonly ManualResetEventSlim _startup = new ManualResetEventSlim(false);
		private Thread _thread;
		private IntPtr _hook;
		private uint _threadId;
		private int _startupError;
		private int _disposed;
		private long _generation;

		public MouseInputListener(Func<Keys, bool> accepts, Action<Keys> pressed)
		{
			this._accepts = accepts ?? throw new ArgumentNullException(nameof(accepts));
			this._pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
			this._hookProc = this.HookCallback;
			this._dispatcher = new Control();
			// Create the HWND here, never lazily on the native hook thread.
			IntPtr dispatcherHandle = this._dispatcher.Handle;
		}

		public bool Start(out int error)
		{
			lock (this._lifecycleGate)
			{
				if (Volatile.Read(ref this._disposed) != 0)
				{
					error = MouseInputNativeMethods.ERROR_INVALID_HANDLE;
					return false;
				}
				if (this._thread != null)
				{
					bool running = Volatile.Read(ref this._hook) != IntPtr.Zero;
					error = running ? 0 : (this._startupError == 0 ? MouseInputNativeMethods.ERROR_GEN_FAILURE : this._startupError);
					return running;
				}

				this._thread = new Thread(this.RunMessageLoop)
				{
					IsBackground = true,
					Name = "EVE-O mouse hotkey listener"
				};
				try
				{
					this._thread.Start();
					if (!this._startup.Wait(TimeSpan.FromSeconds(5)))
					{
						this._startupError = MouseInputNativeMethods.ERROR_TIMEOUT;
						Volatile.Write(ref this._disposed, 1);
						this.StopNativeListener();
					}
				}
				catch (Exception exception) when (exception is SystemException)
				{
					this._startupError = MouseInputNativeMethods.ERROR_GEN_FAILURE;
					Volatile.Write(ref this._disposed, 1);
					this.StopNativeListener();
				}
				error = this._startupError;
				return error == 0 && Volatile.Read(ref this._hook) != IntPtr.Zero;
			}
		}

		/// <summary>
		/// Cancel queued callbacks and publish a new, fast acceptance snapshot together.
		/// The optional mutation must only publish already prepared state, never run UI.
		/// </summary>
		internal void InvalidatePending(Action publish = null)
		{
			lock (this._inputGate)
			{
				Interlocked.Increment(ref this._generation);
				publish?.Invoke();
			}
		}

		public void Dispose()
		{
			// Cancel first, including when the owner is disposed off the UI thread.
			Interlocked.Exchange(ref this._disposed, 1);
			this.InvalidatePending();
			lock (this._lifecycleGate)
			{
				this.StopNativeListener();
			}
			this.DisposeDispatcher();
			GC.SuppressFinalize(this);
		}

		private void RunMessageLoop()
		{
			try
			{
				this._threadId = MouseInputNativeMethods.GetCurrentThreadId();
				// Establish a queue before announcing startup so WM_QUIT cannot be lost.
				MouseInputNativeMethods.PeekMessage(out MouseInputNativeMethods.NativeMessage ignored, IntPtr.Zero, 0, 0, 0);
				if (Volatile.Read(ref this._disposed) != 0) return;
				IntPtr hook = MouseInputNativeMethods.SetWindowsHookEx(MouseInputNativeMethods.WH_MOUSE_LL,
					this._hookProc, MouseInputNativeMethods.GetModuleHandle(null), 0);
				if (hook == IntPtr.Zero)
				{
					this._startupError = Marshal.GetLastWin32Error();
					if (this._startupError == 0) this._startupError = MouseInputNativeMethods.ERROR_GEN_FAILURE;
					return;
				}
				Interlocked.Exchange(ref this._hook, hook);
				if (Volatile.Read(ref this._disposed) != 0) return;
				this._startup.Set();
				while (MouseInputNativeMethods.GetMessage(out MouseInputNativeMethods.NativeMessage message, IntPtr.Zero, 0, 0) > 0)
				{
					MouseInputNativeMethods.TranslateMessage(ref message);
					MouseInputNativeMethods.DispatchMessage(ref message);
				}
			}
			catch (Exception exception) when (exception is SystemException)
			{
				this._startupError = MouseInputNativeMethods.ERROR_GEN_FAILURE;
			}
			finally
			{
				IntPtr hook = Interlocked.Exchange(ref this._hook, IntPtr.Zero);
				if (hook != IntPtr.Zero) MouseInputNativeMethods.UnhookWindowsHookEx(hook);
				this._startup.Set();
				// A stopped native loop must not deliver input it had queued earlier.
				this.InvalidatePending();
			}
		}

		private void StopNativeListener()
		{
			// Unhook before waiting: even a blocked owner thread cannot leave a global hook.
			IntPtr hook = Interlocked.Exchange(ref this._hook, IntPtr.Zero);
			if (hook != IntPtr.Zero) MouseInputNativeMethods.UnhookWindowsHookEx(hook);
			uint threadId = Volatile.Read(ref this._threadId);
			if (threadId != 0) MouseInputNativeMethods.PostThreadMessage(threadId, MouseInputNativeMethods.WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
			Thread thread = this._thread;
			if (thread != null && thread.IsAlive && thread != Thread.CurrentThread) thread.Join(TimeSpan.FromSeconds(2));
		}

		private IntPtr HookCallback(int code, IntPtr message, IntPtr data)
		{
			try
			{
				if (code >= 0 && Volatile.Read(ref this._disposed) == 0)
				{
					int messageId = unchecked((int)message.ToInt64());
					// Do not read coordinates or inspect unrelated input.
					if (MouseInputDecoder.IsSupportedMessage(messageId))
					{
						uint mouseData = messageId == MouseInputNativeMethods.WM_XBUTTONDOWN || messageId == MouseInputNativeMethods.WM_XBUTTONUP
							? unchecked((uint)Marshal.ReadInt32(data, 8)) : 0;
						if (MouseInputDecoder.TryDecode(messageId, mouseData, out Keys button, out bool isDown))
						{
							Keys modifiers = MouseInputNativeMethods.CurrentModifiers();
							bool windowsDown = MouseInputNativeMethods.IsDown(Keys.LWin) || MouseInputNativeMethods.IsDown(Keys.RWin);
							if (this.ProcessInput(button, isDown, modifiers, windowsDown)) return new IntPtr(1);
						}
					}
				}
			}
			catch (Exception) { /* Never unwind a managed exception through a native hook. */ }
			return MouseInputNativeMethods.CallNextHookEx(IntPtr.Zero, code, message, data);
		}

		private bool ProcessInput(Keys button, bool isDown, Keys modifiers, bool windowsDown)
		{
			lock (this._inputGate)
			{
				if (Volatile.Read(ref this._disposed) != 0) return false;
				long generation = Interlocked.Read(ref this._generation);
				bool suppress = this._state.Process(button, isDown, modifiers, windowsDown, this._accepts, out Keys gesture);
				if (gesture != Keys.None && !this.QueuePressed(gesture, generation))
				{
					this._state.CancelSuppression(button);
					return false;
				}
				return suppress;
			}
		}

		private bool QueuePressed(Keys gesture, long generation)
		{
			try
			{
				this._dispatcher.BeginInvoke((MethodInvoker)(() =>
				{
					if (Volatile.Read(ref this._disposed) == 0 && Interlocked.Read(ref this._generation) == generation)
					{
						this._pressed(gesture);
					}
				}));
				return true;
			}
			catch (InvalidOperationException) { return false; }
		}

		private void DisposeDispatcher()
		{
			try
			{
				if (this._dispatcher.IsDisposed) return;
				if (this._dispatcher.InvokeRequired)
				{
					this._dispatcher.BeginInvoke((MethodInvoker)this._dispatcher.Dispose);
				}
				else this._dispatcher.Dispose();
			}
			catch (InvalidOperationException) { /* The owning UI has already exited. */ }
		}
	}

	/// <summary>Pure decoder, shared by the recording and registered-hotkey paths.</summary>
	internal static class MouseInputDecoder
	{
		internal static bool IsSupportedMessage(int message)
		{
			return message == MouseInputNativeMethods.WM_MBUTTONDOWN || message == MouseInputNativeMethods.WM_MBUTTONUP
				|| message == MouseInputNativeMethods.WM_XBUTTONDOWN || message == MouseInputNativeMethods.WM_XBUTTONUP;
		}

		internal static bool TryDecode(int message, uint mouseData, out Keys button, out bool isDown)
		{
			button = Keys.None;
			isDown = message == MouseInputNativeMethods.WM_MBUTTONDOWN || message == MouseInputNativeMethods.WM_XBUTTONDOWN;
			if (message == MouseInputNativeMethods.WM_MBUTTONDOWN || message == MouseInputNativeMethods.WM_MBUTTONUP)
			{
				button = Keys.MButton;
				return true;
			}
			if (message == MouseInputNativeMethods.WM_XBUTTONDOWN || message == MouseInputNativeMethods.WM_XBUTTONUP)
			{
				uint xButton = (mouseData >> 16) & 0xFFFF;
				if (xButton == 1) button = Keys.XButton1;
				else if (xButton == 2) button = Keys.XButton2;
				return button != Keys.None;
			}
			return false;
		}
	}

	/// <summary>Keeps suppression paired with the original down, even if modifiers change.</summary>
	internal sealed class MouseInputState
	{
		private int _down;
		private int _suppressed;

		internal bool Process(Keys button, bool isDown, Keys modifiers, bool windowsDown, Func<Keys, bool> accepts, out Keys gesture)
		{
			gesture = Keys.None;
			int bit = ButtonBit(button);
			if (bit == 0) return false;
			if (!isDown)
			{
				bool suppress = (this._suppressed & bit) != 0;
				this._down &= ~bit;
				this._suppressed &= ~bit;
				return suppress;
			}
			if ((this._down & bit) != 0) return (this._suppressed & bit) != 0;
			this._down |= bit;
			Keys candidate = button | (modifiers & (Keys.Control | Keys.Alt | Keys.Shift));
			if (windowsDown || !accepts(candidate)) return false;
			this._suppressed |= bit;
			gesture = candidate;
			return true;
		}

		internal void CancelSuppression(Keys button) => this._suppressed &= ~ButtonBit(button);
		private static int ButtonBit(Keys button) => button == Keys.MButton ? 1 : button == Keys.XButton1 ? 2 : button == Keys.XButton2 ? 4 : 0;
	}

	internal static class MouseInputNativeMethods
	{
		internal const int WH_MOUSE_LL = 14;
		internal const int WM_MBUTTONDOWN = 0x0207;
		internal const int WM_MBUTTONUP = 0x0208;
		internal const int WM_XBUTTONDOWN = 0x020B;
		internal const int WM_XBUTTONUP = 0x020C;
		internal const uint WM_QUIT = 0x0012;
		internal const int ERROR_INVALID_HANDLE = 6;
		internal const int ERROR_GEN_FAILURE = 31;
		internal const int ERROR_TIMEOUT = 1460;
		internal delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);

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

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetWindowsHookEx(int hookType, HookProc callback, IntPtr module, uint threadId);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool UnhookWindowsHookEx(IntPtr hook);
		[DllImport("user32.dll")]
		internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
		[DllImport("user32.dll")]
		internal static extern short GetAsyncKeyState(int key);
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern int GetMessage(out NativeMessage message, IntPtr window, uint min, uint max);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PeekMessage(out NativeMessage message, IntPtr window, uint min, uint max, uint remove);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool TranslateMessage(ref NativeMessage message);
		[DllImport("user32.dll")]
		internal static extern IntPtr DispatchMessage(ref NativeMessage message);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PostThreadMessage(uint threadId, uint message, UIntPtr wParam, IntPtr lParam);
		[DllImport("kernel32.dll")]
		internal static extern uint GetCurrentThreadId();
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetModuleHandle(string moduleName);

		internal static bool IsDown(Keys key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;
		internal static Keys CurrentModifiers()
		{
			return (IsDown(Keys.ControlKey) ? Keys.Control : Keys.None)
				| (IsDown(Keys.Menu) ? Keys.Alt : Keys.None)
				| (IsDown(Keys.ShiftKey) ? Keys.Shift : Keys.None);
		}
	}
}
