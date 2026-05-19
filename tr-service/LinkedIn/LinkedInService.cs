using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using tr_core.DTO.LinkedIn;
using tr_core.DTO.LinkedIn.Request;
using tr_core.DTO.LinkedIn.Response;
using tr_core.Services;
using tr_service.Exceptions;

namespace tr_service.LinkedIn
{
    public class LinkedInService(HttpClient client, LinkedInConfig enviromentalConfig, IConfiguration appsettingsConfig) : ILinkedInService
    {

        public async Task<string> ExchangeCodeForAccessToken(string code, string redirectUri)
        {
            var values = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = enviromentalConfig.ClientId,
                ["client_secret"] = enviromentalConfig.ClientSecret
            };

            var resp = await client.PostAsync($"{appsettingsConfig["LinkedIn:BaseUrl"]}accessToken", new FormUrlEncodedContent(values));
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new BadRequestException($"LinkedIn token exchange failed: {resp.StatusCode} - {json}");
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("access_token").GetString()!;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Failed to parse access token from LinkedIn response.", ex);
            }
        }

        public async Task<LinkedInAccountInfoResponse> GetAccountInfo(string accessToken)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Remove("X-Restli-Protocol-Version");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");
            var resp = await client.GetAsync($"{appsettingsConfig["LinkedIn:ApiUrl"]}userinfo");
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new BadRequestException($"LinkedIn /userinfo failed: {resp.StatusCode} - {json}");
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                LinkedInAccountInfoResponse response = new()
                {
                    Name = root.GetProperty("name").ToString(),
                    Sub = root.GetProperty("sub").ToString(),
                    PFPurl = root.GetProperty("picture").ToString()
                };

                return response;

            }
            catch (Exception ex)
            {
                throw new BadRequestException("Failed to parse data from LinkedIn response.", ex);
            }
        }

        public async Task PostTextAsync(LinkedInPostRequest request)
        {
            var accessToken = request.AccessToken;
            var authorUrn = $"urn:li:person:{request.ExternalAccountId}";
            var text = request.Content;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Remove("X-Restli-Protocol-Version");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            var body = new
            {
                author = authorUrn,
                lifecycleState = "PUBLISHED",
                specificContent = new
                {
                    com_linkedin_ugc_ShareContent = new
                    {
                        shareCommentary = new 
                        {
                            text 
                        },
                        shareMediaCategory = "NONE"
                    }
                },
                visibility = new { com_linkedin_ugc_MemberNetworkVisibility = "PUBLIC" }
            };

            var raw = JsonSerializer.Serialize(body);

            raw = raw.Replace("com_linkedin_ugc_ShareContent", "com.linkedin.ugc.ShareContent")
                     .Replace("com_linkedin_ugc_MemberNetworkVisibility", "com.linkedin.ugc.MemberNetworkVisibility");

            var resp = await client.PostAsync($"{appsettingsConfig["LinkedIn:ApiUrl"]}ugcPosts", new StringContent(raw, Encoding.UTF8, "application/json"));
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new BadRequestException($"LinkedIn post failed: {resp.StatusCode} - {json}");
            }
        }
    }
}
