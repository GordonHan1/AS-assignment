using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebApplication1.Services
{
    public class GoogleCaptchaService
    {
        private readonly string _secretKey;
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleCaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _secretKey = configuration["GoogleReCaptcha:SecretKey"];
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> VerifyToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Google’s verify URL
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={token}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
                return false;

            var jsonString = await response.Content.ReadAsStringAsync();
            dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

            // The JSON contains: {"success": true/false, "challenge_ts": "", "hostname": ""}
            bool success = jsonData.success;
            return success;
        }
    }
}
