using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Watermark.core.Interfaces;
using Watermark.core.Models;
using Watermark.core.Enum;

namespace Watermark.PdfSharp
{    
    public class PdfWatermarkService : IWatermarkService
    {        
        /// Entry method to apply watermark        
        public void Apply(string inputPath, string outputPath, WatermarkConfig config)
        {
            // Open existing PDF in modify mode
            var document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);

            foreach (var page in document.Pages)
            {
                // Graphics object to draw on PDF page
                var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                // Dynamic font size (auto-scale if not provided)
                double fontSize = config.FontSize > 0
                    ? config.FontSize
                    : Math.Min(page.Width, page.Height) / 8;

                // FONT LOADER (Uses custom font from Fonts folder)
                var font = CreateFont(config.FontName, fontSize);

                //  COLOR RESOLVER (convert string → XColor)
                var color = GetColor(config.Color);

                // Apply transparency using Alpha
                var brush = new XSolidBrush(
                    XColor.FromArgb(config.Alpha, color.R, color.G, color.B));

                // Decide watermark type (single / repeated grid)
                if (config.Repeat)
                    DrawGrid(gfx, page, config, font, brush);
                else
                    DrawSingle(gfx, page, config, font, brush);
            }

            // Save updated PDF
            document.Save(outputPath);
        }

        /// Ensures watermark does NOT go outside page boundary       
        private void DrawSingle(XGraphics gfx, PdfPage page,
               WatermarkConfig config, XFont font, XBrush brush)
        {
            var textSize = gfx.MeasureString(config.Text, font);

            double margin = config.Margin;

            double x = 0;
            double y = 0;

            // PDF COORDINATE (BOTTOM-LEFT ORIGIN)
            switch (config.Placement)
            {
                case WatermarkPlacement.TopLeft:
                    x = margin;
                    y = page.Height - textSize.Height - margin;
                    break;

                case WatermarkPlacement.TopCenter:
                    x = (page.Width - textSize.Width) / 2;
                    y = page.Height - textSize.Height - margin;
                    break;

                case WatermarkPlacement.TopRight:
                    x = page.Width - textSize.Width - margin;
                    y = page.Height - textSize.Height - margin;
                    break;

                case WatermarkPlacement.MiddleLeft:
                    x = margin;
                    y = (page.Height - textSize.Height) / 2;
                    break;

                case WatermarkPlacement.Center:
                    x = (page.Width - textSize.Width) / 2;
                    y = (page.Height - textSize.Height) / 2;
                    break;

                case WatermarkPlacement.MiddleRight:
                    x = page.Width - textSize.Width - margin;
                    y = (page.Height - textSize.Height) / 2;
                    break;

                case WatermarkPlacement.BottomLeft:
                    x = margin;
                    y = margin;
                    break;

                case WatermarkPlacement.BottomCenter:
                    x = (page.Width - textSize.Width) / 2;
                    y = margin;
                    break;

                case WatermarkPlacement.BottomRight:
                    x = page.Width - textSize.Width - margin;
                    y = margin;
                    break;
            }
         
            // Converts PDF bottom-left → top-left behavior
            y = page.Height - y - textSize.Height;
           

            // Calculate center of text (required for rotation)
            double centerX = x + textSize.Width / 2;
            double centerY = y + textSize.Height / 2;

            // Move origin to center of text
            gfx.TranslateTransform(centerX, centerY);

            // Apply rotation
            gfx.RotateTransform(config.Angle);

            // Draw text centered at (0,0)
            gfx.DrawString(
                config.Text,
                font,
                brush,
                new XRect(-textSize.Width / 2, -textSize.Height / 2, textSize.Width, textSize.Height),
                XStringFormats.Center
            );

            // Reset transformation
            gfx.RotateTransform(-config.Angle);
            gfx.TranslateTransform(-centerX, -centerY);

        }
       
        /// Draw repeated watermark (grid style)
        /// Useful for confidential / security documents       
        private void DrawGrid(XGraphics gfx, PdfPage page,
             WatermarkConfig config, XFont font, XBrush brush)
        {
            var textSize = gfx.MeasureString(config.Text, font);

            double stepX = page.Width / config.GridX;
            double stepY = page.Height / config.GridY;

            for (int i = 0; i < config.GridX; i++)
            {
                for (int j = 0; j < config.GridY; j++)
                {
                    double x = stepX * i + (stepX - textSize.Width) / 2;
                    double y = stepY * j + (stepY - textSize.Height) / 2;

                    y = page.Height - y - textSize.Height;

                    gfx.DrawString(
                        config.Text,
                        font,
                        brush,
                        new XRect(x, y, textSize.Width, textSize.Height),
                        XStringFormats.TopLeft
                    );
                }
            }
        }
              
        /// Converts string color → PdfSharp color
        /// Extendable for more colors later        
        private XColor GetColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return XColors.Gray;

            color = color.Trim().ToLower();

            //  Basic named colors
            return color switch
            {
                "red" => XColors.Red,
                "green" => XColors.Green,
                "blue" => XColors.Blue,
                "black" => XColors.Black,
                "gray" => XColors.Gray,
                "grey" => XColors.Gray,
                "white" => XColors.White,
                _ => ParseCustomColor(color) // fallback to custom
            };
        }

        private XColor ParseCustomColor(string color)
        {
            try
            {
                //  HEX support (#RRGGBB)
                if (color.StartsWith("#"))
                {
                    var hex = color.Replace("#", "");

                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

                        return XColor.FromArgb(r, g, b);
                    }
                }

                // fallback
                return XColors.Gray;
            }
            catch
            {
                return XColors.Gray;
            }
        }

        /// Maps font name → font file path inside Fonts folder
        
        private string GetFontPath(string fontName)
        {
            return fontName.ToLower() switch
            {
                "arial" => "Fonts/arial.ttf",
                "times new roman" => "Fonts/times.ttf",
                "calibri" => "Fonts/calibri.ttf",
                _ => "Fonts/arial.ttf" // fallback
            };
        }

        ///  FONT LOADER
        /// Loads font from local Fonts folder
        /// Falls back to system font if not found
        
        private XFont CreateFont(string fontName, double fontSize)
        {
            try
            {
                // Now this will use CustomFontResolver internally
                return new XFont(fontName, fontSize, XFontStyle.Bold);
            }
            catch
            {
                return new XFont("Arial", fontSize, XFontStyle.Bold);
            }
        }
    }
}