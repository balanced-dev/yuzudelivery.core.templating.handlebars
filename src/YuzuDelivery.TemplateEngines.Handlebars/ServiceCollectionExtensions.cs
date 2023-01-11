using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using YuzuDelivery.Core;
using YuzuDelivery.TemplateEngines.Handlebars.Settings;

namespace YuzuDelivery.TemplateEngines.Handlebars;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYuzuHandlebars(this IServiceCollection services)
    {
        services.AddSingleton<IYuzuTemplateEngine,YuzuHandlebarsTemplateEngine>();

        services.AddOptions<HandlebarsSettings>()
                .Configure<IConfiguration, IHostEnvironment> ((s, cfg, host) =>
                {
                    if (s.TemplatesFileProvider != null)
                    {
                        return;
                    }

                    cfg.GetSection("Yuzu:TemplateEngine").Bind(s);

                    if (!Path.IsPathFullyQualified(s.TemplatesPath))
                    {
                        s.TemplatesPath = Path.Combine(host.ContentRootPath, s.TemplatesPath);
                    }

                    s.TemplatesFileProvider = new PhysicalFileProvider(s.TemplatesPath);
                });

        return services;
    }
}
