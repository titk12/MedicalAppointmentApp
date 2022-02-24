﻿using AutoMapper;
using MedicalAppointmentApp.Data.Models;

namespace MedicalAppointmentApp.Models.MapperProfiles
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<MedicalSpeciality, GetMedicalSpecialtyModel>();
            CreateMap<Doctor, GetDoctorModel>();
            CreateMap<Institution, GetInstitutionModel>();
        }
    }
}