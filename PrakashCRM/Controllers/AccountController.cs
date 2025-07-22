using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{

    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public async Task<JsonResult> CheckLoginAndSendOTP(string email, string pass, string adminContactNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "CheckEmailPassForOTP?email=" + email + "&pass=" + pass.Trim();

            HttpClient client = new HttpClient();

            ContactNoOTPForLogin contactNoOTPForLogin = new ContactNoOTPForLogin();
            ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                contactNoOTPForLogin = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(jsonData);

                if (contactNoOTPForLogin.No != null)
                {
                    if (contactNoOTPForLogin.PCPL_Enable_OTP_On_Login)
                    {
                        Task<ContactNoOTPForLogin> task = Task.Run<ContactNoOTPForLogin>(async () => await GenerateOTPAndSend(contactNoOTPForLogin, adminContactNo));
                        contactNoOTPForLoginRes = task.Result;
                    }
                    else
                    {
                        Session["loggedInUserNo"] = contactNoOTPForLogin.No;
                        Session["loggedInUserFName"] = contactNoOTPForLogin.First_Name;
                        Session["loggedInUserLName"] = contactNoOTPForLogin.Last_Name;
                        Session["loggedInUserEmail"] = contactNoOTPForLogin.Company_E_Mail;
                        Session["loggedInUserJobTitle"] = contactNoOTPForLogin.Job_Title;
                        Session["loggedInUserRole"] = contactNoOTPForLogin.Role;
                        Session["loggedInUserMobile"] = contactNoOTPForLogin.Mobile_Phone_No;
                        Session["loggedInUserSPCode"] = contactNoOTPForLogin.Salespers_Purch_Code;

                        string SPCodesOfReportingPersonUser = "";
                        Task<string> task = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLogin.No));
                        SPCodesOfReportingPersonUser = task.Result;

                        if (SPCodesOfReportingPersonUser != "")
                            Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                        else
                            Session["SPCodesOfReportingPersonUser"] = "";
                    }
                    contactNoOTPForLoginRes = contactNoOTPForLogin;
                }

            }

            return Json(contactNoOTPForLoginRes, JsonRequestBehavior.AllowGet);
        }

        public async Task<string> GetSPCodesOfReportingPersonUser(string LoggedInUserNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "GetSPCodesOfReportingPersonUser?LoggedInUserNo=" + LoggedInUserNo;

            HttpClient client = new HttpClient();
            List<SPCodesOfReportingPersonUser> spCodes = new List<SPCodesOfReportingPersonUser>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                spCodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCodesOfReportingPersonUser>>(jsonData);
            }

            string spCodes_ = "";
            if(spCodes.Count > 0)
            {
                for (int a = 0; a < spCodes.Count; a++)
                {
                    spCodes_ += spCodes[a].Salespers_Purch_Code + ",";
                }

                spCodes_ = spCodes_.Substring(0, spCodes_.Length - 1);
            }

            return spCodes_;
        }

        public async Task<ContactNoOTPForLogin> GenerateOTPAndSend(ContactNoOTPForLogin contactNoOTPForLogin, string adminContactNo)
        {
            HttpClient client1 = new HttpClient();
            ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

            int length = 4;
            const string valid = "1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            string generatedOTP = res.ToString();

            string smsApiUrl = ConfigurationManager.AppSettings["SMSApiUrl"].ToString();
            string smsFromMobile = ConfigurationManager.AppSettings["SMSFromMobile"].ToString();
            string smsFromPass = ConfigurationManager.AppSettings["SMSFromPass"].ToString();
            string smsFromSenderId = ConfigurationManager.AppSettings["SMSFromSenderId"].ToString();
            string smsToMobile = contactNoOTPForLogin.Phone_No_2;
            //var parameters = new Dictionary<string, string> {
            //        { "mobile", smsFromMobile },
            //        { "pass", smsFromPass },
            //        { "senderid", smsFromSenderId},
            //        { "to", smsToMobile },
            //        { "msg","Quote no " + generatedOTP + " of 123.59 has been approved. Click on https://www.google.com for further details." },
            //        { "templateid", "1207161552588210502" }
            //    };

            var parameters = new Dictionary<string, string> {
                    { "mobile", smsFromMobile },
                    { "pass", smsFromPass },
                    { "senderid", smsFromSenderId},
                    { "to", smsToMobile },
                    { "msg","Your OTP to log in to the PCAPL Web Portal is " + generatedOTP + ". Please do not share this OTP with anyone. If you did not attempt to log in, call " + adminContactNo + " immediately." },
                    { "templateid", "1207173708488227586" }
                };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var response1 = await client1.PostAsync(smsApiUrl, encodedContent).ConfigureAwait(false);
            if (response1.StatusCode == HttpStatusCode.OK)
            {
                // Do something with response. Example get content:
                // var responseContent = await response.Content.ReadAsStringAsync ().ConfigureAwait (false);

                HttpClient client2 = new HttpClient();
                //ContactNoOTPForLogin contactNoOTPForLoginForUpdate = new ContactNoOTPForLogin();

                //contactNoOTPForLoginForUpdate.No = contactNoOTPForLogin.No;
                //contactNoOTPForLoginForUpdate.PCPL_OTP = generatedOTP;
                //contactNoOTPForLoginForUpdate.Phone_No_2 = contactNoOTPForLogin.Phone_No_2;
                //contactNoOTPForLoginForUpdate.First_Name = contactNoOTPForLogin.First_Name;
                //contactNoOTPForLoginForUpdate.Last_Name = contactNoOTPForLogin.Last_Name;
                //contactNoOTPForLoginForUpdate.Company_E_Mail = contactNoOTPForLogin.Company_E_Mail;
                //contactNoOTPForLoginForUpdate.Job_Title = contactNoOTPForLogin.Job_Title;
                //contactNoOTPForLoginForUpdate.Mobile_Phone_No = contactNoOTPForLogin.Mobile_Phone_No;
                //contactNoOTPForLoginForUpdate.Salespers_Purch_Code = contactNoOTPForLogin.Salespers_Purch_Code;

                ContactNoOTPForLoginUpdate contactNoOTPForLoginForUpdate = new ContactNoOTPForLoginUpdate();

                contactNoOTPForLoginForUpdate.PCPL_OTP = generatedOTP;

                string apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

                apiUrl1 = apiUrl1 + "UpdateOTPForLogin?SPNo=" + contactNoOTPForLogin.No;

                client2.BaseAddress = new Uri(apiUrl1);
                client2.DefaultRequestHeaders.Accept.Clear();
                client2.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string ObjString = JsonConvert.SerializeObject(contactNoOTPForLoginForUpdate);
                var content = new StringContent(ObjString, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl1),
                    Content = content
                };

                HttpResponseMessage response2 = await client2.SendAsync(request);
                if (response2.IsSuccessStatusCode)
                {
                    var data = await response2.Content.ReadAsStringAsync();
                    contactNoOTPForLoginRes = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(data);
                }
            }

            return contactNoOTPForLoginRes;
        }

        //OTP functionality on Login Start
        public async Task<string> CheckLogin(string SPNo, string OTP, string ContactNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "GetByNoOTP?SPNo=" + SPNo + "&OTP=" + OTP;

            HttpClient client = new HttpClient();

            LoggedInUserProfile loggedInUserProfile = new LoggedInUserProfile();
            string loggedInUser = "";

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";

            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                loggedInUserProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<LoggedInUserProfile>(jsonData);
            }
            if (loggedInUserProfile.First_Name != null)
            {
                loggedInUser = loggedInUserProfile.First_Name + ' ' + loggedInUserProfile.Last_Name;
                Session["loggedInUserNo"] = loggedInUserProfile.No;
                Session["loggedInUserFName"] = loggedInUserProfile.First_Name;
                Session["loggedInUserLName"] = loggedInUserProfile.Last_Name;
                Session["loggedInUserEmail"] = loggedInUserProfile.Company_E_Mail;
                Session["loggedInUserJobTitle"] = loggedInUserProfile.Job_Title;
                Session["loggedInUserRole"] = loggedInUserProfile.Role;
                Session["loggedInUserMobile"] = loggedInUserProfile.Mobile_Phone_No;
                Session["loggedInUserSPCode"] = loggedInUserProfile.Salespers_Purch_Code;
                
                SPUpdateOTP contactNoOTPForLoginRes = new SPUpdateOTP();

                Task<SPUpdateOTP> task = Task.Run<SPUpdateOTP>(async () => await UpdateOTPBlank(SPNo, ContactNo));
                contactNoOTPForLoginRes = task.Result;

                string SPCodesOfReportingPersonUser = "";
                Task<string> task1 = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLoginRes.No));
                SPCodesOfReportingPersonUser = task1.Result;

                if (SPCodesOfReportingPersonUser != "")
                    Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                else
                    Session["SPCodesOfReportingPersonUser"] = "";


            }
            return loggedInUser;
        }

        public async Task<SPUpdateOTP> UpdateOTPBlank(string SPNo, string ContactNo)
        {

            HttpClient client1 = new HttpClient();

            SPUpdateOTP contactNoOTPForLoginForUpdate = new SPUpdateOTP();
            SPUpdateOTP contactNoOTPForLoginRes = new SPUpdateOTP();
            
            string apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            contactNoOTPForLoginForUpdate.No = SPNo;
            contactNoOTPForLoginForUpdate.PCPL_OTP = "";
            contactNoOTPForLoginForUpdate.Phone_No_2 = ContactNo;

            apiUrl1 = apiUrl1 + "UpdateOTPBlank?SPNo=" + contactNoOTPForLoginForUpdate.No;

            client1.BaseAddress = new Uri(apiUrl1);
            client1.DefaultRequestHeaders.Accept.Clear();
            client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string ObjString = JsonConvert.SerializeObject(contactNoOTPForLoginForUpdate);
            var content = new StringContent(ObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl1),
                Content = content
            };

            HttpResponseMessage response1 = await client1.SendAsync(request);
            if (response1.IsSuccessStatusCode)
            {
                var data = await response1.Content.ReadAsStringAsync();
                contactNoOTPForLoginRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPUpdateOTP>(data);
            }

            return contactNoOTPForLoginRes;
        }
        //OTP functionality on Login End

        public async Task<bool> ResendOTP(string email, string pass, string adminContactNo)
        {
            bool flag = false;

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "CheckEmailPassForOTP?email=" + email + "&pass=" + pass;

            HttpClient client = new HttpClient();

            ContactNoOTPForLogin contactNoOTPForLogin = new ContactNoOTPForLogin();
            ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                contactNoOTPForLogin = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(jsonData);

                if (contactNoOTPForLogin.No != null)
                {
                    if(contactNoOTPForLogin.PCPL_Enable_OTP_On_Login)
                    {
                        Task<ContactNoOTPForLogin> task = Task.Run<ContactNoOTPForLogin>(async () => await GenerateOTPAndSend(contactNoOTPForLogin, adminContactNo));
                        contactNoOTPForLoginRes = task.Result;
                    }
                    else
                    {
                        contactNoOTPForLoginRes = contactNoOTPForLogin;
                        Session["loggedInUserNo"] = contactNoOTPForLogin.No;
                        Session["loggedInUserFName"] = contactNoOTPForLogin.First_Name;
                        Session["loggedInUserLName"] = contactNoOTPForLogin.Last_Name;
                        Session["loggedInUserEmail"] = contactNoOTPForLogin.Company_E_Mail;
                        Session["loggedInUserJobTitle"] = contactNoOTPForLogin.Job_Title;
                        Session["loggedInUserRole"] = contactNoOTPForLogin.Role;
                        Session["loggedInUserMobile"] = contactNoOTPForLogin.Mobile_Phone_No;
                        Session["loggedInUserSPCode"] = contactNoOTPForLogin.Salespers_Purch_Code;

                        string SPCodesOfReportingPersonUser = "";
                        Task<string> task1 = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLoginRes.No));
                        SPCodesOfReportingPersonUser = task1.Result;

                        if (SPCodesOfReportingPersonUser != "")
                            Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                        else
                            Session["SPCodesOfReportingPersonUser"] = "";

                    }

                }

            }
            
            flag = true;

            return flag;
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session["loggedInUserNo"] = "";
            Session["loggedInUserFName"] = "";
            Session["loggedInUserLName"] = "";
            Session["loggedInUserEmail"] = "";
            Session["loggedInUserJobTitle"] = "";
            Session["loggedInUserRole"] = "";
            Session["loggedInUserMobile"] = "";
            Session["SPProfileImage"] = null;
            Session["SPCodesOfReportingPersonUser"] = "";

            return RedirectToAction("Login", "Account");
        }

        public ActionResult ResetForgotPassword()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }
    }
}