using light_brary.DTOs.Requests;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Repositories;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class AuthService(
        SignInManager<User> signInManager,
        UserService userService,
        UserManager<User> userManager,
        UserRepository userRepository)
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
                throw AppException.Unauthorized("Tài khoản của bạn đã bị khóa, liên hệ quản thư để được hỗ trợ.");

            if (!result.Succeeded)
                throw AppException.Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await userService.GetRequiredUserAsync(userId);

            if (request.NewPassword == request.CurrentPassword)
                throw AppException.BadRequest("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            (await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword)).ThrowIfFailed();
        }

        public async Task ChangePhoneNumberAsync(Guid userId, ChangePhoneNumberRequest request)
        {
            var user = await userService.GetRequiredUserAsync(userId);

            if (request.NewPhoneNumber == user.PhoneNumber)
                throw AppException.BadRequest("Số điện thoại mới không được trùng với số điện thoại hiện tại.");

            if (await userRepository.ExistsByPhoneNumberAsync(request.NewPhoneNumber))
                throw AppException.Conflict("Số điện thoại này đã được sử dụng.");

            (await userManager.SetPhoneNumberAsync(user, request.NewPhoneNumber)).ThrowIfFailed();
        }

        public async Task LogoutAsync(Guid userId)
        {
            var user = await userService.GetRequiredUserAsync(userId);
            (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
        }

    }
}
