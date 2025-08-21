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

    public class SPInwardDetails
    {
        public string PCPL_Vendor_Name { get; set; }
        public string PCPL_Mfg_Name { get; set; }
        public string Document_Type { get; set; }
        public string Lot_No { get; set; }
        public string PCPL_Remarks { get; set; }
        public string Document_No { get; set; }
        public string Posting_Date { get; set; }
        public double Remaining_Quantity { get; set; }
        public double Reserved_Quantity { get; set; }
        public string Entry_Type { get; set; }
        public double Quantity { get; set; }
        public string Source_Description { get; set; }
        public string PCPL_Salesperson_Code { get; set; }
        public string PCPL_Original_Buying_Date { get; set; }
        public int No_of_days { get; set; }
        public double Cost_Amount_Actual { get; set; }
        public string Item_Category_Code { get; set; }
        public string Item_Description { get; set; }
        public string Location_Code { get; set; }
        public string Item_No { get; set; }

        public double Outstanding_Quantity { get; set; }
        public bool Positive { get; set; }
    }
    public class SPReservedQtyDetails
    {
        public string Location_Code { get; set; }
        public string Item_Category_Code { get; set; }
        public string Description { get; set; }
        public string Posting_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public double Outstanding_Quantity { get; set; }
        public string PCPL_Salesperson_Name { get; set; }
        public string PCPL_Remarks { get; set; }
        public bool Positive { get; set; }

    }
}
