using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using sdClassLibrary.Models;
using System.Data.Objects.SqlClient;

namespace stockDataMVC.Controllers
{
    public class StocksDataController : Controller
    {
        /// <summary>
        /// object used to store logged data for a stock.
        /// </summary>
        public class TickerHistoricData
        {
            public List<float> priceList { get; set; }
            public List<float> percentList { get; set; }
            public float maxPrice { get; set; }
            public float minPrice { get; set; }
            public float maxPercent { get; set; }
            public float minPercent { get; set; }

            public TickerHistoricData()
            {//initialize object
                priceList = new List<float>();
                percentList = new List<float>();
                maxPercent = minPercent = maxPrice = minPrice = 0;
            }
        }
        private stdataEntities db = new stdataEntities();

        // GET: StocksData
        public ActionResult Index()
        {
            getLoggedData("AAPL", DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-20), 20);
            return View(db.StockQuoteLogs.ToList());
        }

        // GET: StocksData/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockQuoteLog stockQuoteLog = db.StockQuoteLogs.Find(id);
            if (stockQuoteLog == null)
            {
                return HttpNotFound();
            }
            return View(stockQuoteLog);
        }

        // GET: StocksData/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StocksData/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,stockIndexID,timeStamp,lastSale,volume,askSize,bidSize")] StockQuoteLog stockQuoteLog)
        {
            if (ModelState.IsValid)
            {
                db.StockQuoteLogs.Add(stockQuoteLog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stockQuoteLog);
        }

        // GET: StocksData/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockQuoteLog stockQuoteLog = db.StockQuoteLogs.Find(id);
            if (stockQuoteLog == null)
            {
                return HttpNotFound();
            }
            return View(stockQuoteLog);
        }

        // POST: StocksData/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,stockIndexID,timeStamp,lastSale,volume,askSize,bidSize")] StockQuoteLog stockQuoteLog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockQuoteLog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stockQuoteLog);
        }

        // GET: StocksData/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockQuoteLog stockQuoteLog = db.StockQuoteLogs.Find(id);
            if (stockQuoteLog == null)
            {
                return HttpNotFound();
            }
            return View(stockQuoteLog);
        }

        // POST: StocksData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockQuoteLog stockQuoteLog = db.StockQuoteLogs.Find(id);
            db.StockQuoteLogs.Remove(stockQuoteLog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //returns trendDataObj with all logged data for given ids for the time span given
        //timeSlices is the amount of logged data points to get across the given timespan
        private List<float> getLoggedData(string tickerName, DateTime startDT, DateTime stopDT, int timeSlices)
        {
            int minutesToGrab = 0;
            TickerHistoricData tickerData = new TickerHistoricData();
            //TrendDataObj retObj = new TrendDataObj();
            DateTime firstDT = DateTime.Now;
            DateTime lastDT = DateTime.Now;

            //condtion start and stop date times to fall within trading times
            //fix start date time to start at 9:30 am
            startDT = startDT.AddHours((startDT.Hour * -1) + 9);
            startDT = startDT.AddMinutes((startDT.Minute * -1) + 20);
            //fix stop date time to stop before 4:00 am
            stopDT = stopDT.AddHours((stopDT.Hour * -1) + 4);
            stopDT = stopDT.AddMinutes((stopDT.Minute * -1) + 1);

            minutesToGrab = (getLoggedDays(startDT, DateTime.Now)*7*60)/timeSlices;

            //I get a string in the format of the ticker ID's (ex. "1_2_3_4") this gets parsed into an array of tickers as strings
            if (tickerName == null) return null;
            //make sure the stop time is greater than the start time
            if (startDT >= stopDT) return null;

            try
            {
                int tickerID = db.StockIndexes.Where(s => (s.tickerName == tickerName)).FirstOrDefault().ID;
                //if (tickerID == null) return null;
                float lastDaysPrice = getLastDaysPrice(1, startDT);

                var query = (from data in db.StockQuoteLogs
                             //let e = new {(data.timeStamp.Ticks / divider) / 90}
                             let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab * minutesToGrab, DateTime.MinValue)
                             where startDT < data.timeStamp && stopDT > data.timeStamp
                             join t in db.StockIndexes on
                                data.stockIndexID equals t.ID
                             where t.tickerName == tickerName
                             group data
                                 by d into dg
                             select new
                             {
                                 price = dg.Average(data => data.lastSale),
                                 volume = dg.Average(data => data.volume),
                                 time = dg.Min(data => data.timeStamp),
                             }).OrderBy(dt => dt.time).ToList();
                /*var query = db.StockQuoteLogs
                            .Where(s => ((s.timeStamp > startDT)
                                            && (s.timeStamp < stopDT)
                                            && (s.stockIndexID == tickerID)))
                            .GroupBy(s => s.timeStamp.AddMinutes(minutesToGrab))
                            .Select(s => new { timeStamp = s.Min(x => x.timeStamp), price = s.Average(x => x.lastSale) });*/
                // .GroupBy(x => (int) ((x.Time - startTime).TotalSeconds / intervalInSeconds)

                foreach (var item in query)
                {
                    float tmpp = item.price + 1;
                }
            }
            catch (Exception E)
            {
                ///
            }






                /*var query = (from data in db.StockQuoteLogs
                            //let e = new {(data.timeStamp.Ticks / divider) / 90}
                             let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab * minutesToGrab, DateTime.MinValue)
                             where startDT < data.timeStamp && stopDT > data.timeStamp
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

            //get dictionary for the last price of each stock
            //this is used in calculating the percent changed
            //var lastPriceDict = getLastDaysPrices(tickerStrList, startDT);
            //allocate list for logged data
            //retObj.loggedDataList = new List<LoggedDataObj>();
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
                             where startDT < data.timeStamp && stopDT > data.timeStamp
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
            }*/
            return null;
        }//end function
        /// <summary>
        /// Returns dictioary of the last price for each stock the day before the startDT
        /// </summary>
        /// <param name="tickerStrList"></param>
        /// <param name="startDT"></param>
        /// <returns></returns>
        private float getLastDaysPrice(int tickerID, DateTime startDT)
        {
            Dictionary<string, float> retDict = new Dictionary<string, float>();
            //grab data of the last price of the last day
            var lastPriceQuery = db.StockQuoteLogs
                .Where(s => ((s.ID == tickerID) && (s.timeStamp < startDT)))
                .OrderByDescending(s => s.timeStamp)
                .FirstOrDefault();

            if (lastPriceQuery == null) return -1;
            return lastPriceQuery.lastSale;
        }
         /*       
                
                
                (from data in db.StockQuoteLogs
                                  where data.timeStamp < startDT &&
                                        data.ID 

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
        }*/
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
