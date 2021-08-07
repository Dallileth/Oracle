using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Sandbox.Shared.Models;
using System;

//using System.Timers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Sandbox.Shared.Services
{
    public class RestAPI:IDisposable
    {

        static HttpClient _client;
        public string BaseAddress { get; private set; }

        Auth _auth;
        public Auth Auth
        {
            get
            {
                return _auth;
            }
            set
            {
                if (value?.Parameter != Auth?.Parameter || value?.Scheme!=Auth?.Scheme)
                {
                    _auth = value;
                    _auth_expires_timer.Stop();
                    if (_auth?.Expires != null)
                    {
                        _auth_expires_timer.Interval = (_auth.Expires.Value - DateTime.UtcNow).TotalMilliseconds;
                        _auth_expires_timer.Start();
                    }

                    OnAuthChanged?.Invoke(this, value);
                }

            }
        }
        public void SetAuth(string scheme,string parameter,DateTime?expires=null)
        {
            Auth = new Auth(scheme, parameter, expires);
        }
        public void ClearAuth()
        {
            Auth = null;
        }

        Action<JToken> _error_handler = null;
        public RestAPI(string base_address,bool remove_auth_when_expired=true)
        {
            BaseAddress = base_address;
            _auth_expires_timer= new System.Timers.Timer();
            if (remove_auth_when_expired)
            {
                _auth_expires_timer.Elapsed += (o, e) =>
                {
                    Auth = null;
                };
            }
        }
        public void Dispose()
        {
            _auth_expires_timer.Stop();
            _auth_expires_timer.Dispose();
        }


        static Func<bool> _fast_check_online;
        System.Timers.Timer _auth_expires_timer;
        public static void Init(Func<bool> fast_online_check = null, Func<HttpClient> custom_ctr = null)
        {
            _fast_check_online = fast_online_check;            
            if (custom_ctr != null)
                _client = custom_ctr();
            else
                _client = new HttpClient();            
        }

        /// <summary>
        /// Catch-all fallback handler. Suggested use:
        /// <para>
        /// Any:            Log the result
        /// NotReached:     Serialize the result
        /// </para>
        /// </summary>
        //public static event EventHandler<RestResult> OnAnyResult;
        public event EventHandler<Auth> OnAuthChanged;


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="check_auth">Checks authorization before posting</param>
        /// <param name="timeout"></param>
        /// <param name="cts"></param>
        /// <param name="mock_data"></param>
        /// <param name="is_mocking"></param>
        /// <returns></returns>
        public async Task<T> Do<T>(
            RestCall request,
            bool check_auth = true,
            TimeSpan? timeout = null,
            CancellationTokenSource cts = null,
            T mock_data = default,
            bool is_mocking = false
            )
        {
            void ThrowIfUnauthorized()
            {
                if (check_auth)
                {
                    if (Auth == null)
                        throw new ProblemException("Authorization required");
#if RELEASE
                    if (Auth.Expires != null && Auth.Expires < DateTime.UtcNow)
                    {
                        throw new ProblemException("Authorization expired");
                    }
#endif
                }
            }
            void ThrowIfOffline()
            {

                if (_fast_check_online != null && !_fast_check_online())
                {
                    throw new ProblemException("Device is not connected to the internet");
                }
            }
            Task<T> SendRequest()
            {
                if (is_mocking)
                    return Task.FromResult(mock_data);
                else
                    return request.Call<T>(_client, timeout, cts, _error_handler);
            }

            ThrowIfUnauthorized();
            ThrowIfOffline();

            var result = await SendRequest();
            return result;
        }

        public Task<T> Get<T>(
            string endpoint, object query = null,
            bool check_auth = true,
            TimeSpan? timeout = null,
            CancellationTokenSource cts = null,
            T mock_data = default,
            bool is_mocking = false)
            =>
            Do<T>(
                new RestCall(HTTPMethod.Get, BaseAddress, endpoint, query, null, Auth),
                check_auth,
                timeout, cts, mock_data,
                is_mocking);

        public Task<T> Put<T>(
            string endpoint, object body = null, object query = null,
            bool check_auth = true,
            TimeSpan? timeout = null,
            CancellationTokenSource cts = null,
            T mock_data = default,
            bool is_mocking = false)
            =>
            Do<T>(
                new RestCall(HTTPMethod.Put, BaseAddress, endpoint, query, body, Auth),
                check_auth,
                timeout, cts, mock_data,
                is_mocking);

        public Task<T> Post<T>(
            string endpoint, object body = null, object query = null,
            bool check_auth = true,
            TimeSpan? timeout = null,
            CancellationTokenSource cts = null,
            T mock_data = default,
            bool is_mocking = false)
            =>
            Do<T>(
                new RestCall(HTTPMethod.Post, BaseAddress, endpoint, query, body, Auth),
                check_auth,
                timeout, cts, mock_data,
                is_mocking);
    }

    public class Auth
    {
        [JsonProperty] public string Scheme { get; set; }
        [JsonProperty] public string Parameter { get; set; }
        [JsonProperty] public DateTime? Expires { get; set; }


        public Auth(string scheme, string parameter, DateTime? expires = null)
        {
            Scheme = scheme;
            Parameter = parameter;
            Expires = expires;
        }
        public override string ToString()
        {
            return $"{Scheme} {Parameter}";
        }
    }


}
