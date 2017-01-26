using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using sdClassLibrary.Models;

namespace stockDataMVC.Controllers
{
    public class StockIndexesController : Controller
    {
        private stdataEntities db = new stdataEntities();

        // GET: StockIndexes
        public ActionResult Index()
        {
            return View(db.StockIndexes.ToList());
        }
        // GET: StockIndexes/stockPicker
        public ActionResult StockPicker()
        {
            //return just the tickerName field as a list of strings
            IList<string> tNameList = (db.StockIndexes.Select(si => si.tickerName)).ToList();
           /* IList<string> tNameList = new List<string>();
            tNameList.Clear();
            tNameList.Add("AAPL");
            tNameList.Add("F");
            tNameList.Add("LYG");
            tNameList.Add("C");
            tNameList.Add("FB");
            tNameList.Add("GLD");
            tNameList.Add("BOA");
            tNameList.Add("PFZ");
            tNameList.Add("MM");
            tNameList.Add("ZRT");
            tNameList.Add("P");
            tNameList.Add("S");*/
            return View(tNameList);
        }
        // GET: StockIndexes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockIndex stockIndex = db.StockIndexes.Find(id);
            if (stockIndex == null)
            {
                return HttpNotFound();
            }
            return View(stockIndex);
        }

        // GET: StockIndexes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockIndexes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,tickerName,companyName,notes")] StockIndex stockIndex)
        {
            if (ModelState.IsValid)
            {
                db.StockIndexes.Add(stockIndex);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stockIndex);
        }

        // GET: StockIndexes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockIndex stockIndex = db.StockIndexes.Find(id);
            if (stockIndex == null)
            {
                return HttpNotFound();
            }
            return View(stockIndex);
        }

        // POST: StockIndexes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,tickerName,companyName,notes")] StockIndex stockIndex)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockIndex).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stockIndex);
        }

        // GET: StockIndexes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockIndex stockIndex = db.StockIndexes.Find(id);
            if (stockIndex == null)
            {
                return HttpNotFound();
            }
            return View(stockIndex);
        }

        // POST: StockIndexes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockIndex stockIndex = db.StockIndexes.Find(id);
            db.StockIndexes.Remove(stockIndex);
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
