﻿using MediatR;
using MedicalAppointmentApp.Data.Models;
using MedicalAppointmentApp.Mediator.Queries;
using MedicalAppointmentApp.Models;
using MedicalAppointmentApp.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchView(
            [FromQuery(Name = "q")] string q,
            [FromQuery(Name = "currentFilter")] string currentFilter,
            [FromQuery(Name = "pageNumber")] int? pageNumber,
            [FromQuery(Name = "pageSize")] int? pageSize)
        {
            int numOfAppointmentsToGet = 3;

            if (q != null) pageNumber = 1;
                else q = currentFilter;

            ViewBag.CurrentFilter = q;
            ViewBag.PageNumber = pageNumber ?? 1;
            ViewBag.PageSize = pageSize ?? 10;

            int doctorCount = await _mediator.Send(new GetDoctorCountByQuery.Query(q ?? ""));
            ViewBag.HasNextPage = Math.Ceiling((double)doctorCount / (double)(pageSize ?? 10)) == (pageNumber ?? 1);

            var doctorsViewModel = await _mediator.Send(new GetDoctorsByQuery.Query(q ?? "", numOfAppointmentsToGet, pageNumber ?? 1, pageSize ?? 10));
            return View("Search", doctorsViewModel);
        }

        [Authorize]
        [HttpGet("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
