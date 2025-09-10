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
using System.Web.Http.Results;
using System.Web.Routing;
using System.Web.Security;
using System.Net.Http.Headers;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.SqlServer.Server;
using System.IO.Compression;
using Microsoft.Ajax.Utilities;
using Xipton.Razor.Extension;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPSalesQuotes")]
    public class SPSalesQuotesController : ApiController
    {
        [Route("GetAllSalesQuotes")]
        public List<SPSalesQuotesList> GetAllSalesQuotes(string LoggedInUserRole, string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSalesQuotesList> salesquotes = new List<SPSalesQuotesList>();
            bool isFirstSPCodeFilter = false;

            string SPCodes = "";
            if(SPCode != null)
            {
                if (SPCode.Contains(",") == true)
                {
                    string[] SPCode_ = SPCode.Split(',');
                    if (SPCode_[0] != "")
                        SPCodes = "(Salesperson_Code eq '" + SPCode_[0] + "'";
                    else
                        SPCodes = "(";

                    for (int a = 1; a < SPCode_.Length; a++)
                    {
                        if (SPCode_[a].Trim() != "")
                        {
                            if (SPCodes == "(")
                                SPCodes += "Salesperson_Code eq '" + SPCode_[a] + "'";
                            else
                                SPCodes += " OR Salesperson_Code eq '" + SPCode_[a] + "'";
                        }
                            

                    }
                    SPCodes += ")";
                }
            }
            //string SPCode_ = SPCode.Contains(",") == true ? 

            if (filter == "" || filter == null)
            {
                if(LoggedInUserRole == "Finance")
                    filter = "PCPL_IsInquiry eq false";
                else
                {
                    if (SPCode.Contains(",") == true)
                        filter = SPCodes + " and PCPL_IsInquiry eq false";
                    else
                        filter = "Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                }
            }
            else
            {
                if (LoggedInUserRole == "Finance")
                    filter = filter + " and PCPL_IsInquiry eq false";
                else
                {
                    if (SPCode.Contains(",") == true)
                        filter = filter + " and " + SPCodes + " and PCPL_IsInquiry eq false";
                    else
                        filter = filter + " and Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                }
            }
                
            var result = ac.GetData1<SPSalesQuotesList>("SalesQuoteDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                salesquotes = result.Result.Item1.value;

            for (int i = 0; i < salesquotes.Count; i++)
            {
                string[] strDate = salesquotes[i].Order_Date.Split('-');
                salesquotes[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                //string[] strDate1 = salesquotes[i].Requested_Delivery_Date.Split('-');
                //salesquotes[i].Requested_Delivery_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return salesquotes;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string Page, string LoggedInUserNo, string UserRoleORReportingPerson, string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            string SPCodes = "";
            if (SPCode != null && SPCode.Contains(",") == true)
            {
                string[] SPCode_ = SPCode.Split(',');
                SPCodes = "(Salesperson_Code eq '" + SPCode_[0] + "'";
                for (int a = 1; a < SPCode_.Length; a++)
                {
                    if (SPCode_[a].Trim() != "")
                        SPCodes += " OR Salesperson_Code eq '" + SPCode_[a] + "'";
                    
                }
                SPCodes += ")";
            }

            if (Page == "SQListForApproveReject")
            {
                if (filter == "" || filter == null)
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                    else if (UserRoleORReportingPerson == "ReportingPerson")
                        filter = "PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
                }
                else
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                    else if (UserRoleORReportingPerson == "ReportingPerson")
                        filter = filter + " and PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
                }
            
            }
            else if(Page == "SQList")
            {
                if (filter == "" || filter == null)
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = "PCPL_IsInquiry eq false";
                    else
                    {
                        if (SPCode.Contains(",") == true)
                            filter = SPCodes + " and PCPL_IsInquiry eq false";
                        else
                            filter = "Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                    }
                }
                else
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = filter + " and PCPL_IsInquiry eq false";
                    else
                    {
                        if (SPCode.Contains(",") == true)
                            filter = filter + " and " + SPCodes + " and PCPL_IsInquiry eq false";
                        else
                            filter = filter + " and Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                    }
                }
            }

            //if (Page == "SQListForApproveReject")
            //   filter += " and PCPL_Status ne '0'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetSalesLineItems")]
        public List<SPSQLines> GetSalesLineItems(string DocumentNo)
        {
            API ac = new API();
            List<SPSQLines> SQLines = new List<SPSQLines>();

            var result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + DocumentNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                SQLines = result.Result.Item1.value;

            return SQLines;
        }

        [Route("GetOrderedQtyDetails")]
        public SPSQOrderedQtyDetails GetOrderedQtyDetails(string SQNo)
        {
            API ac = new API();
            SPSQOrderedQtyDetails orderedQtyDetails = new SPSQOrderedQtyDetails();

            var result = ac.GetData<SPSQOrderedQtyDetails>("OrderedQty", "No__FilterOnly eq '" + SQNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                orderedQtyDetails = result.Result.Item1.value[0];

            return orderedQtyDetails;
        }

        [Route("GetInvoicedQtyDetails")]
        public List<SPSQInvoicedQtyDetails> GetInvoicedQtyDetails(string SQNo)
        {
            API ac = new API();
            List<SPSQInvoicedQtyDetails> invoicedQtyDetails = new List<SPSQInvoicedQtyDetails>();

            var result = ac.GetData<SPSQInvoicedQtyDetails>("InvoicedQty", "No__FilterOnly eq '" + SQNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                invoicedQtyDetails = result.Result.Item1.value;

            return invoicedQtyDetails;
        }

        [Route("GetInProcessQtyDetails")]
        public List<SPSQInProcessQtyDetails> GetInProcessQtyDetails(string SQNo)
        {
            API ac = new API();
            List<SPSQInProcessQtyDetails> inProcessQtyDetails = new List<SPSQInProcessQtyDetails>();

            var result = ac.GetData<SPSQInProcessQtyDetails>("SalesOrderCardDotNetAPI", "Quote_No eq '" + SQNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                inProcessQtyDetails = result.Result.Item1.value;

            return inProcessQtyDetails;
        }

        [Route("SalesQuote")]
        public SPSQHeader SalesQuote(SPSQHeaderDetails salesQuoteDetails, string LoggedInSPUserEmail, string SPName) //string ApprovalFormatFile
        {
            SPSQHeaderPost requestSQHeader = new SPSQHeaderPost();
            SPSQHeaderPostWithCustTemplateCode reqSQHeaderWithCustTemplateCode =
                new SPSQHeaderPostWithCustTemplateCode();
            SPSQHeaderUpdate requestSQHeaderUpdate = new SPSQHeaderUpdate();
            SPSQHeader responseSQHeader = new SPSQHeader();
            string LocationCode = "", CustomerName="";
            var ac = new API();
            errorDetails ed = new errorDetails();
            string ApprovalFormatFile = Unzip(salesQuoteDetails.zipApprovalFormatFile);
            int QuoteValidityDays = 0;

            SPFinanceUserDetails financeUserDetails = new SPFinanceUserDetails();
            financeUserDetails.No = "";

            var resFinanceUserDetails = ac.GetData<SPFinanceUserDetails>("EmployeesDotNetAPI", "Role eq 'Finance'");

            if (resFinanceUserDetails != null && resFinanceUserDetails.Result.Item1.value.Count > 0)
                financeUserDetails = resFinanceUserDetails.Result.Item1.value[0];

            //financeUserDetails.Company_E_Mail = "mihir.s@tripearltech.com";

            SPUserReportingPersonDetails reportingPersonDetails = new SPUserReportingPersonDetails();
            reportingPersonDetails.Reporting_Person_No = "";
            string StatusForUrl = "";

            var resultUserDetails = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + salesQuoteDetails.SalespersonCode + "'");

            if (resultUserDetails != null && resultUserDetails.Result.Item1.value.Count > 0)
                reportingPersonDetails = resultUserDetails.Result.Item1.value[0];

            var result = (dynamic)null;

            if (!Convert.ToBoolean(salesQuoteDetails.IsSQEdit))
            {
                if (salesQuoteDetails.CustomerNo != null)
                {
                    //requestSQHeader.No = salesQuoteDetails.QuoteNo;
                    requestSQHeader.Order_Date = salesQuoteDetails.OrderDate;
                    requestSQHeader.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                    requestSQHeader.PCPL_Inquiry_No = salesQuoteDetails.InquiryNo == "" || salesQuoteDetails.InquiryNo == null ? "" : salesQuoteDetails.InquiryNo;
                    requestSQHeader.Sell_to_Contact_No = salesQuoteDetails.ContactCompanyNo;
                    requestSQHeader.Sell_to_Contact = salesQuoteDetails.ContactCompanyName;
                    requestSQHeader.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                    requestSQHeader.Sell_to_Customer_No = salesQuoteDetails.CustomerNo;
                    requestSQHeader.Salesperson_Code = salesQuoteDetails.SalespersonCode;
                    //requestSQHeader.Payment_Method_Code = salesQuoteDetails.PaymentMethodCode;
                    requestSQHeader.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                    requestSQHeader.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                    requestSQHeader.Location_Code = salesQuoteDetails.LocationCode;
                    requestSQHeader.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                    requestSQHeader.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;
                    requestSQHeader.PCPL_IsInquiry = false;
                    requestSQHeader.WorkDescription = salesQuoteDetails.JustificationDetails == "" ? "" : salesQuoteDetails.JustificationDetails;
                    requestSQHeader.PCPL_Target_Date = salesQuoteDetails.TargetDate == null || salesQuoteDetails.TargetDate == "" ? "1900-01-01" : salesQuoteDetails.TargetDate;

                    //if(Convert.ToDouble(salesQuoteDetails.AvailableCreditLimit) <= 0)
                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        requestSQHeader.PCPL_Approver = financeUserDetails.No;
                        requestSQHeader.PCPL_Status = StatusForUrl =  "Approval pending from finance";
                        requestSQHeader.PCPL_ApprovalFor = "Credit Limit";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_ApproverHOD = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        requestSQHeader.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                        requestSQHeader.PCPL_Status = StatusForUrl = "Approval pending from HOD";
                        requestSQHeader.PCPL_ApprovalFor = "Margin";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_Approver = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        requestSQHeader.PCPL_Approver = financeUserDetails.No;
                        requestSQHeader.PCPL_Status = StatusForUrl = "Approval pending from finance";
                        requestSQHeader.PCPL_ApprovalFor = "Both";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                    }
                    else
                    {
                        requestSQHeader.PCPL_Approver = "";
                        requestSQHeader.PCPL_Status = "Approved";
                        requestSQHeader.PCPL_ApprovalFor = "";
                        requestSQHeader.PCPL_Submitted_On = "1900-01-01";
                        requestSQHeader.PCPL_ApproverHOD = "";
                    }
                    //requestSQHeader.PCPL_Status = Convert.ToDouble(salesQuoteDetails.AvailableCreditLimit) < 0 ? "Pending For Approval" : "";

                    result = PostItemSQ("SalesQuoteDotNetAPI", requestSQHeader, responseSQHeader);
                }
                else
                {
                    //reqSQHeaderWithCustTemplateCode.No = salesQuoteDetails.QuoteNo;
                    reqSQHeaderWithCustTemplateCode.Order_Date = salesQuoteDetails.OrderDate;
                    reqSQHeaderWithCustTemplateCode.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                    reqSQHeaderWithCustTemplateCode.PCPL_Inquiry_No = salesQuoteDetails.InquiryNo == "" || salesQuoteDetails.InquiryNo == null ? "" : salesQuoteDetails.InquiryNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Contact_No = salesQuoteDetails.ContactCompanyNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Contact = salesQuoteDetails.ContactCompanyName;
                    reqSQHeaderWithCustTemplateCode.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Customer_No = salesQuoteDetails.CustomerNo == null ? "" : salesQuoteDetails.CustomerNo;
                    reqSQHeaderWithCustTemplateCode.Salesperson_Code = salesQuoteDetails.SalespersonCode;
                    //reqSQHeaderWithCustTemplateCode.Payment_Method_Code = salesQuoteDetails.PaymentMethodCode;
                    //reqSQHeaderWithCustTemplateCode.Transport_Method = salesQuoteDetails.TransportMethodCode;
                    reqSQHeaderWithCustTemplateCode.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                    reqSQHeaderWithCustTemplateCode.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                    reqSQHeaderWithCustTemplateCode.Location_Code = salesQuoteDetails.LocationCode;
                    reqSQHeaderWithCustTemplateCode.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                    reqSQHeaderWithCustTemplateCode.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;
                    reqSQHeaderWithCustTemplateCode.PCPL_IsInquiry = false;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Customer_Templ_Code = salesQuoteDetails.CustomerTemplateCode;
                    reqSQHeaderWithCustTemplateCode.WorkDescription = salesQuoteDetails.JustificationDetails == "" ? "" : salesQuoteDetails.JustificationDetails;
                    reqSQHeaderWithCustTemplateCode.PCPL_Target_Date = salesQuoteDetails.TargetDate == null || salesQuoteDetails.TargetDate == "" ? "1900-01-01" : salesQuoteDetails.TargetDate;

                    //if (Convert.ToDouble(salesQuoteDetails.AvailableCreditLimit) <= 0)
                    //{
                    //    reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from finance";
                    //    reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                    //}
                    //else
                    //{
                    //    reqSQHeaderWithCustTemplateCode.PCPL_Status = "";
                    //    reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = "1900-01-01";
                    //}

                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = financeUserDetails.No;
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from finance";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Credit Limit";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from HOD";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Margin";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = financeUserDetails.No;
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from HOD";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Both";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                    }
                    else
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = "";
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approved";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = "1900-01-01";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = "";
                    }

                    //reqSQHeaderWithCustTemplateCode.PCPL_Status = Convert.ToDouble(salesQuoteDetails.AvailableCreditLimit) < 0 ? "Pending For Approval" : "";

                    result = PostItemSQWithCustTemplateCode("SalesQuoteDotNetAPI", reqSQHeaderWithCustTemplateCode, responseSQHeader);
                }

                //

                if (result.Result.Item1 != null)
                {
                    responseSQHeader = result.Result.Item1;
                    ed = result.Result.Item2;
                    responseSQHeader.errorDetails = ed;
                }

                //

            }
            else
            {
                requestSQHeaderUpdate.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                requestSQHeaderUpdate.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                //requestSQHeaderUpdate.Payment_Method_Code = salesQuoteDetails.PaymentMethodCode;
                //requestSQHeaderUpdate.Transport_Method = salesQuoteDetails.TransportMethodCode;
                requestSQHeaderUpdate.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                requestSQHeaderUpdate.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                requestSQHeaderUpdate.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                requestSQHeaderUpdate.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;

                result = PatchItemSQ("SalesQuoteDotNetAPI", requestSQHeaderUpdate, responseSQHeader, "Document_Type='Quote',No='" + salesQuoteDetails.QuoteNo + "'");

                if (result.Result.Item1 != null)
                {
                    responseSQHeader = result.Result.Item1;
                    ed = result.Result.Item2;
                    responseSQHeader.errorDetails = ed;
                }
            }

            if (responseSQHeader.Sell_to_Customer_Name != null || responseSQHeader.Sell_to_Customer_Name != "")
                CustomerName = responseSQHeader.Sell_to_Customer_Name;

            LocationCode = salesQuoteDetails.LocationCode;

            if (result.Result.Item1.No != null)
            {
                responseSQHeader = result.Result.Item1;
                responseSQHeader.ItemLineNo = "";

                SPSQLinesPost reqSQLine = new SPSQLinesPost();
                SPSQLiquidLinesPost reqSQLiquidLine = new SPSQLiquidLinesPost();
                SPSQLinesUpdate reqSQLineUpdate = new SPSQLinesUpdate();
                SPSQLiquidLinesUpdate reqSQLiquidLineUpdate = new SPSQLiquidLinesUpdate();
                SPSQLines resSQLine = new SPSQLines();
                errorDetails ed1 = new errorDetails();

                for (int a = 0; a < salesQuoteDetails.Products.Count; a++)
                {
                    if (salesQuoteDetails.InquiryNo != null)
                    {

                        SPSQUpdateInqToQuote inqToQuoteReq = new SPSQUpdateInqToQuote();
                        SPInqLines inqToQuoteRes = new SPInqLines();
                        errorDetails edInqToQuote = new errorDetails();

                        inqToQuoteReq.PCPL_Convert_Quote = true;

                        var resultInqToQuote = PatchItemInqToQuote("InquiryProductsDotNetAPI", inqToQuoteReq, inqToQuoteRes, "Document_Type='Quote',Document_No='" + salesQuoteDetails.InquiryNo + "',Line_No=" + Convert.ToInt32(salesQuoteDetails.Products[a].InqProdLineNo));

                        if (resultInqToQuote.Result.Item1 != null)
                            inqToQuoteRes = resultInqToQuote.Result.Item1;

                        if (resultInqToQuote.Result.Item2.message != null)
                            edInqToQuote = result.Result.Item2;

                    }

                    var result1 = (dynamic)null;
                    if (!Convert.ToBoolean(salesQuoteDetails.IsSQEdit))
                    {
                        if (!Convert.ToBoolean(salesQuoteDetails.Products[a].IsLiquidProd))
                        {
                            reqSQLine.Document_No = responseSQHeader.No;
                            reqSQLine.No = salesQuoteDetails.Products[a].No;
                            reqSQLine.Type = "Item";
                            reqSQLine.PCPL_MRP = salesQuoteDetails.Products[a].PCPL_MRP;
                            //reqSQLine.PCPL_Basic_Price = salesQuoteDetails.Products[a].PCPL_Basic_Price;
                            reqSQLine.Location_Code = LocationCode;
                            reqSQLine.Quantity = salesQuoteDetails.Products[a].Quantity;
                            reqSQLine.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            reqSQLine.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLine.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLine.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLine.PCPL_Transport_Cost = salesQuoteDetails.Products[a].PCPL_Transport_Cost;
                            reqSQLine.PCPL_Commission_Payable = salesQuoteDetails.Products[a].PCPL_Commission_Payable == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Payable;
                            reqSQLine.PCPL_Commission_Type = salesQuoteDetails.Products[a].PCPL_Commission_Type == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Type;
                            reqSQLine.PCPL_Commission = salesQuoteDetails.Products[a].PCPL_Commission;
                            reqSQLine.PCPL_Commission_Amount = salesQuoteDetails.Products[a].PCPL_Commission_Amount;
                            //reqSQLine.PCPL_Discount_Type = salesQuoteDetails.Products[a].PCPL_Discount_Type;
                            //reqSQLine.PCPL_Discount = salesQuoteDetails.Products[a].PCPL_Discount;
                            reqSQLine.PCPL_Sales_Discount = salesQuoteDetails.Products[a].PCPL_Sales_Discount;
                            reqSQLine.PCPL_Credit_Days = salesQuoteDetails.Products[a].PCPL_Credit_Days;
                            reqSQLine.PCPL_Margin = salesQuoteDetails.Products[a].PCPL_Margin;
                            reqSQLine.PCPL_Margin_Percent = salesQuoteDetails.Products[a].PCPL_Margin_Percent;
                            reqSQLine.PCPL_Interest = salesQuoteDetails.Products[a].PCPL_Interest;
                            reqSQLine.PCPL_Interest_Rate = salesQuoteDetails.Products[a].PCPL_Interest_Rate;
                            reqSQLine.PCPL_Total_Cost = salesQuoteDetails.Products[a].PCPL_Total_Cost;
                            reqSQLine.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            reqSQLine.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;
                            reqSQLine.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;

                            if(salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                                reqSQLine.GST_Place_Of_Supply = "Bill-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1" && salesQuoteDetails.JobtoCode != "-1")
                                reqSQLine.GST_Place_Of_Supply = "Ship-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1")
                                reqSQLine.GST_Place_Of_Supply = "Ship-to Address";

                            reqSQLine.PCPL_Inquiry_No = salesQuoteDetails.InquiryNo == null || salesQuoteDetails.InquiryNo == "" ? "" : salesQuoteDetails.InquiryNo;
                            reqSQLine.PCPL_Inquiry_Line_No = salesQuoteDetails.Products[a].InqProdLineNo == null || salesQuoteDetails.Products[a].InqProdLineNo == "" ? 0 :
                               Convert.ToInt32(salesQuoteDetails.Products[a].InqProdLineNo);

                            result1 = PostItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLine", reqSQLine, reqSQLiquidLine, resSQLine);
                        }
                        else
                        {
                            reqSQLiquidLine.Document_No = responseSQHeader.No;
                            reqSQLiquidLine.No = salesQuoteDetails.Products[a].No;
                            reqSQLiquidLine.Type = "Item";
                            reqSQLiquidLine.PCPL_MRP = salesQuoteDetails.Products[a].PCPL_MRP;
                            //reqSQLine.PCPL_Basic_Price = salesQuoteDetails.Products[a].PCPL_Basic_Price;
                            reqSQLiquidLine.Location_Code = LocationCode;
                            reqSQLiquidLine.PCPL_Concentration_Rate_Percent = salesQuoteDetails.Products[a].PCPL_Concentration_Rate_Percent;
                            reqSQLiquidLine.Net_Weight = salesQuoteDetails.Products[a].Net_Weight;
                            reqSQLiquidLine.PCPL_Liquid_Rate = salesQuoteDetails.Products[a].PCPL_Liquid_Rate;
                            reqSQLiquidLine.PCPL_Liquid = salesQuoteDetails.Products[a].IsLiquidProd;
                            //reqSQLiquidLine.Quantity = salesQuoteDetails.Products[a].Quantity;
                            //reqSQLiquidLine.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            reqSQLiquidLine.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLine.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLiquidLine.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLiquidLine.PCPL_Transport_Cost = salesQuoteDetails.Products[a].PCPL_Transport_Cost;
                            reqSQLiquidLine.PCPL_Commission_Payable = salesQuoteDetails.Products[a].PCPL_Commission_Payable == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Payable;
                            reqSQLiquidLine.PCPL_Commission_Type = salesQuoteDetails.Products[a].PCPL_Commission_Type == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Type;
                            reqSQLiquidLine.PCPL_Commission = salesQuoteDetails.Products[a].PCPL_Commission;
                            reqSQLiquidLine.PCPL_Commission_Amount = salesQuoteDetails.Products[a].PCPL_Commission_Amount;
                            //reqSQLine.PCPL_Discount_Type = salesQuoteDetails.Products[a].PCPL_Discount_Type;
                            //reqSQLine.PCPL_Discount = salesQuoteDetails.Products[a].PCPL_Discount;
                            reqSQLiquidLine.PCPL_Sales_Discount = salesQuoteDetails.Products[a].PCPL_Sales_Discount;
                            reqSQLiquidLine.PCPL_Credit_Days = salesQuoteDetails.Products[a].PCPL_Credit_Days;
                            reqSQLiquidLine.PCPL_Margin = salesQuoteDetails.Products[a].PCPL_Margin;
                            reqSQLiquidLine.PCPL_Margin_Percent = salesQuoteDetails.Products[a].PCPL_Margin_Percent;
                            reqSQLiquidLine.PCPL_Interest = salesQuoteDetails.Products[a].PCPL_Interest;
                            reqSQLiquidLine.PCPL_Interest_Rate = salesQuoteDetails.Products[a].PCPL_Interest_Rate;
                            reqSQLiquidLine.PCPL_Total_Cost = salesQuoteDetails.Products[a].PCPL_Total_Cost;
                            reqSQLiquidLine.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            reqSQLiquidLine.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;

                            if (salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                                reqSQLiquidLine.GST_Place_Of_Supply = "Bill-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1" && salesQuoteDetails.JobtoCode != "-1")
                                reqSQLiquidLine.GST_Place_Of_Supply = "Ship-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1")
                                reqSQLiquidLine.GST_Place_Of_Supply = "Ship-to Address";

                            reqSQLiquidLine.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;
                            reqSQLiquidLine.PCPL_Inquiry_No = salesQuoteDetails.InquiryNo == null || salesQuoteDetails.InquiryNo == "" ? "" : salesQuoteDetails.InquiryNo;
                            reqSQLiquidLine.PCPL_Inquiry_Line_No = salesQuoteDetails.Products[a].InqProdLineNo == null || salesQuoteDetails.Products[a].InqProdLineNo == "" ? 0 :
                               Convert.ToInt32(salesQuoteDetails.Products[a].InqProdLineNo);

                            result1 = PostItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLiquidLine", reqSQLine, reqSQLiquidLine, resSQLine);
                        }

                    }
                    else
                    {
                        if (!Convert.ToBoolean(salesQuoteDetails.Products[a].IsLiquidProd))
                        {
                            //reqSQLineUpdate.Quantity = salesQuoteDetails.Products[a].Quantity;
                            //reqSQLineUpdate.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            //reqSQLineUpdate.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLineUpdate.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            //reqSQLineUpdate.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;
                            //reqSQLineUpdate.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;

                            //

                            reqSQLineUpdate.PCPL_MRP = salesQuoteDetails.Products[a].PCPL_MRP;
                            //reqSQLine.PCPL_Basic_Price = salesQuoteDetails.Products[a].PCPL_Basic_Price;
                            reqSQLineUpdate.Location_Code = LocationCode;
                            reqSQLineUpdate.Quantity = salesQuoteDetails.Products[a].Quantity;
                            reqSQLineUpdate.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            reqSQLineUpdate.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLineUpdate.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLineUpdate.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLineUpdate.PCPL_Transport_Cost = salesQuoteDetails.Products[a].PCPL_Transport_Cost;
                            reqSQLineUpdate.PCPL_Commission_Payable = salesQuoteDetails.Products[a].PCPL_Commission_Payable == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Payable;
                            reqSQLineUpdate.PCPL_Commission_Type = salesQuoteDetails.Products[a].PCPL_Commission_Type == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Type;
                            reqSQLineUpdate.PCPL_Commission = salesQuoteDetails.Products[a].PCPL_Commission;
                            reqSQLineUpdate.PCPL_Commission_Amount = salesQuoteDetails.Products[a].PCPL_Commission_Amount;
                            //reqSQLineUpdate.PCPL_Discount_Type = salesQuoteDetails.Products[a].PCPL_Discount_Type;
                            //reqSQLineUpdate.PCPL_Discount = salesQuoteDetails.Products[a].PCPL_Discount;
                            reqSQLineUpdate.PCPL_Sales_Discount = salesQuoteDetails.Products[a].PCPL_Sales_Discount;
                            reqSQLineUpdate.PCPL_Credit_Days = salesQuoteDetails.Products[a].PCPL_Credit_Days;
                            reqSQLineUpdate.PCPL_Margin = salesQuoteDetails.Products[a].PCPL_Margin;
                            reqSQLineUpdate.PCPL_Margin_Percent = salesQuoteDetails.Products[a].PCPL_Margin_Percent;
                            reqSQLineUpdate.PCPL_Interest = salesQuoteDetails.Products[a].PCPL_Interest;
                            reqSQLineUpdate.PCPL_Interest_Rate = salesQuoteDetails.Products[a].PCPL_Interest_Rate;
                            reqSQLineUpdate.PCPL_Total_Cost = salesQuoteDetails.Products[a].PCPL_Total_Cost;
                            reqSQLineUpdate.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            reqSQLineUpdate.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;
                            reqSQLineUpdate.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;

                            if (salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                                reqSQLineUpdate.GST_Place_Of_Supply = "Bill-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1" && salesQuoteDetails.JobtoCode != "-1")
                                reqSQLineUpdate.GST_Place_Of_Supply = "Ship-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1")
                                reqSQLineUpdate.GST_Place_Of_Supply = "Ship-to Address";

                            //

                            int SQLineNo = Convert.ToInt32(salesQuoteDetails.Products[a].Line_No);
                            result1 = PatchItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLine", reqSQLineUpdate, reqSQLiquidLineUpdate, resSQLine, "Document_Type='Quote',Document_No='" + responseSQHeader.No + "',Line_No=" + SQLineNo);
                        }
                        else
                        {
                            //reqSQLiquidLineUpdate.PCPL_Concentration_Rate_Percent = salesQuoteDetails.Products[a].PCPL_Concentration_Rate_Percent;
                            //reqSQLiquidLineUpdate.Net_Weight = salesQuoteDetails.Products[a].Net_Weight;
                            //reqSQLiquidLineUpdate.PCPL_Liquid_Rate = salesQuoteDetails.Products[a].PCPL_Liquid_Rate;
                            ////reqSQLiquidLineUpdate.Quantity = salesQuoteDetails.Products[a].Quantity;
                            ////reqSQLiquidLineUpdate.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            //reqSQLiquidLineUpdate.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLiquidLineUpdate.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            //reqSQLiquidLineUpdate.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;
                            //reqSQLiquidLineUpdate.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;

                            //

                            reqSQLiquidLineUpdate.PCPL_MRP = salesQuoteDetails.Products[a].PCPL_MRP;
                            //reqSQLiquidLineUpdate.PCPL_Basic_Price = salesQuoteDetails.Products[a].PCPL_Basic_Price;
                            reqSQLiquidLineUpdate.Location_Code = LocationCode;
                            reqSQLiquidLineUpdate.PCPL_Concentration_Rate_Percent = salesQuoteDetails.Products[a].PCPL_Concentration_Rate_Percent;
                            reqSQLiquidLineUpdate.Net_Weight = salesQuoteDetails.Products[a].Net_Weight;
                            reqSQLiquidLineUpdate.PCPL_Liquid_Rate = salesQuoteDetails.Products[a].PCPL_Liquid_Rate;
                            reqSQLiquidLineUpdate.PCPL_Liquid = salesQuoteDetails.Products[a].IsLiquidProd;
                            //reqSQLiquidLineUpdate.Quantity = salesQuoteDetails.Products[a].Quantity;
                            //reqSQLiquidLineUpdate.Unit_Price = salesQuoteDetails.Products[a].Unit_Price;
                            reqSQLiquidLineUpdate.PCPL_Packing_Style_Code = salesQuoteDetails.Products[a].PCPL_Packing_Style_Code;
                            //reqSQLiquidLineUpdate.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLiquidLineUpdate.PCPL_Transport_Method = salesQuoteDetails.Products[a].PCPL_Transport_Method;
                            reqSQLiquidLineUpdate.PCPL_Transport_Cost = salesQuoteDetails.Products[a].PCPL_Transport_Cost;
                            reqSQLiquidLineUpdate.PCPL_Commission_Payable = salesQuoteDetails.Products[a].PCPL_Commission_Payable == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Payable;
                            reqSQLiquidLineUpdate.PCPL_Commission_Type = salesQuoteDetails.Products[a].PCPL_Commission_Type == null ? "" : salesQuoteDetails.Products[a].PCPL_Commission_Type;
                            reqSQLiquidLineUpdate.PCPL_Commission = salesQuoteDetails.Products[a].PCPL_Commission;
                            reqSQLiquidLineUpdate.PCPL_Commission_Amount = salesQuoteDetails.Products[a].PCPL_Commission_Amount;
                            //reqSQLiquidLineUpdate.PCPL_Discount_Type = salesQuoteDetails.Products[a].PCPL_Discount_Type;
                            //reqSQLiquidLineUpdate.PCPL_Discount = salesQuoteDetails.Products[a].PCPL_Discount;
                            reqSQLiquidLineUpdate.PCPL_Sales_Discount = salesQuoteDetails.Products[a].PCPL_Sales_Discount;
                            reqSQLiquidLineUpdate.PCPL_Credit_Days = salesQuoteDetails.Products[a].PCPL_Credit_Days;
                            reqSQLiquidLineUpdate.PCPL_Margin = salesQuoteDetails.Products[a].PCPL_Margin;
                            reqSQLiquidLineUpdate.PCPL_Margin_Percent = salesQuoteDetails.Products[a].PCPL_Margin_Percent;
                            reqSQLiquidLineUpdate.PCPL_Interest = salesQuoteDetails.Products[a].PCPL_Interest;
                            reqSQLiquidLineUpdate.PCPL_Interest_Rate = salesQuoteDetails.Products[a].PCPL_Interest_Rate;
                            reqSQLiquidLineUpdate.PCPL_Total_Cost = salesQuoteDetails.Products[a].PCPL_Total_Cost;
                            reqSQLiquidLineUpdate.Delivery_Date = salesQuoteDetails.Products[a].Delivery_Date;
                            reqSQLiquidLineUpdate.Drop_Shipment = salesQuoteDetails.Products[a].Drop_Shipment;
                            reqSQLiquidLineUpdate.PCPL_Vendor_No = salesQuoteDetails.Products[a].PCPL_Vendor_No == null ? "" : salesQuoteDetails.Products[a].PCPL_Vendor_No;

                            if (salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                                reqSQLiquidLineUpdate.GST_Place_Of_Supply = "Bill-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1" && salesQuoteDetails.JobtoCode != "-1")
                                reqSQLiquidLineUpdate.GST_Place_Of_Supply = "Ship-to Address";
                            else if (salesQuoteDetails.ShiptoCode != "-1")
                                reqSQLiquidLineUpdate.GST_Place_Of_Supply = "Ship-to Address";

                            //

                            int SQLineNo = Convert.ToInt32(salesQuoteDetails.Products[a].Line_No);
                            result1 = PatchItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLiquidLine", reqSQLineUpdate, reqSQLiquidLineUpdate, resSQLine, "Document_Type='Quote',Document_No='" + responseSQHeader.No + "',Line_No=" + SQLineNo);
                        }

                    }


                    if (result1.Result.Item1 != null)
                    {
                        resSQLine = result1.Result.Item1;
                        ed1 = result1.Result.Item2;
                        responseSQHeader.errorDetails = ed1;

                        responseSQHeader.ItemLineNo += resSQLine.No + "_" + resSQLine.Line_No + ",";

                        //if (salesQuoteDetails.Products[a].InvQuantities != null && salesQuoteDetails.Products[a].InvQuantities.Count > 0)
                        //{
                        //    SPSQInvQtyReserveOData invQtyReserveOData = new SPSQInvQtyReserveOData();
                        //    List<SPSQInvQtyReserve> invQtyReserve = new List<SPSQInvQtyReserve>();
                        //    errorDetails ed2 = new errorDetails();

                        //    for (int b = 0; b < salesQuoteDetails.Products[a].InvQuantities.Count; b++)
                        //    {
                        //        invQtyReserve.Add(new SPSQInvQtyReserve()
                        //        {
                        //            QuoteNo = responseSQHeader.No,
                        //            LineNo = Convert.ToInt32(resSQLine.Line_No),
                        //            ItemNo = salesQuoteDetails.Products[a].InvQuantities[b].ItemNo,
                        //            LotNo = salesQuoteDetails.Products[a].InvQuantities[b].LotNo,
                        //            Qty = salesQuoteDetails.Products[a].InvQuantities[b].Qty,
                        //            LocationCode = salesQuoteDetails.Products[a].InvQuantities[b].LocationCode

                        //        });

                        //    }

                        //    var result2 = PostItemInvQtyReserve<SPSQInvQtyReserveOData>("", invQtyReserve, invQtyReserveOData);
                        //    invQtyReserveOData = result2.Result.Item1;

                        //    if (result2.Result.Item2.message != null)
                        //        ed2 = result1.Result.Item2;

                        //}

                        //

                        //

                    }

                    //if (result1.Result.Item2.message != null)
                    //    ed1 = result1.Result.Item2;

                }

                if (salesQuoteDetails.InquiryNo != "" && salesQuoteDetails.InquiryNo != null)
                {
                    SPSQUpdateInqStatus updateInqStatus = new SPSQUpdateInqStatus();
                    SPSQUpdateInqStatusOData updateInqStatusOData = new SPSQUpdateInqStatusOData();
                    //SPInqLines inqToQuoteRes = new SPInqLines();
                    errorDetails edUpdateInqStatus = new errorDetails();

                    updateInqStatus.salesquoteno = responseSQHeader.No;

                    //inqToQuoteReq.PCPL_Convert_Quote = true;

                    var result2 = PostItemForUpdateInqStatus<SPSQUpdateInqStatusOData>("", updateInqStatus, updateInqStatusOData);
                    updateInqStatusOData = result2.Result.Item1;
                    edUpdateInqStatus = result2.Result.Item2;
                    responseSQHeader.errorDetails = edUpdateInqStatus;

                    //var resultInqToQuote = PatchItemInqToQuote("InquiryProductsDotNetAPI", inqToQuoteReq, inqToQuoteRes, "Document_Type='Quote',Document_No='" + salesQuoteDetails.InquiryNo + "',Line_No=" + Convert.ToInt32(salesQuoteDetails.Products[a].InqProdLineNo));

                    //if (resultInqToQuote.Result.Item1 != null)
                    //    inqToQuoteRes = resultInqToQuote.Result.Item1;

                    //if (result2.Result.Item2.message != null)
                    //    edUpdateInqStatus = result.Result.Item2;

                }

                string toEmail = "";
                string ccEmail = LoggedInSPUserEmail;

                if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    toEmail = financeUserDetails.Company_E_Mail;

                if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    toEmail = reportingPersonDetails.PCPL_Reporting_Person_Email;

                if (salesQuoteDetails.ApprovalFor == "Both")
                    toEmail = financeUserDetails.Company_E_Mail + "," + reportingPersonDetails.PCPL_Reporting_Person_Email;

                string ApprovalForText = salesQuoteDetails.ApprovalFor == "Both" ? "Negative Credit Limit And Margin" : salesQuoteDetails.ApprovalFor;

                //if(Convert.ToDouble(salesQuoteDetails.AvailableCreditLimit) < 0)
                if (salesQuoteDetails.ApprovalFor != null)
                {

                    //

                    DateTime quoteDate = Convert.ToDateTime(salesQuoteDetails.OrderDate);
                    DateTime quoteValidUntilDate = Convert.ToDateTime(salesQuoteDetails.ValidUntillDate);

                    QuoteValidityDays = (quoteValidUntilDate - quoteDate).Days;

                    string myString = ApprovalFormatFile;
                    string type = "";

                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED ");
                        myString = myString.Replace("##heading##", "The user '" + SPName + "' was trying to create the quote, but the Credit limit is exceeded.");
                        type = "CL";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        myString = myString.Replace("##pageheading##", " MARGIN IS LESS THAN ZERO ");
                        myString = myString.Replace("##heading##", "The user '" + SPName + "' was trying to create the quote, but the Margin is less than zero.");
                        type = "MARGIN";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED & MARGIN LESS THAN ZERO");
                        myString = myString.Replace("##heading##", "The user '" + SPName + "' was trying to create the quote, but the Credit limit is exceeded and Margin is less than Zero.");
                        type = "BOTH";
                    }

                    salesQuoteDetails.SQApprovalFormURL = salesQuoteDetails.SQApprovalFormURL + "?SQNo=" + responseSQHeader.No + "&ScheduleStatus=''&SQStatus=" + StatusForUrl +
                        "&SQFor=ApproveReject&LoggedInUserRole=";

                    //myString = myString.Replace("##SRN##", lblSalesQuoteNo.Text);
                    myString = myString.Replace("##SalesQuoteNo##", responseSQHeader.No);
                    myString = myString.Replace("##SalesQuoteDate##", responseSQHeader.Order_Date);

                    string[] CustSellToAddress = salesQuoteDetails.ConsigneeAddress.Split('_');
                    string CustName = CustSellToAddress[0];
                    string CustSellToAddress_ = CustName + "<br />" + CustSellToAddress[1] + ",<br />" + CustSellToAddress[2] + ",<br />" + CustSellToAddress[3] + "-" + CustSellToAddress[4];

                    myString = myString.Replace("##CustomerDetail##", CustSellToAddress_);
                    myString = myString.Replace("##ContactName##", salesQuoteDetails.ContactPersonName);

                    //myString = myString.Replace("##TaxGroupDetails##", ds.Tables[0].Rows[0]["TaxGroupDetails"].ToString());
                    myString = myString.Replace("##FooterDetails##", "");

                    // myString = myString.Replace("##msg##", message);
                    //myString = myString.Replace("##page##", ConfigurationManager.AppSettings["approval"].ToString());
                    myString = myString.Replace("##heading##", "The user '" + SPName + "' was trying to create the quote, but the Credit limit is exceeded or Margin is less than zero.");

                    if (salesQuoteDetails.JobtoCode != "-1")
                    {
                        myString = myString.Replace("##ShippingHeader##", "ShippingDetails");

                        string[] CustJobtoAddress = salesQuoteDetails.JobtoAddress.Split(',');
                        string CustJobtoAddress_ = CustName + "<br />" + CustJobtoAddress[0] + ",<br />" + CustJobtoAddress[1] + ",<br />" + CustJobtoAddress[2];

                        myString = myString.Replace("##ShippingDetail##", CustJobtoAddress_);
                    }
                    else
                    {
                        myString = myString.Replace("##ShippingHeader##", "");
                        myString = myString.Replace("##ShippingDetail##", "");
                    }

                    string str_lineTable = "";
                    /*string SalesLineQuery = @"select ProductName,	PackagingStyle,(CAST(ISNULL(RequiredQty,0) AS DECIMAL(18,3))) AS RequiredQty,	ISNULL((SELECT TOP 1 ISNULL([Description],'') FROM [PCAPL200715].[dbo].[Prakash Chemicals$Shipment Method] WHERE Code COLLATE SQL_Latin1_General_CP1_CI_AS = IncoTerms),'') AS IncoTerms,SUBSTRING(PaymentTerms,0,CHARINDEX('~',PaymentTerms)) as PaymentTerms,			 CAST((CAST(ISNULL(SalesPrice,0) AS DECIMAL(18,4)))/NULLIF(CONVERT(NVARCHAR(50),(SELECT (CAST(ISNULL(RequiredQty,0) AS DECIMAL(18,4))))),'') AS DECIMAL(18,2)) 
                     AS SalesPrice from SalesQuoteDetail SQD     WHERE SQD.SalesQuoteNo = '" + ds.Tables[0].Rows[0]["SalesQuoteNo"].ToString() + "' order By          ProductName";
                    Getconnection c12 = new Getconnection();
                    DataTable dt = Dataacess.GetDataTable(SalesLineQuery, CommandType.Text, null);


                    if (dt.Rows.Count > 0)
                    {*/
                        myString = myString.Replace("##PaymentTermsHeader##", "Payment Terms : ");
                        myString = myString.Replace("##PaymentTerms##", salesQuoteDetails.PaymentTermsCode);
                        // lbl_paymentTerm.Text = "Payment Terms : " + Convert.ToString(dt_salesLine.Rows[0]["PaymentTerms"]);


                        /*string q_deliveryDate = @"select DeliveryDate from SalesQuoteMaster where SalesQuoteNo = '" + ds.Tables[0].Rows[0]["SalesQuoteNo"].ToString() + "'";

                        Getconnection c122 = new Getconnection();
                        SqlDataAdapter sda_deliveryDate = new SqlDataAdapter(q_deliveryDate, c122.getconnection());
                        DataTable dt_deliveryDate = new DataTable();
                        sda_deliveryDate.Fill(dt_deliveryDate);*/

                        /*if (dt_deliveryDate.Rows.Count > 0)
                        {*/
                            //myString = myString.Replace("##ScheduleHeader##", "Schedule : ");
                            //myString = myString.Replace("##Schedule##", Convert.ToString(dt_deliveryDate.Rows[0][0]));
                            //lbl_Schedule.Text = "Schedule : " + Convert.ToString(dt_deliveryDate.Rows[0][0]);
                        /*}*/

                        str_lineTable = "<table cellpadding=\"0\" cellspacing=\"1\" border=\"1\" width=\"100%\" align=\"left\" style=\"border:1px solid #BEBEBE\">";
                        str_lineTable += "<tr style=\"background:#ccc;font-weight:bold !important;text-transform:capitalize;font-size:12\">";
                        str_lineTable += "<td width =\"5%\" align=\"left\">SR.NO.</td>";
                        str_lineTable += "<td width =\"10%\" align=\"left\">Product</td>";
                        str_lineTable += "<td width =\"10%\" align=\"left\">Packaging Style</td>";
                        str_lineTable += "<td width =\"5%\" align=\"right\">Qty(MT)</td>";
                        str_lineTable += "<td width =\"5%\" align=\"right\">Sales Price(Rs.)</td>";
                        str_lineTable += "<td width =\"5%\" align=\"right\">IncoTerms</td>";


                        //str_lineTable += "<td width=\"5%\" align=\"center\">Total Amt per unit</td>";
                        str_lineTable += "</tr>";
                        int counter = 1;

                        for (int a = 0; a < salesQuoteDetails.Products.Count; a++)
                        {
                            str_lineTable += "<tr style=\"font-size:10\">";

                            str_lineTable += "<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + counter + "</td>";
                            str_lineTable += "<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + salesQuoteDetails.Products[a].ProductName + " </td>";
                            str_lineTable += "<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + salesQuoteDetails.Products[a].PCPL_Packing_Style_Code + " </td>";
                            str_lineTable += "<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + salesQuoteDetails.Products[a].Quantity + " </td>";
                            str_lineTable += "<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + salesQuoteDetails.Products[a].Unit_Price + " </td>";
                            str_lineTable += "<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + salesQuoteDetails.ShipmentMethodCode + " </td>";

                            //str_lineTable += "<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">" + (Convert.ToDecimal(Convert.ToString(dr["SalesPrice"])) + GSTAmount) + " </td>";
                            str_lineTable += "</tr>";

                            counter++;
                        }
                    /*}*/

                    /*Getconnection c = new Getconnection();
                    string getcustno = "select No_ from [dbo].[Prakash Chemicals$Customer] where Name='" + txtCustomerName.Text.Trim() + "'";
                    // DataTable dt = Dataacess.GetDataTable(consillor, CommandType.Text, null);
                    SqlCommand daconsillor = new SqlCommand(getcustno, c.getNavconnection());
                    string custno = Convert.ToString(daconsillor.ExecuteScalar());*/

                    myString = myString.Replace("##PaymentTermsHeader##", "");
                    myString = myString.Replace("##PaymentTerms##", "");
                    myString = myString.Replace("##ScheduleHeader##", "");
                    myString = myString.Replace("##Schedule##", "");
                    myString = myString.Replace("##totalcreditlimit##", salesQuoteDetails.TotalCreditLimit);
                    myString = myString.Replace("##usedcreditlimit##", salesQuoteDetails.UsedCreditLimit);
                    myString = myString.Replace("##availablecreditlimit##", salesQuoteDetails.AvailableCreditLimit);
                    myString = myString.Replace("##custno##", salesQuoteDetails.CustomerNo);
                    myString = myString.Replace("##custname##", salesQuoteDetails.ContactCompanyName);

                    if (QuoteValidityDays <= 1)
                        myString = myString.Replace("##QuoteValidityDays##", QuoteValidityDays + " Day");
                    else if (QuoteValidityDays >= 1)
                        myString = myString.Replace("##QuoteValidityDays##", QuoteValidityDays + " Days");

                    //GSTTable += "</table>    </ td >    </ tr > ";

                    str_lineTable += "</table>";
                
                    myString = myString.Replace("##GSTAPPLICABLE##", "GST: As Applicable");

                    myString = myString.Replace("##LineDetails##", str_lineTable); /*.Replace("<td width=\"5%\" align=\"center\">GST</td>", ""));*/

                    myString = myString.Replace("##SalesQuoteApprovalFormURL##", salesQuoteDetails.SQApprovalFormURL);

                    string emailSubject = "Sales Quote Approval - " + responseSQHeader.No + " - " + CustomerName + " - " + DateTime.Now.ToString("dd/MM/yyyy") + " - " + ApprovalForText;

                    EmailService emailService = new EmailService();
                    StringBuilder sbMailBody = new StringBuilder();
                    sbMailBody.Append("");
                    /*sbMailBody.Append("<p>Hi,</p>");
                    sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                    sbMailBody.Append("<p>Approval Request For Sales Quote - " + responseSQHeader.No + "</p>");
                    sbMailBody.Append("<p>Justification For - " + salesQuoteDetails.JustificationFor + "</p>");
                    sbMailBody.Append("<p>Justification - " + salesQuoteDetails.JustificationDetails + "</p>");*/
                    sbMailBody.Append(myString);
                    /*sbMailBody.Append("<p>&nbsp;</p>");
                    sbMailBody.Append("<p>Warm Regards,</p>");
                    sbMailBody.Append("<p>Support Team</p>");*/



                    //emailService.SendEmail(toEmail, ccEmail, "Approval Request For Sales Quote " + responseSQHeader.No + " - PrakashCRM", sbMailBody.ToString());
                    //emailService.SendEmail(toEmail, ccEmail, emailSubject, sbMailBody.ToString());
                    //emailService.SendEmail(toEmail, ccEmail, emailSubject, myString);
                    emailService.SendEmailWithHTMLBody(toEmail, ccEmail, "", emailSubject, myString);

                    //
                }

            }

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return responseSQHeader;

        }

        [Route("GetSQListDataForApproveReject")]
        public List<SPSQListForApproveReject> GetSQListDataForApproveReject(string LoggedInUserNo, string UserRoleORReportingPerson, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSQListForApproveReject> sqList = new List<SPSQListForApproveReject>();

            if (filter == "" || filter == null)
            {
                if (UserRoleORReportingPerson == "Finance")
                    filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                else if (UserRoleORReportingPerson == "ReportingPerson")
                    filter = "PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
            }
            else
            {
                if (UserRoleORReportingPerson == "Finance")
                    filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                else if (UserRoleORReportingPerson == "ReportingPerson")
                    filter = filter + " and PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
            }

            //if (filter == "" || filter == null)
            //    filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false"; // and PCPL_Status ne '0'
            //else
            //    filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false"; // and PCPL_Status ne '0'

            var result = ac.GetData1<SPSQListForApproveReject>("SalesQuoteDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                sqList = result.Result.Item1.value;

            for (int i = 0; i < sqList.Count; i++)
            {
                string[] strDate = sqList[i].Order_Date.Split('-');
                sqList[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                //string[] strDate1 = salesquotes[i].Requested_Delivery_Date.Split('-');
                //salesquotes[i].Requested_Delivery_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return sqList;
        }

        [Route("SQApproveReject")]
        public string SQApproveReject(string SQNosAndApprovalFor, string LoggedInUserNo, string Action, string UserRoleORReportingPerson, string RejectRemarks, string LoggedInUserEmail)
        {
            string resMsg = "";
            API ac = new API();

            //SPBusinessPlanCustWiseForApprove businessPlanCustWiseForApprove = new SPBusinessPlanCustWiseForApprove();
            //SPBusinessPlanCustWiseForReject businessPlanCustWiseForReject = new SPBusinessPlanCustWiseForReject();
            //SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;
            SQNosAndApprovalFor = SQNosAndApprovalFor.Substring(0, SQNosAndApprovalFor.Length - 1);
            string[] SQNosAndApprovalForDetails_ = SQNosAndApprovalFor.Split(',');
            string SQNos = "";

            for(int a = 0; a < SQNosAndApprovalForDetails_.Length; a++)
            {
                SPSQForApprove sqForApprove = new SPSQForApprove();
                SPSQForReject sqForReject = new SPSQForReject();
                SPSQForApproveHOD sqForApproveHOD = new SPSQForApproveHOD();
                SPSQForRejectHOD sqForRejectHOD = new SPSQForRejectHOD();
                SPSQHeader sqHeader = new SPSQHeader();
                string[] SQNosAndApprovalFor_ = SQNosAndApprovalForDetails_[a].Split(':');
                string SQNo = SQNosAndApprovalFor_[0];
                string ApprovalFor = SQNosAndApprovalFor_[1];
                string SPEmail = SQNosAndApprovalFor_[2];

                if (UserRoleORReportingPerson == "Finance")
                {
                    if (Action == "Approve" && ApprovalFor == "Credit Limit")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApprove.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForApprove.PCPL_Status = "Approved";
                        sqForApprove.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Approve" && ApprovalFor == "Both")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApprove.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForApprove.PCPL_Status = "Approval pending from HOD";
                        sqForApprove.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Reject")
                    {
                        //sqForReject.PCPL_Approver = LoggedInUserNo;
                        sqForReject.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForReject.PCPL_Status = "Rejected by finance";
                        sqForReject.PCPL_Rejected_Reason = RejectRemarks;
                        sqForReject.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Reject", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                }
                else if (UserRoleORReportingPerson == "ReportingPerson")
                {
                    if (Action == "Approve")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApproveHOD.PCPL_ApprovedBy_RejectedBy_HOD = LoggedInUserNo;
                        sqForApproveHOD.PCPL_Status = "Approved";
                        sqForApproveHOD.PCPL_Approved_Rejected_On_HOD = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveRejectHOD("SalesQuoteDotNetAPI", sqForApproveHOD, sqForRejectHOD, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Reject")
                    {
                        //sqForReject.PCPL_Approver = LoggedInUserNo;
                        sqForRejectHOD.PCPL_ApprovedBy_RejectedBy_HOD = LoggedInUserNo;
                        sqForRejectHOD.PCPL_Status = "Rejected by HOD";
                        sqForRejectHOD.PCPL_Rejected_Reason_HOD = RejectRemarks;
                        sqForRejectHOD.PCPL_Approved_Rejected_On_HOD = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveRejectHOD("SalesQuoteDotNetAPI", sqForApproveHOD, sqForRejectHOD, sqHeader, "Reject", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                }

                if (result.Result.Item1.No != null)
                {
                    resMsg = "True";
                    sqHeader = result.Result.Item1;
                    ed = result.Result.Item2;
                    sqHeader.errorDetails = ed;
                    SQNos = SQNo + ",";
                }

                if (!sqHeader.errorDetails.isSuccess)
                    resMsg = "Error:" + sqHeader.errorDetails.message;

                if(SPEmail != null || SPEmail != "")
                {
                    EmailService emailService = new EmailService();
                    StringBuilder sbMailBody = new StringBuilder();

                    sbMailBody.Append("");
                    sbMailBody.Append("<p>Hi,</p>");
                    sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");

                    if (Action == "Approve")
                    {
                        if (UserRoleORReportingPerson == "Finance")
                            sbMailBody.Append("<p>Sales Quote : " + SQNo + " Approved By Finance User</p>");
                        else if (UserRoleORReportingPerson == "ReportingPerson")
                            sbMailBody.Append("<p>Sales Quote : " + SQNo + " Approved By HOD User</p>");
                    }
                    else if (Action == "Reject")
                    {
                        if (UserRoleORReportingPerson == "Finance")
                            sbMailBody.Append("<p>Sales Quote : " + SQNo + " Rejected By Finance User</p>");
                        else if (UserRoleORReportingPerson == "ReportingPerson")
                            sbMailBody.Append("<p>Sales Quote : " + SQNo + " Rejected By HOD User</p>");
                    }

                    sbMailBody.Append("<p>&nbsp;</p>");
                    sbMailBody.Append("<p>Warm Regards,</p>");
                    sbMailBody.Append("<p>Support Team</p>");

                    emailService.SendEmail(SPEmail, LoggedInUserEmail, "Sales Quote Approval Status", sbMailBody.ToString());
                }
                
            }

            //SQNos = SQNos.Substring(0, SQNos.Length - 1);

            return resMsg;

        }

        [HttpPost]
        [Route("AddUpdateOnSaveProd")]
        public bool AddUpdateOnSaveProd(SPSQLinesPost salesQuoteLine)
        {
            SPSQLines responseSQLines = new SPSQLines();
            errorDetails ed = new errorDetails();

            //salesQuoteLine.Document_Type = "Quote";
            salesQuoteLine.Type = "Item";
            //salesQuoteLine.Location_Code = "AHM DOM";
            //salesQuoteLine.Location_Code = "";
            //salesQuoteLine.Line_Amount = salesQuoteLine.Unit_Price * salesQuoteLine.Quantity;
            //salesQuoteLine.GST_Place_Of_Supply = "Bill-to Address";

            //var result = PostItemSQLines("SalesQuoteSubFormDotNetAPI", salesQuoteLine, responseSQLines);

            //if (result.Result.Item1 != null)
            //    responseSQLines = result.Result.Item1;

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return true;
        }

        [HttpPost]
        [Route("AddNewIncoTerm")]
        public bool AddNewIncoTerm(string IncoTermCode, string IncoTerm)
        {
            API ac = new API();
            SPSQIncoTerm incoTerm = new SPSQIncoTerm();
            SPSQIncoTerm incoTermRes = new SPSQIncoTerm();
            errorDetails ed = new errorDetails();

            incoTerm.Code = IncoTermCode;
            incoTerm.Description = IncoTerm;

            var result = ac.PostItem("ShipmentMethodsDotNetAPI", incoTerm, incoTermRes);

            if (result.Result.Item1 != null)
                incoTermRes = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return true;
        }

        [Route("GetGeneratedSQNo")]
        public string GetGeneratedSQNo(string NoSeriesCode)
        {
            SPSQGetNextNo requestSQGetNextNo = new SPSQGetNextNo();
            SPSQGetNextNoRes responseSQGetNextNoRes = new SPSQGetNextNoRes();
            errorDetails edForGetNextNo = new errorDetails();
            requestSQGetNextNo.noseriescode = NoSeriesCode;

            string todayDate = DateTime.Now.ToShortDateString();
            string[] todayDate_ = todayDate.Split('-');
            requestSQGetNextNo.usagedate = todayDate_[2] + "-" + todayDate_[1] + "-" + todayDate_[0];

            var resultGetNextNo = PostItemForSQGetNextNo("", requestSQGetNextNo, responseSQGetNextNoRes);
            string generatedSQNo = "";

            if (resultGetNextNo.Result.Item1 != null)
                generatedSQNo = resultGetNextNo.Result.Item1.value;

            return generatedSQNo;
        }

        [Route("GetAllCompanyForDDL")]
        public List<SPCompanyList> GetAllCompanyForDDL(string SPCode)
        {
            API ac = new API();
            List<SPCompanyList> companies = new List<SPCompanyList>();

            var result = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                companies = result.Result.Item1.value;

            List<SPCompanyList> company2 = new List<SPCompanyList>();

            var result2 = ac.GetData<SPCompanyList>("ContactDotNetAPI", "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'");

            if (result2 != null && result2.Result.Item1.value.Count > 0)
            {
                company2 = result2.Result.Item1.value;

                companies.AddRange(company2);
            }

            return companies;
        }

        [Route("GetInquiriesForDDL")]
        public List<SPSQInquiryNos> GetInquiriesForDDL(string SPCode)
        {
            API ac = new API();
            List<SPSQInquiryNos> inquirynos = new List<SPSQInquiryNos>();

            var result = ac.GetData<SPSQInquiryNos>("InquiryDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true");

            if (result.Result.Item1.value.Count > 0)
                inquirynos = result.Result.Item1.value;

            return inquirynos;
        }

        [Route("GetInquiryDetails")]
        public SPInquiryList GetInquiryDetails(string InqNo)
        {
            API ac = new API();
            SPInquiryList inquirydetails = new SPInquiryList();

            var result = ac.GetData<SPInquiryList>("InquiryDotNetAPI", "Inquiry_No eq '" + InqNo + "'");

            if (result.Result.Item1.value.Count > 0)
                inquirydetails = result.Result.Item1.value[0];

            return inquirydetails;
        }

        [Route("GetInquiryProdDetails")]
        public List<SPInquiryProducts> GetInquiryProdDetails(string InqNo)
        {
            API ac = new API();
            List<SPInquiryProducts> inquiryproducts = new List<SPInquiryProducts>();

            var result = ac.GetData<SPInquiryProducts>("InquiryProductsDotNetAPI", "Document_No eq '" + InqNo + "' and PCPL_Convert_Quote eq false");

            if (result.Result.Item1.value.Count > 0)
                inquiryproducts = result.Result.Item1.value;

            return inquiryproducts;
        }

        [Route("GetAllContactsOfCompany")]
        public List<SPSQContacts> GetAllContactsOfCompany(string companyName)
        {
            API ac = new API();
            List<SPSQContacts> contacts = new List<SPSQContacts>();

            var result = ac.GetData<SPSQContacts>("ContactDotNetAPI", "Type eq 'Person' and Company_Name eq '" + companyName + "'");

            if (result.Result.Item1.value.Count > 0)
                contacts = result.Result.Item1.value;

            return contacts;
        }

        [Route("GetAllSQLinesOfSQ")]
        public List<SPSQLines> GetAllSQLinesOfSQ(string QuoteNo, string SQLinesFor)
        {
            API ac = new API();
            List<SPSQLines> SQLines = new List<SPSQLines>();
            var result = (dynamic)null;

            if(SQLinesFor == "SalesQuote")
                result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + QuoteNo + "'");
            else if(SQLinesFor == "ScheduleOrder")
                result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + QuoteNo + "' and TPTPL_Short_Closed eq false");

            if (result.Result.Item1.value.Count > 0)
                SQLines = result.Result.Item1.value;

            return SQLines;
        }

        [Route("GetAllUsers")]
        public List<SPSQUser> GetAllUsers()
        {
            API ac = new API();
            List<SPSQUser> users = new List<SPSQUser>();

            var result = ac.GetData<SPSQUser>("EmployeesDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
            {
                users = result.Result.Item1.value;
                users = users.OrderBy(a => a.First_Name).ToList();
            }
            
            return users;
        }

        [HttpPost]
        [Route("UpdateScheduleQty")]
        public bool UpdateScheduleQty(string QuoteNo, string ProdLineNo, double ScheduleQty)
        {
            bool flag = false;
            SPSQScheduleQtyPost scheduleQtyReq = new SPSQScheduleQtyPost();
            SPSQScheduleQty scheduleQtyRes = new SPSQScheduleQty();
            errorDetails ed = new errorDetails();

            scheduleQtyReq.TPTPL_Qty_to_Order = ScheduleQty;

            var result = PatchItemScheduleQty("SalesQuoteSubFormDotNetAPI", scheduleQtyReq, scheduleQtyRes, "Document_Type='Quote',Document_No='" + QuoteNo + "',Line_No=" + ProdLineNo);

            if (result.Result.Item1 != null)
            {
                flag = true;
                scheduleQtyRes = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        //public string ScheduleOrder(string QuoteNo, string ScheduleDate, string ExternalDocNo)

        [HttpPost]
        [Route("ScheduleOrder")]
        public string ScheduleOrder(SPSQScheduleOrderDetails scheduleOrderDetails)
        {
            string response = "", Err = "";
            SPSQScheduleOrderPost scheduleOrderReq = new SPSQScheduleOrderPost();
            SPSQScheduleOrderOData scheduleOrderRes = new SPSQScheduleOrderOData();
            errorDetails ed = new errorDetails();

            if (scheduleOrderDetails.SchQtyProds != null && scheduleOrderDetails.SchQtyProds.Count > 0)
            {
                for (int a = 0; a < scheduleOrderDetails.SchQtyProds.Count; a++)
                {
                    bool flag = false;
                    SPSQScheduleQtyPost scheduleQtyReq = new SPSQScheduleQtyPost();
                    SPSQScheduleQty scheduleQtyRes = new SPSQScheduleQty();
                    errorDetails edSchQty = new errorDetails();

                    scheduleQtyReq.TPTPL_Qty_to_Order = scheduleOrderDetails.SchQtyProds[a].ScheduleQty;

                    var resultSchQty = PatchItemScheduleQty("SalesQuoteSubFormDotNetAPI", scheduleQtyReq, scheduleQtyRes, "Document_Type='Quote',Document_No='" +
                            scheduleOrderDetails.SchQtyProds[a].QuoteNo + "',Line_No=" + scheduleOrderDetails.SchQtyProds[a].ProdLineNo);

                    if (resultSchQty.Result.Item1 != null)
                    {
                        flag = true;
                        scheduleQtyRes = resultSchQty.Result.Item1;
                        edSchQty = resultSchQty.Result.Item2;
                        scheduleOrderRes.errorDetails = edSchQty;
                    }
                    else
                        Err = "Error";

                    //if (resultSchQty.Result.Item2.message != null)
                    //    edSchQty = resultSchQty.Result.Item2;
                }
            }

            if (!scheduleOrderRes.errorDetails.isSuccess)
            {
                response = "Error_:" + scheduleOrderRes.errorDetails.message;
                return response;
            }

            if(Err == "") 
            { 
                scheduleOrderReq.quoteNo = scheduleOrderDetails.QuoteNo;
                scheduleOrderReq.scheduledate = scheduleOrderDetails.ScheduleDate;
                scheduleOrderReq.externaldocumentno = scheduleOrderDetails.ExternalDocNo;
                scheduleOrderReq.assignto = scheduleOrderDetails.AssignTo == null || scheduleOrderDetails.AssignTo == "" ? "" : scheduleOrderDetails.AssignTo;

                var result = PostItemForScheduleOrder<SPSQScheduleOrderOData>("", scheduleOrderReq, scheduleOrderRes);

                response = result.Result.Item1.value;
                ed = result.Result.Item2;
                scheduleOrderRes.errorDetails = ed;

                if (!scheduleOrderRes.errorDetails.isSuccess)
                {
                    response = "Error_:" + scheduleOrderRes.errorDetails.message;
                    return response;
                }

                //if (result.Result.Item2.message != null)
                //    ed = result.Result.Item2;

                if (response != "" && (!response.Contains("Error_:")))
                {
                    if (scheduleOrderDetails.InvQuantities != null && scheduleOrderDetails.InvQuantities.Count > 0)
                    {
                        SPSQInvQtyReserveOData invQtyReserveOData = new SPSQInvQtyReserveOData();
                        List<SPSQInvQtyReserve> invQtyReserve = new List<SPSQInvQtyReserve>();
                        errorDetails ed2 = new errorDetails();
                        string orderNo = response;

                        for (int b = 0; b < scheduleOrderDetails.InvQuantities.Count; b++)
                        {
                            invQtyReserve.Add(new SPSQInvQtyReserve()
                            {
                                OrderNo = orderNo,
                                LineNo = scheduleOrderDetails.InvQuantities[b].LineNo,
                                ItemNo = scheduleOrderDetails.InvQuantities[b].ItemNo,
                                LotNo = scheduleOrderDetails.InvQuantities[b].LotNo,
                                Qty = scheduleOrderDetails.InvQuantities[b].Qty,
                                LocationCode = scheduleOrderDetails.InvQuantities[b].LocationCode

                            });

                        }

                        var result2 = PostItemInvQtyReserve<SPSQInvQtyReserveOData>("", invQtyReserve, invQtyReserveOData);
                        invQtyReserveOData = result2.Result.Item1;
                        ed2 = result2.Result.Item2;
                        scheduleOrderRes.errorDetails = ed2;

                        //if (result2.Result.Item2.message != null)
                        //    ed2 = result2.Result.Item2;

                        if (!scheduleOrderRes.errorDetails.isSuccess)
                            response = "Error_:" + scheduleOrderRes.errorDetails.message;

                    }
                }
            }

            return response;
        }

        [Route("GetCreditLimitAndCustDetails")]
        public SPSQCreditLimitAndCustDetails GetCreditLimitAndCustDetails(string companyName)
        {
            API ac = new API();
            SPContCustForBusRel contCustForBusRel = new SPContCustForBusRel();
            SPConBusinessRelation conBusinessRelation = new SPConBusinessRelation();
            SPSQCreditLimitAndCustDetails creditlimitcustdetails = new SPSQCreditLimitAndCustDetails();

            var resultCompanyNo = ac.GetData<ContCustForBusRel>("ContactDotNetAPI", "Type eq 'Company' and Name eq '" + companyName + "'");

            if (resultCompanyNo.Result.Item1.value.Count > 0)
                contCustForBusRel.Company_No = resultCompanyNo.Result.Item1.value[0].Company_No;

            var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + contCustForBusRel.Company_No + "'");

            if (resultCustomerNo.Result.Item1.value.Count > 0)
            {
                conBusinessRelation.No = resultCustomerNo.Result.Item1.value[0].No;

                var resultCustomer = ac.GetData<SPCustomer>("CustomerCardDotNetAPI", "No eq '" + conBusinessRelation.No + "'");

                if (resultCustomer.Result.Item1.value.Count > 0)
                {
                    creditlimitcustdetails.CreditLimit = resultCustomer.Result.Item1.value[0].Credit_Limit_LCY.ToString("#,##0.00");
                    double AccBal = Math.Abs(resultCustomer.Result.Item1.value[0].Balance_LCY);
                    double createdQuoteAmt = Math.Abs(resultCustomer.Result.Item1.value[0].PCPL_Credit_Limit_LCY);
                    creditlimitcustdetails.AccountBalance = AccBal.ToString("#,##0.00");
                    creditlimitcustdetails.UsedCreditLimit = (AccBal + createdQuoteAmt).ToString("#,##0.00");
                    creditlimitcustdetails.AvailableCredit = Convert.ToDouble(resultCustomer.Result.Item1.value[0].Credit_Limit_LCY - (AccBal + createdQuoteAmt)).ToString("#,##0.00");
                    creditlimitcustdetails.OutstandingDue = resultCustomer.Result.Item1.value[0].Balance_Due_LCY.ToString("#,##0.00");
                    creditlimitcustdetails.CustNo = resultCustomer.Result.Item1.value[0].No;
                    creditlimitcustdetails.CustName = resultCustomer.Result.Item1.value[0].Name;
                    creditlimitcustdetails.Address = resultCustomer.Result.Item1.value[0].Address;
                    creditlimitcustdetails.Address_2 = resultCustomer.Result.Item1.value[0].Address_2;
                    creditlimitcustdetails.City = resultCustomer.Result.Item1.value[0].City;
                    creditlimitcustdetails.Post_Code = resultCustomer.Result.Item1.value[0].Post_Code;
                    creditlimitcustdetails.PANNo = resultCustomer.Result.Item1.value[0].P_A_N_No;
                    creditlimitcustdetails.PcplClass = resultCustomer.Result.Item1.value[0].PCPL_Class;

                    SPSQShiptoAddress requestShiptoAddress = new SPSQShiptoAddress();
                    List<SPSQShiptoAddressRes> responseShiptoAddress = new List<SPSQShiptoAddressRes>();
                    errorDetails edForShiptoAddress = new errorDetails();
                    requestShiptoAddress.customerno = conBusinessRelation.No;

                    var resultGetShipToAddress = PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>("", requestShiptoAddress, responseShiptoAddress);
                    List<SPSQShiptoAddressRes> shiptoaddresses = new List<SPSQShiptoAddressRes>();

                    if (resultGetShipToAddress.Result.Item1.Count > 0)
                    {
                        responseShiptoAddress = resultGetShipToAddress.Result.Item1;
                        creditlimitcustdetails.ShiptoAddress = responseShiptoAddress.OrderBy(a => a.Address).ToList();
                    }

                    SPSQJobtoAddress requestJobtoAddress = new SPSQJobtoAddress();
                    List<SPSQJobtoAddressRes> responseJobtoAddress = new List<SPSQJobtoAddressRes>();
                    errorDetails edForJobtoAddress = new errorDetails();
                    requestJobtoAddress.customerno = conBusinessRelation.No;

                    var resultGetJobToAddress = PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>("", requestJobtoAddress, responseJobtoAddress);
                    List<SPSQJobtoAddressRes> jobtoaddresses = new List<SPSQJobtoAddressRes>();

                    if (resultGetJobToAddress.Result.Item1.Count > 0)
                    {
                        responseJobtoAddress = resultGetJobToAddress.Result.Item1;
                        creditlimitcustdetails.JobtoAddress = responseJobtoAddress.OrderBy(a => a.Address).ToList();
                    }

                }

            }
            else
            {
                creditlimitcustdetails.CreditLimit = "0.00";
                creditlimitcustdetails.AvailableCredit = "0.00";
                creditlimitcustdetails.OutstandingDue = "0.00";

                SPCompanyList companyList = new SPCompanyList();
                var resultCompanyDetails = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and No eq '" + contCustForBusRel.Company_No + "'");

                if (resultCompanyDetails.Result.Item1.value.Count > 0)
                {
                    companyList = resultCompanyDetails.Result.Item1.value[0];
                    creditlimitcustdetails.CompanyNo = companyList.No;
                    creditlimitcustdetails.CompanyName = companyList.Name;
                    creditlimitcustdetails.Address = companyList.Address;
                    creditlimitcustdetails.Address_2 = companyList.Address_2;
                    creditlimitcustdetails.City = companyList.City;
                    creditlimitcustdetails.Post_Code = companyList.Post_Code;
                }
            }


            return creditlimitcustdetails;
        }

        //[Route("GetAllProducts")]
        //public List<SPItemList> GetAllProducts()
        //{
        //    API ac = new API();
        //    List<SPItemList> items = new List<SPItemList>();
        //    //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
        //    //int skip = 0, top = 10;

        //    var result = ac.GetData<SPItemList>("ItemDotNetAPI", "No eq 'TRD0001' or No eq 'TRD0002' or No eq 'TRD0032' or No eq 'TRD0016' or No eq 'TRD0036' or No eq 'TRD0037' or No eq 'ITM001'");

        //    //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

        //    if (result != null && result.Result.Item1.value.Count > 0)
        //        items = result.Result.Item1.value;

        //    return items;
        //}

        [Route("GetAllProducts")]
        public List<SPContactProducts> GetAllProducts(string CCompanyNo)
        {
            API ac = new API();
            List<SPContactProducts> contactProducts = new List<SPContactProducts>();
            //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
            //int skip = 0, top = 10;

            var result = ac.GetData<SPContactProducts>("ContactProductsDotNetAPI", "Contact_No eq '" + CCompanyNo + "'");

            //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

            if (result != null && result.Result.Item1.value.Count > 0)
                contactProducts = result.Result.Item1.value;

            return contactProducts;
        }

        [Route("GetAllProductsForShowAllProd")]
        public List<SPItemList> GetAllProductsForShowAllProd()
        {
            API ac = new API();
            List<SPItemList> prods = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                prods = result.Result.Item1.value;

            return prods;

        }

        //[Route("GetNoSeriesForDDL")]
        //public List<SPNoSeries> GetNoSeriesForDDL()
        //{
        //    API ac = new API();
        //    List<SPNoSeries> locations = new List<SPNoSeries>();

        //    var result = ac.GetData<SPNoSeries>("NoSeriesRelDotNetAPI", "Code eq 'SQ'"); // and Contact_Business_Relation eq 'Customer'

        //    if (result.Result.Item1.value.Count > 0)
        //        locations = result.Result.Item1.value;

        //    return locations;
        //}

        [Route("GetLocationsForDDL")]
        public List<SPLocations> GetLocationsForDDL()
        {
            API ac = new API();
            List<SPLocations> locations = new List<SPLocations>();

            var result = ac.GetData<SPLocations>("LocationsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                locations = result.Result.Item1.value;

            return locations;
        }

        [Route("GetPaymentTermsForDDL")]
        public List<SPSQPaymentTerms> GetPaymentTermsForDDL()
        {
            API ac = new API();
            List<SPSQPaymentTerms> paymentterms = new List<SPSQPaymentTerms>();

            var result = ac.GetData<SPSQPaymentTerms>("PaymentTermsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                paymentterms = result.Result.Item1.value;

            return paymentterms;
        }

        [Route("GetItemVendorsForDDL")]
        public List<SPSQItemVendors> GetItemVendorsForDDL(string ProdNo)
        {
            API ac = new API();
            List<SPSQItemVendors> itemVendors = new List<SPSQItemVendors>();

            var result = ac.GetData<SPSQItemVendors>("ItemVendorCatalogDotNetAPI", "Item_No eq '" + ProdNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                itemVendors = result.Result.Item1.value;

            return itemVendors;
        }

        [Route("GetIncoTermsForDDL")]
        public List<SPSQShipmentMethods> GetIncoTermsForDDL()
        {
            API ac = new API();
            List<SPSQShipmentMethods> shipmentmethods = new List<SPSQShipmentMethods>();

            var result = ac.GetData<SPSQShipmentMethods>("ShipmentMethodsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                shipmentmethods = result.Result.Item1.value;

            return shipmentmethods;
        }

        [Route("GetVendorsForDDL")]
        public List<SPSQVendors> GetVendorsForDDL()
        {
            API ac = new API();
            List<SPSQVendors> vendors = new List<SPSQVendors>();

            var result = ac.GetData<SPSQVendors>("VendorDotNetAPI", "PCPL_Broker eq true");   //PCPL_Broker eq true and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                vendors = result.Result.Item1.value;

            return vendors;
        }

        [Route("GetPaymentMethodsForDDL")]
        public List<SPSQPaymentMethods> GetPaymentMethodsForDDL()
        {
            API ac = new API();
            List<SPSQPaymentMethods> paymentmethods = new List<SPSQPaymentMethods>();

            var result = ac.GetData<SPSQPaymentMethods>("PaymentMethodsDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                paymentmethods = result.Result.Item1.value;

            return paymentmethods;
        }

        [Route("GetTransportMethodsForDDL")]
        public List<SPSQTransportMethods> GetTransportMethodsForDDL()
        {
            API ac = new API();
            List<SPSQTransportMethods> transportMethods = new List<SPSQTransportMethods>();

            var result = ac.GetData<SPSQTransportMethods>("TransportMethodsDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                transportMethods = result.Result.Item1.value;

            return transportMethods;
        }

        [Route("GetProductDetails")]
        public SPSQProductDetails GetProductDetails(string productName)
        {
            API ac = new API();
            SPSQProductDetails productdetails = new SPSQProductDetails();

            var result = ac.GetData<SPSQProductDetails>("ItemDotNetAPI", "Description eq '" + productName + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                productdetails = result.Result.Item1.value[0];

            return productdetails;
        }

        [Route("GetProductPackingStyle")]
        public List<SPSQProductPackingStyle> GetProductPackingStyle(string prodNo)
        {
            API ac = new API();

            List<SPSQProductPackingStyle> packingStyle = new List<SPSQProductPackingStyle>();

            var result = ac.GetData<SPSQProductPackingStyle>("ItemPackingStyleDotNetAPI", "Item_No eq '" + prodNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                packingStyle = result.Result.Item1.value;

            return packingStyle;
        }

        [Route("updateInquiryNotifcationStatus")]
        public bool updateInquiryNotifcationStatus(string InqNo)
        {
            //use table InquiryMessageMst
            //code for update field Status='Complete' of table InquiryMessageMst
            return true;
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

        [Route("GetSalesQuoteFromNo")]
        public SPSQHeaderDetails GetSalesQuoteFromNo(string SQNo)
        {
            API ac = new API();
            SPSQHeaderDetails SQHeaderDetails = new SPSQHeaderDetails();
            SPSQHeader SQHeader = new SPSQHeader();
            List<SPSQLines> SQLines = new List<SPSQLines>();

            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "No eq '" + SQNo + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                SQHeader = result.Result.Item1.value[0];

                SQHeaderDetails.QuoteNo = SQHeader.No;
                SQHeaderDetails.InquiryNo = SQHeader.PCPL_Inquiry_No;
                SQHeaderDetails.ValidUntillDate = SQHeader.Quote_Valid_Until_Date;
                SQHeaderDetails.LocationCode = SQHeader.Location_Code;
                SQHeaderDetails.ContactCompanyNo = SQHeader.Sell_to_Contact_No;
                SQHeaderDetails.ContactCompanyName = SQHeader.Sell_to_Contact;
                SQHeaderDetails.ContactPersonNo = SQHeader.PCPL_Contact_Person;
                SQHeaderDetails.CustomerNo = SQHeader.Sell_to_Customer_No;
                SQHeaderDetails.OrderDate = SQHeader.Order_Date;
                //SQHeaderDetails.PaymentMethodCode = SQHeader.Payment_Method_Code;
                SQHeaderDetails.TransportMethodCode = SQHeader.Transport_Method;
                SQHeaderDetails.PaymentTermsCode = SQHeader.Payment_Terms_Code;
                SQHeaderDetails.ShipmentMethodCode = SQHeader.Shipment_Method_Code;
                SQHeaderDetails.ShiptoCode = SQHeader.Ship_to_Code;
                SQHeaderDetails.JobtoCode = SQHeader.PCPL_Job_to_Code;
                SQHeaderDetails.ShortcloseStatus = SQHeader.TPTPL_Short_Close;
                SQHeaderDetails.SCRemarksSetupValue = SQHeader.TPTPL_SC_Reason_Setup_Value;
                SQHeaderDetails.Status = SQHeader.PCPL_Status;
                SQHeaderDetails.ApprovalFor = SQHeader.PCPL_ApprovalFor;
                SQHeaderDetails.WorkDescription = SQHeader.WorkDescription;
                SQHeaderDetails.SalespersonEmail = SQHeader.PCPL_SalesPerson_Email;

                var resultSQLines = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + SQNo + "'");
                
                if (resultSQLines.Result.Item1.value.Count > 0)
                {
                    SQLines = resultSQLines.Result.Item1.value;
                    List<SPSQLines> SQLineList = new List<SPSQLines>();
                    for(int a = 0; a < SQLines.Count; a++)
                    {
                        //SPSQLines SQLines_ = new SPSQLines();
                        //SQLines_.Line_No = SQLines[a].Line_No;
                        //SQLines_.No = SQLines[a].No;
                        //SQLines_.Description = SQLines[a].Description;
                        //SQLines_.Location_Code = SQLines[a].Location_Code;
                        //SQLines_.Unit_Price = SQLines[a].Unit_Price;
                        //SQLines_.Quantity = SQLines[a].Quantity;
                        //SQLines_.Delivery_Date = SQLines[a].Delivery_Date;
                        //SQLines_.PCPL_Packing_Style_Code = SQLines[a].PCPL_Packing_Style_Code;
                        //SQLines_.PCPL_Transport_Method = SQLines[a].PCPL_Transport_Method;
                        //SQLines_.PCPL_MRP = SQLines[a].PCPL_MRP;
                        //SQLines_.Drop_Shipment = SQLines[a].Drop_Shipment;
                        //SQLines_.PCPL_Vendor_No = SQLines[a].PCPL_Vendor_No;

                        //SQHeaderDetails.ProductsRes.Add(SQLines_);
                        SQLineList.Add(new SPSQLines()
                        {
                            Line_No = SQLines[a].Line_No,
                            No = SQLines[a].No,
                            Description = SQLines[a].Description,
                            Location_Code = SQLines[a].Location_Code,
                            Unit_Price = SQLines[a].Unit_Price,
                            Unit_Cost_LCY = SQLines[a].Unit_Cost_LCY,
                            Quantity = SQLines[a].Quantity,
                            PCPL_Concentration_Rate_Percent = SQLines[a].PCPL_Concentration_Rate_Percent,
                            Net_Weight = SQLines[a].Net_Weight,
                            PCPL_Liquid_Rate = SQLines[a].PCPL_Liquid_Rate,
                            PCPL_Liquid = SQLines[a].PCPL_Liquid,
                            Delivery_Date = SQLines[a].Delivery_Date,
                            Unit_of_Measure_Code = SQLines[a].Unit_of_Measure_Code,
                            PCPL_Packing_Style_Code = SQLines[a].PCPL_Packing_Style_Code,
                            PCPL_Transport_Method = SQLines[a].PCPL_Transport_Method,
                            PCPL_Transport_Cost = SQLines[a].PCPL_Transport_Cost,
                            PCPL_MRP = SQLines[a].PCPL_MRP,
                            Drop_Shipment = SQLines[a].Drop_Shipment,
                            PCPL_Vendor_No = SQLines[a].PCPL_Vendor_No,
                            PCPL_Vendor_Name = SQLines[a].PCPL_Vendor_Name,
                            PCPL_Total_Cost = SQLines[a].PCPL_Total_Cost,
                            PCPL_Margin = SQLines[a].PCPL_Margin,
                            PCPL_Margin_Percent = SQLines[a].PCPL_Margin_Percent,
                            PCPL_Sales_Discount = SQLines[a].PCPL_Sales_Discount,
                            PCPL_Commission_Type = SQLines[a].PCPL_Commission_Type,
                            PCPL_Commission = SQLines[a].PCPL_Commission,
                            PCPL_Commission_Amount = SQLines[a].PCPL_Commission_Amount,
                            PCPL_Credit_Days = SQLines[a].PCPL_Credit_Days,
                            PCPL_Interest = SQLines[a].PCPL_Interest,
                            PCPL_Commission_Payable = SQLines[a].PCPL_Commission_Payable,
                            PCPL_Commission_Payable_Name = SQLines[a].PCPL_Commission_Payable_Name,
                            TPTPL_Short_Closed = SQLines[a].TPTPL_Short_Closed

                        });
                    }

                    SQHeaderDetails.ProductsRes = SQLineList;

                }
                
            }
                

            return SQHeaderDetails;
        }

        [Route("GetCustomerTemplateCode")]
        public string GetCustomerTemplateCode()
        {
            string customerTemplateCode = "";
            API ac = new API();
            SPSalesReceivableSetup salesReceivableSetup = new SPSalesReceivableSetup();
            //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
            //int skip = 0, top = 10;

            var result = ac.GetData<SPSalesReceivableSetup>("SalesReceivableSetupDotNetAPI", "");

            //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salesReceivableSetup = result.Result.Item1.value[0];
                customerTemplateCode = salesReceivableSetup.PCPL_Inquiry_Customer_Template;
            }

            return customerTemplateCode;
        }

        [Route("GetSQDetailsBySQNo")]
        public List<SPSQLines> GetSQDetailsBySQNo(string DocumentNo)
        {
            API ac = new API();
            List<SPSQLines> lineitems = new List<SPSQLines>();

            var result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + DocumentNo + "' and PCPL_Margin lt " + 0); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                lineitems = result.Result.Item1.value;

            return lineitems;
        }

        //[Route("GetSQDetailsForPrintPreview")]
        //public string GetSQDetailsForPrintPreview(string SQNo)
        //{
        //    string response = "";
        //    API ac = new API();

        //    SPSQDetailsForPrintPost ReqSqDetailsForPrint = new SPSQDetailsForPrintPost();
        //    SPSQDetailsForPrintOData ResSqDetailsForPrint = new SPSQDetailsForPrintOData();

        //    //ReqSqDetailsForPrint.docno = SQNo;
        //    ReqSqDetailsForPrint.docno = "POD000002";

        //    var result = PostItemForGetSQDetailsForPrint<SPSQDetailsForPrintOData>("", ReqSqDetailsForPrint, ResSqDetailsForPrint);

        //    response = result.Result.Item1.value;

        //    return response;
        //}

        [HttpGet]
        [Route("PrintQuote")]
        public string PrintQuote(string QuoteNo)
        {
            var printQuoteResponse = new PrintQuoteResponse();

            PrintQuoteRequest printQuoteRequest = new PrintQuoteRequest
            {
                docno = QuoteNo
            };

            var result = (dynamic)null;
            result = PostQuotePrint("APIMngt_SalesQuoteReportPrint", printQuoteRequest, printQuoteResponse);

            var base64PDF = "";
            if (result.Result.Item1 != null)
            {
                base64PDF = result.Result.Item1.value;

            }
            return base64PDF;
        }

        [HttpGet]
        [Route("GenerateCostSheet")]
        public string GenerateCostSheet(string SQNo, int ItemLineNo)
        {
            string response = "";
            API ac = new API();
            SPSQCostSheet costSheet = new SPSQCostSheet();
            SPSQCostSheetOData costSheetOData = new SPSQCostSheetOData();
            
            costSheet.salesdoctype = "0";
            costSheet.salesdocno = SQNo;
            costSheet.doclineno = ItemLineNo;
            costSheet.frombc = false;

            var result = PostItemForGenerateCostSheet<SPSQCostSheetOData>("", costSheet, costSheetOData);

            response = result.Result.Item1.value;

            return response;
        }

        [Route("GetCostSheetDetails")]
        public List<SPSQCostSheetDetails> GetCostSheetDetails(string SQNo, int ItemLineNo)
        {
            API ac = new API();
            List<SPSQCostSheetDetails> costSheetDetails = new List<SPSQCostSheetDetails>();

            var resultCostSheet = ac.GetData<SPSQCostSheetDetails>("SalesCostSheetLines", "TPTPL_Document_Type eq 'Quote' and TPTPL_Document_No eq '" + SQNo + "' and TPTPL_Source_Document_Line_No eq " + ItemLineNo);

            if (resultCostSheet.Result.Item1.value.Count > 0)
            {
                //responseShiptoAddress = resultGetShipToAddress.Result.Item1;
                //creditlimitcustdetails.ShiptoAddress = responseShiptoAddress;
                costSheetDetails = resultCostSheet.Result.Item1.value;

            }

            return costSheetDetails;
        }


        [Route("UpdateCostSheet")]
        public bool UpdateCostSheet(string SQNo, int CostSheetLineNo, double RatePerUnit)
        {
            bool flag = false;
            SPSQUpdateCostSheet reqCostSheetUpdate = new SPSQUpdateCostSheet();
            SPSQCostSheetDetails resCostSheetUpdate = new SPSQCostSheetDetails();
            errorDetails ed = new errorDetails();

            reqCostSheetUpdate.TPTPL_Rate_per_Unit = RatePerUnit;

            var result = PatchItemCostSheet("SalesCostSheetLines", reqCostSheetUpdate, resCostSheetUpdate, "TPTPL_Document_Type='Quote',TPTPL_Document_No='" + SQNo + "',TPTPL_Line_No=" + CostSheetLineNo);

            if (result.Result.Item1 != null)
            {
                flag = true;
                resCostSheetUpdate = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [Route("GetLocationCode")]
        public string GetLocationCode(string NoSeriesCode)
        {
            API ac = new API();
            List<SPSQNoSeriesDetails> noSeriesDetails = new List<SPSQNoSeriesDetails>();

            var result = ac.GetData<SPSQNoSeriesDetails>("NoSeriesRelDotNetAPI", "Series_Code eq '" + NoSeriesCode + "'");

            if (result.Result.Item1.value.Count > 0)
                noSeriesDetails = result.Result.Item1.value;

            string Location_Code = noSeriesDetails[0].PCPL_Location_Code;

            return Location_Code;

        }

        [Route("GetInterestRate")]
        public SPSalesReceivableSetup GetInterestRate()
        {
            API ac = new API();
            SPSalesReceivableSetup salesReceivableSetups = new SPSalesReceivableSetup();

            var result = ac.GetData<SPSalesReceivableSetup>("SalesReceivableSetupDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                salesReceivableSetups = result.Result.Item1.value[0];

            return salesReceivableSetups;

        }

        [Route("GetInventoryDetails")]
        public List<SPSQInvDetailsRes> GetInventoryDetails(string ProdNo, string LocCode)
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

        [Route("GetPurDiscountDetails")]
        public List<SPSQPurDiscountDetails> GetPurDiscountDetails(string ProdNo)
        {
            API ac = new API();
            List<SPSQPurDiscountDetails> purDisDetails = new List<SPSQPurDiscountDetails>();

            int CurYear = DateTime.Now.Year;
            int CurMon = DateTime.Now.Month;
            int CurDay = DateTime.Now.Day;
            string CurDate = CurYear + "-" + CurMon + "-" + CurDay;

            var result = ac.GetData<SPSQPurDiscountDetails>("PurchaseLineCostingDiscountsDotNetAPI", "Item_No eq '" + ProdNo + "' and (Starting_Date le " + CurDate + " and Ending_Date ge " + CurDate + ")");

            if (result.Result.Item1.value.Count > 0)
                purDisDetails = result.Result.Item1.value;

            return purDisDetails;
        }

        [Route("GetTransSalesInvoiceLine")]
        public List<SPSQSalesInvoiceDetails> GetTransSalesInvoiceLine(string CustNo, string LocCode, string TransType, string ProdNo)
        {
            API ac = new API();
            List<SPSQSalesInvoiceDetails> salesInvioceLines = new List<SPSQSalesInvoiceDetails>();

            var result = (dynamic)null;

            if (TransType == "CustTrans")
                result = ac.GetData1<SPSQSalesInvoiceDetails>("PostedSalesInvoiceLinesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo + "' and Location_Code eq '" + LocCode + "'", 0, 3, "Posting_Date desc");

            if (TransType == "ProdTrans")
                result = ac.GetData1<SPSQSalesInvoiceDetails>("PostedSalesInvoiceLinesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo + "' and Location_Code eq '" + LocCode + "' and No eq '" + ProdNo + "'", 0, 3, "Posting_Date desc");

            if (result.Result.Item1 != null)
                salesInvioceLines = result.Result.Item1.value;

            return salesInvioceLines;
        }

        [Route("GetSQNoFromInqNo")]
        public string GetSQNoFromInqNo(string InqNo)
        {
            API ac = new API();
            SPSQHeader SQHeader = new SPSQHeader();
            string SQNo = "", ScheduleStatus = "";
            
            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "PCPL_Inquiry_No eq '" + InqNo + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                SQHeader = result.Result.Item1.value[0];
                SQNo = SQHeader.No;
                ScheduleStatus = SQHeader.TPTPL_Schedule_status;
            }
            else
            {
                return "";
            }

            string SQNo_ScheduleStatus = SQNo + "_" + ScheduleStatus;

            return SQNo_ScheduleStatus;
        }


        [HttpPost]
        [Route("SalesQuoteSendEmail")]
        public bool SalesQuoteSendEmail(string custEmail, string SPEmail, string SQNo)
        {
            bool flag = false;
            string emailBody = "";
            
            emailBody += "<table width=\"100%\" border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Quote No</th><th>Quote Date</th><th>Customer</th><th>Payment Terms</th><th>Inco Terms</th><th>Transport Method</th></tr></thead><tbody>";
            
                API ac = new API();
                SPSQHeader spSQHeader = new SPSQHeader();
                List<SPSQLines> spSQLines = new List<SPSQLines>();

                var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "No eq '" + SQNo + "'");

            if (result.Result.Item1.value.Count > 0)
                spSQHeader = result.Result.Item1.value[0];

            if (spSQHeader.No != null)
            {
                emailBody += "<tr><td>" + spSQHeader.No + "</td><td>" + spSQHeader.Order_Date + "</td><td>" + spSQHeader.Sell_to_Contact
                        + "</td><td>" + spSQHeader.Payment_Terms_Code + "</td><td>" + spSQHeader.Shipment_Method_Code +
                        "</td><td>" + spSQHeader.Transport_Method + "</td></tr>";

                emailBody += "<tr><td></td><td colspan=\"6\"><table width=\"50%\" border=\"1\"><thead><tr style=\"background-color:gray;color:black\"><th>Product Name</th><th>Qty</th><th>Packing Style</th><th>UOM</th></tr></thead><tbody>";

                var resultSQLines = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + SQNo + "'");

                if (resultSQLines.Result.Item1.value.Count > 0)
                {
                    spSQLines = resultSQLines.Result.Item1.value;

                    for (int a = 0; a < spSQLines.Count; a++)
                    {
                        emailBody += "<tr><td>" + spSQLines[a].Description + "</td><td>" + spSQLines[a].Quantity + "</td><td>" + spSQLines[a].PCPL_Packing_Style_Code + "</td><td>" + spSQLines[a].Unit_of_Measure_Code + "</td></tr>";
                    }

                    emailBody += "</tbody></table></td></tr>";
                }

                emailBody += "</tbody></table>";

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>Sales Quote Details</p>");
                sbMailBody.Append(emailBody);
                //sbMailBody.Append("<table width=\"100%\" border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Inq No</th><th>Inq Date</th><th>Customer</th><th>Delivery Date</th><th>Payment Terms</th><th>Status</th><th>Consignee Address</th></tr></thead>" +
                //    "<tbody><tr><td>INQ42</td><td>19-10-2024</td><td>AADINATH AGENCY</td><td>19-10-2024</td><td>01 DAYS RT</td><td>Pending</td><td>Address Details</td></tr><tr><td></td><td colspan=\"6\"><table width=\"50%\"><thead><tr style=\"background-color:darkgrey;color:black\">" +
                //    "<th>Product Name</th><th>Qty</th><th>UOM</th></tr></thead><tbody><tr><td>CAUSTIC SODA LYE</td><td>4</td><td>MTS</td></tr></tbody></table></td></tr><tr><td>INQ43</td><td>19-10-2024</td><td>AADINATH AGENCY</td><td>19-10-2024</td><td>02 DAYS RT</td><td>Pending</td>" +
                //    "<td>Address Details</td></tr><tr><td></td><td colspan=\"6\"><table width=\"50%\"><thead><tr style=\"background-color:darkgrey;color:black\"><th>Product Name</th><th>Qty</th><th>UOM</th></tr></thead><tbody><tr><td>BENZYL CHLORIDE</td><td>5</td><td>MTS</td></tr></tbody></table>" +
                //    "</td></tr></tbody></table>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                //emailService.SendEmailTo(email, sbMailBody.ToString(), "Change Password - PrakashCRM");
                emailService.SendEmail(custEmail, SPEmail, "Sales Quote Details - PrakashCRM", sbMailBody.ToString());
                flag = true;
            }
            return flag;
        }

        [HttpPost]
        [Route("SalesQuoteShortclose")]
        public string SalesQuoteShortclose(string Type, string SQNo, string SQProdLineNo, string ShortcloseReason, string ShortcloseRemarks)
        {
            bool flag = false;
            string errMsg = "";
            SPSQShortclose sqShortclose = new SPSQShortclose();
            SPSQProdShortclose sqProdShortclose = new SPSQProdShortclose();
            SPSQShortcloseOData sqShortcloseOData = new SPSQShortcloseOData();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;

            if(Type == "SalesQuote")
            {
                sqShortclose.salesheader = SQNo;
                sqShortclose.shortclosereason = ShortcloseReason;
                sqShortclose.shortcloseremarks = ShortcloseRemarks == null ? "" : ShortcloseRemarks;

                result = PostItemSQShortclose("", sqShortclose, sqShortcloseOData);
            }
            else if(Type == "SalesQuoteProd")
            {
                sqProdShortclose.salesheader = SQNo;
                sqProdShortclose.lineno = SQProdLineNo;
                sqProdShortclose.shortclosereason = ShortcloseReason;
                sqProdShortclose.shortcloseremarks = ShortcloseRemarks == null ? "" : ShortcloseRemarks;

                result = PostItemSQProdShortclose("", sqProdShortclose, sqShortcloseOData);
            }

            sqShortcloseOData = result.Result.Item1;
            ed = result.Result.Item2;
            sqShortcloseOData.errorDetails = ed;

            if (!sqShortcloseOData.errorDetails.isSuccess)
                errMsg = sqShortcloseOData.errorDetails.message;
            
            //flag = Convert.ToBoolean(sqShortcloseOData.value);

            return errMsg;
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

        [HttpPost]
        [Route("AddNewContactPerson")]
        public SPContact AddNewContactPerson(SPContact reqCPerson)
        {
            API ac = new API();
            SPContact resCPerson = new SPContact();
            errorDetails ed = new errorDetails();

            var result = ac.PostItem("ContactDotNetAPI", reqCPerson, resCPerson);

            if (result.Result.Item1 != null)
                resCPerson = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return resCPerson;
        }

        [HttpPost]
        [Route("AddNewBillToAddress")]
        public SPInqNewShiptoAddressRes AddNewBillToAddress(SPInqNewShiptoAddress reqNewShiptoAddress)
        {
            API ac = new API();
            SPInqNewShiptoAddressRes resNewShiptoAddress = new SPInqNewShiptoAddressRes();
            errorDetails ed = new errorDetails();

            reqNewShiptoAddress.Address_2 = reqNewShiptoAddress.Address_2 == null || reqNewShiptoAddress.Address_2 == "" ? "" : reqNewShiptoAddress.Address_2;
            reqNewShiptoAddress.Ship_to_GST_Customer_Type = "Registered";

            var result = PostItemAddNewShiptoAddress("ShiptoAddressDotNetAPI", reqNewShiptoAddress, resNewShiptoAddress);

            resNewShiptoAddress = result.Result.Item1;
            ed = result.Result.Item2;
            resNewShiptoAddress.errorDetails = ed;

            return resNewShiptoAddress;
        }

        [HttpPost]
        [Route("AddNewDeliveryToAddress")]
        public SPInqNewJobtoAddressRes AddNewDeliveryToAddress(SPInqNewJobtoAddress reqNewJobtoAddress)
        {
            API ac = new API();
            SPInqNewJobtoAddressRes resNewJobtoAddress = new SPInqNewJobtoAddressRes();
            errorDetails ed = new errorDetails();

            reqNewJobtoAddress.Address_2 = reqNewJobtoAddress.Address_2 == null || reqNewJobtoAddress.Address_2 == "" ? "" : reqNewJobtoAddress.Address_2;
            reqNewJobtoAddress.Job_to_GST_Customer_Type = "Registered";

            var result = PostItemAddNewJobtoAddress("JobtoAddressDotNetAPI", reqNewJobtoAddress, resNewJobtoAddress);

            resNewJobtoAddress = result.Result.Item1;
            ed = result.Result.Item2;
            resNewJobtoAddress.errorDetails = ed;

            return resNewJobtoAddress;
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

            var result = ac.GetData<Area>("AreasListDotNetAPI", "Pincode eq '" + Pincode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                areas = result.Result.Item1.value;

            return areas;
        }

        [Route("GetShortcloseReasons")]
        public List<SPSQShortcloseReasons> GetShortcloseReasons()
        {
            API ac = new API();
            List<SPSQShortcloseReasons> shortCloseReasons = new List<SPSQShortcloseReasons>();

            var result = ac.GetData<SPSQShortcloseReasons>("ShortCloseReasonDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                shortCloseReasons = result.Result.Item1.value;

            return shortCloseReasons;
        }

        [Route("GetSalesQuoteJustificationDetails")]
        public List<SPSQJustificationDetails> GetSalesQuoteJustificationDetails(int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSQJustificationDetails> salesquotes = new List<SPSQJustificationDetails>();
            
            var result = ac.GetData1<SPSQJustificationDetails>("SalesQuoteDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                salesquotes = result.Result.Item1.value;

            for(int a = 0; a < salesquotes.Count; a++)
            {
                DateTime date_ = Convert.ToDateTime(salesquotes[a].PCPL_Target_Date);
                salesquotes[a].PCPL_Target_Date = date_.ToString("dd/MM/yyyy");
            }

            return salesquotes;
        }

        [Route("GetCompanyIndustry")]
        public SPSQCompanyIndustry GetCompanyIndustry(string CCompanyNo)
        {
            API ac = new API();
            SPSQCompanyIndustry companyIndustry = new SPSQCompanyIndustry();

            var result = ac.GetData<SPSQCompanyIndustry>("ContactDotNetAPI", "No eq '" + CCompanyNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                companyIndustry = result.Result.Item1.value[0];

            return companyIndustry;

        }

        public async Task<(SPSQHeader, errorDetails)> PostItemSQ<SPSQHeader>(string apiendpoint, SPSQHeaderPost requestModel, SPSQHeader responseModel)
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
                    responseModel = res.ToObject<SPSQHeader>();

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
        
        public async Task<(SPSQHeader, errorDetails)> PostItemSQWithCustTemplateCode<SPSQHeader>(string apiendpoint, SPSQHeaderPostWithCustTemplateCode requestModel, SPSQHeader responseModel)
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
                    responseModel = res.ToObject<SPSQHeader>();

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

        public async Task<(SPSQLines, errorDetails)> PostItemSQLines<SPSQLines>(string apiendpoint, string SQLineType, SPSQLinesPost requestModel, SPSQLiquidLinesPost requestModel1, SPSQLines responseModel)
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

            string ItemCardObjString = "";

            if (SQLineType == "SQLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            else if (SQLineType == "SQLiquidLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel1);

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
                    responseModel = res.ToObject<SPSQLines>();

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

        public async Task<(SPSQGetNextNoRes, errorDetails)> PostItemForSQGetNextNo<SPSQGetNextNoRes>(string apiendpoint, SPSQGetNextNo requestModel, SPSQGetNextNoRes responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/DeleteDotNetAPIs_GetNextNo?Company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQGetNextNoRes>();

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

        public async Task<(List<SPSQShiptoAddressRes>, errorDetails)> PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>(string apiendpoint, SPSQShiptoAddress requestModel, List<SPSQShiptoAddressRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_GetShipToAddress?company=\'Prakash Company\'");
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
                    SPSQAddressOData shiptoAddressOData = res.ToObject<SPSQAddressOData>();
                    string shipToAddressData = shiptoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQShiptoAddressRes>>(shipToAddressData);

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

        public async Task<(List<SPSQJobtoAddressRes>, errorDetails)> PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>(string apiendpoint, SPSQJobtoAddress requestModel, List<SPSQJobtoAddressRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_GetJobToAddress?company=\'Prakash Company\'");
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
                    SPSQAddressOData jobtoAddressOData = res.ToObject<SPSQAddressOData>();
                    string jobToAddressData = jobtoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQJobtoAddressRes>>(jobToAddressData);

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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQ<SPSQHeader>(string apiendpoint, SPSQHeaderUpdate requestModel, SPSQHeader responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPSQHeader>();


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

        
        public async Task<(SPSQLines, errorDetails)> PatchItemSQLines<SPSQLines>(string apiendpoint, string SQLineType, SPSQLinesUpdate requestModel, SPSQLiquidLinesUpdate requestModel1, SPSQLines responseModel, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (SQLineType == "SQLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            else if (SQLineType == "SQLiquidLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel1);

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
                    responseModel = res.ToObject<SPSQLines>();


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

        public async Task<(SPInqLines, errorDetails)> PatchItemInqToQuote<SPInqLines>(string apiendpoint, SPSQUpdateInqToQuote requestModel, SPInqLines responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPInqLines>();


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


        public async Task<(SPSQScheduleQty, errorDetails)> PatchItemScheduleQty<SPSQScheduleQty>(string apiendpoint, SPSQScheduleQtyPost requestModel, SPSQScheduleQty responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPSQScheduleQty>();


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

        public async Task<(SPSQScheduleOrderOData, errorDetails)> PostItemForScheduleOrder<SPSQScheduleOrderOData>(string apiendpoint, SPSQScheduleOrderPost requestModel, SPSQScheduleOrderOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitEventMngt_MakeOrder?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQScheduleOrderOData>();

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

        //public async Task<(SPSQDetailsForPrintOData, errorDetails)> PostItemForGetSQDetailsForPrint<SPSQDetailsForPrintOData>(string apiendpoint, SPSQDetailsForPrintPost requestModel, SPSQDetailsForPrintOData responseModel)
        //{
        //    string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
        //    string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
        //    string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
        //    string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

        //    API ac = new API();
        //    var accessToken = await ac.GetAccessToken();

        //    HttpClient _httpClient = new HttpClient();
        //    string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_SalesQuoteReportPrint?company=\'Prakash Company\'");
        //    Uri baseuri = new Uri(encodeurl);
        //    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


        //    string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
        //    var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = null;
        //    try
        //    {
        //        response = _httpClient.PostAsync(baseuri, content).Result;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    errorDetails errordetail = new errorDetails();
        //    errordetail.isSuccess = response.IsSuccessStatusCode;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var JsonData = response.Content.ReadAsStringAsync().Result;
        //        try
        //        {
        //            JObject res = JObject.Parse(JsonData);
        //            //SPSQScheduleOrderOData scheduleOrderOData = res.ToObject<SPSQScheduleOrderOData>();
        //            responseModel = res.ToObject<SPSQDetailsForPrintOData>();

        //            //string scheduleOrderData = "{\"value\":" + scheduleOrderOData.value + "}";
        //            //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(scheduleOrderData);

        //            errordetail.code = response.StatusCode.ToString();
        //            errordetail.message = response.ReasonPhrase;
        //        }
        //        catch (Exception ex1)
        //        {
        //        }
        //    }
        //    else
        //    {
        //        var JsonData = response.Content.ReadAsStringAsync().Result;

        //        try
        //        {
        //            JObject res = JObject.Parse(JsonData);
        //            errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
        //            errordetail = emd.error;
        //        }
        //        catch (Exception ex1)
        //        {
        //        }
        //    }
        //    return (responseModel, errordetail);
        //}

        public async Task<(PrintQuoteResponse, errorDetails)> PostQuotePrint<PrintQuoteRequest>(string apiendpoint, PrintQuoteRequest requestModel, PrintQuoteResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
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
                    responseModel = res.ToObject<PrintQuoteResponse>();

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

        public async Task<(SPSQUpdateInqStatusOData, errorDetails)> PostItemForUpdateInqStatus<SPSQUpdateInqStatusOData>(string apiendpoint, SPSQUpdateInqStatus requestModel, SPSQUpdateInqStatusOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_UpdateInquirytoQuoteandStatus?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQUpdateInqStatusOData>();

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

        public async Task<(SPSQInvQtyReserveOData, errorDetails)> PostItemInvQtyReserve<SPSQInvQtyReserveOData>(string apiendpoint, List<SPSQInvQtyReserve> requestModel, SPSQInvQtyReserveOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_InsertReservationEntry?company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            SPSQInvQtyReservePost invQtyReservePost = new SPSQInvQtyReservePost();
            //invQtyReservePost.text = requestModel;
            string ObjString_ = JsonConvert.SerializeObject(requestModel);
            string txtString = ObjString_.Replace("\"", "'");
            invQtyReservePost.text = txtString;
            string txtString_ = JsonConvert.SerializeObject(invQtyReservePost);
            //ObjString_ = ObjString_.Replace("\"text\"", '"' + "text" + '"');
            var content = new StringContent(txtString_, Encoding.UTF8, "application/json");

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
                    SPSQInvQtyReserveOData invQtyReserveOData = res.ToObject<SPSQInvQtyReserveOData>();
                    responseModel = invQtyReserveOData;

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

        public async Task<(SPSQCostSheetOData, errorDetails)> PostItemForGenerateCostSheet<SPSQCostSheetOData>(string apiendpoint, SPSQCostSheet requestModel, SPSQCostSheetOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/Sales_Cost_Sheet_Mngt_CheckandCreateCostSheet?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQCostSheetOData>();
                    //SPSQCostSheetOData costSheetOData = res.ToObject<SPSQCostSheetOData>();
                    
                    //string costSheetData = "{\"value\":" + costSheetOData.value + "}";
                    //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(costSheetData);

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

        
        public async Task<(SPSQCostSheetDetails, errorDetails)> PatchItemCostSheet<SPSQCostSheetDetails>(string apiendpoint, SPSQUpdateCostSheet requestModel, SPSQCostSheetDetails responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPSQCostSheetDetails>();


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

        public async Task<(SPSQShortcloseOData, errorDetails)> PostItemSQShortclose<SPSQShortcloseOData>(string apiendpoint, SPSQShortclose requestModel, SPSQShortcloseOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/ShortCloseMngt_ShortCloseQuote?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQShortcloseOData>();

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

        public async Task<(SPSQShortcloseOData, errorDetails)> PostItemSQProdShortclose<SPSQShortcloseOData>(string apiendpoint, SPSQProdShortclose requestModel, SPSQShortcloseOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/ShortCloseMngt_ShortCloseQuoteLine?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQShortcloseOData>();

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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQApproveReject<SPSQHeader>(string apiendpoint, SPSQForApprove requestModelApprove, SPSQForReject requestModelReject, SPSQHeader responseModel, string Action, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (Action == "Approve")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelApprove);
            else if (Action == "Reject")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelReject);

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
                    responseModel = res.ToObject<SPSQHeader>();


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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQApproveRejectHOD<SPSQHeader>(string apiendpoint, SPSQForApproveHOD requestModelApprove, SPSQForRejectHOD requestModelReject, SPSQHeader responseModel, string Action, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (Action == "Approve")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelApprove);
            else if (Action == "Reject")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelReject);

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
                    responseModel = res.ToObject<SPSQHeader>();


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

        public async Task<(SPInqNewShiptoAddressRes, errorDetails)> PostItemAddNewShiptoAddress<SPInqNewShiptoAddressRes>(string apiendpoint, SPInqNewShiptoAddress requestModel, SPInqNewShiptoAddressRes responseModel)
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
                    responseModel = res.ToObject<SPInqNewShiptoAddressRes>();

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

        public async Task<(SPInqNewJobtoAddressRes, errorDetails)> PostItemAddNewJobtoAddress<SPInqNewJobtoAddressRes>(string apiendpoint, SPInqNewJobtoAddress requestModel, SPInqNewJobtoAddressRes responseModel)
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
                    responseModel = res.ToObject<SPInqNewJobtoAddressRes>();

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

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
