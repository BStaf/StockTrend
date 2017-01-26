using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdClassLibrary.Models
{
    /// <summary>
    /// Holds logged data for one stock
    /// </summary>
    public class StockLoggedData
    {
        public List<StockQuoteLog> loggedData;
        public string tickerName;
        public float maxPrice;
        public float minPrice;
    }
    public interface IStockData
    {
        List<string> getTickerList();
        List<StockLoggedData> getLoggedData(List<string> tickerList, DateTime start, DateTime end);
        StockLoggedData getLoggedData(string ticker, DateTime start, DateTime end);
    }
    public class StockData : IStockData
    {
        private stdataEntities db = new stdataEntities();
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
        public StockLoggedData getLoggedData(string ticker, DateTime start, DateTime end)
        {
            
            throw new Exception("Not yet implemented");
        }
        public List<StockLoggedData> getLoggedData(List<string> tickerList, DateTime start, DateTime end)
        {
            throw new Exception("Not yet implemented");
        }
    }
}
