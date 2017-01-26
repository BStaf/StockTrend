//controller handles web api calls
//When trend data is received from the web api call, the controller formats the data into something more friendly to the chart directive
app.controller("tcController",["$scope",'$window','$http',function ($scope, $window, $http) {
	$scope.selectedList = [];
	$scope.textInUse = false;
	$scope.loggedDataObj;
	$scope.timeStampList;
	$scope.tsList = [];
	$scope.chartParams;// = new Object();
	$scope.startDT;
	$scope.stopDT;
	$scope.tickerList;
	//$scope.chartParams.maxVal = 12;
	
	//call web service to get all available tickers
	/*getTickerList = function () {
		var retVal;
		$http.get('http://ec2-54-187-179-28.us-west-2.compute.amazonaws.com/StockDataApi/api/stocks/tickerList').success(function(data) {
			$scope.tickerList = data;
		});
	};*/
	/*getTickerData = function (tickers){
		var tickerStr = "";
		if (tickers.length < 0)
			return undefined;
		for (var i=0;i<tickers.length;i++){
			tickerStr = tickerStr.concat(tickers[i],"_");
		}
		//trim off the last '_'
		tickerStr = tickerStr.slice(0, -1);
		var webCallPath = "http://ec2-54-187-179-28.us-west-2.compute.amazonaws.com/StockDataApi/api/stocks/ajs/";
		webCallPath = webCallPath.concat(tickerStr);
		$http.get(webCallPath).success(function(data) {
			//var obj = JSON.parse(data);
			$scope.loggedDataObj = processTickerData(data);
			$scope.timeStampList = data.timeStampList;
			$scope.tsList = data.timeStampList;
			$scope.chartParams = getChartParams(data, $scope.loggedDataObj);
		});
	};*/
	/*fixDTToMinute = function (dateTime){
		var oldDT = new Date(dateTime);
		var retDT = new Date(dateTime);
		retDT.setMilliseconds(0);
		retDT.setSeconds(0);
		if (oldDT.getSeconds() > 30)
			retDT.setMinutes(retDT.getMinutes()+1);
		retDT.setHours(retDT.getHours()+4);//for some reason javascript is adjusting the string value to a 4 hour offset
		return retDT;
	};*/

	/*getChartParams = function (data, loggedDataObj){
		if (data == undefined) return undefined;
		if (loggedDataObj == undefined) return undefined;
		var paramObj = new Object();
		if (loggedDataObj.length > 1){
			//we use percent values onthe chart if there is more than 1 ticker
			paramObj.minVal = data.minValPercent;
			paramObj.maxVal = data.maxValPercent;
		}
		else{
			//we use price on the chart if there is only 1 ticker
			paramObj.minVal = data.minValPrice;
			paramObj.maxVal = data.maxValPrice;
		}
		var offset = (paramObj.maxVal - paramObj.minVal)*.15;
		paramObj.maxVal += offset;
		paramObj.minVal -= offset;
		//paramObj.dtList = data.timeStampList;
		return paramObj;
	};*/

	/*processTickerData = function (loggedData){
		if (loggedData == undefined) return undefined;
		var retDataList = [];
		for (var i=0;i<loggedData.loggedDataList.length;i++){
			var dataObj = new Object();
			dataObj.name = loggedData.loggedDataList[i].name;
			dataObj.data = [];
			for (var j=0;j<loggedData.timeStampList.length;j++){
				if(loggedData.loggedDataList[i].name == dataObj.name){
					//still on the same ticker. add the data
					var refDT = fixDTToMinute(loggedData.timeStampList[j]);
					var logDT = fixDTToMinute(loggedData.loggedDataList[i].timeStamp);
					var tickDataList = [];//a list of data for this one tick of time (price, volume, etc..)
					if (refDT.getTime() == logDT.getTime()){
						
						tickDataList.push(loggedData.loggedDataList[i].price);
						tickDataList.push(loggedData.loggedDataList[i].percent);
						tickDataList.push(loggedData.loggedDataList[i].volume);
					}
					else{//time gap, make all data NaN and decrement j 
						tickDataList.push(NaN);
						tickDataList.push(NaN);
						tickDataList.push(NaN);
						j--;
					}
					dataObj.data.push(tickDataList);
				}
				else{//we've mved onto the next ticker
					i--;
					break;
				} 
				if (i>=loggedData.loggedDataList.length) break;
				if ( (j+1) < loggedData.timeStampList.length) i++;
			}
			retDataList.push(dataObj);
		}
		return retDataList;
	};//end processTickerData
    */
	/*setDefaultDateTimes = function(){
		$scope.stopDT = new Date();//get current date
		$scope.startDT = new Date();
		$scope.startDT.setDate($scope.startDT.getDate()-7);
	};
	$scope.onTickerUpdate = function(){
		getTickerData($scope.selectedList);
	}*/
	//call function to get available list of tickers
	//getTickerList();
	//setDefaultDateTimes();
}]);