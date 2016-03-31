using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Data.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using stockDataMVC.Models;
using System.Data.Entity.SqlServer;

namespace stockDataMVC.Controllers
{
    public class StockQuoteLogsController : Controller
    {
        public class StData
        {
            public int tickerID { get; set; }
            public float price { get; set; }
            public float percent { get; set; }
            public float volume { get; set; }
            public DateTime timestamp { get; set; }
        }
        
        public class StPenData
        {
            //public int tickerID { get; set; }
            public string tickerName { get; set; }
            public float maxValue { get; set; }
            public float minValue { get; set; }
            public IList<StData> stockDataList { get; set; }

        }
        public class StDataModel
        {
            public IDictionary<int,StPenData> stockPenDataDict { get; set; }
            public int slices { get; set; }
           // public IEnumerable<StData> stockDataList { get; set; }

        }

        private stdataEntities db = new stdataEntities();

        // GET: StockQuoteLogs
        public ActionResult Index()
        {
            return View(db.StockQuoteLogs.ToList());
        }
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
        public class queryParameters{
            public float price { get; set; }
            public int volume { get; set; }
            public DateTime time { get; set; }
        }
        
        // GET: StockQuoteLogs/DataTrend/5
        public ActionResult DataTrend(string id)
        {
            Dictionary<int,StPenData> stPenDataDict = new Dictionary<int,StPenData>();
            Dictionary<int,float> lastPriceDict = new Dictionary<int,float>();
            StDataModel sModel = new StDataModel();
            int trendedDays = 5;
            DateTime startDT = DateTime.Now.AddDays(-trendedDays);
            startDT= startDT.AddHours(startDT.Hour * -1);
            int timeSlices = 200;
            int actualSlices = 0;//sometimes the timeslices I get don't match the amount I'm lookign for. This is my quick fix until I figure this out
            int minutesToGrab = getLoggedDays(startDT, DateTime.Now);
            bool multiplePens = false;
            minutesToGrab = minutesToGrab * 450 / timeSlices;
           

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            

            int ids = 0;

            //I get a string in the format of the ticker ID's (ex. "1_2_3_4") this gets parsed into an array of id's as strings
            var idStrList = id.Split('_');
            if (idStrList.Length > 1){
                multiplePens = true;
            }
            var query2 = (from data in db.StockQuoteLogs
                                where data.timeStamp < startDT
                                group data by data.stockIndexID into grp
                                let LastSalePriceThatDay = grp.Max(g => g.timeStamp)
                                  
                                from data in grp
                                where data.timeStamp == LastSalePriceThatDay
                                select data);
            //create dictionary of the sale price of the last day before trend period
            foreach (var item in query2){
                lastPriceDict.Add(item.stockIndexID,(float)item.lastSale);
            }
            for (int i=0;i<idStrList.Length;i++){
                if (i >= 5) break;
                string idStr = idStrList[i];
                try
                {
                    ids = Int32.Parse(idStr);//attempt to convert string to integer
                }
                catch (FormatException e)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);//if conversion fails, the fail page load
                }

                //query for each id
                //this works but produces a rediculous query// I'll revisit this when I can efficiently do this query in linq 
                var query = (from data in db.StockQuoteLogs
                            //let e = new {(data.timeStamp.Ticks / divider) / 90}
                            let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab, DateTime.MinValue)
                            where ids == data.stockIndexID && startDT < data.timeStamp 
                            
                            // orderby data.timeStamp descending
                            group data
                                by d into dg
                           // join t in db.StockIndexes on
                          //      dg.Min(data => data.stockIndexID) equals t.ID
                            //orderby DataAnnotationsModelMetadata.
                            select new 
                            {
                                price = dg.Average(data => data.lastSale),
                                volume = dg.Average(data => data.volume),
                                time = dg.Min(data => data.timeStamp),
                            });

