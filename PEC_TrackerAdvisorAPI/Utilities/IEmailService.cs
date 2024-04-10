using PEC_TrackerAdvisorAPI.Models;

namespace PEC_TrackerAdvisorAPI.Utilities
{
    public interface IEmailService
    {
        void SendEmail(Email email);
    }
}
