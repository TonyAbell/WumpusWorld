
using Owin;

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
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseApplicationSignInCookie();

            app.UseExternalSignInCookie();

            app.UseGoogleAuthentication();
        }
    }
}