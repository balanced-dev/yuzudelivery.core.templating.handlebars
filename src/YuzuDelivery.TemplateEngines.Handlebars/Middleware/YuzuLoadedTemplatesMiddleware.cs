using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using YuzuDelivery.Core.Settings;
using YuzuDelivery.TemplateEngines.Handlebars.Settings;

namespace YuzuDelivery.TemplateEngines.Handlebars.Middleware;

public class YuzuLoadedTemplatesMiddleware : IMiddleware
{
    private readonly IOptions<HandlebarsSettings> _handlebarsSettings;

    public YuzuLoadedTemplatesMiddleware(IOptions<HandlebarsSettings> handlebarsSettings)
    {
        _handlebarsSettings = handlebarsSettings;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // ReSharper disable once StringLiteralTypo
        if (!ShouldHandle(context))
        {
            await next(context);
            return;
        }

        var fileProvider = _handlebarsSettings.Value.TemplatesFileProvider;

        var files = fileProvider.GetDirectoryContents(string.Empty)
            .Cast<IFileInfo>();

        await context.Response.WriteAsJsonAsync(
            files,
            files.GetType(),
            new JsonSerializerOptions(),
            MediaTypeNames.Application.Json);
    }

    private bool ShouldHandle(HttpContext context)
    {
        if (context.Request.Query.ContainsKey("showJsonTemplatesSchema"))
        {
            return true;
        }

        return false;
    }
}


