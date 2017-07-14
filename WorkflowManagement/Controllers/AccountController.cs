using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkflowManagement.Models;

namespace WorkflowManagement.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else {

                String uName = (string)Session["username"];


                User user = DBManager.GetUser(uName);
                
                Session["Name"] = user.Name;
                Session["Designation"] = user.Designation;
                Session["UserID"] = user.UserId;

                if (user.Designation.Equals("Student"))
                {
                    return View(DBManager.GetAllApplicationsByThisUser(user.UserId));
                }
                else
                {
                    return View(DBManager.GetAllApplicationsForThisUser(user.UserId));
                }
                
            }
        }

        public ActionResult Logout()
        {
            Session["username"] = null;
            Session.Abandon();

            return Redirect("~/Home/Index");
        }

    }
}