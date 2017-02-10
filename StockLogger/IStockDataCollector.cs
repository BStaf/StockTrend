using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLogger
{
    public class StockDataObject
    {
        public string ticker { get; set; }
        public float price { get; set; }
        public float volume { get; set; }
        public DateTime timestamp { get; set; }
    }
    public interface IStockDataCollector
    {
        StockDataObject getLatestStockDataWithTicker(string ticker);
        Dictionary<string, StockDataObject> getLatestStockDataWithTickerList(List<string> tickerList);

    }
}
