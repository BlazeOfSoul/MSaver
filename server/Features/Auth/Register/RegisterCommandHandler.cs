using MediatR;
using Microsoft.AspNetCore.Identity;

using MSaver.Api.Models;
using MSaver.Api.Repositories;
using MSaver.Api.Services;

namespace MSaver.Api.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new Exception("ѕользователь с таким email уже существует");

        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = request.Username,
            Email = request.Email
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);

        return new RegisterResponse(user.Id, user.Username, user.Email, token);
    }

}
