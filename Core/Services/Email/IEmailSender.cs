using Core.DTO.Email.Models;

namespace Core.Services.Email
{
    public interface IEmailSender
    {
        public Task<bool> SendConfirmationEmail(ConfirmEmailRequest request);
    }
}
