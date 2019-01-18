namespace theCodeJerk.apis.Kraken
{
    public class GetActiveAssets
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
                ApiMethodName = "Assets";
                Request = new GetActiveAssets.Request();
            }
        }
        private GetActiveAssets() { }
    }
}
