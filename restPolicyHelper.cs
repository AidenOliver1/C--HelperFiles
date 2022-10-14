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
.NOTES
    Created on: 14.10.2022
    Author:     Aiden Oliver
*/

using Polly;
using Polly.Timeout;
using RestSharp;

namespace /*InsertNamespaceHere*/.programRestPolicies
{
    public static class RestPolicy
    {
        public class TimeoutConfiguration
        {
            public int executionTimeoutAsync { get; set; }
        }

        public static async Task<RestResponse<T>> ExecuteTaskAsyncWithPolicy<T>(this RestClient client, RestRequest request, CancellationToken cancellationToken, TimeoutConfiguration timeout)
        {
            var timeoutPolicy = Policy.TimeoutAsync<RestResponse<T>>(timeout.executionTimeoutAsync); 

            var timeoutRetryPolicy = Policy<RestResponse<T>>
                .Handle<TimeoutRejectedException>() 
                .WaitAndRetryAsync(
                    retryCount: 2, 
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)), 
                    onRetryAsync: RetryDelegateAsync
                );

            var restResponsePolicy = Policy
                .HandleResult<RestResponse<T>>(result => result.ResponseStatus != ResponseStatus.Completed)
                .WaitAndRetryAsync(
                    retryCount: 2, 
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)),
                    onRetryAsync: RetryDelegateAsync
                    );

            var finalPolicy = Policy.WrapAsync(restResponsePolicy, timeoutRetryPolicy, timeoutPolicy); 

            var policyResult = await finalPolicy
                .ExecuteAndCaptureAsync(
                    (context, ct) => client.ExecuteAsync<T>(request, ct),
                    contextData: new Dictionary<string, object>
                    {
                        { "ParamToPassToRetryDelegate", "ParamValue" }
                    },
                    cancellationToken
                    );

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                return policyResult.Result;
            }
            else if (policyResult.FinalException != null)
            {
                return new RestResponse<T>
                {
                    Request = request,
                    ErrorException = policyResult.FinalException
                };
            }
            else
            {
                return new RestResponse<T>
                {
                    Request = request,
                    ErrorException = new Exception(policyResult.FinalHandledResult?.ErrorMessage)
                };
            }
        }
        public static async Task RetryDelegateAsync<T>(DelegateResult<T> result, TimeSpan calculatedWaitDuration, int retryCount, Context context)
        {
            var msg = $"The control has fallen into Polly's retry method. This is retry attempt: {retryCount}.\n"; // msg is useful for logging
            if (result is TimeoutRejectedException) msg += $"Operation failed after a timeout.\n";

            if (result.Exception != null)
            {
                msg += $"Handled exception triggered this. Exception message is: {result.Exception.Message}.\n";
            }

            if (result.Result != null)
            {
                msg += "Handled exception didn't trigger this but defined policies did. It relates to HandleResult you have defined in your policy. Check error log to find more details about it.\n";
            }

            msg += $"A retry will be made after waiting {calculatedWaitDuration.TotalMilliseconds} milliseconds.";

            var paramValue = context["ParamToPassToRetryDelegate"].ToString(); 
        }
    }
}
