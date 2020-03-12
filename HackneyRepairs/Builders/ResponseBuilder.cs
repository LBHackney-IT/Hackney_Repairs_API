using System;
using System.Collections.Generic;
using HackneyRepairs.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackneyRepairs.Builders
{
    public static class ResponseBuilder
    {
        public static JsonResult Error(int errorCode, string userMessage, string developerMessage)
        {
            var error = new List<ApiErrorMessage>
            {
                new ApiErrorMessage
                {
                    DeveloperMessage = developerMessage,
                    UserMessage = userMessage
                }
            };
            var jsonResponse = new JsonResult(error)
            {
                StatusCode = errorCode
            };

            return jsonResponse;
        }

        public static JsonResult ErrorFromList(int errorCode, IEnumerable<ApiErrorMessage> errors)
        {
            var jsonResponse = new JsonResult(errors)
            {
                StatusCode = errorCode
            };
            return jsonResponse;
        }

        //Overload for JSON API error format
        public static JsonResult ErrorFromList(int errorCode, IEnumerable<JsonApiErrorMessage> errors)
        {
            var jsonResponse = new JsonResult(errors)
            {
                StatusCode = errorCode
            };
            return jsonResponse;
        }

        public static JsonResult Ok(object responseContent)
        {
            var jsonResponse = new JsonResult(responseContent)
            {
                StatusCode = 200,
                ContentType = "application/json" 
            };

            return jsonResponse;
        }

        public static JsonResult OkEmpty(object responseContent)
        {
            var jsonResponse = new JsonResult(responseContent)
            {
                StatusCode = 204
            };

            return jsonResponse;
        }

        public static JsonResult NoItem(object responseContent)
        {
            var jsonResponse = new JsonResult(responseContent)
            {
                StatusCode = 404
            };

            return jsonResponse;
        }
    }
}
