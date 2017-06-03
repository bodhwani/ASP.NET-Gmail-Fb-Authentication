### Startup.Auth.cs :
Configure your app in <a href="http://console.developers.google.com/">Google</a> and <a href="http://developers.facebook.com/">Facebook</a> developers console.
<br>

### Start.Auth.cs
- Register your app with facebook and gmail, by configuring AppId, AppSecret and ClientId, ClientSecret respectively.

### Redirect Url :
- Inside GoogleAuthentication.js file :
Change redirect_uri=http%3a%2f%2flocalhost%3a50462%2fnormal.html to redirect_uri=http%3a%2f%2f[xyz] <br>
Change url(line no. 78) from normal.html to xyz.<br>

Note : This should be same as url(at window.location.href), given in signupExternalUser function in GoogleAuthentication.js file.
### How things are working? 
- Goes to normal.html
- GoogleAuthentication.js
- Call ajax request accoring to the code
- AccountController.js [GET request to  api/Account/UserInfo]
- If user already exisits, redirected to Manager.aspx
- Else, save entries into database and then redirected. 