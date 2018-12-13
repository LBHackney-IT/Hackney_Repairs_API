using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackneyRepairs.Logging
{
    public static class HttpResponseMessageExtension 
    {
        public static async Task<ExceptionResponse> ExceptionResponse(this HttpResponseMessage httpResponseMessage)
        {
            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            ExceptionResponse exceptionResponse = JsonConvert.DeserializeObject<ExceptionResponse>(responseContent);

            return exceptionResponse;
        }
    }

    public class ExceptionResponse
    {
        public string Message { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionType { get; set; }

        public string StackTrace { get; set; }

        public ExceptionResponse InnerException { get; set; }
    }
}
