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

        var report = new FileProviderReport();

        var fileProvider = _handlebarsSettings.Value.TemplatesFileProvider;

        if(fileProvider is CompositeFileProvider)
        {
            var compositeFileProvider = (CompositeFileProvider)fileProvider;
            foreach(var f in compositeFileProvider.FileProviders)
            {
                if(f is EmbeddedFileProvider)
                {
                    var embeddedFileProvider = (EmbeddedFileProvider)f;
                    report.FileProviders.Add("embedded");
                }

                if (f is PhysicalFileProvider)
                {
                    var physicalFileProvider = (PhysicalFileProvider)f;
                    report.FileProviders.Add($"physical {physicalFileProvider.Root}");
                }
            }
        }
        report.Files = fileProvider.GetDirectoryContents(string.Empty)
            .Cast<IFileInfo>();

        await context.Response.WriteAsJsonAsync(
            report,
            report.GetType(),
            new JsonSerializerOptions(),
            MediaTypeNames.Application.Json);
    }

    private bool ShouldHandle(HttpContext context)
    {
        if (context.Request.Query.ContainsKey("showJsonLoadedTemplates"))
        {
            return true;
        }

        return false;
    }

    public class FileProviderReport
    {
        public FileProviderReport()
        {
            FileProviders = new List<string>();
        }

        public List<string> FileProviders { get; set; }
        public IEnumerable<IFileInfo> Files { get; set; }
    }
}


