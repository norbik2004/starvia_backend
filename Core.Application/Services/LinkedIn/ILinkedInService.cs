using System.Threading.Tasks;
using Core.Application.DTO.LinkedIn;
using Core.Application.DTO.LinkedIn.Request;
using Core.Application.DTO.LinkedIn.Response;

namespace Core.Application.Services
{
    public interface ILinkedInService
    {
        Task<string> ExchangeCodeForAccessToken(string code, string redirectUri);
        Task<LinkedInAccountInfoResponse> GetAccountInfo(string accessToken);
        Task<string> PostTextAsync(LinkedInPostRequest request);
        Task<List<LinkedInOrganizationsResponse>> GetUserCompanies(string accessToken);
    }
}
