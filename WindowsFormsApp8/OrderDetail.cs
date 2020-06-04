using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp8
{
    public class OrderDetail
    {
        public string SuplierName { get; set; }
        public string SuplierType { get; set; }

        public decimal cost { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
