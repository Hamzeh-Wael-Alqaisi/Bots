﻿using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class AddToProjectCard
    {
        public const string SubmissionId = "AddToProjectSubmit";

        public static async Task<AdaptiveCard> GetCardAsync(ITurnContext turnContext, ConsultingRequestDetails requestDetails)
        {
            var templateJson = String.Empty;
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("ConsultingBot.Cards.AddToProjectCard.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                templateJson = await reader.ReadToEndAsync();
            }

            requestDetails.monthZero = GetMonthFromNow(0).ToString("MMMM, yyyy");
            requestDetails.monthOne = GetMonthFromNow(1).ToString("MMMM, yyyy");
            requestDetails.monthTwo = GetMonthFromNow(2).ToString("MMMM, yyyy");
            var dataJson = JsonConvert.SerializeObject(requestDetails);

            var transformer = new AdaptiveTransformer();
            var cardJson = transformer.Transform(templateJson, dataJson);

            var result = AdaptiveCard.FromJson(cardJson).Card;
            return result;
        }

        public class AddToProjectCardActionValue : CardActionValue
        {
            public string command { get; set; }
            public string personName { get; set; }
            public string clientName { get; set; }
            public string projectName { get; set; }
            public string role { get; set; }
            public string monthZero { get; set; }
            public string monthOne { get; set; }
            public string monthTwo { get; set; }
            public string forecastZero { get; set; }
            public string forecastOne { get; set; }
            public string forecastTwo { get; set; }
        }

        public static async Task<AdaptiveCard> GetCardAsync(ITurnContext turnContext, AddToProjectCardActionValue payload)
        {
            var templateJson = String.Empty;
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("ConsultingBot.Cards.AddToProjectConfirmationCard.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                templateJson = await reader.ReadToEndAsync();
            }

            var dataJson = JsonConvert.SerializeObject(payload);

            var transformer = new AdaptiveTransformer();
            var cardJson = transformer.Transform(templateJson, dataJson);

            return AdaptiveCard.FromJson(cardJson).Card;

        }

        public static async Task<InvokeResponse> OnSubmit(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var val = turnContext.Activity.Value as JObject;
            var payload = val.ToObject<AddToProjectCardActionValue>();

            if (payload.command == "submit")
            {
                var card = await GetCardAsync(turnContext, payload);
                var newActivity = MessageFactory.Attachment(card.ToAttachment());
                newActivity.Id = turnContext.Activity.ReplyToId;
                await turnContext.UpdateActivityAsync(newActivity, cancellationToken);

                return new InvokeResponse() { Status = 200 };
            }
            else
            {
                var newActivity = MessageFactory.Text("Cancelled request");
                newActivity.Id = turnContext.Activity.ReplyToId;
                await turnContext.UpdateActivityAsync(newActivity, cancellationToken);

                return new InvokeResponse() { Status = 200 };
            }
        }

        // GetMonthFromNow() - returns the 1st of the month +/- delta months
        private static DateTime GetMonthFromNow(int delta)
        {
            var now = DateTime.Now;
            var month = ((now.Month - 1 + delta) % 12) + 1;
            var year = (now.Year + (month < now.Month ? 1 : 0));
            return new DateTime(year, month, 1);
        }

    }
}
