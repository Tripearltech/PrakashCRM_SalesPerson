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

    public class SPNonPerfomingCuslist
    {
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Salesperson_Code { get; set; }
    }


   /* public class CollectionDataModel
    {
        public string Location_Code { get; set; }
        public string Salesperson_Code { get; set; }
        public double CollectionuptoMTD { get; set; }
        public double CollRecdforthePeriod { get; set; }
        public double TotalCollectionRecdtilltoday { get; set; }
        public double Overdueuptopreviousmonthdue { get; set; }

        //public double _x0031_st10thdueofcurrentmonth { get; set; }
        //public double _x0031_1th20thdueofcurrentmonth { get; set; }
        //public double _x0032_1st30_31stdueofcurrentmonth { get; set; }

        //public double AchivementinPercent { get; set; }

    }

    public class SPIOutStandinglistPost
    {
        public string fromdate { get; set; }
        public string todate { get; set; }
    }

    public class SPIOutStandinglistOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public Boolean value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPIOutStandinglistDetails
    {
        public string fromdate { get; set; }
        public string todate { get; set; }
    }*/

    public class SPSelaspersonlist
    {
        public string SalesPerson_Name{ get; set; }
        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public bool IsSalesPerson { get; set; }
        public double Sales_Percentage_Qty { get; set; }
    }
    public class SPSupportSPlist
    {
        public string SalesPerson { get; set; }
        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public double Sales_Percentage_Qty { get; set; }
    }
    public class SPProductlist
    {
        public string Product_Name { get; set; }
        public double Product_Total_Target_Qty { get; set; }
        public double Product_Total_Sales_Qty { get; set; }
        public double Product_Sales_Percentage_Qty { get; set; }
        public bool IsSalesPerson { get; set; }
        public bool IsProduct { get; set; }
        public bool IsIncludTop10Product { get; set; }
    }

    public class CombinedSalesData
    {
        public List<SPSelaspersonlist> Salespersons { get; set; }
        public List<SPSupportSPlist> SupportSPs { get; set; }
        public List<SPProductlist> Products { get; set; }
    }
    public class FeedBackQuestion
    {
        public string No { get; set; }
        public string Contact_No { get; set; }
        public string Contact_Person { get; set; }
        public string Products { get; set; }
        public string Overall_Rating { get; set; }
        public string Overall_Rating_Comments { get; set; }
        public string Suggestion { get; set; }
        public string Employee_Name { get; set; }

    }
    public class FeedbBackLines
    {
        public string Feedback_Header_No { get; set; }
        public string Feedback_Question_No { get; set; }
        public string Feedback_Question { get; set; }
        public string Rating { get; set; }
        public string Comments { get; set; }
    }
}
