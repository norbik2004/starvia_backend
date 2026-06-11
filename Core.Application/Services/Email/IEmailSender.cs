using Core.Application.DTO.Email.Models;

namespace Core.Application.Services.Email
{
    public interface IEmailSender
    {
        public Task<bool> SendConfirmationEmail(ConfirmEmailRequest request);
    }
}
