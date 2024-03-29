﻿using AutoMapper;
using DAL.Data.Models;

namespace MiddleProject.Models.MapperProfiles
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<MedicalSpeciality, GetMedicalSpecialtyModel>();
            CreateMap<Doctor, GetDoctorModel>();
            CreateMap<Institution, GetInstitutionModel>();
            CreateMap<CreateAppointmentModel, Appointment>();
            CreateMap<Appointment, GetAppointmentModel>();
            CreateMap<ScheduleDetailModel, ScheduleDetail>();
            CreateMap<ApplicationUser, GetUserModel>();
        }
    }
}
