using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.MicrosoftGraph
{
    /// <summary>
    /// Provides a data recorder that works with Office 365, OneDrive for Business, and Excel, to store recorded temperatures.
    /// </summary>
    public class Office365DataRecorder : IDataRecorder
    {
        private readonly MicrosoftGraphConfig Config;

        private AuthenticationResult AccessToken { get; set; }

        private HttpClient HttpClient { get; set; }

        public Office365DataRecorder(MicrosoftGraphConfig config)
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

        public override async Task RecordDataAsync(TemperatureData data)
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
                    var error = await ParseResponseAsync<ErrorResponse>(response);
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

            if (null != result && !string.IsNullOrEmpty(result.Id))
                return result.Id;

            return null;
        }

        private StringContent GetJsonObjectContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        public async Task<T> ParseResponseAsync<T>(HttpResponseMessage response) where T : class
        {
            string data = null;
            try
            {
                data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                Program.LogMessage("Unable to deserialize JSON response data: " + ex.Message);
                if (null != data)
                {
                    Program.LogMessage("Received data: " + data);
                }
                return default(T);
            }
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

    #region Wrappers for MS Graph data objects
    // TODO: Replace with the Microsoft Graph SDK
    class GraphItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }
    }

    class ErrorResponse
    {
        [JsonProperty("error")]
        public ErrorMessage Error { get; set; }
    }

    class ErrorMessage
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }


        [JsonProperty("innerError")]
        public InnerError InnerError { get; set; }
    }

    class InnerError
    {
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("request-id")]
        public string RequestId { get; set; }
    }
    #endregion
}
