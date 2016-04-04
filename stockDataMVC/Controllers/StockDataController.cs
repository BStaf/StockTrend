using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using stockDataMVC.Models;
using sdClassLibrary.Models;

namespace stockDataMVC.Controllers
{
    public class StockDataController : Controller
    {
        private stdataEntities db = new stdataEntities();

        // GET: StockData
        public ActionResult Index()
        {
            return View(db.StockIndexes.ToList());
        }
        // GET: StockData
        public ActionResult Trend()
        {
            return View(db.StockIndexes.ToList());
        }
        // GET: StockData/Details/5
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

        // GET: StockData/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockData/Create
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

        // GET: StockData/Edit/5
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

        // POST: StockData/Edit/5
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

        // GET: StockData/Delete/5
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

        // POST: StockData/Delete/5
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
