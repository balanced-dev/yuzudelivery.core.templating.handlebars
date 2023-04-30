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

        var report = new FileProviderMiddleWareReport(_handlebarsSettings.Value.TemplatesFileProvider);

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

    public class FileProviderMiddleWareReport
    {
        public FileProviderMiddleWareReport(IFileProvider rootFileProvider)
        {
            FileProviders = new List<string>();
            Files = new List<IFileInfo>();

            if (rootFileProvider is CompositeFileProvider)
            {
                var compositeFileProvider = (CompositeFileProvider)rootFileProvider;
                foreach (var f in compositeFileProvider.FileProviders)
                {
                    AddFileProviderToReport(f, this);
                }
            }
            else
            {
                AddFileProviderToReport(rootFileProvider, this);
            }

            try
            {
                Files = rootFileProvider.GetDirectoryContents(string.Empty)
                .Cast<IFileInfo>()
                .ToList();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

        }

        public List<string> FileProviders { get; set; }
        public IEnumerable<IFileInfo> Files { get; set; }
        public Exception? Exception { get; set; }

        private void AddFileProviderToReport(IFileProvider f, FileProviderMiddleWareReport report)
        {
            if (f is EmbeddedFileProvider)
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
}


