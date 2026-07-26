using System;

namespace PetriDish.Presentation
{
    public enum TextScaleMode
    {
        Standard = 0,
        Large = 1
    }

    public static class TextScalePolicy
    {
        public const float LargeMultiplier = 1.25f;

        public static TextScaleMode Next(TextScaleMode current)
        {
            return current == TextScaleMode.Large
                ? TextScaleMode.Standard
                : TextScaleMode.Large;
        }

        public static string ButtonLabel(TextScaleMode current)
        {
            return current == TextScaleMode.Large
                ? "Text: Large"
                : "Text: Standard";
        }

        public static int ScaleFontSize(int baseSize, TextScaleMode mode)
        {
            if (baseSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(baseSize), "Base font size must be positive.");

            float multiplier = mode == TextScaleMode.Large ? LargeMultiplier : 1f;
            return Math.Max(1, (int)Math.Round(baseSize * multiplier, MidpointRounding.AwayFromZero));
        }

        public static TextScaleMode FromStoredValue(int value)
        {
            return value == (int)TextScaleMode.Large
                ? TextScaleMode.Large
                : TextScaleMode.Standard;
        }
    }
}
