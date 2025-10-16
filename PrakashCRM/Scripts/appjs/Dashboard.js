﻿/* start pagination filter code */

var apiUrl = $('#getServiceApiUrl').val() + 'SPDashboard/';

var filter = "";
var orderBy = 3;
var orderDir = "asc";

$(document).ready(function () {

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });

    BindDailyVisitsDetails();
    BindFeedback();
    BindMarketUpdate();
    BindWarehouseSalesAcceptTask();
    BindWarehousePurchaseAcceptTask();
    BindNonPerformingList();
    BindTodaylist();
    BindWeeklytasklist();
    BindMonthlyTask();
    //BindSalespersonData();
    //BindSupportSP();
    //BindGProductData();

    BindCombinedData();

    SetCurrentDate();

    $('.btn-close').click(function () {
        $('#modalSalesLineList').css('display', 'none');
        $('#modalPurchaseLineList').css('display', 'none');
    });

});

function BindDailyVisitsDetails() {
    $.ajax({
        url: '/SPDashboard/DailyVisitsDetails',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblDailyVisitsDetails').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.Date + "</td><td>" + item.Visit_Name + "</td><td>" + item.Visit_SubType_Name + "</td><td>" + item.Contact_Company_Name + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblDailyVisitsDetails').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}

function BindFeedback() {

    $.ajax(
        {
            url: '/SPDashboard/GetAllFeedback',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataListFeedback')) {
                    $('#dataListFeedback').DataTable().destroy();
                }
                $('#tableFeedbackBody').empty();
                var rowData = "";

                if (data.length > 0) {
                    $.each(data, function (index, item) {
                        rowData += "<tr><td>" + item.Company_Name + "</td><td>" + item.Overall_Rating + "</td></tr>";
                        // loop and do whatever with data
                    });

                }
                else {
                    rowData = "<tr><td style='text-align:center;'>No Records Found</td><td></td></tr>";
                }
                $('#tableFeedbackBody').append(rowData);

                dtable = $('#dataListFeedback').DataTable({
                    retrieve: true,
                    filter: false,
                    paging: false,
                    info: false,
                    responsive: true,
                    ordering: false,
                });

            },
            error: function (data1) {
                alert(data1);
            }
        }

    );
}

function BindMarketUpdate() {
    var salesperson = $('#getLoggedInUserNo').val();

    $.ajax({
        url: '/SPDashboard/GetMarketUpdateListData?_=' + new Date().getTime(),
        type: 'GET',
        cache: false,
        contentType: 'application/json',
        success: function (data) {
            if ($.fn.dataTable.isDataTable('#dataListMarketUpdate')) {
                $('#dataListMarketUpdate').DataTable().clear().destroy();
            }
            $('#tableMarketUpdate').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    if (salesperson == item.Employee_Code) {
                        rowData += "<tr><td><a onclick='EditMarketUpdate(\"" + item.Entry_No + "\",this)'><i class='bx bxs-edit'></i></a></td><td>" + item.Update_Date + "</td><td>" + item.Update + "</td><td>" + item.Employee_Name + "</td></tr>";
                    } else {
                        rowData += "<tr><td></td><td>" + item.Update_Date + "</td><td>" + item.Update + "</td><td>" + item.Employee_Name + "</td></tr>";
                    }
                });
            } else {
                rowData = "<tr><td></td><td></td><td style='text-align:center;'>No Records Found</td><td></td></tr>";
            }
            $('#tableMarketUpdate').append(rowData);

            dtable = $('#dataListMarketUpdate').DataTable({
                retrieve: true,
                filter: false,
                paging: false,
                info: false,
                responsive: true,
                ordering: false,
            });
        },
        error: function () {
            alert("Error loading Market Updates");
        }
    });
}
function SetCurrentDate() {
    var today = new Date();
    var day = ('0' + today.getDate()).slice(-2);
    var month = ('0' + (today.getMonth() + 1)).slice(-2);
    var year = today.getFullYear();

    $('#txtMUDate').val(`${year}-${month}-${day}`);
}
function AddMarketUpdate() {
    var entryno = $('#hfEntryNo').val();
    var updateDate = $('#txtMUDate').val();
    var update = $('#txtMarketUpdate').val();
    var salesPerson = $('#getLoggedInUserNo').val();

    if (updateDate != "" && update != "") {
        $.post(
            apiUrl + 'AddMarketUpdate?Entry_No=' + entryno +
            '&Update=' + encodeURIComponent(update) +
            '&Update_Date=' + updateDate +
            '&Employee_Code=' + salesPerson +
            '&_=' + new Date().getTime(),

            function (data) {
                if (data) {
                    $('#modalMarketUpdate').css('display', 'none');

                    var actionMsg = (entryno == 0)
                        ? "Market Update Added Successfully."
                        : "Market Update Updated Successfully.";

                    ShowActionMsg(actionMsg);
                    BindMarketUpdate();
                    // ✅ form fields clear after save
                    $('#txtMarketUpdate').val('');
                }
            }
        );
    } else {
        ShowErrMsg("Please Fill data.");
    }
}
function EditMarketUpdate(entryNo, rowobj) {
    //debugger;

    var row = rowobj.closest("tr");
    //alert(row.cells[2].innerHTML);
    $('#hfEntryNo').val(entryNo);
    $('#txtMUDate').val(row.cells[1].innerHTML);
    $('#txtMarketUpdate').val(row.cells[2].innerHTML);

    $('#modalMarketUpdate').css('display', 'block');
}

