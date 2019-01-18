using System.Collections.Generic;

namespace theCodeJerk.apis.Kraken
{
    public class GetOrderBook
    {
        public class Request : Kraken.Request
        {
            public Request(string pair, int count) : base()
            {
                Arguments = new List<KeyValuePair<string, string>>();
                Arguments.Add(new KeyValuePair<string, string>("pair", pair));
                Arguments.Add(new KeyValuePair<string, string>("count", count.ToString()));
            }
        }
        public class Method : Kraken.Method
        {
            public Method(string pair, int count)
            {
                Scope = MethodScope._public;
                ApiMethodName = "Depth";
                Request = new GetOrderBook.Request(pair, count);
            }
        }
        private GetOrderBook() { }
    }
}
