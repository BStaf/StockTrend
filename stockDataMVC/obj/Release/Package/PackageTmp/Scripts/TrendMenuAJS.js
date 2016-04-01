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
PenValueObj = function (price_, percent_, volume_, timestamp_, tName_) {
    this.price = price_;
    this.percent = percent_;
    this.volume = volume_;
    this.timestamp = timestamp_;
    this.tName = tName_;
};

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
TickerObj.prototype.clearLoggedData = function () {
    this.penDataAr = [];
};
//get ranged price value for trends from 0 - 1 scaled agains min and max values
//if type = 1 then the reange comes from the percent value instead of the price
TickerObj.prototype.rangePrice = function (index_,type) {
    var retVal = 0;
    var rawMin = this.tickerMin;
    if (this.penDataAr != null) {
        if (index_ < this.penDataAr.length) {

            var span = this.tickerMax - this.tickerMin;
            if (span > 0) {
                if (rawMin < 0) {
                    rawMin = rawMin * -1;
                }
                if (type == 1) {// multplie pens means I'm using percent vaules
                    retVal = this.penDataAr[index_].percent;
                    //return (selected percent value + adjusted minimum y axis percent value) / span
                    retVal = (retVal + rawMin) / span;
                }
                else {//single pen means we are using price values
                    retVal = this.penDataAr[index_].price;
                    //return (selected price - minimum y Axis price value) / span
                    retVal = (retVal - rawMin) / span;
                }
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
    $scope.zeroYPos = 1000;//used in creating a line or box at the 0 position on the trend if using multiple pens
    $scope.index = 0;

    var penCoord = {
        timeStamp: '',
        price: 0,
        y: 0
    };
    //holds data for each individual pen to be displayed on the page as the ruler moves
    PenCoord = function (timeStamp_,name_,price_,percent_,volume_,yPos_){
        this.timeStamp = timeStamp_;
        this.price = price_;
        if (percent_ == undefined) {
            this.percent = 0;
        } else { this.percent = percent_; }
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
    //initialize function
    //cWidth = width of chart
    //cHeight = height of chart
    //cXBeginOffset = offset from the svg space wherethe chart will begin
    //timeSlices = how many points are on the trend per pen
    //max & minP = the range of the y axis of the chart
    $scope.init = function (cWidth_,cHeight_,cXBeginOffset_,timeSlices_, maxP_, minP_) {
        $scope.timeSlices = timeSlices_;
        $scope.rulerData.length = cWidth_;
        $scope.rulerData.height = cHeight_;
        $scope.index = 0;
        if ((minP_ < 0) && (maxP_ > 0)) {
            var range = maxP_ - minP_;
            if (range > 0) {//make sure we're not dividing by 0
                $scope.zeroYPos =  $scope.rulerData.height - ((minP_ * -1) / range * $scope.rulerData.height);
            }
        }
    }

    $scope.onMouseMove = function ($event) {
        setRulerData($event.offsetX);
    }
    $scope.setZeroYPos = function (minP, maxP) {
        //if trend is fully above or below 0, return 0 to hide box
        if ((minP < 0) && (maxP > 0)) {
            var range = maxP - minP;
            if (range > 0) {//make sure we're not dividing by 0
                $scope.zeroYPos =  /*$scope.rulerData.height - */((minP * -1) / range * $scope.rulerData.height);
            }
        }
       // return 0;
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
        var penCnt = 0;
        for (i = 0; i < tickerList.length; i++) {
            if (tickerList[i].isChecked) {
                penCnt++;
            }
        }
        $scope.rulerData.penCoords = [];
        index = Math.floor(($scope.rulerData.x - $scope.rulerData.offset) / ($scope.rulerData.length/* - $scope.coords.offset*/) * $scope.timeSlices);
        if (tickerList != null) {
            for (i = 0; i < tickerList.length; i++) {
                if (tickerList[i].isChecked) {
                    var pCoord = new PenCoord(tickerList[i].getTimeStamp(index), tickerList[i].name, tickerList[i].getPrice(index), 0, 0);
                    if (penCnt > 1) {
                        t1 = tickerList[i].rangePrice(index,1);
                    }
                    else {
                        t1 = tickerList[i].rangePrice(index,0);
                    }
                    
                    pCoord.y = $scope.rulerData.height - (t1 * $scope.rulerData.height);
                    $scope.rulerData.penCoords.push(pCoord);
                    //penCnt = 2;
                }
            }
            
        }
      
    };
    $scope.onMouseDown = function ($event) {
        coords = getCrossBrowserElementCoords($event);
    }
});
