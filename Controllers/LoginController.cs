using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View("Login");
        }
        
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "index",
            };
            
            return new ChallengeResult(provider, props);
        }
    }
}
