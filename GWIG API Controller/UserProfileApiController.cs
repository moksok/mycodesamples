using Sabio.Web.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sabio.Web.Services;
using Sabio.Web.Models.Responses;
using Sabio.Web.Domain;
using Microsoft.AspNet.Identity;
using Sabio.Web.Models;
using Microsoft.Practices.Unity;
using Sabio.Web.Services.Interface;
using Sabio.Web.Models.Requests.Pagination;

namespace Sabio.Web.Controllers.Api
{

    [RoutePrefix("api/Userprofile")] // set route prefix to user profile
    public class UserProfileController : ApiController
    {
        /*
        [Route(), HttpPost] // post 
        public HttpResponseMessage addUserProfile(CreateUserProfileJsonData model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else
            {
                string currentUserId = User.Identity.GetUserId();

                int userProfileId = UserProfileService.CreateProfile(currentUserId, model);

                ItemResponse<int> response = new ItemResponse<int>();

                response.Item = userProfileId;

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        */
        [Dependency]
        public IUserProfileService _userProfileService { get; set; }

        [Dependency]
        public IFollowingPlacesService _FollowingPlacesService { get; set; }

        [Route("AllUsers"), HttpGet] // get all items
        public HttpResponseMessage getAll()
        {


            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else
            {
                var allProfiles = _userProfileService.Get();

                return Request.CreateResponse(HttpStatusCode.OK, allProfiles);
            }
        }
        [Route("{userId}"), HttpGet] // get by id (the guid)

        public HttpResponseMessage userGetByid(_userProfileService model, string userId) // pass the userId
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }
            else
            {



                PublicUserProfileDomain userProfileCaptured = _userProfileService.GetPublicUserById(userId);
                ItemResponse<PublicUserProfileDomain> response = new ItemResponse<PublicUserProfileDomain>();
                response.Item = userProfileCaptured;

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
        }

        [Route(), HttpPut]
        [Authorize]        
        public HttpResponseMessage updateToForm(UserProfileUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else
            {
                model.id = UserService.GetCurrentUserId();

                _userProfileService.Put(model);

                ItemResponse<bool> response = new ItemResponse<bool>();

                response.IsSuccessful = true;

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }


        [Route("followingplaces"), HttpGet, HttpPost] // get by id (the guid)
        public HttpResponseMessage PublicUserPlacesFollowingGetByUserId(PaginatedRequest model) // pass the userId
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }
            else
            {

                List<FollowingPlacesDomain> PublicUserPlacesFollowing = _FollowingPlacesService.FollowingPlacesGetDataByUserId(model);
                ItemsResponse<FollowingPlacesDomain> response = new ItemsResponse<FollowingPlacesDomain>();
                response.Items = PublicUserPlacesFollowing;

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
        }


        [Route("public/{userName}"), HttpGet]
        public HttpResponseMessage PublicUserGetByUsername(PublicUserProfileDomain model, string userName)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }
            else
            {

                PublicUserProfileDomain PublicUser = _userProfileService.GetPublicUserByUsername(userName);

                ItemResponse<PublicUserProfileDomain> response = new ItemResponse<PublicUserProfileDomain>();

                response.Item = PublicUser;

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
        }

        [Route("current"),HttpGet]
        [Authorize]
        public HttpResponseMessage getUserForProfile()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }

            string UserId = UserService.GetCurrentUserId();
            //Guid userGuid = new Guid(UserId);

            PublicUserProfileDomain profile = _userProfileService.GetPublicUserById(UserId);

            ItemResponse<PublicUserProfileDomain> response = new ItemResponse<PublicUserProfileDomain>();

            response.Item = profile;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("picture/update"),HttpPut]
        [Authorize]
        public HttpResponseMessage updateUserProfile(updateUserProfilePicture model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }

           model.userId = UserService.GetCurrentUserId();

           // model.UserId = userId;

            _userProfileService.updateProfilePicture(model);

            ItemResponse<bool> response = new ItemResponse<bool>();
            response.IsSuccessful = true;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("background/update"),HttpPut]
        [Authorize]
        public HttpResponseMessage updateUserBackground(updateUserBackgroundPicture model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            }

            model.userId = UserService.GetCurrentUserId();

            _userProfileService.updateBackgroundPicture(model);

            ItemResponse<bool> response = new ItemResponse<bool>();
            response.IsSuccessful = true;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}