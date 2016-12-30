using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sabio.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Sabio.Web.Exceptions;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Sabio.Data.Providers;
using Sabio.Web.Controllers;
using System.Threading.Tasks;

namespace Sabio.Web.Services
{
    public class UserService : BaseServiceStatic
    {
        //public UserService(IDao dataProvider) : base(dataProvider)
        //{
        //}

        private static ApplicationUserManager GetUserManager()
        {
            return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public static ApplicationSignInManager GetSigninManager()
        {
            return HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
        }

        public static IdentityUser CreateUser(string email, string password, string username)
        {
            ApplicationUserManager userManager = GetUserManager();

            ApplicationUser newUser = new ApplicationUser { UserName = username, Email = email, LockoutEnabled = false };
            IdentityResult result = userManager.Create(newUser, password);

            if (result.Succeeded)
            {
                return newUser;
            }

            throw new IdentityResultException(result);
        }

        public static bool LinkUserWithExternalLogin()
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            ApplicationUserManager userManager = GetUserManager();

            var loginInfo = authenticationManager.GetExternalLoginInfo();
            var result = userManager.AddLogin(GetCurrentUserId(), loginInfo.Login);

            return result.Succeeded;
        }

        public static bool Signin(string emailaddress, string password)
        {
            ApplicationSignInManager signinManager = GetSigninManager();

            var status = signinManager.PasswordSignIn(emailaddress, password, true, false);

            return status == SignInStatus.Success;
        }

        public static bool ExternalSignin()
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            ApplicationSignInManager signinManager = GetSigninManager();

            var externalLogin = authenticationManager.GetExternalLoginInfo();
            var status = signinManager.ExternalSignIn(externalLogin, true);

            return status == SignInStatus.Success;
        }

        public static bool IsUser(string emailaddress)
        {
            ApplicationUserManager userManager = GetUserManager();
            ApplicationUser user = userManager.FindByEmail(emailaddress);

            return user != null;
        }

        public static ApplicationUser GetUser(string emailaddress)
        {
            ApplicationUserManager userManager = GetUserManager();
            ApplicationUser user = userManager.FindByEmail(emailaddress);

            return user;
        }

        public static ApplicationUser GetUserById(string userId)
        {
            ApplicationUserManager userManager = GetUserManager();
            ApplicationUser user = userManager.FindById(userId);

            return user;
        }

        public static bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            bool result = false;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("You must provide a userId and a password");
            }

            ApplicationUser user = GetUserById(userId);

            if (user != null)
            {
                ApplicationUserManager userManager = GetUserManager();

                result = userManager.ChangePassword(user.Id, oldPassword, newPassword).Succeeded;
            }

            return result;
        }


        public static void Logout()
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut();
        }


        public static IdentityUser GetCurrentUser()
        {
            if (!IsLoggedIn())
                return null;

            ApplicationUserManager userManager = GetUserManager();
            IdentityUser currentUserId = userManager.FindById(GetCurrentUserId());

            return currentUserId;
        }

        public static string GetCurrentUserId()
        {
            return HttpContext.Current?.User.Identity.GetUserId();
        }

        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetCurrentUserId());
        }

        // Dan
        public static bool newResetPassword(string userId, string newPassword)
        {
            bool result = false;

            ApplicationUserManager userManager = GetUserManager();
            ApplicationUser user = GetUserById(userId);
            
            if (user != null)
            {
                string newToken = userManager.GeneratePasswordResetToken(user.Id);

                result = userManager.ResetPassword(user.Id, newToken, newPassword).Succeeded;
            }

            return result;
        }

        public static async Task<bool> ForceUsernameLoginAsync(string username)
        {
            bool result = false;

            ApplicationUserManager userManager = GetUserManager();

            var context = HttpContext.Current.GetOwinContext();

            var updatedUser = userManager.FindByName(username);

            // updated identity from the new data in the user object
            //var result = await Task.Run(() => DoWork());
            var newIdentity = await Task.Run(() => updatedUser.GenerateUserIdentityAsync(userManager));

            // sign in again
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = true };
            context.Authentication.SignIn(authenticationProperties, newIdentity);

            result = true;
            return result;

        }

        public static bool DisableUser(string userId)
        {

            ApplicationUser user = GetUserById(userId);
            ApplicationUserManager userManager = GetUserManager();

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.UtcNow.AddYears(99);
            userManager.SetLockoutEnabled(user.Id, true);
            IdentityResult result = userManager.Update(user);

            return result.Succeeded;
        }
    }
}