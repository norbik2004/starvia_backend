using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Enums
{
    public enum LlmGeminiModelType
    {
        Gemini3FlashPreview,
        Gemini3Point1FlashLitePreview,
    }

    public enum ImageGeminiModelType
    {
        Imagen4UltraGenerate,
        Imagen4Generate,
        Imagen4FastGenerate
    }

    public static class GeminiModelTypeExtensions
    {
        public static string ToModelString<T>(this T type) where T : Enum
        {
            return type switch
            {
                LlmGeminiModelType.Gemini3FlashPreview => "gemini-3-flash-preview",
                LlmGeminiModelType.Gemini3Point1FlashLitePreview => "gemini-3.1-flash-lite-preview",

                ImageGeminiModelType.Imagen4UltraGenerate => "imagen-4-ultra-generate",
                ImageGeminiModelType.Imagen4Generate => "imagen-4-generate",
                ImageGeminiModelType.Imagen4FastGenerate => "imagen-4-fast-generate",

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
