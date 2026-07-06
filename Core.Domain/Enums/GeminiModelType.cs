namespace Core.Domain.Enums
{
    public enum LlmGeminiModelType
    {
        Gemini3FlashPreview,
        Gemini3Point1FlashLitePreview,
    }

    public enum ImageGeminiModelType
    {
        Imagen4Generate,
        Imagen4FastGenerate,
        Imagen4UltraGenerate,

        Gemini25FlashImage,

        Gemini3ProImage,
        Gemini3ProImagePreview,

        Gemini31FlashImage,
        Gemini31FlashImagePreview,
        Gemini31FlashLiteImage,
    }

    public static class GeminiModelTypeExtensions
    {
        public static string ToModelString<T>(this T type) where T : Enum
        {
            return type switch
            {
                // LLM
                LlmGeminiModelType.Gemini3FlashPreview => "gemini-3-flash-preview",
                LlmGeminiModelType.Gemini3Point1FlashLitePreview => "gemini-3.1-flash-lite-preview",

                // Imagen
                ImageGeminiModelType.Imagen4Generate => "imagen-4.0-generate-001",
                ImageGeminiModelType.Imagen4FastGenerate => "imagen-4.0-fast-generate-001",
                ImageGeminiModelType.Imagen4UltraGenerate => "imagen-4.0-ultra-generate-001",

                // Gemini Image
                ImageGeminiModelType.Gemini25FlashImage => "gemini-2.5-flash-image",

                ImageGeminiModelType.Gemini3ProImage => "gemini-3-pro-image",
                ImageGeminiModelType.Gemini3ProImagePreview => "gemini-3-pro-image-preview",

                ImageGeminiModelType.Gemini31FlashImage => "gemini-3.1-flash-image",
                ImageGeminiModelType.Gemini31FlashImagePreview => "gemini-3.1-flash-image-preview",
                ImageGeminiModelType.Gemini31FlashLiteImage => "gemini-3.1-flash-lite-image",

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}