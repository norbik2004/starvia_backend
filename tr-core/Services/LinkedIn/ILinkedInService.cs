using System.Threading.Tasks;
using tr_core.DTO.LinkedIn;
using tr_core.DTO.LinkedIn.Request;
using tr_core.DTO.LinkedIn.Response;

namespace tr_core.Services
{
    public interface ILinkedInService
    {
        Task<string> ExchangeCodeForAccessToken(string code, string redirectUri);
        Task<LinkedInAccountInfoResponse> GetAccountInfo(string accessToken);
        Task<string> PostTextAsync(LinkedInPostRequest request);
    }
}
