using BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApp.Models;
using MyWebApp.Models;
using MyWebAppApi.DTOs;
using MyWebAppApi.Helper;
using MyWebAppApi.Repository.Interfaces;
using MyWebAppApi.Services.Interfaces;
using System.Data;

namespace MyWebAppApi.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserServices> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IJwtHelper _jwt;
        private readonly IUserFinder _findUser;

        public UserServices(IUserRepository userRepository, IWebHostEnvironment hostEnvironment, 
            ILogger<UserServices> logger,IJwtHelper jwtHelper,IUserFinder userFinder) 
        { 
            _userRepository = userRepository;
            _env = hostEnvironment;
            _logger = logger;
            _jwt = jwtHelper;
            _findUser = userFinder;
        }

        public async Task<ApiResponse<string>> RegisterUser(RegisterRequestDto dto)
        {
            var validateUsername = InputIdentifier.Identify(dto.UserName);

            if (validateUsername == InputIdentifier.InputType.Invalid)
            {
                _logger.LogWarning("Registration Failed : Invalid Username Format for {UserName}", dto.UserName);
                return ApiResponseBuilder.Fail<string>("Invalid Email or Phone Number format", 400);
            }

            var passwordCheck = PasswordValidator.Validate(dto.Password);

            if (!passwordCheck.IsValid)
            {
                _logger.LogWarning("Registration Failed : Weak Password for {UserName}", dto.UserName);
                return ApiResponseBuilder.Fail<string>(passwordCheck.ErrorMessage, 400);
            }


            string hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var Today = DateTime.Today;
            int age = Today.Year - dto.DateOfBirth.Year;

            if (dto.DateOfBirth.Month > Today.Month || (dto.DateOfBirth.Month == Today.Month && Today.Day < dto.DateOfBirth.Day))
            {
                age--;
            }
            if (age < 13)
            {
                _logger.LogWarning("Registration rejected: Underage user {UserName}", dto.UserName);
                return ApiResponseBuilder.Fail<string>("Underage!", 500);
            }

            int result = await _userRepository.RegisterUser(dto, hashedPass, age);

            if (result == -1)
            {
                _logger.LogInformation("Registration rejected : Registration attempt with existing username {UserName}", dto.UserName);

                return ApiResponseBuilder.Fail<string>("Username already exists.", 409);

            }
            if (result == 1)
            {
                _logger.LogInformation("User {UserName} registered successfully", dto.UserName);

                return ApiResponseBuilder.Success<string>(null!, "User registered successfully.");
            }

            _logger.LogError("Unexpected registration result for {UserName}",dto.UserName);

            return ApiResponseBuilder.Fail<string>("User registration failed.",500);
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginUser(LoginRequestDto dto)
        {
            var user = await _userRepository.GetUserByUsername(dto.UserName);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {UserName} not found.", dto.UserName);
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid Credentials", 401);
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            {
                _logger.LogWarning("Login failed: Invalid password for {UserName}.", dto.UserName);
                return ApiResponseBuilder.Fail<AuthResponseDto>("Invalid Credentials", 401);
            }


            await _userRepository.SaveLogin(user.Id);

            string token =  _jwt.GetJwtToken(new Credential { Id=user.Id,UserName=user.UserName,Role = user.Role});

            bool isAdmin = user.Role == "Admin";
            _logger.LogInformation("User {UserId} logged in successfully.", user.Id);


            return ApiResponseBuilder.Success<AuthResponseDto>(new AuthResponseDto { Id=user.Id,Token=token,IsAdmin=isAdmin}, "Login Successful");
        }



        public async Task<ApiResponse<Users?>> GetUserProfile(int id)
        {
            var role = _findUser.GetRole();

            _logger.LogInformation("Fetching profile for User {UserId} requested by {role}", id,role);

            var user = await _userRepository.GetUserProfile(id);

            if (user == null)
            {
                _logger.LogWarning("UserProfile retrieval failed: User {UserId} not found.", id);

                return ApiResponseBuilder.Fail<Users?>("User not found", 404);
            }


            _logger.LogWarning("UserProfile retrieval success for User {UserId} by {role}", id,role);

            return ApiResponseBuilder.Success<Users?>(user, "User profile retrieved successfully");
        }

        public async Task<ApiResponse<string>> UpdateUserProfile(int id,UpdateProfileDto updateProfileDto, string role)
        {
            _logger.LogInformation("Updating profile for User {UserId} requested by {role}", id,role);

            var Today = DateTime.Today;
            int age = Today.Year - updateProfileDto.DateOfBirth.Year;

            if (updateProfileDto.DateOfBirth.Month > Today.Month || (updateProfileDto.DateOfBirth.Month == Today.Month && Today.Day < updateProfileDto.DateOfBirth.Day))
            {
                age--;
            }

            if (age < 13)
            {
                _logger.LogWarning("Registration rejected: Underage user {id}", id);
                return ApiResponseBuilder.Fail<string>("Underage!", 500);
            }

            var result = await _userRepository.UpdateUserProfile(id, updateProfileDto,age,role);

            if(result.ResultCode == 1)
            {
                _logger.LogInformation("Profile updated successfully for User {UserId} by {role}", id,role);
                return ApiResponseBuilder.Success<string>(null!,result.Message ?? "Profile Updated Sucessfully");
            }
            if(result.ResultCode == -1)
            {
                _logger.LogWarning("Update failed: User {UserId} not found.", id);
                return ApiResponseBuilder.Fail<string>(result.Message ?? "user notfound",404);
            }

            _logger.LogError("Update failed for User {UserId}. Server Code: {ResultCode}", id, result.ResultCode);
            return ApiResponseBuilder.Fail<string>(result.Message ?? "server down", 500);
        }


        public async Task<ApiResponse<string>>ChangePassword(string oldpassword, string password)
        {
            int id = _findUser.GetId();

            if (oldpassword == password)
            {
                _logger.LogWarning("Password change failed: New password is same as old for User {UserId}", id);
                return ApiResponseBuilder.Fail<string>("New password cannot be the same as the old password.", 400);
            }
            var currentPassword = await _userRepository.GetPasswordById(id);

            if (currentPassword == null)
            {
                _logger.LogWarning("Password change failed: User {UserId} not found.", id);
                return ApiResponseBuilder.Fail<string>("User Notfound !", 404);
            }

            if (!BCrypt.Net.BCrypt.Verify(oldpassword, currentPassword))
            {
                _logger.LogWarning("Password change failed: Invalid current password for User {UserId}", id);
                return ApiResponseBuilder.Fail<string>("Invalid current password!", 401);
            }

            string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var response = await _userRepository.SavePassword(id, hashedNewPassword);


            if (!response)
            {
                _logger.LogError("Password change failed for User {UserId}: Database save error.", id);
                return ApiResponseBuilder.Fail<string>("password changing failed", 500);
            }

            _logger.LogInformation("Password changed successfully for User {UserId}", id);
            return ApiResponseBuilder.Success<string>(null!,"Password Changed successfully");

        }

        public async Task<ApiResponse<ProfileImageDto>> GetCurrentProfilePath(int id) 
        {
            var role = _findUser.GetRole();

            _logger.LogInformation("fetching profile Image for User {UserId} requested by {role}", id, role);

            var imagePath = await _userRepository.GetImagePath(id);
            _logger.LogInformation("User profile image retrived successfully");

            return ApiResponseBuilder.Success(new ProfileImageDto { ExistingImagePath = imagePath });

        }


        public async Task<ApiResponse<string>> UpdateImage(int id,IFormFile file, string role)
        {
            _logger.LogInformation("Updating profile Image for User {UserId} requested by {role}", id, role);

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();

            }

            string folder = Path.Combine(_env.WebRootPath, "uploads", "users", id.ToString());

            Directory.CreateDirectory(folder);

            var existingFiles = Directory.GetFiles(folder);

            foreach (var files in existingFiles)
            {
                File.Delete(files);
            }

            string ext = Path.GetExtension(file.FileName).ToLower();
            string fileName = "profile" + ext;

            string fullPath = Path.Combine(folder, fileName);

            using var stram = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stram);

            string relativePath = $"/uploads/users/{id}/{fileName}";


            var response = await _userRepository.UploadImage(id, bytes, relativePath,role);

            if (!response)
            {
                _logger.LogError("Database update failed for image upload. User: {UserId}", id);
                return ApiResponseBuilder.Fail<string>("Uploadig Image Failed!", 500);
            }

            _logger.LogInformation("Profile image updated successfully for User {UserId} by {role}", id,role);

            return ApiResponseBuilder.Success<string>(null!, "Profile Updated Successfully");
        }
    }
}
