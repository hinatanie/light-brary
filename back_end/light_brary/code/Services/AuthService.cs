using light_brary.DTOs.Requests;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class AuthService(
        SignInManager<User> signInManager,
        UserService userService)
    {
        public async Task LoginAsync(LoginRequest request)
        {
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            var result = await signInManager.PasswordSignInAsync(
                request.UserName.Trim(),
                request.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa, liên hệ quản thư để được hỗ trợ.");

            if (!result.Succeeded)
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var user = await userService.CreateAsync(new CreateUserRequest
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Password = request.Password
            });
            await LoginAsync(new LoginRequest
            {
                UserName = user.PhoneNumber,
                Password = request.Password
            });
        }
    }
}
