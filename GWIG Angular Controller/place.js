(function () {
    angular.module(APPNAME)
        .controller('placeController', PlaceController);
    PlaceController.$inject = ['$scope', '$baseController', '$discoverService', '$tagsService', '$addressService', '$cityService', '$imageService'];

    function PlaceController(
        $scope,
        $baseController,
        $discoverService,
        $tagsService,
        $addressService,
        $cityService,
        $imageService) {

        var vm = this;
        $baseController.merge(vm, $baseController);

        vm.$scope = $scope;
        vm.$addressService = $addressService;
        vm.$tagsService = $tagsService;
        vm.$discoverService = $discoverService;
        vm.$cityService = $cityService;
        vm.$imageService = $imageService;

        vm.toggleNav = _toggleNav;
        vm.selectPlace = _selectPlace;
        vm.getPriceNum = _getPriceNum;
        vm.scrollSpy = _scrollSpy;

        vm.visible = true;
        vm.activePlace = null;

        vm.request = {

        }
        vm.places = null;

        vm.notify = vm.$discoverService.getNotifier($scope);


        render(); /* Startup Function */

        function render() {
            console.log("route params:", vm.$routeParams);

            vm.$systemEventService.listen("toggleNav", vm.toggleNav)

            _loadPlaces();
        }

        function _selectPlace(place) {
            vm.activePlace = place;
            vm.$systemEventService.broadcast("placeSelected", place);
        }

        function _loadPlaces() {


            if (vm.$routeParams.citySlug) {

                vm.request.city = vm.$routeParams.citySlug

            }


            if (vm.$routeParams.categorySlug) {

                vm.request.category = vm.$routeParams.categorySlug
                vm.request.itemsPerPage = 50;
            }

            // var index = vm.request.indexOf();

            if (vm.request.category == "all") {

                vm.request.category = null;
                vm.request.itemsPerPage = 100;

            }



            if (vm.request.city == "all") {

                vm.request.city = null;

            }

            vm.$systemEventService.broadcast("onLoadStart");

            vm.$discoverService.discover(vm.request, _onLoadPlacesSuccess, _onLoadPlacesError);
            console.log("Get Active Category ID", vm.request);
        }


        function _onLoadPlacesError()
        {
            vm.$systemEventService.broadcast("onLoadEnd");
        }
        function _onLoadPlacesSuccess(data) {
            console.log("on load places", data);
            vm.$systemEventService.broadcast("onPlacesLoaded", data);
            
            vm.notify(function () {
                for (var i = 0; i < data.items.length; i++) {
                    if (data.items[i].mediaurl) {
                        data.items[i].mediaurl = vm.$imageService.getImageResizeUrl(data.items[i].mediaurl, 640, 480);
                    } else {
                        data.items[i].mediaurl = "https://s3-us-west-2.amazonaws.com/sabio-training/C23/blank.png";
                    }

                }
                vm.places = data.items;

            });
        };

        function _toggleNav(visible) {
            vm.visible = visible;
        }

        function _getPriceNum(num) {
            var array = [];
            for (var i = 0; i < num; i++) {
                array.push(i);
            }
            return array;
        }

        function _scrollSpy(item) {
            console.log('are you working');
            vm.$systemEventService.broadcast('spyPlace', item);
        }
    }
})();