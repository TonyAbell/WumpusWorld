
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
using Microsoft.AspNet.Identity.Owin;


namespace WumpusWorldWebApi.Controllers
{

    public class IdentityStore : IIdentityStore
    {
        public IdentityStore()
        {
            Users = new UserStore();
            Logins = new UserLoginStore();
            Roles = new RoleStore();
            Secrets = new UserSecretStore();
            Tokens = new TokenStore();
            UserClaims = new UserClaimStore();
            UserManagement = new UserManagementStore();
        }

        public IUserLoginStore Logins
        {
            get;
            private set;
        }

        public IRoleStore Roles
        {
            get;
            private set;
        }

        public IUserSecretStore Secrets
        {
            get;
            private set;
        }

        public ITokenStore Tokens
        {
            get;
            private set;
        }

        public IUserClaimStore UserClaims
        {
            get;
            private set;
        }

        public IUserManagementStore UserManagement
        {
            get;
            private set;
        }

        public IUserStore Users
        {
            get;
            private set;
        }

        public async Task<IdentityResult> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return new IdentityResult(true);
        }
        public void Dispose()
        {

        }
    }

    public class UserManagementStore : IUserManagementStore
    {


        public Task<IdentityResult> CreateAsync(IUserManagement info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IUserManagement CreateNewInstance(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IUserManagement> FindAsync(string userId, CancellationToken cancellationToken)
        {
            return new UserManagement() { UserId = userId, DisableSignIn = false, LastSignInTimeUtc = DateTime.UtcNow };
        }

        public async Task<IdentityResult> UpdateAsync(IUserManagement info, CancellationToken cancellationToken)
        {
            return new IdentityResult(true);
        }
    }
    public class UserClaimStore : IUserClaimStore
    {

        public Task<IdentityResult> AddAsync(IUserClaim userClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IUserClaim>> GetUserClaimsAsync(string userId, CancellationToken cancellationToken)
        {
            return new List<UserClaim>();
        }

        public Task<IdentityResult> RemoveAsync(string userId, string claimType, string claimValue, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public class TokenStore : ITokenStore
    {

        public Task<IdentityResult> AddAsync(IToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IToken CreateNewInstance()
        {
            throw new NotImplementedException();
        }

        public Task<IToken> FindAsync(string id, bool onlyIfValid, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RemoveAsync(string token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(IToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleStore : IRoleStore
    {
        public RoleStore()
        {

        }
        public Task<IdentityResult> AddUserToRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateRoleAsync(IRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteRoleAsync(string roleId, bool failIfNonEmpty, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IRole> FindRoleAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IRole> FindRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IRole>> GetRolesForUserAsync(string userId, CancellationToken cancellationToken)
        {
            return new List<Role>();
        }

        public Task<IEnumerable<string>> GetUsersInRoleAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserInRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RoleExistsAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class UserSecretStore : IUserSecretStore
    {

        public Task<IdentityResult> CreateAsync(IUserSecret userSecret, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IUserSecret CreateNewInstance(string userName, string secret)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IUserSecret> FindAsync(string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(string userName, string newSecret, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateAsync(string userName, string loginSecret, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
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

    //public static class IdentityConfig
    //{
    //    //public const string LocalLoginProvider = "Local";

    //    //public static IUserSecretStore Secrets { get; set; }
    //    public static IUserLoginStore Logins { get; set; }
    //    public static IUserStore Users { get; set; }

    //    public static string RoleClaimType { get; set; }
    //    public static string UserNameClaimType { get; set; }
    //    public static string UserIdClaimType { get; set; }
    //    public static string ClaimsIssuer { get; set; }

    //    public static void ConfigureIdentity()
    //    {
    //        Logins = new UserLoginStore();
    //        Users = new UserStore();
    //        //Logins = new EFUserLoginStore<UserLogin>(dbContextCreator);
    //        //Users = new EFUserStore<User>(dbContextCreator);

    //        RoleClaimType = ClaimsIdentity.DefaultRoleClaimType;
    //        UserIdClaimType = "http://schemas.microsoft.com/aspnet/userid";
    //        UserNameClaimType = "http://schemas.microsoft.com/aspnet/username";
    //        ClaimsIssuer = ClaimsIdentity.DefaultIssuer;
    //        AntiForgeryConfig.UniqueClaimTypeIdentifier = IdentityConfig.UserIdClaimType;
    //    }

    //    public static IList<Claim> RemoveUserIdentityClaims(IEnumerable<Claim> claims)
    //    {
    //        List<Claim> filteredClaims = new List<Claim>();
    //        foreach (var c in claims)
    //        {
    //            // Strip out any existing name/nameid claims
    //            if (c.Type != ClaimTypes.Name &&
    //                c.Type != ClaimTypes.NameIdentifier)
    //            {
    //                filteredClaims.Add(c);
    //            }
    //        }
    //        return filteredClaims;
    //    }

    //    public static void AddRoleClaims(IEnumerable<string> roles, IList<Claim> claims)
    //    {
    //        foreach (string role in roles)
    //        {
    //            claims.Add(new Claim(RoleClaimType, role, ClaimsIssuer));
    //        }
    //    }

    //    public static void AddUserIdentityClaims(string userId, string displayName, IList<Claim> claims)
    //    {
    //        claims.Add(new Claim(ClaimTypes.Name, displayName, ClaimsIssuer));
    //        claims.Add(new Claim(UserIdClaimType, userId, ClaimsIssuer));
    //        claims.Add(new Claim(UserNameClaimType, displayName, ClaimsIssuer));
    //    }

    //    //public static void SignIn(HttpContextBase context, IEnumerable<Claim> userClaims, bool isPersistent)
    //    //{

    //    //    context.SignIn(userClaims, ClaimTypes.Name, RoleClaimType, isPersistent);
    //    //}
    //}
    public class UserClaim : IUserClaim
    {


        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }

        public string UserId { get; set; }
    }

    public class Role : IRole
    {

        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class UserManagement : IUserManagement
    {

        public bool DisableSignIn { get; set; }

        public DateTime LastSignInTimeUtc { get; set; }

        public string UserId { get; set; }
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
        public AccountController()
        {
            IdentityManager = new IdentityManager(new IdentityStore());
            AuthenticationManager = new AuthenticationManager(new IdentityAuthenticationOptions(), IdentityManager);
        }

        public AccountController(IdentityManager storeManager, AuthenticationManager authManager)
        {
            IdentityManager = storeManager;
            AuthenticationManager = authManager;
        }

        public IdentityManager IdentityManager { get; private set; }
        public AuthenticationManager AuthenticationManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private Microsoft.Owin.Security.IAuthenticationManager OwinAuthManager
        {
            get
            {
                return HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication;
            }
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl }), AuthenticationManager);
        }

        private bool VerifyExternalIdentity(ClaimsIdentity id, string loginProvider)
        {
            if (id == null)
            {
                return false;
            }
            Claim claim = id.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            return claim != null && claim.Issuer == loginProvider;
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
        {
            ClaimsIdentity id = await AuthenticationManager.GetExternalIdentityAsync(HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication);
            if (!VerifyExternalIdentity(id, loginProvider))
            {
                return View("ExternalLoginFailure");
            }

            // Sign in this external identity if its already linked
            var authManager = AuthenticationManager;
            var tmp2 = HttpContextBaseExtensions.GetOwinContext(HttpContext);
            var tmp3 = tmp2.Authentication;



            var result = await AuthenticationManager.SignInExternalIdentityAsync(HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication, id);
            if (result.Success)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (User.Identity.IsAuthenticated)
            {
                // Try to link if the user is already signed in
                if ((await AuthenticationManager.LinkExternalIdentityAsync(id, User.Identity.GetUserId())).Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    return View("ExternalLoginFailure");
                }
            }
            else
            {
                // Otherwise prompt to create a local user
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = id.Name, LoginProvider = loginProvider });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                if ((await AuthenticationManager.CreateAndSignInExternalUserAsync(HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication, new User(model.UserName))).Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    return View("ExternalLoginFailure");
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication.SignOut(new string[0]);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
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
            return (ActionResult)PartialView("_ExternalLoginsListPartial", new List<AuthenticationDescription>(
                HttpContextBaseExtensions.GetOwinContext(HttpContext).Authentication.GetExternalAuthenticationTypes()
                ));
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            return Task.Run(async () =>
            {
                var linkedAccounts = await new LoginManager(IdentityManager).GetLoginsAsync(User.Identity.GetUserId());
                ViewBag.ShowRemoveButton = linkedAccounts.Count() > 1;
                return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
            }).Result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && IdentityManager != null)
            {
                IdentityManager.Dispose();
                IdentityManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
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
            public ChallengeResult(string provider, string redirectUrl, AuthenticationManager manager)
            {
                LoginProvider = provider;
                RedirectUrl = redirectUrl;
                Manager = manager;
            }

            public string LoginProvider { get; set; }
            public string RedirectUrl { get; set; }
            public AuthenticationManager Manager { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                HttpContextBaseExtensions.GetOwinContext(context.HttpContext).Authentication.Challenge(new AuthenticationProperties { RedirectUrl = RedirectUrl }, LoginProvider);
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        #endregion
    }
}



//namespace Microsoft.AspNet.Identity
//{
//    public static class IdentityExtensions
//    {
//        public static string GetUserName(this IIdentity identity)
//        {
//            return identity.Name;
//        }

//        public static string GetUserId(this IIdentity identity)
//        {
//            ClaimsIdentity ci = identity as ClaimsIdentity;
//            if (ci != null)
//            {
//                return ci.FindFirstValue(WumpusWorldWebApi.Controllers.IdentityConfig.UserIdClaimType);
//            }
//            return String.Empty;
//        }

//        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
//        {
//            Claim claim = identity.FindFirst(claimType);
//            if (claim != null)
//            {
//                return claim.Value;
//            }
//            return null;
//        }
//    }
//}
