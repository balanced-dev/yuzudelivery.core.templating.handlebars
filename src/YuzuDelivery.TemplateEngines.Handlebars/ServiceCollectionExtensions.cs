using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using YuzuDelivery.Core;
using YuzuDelivery.TemplateEngines.Handlebars.Settings;

namespace YuzuDelivery.TemplateEngines.Handlebars;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYuzuHandlebars(this IServiceCollection services)
    {
        services.AddSingleton<IYuzuTemplateEngine,YuzuHandlebarsTemplateEngine>();

        services.AddOptions<HandlebarsSettings>()
                .Configure<IConfiguration, IHostingEnvironment, IEnumerable<IBaseSiteConfig>> ((s, cfg, host, baseConfigs) =>
                {
                    cfg.GetSection("Yuzu:TemplateEngine").Bind(s);

                    if (!Path.IsPathFullyQualified(s.TemplatesPath))
                    {
                        s.TemplatesPath = Path.Combine(host.ContentRootPath, s.TemplatesPath);
                    }

                    var fileProviders = baseConfigs.Select(c => c.TemplateFileProvider).ToList();
                    if(Directory.Exists(s.TemplatesPath)) fileProviders.Insert(0, new PhysicalFileProvider(s.TemplatesPath));

                    s.TemplatesFileProvider = new CompositeFileProvider(fileProviders);
                });

        return services;
    }
}
