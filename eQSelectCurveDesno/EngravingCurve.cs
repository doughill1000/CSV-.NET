using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eQSelectCurveDesno
{
    /// <summary>
    /// Class generated from JSON object pulled from AirDye IO
    /// </summary>
    public class EngravingCurve
    {
        public string desno { get; set; }
        public int Layer { get; set; }
        public string cylno { get; set; }
        public string curve { get; set; }
        public int LineScreen { get; set; }
        public double hiVoltage { get; set; }
        public double loVoltage { get; set; }
        public string curveNote { get; set; }
        public string blank { get; set; }
        public string fileName { get; set; }
        public string Verified { get; set; }
        public string EntryDate { get; set; }

    }
}
