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
    public partial class Form10 : Form
    {
        private static Form10 inst;
        public static Form10 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form10();
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

        Series ser2 = new Series("", 2);

        private static string storeName;
        private static string supplierType;

        private static int year = new int();
        private static int week;

        private static bool highlight;
        private static bool average;
        private static bool threeLeast;
        private static bool threeMost;
        public Form10()
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
            //comboBox3.DataSource = listWeeks;
        }

        private void Form10_Load(object sender, EventArgs e)
        {

        }

        private async void UpdateChart(Store store, string supplierType, Date date)
        {
            Dictionary<Date, decimal> totalByStorePerSupplierTypeAndDates = await Task.Run(() => Data.Instance.DoGetTotalsPerStorePerSupplierTypeAndDates(store, supplierType));



            label3.Text = String.Format("The total cost of all orders for store \"{0}\"  by Supplier type of \"{1}\" for all the data provided is: {2}",
                store.StoreLocation, supplierType, totalByStorePerSupplierTypeAndDates.Values.Sum().ToString("C"));
            label3.BackColor = Color.LightYellow;
            Dictionary<int, decimal> dicPerYear = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalByStorePerSupplierTypeAndDates, date.Year));

            //totalBySupplierTypeAndDates[date]

            Dictionary<Date, decimal> totalByPerSupplierTypeAndDates = await Task.Run(() => Data.Instance.DoGetTotalsPerSupplierTypeAndDates(supplierType));
            Dictionary<int, decimal> totalByPerSupplierTypeAndDates_1year = Data.Instance.GetTotalPerWeekOneYear(totalByPerSupplierTypeAndDates, date.Year);


            Dictionary<int, decimal> totalByPerSupplierTypeAndDates_1yearAvg = new Dictionary<int, decimal>();


            int stCount = storeNames.Count();
            foreach (var entry in totalByPerSupplierTypeAndDates_1year)
            {
                totalByPerSupplierTypeAndDates_1yearAvg.Add(entry.Key, entry.Value / stCount);
            }

            PrepareChart(dicPerYear , totalByPerSupplierTypeAndDates_1yearAvg);

            //Dictionary<string, decimal> TotalsPerStorePerDatesAndSupplierTypes = await Task.Run(() => Data.Instance.DoGetTotalsPerStorePerDateAndSupplierTypes(store, date));
            //PrepareChart2(TotalsPerStorePerDatesAndSupplierTypes);

            //Dictionary<string, decimal> TotalsPerSupplierTypePerDateAndStores = await Task.Run(() => Data.Instance.DoGetTotalsPerSupplierTypePerDateAndStores(supplierType, date));
            ////PrepareChart3(TotalsPerSupplierTypePerDateAndStores);

            //Dictionary<string, decimal> summ = Data.Instance.GetSummaryOfDictionaryForGraPhic(TotalsPerSupplierTypePerDateAndStores, comboBox4.Text);
            //groupBox1.Text = "" + summ.Count;
            //PrepareChart3(summ);




        }

        private void PrepareChart(Dictionary<int, decimal> newDic, Dictionary<int, decimal> newDic2)
        {
            chart1.Series.RemoveAt(0);
            string printa = ""; 
            foreach(var entry in chart1.Series)
            {
                printa += entry.Name;
            }
            //label3.Text = printa;
            try
            {
                chart1.Series.Remove(ser2);
                chart1.Series.RemoveAt(1); chart1.Series.RemoveAt(2); 
                

            }
            catch (Exception exe)
            {

            }
            


            string seriesName = String.Format("Sales for Store \"{0}\", by suplytype \"{1}\" in {2}",
                storeName, supplierType, year);
            Series ser1 = new Series(seriesName, 1);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;
            chart1.BorderlineColor = Color.AliceBlue;

            chart1.Series[0].AxisLabel = "#VALX";
            


            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);

            

           
            

            
            if (checkBox1.Checked)
            {
                string seriesName2 = String.Format(
                    "Average sales for all the stores for supplier type \"{0}\" arranged by the weeks of {1}",
                    comboBox1.Text, comboBox2.Text);

                ser2 = new Series(seriesName2, 2);
                chart1.Series.Add(ser2);

                chart1.Series[seriesName2].ChartType = SeriesChartType.Point;
                chart1.Series[seriesName2].Color = Color.Green;
                chart1.Series[seriesName2].BorderColor = Color.Black;
                chart1.Series[seriesName2].BorderWidth = 3;
                chart1.Series[seriesName2].Points.DataBindXY(newDic2.Keys, newDic2.Values);
                chart1.Series[1].ToolTip = "#VALX:  #VAL{C}";
                chart1.BackColor = Color.Transparent;
                // chart1.ChartAreas[1].BackColor = Color.Transparent;
                chart1.BorderlineColor = Color.AliceBlue;
            }
            







            UtilChart.ShowMostThreeValues(newDic, chart1, threeMost);
            UtilChart.ShowLeastThreeValues(newDic, chart1, threeLeast);
          //  UtilChart.ShowHighLight(chart1, newDic[Convert.ToInt32(comboBox3.Text)],highlight );
            UtilChart.ShowStripAverage(chart1, newDic, average);


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
               // week = Convert.ToInt32(comboBox3.SelectedItem.ToString());
            }
            catch (Exception ex)
            {

            }
            Date date = new Date { Year = year, Week = week };
            Store store = Data.Instance.GetStore(storeName);
            UpdateChart(store, supplierType, date);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            setupVariables();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagSupplier)
            {
                setupVariables();
            }
            else
            {
                flagSupplier = true;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagYear)
            {
                setupVariables();
            }
            else
            {
                flagYear = true;
            }
        }

        private void Form10_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            setupVariables();

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
