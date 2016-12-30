(function () {
    angular.module(APPNAME)
        .controller('personalizedNetworkController', PersonalizedNetworkController);
    PersonalizedNetworkController.$inject = ['$scope', '$baseController', '$personalizedService', '$imageService'];

    function PersonalizedNetworkController(
         $scope
        , $baseController
        , $personalizedService
        , $imageService) {

        var vm = this;
        vm.items = null;
        vm.places = [];
        vm.data = {};
        vm.data.CurrentPage = 0;
        vm.friendSideBarList = [];

        vm.$scope = $scope;
        vm.$personalizedService = $personalizedService;
        vm.$imageService = $imageService;

        vm.getFriendsOfFriendsReviews = _getFriendsOfFriendsReviews;
        vm.getSpecificFriendData = _getSpecificFriendData;
        vm.scrollSpy = _scrollSpy;
        vm.getPriceNum = _getPriceNum;
        vm.onLoadError = _onLoadError;
        vm.render = render;

        $baseController.merge(vm, $baseController);

        vm.notify = vm.$personalizedService.getNotifier($scope);

        vm.$systemEventService.listen("getNetworkAll", _loadNetworkAll);
        vm.$systemEventService.listen("selectNetworkId", _loadSelectedId);

        render();

        //Fire on load
        function render() {

            vm.data.FriendRecursionLevel = 2;
            vm.data.CurrentPage++;
            vm.data.ItemsPerPage = 25;

            vm.$personalizedService.getFriendsOfFriendsRatingsAll(vm.data, vm.getFriendsOfFriendsReviews, vm.onLoadError);
        }

        function _onLoadError() {
            console.log('data did not load');
        }

        //Receive network friend data
        function _getFriendsOfFriendsReviews(data) {

            //Broadcast to map.js to load markers
            vm.$systemEventService.broadcast("onNetworkReviews", data);

            if (data !== null) {

                vm.places = data;

                vm.notify(function() {
                    //Resize images loaded or apply default img
                    for (var i = 0; i < vm.places[i].length; i++) {
                        if (!vm.places[i].url) {
                            vm.places[i].url = "sabio-training.s3.amazonaws.com/C23/blank.png";
                        } 
                    }

                    //filter out duplicate usernames for navigation profile picture display
                    vm.friendSideBarList = removeDuplicates(data, 'userName');

                    // broadcast network list to navigation.js for sidebar view
                    vm.$systemEventService.broadcast("networkList", vm.friendSideBarList);

                });
            }
        }

        //Pure JS function to remove objects with duplicate userNames
        function removeDuplicates(originalArray, objKey) {
            var trimmedArray = [];
            var values = [];
            var value;

            for (var i = 0; i < originalArray.length; i++) {
                value = originalArray[i][objKey];

                if (values.indexOf(value) === -1) {
                    trimmedArray.push(originalArray[i]);
                    values.push(value);
                }
            }

            return trimmedArray;

        }

        //Load review for specific user clicked
        function _loadSelectedId(name, data) {

            var selectedId = data[1];

            vm.data.FriendRecursionLevel = 2;
            vm.data.CurrentPage = 1;
            vm.data.ItemsPerPage = 25;
            vm.data.selectedId = selectedId;

            vm.$personalizedService.getFriendsOfFriendsRatingsAll(vm.data, vm.getSpecificFriendData, vm.onLoadError);
        }

        //Receive data for specific user clicked
        function _getSpecificFriendData(data) {

            //Broadcast to map.js to load markers
            vm.$systemEventService.broadcast("onNetworkReviews", data);

            if (data !== null) {

                vm.places = data;

                vm.notify(function () {
                    //Resize images loaded or apply default img
                    for (var i = 0; i < vm.places[i].length; i++) {
                        if (!vm.places[i].url) {
                            vm.places[i].url = "sabio-training.s3.amazonaws.com/C23/blank.png";
                        }
                    }
                });

            }
        }

        function _loadNetworkAll() {

            vm.places = [];
            vm.data.CurrentPage = 0;
            vm.data.selectedId = null;

            render();
        }

        //Price integer in array format for ng-repeat on dollar sign icon
        function _getPriceNum(num) {
            var array = [];
            for (var i = 0; i < num; i++) {
                array.push(i);
            }
            return array;
        }

        function _scrollSpy(item) {

            vm.$systemEventService.broadcast('spyDiscoverNetwork', item);
        }

    };

})();