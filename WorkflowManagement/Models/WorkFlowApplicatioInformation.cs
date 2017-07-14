using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkflowManagement.Models
{
    public class WorkFlowApplicatioInformation
    {
        public Workflow Workflow { get; set; }
        public Application Application { get; set; }
    }
}