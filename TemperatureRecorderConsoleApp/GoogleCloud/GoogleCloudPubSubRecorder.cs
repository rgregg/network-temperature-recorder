using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.GoogleCloud
{
    public class GoogleCloudPubSubRecorder : IDataRecorder
    {
        private readonly GoogleCloudConfig config;
        private PublisherClient publisherClient;
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

            Program.LogMessage("Creating PublisherServiceApiClient");
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

            Program.LogMessage("Creating topic " + config.PubSubTopicName + "...");
            TopicName topicName = new TopicName(config.GoogleCloudProjectId, config.PubSubTopicName);
            try
            {
                await publisher.CreateTopicAsync(topicName);
                Program.LogMessage("Topic created.");
            }
            catch (RpcException e)
            when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                // Already exists.  That's fine.
                Program.LogMessage("Topic already exists.");
            }

            Program.LogMessage("Creating PubSub publisher...");
            publisherClient = await GetPublisherAsync(config.GoogleCloudProjectId, config.PubSubTopicName);

            await base.InitalizeAsync();
        }

        private async Task<PublisherClient> GetPublisherAsync(string projectId, string topicId)
        {
            return await PublisherClient.CreateAsync(
                new TopicName(projectId, topicId), 
                null,
                new PublisherClient.Settings
                {
                    BatchingSettings = new Google.Api.Gax.BatchingSettings(
                        elementCountThreshold: 100,
                        byteCountThreshold: 10240,
                        delayThreshold: TimeSpan.FromSeconds(3))
                });
        }

        public override async Task RecordDataAsync(TemperatureData data)
        {
            Program.LogMessage("Publishing temperature: " + data.TemperatureF + " @ " + data.InstanceDateTime + ".");
            var json = JsonConvert.SerializeObject(data, Formatting.None);
            var bytes = Encoding.Unicode.GetBytes(json);

            try
            {
                await publisherClient.PublishAsync(Convert.ToBase64String(bytes));
            }
            catch (Exception ex)
            {
                Program.LogMessage("Unable to record temperature to PubSub topic " + config.PubSubTopicName + ": " + ex);
            }
        }
    }
}
