using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace TransportApp.MainApi.Filters
{
    public class CustomBadRequest
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("messages")]
        public IEnumerable<string> Messages { get; set; }


        public CustomBadRequest(ActionContext context)
        {
            IsSuccess = true;
            Messages = new List<string>();

            ConstructErrorMessages(context);
        }

        private void ConstructErrorMessages(ActionContext context)
        {
            var errorList = context.ModelState.Values.SelectMany(ms => ms.Errors)
                        .Select(er => !string.IsNullOrEmpty(er.ErrorMessage) ? er.ErrorMessage : er.Exception?.Message)
                        .ToList();

            if (errorList != null && errorList.Any())
            {
                Messages = errorList;
                IsSuccess = false;
            }
        }
    }
}
