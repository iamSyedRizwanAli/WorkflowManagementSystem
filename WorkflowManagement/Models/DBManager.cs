using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAccess;

namespace WorkflowManagement.Models
{
    public class DBManager
    {
        public static bool Validate(LoginModel requestedUser)
        {
            var db = new DatabaseEntities();
            List<User> list = db.Users.ToList();

            var result = (from u in list where u.Login.Equals(requestedUser.Username) && u.Password.Equals(requestedUser.Password) select u).ToList();

            return result.Count == 1;
        }

        public static User GetUser(String userName)
        {
            var db = new DatabaseEntities();
            List <User> list = db.Users.ToList();

            var results = (from u in list where u.Login.Equals(userName) select u).ToList();

            return results.ElementAt(0);

        }

        public static List<WorkFlowApplicatioInformation> GetAllApplicationsForThisUser(int userId)
        {
            var db = new DatabaseEntities();
            List<Workflow> faList = db.Workflows.ToList();
            List<Application> applications = db.Applications.ToList();

            var resultA = (from fAps in faList where fAps.ApproverID == userId && (fAps.Status == 1 || fAps.Status == 4) select fAps).ToList();
            var resultB = (from app in applications join f in resultA on app.ApplicationID equals f.AppliationID select new WorkFlowApplicatioInformation() { Application = app, Workflow = f}).ToList();
            
            return resultB;

        }
        
        public static List<WorkFlowApplicatioInformation> GetAllApplicationsByThisUser(int userId)
        {
            var db = new DatabaseEntities();
            List<Application> list = db.Applications.ToList();
            List<Workflow> faList = db.Workflows.ToList();

            var result = (from l in list where l.UserID == userId select new WorkFlowApplicatioInformation() { Application = l, Workflow = null }).ToList();

            return result;
        }

        public static void RegisterNewApplication(ClearanceFormApplication app)
        {
            var db = new DatabaseEntities();
            db.Applications.Add(app.Application);
            db.SaveChanges();

            List<Form_Approver> faList = db.Form_Approver.ToList();
            faList = (from f in faList where f.FormID == app.Application.FormID select f).ToList();

            db.ClearanceFormFinalResultDates.Add(new ClearanceFormFinalResultDate() { ApplicationID = app.Application.ApplicationID, FinalResultDate = app.FinalResultDate });
            db.SaveChanges();

            Workflow wf = new Workflow() { AppliationID = app.Application.ApplicationID, ApproverID = faList.ElementAt(0).ApproverID, Status = 1, Order = faList.ElementAt(0).ApprovalOrder, Time = app.Application.CreatedOn };
            db.Workflows.Add(wf);
            for (int i = 1; i < faList.Count; i++)
            {
                wf = new Workflow() { AppliationID = app.Application.ApplicationID, ApproverID = faList.ElementAt(i).ApproverID, Status = 4, Order = faList.ElementAt(i).ApprovalOrder, Time = app.Application.CreatedOn };
                db.Workflows.Add(wf);
            }

            db.SaveChanges();
        }

        public static ClearanceFormApplication GetApplicationInformationForThisContributor(int app_id, int contributor)
        {
            var db = new DatabaseEntities();
            
            List<Workflow> workflows = db.Workflows.ToList();

            var y = (from w in workflows where w.AppliationID == app_id && w.ApproverID == contributor select w).ToList();

            if (y.Count == 0)
                return null;

            List<Application> apps = db.Applications.ToList();
            List<Workflow> list = (from w in workflows where w.AppliationID == app_id select w).ToList();
            var x = (from a in apps where a.ApplicationID == app_id select a).ToList();
            List<User> users = db.Users.ToList();
            var u = (from user in users where user.UserId == x.ElementAt(0).UserID select user).ToList();

            var finalResultDates = db.ClearanceFormFinalResultDates.ToList();
            var fDate = (from date in finalResultDates where date.ApplicationID == x.ElementAt(0).ApplicationID select date).ToList();

            ClearanceFormApplication cfa = new ClearanceFormApplication() { Application = x.ElementAt(0), RollNumber = u.ElementAt(0).Login, WorkFlows = list, FinalResultDate = fDate.ElementAt(0).FinalResultDate };

            return cfa;

        }