               /* string qStr = @"SELECT min(timeStamp) as time,avg(lastSale) as price,0.0 as volume "+//, min(ID) as ID, min(stockIndexID) as stockIndexID , 0 as askSize , 0 as bidSize " +
                    @"FROM dbo.StockQuoteLogs where stockIndexID = " +
                    ids.ToString()+ @" AND timeStamp > '"+
                    startDT.ToString() + @"' group by DATEADD(MINUTE , DATEDIFF(MINUTE,0,timeStamp)/"+
                    minutesToGrab.ToString() + @", 0) ORDER BY timeStamp ASC";


                /*SELECT min(timeStamp) as time,avg(lastSale) as price,avg(volume) as volume
  FROM dbo.StockQuoteLogs
  where stockIndexID = 1 AND timeStamp > '3/28/16'
  group by DATEADD(MINUTE , DATEDIFF(MINUTE,0,timeStamp)/15, 0)
	ORDER BY time ASC*/
                //var query = db.stockQuoteCalcs.SqlQuery(qStr).ToList();*/
                //List<queryParameters> query = db.StockQuoteLogs.SqlQuery(qStr);
                List<StData> sList = new List<StData>();
                if (query.Count() > 0)
                {
                    StPenData stPennObj = new StPenData();
                    //stPennObj.tickerID = id;
                    stPennObj.tickerName = "";
                    stPennObj.maxValue = 0;
                    stPennObj.minValue = 0;
                    actualSlices = 0;
                    //float lastPrice = 0;
                    bool FirstPass = true;
                    foreach (var item in query)
                    {
                        StData stObj = new StData();
                        stObj.tickerID = ids;// item.tName;// id.ToString();
                        stObj.timestamp = item.time;
                        stObj.price = item.price;
                        stObj.volume = item.volume;
                        //more than one pen, chart will show percent movements instead of price
                        if (multiplePens)
                        {
                            stObj.percent = ((item.price / lastPriceDict[ids]) - 1) * 100;
                            if (FirstPass)
                            {

                                stPennObj.maxValue = stObj.percent;
                                stPennObj.minValue = stObj.percent;
                                FirstPass = false;
                            }
                            //update max and min values
                            if (stObj.percent > stPennObj.maxValue)
                                stPennObj.maxValue = stObj.percent;
                            else if (stObj.percent < stPennObj.minValue)
                                stPennObj.minValue = stObj.percent;
                        }
                        else
                        {
                            if (FirstPass)
                            {
                                stPennObj.maxValue = item.price;
                                stPennObj.minValue = item.price;
                                FirstPass = false;
                            }
                            //update max and min values
                            if (stObj.price > stPennObj.maxValue)
                                stPennObj.maxValue = stObj.price;
                            else if (stObj.price < stPennObj.minValue)
                                stPennObj.minValue = stObj.price;
                        }
                        sList.Add(stObj);
                        actualSlices++;
                        
                    }
                    
                    
                    sList = sList.OrderBy(o => o.timestamp).ToList();
                    stPennObj.stockDataList = sList;
                    stPenDataDict.Add(ids, stPennObj);

                }
            }
            sModel.stockPenDataDict = stPenDataDict;
            sModel.slices = actualSlices;

           // sModel.stockDataList = (IEnumerable)stDataList;
            return View(sModel);
        }
        // GET: StockQuoteLogs/Details/5
        public ActionResult Details(int? id)/*(ICollection<int> ids)*/
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

        // GET: StockQuoteLogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockQuoteLogs/Create
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

        // GET: StockQuoteLogs/Edit/5
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

        // POST: StockQuoteLogs/Edit/5
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

        // GET: StockQuoteLogs/Delete/5
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

        // POST: StockQuoteLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockQuoteLog stockQuoteLog = db.StockQuoteLogs.Find(id);
            db.StockQuoteLogs.Remove(stockQuoteLog);
            db.SaveChanges();
            return RedirectToAction("Index");
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
