using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using sdClassLibrary.Models;
using System.Data.Entity.SqlServer;



namespace sdWebApp.Controllers
{
    public class LoggedDataObj
    {
        public string name { get; set; }
        public DateTime timeStamp { get; set; }
        public float price { get; set; }
        public float percent { get; set; }
        public float volume { get; set; }
    }
    public class TrendDataObj
    {
        public List<LoggedDataObj> loggedDataList { get; set; }
        public float maxValPrice { get; set; }
        public float maxValPercent { get; set; }
        public float minValPrice { get; set; }
        public float minValPercent { get; set; }
        public int maxRecordsPerItem { get; set; }
        public DateTime firstLoggedDT { get; set; } //earliest logged record
        public DateTime lastLoggedDT { get; set; } //latest logged record
    }
    public class TrendDataObjAJS
    {
        public List<LoggedDataObj> loggedDataList { get; set; }
        public List<DateTime> timeStampList { get; set; } //a list of every timestamp in the best case scenario that we have all the data
                                                            //this is used fro reference on the trend so everything stays in sync
        public float maxValPrice { get; set; }
        public float maxValPercent { get; set; }
        public float minValPrice { get; set; }
        public float minValPercent { get; set; }
        //public int maxRecordsPerItem { get; set; }
        //public DateTime firstLoggedDT { get; set; } //earliest logged record
        //public DateTime lastLoggedDT { get; set; } //latest logged record
    }
    [RoutePrefix("api/stocks")]
    public class loggedDataController : ApiController
    {
        private stdataEntities db = new stdataEntities();


        
        [Route("tickerList")]
        [ResponseType(typeof(IEnumerable<string>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetTickerList()
        {
            List<string> retTickerList = new List<string>();
            retTickerList = (from t in db.StockIndexes
                             select t.tickerName).ToList<string>();
            return Ok(retTickerList);
        }
        //both functions return logged data for requested IDs for the given time span
        //in JSON format:
        //              "loggedData":   [
        //                              {"timestamp","tickerId","price","percent","volume"}
        //                              ]
        [Route("{tickers}")]
        [ResponseType(typeof(TrendDataObj))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetLoggedDataWithTickers(string tickers)
        {
            //get stock ID values from string of all tickers
            /*int[] ids = getIDsFromTickerNames(tickers);
            if (ids == null)
            {
                return NotFound();
            }*/
            int trendedDays = 5;
            DateTime startDT = DateTime.Now.AddDays(-trendedDays);
            //startDT = startDT.AddHours((startDT.Hour * -1) + 9);
            //startDT = startDT.AddMinutes((startDT.Minute * -1) + 30);
            //DateTime startDT = getPerviousLoggedDay(DateTime.Now);
            DateTime stopDT = DateTime.Now;
            // TrendDataObj getLoggedData(int[] ids, DateTime startDT, DateTime stopDT, int timeSlices)
            TrendDataObj retVal = getLoggedData(tickers, startDT, stopDT, 200);
            if (retVal == null)
            {
                return NotFound();
            }
            return Ok(retVal);
        }

        [Route("{tickers}/start/{startDT:datetime}/stop/{stopDT:datetime}")]
        [ResponseType(typeof(TrendDataObj))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetLoggedDataWithTickersDateTime(string tickers, DateTime startDT, DateTime stopDT)
        {
            //get stock ID values from string of all tickers
            /*int[] ids = getIDsFromTickerNames(tickers);
            if (ids == null)
            {
                return NotFound();
            }*/
            // TrendDataObj getLoggedData(int[] ids, DateTime startDT, DateTime stopDT, int timeSlices)
            TrendDataObj retVal = getLoggedData(tickers, startDT, stopDT, 200);
            if (retVal == null)
            {
                return NotFound();
            }
            return Ok(retVal);
        }
        [Route("ajs/{tickers}/start/{startDT:datetime}/stop/{stopDT:datetime}")]
        [AcceptVerbs("GET", "POST")]
        [ResponseType(typeof(TrendDataObjAJS))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult AJSGetLoggedDataWithTickersDateTime(string tickers, DateTime startDT, DateTime stopDT)
        {
            //get stock ID values from string of all tickers
            /*int[] ids = getIDsFromTickerNames(tickers);
            if (ids == null)
            {
                return NotFound();
            }*/
            // TrendDataObj getLoggedData(int[] ids, DateTime startDT, DateTime stopDT, int timeSlices)
            TrendDataObjAJS retVal = getLoggedDataAJS(tickers, startDT, stopDT, 200);
            if (retVal == null)
            {
                return NotFound();
            }
            return Ok(retVal);
        }
        [Route("ajs/{tickers}")]
        [AcceptVerbs("GET", "POST")]
        [ResponseType(typeof(TrendDataObjAJS))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult AJSGetLoggedDataWithTickers(string tickers)
        {
            //get stock ID values from string of all tickers
            /*int[] ids = getIDsFromTickerNames(tickers);
            if (ids == null)
            {
                return NotFound();
            }*/
            int trendedDays = 5;
            DateTime startDT = DateTime.Now.AddDays(-trendedDays);
            //startDT = startDT.AddHours((startDT.Hour * -1) + 9);
            //startDT = startDT.AddMinutes((startDT.Minute * -1) + 30);
            //DateTime startDT = getPerviousLoggedDay(DateTime.Now);
            DateTime stopDT = DateTime.Now;
            // TrendDataObj getLoggedData(int[] ids, DateTime startDT, DateTime stopDT, int timeSlices)
            TrendDataObjAJS retVal = getLoggedDataAJS(tickers, startDT, stopDT, 200);
            if (retVal == null)
            {
                return NotFound();
            }
            return Ok(retVal);
        }
        //returns dictioary of the last price for each stock the day before the startDT
        //tickerStrList is unused. This will be in use in the future
        private Dictionary<string, float> getLastDaysPrices(string[] tickerStrList, DateTime startDT)
        {
            Dictionary<string, float> retDict = new Dictionary<string, float>();
            //grab data of the last price of the last day before that start time
            //this is used in the percent values
            var lastPriceQuery = (from data in db.StockQuoteLogs
                                  where data.timeStamp < startDT
                                  
                                  // where t.tickerName == ticker
                                  group data by data.stockIndexID into grp
                                  let LastSalePriceThatDay = grp.Max(g => g.timeStamp)

                                  from data in grp
                                  where data.timeStamp == LastSalePriceThatDay
                                  join t in db.StockIndexes on
                                       data.stockIndexID equals t.ID
                                  select new
                                  {
                                      price = data.lastSale,
                                      name = t.tickerName

                                  });

            //create dictionary of the sale price of the last day before trend period
            foreach (var item in lastPriceQuery)
            {
                retDict.Add((string)item.name, (float)item.price);
                //retDict.Add(item.stockIndexID, (float)item.lastSale);
            }
            return retDict;
        }
        //returns trendDataObj with all logged data for given ids for the time span given
        //timeSlices is the amount of logged data points to get across the given timespan
        private TrendDataObj getLoggedData(string tickerIDString, DateTime startDT, DateTime stopDT, int timeSlices)
        {
            int minutesToGrab = 0;
            
            TrendDataObj retObj = new TrendDataObj();
            DateTime firstDT = DateTime.Now;
            DateTime lastDT = DateTime.Now;
            //Dictionary<string, float> lastPriceDict;// = new Dictionary<int, float>();
            retObj.maxValPrice = 0;
            retObj.maxValPercent = 0;
            retObj.minValPrice = 0;
            retObj.minValPercent = 0;
            retObj.maxRecordsPerItem = 0;

            //condtion start and stop date times to fall within trading times
            //fix start date time to start at 9:30 am
            startDT = startDT.AddHours((startDT.Hour * -1) + 9);
            startDT = startDT.AddMinutes((startDT.Minute * -1) + 20);
            //fix stop date time to stop before 4:00 am
            stopDT = stopDT.AddHours((stopDT.Hour * -1) + 4);
            stopDT = stopDT.AddMinutes((stopDT.Minute * -1) + 1);

            minutesToGrab = getLoggedDays(startDT, DateTime.Now);

            //I get a string in the format of the ticker ID's (ex. "1_2_3_4") this gets parsed into an array of tickers as strings
            if (tickerIDString == null)
                return null;
            string[] tickerStrList = tickerIDString.Split('_');
            //if given t list is ickerempty, return null object
            if (tickerStrList.Length == 0)
                return null;

            //make sure the stop time is greater than the start time
            if (startDT >= stopDT)
                return null;
            //get dictionary for the last price of each stock
            //this is used in calculating the percent changed
            var lastPriceDict = getLastDaysPrices(tickerStrList, startDT);
            //allocate list for logged data
            retObj.loggedDataList = new List<LoggedDataObj>(); 
            bool firstPass = true;
            //query logged data for each id
            for (int i = 0; i < tickerStrList.Length; i++)
            {
           
                //max out at 5 trends
                if (i >= 5) break;

                //query for each id
                string ticker = tickerStrList[i];
                //query for each ticker
                //get minutesToGrab slices of time for each tcker and get the average sale price, volume and minumum timestamp
                //we get the minimum just so each time record is always minutesToGrab apart
                //we do a join agains the sockIndex table to get the stock name
                //at the end I do a linq sortby on the time value so the returned data is time ordered per ticker
                var query = (from data in db.StockQuoteLogs
                             //let e = new {(data.timeStamp.Ticks / divider) / 90}
                             let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab * minutesToGrab, DateTime.MinValue)
                             where /*id == data.stockIndexID &&*/ startDT < data.timeStamp && stopDT > data.timeStamp
                             join t in db.StockIndexes on
                                data.stockIndexID equals t.ID
                                    where t.tickerName == ticker
                             group data
                                 by d into dg
                             select new
                             {
                                 price = dg.Average(data => data.lastSale),
                                 volume = dg.Average(data => data.volume),
                                 time = dg.Min(data => data.timeStamp),
                             }).OrderBy(dt=>dt.time).ToList();

                List<LoggedDataObj> logList = new List<LoggedDataObj>();
                if (query.Count() > 0)
                {
                    int recordCount = 0;
                    
                    
                    foreach (var item in query)
                    {
                        LoggedDataObj logDObj = new LoggedDataObj();
                       // logDObj.id = id;
                        logDObj.name = ticker;
                        logDObj.price = item.price;
                        logDObj.volume = item.volume;
                        logDObj.timeStamp = item.time;
                        logDObj.percent = ((item.price / lastPriceDict[ticker]) - 1) * 100;
                        //set initial min max values
                        if (firstPass)
                        {
                            firstDT = item.time;
                            retObj.maxValPrice = logDObj.price;
                            retObj.maxValPercent = logDObj.percent;
                            retObj.minValPrice = logDObj.price;
                            retObj.minValPercent = logDObj.percent;
                            firstPass = false;
                        }
                        else
                        {//upddate min and max values
                            if (retObj.maxValPrice < logDObj.price)
                                retObj.maxValPrice = logDObj.price;
                            else if (retObj.minValPrice > logDObj.price)
                                retObj.minValPrice = logDObj.price;
                            if (retObj.maxValPercent < logDObj.percent)
                                retObj.maxValPercent = logDObj.percent;
                            else if (retObj.minValPercent > logDObj.percent)
                                retObj.minValPercent = logDObj.percent;
                            lastDT = item.time;
                        }
                        recordCount++;
                        //add this record to data log list
                        retObj.loggedDataList.Add(logDObj);
                    }
                    //get record count and key of object with the max amount of slices
                    if (recordCount > retObj.maxRecordsPerItem)
                    {
                        retObj.maxRecordsPerItem = recordCount;
                        retObj.firstLoggedDT = firstDT;
                        retObj.lastLoggedDT = lastDT;
                    }
                }        
            }
            return retObj;
        }//end function
        //returns trendDataObj with all logged data for given ids for the time span given
        //timeSlices is the amount of logged data points to get across the given timespan
        private TrendDataObjAJS getLoggedDataAJS(string tickerIDString, DateTime startDT, DateTime stopDT, int timeSlices)
        {
            int minutesToGrab = 0;

            TrendDataObjAJS retObj = new TrendDataObjAJS();
            DateTime firstDT = DateTime.Now;
            DateTime lastDT = DateTime.Now;
            //Dictionary<string, float> lastPriceDict;// = new Dictionary<int, float>();
            retObj.maxValPrice = 0;
            retObj.maxValPercent = 0;
            retObj.minValPrice = 0;
            retObj.minValPercent = 0;
            int maxRecordsPerItem = 0;

            //condtion start and stop date times to fall within trading times
            //fix start date time to start at 9:30 am
            startDT = startDT.AddHours((startDT.Hour * -1) + 9);
            startDT = startDT.AddMinutes((startDT.Minute * -1) + 20);
            //fix stop date time to stop before 4:00 am
            stopDT = stopDT.AddHours((stopDT.Hour * -1) + 4);
            stopDT = stopDT.AddMinutes((stopDT.Minute * -1) + 1);

            minutesToGrab = getLoggedDays(startDT, DateTime.Now);

            //I get a string in the format of the ticker ID's (ex. "1_2_3_4") this gets parsed into an array of tickers as strings
            if (tickerIDString == null)
                return null;
            string[] tickerStrList = tickerIDString.Split('_');
            //if given t list is ickerempty, return null object
            if (tickerStrList.Length == 0)
                return null;

            //make sure the stop time is greater than the start time
            if (startDT >= stopDT)
                return null;
            //get dictionary for the last price of each stock
            //this is used in calculating the percent changed
            var lastPriceDict = getLastDaysPrices(tickerStrList, startDT);
            //allocate list for logged data
            retObj.loggedDataList = new List<LoggedDataObj>();
            bool firstPass = true;
            //query logged data for each id
            for (int i = 0; i < tickerStrList.Length; i++)
            {

                //max out at 5 trends
                if (i >= 5) break;

                //query for each id
                string ticker = tickerStrList[i];
                //query for each ticker
                //get minutesToGrab slices of time for each tcker and get the average sale price, volume and minumum timestamp
                //we get the minimum just so each time record is always minutesToGrab apart
                //we do a join agains the sockIndex table to get the stock name
                //at the end I do a linq sortby on the time value so the returned data is time ordered per ticker
                var query = (from data in db.StockQuoteLogs
                             //let e = new {(data.timeStamp.Ticks / divider) / 90}
                             let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab * minutesToGrab, DateTime.MinValue)
                             where /*id == data.stockIndexID &&*/ startDT < data.timeStamp && stopDT > data.timeStamp
                             join t in db.StockIndexes on
                                data.stockIndexID equals t.ID
                             where t.tickerName == ticker
                             group data
                                 by d into dg
                             select new
                             {
                                 price = dg.Average(data => data.lastSale),
                                 volume = dg.Average(data => data.volume),
                                 time = dg.Min(data => data.timeStamp),
                             }).OrderBy(dt => dt.time).ToList();

                List<LoggedDataObj> logList = new List<LoggedDataObj>();
                if (query.Count() > 0)
                {
                    int recordCount = 0;

                    List<DateTime> dtList = new List<DateTime>();
                    foreach (var item in query)
                    {
                        LoggedDataObj logDObj = new LoggedDataObj();
                        // logDObj.id = id;
                        logDObj.name = ticker;
                        logDObj.price = item.price;
                        logDObj.volume = item.volume;
                        logDObj.timeStamp = item.time;
                        logDObj.percent = ((item.price / lastPriceDict[ticker]) - 1) * 100;
                        dtList.Add(item.time); 
                        //set initial min max values
                        if (firstPass)
                        {
                            //firstDT = item.time;
                            retObj.maxValPrice = logDObj.price;
                            retObj.maxValPercent = logDObj.percent;
                            retObj.minValPrice = logDObj.price;
                            retObj.minValPercent = logDObj.percent;
                            firstPass = false;
                        }
                        else
                        {//upddate min and max values
                            if (retObj.maxValPrice < logDObj.price)
                                retObj.maxValPrice = logDObj.price;
                            else if (retObj.minValPrice > logDObj.price)
                                retObj.minValPrice = logDObj.price;
                            if (retObj.maxValPercent < logDObj.percent)
                                retObj.maxValPercent = logDObj.percent;
                            else if (retObj.minValPercent > logDObj.percent)
                                retObj.minValPercent = logDObj.percent;
                            lastDT = item.time;
                        }
                        recordCount++;
                        //add this record to data log list
                        retObj.loggedDataList.Add(logDObj);
                    }
                    //get record count and key of object with the max amount of slices
                    if (recordCount > maxRecordsPerItem)
                    {
                        maxRecordsPerItem = recordCount;
                        retObj.timeStampList = dtList;//update the dateTime reference list to the ticker that has the most data
                        //retObj.firstLoggedDT = firstDT;
                        //retObj.lastLoggedDT = lastDT;
                    }
                }
            }
            return retObj;
        }//end function
        //returns the amount of weekdays in the timespan. Holidays are included in count
        private int getLoggedDays(DateTime t1, DateTime t2)
        {
            int retVal = 0;
            int fullDays = (int)t2.Subtract(t1).TotalDays;
            //get full count of days in this many weeks
            fullDays += (int)t1.DayOfWeek + (7 - (int)t2.DayOfWeek);
            fullDays = fullDays / 7;
            retVal = (fullDays * 5) - ((int)t1.DayOfWeek - 1) - (5 - (int)t2.DayOfWeek);

            return retVal;
        }
        //retruns previous logged day Weekend days are excluded
        private DateTime getPerviousLoggedDay(DateTime DT)
        {
            DateTime retVal = DT.AddDays(-1);
            if ((int)retVal.DayOfWeek == 6)
                retVal = DT.AddDays(-1);
            if ((int)retVal.DayOfWeek == 0)
                retVal = DT.AddDays(-1);
            return retVal;
        }

    }
}
