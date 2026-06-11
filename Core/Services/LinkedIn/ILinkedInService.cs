using System.Threading.Tasks;
using Core.DTO.LinkedIn;
using Core.DTO.LinkedIn.Request;
using Core.DTO.LinkedIn.Response;

namespace Core.Services
{
    public interface ILinkedInService
    {
        Task<string> ExchangeCodeForAccessToken(string code, string redirectUri);
        Task<LinkedInAccountInfoResponse> GetAccountInfo(string accessToken);
        Task<string> PostTextAsync(LinkedInPostRequest request);
    }
}
