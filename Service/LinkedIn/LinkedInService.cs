using Core.Application.DTO.LinkedIn;
using Core.Application.DTO.LinkedIn.Request;
using Core.Application.DTO.LinkedIn.Response;
using Core.Application.DTO.PostAttachment.Response;
using Core.Application.Services;
using Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service.LinkedIn
{
    public class LinkedInService(HttpClient client, LinkedInConfig enviromentalConfig,
        IUserUploadedFileService userUploadedFileService,
        IConfiguration appsettingsConfig) : ILinkedInService
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

                string? pfp = root.TryGetProperty("picture", out var picElem) ? picElem.GetString() : null;

                LinkedInAccountInfoResponse response = new()
                {
                    Name = root.GetProperty("name").ToString(),
                    Sub = root.GetProperty("sub").ToString(),
                    PFPurl = pfp
                };

                return response;

            }
            catch (Exception ex)
            {
                throw new BadRequestException("Failed to parse data from LinkedIn response.", ex);
            }
        }

        public async Task<List<LinkedInOrganizationsResponse>> GetUserCompanies(string accessToken)
        {

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            client.DefaultRequestHeaders.Add("LinkedIn-Version", "202405");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            var response = await client.GetAsync(
                "https://api.linkedin.com/rest/organizations?q=roleAssignee");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<LinkedInOrganizationsResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result?.Elements ?? new();
        }

        public async Task<string> PostTextAsync(LinkedInPostRequest request)
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

            if (json.StartsWith("\""))
            {
                json = JsonSerializer.Deserialize<string>(json);
            }

            using var doc = JsonDocument.Parse(json);

            var id = doc.RootElement.GetProperty("id").GetString();

            return id;
        }

        public async Task<string> PostTextWithMediaAsync(LinkedInPostWithMediaRequest request, string userId)
        {
            var accessToken = request.AccessToken;
            var authorUrn = $"urn:li:person:{request.ExternalAccountId}";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            client.DefaultRequestHeaders.Remove("X-Restli-Protocol-Version");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            var media = await CreateMediaAsync(
                request.Attachments,
                authorUrn,
                accessToken,
                userId);

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
                            text = request.Content
                        },
                        shareMediaCategory = "IMAGE",
                        media
                    }
                },
                visibility = new
                {
                    com_linkedin_ugc_MemberNetworkVisibility = "PUBLIC"
                }
            };

            var raw = JsonSerializer.Serialize(body)
                .Replace("com_linkedin_ugc_ShareContent", "com.linkedin.ugc.ShareContent")
                .Replace("com_linkedin_ugc_MemberNetworkVisibility", "com.linkedin.ugc.MemberNetworkVisibility");

            var resp = await client.PostAsync(
                $"{appsettingsConfig["LinkedIn:ApiUrl"]}ugcPosts",
                new StringContent(raw, Encoding.UTF8, "application/json"));

            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new BadRequestException($"LinkedIn post failed: {resp.StatusCode} - {json}");
            }

            if (json.StartsWith("\""))
            {
                json = JsonSerializer.Deserialize<string>(json);
            }

            using var doc = JsonDocument.Parse(json);

            var id = doc.RootElement.GetProperty("id").GetString();

            return id;
        }


        private async Task<List<object>> CreateMediaAsync(
        IEnumerable<PostAttachmentResponse> attachments,
        string ownerUrn,
        string accessToken,
        string userId)
        {
            var media = new List<object>();

            foreach (var attachment in attachments)
            {
                var (asset, uploadUrl) = await RegisterUploadAsync(ownerUrn);

                await UploadMediaAsync(uploadUrl, accessToken, attachment, userId);

                media.Add(new
                {
                    status = "READY",
                    media = asset
                });
            }

            return media;
        }


        private async Task<(string Asset, string UploadUrl)> RegisterUploadAsync(string ownerUrn)
        {
            var body = new
            {
                registerUploadRequest = new
                {
                    recipes = new[]
                    {
                "urn:li:digitalmediaRecipe:feedshare-image"
            },
                    owner = ownerUrn,
                    serviceRelationships = new[]
                    {
                new
                {
                    relationshipType = "OWNER",
                    identifier = "urn:li:userGeneratedContent"
                }
            }
                }
            };

            var response = await client.PostAsync(
                $"{appsettingsConfig["LinkedIn:ApiUrl"]}assets?action=registerUpload",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new BadRequestException($"LinkedIn register upload failed: {json}");

            using var document = JsonDocument.Parse(json);

            var value = document.RootElement.GetProperty("value");

            var asset = value.GetProperty("asset").GetString()!;

            var uploadUrl = value
                .GetProperty("uploadMechanism")
                .GetProperty("com.linkedin.digitalmedia.uploading.MediaUploadHttpRequest")
                .GetProperty("uploadUrl")
                .GetString()!;

            return (asset, uploadUrl);
        }


        private async Task UploadMediaAsync(
            string uploadUrl,
            string accessToken,
            PostAttachmentResponse attachment,
            string userId)
        {
            var fileResult = await userUploadedFileService.DownloadFileById(attachment.UserUploadedFileId, userId);

            fileResult.FileStream.Position = 0;

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = new StreamContent(fileResult.FileStream);

            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue(fileResult.ContentType);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new BadRequestException($"LinkedIn upload failed: {error}");
            }
        }

    }


}
