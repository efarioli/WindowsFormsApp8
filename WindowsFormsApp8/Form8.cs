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
    public partial class Form8 : Form
    {
        private static Form8 inst;
        public static Form8 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form8();
                return inst;
            }
        }
        private List<string> supplierTypes = new List<string>();
        Dictionary<string, decimal> totalBySupplierType;
        private List<int> years;
        private List<int> listWeeks;
        private HashSet<Date> dates;



        private bool flagYear = false;
        private bool flagWeek = false;

        public Form8()
        {
            supplierTypes = Data.Instance.GetSuplierTypes().ToList();
            years = Data.Instance.GetYears().ToList();
            supplierTypes.Sort();
            years.Sort();
            dates = Data.Instance.GetDates();
            InitializeComponent();

        }

        private void Form8_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = supplierTypes;
            HashSet<int> years = Data.Instance.GetYears();
            HashSet<Date> dates = Data.Instance.GetDates();
            List<int> years2 = years.ToList();
            years2.Sort();
            comboBox2.DataSource = years2;
            List<int> listWeeks = Data.Instance.GetWeeks(dates, Convert.ToInt32(comboBox2.SelectedValue.ToString())).ToList();
            listWeeks.Sort();

            listWeeks = Data.Instance.GetWeeks(dates, Convert.ToInt32(comboBox2.SelectedValue.ToString())).ToList();
            listWeeks.Sort();
            comboBox3.DataSource = listWeeks;

        }
        private async void UpdateChart(string suplierType, Date date)
        {
            Dictionary<Date, decimal> totalBySupplierTypeAndDates = await Task.Run(() => Data.Instance.DoGetTotalsPerSupplierTypeAndDates(suplierType));
            label3.Text = String.Format("The total cost of all orders for all stores by Supplier type of \"{0}\" in week {1} of {2} is: {3}", 
                suplierType, date.Week, date.Year , totalBySupplierTypeAndDates[date].ToString("C"));
            Dictionary<int, decimal> dicPerYear = await Task.Run(() => Data.Instance.GetTotalPerWeekOneYear(totalBySupplierTypeAndDates, date.Year));

            //totalBySupplierTypeAndDates[date]
            PrepareChart(dicPerYear, date.Week);



        }

        private void PrepareChart(Dictionary<int, decimal> newDic, int week)
        {
            chart1.Series.RemoveAt(0);
            string seriesName = String.Format(
                "Totals of sales for Supplier type \"{0}\", arranged by weeks for {1}\nHover over the chart for more information",
                comboBox1.Text, comboBox2.Text
                );
            Series ser1 = new Series(seriesName, 1);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;

            chart1.BorderlineColor = Color.AliceBlue;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 52;
            chart1.ChartAreas[0].AxisX.Interval = 4;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";



            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            //chart1.Series[0].Points[week].Color = Color.DarkOrange;
            double p = (double) newDic[week];
            chart1.Series[0].Points.FindByValue(p).BorderColor = Color.DarkOrange;
            chart1.Series[0].Points.FindByValue(p).BorderWidth = 2;

            StripLine stripline = new StripLine();
            stripline.Interval = 0;
            stripline.IntervalOffset = 80000;//average value of the y axis; eg: 35
stripline.StripWidth = 10;
            stripline.BackColor = Color.DarkRed;
            chart1.ChartAreas[0].AxisY.StripLines.Add(stripline);

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSuplierType = comboBox1.SelectedItem.ToString();
            Date date = SetupDate();
            UpdateChart(selectedSuplierType, date);
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

        private void Form8_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSuplierType = comboBox1.SelectedItem.ToString();

            Date date = SetupDate();
            if (!flagYear)
            {
                flagYear = true;
            }
            else
            {
                UpdateChart(selectedSuplierType, date);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSuplierType = comboBox1.SelectedItem.ToString();

            Date date = SetupDate();
            if (!flagWeek)
            {
                flagWeek = true;
            }
            else
            {
                UpdateChart(selectedSuplierType, date);
            }
        }
    }
}
