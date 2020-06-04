using ExtensionMethods;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtensionMethods
{
    using WindowsFormsApp8;

    public static class MyExtensions
    {
        public static OrderDetail Toorderdetail(this string str)
        {
            string[] strparts = str.Split(',');


            return new OrderDetail
            {
                SuplierName = strparts[0],
                SuplierType = strparts[1],
                cost = Convert.ToDecimal(strparts[2])
            };

        }
        public static string ToStringFormatted(this string str)
        {
            string[] strparts = str.Split(',');
            string currency = strparts[1].Substring(0, 2);
            currency += "---";
            currency += strparts[1].Substring(2, 1); ;

            return strparts[0] + " " + currency;

        }

        //static Index ToIndex(this string str)
        //{
        //    string[] strparts = Path.GetFileNameWithoutExtension(str).Split('_');


        //    return new Index
        //    {
        //        Id = strparts[0],
        //        Year = Convert.ToInt32(strparts[2]),
        //        Week = Convert.ToInt32(strparts[1])
        //    };
        //}

    }
}


namespace WindowsFormsApp8
{
    static class Program
    {
        static string storeCodesFile = "StoreCodes.csv";
        static string folderPath = "StoreData";
        static string storeDataFolder = "StoreData";



        static Dictionary<string, Store> stores = new Dictionary<string, Store>();
        static ConcurrentDictionary<Index, IEnumerable<OrderDetail>> orders55 = new ConcurrentDictionary<Index, IEnumerable<OrderDetail>>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        static public ConcurrentDictionary<Index, IEnumerable<OrderDetail>> getOrders()
        {
            return orders55;
        }
        public static void LoadData()
        {
            //ConcurrentDictionary<Index, IEnumerable<OrderDetail>> ordersTemp = new ConcurrentDictionary<Index, IEnumerable<OrderDetail>>();
            //Dictionary<string, Store> stores = new Dictionary<string, Store>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string storeCodesFilePath = Directory.GetCurrentDirectory() + @"\" + folderPath + @"\" + storeCodesFile;
            string[] storeCodesData = File.ReadAllLines(storeCodesFilePath);
            int countStore = 0;
            foreach (var storeData in storeCodesData)
            {
                string[] storeDataSplit = storeData.Split(',');
                Store store = new Store { StoreCode = storeDataSplit[0], StoreLocation = storeDataSplit[1] };
                if (!stores.ContainsKey(store.StoreCode))
                    stores.Add(store.StoreCode, store);


                countStore++;
            }
            int countBigLoop = 0;
            string[] fileNames = Directory.GetFiles(folderPath + @"\" + storeDataFolder);


            fileNames.AsParallel().ForAll(filePath =>
            // Parallel.ForEach(fileNames, filePath =>
            // foreach (string filePath in fileNames)
            {
                //Index idx = new Index
                //{
                //    Id = Path.GetFileNameWithoutExtension(filePath).Split('_')[0],
                //    Year = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath).Split('_')[2]),
                //    Week = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath).Split('_')[1])
                //    //filePath.ToIndex()

                //};
                orders55.TryAdd(new Index
                {
                    Id = Path.GetFileNameWithoutExtension(filePath).Split('_')[0],

                    DateF =new Date
                    {
                        Year = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath).Split('_')[2]),
                        Week = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath).Split('_')[1])
                    }
                    //    //filePath.ToIndex()
                }
                    ,

                    File.ReadAllLines(folderPath + @"\" + storeDataFolder + @"\" + Path.GetFileName(filePath))/*.AsParallel()*/

                .Select(
                    x => x.Toorderdetail()
                    )
                    .ToList()
                    )
                    ;



            });

            stopWatch.Stop();
            MessageBox.Show("TimeToLoad: " + stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("TimeToLoad: " + stopWatch.Elapsed.TotalSeconds);
            //return ordersTemp;


        }
        private static decimal queryTotals(string storeId, int year, int week, string suplierName, string suplierType)
        {

            var xxx = orders55.AsParallel().AsEnumerable()
                .Where(x => x.Key.Id == (FirstValueOrDefault(storeId, x.Key.Id)))
                .Where(x => x.Key.DateF.Year == (FirstValueOrDefault(year, x.Key.DateF.Year)))
                .Where(x => x.Key.DateF.Week == (FirstValueOrDefault(week, x.Key.DateF.Week)))
                ;

            ConcurrentBag<decimal> sumTotal = new ConcurrentBag<decimal>();

            ////Parallel.ForEach(xxx, entry =>
            //            //foreach (KeyValuePair<Index, IEnumerable<OrderDetail>> entry in xxx)
            xxx.AsParallel().ForAll(entry =>
            {
                sumTotal.Add(
                    entry.Value/*.IeList*/  /*.AsParallel()*/
                    .Where(z => z.SuplierName == (FirstValueOrDefault(suplierName, z.SuplierName)))
                    .Where(z => z.SuplierType == (FirstValueOrDefault(suplierType, z.SuplierType)))
                    .Select(x => x.cost)
                    .Sum()
                    )
                    ;
            });

            return sumTotal.AsParallel().Sum();
        }
        public static T FirstValueOrDefault<T>(T gen1, T gen2)
        {
            if (gen1 == null || gen1.Equals(0))
            {
                return gen2;
            }
            return gen1;
        }
    }
}
