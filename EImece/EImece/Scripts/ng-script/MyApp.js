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

    var titleHtml = '<input type="checkbox" ng-model="showCase.selectAll" ng-click="showCase.toggleAll(showCase.selectAll, showCase.selected)">';

    var app = angular.module('myApp', ['datatables']);
    app.controller('homeCtrl', ['$scope', '$http', 'DTOptionsBuilder', 'DTColumnBuilder',
        function ($scope, $http, DTOptionsBuilder, DTColumnBuilder) {
            $scope.dtColumns = [
                 DTColumnBuilder.newColumn(null).withTitle(titleHtml).notSortable()
            .renderWith(function (data, type, full, meta) {
                vm.selected[full.id] = false;
                return '<input type="checkbox" ng-model="showCase.selected[' + data.id + ']" ng-click="showCase.toggleOne(showCase.selected)">';
            }),
                //here We will add .withOption('name','column_name') for send column name to the server 
                DTColumnBuilder.newColumn("Id", "ID").withOption('name', 'Id'),
                DTColumnBuilder.newColumn("Name", "Name").withOption('name', 'Name'),
                DTColumnBuilder.newColumn("IsActive", "Is Active").withOption('name', 'IsActive'),
                DTColumnBuilder.newColumn("Position", "Position").withOption('name', 'Position'),
                DTColumnBuilder.newColumn("UpdatedDate", "Updated Date").withOption('name', 'UpdatedDate')
            ]

            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('ajax', {
                dataSrc: "data",
                url: "/test/getdata",
                type: "POST"
            })
            .withOption('processing', true) //for show progress bar
            .withOption('serverSide', true) // for server side processing
            .withPaginationType('full_numbers') // for get full pagination options // first / last / prev / next and page numbers
            .withDisplayLength(10) // Page size
            .withOption('aaSorting', [0, 'asc']) // for default sorting column // here 0 means first column
        }])

})();

