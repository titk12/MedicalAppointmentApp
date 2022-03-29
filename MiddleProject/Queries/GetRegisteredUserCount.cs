﻿using MediatR;
using DAL.Data.Models;
using MiddleProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiddleProject.Queries
{
    public static class GetRegisteredUserCount
    {
        public class Query : IRequest<int>
        {
        }

        public class Handler : IRequestHandler<Query, int>
        {
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<int> Handle(Query request, CancellationToken cancellationToken)
            {
                var userRolesViewModel = new List<UserRolesViewModel>();

                var userCount = await _userManager.Users
                    .CountAsync();

                return userCount;
            }
        }
    }
}