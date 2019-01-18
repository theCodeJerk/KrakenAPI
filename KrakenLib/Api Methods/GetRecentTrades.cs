using System.Collections.Generic;

namespace theCodeJerk.apis.Kraken
{
    public class GetRecentTrades
    {
        public class Request : Kraken.Request
        {
            public Request(string pair, long since) : base()
            {
                Arguments = new List<KeyValuePair<string, string>>();
                Arguments.Add(new KeyValuePair<string, string>("pair", pair));
                Arguments.Add(new KeyValuePair<string, string>("since", since.ToString()));
            }
        }
        public class Method : Kraken.Method
        {
            public Method(string pair, long since)
            {
                Scope = MethodScope._public;
                ApiMethodName = "Trades";
                Request = new GetRecentTrades.Request(pair, since);
            }
        }
        private GetRecentTrades() { }
    }
}
