using MyWebAppApi.DTOs;
using MyWebAppApi.Repository.Interfaces;

namespace MyWebAppApi.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<IEnumerable<UsersViewDto>>> GetAllUsers();
        Task<ApiResponse<string>> DeleteUser(int id);
        Task<ApiResponse<string>> ToaggleUserStatus(int id);

    }
}
