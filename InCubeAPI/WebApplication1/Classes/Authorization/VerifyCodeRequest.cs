namespace WebApplication1.Classes.Authorization
{
    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public bool IsLogin { get; set; }
    }
}
