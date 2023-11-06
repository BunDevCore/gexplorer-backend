using System.ComponentModel.DataAnnotations;

namespace GdanskExplorer.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Username required!", AllowEmptyStrings = false)]
    [MinLength(4)]
    [MaxLength(30)]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password required!", AllowEmptyStrings = false)]
    public string Password { get; set; } = null!;
}