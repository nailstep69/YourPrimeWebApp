using Microsoft.EntityFrameworkCore;
using YourPrime.Auth;
using YourPrime.Data;
using YourPrime.DTOs.Auth;
using YourPrime.Entities;
using YourPrime.Interfaces;

namespace YourPrime.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext db, JwtTokenGenerator jwtTokenGenerator)
    {
        _db = db;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _db.Users.AnyAsync(x => x.Email == request.Email);
        if (emailExists)
            throw new Exception("Email already exists");

        var usernameExists = await _db.Users.AnyAsync(x => x.Username == request.Username);
        if (usernameExists)
            throw new Exception("Username already exists");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            Age = request.Age,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            // UserId = user.Id,
            // Email = user.Email,
            // Username = user.Username,
            // Role = user.Role
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            throw new Exception("Invalid email or password");

        var passwordCorrect = PasswordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordCorrect)
            throw new Exception("Invalid email or password");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            // UserId = user.Id,
            // Email = user.Email,
            // Username = user.Username,
            // Role = user.Role
        };
    }
}