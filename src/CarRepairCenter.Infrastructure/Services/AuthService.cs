using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null) 
            return ServiceResult<AuthResponseDto>.Failure("بيانات الدخول غير صحيحة");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded) 
            return ServiceResult<AuthResponseDto>.Failure("بيانات الدخول غير صحيحة");

        var roles = await _userManager.GetRolesAsync(user);
        
        // 1. Get JWT secret and validate it
        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
            ?? _config["Jwt:Key"];
            
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured. Please set the JWT_SECRET_KEY environment variable or Jwt:Key configuration.");
        }
        
        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Key must be at least 32 characters (256 bits) long.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // 2. Read expiration (default to 7 days if not configured)
        var expiryDaysStr = _config["Jwt:ExpiryDays"];
        var expiryDays = double.TryParse(expiryDaysStr, out var d) ? d : 7.0;
        var expires = DateTime.UtcNow.AddDays(expiryDays);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "MakanakService",
            audience: _config["Jwt:Issuer"] ?? "MakanakService",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto(
            Token: tokenString,
            Email: user.Email!,
            FullName: user.FullName,
            Role: roles.FirstOrDefault() ?? "Receptionist",
            Expiration: expires
        ));
    }
}
