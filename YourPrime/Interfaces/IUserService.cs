using YourPrime.DTOs.Users;
using YourPrime.DTOs.Admin;
using Microsoft.AspNetCore.Http;

namespace YourPrime.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    
    Task<UserProfileResponse> UpdateUserStatsAsync(int userId, UpdateUserStatsRequest request);
    
    Task<UserProfileResponse> GetMyProfileAsync(int userId);
    
    Task<UserProfileResponse> UploadAvatarAsync(int userId, IFormFile file);
}