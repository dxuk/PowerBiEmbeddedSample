using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using PowerBiSample.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PowerBiSample.Controllers
{
    public class ReportsController : Controller
    {
        private readonly string workspaceCollection;
        private readonly string workspaceId;
        private readonly string accessKey;
        private readonly string apiUrl;
        private readonly string appKey;

        public ReportsController()
        {
            /**
             * AppToken: A signed JWT to embed in HTTP requests
             * ApiUrl: Azure PowerBi HTTP endpoint
             * Workspace collection: Name of the Azure workspace
             * WorkspaceId: A workspace id within workspace collection
             * CAUTION: Keep customer reports in discrete workspaces
             **/

            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
            this.accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
            this.appKey = "AppKey";
        }

        // Ensure only authorized users can access the reports page
        [Authorize]
        public ActionResult Index()
        {
            // Display all PowerBi reports
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = client.Reports.GetReports(this.workspaceCollection, this.workspaceId);

                var viewModel = new ReportsViewModel
                {
                    Reports = reportsResponse.Value.ToList()
                };

                return View(viewModel);
            }
        }

        // Ensure only authorized users can access a specific report
        [Authorize]
        public async Task<ActionResult> Report(string reportId)
        {
            // Display specific PowerBi report
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId);
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == reportId);

                // Grab username from ASP.NET identity
                var username = User.Identity.GetUserName();
                var userId = User.Identity.GetUserId();

                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                var user = manager.FindById(userId);

                // Populate the PowerBiRole
                var powerBiRole = user.PowerBiRole;

                PowerBIToken embedToken;
                var rowLevelSecurityEnabled = !string.IsNullOrEmpty(powerBiRole);

                if (rowLevelSecurityEnabled)
                {
                    // Convert single PowerBi role to collection as required by SDK
                    List<string> powerBiRoles = new List<string>();
                    powerBiRoles.Add(powerBiRole);

                    // By passing in Username and Roles we are allowing Row Level Security
                    embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, report.Id, username, powerBiRoles);
                }
                else
                {
                    embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, report.Id, username);
                }

                var viewModel = new ReportViewModel
                {
                    Report = report,
                    AccessToken = embedToken.Generate(this.accessKey)
                };

                return View(viewModel);
            }
        }

        private IPowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(accessKey, appKey);
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };

            return client;
        }
    }

}