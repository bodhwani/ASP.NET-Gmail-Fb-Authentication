
function getAccessToken() {
    console.log("Entering into getaccesstoken");

    if (location.hash) {
      
        if (location.hash.split('access_token=')) {
            var accessToken = location.hash.split('access_token=')[1].split('&')[0];
            console.log(accessToken);
            if (accessToken) {
                console.log(accessToken);
                isUserRegistered(accessToken);
            }
        }
    }
}
//checks if the user is already registered or not.
function isUserRegistered(accessToken) {
    
    console.log("inside UserRegistered function");
    $.ajax({
        url: '/api/Account/UserInfo',
        method: 'GET',
        headers: {
            'content-type': 'application/JSON',
            'Authorization': 'Bearer ' + accessToken
        },
        success: function (response) {
            if (response.HasRegistered) {
                //localStorage.setItem('accessToken', accessToken);
                //localStora(ge.setItem('email', response.Email);
                console..log("User registered successfully");
                window.location.href = "Manager.aspx";
            }
            else {
                //window.location.href = "Manager.aspx";
                signupExternalUser(accessToken, response.LoginProvider);
            }
        }
    });
}

//creating user for the first time and saving user information intot the database, in AccountController.js
//By rendering call to api - /api/Account/RegisterExternalFb
function signupExternalUser(accessToken, provider) {
    console.log("Entered into signupExternal User function");

    if (provider === "Facebook") {
        $.ajax({
            url: '/api/Account/RegisterExternalFb',
            method: 'POST',
            headers: {
                'content-type': 'application/json',
                'Authorization': 'Bearer ' + accessToken
            },
            success: function () {

                window.location.href = "/api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=self&redirect_uri=http%3a%2f%2flocalhost%3a50462%2fnormal.html&state=GerGr5JlYx4t_KpsK57GFSxVueteyBunu02xJTak5m01";
            }
        });

    }
    else {
        //this code is for gmail provider.
        //If we want to use some other provider also, then we can use else if condition.
        $.ajax({
            url: '/api/Account/RegisterExternal',
            method: 'POST',
            headers: {
                'content-type': 'application/json',
                'Authorization': 'Bearer ' + accessToken
            },
            success: function () {
                
                window.location.href = "/api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=self&redirect_uri=http%3a%2f%2flocalhost%3a50462%2fnormal.html&state=GerGr5JlYx4t_KpsK57GFSxVueteyBunu02xJTak5m01";
                //After this is done successfully, it redirects to normal.html page again 
                //and again getAccessToken is triggered, entering into this file and entering into
                //isUserRegistered function. This time, response.HasRegistered will come true, and
                //User is redirected to home page.

            }
        });
    }

}
