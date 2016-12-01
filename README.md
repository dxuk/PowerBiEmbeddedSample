#PowerBiEmbeddedSample

Power BI Embedded lets you create visually impactful and interactive reports against your application data in Power BI Desktop – without writing any code. Easily explore your application data through a free-form, drag-and-drop canvas and produce rich data models using formulas and relationships. Choose from a broad range of modern Power BI data visualisations out of the box, or create custom visuals using open technologies such as D3 Web GL for your reporting needs. Visualise very large data sets directly from a wide variety of cloud sources, such as Azure SQL Database and Azure SQL Data Warehouse, using DirectQuery to ensure that your data is always up to date without moving your data to Power BI.

#Sample Application
In this sample we have added PowerBi Emebedded to a simple **ASP.NET MVC 4.5.2** application.

####Controllers/ReportsController.cs

In the constructor we initialise the required global variables to work with the PowerBi SDK. These are stored in the **web.config** file and accessed via the **ConfigurationManager**.
```
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

```
In the default Index action we return a list of all of the reports available in the workspace collection.
```
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
```
In the Report action we use a provided reportId to retrieve a specific PowerBi report to embed. We are using ASP.NET Identity to store our users in a SQL database, however, you can use which ever identity provider you wish. We have also added a 'PowerBiRole' column to the users table - this could be added anywhere as long as you can pair it with a user. This 'role' is a PowerBi Role and is different to the ASP.NET Roles you may be familiar with. PowerBi uses this Role as a predicate to filter the report for the given user.
```
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
```
####Models/ReportViewModel.cs
This is the model that will be constructed in the ReportsController and passed back to the view. It encapsulates both the report to embed in the DOM and the PowerBi access token.
```
public class ReportViewModel
{
    public Report Report { get; set; }
    public string AccessToken { get; set; }
}
```
####Models/ReportsViewModel.cs
A simple collection of PowerBi reports to be passed back to the Index view.
```
public class ReportsViewModel
{
    public List<Report> Reports { get; set; }
}
```
####Views/Reports/Index.cshtml
Render the list of views in a drop down panel.
```
<div id="reports-nav" class="panel-collapse collapse">
    <div class="panel-body">
        <ul class="nav navbar-nav">
            @foreach (var report in Model.Reports)
            {
                var reportClass = Request.QueryString["reportId"] == report.Id ? "active" : "";
                <li class="@reportClass">
                    @Html.ActionLink(report.Name, "Report", new { reportId = report.Id })
                </li>
            }
        </ul>
    </div>
</div>
```
####Views/Reports/Report.cshtml
HTMLHelper for rendering a PowerBi report provided by the SDK.
```
<div class="col-md-9" style="margin-top: 20px;">
    @Html.PowerBIReportFor(m => m.Report, new { id = "pbi-report", style = "height:85vh", powerbi_access_token = Model.AccessToken })
</div>

...

// Allow dynamic interaction with the report's controls
@section PageScripts
{
	...
}
```

####Views/Shared/_Layout.cshtml
Only allow authenticated users to see the Reports page.
```
@if (Request.IsAuthenticated)
{
     @:<li>@Html.ActionLink("Reports", "Index", "Reports")</li>
}
```
Make sure to add the required Javascript libraries and tell MVC to render the required scripts for the reports. **Disclaimer** This isn't the best place to put @RenderSection -  feel free to move it to a more isolated view.
```
<!-- Javascript Libs -->
<script type="text/javascript" src="/lib/js/jquery.min.js"></script>
<script type="text/javascript" src="/lib/js/bootstrap.min.js"></script>
<script type="text/javascript" src="/lib/js/Chart.min.js"></script>
<script type="text/javascript" src="/lib/js/bootstrap-switch.min.js"></script>
<script type="text/javascript" src="/lib/js/jquery.matchHeight-min.js"></script>
<script type="text/javascript" src="/lib/js/jquery.dataTables.min.js"></script>
<script type="text/javascript" src="/lib/js/dataTables.bootstrap.min.js"></script>
<script type="text/javascript" src="/lib/js/select2.full.min.js"></script>
<script type="text/javascript" src="/lib/js/es6-promise.auto.min.js"></script>
<script type="text/javascript" src="/lib/js/ace/ace.js"></script>
<script type="text/javascript" src="/lib/js/ace/mode-html.js"></script>
<script type="text/javascript" src="/lib/js/ace/theme-github.js"></script>

<script type="text/javascript" src="/js/app.js"></script>
<script src="~/Scripts/powerbi.js"></script>
@RenderSection("PageScripts", false)
```

##Database
We have provided a .dacpac file to help you create a database with a complimentary schema for this sample. The .dacpac is in the **Database** folder.

