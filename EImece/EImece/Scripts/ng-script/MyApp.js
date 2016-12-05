(function () {

    //angular module
    var myApp = angular.module('myApp', ['angularTreeview']);

    //controller
    myApp.controller('myController', function ($scope, $http) {
        fetch();
        function fetch() {
            $http({
                method: 'GET',
                url: '/admin/ProductCategories/GetCategories'
            }).then(function successCallback(response) {
                console.log(response.data.treeList);
                $scope.ProductCategoryList = response.data.treeList;
            }, function errorCallback(response) {
                console.log(response);
            });
        }
    });

})();