function ShowErrMsg(errMsg) {

    Lobibox.notify('error', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        delayIndicator: false,
        icon: 'bx bx-x-circle',
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: errMsg
    });

}

function ShowActionMsg(actionMsg) {

    Lobibox.notify('success', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        icon: 'bx bx-check-circle',
        delayIndicator: false,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: actionMsg
    });

}

function BindWarehouseSalesAcceptTask() {

    $.ajax(
        {
            url: '/SPDashboard/GetWarehouseSalesAcceptTaskLis',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataListSalesAcceptTask')) {
                    $('#dataListSalesAcceptTask').DataTable().destroy();
                }
                $('#tableSalesAcceptTask').empty();
                var rowData = "";

                if (data.length > 0) {
                    $.each(data, function (index, item) {
                        rowData += "<tr><td><a onclick='ShowSalesLines(\"" + item.No + "\")'><i class='bx bx-show'></i></a></td><td>" + item.No + "</td><td>" + item.Sell_to_Customer_Name + "</td><td>" + item.Location_Code + "</td><td>" + item.Shipment_Date + "</td><td>" + item.Sell_to_Customer_No + "</td></tr>";// 
                        // loop and do whatever with data
                    });
                }
                else {
                    rowData = "<tr><td></td><td></td><td></td><td></td><td style='text-align:center;'>No Records Found</td><td></td></tr>";
                }
                $('#tableSalesAcceptTask').append(rowData);

                dtable = $('#dataListSalesAcceptTask').DataTable({
                    retrieve: true,
                    filter: false,
                    paging: false,
                    info: false,
                    responsive: true,
                    ordering: false,
                });

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function ShowSalesLines(Document_No) {

    $.ajax(
        {
            url: '/SPDashboard/GetAllSalesLineForPopup?Document_No=' + Document_No,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tbSalesLines').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        var rowData = "<tr><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Qty_to_Ship + "</td><td>" + item.PCPL_Packing_UOM + "</td><td>" + item.PCPL_Packing_Style_Description + "</td><td>" + item.Location_Code + "</td></tr>";
                        $('#tbSalesLines').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                    $('#tbSalesLines').append(rowData);
                }

                $('#modalSalesLineList').css('display', 'block');
                $('.modal-title').text('Sales Line');
                $('#dvSalesLines').css('display', 'block');

            },
            error: function () {
                alert("error");
            }
        }
    );
}

