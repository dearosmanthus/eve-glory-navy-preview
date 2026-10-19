using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	/// <summary>Parses persisted gestures without letting malformed user configuration crash startup.</summary>
	public static class HotkeyGesture
	{
		public static bool TryParse(string value, out Keys keys)
		{
			keys = Keys.None;
			if (string.IsNullOrWhiteSpace(value))
			{
				return true;
			}

			try
			{
				object converted = new KeysConverter().ConvertFromInvariantString(value.Trim());
				if (!(converted is Keys parsed))
				{
					return false;
				}

				if (parsed == Keys.None)
				{
					return true;
				}

				if (!IsValid(parsed))
				{
					return false;
				}

				keys = parsed;
				return true;
			}
			catch (Exception exception) when (exception is ArgumentException || exception is FormatException || exception is NotSupportedException || exception is OverflowException || exception is InvalidCastException)
			{
				return false;
			}
		}

		public static bool IsValid(Keys keys)
		{
			if (keys == Keys.None)
			{
				return true;
			}

			Keys code = keys & Keys.KeyCode;
			return ((int)keys & ~(0xFFFF | (int)(Keys.Control | Keys.Alt | Keys.Shift))) == 0
				&& (int)code <= 255 && Enum.IsDefined(typeof(Keys), code)
				&& code != Keys.None && code != Keys.F12 && code != Keys.ShiftKey && code != Keys.ControlKey && code != Keys.Menu
				&& code != Keys.LShiftKey && code != Keys.RShiftKey && code != Keys.LControlKey && code != Keys.RControlKey
				&& code != Keys.LMenu && code != Keys.RMenu && code != Keys.LWin && code != Keys.RWin
				&& code != Keys.ProcessKey && code != Keys.Packet
				&& code != Keys.LButton && code != Keys.RButton && code != Keys.MButton && code != Keys.XButton1 && code != Keys.XButton2;
		}

		public static string Format(Keys keys)
		{
			return keys == Keys.None ? string.Empty : new KeysConverter().ConvertToInvariantString(keys);
		}
	}
}
