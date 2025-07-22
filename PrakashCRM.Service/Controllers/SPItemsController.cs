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
    [RoutePrefix("api/SPItems")]
    public class SPItemsController : ApiController
    {
        [Route("GetAllItems")]
        public List<SPItemList> GetAllItems()
        {
            API ac = new API();
            List<SPItemList> items = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;
            
            return items;
        }

        [Route("GetItemsFromItemPackingStyle")]
        public List<SPItemPackingStyleDetails> GetItemsFromItemPackingStyle()
        {
            API ac = new API();

            List<SPItemPackingStyleDetails> items = new List<SPItemPackingStyleDetails>();

            var result = ac.GetData<SPItemPackingStyleDetails>("ItemPackingStyleDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            return items;
        }
    }
}
