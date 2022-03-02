﻿using AutoMapper;
using MediatR;
using MedicalAppointmentApp.Data;
using MedicalAppointmentApp.Data.Models;
using MedicalAppointmentApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Queries
{
    public static class GetInstitutions
    {
        public class Query : IRequest<List<GetInstitutionModel>>
        {
        }

        public class Handler : IRequestHandler<Query, List<GetInstitutionModel>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<GetInstitutionModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                var institutionsViewModel = new List<GetInstitutionModel>();
                var institutions = await _context.Institutions.Include(institution => institution.Schedules)
                    .ThenInclude(schedule => schedule.Doctor)
                    .ToListAsync();

                foreach (var institution in institutions) {
                    var viewModel = _mapper.Map<GetInstitutionModel>(institution);
                    institutionsViewModel.Add(viewModel);
                }

                return institutionsViewModel;
            }
        }

        public class Response
        {
            public List<GetInstitutionModel> Institutions { get; set; }
        }
    }
}
