﻿using Newtonsoft.Json.Linq;
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
using System.Net.Http.Headers;
using System.Globalization;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPWarehouse")]
    public class SPWarehouseController : ApiController
    {
        public bool Latest_Rate { get; private set; }
        #region IncomingTask

        [Route("GetWarehouseSales")]
        public List<SPWarehouse> GetWarehouseSales(int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq ''";
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq ''";

            var result = ac.GetData1<SPWarehouse>("SalesOrderCardDotNetAPI", filter, skip, top, orderby); //SalesLinesDotNetAPI, SalesOrderLineDotNetAPI

            if (result.Result.Item1.value.Count > 0)
                warehouseSales = result.Result.Item1.value;

            for (int i = 0; i < warehouseSales.Count; i++)
            {
                string[] strDate = warehouseSales[i].Shipment_Date.Split('-');
                warehouseSales[i].Shipment_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return warehouseSales;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq ''";
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq ''";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllSalesLine")]
        public List<SPWarehouseSalesLine> GetAllSalesLine(string Document_No)
        {
            API ac = new API();
            List<SPWarehouseSalesLine> warehouseSalesLine = new List<SPWarehouseSalesLine>();

            var result = ac.GetData<SPWarehouseSalesLine>("SalesOrderLineDotNetAPI", "Type eq 'Item' and Document_No eq '" + Document_No + "'");

            if (result.Result.Item1.value.Count > 0)
                warehouseSalesLine = result.Result.Item1.value;

            return warehouseSalesLine;
        }

        public async Task<(SPWarehouseSalesAcceptResponse, errorDetails)> PostCodeUnit<SPWarehouseSalesAccept>(string apiendpoint, SPWarehouseSalesAccept requestModel, SPWarehouseSalesAcceptResponse responseModel)
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
                    responseModel = res.ToObject<SPWarehouseSalesAcceptResponse>();

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

        #endregion

        #region AcceptedTask

        [Route("GetWarehouseSalesAcceptedTask")]
        public List<SPWarehouse> GetWarehouseSalesAcceptedTask(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No eq ''"; //Document_Type eq 'Order' and 
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No eq ''";

            var result = ac.GetData1<SPWarehouse>("SalesOrderCardDotNetAPI", filter, skip, top, orderby); //SalesLinesDotNetAPI, SalesOrderLineDotNetAPI

            if (result.Result.Item1.value.Count > 0)
                warehouseSales = result.Result.Item1.value;

            for (int i = 0; i < warehouseSales.Count; i++)
            {
                string[] strDate = warehouseSales[i].Shipment_Date.Split('-');
                warehouseSales[i].Shipment_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return warehouseSales;
        }

        [Route("GetAcceptedApiRecordsCount")]
        public int GetAcceptedApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No eq ''";//Document_Type eq 'Order' and 
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No eq ''"; // and Document_Type eq 'Order' and Type eq 'Item' and Status eq 'Released'

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllTransporterForDDL")]
        public List<Transporter> GetAllTransporterForDDL()
        {
            API ac = new API();
            List<Transporter> transporter = new List<Transporter>();

            var result = ac.GetData<Transporter>("VendorDotNetAPI", "Transporter eq true");

            if (result != null && result.Result.Item1.value.Count > 0)
                transporter = result.Result.Item1.value;

            return transporter;
        }

        [Route("GetTransporterDetailByVendorNo")]
        public List<Transporter> GetTransporterDetailByVendorNo(string vendorno)
        {
            API ac = new API();
            List<Transporter> transporter = new List<Transporter>();

            var result = ac.GetData<Transporter>("VendorDotNetAPI", "No eq '" + vendorno + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                transporter = result.Result.Item1.value;

            return transporter;
        }

        #endregion

        #region ClosedTask

        [Route("GetWarehouseSalesClosedTask")]
        public List<SPWarehouse> GetWarehouseSalesClosedTask(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No ne ''"; //Document_Type eq 'Order' and 
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No ne ''";

            var result = ac.GetData1<SPWarehouse>("SalesOrderCardDotNetAPI", filter, skip, top, orderby); //SalesLinesDotNetAPI, SalesOrderLineDotNetAPI

            if (result.Result.Item1.value.Count > 0)
                warehouseSales = result.Result.Item1.value;

            for (int i = 0; i < warehouseSales.Count; i++)
            {
                string[] strDate = warehouseSales[i].Shipment_Date.Split('-');
                warehouseSales[i].Shipment_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return warehouseSales;
        }

        [Route("GetClosedApiRecordsCount")]
        public int GetClosedApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No ne ''";//Document_Type eq 'Order' and 
            else
                filter += " and Status eq 'Released' and PCPL_Accepted_By eq '" + SPCode + "' and PCPL_Transporter_No ne ''"; // and Document_Type eq 'Order' and Type eq 'Item' and Status eq 'Released'

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        #endregion

        #region New_WarehouseCode

        [Route("GetWarehouseTaskAll")]
        public List<SPWarehouseList> GetWarehouseTaskAll(string orderby, string filter)
        {
            API ac = new API();
            List<SPWarehouseSalesHeaderLine> warehouseSales = new List<SPWarehouseSalesHeaderLine>();

            if (filter == "" || filter == null)
                filter = "PCPL_Closed_Filter_FilterOnly eq false";
            else
                filter += " and PCPL_Closed_Filter_FilterOnly eq false";
            //filter = "";
            orderby = "";

            var result = ac.GetData1<SPWarehouseSalesHeaderLine>("Warehouse_Sales_Details", filter, 0, 0, orderby);

            if (result.Result.Item1.value.Count > 0)
                warehouseSales = result.Result.Item1.value;

            for (int i = 0; i < warehouseSales.Count; i++)
            {
                string[] strDate = warehouseSales[i].ShipmentDate_SalesHeader.Split('-');
                warehouseSales[i].ShipmentDate_SalesHeader = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehousePurchaseHeaderLine> warehousePurchase = new List<SPWarehousePurchaseHeaderLine>();
            var result1 = ac.GetData1<SPWarehousePurchaseHeaderLine>("Warehouse_Purchase_Details", filter, 0, 0, orderby);

            if (result1.Result.Item1.value.Count > 0)
                warehousePurchase = result1.Result.Item1.value;

            for (int i = 0; i < warehousePurchase.Count; i++)
            {
                string[] strDate = warehousePurchase[i].ShipmentDate_Purchase_Header.Split('-');
                warehousePurchase[i].ShipmentDate_Purchase_Header = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehouseTransferHeaderLine> warehouseTransfer = new List<SPWarehouseTransferHeaderLine>();
            var result2 = ac.GetData1<SPWarehouseTransferHeaderLine>("Warehouse_Transfer_Details", filter, 0, 0, orderby);

            if (result2.Result.Item1.value.Count > 0)
                warehouseTransfer = result2.Result.Item1.value;

            for (int i = 0; i < warehouseTransfer.Count; i++)
            {
                string[] strDate = warehouseTransfer[i].Shipment_Date.Split('-');
                warehouseTransfer[i].Shipment_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehouseList> allWarehouseData = MapToSPWarehouseList(warehouseSales, warehousePurchase, warehouseTransfer);

            return allWarehouseData;
        }

        public List<SPWarehouseList> MapToSPWarehouseList(List<SPWarehouseSalesHeaderLine> salesHeaderLines,
                                                  List<SPWarehousePurchaseHeaderLine> purchaseHeaderLines,
                                                  List<SPWarehouseTransferHeaderLine> transferHeaderLines)
        {
            List<SPWarehouseList> combinedList = new List<SPWarehouseList>();

            // Map SalesHeaderLines
            string isAnyLineWithDropShipmt = "No";
            foreach (var salesLine in salesHeaderLines)
            {
                if (salesLine.TPTPL_Drop_Shipment == "true" && salesLine.PCPLVendorName_SalesLine.ToString() == "")
                {
                    isAnyLineWithDropShipmt = "Yes";
                }
                else
                {
                    isAnyLineWithDropShipmt = "No";
                }

                combinedList.Add(new SPWarehouseList
                {
                    DocumentNo = salesLine.No_SalesHeader,
                    ShipmentDate = salesLine.ShipmentDate_SalesHeader,
                    CustomerVendorNo = "", // or use another logic if needed
                    CustomerVendorName = salesLine.Sell_to_Customer_Name,
                    DocumentType = salesLine.DocumentType_SalesHeader == "Return Order" ? "Sales Return" : "Sales Order",
                    FromAddress = $"{salesLine.Name_Location}, {salesLine.Address_Location}, {salesLine.Address2_Location}, {salesLine.City_Location}, {salesLine.PostCode_Location}, {salesLine.CountryRegionCode_Location}",
                    ToAddress = salesLine.Job_to_Code == "" ? $"{salesLine.ShiptoAddress}, {salesLine.ShiptoAddress2}, {salesLine.ShiptoCity}, {salesLine.ShiptoPostCode}, {salesLine.ShiptoCountryRegionCode}" : $"{salesLine.Job_to_Code},{salesLine.Job_To_Address}, {salesLine.Job_To_Address_2}, {salesLine.Job_To_City}, {salesLine.Job_To_Post_Code}, {salesLine.Job_To_Country_Region_Code}",
                    ItemNo = salesLine.No_SalesLine,
                    ItemDescription = salesLine.Description_SalesLine,
                    Qty = salesLine.Quantity_SalesLine,
                    UOM = salesLine.UnitofMeasure_SalesLine,
                    PackingUOM = salesLine.PCPL_Packing_UOM,
                    PackingStyle = salesLine.PCPLPackingStyleDescription_SalesLine,
                    TransportQty = salesLine.Transport_Quantity_Line,
                    DropShipment = salesLine.TPTPL_Drop_Shipment == "true" ? "Yes" : "No",
                    DropShipmentVendor = salesLine.PCPLVendorName_SalesLine.ToString(),
                    LineNo = salesLine.Line_No_,
                    AcceptedBy = salesLine.PCPLAcceptedbyName_SalesHeader,
                    IncoTerm = salesLine.IncoTerms_SalesHeader,
                    SystemId = salesLine.SystemId,
                    IsAnyLineWithDropShipment = isAnyLineWithDropShipmt,
                    PackingQty = salesLine.PCPL_Packing_Qty_,
                    ToLocation = salesLine.Location_Code,
                    FromLocation = salesLine.Location_Code,
                    FromArea = salesLine.PCPL_Area_Location,
                    ToArea = salesLine.PCPL_Ship_to_Area,



                });
            }

            // Map PurchaseHeaderLines
            foreach (var purchaseLine in purchaseHeaderLines)
            {
                combinedList.Add(new SPWarehouseList
                {
                    DocumentNo = purchaseLine.No_Purchase_Header,
                    ShipmentDate = purchaseLine.ShipmentDate_Purchase_Header,
                    CustomerVendorNo = "",
                    CustomerVendorName = purchaseLine.Name_Location,
                    DocumentType = "Purchase Order",
                    FromAddress = $"{purchaseLine.Name_Location}, {purchaseLine.Address_Location}, {purchaseLine.Address2_Location}, {purchaseLine.CityLocation}, {purchaseLine.PostCodeLocation}, {purchaseLine.CountryRegionCode_Location}",
                    ToAddress = $"{purchaseLine.ShiptoAddress}, {purchaseLine.ShiptoAddress2}, {purchaseLine.ShiptoCity}, {purchaseLine.ShiptoPostCode}, {purchaseLine.ShiptoCountryRegionCode}",
                    ItemNo = purchaseLine.No_Purchase_Line,
                    ItemDescription = purchaseLine.Description_Purchase_Line,
                    Qty = purchaseLine.Quantity_Purchase_Line,
                    UOM = purchaseLine.UnitofMeasure_Purchase_Line,
                    PackingUOM = purchaseLine.PCPL_Packing_UOM,
                    PackingStyle = purchaseLine.PCPLPackingStyleDescription_Purchase_Line,
                    TransportQty = purchaseLine.Transport_Quantity_Line,
                    PackingQty = purchaseLine.PCPL_Packing_Qty_,
                    DropShipment = "",
                    DropShipmentVendor = "",
                    LineNo = purchaseLine.Line_No_,
                    AcceptedBy = purchaseLine.PCPLAcceptedbyName_Purchase_Header,
                    IncoTerm = purchaseLine.IncoTerms_Purchase_Header,
                    SystemId = purchaseLine.SystemId,
                    IsAnyLineWithDropShipment = "",
                    ToLocation = purchaseLine.LocationCode,
                    FromArea = purchaseLine.PCPLBuyfromArea,
                    ToArea = (string)purchaseLine.PCPL_Ship_to_Area,

                });
            }

            // Map TransferHeaderLines
            foreach (var transferLine in transferHeaderLines)
            {
                combinedList.Add(new SPWarehouseList
                {
                    DocumentNo = transferLine.No,
                    ShipmentDate = transferLine.Shipment_Date,
                    CustomerVendorNo = "", // or use another logic if needed
                    CustomerVendorName = "",
                    DocumentType = "Transfer Order",
                    FromAddress = $"{transferLine.TransferfromName}, {transferLine.TransferfromAddress} {transferLine.TransferfromAddress2}, {transferLine.TransferfromCity}, {transferLine.TransferfromPostCode}",
                    ToAddress = $"{transferLine.TransfertoName}, {transferLine.TransfertoAddress} {transferLine.TransfertoAddress2}, {transferLine.TransfertoCity}, {transferLine.TransfertoPostCode}",
                    ItemNo = transferLine.Item_No_,
                    ItemDescription = transferLine.Description_SalesLine,
                    Qty = transferLine.Quantity_SalesLine,
                    UOM = transferLine.UnitofMeasure_SalesLine,
                    PackingUOM = transferLine.PCPL_Packing_UOM,
                    PackingStyle = transferLine.PCPLPackingStyleDescription_SalesLine,
                    TransportQty = transferLine.Transport_Quantity_Line,
                    PackingQty = transferLine.PCPL_Packing_Qty_,
                    DropShipment = "",
                    DropShipmentVendor = "",
                    LineNo = transferLine.Line_No_,
                    AcceptedBy = transferLine.PCPLAcceptedName,
                    IncoTerm = "",
                    SystemId = transferLine.SystemId,
                    IsAnyLineWithDropShipment = "",
                    FromLocation = transferLine.TransferfromCode,
                    ToLocation = transferLine.TransfertoCode,
                    /*FromArea = transferLine.PCPLFromArea,*/
                    ToArea = (string)transferLine.PCPLToArea,

                });
            }

            return combinedList;
        }

        //New
        [Route("GetWarehouseFromNo")]
        public SPWarehouseCard GetWarehouseFromNo(string No, string DocumentType)
        {
            API ac = new API();
            SPWarehouseCard sPWarehouseCard = new SPWarehouseCard();
            var result = (dynamic)null;
            if (DocumentType == "Sales Order" || DocumentType == "Sales Return")
            {
                result = ac.GetData<SPWarehouseSalesHeaderLine>("Warehouse_Sales_Details", "Order_No_Filter_FilterOnly eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    List<WarehouseCardLine> warehouseCardLine = new List<WarehouseCardLine>();

                    int i = 0;
                    //string date = "";

                    foreach (var salesLine in result.Result.Item1.value)
                    {
                        if (i == 0)
                        {
                            sPWarehouseCard.DocumentNo = salesLine.No_SalesHeader;

                            string[] SalesShipDate = salesLine.ShipmentDate_SalesHeader.Split('-');
                            sPWarehouseCard.ShipmentDate = SalesShipDate[2] + '-' + SalesShipDate[1] + '-' + SalesShipDate[0];

                            sPWarehouseCard.CustomerVendorNo = ""; // or use another logic if needed
                            sPWarehouseCard.CustomerVendorName = salesLine.Job_to_Code == "" ? salesLine.Sell_to_Customer_Name : salesLine.Job_to_Code;
                            sPWarehouseCard.DocumentType = salesLine.DocumentType_SalesHeader == "Return Order" ? "Sales Return" : "Sales Order";
                            sPWarehouseCard.FromAddress = $"{salesLine.Name_Location}, {salesLine.Address_Location}, {salesLine.Address2_Location}, {salesLine.City_Location}, {salesLine.PostCode_Location}, {salesLine.CountryRegionCode_Location}";
                            //sPWarehouseCard.ToAddress = $"{salesLine.ShiptoAddress}, {salesLine.ShiptoAddress2}, {salesLine.ShiptoCity}, {salesLine.ShiptoPostCode}, {salesLine.ShiptoCountryRegionCode}";
                            sPWarehouseCard.ToAddress = salesLine.Job_to_Code == "" ? $"{salesLine.ShiptoAddress}, {salesLine.ShiptoAddress2}, {salesLine.ShiptoCity}, {salesLine.ShiptoPostCode}, {salesLine.ShiptoCountryRegionCode}" : $"{salesLine.Job_to_Code},{salesLine.Job_To_Address}, {salesLine.Job_To_Address_2}, {salesLine.Job_To_City}, {salesLine.Job_To_Post_Code}, {salesLine.Job_To_Country_Region_Code}";
                            sPWarehouseCard.FromPincode = salesLine.PostCode_Location;
                            sPWarehouseCard.ToPincode = salesLine.Job_to_Code == "" ? salesLine.ShiptoPostCode : salesLine.Job_To_Post_Code;
                            sPWarehouseCard.DropShipmentVendor = salesLine.PCPLVendorName_SalesLine.ToString();
                            sPWarehouseCard.AcceptedBy = salesLine.PCPLAcceptedbyName_SalesHeader;
                            sPWarehouseCard.IncoTerm = salesLine.IncoTerms_SalesHeader;
                            sPWarehouseCard.FromLocation = salesLine.Location_Code;
                            sPWarehouseCard.ToLocation = salesLine.Location_Code;
                            sPWarehouseCard.SystemId = salesLine.SystemId;
                            sPWarehouseCard.PCPL_Transporter_No = salesLine.PCPL_Transporter_No_;
                            sPWarehouseCard.PCPL_Transporter_Name = salesLine.PCPL_Transporter_Name;
                            sPWarehouseCard.PCPL_Transport_Amount = salesLine.PCPL_Transport_Amount;
                            sPWarehouseCard.LR_RR_No_ = salesLine.LR_RR_No_;
                            sPWarehouseCard.FromArea = salesLine.PCPL_Area_Location;
                            sPWarehouseCard.ToArea = salesLine.PCPL_Ship_to_Area;

                            if (salesLine.LR_RR_Date != "0001-01-01")
                            {
                                string[] slrdate = salesLine.LR_RR_Date.Split('-');
                                sPWarehouseCard.LR_RR_Date = slrdate[2] + '-' + slrdate[1] + '-' + slrdate[0];
                            }
                            else
                            {
                                sPWarehouseCard.LR_RR_Date = "";
                            }

                            sPWarehouseCard.PCPL_Loading_Charges = salesLine.PCPL_Loading_Charges;
                            sPWarehouseCard.PCPL_UnLoading_Charges = salesLine.PCPL_UnLoading_Charges;
                            sPWarehouseCard.VehiclenNo = salesLine.VehiclenNo;
                            sPWarehouseCard.PCPL_Driver_Name = salesLine.PCPL_Driver_Name;
                            sPWarehouseCard.PCPL_Driver_Mobile_No_ = salesLine.PCPL_Driver_Mobile_No_;
                            sPWarehouseCard.PCPL_Driver_License_No_ = salesLine.PCPL_Driver_License_No_;
                            sPWarehouseCard.PCPL_Remarks = salesLine.PCPL_Remarks;
                            sPWarehouseCard.PCPL_Loading_Vendor = salesLine.PCPL_Loading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor = salesLine.PCPL_UnLoading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor_Name = salesLine.PCPL_UnLoading_Vendor_Name;
                            sPWarehouseCard.PCPL_Loading_Vendor_Name = salesLine.PCPL_Loading_Vendor_Name;

                        }
                        warehouseCardLine.Add(new WarehouseCardLine
                        {
                            ItemNo = salesLine.No_SalesLine,
                            ItemDescription = salesLine.Description_SalesLine,
                            Qty = salesLine.Quantity_SalesLine,
                            UOM = salesLine.UnitofMeasure_SalesLine,
                            GrossWeightperunit = salesLine.GrossWeightperunit,
                            PackingUOM = salesLine.PCPL_Packing_UOM,
                            PackingStyle = salesLine.PCPLPackingStyleDescription_SalesLine,
                            TransportQty = salesLine.Transport_Quantity_Line,
                            PackingQty = salesLine.PCPL_Packing_Qty_
                        });

                    }
                    sPWarehouseCard.warehouseCardLines = warehouseCardLine;
                }
            }

            else if (DocumentType == "Purchase Order")
            {
                result = ac.GetData<SPWarehousePurchaseHeaderLine>("Warehouse_Purchase_Details", "Order_No_Filter_FilterOnly eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    List<WarehouseCardLine> warehouseCardLine = new List<WarehouseCardLine>();

                    int i = 0;

                    foreach (var purchaseLine in result.Result.Item1.value)
                    {
                        if (i == 0)
                        {
                            sPWarehouseCard.DocumentNo = purchaseLine.No_Purchase_Header;

                            string[] Purchaseshipdate = purchaseLine.ShipmentDate_Purchase_Header.Split('-');
                            sPWarehouseCard.ShipmentDate = Purchaseshipdate[2] + '-' + Purchaseshipdate[1] + '-' + Purchaseshipdate[0];

                            sPWarehouseCard.CustomerVendorNo = ""; // or use another logic if needed
                            sPWarehouseCard.CustomerVendorName = purchaseLine.Name_Location;
                            sPWarehouseCard.DocumentType = "Purchase Order";
                            sPWarehouseCard.FromAddress = $"{purchaseLine.Name_Location}, {purchaseLine.Address_Location}, {purchaseLine.Address2_Location}, {purchaseLine.CityLocation}, {purchaseLine.PostCodeLocation}, {purchaseLine.CountryRegionCode_Location}";
                            sPWarehouseCard.ToAddress = $"{purchaseLine.ShiptoAddress}, {purchaseLine.ShiptoAddress2}, {purchaseLine.ShiptoCity}, {purchaseLine.ShiptoPostCode}, {purchaseLine.ShiptoCountryRegionCode}";
                            sPWarehouseCard.FromPincode = purchaseLine.PostCodeLocation;
                            sPWarehouseCard.ToPincode = purchaseLine.ShiptoPostCode;
                            sPWarehouseCard.DropShipmentVendor = "";
                            sPWarehouseCard.AcceptedBy = purchaseLine.PCPLAcceptedbyName_Purchase_Header;
                            sPWarehouseCard.IncoTerm = purchaseLine.IncoTerms_Purchase_Header;
                            sPWarehouseCard.FromLocation = "";
                            sPWarehouseCard.FromLocation = purchaseLine.Location_Code;
                            sPWarehouseCard.ToLocation = purchaseLine.LocationCode;
                            sPWarehouseCard.ToLocation = purchaseLine.shiptocode;
                            sPWarehouseCard.SystemId = purchaseLine.SystemId;
                            sPWarehouseCard.PCPL_Transporter_No = purchaseLine.PCPL_Transporter_No_;
                            sPWarehouseCard.PCPL_Transporter_Name = purchaseLine.PCPL_Transporter_Name;
                            sPWarehouseCard.PCPL_Transport_Amount = purchaseLine.PCPL_Transport_Amount;
                            sPWarehouseCard.LR_RR_No_ = purchaseLine.PCPL_LR_RR_No_;
                            sPWarehouseCard.FromArea = purchaseLine.PCPLBuyfromArea;
                            sPWarehouseCard.ToArea = purchaseLine.PCPL_Ship_to_Area;

                            if (purchaseLine.LR_RR_Date != "0001-01-01")
                            {
                                string[] plrdate = purchaseLine.LR_RR_Date.Split('-');
                                sPWarehouseCard.LR_RR_Date = plrdate[2] + '-' + plrdate[1] + '-' + plrdate[0];
                            }
                            else
                            {
                                sPWarehouseCard.LR_RR_Date = "";
                            }

                            sPWarehouseCard.PCPL_Loading_Charges = purchaseLine.PCPL_Loading_Charges;
                            sPWarehouseCard.PCPL_UnLoading_Charges = purchaseLine.PCPL_UnLoading_Charges;
                            sPWarehouseCard.VehiclenNo = purchaseLine.VehiclenNo;
                            sPWarehouseCard.PCPL_Driver_Name = purchaseLine.PCPL_Driver_Name;
                            sPWarehouseCard.PCPL_Driver_Mobile_No_ = purchaseLine.PCPL_Driver_Mobile_No_;
                            sPWarehouseCard.PCPL_Driver_License_No_ = purchaseLine.PCPL_Driver_License_No_;
                            sPWarehouseCard.PCPL_Remarks = purchaseLine.PCPL_Remarks;
                            sPWarehouseCard.PCPL_Loading_Vendor = purchaseLine.PCPL_Loading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor = purchaseLine.PCPL_UnLoading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor_Name = purchaseLine.PCPL_UnLoading_Vendor_Name;
                            sPWarehouseCard.PCPL_Loading_Vendor_Name = purchaseLine.PCPL_Loading_Vendor_Name;
                            //sPWarehouseCard.PCPL_Packing_UOM = purchaseLine.PCPL_Packing_UOM;

                        }
                        warehouseCardLine.Add(new WarehouseCardLine
                        {
                            ItemNo = purchaseLine.No_Purchase_Line,
                            ItemDescription = purchaseLine.Description_Purchase_Line,
                            Qty = purchaseLine.Quantity_Purchase_Line,
                            UOM = purchaseLine.UnitofMeasure_Purchase_Line,
                            GrossWeightperunit = purchaseLine.GrossWeightperunit,
                            PackingUOM = purchaseLine.PCPL_Packing_UOM,
                            PackingStyle = purchaseLine.PCPLPackingStyleDescription_Purchase_Line,
                            TransportQty = purchaseLine.Transport_Quantity_Line,
                            PackingQty = purchaseLine.PCPL_Packing_Qty_
                        });

                    }
                    sPWarehouseCard.warehouseCardLines = warehouseCardLine;
                }
            }

            else if (DocumentType == "Transfer Order")
            {
                result = ac.GetData<SPWarehouseTransferHeaderLine>("Warehouse_Transfer_Details", "Order_No_Filter_FilterOnly eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    List<WarehouseCardLine> warehouseCardLine = new List<WarehouseCardLine>();

                    int i = 0;

                    foreach (var transferLine in result.Result.Item1.value)
                    {
                        if (i == 0)
                        {
                            sPWarehouseCard.DocumentNo = transferLine.No;

                            string[] Transfershipdate = transferLine.Shipment_Date.Split('-');
                            sPWarehouseCard.ShipmentDate = Transfershipdate[2] + '-' + Transfershipdate[1] + '-' + Transfershipdate[0];

                            sPWarehouseCard.CustomerVendorNo = ""; // or use another logic if needed
                            sPWarehouseCard.CustomerVendorName = "";
                            sPWarehouseCard.DocumentType = "Transfer Order";
                            sPWarehouseCard.FromAddress = $"{transferLine.TransferfromAddress}, {transferLine.TransferfromAddress2}, {transferLine.TransferfromCity}, {transferLine.TransferfromPostCode}, {transferLine.TransferfromCounty}"; //$"{transferLine.Name_Location}, {transferLine.Address_Location}, {transferLine.Address2_Location}, {transferLine.CityLocation}, {transferLine.PostCodeLocation}, {transferLine.CountryRegionCode_Location}";
                            sPWarehouseCard.ToAddress = $"{transferLine.TransfertoAddress}, {transferLine.TransfertoAddress2}, {transferLine.TransfertoCity}, {transferLine.TransfertoPostCode}, {transferLine.TransfertoCounty}";
                            sPWarehouseCard.FromPincode = transferLine.TransferfromPostCode;
                            sPWarehouseCard.ToPincode = transferLine.TransfertoPostCode;
                            sPWarehouseCard.DropShipmentVendor = "";
                            sPWarehouseCard.AcceptedBy = transferLine.PCPLAcceptedName;
                            sPWarehouseCard.IncoTerm = "";
                            sPWarehouseCard.FromLocation = transferLine.TransferfromCode;
                            sPWarehouseCard.ToLocation = transferLine.TransfertoCode;
                            sPWarehouseCard.SystemId = transferLine.SystemId;
                            sPWarehouseCard.PCPL_Transporter_No = transferLine.PCPL_Transporter_No_;
                            sPWarehouseCard.PCPL_Transporter_Name = transferLine.PCPL_Transporter_Name;
                            sPWarehouseCard.PCPL_Transport_Amount = transferLine.PCPL_Transport_Amount;
                            sPWarehouseCard.LR_RR_No_ = transferLine.LR_RR_No_;
                            sPWarehouseCard.FromArea = transferLine.PCPLFromArea;
                            sPWarehouseCard.ToArea = transferLine.PCPLToArea;


                            if (transferLine.LR_RR_Date != "0001-01-01")
                            {
                                string[] tlrdate = transferLine.LR_RR_Date.Split('-');
                                sPWarehouseCard.LR_RR_Date = tlrdate[2] + '-' + tlrdate[1] + '-' + tlrdate[0];
                            }
                            else
                            {
                                sPWarehouseCard.LR_RR_Date = "";
                            }

                            sPWarehouseCard.PCPL_Loading_Charges = transferLine.PCPL_Loading_Charges;
                            sPWarehouseCard.PCPL_UnLoading_Charges = transferLine.PCPL_UnLoading_Charges;
                            sPWarehouseCard.VehiclenNo = transferLine.VehiclenNo;
                            sPWarehouseCard.PCPL_Driver_Name = transferLine.PCPL_Driver_Name;
                            sPWarehouseCard.PCPL_Driver_Mobile_No_ = transferLine.PCPL_Driver_Mobile_No_;
                            sPWarehouseCard.PCPL_Driver_License_No_ = transferLine.PCPL_Driver_License_No_;
                            sPWarehouseCard.PCPL_Remarks = transferLine.PCPL_Remarks;
                            sPWarehouseCard.PackingQty = transferLine.PCPL_Packing_Qty_;
                            sPWarehouseCard.PCPL_Loading_Vendor = transferLine.PCPL_Loading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor = transferLine.PCPL_UnLoading_Vendor;
                            sPWarehouseCard.PCPL_UnLoading_Vendor_Name = transferLine.PCPL_UnLoading_Vendor_Name;
                            sPWarehouseCard.PCPL_Loading_Vendor_Name = transferLine.PCPL_Loading_Vendor_Name;

                        }
                        warehouseCardLine.Add(new WarehouseCardLine
                        {
                            ItemNo = transferLine.Item_No_,
                            ItemDescription = transferLine.Description_SalesLine,
                            Qty = transferLine.Quantity_SalesLine,
                            UOM = transferLine.UnitofMeasure_SalesLine,
                            GrossWeightperunit = transferLine.GrossWeightperunit,
                            PackingUOM = transferLine.PCPL_Packing_UOM,
                            PackingStyle = transferLine.PCPLPackingStyleDescription_SalesLine,
                            TransportQty = transferLine.Transport_Quantity_Line,
                            PackingQty = transferLine.PCPL_Packing_Qty_


                        });

                    }
                    sPWarehouseCard.warehouseCardLines = warehouseCardLine;
                }
            }

            return sPWarehouseCard;
        }

        [Route("GetTransporterRate")]
        public List<TransporterRateCard> GetTransporterRate(string FromPincode, string ToPincode, string PackingUOMs, string TransporterNo)
        {
            API ac = new API();
            List<TransporterRateCard> transporterRateCards = new List<TransporterRateCard>();

            string filter = "";
            string UOMfilter = "";
            string[] packinguomlist = PackingUOMs.Trim().Split(',');

            if (packinguomlist.Length > 0)
            {
                for (int i = 0; i < packinguomlist.Length; i++)
                {
                    if (packinguomlist[i] != "")
                        UOMfilter += "UOM eq '" + packinguomlist[i] + "' or ";
                }
                UOMfilter += " UOM eq ''";
                UOMfilter = "(" + UOMfilter + ")";
            }
            else
            {
                UOMfilter = "UOM eq ''";
            }

            string latestRateFilter = "Latest_Rate eq true";

            if (string.IsNullOrEmpty(TransporterNo))
            {
                // TransporterNo Empty + Pincode + TransporterNo + LatestRate
                filter += $"From_Post_Code eq '{FromPincode}' and To_Post_Code eq '{ToPincode}' and {latestRateFilter}";
            }
            else
            {
                // TransporterNo + Pincode + TransporterNo + LatestRate
                filter = $"From_Post_Code eq '{FromPincode}' and To_Post_Code eq '{ToPincode}' and Transporter_No eq '{TransporterNo}' and {latestRateFilter}";
            }

            var result = ac.GetData1<TransporterRateCard>("Transporter_Rate_Details", filter, 0, 0, "Rate_for_Standard_Weight asc");

            if (result.Result.Item1 != null)
                transporterRateCards = result.Result.Item1.value;

            return transporterRateCards;
        }


        [Route("GetContactOfCompany")]
        public List<WarehouseCardDrivers> GetContactOfCompany(string No)
        {
            API ac = new API();
            List<WarehouseCardDrivers> warehouseCardDrivers = new List<WarehouseCardDrivers>();


            var result = ac.GetData<WarehouseCardDrivers>("ContactDotNetAPI", "Company_No eq '" + No + "' and Type eq 'Person'");
            if (result.Result.Item1.value.Count > 0)
                warehouseCardDrivers = result.Result.Item1.value;

            return warehouseCardDrivers;
        }

        [Route("GetLoadingUnloadingCharge")]
        public WarehouseLoadingUnloading GetLoadingUnloadingCharge(string filter)
        {
            API ac = new API();
            WarehouseLoadingUnloading warehouseLoadingUnloading = new WarehouseLoadingUnloading();

            var result = ac.GetData<WarehouseLoadingUnloading>("LocationCardDotNetAPI", filter);

            if (result.Result.Item1.value.Count > 0)
                warehouseLoadingUnloading = result.Result.Item1.value[0];

            return warehouseLoadingUnloading;
        }

        [HttpPost]
        [Route("ClosedTaskOfWarehouse")]
        public bool ClosedTaskOfWarehouse(string doctype, string transporterCode, string systemids, string lrno, string lrdate, string drivername, string driverlicenseno, string drivermobileno, string vehicleno, decimal loadingcharges, decimal unloadingcharges, decimal transporteramount, string remarks, bool isclosed, bool selectedExisting, string vendorcompanyNo, string loadingvendor, string unloadingvendor)
        {
            bool flag = false;

            if (selectedExisting == false && drivername != "")
            {
                AddNewDriver(vendorcompanyNo, drivername, drivermobileno, driverlicenseno);
            }

            //DateTime LRDate = Convert.ToDateTime(lrdate);
            //lrdate = LRDate.ToString("yyyy-MM-dd");
            lrdate = dd_MM_yyyytoyyyy_MM_dd(lrdate);

            SPWarehouseClose requestWarehouse = new SPWarehouseClose
            {
                transporterCode = transporterCode,
                systemids = systemids,
                lrno = lrno.ToUpper(),
                lrdate = lrdate,
                drivername = drivername,
                driverlicenseno = driverlicenseno.ToUpper(),
                drivermobileno = drivermobileno,
                vehicleno = vehicleno.ToUpper(),
                loadingcharges = loadingcharges,
                unloadingcharges = unloadingcharges,
                transporteramount = transporteramount,
                remarks = remarks,
                isclosed = isclosed,
                loadingvendor = loadingvendor,
                unloadingvendor = unloadingvendor
                //loadingvendorname = loadingvendorname,
                //unloadingvendorname = unloadingvendorname

            };
            var resWarehouse = new SPWarehouseSalesAcceptResponse();
            dynamic result = null;
            string APIname = "";

            if (doctype == "Sales Order" || doctype == "Sales Return")
            {
                APIname = "APIMngt_updateTransporterdetails";
            }
            else if (doctype == "Purchase Order")
            {
                APIname = "APIMngt_updateTransporterdetailsPurchase";
            }
            else if (doctype == "Transfer Order")
            {
                APIname = "APIMngt_updateTransporterdetailsTransfer";
            }
            result = PostClosedTask(APIname, requestWarehouse, resWarehouse);

            if (result.Result.Item1 != null)
            {
                flag = Convert.ToBoolean(result.Result.Item1.value);
            }

            return flag;
        }

        public string dd_MM_yyyytoyyyy_MM_dd(string dd_MM_yyyyDate)
        {
            DateTime yyyy_MM_ddDate;
            bool success = DateTime.TryParseExact(dd_MM_yyyyDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out yyyy_MM_ddDate);

            if (success)
            {
                Console.WriteLine("TryParseExact date: " + yyyy_MM_ddDate);
            }
            else
            {
                yyyy_MM_ddDate = DateTime.ParseExact(dd_MM_yyyyDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                Console.WriteLine("ParseExact date: " + yyyy_MM_ddDate);
            }

            return yyyy_MM_ddDate.ToString("yyyy-MM-dd");
        }

        public bool AddNewDriver(string CompanyNo, string Name, string Mobile_Phone_No, string LicenseNumber)
        {
            bool flag = false;
            SPDriver requestDriver = new SPDriver();

            requestDriver.Company_No = CompanyNo;
            requestDriver.Name = Name;
            requestDriver.Type = "Person";
            requestDriver.Mobile_Phone_No = Mobile_Phone_No;
            requestDriver.Job_Title = "Driver";
            requestDriver.Registration_Number = LicenseNumber;

            var ac = new API();
            errorDetails ed = new errorDetails();
            SPDriver responseDriver = new SPDriver();
            var result = (dynamic)null;

            result = ac.PostItem("ContactDotNetAPI", requestDriver, responseDriver);

            if (result.Result.Item1 != null)
            {
                responseDriver = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        public async Task<(SPWarehouseSalesAcceptResponse, errorDetails)> PostClosedTask<SPWarehouseClose>(string apiendpoint, SPWarehouseClose requestModel, SPWarehouseSalesAcceptResponse responseModel)
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
                    responseModel = res.ToObject<SPWarehouseSalesAcceptResponse>();

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

        [HttpPost]
        [Route("AcceptTaskOfWarehouse")]
        public bool AcceptTaskOfWarehouse(string systemids, string AcceptedBy)
        {
            bool flag = true;

            bool soflag = true;
            bool poflag = true;
            bool toflag = true;

            string[] systemidwiddocumenttype = systemids.Split('|');

            string sosystemids = "";
            string posystemids = "";
            string tosystemids = "";

            for (
                int i = 0; i < systemidwiddocumenttype.Length; i++)
            {
                string[] systemidanddoctype = systemidwiddocumenttype[i].Split(',');
                if (systemidanddoctype.Length == 2)
                {
                    if (systemidanddoctype[1].ToString() == "Sales Order" || systemidanddoctype[1].ToString() == "Sales Return")
                        sosystemids += systemidanddoctype[0] + "|";
                    else if (systemidanddoctype[1].ToString() == "Purchase Order")
                        posystemids += systemidanddoctype[0] + "|";
                    else if (systemidanddoctype[1].ToString() == "Transfer Order")
                        tosystemids += systemidanddoctype[0] + "|";
                }
            }

            if (sosystemids.Length > 0)
            {
                var resWarehouseSales = new SPWarehouseSalesAcceptResponse();
                dynamic result = null;
                SPWarehouseSalesAccept requestWarehouseSales = new SPWarehouseSalesAccept
                {
                    systemids = sosystemids.TrimEnd('|'),
                    salesperson = AcceptedBy
                };
                result = PostCodeUnit("CodeunitAPIMgmt_updateacceptedby", requestWarehouseSales, resWarehouseSales);
                if (result.Result.Item1 != null)
                {
                    soflag = Convert.ToBoolean(result.Result.Item1.value);
                }
            }

            if (posystemids.Length > 0)
            {
                var resWarehouseSales = new SPWarehouseSalesAcceptResponse();
                dynamic result = null;
                SPWarehouseSalesAccept requestWarehouseSales = new SPWarehouseSalesAccept
                {
                    systemids = posystemids.TrimEnd('|'),
                    salesperson = AcceptedBy
                };
                result = PostCodeUnit("CodeunitAPIMgmt_updateacceptedbyPurchase", requestWarehouseSales, resWarehouseSales);
                if (result.Result.Item1 != null)
                {
                    soflag = Convert.ToBoolean(result.Result.Item1.value);
                }
            }

            if (tosystemids.Length > 0)
            {
                var resWarehouseSales = new SPWarehouseSalesAcceptResponse();
                dynamic result = null;
                SPWarehouseSalesAccept requestWarehouseSales = new SPWarehouseSalesAccept
                {
                    systemids = tosystemids.TrimEnd('|'),
                    salesperson = AcceptedBy
                };
                result = PostCodeUnit("CodeunitAPIMgmt_updateacceptedbyTransfer", requestWarehouseSales, resWarehouseSales);
                if (result.Result.Item1 != null)
                {
                    soflag = Convert.ToBoolean(result.Result.Item1.value);
                }
            }

            if (soflag == false || poflag == false || toflag == false)
            {
                flag = false;
            }

            return flag;
        }

        [Route("GetCompanyNoByVendorNo")]
        public string GetCompanyNoByVendorNo(string vendorno)
        {
            API ac = new API();

            string companyno = "";
            var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "No eq '" + vendorno + "'");

            if (resultCustomerNo.Result.Item1.value.Count > 0)
            {
                companyno = resultCustomerNo.Result.Item1.value[0].Contact_No;
            }
            return companyno;
        }

        [Route("GetVendorForDDL")]
        public List<WarehouseVendor> GetVendorForDDL(string ItemNo)
        {
            API ac = new API();
            List<WarehouseVendor> vendor = new List<WarehouseVendor>();

            var result = ac.GetData<WarehouseVendor>("ItemVendor", "Item_No eq '" + ItemNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                vendor = result.Result.Item1.value;

            return vendor;
        }

        [Route("GetWarehouseClosedTaskAll")]
        public List<SPWarehouseList> GetWarehouseClosedTaskAll(string orderby, string filter)
        {
            API ac = new API();
            List<SPWarehouseSalesHeaderLine> warehouseSales = new List<SPWarehouseSalesHeaderLine>();

            if (filter == "" || filter == null)
                filter = "PCPL_Closed_Filter_FilterOnly eq true";
            else
                filter += " and PCPL_Closed_Filter_FilterOnly eq true";
            //filter = "";
            orderby = "";

            var result = ac.GetData1<SPWarehouseSalesHeaderLine>("Warehouse_Sales_Details", filter, 0, 0, orderby);

            if (result.Result.Item1.value.Count > 0)
                warehouseSales = result.Result.Item1.value;

            for (int i = 0; i < warehouseSales.Count; i++)
            {
                string[] strDate = warehouseSales[i].ShipmentDate_SalesHeader.Split('-');
                warehouseSales[i].ShipmentDate_SalesHeader = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehousePurchaseHeaderLine> warehousePurchase = new List<SPWarehousePurchaseHeaderLine>();
            var result1 = ac.GetData1<SPWarehousePurchaseHeaderLine>("Warehouse_Purchase_Details", filter, 0, 0, orderby);

            if (result1.Result.Item1.value.Count > 0)
                warehousePurchase = result1.Result.Item1.value;

            for (int i = 0; i < warehousePurchase.Count; i++)
            {
                string[] strDate = warehousePurchase[i].ShipmentDate_Purchase_Header.Split('-');
                warehousePurchase[i].ShipmentDate_Purchase_Header = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehouseTransferHeaderLine> warehouseTransfer = new List<SPWarehouseTransferHeaderLine>();
            var result2 = ac.GetData1<SPWarehouseTransferHeaderLine>("Warehouse_Transfer_Details", filter, 0, 0, orderby);

            if (result2.Result.Item1.value.Count > 0)
                warehouseTransfer = result2.Result.Item1.value;

            for (int i = 0; i < warehouseTransfer.Count; i++)
            {
                string[] strDate = warehouseTransfer[i].Shipment_Date.Split('-');
                warehouseTransfer[i].Shipment_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPWarehouseList> allWarehouseData = MapToSPWarehouseList(warehouseSales, warehousePurchase, warehouseTransfer);

            return allWarehouseData;
        }

        [HttpPost]
        [Route("SaveDropShipmentVendor")]
        public string SaveDropShipmentVendor(string documentno, string lineno, string vendorno)
        {
            string msg = "";

            var resWarehouseSales = new SPWarehouseSalesAcceptResponse();
            dynamic result = null;
            var requestWarehouseSales = new
            {
                documentno = documentno,
                lineno = lineno,
                vendorno = vendorno
            };
            result = PostCodeUnit("APIMngt_CreatePOForProductIncharge", requestWarehouseSales, resWarehouseSales);
            if (result.Result.Item1 != null)
            {
                msg = result.Result.Item1.value;
            }

            return msg;
        }

        [Route("GetloadingAndUnloadingDropDown")]
        public List<UnloadingAndLoading> GetloadingAndUnloadingDropDown(string prefix)
        {
            API ac = new API();
            List<UnloadingAndLoading> unloadingAndloading = new List<UnloadingAndLoading>();
            string filter = "startswith(Name,'" + prefix.ToUpper() + "')";

            filter += " and Transporter eq true";


            var result = ac.GetData<UnloadingAndLoading>("VendorDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
                unloadingAndloading = result.Result.Item1.value;

            return unloadingAndloading;
        }
        #endregion
    }
}