function BindWarehousePurchaseAcceptTask() {

    $.ajax(
        {
            url: '/SPDashboard/GetWarehousePurchaseAcceptTaskList',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataListPurchaseAcceptTask')) {
                    $('#dataListPurchaseAcceptTask').DataTable().destroy();
                }
                $('#tablePurchaseAcceptTask').empty();
                var rowData = "";

                if (data.length > 0) {
                    $.each(data, function (index, item) {
                        rowData += "<tr><td><a onclick='ShowPurchaseLines(\"" + item.No + "\")'><i class='bx bx-show'></i></a></td><td>" + item.No + "</td><td>" + item.Buy_From_Vendor_Name + "</td><td>" + item.Location_Code + "</td><td>" + item.Shipment_Date + "</td><td>" + item.Buy_from_Vendor_No + "</td></tr>";
                        // loop and do whatever with data
                    });
                }
                else {
                    rowData = "<tr><td></td><td></td><td></td><td style='text-align:center;'>No Records Found</td><td></td><td></td></tr>";
                }
                $('#tablePurchaseAcceptTask').append(rowData);

                dtable = $('#dataListPurchaseAcceptTask').DataTable({
                    retrieve: true,
                    filter: false,
                    paging: false,
                    info: false,
                    responsive: true,
                    ordering: false,
                });

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function ShowPurchaseLines(Document_No) {

    $.ajax(
        {
            url: '/SPDashboard/GetAllPurchaseLineForPopup?Document_No=' + Document_No,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tbPurchaseLines').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        var rowData = "<tr><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Qty_to_Receive + "</td><td>" + item.PCPL_Packing_UOM + "</td><td>" + item.PCPL_Packing_Style_Description + "</td><td>" + item.Location_Code + "</td></tr>";
                        $('#tbPurchaseLines').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                    $('#tbPurchaseLines').append(rowData);
                }

                $('#modalPurchaseLineList').css('display', 'block');
                $('.modal-title').text('Purchase Line');
                $('#dvPurchaseLines').css('display', 'block');

            },
            error: function () {
                alert("error");
            }
        }
    );
}
// Non Performing Customers list
function BindNonPerformingList() {
    $.ajax({
        url: '/SPDashboard/GetNonPerfomingCuslist',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblNonPerforminglist').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.Customer_No + "</td><td>" + item.Customer_Name + "</td><td>" + item.Salesperson_Code + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblNonPerforminglist').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
// Taeget vs Sales list
/*function BindSalespersonData() {
    $.ajax({
        url: '/SPDashboard/GetSalespersonData',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblSalesperson').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.SalesPerson_Name + "</td><td>" + item.Demand_Qty + "</td><td>" + item.Target_Qty + "</td><td>" + item.Sales_Qty + "</td><td>" + item.Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblSalesperson').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
function BindSupportSP() {
    $.ajax({
        url: '/SPDashboard/GetSupportSP',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblSupportSP').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.SalesPerson + "</td><td>" + item.Demand_Qty + "</td><td>" + item.Target_Qty + "</td><td>" + item.Sales_Qty + "</td><td>" + item.Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblSupportSP').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
function BindGProductData() {
    $.ajax({
        url: '/SPDashboard/GetProductData',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblProductsalesqty').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.Product_Name + "</td><td>" + item.Product_Total_Target_Qty + "</td><td>" + item.Product_Total_Sales_Qty + "</td><td>" + item.Product_Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblProductsalesqty').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}*/

function BindCombinedData() {
    $.ajax({
        url: '/SPDashboard/GetCombinedSalesData',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            // --- Salesperson Table ---
            $('#tblSalesperson').empty();
            var salespersonRows = "";
            if (data.Salespersons && data.Salespersons.length > 0) {
                $.each(data.Salespersons, function (index, item) {
                    salespersonRows += "<tr><td>" + item.SalesPerson_Name + "</td><td>" + item.Demand_Qty +
                        "</td><td>" + item.Target_Qty + "</td><td>" + item.Sales_Qty +
                        "</td><td>" + item.Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                salespersonRows = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblSalesperson').append(salespersonRows);

            // --- Support SP Table ---
            $('#tblSupportSP').empty();
            var supportSPRows = "";
            if (data.SupportSPs && data.SupportSPs.length > 0) {
                $.each(data.SupportSPs, function (index, item) {
                    supportSPRows += "<tr><td>" + item.SalesPerson + "</td><td>" + item.Demand_Qty +
                        "</td><td>" + item.Target_Qty + "</td><td>" + item.Sales_Qty +
                        "</td><td>" + item.Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                supportSPRows = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblSupportSP').append(supportSPRows);

            // --- Product Sales Table ---
            $('#tblProductsalesqty').empty();
            var productRows = "";
            if (data.Products && data.Products.length > 0) {
                $.each(data.Products, function (index, item) {
                    productRows += "<tr><td>" + item.Product_Name + "</td><td>" + item.Product_Total_Target_Qty +
                        "</td><td>" + item.Product_Total_Sales_Qty + "</td><td>" +
                        item.Product_Sales_Percentage_Qty + "</td></tr>";
                });
            } else {
                productRows = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblProductsalesqty').append(productRows);
        },

        error: function (xhr, status, error) {
            alert("Error fetching combined data: " + xhr.responseText);
        }
    });
}

function BindTodaylist() {
    $.ajax({
        url: '/SPDashboard/GetTodayVisit',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tbltodayvist').empty();
            var rowData = "";

            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr>" + "<td>" + item.Week_Plan_Date + "</td>" + "<td>" + item.Visit_Name + "</td>" + "<td>" + item.Visit_Sub_Type_Name + "</td>" + "<td>" + item.ContactCompanyName + "</td>" + "<td>" + item.Pur_Visit + "</td>" + "</tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tbltodayvist').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
function BindWeeklytasklist() {
    $.ajax({
        url: '/SPDashboard/GetWeeklytask',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblweeklytasklist').empty();
            var rowData = "";

            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr>" + "<td>" + item.Date + "</td>" + "<td>" + item.Visit_Name + "</td>" + "<td>" + item.Visit_SubType_Name + "</td>" + "<td>" + item.Purpose_Of_Visit + "</td>" + "<td>" + item.Customer_Name + "</td>" + "</tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblweeklytasklist').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
function BindMonthlyTask() {
    $.ajax({
        url: '/SPDashboard/GetMonthlyTask',
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {
            $('#tblMonthlist').empty();
            var rowData = "";

            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr>" + "<td>" + item.Visit_Month + "</td>" + "<td>" + item.Visit_Type + "</td>" + "<td>" + item.Visit_SubType_Name + "</td>" + "<td>" + item.No_of_Visit + "</td>" + "</tr>";
                });
            } else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }

            $('#tblMonthlist').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
