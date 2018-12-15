using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServerAngular.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAngular.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        //private readonly IUsersService _usersService;
        private readonly ILogger<AccountController> _logger;
        //private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;


        public AccountController(ILogger<AccountController> logger,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IClientStore clientStore,
            //ITwoFactorAuthenticationService twoFactorAuthenticationService,
            //IUsersService usersService,
            IEventService events)
        {
            _logger = logger;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _clientStore = clientStore;
            //_twoFactorAuthenticationService = twoFactorAuthenticationService;
            //_usersService = usersService;
            _events = events;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return RedirectToRoute("../ClientApp/src/index.html", returnUrl);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginInputModel model, string button)
        //{
        //    // check if we are in the context of an authorization request
        //    var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

        //    //the user clicked the "cancel" button
        //    if (button != "login")
        //    {
        //        if (context != null)
        //        {
        //            // if the user cancels, send a result back into IdentityServer as if they 
        //            // denied the consent (even if this client does not require consent).
        //            // this will send back an access denied OIDC error response to the client.
        //            await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

        //            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
        //            //if (await _clientStore.IsPkceClientAsync(context.ClientId))
        //            //{
        //            //    // if the client is PKCE then we assume it's native, so this change in how to
        //            //    // return the response is for better UX for the end user.
        //            //    return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
        //            //}

        //            return Redirect(model.ReturnUrl);
        //        }
        //        else
        //        {
        //            // since we don't have a valid context, then we just go back to the home page
        //            return Redirect("~/");
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        if (await _usersService.AreUserCredentialsValidAsync(model.Username, model.Password))
        //        {
        //            var user = await _usersService.GetUserByUsernameAsync(model.Username);

        //            // 2-F.A
        //            var id = new ClaimsIdentity();
        //            id.AddClaim(new Claim(JwtClaimTypes.Subject, user.SubjectId));
        //            await HttpContext.SignInAsync(scheme: Startup.TwoFactorAuthenticationScheme,
        //                principal: new ClaimsPrincipal(id));

        //            await _twoFactorAuthenticationService.SendTemporaryCodeAsync(user.SubjectId);

        //            var redirectToAdditionalFactorUrl =
        //                Url.Action("AdditionalAuthenticationFactor",
        //                    new
        //                    {
        //                        returnUrl = model.ReturnUrl,
        //                        rememberLogin = model.RememberLogin
        //                    });

        //            // request for a local page
        //            if (Url.IsLocalUrl(model.ReturnUrl))
        //            {
        //                //return Redirect(model.ReturnUrl);
        //                return Redirect(redirectToAdditionalFactorUrl);
        //            }

        //            if (string.IsNullOrEmpty(model.ReturnUrl))
        //            {
        //                return Redirect("~/");
        //            }

        //            // user might have clicked on a malicious link - should be logged
        //            throw new Exception("invalid return URL");
        //        }

        //        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
        //        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
        //    }

        //    // something went wrong, show form with error
        //    //var vm = await BuildLoginViewModelAsync(model);
        //    //return View(vm);
        //    return View();
        //}


        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                _logger.LogInformation("this is meant to short circuit the UI and only trigger the one external IdP");
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context.LoginHint,
                    ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }

            var schemes = (await _schemeProvider.GetAllSchemesAsync()).ToList();
            foreach (var scheme in schemes)
            {
                _logger.LogInformation(
                    $"ExternalProvider Scheme -> {scheme.Name}:{scheme.DisplayName}:{scheme.HandlerType}");
            }

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName,
                                StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider =>
                            client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }
    }
}
