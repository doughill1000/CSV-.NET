using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eQSelectCurveDesno
{
    class Design
    {
        public String order { get; set; }
        public String designNum { get; set; }
        public String designName { get; set; }
        public List<EngravingCurve> json { get; set; }
    }
}
