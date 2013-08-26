
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Security.Principal;
using System.Text;
using System.Net.Http;
using System.Web.Routing;
using Microsoft.WindowsAzure.Storage.Table;


namespace WumpusWorldWebApi.Controllers
{

    public class UserStore : IUserStore
    {


        public async Task<IdentityResult> CreateAsync(IUser user, CancellationToken cancellationToken)
        {
            Random r = new Random();
            var apiToken = r.Next(0, int.MaxValue).ToString();
            var entity = user as User;
            entity.ApiToken = apiToken;
            var op = Microsoft.WindowsAzure.Storage.Table.TableOperation.Insert(entity);
            var results = Azure.userstoreTable.Execute(op);

            if (results.Result == null)
            {
                return new IdentityResult(false);
            }
            else
            {


                WumpusWorld.ApiToken tokenEntity = new WumpusWorld.ApiToken();
                tokenEntity.PartitionKey = WumpusWorld.ApiToken.PartitionKeyName;
                tokenEntity.RowKey = apiToken;
                
                tokenEntity.UserId = user.Id;
                tokenEntity.IsActive = true;

                var apiTokenOp = Microsoft.WindowsAzure.Storage.Table.TableOperation.InsertOrMerge(tokenEntity);
                var insertTokenResult = Azure.apitokensTable.Execute(apiTokenOp);

                return new IdentityResult(true);
            }


        }

        public async Task<IdentityResult> DeleteAsync(string userId, CancellationToken cancellationToken)
        {
            return new IdentityResult(true);
        }

        public async Task<IUser> FindAsync(string userId, CancellationToken cancellationToken)
        {

            var op = Microsoft.WindowsAzure.Storage.Table.TableOperation.Retrieve<User>("user", userId);
            var results = Azure.userstoreTable.Execute(op);
            if (results.Result == null)
            {
                return null;

            }
            else
            {
                var user = results.Result as User;
                return user;
            }


        }

        public async Task<IUser> FindByNameAsync(string userName, CancellationToken cancellationToken)
        {
            return new User();
        }
    }
    public class UserLoginStore : IUserLoginStore
    {

        public async Task<IdentityResult> AddAsync(IUserLogin login, CancellationToken cancellationToken)
        {


            var entity = login as UserLogin;

            var op = Microsoft.WindowsAzure.Storage.Table.TableOperation.Insert(entity);
            var results = Azure.userloginstoreTable.Execute(op);
            if (results.Result == null)
            {
                return new IdentityResult(false);
            }
            else
            {
                return new IdentityResult(true);
            }


        }

        public IUserLogin CreateNewInstance(string userId, string loginProvider, string providerKey)
        {
            return new UserLogin(userId, loginProvider, providerKey);
        }

        public async Task<IEnumerable<IUserLogin>> GetLoginsAsync(string userId, CancellationToken cancellationToken)
        {
            return new List<UserLogin>();
        }

        public async Task<string> GetProviderKeyAsync(string userId, string loginProvider, CancellationToken cancellationToken)
        {
            return string.Empty;
        }

        public async Task<string> GetUserIdAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {

            var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(providerKey));
            var op = Microsoft.WindowsAzure.Storage.Table.TableOperation.Retrieve<UserLogin>(loginProvider, rowKey);
            var results = Azure.userloginstoreTable.Execute(op);
            if (results.Result == null)
            {
                return string.Empty;
            }
            else
            {
                var userLogin = results.Result as UserLogin;
                return userLogin.UserId;
            }
        }

