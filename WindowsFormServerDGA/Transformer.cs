using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormServerDGA
{
    [Serializable]
    public class Transformer
    {
        public int Id { get; set; }
        public int SerialNumber { get; set; }
        public int DesignationId { get; set; }
    }
}
