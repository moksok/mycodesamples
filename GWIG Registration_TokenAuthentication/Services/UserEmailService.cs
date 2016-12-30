using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using SendGrid;

namespace Sabio.Web.Services
{
    public class UserEmailService : BaseServiceStatic
    {
        // SendGrid email service plugin (using older version 6)
        public static void SendProfileEmail(Guid Token, string Email)
        {

            SendGridMessage ActivateUserEmail = new SendGridMessage();
            ActivateUserEmail.AddTo(Email);
            ActivateUserEmail.From = new MailAddress("GWIGSupport@GWIG.com");
            ActivateUserEmail.Subject = "Activate your GWIG account.";
            ActivateUserEmail.Text = "Click this link to activate your account " + ConfigService.SiteBaseUrl +"/public/authentication/" + Token;
            
            // Api Key provided by Sabio
            var transportWeb = new SendGrid.Web("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");

            transportWeb.DeliverAsync(ActivateUserEmail);
        }


        public static void SendProfileEmailForForgotPassword(Guid Token, string Email)
        {

            SendGridMessage ActivateUserEmail = new SendGridMessage();
            ActivateUserEmail.AddTo(Email);
            ActivateUserEmail.From = new MailAddress("GWIGSupport@GWIG.com");
            ActivateUserEmail.Subject = "Change your GWIG password.";
            ActivateUserEmail.Text = "Click this link to change your account password " + ConfigService.SiteBaseUrl + "/public/passwordreset/" + Token;

            // Api Key provided by Sabio
            var transportWeb = new SendGrid.Web("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");

            transportWeb.DeliverAsync(ActivateUserEmail);
        }


    }
}