using Alba.Internal;
using Microsoft.AspNetCore.StaticFiles;

namespace Alba;

/// <summary>
/// Strong-typed enumeration of common mime types that may be a useful helper within Alba
/// scenario definitions
/// </summary>
public sealed class MimeType
{
    public static readonly string HttpFormMimetype = "application/x-www-form-urlencoded";
    public static readonly string MultipartMimetype = "multipart/form-data";


    // This *must* go before the code below
    private static readonly LightweightCache<string, MimeType> _mimeTypes = new(key => new MimeType(key));



    public static readonly MimeType Html = New("text/html", ".htm", ".html");
    public static readonly MimeType Json = New("application/json");
    public static readonly MimeType Text = New("text/plain", ".txt");
    public static readonly MimeType Javascript = New("application/javascript", ".js", ".coffee");
    public static readonly MimeType Css = New("text/css", ".css");

    public static readonly MimeType Gif = New("image/gif", ".gif");
    public static readonly MimeType Png = New("image/png", ".png");
    public static readonly MimeType Jpg = New("image/jpeg", ".jpg", ".jpeg");
    public static readonly MimeType Bmp = New("image/bmp", ".bmp", ".bm");
    public static readonly MimeType Unknown = New("dunno");
    public static readonly MimeType EventStream = New("text/event-stream");

    public static readonly MimeType Xml = New("application/xml", ".xml");
    public static readonly MimeType Any = New("*/*");
    public static readonly MimeType TrueTypeFont = New("application/octet-stream", ".ttf");
    public static readonly MimeType WebOpenFont = New("application/font-woff", ".woff");
    public static readonly MimeType WebOpenFont2 = New("application/font-woff2", ".woff2");
    public static readonly MimeType EmbeddedOpenType = New("application/vnd.ms-fontobject", ".eot");
    public static readonly MimeType Svg = New("image/svg+xml", ".svg");

    private readonly HashSet<string> _extensions = new();
    private readonly string _mimeType;

    private MimeType(string mimeType)
    {
        _mimeType = mimeType;
    }

    public string Value => _mimeType;

    public static MimeType New(string mimeTypeValue, params string[] extensions)
    {
        var mimeType = new MimeType(mimeTypeValue);
        foreach (var extension in extensions)
        {
            mimeType.AddExtension(extension);
        }
        _mimeTypes[mimeTypeValue] = mimeType;

        return mimeType;
    }

    public void AddExtension(string extension)
    {
        _extensions.Add(extension);
    }

    public override string ToString()
    {
        return _mimeType;
    }

    public static IEnumerable<MimeType> All()
    {
        return _mimeTypes.GetAll();
    }

    public static MimeType MimeTypeByValue(string mimeTypeValue)
    {
        return _mimeTypes[mimeTypeValue];
    }

    public bool HasExtension(string extension)
    {
        return _extensions.Contains(extension);
    }

    public string? DefaultExtension()
    {
        return _extensions.FirstOrDefault();
    }

    public static MimeType? MimeTypeByFileName(string name)
    {
        var extension = Path.GetExtension(name);

        return _mappingFromExtension[extension];
    }

    private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new();

    public IEnumerable<string> Extensions => _extensions;

    private static readonly LightweightCache<string, MimeType?> _mappingFromExtension;

    static MimeType()
    {
        foreach (var pair in FileExtensionContentTypeProvider.Mappings)
        {
            _mimeTypes[pair.Value].AddExtension(pair.Key);
        }


        _mappingFromExtension = new LightweightCache<string, MimeType?>(extension => {
            return _mimeTypes.GetAll().FirstOrDefault(x => x.HasExtension(extension));
        });
    }
}