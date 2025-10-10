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
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPDashboard")]
    public class SPDashboardController : ApiController
    {
        [Route("GetDashboardDetails")]
        public SPDashboardDetails GetDashboardDetails(string SPCode)
        {
            API ac = new API();
            SPDashboardDetails sdbDetails = new SPDashboardDetails();
            sdbDetails.OrdersCount = 0;
            sdbDetails.InvoicesCount = 0;
            sdbDetails.ContactsCount = 0;
            sdbDetails.CustomersCount = 0;
            sdbDetails.QuotesCount = 0;
            sdbDetails.InquiryCount = 0;

            //var resultOrders = ac.GetData<SPSalesOrdersList>("SalesOrdersListDotNetAPI", "Status in ('Open','Released') and Salesperson_Code eq '" + SPCode + "'");

            //if (resultOrders.Result.Item1.value.Count > 0)
            //    sdbDetails.OrdersCount = resultOrders.Result.Item1.value.Count;

            var resultInvoices = ac.GetData<SPPostedSalesInvoiceList>("PostedSalesInvoicesDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if(resultInvoices.Result.Item1.value != null)
            {
                if (resultInvoices.Result.Item1.value.Count > 0)
                    sdbDetails.InvoicesCount = resultInvoices.Result.Item1.value.Count;
            }
            
            var resultContacts = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'");

            if(resultContacts.Result.Item1.value != null)
            {
                if (resultContacts.Result.Item1.value.Count > 0)
                    sdbDetails.ContactsCount = resultContacts.Result.Item1.value.Count;
            }
            
            var resultCustomers = ac.GetData<SPCustomersList>("CustomerCardDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if(resultCustomers.Result.Item1.value != null)
            {
                if (resultCustomers.Result.Item1.value.Count > 0)
                    sdbDetails.CustomersCount = resultCustomers.Result.Item1.value.Count;
            }
            
            var resultQuotes = ac.GetData<SPSalesQuotesList>("SalesQuotesListDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if(resultQuotes.Result.Item1.value != null)
            {
                if (resultQuotes.Result.Item1.value.Count > 0)
                    sdbDetails.QuotesCount = resultQuotes.Result.Item1.value.Count;
            }

            //get inquiry count for salesperson code = DD then change salesperson filter
            var resultInquiries = ac.GetData<SPInquiry>("InquiryDotNetAPI", "Document_Type eq 'Quote' and PCPL_IsInquiry eq true and Salesperson_Code eq '" + SPCode + "'");

            if (resultInquiries.Result.Item1.value.Count > 0)
                sdbDetails.InquiryCount = resultInquiries.Result.Item1.value.Count;

            return sdbDetails;
        }

        [HttpGet]
        [Route("DailyVisitsDetails")]
        public List<SPDailyVisitDetails> DailyVisitsDetails(string SPCode)
        {
            API ac = new API();
            List<SPDailyVisitDetails> dailyVisitData = new List<SPDailyVisitDetails>();

            var DailyVisitsResult = ac.GetData<SPDailyVisitDetails>("DailyVisitsDotNetAPI", "Entry_Type eq 'ENTRY' and Salesperson_Code eq '" + SPCode + "'");

            if (DailyVisitsResult != null && DailyVisitsResult.Result.Item1.value.Count > 0)
                dailyVisitData = DailyVisitsResult.Result.Item1.value;

            return dailyVisitData;
        }

        [Route("GetAllFeedback")]
        public List<SPFeedBacksForDashboard> GetAllFeedback()
        {
            API ac = new API();
            List<SPFeedBacksForDashboard> feedBacksForDashboard = new List<SPFeedBacksForDashboard>();

            var result = ac.GetData1<SPFeedBacksForDashboard>("FeedbackHeadersListDotNetAPI", "", 0, 5, "No desc");

            if (result != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                feedBacksForDashboard = result.Result.Item1.value;

            return feedBacksForDashboard;
        }

        [Route("GetAllMarketUpdateDetails")]
        public List<SPMarketUpdateList> GetAllMarketUpdateDetails()
        {
            API ac = new API();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd");

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var result = (dynamic)null;

            result = ac.GetData1<SPMarketUpdateList>("MarketUpdateDotNetAPI", "Update_Date eq " + today, 0, 0, "SystemCreatedAt desc");

            if (result.Result.Item1.value.Count > 0)
                marketUpdateList = result.Result.Item1.value;

            for (int i = 0; i < marketUpdateList.Count; i++)
            {
                string[] strDate = marketUpdateList[i].Update_Date.Split('-');
                marketUpdateList[i].Update_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return marketUpdateList;
        }

        [HttpPost]
        [Route("AddMarketUpdate")]
        public SPMarketUpdateResponse AddMarketUpdate(int Entry_No, string Update, string Update_Date, string Employee_Code)//SPMarketUpdate MarketUpdate, bool isEdit, string EntryNo
        {
           
            DateTime UpdateDate = Convert.ToDateTime(Update_Date);
            Update_Date = UpdateDate.ToString("yyyy-MM-dd");

            SPMarketUpdate requestMU = new SPMarketUpdate
            {
                Update_Date = Update_Date,
                Update = Update,
                Employee_Code = Employee_Code
            };
            var responseMU = new SPMarketUpdateResponse();
            dynamic result = null;

            if (Entry_No == 0)
            {
                result = PostItemMarketUpdate("MarketUpdateDotNetAPI", requestMU, responseMU);
            }
            else
            {
                result = PatchItemMarketUpdate("MarketUpdateDotNetAPI", requestMU, responseMU, "Entry_No=" + Entry_No);
            }

            if (result.Result.Item1 != null)
            {
                responseMU = result.Result.Item1;
            }

            return responseMU;
        }

        public async Task<(SPMarketUpdateResponse, errorDetails)> PostItemMarketUpdate<SPMarketUpdateResponse>(string apiendpoint, SPMarketUpdate requestModel, SPMarketUpdateResponse responseModel)
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
                    responseModel = res.ToObject<SPMarketUpdateResponse>();

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

        public async Task<(SPMarketUpdateResponse, errorDetails)> PatchItemMarketUpdate<SPMarketUpdateResponse>(string apiendpoint, SPMarketUpdate requestModel, SPMarketUpdateResponse responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPMarketUpdateResponse>();


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
        // Non Performing Customers list
        [Route("GetNonPerfomingCuslist")]
        public List<SPNonPerfomingCuslist> GetNonPerfomingCuslist()
        {
            API ac = new API();
            List<SPNonPerfomingCuslist> nonperfoming= new List<SPNonPerfomingCuslist>();

            var result = ac.GetData<SPNonPerfomingCuslist>("NonPerformingCustomer", "");

            if (result.Result.Item1.value.Count > 0)
                nonperfoming = result.Result.Item1.value;

            return nonperfoming;
        }

        // Taeget vs Sales list
        //[Route("GetSalespersonData")]
        //public List<SPSelaspersonlist> GetSalespersonData()
        //{
        //    API ac = new API();
        //    List<SPSelaspersonlist> salespersonlist = new List<SPSelaspersonlist>();

        //    string filter = "IsSalesPerson eq true";
        //    var result = ac.GetData<SPSelaspersonlist>("TargetvsSalesReport", filter);

        //    if (result.Result.Item1.value.Count > 0)
        //        salespersonlist = result.Result.Item1.value;
        //    salespersonlist = salespersonlist.DistinctBy(a => a.SalesPerson_Name).ToList();

        //    return salespersonlist;
        //}


        //    [HttpGet]
        //    [Route("GetSupportSP")]
        //    public List<SPSupportSPlist> GetSupportSP()
        //    {
        //        API ac = new API();
        //        List<SPSupportSPlist> supportsp = new List<SPSupportSPlist>();

        //        var result = ac.GetData<SPSupportSPlist>("TargetvsSalesReport", "");

        //        if (result.Result.Item1.value.Count > 0)
        //            supportsp = result.Result.Item1.value;
        //            supportsp = supportsp.DistinctBy(a => a.SalesPerson).ToList();

        //        return supportsp;
        //    }
        //    [HttpGet]
        //    [Route("GetProductData")]
        //    public List<SPProductlist> GetProductData()
        //    {
        //        API ac = new API();
        //        List<SPProductlist> productlists = new List<SPProductlist>();

        //        string filter = "IsSalesPerson eq true and IsProduct eq true and IsIncludeTop10Product eq true";

        //        var result = ac.GetData<SPProductlist>("TargetvsSalesReport", filter);

        //        if (result.Result.Item1.value.Count > 0)
        //            productlists = result.Result.Item1.value;

        //        return productlists;
        //    }

        [HttpGet]
        [Route("GetCombinedSalesData")]
        public CombinedSalesData GetCombinedSalesData()
        {
            API ac = new API();

            CombinedSalesData combinedData = new CombinedSalesData();

            // Salespersons
            //string salespersonFilter = "IsSalesPerson eq true";
            string Filter = "IsSalesPerson eq true and IsProduct eq true and IsIncludTop10Product eq true";
            var salespersonResult = ac.GetData<SPSelaspersonlist>("TargetvsSalesReport", Filter);
            List<SPSelaspersonlist> salespersons = new List<SPSelaspersonlist>();
            if (salespersonResult.Result.Item1.value.Count > 0)
            {
                salespersons = salespersonResult.Result.Item1.value.DistinctBy(a => a.SalesPerson_Name).ToList();
            }

            // SupportSPs
            var supportResult = ac.GetData<SPSupportSPlist>("TargetvsSalesReport", "");
            List<SPSupportSPlist> supportSPs = new List<SPSupportSPlist>();
            if (supportResult.Result.Item1.value.Count > 0)
            {
                supportSPs = supportResult.Result.Item1.value.DistinctBy(a => a.SalesPerson).ToList();
            }

            //  Products
            string productFilter = "IsSalesPerson eq true and IsProduct eq true and IsIncludTop10Product eq true";
            var productResult = ac.GetData<SPProductlist>("TargetvsSalesReport", productFilter);
            List<SPProductlist> products = new List<SPProductlist>();
            if (productResult.Result.Item1.value.Count > 0)
            {
                products = productResult.Result.Item1.value;
            }

            // Combined Model
            combinedData.Salespersons = salespersons;
            combinedData.SupportSPs = supportSPs;
            combinedData.Products = products;

            return combinedData;
        }
        [HttpGet]
        [Route("GetTodayVisit")]
        public List<SPTodayVisitlist> GetTodayVisit()
        {
            API ac = new API();
            List<SPTodayVisitlist> todayVisit = new List<SPTodayVisitlist>();

            //string filter = $"Salesperson_Code eq '{salespersonCode}' and Date eq '{date}'";
            var result = ac.GetData<SPTodayVisitlist>("DailyVisitsDotNetAPI","");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                todayVisit = result.Result.Item1.value;

            return todayVisit;
        }
    }
}
