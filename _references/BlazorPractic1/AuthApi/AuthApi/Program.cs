using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Services;
using AuthApi.UniversalMethods;
using AuthApi.Hubs;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ContextDb>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbString")));
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IMovieServices, MovieServices>();
builder.Services.AddScoped<IChatServices, ChatService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddSingleton<JwtGenerator>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024 * 15;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.Run();
