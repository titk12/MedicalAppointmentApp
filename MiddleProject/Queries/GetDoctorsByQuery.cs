﻿using AutoMapper;
using DAL.Data.Models;
using DAL.Repositories.Interfaces;
using MediatR;
using MiddleProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiddleProject.Queries
{
    public class GetDoctorsByQuery
    {
        public class Query : IRequest<List<GetDoctorsWithNextAppointments>>
        {
            public Query(string stringQuery, int numOfAppointmentsToGet, int page, int pageSize)
            {
                StringQuery = stringQuery;
                NumOfAppointmentsToGet = numOfAppointmentsToGet;
                Page = page;
                PageSize = pageSize;
            }
            public string StringQuery { get; }
            public int NumOfAppointmentsToGet { get; }
            public int Page { get; }
            public int PageSize { get; }
        }

        public class Handler : IRequestHandler<Query, List<GetDoctorsWithNextAppointments>>
        {
            private readonly IDoctorRepository _doctorRepository;
            private readonly IMapper _mapper;

            public Handler(IDoctorRepository doctorRepository, IMapper mapper)
            {
                _doctorRepository = doctorRepository;
                _mapper = mapper;
            }

            public async Task<List<GetDoctorsWithNextAppointments>> Handle(Query request, CancellationToken cancellationToken)
            {
                var doctors = await _doctorRepository.GetByQueryAsync(request.StringQuery, request.Page, request.PageSize);

                //getting sorted doctors
                var doctorsWithNextAppointments = GetDoctorWithNextAppointment(doctors, request.NumOfAppointmentsToGet);

                return doctorsWithNextAppointments;
            }

            private List<GetDoctorsWithNextAppointments> GetDoctorWithNextAppointment(List<Doctor> doctors, int numOfFreeAppointmentSpaces)
            {
                var doctorsWithNextAppointments = new List<GetDoctorsWithNextAppointments>();
                //adding one day to start checking from tomorrow
                var currentDate = DateTime.Today.AddDays(1);

                foreach (var doctor in doctors)
                {
                    var upcomingFreeAppointmentSpaces = new List<DateTime?>();

                    //getting all valid schedule details
                    var filteredScheduleDetails = doctor.Schedules
                        .Where(s => currentDate <= s.EndDate)
                        .SelectMany(s => s.ScheduleDetails)
                        .OrderByDescending(s => s.Schedule.EndDate);

                    if (filteredScheduleDetails.Any())
                    {
                        //taking first end date from ordered list 
                        var maxEndDateTime = filteredScheduleDetails.Select(s => s.Schedule.EndDate).First();

                        //iterate from current date to end date time of all filtered schedules 
                        for (var day = currentDate; day.Date <= maxEndDateTime; day = day.AddDays(1))
                        {
                            if (filteredScheduleDetails.Any(s => s.Day == day.DayOfWeek && s.Schedule.EndDate.Date >= day.Date
                                && s.Schedule.StartDate.Date <= day.Date))
                            {
                                var filteredScheduleDetail = filteredScheduleDetails
                                    .Where(s => s.Day == day.DayOfWeek && s.Schedule.EndDate.Date >= day.Date
                                        && s.Schedule.StartDate.Date <= day.Date)
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
                                        if (upcomingFreeAppointmentSpaces.Count() >= numOfFreeAppointmentSpaces)
                                        {
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

                return doctorsWithNextAppointments;
            }
        }
    }
}
