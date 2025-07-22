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
    [RoutePrefix("api/SPSiteActivity")]
    public class SPSiteActivityController : ApiController
    {
        [Route("GetSiteActivity")]
        public List<SPSiteActivity> GetSiteActivity(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSiteActivity> siteactivity = new List<SPSiteActivity>();
            
            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPCode + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

            var result = ac.GetData1<SPSiteActivity>("SiteActivitiesListDotNetAPI", filter, skip, top, orderby); 

            if (result.Result.Item1.value.Count > 0)
                siteactivity = result.Result.Item1.value;
            
            return siteactivity;
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
