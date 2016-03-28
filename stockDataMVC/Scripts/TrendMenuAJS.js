var outdata = "";
$(function () {
    $('form').submit(function () {
        outdata = $(this).serialize();
        //if ($(this).valid()) {
        /* $.ajax({
             url: "/StockQuoteLogs/DataTrend",
             type: "POST",
             data: $(this).serialize(),
             success: function (result) {
                 $('#stockTable').html(result);
             }
         });
         // }*/
      //   return false;
    });
});
/***************************************************************************
*                           Global variables
***************************************************************************/
var tickerList = []; //create list of tickers to be selected from
//create list of all stock values paseed to page
//this array will be treated as a dictionary who's index is the ticker name
//ex. stockPrices["AAPL"] = PenValue Object
var stockPrcies = [];



//object that holds stock data for 1 logged period for 1 ticker
PenValueObj = function (price_, volume_, timestamp_, tName_) {
    this.price = price_;
    this.volume = volume_;
    this.timestamp = timestamp_;
    this.tName = tName_;
};
/*PenDictObj = function (key, pVal) {
    this.key = key;
    this.penValue = pVal;
}*/
/***************************************************************************
*                           TickerObj class
*   Main stock object used in selecting stocks to be trended and used in
*   creating and navigating the trend
***************************************************************************/
//object to hold ticker name and trending min max vaules -- future
TickerObj = function (tickerId_, tickerName_, tickerMax_, tickerMin_,penDataArray_) {
    this.id = tickerId_;
    this.name = tickerName_;
    if (tickerMax_ == undefined) {
        this.tickerMax = 0;
        this.tickerMin = 0;
    }
    else {
        this.tickerMax = tickerMax_;
        this.tickerMin = tickerMin_;
    }
    if (penDataArray_ == undefined) {
        this.penDataAr = []; // dictionary of PenDictObj
    }
    else {
        this.penDataAr = penDataArray_;
    }
}

//sets the min max values of this object.
//not sure if I can use the .id field as a key
TickerObj.prototype.setTickerMinMax = function (tickerMin_, tickerMax_) {
    this.tickerMin = tickerMin_;
    this.tickerMax = tickerMax_;
};
//push logged value
TickerObj.prototype.pushLoggedData = function (penValueObj_) {
    this.penDataAr.push(penValueObj_);
};
//get ranged price value for trends from 0 - 1 scaled agains min and max values
TickerObj.prototype.rangePrice = function (index_) {
    var retVal = 0;
    if (this.penDataAr != null) {
        if (index_ < this.penDataAr.length) {
            var span = this.tickerMax - this.tickerMin;
            if (span > 0) {
                retVal = this.penDataAr[index_].price;
                retVal = (retVal - this.tickerMin) / span;
            }
        }
    }
    return retVal;
};
//returns price value at that index
TickerObj.prototype.getPrice = function (index_) {
    var retVal = 0;
    if (this.penDataAr != null) {
        if (index_ < this.penDataAr.length) {
            retVal = this.penDataAr[index_].price;
        }
    }
    return retVal;
};
//returns price value at that index
TickerObj.prototype.getTimeStamp = function (index_) {
    var retVal = '';
    if (this.penDataAr != null) {
        if (index_ < this.penDataAr.length) {
            retVal = this.penDataAr[index_].timestamp;
        }
    }
    return retVal;
};
/***************************************************************************                             
*                          end TickerObj class
***************************************************************************/

setTickerList = function (list_) {
    tickerList = list_;  
};
getTickerIndexFromID = function (tickerID_) {
    var retVal = -1;
    for (i = 0; i < tickerList.length; i++) {
        if (tickerList[i].id == tickerID_) {
            retVal = i;
            break;
        }
    }
    return retVal;
}
setDataforTicker = function (tickerID_,tickerMax_, tickerMin_){
    for (i = 0; i < tickerList.length; i++) {
        if (tickerList[i].id == tickerID_) {
            tickerList[i].setTickerMinMax(tickerMax_, tickerMin_);
            break;
        }
    }
}
//create reusable object for each price log

//populateStockPrices = function (penDictObj) {
 //   stockPrcies.push(penDictObj);

    /*var pValue = new PenValue(pvObj.price, pvObj.volume, pvObj.timestamp);
    

    var index = stockPrcies.indexOf(tickerName);
    if (stockPrcies[tickerName] == null) {
        var dictObj = { tickerName, obj: [] };
        dictObj.obj.push(pvObj);
        stockPrcies = dictObj;//[tickerName] = pValue;
    }
    else {
        stockPrcies[tickerName].push(pValue);
    }*/

//};
/***************************************************************************                             
*                          Angular Logic
***************************************************************************/
var trendsApp = angular.module('trendsApp', []);
trendsApp.controller("myCtrl", ["$scope", function ($scope) {
    $scope.list = tickerList;
    $scope.tickerID = "0";

    $scope.submitData = function () {
        for (var i in $scope.list) {
            var it = $scope.list[i];
            if (it.isChecked) {
                $scope.tickerID = it.id;
            }
        }
        
    };
    $scope.getTrendURL = function () {
        var path = '/StockQuoteLogs/DataTrend/' + $scope.tickerID;
        return path;
    };

}]);

trendsApp.controller('testController', function ($scope) {
    $scope.testt = "hello";
    $scope.onMouseMoveResult = "zzz";
    $scope.timeSlices = 0;


    $scope.coords = {
        length: 500,
        height: 300,
        offset: 40,
        timeStamp: '',
        price: 0,
        x: 40,
        y: 0
    };


    /*var getMouseEventResult = function (mouseEvent, mouseEventDesc)
    {
        var coords = getCrossBrowserElementCoords(mouseEvent);
        return mouseEventDesc + " at (" + coords.x + ", " + coords.y + ")";
    };*/


    $scope.onMouseMove = function ($event) {
        //get x position as it will appear on the chart
        $scope.coords.x = $event.offsetX;
        var index = 0;
        //get 0 - 100% position across the chart
        
        if ($scope.coords.x < $scope.coords.offset) {
            $scope.coords.x = $scope.coords.offset;
           // $scope.coords.index = 0;
        }
        else if ($scope.coords.x > $scope.coords.length) {
            $scope.coords.x = $scope.coords.length;
            index = $scope.timeSlices-1;
        }
        $scope.coords.y = 0;
        //figure out index of data on trend where the mouse is
        index = Math.floor(($scope.coords.x - $scope.coords.offset) / ($scope.coords.length/* - $scope.coords.offset*/) * $scope.timeSlices);
        if (tickerList != null) {
            $scope.coords.y = $scope.coords.height - (tickerList[0].rangePrice(index) * $scope.coords.height);
            $scope.coords.price = tickerList[0].getPrice(index);
            $scope.coords.timeStamp = tickerList[0].getTimeStamp(index);
        }

        $scope.onMouseMoveResult = "(" + $scope.coords.timeStamp + ", " + $scope.coords.price + ")"; //getMouseEventResult($event, "Mouse move");
    };
    $scope.onMouseDown = function ($event) {
        coords = getCrossBrowserElementCoords($event);
    }
   // $scope.onMouseOver = function ($event) {
   //     $scope.onMouseOverResult = getMouseEventResult($event, "Mouse over");
   // };
});
