﻿using DAL.Repositories.Interfaces;
using MediatR;
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
            private readonly IUserRepository _userRepository;

            public Handler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<int> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _userRepository.GetUsersCountAsync();
            }
        }
    }
}
