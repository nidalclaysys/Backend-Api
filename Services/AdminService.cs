using MyWebAppApi.DTOs;
using MyWebAppApi.Helper;
using MyWebAppApi.Repository.Interfaces;
using MyWebAppApi.Services.Interfaces;

namespace MyWebAppApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFinder _userFinder;
        private readonly ILogger<AdminService> _logger;

        public AdminService(IUserRepository userRepository,IUserFinder userFinder,ILogger<AdminService> logger)
        {
            _userRepository = userRepository;
            _userFinder = userFinder;
            _logger = logger;

        }

        public async Task<ApiResponse<IEnumerable<UsersViewDto>>> GetAllUsers()
        {
            _logger.LogInformation("Fetching AllUsers data");

            var role = _userFinder.GetRole();

            if (role != "Admin")
            {
                _logger.LogWarning("Autharization revoked! for {role}", role);
                return ApiResponseBuilder.Fail<IEnumerable<UsersViewDto>>("Forbidden", 403);
            }
            var users = await _userRepository.GetAllUsers();
            _logger.LogInformation("Retrived all users data successfully");
            return ApiResponseBuilder.Success(users, "Retrived all users successfully");
        }

        public async Task<ApiResponse<IEnumerable<UsersViewDto>>> GetBySearch(string search)
        {
            _logger.LogInformation("Fetching AllUsers data");

            var role = _userFinder.GetRole();

            if (role != "Admin")
            {
                _logger.LogWarning("Autharization revoked! for {role}", role);
                return ApiResponseBuilder.Fail<IEnumerable<UsersViewDto>>("Forbidden", 403);
            }

            _logger.LogInformation("adimin try to search term on : {search}", search);

            var users = await _userRepository.GetAllBySearch(search);
            _logger.LogInformation("Retrived search result no users present :  {count}",users.Count());

            return ApiResponseBuilder.Success(users, "Retrived search result successfully");
        }

        public async Task<ApiResponse<string>> DeleteUser(int id)
        {
            _logger.LogInformation("operation started for remove user : {UserId}", id);
            var result = await _userRepository.DeleteUser(id);
            if (result == null)
            {
                _logger.LogError("User Not Found For {userId}", id);
                return ApiResponseBuilder.Fail<string>("Invalid user id", 404);
            }

            if (result.ResultCode == 1)
            {
                _logger.LogInformation("User {UserId} removed successfully ", id);

                return ApiResponseBuilder.Success<string>(null!,"User deleted successfully");
            }
            _logger.LogError("operatin failed for user {userId}",id);
            return ApiResponseBuilder.Fail<string>(result.Message ?? "Operation Failed",500);
        }

       public async Task<ApiResponse<string>> ToaggleUserStatus(int id)
       {
            _logger.LogInformation("Fetching AllUsers data");

            var role = _userFinder.GetRole();

            if (role != "Admin")
            {
                _logger.LogWarning("Autharization revoked! for {role}", role);
                return ApiResponseBuilder.Fail<string>("Forbidden", 403);
            }

            var res = await _userRepository.ToaggleStatus(id);

            return res ? ApiResponseBuilder.Success<string>(null!,"Staus Updated Successfully") : ApiResponseBuilder.Fail<string>("Status Update Failed", 500);
        }
    }
}
