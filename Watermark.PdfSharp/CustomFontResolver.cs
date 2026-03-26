using PdfSharpCore.Fonts;

namespace Watermark.PdfSharp
{
    public class CustomFontResolver : IFontResolver
    {
        private readonly Dictionary<string, byte[]> _fontData;

        public CustomFontResolver()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            _fontData = new Dictionary<string, byte[]>
            {
                { "arial", File.ReadAllBytes(Path.Combine(basePath, "Fonts/arial.ttf")) },
                { "calibri", File.ReadAllBytes(Path.Combine(basePath, "Fonts/calibri.ttf")) },
                { "times", File.ReadAllBytes(Path.Combine(basePath, "Fonts/times.ttf")) }
            };
        }

        // REQUIRED in some versions
        public string DefaultFontName => "arial";

        // Maps font name → internal key
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            familyName = familyName.ToLower();

            if (familyName.Contains("calibri"))
                return new FontResolverInfo("calibri");

            if (familyName.Contains("times"))
                return new FontResolverInfo("times");

            return new FontResolverInfo("arial"); // default
        }

        // Returns actual font bytes
        public byte[] GetFont(string faceName)
        {
            return _fontData[faceName];
        }
    }
}