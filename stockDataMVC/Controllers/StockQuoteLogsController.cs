using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
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
            //public IDictionary<DateTime, List<StData>> stockDataDict { get; set; }
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
        // GET: StockQuoteLogs/DataTrend/5
        public ActionResult DataTrend(int? id)
        {
            IList<int> formData = new List<int>();
            SortedDictionary<DateTime,List<StData>> cdDict = new SortedDictionary<DateTime,List<StData>>();
            Dictionary<int,StPenData> stPenDataDict = new Dictionary<int,StPenData>();
            StDataModel sModel = new StDataModel();
            int trendedDays = 5;
            DateTime startDT = DateTime.Now.AddDays(-trendedDays);
            startDT= startDT.AddHours(startDT.Hour * -1);
            int timeSlices = 100;
            int minutesToGrab = getLoggedDays(startDT, DateTime.Now);
            minutesToGrab = minutesToGrab * 450 / timeSlices;
           
            //foreach ticker, query for its data in whatever time incremement gets me 30 slices
            //foreach (var cBox in collection)
           // {
            //string cBox = collection;
           // int id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ids = (int)id;
           // else id = (int)ids;// Convert.ToInt32(cBox);
                //long divider = 10000*1000*60;

                var query = from data in db.StockQuoteLogs
                            //let e = new {(data.timeStamp.Ticks / divider) / 90}
                            let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab, DateTime.MinValue)
                            where id == data.stockIndexID && startDT < data.timeStamp 
                            
                           // orderby data.timeStamp descending
                            group data
                                by d into dg
                            join t in db.StockIndexes on
                                dg.Min(data => data.stockIndexID) equals t.ID
                            //orderby DataAnnotationsModelMetadata.
                            select new
                            {
                                tName = t.tickerName,
                                price = dg.Average(data => data.lastSale),
                                volume = dg.Average(data => data.volume),
                                time = dg.Min(data => data.timeStamp)
                            };

                List<StData> sList = new List<StData>();
                if (query.Count() > 0)
                {
                    StPenData stPennObj = new StPenData();
                    //stPennObj.tickerID = id;
                    stPennObj.tickerName = "";
                    stPennObj.maxValue = 0;
                    stPennObj.minValue = 0;
                    
                    foreach (var item in query)
                    {
                        StData stObj = new StData();
                        stObj.tickerID = ids;// item.tName;// id.ToString();
                        //if the ticker name is empty, assume first run
                        if (stPennObj.tickerName == ""){
                            stPennObj.tickerName = item.tName;
                            stPennObj.maxValue = item.price;
                            stPennObj.minValue = item.price;
                        }
                        stObj.timestamp = item.time;
                        stObj.price = item.price;
                        //update max and min values
                        if (stObj.price > stPennObj.maxValue)
                            stPennObj.maxValue = stObj.price;
                        else if (stObj.price < stPennObj.minValue)
                            stPennObj.minValue = stObj.price;
                        stObj.volume = item.volume;

                        sList.Add(stObj);
                    }
                    
                    
                    sList = sList.OrderBy(o => o.timestamp).ToList();
                    stPennObj.stockDataList = sList;
                    stPenDataDict.Add(ids, stPennObj);
                    //update dictionary values
                    /*for (int i = 0; i < sList.Count; i++)
                    {
                        StData stObj = sList[i];

                        if (cdDict.ContainsKey(stObj.timestamp))
                        {
                            cdDict[stObj.timestamp].Add(stObj);
                            //maybe check for duplicates
                        }
                        else
                        {
                            List<StData> cdList = new List<StData>();
                            cdList.Add(stObj);
                            cdDict.Add(stObj.timestamp, cdList);
                        }

                    }*/
                }
                    //stDataList.Add(sList);
          //  }



            //sModel.stockDataDict = cdDict;
            sModel.stockPenDataDict = stPenDataDict;
            sModel.slices = timeSlices;

           // sModel.stockDataList = (IEnumerable)stDataList;
            return View(sModel);
        }
        // GET: StockQuoteLogs/Details/5
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
