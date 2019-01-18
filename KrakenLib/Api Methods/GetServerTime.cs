namespace theCodeJerk.apis.Kraken
{
    public class GetServerTime
    {
        public class Request : Kraken.Request
        {
            public Request() : base()
            {
            }
        }
        public class Method : Kraken.Method
        {
            public Method()
            {
                Scope = MethodScope._public;
                ApiMethodName = "Time";
                Request = new GetServerTime.Request();
            }
        }
        private GetServerTime() { }
    }
}
