using PdfSharpCore.Fonts;
using Watermark.core.Enum;
using Watermark.core.Interfaces;
using Watermark.core.Models;
using Watermark.PdfSharp;

class Program
{
    static void Main()
    {
        string inputPath = @"C:\Users\KamaleshDevarasu\Desktop\Demo\Sample.pdf";
        string outputPath = @"C:\Users\KamaleshDevarasu\Desktop\Demo\output.pdf";

        GlobalFontSettings.FontResolver = new CustomFontResolver();
        IWatermarkService service = new PdfWatermarkService();

        var config = new WatermarkConfig
        {
            Text = "VJSAR-Employee",
            FontName = "calibri",
            FontSize = 35,
            Color = "green",
            Alpha = 80,
            Angle = -45,
            Placement = WatermarkPlacement.Center,
            Repeat = false,
            Margin = 20 
        };

        service.Apply(inputPath, outputPath, config);

        Console.WriteLine("Watermark applied successfully! ");
    }
}