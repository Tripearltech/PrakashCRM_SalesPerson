using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPRoleRights")]
    public class SPRoleRightsController : ApiController
    {
        [Route("RoleRights")]
        public bool RoleRights(string RoleNo,string PrevSavedMenusRights, string MenusWithRights)
        {

            if(PrevSavedMenusRights != "" ||  MenusWithRights != null)
            {
                SPMenusRightsForDel requestMenusRightsForDel = new SPMenusRightsForDel();
                SPMenusRightsForDelRes responseMenusRightsForDel = new SPMenusRightsForDelRes();
                errorDetails edForDel = new errorDetails();
                requestMenusRightsForDel.roleid = RoleNo;

                var result = PostItemForMenusRightsDel("", requestMenusRightsForDel, responseMenusRightsForDel);

                if (result.Result.Item2.message != null)
                    edForDel = result.Result.Item2;
            }

            bool flag = false;
            
            errorDetails ed = new errorDetails();

            MenusWithRights = MenusWithRights.Substring(0, MenusWithRights.Length - 1);
            string[] strMenuNo = MenusWithRights.Split(',');
            int rightsCnt = 0;
            for(int a = 0; a < strMenuNo.Length; a++)
            {
                if (strMenuNo[a] == "Full_Rights" || strMenuNo[a] == "Add_Rights" || strMenuNo[a] == "Edit_Rights" || strMenuNo[a] == "View_Rights" || strMenuNo[a] == "Delete_Rights")
                    rightsCnt++;
            }

            string[] strMenuRights = new string[rightsCnt];
            int cnt = 0;
            
            for(int a = 0; a < strMenuNo.Length; a++)
            {
                switch (strMenuNo[a])
                {
                    case "Full_Rights":
                        strMenuRights[cnt] = "Full_Rights";
                        cnt++;
                        break;
                    case "Add_Rights":
                        strMenuRights[cnt] = "Add_Rights";
                        cnt++;
                        break;
                    case "Edit_Rights":
                        strMenuRights[cnt] = "Edit_Rights";
                        cnt++;
                        break;
                    case "View_Rights":
                        strMenuRights[cnt] = "View_Rights";
                        cnt++;
                        break;
                    case "Delete_Rights":
                        strMenuRights[cnt] = "Delete_Rights";
                        cnt++;
                        break;
                }
                
            }

            //M004-M004, M004-M005, M004-M006
            int chkSubMenu, chkSubSubMenu;
            for (int b = 0; b < strMenuNo.Length; b++)
            {
                if (strMenuNo[b] != "Full_Rights" && strMenuNo[b] != "Add_Rights" && strMenuNo[b] != "Edit_Rights" && strMenuNo[b] != "View_Rights" && strMenuNo[b] != "Delete_Rights")
                {
                    string[] MenuNo = strMenuNo[b].Split('-');
                    SPRoleRights roleRights = new SPRoleRights();

                    roleRights.Full_Rights = roleRights.Add_Rights = roleRights.Edit_Rights = roleRights.View_Rights = roleRights.Delete_Rights = false;

                    roleRights.Role_No = RoleNo;
                    
                    roleRights.Menu_No = MenuNo[0].ToString();

                    chkSubMenu = 0;
                    chkSubSubMenu = 0;

                    //M029-M029, M029-M030, M030-M031, M030-M032, M030-M033

                    for (int c = b + 1; c < strMenuNo.Length; c++)
                    {
                        if (strMenuNo[c].Substring(0, 4).Contains(MenuNo[0].ToString()))
                            chkSubMenu += 1;

                        if(strMenuNo[c].Substring(0, 4).Contains(MenuNo[1].ToString()))
                            chkSubSubMenu += 1;
                    }

                    if((MenuNo[0].ToString() == MenuNo[1].ToString() && chkSubMenu > 0) || (MenuNo[0].ToString() != MenuNo[1].ToString() && chkSubSubMenu > 0))
                        continue;
                    else
                    {
                        if (MenuNo[0].ToString() == MenuNo[1].ToString())
                            roleRights.Sub_Menu_No = "";
                        else
                            roleRights.Sub_Menu_No = MenuNo[1].ToString();

                        for (int c = 0; c < strMenuRights.Length; c++)
                        {
                            if (strMenuRights[c] == "Full_Rights")
                                roleRights.Full_Rights = true;
                            else if (strMenuRights[c] == "Add_Rights")
                                roleRights.Add_Rights = true;
                            else if (strMenuRights[c] == "Edit_Rights")
                                roleRights.Edit_Rights = true;
                            else if (strMenuRights[c] == "View_Rights")
                                roleRights.View_Rights = true;
                            else if (strMenuRights[c] == "Delete_Rights")
                                roleRights.Delete_Rights = true;
                        }

                        roleRights.IsActive = true;

                        SPRoleRights requestRoleRights = roleRights;
                        SPRoleRightsResponse responseRoleRights = new SPRoleRightsResponse();

                        var result = PostItemRoleRights("RoleWiseMenuRightsListDotNetAPI", requestRoleRights, responseRoleRights);

                        if (result.Result.Item1.No != null)
                        {
                            flag = true;
                            responseRoleRights = result.Result.Item1;
                        }
                        else
                            flag = false;

                        if (result.Result.Item2.message != null)
                            ed = result.Result.Item2;

                    }
                }
            }

            return flag;
        }

        [Route("GetAllRolesForDDL")]
        public List<SPRolesForDDL> GetAllRolesForDDL()
        {
            API ac = new API();
            List<SPRolesForDDL> roles = new List<SPRolesForDDL>();

            var result = ac.GetData<SPRolesForDDL>("RolesListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                roles = result.Result.Item1.value;

            return roles;
        }

        [Route("GetAllMenusSubMenusOfRole")]
        public List<SPMenusSubMenusOfRole> GetAllMenusSubMenusOfRole(string RoleNo)
        {
            API ac = new API();
            List<SPMenusSubMenusOfRole> menussubmenus = new List<SPMenusSubMenusOfRole>();

            var result = ac.GetData<SPMenusSubMenusOfRole>("RoleWiseMenuRightsListDotNetAPI", "Role_No eq '" + RoleNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                menussubmenus = result.Result.Item1.value;

            return menussubmenus;
        }

        public async Task<(SPRoleRightsResponse, errorDetails)> PostItemRoleRights<SPRoleRightsResponse>(string apiendpoint, SPRoleRights requestModel, SPRoleRightsResponse responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPRoleRightsResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPMenusRightsForDelRes, errorDetails)> PostItemForMenusRightsDel<SPMenusRightsForDelRes>(string apiendpoint, SPMenusRightsForDel requestModel, SPMenusRightsForDelRes responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/DeleteDotNetAPIs_deleterolerisemenurights?Company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPMenusRightsForDelRes>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }
    }
}
