using BlazorPractice1;
using BlazorPractice1.ApiRequests;
using BlazorPractice1.ApiRequests.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalStorageServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5126/") });
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiRequest>();

await builder.Build().RunAsync();
