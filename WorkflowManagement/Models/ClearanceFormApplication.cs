using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkflowManagement.Models
{
    public class ClearanceFormApplication
    {
        public Application Application { get; set; }
        public String RollNumber { get; set; }
        public DateTime FinalResultDate { get; set; }
        public List<Workflow> WorkFlows { get; set; }
    }
}