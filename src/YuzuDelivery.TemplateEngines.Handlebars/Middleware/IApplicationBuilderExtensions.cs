using Microsoft.AspNetCore.Builder;
using YuzuDelivery.TemplateEngines.Handlebars.Middleware;

// ReSharper disable once CheckNamespace
namespace YuzuDelivery.TemplateEngines.Handlebars;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseYuzuTemplatesMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<YuzuLoadedTemplatesMiddleware>();
        return app;
    }
}
