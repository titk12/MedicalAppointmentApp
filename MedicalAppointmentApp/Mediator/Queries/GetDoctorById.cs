﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MediatR;
using MedicalAppointmentApp.Data;
using MedicalAppointmentApp.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentApp.Mediator.Queries
{
    public class GetDoctorById
    {
        public class Query : IRequest<GetDoctorModel>
        {
            public Query(int id)
            {
                Id = id;
            }
            public int Id { get; }
        }

        public class Handler : IRequestHandler<Query, GetDoctorModel>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<GetDoctorModel> Handle(Query request, CancellationToken cancellationToken)
            {

                var doctor = await _context.Doctors.Include(doctor => doctor.MedicalSpeciality).Include(doctor => doctor.Schedules)
                    .ThenInclude(schedules => schedules.Institution)
                    .Where(doctor => doctor.DoctorId.Equals(request.Id))
                    .FirstOrDefaultAsync();

                var doctorViewModel = _mapper.Map<GetDoctorModel>(doctor);

                return doctorViewModel;
            }
        }
    }
}
