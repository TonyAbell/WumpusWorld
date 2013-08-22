
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.ComponentModel.DataAnnotations;

namespace WumpusWorldWebApi.Controllers
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        public string LoginProvider { get; set; }
    }

    public class AccountController : Controller
    {
        //
        // GET: /Account/

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private async Task SignIn(string userId, IEnumerable<Claim> claims, bool isPersistent)
        {
            //User user = await Users.Find(userId) as User;
            //if (user != null)
            //{
            //    // Replace UserIdentity claims with the application specific claims
            //    IList<Claim> userClaims = IdentityConfig.RemoveUserIdentityClaims(claims);
            //    IdentityConfig.AddUserIdentityClaims(userId, user.UserName, userClaims);
            //    IdentityConfig.AddRoleClaims(await Roles.GetRolesForUser(userId), userClaims);
            //    IdentityConfig.SignIn(HttpContext, userClaims, isPersistent);
            //}
        }

        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl }));
        }

        public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
        {
            // Get the information about the user from the external login provider
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
            //string userId = await Logins.GetUserId(loginProvider, providerKey);
            //if (!String.IsNullOrEmpty(userId))
            //{
            //    await SignIn(providerKey, id.Claims, isPersistent: false);
            //}
            //else
            //{
            //    // No local user for this account
            //    if (User.Identity.IsAuthenticated)
            //    {
            //        // If the current user is logged in, just add the new account
            //        await Logins.Add(new UserLogin(User.Identity.GetUserId(), loginProvider, providerKey));
            //    }
            //    else
            //    {
            //        ViewBag.ReturnUrl = returnUrl;
            //        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = id.Name, LoginProvider = loginProvider });
            //    }
            //}

            return RedirectToLocal(returnUrl);
        }

        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
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
                    // Create a local user and sign in
                    //var user = new User(model.UserName);
                    //if (await Users.Create(user) &&
                    //    await Logins.Add(new UserLogin(user.Id, model.LoginProvider, id.FindFirstValue(ClaimTypes.NameIdentifier))))
                    //{
                    //    await SignIn(user.Id, id.Claims, isPersistent: false);
                    //    return RedirectToLocal(returnUrl);
                    //}
                    //else
                    //{
                    //    return View("ExternalLoginFailure");
                    //}
                }
                catch (DbEntityValidationException e)
                {
                    ModelState.AddModelError("", e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage);
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
