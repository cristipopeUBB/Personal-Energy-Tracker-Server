namespace PEC_TrackerAdvisorAPI.DTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; } = default!;
        public string EmailToken { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
