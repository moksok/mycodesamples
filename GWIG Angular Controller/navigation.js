(function () {
    angular.module(APPNAME)
        .controller('navigationController', NavigationController);
    NavigationController.$inject = ['$scope', '$location', '$baseController', '$discoverService'
                                    , '$tagsService', '$addressService', '$cityService', '$geocoder'
                                    , '$placesService', '$uibModal', '$window', '$imageService'];

    function NavigationController(
        $scope,
        $location,
        $baseController,
        $discoverService,
        $tagsService,
        $addressService,
        $cityService,
        $geocoder,
        $placesService,
        $uibModal,
        $window,
        $imageService) {


        var vm = this;
        $baseController.merge(vm, $baseController);
        vm.items = null;
        vm.allPlaces = null;
        vm.specificPlaces = null;
        vm.categoryPanel = null;
        vm.sidebarFillout = null;
        vm.error = false;
        vm.request = {
            categoryId: null
        }
        vm.cities = null;
        vm.cityMap = null;

        vm.selectedCat = null;
        vm.largestPic = null;

        vm.cityDropdown = "Select a City";
        vm.citySlug = null;

        vm.mode = 'community'   //  can be community, following, groups, missions

        vm.getTagsById = _getTagsById;
        vm.getFreeTagsById = _getFreeTagsById;

        vm.onLoadCat = _onLoadCat;
        vm.onLoadAll = _onLoadAll;
        vm.onResize = _windowResize;

        vm.sideBarViewSwitch = _sideBarViewSwitch;
        vm.loadFriendList = _loadFriendList;
        vm.loadGroupsList = _loadGroupsList;
        vm.friendList = [];
        vm.seeFriendPlace = _seeFriendPlace;
        vm.seeNetworkPlace = _seeNetworkPlace;
        vm.getFriendsAll = _getFriendsAll;
        vm.getNetworkAll = _getNetworkAll;

        vm.cityItems = _cityItems;
        vm.showCity = _showCity;        
        vm.selectPlace = onSelectPlace;
        vm.toggleNav = _toggleNav;
        vm.broadcastNav = _broadcastNav;
        vm.loadCities = _loadCities;


        vm.$scope = $scope;
        vm.$location = $location;
        vm.$addressService = $addressService;
        vm.$tagsService = $tagsService;
        vm.$discoverService = $discoverService;
        vm.$cityService = $cityService;
        vm.$uibModal = $uibModal;
        vm.$imageService = $imageService;

        vm.showPlaces = true;

        vm.notify = vm.$discoverService.getNotifier($scope);

        render(); /* Startup Function */

        vm.$scope.$on('$routeChangeSuccess', _onRouteChangeSuccess);

        vm.$scope.$watch(angular.bind(this, function (showPlaces) { return this.showPlaces; }), _broadcastNav);

        angular.element($window).on('resize', vm.onResize);

        //show selected color on category icon based on url slug
        function _onRouteChangeSuccess(next, current) {
            console.log("show current:", current);

            if (current && current.params) {

                if (current.params.citySlug) {

                    vm.mode = 'community';

                    if (vm.cityMap && vm.cityMap[current.params.citySlug]) {

                        vm.city = vm.cityMap[current.params.citySlug];

                        _broadcastCity();

                        vm.category = (current.params.categorySlug) ? { tagSlug: current.params.categorySlug } : { tagSlug: "all" };


                        vm.cityId = (vm.cityMap[current.params.citySlug].id);

                        vm.cityDropdown = vm.city.address;

                        vm.selectedCat = (current.params.categorySlug)

                        if (current.params.categorySlug == "all") {
                            vm.selectedCat = { tagSlug: "all" };
                        };
                        console.log("setting tag slug to all");

                        vm.location = { lat: vm.city.latitude, lng: vm.city.longitude }; //Search results are based on city dropdown

                    }

                }
                else if (current.params.groupCitySlug) {
                    vm.mode = 'groups';

                }
                else
                {
                    console.log("no mode for params", current.params)
                }
            }

            console.log("current mode:", vm.mode)

            return true;
        }

        function render() {
            console.log("render is working");
            vm.$tagsService.apiSearchTypeById(1, vm.getTagsById);
            vm.$tagsService.apiSearchTypeById(2, vm.getFreeTagsById);


            vm.setUpCurrentRequest(vm);

            _loadCities();

            //vm.$systemEventService.listen("onPlacesLoaded", _onLoadPlacesSuccess);
            vm.$systemEventService.listen("onlyFriendsList", _loadFriendList);
            vm.$systemEventService.listen("networkList", _loadFriendList);

            //Receive Groups Broadcast 
            vm.$systemEventService.listen("groupsList", _loadGroupsList);
        }

        function _loadCities() {
            vm.$cityService.get(_cityItems); //Ajax Get Call for cities
        }

        function _broadcastCity() {

            vm.$systemEventService.broadcast("cityChanged", vm.city);

        }


        //Google Search
        function onSelectPlace(place) {
            $geocoder.getPlace(place.place_id).then(onGetPlaceDetails);
        }

        //Get Details of search
        function onGetPlaceDetails(data) {
            // console.log(data);

            vm.placeContent = data;


            var largestPicWidth = 0;

            if (data.photos != null) {
                //Loop through pics finding the largest width
                for (var i = 0; i < data.photos.length; i++) {

                    if (data.photos[i].width > largestPicWidth) {
                        largestPicWidth = data.photos[i].width;
                        vm.largestPic = data.photos[i].getUrl({ 'maxWidth': largestPicWidth });
                    };
                };
            } else {

                vm.largestPic = "https://s3-us-west-2.amazonaws.com/sabio-training/C23/blank.png";

            };

            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: 'myModalContent.html',
                controller: 'modalController as mc',
                resolve: {
                    modalContent: function () {
                        return vm.placeContent

                    },
                    pictureContent: function () {
                        return vm.largestPic
                    },
                    categoryContent: function () {
                        return vm.allcategories
                    },
                    freeTagContent: function () {
                        return vm.allFreeTags
                    },
                    cityContent: function () {
                        return vm.cityId
                    }
                }

            });

            modalInstance.result.then(function (selectedItem) {
                vm.modalSelected = selectedItem;
            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });

            //console.log("Content inside Open Modal function", vm.placeContent);


        }

        //Get ajax success handler for cities
        function _cityItems(data) {


            var cities = data.items;
            var map = {};

            if (cities) {
                for (var x = 0; x < cities.length; x++) {
                    var city = cities[x];

                    map[city.slug] = city;
                }
            }



            vm.notify(function () {
                vm.cities = cities;
                vm.cityMap = map;

                //console.log("Get City Data: >>>", vm.cities, vm.cityMap);

            });

        }

        function _showCity(citySlug) {
            console.log("show the city", citySlug);

            vm.city = vm.cityMap[citySlug];

            vm.cityDropdown = vm.city.address;

            vm.cityId = vm.city.id;

            // $geocoder.searchPlacesNear(city.latitude , city.longitude, '250');


            _loadPage();
            _broadcastCity();
        }

        function _loadPage() {
            vm.$location.url(vm.city.slug + "/" + vm.category.tagSlug);


            // console.log("show city slug:", city.slug);
        }

        function _getFreeTagsById(data) {

            vm.notify(function () {
                vm.allFreeTags = data;
            });

            //console.log("free tags:", vm.allFreeTags);

        }

        function _getTagsById(data) {

            vm.notify(function () {
                vm.allcategories = data;
                //console.log("Grabbing Category Data:", vm.allcategories);

                //    _loadPlaces();
            });
        }

        function _onLoadAll(category) {

            //console.log("All Categories", vm.category);

            vm.category = { tagSlug: "all" };

            vm.selectedCat = { tagSlug: "all" };

            _loadPage();
        }

        function _onLoadCat(category) {

            vm.category = category;

            vm.selectedCat = category.tagSlug; //change and keep category icon to red

            //console.log("on load category: ", category);

            vm.$location.url("all" + "/" + vm.category.tagSlug);

            _loadPage();

        }

        function _sideBarViewSwitch() {
            return vm.$location.path();
        }

        function _loadFriendList(name, data) {
            vm.friendList = [];
            var profilePicList = data[1];
            for (var i = 0; i < profilePicList.length; i++) {
                if (!profilePicList[i].profileUrl) {
                    profilePicList[i].profileUrl = "https://social.liteforex.com/images/general/avatar.png";
                } else {
                    profilePicList[i].profileUrl = vm.$imageService.getImageResizeUrl(profilePicList[i].profileUrl, 200, 200);
                }
                vm.friendList.push(profilePicList[i]);
            }

            console.log(vm.friendList);
        }

        function _loadGroupsList(groupData) {
            console.log("Received Group Data in Navigation: ", groupData);
        }

        function _seeFriendPlace(friendId) {
            vm.$systemEventService.broadcast("selectedId", friendId);
        }

        function _seeNetworkPlace(networkId) {
            vm.$systemEventService.broadcast("selectNetworkId", networkId);
        }

        function _getFriendsAll() {
            vm.$systemEventService.broadcast("getFriendsAll");
        }

        function _getNetworkAll() {
            console.log('getnetworkall broadcast sent');
            vm.$systemEventService.broadcast("getNetworkAll");
        }

        function _toggleNav() {
            vm.showPlaces = !vm.showPlaces;
        }

        function _windowResize() {
            var width = angular.element($window).width();

            vm.showPlaces = width > 720;
        }

        function _broadcastNav(newVal) {
            vm.$systemEventService.broadcast("toggleNav", newVal);
        }
    }
})();