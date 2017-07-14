using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorkflowManagement.Startup))]
namespace WorkflowManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
