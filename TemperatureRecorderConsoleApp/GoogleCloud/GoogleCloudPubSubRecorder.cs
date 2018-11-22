using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;

namespace TemperatureRecorderConsoleApp.GoogleCloud
{
    public class GoogleCloudPubSubRecorder : IDataRecorder
    {
        private readonly GoogleCloudConfig config;

        private GoogleCredential credential;
        private HttpClient httpClient;
        public GoogleCloudPubSubRecorder(GoogleCloudConfig config)
        {
            this.config = config;
        }

        public override async Task InitalizeAsync()
        {
            if (!string.IsNullOrEmpty(config.AuthorizationTokenPath))
            {
                Program.LogMessage("Setting Google Cloud credentials...");
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", config.AuthorizationTokenPath);
            }

            this.credential = GoogleCredential.GetApplicationDefault();

            await base.InitalizeAsync();
        }

        public override async Task RecordDataAsync(TemperatureData data)
        {
            Program.LogMessage("Publishing temperature: " + data.TemperatureF + " @ " + data.InstanceDateTime + ".");
            var json = JsonConvert.SerializeObject(data, Formatting.None);
            var bytes = Encoding.Unicode.GetBytes(json);

            try
            {
                await PublishMessageAsync(Convert.ToBase64String(bytes));
            }
            catch (Exception ex)
            {
                Program.LogMessage("Unable to record temperature to PubSub topic " + config.PubSubTopicName + ": " + ex);
            }
        }

        private HttpClient GetHttpClient()
        {
            if (this.httpClient != null)
                return this.httpClient;

            var client = new HttpClient();

            // TODO: Add any configuration here

            this.httpClient = client;
            return client;
        }

        private Task PublishMessageAsync(string message) 
        {
            return PublishMessagesAsync(new string[] { message });
        }

        private async Task PublishMessagesAsync(string[] messages) 
        {
            var accessToken = await this.credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var project_url = Uri.EscapeUriString(config.GoogleCloudProjectId);
            var topic_url = Uri.EscapeUriString(config.PubSubTopicName);

            var requestUri = "https://pubsub.googleapis.com/v1/projects/" + project_url + "/topics/" + topic_url + ":publish";

            var client = GetHttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var body = new PubSubPublishBody();
            body.Messages = new List<PubSubMessage>();
            body.Messages.AddRange(from m in messages select new PubSubMessage() { Data = m });
            request.Content = GetJsonObjectContent(body);

            try 
            {
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Program.LogMessage("Error publishing message: " + responseBody);
                }
            }
            catch (Exception ex)
            {
                Program.LogMessage("Exception occured while uploading: " + ex.Message);
                Program.LogMessage(ex.ToString());
            }
        }

        private StringContent GetJsonObjectContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        private class PubSubPublishBody {

            [JsonProperty("messages")]
            public List<PubSubMessage> Messages { get; set; }
        }

        private class PubSubMessage 
        {
            [JsonProperty("data")]
            public string Data { get; set; }
            
            [JsonProperty("messageId")]
            public string MessageId { get; set; }
            
            [JsonProperty("publishTime")]
            public DateTimeOffset PublishTime { get; set; }

            [JsonProperty("attributes")]
            public Dictionary<string, string> Attributes { get; set; }
        }
    }
}
