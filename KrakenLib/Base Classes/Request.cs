using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace theCodeJerk.apis.Kraken
{
    public interface IRequest
    {
        List<KeyValuePair<string, string>> Arguments { get; }
        string ToString();
    }
    public abstract class Request : IRequest
    {
        public List<KeyValuePair<string, string>> Arguments { get; protected internal set; }

        public override string ToString()
        {
            string retVal = "";
            if (Arguments != null)
            {
                if (Arguments.Count > 0)
                {
                    foreach (KeyValuePair<string, string> argument in Arguments)
                    {
                        if (retVal != "") { retVal += "&"; }
                        retVal += string.Format("{0}={1}", argument.Key, argument.Value);
                    }
                }
            }
            return retVal;
        }
        protected internal Request(List<KeyValuePair<string, string>> args)
        {
            Arguments = args;
        }
        protected internal Request()
        {
            Arguments = null;
        }
    }
}
