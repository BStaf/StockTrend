using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockLogger;
using System.Collections.Generic;

namespace StockLogger_Tests
{
    [TestClass]
    public class YahooStockLoggerTests
    {
        [TestMethod]
        //test requires connection to yahoo
        public void getLatestStockDataWithTicker_validTicker_shouldReturnValidStockDataObject()
        {
            IStockDataCollector ysl = new YahooStockDataCollector();
            StockDataObject sdObj = ysl.getLatestStockDataWithTicker("F");
            Assert.IsTrue(checkIfValidObject("F",sdObj));
        }
        [TestMethod]
        public void getLatestStockDataWithTicker_invalidTicker_shouldThrowException()
        {
            IStockDataCollector ysl = new YahooStockDataCollector();
            try
            {
                StockDataObject sdObj = ysl.getLatestStockDataWithTicker("xxxx");
                Assert.Fail();
            }
            catch (Exception E)
            {
                Assert.AreEqual(E.Message, "failed to get data.");
            }
            
        }
        [TestMethod]
        //test requires connection to yahoo
        public void getLatestStockDataWithTickerList_validTickerList_shouldReturnValidStockDataObject()
        {
            IStockDataCollector ysl = new YahooStockDataCollector();
            var tList = new List<string>(){"F","gm"};
            Dictionary<string,StockDataObject> sdDict = ysl.getLatestStockDataWithTickerList(tList);
            Assert.IsTrue(checkIfValidObjectDictionary(tList, sdDict));
        }
        [TestMethod]
        //test requires connection to yahoo
        public void getLatestStockDataWithTickerList_invalidTickerList_shouldReturnNothing()
        {
            IStockDataCollector ysl = new YahooStockDataCollector();
            var tList = new List<string>() {"xxxx" };
           // try { 
                Dictionary<string, StockDataObject> sdDict = ysl.getLatestStockDataWithTickerList(tList);
                Assert.IsTrue(sdDict.Count == 0);
           /*     Assert.Fail();
            }
            catch (Exception E)
            {
                Assert.AreEqual(E.Message, "failed to get data.");
            }*/
        }
        [TestMethod]
        //test requires connection to yahoo
        public void getLatestStockDataWithTickerList_EmptyTickerList_shouldThrowException()
        {
            IStockDataCollector ysl = new YahooStockDataCollector();
            var tList = new List<string>() { "F", "xxxx" };
            try
            {
                Dictionary<string, StockDataObject> sdDict = ysl.getLatestStockDataWithTickerList(new List<string>());
                Assert.Fail();
            }
            catch (Exception E)
            {
                Assert.AreEqual(E.Message, "failed to get data.");
            }
        }
        private bool checkIfValidObjectDictionary(List<string> tickerList, Dictionary<string, StockDataObject> sdDict)
        {
            foreach (string ticker in tickerList)
            {
                try
                {
                    StockDataObject sdObj = sdDict[ticker.ToUpper()];
                    if (!checkIfValidObject(ticker, sdObj))
                        return false;
                }
                catch (Exception E)
                {
                    return false;
                }
            }
            return true;
        }
        private bool checkIfValidObject(string ticker, StockDataObject sdObj)
        {
            return (sdObj.ticker.ToUpper() == ticker.ToUpper())
                && (sdObj.price >= 0) 
                && (sdObj.volume >= 0) 
                && (sdObj.timestamp > DateTime.MinValue);
        }
    }
}
