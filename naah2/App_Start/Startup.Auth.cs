using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using naah2.Providers;
using naah2.Models;
using Microsoft.Owin.Security.Facebook;
using naah2.Facebook;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace naah2
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //var facebookOptions = new FacebookAuthenticationOptions()
            //{
            //    AppId = "",
            //    AppSecret = "",
            //    //BackchannelHttpHandler = new FacebookBackChannelHandler(),
            //    //UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,email"
            //};


            //facebookOptions.Scope.Add("email");
            //app.UseFacebookAuthentication(facebookOptions);

            
            var facebookOptions = new FacebookAuthenticationOptions()
            {
                AppId = "218412062002086",
                AppSecret = "3be7ddece93ee4f153bdaeb558d2d84d",
                SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                CallbackPath = new PathString("/Manager.aspx/")
            };

            facebookOptions.Scope.Add("email");
            facebookOptions.Scope.Add("public_profile");

       
            facebookOptions.Fields.Add("first_name");
            facebookOptions.Fields.Add("last_name");

            facebookOptions.Provider = new FacebookAuthenticationProvider()
            {
                OnAuthenticated = (context) =>
                {
                    // Attach the access token if you need it later on for calls on behalf of the user
                    context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));

                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("{0}", claim.Key);
                        var claimValue = claim.Value.ToString();

                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, "XmlSchemaString", "Facebook"));
                    }

                    return Task.FromResult(0);
                }
            };

            app.UseFacebookAuthentication(facebookOptions);


            // app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            // {
            //     ClientId = "",
            //     ClientSecret = ""
            // });

            var googleOptions = new GoogleOAuth2AuthenticationOptions()
            {
               ClientId = "420284509839-34s68rvqsbu2f951etalf8s7irqr1fb9.apps.googleusercontent.com",
               ClientSecret = "5zlTK7vvr6qqnrrww0I4AbmM",
               Provider = new GoogleOAuth2AuthenticationProvider()
               {
                   OnAuthenticated = (context) =>
                   {
                       context.Identity.AddClaim(new Claim("urn:google:name", context.Identity.FindFirstValue(ClaimTypes.Name)));
                       context.Identity.AddClaim(new Claim("urn:google:email", context.Identity.FindFirstValue(ClaimTypes.Email)));
                       //This following line is need to retrieve the profile image
                       context.Identity.AddClaim(new System.Security.Claims.Claim("urn:google:accesstoken", context.AccessToken, ClaimValueTypes.String, "Google"));

                       return Task.FromResult(0);
                   }
               }
            };

            app.UseGoogleAuthentication(googleOptions);


        }
    }
}
