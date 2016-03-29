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
//returns price ticker name at that index
//TickerObj.prototype.getName = function (index_) {
    /*var retVal = '';
    if (this.penDataAr != null) {
        if (index_ < this.penDataAr.length) {
            retVal = this.penDataAr[index_].name;
        }
    }*/
    //return this.name;
//};
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
    $scope.tickerIDAr = [];
   // $scope.tickerIDAr.push(0);

    $scope.submitData = function () {
        $scope.tickerIDAr = [];
        for (var i in $scope.list) {
            var it = $scope.list[i];
            if (it.isChecked) {
                $scope.tickerIDAr.push(it.id);
            }
        }
    };
    //greates the link to the parital page call (this is the chart page)
    $scope.getTrendURL = function () {
        var path = '/StockQuoteLogs/DataTrend/';//
        if ($scope.tickerIDAr.length > 0) {
            for (i = 0; i < $scope.tickerIDAr.length;i++) {
                path += $scope.tickerIDAr[i] + '_';
            }
            path = path.substring(0, path.length - 1); //remove last &
        }
        else
            path += '0'
        return path;
    };
    /*var getSelectedTickers = function () {
        return productList;
    };*/

}]);

trendsApp.controller('testController', function ($scope) {
    $scope.testt = "hello";
   // $scope.onMouseMoveResult = "zzz";
    $scope.timeSlices = 0;

    var penCoord = {
        timeStamp: '',
        price: 0,
        y: 0
    };
    //holds data for each individual pen to be displayed on the page as the ruler moves
    PenCoord = function (timeStamp_,name_,price_,volume_,yPos_){
        this.timeStamp = timeStamp_;
        this.price = price_;
        this.volume = volume_;
        this.y = yPos_;
        this.name = name_;
    };
    //holds all data to display each pen value and dot position when the ruler is used
    $scope.rulerData = {
        length: 500,
        height: 300,
        offset: 40,
        x: 40,
        penCoords: []
    };
    /*
    $scope.coords = {
        length: 500,
        height: 300,
        offset: 40,
        timeStamp: '',
        price: 0,
        x: 40,
        y: 0
    };
*/


    /*var getMouseEventResult = function (mouseEvent, mouseEventDesc)
    {
        var coords = getCrossBrowserElementCoords(mouseEvent);
        return mouseEventDesc + " at (" + coords.x + ", " + coords.y + ")";
    };*/


    $scope.onMouseMove = function ($event) {
        setRulerData($event.offsetX);
    }

    setRulerData = function(xPos_){
        //get x position as it will appear on the chart
        $scope.rulerData.x = xPos_;
        var index = 0;
        //get 0 - 100% position across the chart
        
        if ($scope.rulerData.x < $scope.rulerData.offset) {
            $scope.rulerData.x = $scope.rulerData.offset;
           // $scope.coords.index = 0;
        }
        else if ($scope.rulerData.x > $scope.rulerData.length) {
            $scope.rulerData.x = $scope.rulerData.length;
            index = $scope.timeSlices-1;
        }
        //clear individual pen coordinate array
        $scope.rulerData.penCoords = [];
        index = Math.floor(($scope.rulerData.x - $scope.rulerData.offset) / ($scope.rulerData.length/* - $scope.coords.offset*/) * $scope.timeSlices);
        if (tickerList != null) {
            for (i = 0; i < tickerList.length; i++) {
                if (tickerList[i].isChecked) {
                    var pCoord = new PenCoord(tickerList[i].getTimeStamp(index), tickerList[i].name, tickerList[i].getPrice(index), 0, 0);
                    pCoord.y = $scope.rulerData.height - (tickerList[i].rangePrice(index) * $scope.rulerData.height);
                    $scope.rulerData.penCoords.push(pCoord);
                }
            }
        }



        
    };
    $scope.onMouseDown = function ($event) {
        coords = getCrossBrowserElementCoords($event);
    }
   // $scope.onMouseOver = function ($event) {
   //     $scope.onMouseOverResult = getMouseEventResult($event, "Mouse over");
   // };
});
