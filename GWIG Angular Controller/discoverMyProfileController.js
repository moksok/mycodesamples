(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('discoverMyProfileController', DiscoverMyProfileController);
    DiscoverMyProfileController.$inject = ['$scope', '$baseController', '$personalizedService', '$imageService', '$userProfileService', '$placesService'];

    function DiscoverMyProfileController(
         $scope
        , $baseController
        , $personalizedService
        , $imageService
        , $userProfileService
        , $placesService) {

        var vm = this;
        vm.profile = null;
        vm.places = null;
        vm.currentbuttontoshow = null;
        vm.data = {};
        vm.tabs = [
        { label: 'Reviews' },
        { label: 'Want To Go' },
        { label: 'Been There' },
        { label: 'Following' }
        ];
        vm.ratingStates = [
        { stateOn: 'glyphicon-usd', stateOff: 'glyphicon-usd' }];
        vm.selectedTab = vm.tabs[0];

        vm.$scope = $scope;
        vm.$personalizedService = $personalizedService;
        vm.$imageService = $imageService;
        vm.$userProfileService = $userProfileService;
        vm.$placesService = $placesService;

        vm.getCurrentUserSuccess = _getCurrentUserSuccess;
        vm.getDataSuccess = _getDataSuccess;
        vm.selectPlace = _selectPlace;
        vm.scrollSpy = _scrollSpy;
        vm.setSelectedTab = _setSelectedTab;

        $baseController.merge(vm, $baseController);

        vm.notify = vm.$placesService.getNotifier($scope);

        render();

        //Fire on load
        function render() {

            vm.$userProfileService.getCurrentUser(vm.getCurrentUserSuccess, vm.getCurrentUserError);

            vm.$personalizedService.getRatingList(vm.getDataSuccess);

        }

        function _getCurrentUserSuccess(data) {

            // resize profile picture and cover photo
            if (data.item.bgMyMedia.url && data.item.myMedia.url) {
                data.item.bgMyMedia.url = vm.$imageService.getImageResizeUrl(data.item.bgMyMedia.url, 300);
                data.item.myMedia.url = vm.$imageService.getImageResizeUrl(data.item.myMedia.url, 100);
            }


            vm.notify(function () {
                vm.profile = data;
            });

            //console.log(vm.profile);
        }

        function _getDataSuccess(data) {
           //console.log(data);

            if (data !== null) {

                vm.places = data;

                vm.notify(function () {

                    for (var i = 0; i < vm.places.length; i++) {
                        console.log(vm.places[i].url);
                        if (!vm.places[i].url) {
                            
                            //vm.places[i].url = vm.$imageService.getImageResizeUrl(vm.places[i].url, 400);
                            vm.places[i].url = "sabio-training.s3.amazonaws.com/C23/blank.png";
                        }
                    }

                });
            } else {
                vm.places = [];
            }

            if (data[0].review) {

                //Broadcast to map.js to load markers
                vm.$systemEventService.broadcast("onRatingsLoad", data);

            } else if (data[0].favoriteType) {

                //Broadcast to map.js to load markers
                vm.$systemEventService.broadcast("onFavoritePlacesLoad", data);
            } else {

                vm.$systemEventService.broadcast("onFollowingPlacesLoad", data);
            }


        }

        function _setSelectedTab(tab) {
            //console.log("set selected tab", tab);
            vm.selectedTab = tab;

            if (tab.label == "Reviews") {

                vm.$personalizedService.getRatingList(vm.getDataSuccess);

            } else if (tab.label == "Want To Go") {

                vm.data.FavoriteType = 2;
                vm.$personalizedService.getFavoritePlacesList(vm.data.FavoriteType, vm.getDataSuccess);

            } else if (tab.label == "Been There") {

                vm.data.FavoriteType = 1;
                vm.$personalizedService.getFavoritePlacesList(vm.data.FavoriteType, vm.getDataSuccess);

            } else if (tab.label == "Following") {

                vm.$personalizedService.getFollowingPlacesList(vm.getDataSuccess);
            }
        }

        function _selectPlace(place) {
            vm.$systemEventService.broadcast("placeSelected", place);
        }

        function _scrollSpy(item) {
            //console.log('are you working');
            vm.$systemEventService.broadcast('spyDiscoverMyProfile', item);
        }

    };

})();