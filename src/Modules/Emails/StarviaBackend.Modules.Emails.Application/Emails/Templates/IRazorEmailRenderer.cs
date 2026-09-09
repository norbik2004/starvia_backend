namespace StarviaBackend.Modules.Emails.Application.Emails.Templates;

internal interface IRazorEmailRenderer
{
    Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model);
}
