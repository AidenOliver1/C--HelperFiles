/*
.SYNOPSIS
    HelperFile for configuring authentication to an Azure Storage Account
.PREREQUISITES
    Install NUGET package RestSharp v108.0.2
    Configure classFile using .Net Core v6.0
    Configure an Azure Application with access that fits the requirements of the Storage Account requests
.DESCRIPTION

    Creates callable public properties of the "azureAuthenticationHelper" class for use in the next methods
    Creates a callable method called "getAzureAuthenticationToken" which sends a POST request to the "{azAppId}/oauth2/token" endpoint of the Azure Application's API to return an OAuth2.0 AccessToken
    Creates a callable method called "azConnectionStringBuilder" which sends a POST request to the Azure Management API to generate an Azure Storage Account Key and then it builds the Azure Storage Account Authentication String.

.EXAMPLE
    Call from Controller / Program using the following syntax:
        var azureAuthenticationClass = new azureAuthenticationHelper();
        azureAuthenticationClass.restClientUrl = azureRestClientUrl;
        azureAuthenticationClass.clientId = azureClientId;
        azureAuthenticationClass.clientSecret = azureClientSecret;
        azureAuthenticationClass.azAppId = azureAppId;
        azureAuthenticationClass.azResourceUrl = azureResourceUrl;
        azureAuthenticationClass.bearerAccessToken = azureAuthenticationClass.getAzAuthenticationToken(azureAuthenticationClass);


        azureAuthenticationClass.azSubscriptionId = azureSubscriptionUrl;
        azureAuthenticationClass.azResourceGroupName = azureResourceGroupName;
        azureAuthenticationClass.azStorageAccName = azureStorageAccountName;
        azureAuthenticationClass.azStorageAccountKeyIndex = 0; // 0 or 1

        azureAuthenticationClass.azConnectionStringBuilder(azureAuthenticationClass.bearerAccessToken);

.OUTPUTS
    Returns an Azure Connection String used for authentication to the Storage Account
.NOTES
    Created on: 14.10.2022
    Author:     Aiden Oliver
*/

using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace AidenOliver.HelperFiles
{
    public class azureAuthenticationHelper
    {
        public string restClientUrl { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string azAppId { get; set; }
        public string bearerAccessToken { get; set; }
        public string azResourceUrl { get; set; }
        public string azSubscriptionId { get; set; }
        public string azResourceGroupName { get; set; }
        public string azStorageAccName { get; set; }
        public int azStorageAccountKeyIndex { get; set; }

        public string getAzureTokenFromClientCredentials(azureAuthenticationHelper classFile)
        {
            RestClient restClient = new RestClient(classFile.restClientUrl);
            var request = new RestRequest($"{classFile.azAppId}/oauth2/token", Method.Post);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("Accept", "application/json");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", classFile.clientId);
            request.AddParameter("client_secret", classFile.clientSecret);
            request.AddParameter("resource", classFile.azResourceUrl);
            var response = restClient.Execute(request) as RestResponse;
            var objIncident = JsonConvert.DeserializeObject<dynamic>(response.Content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Log Success
                return objIncident.access_token;
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Log Warning
            }
            // Log Error
            return null;
        }

        public async Task<string> GetAzureTokenFromClientCredentialsAsync(azureAuthenticationHelper classFile)
        {
            RestClient restClient = new RestClient(classFile.restClientUrl);
            var request = new RestRequest($"{classFile.azAppId}/oauth2/token", Method.Post);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("Accept", "application/json");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", classFile.clientId);
            request.AddParameter("client_secret", classFile.clientSecret);
            request.AddParameter("resource", classFile.azResourceUrl);

            var response = await restClient.ExecuteAsync(request) as RestResponse;
            var objIncident = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Log Success
                return objIncident.access_token;
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Log Warning
            }
            // Log Error
            return null;
        }

        public string azConnectionStringBuilder(azureAuthenticationHelper classFile)
        {
            var azKeyClient = new RestClient("https://management.azure.com/");
            var azKeyRequest = new RestRequest($"subscriptions/{classFile.azSubscriptionId}/resourceGroups/{classFile.azResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{classFile.azStorageAccName}/listKeys?api-version=2016-01-01", Method.Post);
            azKeyRequest.AddHeader("Authorization", string.Format("Bearer {0}", classFile.bearerAccessToken));
            azKeyRequest.AddHeader("Accept", "application/json");
            var azKeyResponse = azKeyClient.Execute(azKeyRequest) as RestResponse;
            if (azKeyResponse.StatusCode == HttpStatusCode.OK)
            {
                // Log Success
                var deseralisedJson = JsonConvert.DeserializeObject<dynamic>(azKeyResponse.Content);
                var azConnectionKeyString = $"DefaultEndpointsProtocol=https;AccountName={classFile.azStorageAccName};AccountKey={deseralisedJson.keys[azStorageAccountKeyIndex].value.ToString()};EndpointSuffix=core.windows.net";
                return azConnectionKeyString;
            }
            if (azKeyResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Log Warning
                return null;
            }
            // Log Error
            return null;
        }

        public async Task<string> azConnectionStringBuilderAsync(azureAuthenticationHelper classFile)
        {
            var azKeyClient = new RestClient("https://management.azure.com/");
            var azKeyRequest = new RestRequest($"subscriptions/{classFile.azSubscriptionId}/resourceGroups/{classFile.azResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{classFile.azStorageAccName}/listKeys?api-version=2016-01-01", Method.Post);
            azKeyRequest.AddHeader("Authorization", string.Format("Bearer {0}", classFile.bearerAccessToken));
            azKeyRequest.AddHeader("Accept", "application/json");

            var azKeyResponse = await azKeyClient.ExecuteAsync(azKeyRequest) as RestResponse;

            if (azKeyResponse.StatusCode == HttpStatusCode.OK)
            {
                // Log Success
                var deserializedJson = JsonConvert.DeserializeObject<dynamic>(azKeyResponse.Content);
                var azConnectionKeyString = $"DefaultEndpointsProtocol=https;AccountName={classFile.azStorageAccName};AccountKey={deserializedJson.keys[azStorageAccountKeyIndex].value.ToString()};EndpointSuffix=core.windows.net";
                return azConnectionKeyString;
            }
            if (azKeyResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Log Warning
                return null;
            }
            // Log Error
            return null;
        }

    }
}
