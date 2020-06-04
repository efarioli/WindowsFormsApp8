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
    public partial class Form5 : Form
    {
        private static Form5 inst;
        public static Form5 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form5();
                return inst;
            }
        }
        public Form5()
        {
            InitializeComponent();

        }


        private bool flagYear = false;
        private bool flagWeek = false;

        
        private void Form5_Load(object sender, EventArgs e)
        {
            Dictionary<string, Store> stores = Data.Instance.GetStores();
            HashSet<int> years = Data.Instance.GetYears();
            HashSet<Date> dates = Data.Instance.GetDates();
            List<string> storeNames = Data.Instance.GetStoreNames();//it is already loaded in memory
            comboBox1.DataSource = storeNames;

            List<int> years2 = years.ToList();
            years2.Sort();
            comboBox2.DataSource = years2;
            List<int> listA = Data.Instance.GetWeeks(dates, Convert.ToInt32(comboBox2.SelectedValue.ToString())).ToList();
            listA.Sort();

            comboBox3.DataSource = listA;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Date date = SetupDate();
            Store store = SetupStore();
            UpdateChart(store, date);
            //UpdateChart2(store, date);




        }
        private Date SetupDate()
        {
            HashSet<Date> dates = Data.Instance.GetDates();

            List<int> years = Data.Instance.GetYears().ToList();
            years.Sort();
            int selectedYear = years.ElementAt(0);
            List<int> weeks = Data.Instance.GetWeeks(dates, selectedYear).ToList();
            weeks.Sort();
            int selectedWeek = weeks.ElementAt(0);//default value
            try
            {
                selectedWeek = Convert.ToInt32(comboBox3.SelectedItem.ToString());
                selectedYear = Convert.ToInt32(comboBox2.SelectedItem.ToString());
            }
            catch (Exception ex)
            {

            }

            Date date = new Date { Year = selectedYear, Week = selectedWeek };
            return date;
        }
        private Store SetupStore()
        {
            string storeName = comboBox1.SelectedItem.ToString();
            string key = "";
            foreach (var entry in Data.Instance.GetStores())
            {
                if (entry.Value.StoreLocation == storeName)
                {
                    key = entry.Key;
                    break;
                }
            }
            Store store = Data.Instance.GetStores()[key];
            return store;

        }
        private async void UpdateChart(Store store, Date date)
        {
            Dictionary<Date, decimal> totalPerStoreByweek = await Task.Run(() => Data.Instance.DoGetTotalsPerStoreByWeeks(store));

            Dictionary<int, decimal> dic = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalPerStoreByweek, date.Year));
            Dictionary<Store, decimal> totalsByWeekPerStores = await Task.Run(() => Data.Instance.DoGetTotalsByWeekPerStores(date));

            Dictionary<string, decimal> totalsByWeekPerStores2 = ConvertDictionaryStoreToString(totalsByWeekPerStores);
            Dictionary<string, decimal> totalsByWeekPerStores3 = GetMaxMinAvg(totalsByWeekPerStores2, store.StoreLocation);

            Dictionary<string, decimal> newDic = GetMaxMinAvg(dic, date.Week);

            label4.Text = String.Format("The total cost of all orders for store \"{0}\" for week {1} of {2} is: {3}", store.StoreLocation, date.Week, date.Year, newDic.ElementAt(2).Value.ToString("C"));
            PrepareChart2(dic);
            PrepareChart(newDic);
            PrepareChart3(totalsByWeekPerStores3);


        }

        private Dictionary<string, decimal> ConvertDictionaryStoreToString(Dictionary<Store, decimal> totalsByWeekPerStores)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();
            foreach(var entry in totalsByWeekPerStores)
            {
                result.Add(entry.Key.StoreLocation, entry.Value);
            }
            return result;
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
        private Dictionary<string, decimal> GetMaxMinAvg(Dictionary<string, decimal> totalByStore, string storeName)
        {
            Dictionary<string, decimal> newDic = new Dictionary<string, decimal>();
            decimal min = totalByStore.Values.Min();
            decimal avg = totalByStore.Values.Average();
            decimal max = totalByStore.Values.Max();
            foreach (var entry in totalByStore)
            {
                if (entry.Value == min)
                {
                    newDic.Add(entry.Key, entry.Value);
                }

            }
            Store averageStore = new Store { StoreLocation = storeName };
            newDic.Add("average week", avg);

            string keySelected = "";
            foreach (var entry in totalByStore)
            {
                if (entry.Key == storeName)
                {
                    keySelected = "_" + entry.Key;
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
            string seriesName = String.Format(
                "Week {0} of {1}for store \"{2}\" compared with other weeks for the same store." +
                "\nHover over the chart for more information", comboBox3.Text, comboBox2.Text, comboBox1.Text);
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";

            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart1.Series[0].Points[2].Color = Color.DarkOrange;
            

            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;

        }
        private void PrepareChart2(Dictionary<int, decimal> newDic)
        {

            chart2.Series.RemoveAt(0);
            string seriesName = String.Format(
                "Totals of sales for store {0}, arranged by weeks for {1}\nHover over the chart for more information"
                , comboBox1.Text, comboBox2.Text);
            Series ser1 = new Series(seriesName, 3);
            chart2.Series.Add(ser1);
            chart2.Series[0].ToolTip = "#VALX:  #VAL{C}";

           

            chart2.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart2.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";
            double pp = (double)newDic[Convert.ToInt32( comboBox3.Text )];
            chart2.Series[0].Points.FindByValue(pp).Color = Color.DarkOrange;

        }
        private void PrepareChart3(Dictionary<string, decimal> newDic)
        {

            chart3.Series.RemoveAt(0);
            string seriesName = String.Format(
                "Week {0} of {1} for store \"{2}\" compared with the same week for different stores."+
                "Hover over the chart for more information",
                comboBox3.Text, comboBox2.Text, comboBox1.Text
                );
            Series ser1 = new Series(seriesName, 3);
            chart3.Series.Add(ser1);
            chart3.Series[0].ToolTip = "#VALX:  #VAL{C}";

            chart3.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart3.Series[0].Points[2].Color = Color.DarkOrange;
            chart3.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";
            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Date date = SetupDate();
            Store store = SetupStore();
            if (!flagYear)
            {
                flagYear = true;
            }
            else
            {
                UpdateChart(store, date);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Date date = SetupDate();
            Store store = SetupStore();

            if (!flagWeek)
            {
                flagWeek = true;
            }
            else
            {
                UpdateChart(store, date);
            }
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
