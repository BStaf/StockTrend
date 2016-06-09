//dropdown selector directive
//user can click on the searchbox and get a dropdown of all passed items
//if user starts typing in the searchbox, the selectable items are filtered
//selected items are shown with an X next to them that can be pressed to remove the selected item
//a function can be passed to be called whenever the selected list is altered
	app.directive('sDropdown', function() {
		return {
			restrict: 'AE',
			templateUrl: "apps/directives/sDropdown.html",
			replace: 'true',
			scope: {	searchList: '=',
						selectedList: '=',
						searchText: '@',
						action: '&'
						},
			link: function(scope,elem,attrs) {
				scope.itemNotFound=false;
				scope.itemAlreadySelected=false;
				scope.showDropdown=false;
				scope.submit = function(){
					scope.showDropDown = false;
					scope.clearWarnings();
					if (scope.searchList.indexOf(scope.searchText) >=0){
						scope.addItem(scope.searchText);
						scope.searchText = "";
						scope.showDropdown=false;
					}

					else{
						scope.itemNotFound = true;
					}
				}
				scope.addItem = function (_item){
					if (scope.selectedList.indexOf(_item)<0){
						scope.selectedList.push(_item);
						scope.action();
					}
					else
						scope.itemAlreadySelected=true;
				};
				scope.selectItem = function (_item){
					scope.showDropDown = false;
					scope.clearWarnings();
					if (scope.selectedList.indexOf(_item)<0)
						scope.searchText = _item;
				};
				scope.removeItem = function (_item){
					scope.selectedList.splice(scope.selectedList.indexOf(_item),1);
					scope.action();
				};
				scope.textFieldSelected = function(){
					scope.showDropdown = true;
					scope.clearWarnings();
				}	
				scope.clearWarnings = function(){
					scope.itemNotFound = false;
					scope.itemAlreadySelected = false;
				}	
				
				
			}
		};
	});