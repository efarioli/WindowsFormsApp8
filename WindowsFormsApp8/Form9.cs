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
    public partial class Form9 : Form
    {
        private static Form9 inst;
        public static Form9 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form9();
                return inst;
            }
        }

        private static List<string> storeNames = new List<string>();
        private static List<int> years = new List<int>();
       
        private static List<int> listWeeks = new List<int>();
        private static HashSet<Date> dates = new HashSet<Date>();

        private static List<string> supplierTypes = new List<string>();
        
        private static bool flagYear = false;
        private static bool flagWeek = false;
        private static bool flagSupplier = false;


        private static string storeName;
        private static  string supplierType;

        private static int year = new int();
        private static int week;

        private static bool highlight;
        private static bool average;
        private static bool threeLeast;
        private static bool threeMost;

        

        //private Date date = new Date();
        //private Store store = new Store();

        public Form9()
        {
            storeNames = Data.Instance.GetStoreNames();//it is already loaded in memory
            supplierTypes = Data.Instance.GetSuplierTypes().ToList();
            years = Data.Instance.GetYears().ToList();
            supplierTypes.Sort();
            years.Sort();
            dates = Data.Instance.GetDates();
            listWeeks = Data.Instance.GetWeeks(dates, years.ElementAt(0)).ToList();
            listWeeks.Sort();

            storeName = storeNames.ElementAt(0);
            supplierType = supplierTypes.ElementAt(0);
            year = years.ElementAt(0);
            week = listWeeks.ElementAt(0);
            highlight = false;
            average = false;
            threeLeast = false;
            threeMost = false;

            

            InitializeComponent();

            comboBox1.DataSource = supplierTypes;
            comboBox4.DataSource = storeNames;
            comboBox1.DataSource = supplierTypes;
            comboBox2.DataSource = years.ToList();
            comboBox3.DataSource = listWeeks;

        }

        private void Form9_Load(object sender, EventArgs e)
        {
            

            StripLine stripLine = new StripLine();
            chart1.ChartAreas[0].AxisY.StripLines.Add(stripLine);

        }

        private async void UpdateChart(Store store, string supplierType, Date date)
        {
            Dictionary<Date, decimal> totalByStorePerSupplierTypeAndDates = await Task.Run(() => Data.Instance.DoGetTotalsPerStorePerSupplierTypeAndDates(store, supplierType));
            
            label3.Text = String.Format("The total cost of all orders for store \"{0}\"  by Supplier type of \"{1}\" in week {2} of {3} is: {4}",
                store.StoreLocation, supplierType, date.Week, date.Year, totalByStorePerSupplierTypeAndDates[date].ToString("C"));
            label3.BackColor = Color.LightYellow;
            Dictionary<int, decimal> dicPerYear = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalByStorePerSupplierTypeAndDates, date.Year));

            //totalBySupplierTypeAndDates[date]

            PrepareChart(dicPerYear);

            Dictionary<string, decimal> TotalsPerStorePerDatesAndSupplierTypes = await Task.Run(() => Data.Instance.DoGetTotalsPerStorePerDateAndSupplierTypes(store, date));
            PrepareChart2(TotalsPerStorePerDatesAndSupplierTypes);

            Dictionary<string, decimal> TotalsPerSupplierTypePerDateAndStores = await Task.Run(() => Data.Instance.DoGetTotalsPerSupplierTypePerDateAndStores(supplierType,date));
            //PrepareChart3(TotalsPerSupplierTypePerDateAndStores);

            Dictionary<string, decimal>  summ = Data.Instance.GetSummaryOfDictionaryForGraPhic(TotalsPerSupplierTypePerDateAndStores, comboBox4.Text);
            PrepareChart3(summ);

            


        }

        private void PrepareChart(Dictionary<int, decimal> newDic)
        {
            chart1.Series.RemoveAt(0);
            
            string seriesName = String.Format("Sales for Store \"{0}\", by suplytype \"{1}\" in {2}",
                storeName, supplierType, year);
            Series ser1 = new Series(seriesName, 1);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;
            chart1.BorderlineColor = Color.AliceBlue;

            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);







            ShowMostThreeValues(newDic, chart1, threeMost);
            ShowLeastThreeValues(newDic, chart1, threeLeast);
            ShowHighLight(chart1, newDic[Convert.ToInt32(comboBox3.Text)], highlight);
            ShowStripAverage(chart1, newDic);


        }
        private void PrepareChart2(Dictionary<string, decimal> newDic)
        {
            SortedDictionary<string, decimal> newDic2 = new SortedDictionary<string, decimal>(newDic);
            chart2.Series.RemoveAt(0);
            string seriesName = "Comparative Table\nFrom stores with less Sell to the Most";
            Series ser1 = new Series(seriesName, 1);
            chart2.Series.Add(ser1);
            chart2.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart2.BackColor = Color.Transparent;
            chart2.ChartAreas[0].BackColor = Color.Transparent;
            chart2.BorderlineColor = Color.AliceBlue;                       
            chart2.Series[seriesName].Points.DataBindXY(newDic2.Keys, newDic2.Values);           
           
            ShowHighLight(chart2, newDic2[comboBox1.Text], highlight);
            ShowStripAverage(chart2, newDic);


        }
        private void PrepareChart3(Dictionary<string, decimal> newDic)
        {
            chart3.Series.RemoveAt(0);
            string seriesName = "Comparative Table\nFrom stores with less Sell to the Most";
            Series ser1 = new Series(seriesName, 1);
            chart3.Series.Add(ser1);
            chart3.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart3.BackColor = Color.Transparent;
            chart3.ChartAreas[0].BackColor = Color.Transparent;

            chart3.BorderlineColor = Color.AliceBlue;



            chart3.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            //chart1.Series[0].Points[week].Color = Color.DarkOrange;
            //double p = (double)newDic[week];
            // chart2.Series[0].Points.FindByValue(p).BorderColor = Color.DarkOrange;
            //chart2.Series[0].Points.FindByValue(p).BorderWidth = 2;

            double avg = (double)newDic.Values.Average();
            StripLine stripline = new StripLine();
            stripline.Text = "Average";
            stripline.Interval = 0;
            stripline.IntervalOffset = avg;//averagege value of the y axis; eg: 35
            stripline.StripWidth = 10;
            stripline.BackColor = Color.DarkRed;

            //if (highlight)
            //{
            //    double p = (double)newDic["_" +comboBox4.Text];
            //    chart3.Series[0].Points.FindByValue(p).BorderColor = Color.DarkOrange;
            //    chart3.Series[0].Points.FindByValue(p).BorderWidth = 2;
            //}
            ShowHighLight(chart3, newDic["_" + comboBox4.Text], highlight);

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            setupVariables();
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();
           
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();

        }


        private void setupVariables()
        {
            try
            {
                threeMost = checkBox4.Checked;
                threeLeast = checkBox3.Checked;
                average = checkBox2.Checked;
                highlight = checkBox1.Checked;
                storeName = comboBox4.SelectedItem.ToString();
                supplierType = comboBox1.SelectedItem.ToString();
                year = Convert.ToInt32(comboBox2.SelectedItem.ToString());
                week = Convert.ToInt32(comboBox3.SelectedItem.ToString());
            }
            catch (Exception ex)
            {

            }
            Date date = new Date { Year = year, Week = week };
            Store store = Data.Instance.GetStore(storeName);
            UpdateChart(store, supplierType, date);
        }
        private void ShowHighLight(Chart chart, decimal element, bool flag)
        {
            if (highlight)
            {
                double p = (double) element;
                chart.Series[0].Points.FindByValue(p).BorderColor = Color.DarkOrange;
                chart.Series[0].Points.FindByValue(p).BorderWidth = 2;
            }
        }
        private void  ShowStripAverage<T>(Chart chart, Dictionary<T, decimal> dic)
        {
            try
            {
                chart.ChartAreas[0].AxisY.StripLines.RemoveAt(0);
            }
            catch (Exception ex)
            {

            }
            if (average)
            {
               
                double avg = (double)dic.Values.Average();
                StripLine stripline = new StripLine();
                stripline.Interval = 0;
                stripline.IntervalOffset = avg;//averagege value of the y axis; eg: 35
                stripline.StripWidth = 10;
                stripline.BackColor = Color.Red;
                chart.ChartAreas[0].AxisY.StripLines.Add(stripline);
            }
            
        }
        private void ShowLeastThreeValues<T>(Dictionary<T, decimal> dic, Chart chart, bool flag)
        {
            if (threeLeast)
            {
                foreach (var entry in Data.Instance.GetLowestThreeValuesFromDictionary(dic))
                {
                    double pp = (double)entry.Value;
                    chart.Series[0].Points.FindByValue(pp).Color = Color.Red;
                    chart.Series[0].Points.FindByValue(pp).BorderColor = Color.Red;
                    chart.Series[0].Points.FindByValue(pp).BorderWidth = 5;
                }
            }
            
        }
        private void ShowMostThreeValues<T>(Dictionary<T, decimal> dic, Chart chart, bool flag)
        {
            if (threeMost)
            {
                foreach (var entry in Data.Instance.GetHighestThreeValuesFromDictionary(dic))
                {
                    double pp = (double)entry.Value;
                    chart.Series[0].Points.FindByValue(pp).Color = Color.Green;
                }
            }
               
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagSupplier)
            {
                setupVariables();
            } else
            {
                flagSupplier = true;
            }
            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagYear)
            {
                setupVariables();
            } else
            {
                flagYear = true;
            }
            
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagWeek)
            {
                setupVariables();
            } else
            {
                flagWeek = true;
            }
            
        }

        private void Form9_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();

        }
    }
}
