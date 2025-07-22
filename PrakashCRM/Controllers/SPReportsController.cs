using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PrakashCRM.Data.Models;

namespace PrakashCRM.Controllers
{
    public class SPReportsController : Controller
    {
        // GET: SPReports
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult InventoryView()
        {
            return View();
        }

        public async Task<JsonResult> GetBranchWiseTotal()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetBranchWiseTotal";

            HttpClient client = new HttpClient();
            List<GetBranchWiseTotalSum> InvBranchWiseTotals = new List<GetBranchWiseTotalSum>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                InvBranchWiseTotals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GetBranchWiseTotalSum>>(data);
            }

            return Json(InvBranchWiseTotals, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInv_ProductGroupsWise(string branchCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInv_ProductGroupsWise?branchCode=" + branchCode;
            HttpClient Client = new HttpClient();
            List<ProductGroupsWise> Inv_ProductGroupsWise = new List<ProductGroupsWise>();
            Client.BaseAddress = new Uri(apiUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await Client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content?.ReadAsStringAsync();
                Inv_ProductGroupsWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductGroupsWise>>(data);
            }
            return Json(Inv_ProductGroupsWise, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetInv_ItemWise(string branchCode, string pgCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInv_ItemWise?branchCode=" + branchCode + "&pgCode=" + pgCode;

            HttpClient client = new HttpClient();
            List<ItemWise> Inv_ItemWise = new List<ItemWise>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_ItemWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemWise>>(data);
            }

            return Json(Inv_ItemWise, JsonRequestBehavior.AllowGet);
        }
    }
}