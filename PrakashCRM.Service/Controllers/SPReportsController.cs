using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPReports")]
    public class SPReportsController : ApiController
    {
        [HttpPost]
        [Route("GenerateInvData")]
        public string GenerateInvData(string FromDate, string ToDate)
        {
            bool response = false;
            SPInvGenerateDataPost invGeneratereq = new SPInvGenerateDataPost();
            SPInvGenerateDataOData invGenerateres = new SPInvGenerateDataOData();
            errorDetails ed = new errorDetails();

            invGeneratereq.startdate = FromDate;
            invGeneratereq.enddate = ToDate;

            var result = PostItemForGenerateInvData<SPInvGenerateDataOData>("", invGeneratereq, invGenerateres);

            if (result?.Result != null)
            {
                response = result.Result.Item1?.value ?? false;
                ed = result.Result.Item2;
                invGenerateres.errorDetails = ed;
            }
            else
            {
                response = true;
            }
            return response.ToString().ToLower();
        }

        [HttpGet]
        [Route("GetBranchWiseTotal")]
        public List<GetBranchWiseTotalSum> GetBranchWiseTotal()
        {
            API ac = new API();
            List<GetBranchWiseTotalSum> InvBranchWiseTotals = new List<GetBranchWiseTotalSum>();

            var BranchWiseTotalResult = ac.GetData<GetBranchWiseTotalSum>("LocationByQty", "");

            if (BranchWiseTotalResult != null && BranchWiseTotalResult.Result.Item1.value.Count > 0)
                InvBranchWiseTotals = BranchWiseTotalResult.Result.Item1.value;


            return InvBranchWiseTotals;
        }

        [HttpGet]
        [Route("GetInv_ProductGroupsWise")]
        public List<ProductGroupsWise> GetInv_ProductGroupsWise(string branchCode)
        {
            API ac = new API();
            List<ProductGroupsWise> Inv_ProductGroupsWise = new List<ProductGroupsWise>();

            var ProductGroupsWiseResult = ac.GetData<ProductGroupsWise>("IndustryWiseQty", "Location_Filter_FilterOnly eq '" + branchCode + "'");

            if (ProductGroupsWiseResult != null && ProductGroupsWiseResult.Result.Item1.value.Count > 0)
                Inv_ProductGroupsWise = ProductGroupsWiseResult.Result.Item1.value;

            Inv_ProductGroupsWise = Inv_ProductGroupsWise.DistinctBy(a => a.Code).ToList();

            return Inv_ProductGroupsWise;
        }

        [HttpGet]
        [Route("GetInv_ItemWise")]
        public List<ItemWise> GetInv_ItemWise(string branchCode, string pgCode)
        {
            API ac = new API();
            List<ItemWise> Inv_ItemWise = new List<ItemWise>();

            var ItemWiseResult = ac.GetData<ItemWise>("IndustryWiseQty", "Location_Filter_FilterOnly eq '" + branchCode + "' and Code eq '" + pgCode + "'");
            if (ItemWiseResult != null && ItemWiseResult.Result.Item1.value.Count > 0)
                Inv_ItemWise = ItemWiseResult.Result.Item1.value;

            return Inv_ItemWise;
        }
        public async Task<(SPInvGenerateDataOData, errorDetails)> PostItemForGenerateInvData<SPInvGenerateDataOData>(string apiendpoint, SPInvGenerateDataPost requestModel, SPInvGenerateDataOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/InventoryView_GenerateInventoryViewReportDate?company=\'Prakash Company\'");
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
                    //SPSQScheduleOrderOData scheduleOrderOData = res.ToObject<SPSQScheduleOrderOData>();
                    responseModel = res.ToObject<SPInvGenerateDataOData>();

                    //string scheduleOrderData = "{\"value\":" + scheduleOrderOData.value + "}";
                    //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(scheduleOrderData);

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

        [HttpGet]
        [Route("GetInv_Inward")]
        public List<SPInwardDetails> GetInv_Inward(string branchCode, string pgCode, string itemName, string FromDate, string ToDate, string Type, bool Positive)
        {
            API ac = new API();
            List<SPInwardDetails> Inv_Inward = new List<SPInwardDetails>();
            string filter1 = "";
            if (!string.IsNullOrWhiteSpace(branchCode) && !string.IsNullOrWhiteSpace(pgCode))
            {
                filter1 += $" Location_Code eq '{branchCode}' and Item_Category_Code eq '{pgCode}'";
            }
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                filter1 += $" and Item_Description eq '{itemName}'";
            }
            if (Type == "Inward")
            {
                if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                        DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string from = fromDateParsed.ToString("yyyy-MM-dd");
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date ge {from} and Posting_Date le {to} and Positive eq true";
                    }
                }
            }
            else if (string.Equals(Type, "Outward", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                        DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string from = fromDateParsed.ToString("yyyy-MM-dd");
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date ge {from} and Posting_Date le {to} and Positive eq false";
                    }
                }
            }
            else if (Type == "CLStock")
            {
                FromDate = ""; 

                if (!string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date le {to}";
                    }
                }
            }

            var result1 = ac.GetData<SPInwardDetails>("ItemLedgerEntriesDotNetAPI", filter1);

            if (result1 != null && result1.Result.Item1.value.Count > 0)
            {
                Inv_Inward = result1.Result.Item1.value;
            }

            // Calculate No_of_days
            foreach (var item in Inv_Inward)
            {
                if (DateTime.TryParse(item.PCPL_Original_Buying_Date, out DateTime buyingDate) &&
                    DateTime.TryParse(item.Posting_Date, out DateTime postingDate))
                {
                    item.No_of_days = (buyingDate - postingDate).Days;
                }
            }

            return Inv_Inward;
        }


        [HttpGet]
        [Route("GetReservedDetails")]
        public List<SPReservedQtyDetails> GetReservedDetails(string branchCode, string pgCode, string itemName, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPReservedQtyDetails> Inv_ReservedDetails = new List<SPReservedQtyDetails>();
            string filter1 = "";

            if (!string.IsNullOrWhiteSpace(branchCode) && !string.IsNullOrWhiteSpace(pgCode))
            {
                filter1 += $" Location_Code eq '{branchCode}' and Item_Category_Code eq '{pgCode}'";
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                filter1 += $" and Description eq '{itemName}'";
            }

            if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
            {
                if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                    DateTime.TryParse(ToDate, out DateTime toDateParsed))
                {
                    string from = fromDateParsed.ToString("yyyy-MM-dd");
                    string to = toDateParsed.ToString("yyyy-MM-dd");
                    filter1 += $" and Posting_Date ge {from} and Posting_Date le {to}";
                }
            }

            var result1 = ac.GetData<SPReservedQtyDetails>("SalesLinesDotNetAPI", filter1);

            if (result1 != null && result1.Result.Item1.value.Count > 0)
            {
                Inv_ReservedDetails = result1.Result.Item1.value;
            }
            return Inv_ReservedDetails;
        }

        // Customer Ledger Entry Pdf Api
        [HttpGet]
        [Route("PrintCustomerLedgerEntryPostApi")]
        public string PrintCustomerLedgerEntryPostApi(string CustomerNo, string FromDate, string ToDate)
        {
            var PrintCustomerLedgerReportResponse = new PrintCustomerLedgerReportResponse();

            PrintCustomerLedgerReportRequest PrintCustomerLedgerReportRequest = new PrintCustomerLedgerReportRequest
            {
                customerno = CustomerNo,
                fromdate = FromDate,
                todate = ToDate,
            };

            var result = (dynamic)null;
            result = PostCustomerLegerEntryPrint("ReportAPIMngtDotNetAPI_CustomerLedgerReportPrint", PrintCustomerLedgerReportRequest, PrintCustomerLedgerReportResponse);

            var base64PDF = "";
            if (result.Result.Item1 != null)
            {
                base64PDF = result.Result.Item1.value;

            }
            return base64PDF;
        }

        public async Task<(PrintCustomerLedgerReportResponse, errorDetails)> PostCustomerLegerEntryPrint<PrintCustomerLedgerReportRequest>(string apiendpoint, PrintCustomerLedgerReportRequest requestModel, PrintCustomerLedgerReportResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
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
                    responseModel = res.ToObject<PrintCustomerLedgerReportResponse>();

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


        // Customer Report DrowDown Api.
        [HttpGet]
        [Route("GetCustomerReport")]
        public List<SPCustomerReport> GetCustomerReport(string prefix)
        {
            API ac = new API();
            List<SPCustomerReport> customerReports = new List<SPCustomerReport>();
            var result = ac.GetData<SPCustomerReport>("CustomerCardDotNetAPI", "startswith(Name,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)

                customerReports = result.Result.Item1.value;
            return customerReports;

        }

    }
}