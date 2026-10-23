using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.DTOs;
using ReactApp1.Server.Models;

namespace ReactApp1.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();


            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();


            app.MapPost("/api/auth/register", async (RegisterRequest request, AppDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(new
                    {
                        message = "Логин и пароль обязательны."
                    });
                }

                if (request.Login.Length > 100)
                {
                    return Results.BadRequest(new
                    {
                        message = "Логин не должен превышать 100 символов."
                    });
                }

                if (request.Password.Length < 8)
                {
                    return Results.BadRequest(new
                    {
                        message = "Пароль должен содержать не менее 8 символов."
                    });
                }

                bool loginExists = await db.Users.AnyAsync(user => user.Login == request.Login);

                if (loginExists)
                {
                    return Results.Conflict(new
                    {
                        message = "Этот логин уже занят."
                    });
                }

                byte[] salt = RandomNumberGenerator.GetBytes(16);

                byte[] hash = KeyDerivation.Pbkdf2(
                    password: request.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 600_000,
                    numBytesRequested: 32);

                var user = new User
                {
                    Login = request.Login,
                    PasswordSalt = Convert.ToBase64String(salt),
                    PasswordHash = Convert.ToBase64String(hash)
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = "Регистрация прошла успешно."
                });
            });

            

            app.MapPost("/api/auth/login", async (
                LoginRequest request,
                AppDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(request.Login) ||
                    string.IsNullOrEmpty(request.Password))
                {
                    return Results.BadRequest(new
                    {
                        message = "Введите логин и пароль."
                    });
                }

                var user = await db.Users.FirstOrDefaultAsync(
                    user => user.Login == request.Login);

                if (user == null)
                {
                    return Results.Json(
                        new { message = "Неверный логин или пароль." },
                        statusCode: 401);
                }

                byte[] salt = Convert.FromBase64String(user.PasswordSalt);

                byte[] storedHash = Convert.FromBase64String(user.PasswordHash);

                byte[] enteredHash = KeyDerivation.Pbkdf2(
                    password: request.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 600_000,
                    numBytesRequested: 32);

                bool passwordIsCorrect =
                    CryptographicOperations.FixedTimeEquals(
                        storedHash,
                        enteredHash);

                if (!passwordIsCorrect)
                {
                    return Results.Json(
                        new { message = "Неверный логин или пароль." },
                        statusCode: 401);
                }

                return Results.Ok(new
                {
                    message = "Вход выполнен успешно."
                });
            });


            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();

        }
    }
}
