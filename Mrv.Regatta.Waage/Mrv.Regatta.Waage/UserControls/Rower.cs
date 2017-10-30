using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.UserControls
{
    public class Rower
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RowerType Type { get; set; }
        public RowerStatus Status { get; set; }

        public float? WeightInfo { get; set; }
    }

}
