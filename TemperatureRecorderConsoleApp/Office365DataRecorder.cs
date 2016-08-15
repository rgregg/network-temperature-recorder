using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TemperatureRecorderConsoleApp.Graph;

namespace TemperatureRecorderConsoleApp
{
    /// <summary>
    /// Provides a data recorder that works with Office 365, OneDrive for Business, and Excel, to store recorded temperatures.
    /// </summary>
    public class Office365DataRecorder : IDataRecorder
    {
        private readonly ConfigurationFile Config;

        private AuthenticationResult AccessToken { get; set; }

        private HttpClient HttpClient { get; set; }

        public Office365DataRecorder(ConfigurationFile config)
        {
            this.Config = config;
            Program.LogMessage("Recording data to OneDrive for Business: " + config.CloudDataFilePath);

            System.Net.ServicePointManager.ServerCertificateValidationCallback += 
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                   System.Security.Cryptography.X509Certificates.X509Chain chain,
                                   System.Net.Security.SslPolicyErrors sslPolicyErrors)
           {
               return true; // **** Always accept
           };
        }

        public async Task RecordDataAsync(TemperatureData data)
        {
            var haveAccessToken = await RetrieveAccessToken();
            if (!haveAccessToken)
            {
                Program.LogMessage("Unable to record data point. No access token.");
                return;
            }

            //var sessionId = await CreateExcelSessionAsync();

            await AppendDataToTableAsync(null, data);
        }

        private async Task AppendDataToTableAsync(string sessionId, TemperatureData data)
        {
            Program.LogMessage(data.ToString());

            var client = GetHttpClient();
            var requestUri = new Uri(BaseUri, "/v1.0/me/drive/root:/" + Uri.EscapeUriString(Config.CloudDataFilePath) + ":/workbook/tables('RecordedData')/Rows");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.AccessToken.AccessToken);
            if (!string.IsNullOrEmpty(sessionId))
            {
                request.Headers.TryAddWithoutValidation("workbook-session-id", sessionId);
            }
            var dateString = data.InstanceDateTime.ToString("MM/dd/yyyy HH:mm:ss");
            request.Content = GetJsonObjectContent(new { values = new object[] { new object[] { dateString, data.TemperatureC, data.TemperatureF } } });

            try 
            {
                var response = await client.SendAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    var error = await ParseResponseAsync<Graph.ErrorResponse>(response);
                    if (null != error)
                    {
                        Program.LogMessage("Error appending data: " + error.Error.Code);
                    }
                }
            } catch (Exception ex) {
                Program.LogMessage("Exception occured while uploading: " + ex.Message);
                Program.LogMessage(ex.ToString());
            }
        }

        private async Task<string> CreateExcelSessionAsync()
        {
            var client = GetHttpClient();
            var requestUri = new Uri(BaseUri, "/v1.0/me/drive/root:/" + Uri.EscapeUriString(Config.CloudDataFilePath) + ":/workbook/createSession");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.AccessToken.AccessToken);
            request.Content = GetJsonObjectContent(new { persistChanges = true });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Unable to create excel session. Request was unsuccessful.");
            }

            var result = await ParseResponseAsync<GraphItem>(response);

            if (null != response && !string.IsNullOrEmpty(result.Id))
                return result.Id;

            return null;
        }

        private StringContent GetJsonObjectContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        public async Task<T> ParseResponseAsync<T>(HttpResponseMessage response)
        {
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }

        private Uri BaseUri
        {
            get { return new Uri(Config.Office365ResourceUrl); }
        }

        private HttpClient GetHttpClient()
        {
            if (this.HttpClient != null)
                return this.HttpClient;

            var client = new HttpClient();

            // TODO: Add any configuration here

            this.HttpClient = client;
            return client;
        }

        private async Task<bool> RetrieveAccessToken()
        {
            if (null != AccessToken)
            {
                // Check to see if the token is still valid
                if (AccessToken.ExpiresOn >= DateTime.Now.AddMinutes(5))
                    return true;
            }

            try
            {
                var creds = new UserCredential(Config.Office365UserName, Config.Office365Password);
                var context = new AuthenticationContext(Config.Office365TokenService);
                var token = await context.AcquireTokenAsync(Config.Office365ResourceUrl, Config.Office365ClientId, creds);
                if (null != token)
                {
                    this.AccessToken = token;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Program.LogMessage("Unable to retrieve new access token:" + ex.Message);
                return false;
            }
        }
    }
}
