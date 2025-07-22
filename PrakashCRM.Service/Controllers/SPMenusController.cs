using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPMenus")]
    public class SPMenusController : ApiController
    {
        [Route("GetAllMenus")]
        public List<SPMenuList> GetAllMenus(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPMenuList> menus = new List<SPMenuList>();
            if (filter == null)
                filter = "";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPMenuList>("MenuListDotNetAPI", filter);
            else
                result = ac.GetData1<SPMenuList>("MenuListDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                menus = result.Result.Item1.value;

            return menus;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string apiEndPointName, string filter)
        {
            API ac = new API();
            if (filter == null)
                filter = "";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllParentMenuNoForDDL")]
        public List<SPParentMenuNo> GetAllParentMenuNoForDDL()
        {
            API ac = new API();
            List<SPParentMenuNo> parentmenuno = new List<SPParentMenuNo>();

            var result = ac.GetData<SPParentMenuNo>("MenuListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                parentmenuno = result.Result.Item1.value;

            return parentmenuno;
        }

        [Route("GetMenuFromNo")]
        public SPMenus GetMenuFromNo(string No)
        {
            API ac = new API();
            SPMenus menu = new SPMenus();

            var result = ac.GetData<SPMenus>("MenuListDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
                menu = result.Result.Item1.value[0];

            return menu;
        }

        [Route("Menu")]
        public SPMenusResponse Menu(SPMenus Menu, bool isEdit, string MenuNo)
        {
            SPMenus requestMenu = new SPMenus();
            SPMenusResponse responseMenu = new SPMenusResponse();

            requestMenu.Menu_Name = Menu.Menu_Name;
            requestMenu.Parent_Menu_No = Menu.Parent_Menu_No;
            requestMenu.Parent_Menu_Name = Menu.Parent_Menu_Name;
            requestMenu.Type = Menu.Type;
            requestMenu.Serial_No = Menu.Serial_No;
            requestMenu.ClassName = Menu.ClassName;
            requestMenu.IsActive = Menu.IsActive;

            var ac = new API();
            errorDetails ed = new errorDetails();

            var result = (dynamic)null;

            if (isEdit)
            {
                result = PatchItemMenu("MenuListDotNetAPI", requestMenu, responseMenu, "No='" + MenuNo + "'");
                if (result.Result.Item1 != null)
                    responseMenu = result.Result.Item1;
            }
            else
            {
                result = PostItemMenu("MenuListDotNetAPI", requestMenu, responseMenu);

                if (result.Result.Item1 != null)
                    responseMenu = result.Result.Item1;

            }

            if (result.Result.Item1.message != null)
                ed = result.Result.Item1;

            return responseMenu;
        }

        public async Task<(SPMenusResponse, errorDetails)> PostItemMenu<SPMenusResponse>(string apiendpoint, SPMenus requestModel, SPMenusResponse responseModel)
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
                    responseModel = res.ToObject<SPMenusResponse>();

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

        public async Task<(SPMenusResponse, errorDetails)> PatchItemMenu<SPMenusResponse>(string apiendpoint, SPMenus requestModel, SPMenusResponse responseModel, string fieldWithValue)
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
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), baseuri + "(" + fieldWithValue + ")");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("If-Match", "*");

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            request.Content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
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
                    responseModel = res.ToObject<SPMenusResponse>();


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
