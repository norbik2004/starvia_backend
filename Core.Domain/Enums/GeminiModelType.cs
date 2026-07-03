using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Enums
{
    public enum GeminiModelType
    {
        Gemini3FlashPreview,
        Gemini3Point1FlashLitePreview,
        Imagen4UltraGenerate,
        Imagen4Generage,
        Imagen4FastGenerate
    }

    public static class GeminiModelTypeExtensions
    {
        public static string ToModelString(this GeminiModelType type)
        {
            return type switch
            {
                GeminiModelType.Gemini3FlashPreview => "gemini-3-flash-preview",
                GeminiModelType.Gemini3Point1FlashLitePreview => "gemini-3.1-flash-lite-preview",
                GeminiModelType.Imagen4UltraGenerate => "imagen-4-ultra-generate",
                GeminiModelType.Imagen4Generage => "imagen-4-generate",
                GeminiModelType.Imagen4FastGenerate => "imagen-4-fast-generate",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
