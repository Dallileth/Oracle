using System;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using Sandbox.Shared.Models;

namespace Sandbox.Shared.Services
{
    public enum HTTPMethod { Get, Put, Post, Delete, Head, Options, Trace };
    [JsonObject(MemberSerialization.OptOut)]
    public struct RestCall
    {

        //Request
        public object Query { get; private set; }
        public object Body { get; private set; }
        //public Encoding Encoding { get; private set; } //if null, use utf8 //serializable?

        //Endpoint
        public HTTPMethod HTTPMethod { get; private set; }
        public string BaseAddress { get; private set; }
        public string EndPoint { get; private set; }

        //Response
        //public HttpStatusCode? StatusCode;
        //public JToken Json;
        public Auth Auth { get; private set; }

        public RestCall(HTTPMethod http_method, string baseaddress, string endpoint, object query, object body, Auth auth)//, Encoding encoding = null)
        {
            HTTPMethod = http_method;
            BaseAddress = baseaddress;
            EndPoint = endpoint;

            Query = query;
            Body = body;

            Auth = auth;

            //Encoding = encoding;
        }

        /// <summary>
        /// Returns a URI containing $"{BaseAddress}/{EndPoint}?{Query}"
        /// </summary>
        /// <returns></returns>
        string CreateURI()
        {
            string query = Query == null ? null : JsonConvert.SerializeObject(Query);
            string uri = $"{BaseAddress}/{EndPoint}";
            if (!string.IsNullOrWhiteSpace(query))
            {
                var jquery = JToken.Parse(query);// JObject.FromObject(Query);
                var squery = String.Join("&", jquery.Children().Cast<JProperty>().Select(jp => $"{jp.Name}={WebUtility.UrlEncode(jp.Value.ToString())}"));
                uri = $"{uri}?{squery}";
            }
            return uri;
        }
        StringContent CreateContent(out double upkb)
        {
            upkb = 0;
            string body = Body == null ? null : JsonConvert.SerializeObject(Body);
            StringContent content = null;
            if (!string.IsNullOrWhiteSpace(body))
            {
                string stringified = body;//JsonConvert.SerializeObject(Body, Formatting.Indented);
                upkb = Encoding.UTF8.GetByteCount(stringified);
                content = new StringContent(stringified, /*Encoding ??*/ Encoding.UTF8, "application/json");
            }
            return content;
        }


        public async Task<T> Call<T>(HttpClient client, TimeSpan? timeout = null, CancellationTokenSource cts = null, Action<JToken> error_handler = null)
        {
            try
            {
                double upkb = 0;
                double downkb = 0;
                DateTime started = DateTime.Now;

                string uri = CreateURI();
                StringContent content = CreateContent(out upkb);

                timeout = timeout ?? TimeSpan.FromSeconds(10);


                using var request = new HttpRequestMessage(
                    HTTPMethod switch
                    {
                        HTTPMethod.Get => HttpMethod.Get,
                        HTTPMethod.Post => HttpMethod.Post,
                        HTTPMethod.Put => HttpMethod.Put,
                        _ => throw new NotImplementedException($"{HTTPMethod} not supported")
                    }
                    , uri);


                using var timeoutcts = new CancellationTokenSource();
                CancellationTokenSource compositeTokenSource = null;
                if (cts != null)
                    compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutcts.Token, cts.Token);
                else
                    compositeTokenSource = timeoutcts;



                request.Content = content;
                if (!string.IsNullOrWhiteSpace(Auth?.Scheme) && !string.IsNullOrWhiteSpace(Auth?.Parameter))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(Auth.Scheme, Auth.Parameter);
                }



                timeoutcts.CancelAfter(timeout.Value);
                using HttpResponseMessage response = await client.SendAsync(request, compositeTokenSource.Token).ConfigureAwait(false);
                string actual_response = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                //todo: cancel the... timeoutcts
                downkb = Encoding.UTF8.GetByteCount(actual_response);
                if (typeof(T)==typeof(HttpStatusCode))
                {
                    return (T)Convert.ChangeType(response.StatusCode,typeof(T));
                }
                if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299)
                {

                    if (typeof(T) == typeof(object))
                        return default;
                    else if (typeof(T) == typeof(string))
                    {
                            return JsonConvert.DeserializeObject<T>($"'{actual_response}'");

                    }
                    else
                        return JsonConvert.DeserializeObject<T>(actual_response);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ProblemException("Service not found");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new ProblemException("Forbidden");
                }
                else if (string.IsNullOrWhiteSpace(actual_response))
                {
                    throw new ProblemException(response.StatusCode.ToString());
                }
                else
                {
                    var jtoken = JToken.Parse(actual_response);
                    throw new ProblemException((string)jtoken["detail"],(string)jtoken["title"]);
                }


            }

            catch (OperationCanceledException operationcancelled)
            {
                throw new ProblemException("Could not reach the server");
            }

        }
    }
}
