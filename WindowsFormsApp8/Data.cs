using ExtensionMethods;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace WindowsFormsApp8
{
     sealed class Data
    {

        private static string storeCodesFile = "StoreCodes.csv";
        private static string folderPath = "StoreData";
        private static string storeDataFolder = "StoreData";



        static Dictionary<string, Store> stores = new Dictionary<string, Store>();
         static ConcurrentDictionary<Index, IEnumerable<OrderDetail>> orders = new ConcurrentDictionary<Index, IEnumerable<OrderDetail>>();
        static HashSet<Date> dates = new HashSet<Date>();
        static HashSet<int> years = new HashSet<int>();
        static List<string> locations = new List<string>();
        static HashSet<string> supplierNames = new HashSet<string>();
        static HashSet<string> supplierTypes = new HashSet<string>();


        Dictionary<Date, decimal> totalPerWeek;
        Dictionary<Store, decimal> totalPerStore;

        static Data instance = null;
        static readonly object padlock = new object();

        Data()
        {
        }

        public static Data Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Data();
                    }
                    return instance;
                }
            }
        }


        public /*static*/ void LoadData( string selectedPath)
        {
            folderPath = selectedPath;

             Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string storeCodesFilePath = /*Directory.GetCurrentDirectory() + @"\" +*/ folderPath + @"\" + storeCodesFile;
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
            {
            
            orders.TryAdd(
                
                     GetIndexFromString(Path.GetFileNameWithoutExtension(filePath))
                    ,
                    GetOrdetailArray(File.ReadAllLines(folderPath + @"\" + storeDataFolder + @"\" + Path.GetFileName(filePath)))
                    )
                    ;



            });



            dates = GetDateHashSet(orders);
            years = getYearsFromDate(dates);
            locations = GetStoreLocation(stores);
            DoGetsupplierName(orders);


        }

        

        private static HashSet<int> getYearsFromDate(HashSet<Date> dates)
        {
            HashSet<int> result = new HashSet<int>();
            foreach (var entry in dates)
            {
                result.Add(entry.Year);
            }
            return result;
        }

        private static HashSet<Date> GetDateHashSet(ConcurrentDictionary<Index, IEnumerable<OrderDetail>> dic)
        {
            List<Date> l = dic.AsParallel().Select(x => x.Key.DateF).ToList();
            HashSet<Date> result = new HashSet<Date>(l);
            return result;
        }

        public  ConcurrentDictionary<Index, IEnumerable<OrderDetail>> GetOrders()
        {
            return orders;
        }
        public HashSet<int> GetYears()
        {
            return years;
        }
        public Dictionary<string, Store> GetStores()
        {
            return stores;
        }
        public HashSet<Date> GetDates()
        {
            return dates;
        }

        public Dictionary<Store, decimal> DoGetTotalsByWeekPerStores(Date date)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Store, decimal> resultP = new ConcurrentDictionary<Store, decimal>();
            //foreach (var entry in dates)
            stores.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry.Value, queryTotals(entry.Key, date.Year, date.Week, null, null));

            });
            Dictionary<Store, decimal> result = new Dictionary<Store, decimal>(resultP);



            return result;
        }
        public Dictionary<Date, decimal> DoGetTotalsPerStoreByWeeks(Store st)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Date, decimal> resultP = new ConcurrentDictionary<Date, decimal>();
            //foreach (var entry in dates)
            dates.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(st.StoreCode, entry.Year, entry.Week, null, null));

            });
            Dictionary<Date, decimal> result = new Dictionary<Date, decimal>(resultP);



            return result;
        }
        public Dictionary<Date, decimal> DoGetTotalsPerWeek()
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Date, decimal> resultP = new ConcurrentDictionary<Date, decimal>();
            //foreach (var entry in dates)
            dates.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(null, entry.Year, entry.Week, null, null));

            });
            Dictionary<Date, decimal> result = new Dictionary<Date, decimal>(resultP);


            totalPerWeek = result;

            return result;
        }
        public Dictionary<string, decimal> DoGetTotalsPerSupplierName()
        {
            ConcurrentDictionary<string, decimal> resultP = new ConcurrentDictionary<string, decimal>();
            supplierNames.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(null, 0, 0, entry, null));

            });
            Dictionary<string, decimal> result = new Dictionary<string, decimal>(resultP);



            return result;
        }
        public Dictionary<string, decimal> DoGetTotalsPerSupplierTypes()
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<string, decimal> resultP = new ConcurrentDictionary<string, decimal>();
            //foreach (var entry in dates)
            supplierTypes.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(null, 0, 0, null, entry));

            });
            Dictionary<string, decimal> result = new Dictionary<string, decimal>(resultP);



            return result;
        }
        public Dictionary<Date, decimal> DoGetTotalsPerSupplierTypeAndDates(string supplierType)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Date, decimal> resultP = new ConcurrentDictionary<Date, decimal>();
            //foreach (var entry in dates)
            dates.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(null, entry.Year, entry.Week, null, supplierType));

            });
            Dictionary<Date, decimal> result = new Dictionary<Date, decimal>(resultP);



            return result;
        }
        public Dictionary<Date, decimal> DoGetTotalsPerStorePerSupplierTypeAndDates(Store store, string supplierType)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Date, decimal> resultP = new ConcurrentDictionary<Date, decimal>();
            //foreach (var entry in dates)
            dates.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(store.StoreCode, entry.Year, entry.Week, null, supplierType));

            });
            Dictionary<Date, decimal> result = new Dictionary<Date, decimal>(resultP);



            return result;
        }
        public Dictionary<string, decimal> DoGetTotalsPerStorePerDateAndSupplierTypes(Store store, Date date)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<string, decimal> resultP = new ConcurrentDictionary<string, decimal>();
            //foreach (var entry in dates)
            supplierTypes.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry, queryTotals(store.StoreCode, date.Year, date.Week, null, entry));

            });
            Dictionary<string, decimal> result = new Dictionary<string, decimal>(resultP);



            return result;
        }
        public Dictionary<string, decimal> DoGetTotalsPerSupplierTypePerDateAndStores(string supplierType, Date date)
        {
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<string, decimal> resultP = new ConcurrentDictionary<string, decimal>();
            //foreach (var entry in dates)
            stores.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry.Value.StoreLocation, queryTotals(entry.Key, date.Year, date.Week, null, supplierType));

            });
            Dictionary<string, decimal> result = new Dictionary<string, decimal>(resultP);



            return result;
        }

        public Dictionary<Store, decimal> DoGetTotalsByStores()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // ConcurrentDictionary<Date,Date > dates2 = new ConcurrentDictionary<Date,Date>(dates);
            ConcurrentDictionary<Store, decimal> resultP = new ConcurrentDictionary<Store, decimal>();
            //foreach (var entry in dates)
            stores.AsParallel().ForAll(entry =>
            {
                resultP.TryAdd(entry.Value, queryTotals(entry.Key, 0, 0, null, null));

            });
            Dictionary<Store, decimal> result = new Dictionary<Store, decimal>(resultP);
            stopwatch.Stop();
            //  MessageBox.Show("" + stopwatch.Elapsed);

            this.totalPerStore = result;

            return result;
        }
        public  decimal queryTotals(string storeId, int year, int week, string suplierName, string suplierType)
        {

            var xxx = orders.AsParallel().AsEnumerable()
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
                    entry.Value /*.IeList*/  /*.AsParallel()*/
                    .Where(z => z.SuplierName == (FirstValueOrDefault(suplierName, z.SuplierName)))
                    .Where(z => z.SuplierType == (FirstValueOrDefault(suplierType, z.SuplierType)))
                    .Select(x => x.cost)
                    .Sum()
                    )
                    ;
            });

            return sumTotal.AsParallel().Sum();
        }
        public /*static*/ T FirstValueOrDefault<T>(T gen1, T gen2)
        {
            if (gen1 == null || gen1.Equals(0))
            {
                return gen2;
            }
            return gen1;
        }
        public  decimal GetBigTotal()
        {
            return queryTotals(null, 0, 0, null, null);
        }
        static List<string> GetStoreLocation(Dictionary<string, Store> stores)
        {
            List<string> result = new List<string>();
            foreach(var entry in stores)
            {
                result.Add(entry.Value.StoreLocation);
            }
            return result;
        }
        public  List<string> GetStoreNames()
        {
            return locations;
        }
        public Dictionary<Date, decimal>  GetTotalPerWeek()
        {
            return this.totalPerWeek;
        }
        public Dictionary<int, decimal> GetTotalPerWeekOneYear(Dictionary<Date, decimal> dic, int year)
        {
            Dictionary<int, decimal> result = new Dictionary<int, decimal>();
            foreach (var entry in dic)
            {
                if (entry.Key.Year == year)
                {
                    result.Add(entry.Key.Week, entry.Value);
                }
            }
            return result;
        }
        public HashSet<int> GetWeeks(HashSet<Date> dates, int year)
        {
            HashSet<int> result = new HashSet<int>();
            foreach (var entry in dates)
            {
                if (entry.Year == year)
                {
                    result.Add(entry.Week);
                }

            }
            return result;
        }

        private static void DoGetsupplierName(ConcurrentDictionary<Index, IEnumerable<OrderDetail>> dic)
        {
            ConcurrentDictionary<string, string> supplierNamesP = new ConcurrentDictionary<string, string>();
            ConcurrentDictionary<string, string> supplierTypeP = new ConcurrentDictionary<string, string>();

            dic.AsParallel().ForAll(x =>
            {
                foreach (var y in x.Value)
                {
                    supplierNamesP.TryAdd(y.SuplierName, null);
                    supplierTypeP.TryAdd(y.SuplierType, null);


                }
            });
            supplierNames = new HashSet<string>(supplierNamesP.Keys);
            supplierTypes = new HashSet<string>(supplierTypeP.Keys);


        }
       
        public HashSet<string> GetSuplierNames()
        {
            return supplierNames;
        }
        public HashSet<string> GetSuplierTypes()
        {
            return supplierTypes;
        }
        public Store GetStore(string storeName)
        {
            foreach(var entry in stores)
            {
                if (storeName == entry.Value.StoreLocation)
                    return entry.Value;
            }
            return null;
        }
        public  Dictionary<string, decimal> GetSummaryOfDictionaryForGraPhic<T>(Dictionary<T, decimal> dic, T element)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();            
            Dictionary<string, decimal> temp = dic.AsEnumerable().OrderBy(z => z.Value).ToDictionary(t => "" + t.Key, t => t.Value);
            for (int i = 0; i < 3; i++)
            {
                result.Add(temp.ElementAt(i).Key, temp.ElementAt(i).Value);

            }
            result.Add("_" +element, dic[element]);

            for (int i = temp.Count-3; i < temp.Count; i++)
            {
                result.Add(temp.ElementAt(i).Key, temp.ElementAt(i).Value);

            }



            return result;
        }
        public Dictionary<T, decimal> GetLowestThreeValuesFromDictionary<T>(Dictionary<T, decimal> dic)
        {
            Dictionary<T, decimal> result = new Dictionary<T, decimal>();
            Dictionary<T, decimal> temp = dic.AsEnumerable().OrderBy(z => z.Value).ToDictionary(t => t.Key, t => t.Value);
            for (int i = 0; i < 3; i++)
            {
                result.Add(temp.ElementAt(i).Key, temp.ElementAt(i).Value);

            }
            return result;
        }
        public Dictionary<T, decimal> GetHighestThreeValuesFromDictionary<T>(Dictionary<T, decimal> dic)
        {
            Dictionary<T, decimal> result = new Dictionary<T, decimal>();
            Dictionary<T, decimal> temp = dic.AsEnumerable().OrderByDescending(z => z.Value).ToDictionary(t => t.Key, t => t.Value);
            for (int i = 0; i < 3; i++)
            {
                result.Add(temp.ElementAt(i).Key, temp.ElementAt(i).Value);

            }
            return result;
        }

        private static OrderDetail[] GetOrdetailArray (string[] strArray)
        {


            int size = strArray.Length;
            OrderDetail[] result = new OrderDetail[size];

            for (int i = 0; i < size; i++)
            {
                result[i] = strArray[i].Toorderdetail();
            }
            return result;



        }
        private static Index GetIndexFromString(string s)
        {           
            return new Index {
                Id = s.Split('_')[0],
                DateF = new Date
                {
                    Year = Convert.ToInt32(s.Split('_')[2]),
                    Week = Convert.ToInt32(s.Split('_')[1])
                }
            };
        }
        private static OrderDetail StringToOrderDetail(string s)
        {
            return new OrderDetail
            {
                SuplierName =(string) s.Split(',')[0],
                SuplierType = (string) s.Split(',')[1],
                cost = (decimal) Convert.ToDecimal(s.Split(',')[2])
            };
        }

    }
}
