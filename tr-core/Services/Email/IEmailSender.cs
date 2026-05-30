using tr_core.DTO.Email.Models;

namespace tr_core.Services.Email
{
    public interface IEmailSender
    {
        public Task<bool> SendConfirmationEmail(ConfirmEmailRequest request);
    }
}
