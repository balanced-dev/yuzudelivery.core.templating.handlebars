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
    private readonly IOptions<CoreSettings> _coreSettings;

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

    public YuzuHandlebarsTemplateEngine(
        ILogger<YuzuHandlebarsTemplateEngine> logger,
        IOptions<HandlebarsSettings> settings,
        IOptions<CoreSettings> coreSettings)
    {
        _logger = logger;
        _settings = settings;
        _coreSettings = coreSettings;

        ProcessTemplates(settings.Value.TemplatesFileProvider, string.Empty);
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

    private void ProcessTemplates(IFileProvider contents, string path)
    {
        var files = contents.GetDirectoryContents(path).Select(x => x.Name);

        foreach (var fileInfo in contents.GetDirectoryContents(path))
        { 

            if (fileInfo.IsDirectory)
            {
                ProcessTemplates(contents, Path.Combine(path, fileInfo.Name));
                continue;
            }

            if(fileInfo.Name.StartsWith(_settings.Value.LayoutPrefix))
            {
                continue;
            }

            if (Path.GetExtension(fileInfo.Name) != _settings.Value.HandlebarsFileExtension)
            {
                continue;
            }

            var templateName = Path.GetFileNameWithoutExtension(fileInfo.Name);

            if (fileInfo.Name.StartsWith(_coreSettings.Value.PartialPrefix))
            {
                using var partialStream = fileInfo.CreateReadStream();
                AddPartial(templateName, partialStream);
            }

            using var stream = fileInfo.CreateReadStream();
            AddPage(templateName, stream);
        }
    }

    private void AddPartial(string name, Stream fileStream)
    {
        try
        {
            _logger.LogDebug("Registering partial view: '{partial}", name);
            using var reader = new StreamReader(fileStream);
            var compiled = HandlebarsDotNet.Handlebars.Compile(reader);
            HandlebarsDotNet.Handlebars.RegisterTemplate(name, compiled);
        }
        catch (Exception ex)
        {
            var d = "d";
        }
    }

    private void AddPage(string name, Stream fileStream)
    {
        _logger.LogDebug("Registering view: '{view}", name);
        using var reader = new StreamReader(fileStream);
        var compiled = HandlebarsDotNet.Handlebars.Compile(reader.ReadToEnd());
        _cache[name] = compiled;
    }
}
