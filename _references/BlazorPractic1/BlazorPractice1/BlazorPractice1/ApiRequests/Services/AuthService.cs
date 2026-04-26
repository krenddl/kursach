using BlazorPractice1.ApiRequests.Model;
using Microsoft.JSInterop;
using static BlazorPractice1.ApiRequests.Model.Auth;
using System.Text.Json;

namespace BlazorPractice1.ApiRequests.Services
{
    public class AuthService
    {
        private readonly ILocalStorageService _localStorageServcie;
        private readonly ApiRequest _api;
        public string? Token { get; private set; }
        public UserResponse? CurrentUser { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

        public AuthService(ApiRequest api, ILocalStorageService localStorageService)
        {
            _api = api;
            _localStorageServcie = localStorageService;
        }

        public async Task<bool> LoginAsync (LoginRequest request)
        {
            var result = await _api.AuthorizeResponse(request);
            
            if(result == null || !result.status || string.IsNullOrWhiteSpace(result.token))
            {
                return false;
            }

            Token = result.token;
            CurrentUser = result.user;

            await _localStorageServcie.SetItemAsync("token", Token);
            var userJosn = JsonSerializer.Serialize(CurrentUser);
            await _localStorageServcie.SetItemAsync("user", userJosn);

            return true;
        }

        public async Task<bool> RegistrationAsync(RegistrationRequest request)
        {
            var result = await _api.RegistrationAsync(request);

            if(result == null || !result.status)
            {
                return false;
            }

            return true;
        }

        public async Task LoadSessionAsync()
        {
            try 
            {
                Token = await _localStorageServcie.GetItemAsync("token");
                var userJson = await _localStorageServcie.GetItemAsync("user");

                if(!string.IsNullOrWhiteSpace(userJson))
                {
                    CurrentUser = JsonSerializer.Deserialize<UserResponse>(userJson);
                }
                else
                {
                    CurrentUser = null;
                }

            }
            catch
            {
                Token = null;
                CurrentUser = null;

                await _localStorageServcie.RemoveItemAsync("token");
                await _localStorageServcie.RemoveItemAsync("user");
            }

        }

        public async Task Logout()
        {
            Token = null;
            CurrentUser = null;

            await _localStorageServcie.RemoveItemAsync("token");
            await _localStorageServcie.RemoveItemAsync("user");
        }
    }
}
