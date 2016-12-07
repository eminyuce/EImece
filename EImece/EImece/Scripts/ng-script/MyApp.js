(function () {

    //angular module
    var myApp = angular.module('myApp', ['angularTreeview', 'datatables']);
  //  var myApp = angular.module('myApp', []);

    //controller
    myApp.controller('treeViewController', function ($scope, $http) {
        fetch();
        function fetch() {
            $http({
                method: 'GET',
                url: '/admin/ProductCategories/GetCategories'
            }).then(function successCallback(response) {
                console.log(response.data.treeList);
                $scope.ProductCategoryList = response.data.treeList;
                console.log($scope);
                console.log($http);
                //$("#selectedTreeItem").text("test");
            }, function errorCallback(response) {
                console.log(response);
            });
        }
    });


  
   // var app2 = angular.module('myApp', ['datatables']);
    myApp.controller('BindAngularDirectiveCtrl', generateGrid);

    function generateGrid($scope, $compile, DTOptionsBuilder, DTColumnBuilder) {
        var vm = this;
        vm.message = '';
        vm.edit = edit;
        vm.delete = deleteRow;
        vm.dtInstance = {};
        vm.items = {};
        vm.selected = {};
        vm.selectAll = false;
        vm.toggleAll = toggleAll;
        vm.toggleOne = toggleOne;

        vm.dtOptions = DTOptionsBuilder.newOptions().withOption('ajax', {
            dataSrc: "data",
            url: "/test/getdata",
            type: "POST"
        })
                    .withOption('processing', true) //for show progress bar
                    .withOption('serverSide', true) // for server side processing
                    .withPaginationType('full_numbers') // for get full pagination options // first / last / prev / next and page numbers
                    .withDisplayLength(10) // Page size
                    .withOption('aaSorting', [0, 'asc']) // for default sorting column // here 0 means first column
                    .withOption('createdRow', createdRow)
                    .withOption('headerCallback', headerCallback);

        var titleHtml = '<input type="checkbox" ng-model="showCase.selectAll" ng-click="showCase.toggleAll(showCase.selectAll, showCase.selected)">';


        vm.dtColumns = [
             DTColumnBuilder.newColumn(null).withTitle(titleHtml).notSortable().renderWith(checkboxesHtml),
             DTColumnBuilder.newColumn("Id", "ID").withOption('name', 'Id'),
             DTColumnBuilder.newColumn("Name", "Name").withOption('name', 'Name'),
             DTColumnBuilder.newColumn("IsActive", "Is Active").withOption('name', 'IsActive'),
             DTColumnBuilder.newColumn("Position", "Position").withOption('name', 'Position'),
             DTColumnBuilder.newColumn(null).withTitle('Actions').notSortable()
             .renderWith(actionsHtml)
        ];
     
        function headerCallback(header) {
            if (!vm.headerCompiled) {
                // Use this headerCompiled field to only compile header once
                vm.headerCompiled = true;
                $compile(angular.element(header).contents())($scope);
            }
        }
        function edit(product) {
            vm.message = 'You are trying to edit the row: ' + JSON.stringify(product);
            // Edit some data and call server to make changes...
            // Then reload the data so that DT is refreshed

 
            vm.dtInstance.reloadData();
        }
        function deleteRow(product) {
            vm.message = 'You are trying to remove the row: ' + JSON.stringify(product);
            // Delete some data and call server to make changes...
            // Then reload the data so that DT is refreshed

 
            vm.dtInstance.reloadData();
        }
        function createdRow(row, data, dataIndex) {
            // Recompiling so we can bind Angular directive to the DT
            $compile(angular.element(row).contents())($scope);
        }
      
        function actionsHtml(data, type, full, meta) {
            vm.items[data.Id] = data;

            return '<button class="btn btn-warning"   ng-click="showCase.edit(showCase.items[' + data.Id + '])">' +
                '   <i class="fa fa-edit">Edit</i>' +
                '</button>&nbsp;' +
                '<button class="btn btn-danger"   ng-click="showCase.delete(showCase.items[' + data.Id + '])" )">' +
                '   <i class="fa fa-trash-o">Delete</i>' +
                '</button>'+
                '<a href="/admin/products/saveoredit/'+data.Id+'">Edit 2</a>'
        }
        function checkboxesHtml(data, type, full, meta) {
            vm.selected[data.Id] = false;
            return '<input type="checkbox" name="selectedItems" ng-model="showCase.selected[' + data.Id + ']" ng-click="showCase.toggleOne(showCase.selected)">';
        }
        function toggleAll(selectAll, selectedItems) {
            console.log("test");
            for (var id in selectedItems) {
                if (selectedItems.hasOwnProperty(id)) {
                    selectedItems[id] = selectAll;
                }
            }
        }
        function toggleOne(selectedItems) {
            for (var id in selectedItems) {
                if (selectedItems.hasOwnProperty(id)) {
                    if (!selectedItems[id]) {
                        vm.selectAll = false;
                        return;
                    }
                }
            }
            vm.selectAll = true;
        }
        console.log(vm);
    }
})();

