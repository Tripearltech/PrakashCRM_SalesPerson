using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPDashboardDetails
    {
        public int OrdersCount { get; set; }

        public int InvoicesCount { get; set; }

        public int CustomersCount { get; set; }

        public int ContactsCount { get; set; }

        public int QuotesCount { get; set; }

        public int InquiryCount { get; set; }
    }

    public class GetBranchWiseTotalSum
    {
        public string LocationCode { get; set; }
        public double Opening_LocationCode { get; set; }
        public double Inward_LocationCode { get; set; }
        public double Outward_LocationCode { get; set; }
        public double Reserved_LocationCode { get; set; }
        public double CLStock_LocationCode { get; set; }
    }
    public class ProductGroupsWise
    {
        public string Code { get; set; }
        public double Opening { get; set; }
        public double Inward { get; set; }
        public double Outward { get; set; }
        public double CLStock { get; set; }
        public double Reserved { get; set; }
        public string Location_Filter_FilterOnly { get; set; }
    }

    public class ItemWise
    {
        public string ItemName { get; set; }
        public double Opening_Item { get; set; }
        public double Inward_Item { get; set; }
        public double Outward_Item { get; set; }
        public double CLStock_Item { get; set; }
        public double Reserved_Item { get; set; }
        public string Location_Filter_FilterOnly { get; set; }
    }

    public class SPInvGenerateDataPost
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
    }

    public class SPInvGenerateDataOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public Boolean value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPInvGenerateDetails
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
    }

}
