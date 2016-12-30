(function () {

    angular.module(APPNAME)
        .controller('UserEventFeedController', UserEventFeedController);

    UserEventFeedController.$inject = ['$scope', '$baseController', '$userEventFeedService', '$gwigHub'];

    function UserEventFeedController($scope, $baseController, $userEventFeedService, $gwigHub) {

        var vm = this;

        vm.$eventFeedService = $userEventFeedService;
        vm.$scope = $scope;
        vm.notify = $userEventFeedService.getNotifier($scope);

        vm.eventPage = 1;
        vm.eventsPerPage = 20;
        vm.pageChanged = onPageChange;
        vm.totalEventFeedItems = 0;
        vm.eventFeed = [];
        vm.userName = $('#userName').val();

        $baseController.merge(vm, $baseController);

        render();

        function render() {
            $gwigHub.onEventFeedUpdate.then(function (data) {
                var any = $.grep(vm.eventFeed, function (v) {
                    return v.id === data.id;
                });

                if (!any.length) {
                    vm.eventFeed.unshift(data);
                }
            });

            vm.pageChanged();
        }

        function onPageChange() {
            vm.$eventFeedService.getUserEventFeed(vm.userName, vm.eventPage, vm.eventsPerPage).success(onGetEventFeed);
        }

        function onGetEventFeed(data) {
            //console.log('feed data ' + data.items);
            vm.notify(function () {
                vm.eventFeed = data.items;
                vm.totalEventFeedItems = Math.ceil(data.totalItemCount / vm.eventsPerPage);
                console.log(vm.eventFeed);
            });
        }
    }
})();