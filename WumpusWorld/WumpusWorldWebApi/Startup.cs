using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;

using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;
using WumpusWorldWebApi.Controllers;
using WumpusWorldWebApi.EntityStore;
using System.Configuration;
using WumpusWorldWebApi.Models;
using System.Web.Http;


[assembly: OwinStartup(typeof(WumpusWorldWebApi.Startup))]
namespace WumpusWorldWebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            

        }
    }

    public partial class Startup
    {
          
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
                  
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

         

            ////app.UseMicrosoftAccountAuthentication(
            ////      clientId: "",
            ////      clientSecret: "");

            var twitterConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
            var twitterconsumerSecret = ConfigurationManager.AppSettings["twitterconsumerSecret"];
            if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterconsumerSecret))
            {
                app.UseTwitterAuthentication(
                    consumerKey: twitterConsumerKey,
                    consumerSecret: twitterconsumerSecret);
            }
            var facebookAppId = ConfigurationManager.AppSettings["facebookAppId"];
            var facebookAppSecret = ConfigurationManager.AppSettings["facebookAppSecret"];
            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
            {
                app.UseFacebookAuthentication(
                   appId: facebookAppId,
                   appSecret: facebookAppSecret);
            }
            app.UseGoogleAuthentication();
        }
    }
}