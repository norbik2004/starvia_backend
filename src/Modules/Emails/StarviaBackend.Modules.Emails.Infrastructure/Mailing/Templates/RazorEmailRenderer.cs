using RazorLight;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing;

internal sealed class RazorEmailRenderer : IRazorEmailRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorEmailRenderer()
    {
        var templatesPath = Path.Combine(
            AppContext.BaseDirectory,
            "Mailing",
            "Templates");

        if (!Directory.Exists(templatesPath))
        {
            throw new DirectoryNotFoundException(
                $"Email templates directory not found: {templatesPath}");
        }

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesPath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model)
    {
        return _engine.CompileRenderAsync(
            templateName,
            model);
    }
}
