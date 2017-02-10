using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StockLogger
{
    public interface IDataLogger
    {
        void pushStockData(Dictionary<string, StockDataObject> data);
    }

    
}
