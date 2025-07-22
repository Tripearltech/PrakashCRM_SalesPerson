using ClosedXML.Excel;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPWarehouseController : Controller
    {
        // GET: Warehouse
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 2:
                    orderByField = "Description " + orderDir;
                    break;
                case 3:
                    orderByField = "Quantity " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                case 6:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetWarehouseSales?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouse>>(data);
            }

            DataTable dt = ToDataTable(warehouseSales);

            //Name of File  
            string fileName = "WarehouseSalesIncomingTask.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);

                using (var exportData = new MemoryStream())
                {
                    wb.SaveAs(exportData);
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    exportData.WriteTo(file);
                    file.Close();
                }
            }

            return Json(new { fileName = fileName, errorMessage = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        #region IncomingTask

        //public ActionResult WarehouseList()
        //{
        //    return View();
        //}

        public async Task<JsonResult> GetWarehouseIncomingTaskListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetWarehouseSales?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouse>>(data);
            }

            return Json(warehouseSales, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllSalesLineForPopup(string Document_No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            apiUrl = apiUrl + "GetAllSalesLine?Document_No=" + Document_No;

            HttpClient client = new HttpClient();
            List<SPWarehouseSalesLine> warehouseSalesLine = new List<SPWarehouseSalesLine>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSalesLine = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouseSalesLine>>(data);
            }

            return Json(warehouseSalesLine, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region AcceptedTask

        public ActionResult AcceptedTaskList()
        {
            return View();
        }

        public async Task<JsonResult> GetWarehouseAcceptedTaskListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetWarehouseSalesAcceptedTask?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouse>>(data);
            }

            return Json(warehouseSales, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllTransporterForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/GetAllTransporterForDDL";

            HttpClient client = new HttpClient();
            List<Transporter> transporter = new List<Transporter>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                transporter = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Transporter>>(data);
            }

            return Json(transporter, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetTransporterDetailByVendorNo(string vendorno)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/GetAllTransporterForDDL?No=" + vendorno;

            HttpClient client = new HttpClient();
            List<Transporter> transporter = new List<Transporter>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                transporter = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Transporter>>(data);
            }

            return Json(transporter, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ClosedTask

        public ActionResult ClosedTaskList()
        {
            return View();
        }

        public async Task<JsonResult> GetWarehouseClosedTaskListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetWarehouseSalesClosedTask?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouse>>(data);
            }

            return Json(warehouseSales, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region New_WarehouseCode

        public ActionResult WarehouseList()
        {
            return View();
        }

        public async Task<JsonResult> GetWarehouseTaskAll(int orderBy, string orderDir, string filter, string documenttype = "")
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Document_No " + orderDir;
                    break;
                case 2:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "PCPL_Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            orderByField = orderByField + ",Document_No asc";
            apiUrl = apiUrl + "GetWarehouseTaskAll?orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouseList> warehouseAll = new List<SPWarehouseList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            List<SPWarehouseList> warehouseAllReturn = new List<SPWarehouseList>();
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseAll = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouseList>>(data);
                warehouseAllReturn = warehouseAll.OrderBy(x => x.ShipmentDate).ToList();
            }

            return Json(warehouseAll, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WarehouseCard(string No, string DocumentType)
        {
            SPWarehouseCard sPWarehouseCard = new SPWarehouseCard();

            if (No != "" && DocumentType != "")
            {
                Task<SPWarehouseCard> task = Task.Run<SPWarehouseCard>(async () => await GetWarehouseFromNo(No, DocumentType));
                sPWarehouseCard = task.Result;
            }

            if (sPWarehouseCard != null)
                return View(sPWarehouseCard);
            else
                return View(new SPWarehouseCard());
        }
       
        public async Task<SPWarehouseCard> GetWarehouseFromNo(string No, string DocumentType)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            apiUrl = apiUrl + "GetWarehouseFromNo?No=" + No + "&DocumentType=" + DocumentType;

            HttpClient client = new HttpClient();
            SPWarehouseCard warehouseCardSales = new SPWarehouseCard();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseCardSales = Newtonsoft.Json.JsonConvert.DeserializeObject<SPWarehouseCard>(data);
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
            }
            return warehouseCardSales;
        }

        public async Task<JsonResult> GetTransporterRate(string FromPincode, string ToPincode, string PackingUOMs, string TransporterNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            apiUrl = apiUrl + "GetTransporterRate?FromPincode=" + FromPincode + "&ToPincode=" + ToPincode + "&PackingUOMs=" + PackingUOMs + "&TransporterNo=" + TransporterNo;

            HttpClient client = new HttpClient();
            List<TransporterRateCard> salesInvoiceDetails = new List<TransporterRateCard>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesInvoiceDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TransporterRateCard>>(data);

            }

            return Json(salesInvoiceDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetVendorDrivers(string vendorNo, string numberType)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            HttpClient client = new HttpClient();
            List<WarehouseCardDrivers> warehouseCardDrivers = new List<WarehouseCardDrivers>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            if (numberType == "company")
            {
                apiUrl = apiUrl + "GetContactOfCompany?No=" + vendorNo;

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    warehouseCardDrivers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WarehouseCardDrivers>>(data);
                }
            }
            else
            {
                string apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";
                HttpClient client1 = new HttpClient();
                client1.BaseAddress = new Uri(apiUrl);
                client1.DefaultRequestHeaders.Accept.Clear();
                client1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiUrl1 = apiUrl1 + "GetCompanyNoByVendorNo?vendorno=" + vendorNo;
                HttpResponseMessage response1 = await client1.GetAsync(apiUrl1);
                string vendorcompanyno = "";
                if (response1.IsSuccessStatusCode)
                {
                    vendorcompanyno = await response1.Content.ReadAsStringAsync();
                }
                vendorcompanyno = vendorcompanyno.Replace("\"", "");
                if (vendorcompanyno != "" && vendorcompanyno != null)
                {
                    apiUrl = apiUrl + "GetContactOfCompany?No=" + vendorcompanyno.Replace("\"", "");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        warehouseCardDrivers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WarehouseCardDrivers>>(data);
                    }
                }
            }

            return Json(warehouseCardDrivers, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetVendorForDDL(string ItemNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/GetVendorForDDL?ItemNo=" + ItemNo; //+ "&LineNo=" + LineNo + "&DocumentNo=" + DocumentNo;

            HttpClient client = new HttpClient();
            List<WarehouseVendor> vendor = new List<WarehouseVendor>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                vendor = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WarehouseVendor>>(data);
            }

            return Json(vendor, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WarehouseClosedList()
        {
            return View();
        }
        
        public async Task<JsonResult> GetWarehouseClosedTaskAll(int orderBy, string orderDir, string filter, string documenttype = "")
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPWarehouse/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Document_No " + orderDir;
                    break;
                case 2:
                    orderByField = "Sell_to_Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "PCPL_Sell_to_Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Location_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "Shipment_Date " + orderDir;
                    break;
                default:
                    orderByField = "Shipment_Date asc";
                    break;
            }

            orderByField = orderByField + ",Document_No asc";
            apiUrl = apiUrl + "GetWarehouseClosedTaskAll?orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPWarehouseList> warehouseAll = new List<SPWarehouseList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseAll = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouseList>>(data);
            }

            return Json(warehouseAll, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WarehouseClosedCard(string No, string DocumentType)
        {
            SPWarehouseCard sPWarehouseCard = new SPWarehouseCard();

            if (No != "" && DocumentType != "")
            {
                Task<SPWarehouseCard> task = Task.Run<SPWarehouseCard>(async () => await GetWarehouseFromNo(No, DocumentType));
                sPWarehouseCard = task.Result;
            }

            if (sPWarehouseCard != null)
                return View(sPWarehouseCard);
            else
                return View(new SPWarehouseCard());
        }

        #endregion
    }
}