(function () {
    "use strict";

    // Declare: Controller with APPNAME module
    angular.module(APPNAME)
        .controller('reviewsController', reviewsController);

    // Inject: $scope && $baseController property, reviews and vote service, bootstrap modal logic
    reviewsController.$inject = ['$scope', '$baseController', '$reviewsService', '$voteService', '$uibModal'];

    // Declare: Controller Function
    function reviewsController(
        $scope
        , $baseController
        , $reviewsService
        , $voteService
        , $uibModal) {

        // Save variables
        var vm = this;
        vm.items = null;
        vm.placeId = null;
        vm.avgRating = null;
        vm.postVote = {};
        vm.postVote.VoteType = 1;
        vm.postVote.VoterId = "";

        // Initialize 
        vm.$reviewsService = $reviewsService;
        vm.$voteService = $voteService;
        vm.$uibModal = $uibModal;
        vm.$scope = $scope;

        // Declaring functions
        vm.reviewError = _reviewError;
        vm.voteError = _voteError;
        vm.receiveReviewsByPlaceId = _receiveReviewsByPlaceId;
        vm.openModal = _openModal;
        vm.storePlacesId = _storePlacesId;
        vm.upVote = _upVote;
        vm.downVote = _downVote;
        vm.receiveAvgRating = _receiveAvgRating;

        // Inherit: View Model with $baseController
        $baseController.merge(vm, $baseController);

        vm.notify = vm.$reviewsService.getNotifier($scope);

        //Talks to placescontroller to receive placeid
        vm.$systemEventService.listen("placeLoaded", _storePlacesId);

        // placeId can be used by reviews controller and renders reviews function is called
        function _storePlacesId(eventName, payLoad) {
            
            var placeObjectIndex = 1;

            vm.placeId = payLoad[placeObjectIndex].id;

            //console.log("ID: ", vm.placeId);

        }

        render();


        // AJAX calls to render reviews and average rating
        function render() {

            // Retrieves all reviews for place page
            vm.$reviewsService.getByplacesId(vm.placeId, vm.receiveReviewsByPlaceId, vm.reviewError);

            // Retrieves average rating for this place
            vm.$reviewsService.placesAvg(vm.placeId, vm.receiveAvgRating, vm.reviewError);
        }

        // Receive array of all reviews
        function _receiveReviewsByPlaceId(data) {

            vm.notify(function () {
                vm.items = data;
            });
        }

        // Receive data for average rating
        function _receiveAvgRating(data) {

            // Average rating is rounded up to a whole number
            // bootstrap star rating has trouble with decimal numbers
            vm.notify(function () {
                vm.avgRating = Math.round(data);
            });
        }
       
        // Generic Error Handler
        function _reviewError(jqXhr, error) {
            console.log("Error Occuring: ", error);
        }

        function _voteError(error) {
            if (error.status == 401) {
                alert('You need to be logged in to vote');
            } else {
                console.log('Vote not inserted');
            }
        }

        // Function opens up modal and passes in placeId to submit review
        function _openModal() {
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: '/scripts/sabio/app/reviews/templates/submitReviewModal.html',
                controller: 'modalController as mc',
                resolve: {
                    placeId: function () {
                        return vm.placeId;
                    }
                }
            });

            // When modal closes it re-renders the reviews onto page
            modalInstance.result.then(render);

        }

        // Functions to handle upvote for review
        function _upVote(review) {

            vm.postVote.UserId = review.userId;
            vm.postVote.NetVote = 1;
            vm.postVote.ContentId = review.id;
            vm.postVote.UserName = review.userName;

            // Vote can only be submitted once per user and is permanent
            if (!review.hasVoted) {
                vm.$voteService.insert(vm.postVote, render, vm.voteError);
            } else {
                alert('You have already voted');
            }
        }

        // Function to handle downvote for review
        function _downVote(review) {

            vm.postVote.UserId = review.userId;
            vm.postVote.NetVote = -1;
            vm.postVote.ContentId = review.id;
            vm.postVote.UserName = review.userName;

            // Vote can only be submitted once per user and is permanent
            if (!review.hasVoted) {
                vm.$voteService.insert(vm.postVote, render, vm.voteError);
            } else {
                alert('You have already voted');
            }
        }
    }
})();