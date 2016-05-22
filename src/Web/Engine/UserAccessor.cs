﻿using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Web.Models;

namespace Web.Engine
{
    public interface IUserAccessor
    {
        ClaimsPrincipal User { get; }
        string UserId { get; }
    }

    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAccessor(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager)
        {
            _accessor = accessor;
            _userManager = userManager;
        }

        public ClaimsPrincipal User => _accessor.HttpContext.User;
        public string UserId => _userManager.GetUserId(_accessor.HttpContext.User);
    }
}