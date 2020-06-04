using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WindowsFormsApp8;

namespace WindowsFormsApp8
{
    public partial class Form1 : Form
    {
        private static string storeCodesFile = "StoreCodes.csv";
        private static string folderPath = "StoreData";
        private static string storeDataFolder = "StoreData";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadData();

        }
        private async void LoadData()
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            fdb.Description = "Select the folder where is located the data";
            fdb.ShowNewFolderButton = false;
            fdb.SelectedPath = Environment.CurrentDirectory;       

            if (fdb.ShowDialog() == DialogResult.OK &&  File.Exists(fdb.SelectedPath +"\\" + storeCodesFile)
                    && Directory.Exists(fdb.SelectedPath + "\\" + folderPath))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                button1.Enabled = false;
                Data d = Data.Instance;
                
                await Task.Run(() => d.LoadData(fdb.SelectedPath));
                stopwatch.Stop();
                groupBox1.Enabled = true;

                MessageBox.Show("Time for load was: " +stopwatch.Elapsed.TotalSeconds + " seconds", "Load Completed!");
                

            }
            else
            {
                MessageBox.Show(button1, "Folder not contains data");

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2.GetForm.Show();
            Form2.GetForm.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form3.GetForm.Show();
            Form3.GetForm.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form4.GetForm.Show();
            Form4.GetForm.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form5.GetForm.Show();
            Form5.GetForm.Focus();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form6.GetForm.Show();
            Form6.GetForm.Focus();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form7.GetForm.Show();
            Form7.GetForm.Focus();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form8.GetForm.Show();
            Form8.GetForm.Focus();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Form10.GetForm.Show();
            Form10.GetForm.Focus();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Form9.GetForm.Show();
            Form9.GetForm.Focus();
        }
    }
}
