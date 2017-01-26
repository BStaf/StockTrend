/* This is more of an integrity test. I am currently establishing a direct connection to the database
 * to test these functions out
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sdClassLibrary.Models;

namespace sdClassLibrary_Tests
{
   
    [TestClass]
    public class StockData_Tests
    {
        [TestMethod]
        //I don't know what tickers are stored, so I will be happy if a valid string list is returned
        public void getTickerList_shouldReturnListOfStrings()
        {
            StockData sd = new StockData();
            List<string> sList = sd.getTickerList();
            Assert.IsTrue((sList.Count > 0));
        }
        [TestMethod]
        public void getLoggedDataSingleString_validStringAndRange_ReturnsSomeLoggedDataForThatTicker()
        {
            StockData sd = new StockData();
            var data = sd.getLoggedData("F", DateTime.Now.AddDays(-5), DateTime.Now);
            Assert.IsTrue(data.tickerName == "F") ;
        }
    }
}
