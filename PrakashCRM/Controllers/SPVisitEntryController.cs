using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using HtmlAgilityPack;
using System.Security.Policy;
using System.Net.Http.Headers;
using DocumentFormat.OpenXml.Bibliography;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPVisitEntryController : Controller
    {
        // GET: SPVisitEntry
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult YearMonthVisitPlan(string No = "")
        {
            SPVEYearMonthPlanPost yearMonthPlanData = new SPVEYearMonthPlanPost();
            if (No != "" || Session["YearMonthVisitPlanNo"] != null)
            {
                if (Session["YearMonthVisitPlanNo"] == null)
                    Session["YearMonthVisitPlanNo"] = No;

                Task<SPVEYearMonthPlanPost> task = Task.Run<SPVEYearMonthPlanPost>(async () => await GetYearMonthVisitPlanForEdit(Session["YearMonthVisitPlanNo"].ToString()));
                yearMonthPlanData = task.Result;
                ViewBag.Type = yearMonthPlanData.Visit_Type;
                ViewBag.SubType = yearMonthPlanData.Visit_Sub_Type;
                ViewBag.Year = yearMonthPlanData.Year;
                ViewBag.Edate = yearMonthPlanData.Edate;
                Session["isYearMonthVisitPlanEdit"] = true;

            }

            if (yearMonthPlanData != null)
                return View(yearMonthPlanData);
            else
                return View(new SPVEYearMonthPlanPost());

        }

        public async Task<SPVEYearMonthPlanPost> GetYearMonthVisitPlanForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetYearMonthVisitPlanNo?No=" + No;

            HttpClient client = new HttpClient();
            SPVEYearMonthPlanPost yearMonthVisitPlan = new SPVEYearMonthPlanPost();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                yearMonthVisitPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<SPVEYearMonthPlanPost>(data);
            }

            return yearMonthVisitPlan;
        }


        [HttpPost]
        public async Task<ActionResult> YearMonthVisitPlan(SPVEYearMonthPlanPost yearMonthPlanPost)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            //apiUrl = apiUrl + "YearMonthVisitPlan";

            SPVEYearMonthPlan yearMonthPlanRes = new SPVEYearMonthPlan();

            string No = "";

            if (Convert.ToBoolean(Session["isYearMonthVisitPlanEdit"]) == true)
            {
                No = Session["YearMonthVisitPlanNo"].ToString();
                //string[] date_ = Request.Form["hfEdate"].ToString().Split('-');
                //yearMonthPlanPost.Edate = date_[2] + '-' + date_[1] + '-' + date_[0];
                yearMonthPlanPost.Edate = Request.Form["hfEdate"].ToString();
                apiUrl = apiUrl + "YearMonthVisitPlan?isEdit=true&YearMonthPlanNo=" + No;
            }
            else
            {
                yearMonthPlanPost.Edate = DateTime.Now.ToString("yyyy-MM-dd");
                apiUrl = apiUrl + "YearMonthVisitPlan?isEdit=false&YearMonthPlanNo=" + No;
            }

            yearMonthPlanPost.SalesPerson_Code = Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(yearMonthPlanPost);
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
                yearMonthPlanRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPVEYearMonthPlan>(data);
                //Session["YearMonthPlanAction"] = "Created";

                if (yearMonthPlanRes.errorDetails.isSuccess)
                {
                    if (yearMonthPlanRes != null && Convert.ToBoolean(Session["isYearMonthVisitPlanEdit"]) == true)
                        Session["YearMonthPlanAction"] = "Updated";
                    else if (yearMonthPlanRes != null)
                        Session["YearMonthPlanAction"] = "Created";
                    else
                        Session["YearMonthPlanAction"] = "Error";
                }
                else
                    Session["YearMonthPlanActionErr"] = yearMonthPlanRes.errorDetails.message;
            }

            return RedirectToAction("YearMonthVisitPlan");

        }

        public bool NullYearMonthPlanSession()
        {
            bool isSessionNull = false;

            Session["YearMonthPlanAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public bool NullYearMonthPlanErrSession()
        {
            bool isSessionNull = false;

            Session["YearMonthPlanActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public ActionResult YearMonthPlanList()
        {
            return View();
        }

        public async Task<JsonResult> GetYearMonthPlanListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Year " + orderDir;
                    break;
                case 3:
                    orderByField = "Visit_Type " + orderDir;
                    break;
                case 4:
                    orderByField = "Visit_Sub_Type " + orderDir;
                    break;
                case 5:
                    orderByField = "No_of_Visit " + orderDir;
                    break;
                case 6:
                    orderByField = "No_of_Actual_Visit " + orderDir;
                    break;
                default:
                    orderByField = "Year asc";
                    break;
            }

            apiUrl = apiUrl + "GetYearMonthPlanListData?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPVEYearMonthPlan> yearMonthPlanData = new List<SPVEYearMonthPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                yearMonthPlanData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEYearMonthPlan>>(data);

                for (int i = 0; i < yearMonthPlanData.Count; i++)
                {
                    string[] date_ = yearMonthPlanData[i].Edate.ToString().Split('-');
                    yearMonthPlanData[i].Edate = date_[2] + '-' + date_[1] + '-' + date_[0];
                }
            }

            Session["YearMonthVisitPlanNo"] = null;
            Session["isYearMonthVisitPlanEdit"] = false;
            Session["YearMonthPlanAction"] = "";

            return Json(yearMonthPlanData, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetYearMonthPlanDataForYear(string Year)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetYearMonthPlanDataForYear?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&Year=" + Year;

            HttpClient client = new HttpClient();
            List<SPVEYearMonthPlan> yearmonthplan = new List<SPVEYearMonthPlan>();
            List<SPVEYearMonthPlan> yearmonthplan_ = new List<SPVEYearMonthPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                yearmonthplan = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEYearMonthPlan>>(data);

                //string indexNo = "";
                //for(int a = 0; a < yearmonthplan.Count; a++)
                //{
                //    int UpdateNoOfVisit = 0;
                //    bool skip = false;
                //    for(int b = a + 1; b < yearmonthplan.Count; b++)
                //    {
                //        if ((yearmonthplan[a].Year == yearmonthplan[b].Year) && (yearmonthplan[a].Visit_Type == yearmonthplan[b].Visit_Type) 
                //            && (yearmonthplan[a].Visit_Sub_Type == yearmonthplan[b].Visit_Sub_Type))
                //        {
                //            //UpdateNoOfVisit += yearmonthplan[b].No_of_Visit;
                //            yearmonthplan[a].No_of_Visit += yearmonthplan[b].No_of_Visit;
                //            indexNo += b + ",";
                //            //skip = true;
                //        }
                //    }

                //    //if (!skip) { 
                //    //    yearmonthplan_.Add(new SPVEYearMonthPlan
                //    //    {
                //    //        No = yearmonthplan[a].No,
                //    //        Edate = yearmonthplan[a].Edate,
                //    //        Year = yearmonthplan[a].Year,
                //    //        Visit_Type = yearmonthplan[a].Visit_Type,
                //    //        Visit_Type_Name = yearmonthplan[a].Visit_Type_Name,
                //    //        Visit_Sub_Type = yearmonthplan[a].Visit_Sub_Type,
                //    //        Visit_Sub_Type_Name = yearmonthplan[a].Visit_Sub_Type_Name,
                //    //        SalesPerson_Code = yearmonthplan[a].SalesPerson_Code,
                //    //        No_of_Visit = yearmonthplan[a].No_of_Visit + UpdateNoOfVisit,
                //    //        IsActive = yearmonthplan[a].IsActive
                //    //    });
                //    //}
                //    //yearmonthplan_[a].No = yearmonthplan[a].No;
                //    //yearmonthplan_[a].Edate = yearmonthplan[a].Edate;
                //    //yearmonthplan_[a].Year = yearmonthplan[a].Year;
                //    //yearmonthplan_[a].Visit_Type = yearmonthplan[a].Visit_Type;
                //    //yearmonthplan_[a].Visit_Type_Name = yearmonthplan[a].Visit_Type_Name;
                //    //yearmonthplan_[a].Visit_Sub_Type = yearmonthplan[a].Visit_Sub_Type;
                //    //yearmonthplan_[a].Visit_Sub_Type_Name = yearmonthplan[a].Visit_Sub_Type_Name;
                //    //yearmonthplan_[a].SalesPerson_Code = yearmonthplan[a].SalesPerson_Code;
                //    //yearmonthplan_[a].No_of_Visit = yearmonthplan[a].No_of_Visit + UpdateNoOfVisit;
                //    //yearmonthplan_[a].IsActive = yearmonthplan[a].IsActive;
                //}

                //if (indexNo != "")
                //{
                //    string indexNo_ = indexNo.Substring(0, indexNo.Length - 1);
                //    string[] chkIndex = indexNo_.Split(',');
                //    for (int a = 0; a < yearmonthplan.Count; a++)
                //    {
                //        if (!chkIndex.Contains(a.ToString()))
                //        {
                //            yearmonthplan_.Add(new SPVEYearMonthPlan
                //            {
                //                No = yearmonthplan[a].No,
                //                Edate = yearmonthplan[a].Edate,
                //                Year = yearmonthplan[a].Year,
                //                Visit_Type = yearmonthplan[a].Visit_Type,
                //                Visit_Type_Name = yearmonthplan[a].Visit_Type_Name,
                //                Visit_Sub_Type = yearmonthplan[a].Visit_Sub_Type,
                //                Visit_Sub_Type_Name = yearmonthplan[a].Visit_Sub_Type_Name,
                //                SalesPerson_Code = yearmonthplan[a].SalesPerson_Code,
                //                No_of_Visit = yearmonthplan[a].No_of_Visit,
                //                IsActive = yearmonthplan[a].IsActive
                //            });
                //        }
                //    }
                //}
                //else
                //    yearmonthplan_ = yearmonthplan;
            }

            return Json(yearmonthplan, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> DeleteYearMonthPlan(int YearMonthPlanNo)
        {
            bool flag = false;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl += "DeleteYearMonthPlan?YearMonthPlanNo=" + YearMonthPlanNo;

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.DeleteAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                flag = true;
            }

            return flag;
        }

        public async Task<JsonResult> GetMonthlyVisitDataForYear(string Year, string TypeNo, string SubTypeNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetMonthlyVisitDataForYear?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&Year=" + Year + "&TypeNo=" + TypeNo + "&SubTypeNo=" + SubTypeNo;

            HttpClient client = new HttpClient();
            List<SPVEMonthlyPlanList> monthplanlist = new List<SPVEMonthlyPlanList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                monthplanlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEMonthlyPlanList>>(data);
            }

            return Json(monthplanlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WeekPlan()
        {
            //SPVEWeekSalesPlanDetails weekPlan = new SPVEWeekSalesPlanDetails();
            //if (No != "" || Session["WeekPlanNo"] != null)
            //{
            //    if (Session["WeekPlanNo"] == null)
            //        Session["WeekPlanNo"] = No;

            //    Task<SPVEWeekSalesPlanDetails> task = Task.Run<SPVEWeekSalesPlanDetails>(async () => await GetWeekPlanForEdit(Session["WeekPlanNo"].ToString()));
            //    weekPlan = task.Result;
            //    ViewBag.Type = weekPlan.Visit_Type;
            //    ViewBag.SubType = weekPlan.Visit_Sub_Type;
            //    //string[] date_ = weekPlan.Week_Date.Split('-');
            //    //weekPlan.Week_Date = date_[2] + '-' + date_[1] + '-' + date_[0];
            //    Session["isWeekPlanEdit"] = true;

            //}
            //else
            //{
            //    DateTime weekPlanDate = Convert.ToDateTime(DateTime.Now);
            //    //weekPlan.Week_Date = weekPlanDate.ToString("dd-MM-yyyy");
            //}

            //return View(weekPlan);
            return View();
        }

        public async Task<SPVEWeekSalesPlanDetails> GetWeekPlanForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlan?No=" + No;

            HttpClient client = new HttpClient();
            SPVEWeekSalesPlanDetails weekPlan = new SPVEWeekSalesPlanDetails();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<SPVEWeekSalesPlanDetails>(data);
                Session["ContactCompanyNo"] = weekPlan.Contact_Company_No;
            }

            return weekPlan;
        }

        [HttpPost]
        public async Task<bool> PostWeekPlan(SPVEWeekSalesPlanDetails weekSalesPlanDetails)
        {
            bool flag = false;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            weekSalesPlanDetails.SalesPerson_Code = Session["loggedInUserSPCode"].ToString();

            if (Convert.ToBoolean(weekSalesPlanDetails.IsWeekPlanEdit))
                apiUrl = apiUrl + "WeekPlan?isEdit=true&No=" + weekSalesPlanDetails.No;
            else
                apiUrl = apiUrl + "WeekPlan?isEdit=false&No=" + weekSalesPlanDetails.No;

            SPVEWeekSalesPlan responseWeekPlan = new SPVEWeekSalesPlan();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(weekSalesPlanDetails);
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
                flag = true;
                var data = await response.Content.ReadAsStringAsync();
                responseWeekPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<SPVEWeekSalesPlan>(data);


                if (responseWeekPlan.errorDetails.isSuccess)
                {

                    if (Convert.ToBoolean(weekSalesPlanDetails.IsWeekPlanEdit) == true)// && responseInquiry.No != null
                    {
                        Session["WeekPlanAction"] = "Updated";
                    }
                    else if (Convert.ToBoolean(weekSalesPlanDetails.IsWeekPlanEdit) == false)// && responseInquiry.No != null
                    {
                        Session["WeekPlanAction"] = "Created";
                    }
                    else
                        Session["WeekPlanAction"] = "Error";

                }
                else
                    Session["WeekPlanActionErr"] = responseWeekPlan.errorDetails.message;

            }

            return flag;
        }

        [HttpPost]
        public async Task<ActionResult> WeekPlan(SPVEWeekSalesPlanDetails weekSalesPlanDetails)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string No = "";

            if (Convert.ToBoolean(Session["isWeekPlanEdit"]) == true)
            {
                No = Session["WeekPlanNo"].ToString();
                apiUrl = apiUrl + "WeekPlan?isEdit=true&WeekPlanNo=" + No;
            }
            else
                apiUrl = apiUrl + "WeekPlan?isEdit=false&WeekPlanNo=" + No;


            SPVEWeekSalesPlanPost weekSalesPlan = new SPVEWeekSalesPlanPost();
            SPVEWeekSalesPlan weekSalesPlanRes = new SPVEWeekSalesPlan();

            //weekSalesPlan.Week_Date = weekSalesPlanDetails.Week_Date;
            weekSalesPlan.Visit_Type = weekSalesPlanDetails.Visit_Type;
            weekSalesPlan.Visit_Sub_Type = weekSalesPlanDetails.Visit_Sub_Type;
            weekSalesPlan.Pur_Visit = weekSalesPlanDetails.Pur_Visit;
            weekSalesPlan.Remarks = weekSalesPlanDetails.Remarks;

            weekSalesPlan.Week_Plan_Date = DateTime.Now.ToString("yyyy-MM-dd");

            //string myDateTimeString = weekSalesPlan.Week_Date;
            //DateTime dt = DateTime.ParseExact(myDateTimeString, "dd/MM/yyyy", new CultureInfo("en-Us", true), DateTimeStyles.NoCurrentDateDefault);
            //DateTime dt = Convert.ToDateTime(myDateTimeString);
            //string day = dt.DayOfWeek.ToString();

            //string[] date_ = weekSalesPlan.Week_Date.Split('-');
            //weekSalesPlan.Week_Date = date_[2] + "-" + date_[1] + "-" + date_[0];

            //weekSalesPlan.Week_Day = day;
            //weekSalesPlan.Month_Detail_No = "";

            if (Request.Form["hfContactCompanyNo"].ToString() != "")
                weekSalesPlan.Contact_Company_No = Request.Form["hfContactCompanyNo"];
            else
                weekSalesPlan.Contact_Company_No = Session["ContactCompanyNo"].ToString();

            //weekSalesPlan.Target = "";
            //weekSalesPlan.Status = "Pending";
            weekSalesPlan.SalesPerson_Code = Session["loggedInUserSPCode"].ToString();
            //weekSalesPlan.IsActive = weekSalesPlanDetails.IsActive;

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(weekSalesPlan);
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
                weekSalesPlanRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPVEWeekSalesPlan>(data);
            }

            if (weekSalesPlanRes != null && Convert.ToBoolean(Session["isWeekPlanEdit"]) == true)
                Session["WeekPlanAction"] = "Updated";
            else if (weekSalesPlanRes != null)
                Session["WeekPlanAction"] = "Created";
            else
                Session["WeekPlanAction"] = "Error";

            return RedirectToAction("WeekPlan");
        }

        public ActionResult WeekPlanList()
        {
            return View();
        }

        public async Task<JsonResult> GetWeekPlanNoDetailsForList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlanNoDetailsForList?SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPWeekPlanNoDetails> weekPlanDatails = new List<SPWeekPlanNoDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlanDatails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWeekPlanNoDetails>>(data);

                for(int a = 0; a < weekPlanDatails.Count; a++)
                {
                    string[] FromDate_ = weekPlanDatails[a].Week_Start_Date.Split('-');
                    string[] ToDate_ = weekPlanDatails[a].Week_End_Date.Split('-');

                    weekPlanDatails[a].Week_Start_Date = FromDate_[2] + '-' + FromDate_[1] + '-' + FromDate_[0];
                    weekPlanDatails[a].Week_End_Date = ToDate_[2] + '-' + ToDate_[1] + '-' + ToDate_[0];
                }

            }

            return Json(weekPlanDatails, JsonRequestBehavior.AllowGet);

        }

        
        public async Task<JsonResult> GetWeekPlanDetailsTypeSubTypeWise(int WeekNo, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlanDetailsTypeSubTypeWise?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&WeekNo=" + WeekNo + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<SPWeekPlanDetailsTypeWise> weekPlanDatailsTypeWise = new List<SPWeekPlanDetailsTypeWise>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlanDatailsTypeWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWeekPlanDetailsTypeWise>>(data);

            }

            return Json(weekPlanDatailsTypeWise, JsonRequestBehavior.AllowGet);

        }

        public async Task<JsonResult> GetWeekPlanTypeWiseCountDetails(string VisitType, string VisitSubType, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlanTypeWiseCountDetails?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&VisitType=" + VisitType + "&VisitSubType=" + VisitSubType + "&FromDate=" + FromDate + 
                    "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<SPVEWeekSalesPlan> weekPlanDatailsTypeWise = new List<SPVEWeekSalesPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlanDatailsTypeWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEWeekSalesPlan>>(data);

            }

            return Json(weekPlanDatailsTypeWise, JsonRequestBehavior.AllowGet);

        }

        public async Task<JsonResult> GetWeekPlanListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Financial_Year " + orderDir;
                    break;
                case 3:
                    orderByField = "Week_Plan_Date " + orderDir;
                    break;
                case 4:
                    orderByField = "Visit_Name " + orderDir;
                    break;
                case 5:
                    orderByField = "Visit_Sub_Type_Name " + orderDir;
                    break;
                case 6:
                    orderByField = "ContactCompanyName " + orderDir;
                    break;
                case 7:
                    orderByField = "Mode_Of_Visit " + orderDir;
                    break;
                //case 8:
                //    orderByField = "Pur_Visit " + orderDir;
                //    break;
                //case 9:
                //    orderByField = "Contact_Person_Name " + orderDir;
                //    break;
                //case 10:
                //    orderByField = "Event_Name " + orderDir;
                //    break;
                //case 11:
                //    orderByField = "Topic_Name " + orderDir;
                //    break;
                //case 12:
                //    orderByField = "Remarks " + orderDir;
                //    break;
                default:
                    orderByField = "Financial_Year asc";
                    break;
            }

            apiUrl = apiUrl + "GetWeekPlanListData?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPVEWeekSalesPlan> weekPlanData = new List<SPVEWeekSalesPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlanData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEWeekSalesPlan>>(data);

                //for (int i = 0; i < weekPlanData.Count; i++)
                //{
                //    string[] date_ = weekPlanData[i].Week_Date.ToString().Split('-');
                //    weekPlanData[i].Week_Date = date_[2] + '-' + date_[1] + '-' + date_[0];
                //}
            }

            Session["WeekPlanNo"] = null;
            Session["ContactCompanyNo"] = null;
            Session["isWeekPlanEdit"] = false;
            Session["WeekPlanAction"] = "";

            return Json(weekPlanData, JsonRequestBehavior.AllowGet);
        }

        public bool NullWeekPlanSession()
        {
            bool isSessionNull = false;

            Session["WeekPlanAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public bool NullWeekPlanErrSession()
        {
            bool isSessionNull = false;

            Session["WeekPlanActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public ActionResult DailyVisit()
        {
            return View(new SPDailyVisitDetails());
        }

        [HttpPost]
        public async Task<bool> PostDailyVisit(SPDailyVisitDetails dailyVisitDetails, string CustomerName, string CustEmail)
        {
            bool flag = false;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            if (Convert.ToBoolean(dailyVisitDetails.IsDailyVisitEdit))
                apiUrl = apiUrl + "DailyVisit?isEdit=true&No=" + dailyVisitDetails.No + "&CustomerName=" + CustomerName + "&CustEmail=" + CustEmail;
            else
                apiUrl = apiUrl + "DailyVisit?isEdit=false&No=" + dailyVisitDetails.No + "&CustomerName=" + CustomerName + "&CustEmail=" + CustEmail;

            SPDailyVisit responseDailyVisit = new SPDailyVisit();

            dailyVisitDetails.Market_Update_Date = DateTime.Now.ToString("yyyy-MM-dd");
            dailyVisitDetails.Salesperson_Code = Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(dailyVisitDetails);
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
                flag = true;
                var data = await response.Content.ReadAsStringAsync();
                responseDailyVisit = Newtonsoft.Json.JsonConvert.DeserializeObject<SPDailyVisit>(data);


                if (responseDailyVisit.errorDetails.isSuccess)
                {
                    if (Convert.ToBoolean(dailyVisitDetails.IsDailyVisitEdit) == true)
                    {
                        Session["DailyVisitAction"] = "Updated";
                    }
                    else if (Convert.ToBoolean(dailyVisitDetails.IsDailyVisitEdit) == false)
                    {
                        Session["DailyVisitAction"] = "Created";
                    }
                    else
                        Session["DailyVisitAction"] = "Error";

                }
                else
                    Session["DailyVisitActionErr"] = responseDailyVisit.errorDetails.message;

            }

            return flag;
        }

        public bool NullDailyVisitErrSession()
        {
            bool isSessionNull = false;

            Session["DailyVisitActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        [HttpPost]
        public async Task<ActionResult> DailyVisit(SPDailyVisitDetails dailyVisitDetails)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "DailyVisit?SPCode=" + Session["loggedInUserSPCode"].ToString();

            ////dailyVisitDetails.Start_Time = dailyVisitDetails.Start_Time + ":" + dailyVisitDetails.Start_Minutes + ":" + dailyVisitDetails.StartTimeAMPM;
            //dailyVisitDetails.Start_Time = dailyVisitDetails.Start_Time + ":" + dailyVisitDetails.Start_Minutes + ":00";
            ////dailyVisitDetails.End_Time = dailyVisitDetails.End_Time + ":" + dailyVisitDetails.End_Minutes + ":" + dailyVisitDetails.EndTimeAMPM;
            //dailyVisitDetails.End_Time = dailyVisitDetails.End_Time + ":" + dailyVisitDetails.End_Minutes + ":00";

            dailyVisitDetails.Com_Date = DateTime.Now.ToString("yyyy-MM-dd");
            dailyVisitDetails.Root_Analysis_date = DateTime.Now.ToString("yyyy-MM-dd");
            dailyVisitDetails.Corrective_Action_Date = DateTime.Now.ToString("yyyy-MM-dd");
            dailyVisitDetails.Preventive_Date = DateTime.Now.ToString("yyyy-MM-dd");
            dailyVisitDetails.Market_Update_Date = DateTime.Now.ToString("yyyy-MM-dd");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument { OptionUseIdAttribute = true };
            doc = web.Load(ConfigurationManager.AppSettings["SPPortalUrl"].ToString() + "SPVisitEntry/DailyVisit");
            HtmlNode table = doc.GetElementbyId("tblProdList");

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(dailyVisitDetails);
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
                //responseCompany = Newtonsoft.Json.JsonConvert.DeserializeObject<SPCompanyResponse>(data);
                Session["DailyVisitAction"] = "Created";
            }

            return RedirectToAction("DailyVisitList");
        }

        public bool NullDailyVisitSession()
        {
            bool isSessionNull = false;

            Session["DailyVisitAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public ActionResult DailyVisitList()
        {
            return View();
        }

        public async Task<JsonResult> GetDailyVisitsListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string orderByField = "";

            switch (orderBy)
            {
                case 3:
                    orderByField = "Date " + orderDir;
                    break;
                case 4:
                    orderByField = "Visit_Name " + orderDir;
                    break;
                case 5:
                    orderByField = "Visit_SubType_Name " + orderDir;
                    break;
                case 6:
                    orderByField = "Contact_Company_Name " + orderDir;
                    break;
                default:
                    orderByField = "Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetDailyVisitsListData?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisit>>(data);

                for (int i = 0; i < dailyVisitData.Count; i++)
                {
                    string[] date_ = dailyVisitData[i].Date.ToString().Split('-');
                    dailyVisitData[i].Date = date_[2] + '-' + date_[1] + '-' + date_[0];

                    string[] paymentDate_ = dailyVisitData[i].Payment_Date.ToString().Split('-');
                    dailyVisitData[i].Payment_Date = paymentDate_[2] + '-' + paymentDate_[1] + '-' + paymentDate_[0];
                }
            }

            //Session["YearMonthVisitPlanNo"] = null;
            //Session["isYearMonthVisitPlanEdit"] = false;
            //Session["YearMonthPlanAction"] = "";

            return Json(dailyVisitData, JsonRequestBehavior.AllowGet);
        }

        
        public async Task<JsonResult> GetDailyVisitsDetails(string VisitType, string VisitSubType, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetDailyVisitsDetails?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&VisitType=" + VisitType + "&VisitSubType=" + VisitSubType + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisit>>(data);

                for (int i = 0; i < dailyVisitData.Count; i++)
                {
                    string[] date_ = dailyVisitData[i].Date.ToString().Split('-');
                    dailyVisitData[i].Date = date_[2] + '-' + date_[1] + '-' + date_[0];
                }
            }

            return Json(dailyVisitData, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetWeekPlanForDailyPlan(string date)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlanForDailyPlan?date=" + date + "&SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPVEWeekSalesPlan> weekplans = new List<SPVEWeekSalesPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekplans = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEWeekSalesPlan>>(data);
            }

            return Json(weekplans, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetVisitTypesForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetVisitTypesForDDL";

            HttpClient client = new HttpClient();
            List<SPVisitTypes> visittypes = new List<SPVisitTypes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                visittypes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVisitTypes>>(data);
            }

            return Json(visittypes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetWeekPlanNoDetailsForDDL(string SPCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetWeekPlanNoDetailsForDDL?SPCode=" + SPCode;

            HttpClient client = new HttpClient();
            List<SPWeekPlanNoDetails> weekPlanNoDetails = new List<SPWeekPlanNoDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weekPlanNoDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWeekPlanNoDetails>>(data);
            }

            return Json(weekPlanNoDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetVisitSubTypesForDDL(string TypeNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetVisitSubTypesForDDL?TypeNo=" + TypeNo;

            HttpClient client = new HttpClient();
            List<SPVisitSubTypes> visitsubtypes = new List<SPVisitSubTypes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                visitsubtypes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVisitSubTypes>>(data);
            }

            return Json(visitsubtypes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDailyWeeklyPlanEventsForDDL(string TypeNo, string SubTypeNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetDailyWeeklyPlanEventsForDDL?TypeNo=" + TypeNo + "&SubTypeNo=" + SubTypeNo;

            HttpClient client = new HttpClient();
            List<SPWeeklyDailyPlanEvents> events = new List<SPWeeklyDailyPlanEvents>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                events = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWeeklyDailyPlanEvents>>(data);
            }

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetProductsForDDL(string SPCode, string CCompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetProductsForDDL?SPCode=" + SPCode + "&CCompanyNo=" + CCompanyNo;

            HttpClient client = new HttpClient();
            List<SPVEContactProducts> prods = new List<SPVEContactProducts>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                prods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEContactProducts>>(data);
            }

            return Json(prods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllProductsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetAllProductsForDDL";

            HttpClient client = new HttpClient();
            List<SPItemList> prods = new List<SPItemList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                prods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPItemList>>(data);
            }

            return Json(prods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInvoiceProductsForDDL(string InvNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetInvoiceProductsForDDL?InvNo=" + InvNo;

            HttpClient client = new HttpClient();
            List<SPVEInvoiceProducts> invoiceproducts = new List<SPVEInvoiceProducts>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                invoiceproducts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEInvoiceProducts>>(data);
            }

            return Json(invoiceproducts, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetCompetitorsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetCompetitorsForDDL";

            HttpClient client = new HttpClient();
            List<SPVECompetitors> competitors = new List<SPVECompetitors>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                competitors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVECompetitors>>(data);
            }

            return Json(competitors, JsonRequestBehavior.AllowGet);
        }
        

        public async Task<JsonResult> GetSalespersonForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetSalespersonForDDL";

            HttpClient client = new HttpClient();
            List<VisitEntrySP> salespersons = new List<VisitEntrySP>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salespersons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VisitEntrySP>>(data);
            }

            return Json(salespersons, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetContactCompanyForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetContactCompanyForDDL?SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPVEContactCompany> companies = new List<SPVEContactCompany>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEContactCompany>>(data);
            }

            return Json(companies, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<bool> AddNewContactPerson(SPContact CPersonDetails)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/AddNewContactPerson";
            bool flag = false;

            HttpClient client = new HttpClient();
            SPContact CPersonDetailsRes = new SPContact();

            CPersonDetails.Salesperson_Code = Session["loggedInUserSPCode"].ToString();

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

        public async Task<JsonResult> GetContactPersonForDDL(string CompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetContactPersonForDDL?CompanyNo=" + CompanyNo;

            HttpClient client = new HttpClient();
            List<SPVEContactPerson> contacts = new List<SPVEContactPerson>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEContactPerson>>(data);
            }

            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllDepartmentForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/GetAllDepartmentForDDL";

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

        public async Task<JsonResult> GetCustomerInvoiceForDDL(string CompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetCustomerInvoiceForDDL?CompanyNo=" + CompanyNo;

            HttpClient client = new HttpClient();
            List<SPDailyVisitInvoiceDetails> invoiceDetails = new List<SPDailyVisitInvoiceDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                invoiceDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisitInvoiceDetails>>(data);
            }

            return Json(invoiceDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetComplainDetails(string dvpNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";
            
            apiUrl += "GetComplainDetails?dvpNo=" + dvpNo;

            HttpClient client = new HttpClient();
            List<SPDVComplainList> complainlist = new List<SPDVComplainList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                complainlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDVComplainList>>(data);
            }

            return Json(complainlist, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetPaymentDetails(string dvpNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl += "GetPaymentDetails?dvpNo=" + dvpNo;

            HttpClient client = new HttpClient();
            List<SPDVPaymentList> paymentlist = new List<SPDVPaymentList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                paymentlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDVPaymentList>>(data);
            }

            return Json(paymentlist, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDailyVisitProductDetails(string dvpNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl = apiUrl + "GetDailyVisitProductDetails?dvpNo=" + dvpNo;

            HttpClient client = new HttpClient();
            List<SPVEWeeklyDailyPlanProds> dailyvisitprods = new List<SPVEWeeklyDailyPlanProds>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyvisitprods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPVEWeeklyDailyPlanProds>>(data);
            }

            return Json(dailyvisitprods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetExpanseDetails(string dvpNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            apiUrl += "GetExpanseDetails?dvpNo=" + dvpNo;

            HttpClient client = new HttpClient();
            List<SPDailyVisitExpanse> expanselist = new List<SPDailyVisitExpanse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                expanselist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisitExpanse>>(data);
            }

            return Json(expanselist, JsonRequestBehavior.AllowGet);
        }

        #region DailyVisit Expense Report
        public ActionResult DailyVisitExpenseReport()
        {
            return View();
        }

        public async Task<JsonResult> GetDailyVisitExpenseReport(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Visit_Date " + orderDir;
                    break;
                case 3:
                    orderByField = "User_Code " + orderDir;
                    break;
                //case 4:
                //    orderByField = "Visit_Sub_Type " + orderDir;
                //    break;
                //case 5:
                //    orderByField = "No_of_Visit " + orderDir;
                //    break;
                default:
                    orderByField = "Visit_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetDailyVisitExpenseReport?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPDailyVisitExpenseForReport> dailyVisitExpense = new List<SPDailyVisitExpenseForReport>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitExpense = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisitExpenseForReport>>(data);

                for (int i = 0; i < dailyVisitExpense.Count; i++)
                {
                    string[] date_ = dailyVisitExpense[i].Visit_Date.ToString().Split('-');
                    dailyVisitExpense[i].Visit_Date = date_[2] + '-' + date_[1] + '-' + date_[0];
                }
            }

            //Session["YearMonthVisitPlanNo"] = null;
            //Session["isYearMonthVisitPlanEdit"] = false;
            //Session["YearMonthPlanAction"] = "";

            return Json(dailyVisitExpense, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPVisitEntry/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Visit_Date " + orderDir;
                    break;
                case 3:
                    orderByField = "User_Code " + orderDir;
                    break;
                //case 4:
                //    orderByField = "Visit_Sub_Type " + orderDir;
                //    break;
                //case 5:
                //    orderByField = "No_of_Visit " + orderDir;
                //    break;
                default:
                    orderByField = "Visit_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetDailyVisitExpenseReport?SPNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPDailyVisitExpenseForReport> dailyVisitExpense = new List<SPDailyVisitExpenseForReport>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitExpense = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisitExpenseForReport>>(data);
            }

            DataTable dt = ToDataTable(dailyVisitExpense);

            //Name of File  
            string fileName = "DailyVisitExpense.xlsx";
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

        #endregion
    }
}