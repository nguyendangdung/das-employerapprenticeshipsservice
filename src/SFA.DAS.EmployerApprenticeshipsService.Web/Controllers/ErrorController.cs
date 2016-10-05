﻿using System.Net;
using System.Web.Mvc;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }
    }
}