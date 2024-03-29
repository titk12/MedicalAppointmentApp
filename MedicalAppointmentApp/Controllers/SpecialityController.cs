﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiddleProject.Commands;
using MiddleProject.Models;
using MiddleProject.Queries;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Controllers
{
    [Route("[controller]")]
    public class SpecialityController : Controller
    {
        private readonly IMediator _mediator;

        public SpecialityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateMedicalSpeciality()
        {
            return View();
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SpecialityList(
            [FromQuery(Name = "pageNumber")] int? pageNumber,
            [FromQuery(Name = "pageSize")] int? pageSize)
        {
            ViewBag.PageNumber = pageNumber ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SpecialityCount = await _mediator.Send(new GetMedicalSpecialtyCount.Query());
            var specialitiesViewModel = await _mediator.Send(new GetMedicalSpecialties.Query(pageNumber ?? 1, pageSize ?? 10));

            var customResponse = TempData.Get<CustomResponse>("CustomResponse");
            if (customResponse != null)
            {
                ViewBag.CustomResponse = customResponse;
            }

            return View(specialitiesViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMedicalSpeciality([FromForm] CreateMedicalSpecialityModel medicalSpecialityModel)
        {
            var response = await _mediator.Send(new CreateMedicalSpeciality.Command
            {
                MedicalSpecialityModel = medicalSpecialityModel
            });

            TempData.Put("CustomResponse", response);

            return RedirectToAction("SpecialityList");
        }

        [HttpPost("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpeciality(int id)
        {
            var response = await _mediator.Send(new DeleteSpeciality.Command { Id = id });

            TempData.Put("CustomResponse", response);

            return RedirectToAction("SpecialityList");
        }
    }
}
