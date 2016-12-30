using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests;
using Sabio.Web.Services;
using Sabio.Web.Exceptions;
using Sabio.Web.Models.Responses;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;
using Nest;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.SystemEvents;
using Sabio.Web.Services.Interface;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/public")]
    public class PublicApiController : ApiController
    {
        [Dependency]
        public ISystemEventsService SystemEventService { get; set; }

        [Dependency]
        public IUserProfileService _userProfileService { get; set; }

        [Dependency]
        public IUserTokenService _IUserTokenService { get; set; }


        [Route("registration"), HttpPost]
        public HttpResponseMessage PostRegistration(PostRegistrationRequest model)
        {
            IdentityUser newUserRegistration;

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            // When model is valid, CreateUser will post email and pw to database
            try
            {

                newUserRegistration = UserService.CreateUser(model.Email, model.Password, model.Username);

                CreateUserProfileJsonData NewUserProfile = new CreateUserProfileJsonData();
                NewUserProfile.userName = model.Username;

                _userProfileService.CreateProfile(newUserRegistration.Id, NewUserProfile);


            }
            catch (IdentityResultException) // Display error code and message if user was not created
            {

                var ExceptionError = new ErrorResponse("Failed to register new user. (server side)");

                return Request.CreateResponse(HttpStatusCode.BadRequest, ExceptionError);

            }

            // Insert new user's id into token table and generate new token
            string UserId = newUserRegistration.Id;

            Guid NewToken = _IUserTokenService.Insert(UserId);

            // Pass new user's email and token into emailservice for activation link
            try
            {

                string NewUserEmail = newUserRegistration.Email;

                UserEmailService.SendProfileEmail(NewToken, NewUserEmail);

            }
            catch (NotImplementedException)
            {

                var ExceptionError = new ErrorResponse("Failed to send activation email to new user");

                return Request.CreateResponse(HttpStatusCode.BadRequest, ExceptionError);

            }

            SystemEventService.AddSystemEvent(new AddSystemEventModel
            {
                ActorUserId = UserId,
                ActorType = ActorType.User,
                EventType = SystemEventType.UserRegistration
            });

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("forgotpassword"), HttpPost]
        public HttpResponseMessage PostForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }



            ApplicationUser userInFormation = UserService.GetUser(model.Email);

            if (userInFormation == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Email Address is not in database.");
            }
            else
            {
                Guid newToken = _IUserTokenService.ForgotPasswordInsert(userInFormation.Id);

                UserEmailService.SendProfileEmailForForgotPassword(newToken, userInFormation.Email);
                return Request.CreateResponse(HttpStatusCode.OK, "PLease check your email");
            }
        }
        //Dans
        [Route("resetpassword/{token:Guid}"), HttpPut]
        public HttpResponseMessage GetPasswordUserId(Guid token, ForgotPasswordRequest model )
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Could not find UserId");
            }
            else
            {
                 TokenDomain theTokenData = _IUserTokenService.getUserIdByToken(token);
                string newToken = token.ToString();

                   bool passwordBool = UserService.newResetPassword(theTokenData.UserId, model.Password);
                return Request.CreateResponse(HttpStatusCode.OK, passwordBool);
            }
        }


    }
}

