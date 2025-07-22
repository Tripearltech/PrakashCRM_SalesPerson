using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;

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
    }
}