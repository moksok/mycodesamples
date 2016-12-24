(function () {
    angular.module(APPNAME)
        .controller('mapController', MapController);
    MapController.$inject = ['$scope', '$baseController', '$discoverService', '$tagsService', '$addressService', '$cityService', '$rootScope'];

    function MapController(
        $scope,
        $baseController,
        $discoverService,
        $tagsService,
        $addressService,
        $cityService,
        $rootScope) {


        var vm = this;
        $baseController.merge(vm, $baseController);

        vm.$scope = $scope;
        vm.$addressService = $addressService;
        vm.$tagsService = $tagsService;
        vm.$discoverService = $discoverService;
        vm.$cityService = $cityService;

        vm.items = null;
        vm.allPlaces = null;
        vm.specificPlaces = null;
        vm.categoryPanel = null;
        vm.sidebarFillout = null;
        vm.error = false;
        vm.activeCity = null;
        vm.activeMarker = null;
        vm.activeModel = null;
        vm.request = {
            categoryId: null
        }

        vm.selectedCat = null;

        vm.onLoadPlacesSuccess = _onLoadPlacesSuccess;
        vm.openInfoForPlaceId = _openInfoForPlaceId;
        vm.onMarkerClick = _onMarkerClick;
        
        vm.markers = [];

        vm.notify = vm.$discoverService.getNotifier($scope);

        vm.changeMapCenter = _changeMapCenter; //map controller

        vm.map = {
            center: { latitude: 39.7392, longitude: -108.5506 },
            zoom: 5,
            options: {

                scrollwheel: false,
                styles: [{ "featureType": "road", "elementType": "geometry", "stylers": [{ "visibility": "simplified" }] }, { "featureType": "road.arterial", "stylers": [{ "hue": 149 }, { "saturation": -78 }, { "lightness": 0 }] }, { "featureType": "road.highway", "stylers": [{ "hue": -31 }, { "saturation": -40 }, { "lightness": 2.8 }] }, { "featureType": "poi", "elementType": "label", "stylers": [{ "visibility": "off" }] }, { "featureType": "landscape", "stylers": [{ "hue": 163 }, { "saturation": -26 }, { "lightness": -1.1 }] }, { "featureType": "transit", "stylers": [{ "visibility": "off" }] }, { "featureType": "water", "stylers": [{ "color": "#5e9bff" }] }],
                zoomControlOptions: {
                    position: google.maps.ControlPosition.RIGHT_CENTER
                }
            },
            markersEvents: {
                click: _onMarkerClick
            },
            window: {
                marker: {},
                show: false,
                closeClick: function () {
                    this.show = false;
                },
                options: {} // define when map is ready

            }
        };

        render(); /* Startup Function */

        function render() {

            vm.$systemEventService.listen("onPlacesLoaded", _onLoadPlacesSuccess);

            vm.$systemEventService.listen("cityChanged", _onCityChanged);

            vm.$systemEventService.listen("onFriendsReviews", _onLoadPlacesSuccess);

            vm.$systemEventService.listen("onNetworkReviews", _onLoadPlacesSuccess);            

            vm.$systemEventService.listen("spyPlace", _onSpyPlace);

            vm.$systemEventService.listen("spyDiscoverMyProfile", _onSpyPlace);

            vm.$systemEventService.listen("spyDiscoverFriends", _onSpyPlace);

            vm.$systemEventService.listen("spyDiscoverNetwork", _onSpyPlace);
        }

        function _onMarkerClick(marker, eventName, model, arguments)
        {
            vm.activeModel = model;
            _openInfoForPlaceId(model.id);
        }

        function _closeAllMarkers()
        {
            for (var x = 1; x < vm.markers.length; x++) {
                vm.markers[x].show = false;
            }
        }

        function _openInfoForPlaceId(placeId)
        {
            
            if (vm.markers && vm.markers.length) {
                for (var x = 0; x < vm.markers.length; x++) {
                    
                    if (vm.markers[x].id == placeId) {
                        if (vm.markers[x] !== vm.activeMarker) {
                            
                            vm.markers[x].show = true;
                            vm.activeMarker = vm.markers[x];
                            vm.map.center = vm.markers[x].center;                            
                        }
                    }
                    else {
                        vm.markers[x].show = false;
                    }
                }
            }
        }

        function _onPlaceSelected(name, data)
        {
            console.log("MAP place selected", data);
            if (data && data[1]) {

                    var placeId = data[1].id || data[1].placesId;



                _openInfoForPlaceId(placeId);
            }
        }

        //Map Controller
        function _changeMapCenter(markerId) {

            for (var i = 0; i < vm.markers.length; i++) {
                if (markerId === vm.markers[i].id) {
                    console.log("this is vm.markers for show marker", vm.markers)
                    // open this markers infoWindow
                    //vm.markers[i].show = true;
                    vm.map.zoom = 11;
                    console.log(vm.markers[i].center);
                    vm.map.center = vm.markers[i].center;
                }

                else {
                    vm.markers[i].show = false;
                }
            }
        }

        function _onCityChanged(name, data) {

            if (data && data[1]) {

                console.log("MAP on city changed", data[1]);

                var city = data[1];

                vm.map.center.latitude = city.latitude;
                vm.map.center.longitude = city.longitude;
                vm.map.zoom = 12;
                vm.activeCity = city;
            }
        };

        function _onLoadPlacesSuccess(name, data) {
            vm.markers = [];

            console.log('on load places success', data);

            if (data && data[1] || data[1].items) {

                var places = null;

                if (data[1].items) {
                    places = data[1].items;
                } else {
                    places = data[1];
                }

                

                for (var i = 0; i < places.length; i++) {

                    var place = places[i];
                    
                    if (place.category == null) {
                        switch (place.categoryId) {
                            case 1:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/486d2f3a-2505-4ddc-a56e-6ba232148902bar.png";
                                break;
                            case 2:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/3def73e4-ddc0-4670-ad7f-dd78eef005e3beach.png";
                                break;
                            case 3:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/0f6c19cb-38db-4cd4-82f5-5b171cf10314film.png";
                                break;
                            case 4:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/3d2da377-c14f-418a-aa7a-e1aac7d128a8food.png";
                                break;
                            case 5:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/ea0d758f-b044-4098-9176-edabcd08bc16post.png";
                                break;
                            case 6:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/1996582d-1dd7-4123-b216-728417b19328shopping.png";
                                break;
                            case 7:
                                place.categoryId = "Https://s3-us-west-2.amazonaws.com/sabio-training/C23/94c68a7a-28ac-4cc8-bda2-cc35554cd5d5sport.png";
                                break;
                        }
                    }

                    // note: it was a huge pain in the ass to load the map info window 
                    // from an external template but we had to do that to get the ng-* directives to work properly
                    // in the info window. here is the only SO I found that has a good answer: http://stackoverflow.com/a/36095056
                    var marker = {

                        latitude: (place.location) ? place.location.lat : place.latitude,
                        longitude: (place.location) ? place.location.lon : place.longitude,
                        id: (place.placesId) ? place.placesId : place.id,

                        info:{
                            image: (place.mediaurl) ? place.mediaurl : place.url,
                            name: place.name,
                            description: place.description,
                            website: place.website,
                            slug: place.slug,
                            phonenumber: place.phonenumber,
                        },                        
                        options: {
                            icon: (place.category) ? place.category.url : place.categoryId

                        },
                        show: false,
                        center: {
                            latitude: (place.location) ? place.location.lat : place.latitude,
                            longitude: (place.location) ? place.location.lon : place.longitude
                        }
                        , template:"/scripts/sabio/app/discover/templates/mapInfoWindow.html"
                    };
                    
                    if (marker.image)
                    {
                        //console.log(marker.image);
                        marker.src = marker.image.replace(/.*?:\/\//g, "");

                        //marker.image = marker.image.replace(/.*?:\/\//g, "");                        
                    }

                   // marker.info = marker; //this is a hack so that we can pass it into the info window template

                    //console.log('marker', marker);

                    vm.markers.push(marker);

                }

                if (vm.markers && vm.markers.length) {

                    if (vm.markers.length == 1) {

                        //console.log("inside vm.markers.length if statement");

                        vm.markers[0].show = true;

                        //console.log("vm.markers", vm.markers);
                    }

                    //console.log("inside markers if statement"); 

                    vm.map.center = vm.markers[0].center;
                    vm.map.zoom = 11;
                }

                vm.$systemEventService.broadcast("onLoadEnd");
            }
        }

        function _onSpyPlace(name, data) {
            var place = data[1];

            vm.$scope.$apply(function () {
                if (place.placesId) {
                    
                    _openInfoForPlaceId(place.placesId);
                } else if (place.id) {
                    _openInfoForPlaceId(place.id);
                }
                
            });
        }
    }
})();