using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using System.Linq;
using System.Text.Json;

namespace YtNotifRelay
{
    public class YtPubSubListener
    {
        private readonly ILogger _logger;

        public YtPubSubListener(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<YtPubSubListener>();
        }

        [Function("YtPubSubListener")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }

        [Function("SubscribeToYouTube")]
        public async Task<HttpResponseData> SubscribeToYouTube([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Function SubscribeToYouTube processed a request");
            _logger.LogInformation("Headers:");
            foreach (var header in req.Headers)
            {
                _logger.LogInformation($"{header.Key}: {string.Join(",", header.Value)}");
            }

            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            if (hostname.StartsWith("localhost"))
            {
                var withPort = hostname.Contains(":") ? $":{hostname.Split(':')[1]}" : "";
                hostname = $"{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME_LOCAL")}{withPort}";
            }
            var callbackUrl = $"http://{hostname}/api/ReceiveNotification";
            var topicUrl = Environment.GetEnvironmentVariable("YT_TOPIC_URL");
            var hubUrl = Environment.GetEnvironmentVariable("YT_HUB_URL");

            var configLog = "Retrieved configuration:" + Environment.NewLine +
                            "  callbackUrl= " + callbackUrl + Environment.NewLine +
                            "  topicUrl   = " + topicUrl + Environment.NewLine +
                            "  hubUrl     = " + hubUrl + Environment.NewLine;
            _logger.LogInformation(configLog);

            if (string.IsNullOrEmpty(callbackUrl) || string.IsNullOrEmpty(topicUrl) || string.IsNullOrEmpty(hubUrl))
                return req.CreateResponse(HttpStatusCode.InternalServerError); // Ensure configs exist

            var postData = new Dictionary<string, string>
            {
                { "hub.mode", "subscribe" },
                { "hub.topic", topicUrl },
                { "hub.callback", callbackUrl }
            };
            
            _logger.LogInformation("Sending hub POST with 'subscribe'...");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(hubUrl, new FormUrlEncodedContent(postData));
                var resultMsg = "";
                if (response.IsSuccessStatusCode)
                {
                    resultMsg = $"Hub responded {response.StatusCode} {response.ReasonPhrase}. Subscribe successful!";
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    resultMsg = $"Subscribe failed with Error {response.StatusCode} {response.ReasonPhrase}! {Environment.NewLine}{errorDetails}";
                }
                _logger.LogInformation(resultMsg);
                return response.IsSuccessStatusCode ? req.CreateResponse(HttpStatusCode.OK) : req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }


        [Function("UnSubscribeToYouTube")]
        public async Task<HttpResponseData> UnSubscribeToYouTube([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Function UnSubscribeToYouTube processed a request");
            _logger.LogInformation("Headers:");
            foreach (var header in req.Headers)
            {
                _logger.LogInformation($"{header.Key}: {string.Join(",", header.Value)}");
            }

            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            if (hostname.StartsWith("localhost"))
            {
                var port = "";
                if (hostname.Contains(":"))
                {
                    port = hostname.Split(':')[1];
                }
                hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME_LOCAL") + ":" + port;
            }
            var callbackUrl = $"http://{hostname}/api/ReceiveNotification";
            var topicUrl = Environment.GetEnvironmentVariable("YT_TOPIC_URL");
            var hubUrl = Environment.GetEnvironmentVariable("YT_HUB_URL");

            var configLog = "Retrieved configuration:" + Environment.NewLine +
                            "  callbackUrl= " + callbackUrl + Environment.NewLine +
                            "  topicUrl   = " + topicUrl + Environment.NewLine +
                            "  hubUrl     = " + hubUrl + Environment.NewLine;
            _logger.LogInformation(configLog);

            if (string.IsNullOrEmpty(callbackUrl) || string.IsNullOrEmpty(topicUrl) || string.IsNullOrEmpty(hubUrl))
                return req.CreateResponse(HttpStatusCode.InternalServerError); // Ensure configs exist

            var postData = new Dictionary<string, string>
            {
                { "hub.mode", "unsubscribe" },
                { "hub.topic", topicUrl },
                { "hub.callback", callbackUrl }
            };

            _logger.LogInformation("Sending hub POST with 'unsubscribe'...");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(hubUrl, new FormUrlEncodedContent(postData));
                var resultMsg = "";
                if (response.IsSuccessStatusCode)
                {
                    resultMsg = $"Hub responded {response.StatusCode} {response.ReasonPhrase}. Unsubscribe successful!";
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    resultMsg = $"Unsubscribe failed with Error {response.StatusCode} {response.ReasonPhrase}! {Environment.NewLine}{errorDetails}";
                }
                _logger.LogInformation(resultMsg);
                return response.IsSuccessStatusCode ? req.CreateResponse(HttpStatusCode.OK) : req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        public static YouTubeFeed DeserializeFeed(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(YouTubeFeed));
            using (StringReader reader = new StringReader(xml))
            {
                return (YouTubeFeed)serializer.Deserialize(reader);
            }
        }

        [Function("ReceiveNotification")]
        public async Task<HttpResponseData> ReceiveNotification([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Function ReceiveNotification processed a request");
            _logger.LogInformation("Headers:");
            foreach (var header in req.Headers)
            {
                _logger.LogInformation($"{header.Key}: {string.Join(",", header.Value)}");
            }

            string webhookUrl = Environment.GetEnvironmentVariable("DISCORD_NOTIFY_WEBHOOK_URL");

            // Verification Request
            if (req.Method == HttpMethod.Get.Method)
            {
                var queryParams = req.Url.Query;
                string challenge = HttpUtility.ParseQueryString(queryParams)["hub.challenge"];
                _logger.LogInformation($"It's a verification request. Sending a response with challenge: {challenge}");
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(challenge);
                return response;
            }

            // Notification Handling
            if (req.Method == HttpMethod.Post.Method)
            {
                string content = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Received a notification!");
                YouTubeFeed feed = DeserializeFeed(content);

                _logger.LogInformation($"Feed Title: {feed.Title}");
                _logger.LogInformation($"Last Updated: {feed.Updated}");

                var discordMsg = new
                {
                    content = "New video!",
                    embeds = new List<dynamic>()
                };
                foreach (var entry in feed.Entries)
                {
                    var videoUrl = $"https://www.youtube.com/watch?v={entry.VideoId}";
                    var author = entry.Author.Name;
                    discordMsg.embeds.Add(new
                    {
                        title = entry.Title,
                        description = $"**{author} just posted!**" + Environment.NewLine + $"{videoUrl}",
                        url = videoUrl,
                        color = 15548997, // Red
                        image = new
                        {
                            url = $"https://img.youtube.com/vi/{entry.VideoId}/0.jpg"
                        }
                    });
                }
                var jsonPayload = JsonSerializer.Serialize(discordMsg);
                _logger.LogInformation("Constructed discord webhook payload:" + Environment.NewLine + jsonPayload);
                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        await client.PostAsync(webhookUrl, httpContent);
                        _logger.LogInformation("Posted notification to discord webhook");
                    }
                }
                else
                {
                    _logger.LogInformation("ERROR! DISCORD_NOTIFY_WEBHOOK_URL is null!");
                }
                
                return req.CreateResponse(HttpStatusCode.OK);
            }

            return req.CreateResponse(HttpStatusCode.BadRequest);
        }


    }
}
