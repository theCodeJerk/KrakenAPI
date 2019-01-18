using System;
using System.Collections.Generic;
using System.Linq;
using Jayrock.Json;

namespace theCodeJerk.apis.Kraken
{
    public enum ResultType
    {
        error,
        exception,
        content
    }
    static class ResultTypeUtils
    {
        public static bool Success(this ResultType value)
        {
            return (value == ResultType.content);
        }
        public static bool Failed(this ResultType value)
        {
            return (value != ResultType.content);
        }
    }
    public interface IResponse
    {
        ResultType Result { get; }
        List<string> Errors { get; }
        Exception Exception { get; }
        JsonObject Content { get; }
        string ToString();
    }
    public class Response : IResponse
    {
        public ResultType Result { get; protected internal set; }
        public List<string> Errors { get; protected internal set; }
        private Exception exception;
        public Exception Exception
        {
            get
            {
                return exception;
            }
            protected internal set
            {
                exception = value;
                Result = ResultType.exception;
            }
        }
        public JsonObject Content { get; protected internal set; }
        protected internal Response(JsonObject json)
        {
            try
            {
                JsonArray error = (JsonArray)json["error"];
                if (error.Count() > 0)
                {
                    Result = ResultType.error;
                    List<string> errors = new List<string>();
                    foreach (var item in error)
                    {
                        errors.Add(item.ToString());
                    }
                    Errors = errors;
                }
                else
                {
                    Result = ResultType.content;
                    Content = (JsonObject)json["result"];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Parse Response.", ex);
            }
        }
        protected internal Response(Exception ex)
        {
            Exception = ex;
        }

        public override string ToString()
        {
            string retval = "";
            switch (Result) {
                case ResultType.content:
                    retval = Content.ToString();
                    break;
                case ResultType.error:
                    foreach(string error in Errors)
                    {
                        retval += error;
                    }
                    break;
                case ResultType.exception:
                    Exception ex = Exception;
                    retval = Exception.Message;
                    ex = Exception;
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        retval += ex.Message;
                    }
                    break;
            }
            return retval;
        }
    }
}
