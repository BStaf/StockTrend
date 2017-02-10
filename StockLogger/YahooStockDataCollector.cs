using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace StockLogger
{
    public class YahooStockDataCollector : IStockDataCollector
    {
        /// <summary>
        /// Returns latest trading data on the passed ticker returning a StockDataObject. 
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        public StockDataObject getLatestStockDataWithTicker(string ticker)
        {
            try
            {
                var data = getLatestStockDataWithTickerList(new List<string>() { ticker });
                return data[ticker.ToUpper()];
            }
            catch (Exception E)
            {
                throw new Exception("failed to get data.", E);
            }
            
        }
        /// <summary>
        /// Returns dictionary of returned stock data with the ticker symbol as the key and a StockDataObject as the value. 
        /// Keys are always in caps whether passed that way or not.
        /// </summary>
        /// <param name="tickerList"></param>
        /// <returns></returns>
        public Dictionary<string, StockDataObject> getLatestStockDataWithTickerList(List<string> tickerList)
        {
            Dictionary<string, StockDataObject> retDict = new Dictionary<string,StockDataObject>();
            var httpClient = new HttpClient();
            string uri = getYahooURI(tickerList);

            using (httpClient = new HttpClient())
            {
                try
                {
                    string result = Task.Run(() => httpClient.GetStringAsync(uri)).Result;
                    foreach (var item in result.Split('\n'))
                    {
                        if (item == "") 
                            break;
                        try
                        {
                            StockDataObject sdObj = parseDataString(item);
                            retDict.Add(sdObj.ticker.ToUpper(), sdObj);
                        }
                        catch (Exception) { }//possibly due to bad ticker. Ignore.
                    }
                    retDict = removeOutOfRangeLoggs(retDict);
                    return retDict;
                }
                catch (Exception E)
                {
                    throw new Exception("failed to get data.", E);
                }
            }
        }
        
        /// <summary>
        /// returns new dictionary that comprises of only logged values that are within a valid trading timespan
        /// </summary>
        /// <param name="dataDict"></param>
        /// <returns></returns>
        private Dictionary<string, StockDataObject> removeOutOfRangeLoggs(Dictionary<string, StockDataObject> dataDict)
        {
            Dictionary<string, StockDataObject> retDict = new Dictionary<string, StockDataObject>();
            foreach (var item in dataDict)
            {
                if (checkIfWithingValidTimeRange(item.Value.timestamp))
                    retDict.Add(item.Key,item.Value);
            }
            return retDict;
        }
        /// <summary>
        /// cheks if logged value is way outside normal trading times
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool checkIfWithingValidTimeRange(DateTime time)
        {
            if ((time.Hour > 16) || (time.Hour < 9))
                return false;
            if ((time.Hour == 16) && (time.Minute > 1))
                return false;
            if ((time.Hour == 9) && (time.Minute < 30))
                return false;
            return true;
            
        }
        /// <summary>
        /// creates properly fomrated yahoo finance request path for supplied tickers
        /// </summary>
        /// <param name="tickerList"></param>
        /// <returns></returns>
        private string getYahooURI(List<string> tickerList)
        {
            if (tickerList.Count == 0)
                return null;
            string tickerStr = tickerList[0];
            if (tickerList.Count > 1)
                tickerStr = tickerList.Aggregate((x, y) => x + "+" + y);
            return @"http://finance.yahoo.com/d/quotes.csv?s=" + tickerStr + @"&f=sd1t1l1v";
        }
        private DateTime makeDateTimeFromDateAndTimeStrings(string date, string time)
        {
            return Convert.ToDateTime(date + " " + time);
        }
        private string cleanDataString(string dataStr)
        {
            return dataStr.Replace("\"", "").Replace(@"\", "").Replace("\n","");
        }
        private StockDataObject parseDataString(string dataStr)
        {
            dataStr = cleanDataString(dataStr);
            List<string> dataList = dataStr.Split(',').ToList();
            if (dataList.Count < 5)
                return null;
            return new StockDataObject()
            {
                ticker = dataList[0],
                price = Convert.ToSingle(dataList[3]),
                volume = Convert.ToSingle(dataList[4]),
                timestamp = makeDateTimeFromDateAndTimeStrings(dataList[1],dataList[2])
            };
        }
    }
}
