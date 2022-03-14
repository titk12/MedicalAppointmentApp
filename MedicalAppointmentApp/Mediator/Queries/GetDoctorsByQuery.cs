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
using MedicalAppointmentApp.Data.Models;

namespace MedicalAppointmentApp.Mediator.Queries
{
    public class GetDoctorsByQuery
    {
        public class Query : IRequest<List<GetDoctorsWithNextAppointments>>
        {
            public Query(string stringQuery, int numOfAppointmentsToGet)
            {
                StringQuery = stringQuery;
                NumOfAppointmentsToGet = numOfAppointmentsToGet;
            }
            public string StringQuery { get; }
            public int NumOfAppointmentsToGet { get; }
        }

        public class Handler : IRequestHandler<Query, List<GetDoctorsWithNextAppointments>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<GetDoctorsWithNextAppointments>> Handle(Query request, CancellationToken cancellationToken)
            {
                var doctors = await _context.Doctors
                    .Include(doctor => doctor.MedicalSpeciality)
                    .Include(doctor => doctor.Appointments)
                    .Include(doctor => doctor.Schedules)
                        .ThenInclude(schedule => schedule.ScheduleDetails)
                    .Include(doctor => doctor.Schedules)
                        .ThenInclude(schedule => schedule.Institution)
                    .Where(doctor => doctor.FirstName.Contains(request.StringQuery)
                    || doctor.LastName.Contains(request.StringQuery)
                    || (doctor.FirstName + " " + doctor.LastName).Contains(request.StringQuery)
                    || doctor.MedicalSpeciality.Name.Contains(request.StringQuery)
                    || doctor.Schedules.Any(c => c.Institution.Name.Contains(request.StringQuery)))
                    .ToListAsync();

                //getting sorted doctors
                var doctorsWithNextAppointments = GetDoctorWithNextAppointment(doctors, request.NumOfAppointmentsToGet);

                return doctorsWithNextAppointments;
            }

            private List<GetDoctorsWithNextAppointments> GetDoctorWithNextAppointment(List<Doctor> doctors, int numOfFreeAppointmentSpaces) 
            {
                var doctorsWithNextAppointments = new List<GetDoctorsWithNextAppointments>();
                var currentDate = DateTime.Today;

                foreach (var doctor in doctors)
                {
                    var upcomingFreeAppointmentSpaces = new List<DateTime?>();

                    //getting all valid schedule details
                    var filteredScheduleDetails = doctor.Schedules
                        .Where(s => (currentDate >= s.StartDate && currentDate <= s.EndDate))
                        .SelectMany(s => s.ScheduleDetails)
                        .OrderByDescending(s => s.Schedule.EndDate);

                    if (filteredScheduleDetails != null) 
                    {
                        //taking first end date from ordered list 
                        var maxEndDateTime = filteredScheduleDetails.Select(s => s.Schedule.EndDate).First();

                        //iterate from current date to end date time of all filtered schedules 
                        for (var day = currentDate; day.Date <= maxEndDateTime; day = day.AddDays(1))
                        {
                            if (filteredScheduleDetails.Any(s => s.Day == day.DayOfWeek && s.Schedule.EndDate.Date >= day.Date))
                            {
                                var filteredScheduleDetail = filteredScheduleDetails
                                    .Where(s => s.Day == day.DayOfWeek && s.Schedule.EndDate.Date >= day.Date)
                                    .First();

                                TimeSpan startTime = TimeSpan.Parse(filteredScheduleDetail.StartDateTime);
                                TimeSpan endTime = TimeSpan.Parse(filteredScheduleDetail.EndDateTime);

                                var startDateTime = day.Date + startTime;
                                var startEndTime = day.Date + endTime;

                                for (var currentStartDT = startDateTime; currentStartDT < startEndTime; currentStartDT = currentStartDT.AddMinutes(30))
                                {
                                    var appointment = doctor.Appointments
                                        .Where(appointment => appointment.StartDateTime == currentStartDT)
                                        .FirstOrDefault();

                                    if (appointment == null)
                                    {
                                        upcomingFreeAppointmentSpaces.Add(currentStartDT);
                                        if (upcomingFreeAppointmentSpaces.Count() >= numOfFreeAppointmentSpaces) {
                                            break;
                                        }
                                    }
                                }

                                if (upcomingFreeAppointmentSpaces.Count() >= numOfFreeAppointmentSpaces) 
                                {
                                    break;
                                }
                            }
                        }
                    }

                    var doctorWithNextAppointments = new GetDoctorsWithNextAppointments
                    {
                        Doctor = doctor,
                        NextFreeAppointmentDates = upcomingFreeAppointmentSpaces
                    };

                    doctorsWithNextAppointments.Add(doctorWithNextAppointments);
                }

                //order by closest free appointment date
                doctorsWithNextAppointments = doctorsWithNextAppointments
                    .OrderByDescending(s => s.NextFreeAppointmentDates.FirstOrDefault().HasValue)
                    .OrderBy(s => s.NextFreeAppointmentDates.FirstOrDefault())
                    .ToList();

                return doctorsWithNextAppointments;
            }
        }
    }
}
