﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiddleProject.Commands;
using MiddleProject.Models;
using MiddleProject.Queries;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedicalAppointmentApp.Controllers
{
    [Route("[controller]")]
    public class AppointmentController : Controller
    {
        private readonly IMediator _mediator;
        public AppointmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Authorize(Roles = "Basic")]
        public async Task<IActionResult> CreateAppointmentView(string doctorId, string address, DateTime date)
        {
            if (await IsUserLocked()) return RedirectToAction("Index", "Home");
            if (DateTime.Compare(DateTime.Now, date) >= 0) date = DateTime.Now;
            var appointmentViewModel = new CreateAppointmentModel()
            {
                DoctorId = Int32.Parse(doctorId),
                Address = address,
                ApplicationUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                CurrentDateTime = date,
                DoctorScheduleDetails = await _mediator.Send(new GetDoctorSchedule.Query(Int32.Parse(doctorId), address, date)),
                DoctorAppointments = await _mediator.Send(new GetAppointmentsByDoctorId.Query(Int32.Parse(doctorId)))
            };
            return View("CreateAppointment", appointmentViewModel);
        }

        private async Task<bool> IsUserLocked()
        {
            //check if user is locked
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var lockedUser = await _mediator.Send(new GetLockedUser.Query(userId));
            if (lockedUser.IsBlackListed)
            {
                var response = new CustomResponse();
                response.AddError(new CustomError { Error = "Failed", Message = "Account is locked until " + lockedUser.BlackListedEndDate });
                TempData.Put("CustomResponse", response);
            }
            return lockedUser.IsBlackListed;
        }

        [HttpPost]
        [Authorize(Roles = "Basic")]
        public async Task<IActionResult> CreateAppointment([FromForm] CreateAppointmentModel appointmentModel)
        {
            var response = await _mediator.Send(new CreateAppointment.Command
            {
                AppointmentModel = appointmentModel
            });

            TempData.Put("CustomResponse", response);

            var parms = new Dictionary<string, string>
            {
                { "userId", this.User.FindFirst(ClaimTypes.NameIdentifier).Value }
            };
            return RedirectToAction("GetAppointmentsByUserId", "Appointment", parms);
        }

        [HttpGet("user")]
        [Authorize(Roles = "Basic")]
        public async Task<IActionResult> GetAppointmentsByUserId(
            [FromQuery(Name = "userId")] string userId,
            [FromQuery(Name = "pageNumber")] int? pageNumber,
            [FromQuery(Name = "pageSize")] int? pageSize)
        {
            ViewBag.PageNumber = pageNumber ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.CurrentFilter = userId;
            ViewBag.AppointmentCount = await _mediator.Send(new GetAppointmentCountByUserId.Query(userId));
            var appointmentsListViewModel = await _mediator.Send(new GetAppointmentsByUserId.Query(userId, pageNumber ?? 1, pageSize ?? 10));

            var customResponse = TempData.Get<CustomResponse>("CustomResponse");
            if (customResponse != null)
            {
                ViewBag.CustomResponse = customResponse;
            }

            return View("UserAppointmentList", appointmentsListViewModel);
        }

        [HttpPost("adminDelete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var response = await _mediator.Send(new DeleteAppointmentId.Command { Id = id });

            TempData.Put("CustomResponse", response);

            return RedirectToAction("GetAppointments", "Appointment");
        }

        [HttpPost("delete/{id}")]
        [Authorize(Roles = "Basic")]
        public async Task<IActionResult> UserDeleteAppointment(int id)
        {
            var parms = new Dictionary<string, string>
            {
                { "userId", this.User.FindFirst(ClaimTypes.NameIdentifier).Value }
            };

            if (!(await IsUsersAppointment(id)))
            {
                return RedirectToAction("GetAppointmentsByUserId", "Appointment", parms);
            }
            var response = await _mediator.Send(new DeleteAppointmentId.Command { Id = id });

            TempData.Put("CustomResponse", response);

            return RedirectToAction("GetAppointmentsByUserId", "Appointment", parms);
        }

        private async Task<bool> IsUsersAppointment(int appointmentId)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userAppointments = await _mediator.Send(new GetAppointmentsByUserIdAndAppointmentId.Query(userId, appointmentId));
            if (!userAppointments)
            {
                var response = new CustomResponse();
                response.AddError(new CustomError { Error = "Failed", Message = "You can only delete your appointments" });
                TempData.Put("CustomResponse", response);
            }
            return userAppointments;
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointments(
                [FromQuery(Name = "pageNumber")] int? pageNumber,
                [FromQuery(Name = "pageSize")] int? pageSize)
        {
            ViewBag.PageNumber = pageNumber ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.AppointmentCount = await _mediator.Send(new GetAppointmentCount.Query());
            var appointmentsListViewModel = await _mediator.Send(new GetAppointments.Query(pageNumber ?? 1, pageSize ?? 10));

            var customResponse = TempData.Get<CustomResponse>("CustomResponse");
            if (customResponse != null)
            {
                ViewBag.CustomResponse = customResponse;
            }

            return View("AppointmentList", appointmentsListViewModel);
        }
    }
}
