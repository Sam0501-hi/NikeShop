using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace NikeShop.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Đây là hàm giả, chỉ cần trả về Task.CompletedTask là xong
            return Task.CompletedTask;
        }
    }
}