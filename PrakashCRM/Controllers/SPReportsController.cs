using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    public class SPReportsController : Controller
    {
        public string FromDate { get; private set; }
        public string ToDate { get; private set; }

        // GET: SPReports
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult InventoryView()
        {
            return View();
        }
        public ActionResult CustomerLedgerEntry()
        {
            var Model = new SPCustomerReport
            {
                Name = "",
                No = "",   
            };

            return View(Model);
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
        public async Task<JsonResult> GetInv_Inward(string Entry_Type, string Document_Type,string branchCode,string pgCode,string itemName)
        {
            string baseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string apiUrl = baseUrl + "SPReports/GetInv_Inward?" + "Entry_Type=" + HttpUtility.UrlEncode(Entry_Type) + "&Document_Type=" + HttpUtility.UrlEncode(Document_Type) + "&branchCode=" + HttpUtility.UrlEncode(branchCode) + "&pgCode=" + HttpUtility.UrlEncode(pgCode) + "&itemName=" + HttpUtility.UrlEncode(itemName);

            HttpClient client = new HttpClient();
            List<SPInwardDetails> Inv_Inward = new List<SPInwardDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_Inward = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPInwardDetails>>(data);
            }

            return Json(Inv_Inward, JsonRequestBehavior.AllowGet);
        }

        // Customer Ledger Entry Report

        [HttpPost]
        public async Task<string> PrintCustomerLedgerEntryPostApi(string CustomerNo, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/";

            apiUrl = apiUrl + "PrintCustomerLedgerEntryPostApi?CustomerNo=" + CustomerNo + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            string savedPath = "";

            string path = Server.MapPath("~/CustomerLedgerEntryPrint/");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            bool flag = false;
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == CustomerNo)
                {
                    flag = true;
                    savedPath = smFile.Name;
                    break;
                }
            }

            if (flag)
                return savedPath;
            else
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var base64PDF = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(data);
                    savedPath = SaveBase64ToPdf(base64PDF, "CustomerLedgerEntryPrint", CustomerNo);
                }
                else
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                }
                return savedPath;
            }

        }

        public string SaveBase64ToPdf(string base64String, string relativeFolderPath, string fileNameWithoutExtension)
        {
            string projectRoot = Server.MapPath("~/");
            string fullFolderPath = Path.Combine(projectRoot, relativeFolderPath);
            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }
            string filePath = Path.Combine(fullFolderPath, $"{fileNameWithoutExtension}.pdf");
            byte[] pdfBytes = Convert.FromBase64String(base64String);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            return $"{fileNameWithoutExtension}.pdf";
        }

        // Customer Report DrowDown Api.

        [HttpPost]
        public async Task<JsonResult> GetCustomerReport(string prefix)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetCustomerReport?prefix=" + prefix;

            HttpClient client = new HttpClient();
            List<SPCustomerReport> customerReports = new List<SPCustomerReport>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                customerReports = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerReport>>(data);
            }

            return Json(customerReports, JsonRequestBehavior.AllowGet);
        }

    }
}