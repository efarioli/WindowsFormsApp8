using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp8
{
    public partial class Form2 : Form
    {
        private static Form2 inst;
        public static Form2 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form2();
                return inst;
            }
        }
        public Form2()
        {
            InitializeComponent();

        }

        private ConcurrentDictionary<Index, IEnumerable<OrderDetail>> orders = Data.Instance.GetOrders();
        private HashSet<Date> dates = Data.Instance.GetDates();
        Dictionary<Date, decimal> totalPerWeek;
       

        private  void Form2_Load(object sender, EventArgs e)
        {

            UpdateLabelTotal();
            setUpUI();

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private async void setUpUI()
        {
            HashSet<int> years = Data.Instance.GetYears();
            comboBox1.DataSource = years.ToList();

       }

      


        private Dictionary<int, decimal> GetTotalPerWeekOneYear(Dictionary<Date, decimal> dic, int year)
        {
            Dictionary<int, decimal> result = new Dictionary<int, decimal>();
            foreach(var entry in dic)
            {
                if(entry.Key.Year == year)
                {
                    result.Add(entry.Key.Week, entry.Value);
                }
            }
            return result;
        }
       

        private static Dictionary<int, decimal> getDateOneYear(Dictionary<Date, decimal> dic, int year)
        {
            Dictionary<int, decimal> result = new Dictionary<int, decimal>();
            foreach (var entry in dic)
            {
                if (entry.Key.Year == year)
                {
                    result.Add(entry.Key.Week, entry.Value);
                }
            }
            return result;

        }

        private HashSet<int> getYearsFromDate(HashSet<Date> dates)
        {
            HashSet<int> result = new HashSet<int>();
            foreach(var entry in dates)
            {
                result.Add(entry.Year);
            }
            return result;
        }

         static HashSet<Date> GetDateHashSet(ConcurrentDictionary<Index, IEnumerable<OrderDetail>> dic)
        {
            List<Date> l = dic.AsParallel().Select(x => x.Key.DateF).ToList();
            HashSet<Date> result = new HashSet<Date>(l);
            return result;
        }
      
        
        public static T FirstValueOrDefault<T>(T gen1, T gen2)
        {
            if (gen1 == null || gen1.Equals(0))
            {
                return gen2;
            }
            return gen1;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = Int32.Parse(comboBox1.SelectedItem.ToString());
            UpdateChart(selected);
        }
        private  async void UpdateChart(int year)
        {
            if (Data.Instance.GetTotalPerWeek() == null)
            {
                totalPerWeek = await Task.Run(() => Data.Instance.DoGetTotalsPerWeek());
            }
            else
            {
                totalPerWeek = Data.Instance.GetTotalPerWeek();
            }
                           



            Dictionary<int, decimal> dic = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalPerWeek, year));
            chart1.Series.RemoveAt(0);
            string seriesName = "Year " + year + "\n";
            seriesName += dic.Values.Sum().ToString("C");
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);          

            chart1.Series[0].ToolTip = "week #VALX: #VAL{C}";                        
            chart1.Series[seriesName].Points.DataBindXY(dic.Keys, dic.Values);
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum  = 52;
            chart1.ChartAreas[0].AxisX.Interval = 4;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;

        }
        private async void UpdateLabelTotal()
        {
            decimal total = await Task.Run(() => Data.Instance.GetBigTotal());
            label1.BackColor = Color.LightYellow;
            label1.Text += " " + total.ToString("C");
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
