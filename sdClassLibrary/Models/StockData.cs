using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;

namespace sdClassLibrary.Models
{
    public class StockDataPoint
    {
        public float price { get; set; }
        public float volume { get; set; }
        public DateTime timeStamp { get; set; }
    }
    /// <summary>
    /// Holds logged data for one stock
    /// </summary>
    public class StockLoggedData
    {
        public List<StockDataPoint> loggedData { get; set; }
        public string tickerName  {get;set;}
        public float maxPrice {get;set;}
        public float minPrice {get;set;}
        public float lastDaysPrice { get; set; } // price of stock before first days price for percent chart
    }
    public interface IStockData
    {
        List<string> getTickerList();
        List<StockLoggedData> getLoggedData(List<string> tickerList, DateTime startDT, DateTime endDT, int timeSlices);
        StockLoggedData getLoggedData(string ticker, DateTime startDT, DateTime endDT, int timeSlices);
    }
    public class StockData : IStockData
    {
        private stdataEntities db = new stdataEntities();

        //private StockData(int timeSlices)
        public List<string> getTickerList()
        {
            try
            {
                return db.StockIndexes.Select(si => si.tickerName).ToList();
            }
            catch (Exception E)
            {
                throw new Exception("Failed to Connect to Database. See inner Exception.", E);
            }
            
        }
        public StockLoggedData getLoggedData(string ticker, DateTime startDT, DateTime endDT, int timeSlices)
        {
            if (timeSlices == 0) return null;
            StockLoggedData retVal;
            startDT = setToBeginningOfTradingDay(startDT);
            endDT = setToEndOfTradingDay(endDT);
            int minutesToGrab = (int)Math.Round((float)getLoggedSpanInMinutes(startDT, endDT) / timeSlices);
            
            return getStockLoggedDataObjectForTicker(ticker,startDT,endDT,minutesToGrab);
        }
        public List<StockLoggedData> getLoggedData(List<string> tickerList, DateTime startDT, DateTime endDT, int timeSlices)
        {
            if (timeSlices == 0) return null;
            List<StockLoggedData> retDataList = new List<StockLoggedData>();
            startDT = setToBeginningOfTradingDay(startDT);
            endDT = setToEndOfTradingDay(endDT);
            int minutesToGrab = (int)Math.Round((float)getLoggedSpanInMinutes(startDT, endDT) / timeSlices);
            //var data = getLoggedDataFromDataBase(tickerList, startDT, endDT, minutesToGrab);
            foreach (string ticker in tickerList)
            {
                retDataList.Add(getStockLoggedDataObjectForTicker(ticker, startDT, endDT, minutesToGrab));
            }
            return retDataList;
           // throw new Exception("Not yet implemented");
        }
        private StockLoggedData getStockLoggedDataObjectForTicker(string ticker, DateTime startDT, DateTime endDT, int minutesToGrab)
        {
            StockLoggedData retVal;
            try
            {
                int tickerID = db.StockIndexes.Where(t => t.tickerName == ticker).FirstOrDefault().ID;
                var stockDataPointList = getLoggedDataFromDataBase(tickerID, startDT, endDT, minutesToGrab);
                float lastDaysPrice = getLastDaysPrice(tickerID, startDT);
                retVal = new StockLoggedData()
                {
                    lastDaysPrice = lastDaysPrice,
                    maxPrice = stockDataPointList.Max(m => m.price),
                    minPrice = stockDataPointList.Min(m => m.price),
                    loggedData = stockDataPointList,
                    tickerName = ticker
                };
                if (retVal.maxPrice < lastDaysPrice) retVal.maxPrice = lastDaysPrice;
                else if (retVal.minPrice > lastDaysPrice) retVal.minPrice = lastDaysPrice;
            }
            catch (Exception E)
            {
                throw new Exception("failed to query for logged data. See inner exception.", E);
            }
            return retVal;
        }
        
        
        /// <summary>
        /// gets the number of days that data was logged (skips weekends)
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        private int getLoggedDays(DateTime t1, DateTime t2)
        {//WRONG
            //datetime's DayOfWeek = 0-6. 0 = Sunday
            int retVal = 0;
            if ((int)t1.DayOfWeek == 6) t1 = t1.AddDays(+1);
            if ((int)t1.DayOfWeek == 0) t1 = t1.AddDays(+1);
            if ((int)t2.DayOfWeek == 0) t1 = t1.AddDays(-1);
            if ((int)t2.DayOfWeek == 6) t1 = t1.AddDays(-1);
            //t1 = fixTradingDAteTimeToTradableDay(t1);
            //t2 = fixTradingDAteTimeToTradableDay(t2);
            //int endDayOfWeek = getValidDayOfWeek(t2);
            int totalDays = (int)t2.Subtract(t1).TotalDays;
            int fullWeeks = (int)Math.Floor((double)(totalDays / 7));
            if (fullWeeks == 0)
                return totalDays;
            fullWeeks -= 1;
            retVal = (int)t1.DayOfWeek + (7 - (int)t2.DayOfWeek) + (fullWeeks * 7);
            return retVal;
        }
        private int getLoggedSpanInMinutes(DateTime startDT, DateTime endDT)
        {
            //startDT = startDT.AddDays(-1);
            startDT = startDT.AddMinutes(-startDT.Minute).AddHours(-startDT.Hour);
            try
            {
                var span = db.LoggedTimeRanges
                    .Where(l => (l.LogDate >= startDT) && (l.LogDate < endDT))
                    .Sum(l => DbFunctions.DiffMinutes(l.StartTime, l.StopTime));
                if (span == null)
                    return 0;
                return (int)span;
            }
            catch (Exception E)
            {
                throw E;
            }
        }
        private float getLastDaysPrice(int tickerID, DateTime startDT)
        {
            DateTime dt = startDT.AddDays(-1);
            if ((int)dt.DayOfWeek == 0)
                dt = dt.AddDays(-1);
            if ((int)dt.DayOfWeek == 6)
                dt = dt.AddDays(-1);
            dt = setToBeginningOfTradingDay(dt);
            //get the most recent value of the stock for the last traded day
            return (float)db.LoggedDatas
                .Where(sq => (sq.timestamp > dt)
                                && (sq.timestamp < startDT)
                                && (sq.stockID == tickerID))
                .OrderByDescending(sq => sq.timestamp)
                .Select(sqp => sqp.price).FirstOrDefault();
            //return (float)data.FirstOrDefault();
            //.FirstOrDefault());

        }
        private DateTime setToBeginningOfTradingDay(DateTime dt)
        {
            //condtion start and stop date times to fall within trading times
            //fix start date time to start at 9:30 am
            return dt.AddHours((dt.Hour * -1) + 9)
                      .AddMinutes((dt.Minute * -1) + 20);
        }
        private DateTime setToEndOfTradingDay(DateTime dt)
        {
            //fix stop date time to stop before 4:00 am
            return dt.AddHours((dt.Hour * -1) + 16)
                        .AddMinutes((dt.Minute * -1) + 1);
        }
        private List<StockDataPoint> getLoggedDataFromDataBase(int tickerID, DateTime startDT, DateTime endDT, int minutesToGrab)
        {
            //query collects all logged data between start and stop dates
            //uses SQL DATEADD and DATEDIFF to calculate the minutes from some arbitrary start time (DateTime.Min)
            //  and divides that by the minutesToGrab. This produces a number that if rounded down will be the same 
            //  for all logged data in that minutesToGrab block. Rounding down happens automatically due to SQL
            //  converting the real number to an integer.
            //The data is grouped by this number and averages or other calculations can be made.
            //This will put a strain on the SQL server, but if not there, it would happen on the web server.
            //This also cuts down on the amount of data passed.
            //I will have to tune the SQL server to handle this query more efficiently
            var sData = (from data in db.LoggedDatas
                    //let e = new {(data.timeStamp.Ticks / divider) / 90}
                    where tickerID == data.stockID
                        && startDT < data.timestamp
                        && endDT > data.timestamp 
                    let d = DbFunctions.AddMinutes(
                                               DateTime.MinValue,
                                               DbFunctions.DiffMinutes(DateTime.MinValue, data.timestamp) / minutesToGrab)
                    
                    group data
                        by d into dg
                    select new StockDataPoint
                    {
                        price = (float)dg.Average(data => data.price),
                        volume = (float)dg.Average(data => data.volume),
                        timeStamp = dg.Min(data => data.timestamp),
                    }).OrderBy(dt => dt.timeStamp);//.ToList();
            return sData.ToList();
        }
       /* private Dictionary<string , List<StockDataPoint>> getLoggedDataFromDataBase(List<string> tickers, DateTime startDT, DateTime endDT, int minutesToGrab)
        {
            Dictionary<string , List<StockDataPoint>> retDict = new Dictionary<string,List<StockDataPoint>>();
            Dictionary <int,string> idToSymbolDictionary = db.StockIndexes.Where(si => tickers.Contains(si.tickerName)).ToDictionary(si => si.ID, si => si.tickerName);
            /*var sData = (from data in db.LoggedDatas
                         //let e = new {(data.timeStamp.Ticks / divider) / 90}
                         where idToSymbolDictionary.Keys.Contains(data.stockID) 
                            && startDT < data.timestamp && endDT > data.timestamp
                         group data by data.stockID into dataG

                         from stock in dataG
                         let d = DbFunctions.AddMinutes(
                                                    DateTime.MinValue,
                                                    DbFunctions.DiffMinutes(DateTime.MinValue, stock.timestamp) / minutesToGrab)
                         group stock by d into dg

                         select new //StockDataPoint
                         {
                             stockID = dg.Min(d => d.stockID),
                             //name = dg.Min(d => d.StockIndex.tickerName),
                             price = (float)dg.Average(d => d.price),
                             volume = (float)dg.Average(d => d.volume),
                             timeStamp = dg.Min(d => d.timestamp),
                         }
                         ).OrderBy(dt => dt.timeStamp).ToList();//.ToList();
            foreach (var item in sData)
            {
                string ticker = idToSymbolDictionary[item.stockID];
                if (!retDict.ContainsKey(ticker))
                    retDict.Add(ticker, new List<StockDataPoint>());

                retDict[ticker].Add(new StockDataPoint()
                {
                    price = item.price,
                    volume = item.volume,
                    timeStamp = item.timeStamp
                });
            }*//*
            return retDict;// sData.ToList();
        }*/
    }
}
