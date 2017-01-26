//Chart Directive
//draws a line chart for whatever data is passed
//below the chart is a table showing the logged values at whatever time period the user moves the chart ruler to
//look at the scope declaration to see what needs passed
app.directive('sChart', function() {
	return {
		restrict: 'AE',
		templateUrl: "apps/directives/sChart.html",
		replace: 'true',
		scope: {	cWidth: '=', //width of the chart object
					cHeight: '=',
					yAxisMax: '=',
					yAxisMin: '=',
					dtList: '=',//list of DateTimes
					dataDescription: '=',//List of strings that are the description for each data point
					tickerData: '=' //a list of objects in this format {tickerName, ListofValues, and more values...}
					},
		link: function(scope,elem,attrs) {
			scope.textOffset = 40;
			scope.axisPath = "M "+scope.textOffset+" 0 l 0 " + (scope.cHeight-2).toString() + " l " + (scope.cWidth - scope.textOffset).toString() + " 0 ";
			scope.penPaths = [];
			scope.penColors = [ "#4251A3", "#CA407C", "#68912E", "#AB7F10", "#2AB78D" ];
			scope.zeroLineYPos = 0;//the yAxis position of 0. used in multipen trends
			scope.mouseX;// = $event.pageX;
			scope.mouseY; //= $event.pageY;
			scope.rulerPos = scope.textOffset;
			scope.dataIndex;
			var dataPointForMultiPens = 1;

			//watch for data change to update the chart
			scope.$watch('tickerData', function(){
				if (scope.tickerData == undefined) return;
				setPens();
			});
			//event for mouse movement over the chart. Update ruler and ruler data
			scope.onMouseMove = function ($event) {
			    var xSpan = scope.cWidth;// - scope.textOffset;
				//correct the mouse x position to be inside the chart
				var xPos = $event.offsetX;//for some reason I have an offset of ten
				if (xPos <	(scope.textOffset))
					xPos = scope.textOffset;
				//adjust ruler bar position
				scope.rulerPos = xPos;
				//adjust for textoffset
				xPos -= scope.textOffset
				scope.dataIndex = Math.ceil((xPos/xSpan) * getLoggedDataLength());

				scope.mouseX = scope.dtList[scope.dataIndex];
				scope.mouseY = $event.pageY;
			};

			var setPens = function(){
				var xStep = (scope.cWidth - scope.textOffset)/scope.tickerData[0].data.length;
				var ySpan = (scope.yAxisMax - scope.yAxisMin);//scope.cHeighta
				var isMulti = false;
				var pointIndex=0;
				if (scope.yAxisMin < 0)
					scope.zeroLineYPos = (0-scope.yAxisMin)/ySpan*scope.cHeight;
				else 
					scope.zeroLineYPos=0;

				if (scope.tickerData.length > 1) pointIndex = dataPointForMultiPens;;
				scope.penPaths = [];
				for (var i=0;i<scope.tickerData.length;i++){
					var tDataList = scope.tickerData[i].data;
					var penPath = "M "; 
					for (var j=0;j<tDataList.length;j++){
						var x = j*xStep + scope.textOffset;
						var y = (tDataList[j][pointIndex]-scope.yAxisMin)/ySpan*scope.cHeight;
						y = scope.cHeight - y;
						penPath += x + " " + y + " L ";
					}
					penPath = penPath.slice(0, -2);
					scope.penPaths.push(penPath);
				}
			};
			//returns the amaount of leeged datapoints per pen
			var getLoggedDataLength = function(){
				if (scope.tickerData == undefined) return 0;
				return  scope.tickerData[0].data.length;
			};
		}
	};
});