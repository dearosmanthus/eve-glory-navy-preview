using System;

namespace EveOPreview.View
{
	public static class ClientTitleFormatter
	{
		public static string GetCharacterName(string title)
		{
			if (string.IsNullOrWhiteSpace(title)) return string.Empty;
			string value = title.Trim();
			string[] prefixes = { "EVE - ", "EVE Frontier - ", "星战前夜：晨曦 [Serenity] - ", "星战前夜:晨曦 [Serenity] - " };
			foreach (string prefix in prefixes)
				if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return value.Substring(prefix.Length).Trim();
			if (value.Equals("EVE", StringComparison.OrdinalIgnoreCase) || value.Equals("EVE Frontier", StringComparison.OrdinalIgnoreCase)) return string.Empty;
			return value;
		}
	}
}
