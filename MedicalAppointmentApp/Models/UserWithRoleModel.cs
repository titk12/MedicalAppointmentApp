﻿using MedicalAppointmentApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Models
{
    public class UserWithRoleModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}