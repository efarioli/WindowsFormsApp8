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
    public partial class Form7 : Form
    {
        private static Form7 inst;
        public static Form7 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form7();
                return inst;
            }
        }
        private List<string> supplierTypes = new List<string>();
        Dictionary<string, decimal> totalBySupplierType;
        public Form7()
        {
            supplierTypes = Data.Instance.GetSuplierTypes().ToList();
            supplierTypes.Sort();
            InitializeComponent();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = supplierTypes;
        }

        private async void UpdateChart(string suplierType)
        {
            if (totalBySupplierType == null)
            {
                totalBySupplierType = await Task.Run(() => Data.Instance.DoGetTotalsPerSupplierTypes());
            }
            
            label3.Text = String.Format("The total cost of all orders for all stores by Supplier \"{0}\"  is: {1}", suplierType, totalBySupplierType[suplierType].ToString("C"));
            
            PrepareChart(totalBySupplierType);



        }

        private void PrepareChart(Dictionary<string, decimal> newDic)
        {
            chart1.Series.RemoveAt(0);
            string seriesName = "Comparative Table\nFrom stores with less Sell to the Most";
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            chart1.BackColor = Color.Transparent;
            chart1.ChartAreas[0].BackColor = Color.Transparent;
            chart1.BorderlineColor = Color.AliceBlue;



            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSuplierType = comboBox1.SelectedItem.ToString();
            UpdateChart(selectedSuplierType);
        }

        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
