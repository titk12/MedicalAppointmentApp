﻿using DAL.Repositories.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MiddleProject.Queries
{
    public static class GetInstitutionCount
    {
        public class Query : IRequest<int>
        {
        }

        public class Handler : IRequestHandler<Query, int>
        {
            private readonly IInstitutionRepository _institutionRepository;

            public Handler(IInstitutionRepository institutionRepository)
            {
                _institutionRepository = institutionRepository;
            }

            public async Task<int> Handle(Query request, CancellationToken cancellationToken)
            {
                var institutionCount = await _institutionRepository.GetCountAsync();


                return institutionCount;
            }
        }
    }
}
