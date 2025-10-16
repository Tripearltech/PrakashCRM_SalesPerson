﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Routing;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Xml.Linq;
using System.Web.Helpers;
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPContacts")]
    public class SPContactsController : ApiController
    {
        [Route("GetAllCompanies")]
        public List<SPCompanyList> GetAllCompanies(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPCompanyList> Companies = new List<SPCompanyList>();
            List<SPCompanyList> company2 = new List<SPCompanyList>();
            string filter2 = "";

            if (string.IsNullOrEmpty(filter))
                filter = "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'";

            if (string.IsNullOrEmpty(filter2))
                filter2 = "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'";
            else
                filter2 = filter2 + " and PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'";

            var result = (dynamic)null;
            var result2 = (dynamic)null;

            if (isExport)
            {
                result = ac.GetData<SPCompanyList>("ContactDotNetAPI", filter);
                result2 = ac.GetData<SPCompanyList>("ContactDotNetAPI", filter2);
            }
            else
            {
                result = ac.GetData1<SPCompanyList>("ContactDotNetAPI", filter, skip, top, orderby);
                result2 = ac.GetData1<SPCompanyList>("ContactDotNetAPI", filter2, skip, top, orderby);
            }

            if (result.Result.Item1.value.Count > 0)
                Companies = result.Result.Item1.value;

            if (result2 != null && result2.Result.Item1.value.Count > 0)
            {
                company2 = result2.Result.Item1.value;
                Companies.AddRange(company2);
            }

            // ✅ Add Mobile, Name, Email from Primary Contact
            foreach (var company in Companies)
            {
                try
                {
                    // Updated filter as per your requirement
                    var contactResult = ac.GetData<SPContact>(
                        "ContactDotNetAPI",
                        $"(Type eq 'Person' or Type eq 'Company') and Company_No eq '{company.No}' and Is_Primary eq true"
                    );

                    if (contactResult?.Result.Item1?.value?.Count > 0)
                    {
                        var contact = contactResult.Result.Item1.value[0];

                        company.Mobile_Phone_No = contact.Mobile_Phone_No;
                        company.PCPL_Primary_Contact_Name = contact.Name;
                        company.PCPL_Primary_Contact_Email = contact.E_Mail;
                    }
                }
                catch (Exception ex)
                {
                    // Optional: Handle or log error
                    company.Mobile_Phone_No = "";
                    company.PCPL_Primary_Contact_Name = "";
                    company.PCPL_Primary_Contact_Email = "";
                }
            }

            return Companies;
        }


        [Route("GetAllContacts")]
        public List<SPContactList> GetAllContacts(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPContactList> contacts = new List<SPContactList>();

            if (filter == "" || filter == null)
                filter = "Type eq 'Person' and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Type eq 'Person' and Salesperson_Code eq '" + SPCode + "'";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPContactList>("ContactDotNetAPI", filter); // and Contact_Business_Relation eq 'Customer'
            else
                result = ac.GetData1<SPContactList>("ContactDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                contacts = result.Result.Item1.value;

            return contacts;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string type, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Type eq '" + type + "' and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Type eq '" + type + "' and Salesperson_Code eq '" + SPCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetContactOfCompany")]
        public SPContact GetContactOfCompany(string No)
        {
            API ac = new API();
            SPContact contact = new SPContact();

            var result = ac.GetData<SPContact>("ContactDotNetAPI", "Company_No eq '" + No + "' and Type eq 'Person'");

            if (result.Result.Item1.value.Count > 0)
                contact = result.Result.Item1.value[0];

            return contact;
        }

        [Route("GetAllCompanyForDDL")]
        public List<Company> GetAllCompanyForDDL()
        {
            API ac = new API();
            List<Company> company = new List<Company>();

            var result = ac.GetData<Company>("ContactDotNetAPI", "Type eq 'Company' and (No eq 'C-00323' or No eq 'C-00135')");
            //var result = ac.GetData<Company>("ContactDotNetAPI", "Type eq 'Company'");

            if (result != null && result.Result.Item1.value.Count > 0)
                company = result.Result.Item1.value;

            return company;
        }

        [Route("CompanyContactCard")]
        public SPCompanyResponse CompanyContactCard(SPCompanyContact companycontact, bool isEdit, string CompanyNo)
        {
            SPCompany requestCompany = new SPCompany();
            SPCompanyResponse responseCompany = new SPCompanyResponse();
            SPContact responseContact = new SPContact();
            var ac = new API();
            errorDetails ed = new errorDetails();
            errorDetails ed1 = new errorDetails();
            var result = (dynamic)null;

            requestCompany.Name = companycontact.Company_Name;
            requestCompany.Area_Code = companycontact.Area_Code == "-1" ? "" : companycontact.Area_Code;
            requestCompany.District = string.IsNullOrEmpty(companycontact.District) ? "" : companycontact.District;
            requestCompany.Post_Code = string.IsNullOrEmpty(companycontact.Post_Code) ? "" : companycontact.Post_Code;
            requestCompany.State_Code = string.IsNullOrEmpty(companycontact.State_Code) ? "" : companycontact.State_Code;
            requestCompany.City = string.IsNullOrEmpty(companycontact.City) ? "" : companycontact.City;
            requestCompany.Country_Region_Code = string.IsNullOrEmpty(companycontact.Country_Region_Code) ? "" : companycontact.Country_Region_Code;
            requestCompany.Salesperson_Code = companycontact.Salesperson_Code;
            requestCompany.PCPL_Secondary_SP_Code = (companycontact.PCPL_Secondary_SP_Code == "-1" || string.IsNullOrEmpty(companycontact.PCPL_Secondary_SP_Code)) ? "" : companycontact.PCPL_Secondary_SP_Code;
            requestCompany.Address = companycontact.Address;
            requestCompany.Address_2 = companycontact.Address_2 ?? "";
            requestCompany.PCPL_URL = companycontact.PCPL_URL ?? "";
            requestCompany.Industry_No = companycontact.Industry_No == "-1" ? "" : companycontact.Industry_No;
            requestCompany.Business_Type_No = companycontact.Business_Type_No == "-1" ? "" : companycontact.Business_Type_No;
            requestCompany.GST_Registration_No = companycontact.GST_Registration_No ?? "";
            requestCompany.P_A_N_No = companycontact.P_A_N_No ?? "";
            requestCompany.Assessee_Code = companycontact.Assessee_Code ?? "";
            requestCompany.Source_Of_Contact_No = companycontact.Source_Of_Contact_No == "-1" ? "" : companycontact.Source_Of_Contact_No;
            requestCompany.E_Mail = companycontact.E_Mail;
            requestCompany.Credit_Limit = companycontact.Credit_Limit;
            requestCompany.Type = "Company";
            if (!string.IsNullOrEmpty(companycontact.Phone_No))
                requestCompany.Phone_No = companycontact.Phone_No;
            if (!string.IsNullOrEmpty(companycontact.Mobile_Phone_No))
                requestCompany.Mobile_Phone_No = companycontact.Mobile_Phone_No;

            if (isEdit)
            {
                var existingCompanyResult = ac.GetData<SPCompany>("ContactDotNetAPI", $"No eq '{CompanyNo}'");
                if (existingCompanyResult.Result.Item1.value.Count > 0)
                {
                    var existingCompany = existingCompanyResult.Result.Item1.value[0];
                    if (string.IsNullOrEmpty(requestCompany.Phone_No))
                        requestCompany.Phone_No = existingCompany.Phone_No;
                    if (string.IsNullOrEmpty(requestCompany.Mobile_Phone_No))
                        requestCompany.Mobile_Phone_No = existingCompany.Mobile_Phone_No;
                    if (string.IsNullOrEmpty(requestCompany.E_Mail))
                        requestCompany.E_Mail = existingCompany.E_Mail;
                }
                //Call Patch API
                result = PatchItemContact("ContactDotNetAPI", requestCompany, responseCompany, $"No='{CompanyNo}'");
                if (result.Result.Item1 != null)
                {
                    responseCompany = result.Result.Item1;
                    ed = result.Result.Item2;
                    responseCompany.errorDetails = ed;
                }
            }
            else
            {
                result = PostItemContact("ContactDotNetAPI", requestCompany, responseCompany);

                if (result.Result.Item1 != null)
                {
                    responseCompany = result.Result.Item1;
                    ed = result.Result.Item2;
                    responseCompany.errorDetails = ed;
                    if (!string.IsNullOrEmpty(responseCompany.No))
                    {
                        SPContact requestContact = new SPContact
                        {
                            Salesperson_Code = companycontact.Salesperson_Code,
                            Name = companycontact.Contact_Name,
                            Company_No = responseCompany.No,
                            Type = "Person",
                            Mobile_Phone_No = companycontact.Mobile_Phone_No,
                            E_Mail = companycontact.Contact_EMail,
                            PCPL_Job_Responsibility = companycontact.PCPL_Job_Responsibility,
                            PCPL_Department_Code = companycontact.PCPL_Department_Code,
                            PCPL_Allow_Login = companycontact.PCPL_Allow_Login,
                            PCPL_Enable_OTP_On_Login = companycontact.PCPL_Enable_OTP_On_Login,
                            Is_Primary = companycontact.Is_Primary
                        };

                        var result1 = ac.PostItem("ContactDotNetAPI", requestContact, responseContact);
                        if (result1.Result.Item1 != null)
                        {
                            responseContact = result1.Result.Item1;
                            ed1 = result1.Result.Item2;
                            responseCompany.errorDetails = ed1;
                        }
                    }
                }
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return responseCompany;
        }

        public async Task<(SPCompanyResponse, errorDetails)> PostItemContact<SPCompanyResponse>(string apiendpoint, SPCompany requestModel, SPCompanyResponse responseModel)
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
                    responseModel = res.ToObject<SPCompanyResponse>();

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

        public async Task<(SPCompanyResponse, errorDetails)> PatchItemContact<SPCompanyResponse>(string apiendpoint, SPCompany requestModel, SPCompanyResponse responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPCompanyResponse>();


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

        [Route("GetCompanyContactFromNo")]
        public SPCompanyContact GetCompanyContactFromNo(string No)
        {
            API ac = new API();
            SPCompanyContact companycontact = new SPCompanyContact();

            var result = ac.GetData<SPCompanyContact>("ContactDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
                companycontact = result.Result.Item1.value[0];

            return companycontact;
        }

        [Route("GetAllContactsOfCompany")]
        public List<SPContactResponse> GetAllContactsOfCompany(string SPCode, string No)
        {
            API ac = new API();
            List<SPContactResponse> contacts = new List<SPContactResponse>();

            var result = ac.GetData<SPContactResponse>("ContactDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Type eq 'Person' and Company_No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
                contacts = result.Result.Item1.value;

            return contacts;
        }

        [Route("GetAllProductsOfCompany")]
        public List<SPContactProducts> GetAllProductsOfCompany(string SPCode, string No)
        {
            API ac = new API();
            List<SPContactProducts> contactProducts = new List<SPContactProducts>();

            var result = ac.GetData<SPContactProducts>("ContactProductsDotNetAPI", "Contact_No eq '" + No + "' and SalesPerson_Code eq '" + SPCode + "' and IsActive eq true");

            if (result.Result.Item1.value.Count > 0)
                contactProducts = result.Result.Item1.value;

            return contactProducts;
        }

        [Route("AddNewContactOfCompany")]
        public bool AddNewContactOfCompany(string CompanyNo, string SPCode, string Name, string Mobile_Phone_No, string E_Mail, string PCPL_Department_Code, string PCPL_Job_Responsibility, bool PCPL_Allow_Login, bool PCPL_Enable_OTP_On_Login, bool Is_Primary, bool isEdit)
        {
            bool flag = false;
            SPContact requestContact = new SPContact();

            requestContact.Company_No = CompanyNo;
            requestContact.Salesperson_Code = SPCode;
            requestContact.Name = Name;
            requestContact.Type = "Person";
            requestContact.Mobile_Phone_No = Mobile_Phone_No;
            requestContact.E_Mail = E_Mail;
            requestContact.PCPL_Department_Code = PCPL_Department_Code;
            requestContact.PCPL_Job_Responsibility = PCPL_Job_Responsibility;
            requestContact.PCPL_Allow_Login = PCPL_Allow_Login;
            requestContact.PCPL_Enable_OTP_On_Login = PCPL_Enable_OTP_On_Login;
            requestContact.Is_Primary = Is_Primary;

            var ac = new API();
            errorDetails ed = new errorDetails();
            SPContact responseContact = new SPContact();
            var result = (dynamic)null;

            if (!isEdit)
                result = ac.PostItem("ContactDotNetAPI", requestContact, responseContact);

            if (result.Result.Item1 != null)
            {
                responseContact = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [HttpPost]
        [Route("UpdateContactOfCompany")]
        public bool UpdateContactOfCompany(string No, string SPCode, string Name, string Mobile_Phone_No, string E_Mail, string PCPL_Department_Code, string PCPL_Job_Responsibility, bool PCPL_Allow_Login, bool PCPL_Enable_OTP_On_Login, bool Is_Primary, string Company_No)
        {
            bool flag = false;
            SPContact requestContact = new SPContact();

            requestContact.Salesperson_Code = SPCode;
            requestContact.Name = Name;
            requestContact.Type = "Person";
            requestContact.Mobile_Phone_No = Mobile_Phone_No;
            requestContact.E_Mail = E_Mail;
            requestContact.PCPL_Department_Code = PCPL_Department_Code;
            requestContact.PCPL_Job_Responsibility = PCPL_Job_Responsibility;
            requestContact.PCPL_Allow_Login = PCPL_Allow_Login;
            requestContact.PCPL_Enable_OTP_On_Login = PCPL_Enable_OTP_On_Login;
            requestContact.Is_Primary = Is_Primary;
            requestContact.Company_No = Company_No;

            var ac = new API();
            errorDetails ed = new errorDetails();
            SPContact responseContact = new SPContact();
            var result = (dynamic)null;

            result = ac.PatchItem("ContactDotNetAPI", requestContact, responseContact, "No='" + No + "'");

            if (result.Result.Item1 != null)
            {
                responseContact = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [HttpPost]
        [Route("DeleteContactOfCompany")]
        public bool DeleteContactOfCompany(string No, string Name)
        {
            bool flag = false;
            SPContact requestContact = new SPContact();

            requestContact.Name = Name;

            var ac = new API();
            errorDetails ed = new errorDetails();
            SPContact responseContact = new SPContact();
            var result = (dynamic)null;

            result = ac.DeleteItem("ContactDotNetAPI", requestContact, responseContact, "No='" + No + "'");

            if (result.Result.Item1 != null)
            {
                responseContact = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [HttpPost]
        [Route("DeleteContactProduct")]
        public string DeleteContactProduct(string contactNo, string prodNo)
        {
            string response = "";
            SPDeleteContactProd reqDeleteContactProd = new SPDeleteContactProd();
            SPDeleteContactProdOData resDeleteContactProd = new SPDeleteContactProdOData();

            reqDeleteContactProd.contactno = contactNo;
            reqDeleteContactProd.productno = prodNo;

            var ac = new API();
            errorDetails ed = new errorDetails();

            var result = PostItemForDeleteContactProd<SPDeleteContactProdOData>("", reqDeleteContactProd, resDeleteContactProd);

            response = result.Result.Item1.value;
            ed = result.Result.Item2;
            resDeleteContactProd.errorDetails = ed;

            if (!resDeleteContactProd.errorDetails.isSuccess)
                response = "Error_:" + resDeleteContactProd.errorDetails.message;

            return response;
        }

        [Route("GetQtyForCountStockFromILE")]
        public List<SPILEQtyForCountStock> GetQtyForCountStockFromILE(string LocationCode, string ItemNo)
        {
            API ac = new API();
            List<SPILEQtyForCountStock> ILEList = new List<SPILEQtyForCountStock>();

            var result = ac.GetData<SPILEQtyForCountStock>("ItemLedgerEntriesDotNetAPI", "Location_Code eq '" + LocationCode + "' and Item_No eq '" + ItemNo + "'");

            if (result.Result.Item1.value.Count > 0)
                ILEList = result.Result.Item1.value;

            return ILEList;
        }

        [Route("GetBatchWiseQty")]
        public List<SPSQInvDetailsRes> GetBatchWiseQty(string ProdNo, string LocCode)
        {
            API ac = new API();
            SPSQInvDetails reqInvDetails = new SPSQInvDetails();
            List<SPSQInvDetailsRes> resInvDetails = new List<SPSQInvDetailsRes>();

            reqInvDetails.itemno = ProdNo;
            reqInvDetails.locationcode = LocCode;

            var result = PostItemForGetInventoryDetails<SPSQInvDetailsRes>("", reqInvDetails, resInvDetails);

            if (result.Result.Item1.Count > 0)
                resInvDetails = result.Result.Item1;

            return resInvDetails;
        }

        [Route("GetContactSalesQuotes")]
        public List<SPSalesQuotesList> GetContactSalesQuotes(string CCompanyNo, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPSalesQuotesList> salesQuotes = new List<SPSalesQuotesList>();
            string filter = "Sell_to_Contact_No eq '" + CCompanyNo + "' and PCPL_IsInquiry eq false";

            if (FromDate != null && ToDate != null)
                filter += " and Order_Date ge " + FromDate + " and Order_Date le " + ToDate;

            var result = ac.GetData<SPSalesQuotesList>("SalesQuoteDotNetAPI", filter);

            if (result.Result.Item1.value.Count > 0)
                salesQuotes = result.Result.Item1.value;

            return salesQuotes;
        }

        [Route("GetContactBusinessPlan")]
        public List<SPCustBusinessPlan> GetContactBusinessPlan(string SPCode, string CCompanyNo, string PlanYear)
        {
            API ac = new API();
            List<SPCustBusinessPlan> contactBusinessPlan = new List<SPCustBusinessPlan>();

            var result = ac.GetData<SPCustBusinessPlan>("BusinessPlanListDotNetAPI", "PCPL_Contact_Company_No eq '" + CCompanyNo + "' and PCPL_Plan_Year eq '" + PlanYear +
                    "' and Sales_Person eq '" + SPCode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                contactBusinessPlan = result.Result.Item1.value;

            return contactBusinessPlan;
        }

        [Route("GetContactDailyVisits")]
        public List<SPDailyVisit> GetContactDailyVisits(string SPCode, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();
            var result = (dynamic)null;

            if (FromDate != null && ToDate != null)
                result = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY' and Date ge " + FromDate + " and Date le " + ToDate);
            else
                result = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY'");

            if (result != null && result.Result.Item1.value.Count > 0)
                dailyVisitData = result.Result.Item1.value;

            return dailyVisitData;
        }

        //

        [Route("SendItemSpecMSDSCOA")]
        public bool SendItemSpecMSDSCOA(bool isEmail, string ToEmail, bool isWhatsApp, string ToWhatsApp, string Subject, string BodyText, string SelectedProd, bool isCOA, string SelectedLocation, int availStock, string Path)
        {
            bool flag = false;

            EmailService emailService = new EmailService();
            Attachment attachment = new Attachment(Path);

            StringBuilder sbMailBody = new StringBuilder();
            sbMailBody.Append("");
            sbMailBody.Append("<p>Hi,</p>");
            sbMailBody.Append("<p>&nbsp;</p>");
            sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");

            sbMailBody.Append("<br />");
            sbMailBody.Append("<h4>" + Subject + "</h4><br />");
            sbMailBody.Append("<table cellpadding='8' cellspacing='5' border='0' width='100%' style='box-shadow: 0 0 4px rgba(0, 0, 0, 0.3);'>");
            if (isCOA)
                sbMailBody.Append("<tr><td>Branch Location</td><td>" + SelectedLocation.Trim() + "</td></tr>");

            sbMailBody.Append("<tr><td>Product</td><td>" + SelectedProd.Trim() + "</td></tr>");

            if (isCOA)
                sbMailBody.Append("<tr><td>Available Stock</td><td>" + availStock + "</td></tr>");

            sbMailBody.Append("<tr><td>Details</td><td>" + BodyText.Trim() + "</td></tr>");
            sbMailBody.Append("<tr><td colspan=2>Thanks.</td></tr>");
            sbMailBody.Append("<tr><td colspan=2><b>Prakash Chemicals Agencies Pvt. Ltd.</b></td></tr>");
            sbMailBody.Append("</table>");

            emailService.SendAttachmentEmailTo(ToEmail, Subject + " - PrakashCRM", attachment, sbMailBody.ToString());
            flag = true;
            return flag;
        }

        //

        [Route("SendFeedbackFormLink")]
        public bool SendFeedbackFormLink(string contactCompanyNo, string ToCCEmail, string contactName, string contactMobileNo, string contactAddress, string custVendorPortalUrl, string SPNo)
        {
            string Link = "";
            string ToEmail = "";
            string CCEmail = "";
            string ToName = "";
            string Body = "";
            bool flag = false;

            string[] ToCCEmail_ = ToCCEmail.Split(',');
            //ToEmail += ToCCEmail_[0];
            //if (ToCCEmail_[1] != "")
            //{
            //    ToEmail += ";" + ToCCEmail_[1];
            //}
            //CCEmail = ToCCEmail_[2];

            if (ToCCEmail_[0] != "")
            {
                ToEmail += ToCCEmail_[0] + ";";
            }
            ToEmail = ToEmail.Substring(0, ToEmail.Length - 1);
            CCEmail = ToCCEmail_[1];

            //
            SPFeedBackHeaderOnSendLink reqFeedbackHeader = new SPFeedBackHeaderOnSendLink();

            //reqFeedbackHeader.Company_No = contactCompanyNo;
            //reqFeedbackHeader.Employee = SPNo;
            //reqFeedbackHeader.IsActive = true;

            reqFeedbackHeader.companyno = contactCompanyNo;
            reqFeedbackHeader.employee = SPNo;
            reqFeedbackHeader.isactive = true;

            string encFHSystemID = "";

            errorDetails ed = new errorDetails();
            SPFeedBackHeaderOnSendLinkRes resFeedbackHeader = new SPFeedBackHeaderOnSendLinkRes();
            var result = (dynamic)null;

            //result = PostItemFeedbackHeader("FeedbackHeadersListDotNetAPI", reqFeedbackHeader, resFeedbackHeader);
            result = PostItemFeedbackHeader("", reqFeedbackHeader, resFeedbackHeader);

            if (result.Result.Item1 != null)
            {
                resFeedbackHeader = result.Result.Item1;
                encFHSystemID = EncryptDecryptClass.Encrypt(resFeedbackHeader.systemid, true);
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            if (flag)
            {

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();

                Link = "<a title='Click Here For Your Feedback' href='" + custVendorPortalUrl + "CustFeedback/Feedback?Token=" + encFHSystemID + "' target='_blank'>Click Here For Your Feedback</a>";
                ToName = contactName.Trim();
                Body = "Please Click below Link For Giving Your Valuable FeedBack.";
                sbMailBody.Append("<table cellpadding='8' cellspacing='5' border='0' width='100%' style='box-shadow: 0 0 4px rgba(0, 0, 0, 0.3);'>");
                sbMailBody.Append("<tr><td><b> Dear " + ToName + "</b></td></tr>");
                sbMailBody.Append("<tr><td>" + Body + "</td></tr>");
                sbMailBody.Append("<tr><td>" + Link + "</td></tr>");
                sbMailBody.Append("<tr><td>Thanks.</td></tr>");
                sbMailBody.Append("<tr><td><b>Prakash Chemicals Agencies Pvt. Ltd.</b></td></tr>");
                sbMailBody.Append("</table>");

                emailService.SendEmail(ToEmail, CCEmail, "Customer Feedback - PrakashCRM", sbMailBody.ToString());
                flag = true;

            }

            return flag;
        }

        [Route("GetAllProducts")]
        public List<SPItemList> GetAllProducts()
        {
            API ac = new API();
            List<SPItemList> items = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            return items;
        }


        [Route("AddContactProducts")]
        public bool AddContactProducts(string CCompanyNo, string ProdNo, string SPCode, string CustomerNo)
        {
            bool flag = false;

            API ac = new API();
            SPContactProductsPost reqContactProduct = new SPContactProductsPost();
            reqContactProduct.Contact_No = CCompanyNo;
            reqContactProduct.Item_No = ProdNo;
            reqContactProduct.SalesPerson_Code = SPCode;

            if (CustomerNo == null)
                reqContactProduct.Customer_No = "";
            else
                reqContactProduct.Customer_No = CustomerNo;

            reqContactProduct.IsActive = true;

            errorDetails ed = new errorDetails();
            SPContactProductsPost resContactProduct = new SPContactProductsPost();

            var result = ac.PostItem("ContactProductsDotNetAPI", reqContactProduct, resContactProduct);

            if (result.Result.Item1 != null)
            {
                resContactProduct = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        public async Task<(SPFeedBackHeaderOnSendLinkRes, errorDetails)> PostItemFeedbackHeader<SPFeedBackHeaderOnSendLinkRes>(string apiendpoint, SPFeedBackHeaderOnSendLink requestModel, SPFeedBackHeaderOnSendLinkRes responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/Sandbox/api/pcpl/pcplcrm/v1.0/companies(8e7f221c-b3ab-ee11-a56a-000d3a3e6d80)/feedbackheaderset");
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
                    responseModel = res.ToObject<SPFeedBackHeaderOnSendLinkRes>();

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

        [Route("GetAllCountryForDDL")]
        public List<Country> GetAllCountryForDDL()
        {
            API ac = new API();
            List<Country> country = new List<Country>();

            var result = ac.GetData<Country>("CountriesRegionsListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                country = result.Result.Item1.value;

            return country;
        }

        [Route("GetAllStateForDDL")]
        public List<State> GetAllStateForDDL()
        {
            API ac = new API();
            List<State> state = new List<State>();

            var result = ac.GetData<State>("StatesListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                state = result.Result.Item1.value;

            return state;
        }

        [Route("GetAllPostCodesForDDL")]
        public List<PostCodes> GetAllCityForDDL()
        {
            API ac = new API();
            List<PostCodes> postcodes = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                postcodes = result.Result.Item1.value;

            return postcodes;
        }

        [Route("GetPincodeForDDL")]
        public List<PostCodes> GetPincodeForDDL(string prefix)
        {
            API ac = new API();
            List<PostCodes> pincode = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "startswith(Code,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)
                pincode = result.Result.Item1.value;

            List<PostCodes> returnpc = pincode.DistinctBy(x => x.Code).ToList();

            return returnpc;
        }

        [Route("GetAllAreasForDDL")]
        public List<Area> GetAllAreasForDDL()
        {
            API ac = new API();
            List<Area> areas = new List<Area>();

            var result = ac.GetData<Area>("AreasListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                areas = result.Result.Item1.value;

            return areas;
        }

        [Route("GetAllDistrictForDDL")]
        public List<District> GetAllDistrictForDDL()
        {
            API ac = new API();
            List<District> district = new List<District>();

            var result = ac.GetData<District>("DistrictListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                district = result.Result.Item1.value;

            return district;
        }

        [Route("GetAllIndustryForDDL")]
        public List<Industry> GetAllIndustryForDDL()
        {
            API ac = new API();
            List<Industry> industry = new List<Industry>();

            var result = ac.GetData<Industry>("IndustryGroupsListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                industry = result.Result.Item1.value;

            return industry;
        }

        [Route("GetAllBusinessTypeForDDL")]
        public List<BusinessType> GetAllBusinessTypeForDDL()
        {
            API ac = new API();
            List<BusinessType> businesstype = new List<BusinessType>();

            var result = ac.GetData<BusinessType>("BusinessTypeListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                businesstype = result.Result.Item1.value;

            return businesstype;
        }

        [Route("GetAllSalesPersonForDDL")]
        public List<SPSalespeoplePurchaser> GetAllSalesPersonForDDL()
        {
            API ac = new API();
            List<SPSalespeoplePurchaser> salesperson = new List<SPSalespeoplePurchaser>();

            var result = ac.GetData<SPSalespeoplePurchaser>("SalespersonPurchaserDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                salesperson = result.Result.Item1.value;

            return salesperson;
        }

        [Route("GetAllSourceofContactsForDDL")]
        public List<SourceofContacts> GetAllSourceofContactsForDDL()
        {
            API ac = new API();
            List<SourceofContacts> sourceofcontacts = new List<SourceofContacts>();

            var result = ac.GetData<SourceofContacts>("SourceofContactsDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                sourceofcontacts = result.Result.Item1.value;

            return sourceofcontacts;
        }

        [Route("GetAllDepartmentForDDL")]
        public List<Departments> GetAllDepartmentForDDL()
        {
            API ac = new API();
            List<Departments> departments = new List<Departments>();

            var result = ac.GetData<Departments>("DepartmentsDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                departments = result.Result.Item1.value;

            return departments;
        }

        [Route("GetAllPincodeForDDL")]
        public List<Pincode> GetAllPincodeForDDL()
        {
            API ac = new API();
            List<Pincode> pincode = new List<Pincode>();

            var result = ac.GetData<Pincode>("PostCodesDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                pincode = result.Result.Item1.value;

            return pincode;
        }

        [Route("GetDetailsByCode")]
        public List<PostCodes> GetDetailsByCode(string Code)
        {
            API ac = new API();
            List<PostCodes> postcodes = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "Code eq '" + Code + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                postcodes = result.Result.Item1.value;

            return postcodes;
        }

        [Route("GetAreasByPincodeForDDL")]
        public List<Area> GetAreasByPincodeForDDL(string Pincode)
        {
            API ac = new API();
            List<Area> areas = new List<Area>();


            var result = ac.GetData<Area>("AreasListDotNetAPI", "Pincode eq '" + Pincode + "' and IsActive eq true");
            if (result != null && result.Result.Item1.value.Count > 0)
                areas = result.Result.Item1.value;

            return areas;
        }

        public async Task<(List<SPSQInvDetailsRes>, errorDetails)> PostItemForGetInventoryDetails<SPSQInvDetailsRes>(string apiendpoint, SPSQInvDetails requestModel, List<SPSQInvDetailsRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CreateAvailableQty_GetAvailableQty?company=\'Prakash Company\'");
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
                    SPSQInvDetailsOData invDetailsOData = res.ToObject<SPSQInvDetailsOData>();
                    string invDetailsData = invDetailsOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQInvDetailsRes>>(invDetailsData);

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

        public async Task<(SPDeleteContactProdOData, errorDetails)> PostItemForDeleteContactProd<SPDeleteContactProdOData>(string apiendpoint, SPDeleteContactProd requestModel, SPDeleteContactProdOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_DeleteContactProducts?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPDeleteContactProdOData>();

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
    }
}