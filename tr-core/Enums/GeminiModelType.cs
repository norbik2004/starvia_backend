using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Enums
{
    public enum GeminiModelType
    {
        Gemini3FlashPreview,
        Gemini3Point1FlashLitePreview
    }

    public static class GeminiModelTypeExtensions
    {
        public static string ToModelString(this GeminiModelType type)
        {
            return type switch
            {
                GeminiModelType.Gemini3FlashPreview => "gemini-3-flash-preview",
                GeminiModelType.Gemini3Point1FlashLitePreview => "gemini-3.1-flash-lite-preview",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
