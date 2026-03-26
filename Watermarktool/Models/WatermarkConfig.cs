using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watermark.core.Enum;

namespace Watermark.core.Models
{
    public class WatermarkConfig
    {
        public string Text { get; set; } = "CONFIDENTIAL";
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 0; // auto-scale
        public string Color { get; set; } = "Red";
        public int Alpha { get; set; } = 70;
        public WatermarkPlacement Placement { get; set; } = WatermarkPlacement.Center;
        public double Angle { get; set; } = 0;
        public bool Repeat { get; set; } = false;
        public int GridX { get; set; } = 3;
        public int GridY { get; set; } = 3;
        public double Margin { get; set; } = 40;
    }
}
