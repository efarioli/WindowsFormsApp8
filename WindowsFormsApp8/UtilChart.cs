using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp8
{
    public static class UtilChart
    {
        public static void ShowHighLight(Chart chart, decimal element, bool highlight)
        {
            if (highlight)
            {
                double p = (double)element;
                chart.Series[0].Points.FindByValue(p).BorderColor = Color.DarkOrange;
                chart.Series[0].Points.FindByValue(p).BorderWidth = 2;
            }
        }
        public static void ShowStripAverage<T>(Chart chart, Dictionary<T, decimal> dic, bool average)
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
        public static void ShowLeastThreeValues<T>(Dictionary<T, decimal> dic, Chart chart, bool threeLeast)
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
        public static void ShowMostThreeValues<T>(Dictionary<T, decimal> dic, Chart chart, bool threeMost)
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
    }
}
