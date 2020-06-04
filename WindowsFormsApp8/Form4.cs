using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp8
{
    public partial class Form4 : Form
    {
        private static Form4 inst;
        public static Form4 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form4();
                return inst;
            }
        }
        public Form4()
        {
            InitializeComponent();

        }

        Dictionary<Date, decimal> totalPerweek;

        bool flag = false;

        private void Form4_Load(object sender, EventArgs e)
        {
            setUpUI();
        }
        private async void setUpUI()
        {
            HashSet<int> years = Data.Instance.GetYears();
            HashSet<Date> dates = Data.Instance.GetDates();
            comboBox1.DataSource = years.ToList();
            List<int> listA = GetWeeks(dates, Convert.ToInt32(comboBox1.SelectedValue.ToString())).ToList();
            listA.Sort();

            comboBox2.DataSource = listA;

        }
        private HashSet<int> GetWeeks(HashSet<Date> dates, int year)
        {
            HashSet<int> result = new HashSet<int>();
            foreach(var entry in dates)
            {
                if (entry.Year == year)
                {
                    result.Add(entry.Week);
                }
                
            }
            return result;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedYear = Convert.ToInt32( comboBox1.SelectedItem.ToString() );

            int selectedWeek = 1;//default value
            try
            {
                selectedWeek = Convert.ToInt32(comboBox2.SelectedItem.ToString());
            }
            catch(Exception ex)
            {

            }       

            Date date = new Date { Year = selectedYear, Week = selectedWeek };


            UpdateChart(date);
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            int selectedYear = Convert.ToInt32(comboBox1.SelectedItem.ToString());

            int selectedWeek = 1;//default value
            try
            {
                selectedWeek = Convert.ToInt32(comboBox2.SelectedItem.ToString());
            }
            catch (Exception ex)
            {

            }

            Date date = new Date { Year = selectedYear, Week = selectedWeek };

            if (!flag)
            {
                flag = true;
            } else
            {
                UpdateChart(date);
            }
            
        }

       
        private async void UpdateChart(Date date)
        {
            if (Data.Instance.GetTotalPerWeek() == null)
            {
                totalPerweek = await Task.Run(() => Data.Instance.DoGetTotalsPerWeek());

            } else
            {
                totalPerweek = Data.Instance.GetTotalPerWeek();
            }
            label3.Text = "" + totalPerweek.Count();

            Dictionary<int, decimal> dic = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalPerweek, date.Year));

            Dictionary<string, decimal> newDic = GetMaxMinAvg(dic, date.Week);

            label3.Text = String.Format("The total cost of all orders for all the stores for week {0} of {1} is: {2}", date.Week , date.Year, newDic.ElementAt(2).Value.ToString("C"));

            PrepareChart(newDic);
        }
        private Dictionary<string, decimal> GetMaxMinAvg(Dictionary<int, decimal> totalByStore, int week)
        {
            Dictionary<string, decimal> newDic = new Dictionary<string, decimal>();
            decimal min = totalByStore.Values.Min();
            decimal avg = totalByStore.Values.Average();
            decimal max = totalByStore.Values.Max();
            foreach (var entry in totalByStore)
            {
                if (entry.Value == min)
                {
                    newDic.Add("week" + entry.Key, entry.Value);
                }

            }
            Date averageStore = new Date { Week = week };
            newDic.Add("average week", avg);

            string keySelected = "";
            foreach (var entry in totalByStore)
            {
                if (entry.Key == week)
                {
                    keySelected = "_week" + entry.Key;
                    newDic.Add(keySelected, entry.Value);
                }

            }

            foreach (var entry in totalByStore)
            {
                if (entry.Value == max)
                {
                    newDic.Add("week" + entry.Key, entry.Value);
                }

            }
            return newDic;
        }
        private void PrepareChart(Dictionary<string, decimal> newDic)
        {
            chart1.Series.RemoveAt(0);
            string seriesName = "Comparative chart\nfrom the week which has registered the least sold to the one which sold the most";
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";

            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart1.Series[0].Points[2].Color = Color.DarkOrange;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";

        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }      
        }
    }
}
