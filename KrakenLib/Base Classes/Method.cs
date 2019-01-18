using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JackLeitch.RateGate;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace theCodeJerk.apis.Kraken
{
    public interface IMethod : IDisposable
    {
        Request Request { get; }
        Response Response { get; }
        void Call();
        string URL();
    }
    public enum MethodScope { _public, _private }
    static class ScopeUtils
    {
        public static string ToString(this MethodScope value)
        {
            switch (value)
            {
                case MethodScope._private:
                    return "private";
                case MethodScope._public:
                    return "public";
                default:
                    return "";
            }
        }
    }
    public abstract class Method : IMethod, IDisposable
    {
        private RateGate _rateGate;
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_rateGate != null)
                        _rateGate.Dispose();
                }

                _rateGate = null;
                disposedValue = true;
            }
        }
        ~Method()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        protected internal MethodScope Scope { get; set; }

        protected internal string ApiMethodName { get; set; }
        public Request Request { get; protected internal set; }
        public Response Response { get; protected internal set; }
        public string URL()
        {
            string retval = string.Format("{0}/{1}/{2}/{3}{4}",
                Credentials.Instance.BaseAddress,
                Credentials.Instance.Version,
                Scope.ToString(),
                ApiMethodName,
                Request.ToString());
            return retval;
        }
        public void Call()
        {
            switch (Scope)
            {
                case MethodScope._private:
                    Response = new Response(QueryPrivate());
                    break;
                case MethodScope._public:
                    Response = new Response(QueryPublic());
                    break;
            }
            Log.Write(this);
        }
        protected internal Method()
        {
            _rateGate = new RateGate(1, TimeSpan.FromSeconds(5));
        }
        #region Helper methods

        private byte[] sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;

                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                return result;
            }
        }

        private byte[] getHash(byte[] keyByte, byte[] messageBytes)
        {
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {

                Byte[] result = hmacsha512.ComputeHash(messageBytes);

                return result;

            }
        }


        #endregion

        protected internal JsonObject QueryPublic()
        {
            string props = Request.ToString();
            string address = string.Format("{0}/{1}/public/{2}", Credentials.Instance.BaseAddress, Credentials.Instance.Version, ApiMethodName);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(address);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";


            if (props != null)
            {

                using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                {
                    writer.Write(props);
                }
            }

            //Make the request
            try
            {
                //Wait for RateGate
                _rateGate.WaitToProceed();

                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (Stream str = webResponse.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return (JsonObject)JsonConvert.Import(sr);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            if (response.StatusCode != HttpStatusCode.InternalServerError)
                            {
                                throw;
                            }
                            return (JsonObject)JsonConvert.Import(sr);
                        }
                    }
                }

            }
        }

        protected internal JsonObject QueryPrivate()
        {
            string props = Request.ToString();
            // generate a 64 bit nonce using a timestamp at tick resolution
            Int64 nonce = DateTime.Now.Ticks;
            props = "nonce=" + nonce + props;

            string path = string.Format("/{0}/private/{1}", Credentials.Instance.Version, ApiMethodName);
            string address = Credentials.Instance.BaseAddress + path;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(address);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.Headers.Add("API-Key", Credentials.Instance.Key);


            byte[] base64DecodedSecred = Convert.FromBase64String(Credentials.Instance.Secret);

            var np = nonce + Convert.ToChar(0) + props;

            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = sha256_hash(np);
            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());

            var signature = getHash(base64DecodedSecred, z);

            webRequest.Headers.Add("API-Sign", Convert.ToBase64String(signature));

            if (props != null)
            {

                using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                {
                    writer.Write(props);
                }
            }

            //Make the request
            try
            {
                //Wait for RateGate
                _rateGate.WaitToProceed();

                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (Stream str = webResponse.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return (JsonObject)JsonConvert.Import(sr);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            if (response.StatusCode != HttpStatusCode.InternalServerError)
                            {
                                throw;
                            }
                            return (JsonObject)JsonConvert.Import(sr);
                        }
                    }
                }

            }
        }


    }
}
