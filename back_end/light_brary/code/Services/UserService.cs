using light_brary.Services.Authentication;
using light_brary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;

namespace light_brary.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;
        private readonly UserRoleRepository _userRoleRepository;
        private readonly BorrowRecordRepository _borrowRecordRepository;
        private readonly PasswordHasher _passwordHasher;

        public UserService(
            UserRepository userRepository,
            RoleRepository roleRepository,
            UserRoleRepository userRoleRepository,
            BorrowRecordRepository borrowRecordRepository,
            PasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync(string? keyword = null, string? role = null)
        {
            var users = await _userRepository.GetAllWithRolesAsync(keyword, role);
            return users.Select(MapToResponse).ToList();
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

            return MapToResponse(user);
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var phone = request.PhoneNumber.Trim();
            if (await _userRepository.ExistsByPhoneNumberAsync(phone))
                throw new ConflictException("Số điện thoại này đã được sử dụng.");

            var user = new User
            {
                FullName = request.FullName.Trim(),
                PhoneNumber = phone,
                Address = request.Address?.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var defaultRole = await _roleRepository.GetByNameAsync(Role.USER);
            if (defaultRole is not null)
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                });
            }

            var loadedUser = await _userRepository.GetByIdWithRolesAsync(user.Id);
            return MapToResponse(loadedUser ?? user);
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

            var phone = request.PhoneNumber.Trim();
            var existing = await _userRepository.GetByPhoneNumberAsync(phone);
            if (existing is not null && existing.Id != id)
                throw new ConflictException("Số điện thoại này đã được sử dụng.");

            user.FullName = request.FullName.Trim();
            user.PhoneNumber = phone;
            user.Address = request.Address?.Trim();

            await _userRepository.UpdateAsync(user);

            return MapToResponse(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

            if (await _userRepository.HasBorrowHistoryAsync(id))
                throw new ConflictException("Người dùng đã có lịch sử mượn sách, không thể xóa.");

            var adminRole = await _roleRepository.GetByNameAsync(Role.ADMIN);
            if (adminRole is not null && user.UserRoles.Any(ur => ur.RoleId == adminRole.Id))
            {
                var adminUsers = await _userRoleRepository.GetByRoleIdAsync(adminRole.Id);
                if (adminUsers.Count() <= 1)
                    throw new BusinessException("Không thể xóa tài khoản Admin cuối cùng trong hệ thống.");
            }

            await _userRepository.DeleteAsync(user);
        }

        public async Task<UserResponse> GetProfileAsync(Guid userId)
        {
            return await GetByIdAsync(userId);
        }

        public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
        {
            return await UpdateAsync(userId, request);
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetUserBorrowRecordsAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var records = await _borrowRecordRepository.GetByUserIdAsync(userId);
            var userResponse = MapToResponse(user);

            return records.Select(r => MapToBorrowRecordResponse(r, userResponse)).ToList();
        }

        public async Task<IEnumerable<BorrowedBookResponse>> GetUserBorrowedBooksAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var records = await _borrowRecordRepository.GetByUserIdAsync(userId);

            var list = new List<BorrowedBookResponse>();
            foreach (var record in records)
            {
                foreach (var detail in record.BorrowDetails.Where(d => d.Status == BorrowDetailStatus.Borrowing))
                {
                    list.Add(new BorrowedBookResponse
                    {
                        BorrowDetailId = detail.Id,
                        BorrowRecordId = record.Id,
                        BookCopyId = detail.BookCopyId,
                        Barcode = detail.BookCopy?.Barcode ?? string.Empty,
                        BookId = detail.BookCopy?.BookId ?? Guid.Empty,
                        BookTitle = detail.BookCopy?.Book?.Title ?? string.Empty,
                        BorrowDate = record.BorrowDate,
                        DueDate = record.DueDate,
                        IsOverdue = DateTime.UtcNow > record.DueDate
                    });
                }
            }

            return list.OrderBy(b => b.DueDate).ThenBy(b => b.BorrowDetailId).ToList();
        }

        public async Task AdminUpdatePasswordAsync(Guid id, AdminUpdatePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword.Trim());
            await _userRepository.UpdateAsync(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedException("Mật khẩu hiện tại không chính xác.");

            if (request.NewPassword.Trim() == request.CurrentPassword)
                throw new BusinessException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword.Trim());
            await _userRepository.UpdateAsync(user);
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = user.UserRoles?.Where(ur => ur.Role != null).Select(ur => ur.Role.Name).ToList() ?? new List<string>(),
                CreatedAt = user.CreatedAt
            };
        }

        private static BorrowRecordResponse MapToBorrowRecordResponse(BorrowRecord record, UserResponse user)
        {
            var isOverdue = record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && DateTime.UtcNow > record.DueDate;

            return new BorrowRecordResponse
            {
                Id = record.Id,
                User = user,
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                Status = record.Status.ToString(),
                Note = record.Note,
                IsOverdue = isOverdue,
                Details = record.BorrowDetails.Select(d => new BorrowDetailResponse
                {
                    Id = d.Id,
                    BookCopyId = d.BookCopyId,
                    Barcode = d.BookCopy?.Barcode ?? string.Empty,
                    BookId = d.BookCopy?.BookId ?? Guid.Empty,
                    BookTitle = d.BookCopy?.Book?.Title ?? string.Empty,
                    Status = d.Status.ToString(),
                    ReturnDate = d.ReturnDate
                }).ToList()
            };
        }
    }
}