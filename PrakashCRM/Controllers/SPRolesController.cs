using ClosedXML.Excel;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPRolesController : Controller
    {
        // GET: SPRoles
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RoleList()
        {
            return View();
        }

        public async Task<JsonResult> GetRoleListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Role_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllRoles?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPRoleList> roles = new List<SPRoleList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRoleList>>(data);
            }

            Session["RoleNo"] = "";
            Session["isRoleEdit"] = false;
            Session["RoleAction"] = "";

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Role_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllRoles?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPRoleList> roles = new List<SPRoleList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRoleList>>(data);
            }
            DataTable dt = ToDataTable(roles);

            //Name of File  
            string fileName = "SPRoleList.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);


                using (var exportData = new MemoryStream())
                {
                    wb.SaveAs(exportData);
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    exportData.WriteTo(file);
                    file.Close();
                }
            }

            return Json(new { fileName = fileName, errorMessage = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public ActionResult Role(string No = "")
        {
            SPRoles role = new SPRoles();

            if (No != "" || Session["RoleNo"].ToString() != "")
            {
                if (Session["RoleNo"].ToString() == "")
                    Session["RoleNo"] = No;

                Task<SPRoles> task = Task.Run<SPRoles>(async () => await GetRoleForEdit(Session["RoleNo"].ToString()));
                role = task.Result;

                Session["isRoleEdit"] = true;
            }

            if (role != null)
                return View(role);
            else
                return View(new SPRoles());

        }

        public async Task<SPRoles> GetRoleForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            apiUrl = apiUrl + "GetRoleFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPRoles role = new SPRoles();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                role = Newtonsoft.Json.JsonConvert.DeserializeObject<SPRoles>(data);
            }

            return role;
        }

        [HttpPost]
        public async Task<ActionResult> Role(SPRoles role)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string RoleNo = "";
            if (Convert.ToBoolean(Session["isRoleEdit"]) == true)
            {
                RoleNo = Session["RoleNo"].ToString();
                apiUrl = apiUrl + "Role?isEdit=true&RoleNo=" + RoleNo;
            }
            else
                apiUrl = apiUrl + "Role?isEdit=false&RoleNo=" + RoleNo;

            SPRolesResponse responseRole = new SPRolesResponse();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(role);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                responseRole = Newtonsoft.Json.JsonConvert.DeserializeObject<SPRolesResponse>(data);
            }

            if (Convert.ToBoolean(Session["isRoleEdit"]) == true)
                Session["RoleAction"] = "Updated";
            else
                Session["RoleAction"] = "Added";

            return RedirectToAction("Role");
        }

        public bool NullRoleSession()
        {
            bool isSessionNull = false;

            Session["RoleAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }
    }
}