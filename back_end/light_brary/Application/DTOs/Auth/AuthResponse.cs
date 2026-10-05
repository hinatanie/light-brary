using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Users;

namespace light_brary.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public UserResponse User { get; set; } = null!;
    }
}