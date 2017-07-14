using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess;
using WorkflowManagement.Models;

namespace WorkflowManagement.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("Login")]
        [HttpPost]
        public ActionResult Login()
        {
            String uName = Request["Username"];
            String pass = Request["Password"];

            LoginModel model = new LoginModel() { Username = uName, Password = pass };

            if (!DBManager.Validate(model))
            {
                return Redirect("~/Home/Index");
            }
            else {
                Session["username"] = uName;
                return Redirect("~/Account/Index");
            }
        }



    }
}