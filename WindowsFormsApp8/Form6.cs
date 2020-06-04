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
    public partial class Form6 : Form
    {
        private static Form6 inst;
        public static Form6 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form6();
                return inst;
            }
        }
        List<string> supliernames;
        Dictionary<string, decimal> totalPerSupplierName;

        public Form6()
        {
            supliernames = Data.Instance.GetSuplierNames().ToList();
            supliernames.Sort();
            InitializeComponent();

        }

        private void Form6_Load(object sender, EventArgs e)
        {
            
            comboBox1.DataSource = supliernames;
        }

        private async void UpdateChart(string suplierName)
        {
            decimal totalBySupplierName = await Task.Run(() => Data.Instance.queryTotals(null,0,0, suplierName,null));
            label3.Text = String.Format("The total cost of all orders for all stores by Supplier \"{0}\"  is: {1}", suplierName, totalBySupplierName.ToString("C"));
            decimal bigTotal = Data.Instance.GetBigTotal();
            decimal totalRestOfBrands = bigTotal - totalBySupplierName;

            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            dic.Add(suplierName, totalBySupplierName);
            dic.Add("Rest of the Suppliers", totalRestOfBrands);
            PrepareChart(dic);




        }

        private void PrepareChart(Dictionary<string, decimal> newDic)
        {
            chart1.Series.RemoveAt(0);
            string seriesName = "Comparative Table\nFrom stores with less Sell to the Most";
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;

            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSuplier = comboBox1.SelectedItem.ToString();
            UpdateChart(selectedSuplier);
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