        public static ClearanceFormApplication GetApplicationInformationOfThisUser(int app_id, int user_id, String rollNumber)
        {
            var db = new DatabaseEntities();

            List<Application> apps = db.Applications.ToList();
            var y = (from a in apps where a.ApplicationID == app_id && a.UserID == user_id select a).ToList();

            if (y.Count == 0)
                return null;

            List<Workflow> workflows = db.Workflows.ToList();
            var finalResultDates = db.ClearanceFormFinalResultDates.ToList();
            var fDate = (from date in finalResultDates where date.ApplicationID == app_id select date).ToList();
            List<Workflow> wfs = (from wf in workflows where wf.AppliationID == app_id select wf).ToList();
            
            ClearanceFormApplication cfa = new ClearanceFormApplication() { Application = y.ElementAt(0), RollNumber = rollNumber, WorkFlows = wfs, FinalResultDate = fDate.ElementAt(0).FinalResultDate };

            return cfa;


        }

        public static void UpdateWorkFlow(int app_id, int user_id, bool option, String remarks)
        {
            var db = new DatabaseEntities();

            var wfs = db.Workflows.ToList();

            List<Workflow> wffffs = new List<Workflow>();

            for(int i = 0; i< wfs.Count; i++)
            {
                if (wfs.ElementAt(i).AppliationID == app_id)
                    wffffs.Add(wfs.ElementAt(i));
            }

            int o;
            bool flag = false;
            for(o = 0; o < wffffs.Count && !flag; o++)
            {
               if(wffffs.ElementAt(o).Status == 1)
                {
                    flag = true;
                    wffffs.ElementAt(o).Status = option ? 2 : 3;
                    wffffs.ElementAt(o).Remarks = remarks;
                }
            }

            if (o == wffffs.Count)
            {
                var t = db.Applications.ToList();

                Application app = new Application();
                foreach (Application a in t)
                {
                    if (a.ApplicationID == app_id)
                        app = a;
                }
                app.Status = option ? 2 : 3;
            }
            else if (option && o < wffffs.Count)
            {
                wffffs.ElementAt(o).Status = 1;
                wffffs.ElementAt(o).Time = DateTime.Now;
            }
            else if (!option)
            {
                while (o < wffffs.Count)
                {
                    wffffs.ElementAt(o).Status = 3;
                    o++;
                }
                var t = db.Applications.ToList();

                Application app = new Application();
                foreach (Application a in t)
                {
                    if (a.ApplicationID == app_id)
                        app = a;
                }
                app.Status = option ? 2 : 3;
            }
            db.SaveChanges();
        }

        public static List<WorkFlowApplicatioInformation> SearchApplicationByThisUser(int app_id, int userId)
        {
            var db = new DatabaseEntities();
            List<Application> list = db.Applications.ToList();
            List<Workflow> faList = db.Workflows.ToList();

            var result = (from l in list where l.UserID == userId && l.ApplicationID == app_id select new WorkFlowApplicatioInformation() { Application = l, Workflow = null }).ToList();

            return result;
        }

        public static List<WorkFlowApplicatioInformation> SearchApplicationForThisUser(int app_id, int userId)
        {
            var db = new DatabaseEntities();
            List<Workflow> faList = db.Workflows.ToList();
            List<Application> applications = db.Applications.ToList();

            var resultA = (from fAps in faList where fAps.AppliationID == app_id && fAps.ApproverID == userId select fAps).ToList();
            var resultB = (from app in applications join f in resultA on app.ApplicationID equals f.AppliationID select new WorkFlowApplicatioInformation() { Application = app, Workflow = f }).ToList();
            
            return resultB;
        }

        public static SemesterFreezeClass GetSemesterFreezeApplicationByThisUser(int app_id, int user_id, String rollNumber)
        {
            var db = new DatabaseEntities();

            List<Application> apps = db.Applications.ToList();
            var y = (from a in apps where a.ApplicationID == app_id && a.UserID == user_id select a).ToList();

            if (y.Count == 0)
                return null;

            List<Workflow> workflows = db.Workflows.ToList();
            var finalResultDates = db.ClearanceFormFinalResultDates.ToList();
            List<Workflow> wfs = (from wf in workflows where wf.AppliationID == app_id select wf).ToList();

            List<SemesterDropApplicationRecord> sdar = db.SemesterDropApplicationRecords.ToList();
            var ss = (from s in sdar where s.Application_ID == app_id select s).ToList();

            List<String> sSemesters = new List<string>();

            if (ss.ElementAt(0).SemesterOne)
                sSemesters.Add("1");
            if (ss.ElementAt(0).SemesterTwo)
                sSemesters.Add("2");
            if (ss.ElementAt(0).SemesterThree)
                sSemesters.Add("3");
            if (ss.ElementAt(0).SemesterFour)
                sSemesters.Add("4");
            if (ss.ElementAt(0).SemesterFive)
                sSemesters.Add("5");
            if (ss.ElementAt(0).SemesterSix)
                sSemesters.Add("6");
            if (ss.ElementAt(0).SemesterSeven)
                sSemesters.Add("7");
            if (ss.ElementAt(0).SemesterEight)
                sSemesters.Add("8");

            String selectedSemesters = String.Join(", ", sSemesters.ToArray());

            SemesterFreezeClass sfc = new SemesterFreezeClass() { Application = y.ElementAt(0), RollNumber = rollNumber, SelectedSemesters = selectedSemesters, WorkFlows = wfs };

            return sfc;
        }

