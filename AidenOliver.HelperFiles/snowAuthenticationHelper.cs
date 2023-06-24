/*
.SYNOPSIS
    HelperFile for setting up authentication to the ServiceNow REST API ([Incident] table) and returning an OAuth2.0 AccessToken
.PREREQUISITES
    Install NUGET package RestSharp v108.0.2
.DESCRIPTION

    Creates callable public properties of the "snowAuthenticationHelper" class for use in the next method 
    Creates a callable method called "getSnowAuthenticationToken" which sends a POST request to the "oauth_token.do" endpoint of the ServiceNow API

.EXAMPLE
    Call from Controller / Program using the following syntax:
        var snowAuth = new snowAuthenticationHelper();
        var snowRequest = new snowRequestHelper();
        snowAuth.restClientUrl = snowRestClientUrl;
        snowAuth.clientId = snowClientId;
        snowAuth.clientSecret = snowClientSecret;
        snowAuth.username = snowIntegratedUsername;
        snowAuth.password = snowIntegratedPassword;
        snowAuth.bearerAccessToken = snowAuth.getSnowAuthenticationToken();
.OUTPUTS
    Returns the ServiceNow OAuth2.0 AccessToken as a string
.NOTESs
    Created on: 14.10.2022
    Author:     Aiden Oliver
*/

using Newtonsoft.Json;
using RestSharp;


namespace AidenOliver.HelperFiles
{
    internal class snowAuthenticationHelper
    {
        public string restClientUrl { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string bearerAccessToken { get; set; }

        public string getSnowAuthenticationToken(snowAuthenticationHelper HelperClass)
        {
            RestClient restClient = new RestClient(HelperClass.restClientUrl);
            var request = new RestRequest("oauth_token.do", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Accept", "application/json");
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", HelperClass.clientId);
            request.AddParameter("client_secret", HelperClass.clientSecret);
            request.AddParameter("username", HelperClass.username);
            request.AddParameter("password", HelperClass.password);
            try
            {
                var response = restClient.Execute(request) as RestResponse;
                var objIncident = JsonConvert.DeserializeObject<dynamic>(response.Content);
                // Log success
                return objIncident.access_token.ToString();
            }
            catch
            {
                // Log exception
                return null;
            }
        }
    }
}
