using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkflowManagement.Models;

namespace WorkflowManagement.Controllers
{
    public class FormsController : Controller
    {
        // GET: Forms
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ClearanceForm()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {
                
                return View();
            }
        }

        public ActionResult SemesterFreezeOrWithdrawalForm()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {
                ViewBag.fullNameError = TempData["fullNameError"];
                ViewBag.fullname = TempData["fullName"];
                ViewBag.reasonsError = TempData["reasonsError"];
                ViewBag.reasons = TempData["reasons"];
                ViewBag.selectedSemesterError = TempData["selectedSemesterError"];

                for (int i = 1; i <= 8; i++)
                {
                    ViewData["cBox" + i] = TempData["cBox" + i];
                }

                return View();
            }
        }

        [ActionName("ClearanceFormSubmission")]
        public ActionResult ClearanceFormSubmission()
        {
            String fName = Request["name"];
            String reasons = Request["reasons"];

            String finalResultDate = Request["finalresultdate"];

            int dateTruthness = 0;
            if(finalResultDate.Length != 0)
            {
                DateTime dt = DateTime.Parse(finalResultDate);
                dateTruthness = DateTime.Compare(dt, DateTime.Now);
            }

            if (fName.Length < 7 || reasons.Length < 10 || finalResultDate.Length == 0 || dateTruthness > 0)
            {
                if (fName.Length < 7)
                    ViewBag.fNameError = "Kindly enter your full name";
                else
                    ViewBag.fullname = fName;

                if (reasons.Length < 10)
                    ViewBag.reasonError = "Kindly enter valid reasons";
                else
                    ViewBag.reasons = reasons;

                if (finalResultDate.Length == 0 || dateTruthness >= 0)
                    ViewBag.finalResultDateError = "Kindly Enter valid result date";
                else
                    ViewBag.finalResultDate = finalResultDate;

                return View("ClearanceForm");
            }
            else
            {
                
                ClearanceFormApplication app = new ClearanceFormApplication { Application = new Application() { CreatedOn = DateTime.Now, FullName = fName, FormID = 1, Reasons = reasons, Status = 1, UserID = Convert.ToInt32(Session["UserID"])}, FinalResultDate = DateTime.Parse(finalResultDate) };

                DBManager.RegisterNewApplication(app);

                return Redirect("~/Account/Index");
            }
        }

        [ActionName("SemesterFreezeOrWithdrawFormSubmission")]
        public ActionResult SemesterFreezeOrWithdrawFormSubmission()
        {

            String fName = Request["name"];
            String reasons = Request["reasons"];

            int[] cBox = new int[8];

            for(int i = 1; i <= 8; i++)
            {
                try {
                    String key = "checkbox" + i;
                    cBox[i - 1] = Request[key].Equals("on") ? 1 : 0;
                }
                catch (NullReferenceException)
                {
                    cBox[i - 1] = 0;
                }
            }

            if (fName.Length < 7 || reasons.Length < 10 || cBox.Sum() == 0)
            {
                if (fName.Length < 7)
                    TempData["fullNameError"] = "Kindly enter your full name";
                else
                    TempData["fullname"] = fName;

                if (reasons.Length < 10)
                    TempData["reasonsError"] = "Kindly enter valid reasons";
                else
                    TempData["reasons"] = reasons;

                if (cBox.Sum() == 0)
                    TempData["selectedSemesterError"] = "Kindly select the valid semesters";
                else
                {
                    for (int i = 1; i <= 8; i++)
                        TempData["cBox" + i] = cBox[i - 1] == 1 ? "checked" : null;
                }

                return Redirect("SemesterFreezeOrWithdrawalForm");
            }
            else
            {
                String [] selectedSemester = new string [8];

                for(int i = 0; i < 8; i++)
                {
                    selectedSemester[i] = cBox[i].ToString();
                }

                String selectedSemesters = String.Join(",", selectedSemester);

                SemesterFreezeClass sfc = new SemesterFreezeClass() { Application = new Application() { CreatedOn = DateTime.Now, FullName = fName, FormID = 9, Reasons = reasons, Status = 1, UserID = Convert.ToInt32(Session["UserID"]) }, RollNumber = (String)Session["username"], WorkFlows = null, SelectedSemesters = selectedSemesters };

                DBManager.RegisterSemesterFreezeApplication(sfc);

                return Redirect("~/Account/Index");
            }
        }

        [ActionName("Search")]
        public ActionResult Search()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {

                String id = Request["q"];
                if (id.Length == 0)
                {
                    return Redirect("~/Account/Index");
                }
                else
                {
                    ViewBag.Title = "Result for " + id;

                    if (Session["Designation"].Equals("Student"))
                    {
                        return View(DBManager.SearchApplicationByThisUser(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"])));
                    }
                    else
                    {
                        return View(DBManager.SearchApplicationForThisUser(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"])));
                    }
                    
                }
            }
        }

        public ActionResult ViewClearanceForm()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {
                String id = Request["id"];

                if (Session["Designation"].Equals("Student"))
                {
                    return View(DBManager.GetApplicationInformationOfThisUser(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"]), (String)Session["username"]));
                }
                else
                {
                    return View(DBManager.GetApplicationInformationForThisContributor(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"])));
                }

            }
        }

        public ActionResult ViewSemesterFreezeForm()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {
                String id = Request["id"];

                if (Session["Designation"].Equals("Student"))
                {
                    return View(DBManager.GetSemesterFreezeApplicationByThisUser(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"]),(String) Session["username"]));
                }
                else
                {
                    return View(DBManager.GetSemesterFreezeApplicationForThisUser(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"])));
                }
                
            }
        }

        public ActionResult ClearanceFormUpdation()
        {
            if (Session["username"] == null)
            {
                return Redirect("~/Home/Index");
            }
            else
            {
                String id = Request["id"];
                String remarks = Request["Remarks"];
                String option = Request["option"];

                if(id.Length > 0 && remarks.Length > 0 && option !=  null)
                    DBManager.UpdateWorkFlow(Convert.ToInt32(id), Convert.ToInt32(Session["UserID"]), option.Equals("accept"), remarks);

                return Redirect("~/Account/Index");
            }
        }

    }
}