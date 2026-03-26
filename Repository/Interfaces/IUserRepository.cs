using Microsoft.AspNetCore.Identity.Data;
using MyApp.Models;
using MyWebApp.Models;
using MyWebAppApi.DTOs;

namespace MyWebAppApi.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<int> RegisterUser(RegisterRequestDto dto, string hashedpass, int age);
        Task<Credential?> GetUserByUsername(string username);
        Task SaveLogin(int id);
        Task<Users?> GetUserProfile(int id);
        Task<DbResponse> UpdateUserProfile(int id,UpdateProfileDto updateProfileDto,int age, string role);
        Task<string?> GetPasswordById(int id);
        Task<bool> SavePassword(int id, string password);
        Task<bool> UploadImage(int id, byte[] imageBytes, string imagePath, string role);
        Task<IEnumerable<UsersViewDto>> GetAllUsers();
        Task<string?> GetImagePath(int id);
        Task<DbResponse?> DeleteUser(int id);

        Task<bool> LockUser(int id);
        Task<bool> LockedOrNot(string username);

        Task SaveLoginAttempt(int id);

        Task<bool> ToaggleStatus(int id);
    }

}
