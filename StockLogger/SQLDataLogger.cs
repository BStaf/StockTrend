using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sdClassLibrary.Models;

namespace StockLogger
{
    public class SQLDataLogger : IDataLogger
    {
        private stdataEntities db;
        public SQLDataLogger()
        {
            db = new stdataEntities();
        }
        /// <summary>
        /// Takes dictionary of ticker symbol and logged data and pushes it to the SQL database
        /// if the symbol is not already in the StockIndexes table, it is added
        /// </summary>
        /// <param name="data"></param>
        public void pushStockData(Dictionary<string, StockDataObject> data)
        {
            foreach (var item in data)
            {
                int sId = getSymbolIDFromDatabase(item.Key);
                if (checkIfNotEqualToLastLoggedData(sId,item.Value))
                {//don't bother if this is a duplicate
                    if (sId == 0)//not found
                        sId = addNewSymbolToDatabase(item.Key);
                    addLoggedEntryToDatabaseWithSotckIndexID(sId, item.Value);
                }
            }
            //throw new Exception("Not yet implemented");
        }
        /// <summary>
        /// adds new logged data to database. Function does not check if ID is valid
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="sdObj"></param>
        private void addLoggedEntryToDatabaseWithSotckIndexID(int ID, StockDataObject sdObj)
        {
            //update LoggedTimeRangeTable
            updateLoggedTimeRanges(sdObj.timestamp);
            //update LoggedData table
            db.LoggedDatas.Add(new LoggedData()
            {
                price = sdObj.price,
                timestamp = sdObj.timestamp,
                volume = sdObj.volume,
                stockID = ID
            });
            db.SaveChanges();
        }
        private void updateLoggedTimeRanges(DateTime loggedTime)
        {
            DateTime yesterday = loggedTime.AddDays(-1);
            LoggedTimeRanx loggedTimeObj = db.LoggedTimeRanges.Where(ltr => ltr.LogDate > yesterday).OrderByDescending(ltr => ltr.LogDate).FirstOrDefault();
            if (loggedTimeObj == null)
            {
                loggedTimeObj = new LoggedTimeRanx()
                {//new entry for the day
                    LogDate = loggedTime.Date,
                    StartTime = loggedTime.TimeOfDay,
                    StopTime = loggedTime.TimeOfDay           
                };
                db.LoggedTimeRanges.Add(loggedTimeObj);
                db.SaveChanges();
            }
            else if (loggedTimeObj.StopTime < loggedTime.TimeOfDay)
            {//update todays entry if greater than last update
                db.Entry(loggedTimeObj).State = System.Data.Entity.EntityState.Modified;
                loggedTimeObj.StopTime = loggedTime.TimeOfDay;
                db.SaveChanges();
            }
        }
        private bool checkIfNotEqualToLastLoggedData(int stockID, StockDataObject sdObj)
        {
            var latestLogged = db.LoggedDatas.Where(ld => ld.stockID == stockID).OrderByDescending(ld => ld.timestamp).FirstOrDefault();
            if (latestLogged == null)
                return true;
            if (latestLogged.timestamp.ToString("M/dd/yyyy HH:mm") == sdObj.timestamp.ToString("M/dd/yyyy HH:mm"))
                return false;
            return true;
        }
        private int getSymbolIDFromDatabase(string ticker)
        {
            var stockIndex = db.StockIndexes.Where(si => si.tickerName.ToUpper() == ticker.ToUpper()).FirstOrDefault();
            if (stockIndex != null)
            {
                return stockIndex.ID;
            }
            return 0;
        }
        private int addNewSymbolToDatabase(string ticker)
        {
            StockIndex siObj = new StockIndex()
            {
                tickerName = ticker
            };
            db.StockIndexes.Add(siObj);
            db.SaveChanges();
            return siObj.ID;
        }
    }
}
