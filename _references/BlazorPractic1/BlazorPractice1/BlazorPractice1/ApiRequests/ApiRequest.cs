using BlazorPractice1.ApiRequests.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using static BlazorPractice1.ApiRequests.Model.Auth;
using static BlazorPractice1.ApiRequests.Model.Common;

namespace BlazorPractice1.ApiRequests
{
    public class ApiRequest
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiRequest> _logger;
        public ApiRequest(HttpClient httpClient, ILogger<ApiRequest> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);
        }

        public async Task<AuthorizeResponse> AuthorizeResponse(LoginRequest request)
        {
            var url = "Authorize";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<AuthorizeResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userAdd ?? new AuthorizeResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new AuthorizeResponse();
            }
        }

        public async Task<StatusResponse> RegistrationAsync(RegistrationRequest request)
        {
            var url = "Registration";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<StatusResponse>(content);

                return userAdd ?? new StatusResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new StatusResponse();
            }
        }


        public async Task<UsersListResponse> GetAllUsersAsyncResponse(string token)
        {
            var url = "GetAllUsers";
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Ответ от сервера пуст.");
                    return new UsersListResponse();
                }

                var userlist = JsonSerializer.Deserialize<UsersListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }); 

                return userlist ?? new UsersListResponse();
            }
            catch(Exception ex)
            {
                    _logger.LogError(ex, "Ошибка при запросе");
                    return new UsersListResponse();
            }
        }

        public async Task<ChatUsersDirectoryResponse> GetUsersForChatAsyncResponse(string token)
        {
            var url = "GetUsersForChat";
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return new ChatUsersDirectoryResponse();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<ChatUsersDirectoryResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ChatUsersDirectoryResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersForChat");
                return new ChatUsersDirectoryResponse();
            }
        }

        public async Task<CreateUserResponse> CreateUserAsyncResponse(CreateUserRequest request, string token)
        {
            var url = "CreateNewUser";

            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<CreateUserResponse>(content);

                return userAdd ?? new CreateUserResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new CreateUserResponse();
            }
        }



        public async Task<UpdateUserResponse> UpdateUserAsyncResponse(UpdateUserRequest request, string token)
        {
            var url = "UpdateUser";
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.PutAsJsonAsync(url, request);
                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var userUpdate = JsonSerializer.Deserialize<UpdateUserResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userUpdate ?? new UpdateUserResponse();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new UpdateUserResponse();
            }
        }

        public async Task<StatusResponse?> DeleteUserAsync(int id, string token)
        {
            var url = $"/DeleteUsers/?user_id={id}";

            try
            {
                SetAuthorizationHeader(token);
                var resp = await _httpClient.DeleteAsync(url);
                var content = await resp.Content.ReadAsStringAsync();
                resp.EnsureSuccessStatusCode();
                var userDelete = JsonSerializer.Deserialize<StatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userDelete ?? new StatusResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new StatusResponse();
            }
            
        }

        public async Task<UpdateProfileResponse?> UpdateProfileAsyncResponse(UpdateProfileRequest request, string token)
        {
            var url = "Profile";
            SetAuthorizationHeader(token);
            try
            {
                var resp = await _httpClient.PutAsJsonAsync(url, request);
                resp.EnsureSuccessStatusCode();
                var content = await resp.Content.ReadAsStringAsync();
                return await resp.Content.ReadFromJsonAsync<UpdateProfileResponse>();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new UpdateProfileResponse();
            }
            
        }

        public async Task<CreateMovieResponse> CreateMovieAsyncResponse(CreateMovieRequest request, string token)
        {
            var url = "CreateMovie";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<CreateMovieResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<UpdateMovieResponse> UpdateMovieAsyncResponse(UpdateMovieRequest request, string token)
        {
            var url = "UpdateMovie";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<UpdateMovieResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<StatusResponse> DeleteMovieAsyncResponse(int id, string token)
        {
            var url = $"/DeleteMovie/?id={id}";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<StatusResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<MoviesListResponse> GetAllMovies(string token)
        {
            var url = "GetAllMovies";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MoviesListResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<GenresListResponse> GetAllGenres(string token)
        {
            var url = "GetAllGenres";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GenresListResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<GetMovieByIdResponse> GetMovieByIdResponse(int id, string token)
        {
            var url = $"/GetMovieById/?id={id}";
            SetAuthorizationHeader(token);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GetMovieByIdResponse>();
            }
            catch(Exception ex)
            {
                Console.Write($"Ошибка при запросе: {ex.Message}");
                return null;
            }
        }

        public async Task<UploadImageResponse> UploadMovieImageAsync(IBrowserFile file, string token)
        {
            var url = "/Image/MovieImage";
            SetAuthorizationHeader(token);
            try
            {
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(file.OpenReadStream(10 * 1024 * 1024));
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                content.Add(streamContent, "file", file.Name);
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<UploadImageResponse>();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка прии запросе: {ex.Message}");
                return null;
            }
        }
    }
}
