using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	/// <summary>All active native mouse hotkeys share one hook, removed with the final binding.</summary>
	internal static class MouseHotkeyRegistry
	{
		private static readonly object Gate = new object();
		private static Dictionary<Keys, Action> _bindings = new Dictionary<Keys, Action>();
		private static MouseInputListener _listener;

		internal static bool Register(Keys gesture, Action pressed, out int error)
		{
			lock (Gate)
			{
				if (_bindings.ContainsKey(gesture))
				{
					error = (int)HotkeyHandlerNativeMethods.ERROR_HOTKEY_ALREADY_REGISTERED;
					return false;
				}
				if (_listener == null)
				{
					MouseInputListener listener;
					try { listener = new MouseInputListener(Accepts, Pressed); }
					catch (Win32Exception exception)
					{
						error = exception.NativeErrorCode == 0 ? MouseInputNativeMethods.ERROR_GEN_FAILURE : exception.NativeErrorCode;
						return false;
					}
					if (!listener.Start(out error))
					{
						listener.Dispose();
						return false;
					}
					_listener = listener;
				}
				var next = new Dictionary<Keys, Action>(_bindings) { [gesture] = pressed };
				_listener.InvalidatePending(() => Volatile.Write(ref _bindings, next));
				error = 0;
				return true;
			}
		}

		internal static void Unregister(Keys gesture)
		{
			lock (Gate)
			{
				if (!_bindings.ContainsKey(gesture)) return;
				var next = new Dictionary<Keys, Action>(_bindings);
				next.Remove(gesture);
				_listener.InvalidatePending(() => Volatile.Write(ref _bindings, next));
				if (next.Count == 0)
				{
					_listener.Dispose();
					_listener = null;
				}
			}
		}

		private static bool Accepts(Keys gesture) => Volatile.Read(ref _bindings).ContainsKey(gesture);
		private static void Pressed(Keys gesture)
		{
			if (Volatile.Read(ref _bindings).TryGetValue(gesture, out Action action)) action();
		}
	}
}
