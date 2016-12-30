using Microsoft.Practices.Unity;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.PersonalizedFriends;
using Sabio.Web.Models.Requests.UserFavoritePlaces;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using Sabio.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/personalized")]
    public class PersonalizedApiController : BaseApiController
    {
        [Dependency]
        public IPersonalizedService PersonalizedService { get; set; }

        [Dependency]
        public IUserProfileService ProfileService { get; set; }

        [Route, HttpGet]
        public HttpResponseMessage getNetworkPlaces([FromUri] PersonalizedFriendRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            string userId = UserService.GetCurrentUserId();

            List<PersonalizedFriendsDomain> FriendsOfFriends = PersonalizedService.GetFriendsOfFriendsReviews(model, userId);

            ItemsResponse<PersonalizedFriendsDomain> newitemResponse = new ItemsResponse<PersonalizedFriendsDomain>();

            newitemResponse.Items = FriendsOfFriends;

            return Request.CreateResponse(HttpStatusCode.OK, newitemResponse.Items);
        }

        [Route("reviewed/{username}"), HttpGet]
        public BaseResponse GetReviewedPlaces(string username)
        {
            PublicUserProfileDomain user = ProfileService.GetPublicUserByUsername(username);

            if (user == null)
            {
                return Error("unable to find user " + username, HttpStatusCode.NotFound);
            }

            List<PersonalizedRatingDomain> reviews = PersonalizedService.GetPersonalReviews(user.userId);

            return Items(reviews);
        }

        [Route("favorite/{username}/{favoriteType:int}"), HttpGet]
        public BaseResponse GetFavoritePlaces(string username, int favoriteType)
        {
            PublicUserProfileDomain user = ProfileService.GetPublicUserByUsername(username);

            if (user == null)
            {
                return Error("unable to find user " + username, HttpStatusCode.NotFound);
            }

            List<PersonalizedFavoritePlaceDomain> places = PersonalizedService.GetFavoritePlaces(user.userId, favoriteType);

            return Items(places);
        }

        [Route("followed/{username}"), HttpGet]
        public BaseResponse GetFollowedPlaces(string username)
        {
            PublicUserProfileDomain user = ProfileService.GetPublicUserByUsername(username);

            if (user == null)
            {
                return Error("unable to find user " + username, HttpStatusCode.NotFound);
            }

            List<PersonalizedFollowingPlaceDomain> reviews = PersonalizedService.GetFollowingPlaces(user.userId);

            return Items(reviews);
        }

        [Route("reviewed"), HttpGet]
        [Authorize]
        public BaseResponse GetReviewedPlacesCurrentUser()
        {
            List<PersonalizedRatingDomain> reviews = PersonalizedService.GetPersonalReviews(UserService.GetCurrentUserId());

            return Items(reviews);
        }

        [Route("favorite/{favoriteType:int}"), HttpGet]
        [Authorize]
        public BaseResponse GetReviewedPlacesCurrentUser(int favoriteType)
        {
            List<PersonalizedFavoritePlaceDomain> places = PersonalizedService.GetFavoritePlaces(UserService.GetCurrentUserId(), favoriteType);

            return Items(places);
        }

        [Route("followed"), HttpGet]
        [Authorize]
        public BaseResponse GetFollowedPlacesCurrentUser()
        {
            List<PersonalizedFollowingPlaceDomain> reviews = PersonalizedService.GetFollowingPlaces(UserService.GetCurrentUserId());

            return Items(reviews);
        }

    }
}
