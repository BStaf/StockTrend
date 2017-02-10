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
            DateTime dt = Convert.ToDateTime("2/9/2017");
            var data = sd.getLoggedData("F", dt.AddDays(-2), dt, 200);
            Assert.IsTrue(data.tickerName == "F") ;
        }
        [TestMethod]
        public void getLoggedDataSingleString_validStringAndRange_shouldReturnCorrectNumberOfTimeSlices()
        {
            StockData sd = new StockData();
            DateTime dt = Convert.ToDateTime("2/9/2017");
            //the chart will use a given number of data points The getLoggedData function should return the 
            //propper number of data points requested 
            int timeSlices = 200;
            var data = sd.getLoggedData("F", dt.AddDays(-1), dt, timeSlices);
            Assert.IsTrue(checkIfWithinDeadbandOfPercent(timeSlices,data.loggedData.Count,5));
        }
        [TestMethod]
        public void getLoggedDataStringList_validStringAndRange_shouldReturnCorrectNumberOfTimeSlices()
        {
            StockData sd = new StockData();
            DateTime dt = Convert.ToDateTime("2/9/2017");
            //the chart will use a given number of data points The getLoggedData function should return the 
            //propper number of data points requested 
            int timeSlices = 200;
            var data = sd.getLoggedData(new List<string>(){"F","GM"}, dt.AddDays(-2), dt, timeSlices);
            Assert.IsTrue((data[0].tickerName == "F") && (data[1].tickerName == "GM"));
            //Assert.IsTrue(checkIfWithinDeadbandOfPercent(timeSlices, data.loggedData.Count, 5));
        }
        private bool checkIfWithinDeadbandOfPercent(int expectedCount, int actualCount, float deadbandPercent)
        {
            int deadband = (int)(deadbandPercent / 100 * expectedCount);
            int highDB = expectedCount + deadband;
            int lowDB = expectedCount - deadband;
            if ((actualCount > lowDB) && (actualCount < highDB))
                return true;
            return false;
        }
    }
}
