using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPMenusController : Controller
    {
        // GET: SPMenus
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuList()
        {
            return View();
        }

        public async Task<JsonResult> GetMenuListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPMenuList> menus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }

            Session["MenuNo"] = "";
            Session["isMenuEdit"] = false;
            ViewBag.isMenuEdit = false;
            Session["MenuAction"] = "";

            return Json(menus, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSubMenuListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPMenuList> submenus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                submenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }

            return Json(submenus, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPMenuList> menus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }
            DataTable dt = ToDataTable(menus);

            string fileName = "SPMenuList.xlsx";
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

        public ActionResult Menu(string No = "")
        {
            SPMenus menu = new SPMenus();

            ViewBag.MenuType = "";
            ViewBag.ParentMenuNo = "";
            if (No != "" || Session["MenuNo"].ToString() != "")
            {
                if (Session["MenuNo"].ToString() == "")
                    Session["MenuNo"] = No;

                Task<SPMenus> task = Task.Run<SPMenus>(async () => await GetMenuForEdit(Session["MenuNo"].ToString()));
                menu = task.Result;

                Session["isMenuEdit"] = true;
                ViewBag.MenuType = menu.Type;
                ViewBag.ParentMenuNo = menu.Parent_Menu_No;
            }

            if (menu != null)
                return View(menu);
            else
                return View(new SPMenus());

        }

        public async Task<SPMenus> GetMenuForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetMenuFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPMenus menu = new SPMenus();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menu = Newtonsoft.Json.JsonConvert.DeserializeObject<SPMenus>(data);
            }

            return menu;
        }

        [HttpPost]
        public async Task<ActionResult> Menu(SPMenus menu)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string MenuNo = "";
            if (Convert.ToBoolean(Session["isMenuEdit"]) == true)
            {
                MenuNo = Session["MenuNo"].ToString();
                apiUrl = apiUrl + "Menu?isEdit=true&MenuNo=" + MenuNo;
            }
            else
                apiUrl = apiUrl + "Menu?isEdit=false&MenuNo=" + MenuNo;

            SPMenusResponse responseMenu = new SPMenusResponse();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(menu);
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
                responseMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<SPMenusResponse>(data);
            }

            if (Convert.ToBoolean(Session["isMenuEdit"]) == true)
                Session["MenuAction"] = "Updated";
            else
                Session["MenuAction"] = "Added";

            return RedirectToAction("Menu");

        }

        public bool NullMenuSession()
        {
            bool isSessionNull = false;

            Session["MenuAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<JsonResult> GetAllParentMenuNoForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/GetAllParentMenuNoForDDL";

            HttpClient client = new HttpClient();
            List<SPParentMenuNo> parentmenuno = new List<SPParentMenuNo>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                parentmenuno = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPParentMenuNo>>(data);
            }

            //List<SPParentMenuNo> parentmenuno1 = parentmenuno.DistinctBy(a => a.Parent_Menu_No).ToList();

            return Json(parentmenuno, JsonRequestBehavior.AllowGet);
        }
    }
}