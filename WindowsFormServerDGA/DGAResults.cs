using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormServerDGA
{
    [Serializable]
    public class DGAResults
    {
        public int Id { get; set; }
        public int TransformerId { get; set; }
        public DateTime DT { get; set; }
        public decimal N2 { get; set; }
        public decimal O2 { get; set; }
        public decimal H2 { get; set; }
        public decimal CH4 { get; set; }
        public decimal C2H6 { get; set; }
        public decimal C2H4 { get; set; }
        public decimal C2H2 { get; set; }
        public decimal CO { get; set; }
        public decimal CO2 { get; set; }
    }
}
