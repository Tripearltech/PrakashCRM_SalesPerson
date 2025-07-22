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
    [RoutePrefix("api/SPSiteError")]
    public class SPSPSiteErrorController : ApiController
    {
        [Route("GetSiteError")]
        public List<SPSiteError> GetSiteError(int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSiteError> siteerror = new List<SPSiteError>();
            
            if (filter == null)
                filter = "";

            var result = ac.GetData1<SPSiteError>("SiteErrorsListDotNetAPI", filter, skip, top, orderby); 

            if (result.Result.Item1.value.Count > 0)
                siteerror = result.Result.Item1.value;
            
            return siteerror;
        }
        
        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == null)
                filter = "";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

    }
}
