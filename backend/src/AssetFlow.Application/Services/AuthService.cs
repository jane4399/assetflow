using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common.Exceptions;
using AssetFlow.Application.Contracts.Auth;
using AssetFlow.Application.Mapping;
using AssetFlow.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = Normalize(request.Email);
        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException($"An account with e-mail '{email}' already exists.");
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Technician
        };

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registered new user {UserId} with role {Role}", user.Id, user.Role);
        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = Normalize(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        // Verify even when the user is missing to keep the response time uniform
        // and avoid leaking which e-mails are registered.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            throw new UnauthorizedException("Invalid e-mail or password.");
        }

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = _tokenService.CreateToken(user);
        return new AuthResponse(token.AccessToken, token.ExpiresAtUtc, user.ToDto());
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
