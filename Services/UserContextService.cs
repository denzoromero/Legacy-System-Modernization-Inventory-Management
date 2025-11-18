using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly ILogger<UserContextService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(ILogger<UserContextService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserId()
        {

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                _logger.LogError("UserId is missing from claims.");
                throw new UserContextException("UserId is missing from claims.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogError("UserId is an invalid format.");
                throw new UserContextException("UserId is an invalid format.");
            }

            return userId;
        }

        public UserClaimModel GetUserClaimData()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                _logger.LogError("UserId is missing from claims.");
                throw new UserContextException("UserId is missing from claims.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogError("UserId is an invalid format.");
                throw new UserContextException("UserId is an invalid format.");
            }

            string? UserChapa = _httpContextAccessor.HttpContext?.User.FindFirst("Chapa")?.Value;
            if (string.IsNullOrEmpty(UserChapa))
            {
                _logger.LogError("UserChapa is missing from claims.");
                throw new UserContextException("UserChapa is missing from claims.");
            }

            string? UserName = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(UserName))
            {
                _logger.LogError("UserName is missing from claims.");
                throw new UserContextException("UserName is missing from claims.");
            }

            UserClaimModel userClaim = new UserClaimModel()
            {
                Id = userId,
                Chapa = UserChapa,
                Nome = UserName,
            };

            return userClaim;

        }

    }




}
