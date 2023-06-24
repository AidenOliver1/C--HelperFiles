# C#-HelperFiles

This is a public repository which stores C# Helper Files developed using DotNet Core. This repository has a GitHub Action configured to automatically publish this solution to the Nuget Package when a pull request is made to the Main branch.

Nuget File: https://www.nuget.org/packages/AidenOliver.HelperFiles/

Release_v1.0.0:
- New:
  - azureAuthenticationHelper:
    - public string getAzureTokenFromClientCredentials(azureAuthenticationHelper classFile)
    - public async Task<string> GetAzureTokenFromClientCredentialsAsync(azureAuthenticationHelper classFile)
    - public string azConnectionStringBuilder(azureAuthenticationHelper classFile)
    - public async Task<string> azConnectionStringBuilderAsync(azureAuthenticationHelper classFile)
  - azureStorageHelper:
    - public async Task<string> downloadFromBlobAsync(string connectionString, string fileDirectory, string azureContainerName)
    - public void UploadFileToStorage(string fileDirectory, string fileName, string containerName, bool bOverWrite = true)
    - public async Task UploadFileToStorageAsync(string fileDirectory, string fileName, string containerName, bool bOverWrite = true)
  - snowAuthenticationHelper:
    - public string getSnowAuthenticationToken(snowAuthenticationHelper HelperClass)
  - snowRequestHelper:
    - public dynamic PrepareIncidentPayload(Incident incident)
    - public RestResponse? ExecuteRequest(string restClientUrl, string accessToken, string endpoint, Method method, object? body = null)

