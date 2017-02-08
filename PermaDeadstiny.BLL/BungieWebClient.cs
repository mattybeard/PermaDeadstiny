using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using PermaDeadstiny.BLL.HelperModel;
using PermaDeadstiny.BLL.Model;
using PermaDeadstiny.BLL.Model.Character;
using PermaDeadstiny.BLL.Model.Stats;

namespace PermaDeadstiny.BLL
{
    public class BungieClient
    {
        private const string BungieBaseUri = "https://www.bungie.net/";
        private const string AccessTokenRequest = "Platform/App/GetAccessTokensFromCode/";
        private const string RefreshTokenRequest = "Platform/App/GetAccessTokensFromRefreshToken/";
        private const string AuthenticationCodeRequest = "https://www.bungie.net/en/Application/Authorize/6871";
        private const string ApiKey = "2aa2b040a1904c97b94550eaaabd54ab";
        private const int Success = 1;
        private string _authCode;
        public int MembershipType;

        public BungieClient()
        {
            PrepareBungieRequests();
        }

        public HttpClient Client { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void PrepareBungieRequests()
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            Client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
        }

        /// <summary>
        ///     Makes an HTTP GET request to the specified endpoint
        /// </summary>
        /// <typeparam name="T">Type of object expected</typeparam>
        /// <param name="endpoint">Endpoint which is being accessed</param>
        public T RunGetAsync<T>(string endpoint)
        {
            return RunRequestAsync<T>(endpoint, "GET");
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T">Type of object expected</typeparam>
        /// <param name="requestObject">Object to be serialized and sent in the POST request</param>
        /// <param name="endpoint">Endpoint which is being accessed</param>
        public T RunPostAsync<T>(object requestObject, string endpoint)
        {
            return RunRequestAsync<T>(endpoint, "POST", requestObject);
        }

        /// <summary>
        ///     Makes an HTTP Web Request to the specified Bungie.net endpoint.
        /// </summary>
        /// <returns>
        ///     Returns deserialized object of specified type T.
        /// </returns>
        private T RunRequestAsync<T>(string endpoint, string requestType, object postObject = null)
        {
            try
            {
                HttpResponseMessage response = null;
                //if (!string.IsNullOrEmpty(_accessToken) && endpoint != "Platform/App/GetAccessTokensFromRefreshToken/")
                //{
                //    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                //}

                if (requestType == "POST")
                {
                    var json = postObject.ToJson();
                    var requestStringContent = new StringContent(json);
                    response = Client.PostAsync(new Uri(BungieBaseUri + endpoint), requestStringContent).Result;
                    if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        var url = response.Headers.Location;
                        var retryContent = new StringContent(json);
                        response = Client.PostAsync(url, retryContent).Result;
                    }
                }

                else if (requestType == "GET")
                    response = Client.GetAsync(new Uri(BungieBaseUri + endpoint)).Result;

                if (response != null && response.IsSuccessStatusCode)
                {
                    var message = response.Content.ReadAsStringAsync().Result;
                    return message.FromJson<T>();
                }
            }
            catch (Exception ex)
            {
                SendErrorAlert(ex);
                //log
            }
            return default(T);
        }

        private void SendErrorAlert(Exception exception)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("mattybeard@gmail.com");
            msg.To.Add("mattybeard@gmail.com");
            msg.Subject = "GO Exception";
            msg.Body = exception.ToString();
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("mattybeard@gmail.com", "Baxter2242");
            client.Timeout = 20000;
            try
            {
                client.Send(msg);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                msg.Dispose();
            }
        }

        public UserDetails RetrieveUserDetails(int membershipType, string membershipName)
        {
            var membershipDetails = RunGetAsync<MembershipResponse>($"Platform/Destiny/SearchDestinyPlayer/{membershipType}/{membershipName}/");
            var membershipId = membershipDetails?.Response?.FirstOrDefault();
            if (membershipId == null)
                return null;

            var results = new UserDetails { MembershipId = membershipId.membershipId, Console = membershipId.membershipType };
            var characterDetails = RunGetAsync<CharacterEndpoint>($"Platform/Destiny/{membershipId.membershipType}/Account/{membershipId.membershipId}/Summary/");
            var _characterIds = characterDetails.Response.data.characters.Select(c => c.characterBase.characterId).ToArray();

            results.CharacterIds = _characterIds;

            return results;
        }

        public PermaResults PermaCheck(UserDetails details)
        {
            var results = new PermaResults() {Details = details, MaxPermaLight = 0, MaxPermaLevel = 0 };

            foreach (var character in details.CharacterIds)
            {
                var characterStats = RunGetAsync<StatResponse>($"Platform/Destiny/Stats/{details.Console}/{details.MembershipId}/{character}/");
                var totalDeaths = CalculateTotalDeaths(characterStats);

                if(totalDeaths == 0)
                {
                    if (((int) characterStats.Response.story.allTime.highestCharacterLevel.basic.value) > results.MaxPermaLevel)
                        results.MaxPermaLevel = (int) characterStats.Response.story.allTime.highestCharacterLevel.basic.value;

                    if (((int)characterStats.Response.story.allTime.highestLightLevel.basic.value) > results.MaxPermaLight)
                        results.MaxPermaLight = (int) characterStats.Response.story.allTime.highestLightLevel.basic.value;
                }
            }

            return results;
        }

        private int CalculateTotalDeaths(StatResponse characterStats)
        {
            var deathSum = 0.0;
            if(characterStats.Response.allPvP.allTime != null)
                deathSum = deathSum + characterStats.Response.allPvP.allTime.deaths.basic.value;

            if (characterStats.Response.allArena.allTime != null)
                deathSum = deathSum + characterStats.Response.allArena.allTime.deaths.basic.value;

            if (characterStats.Response.allStrikes.allTime != null)
                deathSum = deathSum + characterStats.Response.allStrikes.allTime.deaths.basic.value;

            if (characterStats.Response.patrol.allTime != null)
                deathSum = deathSum + characterStats.Response.patrol.allTime.deaths.basic.value;

            if (characterStats.Response.raid.allTime != null)
                deathSum = deathSum + characterStats.Response.raid.allTime.deaths.basic.value;

            if (characterStats.Response.story.allTime != null)
                deathSum = deathSum + characterStats.Response.story.allTime.deaths.basic.value;

            return Convert.ToInt32(deathSum);
        }
    }

    public class PermaResults
    {
        public UserDetails Details { get; set; }
        public int MaxPermaLight { get; set; }
        public int MaxPermaLevel { get; set; }
    }
}
