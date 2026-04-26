namespace BlazorPractice1.ApiRequests.Model
{
    public class Auth
    {
        public class AuthorizeResponse
        {
            public bool status { get; set; }
            public string token { get; set; }
            public UserResponse user { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegistrationRequest
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string? Description { get; set; }
            public string Password { get; set; }
        }

    }
}
