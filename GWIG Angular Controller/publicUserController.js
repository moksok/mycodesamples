
(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('publicUserController', PublicUserController);


    PublicUserController.$inject = ['$scope', '$baseController', '$publicUserService', '$followService', '$userProfileService','$followingPlacesService'];


    function PublicUserController(
        $scope
        , $baseController
        , $publicUserService
        , $followService
        , $userProfileService
        , $followingPlacesService) {

        var vm = this;
        vm.user = null;
        vm.places = [];
        vm.userName = $('#userName').val();
        vm.paginatedModel = {};
        vm.paginatedModel.CurrentPage = 0;

        vm.$scope = $scope;
        vm.$followService = $followService;
        vm.$publicUserService = $publicUserService;
        vm.$userProfileService = $userProfileService;
        vm.$followingPlacesService = $followingPlacesService;

        vm.receiveItems = _receiveItems;
        vm.receivePlacesItems = _receivePlacesItems;
        vm.onPubError = _onPubError;
        vm.addFollower = _addFollower;
        vm.showMorePlaces = _showMorePlaces;

        vm.currentUser = _dataItem;
        
        $baseController.merge(vm, $baseController);

        vm.notify = vm.$publicUserService.getNotifier($scope);

        render();

        // Fires AJAX function to receive data for public user
        function render() {
          
            vm.$userProfileService.getPublicUserByUserName(vm.userName, vm.receiveItems, vm.onPubError);
           
            vm.$followService.getCurrentUser(vm.currentUser, vm.onPubError)
        }

        // Load data for current logged in user to hide 'following and send message buttons'
        // in order to prevent a user to follow him/herself
        function _dataItem(data) {
            console.log(data.item)

            vm.notify(function () {
                vm.currentUser = data.item;

            })
        }

       
        // Receive data for public user you are visiting
        function _receiveItems(data) {
            console.log(data.item);

            vm.notify(function () {
                vm.user = data.item;

                // If no profile picture, then display default avatar image
                if (vm.user.myMedia.url == null) {
                    vm.user.myMedia.url = "https://www.kirkleescollege.ac.uk/wp-content/uploads/2015/09/default-avatar.png";
                }

            });
            
            // Load initial following places data
            _showMorePlaces();
        }

        // Function to call when user wants to load more places
        function _showMorePlaces() {

            vm.paginatedModel.userId = vm.user.userId;
            vm.paginatedModel.CurrentPage++;
            vm.paginatedModel.ItemsPerPage = 3;

            vm.$followingPlacesService.GetDataByUserId(vm.paginatedModel, vm.receivePlacesItems, vm.receivePlacesError);

        }

        // Receive data for following places of public user
        function _receivePlacesItems(data) {

            if (data.items == null) {
                console.log('no more data');
            } else {
                vm.notify(function () {
                    // Loop through next set and push it into array for 'show more' link
                    for (var i = 0; i < data.items.length; i++) {
                        vm.places.push(data.items[i]);
                    }
                });
            }
        }

        function _onPubError(jqXhr, Error) {
            console.error(error);
        }

        function _addFollower() {

            //console.log("inside addfollower", vm.user);

            if (vm.user.isFollowed) {
                console.log("inside deletefollower");
                vm.$followService.apiDeleteById(vm.user.userId, render, vm.onPubError);
                vm.$alertService.error(vm.user.userName + " has been unfollowed!", "Delete!")

            } else {
                console.log("inside addfollower");

                var followingUsers = {
                "FollowingUserId": vm.user.userId,
                "FollowerUserId": "",
                }

                vm.$followService.apiAddFollower(followingUsers, render, vm.onPubError);
                vm.$alertService.success("You are now following " + "" + vm.user.userName, "Success!");
           }
        }
    }

})();

