﻿using AutoMapper;
using MediatR;
using MedicalAppointmentApp.Data;
using MedicalAppointmentApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Mediator.Queries
{
    public class GetAppointmentCountByUserId
    {
        public class Query : IRequest<int>
        {
            public Query(string id)
            {
                Id = id;
            }
            public string Id { get; }
        }

        public class Handler : IRequestHandler<Query, int>
        {
            private readonly ApplicationDbContext _context;

            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<int> Handle(Query request, CancellationToken cancellationToken)
            {
                var appointmentCount = await _context.Appointments
                    .Where(a => a.ApplicationUserId.Equals(request.Id))
                    .CountAsync();


                return appointmentCount;
            }
        }
    }
}
