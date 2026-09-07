using EveOPreview.Configuration;

namespace EveOPreview.View
{
	static class ViewCaptionBarStyleConverter
	{
		public static CaptionBarStyle Convert(ViewCaptionBarStyle value)
		{
			// Cheat based on fact that the order and byte values of both enums are the same
			return (CaptionBarStyle)((int)value);
		}

		public static ViewCaptionBarStyle Convert(CaptionBarStyle value)
		{
			// Cheat based on fact that the order and byte values of both enums are the same
			return (ViewCaptionBarStyle)((int)value);
		}
	}
}