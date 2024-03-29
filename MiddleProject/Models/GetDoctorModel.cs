﻿using DAL.Data.Models;
using System.Collections.Generic;

namespace MiddleProject.Models
{
    public class GetDoctorModel
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int MedicalSpecialityId { get; set; }
        public MedicalSpeciality MedicalSpeciality { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}
