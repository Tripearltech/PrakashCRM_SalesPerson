﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPSalesQuotesController : Controller
    {
        // GET: SPSalesQuotes
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SalesQuote()
        {
            //List<SPSQInvQtyReserve> invQtyReserve = new List<SPSQInvQtyReserve>();

            //invQtyReserve.Add(new SPSQInvQtyReserve()
            //{
            //    QuoteNo = "Quote123",
            //    LineNo = 10000,
            //    ItemNo = "TRD00032",
            //    LotNo = "LOT00065",
            //    Qty = 1,
            //    LocationCode = "AHM DOM"

            //});

            //invQtyReserve.Add(new SPSQInvQtyReserve()
            //{
            //    QuoteNo = "Quote123",
            //    LineNo = 10000,
            //    ItemNo = "TRD00032",
            //    LotNo = "LOT00082",
            //    Qty = 1,
            //    LocationCode = "AHM DOM"

            //});

            //SPSQInvQtyReservePost invQtyReservePost = new SPSQInvQtyReservePost();
            //string ObjString_ = JsonConvert.SerializeObject(invQtyReserve);
            //string txtString = ObjString_.Replace("\"", "'");
            //invQtyReservePost.text = txtString;
            //string txtString_ = JsonConvert.SerializeObject(invQtyReservePost);
            ////ObjString_ = ObjString_.Replace("\"text\"", '"' + "text" + '"');
            //var content = new StringContent(txtString_, Encoding.UTF8, "application/json");

            //if (SQNo != "" || Session["SalesQuoteNo"] != null)
            //{
            //    if (Session["SalesQuoteNo"] == null)
            //    {
            //        Session["SalesQuoteNo"] = SQNo;
            //        Session["isSalesQuoteEdit"] = "true";
            //    }
            //}

            return View();
        }

        [HttpPost]
        public async Task<string> PostSalesQuote(SPSQHeaderDetails salesquoteheader)
        {
            bool flag = false;
            string SQNoLineNoDetails = "";
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";
            salesquoteheader.SQApprovalFormURL = ConfigurationManager.AppSettings["SPPortalUrl"].ToString() + "SPSalesQuotes/SalesQuote";
            string apiUrl1, apiUrl2;
            apiUrl1 = apiUrl2 = "";

           DateTime quoteDate = Convert.ToDateTime(salesquoteheader.OrderDate);
            salesquoteheader.OrderDate = quoteDate.ToString("yyyy-MM-dd");

            DateTime validUntillDate = Convert.ToDateTime(salesquoteheader.ValidUntillDate);
            salesquoteheader.ValidUntillDate = validUntillDate.ToString("yyyy-MM-dd");

            if (salesquoteheader.TargetDate != "" && salesquoteheader.TargetDate != null)
            {
                DateTime targetDate = Convert.ToDateTime(salesquoteheader.TargetDate);
                salesquoteheader.TargetDate = targetDate.ToString("yyyy-MM-dd");
            }
            else
                salesquoteheader.TargetDate = "";

            salesquoteheader.SalespersonCode = Session["loggedInUserSPCode"].ToString();

            string SPName = Session["loggedInUserFName"].ToString() + " " + Session["loggedInUserLName"].ToString();

            string approvalFormatFile = "";
            //byte[] zipApprovalFormatFile = {};
            if (salesquoteheader.ApprovalFor != null || salesquoteheader.ApprovalFor != "")
            {
                StreamReader reader = new StreamReader(Server.MapPath("~/Files/CreditLimitEmail-1.html"));
                approvalFormatFile = reader.ReadToEnd();
                approvalFormatFile = approvalFormatFile.Replace(System.Environment.NewLine, "");
                //approvalFormatFile = Regex.Replace(approvalFormatFile, @"\s+", "");
                approvalFormatFile = approvalFormatFile.Replace("{ApprovalFileFormatURL}", GetApprovalFileFormatUrl("Files/logo-3.jfif"));
                salesquoteheader.zipApprovalFormatFile = Zip(approvalFormatFile);
            }

            apiUrl2 = apiUrl + "SalesQuote?LoggedInSPUserEmail=" + Session["loggedInUserEmail"].ToString() + "&SPName=" + SPName; //+ "&ApprovalFormatFile=" + approvalFormatFile;

            SPInquiry responseInquiry = new SPInquiry();
            SPSQHeader responseSQ = new SPSQHeader();

            HttpClient client1 = new HttpClient();

            client1.BaseAddress = new Uri(apiUrl2);
            client1.DefaultRequestHeaders.Accept.Clear();
            client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(salesquoteheader);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl2),
                Content = content
            };

            HttpResponseMessage response1 = await client1.SendAsync(request);
            if (response1.IsSuccessStatusCode)
            {
                flag = true;
                var data = await response1.Content.ReadAsStringAsync();
                responseSQ = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQHeader>(data);

                if (responseSQ.errorDetails.isSuccess)
                {
                    SQNoLineNoDetails = responseSQ.No + "," + responseSQ.ItemLineNo;
                    //resMsg = flag.ToString() + "_" + responseSQ.No;
                    //Session["SalesQuoteAction"] = "Created";
                    Session["SalesQuoteNo"] = responseSQ.No;
                }
                else
                    Session["SalesQuoteActionErr"] = responseSQ.errorDetails.message;
            }

            return SQNoLineNoDetails;
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public string GetApprovalFileFormatUrl(string imagePath)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(Server.MapPath("~/" + imagePath));
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            Byte[] bytes = new Byte[memoryStream.Length];
            memoryStream.Position = 0;
            memoryStream.Read(bytes, 0, (int)bytes.Length);
            string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
            string imageUrl = "data:image/png;base64," + base64String;
            return imageUrl;
        }

        public bool NullSalesQuoteSession()
        {
            bool isSessionNull = false;

            Session["SalesQuoteActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        [HttpPost]
        public ActionResult SalesQuote(SPSQHeaderDetails salesQuote)
        {

            return RedirectToAction("SalesQuote");

        }

        [HttpPost]
        public async Task<string> ScheduleOrder(SPSQScheduleOrderDetails scheduleorder)
        {
            string res = "";
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";
            apiUrl += "ScheduleOrder";
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(scheduleorder);
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
                res = await response.Content.ReadAsStringAsync();
            }

            return res;
        }

        [HttpPost]
        public async Task<bool> AddUpdateOnSaveProd(SPSQLinesPost salesQuoteLine)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl += "AddUpdateOnSaveProd";

            bool flag = false;

            HttpClient client = new HttpClient();
            SPSQLines responseSQLines = new SPSQLines();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(salesQuoteLine);
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
                responseSQLines = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQLines>(data);
                flag = true;
            }
            
            return flag;

        }

        public async Task<JsonResult> GetAllSQLinesOfSQ(string QuoteNo, string SQLinesFor)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetAllSQLinesOfSQ?QuoteNo=" + QuoteNo + "&SQLinesFor=" + SQLinesFor;

            HttpClient client = new HttpClient();
            List<SPSQLines> SQLines = new List<SPSQLines>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                SQLines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQLines>>(data);
            }

            return Json(SQLines, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInterestRate()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetInterestRate";

            HttpClient client = new HttpClient();
            SPSalesReceivableSetup resSalesReceivableSetups = new SPSalesReceivableSetup();
            double interestRate = 0;

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                resSalesReceivableSetups = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSalesReceivableSetup>(data);
                interestRate = resSalesReceivableSetups.PCPL_Interest_Rate_Percent;
            }

            return Json(resSalesReceivableSetups, JsonRequestBehavior.AllowGet);
            //return interestRate.ToString();
        }

        public bool NullSQSession()
        {
            bool isSessionNull = false;

            Session["SalesQuoteAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public ActionResult SalesQuoteList()
        {
            //Session["isSalesQuoteEdit"] = "false";
            Session["SalesQuoteNo"] = "";
            Session["savedNoSeriesCode"] = "";

            return View();
        }

        public async Task<JsonResult> GetSalesQuoteListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            string orderByField = "";

            switch (orderBy)
            {
                case 3:
                    orderByField = "No " + orderDir;
                    break;
                case 4:
                    orderByField = "Order_Date " + orderDir + ",No " + orderDir;
                    break;
                case 5:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
            }

            string LoggedInUserRole = "";
            LoggedInUserRole = Session["loggedInUserRole"].ToString();

            if (Session["SPCodesOfReportingPersonUser"].ToString() == "")
                apiUrl = apiUrl + "GetAllSalesQuotes?LoggedInUserRole=" + LoggedInUserRole + "&SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;
            else
            {
                string SPCode = Session["loggedInUserSPCode"].ToString() + "," + Session["SPCodesOfReportingPersonUser"].ToString();
                apiUrl = apiUrl + "GetAllSalesQuotes?LoggedInUserRole=" + LoggedInUserRole + "&SPCode=" + SPCode + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;
            }

            HttpClient client = new HttpClient();
            List<SPSalesQuotesList> salesquotes = new List<SPSalesQuotesList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesquotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSalesQuotesList>>(data);

            }
            return Json(salesquotes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SalesQuoteStatus()
        {
            return View();
        }
        public ActionResult DispatchDetails()
        {
            return View();
        }

        public async Task<JsonResult> GetSQListDataForApproveReject(string UserRoleORReportingPerson, int orderBy, string orderDir, string filter, int skip, int top)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            string orderByField = "";

            switch (orderBy)
            {
                case 3:
                    orderByField = "No " + orderDir;
                    break;
                case 4:
                    orderByField = "Order_Date " + orderDir;
                    break;
                case 5:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                case 6:
                    orderByField = "Payment_Terms_Code " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetSQListDataForApproveReject?LoggedInUserNo=" + Session["loggedInUserNo"].ToString() + "&UserRoleORReportingPerson=" + UserRoleORReportingPerson + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSQListForApproveReject> sqList = new List<SPSQListForApproveReject>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                sqList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQListForApproveReject>>(data);
            }

            return Json(sqList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSalesLineItems(string DocumentNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetSalesLineItems?DocumentNo=" + DocumentNo;

            HttpClient client = new HttpClient();
            List<SPSQLines> lineitems = new List<SPSQLines>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                lineitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQLines>>(data);
            }

            return Json(lineitems, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetOrderedQtyDetails(string SQNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetOrderedQtyDetails?SQNo=" + SQNo;

            HttpClient client = new HttpClient();
            SPSQOrderedQtyDetails orderedQtyDetails = new SPSQOrderedQtyDetails();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                orderedQtyDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQOrderedQtyDetails>(data);
            }

            return Json(orderedQtyDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInvoicedQtyDetails(string SQNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetInvoicedQtyDetails?SQNo=" + SQNo;

            HttpClient client = new HttpClient();
            List<SPSQInvoicedQtyDetails> invoicedQtyDetails = new List<SPSQInvoicedQtyDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                invoicedQtyDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQInvoicedQtyDetails>>(data);
            }

            return Json(invoicedQtyDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInProcessQtyDetails(string SQNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetInProcessQtyDetails?SQNo=" + SQNo;

            HttpClient client = new HttpClient();
            List<SPSQInProcessQtyDetails> inProcessQtyDetails = new List<SPSQInProcessQtyDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inProcessQtyDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQInProcessQtyDetails>>(data);
            }

            return Json(inProcessQtyDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllDepartmentForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/GetAllDepartmentForDDL";

            HttpClient client = new HttpClient();
            List<Departments> departments = new List<Departments>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                departments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Departments>>(data);
            }

            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<bool> AddNewContactPerson(SPContact CPersonDetails)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/AddNewContactPerson";
            bool flag = false;

            HttpClient client = new HttpClient();
            SPContact CPersonDetailsRes = new SPContact();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(CPersonDetails);
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
                CPersonDetailsRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPContact>(data);
                flag = true;
            }

            return flag;
        }

        public async Task<JsonResult> GetAreasByPincodeForDDL(string Pincode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAreasByPincodeForDDL?Pincode=" + Pincode;

            HttpClient client = new HttpClient();
            List<Area> areas = new List<Area>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                areas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Area>>(data);
            }

            return Json(areas, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDetailsByCode(string Code)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetDetailsByCode?Code=" + Code;

            HttpClient client = new HttpClient();
            List<PostCodes> postcodes = new List<PostCodes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                postcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PostCodes>>(data);
            }

            return Json(postcodes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<string> AddNewBillToAddress(SPInqNewShiptoAddress newShiptoAddress)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/AddNewBillToAddress";
            string resMsg = "";

            HttpClient client = new HttpClient();
            SPInqNewShiptoAddressRes newShiptoAddressRes = new SPInqNewShiptoAddressRes();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(newShiptoAddress);
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
                newShiptoAddressRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPInqNewShiptoAddressRes>(data);

                if (!newShiptoAddressRes.errorDetails.isSuccess)
                    resMsg = newShiptoAddressRes.errorDetails.message;
            }

            return resMsg;
        }

        [HttpPost]
        public async Task<string> AddNewDeliveryToAddress(SPInqNewShiptoAddress newShiptoAddress)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/AddNewDeliveryToAddress";
            string resMsg = "";

            HttpClient client = new HttpClient();
            SPInqNewJobtoAddressRes newJobtoAddressRes = new SPInqNewJobtoAddressRes();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(newShiptoAddress);
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
                newJobtoAddressRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPInqNewJobtoAddressRes>(data);

                if (!newJobtoAddressRes.errorDetails.isSuccess)
                    resMsg = newJobtoAddressRes.errorDetails.message;
            }

            return resMsg;
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Sell_to_Contact " + orderDir;
                    break;
                case 5:
                    orderByField = "Posting_Date " + orderDir;
                    break;
                case 6:
                    orderByField = "Due_Date " + orderDir;
                    break;
                case 7:
                    orderByField = "Amount " + orderDir;
                    break;
            }

            apiUrl = apiUrl + "GetAllSalesQuotes?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSalesQuotesList> salesquotes = new List<SPSalesQuotesList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesquotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSalesQuotesList>>(data);
            }

            DataTable dt = ToDataTable(salesquotes);

            //Name of File  
            string fileName = "SalesQuoteList.xlsx";
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

        public async Task<JsonResult> GetAllContactsOfCompany(string companyName)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetAllContactsOfCompany?companyName=" + companyName;

            HttpClient client = new HttpClient();
            List<SPSQContacts> contacts = new List<SPSQContacts>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQContacts>>(data);
                contacts = contacts.OrderBy(a => a.Name).ToList();
            }

            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetCreditLimitAndCustDetails(string companyName)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetCreditLimitAndCustDetails?companyName=" + companyName;

            HttpClient client = new HttpClient();
            SPSQCreditLimitAndCustDetails creditlimitcustdetails = new SPSQCreditLimitAndCustDetails();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                creditlimitcustdetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQCreditLimitAndCustDetails>(data);
            }

            return Json(creditlimitcustdetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInventoryDetails(string ProdNo, string LocCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetInventoryDetails?ProdNo=" + ProdNo + "&LocCode=" + LocCode;

            HttpClient client = new HttpClient();
            List<SPSQInvDetailsRes> invdetails = new List<SPSQInvDetailsRes>();
            SPSQInvDetails invdetails_ = new SPSQInvDetails();
            double availableQty = 0;
            double mrp = 0;

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                invdetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQInvDetailsRes>>(data);

                //if (invdetails.Count > 0)
                //{
                //    for (int i = 0; i < invdetails.Count; i++)
                //    {
                //        availableQty += invdetails[i].Remaining_Quantity;

                //        if (invdetails[i].PCPL_MRP_Price > 0)
                //            mrp = invdetails[i].PCPL_MRP_Price;
                //    }

                //    invdetails_.Remaining_Quantity = availableQty;
                //    invdetails_.PCPL_Manufacturer_Name = invdetails[0].PCPL_Manufacturer_Name;
                //    invdetails_.Lot_No = invdetails[0].Lot_No;
                //    invdetails_.PCPL_MRP_Price = mrp;
                //}

            }

            return Json(invdetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetLastThreeTrans(string CustNo, string LocCode, string TransType, string ProdNo)
        {
            string apiUrl;

            apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetTransSalesInvoiceLine?CustNo=" + CustNo + "&LocCode=" + LocCode + "&TransType=" + TransType + "&ProdNo=" + ProdNo;

            HttpClient client = new HttpClient();
            List<SPSQSalesInvoiceDetails> salesInvoiceDetails = new List<SPSQSalesInvoiceDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesInvoiceDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQSalesInvoiceDetails>>(data);

            }

            return Json(salesInvoiceDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPurDiscountDetails(string ProdNo)
        {
            string apiUrl;

            apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetPurDiscountDetails?ProdNo=" + ProdNo;

            HttpClient client = new HttpClient();
            List<SPSQPurDiscountDetails> purDisDetails = new List<SPSQPurDiscountDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                purDisDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQPurDiscountDetails>>(data);

            }

            return Json(purDisDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInquiriesForDDL(string SPCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetInquiriesForDDL?SPCode=" + SPCode;

            HttpClient client = new HttpClient();
            List<SPSQInquiryNos> inquirynos = new List<SPSQInquiryNos>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inquirynos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQInquiryNos>>(data);
            }

            return Json(inquirynos, JsonRequestBehavior.AllowGet);
        }

        //public async Task<JsonResult> GetNoSeriesForDDL()
        //{
        //    string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

        //    apiUrl = apiUrl + "GetNoSeriesForDDL";

        //    HttpClient client = new HttpClient();
        //    List<SPNoSeries> locations = new List<SPNoSeries>();

        //    client.BaseAddress = new Uri(apiUrl);
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage response = await client.GetAsync(apiUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var data = await response.Content.ReadAsStringAsync();
        //        locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPNoSeries>>(data);
        //    }

        //    return Json(locations, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public async Task<JsonResult> GetPincodeForDDL(string prefix)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/GetPincodeForDDL?prefix=" + prefix;

            HttpClient client = new HttpClient();
            List<PostCodes> pincode = new List<PostCodes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                pincode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PostCodes>>(data);
            }

            return Json(pincode, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetLocationsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetLocationsForDDL";

            HttpClient client = new HttpClient();
            List<SPLocations> locations = new List<SPLocations>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPLocations>>(data);
                locations = locations.OrderBy(a => a.Name).ToList();
            }

            return Json(locations, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPaymentTermsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetPaymentTermsForDDL";

            HttpClient client = new HttpClient();
            List<SPSQPaymentTerms> paymentterms = new List<SPSQPaymentTerms>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                paymentterms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQPaymentTerms>>(data);
                paymentterms = paymentterms.OrderBy(a => a.Description).ToList();
            }

            return Json(paymentterms, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetItemVendorsForDDL(string ProdNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetItemVendorsForDDL?ProdNo=" + ProdNo;

            HttpClient client = new HttpClient();
            List<SPSQItemVendors> itemVendors = new List<SPSQItemVendors>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                itemVendors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQItemVendors>>(data);
                itemVendors = itemVendors.OrderBy(a => a.Vendor_Name).ToList();
            }

            return Json(itemVendors, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetIncoTermsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetIncoTermsForDDL";

            HttpClient client = new HttpClient();
            List<SPSQShipmentMethods> shipmentmethods = new List<SPSQShipmentMethods>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                shipmentmethods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQShipmentMethods>>(data);
                shipmentmethods = shipmentmethods.OrderBy(a => a.Description).ToList();
            }

            return Json(shipmentmethods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetVendorsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetVendorsForDDL";

            HttpClient client = new HttpClient();
            List<SPSQVendors> vendors = new List<SPSQVendors>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                vendors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQVendors>>(data);
                vendors = vendors.OrderBy(a => a.Name).ToList();
            }

            return Json(vendors, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPaymentMethodsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetPaymentMethodsForDDL";

            HttpClient client = new HttpClient();
            List<SPSQPaymentMethods> paymentmethods = new List<SPSQPaymentMethods>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                paymentmethods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQPaymentMethods>>(data);
            }

            return Json(paymentmethods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetTransportMethodsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetTransportMethodsForDDL";

            HttpClient client = new HttpClient();
            List<SPSQTransportMethods> transportMethods = new List<SPSQTransportMethods>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                transportMethods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQTransportMethods>>(data);
                transportMethods = transportMethods.OrderBy(a => a.Description).ToList();
            }

            return Json(transportMethods, JsonRequestBehavior.AllowGet);
        }

        //public async Task<JsonResult> GetProductDetails(string productName)
        //{
        //    string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

        //    apiUrl = apiUrl + "GetProductDetails?productName=" + productName;

        //    HttpClient client = new HttpClient();
        //    SPSQProductDetails productdetails = new SPSQProductDetails();

        //    client.BaseAddress = new Uri(apiUrl);
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage response = await client.GetAsync(apiUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var data = await response.Content.ReadAsStringAsync();
        //        productdetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQProductDetails>(data);
        //    }

        //    return Json(productdetails, JsonRequestBehavior.AllowGet);
        //}

        public async Task<JsonResult> GetProductDetails(string productName)
        {
            string apiUrl, apiUrl1;
            apiUrl = apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetProductDetails?productName=" + productName;

            HttpClient client = new HttpClient();
            SPSQProductDetails productdetails = new SPSQProductDetails();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                productdetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQProductDetails>(data);
            }

            //

            apiUrl1 = apiUrl1 + "GetProductPackingStyle?prodNo=" + productdetails.No;

            HttpClient client1 = new HttpClient();

            List<SPSQProductPackingStyle> packingStyle = new List<SPSQProductPackingStyle>();

            client1.BaseAddress = new Uri(apiUrl1);
            client1.DefaultRequestHeaders.Accept.Clear();
            client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response1 = await client1.GetAsync(apiUrl1);
            if (response1.IsSuccessStatusCode)
            {
                var data = await response1.Content.ReadAsStringAsync();
                packingStyle = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQProductPackingStyle>>(data);

                productdetails.ProductPackingStyle = packingStyle.OrderBy(a => a.Packing_Style_Description).ToList();
            }

            //

            return Json(productdetails, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<string> AddUpdateOnAddQtyChange(SPSQHeaderAndLinesFromAddQtyChange salesQuoteFromAddQtyChange)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "SalesQuote";

            //consignee address
            //data.CustName + "," + data.Address + " " + data.City + "-" + data.Post_Code;

            SPSQHeaderAndLinesPost salesQuote = new SPSQHeaderAndLinesPost();
            salesQuote.Location_Code = salesQuoteFromAddQtyChange.Location_Code;
            //salesQuote.Inquiry_Type = salesQuoteFromAddQtyChange.Inquiry_Type;
            salesQuote.Sell_to_Customer_No = salesQuoteFromAddQtyChange.Sell_to_Customer_No;
            salesQuote.Sell_to_Contact = salesQuoteFromAddQtyChange.Sell_to_Contact;
            salesQuote.Order_Date = salesQuoteFromAddQtyChange.Order_Date;

            string[] custAddDetails = salesQuoteFromAddQtyChange.ConsigneeAddress.ToString().Split(',');
            //string[] custAdd = custAddDetails[1].ToString().Split(',');
            string[] custPostcode = custAddDetails[2].ToString().Split('-');
            string[] custAddress = new string[3];

            //custAddress[0] = custAdd[0];
            //custAddress[1] = custPostcode[0];
            //custAddress[2] = custPostcode[1];

            custAddress[0] = custAddDetails[1];
            custAddress[1] = custPostcode[0];
            custAddress[2] = custPostcode[1];

            salesQuote.Sell_to_Address = custAddress[0];
            salesQuote.Sell_to_City = custAddress[1];
            salesQuote.Sell_to_Post_Code = custAddress[2];
            salesQuote.Salesperson_Code = Session["loggedInUserSPCode"].ToString();
            salesQuote.Prod_No = salesQuoteFromAddQtyChange.Prod_No;
            salesQuote.Document_No = salesQuoteFromAddQtyChange.Document_No;
            salesQuote.Unit_Price = salesQuoteFromAddQtyChange.Unit_Price;
            salesQuote.Quantity = salesQuoteFromAddQtyChange.Quantity;
            salesQuote.Line_Amount = salesQuoteFromAddQtyChange.Unit_Price * salesQuoteFromAddQtyChange.Quantity;
            salesQuote.Sell_to_Customer_No = salesQuoteFromAddQtyChange.Sell_to_Customer_No;
            salesQuote.Prod_No = salesQuoteFromAddQtyChange.Prod_No;

            HttpClient client = new HttpClient();
            SPSQHeader responseSQHeader = new SPSQHeader();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(salesQuote);
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
                responseSQHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQHeader>(data);
            }

            return "";
        }

        public async Task<int> GetSQDetailsBySQNo(string DocumentNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetSQDetailsBySQNo?DocumentNo=" + DocumentNo;

            HttpClient client = new HttpClient();
            List<SPSQLines> lineitems = new List<SPSQLines>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                lineitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQLines>>(data);
            }

            int SQDetailsCnt = Convert.ToInt32(lineitems.Count);

            return SQDetailsCnt;
        }

        /*public async Task<JsonResult> GetDetailsOfInInventoryLineItems(string itemInvStatus)
        {

            return Json(productdetails, JsonRequestBehavior.AllowGet);
        }*/

        /*[HttpPost]
        public async Task<string> UpdateValueForTotalCost(SPSQHeaderAndLinesFromAddQtyChange salesQuoteFromAddQtyChange)
        {

        }*/

        //[HttpPost]
        //public async Task<string> PrintPreviewSQ(string SQNo)
        //{
        //    string path = Server.MapPath("~/SQFilesPrint/");

        //    DirectoryInfo di = new DirectoryInfo(path);
        //    FileInfo[] smFiles = di.GetFiles("*.*");
        //    bool flag = false;
        //    foreach (FileInfo smFile in smFiles)
        //    {
        //        if (Path.GetFileNameWithoutExtension(smFile.Name) == SQNo)
        //        {
        //            flag = true;
        //            Process proc = new Process();
        //            proc.StartInfo.UseShellExecute = true;
        //            proc.StartInfo.FileName = path + smFile.Name;
        //            proc.Start();
        //            break;
        //        }
        //    }

        //    if (!flag)
        //    {

        //        string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

        //        apiUrl = apiUrl + "GetSQDetailsForPrintPreview?SQNo=" + SQNo;

        //        HttpClient client = new HttpClient();

        //        client.BaseAddress = new Uri(apiUrl);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //        HttpResponseMessage response = await client.GetAsync(apiUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var data = await response.Content.ReadAsStringAsync();
        //            string base64str = data.Replace("\"","");

        //            byte[] bytes = Convert.FromBase64String(base64str);
        //            System.IO.FileStream stream = new FileStream(path + SQNo + ".pdf", FileMode.CreateNew);
        //            System.IO.BinaryWriter writer = new BinaryWriter(stream);
        //            writer.Write(bytes, 0, bytes.Length);
        //            writer.Close();

        //            Process proc = new Process();
        //            proc.StartInfo.UseShellExecute = true;
        //            proc.StartInfo.FileName = path + SQNo + ".pdf";
        //            proc.Start();

        //        }

        //    }

        //    return "";
        //}

        [HttpGet]
        public async Task<JsonResult> GetShortcloseReasons()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/GetShortcloseReasons";

            HttpClient client = new HttpClient();
            List<SPSQShortcloseReasons> shortclosereasons = new List<SPSQShortcloseReasons>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                shortclosereasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQShortcloseReasons>>(data);
            }

            return Json(shortclosereasons, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSalesQuoteJustificationDetails(string LoggedInUserRole, string CCompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            string orderByField = "No desc";
            string filter = "PCPL_IsInquiry eq false and Sell_to_Contact_No eq '" + CCompanyNo + "' and (PCPL_Status eq 'Approved' OR PCPL_Status eq 'Rejected by finance' OR PCPL_Status eq 'Rejected by HOD')";
            int skip, top;
            skip = top = 0;

            if(LoggedInUserRole == "Finance")
            {
                skip = 0;
                top = 10;
                apiUrl = apiUrl + "GetSalesQuoteJustificationDetails?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;
            }
            else
            {
                skip = 0;
                top = 3;
                apiUrl = apiUrl + "GetSalesQuoteJustificationDetails?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;
            }

            HttpClient client = new HttpClient();
            List<SPSQJustificationDetails> salesquotes = new List<SPSQJustificationDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesquotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSQJustificationDetails>>(data);

            }

            return Json(salesquotes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<string> PrintQuote(string QuoteNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "PrintQuote?QuoteNo=" + QuoteNo;

            HttpClient client = new HttpClient();
            string savedPath = "";

            string path = Server.MapPath("~/SalesQuotePrint/");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            bool flag = false;
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == QuoteNo)
                {
                    flag = true;
                    //Process proc = new Process();
                    //proc.StartInfo.UseShellExecute = true;
                    //proc.StartInfo.FileName = path + smFile.Name;
                    //proc.Start();
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
                    savedPath = SaveBase64ToPdf(base64PDF, "SalesQuotePrint", QuoteNo);
                }
                else
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                }
                return savedPath;
            }
            
        }


        public async Task<string> GetCompanyIndustry(string CCompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSalesQuotes/";

            apiUrl = apiUrl + "GetCompanyIndustry?CCompanyNo=" + CCompanyNo;

            HttpClient client = new HttpClient();
            SPSQCompanyIndustry companyIndustry = new SPSQCompanyIndustry();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                companyIndustry = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSQCompanyIndustry>(data);
            }

            string companyIndustry_ = companyIndustry.Business_Type == null || companyIndustry.Business_Type == "" ? "" : companyIndustry.Business_Type;

            return companyIndustry_;
        }

        public string SaveBase64ToPdf(string base64String, string relativeFolderPath, string fileNameWithoutExtension)
        {
            // Get absolute path to the project directory
            string projectRoot = Server.MapPath("~/");

            // Combine with relative folder path
            string fullFolderPath = Path.Combine(projectRoot, relativeFolderPath);

            // Ensure the directory exists
            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            // Build the full file path
            string filePath = Path.Combine(fullFolderPath, $"{fileNameWithoutExtension}.pdf");

            // Decode and write the file
            byte[] pdfBytes = Convert.FromBase64String(base64String);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            return $"{fileNameWithoutExtension}.pdf";
        }

    }
}