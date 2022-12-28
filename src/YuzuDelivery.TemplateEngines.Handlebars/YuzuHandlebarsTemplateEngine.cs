using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YuzuDelivery.Core;
using YuzuDelivery.Core.Settings;
using YuzuDelivery.TemplateEngines.Handlebars.Settings;

namespace YuzuDelivery.TemplateEngines.Handlebars;

// ReSharper disable once UnusedType.Global -- Used downstream
public class YuzuHandlebarsTemplateEngine : IYuzuTemplateEngine
{
    private readonly ILogger<YuzuHandlebarsTemplateEngine> _logger;
    private readonly IOptions<HandlebarsSettings> _settings;

    private readonly IDictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, string>> _cache =
        new Dictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, string>>();

    static YuzuHandlebarsTemplateEngine()
    {
        Helpers.IfCond.Register();
        Helpers.Array.Register();
        Helpers.Enum.Register();
        Helpers.DynPartial.Register();
        Helpers.ModPartial.Register();
        Helpers.ToString.Register();
        Helpers.ToLowerCase.Register();
        Helpers.PictureSource.Register();
    }

    public YuzuHandlebarsTemplateEngine(ILogger<YuzuHandlebarsTemplateEngine> logger,
        IOptions<HandlebarsSettings> settings, IOptions<CoreSettings> coreSettings)
    {
        _logger = logger;
        _settings = settings;

        settings.Value.TemplatesFileProvider.GetPagesAndPartials(_settings.Value.HandlebarsFileExtension, coreSettings.Value, AddCompiled);
    }

    public string Render(string templateName, object model)
    {
        if (!_cache.TryGetValue(templateName, out var template))
        {
            throw new TemplateNotFound(templateName);
        }

        return template(model);
    }

    public class TemplateNotFound : ApplicationException
    {
        public TemplateNotFound(string templateName)
            : base($"Unknown template: '{templateName}'")
        {
        }
    }

    private void AddCompiled(bool isPartial, bool islayout, string name, IFileInfo fileInfo)
    {
        if(!islayout)
        {

            if (isPartial)
            {
                var partialFileStream = fileInfo.CreateReadStream();
                using var partialReader = new StreamReader(partialFileStream);
                _logger.LogDebug("Registering partial view: '{partial}", name);
                var partial = HandlebarsDotNet.Handlebars.Compile(partialReader);
                HandlebarsDotNet.Handlebars.RegisterTemplate(name, partial);
            }

            //Can't re-used the stream, easiest to recreate from fileinfo
            var fileStream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(fileStream);
            _logger.LogDebug("Registering view: '{view}", name);
            var compiled = HandlebarsDotNet.Handlebars.Compile(reader.ReadToEnd());
            _cache.Add(name, compiled);
        }
    }
}
