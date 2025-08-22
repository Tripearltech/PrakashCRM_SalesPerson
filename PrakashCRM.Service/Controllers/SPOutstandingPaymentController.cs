using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPOutstandingPayment")]
    public class SPOutstandingPaymentController : ApiController
    {
        [Route("GetOutstandingPaymentDetails")]
        public List<SPOutstandingPaymentList> GetOutstandingPaymentDetails(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPOutstandingPaymentList> osPaymentDetails = new List<SPOutstandingPaymentList>();

            if (filter == "" || filter == null)
                filter = "Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";

            var result = ac.GetData1<SPOutstandingPaymentList>("CustomerLedgerEntriesDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                osPaymentDetails = result.Result.Item1.value;

            for (int i = 0; i < osPaymentDetails.Count; i++)
            {
                string[] strDate = osPaymentDetails[i].Due_Date.Split('-');
                osPaymentDetails[i].Due_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                string[] strDate1 = osPaymentDetails[i].Posting_Date.Split('-');
                osPaymentDetails[i].Posting_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return osPaymentDetails;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        // Added the customerOutStanding Report
        [HttpGet]
        [Route("GetCustomerOutStanding")]
        public List<SPCustmerOutStandingDtails> GetCustomerOutStanding()
        {
            API ac = new API();
            List<SPCustmerOutStandingDtails> outStandingDtails = new List<SPCustmerOutStandingDtails>();

            var Result1 = ac.GetData<SPCustmerOutStandingDtails>("DailyCustomerCollectionView", "");
            if (Result1 != null && Result1.Result.Item1.value.Count > 0)
                outStandingDtails = Result1.Result.Item1.value;

            return outStandingDtails;
        }
    }
}