        public async Task<IdentityResult> RemoveAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return new IdentityResult(true);
        }
    }

    public static class IdentityConfig
    {
        //public const string LocalLoginProvider = "Local";

        //public static IUserSecretStore Secrets { get; set; }
        public static IUserLoginStore Logins { get; set; }
        public static IUserStore Users { get; set; }

        public static string RoleClaimType { get; set; }
        public static string UserNameClaimType { get; set; }
        public static string UserIdClaimType { get; set; }
        public static string ClaimsIssuer { get; set; }

        public static void ConfigureIdentity()
        {
            Logins = new UserLoginStore();
            Users = new UserStore();
            //Logins = new EFUserLoginStore<UserLogin>(dbContextCreator);
            //Users = new EFUserStore<User>(dbContextCreator);

            RoleClaimType = ClaimsIdentity.DefaultRoleClaimType;
            UserIdClaimType = "http://schemas.microsoft.com/aspnet/userid";
            UserNameClaimType = "http://schemas.microsoft.com/aspnet/username";
            ClaimsIssuer = ClaimsIdentity.DefaultIssuer;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = IdentityConfig.UserIdClaimType;
        }

        public static IList<Claim> RemoveUserIdentityClaims(IEnumerable<Claim> claims)
        {
            List<Claim> filteredClaims = new List<Claim>();
            foreach (var c in claims)
            {
                // Strip out any existing name/nameid claims
                if (c.Type != ClaimTypes.Name &&
                    c.Type != ClaimTypes.NameIdentifier)
                {
                    filteredClaims.Add(c);
                }
            }
            return filteredClaims;
        }

        public static void AddRoleClaims(IEnumerable<string> roles, IList<Claim> claims)
        {
            foreach (string role in roles)
            {
                claims.Add(new Claim(RoleClaimType, role, ClaimsIssuer));
            }
        }

        public static void AddUserIdentityClaims(string userId, string displayName, IList<Claim> claims)
        {
            claims.Add(new Claim(ClaimTypes.Name, displayName, ClaimsIssuer));
            claims.Add(new Claim(UserIdClaimType, userId, ClaimsIssuer));
            claims.Add(new Claim(UserNameClaimType, displayName, ClaimsIssuer));
        }

        public static void SignIn(HttpContextBase context, IEnumerable<Claim> userClaims, bool isPersistent)
        {
            context.SignIn(userClaims, ClaimTypes.Name, RoleClaimType, isPersistent);
        }
    }

    public class User : Microsoft.WindowsAzure.Storage.Table.TableEntity, IUser
    {
        public User()
            : this(String.Empty)
        {
        }

        public User(string userName)
        {


            UserName = userName;
            Id = Guid.NewGuid().ToString();
            this.PartitionKey = "user";
            this.RowKey = Id;
        }


        public string Id { get; set; }

        public string UserName { get; set; }

        public string ApiToken { get; set; }
    }
    public class UserLogin : Microsoft.WindowsAzure.Storage.Table.TableEntity, IUserLogin
    {

        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }

        public string UserId { get; set; }

        public UserLogin() { }

        public UserLogin(string userId, string loginProvider, string providerKey)
        {
            this.PartitionKey = loginProvider;
            var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(providerKey));
            this.RowKey = rowKey;

            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
        }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        public string LoginProvider { get; set; }
    }

    [Authorize]
    public class AccountController : Controller
    {
        public IUserLoginStore Logins { get; private set; }
        public IUserStore Users { get; private set; }
        public AccountController()
        {
            Logins = IdentityConfig.Logins;
            Users = IdentityConfig.Users;
        }


        public async Task<ActionResult> Manage()
        {
            var token = new CancellationToken();
            var id = this.User.Identity.GetUserId();
            var user = await Users.FindAsync(id, token) as User;
            ViewBag.ApiToken = user.ApiToken;
            return View();
        }


        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private async Task SignIn(string userId, IEnumerable<Claim> claims, bool isPersistent)
        {
            var token = new CancellationToken();
            User user = await Users.FindAsync(userId, token) as User;
            if (user != null)
            {
                // Replace UserIdentity claims with the application specific claims
                IList<Claim> userClaims = IdentityConfig.RemoveUserIdentityClaims(claims);
                IdentityConfig.AddUserIdentityClaims(userId, user.UserName, userClaims);

                IdentityConfig.SignIn(HttpContext, userClaims, isPersistent);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
        {
            // Get the information about the user from the external login provider
            var token = new CancellationToken();

            ClaimsIdentity id = await HttpContext.GetExternalIdentity();
            if (id == null)
            {
                return View("ExternalLoginFailure");
            }

            // Make sure the external identity is from the loginProvider we expect
            Claim providerKeyClaim = id.FindFirst(ClaimTypes.NameIdentifier);
            if (providerKeyClaim == null || providerKeyClaim.Issuer != loginProvider)
            {
                return View("ExternalLoginFailure");
            }

            // Succeeded so we should be able to lookup the local user name and sign them in
            string providerKey = providerKeyClaim.Value;
            string userId = await Logins.GetUserIdAsync(loginProvider, providerKey, token);
            if (!String.IsNullOrEmpty(userId))
            {
                await SignIn(userId, id.Claims, isPersistent: true);
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = id.Name, LoginProvider = loginProvider });

            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            var token = new CancellationToken();
            if (User.Identity.IsAuthenticated)
            {
                // return RedirectToAction("Manage");
                return RedirectToAction("Home", "Index");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                ClaimsIdentity id = await HttpContext.GetExternalIdentity();
                if (id == null)
                {
                    return View("ExternalLoginFailure");
                }
                try
                {

                    //  Create a local user and sign in
                    var user = new User(model.UserName);
                    var tmp1 = await Users.CreateAsync(user, token);
                    var tmp2 = await Logins.AddAsync(new UserLogin(user.Id, model.LoginProvider, id.FindFirstValue(ClaimTypes.NameIdentifier)), token);

                    if (tmp1.Success && tmp2.Success)
                    {
                        await SignIn(user.Id, id.Claims, isPersistent: true);
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        return View("ExternalLoginFailure");
                    }
                }
                catch (Exception e)
                {
                    return View("ExternalLoginFailure");
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContext.SignOut();
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return (ActionResult)PartialView("_ExternalLoginsListPartial", new List<AuthenticationDescription>(HttpContext.GetExternalAuthenticationTypes()));
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUrl)
            {
                LoginProvider = provider;
                RedirectUrl = redirectUrl;
            }

            public string LoginProvider { get; set; }
            public string RedirectUrl { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.Challenge(LoginProvider, new AuthenticationExtra() { RedirectUrl = RedirectUrl });
            }
        }
    }
}

namespace Microsoft.AspNet.Identity
{
    public static class IdentityExtensions
    {
        public static string GetUserName(this IIdentity identity)
        {
            return identity.Name;
        }

        public static string GetUserId(this IIdentity identity)
        {
            ClaimsIdentity ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue(WumpusWorldWebApi.Controllers.IdentityConfig.UserIdClaimType);
            }
            return String.Empty;
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            Claim claim = identity.FindFirst(claimType);
            if (claim != null)
            {
                return claim.Value;
            }
            return null;
        }
    }
}
