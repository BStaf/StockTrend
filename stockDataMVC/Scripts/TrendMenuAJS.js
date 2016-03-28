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

//create list of tickers to be selected from
var tickerList = {};
//object to hold ticker name and trending min max vaules -- future
tickerObj = function (tickerId_, tickerName_, tickerMax_, tickerMin_) {
    this.id = tickerId_;
    this.name = tickerName_;
    this.tickerMax = tickerMax_;
    this.tickerMin = tickerMin_;
}
//sets the min max values of this object.
//not sure if I can use the .id field as a key
setTickerMinMax = function (tickerId_, tickerMax_, tickerMin_) {
    tickerList[tickerId_].tickerMin = tickerMin_;
    tickerList[tickerId_].tickerMin = tickerMax_;
}
var setTickerList = function (list) {
    tickerList = list;  
};
//create list of all stock values paseed to page
//this array will be treated as a dictionary who's index is the ticker name
//ex. stockPrices["AAPL"] = PenValue Object
var stockPrcies = [];
//create reusable object for each price log
PenValue= function (price_, volume_, timestamp_,tName_) {
    this.price = price_;
    this.volume = volume_;
    this.timestamp = timestamp_;
    this.tName = tName_;
};
PenDictObj = function (key, pVal){
    this.key = key;
    this.penValue = pVal;
}
populateStockPrices = function (penDictObj) {
    stockPrcies.push(penDictObj);

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

};
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
        offset: 40,
        index: 0,
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
        //get 0 - 100% position across the chart
        $scope.coords.index = Math.floor(($scope.coords.x - $scope.coords.offset) / ($scope.coords.length - $scope.coords.offset) * $scope.timeSlices);
        if ($scope.coords.x < $scope.coords.offset) {
            $scope.coords.x = $scope.coords.offset;
            $scope.coords.index = 0;
        }
        else if ($scope.coords.x > $scope.coords.length) {
            $scope.coords.x = $scope.coords.length;
            $scope.coords.index = $scope.timeSlices-1;
        }
        //var i = 0;
        if (stockPrcies[$scope.coords.index] != null){
            $scope.coords.y = stockPrcies[$scope.coords.index][0].price;
        }
      /*  else {
            $scope.coords.x += $scope.coords.offset;
        }*/
        $scope.onMouseMoveResult = "(" + $scope.coords.x + ", " + $scope.coords.index + ")"; //getMouseEventResult($event, "Mouse move");
    };
    $scope.onMouseDown = function ($event) {
        coords = getCrossBrowserElementCoords($event);
    }
   // $scope.onMouseOver = function ($event) {
   //     $scope.onMouseOverResult = getMouseEventResult($event, "Mouse over");
   // };
});
