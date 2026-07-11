using System;
using UnityEngine;

namespace UI
{
	public struct Markup
	{
		public const string breakLine = "<br>";
		public const string bold = "<b>";
		public const string noBold = "</b>";
		public const string italic = "<i>";
		public const string noItalic = "</i>";
		public const string underline = "<u>";
		public const string noUnderline = "</u>";
		public static string color(Color color) => "<color=#" + ToHexString(color) + ">";
		public static string color(float r, float g, float b, float a = 1f)
			=> "<color=#" + ToHexString(new Color(r, g, b, a)) + ">";
		public static string color(int r, int g, int b, int a = 255)
			=> "<color=#" + ToHexString(new Color(r / 255f, g / 255f, b / 255f, a / 255f)) + ">";
		public const string noColor = "</color>";
		public static string size(float size) => "<size=" + size.ToString() + ">";
		public const string noSize = "</size>";

		public static string ToBold(string text) => bold + text + noBold;
		public static string ToItalic(string text) => italic + text + noItalic;
		public static string ToUnderlined(string text) => underline + text + noUnderline;
		public static string ToColor(string text, Color color) => Markup.color(color) + text + noColor;
		public static string ToColorLerped(string text, Color color0, Color color1)
            => ToLerped(text, t => color(Color.Lerp(color0, color1, t)), noColor);
        public static string ToSized(string text, float size) => Markup.size(size) + text + noSize;
		public static string ToSizedLerp(string text, float sizeStart, float sizeEnd)
			=> ToLerped(text, t => size(Mathf.Lerp(sizeStart, sizeEnd, t)), noSize);
		private static string ToLerped(string text, Func<float, string> markupFromLerpedValue, string endMarkup)
		{
            string result = "";

            int offset = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (i + offset >= text.Length)
                    break;

                int index = i + offset;
                char c = text[index];

                float t = i / (text.Length - 1f);
                result += markupFromLerpedValue(t);

                result += c;

                // Handle markups
                if (c == '<')
                {
                    int markupEndIndex = text.IndexOf('>', index);
                    if (markupEndIndex == -1)
                        continue;
                    result += text.Substring(index + 1, markupEndIndex - index);
                    offset += markupEndIndex - index;
                }
            }

            result += endMarkup;
			return result;
        }

        public static string ToHexString(Color color)
        {
            return ((byte)(color.r * 255f)).ToString("X2") + ((byte)(color.g * 255f)).ToString("X2") + ((byte)(color.b * 255f)).ToString("X2") + ((byte)(color.a * 255f)).ToString("X2");
        }
    }

}