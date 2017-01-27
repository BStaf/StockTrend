using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;

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
        public StockLoggedData getLoggedData(string ticker, DateTime startDT, DateTime endDT,int timeSlices)
        {
            int minutesToGrab = 0;
            StockLoggedData retVal;
            startDT = setToBeginningOfTradingDay(startDT);
            endDT = setToEndOfTradingDay(endDT);
            int loggedDays = getLoggedDays(startDT, endDT);
            minutesToGrab = (int)((loggedDays * 6.5 * 60) / timeSlices);
            try
            {
                var stockDataPointList = getLoggedDataFromDataBase(ticker, startDT, endDT, minutesToGrab);
                float lastDaysPrice = getLastDaysPrice(ticker, startDT);
                retVal = new StockLoggedData()
                {
                    lastDaysPrice = lastDaysPrice,
                    maxPrice = stockDataPointList.Max(m => m.price),
                    minPrice = stockDataPointList.Min(m => m.price),
                    loggedData = stockDataPointList,
                    tickerName = ticker
                };
            }
            catch (Exception E)
            {
                throw new Exception("failed to query for logged data. See inner exception.", E);
            }
            return retVal;

            //throw new Exception("Not yet implemented");
        }
        public List<StockLoggedData> getLoggedData(List<string> tickerList, DateTime startDT, DateTime endDT, int timeSlices)
        {
            throw new Exception("Not yet implemented");
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
        /*private DateTime fixTradingDteTimeToTradableDay(DateTime dt)
        {
            if ((int)dt.DayOfWeek == 0)
                return dt.AddDays(+1);
            if ((int)dt.DayOfWeek == 6)
                return dt.AddDays(-1);
            return dt;
        }*/
        private float getLastDaysPrice(string ticker, DateTime startDT)
        {
            DateTime dt = startDT.AddDays(-1);
            if ((int)dt.DayOfWeek == 0)
                dt = dt.AddDays(-1);
            if ((int)dt.DayOfWeek == 6)
                dt = dt.AddDays(-1);
            dt = setToBeginningOfTradingDay(dt);
            //get the most recent value of the stock for the last traded day
            return (float)(db.StockQuoteLogs
                .Where(sq => (sq.timeStamp > dt)
                                && (sq.timeStamp < startDT))
                .OrderByDescending(sq => sq.timeStamp)
                .Select(sqp => sqp.lastSale).FirstOrDefault());

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
            return dt.AddHours((dt.Hour * -1) + 4)
                        .AddMinutes((dt.Minute * -1) + 1);
        } 
        private List<StockDataPoint> getLoggedDataFromDataBase(string ticker, DateTime startDT, DateTime endDT, int minutesToGrab)
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
            return (from data in db.StockQuoteLogs
                    //let e = new {(data.timeStamp.Ticks / divider) / 90}
                    let d = EntityFunctions.AddMinutes(
                                               DateTime.MinValue,
                                               EntityFunctions.DiffMinutes(DateTime.MinValue, data.timeStamp) / minutesToGrab)
                    where /*id == data.stockIndexID &&*/ startDT < data.timeStamp && endDT > data.timeStamp //&& t.tickerName == ticker
                    join t in db.StockIndexes on
                       data.stockIndexID equals t.ID
                    where t.tickerName == ticker
                    group data
                        by d into dg
                    select new StockDataPoint
                    {
                        price = dg.Average(data => data.lastSale),
                        volume = dg.Average(data => data.volume),
                        timeStamp = dg.Min(data => data.timeStamp),
                    }).OrderBy(dt => dt.timeStamp).ToList();
        }
    }
}
