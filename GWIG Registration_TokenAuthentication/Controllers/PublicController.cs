using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sabio.Web.Services;
using System.Data.SqlClient;
using System.Data;
using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Models.ViewModels;
using Microsoft.Practices.Unity;

namespace Sabio.Web.Controllers
{
    [RoutePrefix("public")]
    public class PublicController : Controller
      

    {
        [Dependency]
        public IUserTokenService _IUserTokenService { get; set; }

        // Route for public registration page
        [Route("registration")]        
        public ActionResult Registration()
        {
            return View();
        }

        [Route("activationerror")]
        public ActionResult ActivationError()
        {
            return View();
        }
        [Route("resetpassword/{token:guid}")]
        public ActionResult ResetPassword(Guid token)
        {
            ItemViewModel<Guid> vm = new ItemViewModel<Guid>();
            vm.Item = token;
            return View("resetpassword",vm);
        }

        // View for new user when they click the activation link
        [Route("authentication/{Token:guid}"), HttpGet]
        public ActionResult Authentication(Guid Token)
        {
            TokenDomain NewUserModel = _IUserTokenService.Authenticate(Token);

            if (NewUserModel != null && NewUserModel.Used == false)
            {
                string UserIdForUpdateEmailConfirm = NewUserModel.UserId;
                Guid TokenForUpdateUsed = NewUserModel.Token;

                // update EmailConfirmed in dbo.NetAspUsers to true
                // update Used in dbo.Tokens to true
                _IUserTokenService.ActivateUser(TokenForUpdateUsed, UserIdForUpdateEmailConfirm);

                // forward to login page
                return RedirectToAction("Index", "Login");
            }
            else
            {
                // skeleton html page displaying error
                return RedirectToAction("ActivationError");
            }
        }


        [Route("PasswordReset/{Token:guid}"), HttpGet]
        public ActionResult PasswordReset(Guid Token)
        {
            TokenDomain UserModel = _IUserTokenService.Authenticate(Token);

            if (UserModel != null && UserModel.Used == false)
            {
                string UserIdForUpdateEmailConfirm = UserModel.UserId;
                Guid TokenForUpdateUsed = UserModel.Token;

                // update EmailConfirmed in dbo.NetAspUsers to true
                // update Used in dbo.Tokens to true
                _IUserTokenService.ActivateUser(TokenForUpdateUsed, UserIdForUpdateEmailConfirm);

                // forward to login page
                return RedirectToAction("resetpassword", "public", UserModel.Token);
            }
            else
            {
                // skeleton html page displaying error
                return RedirectToAction("ActivationError");
            }
        }
        [Route("logout")]
        public ActionResult Logout ()
        {
            UserService.Logout();
            return RedirectToAction("index", "home");
        }



    }
}