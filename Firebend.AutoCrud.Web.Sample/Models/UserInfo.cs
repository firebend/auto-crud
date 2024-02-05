using System;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class UserInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime? CreatedDate { get; set; }
}
