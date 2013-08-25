
using Owin;
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

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

          

            app.UseGoogleAuthentication();
        }
    }
}