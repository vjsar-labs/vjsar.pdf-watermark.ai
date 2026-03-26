using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watermark.core.Models;

namespace Watermark.core.Interfaces
{
    public interface IWatermarkService
    {
        void Apply(string inputPath, string outputPath, WatermarkConfig config);
    }
}
