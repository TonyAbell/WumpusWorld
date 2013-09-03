
using Owin;
using System.Configuration;
using WumpusWorldWebApi.Controllers;

namespace WumpusWorldWebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //IdentityConfig.ConfigureIdentity();

            ConfigureAuth(app);
        }
    }

    public partial class Startup
    {


        public void ConfigureAuth(IAppBuilder app)
        {
            //app.UseApplicationSignInCookie();

            app.UseSignInCookies();

            //app.UseMicrosoftAccountAuthentication(
            //      clientId: "",
            //      clientSecret: "");

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