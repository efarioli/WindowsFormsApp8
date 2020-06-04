using ExtensionMethods;
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
    public partial class Form3 : Form
    {
        private static Form3 inst;
        public static Form3 GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed)
                    inst = new Form3();
                return inst;
            }
        }
        public Form3()
        {
            InitializeComponent();

        }
        Dictionary<Store, decimal> totalByStore;

        private void Form3_Load(object sender, EventArgs e)
        {
            List<string> storeNames = Data.Instance.GetStoreNames();//it is already loaded in memory
            comboBox1.DataSource = storeNames;
           


        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = comboBox1.SelectedItem.ToString();
            UpdateChart(selected);
        }

        private async void UpdateChart(string selected)
        {

            if (totalByStore == null)
            {
                totalByStore = await Task.Run(() =>  Data.Instance.DoGetTotalsByStores());

            }
            Dictionary<string, decimal> newDic = GetMaxMinAvg(totalByStore, selected);

            label2.Text = String.Format("The total cost of all orders in store \"{0}\"  is: {1}", selected, newDic.ElementAt(2).Value.ToString("C"));

            PrepareChart(newDic);



            

        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Form3_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private Dictionary<string,decimal> GetMaxMinAvg(Dictionary<Store, decimal> totalByStore, string selected)
        {
            Dictionary<string, decimal> newDic = new Dictionary<string, decimal>();
            decimal min = totalByStore.Values.Min();
            decimal avg = totalByStore.Values.Average();
            decimal max = totalByStore.Values.Max();
            foreach (var entry in totalByStore)
            {
                if (entry.Value == min)
                {
                    newDic.Add(entry.Key.StoreLocation, entry.Value);
                }

            }
            Store averageStore = new Store { StoreCode = "AVER", StoreLocation = "AVERAGE" };
            newDic.Add(averageStore.StoreLocation, avg);

            string keySelected = "";
            foreach (var entry in totalByStore)
            {
                if (entry.Key.StoreLocation == selected)
                {
                    keySelected = "_" + entry.Key.StoreLocation;
                    newDic.Add(keySelected, entry.Value);
                }

            }

            foreach (var entry in totalByStore)
            {
                if (entry.Value == max)
                {
                    newDic.Add(entry.Key.StoreLocation, entry.Value);
                }

            }
            return newDic;
        }
        private void PrepareChart (Dictionary<string, decimal> newDic)
        {
            chart1.Series.RemoveAt(0);
            string seriesName = "Comparative chart\nfrom the store which has sold the least to the one which sold the most";
            Series ser1 = new Series(seriesName, 3);
            chart1.Series.Add(ser1);
            chart1.Series[0].ToolTip = "#VALX:  #VAL{C}";

            chart1.Series[seriesName].Points.DataBindXY(newDic.Keys, newDic.Values);
            chart1.Series[0].Points[2].Color = Color.DarkOrange;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{C}";

        }
    }
}
