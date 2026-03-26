using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection.PortableExecutable;
using Microsoft.Data.SqlClient;
using MyApp.Models;
using MyWebApp.Models;
using MyWebAppApi.DTOs;
using MyWebAppApi.Repository.Interfaces;

namespace MyWebAppApi.Repository
{
    public class UserRepository : BaseRepository,IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> RegisterUser(RegisterRequestDto dto,string hashedpass,int age)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@username",dto.UserName),
                new SqlParameter("@hashed_password",hashedpass),
                new SqlParameter("@first_name",dto.FirstName),
                new SqlParameter("@last_name",dto.LastName),
                new SqlParameter("@display_name",string.IsNullOrEmpty(dto.DisplayName) ? (object)DBNull.Value : dto.DisplayName),
                new SqlParameter("@date_of_birth",dto.DateOfBirth),
                new SqlParameter("@age",age),
                new SqlParameter("@gender",dto.Gender),
                new SqlParameter("@address",dto.Address),
                new SqlParameter("@city",dto.City),
                new SqlParameter("@state",dto.State),
                new SqlParameter("@zipcode",dto.ZipCode),
                new SqlParameter("@phone",dto.Phone),
                new SqlParameter("@mobile", string.IsNullOrEmpty(dto.Mobile) ? (object)DBNull.Value : dto.Phone)
            };

            SqlDataReader reader = await ExecuteSp("Auth.RegisterUser", parameters);

            if(await reader.ReadAsync())
            {
                int ResultCode = Convert.ToInt32(reader["ResultCode"]);
                string Message = Convert.ToString(reader["Message"]) ?? "";

                return ResultCode;
            }

            return 0;

        }

        public async Task<bool> LockedOrNot(string username)
        {
            const string sql = @"
        SELECT 1 
        FROM Auth.Credentials 
        WHERE Username = @username 
          AND  IsDelete = 1;";

            await using var conn = GetConnection();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result != null;
        }


        public async Task<Credential?> GetUserByUsername(string username)
        {
            string sql = "SELECT Id,Username,HashedPassword,Role,LoginAttempts FROM Auth.Credentials WHERE Username = @username AND (IsActive = 1 AND IsDelete = 0 );";
            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);

            Credential credential = new Credential();
            cmd.Parameters.AddWithValue("@username", username);

            await conn.OpenAsync();



            await using var read = await cmd.ExecuteReaderAsync();

            if (!await read.ReadAsync()) return null;
         
                credential.Id = Convert.ToInt32(read["Id"]);
                credential.UserName = Convert.ToString(read["Username"]) ?? "";
                credential.HashedPassword = Convert.ToString(read["HashedPassword"]) ?? "";
                credential.Role = Convert.ToString(read["Role"]);

                int loginAttemptsOrdinal = read.GetOrdinal("LoginAttempts");

                credential.LoginAttempts = !read.IsDBNull(loginAttemptsOrdinal)
                                 ? Convert.ToInt32(read.GetValue(loginAttemptsOrdinal))
                                 : 0;
            return credential;

        }

        public async Task SaveLogin(int id)
        {
           await using var conn = GetConnection();

            DateTime now = DateTime.Now;

            string sql = "UPDATE Auth.Credentials SET LoginAt = @now ,LoginAttempts = 0  WHERE Id = @id;";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();

                cmd.Parameters.AddWithValue("@now", now);
                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();

            }
        }

        public async Task<Users?> GetUserProfile(int id)
        {
            await using var conn = GetConnection();

            string sql = "SELECT * FROM App.Users WHERE UserId = @id;";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();

                cmd.Parameters.AddWithValue("@id", id);

                var read = await cmd.ExecuteReaderAsync();

                if (!await read.ReadAsync()) return null;

                return new Users
                {
                    Id = id,
                    FirstName = Convert.ToString(read["FirstName"]),
                    LastName = Convert.ToString(read["LastName"]),
                    DisplayName = read["DisplayName"] == DBNull.Value ? null : Convert.ToString(read["DisplayName"]),
                    DateOfBirth = Convert.ToDateTime(read["DOB"]),
                    Gender = Convert.ToBoolean(read["Gender"]),
                    Age = Convert.ToInt32(read["Age"]),
                    Address = Convert.ToString(read["Address"]),
                    City = read["City"] == DBNull.Value ? null : Convert.ToString(read["City"]),
                    State = read["State"] == DBNull.Value ? null : Convert.ToString(read["State"]),
                    ZipCode = Convert.ToInt32(read["Zipcode"]),
                    Phone = Convert.ToString(read["Phone"]),
                    Mobile = read["Mobile"] == DBNull.Value ? null : Convert.ToString(read["Mobile"]),
                    ProfileImagePath = read["ProfileImagePath"] == DBNull.Value ? null : Convert.ToString(read["ProfileImagePath"])
                };

            }

        }
        public async Task<DbResponse> UpdateUserProfile(int id,UpdateProfileDto updateProfile,int age, string role)
        {
            var paramiters = new SqlParameter[]
            {
                new SqlParameter("@id",id),
                new SqlParameter("@first_name",updateProfile.FirstName),
                new SqlParameter("@last_name",updateProfile.LastName),
                new SqlParameter("@display_name",string.IsNullOrEmpty(updateProfile.DisplayName) ? (object)DBNull.Value : updateProfile.DisplayName),
                new SqlParameter("@date_of_birth",updateProfile.DateOfBirth),
                new SqlParameter("@age",age),
                new SqlParameter("@gender",updateProfile.Gender),
                new SqlParameter("@address",updateProfile.Address),
                new SqlParameter("@city",string.IsNullOrEmpty(updateProfile.City) ? (object)DBNull.Value : updateProfile.City),
                new SqlParameter("@state",string.IsNullOrEmpty(updateProfile.State) ? (object)DBNull.Value : updateProfile.State),
                new SqlParameter("@zipcode",updateProfile.ZipCode),
                new SqlParameter("@phone",updateProfile.Phone),
                new SqlParameter("@mobile",string.IsNullOrEmpty(updateProfile.Mobile) ? (object)DBNull.Value : updateProfile.Mobile),
                new SqlParameter("@role",role)

            };

            var read = await ExecuteSp("App.ProfileUpdate",paramiters);

            await read.ReadAsync();

            return new DbResponse
            {
                ResultCode = Convert.ToInt32(read["ResultCode"]),
                Message = Convert.ToString(read["Message"]) ?? ""
            };
               
        }

        public async Task<string?> GetPasswordById(int id)
        {
            string sql = "SELECT HashedPassword FROM Auth.Credentials WHERE Id = @id;";
            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();


            await using var read = await cmd.ExecuteReaderAsync();

            if (!await read.ReadAsync()) return null;

            return Convert.ToString(read["HashedPassword"]);

        }

        public async Task<bool> SavePassword(int id, string password)
        {

            string sql =
                "Update Auth.Credentials  SET HashedPassword = @password , PasswordChangedAt = @now WHERE Id = @id";
            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);

            await conn.OpenAsync();


            var result = await cmd.ExecuteNonQueryAsync();


            return result > 0;

        }

        public async Task<bool> UploadImage(int id, byte[] imageBytes, string imagePath, string role)
        {
                string sql = "UPDATE App.Users SET ProfileImage = @img, ProfileImagePath = @path, ProfileImageUpdatedAt = GETDATE(),ProfileImageUpdatedBy = @role WHERE UserId = @id;";
                await using var conn = GetConnection();
                await using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = imageBytes;
                cmd.Parameters.AddWithValue("@path", imagePath);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@role", role);


            await conn.OpenAsync();
                var result = await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"dbresult : {result}");
                return result > 0;
        }
        public async Task<IEnumerable<UsersViewDto>> GetAllUsers()
        {
            string sql =
                "SELECT c.Id,c.UserName,c.Role ,c.IsActive,u.FirstName,u.LastName,u.Phone,u.ProfileImagePath FROM App.Users as u JOIN Auth.Credentials as c on u.UserId = c.Id WHERE c.Role != 'Admin' AND IsDelete = 0";

            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);

            await conn.OpenAsync();


            await using var read = await cmd.ExecuteReaderAsync();

            List<UsersViewDto> users = new List<UsersViewDto>();

            while (await read.ReadAsync())
            {
                users.Add(new UsersViewDto
                {
                    Id = Convert.ToInt32(read["Id"]),
                    FirstName = Convert.ToString(read["FirstName"]),
                    UserName = Convert.ToString(read["UserName"]),
                    LastName = Convert.ToString(read["LastName"]),
                    Phone = Convert.ToString(read["Phone"]),
                    Role = Convert.ToString(read["Role"]),
                    IsActive = Convert.ToBoolean(read["IsActive"]),
                    ProfileImage = read["ProfileImagePath"] == DBNull.Value ? null : Convert.ToString(read["ProfileImagePath"])

                });
            }

            return users;
        }

        public async Task<string?> GetImagePath(int id)
        {
            string sql = "SELECT ProfileImagePath FROM app.Users WHERE UserId = @id;";

            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();


            await using var read = await cmd.ExecuteReaderAsync();

            if (!await read.ReadAsync()) return null;

            return read["ProfileImagePath"] == DBNull.Value ? null : Convert.ToString(read["ProfileImagePath"]);
        }

        public async Task<DbResponse?> DeleteUser(int id)
        {
            string sql = "Auth.DeleteUser";

            await using var conn = GetConnection();

            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();


            await using var read = await cmd.ExecuteReaderAsync();

            if (!await read.ReadAsync()) return null;

            return new DbResponse
            {
                ResultCode = Convert.ToInt32(read["ResultCode"]),
                Message = Convert.ToString(read["Message"])

            };


        }

        public async Task<bool> LockUser(int id)
        {
            string sql = "UPDATE Auth.Credentials SET IsActive = 0 , LockedAt = GETDATE() WHERE Id = @id";

            await using var conn = GetConnection();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);


            await conn.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();

            return result > 0;

        }

        public async Task SaveLoginAttempt(int id)
        {
            const string sql = @"UPDATE Auth.Credentials SET LoginAttempts = LoginAttempts + 1 WHERE Id = @id;";

            await using var conn = GetConnection();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ToaggleStatus(int id)
        {
            const string sql = @"
            UPDATE Auth.Credentials 
            SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END 
            WHERE Id = @id;";

            await using var conn = GetConnection();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            int result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

    }
}