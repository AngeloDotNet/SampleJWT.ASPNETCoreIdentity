namespace Identity.Models;

public class ChangePasswordRequest
{
    public string emailUser { get; set; }
    public string oldPassword { get; set; }
    public string newPassword { get; set; }
}
