var app = angular.module('app', []);
//"ui.bootstrap"
app.controller('MainCtrl', function ($scope) {

    $scope.allLanguages = [
      { name: 'Arabic' },
      { name: 'English' },
      { name: 'French' },
      { name: 'German' },
      { name: 'Hindi' },
      { name: 'Japanese' },
      { name: 'Korean' },
      { name: 'Mandarin' },
      { name: 'Portuguese' },
      { name: 'Russian' },
      { name: 'Spanish' },
      { name: 'Urdu' }
    ];
    $scope.user = {
        id: 1,
        name: 'Foo Bar',
        languages: [
          { name: 'English' },
          { name: 'Spanish' }
        ]
    };

    $scope.addLanguageToUser = function (language, user) {
        $scope.user.languages.push(language)
    };

    $scope.removeLanguageFromUser = function (language, user) {
        var idx = $scope.user.languages.indexOf(language);
        $scope.user.languages.splice(idx, 1);
    };
});

app.directive("searchableMultiselect", function ($timeout) {
    return {
        templateUrl: '/Content/searchableMultiselect.html',
        restrict: 'AE',
        scope: {
            displayAttr: '@',
            selectedItems: '=',
            allItems: '=',
            readOnly: '=',
            addItem: '&',
            removeItem: '&'
        },
        link: function (scope, element, attrs) {
            element.bind('click', function (e) {
                e.stopPropagation();
            });

            scope.width = element[0].getBoundingClientRect();

            scope.updateSelectedItems = function (obj) {
                var selectedObj;
                for (i = 0; typeof scope.selectedItems !== 'undefined' && i < scope.selectedItems.length; i++) {
                    if (scope.selectedItems[i][scope.displayAttr].toUpperCase() === obj[scope.displayAttr].toUpperCase()) {
                        selectedObj = scope.selectedItems[i];
                        break;
                    }
                }
                if (typeof selectedObj === 'undefined') {
                    scope.addItem({ item: obj });
                } else {
                    scope.removeItem({ item: selectedObj });
                }
            };

            scope.isItemSelected = function (item) {
                if (typeof scope.selectedItems === 'undefined') return false;

                var tmpItem;
                for (i = 0; i < scope.selectedItems.length; i++) {
                    tmpItem = scope.selectedItems[i];
                    if (typeof tmpItem !== 'undefined'
					&& typeof tmpItem[scope.displayAttr] !== 'undefined'
					&& typeof item[scope.displayAttr] !== 'undefined'
					&& tmpItem[scope.displayAttr].toUpperCase() === item[scope.displayAttr].toUpperCase()) {
                        return true;
                    }
                }

                return false;
            };

            scope.commaDelimitedSelected = function () {
                var list = "";
                angular.forEach(scope.selectedItems, function (item, index) {
                    list += item[scope.displayAttr];
                    if (index < scope.selectedItems.length - 1) list += ', ';
                });
                return list.length ? list : "Nothing Selected";
            }
        }
    }
});
