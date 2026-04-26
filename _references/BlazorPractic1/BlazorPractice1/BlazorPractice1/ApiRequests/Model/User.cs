namespace BlazorPractice1.ApiRequests.Model
{
    public class UserResponse
    {
        public int id_User { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Role_Id { get; set; }
        public string? Role { get; set; }
    }
    public class Role
    {
        public int id_Role { get; set; }
        public string Name { get; set; }
    }
    
    public class UsersListResponse
    {
        public bool status { get; set; }
        public List<UserResponse> users { get; set; }
        public string? message { get; set; }
    }

    
    
    public class CreateUserResponse
    {
        public bool status { get; set; }
        public UserResponse user { get; set; }
    }
    public class UpdateUserResponse
    {
        public bool status { get; set; }
        public UserResponse user { get; set; }
    }
    public class UpdateProfileResponse
    {
        public bool status { get; set; }
        public UserResponse user { get; set; }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Password { get; set; }
        public int Role_Id { get; set; }
    }

    public class UpdateUserRequest
    {
        public int id_User { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Password { get; set; }
        public int Role_id { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Password { get; set; }
    }
}
