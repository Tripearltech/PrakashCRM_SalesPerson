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
        [HttpGet]
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


        [HttpGet]
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

        public async Task<JsonResult> GetInv_Inward(string branchCode, string pgCode, string itemName, string FromDate, string ToDate, string Type, bool Positive)
        {
            string baseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string apiUrl = baseUrl + "SPReports/GetInv_Inward?" + "branchCode=" + HttpUtility.UrlEncode(branchCode) + "&pgCode=" + HttpUtility.UrlEncode(pgCode) + "&itemName=" + HttpUtility.UrlEncode(itemName) + "&FromDate=" + HttpUtility.UrlEncode(FromDate) + "&ToDate=" + HttpUtility.UrlEncode(ToDate) + "&Type=" + HttpUtility.UrlEncode(Type) + "&Positive=" + HttpUtility.UrlEncode(Positive.ToString());

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
        public async Task<JsonResult> GetReservedDetails(string branchCode, string pgCode, string itemName, string FromDate, string ToDate)
        {
            string baseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string apiUrl = baseUrl + "SPReports/GetReservedDetails?" + "&branchCode=" + HttpUtility.UrlEncode(branchCode) + "&pgCode=" + HttpUtility.UrlEncode(pgCode) + "&itemName=" + HttpUtility.UrlEncode(itemName) + "&FromDate=" + HttpUtility.UrlEncode(FromDate) + "&ToDate=" + HttpUtility.UrlEncode(ToDate);

            HttpClient client = new HttpClient();
            List<SPReservedQtyDetails> Inv_ReservedDetails = new List<SPReservedQtyDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_ReservedDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPReservedQtyDetails>>(data);
            }

            return Json(Inv_ReservedDetails, JsonRequestBehavior.AllowGet);
        }

        // Customer Ledger Entry Report

        [HttpPost]
        public async Task<string> PrintCustomerLedgerEntryPostApi(string CustomerNo, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() +
                $"SPReports/PrintCustomerLedgerEntryPostApi?CustomerNo={CustomerNo}&FromDate={FromDate}&ToDate={ToDate}";

            HttpClient client = new HttpClient();
            string savedPath = "";

            // Generate unique file name using CustomerNo + FromDate + ToDate
            string expectedFileName = $"{CustomerNo}_{FromDate}_{ToDate}".Replace("/", "-").Replace(":", "-").Replace(" ", "_");

            string path = Server.MapPath("~/CustomerLedgerEntryPrint/");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            bool flag = false;

            // Check if file already exists
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name).Equals(expectedFileName, StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    savedPath = smFile.Name;
                    break;
                }
            }

            if (flag)
            {
                return savedPath;
            }
            else
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var base64PDF = JsonConvert.DeserializeObject<string>(data);
                    savedPath = SaveBase64ToPdf(base64PDF, "CustomerLedgerEntryPrint", expectedFileName);
                }
                else
                {
                    var JsonData = await response.Content.ReadAsStringAsync(); // For debugging/logging if needed
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