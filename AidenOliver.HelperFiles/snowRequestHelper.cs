/*
.SYNOPSIS
    HelperFile for making requests to the ServiceNow REST API ([Incident] table)
.PREREQUISITES
    Install NUGET package RestSharp v108.0.2
    Configure classFile using .Net Core v6.0
.DESCRIPTION

    Creates a callable class called "Incident" inside the "snowRequestHelper" parent class to store incident ticket properties 
    Creates a callable helper method called "PrepareIncidentPayload" which is used in Program.cs as a parameter in the "ExecuteRequest" method
    Creates a callable method called "ExecuteRequest" which either sends a GET, PUT or POST request to the ServiceNow REST API, based on a switch statement

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

        var snowRequestPutJsonBuilder = new snowRequestHelper.Incident();
        snowRequestPutJsonBuilder.WorkNotes = snowNewWorkNotes;

        RestResponse preDeseralisedObjPut = snowRequest.ExecuteRequest(
            snowAuth.restClientUrl, 
            snowAuth.bearerAccessToken, 
            $"api/now/table/incident/{Incident sys_id}", 
            Method.Put, 
            snowRequest.PrepareIncidentPayload(snowRequestPutJsonBuilder));

.OUTPUTS
    Returns a RestSharp.RestResponse class
.NOTES
    Created on: 14.10.2022
    Author:     Aiden Oliver
*/


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;

namespace AidenOliver.HelperFiles
{
    internal class snowRequestHelper
    {

        public class Incident
        {
            public string Category { get; set; }         // Example: Integration
            public string Subcategory { get; set; }      // Example: Server
            public string AssignedTo { get; set; }      // Example: {sys_id}
            public string Description { get; set; }      // Example: This is a test description value
            public string ShortDescription { get; set; }// Example: This is a test short_description value
            public string ContactType { get; set; }     // Example: System Generated
            public string AssignmentGroup { get; set; } // Example: 99999999999999999999999999999 
            public string CalledIentifier { get; set; }        // Example: 99999999999999999999999999
            public string WorkNotes { get; set; }       // Example: This is a test work_notes value
        }

        //public dynamic contentReturn;
        public dynamic PrepareIncidentPayload(Incident incident)
        {
            dynamic jsonObject = new JObject();
            if (incident.Category != String.Empty)
            {
                jsonObject.category = incident.Category;
            }
            if (incident.Subcategory != String.Empty)
            {
                jsonObject.subcategory = incident.Subcategory;
            }
            if (incident.AssignedTo != String.Empty)
            {
                jsonObject.assigned_to = incident.AssignedTo;
            }
            if (incident.Description != String.Empty)
            {
                jsonObject.description = incident.Description;
            }
            if (incident.ShortDescription != String.Empty)
            {
                jsonObject.short_description = incident.ShortDescription;
            }
            jsonObject.u_type = "incident";
            if (incident.ContactType != null)
            {
                jsonObject.contact_type = incident.ContactType;
            }
            if (incident.AssignmentGroup != null)
            {
                jsonObject.assignment_group = incident.AssignmentGroup;
            }
            if (incident.CalledIentifier != null)
            {
                jsonObject.called_id = incident.CalledIentifier;
            }
            if (incident.WorkNotes != null)
            {
                jsonObject.work_notes = incident.WorkNotes;
            }
            return jsonObject;
        }

        public RestResponse? ExecuteRequest(string restClientUrl, string accessToken, string endpoint, Method method, object? body = null)
        {
            // CancellationTokenSource source = new CancellationTokenSource();
            // CancellationToken token = source.Token;
            RestClient restClient = new RestClient(restClientUrl);
            switch (method)
            {
                case Method.Get:
                    var requestGet = new RestRequest(endpoint, method);
                    requestGet.AddHeader("Accept", "application/json");
                    requestGet.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                    var responseGet = restClient.Execute(requestGet) as RestResponse;
                    // RestResponse responseGet = RestPolicy.ExecuteTaskAsyncWithPolicy<RestResponse>(restClient, requestGet, token).Result;
                    if (responseGet.StatusCode == HttpStatusCode.OK)
                    {
                        return responseGet;
                    }
                    break;
                case Method.Put:
                    RestRequest requestPut = new RestRequest(endpoint, method);
                    requestPut.AddHeader("Accept", "application/json");
                    requestPut.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                    requestPut.AddHeader("Content-Type", "application/json");
                    requestPut.AddBody(JsonConvert.SerializeObject(body));
                    var responsePut = restClient.Execute(requestPut) as RestResponse;
                    // RestResponse responsePut = RestPolicy.ExecuteTaskAsyncWithPolicy<RestResponse>(restClient, requestPut, token).Result;
                    if (responsePut.StatusCode == HttpStatusCode.OK)
                    {
                        return responsePut;
                    }
                    break;
                case Method.Post:
                    var requestPost = new RestRequest(endpoint, method);
                    requestPost.AddHeader("Accept", "application/json");
                    requestPost.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                    requestPost.AddHeader("Content-Type", "application/json");
                    requestPost.AddBody(JsonConvert.SerializeObject(body));
                    var responsePost = restClient.Execute(requestPost) as RestResponse;
                    // RestResponse responsePost = RestPolicy.ExecuteTaskAsyncWithPolicy<RestResponse>(restClient, requestPost, token).Result;
                    if (responsePost.StatusCode == HttpStatusCode.OK)
                    {
                        return responsePost;
                    }
                    break;
            }
            return null;

        }
    }
}

