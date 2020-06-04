using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp8
{
    class Date
    {
        public int Year { get; set; }
        public int Week { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if(!(obj is Date))
            {
                return false;
            }
            return this.Year == ((Date)obj).Year && this.Week == ((Date)obj).Week;




        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public override string ToString()
        {
            string year = "0000";
            string week = "00";
            year += this.Year;
            week += this.Week;
            return year.Substring(year.Length - 4, 4) + week.Substring(week.Length - 2, 2);
        }
    }
   
}
