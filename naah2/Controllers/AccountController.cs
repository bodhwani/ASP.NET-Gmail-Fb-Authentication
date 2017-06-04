using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using naah2.Models;
using naah2.Providers;
using naah2.Results;

using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;
using DotNetOpenAuth.OpenId;
using Facebook;
using System.IO;

namespace naah2.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            //StreamWriter File = new StreamWriter("C://Users//Amit//Documents//Visual Studio 2017//Projects//naah2folder//naah2//logFile1.txt");
            //File.Write("This is second time");
            //File.Close();

            using (StreamWriter w = File.AppendText("C://Users//Amit//Documents//Visual Studio 2017//Projects//naah2folder//naah2//log.txt"))
            {
                w.Write("This is the second message to be printed\r\n");

            }

            //saving details as User.Identity
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            System.Diagnostics.Debug.WriteLine("userIdentity is " + User.Identity);
            //System.Diagnostics.Debug.WriteLine("userIdentity is " + User.Identity.GetUserId());
            //System.Diagnostics.Debug.WriteLine("userIdentity is " + User.Identity.GetUserName());
            // No need to save data into database that why it is commented out
            // I wrote this just to show how things are working and for debugging.
            //try
            //{
            //    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RegisterConnectionString"].ConnectionString);

            //    conn.Open();
            //    string insertQuery = "INSERT INTO UserData(id,email,name,picture,timestamp) VALUES(@ID,@EMAIL,@NAME,@PICTURE,@TIMESTAMP)";

            //    SqlCommand com = new SqlCommand(insertQuery, conn);
            //    com.Parameters.AddWithValue("@ID", newGuid);
            //    com.Parameters.AddWithValue("@EMAIL", User.Identity.GetUserName());
            //    com.Parameters.AddWithValue("@NAME", User.Identity.Name);
            //    com.Parameters.AddWithValue("@PICTURE", "https://lh3.googleusercontent.com/-XdUIqdMkCWA/AAAAAAAAAAI/AAAAAAAAAAA/4252rscbv5M/photo.jpg");
            //    com.Parameters.AddWithValue("@TIMESTAMP", "03-Jan-00 01:00:00 AM");


            //    com.ExecuteNonQuery();

            //    conn.Close();
            //    System.Diagnostics.Debug.WriteLine("Connection closed");


            //}

            //catch (Exception)
            //{

            //}




            return new UserInfoViewModel
            {
                Email = User.Identity.Name,
               
                HasRegistered = externalLogin == null,

                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }


        //Facebook Authetication :
        // POST api/Account/RegisterExternalFb
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternalFb")]
        public async Task<IHttpActionResult> RegisterExternalFb()
        {
            Guid newGuid = Guid.NewGuid();
            //defining variables
            var userPicture = "http://";
            var userName = "temp";
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var info = await Authentication.GetExternalLoginInfoAsync();
            var identity = Authentication.GetExternalIdentity(DefaultAuthenticationTypes.ExternalCookie);
            var accessToken = identity.FindFirstValue("FacebookAccessToken");
            var fb = new FacebookClient(accessToken);
            dynamic myInfo = fb.Get("/me?fields=email");
            info.Email = myInfo.email;
          
            if (info == null)
            {
                return InternalServerError();
            }

            //get access token to use in profile image request
            Uri apiRequestUri = new Uri("https://graph.facebook.com/me?access_token=" + accessToken);

            //request profile image
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(apiRequestUri);
                dynamic result1 = JsonConvert.DeserializeObject(json);
                var id = result1.id;
                var result2 = JsonConvert.DeserializeObject(json);
                userPicture = "http://graph.facebook.com/" + id + "/picture?type=square";
                userName = result1.name;

                System.Diagnostics.Debug.WriteLine("entering into api/register ");
                System.Diagnostics.Debug.WriteLine("this is info.id" + newGuid);
                System.Diagnostics.Debug.WriteLine("this is info.email" + info.Email);
                System.Diagnostics.Debug.WriteLine("this is info.name" + userName);
                System.Diagnostics.Debug.WriteLine("this is info.picture" + userPicture);
                System.Diagnostics.Debug.WriteLine("this is info.time" + unixTimestamp);

                //saving data into database
                try
                {
                    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RegisterConnectionString"].ConnectionString);

                    conn.Open();
                    string insertQuery = "INSERT INTO UserData(id,email,name,picture,timestamp) VALUES(@ID,@EMAIL,@NAME,@PICTURE,@TIMESTAMP)";

                    SqlCommand com = new SqlCommand(insertQuery, conn);
                    com.Parameters.AddWithValue("@ID", newGuid);
                    com.Parameters.AddWithValue("@EMAIL", info.Email);
                    com.Parameters.AddWithValue("@NAME", userName);
                    com.Parameters.AddWithValue("@PICTURE", userPicture);
                    com.Parameters.AddWithValue("@TIMESTAMP", "04-Jan-00 01:00:00 AM");


                    com.ExecuteNonQuery();

                    conn.Close();
                }

                catch (Exception)
                {

                }
            }

            var user = new ApplicationUser() { UserName = userName, Email = info.Email };

            
            IdentityResult result = await UserManager.CreateAsync(user);
         
            System.Diagnostics.Debug.WriteLine("this is result.Succeeded " + result.Succeeded); 




            if (!result.Succeeded)
            {

                var exceptionText = result.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error + "\n\r"));
                System.Diagnostics.Debug.WriteLine("Exception is " + exceptionText);


                System.Diagnostics.Debug.WriteLine("Result is not succeeded-1");

                return GetErrorResult(result);


            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            System.Diagnostics.Debug.WriteLine("this is again result" + result);

            if (!result.Succeeded)
            {
                System.Diagnostics.Debug.WriteLine("Result is not succeeded-2");

                return GetErrorResult(result);
            }
            return Ok();
            //return Redirect("http://www.google.com");
        }




        //----------------------------

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal()
        {
            Guid newGuid = Guid.NewGuid();
            var userPicture = "http://";
            var userName = "Not asigning";
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


            var info = await Authentication.GetExternalLoginInfoAsync();

            System.Diagnostics.Debug.WriteLine("-----This is the info" + info);





            if (info == null)
            {
                return InternalServerError();
            }


            //---------------------
            //get access token to use in profile image request
            var accessToken = info.ExternalIdentity.Claims.Where(c => c.Type.Equals("urn:google:accesstoken")).Select(c => c.Value).FirstOrDefault();
            Uri apiRequestUri = new Uri("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + accessToken);
            System.Diagnostics.Debug.WriteLine("-----------Here is the apiRequestUri --------" + apiRequestUri);
            //var givenNameClaim = externalIdentity.Result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);

            //request profile image
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(apiRequestUri);
                dynamic result1 = JsonConvert.DeserializeObject(json);
                var result2 = JsonConvert.DeserializeObject(json);
                userPicture = result1.picture;
                userName = result1.name;


                System.Diagnostics.Debug.WriteLine("-----------Here is the json --------" + json);
                System.Diagnostics.Debug.WriteLine("-----------Here is the result2 --------" + result2);

                System.Diagnostics.Debug.WriteLine("-----------Here is the userPicture --------" + userPicture);
                System.Diagnostics.Debug.WriteLine("-----------Here is the userPicture --------" + userName);




                //---------------------


                System.Diagnostics.Debug.WriteLine("@@@@@@@@----@@@@@@@@Output please");

                System.Diagnostics.Debug.WriteLine("entering into api/register ");
                System.Diagnostics.Debug.WriteLine("this is info.id" + newGuid);
                System.Diagnostics.Debug.WriteLine("this is info.email" + info.Email);
                System.Diagnostics.Debug.WriteLine("this is info.name" + userName); 
                System.Diagnostics.Debug.WriteLine("this is info.picture" + userPicture);
                System.Diagnostics.Debug.WriteLine("this is info.time" + unixTimestamp);


                try
                {
                    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RegisterConnectionString"].ConnectionString);

                    conn.Open();
                    string insertQuery = "INSERT INTO UserData(id,email,name,picture,timestamp) VALUES(@ID,@EMAIL,@NAME,@PICTURE,@TIMESTAMP)";

                    SqlCommand com = new SqlCommand(insertQuery, conn);
                    com.Parameters.AddWithValue("@ID", newGuid);
                    com.Parameters.AddWithValue("@EMAIL", info.Email);
                    com.Parameters.AddWithValue("@NAME", userName);
                    com.Parameters.AddWithValue("@PICTURE", userPicture);
                    com.Parameters.AddWithValue("@TIMESTAMP", "30-Jan-00 08:00:00 PM");


                    com.ExecuteNonQuery();

                    conn.Close();


                }

                catch (Exception)
                {

                }



            }

            var user = new ApplicationUser() { UserName = userName , Email = info.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            

            if (!result.Succeeded)
            {

                var exceptionText = result.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error + "\n\r"));
                System.Diagnostics.Debug.WriteLine("Exception is " + exceptionText);


                System.Diagnostics.Debug.WriteLine("Result is not succeeded-1");

                return GetErrorResult(result);


            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {

                var exceptionText = result.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error + "\n\r"));
                System.Diagnostics.Debug.WriteLine("Exception is " + exceptionText);


                System.Diagnostics.Debug.WriteLine("Result is not succeeded-2");

                return GetErrorResult(result); 
            }
            return Ok();
            //return Redirect("http://www.google.com");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
