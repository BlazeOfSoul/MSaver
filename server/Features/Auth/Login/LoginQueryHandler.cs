using MediatR;

using Microsoft.AspNetCore.Identity;

using MSaver.Api.Models;
using MSaver.Api.Repositories;
using MSaver.Api.Services;

namespace MSaver.Api.Features.Auth.Login;

public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("Invalid credentials");

        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Invalid credentials");

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);

        return new LoginResponse(user.Id, user.Username, user.Email, token);
    }

}
