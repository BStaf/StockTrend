using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Data.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
//using stockDataMVC.Models;
using sdClassLibrary.Models;
using System.Data.Entity.SqlServer;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace stockDataMVC.Controllers
{
    public class StockQuoteLogsController : Controller
    {
        /***************************************************/
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
        /*public class StDataModel
        {
            public IList<LoggedDataObj> loggedDataList { get; set; }
            //public int slices { get; set; }
            public IList<DateTime> trendTimeList { get; set; }
            // public IEnumerable<StData> stockDataList { get; set; }

        }*/
        /***************************************************/

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
       /* public class StDataModel
        {
            public IDictionary<int,StPenData> stockPenDataDict { get; set; }
            public int slices { get; set; }
            public IList<DateTime> trendTimeList { get; set; }
           // public IEnumerable<StData> stockDataList { get; set; }

        }*/

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
       /* public ActionResult DataTrend(string id)
        {
            
            StDataModel sModel = new StDataModel();
            TrendDataObj trendDataObj = (TrendDataObj)GetDataFromWebApi(id).Wait();
            
        }*/
        // GET: StockQuoteLogs/DataTrend/F_C_AAPL
        //this replaces old function that handeled the database data collection
        //this is noew pushed off to the web api service. 
        //In the end, this page will not be in charge of collecting data at all.
        //there will be a page that will grab stock data directly via the web api and then live trend it
        //this will allow the ability to scroll and zoom in and out live.
        public async Task<ActionResult> DataTrend(string id)
        {
            TrendDataObj trendDataObj;
            if (id == null)
            
            {
                trendDataObj = new TrendDataObj();
                trendDataObj.loggedDataList = new List<LoggedDataObj>();
                return View(trendDataObj);
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:40966/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = @"api/stocks/" + id;// +@"/start/2016-3-20/stop/2016-4-8";
                    Task<String> response = client.GetStringAsync(url);
                    trendDataObj = JsonConvert.DeserializeObjectAsync<TrendDataObj>(response.Result).Result;
                    return View(trendDataObj);
                }
            }
            catch (Exception E)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }
 
       /* public TrendDataObj GetDataFromWebApi(string tickers)
        {
            
            
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:40966/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Task<String> response = client.GetStringAsync("api/stocks/FB_AAPL/start/2016-3-20/stop/2016-4-8");
                return JsonConvert.DeserializeObjectAsync<Car>(response.Result).Result;
            }
        }

        private async Task<TrendDataObj> GetDataFromWebApi(string tickers)
        {
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:40966/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/stocks/FB_AAPL/start/2016-3-20/stop/2016-4-8");
                if (response.IsSuccessStatusCode)
                {
                    TrendDataObj trendDataObj = await response.Content.ReadAsAsync <TrendDataObj>();
                    return trendDataObj;
                    //Product product = await response.Content.ReadAsAsync<Product>();
                    //Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
                }

                // HTTP POST

            }
            return null;
        }*/
        // GET: StockQuoteLogs/DataTrend/5
        /*public ActionResult DataTrend(string id)
        {
            Dictionary<int,StPenData> stPenDataDict = new Dictionary<int,StPenData>();
            Dictionary<int,float> lastPriceDict = new Dictionary<int,float>();
            StDataModel sModel = new StDataModel();
            int trendedDays = 5;
            DateTime startDT = DateTime.Now.AddDays(-trendedDays);
            startDT= startDT.AddHours((startDT.Hour * -1)+9);
            startDT = startDT.AddMinutes((startDT.Minute * -1) + 30);
            //startDT.AddMinutes
            int timeSlices = 200;
            int actualSlices = 0;//sometimes the timeslices I get don't match the amount I'm lookign for. This is my quick fix until I figure this out
            int maxSlicesKey = 0;//dictionary key of a record with the maximum slices
            int checkSlices = 0;
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
                             let d = SqlFunctions.DateAdd("mi", SqlFunctions.DateDiff("mi", DateTime.MinValue, data.timeStamp) / minutesToGrab * minutesToGrab, DateTime.MinValue)
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




                //List<queryParameters> query = db.StockQuoteLogs.SqlQuery(qStr);
                List<StData> sList = new List<StData>();
                if (query.Count() > 0)
                {
                    StPenData stPennObj = new StPenData();
                    //stPennObj.tickerID = id;
                    stPennObj.tickerName = "";
                    stPennObj.maxValue = 0;
                    stPennObj.minValue = 0;
                    checkSlices = 0;
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
                        checkSlices++;
                        
                    }
                    //get record count and key of object with the max amount of slices
                    if (checkSlices > actualSlices)
                    {
                        actualSlices = checkSlices;
                        maxSlicesKey = ids;
                    }
                    sList = sList.OrderBy(o => o.timestamp).ToList();
                    stPennObj.stockDataList = sList;
                    stPenDataDict.Add(ids, stPennObj);

                }
            }
            List<DateTime> trendDTList = new List<DateTime>();
            if (stPenDataDict.ContainsKey(maxSlicesKey)) { 
                for (int i = 0; i < stPenDataDict[maxSlicesKey].stockDataList.Count; i++)
                {
                    trendDTList.Add(stPenDataDict[maxSlicesKey].stockDataList[i].timestamp);
                }
            }
            sModel.stockPenDataDict = stPenDataDict;
            sModel.slices = actualSlices;
            sModel.trendTimeList = trendDTList;

           // sModel.stockDataList = (IEnumerable)stDataList;
            return View(sModel);
        }*/
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
        /*[HttpPost]
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
        }*/

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
        /*public ActionResult Edit([Bind(Include = "ID,stockIndexID,timeStamp,lastSale,volume,askSize,bidSize")] StockQuoteLog stockQuoteLog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockQuoteLog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stockQuoteLog);
        }
         */

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