        public static SemesterFreezeClass GetSemesterFreezeApplicationForThisUser(int app_id, int user_id)
        {
            var db = new DatabaseEntities();

            List<Workflow> workflows = db.Workflows.ToList();

            var y = (from w in workflows where w.AppliationID == app_id && w.ApproverID == user_id select w).ToList();

            if (y.Count == 0)
                return null;

            List<Application> apps = db.Applications.ToList();
            List<Workflow> list = (from w in workflows where w.AppliationID == app_id select w).ToList();
            var x = (from a in apps where a.ApplicationID == app_id select a).ToList();
            List<User> users = db.Users.ToList();
            var u = (from user in users where user.UserId == x.ElementAt(0).UserID select user).ToList();

            List<SemesterDropApplicationRecord> sdar = db.SemesterDropApplicationRecords.ToList();
            var ss = (from s in sdar where s.Application_ID == app_id select s).ToList();

            List<String> sSemesters = new List<string>();

            if (ss.ElementAt(0).SemesterOne)
                sSemesters.Add("1");
            if (ss.ElementAt(0).SemesterTwo)
                sSemesters.Add("2");
            if (ss.ElementAt(0).SemesterThree)
                sSemesters.Add("3");
            if (ss.ElementAt(0).SemesterFour)
                sSemesters.Add("4");
            if (ss.ElementAt(0).SemesterFive)
                sSemesters.Add("5");
            if (ss.ElementAt(0).SemesterSix)
                sSemesters.Add("6");
            if (ss.ElementAt(0).SemesterSeven)
                sSemesters.Add("7");
            if (ss.ElementAt(0).SemesterEight)
                sSemesters.Add("8");

            String selectedSemesters = String.Join(", ", sSemesters.ToArray());

            SemesterFreezeClass sfc = new SemesterFreezeClass() { Application = x.ElementAt(0), RollNumber = u.ElementAt(0).Login, SelectedSemesters = selectedSemesters, WorkFlows = list};

            return sfc;
        }

        public static void RegisterSemesterFreezeApplication(SemesterFreezeClass app)
        {
            var db = new DatabaseEntities();
            db.Applications.Add(app.Application);
            db.SaveChanges();

            List<Form_Approver> faList = db.Form_Approver.ToList();
            faList = (from f in faList where f.FormID == app.Application.FormID select f).ToList();

            String[] semesters = app.SelectedSemesters.Split(',');

            db.SemesterDropApplicationRecords.Add(new SemesterDropApplicationRecord() { Application_ID = app.Application.ApplicationID, SemesterOne = semesters[0].Equals("1"), SemesterTwo = semesters[1].Equals("1"), SemesterThree = semesters[2].Equals("1"), SemesterFour = semesters[3].Equals("1"), SemesterFive = semesters[4].Equals("1"), SemesterSix = semesters[5].Equals("1"), SemesterSeven = semesters[6].Equals("1"), SemesterEight = semesters[7].Equals("1") });
            db.SaveChanges();

            Workflow wf = new Workflow() { AppliationID = app.Application.ApplicationID, ApproverID = faList.ElementAt(0).ApproverID, Status = 1, Order = faList.ElementAt(0).ApprovalOrder, Time = app.Application.CreatedOn };
            db.Workflows.Add(wf);
            for (int i = 1; i < faList.Count; i++)
            {
                wf = new Workflow() { AppliationID = app.Application.ApplicationID, ApproverID = faList.ElementAt(i).ApproverID, Status = 4, Order = faList.ElementAt(i).ApprovalOrder, Time = app.Application.CreatedOn };
                db.Workflows.Add(wf);
            }
            

            db.SaveChanges();
        }

    }
}