using EveOPreview.Configuration;

namespace EveOPreview.View
{
	static class ViewAnimationStyleConverter
	{
		public static AnimationStyle Convert(ViewAnimationStyle value)
		{
			// Cheat based on fact that the order and byte values of both enums are the same
			return (AnimationStyle)((int)value);
		}

		public static ViewAnimationStyle Convert(AnimationStyle value)
		{
			// Cheat based on fact that the order and byte values of both enums are the same
			return (ViewAnimationStyle)((int)value);
		}
	}
}