StockTrend Website 
==================

The goal here is to create a usable chart object using web technologies (AVG, Angular,...). Not flash or java plugin.
I am attempting this in 2 ways. 1 is an MVC app where the trend is loaded as a partial view pre-configured
with the selected pen data. The is other is creating an angularJS directive to take the data and transform 
it into a chart. Both ways use a asp.net Web Api calls to get the logged data for the chart. The data is stored
on an MS SQL database hosting in AWS RDS

This is a Visual Studio 2013 Solution with 4 separate projects

angularSite -> Angular JS website version
sdClassLibrary -> class library that handle connection to the SQL server for stock logged data 
sdWebApp -> asp.net Web Api used by both angular and MVC sites to get logged data from the SQL server
stockDataMVC -> asp.net MVC website version

I am hosting this on an AWS EC2 server.
* MVC version web site: http://ec2-54-187-179-28.us-west-2.compute.amazonaws.com/StockData/Trend
* Angular Version web site: http://ec2-54-187-179-28.us-west-2.compute.amazonaws.com/AngularSite/index.html


Built in Visual Studio 2013
