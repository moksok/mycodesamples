(function () {
    "use strict";

    // Declare: Controller with APPNAME module
    angular.module(APPNAME)
    .controller('myProfileController', MyProfileController);

    // Inject: $scope && $baseController property, reviews and vote service, bootstrap modal logic
    MyProfileController.$inject = ['$scope', '$baseController', '$userProfileService', '$mediaService'];

    // Declare: Controller Function
    function MyProfileController(
    $scope
    , $baseController
    , $userProfileService
    , $mediaService) {

        // Save variables
        var vm = this;
        vm.currentUser = null;
        vm.editUserInfoModel = {};
        vm.mediaId = {};
        vm.bgMediaId = {};
        vm.nameForm = null;
        vm.nameError = false;

        // Initialize
        vm.$userProfileService = $userProfileService;
        vm.$mediaService = $mediaService;
        vm.$scope = $scope;

        // Declaring functions
        vm.dashboardError = _dashboardError;
        vm.getCurrentUserData = _getCurrentUserData;
        vm.editName = _editName;
        vm.currentUserUpdateSuccess = _currentUserUpdateSuccess;


        // Declaring dropzone functions
        vm.dzAddedFile = _dzAddedFile;
        vm.dzError = _dzError;
        vm.dzMaxFileExceeded = _dzMaxFileExceeded;
        vm.dzSending = _dzSending;
        vm.dzSuccess = _dzSuccess;
        vm.dzBgSending = _dzBgSending;
        vm.dzBgSuccess = _dzBgSuccess;

        // Inherit: View Model with $baseController
        $baseController.merge(vm, $baseController);

        vm.notify = vm.$userProfileService.getNotifier($scope);

        render();

        // AJAX calls to receive current user data
        function render() {

            vm.$userProfileService.getCurrentUser(vm.getCurrentUserData, vm.reviewError);
        }

        function _dashboardError(error) {
            console.log('error is: ' + error);
        }

        // Received data from AJAX call and store data in variable vm.item
        function _getCurrentUserData(data) {
            console.log(data);

            vm.notify(function () {
                vm.currentUser = data;
            });

            vm.editUserInfoModel.firstName = data.item.firstName;
            vm.editUserInfoModel.lastName = data.item.lastName;
            vm.editUserInfoModel.tagLine = data.item.tagLine;
            vm.editUserInfoModel.profileContent = data.item.profileContent;
        }

        // Send updated info to update current user
        function _editName() {
            console.log(vm.editUserInfoModel);

            vm.nameError = true;
            
            if (vm.nameForm.$valid) {
                vm.$userProfileService.update(vm.editUserInfoModel, vm.currentUserUpdateSuccess);
            }
        }

        // Render page when update is successful
        function _currentUserUpdateSuccess() {
            render();
        }

        // Validation

        // Dropzone - Options Object
        vm.dzOptions = {
            url: "/api/media",
            autoProcessQueue: true,
            uploadMultiple: false,
            parallelUploads: 1,
            maxFiles: 1,
            maxFilesize: 5
        }

        function _dzError(file, errorMessage) {
            console.log('Dropzone error: ' + errorMessage);
        }

        function _dzAddedFile(file) {
            console.log(file);
        }

        function _dzMaxFileExceeded(file) {
            console.log('max file size exceeded');
        }

        function _dzSending(file, xhr, formData) {
            console.log('image is being sent');

            var userId = $('#userId').val();

            console.log(userId);
            formData.append("UserId", userId);
            formData.append("Mediatype", "Profile");
            formData.append("Title", "UserProfile");
            formData.append("Description", "UserProfile");
        }

        function _dzSuccess(file, response) {
            console.log("Dropzone image successfully sent to database.");

            vm.mediaId.mediaId = response.item;

            console.log(vm.mediaId);
            vm.$userProfileService.putProfilePicture(vm.mediaId, render);
            $('.dz-image-preview').hide();
            $('.dz-message').show();
        }

        function _dzBgSending(file, xhr, formData) {
            console.log('bkg image is being sent');

            var userId = $('#userId').val();

            console.log(userId);
            formData.append("UserId", userId);
            formData.append("Mediatype", "Profile");
            formData.append("Title", "backgroundpic");
            formData.append("Description", "backgroundpic");
        }

        function _dzBgSuccess(file, response) {
            console.log("Dropzone bkg image successfully sent to database.");

            vm.bgMediaId.bgMediaId = response.item;

            console.log(vm.mediaId);
            vm.$userProfileService.putBackgroundPicture(vm.bgMediaId, render);
            $('.dz-image-preview').hide();
            $('.dz-message').show();
        }
    }
})();