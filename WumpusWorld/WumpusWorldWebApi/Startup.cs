
using Owin;
using System.Configuration;
using WumpusWorldWebApi.Controllers;

namespace WumpusWorldWebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IdentityConfig.ConfigureIdentity();

            ConfigureAuth(app);
        }
    }

    public partial class Startup
    {
         

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseApplicationSignInCookie();

            app.UseExternalSignInCookie();

            //app.UseMicrosoftAccountAuthentication(
            //      clientId: "",
            //      clientSecret: "");
            var twitterConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
            var twitterconsumerSecret = ConfigurationManager.AppSettings["twitterconsumerSecret"];
            app.UseTwitterAuthentication(
               consumerKey: twitterConsumerKey,
               consumerSecret: twitterconsumerSecret);

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

          

            app.UseGoogleAuthentication();
        }
    }
}