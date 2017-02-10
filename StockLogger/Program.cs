using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace StockLogger
{
    class Program
    {
        private static IStockDataCollector dataCollector;
        private static IDataLogger dataLogger;
       // private static System.Timers.Timer mainTmr;
        private static int curMinute;
        private static List<string> tickerList;
        static void Main(string[] args)
        {
            curMinute = 0;
            dataCollector = new YahooStockDataCollector();
            dataLogger = new SQLDataLogger();
           // mainTmr = new System.Timers.Timer(1000);
           // mainTmr.Elapsed += timerEvent;
            tickerList = getTickerList(Environment.CurrentDirectory + @"\stocks.txt");
            if (tickerList.Count > 0){
                Console.WriteLine("Begin Logging");
                mainLoop();
                //mainTmr.Enabled = true;    
            }
            else 
            {
                Console.WriteLine("Cannot Find stocks.txt file or file is empty. Quitting.");
            }
        }
        private static List<string> getTickerList(string path)
        {
            List<string> retList = new List<string>();
            try
            {
                var readList = File.ReadAllLines(path).ToList();
                foreach (var item in readList)
                {
                    retList.Add(item);
                }
            }
            catch (Exception) { }
            return retList;
        }
        private static void mainLoop()
        {
            while (true)
            {
                logData();
                Thread.Sleep(1000);
            }
        }
        private static void logData()
        {
            Dictionary<string, StockDataObject> dataDict;
            if (!checkIfWithInTradingTimeSpan()) return;
            if (DateTime.Now.Minute != curMinute)
            {
                Console.WriteLine("Logging for " + DateTime.Now.ToString("M/dd/yyy - hh:mm:ss"));
                foreach (var ticker in tickerList)
                {
                    try
                    {
                        dataDict = dataCollector.getLatestStockDataWithTickerList(tickerList);
                        
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine("Failed To Get Data.");
                        break;
                    }
                    try
                    {
                        dataLogger.pushStockData(dataDict);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed To Log Data.");
                        break;
                    }
                }
                curMinute = DateTime.Now.Minute;
            }
        }
        private static bool checkIfWithInTradingTimeSpan()
        {
            DateTime tm = DateTime.Now;
            if ((tm.Hour > 16) || (tm.Hour < 9))
                return false;
            else if ((tm.Hour == 16) && (tm.Minute > 30))
                return false;
            return true;
        }

    }
}
