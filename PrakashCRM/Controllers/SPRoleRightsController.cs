using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using System.Text;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPRoleRightsController : Controller
    {
        // GET: SPRoleRights
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> RoleRights()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Type eq \'Navigation\' and ClassName ne \'bx bx-right-arrow-alt\' and ClassName ne \'has-arrow\'";

            HttpClient client = new HttpClient();
            List<SPMenuList> menus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }

            for(int a = 0; a < menus.Count; a++)
            {
                Task<List<SPSubMenuList>> task = Task.Run<List<SPSubMenuList>>(async () => await GetSubMenusForMenu(menus[a].No));
                List<SPSubMenuList> submenus = task.Result;

                for (int b = 0; b < submenus.Count; b++)
                {
                    menus[a].subMenuList.Add(
                        new SPSubMenuList
                        {
                            No = submenus[b].No,
                            Menu_Name = submenus[b].Menu_Name
                        }
                   );

                }

                List<SPSubMenuList> sml = menus[a].subMenuList.ToList();
                for (int c = 0; c < sml.Count; c++)
                {
                    Task<List<SPSubSubMenuList>> task1 = Task.Run<List<SPSubSubMenuList>>(async () => await GetSubSubMenusForSubMenu(sml[c].No));
                    List<SPSubSubMenuList> subsubmenus = task1.Result;

                    menus[a].subSubListCnt = subsubmenus.Count;
                    for (int d = 0; d < subsubmenus.Count; d++)
                    {
                        sml[c].subSubMenuList.Add(
                            new SPSubSubMenuList
                            {
                                No = subsubmenus[d].No,
                                Menu_Name = subsubmenus[d].Menu_Name
                            }
                        );
                    }
                }
            }

            return View(menus);
        }

        public async Task<List<SPSubMenuList>> GetSubMenusForMenu(string menuNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Parent_Menu_No eq \'" + menuNo + "\' and Type eq \'Navigation\' and No ne \'" + menuNo + "\'";

            HttpClient client = new HttpClient();
            List<SPSubMenuList> submenus = new List<SPSubMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                submenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSubMenuList>>(data);
            }

            return submenus;
        }

        public async Task<List<SPSubSubMenuList>> GetSubSubMenusForSubMenu(string subMenuNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Parent_Menu_No eq \'" + subMenuNo + "\' and Type eq \'Navigation\' and ClassName eq \'bx bx-right-arrow-alt\'";

            HttpClient client = new HttpClient();
            List<SPSubSubMenuList> subsubmenus = new List<SPSubSubMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                subsubmenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSubSubMenuList>>(data);
            }

            return subsubmenus;
        }

        public async Task<JsonResult> GetAllRolesForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoleRights/GetAllRolesForDDL";

            HttpClient client = new HttpClient();
            List<SPRolesForDDL> roles = new List<SPRolesForDDL>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRolesForDDL>>(data);
            }

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public async Task<string> GetAllMenusSubMenusOfRole(string RoleNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoleRights/";

            HttpClient client = new HttpClient();
            List<SPMenusSubMenusOfRole> menussubmenus = new List<SPMenusSubMenusOfRole>();

            apiUrl = apiUrl + "GetAllMenusSubMenusOfRole?RoleNo=" + RoleNo;

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menussubmenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenusSubMenusOfRole>>(data);
            }

            string strMenusSubMenus = "";

            for(int i = 0; i < menussubmenus.Count; i++)
            {
                strMenusSubMenus += menussubmenus[i].Sub_Menu_No == "" ? menussubmenus[i].Menu_No + "-" + menussubmenus[i].Menu_No + "," : menussubmenus[i].Menu_No + "-" + menussubmenus[i].Sub_Menu_No + ",";
            }

            strMenusSubMenus = strMenusSubMenus.Substring(0, strMenusSubMenus.Length - 1);

            strMenusSubMenus += menussubmenus[0].Add_Rights == true ? ",Add_Rights" : "";
            strMenusSubMenus += menussubmenus[0].Edit_Rights == true ? ",Edit_Rights" : "";
            strMenusSubMenus += menussubmenus[0].View_Rights == true ? ",View_Rights" : "";
            strMenusSubMenus += menussubmenus[0].Delete_Rights == true ? ",Delete_Rights" : "";
            strMenusSubMenus += menussubmenus[0].Full_Rights == true ? ",Full_Rights" : "";

            return strMenusSubMenus;
        }
    }
}