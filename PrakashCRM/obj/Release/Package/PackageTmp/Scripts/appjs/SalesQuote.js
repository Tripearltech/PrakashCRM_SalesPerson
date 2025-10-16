﻿var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
var InqNo = "", SQNo = "", ScheduleStatus = "", SQStatus = "", SQFor = "", LoggedInUser = "";

$(document).ready(function () {

    GetInterestRate();
    //BindCSOutstandingDuelist();
    var UrlVars = getUrlVars();

    if (UrlVars["CompanyNo"] != undefined && UrlVars["CompanyName"] != undefined) {

        var companyName = UrlVars["CompanyName"];
        companyName = companyName.replaceAll("%20", " ");
        $('#hfCustomerName').val(companyName);
        $('#hfContactCompanyNo').val(UrlVars["CompanyNo"]);
    }

    if ($('#hdnSalesQuoteActionErr').val() != "") {

        var SalesQuoteActionErr = $('#hdnSalesQuoteActionErr').val();

        $('#JustificationTitle').text("");
        $('#modalJustification').css('display', 'none');
        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(SalesQuoteActionErr);

        $.get('NullSalesQuoteSession', function (data) {

        });

    }

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    var curDate = GetCurrentDate();

    $('#txtSalesQuoteDate').val(curDate);
    $('#txtSQValidUntillDate').val(curDate);

    //BindInquiries();
    //BindNoSeries();
    BindLocations();
    ////BindInquiryType();
    //BindPaymentMethod();
    BindPaymentTerms();
    BindIncoTerms();
    GetCustomerTemplateCode();

    //BindGSTPlaceOfSupply();
    companyName_autocomplete();
    $('#ddlPackingStyle').append("<option value='-1'>---Select---</option>");
    $('#ddlPackingStyle').val('-1');
    BindTransportMethod();
    BindVendors();

    if (UrlVars["InquiryNo"] != undefined) {

        InqNo = UrlVars["InquiryNo"];
        //var SQNo = GetSQNoFromInqNo(InqNo);

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        var SQNo = "", ScheduleStatus = "";

        $.get(apiUrl + 'GetSQNoFromInqNo?InqNo=' + InqNo, function (data) {

            if (data != "") {
                $('#txtInqNo').val(InqNo);

                const SQDetails = data.split('_');

                SQNo = SQDetails[0];
                ScheduleStatus = SQDetails[1];

                $('#dvInquiryLineDetails').css('display', 'block');
                BindInquiryLineDetails(InqNo);
                GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus);
            }
            else {
                $('#txtInqNo').val(InqNo);
                GetAndFillInquiryDetails(InqNo);
            }

        });
    }

    if (UrlVars["SQNo"] != undefined && UrlVars["ScheduleStatus"] != undefined && UrlVars["SQStatus"] != undefined
        && UrlVars["SQFor"] != undefined && UrlVars["LoggedInUserRole"] != undefined &&
        UrlVars["LoggedInUserRole"] != "") {

        SQNo = UrlVars["SQNo"];
        ScheduleStatus = UrlVars["ScheduleStatus"];
        SQStatus = UrlVars["SQStatus"];
        SQFor = UrlVars["SQFor"];
        LoggedInUserRole = UrlVars["LoggedInUserRole"];
        GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole);

    }
    else if (UrlVars["SQNo"] != undefined && UrlVars["ScheduleStatus"] != undefined && UrlVars["SQStatus"] != undefined
        && UrlVars["SQFor"] != undefined && UrlVars["LoggedInUserRole"] == "") {

        SQNo = UrlVars["SQNo"];
        ScheduleStatus = UrlVars["ScheduleStatus"];
        SQStatus = UrlVars["SQStatus"];
        SQFor = UrlVars["SQFor"];
        LoggedInUserRole = $('#hdnLoggedInUserRole').val();
        GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole);

    }

    $('#txtCustomerName').blur(function () {
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    });

    $('#txtProductName').blur(function () {
        GetProductDetails($('#txtProductName').val());
        CalculateFormula();
        $('#btnSaveProd').css('display', 'block');
        $('#btnSave').css('display', 'block');
    });

    $('#txtAdditionalQty').change(function () {

        AdditionalQtyChange();

    });

    $('#txtPurDiscount').change(function () {

        //CalculateFormula();

    });

    $('#ddlIncoTerms').change(function () {

        $('#ddlTransportMethod').prop('disabled', false);
        $('#txtLineDetailsIncoTerms').val($('#ddlIncoTerms option:selected').text());
        $('#txtLineDetailsIncoTerms').attr('readonly', true);
        $('#ddlTransportMethod').val("-1");
        $('#txtTransportCost').val("0");
        $('#txtInsurance').val("0");

        if ($('#ddlIncoTerms').val() == "CFR" || $('#ddlIncoTerms').val() == "CIF" || $('#ddlIncoTerms').val() == "DELIVERED") {
            $('#btnShowTPDet').css('display', 'block');
            $('#ddlTransportMethod').val("PAID");
            $('#ddlTransportMethod').prop('disabled', true);
        }
        else if ($('#ddlIncoTerms').val() == "EXF" || $('#ddlIncoTerms').val() == "EXW" || $('#ddlIncoTerms').val() == "FOB") {
            $('#btnShowTPDet').css('display', 'none');
            $('#ddlTransportMethod').val("TOPAY");
            $('#ddlTransportMethod').prop('disabled', true);
        }
        else {
            $('#ddlTransportMethod').prop('disabled', false);
        }
        CalculateFormula();

    });

    $('#txtSalesDiscount').change(function () {

        CalculateFormula();

    });

    $('#ddlTransportMethod').change(function () {

        $('#txtTransportCost').val("0");

        if ($('#ddlTransportMethod').val() == "-1") {

            var msg = "Please Select Transport Method";
            ShowErrMsg(msg);

        }
        else {
            if ($("#ddlTransportMethod option:selected").text() == "ToPay") {
                $('#btnShowTPDet').css('display', 'none');
            }
            else {
                $('#btnShowTPDet').css('display', 'block');
            }
        }

    });

    $('#ddlCommissionPayable').change(function () {

        if ($('#ddlCommissionPayable').val() == "-1") {
            $('#txtCommissionAmt').val("0");
        }

    });

    $('#txtTransportCost').change(function () {

        CalculateFormula();


    });

    $('#chkIsCommission').change(function () {

        var isChkCommissionChecked = $(this).is(":checked");

        if (isChkCommissionChecked == true) {
            $('#ddlCommissionPerUnitPercent').prop('disabled', false);
        }
        else {
            $('#ddlCommissionPerUnitPercent').val('-1');
            $('#txtCommissionPercent, #txtCommissionAmt').val("");
            $('#ddlCommissionPayable').val('-1');
            $('#ddlCommissionPerUnitPercent, #txtCommissionPercent, #txtCommissionAmt, #ddlCommissionPayable').prop('disabled', true);
            CalculateFormula();
        }

    });

    $('#ddlCommissionPerUnitPercent').change(function () {

        if ($(this).val() != "-1") {

            if ($('#ddlCommissionPerUnitPercent').val() == "%") {

                $('#txtCommissionPercent, #txtCommissionAmt').prop('disabled', false);
                //$('#txtCommissionAmt').prop('disabled', false);
            }
            else {
                $('#txtCommissionPercent, #txtCommissionAmt').val("");
                $('#txtCommissionPercent, #ddlCommissionPayable').prop('disabled', true);
                $('#txtCommissionAmt').prop('disabled', false);
                $('#txtCommissionAmt').prop('readonly', false);

            }

        }
        else {
            $('#txtCommissionPercent,#txtCommissionAmt').prop('disabled', true);
            //$('#txtCommissionAmt').prop('disabled', true);
        }

    });

    $('#txtCommissionPercent').change(function () {

        if ($('#txtCommissionPercent').val() != "" || parseInt($('#txtCommissionPercent').val()) > 0) {

            $('#txtCommissionAmt').val((($('#txtSalesPrice').val() * $('#txtCommissionPercent').val()) / 100).toFixed(2));
            $('#txtCommissionAmt').prop('readonly', true);
            $('#txtCommissionAmt').change();

        }

    });

    $('#txtCommissionAmt').change(function () {

        if ($('#txtCommissionAmt').val() != "" && parseFloat($('#txtCommissionAmt').val()) > 0) {

            $('#ddlCommissionPayable').prop('disabled', false);
        }

        CalculateFormula();

    });

    $('#ddlPaymentTerms').change(function () {

        PaymentTermsChange();
        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        $('#txtLineDetailsPaymentTerms').attr('readonly', true);

    });

    $('#txtInsurance').change(function () {

        $('#txtAdditionalQty').change();

    });

    $('#txtSalesPrice').blur(function () {

        CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#ddlTaxGroup').change(function () {

        //CalculateFormula();
        //UpdateValueForTotalCost();
        $('#txtAdditionalQty').change();

    });

    $('#ddlGSTPlaceOfSupply').change(function () {
        $('#txtAdditionalQty').change();

    });

    $('#chkDropShipment').change(function () {
        //debugger
        var isChecked = $(this).is(":checked");
        if (isChecked == true) {
            if ($('#hfProdNo').val() != '') {

                BindItemVendors($('#hfProdNo').val());
                $('#dvVendors').css('display', 'block');
                $('#ddlItemVendors').css('display', 'block');
            } else {
                $('#chkDropShipment').prop('checked', false);
            }
        }
        else {
            $('#ddlItemVendors').val('-1');
            $('#dvVendors').css('display', 'none');
        }

    });

    $('#ddlPackingStyle').change(function () {
        const packingStyleDetails = $('#ddlPackingStyle').val().split('_');
        $('#txtBasicPurchaseCost').val(parseFloat(packingStyleDetails[0]).toFixed(2));
        $('#txtBasicPurchaseCost').prop('disabled', true);
        $('#txtMRPPrice').val(parseFloat(packingStyleDetails[3]).toFixed(4));
        $('#txtMRPPrice').prop('disabled', true);
        $('#hfPurchaseDays').val(parseInt(packingStyleDetails[2]));
    });

    $('#btnResetProdDetails').click(function () {

        ResetQuoteLineDetails();

    });
    // Added the DataTable initialization
    var dtable;
    dtable = $('#dataList').DataTable({
        searching: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
        columnDefs: [{ className: 'dtr-control', targets: 0 }],
        initComplete: function (settings, json) {
            $('#dataList td.dataTables_empty').remove(); // row hata diya
        }
    });

    $('#btnSaveProd').click(function () {

        debugger;

        var isLiquidProd = $('#chkIsLiquidProd').prop('checked');

        var isCommission = $('#chkIsCommission').prop('checked');

        // Validations

        if ($('#txtProductName').val() === "" || $('#txtProductName').val() === null) {

            ShowErrMsg("Please select a Product Name before saving.");

            $('#txtProductName').focus();

            return;

        }

        if ($('#ddlPackingStyle').val() === "" || $('#ddlPackingStyle').val() === null || $('#ddlPackingStyle').val() === "-1") {

            ShowErrMsg("Please select a Packing Style before saving.");

            $('#ddlPackingStyle').focus();

            return;

        }

        if ($('#txtProdQty').val() === "" || $('#txtProdQty').val() === null) {

            ShowErrMsg("Please select a Qty before saving.");

            $('#txtProdQty').focus();

            return;

        }

        if ($('#txtBasicPurchaseCost').val() === "" || $('#txtBasicPurchaseCost').val() === null) {

            ShowErrMsg("Please select a Basic Purchase Cost before saving.");

            $('#txtBasicPurchaseCost').focus();

            return;

        }
        if ($('#txtMRPPrice').val() === "" || $('#txtMRPPrice').val() === null) {

            ShowErrMsg("Please select a MRP Price before saving.");

            $('#txtMRPPrice').focus();

            return;

        }

        // Enhanced validation for Inco Terms

        if ($('#ddlIncoTerms').val() === "" || $('#ddlIncoTerms').val() === null || $('#ddlIncoTerms').val() === "-1") {

            ShowErrMsg("Please select Inco Terms before saving.");

            $('#ddlIncoTerms').focus();

            console.log("Inco Terms validation failed: ", $('#ddlIncoTerms').val());

            return;

        }

        // Enhanced validation for Payment Terms

        if ($('#ddlPaymentTerms').val() === "" || $('#ddlPaymentTerms').val() === null || $('#ddlPaymentTerms').val() === "-1") {

            ShowErrMsg("Please select Payment Terms before saving.");

            $('#ddlPaymentTerms').focus();

            console.log("Payment Terms validation failed: ", $('#ddlPaymentTerms').val());

            return;

        }

        if ($('#txtSalesPrice').val() === "" || $('#txtSalesPrice').val() === null) {

            ShowErrMsg("Please select a Sales Price before saving.");

            $('#txtSalesPrice').focus();

            return;

        }

        if ($('#txtDeliveryDate').val() === "" || $('#txtDeliveryDate').val() === null) {

            ShowErrMsg("Please select a Delivery Date before saving.");

            $('#txtDeliveryDate').focus();

            return;

        }

        // Proceed with saving the product details


        var prodOpts = $('#hfProdNo').val();

        var prodOptsTR = $('#hfProdNoEdit').val();

        var dropShipmentOpt = $('#chkDropShipment').is(':checked') ? 'Yes' : 'No';

        var inqProdLineNo = $('#hfProdLineNo').val() || ''; // Capture InqProdLineNo

        var actionsHtml = `<a class='SQLineCls' onclick='EditSQProd(${inqProdLineNo || 0},"ProdTR_${prodOpts}")'><i class='bx bxs-edit'></i></a>`;
        actionsHtml += `&nbsp;<a class='SQLineCls' onclick='DeleteSQProd(this)'><i class='bx bxs-trash'></i></a>`;

        var commissionPerUnit = isCommission ? $('#ddlCommissionPerUnitPercent').val() : "";

        var commissionPercent = isCommission ? $('#txtCommissionPercent').val() : "";

        var commissionAmt = isCommission ? $('#txtCommissionAmt').val() : "";

        var commissionPayable = isCommission ? $('#ddlCommissionPayable').val() : "";

        var Concentrate = isLiquidProd ? $('#txtConcentratePercent').val() : "";

        var NetWeigth = isLiquidProd ? $('#txtNetWeight').val() : "";

        var LiquidRate = isLiquidProd ? $('#txtLiquidRate').val() : "";

        var prodOptsArray = [

            '', actionsHtml,
            prodOpts,
            $('#txtProductName').val(),
            $('#txtProdQty').val(),
            $('#txtUOM').val(),
            $('#ddlPackingStyle option:selected').text(),
            $('#txtBasicPurchaseCost').val(),
            $('#txtSalesPrice').val(),

            $('#txtDeliveryDate').val(),

            $('#txtTotalCost').val(),

            $('#txtMargin').val(),

            $('#ddlPaymentTerms').val(),

            $('#ddlIncoTerms').val(),

            $('#ddlTransportMethod').val(),

            $('#txtTransportCost').val(),

            $('#txtSalesDiscount').val(),

            commissionPerUnit,

            commissionPercent,

            commissionAmt,

            $('#txtCreditDays').val(),

            $('#txtInterest').val(),

            `<label id="${prodOpts}_DropShipment">${dropShipmentOpt}</label>`,

            "", // 23 → hidden
            "", // 24 → hidden
            `<label id="${prodOpts}_MarginPercent">${$('#spnMarginPercent').text()}</label>`,
            commissionPayable,
            `<label id="${prodOpts}_InqProdLineNo" style="display:none;">${inqProdLineNo}</label>`,
            isLiquidProd,
            Concentrate,
            NetWeigth,
            LiquidRate,
            (dropShipmentOpt === 'Yes' ? $('#ddlItemVendors').val() : ""),
            $('#txtMRPPrice').val()

        ];
        // Ensure column count matches header count

        var colCount = $('#dataList thead th').length;

        if (prodOptsArray.length < colCount) {

            while (prodOptsArray.length < colCount) prodOptsArray.push('');

        } else if (prodOptsArray.length > colCount) {

            prodOptsArray = prodOptsArray.slice(0, colCount);

        }

        if (prodOptsTR) {

            var rowIdSelector = '#ProdTR_' + prodOptsTR;

            var rowApi = dtable.row(rowIdSelector);

            if (rowApi.any()) {

                rowApi.data(prodOptsArray).draw(false);

                $(rowApi.node()).attr('id', 'ProdTR_' + prodOpts);

                $(rowApi.node()).find("td").eq(0).addClass("dtr-control");

            } else {

                var newNode = dtable.row.add(prodOptsArray).draw(false).node();

                $(newNode).attr('id', 'ProdTR_' + prodOpts);

                $(newNode).find("td").eq(0).addClass("dtr-control");

            }

            $('#hfProdNoEdit').val('');

        } else {

            var newNode = dtable.row.add(prodOptsArray).draw(false).node();

            $(newNode).attr('id', 'ProdTR_' + prodOpts);

            $(newNode).find("td").eq(0).addClass("dtr-control");

            $('#dataList').show();

        }

        dtable.responsive.recalc();

        // Update available credit limit

        $('#txtAvailableCreditLimit').prop('disabled', false);

        var availableCreditLimit = parseFloat($('#txtAvailableCreditLimit').val().replaceAll(",", "")) -

            (parseFloat($('#txtSalesPrice').val()) * parseFloat($('#txtProdQty').val()));

        $('#txtAvailableCreditLimit').val(commaSeparateNumber(availableCreditLimit.toFixed(2)));

        $('#txtAvailableCreditLimit').prop('disabled', true);

        dataTableFunction();

        ResetQuoteLineDetails();

    });


    $('#btnClear').click(function () {

        ResetQuoteLineDetails();

    });

    $('#ddlQtyPopupLocCode').change(function () {

        $.ajax(
            {
                url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + $('#hfProdNo').val() + '&LocCode=' + $('#ddlQtyPopupLocCode').val(),
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#tblInvDetails').empty();

                    var invDetailsTR = "";

                    for (var i = 0; i < data.length; i++) {

                        invDetailsTR += "<tr><td hidden>" + data[i].ItemNo + "</td><td>" + data[i].ManufactureCode + "</td><td>" + data[i].LotNo + "</td><td>" + data[i].AvailableQty + "</td><td>" + data[i].RequestedQty + "</td>" +
                            "<td>" + data[i].UnitCost + "</td></tr>";

                    }

                    $('#tblInvDetails').append(invDetailsTR);

                },
                error: function () {
                    //alert("error");
                }
            }
        );

    });

    $('#btnAddIncoTerm').click(function () {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

        if ($('#txtIncoTermCode').val() == "" || $('#txtIncoTerm').val() == "") {

            $('#lblIncoTermAddMsg').css('display', 'block');
            $('#lblIncoTermAddMsg').text('Please Fill Inco Term Details');
            $('#lblIncoTermAddMsg').css('color', 'red');

        }
        else {

            $.post(
                apiUrl + 'AddNewIncoTerm?IncoTermCode=' + $('#txtIncoTermCode').val() + '&IncoTerm=' + $('#txtIncoTerm').val(),
                function (data) {

                    if (data) {

                        $('#lblIncoTermAddMsg').css('display', 'block');
                        $('#lblIncoTermAddMsg').text('Inco Term Added Successfully.');
                        $('#lblIncoTermAddMsg').css('color', 'green');
                        $('#txtIncoTermCode').val("");
                        $('#txtIncoTerm').val("");
                        BindIncoTerms();
                    }

                }
            );

        }

    });

    $('#btnCloseCostSheetMsg').click(function () {

        $('#dvCostSheetMsg').css('display', 'none');
        $('#dvUpdateCostSheetMsg').css('display', 'none');
        $('#modalCostSheetMsg').css('display', 'none');

        if ($('#hfCostSheetFlag').val() == "true") {
            showCostSheetDetails($('#hfItemNo').val(), $('#hfItemName').val());
        }

    });

    $('#btnCloseCostSheet').click(function () {

        $('#modalCostSheet').css('display', 'none');

    });

    $('#btnSaveCostSheet').click(function () {

        var flag = false;
        var errMsg = "";

        $('#tblCostSheetDetails tr').each(function () {

            var row = $(this)[0];
            var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();
            if (RatePerUnit == "") {
                errMsg = "Please Fill Cost Per Unit In All Charge Item";
            }

        });

        if (errMsg != "") {

            $('#lblCostSheetErrMsg').text(errMsg);
            $('#lblCostSheetErrMsg').css('display', 'block');

        }
        else {

            $('#divImage').show();
            $('#tblCostSheetDetails tr').each(function () {

                var row = $(this)[0];

                var CostSheetLineNo = parseInt(row.cells[0].innerHTML);
                var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();

                $.post(apiUrl + "UpdateCostSheet?SQNo=" + $('#lblCostSheetSQNo').text() + "&CostSheetLineNo=" + CostSheetLineNo +
                    "&RatePerUnit=" + parseFloat(RatePerUnit), function (data) {

                        if (data) {
                            flag = true;
                        }

                    });

            });

            /*if (flag) {*/

            $('#divImage').hide();
            $('#hfCostSheetFlag').val("false");
            $('#modalCostSheet').css('display', 'none');

            var prodNo = $('#hfItemNo').val();
            $("#" + prodNo + "_TotalUnitPrice").text($('#lblCostSheetTotalUnitPrice').text());
            $("#" + prodNo + "_Margin").text($('#lblCostSheetMargin').text());
            $('#modalCostSheetMsg').css('display', 'block');
            $('#dvUpdateCostSheetMsg').css('display', 'block');


        }


    });

    $('#btnCloseSQMsg').click(function () {

        $('#lblSQMsg').text("");
        $('#modalSQMsg').css('display', 'none');
        location.reload(true);

    });

    $('#chkIsShortclose').change(function () {

        if ($('#chkIsShortclose').prop('checked')) {
            $('#modalShortclose').css('display', 'block');
            $('#ShortcloseTitle').text("Shortclose");
            $('#hfShortcloseType').val("SalesQuote");
        }
        else {
            $('#modalShortclose').css('display', 'none');
        }

    });

    $('#ddlShortcloseReason').change(function () {

        if ($('#ddlShortcloseReason option:selected').text() == $('#hfSCRemarksSetupValue').val()) {

            $('#dvShortcloseRemarks').css('display', 'block');
            $('#hfShortcloseWithRemarks').val("true");

        }
        else {

            $('#txtShortcloseRemarks').val("");
            $('#dvShortcloseRemarks').css('display', 'none');
            $('#hfShortcloseWithRemarks').val("false");
        }

    });

    $('#btnShortclose').click(function () {

        apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        if ($('#hfShortcloseWithRemarks').val() == "true" && $('#txtShortcloseRemarks').val() == "") {
            $('#lblShortcloseErrMsg').css('display', 'block');
            $('#lblShortcloseErrMsg').text("Please Fill Shortclose Remarks");
        }
        else {

            $('#lblShortcloseErrMsg').css('display', 'none');
            $('#lblShortcloseErrMsg').text("");
            $('#btnShortcloseSpinner').show();
            SQNo = UrlVars["SQNo"];
            if ($('#hfShortcloseType').val() == "SalesQuote") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuote&SQNo=' + SQNo + '&SQProdLineNo=-1&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }
            else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuoteProd&SQNo=' + SQNo + '&SQProdLineNo=' + $('#hfSQProdLineNoForShortclose').val() + '&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }

            $.post(apiUrl, function (data) {

                $('#btnShortcloseSpinner').hide();
                $('#modalShortclose').css('display', 'none');
                $('#lblShortcloseErrMsg').text("");
                $('#hfSQProdLineNoForShortclose').val("");

                if (data == "") {

                    $('#modalShortcloseMsg').css('display', 'block');

                    if ($('#hfShortcloseType').val() == "SalesQuote") {
                        $('#lblSQShortclose').text("Sales quote shortclosed successfully");
                    }
                    else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                        $('#lblSQShortclose').text("Sales quote product shortclosed successfully");
                    }

                }
                else {

                    $('#modalErrMsg').css('display', 'block');
                    $('#modalErrDetails').text(data);

                }

            });

        }

    });

    $('#btnCloseModalShortclose').click(function () {

        $('#modalShortclose').css('display', 'none');
        $('#lblShortcloseErrMsg').css('display', 'none');
        $('#lblShortcloseErrMsg').text("");
        $('#hfShortcloseType').val("");

    });

    $('#btnCloseShortcloseMsg').click(function () {

        $('#lblSQShortclose').text("");
        $('#modalShortcloseMsg').css('display', 'none');
        location.reload(true);

    });

    $('#txtConcentratePercent').blur(function () {

        $('#txtProdQty').val(($('#txtConcentratePercent').val() * $('#txtNetWeight').val() / 100).toFixed(3));
        $('#txtProdQty').prop('readonly', false);

    });

    $('#txtNetWeight').blur(function () {

        $('#txtConcentratePercent').blur();
    });

    $('#txtLiquidRate').blur(function () {

        $('#txtSalesPrice').val(((parseFloat($('#txtNetWeight').val()) * parseFloat($('#txtLiquidRate').val())) / parseFloat($('#txtProdQty').val())).toFixed(2));

    });

    $('#chkShowAllProducts').change(function () {

        productName_autocomplete();

    });

    $('#btnAddNewContactPerson').click(function () {

        $('.modal-title').html("Add New Contact Person<br />In Selected Company");
        $('#hfAddNewDetails').val("ContactPerson");
        $('#dvAddNewCPerson').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        BindDepartment();

    });

    $('#btnAddNewBillTo').click(function () {

        $('.modal-title').html("Add New Bill-to Address");
        $('#hfAddNewDetails').val("BillToAddress");
        $('#dvAddNewShiptoAddress').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        $('#txtNewShiptoAddName').val($('#txtCustomerName').val());
        $('#txtNewShiptoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnAddNewJobTo').click(function () {

        $('.modal-title').html("Add New Delivery-to Address");
        $('#hfAddNewDetails').val("DeliveryToAddress");
        $('#dvAddNewJobtoAddress').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        $('#txtNewJobtoAddName').val($('#txtCustomerName').val());
        $('#txtNewJobtoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnCloseModalSQ').click(function () {

        $('#lblMsg').text("");
        $('#hfAddNewDetails').val("");
        $('#dvAddNewCPerson').css('display', 'none');
        $('#dvAddNewShiptoAddress').css('display', 'none');
        $('#dvAddNewJobtoAddress').css('display', 'none');
        ResetCPersonDetails();
        ResetNewBillToAddressDetails();
        ResetNewDeliveryToAddressDetails();
        $('#modalSQ').css('display', 'none');

    });

    $('#btnConfirmAdd').click(function () {

        if ($('#hfAddNewDetails').val() == "ContactPerson") {

            var errMsg = CheckCPersonFieldValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var CPersonDetails = {};

                CPersonDetails.Name = $('#txtCPersonName').val();
                CPersonDetails.Company_No = $('#hfContactCompanyNo').val();
                CPersonDetails.Mobile_Phone_No = $('#txtCPersonMobile').val();
                CPersonDetails.E_Mail = $('#txtCPersonEmail').val();
                CPersonDetails.PCPL_Job_Responsibility = $('#txtJobResponsibility').val();
                CPersonDetails.PCPL_Department_Code = $('#ddlDepartment').val();
                CPersonDetails.Type = "Person";
                CPersonDetails.Salesperson_Code = $('#hdnLoggedInUserSPCode').val();
                CPersonDetails.PCPL_Allow_Login = $('#chkAllowLogin').prop('checked');
                CPersonDetails.chkEnableOTPOnLogin = $('#chkEnableOTPOnLogin').prop('checked');
                CPersonDetails.Is_Primary = $('#chkIsPrimary').prop('checked');

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewContactPerson', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(CPersonDetails)
                    });
                    const res = await rawResponse.ok;
                    if (res) {

                        $('#btnAddSpinner').css('display', 'none');
                        GetContactsOfCompany($('#txtCustomerName').val());
                        $('#lblMsg').text("Contact Person Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetCPersonDetails();

                    }
                })();
            }

        }
        else if ($('#hfAddNewDetails').val() == "BillToAddress") {

            var errMsg = CheckNewBilltoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewBillToAddress = {};

                NewBillToAddress.Customer_No = $('#hfCustomerNo').val();
                NewBillToAddress.Code = $('#txtNewShiptoAddCode').val();
                NewBillToAddress.Address = $('#txtNewShiptoAddress').val();
                NewBillToAddress.Address_2 = $('#txtNewShiptoAddress2').val();
                NewBillToAddress.Post_Code = $('#txtNewShiptoAddPostCode').val();
                NewBillToAddress.PCPL_Area = $('#ddlNewShiptoAddArea').val();
                NewBillToAddress.State = $('#txtNewShiptoAddState').val();
                NewBillToAddress.GST_Registration_No = $('#txtNewShiptoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewBillToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewBillToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Bill-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewBillToAddressDetails();
                        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }
        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

            var errMsg = CheckNewDeliverytoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewDeliveryToAddress = {};

                NewDeliveryToAddress.Customer_No = $('#hfCustomerNo').val();
                NewDeliveryToAddress.Code = $('#txtNewJobtoAddCode').val();
                NewDeliveryToAddress.Address = $('#txtNewJobtoAddress').val();
                NewDeliveryToAddress.Address_2 = $('#txtNewJobtoAddress2').val();
                NewDeliveryToAddress.Post_Code = $('#txtNewJobtoAddPostCode').val();
                NewDeliveryToAddress.PCPL_Area = $('#ddlNewJobtoAddArea').val();
                NewDeliveryToAddress.State = $('#txtNewJobtoAddState').val();
                NewDeliveryToAddress.GST_Registration_No = $('#txtNewJobtoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewDeliveryToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewDeliveryToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Delivery-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewDeliveryToAddressDetails();
                        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }

    });

    $('#chkIsLiquidProd').change(function () {

        $('#txtSalesPrice').val("0");
        if ($('#chkIsLiquidProd').prop('checked')) {

            $('#dvLiquidProdFields').css('display', 'block');
            $('#txtSalesPrice').prop('disabled', true);
            $('#hfIsLiquidProd').val("true");

        }
        else {

            $('#dvLiquidProdFields').css('display', 'none');
            $('#txtSalesPrice').prop('disabled', false);
            $('#hfIsLiquidProd').val("false");
            $('#txtConcentratePercent').val("");
            $('#txtLiquidRate').val("");
            $('#txtProdQty').val("");
            $('#txtProdQty').prop('readonly', false);
            //$('#lblGrossWeight').text("");
            //$('#txtNetWeight').val("");
            $('#txtTransportCost').val("0");
            $('#txtSalesDiscount').val("0");
            $('#chkIsCommission').prop('checked', false);
            $('#chkIsCommission').change();
            $('#ddlCommissionPayable').val('-1');
            $('#txtCreditDays').val("0");
            $('#txtMargin').val("0.00");
            $('#txtInterest').val("0.00");
            $('#txtTotalCost').val("0.00");
            $('#txtDeliveryDate').val("");
            $('#chkDropShipment').prop('checked', false);
            $('#chkDropShipment').change();
            $('#txtMRPPrice').val("0.00");

        }

    });

    $('#btnCloseApproveRejectMsg').click(function () {

        $('#modalApproveRejectMsg').css('display', 'none');
        $('#lblApproveRejectMsg').text("");
        RedirectToSQApproval();

    });

    $('#btnReject').click(function () {

        $('#modalRejectRemarks').css('display', 'block');

    });

    $('#btnConfirmReject').click(function () {

        if ($('#txtRejectRemarks').val() == "") {
            $('#lblRemarksMsg').text("Please Fill Remarks");
        }
        else {
            ApproveRejectSQ("Reject", $('#txtRejectRemarks').val());
        }

    });

    $('#btnCloseModalRejectRemarks').click(function () {

        $('#modalRejectRemarks').css('display', 'none');

    });

});


function BindDepartment() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllDepartmentForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlDepartment').empty();
                    $('#ddlDepartment').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Department
                            }
                        ).html(data.Department).appendTo("#ddlDepartment");
                    });

                    $("#ddlDepartment").val('-1');

                    //if ($('#hfDepartmentCode').val() != "") {
                    //    $("#ddlDepartment").val($('#hfDepartmentCode').val());
                    //}
                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function GetAndFillInquiryDetails(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetInquiryDetails?InqNo=' + InqNo, function (data) {

        $('#hfInqNo').val(data.Inquiry_No);
        $('#hfContactCompanyNo').val(data.Inquiry_Customer_Contact);
        $('#hfSavedContactPersonNo').val(data.PCPL_Contact_Person);
        $('#txtCustomerName').val(data.Contact_Company_Name);
        $('#txtCustomerName').blur();
        $('#hfPaymentTerms').val(data.Payment_Terms);
        $('#hfShiptoCode').val(data.Ship_to_Code);
        $('#hfJobtoCode').val(data.PCPL_Job_to_Code);
        $('#dvInquiryLineDetails').css('display', 'block');
        BindPaymentTerms();
        $('#ddlPaymentTerms').change();
        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        BindInquiryLineDetails(InqNo);

    });

}

function GetAndFillSQDetails(SQNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetSQDetailsForSQNo?SQNo=' + SQNo, function (data) {

        $('#hfInqNo').val(data.Inquiry_No);
        $('#txtCustomerName').val(data.Contact_Company_Name);
        $('#txtCustomerName').blur();
        $('#hfPaymentTerms').val(data.Payment_Terms);
        $('#hfShiptoCode').val(data.Ship_to_Code);
        $('#hfJobtoCode').val(data.PCPL_Job_to_Code);
        $('#dvInquiryLineDetails').css('display', 'block');
        BindInquiryLineDetails(InqNo);

    });

}


function BindSQLineDetails(SalesQuoteNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllSQLinesOfSQ?QuoteNo=' + SalesQuoteNo + '&SQLinesFor=SalesQuote',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#dataList').css('display', 'block');
                $('#tblProducts').empty();
                var SQLineTR = "";
                $.each(data, function (index, item) {

                    prodOpts = "<tr><td></td><td hidden>" + item.Line_No + "</td><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" +
                        item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.PCPL_Basic_Price +
                        "</td><td>" + item.Delivery_Date + "</td><td>" + $('#ddlTransportMethod option:selected').text() + "</td><td>" + item.PCPL_Sales_Price + "</td><td>" +
                        $('#txtLineDetailsPaymentTerms').val() + "</td><td>" + $('#txtLineDetailsIncoTerms').val() + "</td><td id='CostSheetOpt'><a class='CostSheetCls' onclick='showCostSheetDetails(\"" + $('#hfSalesQuoteNo').val() + "\"," + item.Line_No + ")'><span class='badge bg-primary'>Cost Sheet</span></a></td></tr>";

                    $('#tblProducts').append(prodOpts);

                });

                dataTableFunction();

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function dTableFunction() {

    dataTableFunction();

}

function dataTableFunction() {

    dtable = $('#dataList').DataTable({
        retrieve: true,
        filter: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
    });

}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function BindInquiryLineDetails(InqNo) {
    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetInquiryProdDetails?InqNo=' + InqNo, function (data) {
        var ProdTR = "";
        $.each(data, function (index, item) {
            ProdTR += "<tr><td hidden>" + item.Line_No + "</td><td hidden>" + item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" +
                item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.Unit_of_Measure + "</td><td>" + item.Delivery_Date + "</td><td>" + item.PCPL_Payment_Terms + "</td>" +
                "<td><a class='InqProdCls' onclick='FillInqProdDetails(\"" + item.Line_No + "\",\"" + item.Product_No + "\",\"" + item.Product_Name + "\"," +
                item.Quantity + ",\"" + item.PCPL_Packing_Style_Code + "\",\"" + item.Delivery_Date + "\")'><i class='bx bx-edit'></i></a></td>" +
                "<td hidden><label id='InqProdLineNo_" + item.Product_No + "'>" + item.Line_No + "</label></td></tr>";
        });

        $('#tblInqProdDetails').append(ProdTR);
    });
}

function FillInqProdDetails(ProdLineNo, ProductNo, ProductName, Quantity, PackingStyleCode, DeliveryDate) {
    ResetQuoteLineDetails();
    $('#hfProdLineNo').val(ProdLineNo); // Store InqProdLineNo
    $('#hfProdNo').val(ProductNo);
    $('#hfInqProdPackingStyle').val(PackingStyleCode);
    $('#txtProductName').val(ProductName);
    $('#txtProductName').blur();
    $('#txtProdQty').val(Quantity);
    $('#txtDeliveryDate').val(DeliveryDate);
    $('#ddlPaymentTerms').change();
}

function BindLocations() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetLocationsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlLocations option').remove();
                var locationsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    locationsOpts += "<option value='" + item.Code + "'>" + item.Name + "</option>";
                });

                $('#ddlLocations').append(locationsOpts);

                $('#ddlLocations').val('-1');

                if ($('#hfSavedLocationCode').val() != "" && $('#hfSavedLocationCode').val() != null) {
                    $('#ddlLocations').val($('#hfSavedLocationCode').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function GetCustomerTemplateCode() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetCustomerTemplateCode', function (data) {

        if (data != "") {
            $('#hfCustomerTemplateCode').val(data);
        }

    });

}

function BindInquiryType() {

    $('#ddlInquiryType option').remove();
    var inquirytypeOpts = "<option value='-1'>---Select---</option>";
    inquirytypeOpts += "<option value='Local'>Local</option>";
    inquirytypeOpts += "<option value='Export'>Export</option>";
    inquirytypeOpts += "<option value='Sample'>Sample</option>";

    $('#ddlInquiryType').append(inquirytypeOpts);
    $('#ddlInquiryType').val('Local');
    $('#ddlInquiryType').attr('disabled', true);
}

function GetContactsOfCompany(companyName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllContactsOfCompany?companyName=' + companyName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    var primaryContactNo = "";
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        if (data.Is_Primary) {
                            primaryContactNo = data.No;
                        }
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlContactName");
                    });

                    $("#ddlContactName").val(primaryContactNo);
                    $('#btnSaveProd').css('display', 'block');
                    $('#btnSave').css('display', 'block');

                    if ($('#hfSavedContactPersonNo').val() != "") {

                        $("#ddlContactName").val($('#hfSavedContactPersonNo').val());

                    }
                    //if ($('#hfAreaCode').val() != "") {
                    //    $("#ddlArea").val($('#hfAreaCode').val());
                    //}
                }
                else {
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));

                    $('#btnSaveProd').css('display', 'none');
                    $('#btnSave').css('display', 'none');
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function companyName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var company = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var companiesArray = $.map(company, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtCustomerName').autocomplete({
                lookup: companiesArray,
                onSelect: function (selecteditem) {
                    $('#hfContactCompanyNo').val(selecteditem.data);
                    $('#btnAddNewContactPerson').prop('disabled', false);
                },
            });
        }
    });

    $('#ddlContactName').empty();
    $('#ddlContactName').append("<option value='-1'>---Select---</option>");
    $('#ddlContactName').val('-1');

    if ($('#hfSavedCustomerName').val() != "" && $('#hfSavedCustomerName').val() != null) {
        $('#txtCustomerName').val($('#hfSavedCustomerName').val());
        $('#txtCustomerName').blur();
    }
    else if ($('#hfCustomerName').val() != "" && $('#hfCustomerName').val() != null) {
        $('#txtCustomerName').val($('#hfCustomerName').val());
        //$('#txtCustomerName').change();
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    }

};

function BindPincodeMin2Char() {
    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    $('#txtNewShiptoAddPostCode, #txtNewJobtoAddPostCode').autocomplete({
        serviceUrl: '/SPSalesQuotes/GetPincodeForDDL',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },
        onSelect: function (suggestion) {
            jQuery("#hfPostCode").val(suggestion.value);

            var citydis = suggestion.data.split(",");
            var statecountry = suggestion.id.split(",");

            if ($('#hfAddNewDetails').val() == "BillToAddress") {

                jQuery("#txtNewShiptoAddCity").val(citydis[0]);
                $("#txtNewShiptoAddCity").prop('readonly', true);
                jQuery("#txtNewShiptoAddDistrict").val(citydis[1]);
                $("#txtNewShiptoAddDistrict").prop('readonly', true);

                jQuery("#txtNewShiptoAddCountryCode").val(statecountry[1]);
                $("#txtNewShiptoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewShiptoAddState").val(statecountry[0]);
                $("#txtNewShiptoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewShiptoAddPostCode').val());

            }

            if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                jQuery("#txtNewJobtoAddCity").val(citydis[0]);
                $("#txtNewJobtoAddCity").prop('readonly', true);
                jQuery("#txtNewJobtoAddDistrict").val(citydis[1]);
                $("#txtNewJobtoAddDistrict").prop('readonly', true);

                jQuery("#txtNewJobtoAddCountryCode").val(statecountry[1]);
                $("#txtNewJobtoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewJobtoAddState").val(statecountry[0]);
                $("#txtNewJobtoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewJobtoAddPostCode').val());
            }

            return false;
        },
        select: function (event, ui) {
        },
        transformResult: function (response) {
            return {
                suggestions: jQuery.map(jQuery.parseJSON(response), function (item) {
                    return {
                        value: item.Code,
                        data: item.City + ',' + item.District_Code,
                        id: item.PCPL_State_Code + ',' + item.Country_Region_Code
                    };
                })
            };
        },
    });

}


function BindArea() {

    $.get(apiUrl + 'GetAllAreasForDDL', function (data) {

        if (data.length > 0) {

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').empty();
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').empty();
            }

            var AreaOpts = "<option value='-1'>---Select---</option>";

            for (var a = 0; a < data.length; a++) {

                AreaOpts += "<option value='" + data[a].Code + "'>" + data[a].Text + "</option>";

            }

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').append(AreaOpts);
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').append(AreaOpts);
            }

        }

    });

}

function GetCreditLimitAndCustDetails(companyName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetCreditLimitAndCustDetails?companyName=' + companyName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#txtTotalCreditLimit').val(data.CreditLimit).prop('disabled', true);
                    $('#txtAvailableCreditLimit').val(data.AvailableCredit).prop('disabled', true);
                    $('#txtOutstandingDue').val(data.OutstandingDue).prop('disabled', true);
                    $('#hfUsedCreditLimit').val(data.UsedCreditLimit);
                    $('#hdnCustBalanceLCY').val(data.AccountBalance);
                    $('#tdClassCustomer').text(data.PcplClass); // Set the text content of tdClassCustomer
                    $('#tdClassCustomer').val(data.PcplClass);
                    $('#tdAvgDelayDays').text(data.AverageDelayDays); // Set the text content of td AverageDelayDays
                    $('#tdAvgDelayDays').val(data.AverageDelayDays);

                    if (SQFor == "ApproveReject") {
                        $('#tdOverdue').text($('#txtOutstandingDue').val());
                        $('#tdTotalExpo').text($('#hdnCustBalanceLCY').val());
                    }

                    $('#ddlConsigneeAddress option').remove();
                    var consigneeAddOpts = "<option value='-1'>---Select---</option>";

                    if ((data.Address != "" && data.Address != null) || (data.City != "" && data.City != null) ||
                        (data.Post_Code != "" && data.Post_Code != null)) {

                        var consigneeAddText = "", consigneeAddValue = "";


                        if (data.CustNo != null) {
                            $('#hfCustomerNo').val(data.CustNo);
                            consigneeAddText = data.CustName + "," + data.Address + " ," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            //
                            consigneeAddValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            //

                        }
                        else {
                            consigneeAddText = data.CompanyName + "," + data.Address + "," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            //
                            consigneeAddValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            //
                        }

                        consigneeAddOpts += "<option value=\"" + consigneeAddValue + "\">" + consigneeAddText + "</option>";

                        $('#ddlConsigneeAddress').append(consigneeAddOpts);
                        $('#ddlConsigneeAddress').val(consigneeAddValue);

                        $('#hfConsigneeAdd').val(consigneeAddValue);
                    }
                    else {
                        $('#ddlConsigneeAddress').empty();
                        $('#ddlConsigneeAddress').append(consigneeAddOpts);
                        $('#ddlConsigneeAddress').prop('disabled', true);
                    }

                    if (data.CustNo != null) {

                        $('#hfCustPANNo').val(data.PANNo);

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";
                        var billtoCode = "";
                        var billtoAdd = "";


                        if (data.ShiptoAddress != null) {
                            for (var i = 0; i < data.ShiptoAddress.length; i++) {
                                //billtoAdd = data.ShiptoAddress[i].Address;
                                billtoAddOpts += "<option value=\"" + data.ShiptoAddress[i].Code + "\">" + data.ShiptoAddress[i].Address + "</option>";
                            }
                            billtoCode = data.ShiptoAddress[0].Code;
                            $('#ddlBillTo').append(billtoAddOpts);
                            $('#ddlBillTo').val(billtoCode);
                        }
                        else {
                            $('#ddlBillTo').append("<option value='-1'>---Select---</option>");
                        }

                        $('#btnAddNewBillTo').prop('disabled', false);

                        if ($('#hfShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfShiptoCode').val());
                        }
                        else if ($('#hfSavedShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfSavedShiptoCode').val());
                        }
                        //else {
                        //    $('#ddlBillTo').append("<option value='-1'>---Select---</option>");
                        //}

                        //$('#ddlBillTo').attr('disabled', true);

                        $('#ddlDeliveryTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        if (data.JobtoAddress != null) {
                            for (var i = 0; i < data.JobtoAddress.length; i++) {
                                shiptoAdd = shiptoAdd + "<option value=\"" + data.JobtoAddress[i].Code + "\">" + data.JobtoAddress[i].Address + "</option>";
                            }
                        }

                        $('#ddlDeliveryTo').append(shiptoAdd);
                        $('#btnAddNewJobTo').prop('disabled', false);

                        if ($('#hfJobtoCode').val() != "") {
                            $('#ddlDeliveryTo').val($('#hfJobtoCode').val());
                        }
                        else if ($('#hfSavedJobtoCode').val() != "") {
                            $('#ddlDeliveryTo').val($('#hfSavedJobtoCode').val());
                        }
                        else {
                            $('#ddlDeliveryTo').val('-1');
                        }

                        $('#ddlDeliveryTo').attr('disabled', false);

                        $('#hfCustomerNo').val(data.CustNo);
                    }
                    else {

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";

                        $('#ddlBillTo').append(billtoAddOpts);
                        $('#ddlBillTo').val('-1');
                        //$('#ddlBillTo').attr('disabled', true);

                        $('#ddlDeliveryTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        $('#ddlDeliveryTo').append(shiptoAdd);
                        $('#ddlDeliveryTo').val('-1');
                        $('#ddlDeliveryTo').attr('disabled', true);
                    }


                }

            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function productName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    if ($('#chkShowAllProducts').prop('checked')) {
        apiUrl += 'GetAllProductsForShowAllProd';
    }
    else {
        apiUrl += 'GetAllProducts?CCompanyNo=' + $('#hfContactCompanyNo').val();
    }

    $.get(apiUrl, function (data) {

        if (data != null) {
            let str1 = "";

            var i;
            for (i = 0; i < data.length; i++) {

                if ($('#chkShowAllProducts').prop('checked')) {
                    str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Description.trim() + '"' + ",";
                }
                else {
                    str1 = str1 + '"' + data[i].Item_No + '"' + ":" + '"' + data[i].Item_Name.trim() + '"' + ",";
                }

            }

            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var products = objFromStr; // { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            //var item = {"AB001":"abc", "AB002":"bcd"};

            var productsArray = $.map(products, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtProductName').autocomplete({
                lookup: productsArray,
                onSelect: function (selecteditem) {
                    $('#hfProdNo').val(selecteditem.data);
                },
            });
        }
    });
};

function BindPaymentTerms() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetPaymentTermsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlPaymentTerms option').remove();
                var paymentTermsCodeForInq = "";
                var paymenttermsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {

                    if ($('#hfPaymentTerms').val() != "" && $('#hfPaymentTerms').val() != null) {

                        if ($('#hfPaymentTerms').val() == item.Code) {
                            paymentTermsCodeForInq = item.Code + "_" + item.Due_Date_Calculation;
                        }

                    }
                    paymenttermsOpts += "<option value=\"" + item.Code + "_" + item.Due_Date_Calculation + "\">" + item.Description + "</option>";
                });

                $('#ddlPaymentTerms').append(paymenttermsOpts);

                $('#ddlPaymentTerms').val('-1');

                if ($('#hfPaymentTerms').val() != "" && $('#hfPaymentTerms').val() != null) {

                    $('#ddlPaymentTerms').val(paymentTermsCodeForInq);
                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindItemVendors(ProdNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetItemVendorsForDDL?ProdNo=' + ProdNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    $('#ddlItemVendors option').remove();
                    var itemVendorsOpts = "<option value='-1'>---Select---</option>";
                    $.each(data, function (index, item) {
                        itemVendorsOpts += "<option value='" + item.Vendor_No + "'>" + item.Vendor_Name + "</option>";
                    });

                    $('#ddlItemVendors').append(itemVendorsOpts);
                    $('#ddlItemVendors').val('-1');
                    if ($('#hfSavedItemVendorNo').val() != "" && $('#hfSavedItemVendorNo').val() != null) {
                        $('#ddlItemVendors').val($('#hfSavedItemVendorNo').val());
                    }
                }
                else {
                    $('#ddlItemVendors').append("<option value='-1'>---Select---</option>");
                    $('#ddlItemVendors').val('-1');
                }


            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindIncoTerms() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetIncoTermsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlIncoTerms option').remove();
                var incotermsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    incotermsOpts += "<option value='" + item.Code + "'>" + item.Description + "</option>";
                });

                $('#ddlIncoTerms').append(incotermsOpts);

                $('#ddlIncoTerms').val('-1');

                if ($('#hfSavedIncoTerms').val() != "" && $('#hfSavedIncoTerms').val() != null) {
                    $('#ddlIncoTerms').val($('#hfSavedIncoTerms').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindTransportMethod() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetTransportMethodsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlTransportMethod option').remove();
                var transportMethods = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    transportMethods += "<option value='" + item.Code + "'>" + item.Description + "</option>";
                });

                $('#ddlTransportMethod').append(transportMethods);

                $('#ddlTransportMethod').val('-1');

                if ($('#hfSavedTransportMethod').val() != "" && $('#hfSavedTransportMethod').val() != null) {
                    $('#ddlTransportMethod').val($('#hfSavedTransportMethod').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindGSTPlaceOfSupply() {
    $('#ddlGSTPlaceOfSupply option').remove();
    var gstplaceofsupplyOpts = "<option value='-1'>---Select---</option>";
    gstplaceofsupplyOpts += "<option value='1'>Bill-to Address</option>";
    gstplaceofsupplyOpts += "<option value='2'>Ship-to Address</option>";
    gstplaceofsupplyOpts += "<option value='3'>Location Address</option>";

    $('#ddlGSTPlaceOfSupply').append(gstplaceofsupplyOpts);
    $('#ddlGSTPlaceOfSupply').val('1');
    $('#ddlGSTPlaceOfSupply').attr('disabled', true);
}

function GetSalesQuoteDetailsAndFill(SalesQuoteNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    //$.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + $('#hfSalesQuoteNo').val(), function (data) {
    $.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + SalesQuoteNo, function (data) {

        if (data.No != "" || data.No != null) {

            $('#hfSQEdit').val("true");
            $('#hfSalesQuoteNo').val(SalesQuoteNo);
            //$('#ddlNoSeries').prop('disabled', true);
            $('#hfInqNo').val(data.InquiryNo);
            $('#txtInqNo').val(data.InquiryNo);
            $('#hfSavedLocationCode').val(data.LocationCode);
            $('#txtSalesQuoteDate').val(data.OrderDate);
            $('#txtSQValidUntillDate').val(data.ValidUntillDate);
            $('#hfContactCompanyNo').val(data.ContactCompanyNo);
            $('#hfSavedCustomerName').val(data.ContactCompanyName);
            $('#hfSavedContactPersonNo').val(data.ContactPersonNo);
            $('#hfSavedShiptoCode').val(data.ShiptoCode);
            $('#hfSavedJobtoCode').val(data.JobtoCode);
            $('#hfSavedPaymentMethod').val(data.PaymentMethodCode);
            //$('#hfSavedTransportMethod').val(data.TransportMethodCode);
            $('#hfPaymentTerms').val(data.PaymentTermsCode);
            $('#hdnSalespersonEmail').val(data.SalespersonEmail);
            $('#hdnApprovalFor').val(data.ApprovalFor);
            BindPaymentTerms();
            //$('#ddlPaymentTerms').val(data.PaymentTermsCode);
            $('#hfSavedIncoTerms').val(data.ShipmentMethodCode);
            $('#ddlIncoTerms').val(data.ShipmentMethodCode);
            companyName_autocomplete();
            //$('#CostSheetOpt').css('display', 'block');
            $('#txtCustomerName').prop('readonly', true);
            $('#txtSalesQuoteDate').prop('readonly', true);
            $('#hfSCRemarksSetupValue').val(data.SCRemarksSetupValue);
            if ($('#hfInqNo').val() == "-1") {
                $('#ddlInquiries').prop('disabled', true);
            }

            if (ScheduleStatus == "Completed") {
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
            }

            if (data.ShortcloseStatus) {
                //$('#chkIsShortclose').prop('checked', true);
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
            }
            else {
                //$('#chkIsShortclose').prop('checked', false);
                $('#btnSaveProd').prop('disabled', false);
                $('#btnSave').prop('disabled', false);
            }

            if (data.Status == "Approval pending from finance" || data.Status == "Approval pending from HOD"
                || data.Status == "Approved") {
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
            }
            else {
                $('#btnSaveProd').prop('disabled', false);
                $('#btnSave').prop('disabled', false);
            }

            $('#dataList').css('display', 'block');
            var itemLineNo = "";
            var dtable = $('#dataList').DataTable({
                retrieve: true,
                searching: false,
                paging: false,
                info: false,
                responsive: true,
                ordering: false,
                columnDefs: [

                    { className: 'dtr-control', targets: 0 }
                ]
            });

            $.each(data.ProductsRes, function (index, item) {
                var actionsHtml = "";
                if (item.TPTPL_Short_Closed) {
                    actionsHtml = "<span class='badge bg-secondary'>Shortclosed</span>";
                } else {
                    actionsHtml = `<a class='SQLineCls' onclick='EditSQProd(${item.Line_No},"ProdTR_${item.No}")'><i class='bx bxs-edit'></i></a>`;
                    if (!(ScheduleStatus === "Completed" || data.ShortcloseStatus === true)) {
                        actionsHtml += `&nbsp;<a class='SQLineCls' title='Click to shortclose' onclick='ShortcloseSQProd("${item.Line_No}")'><i class='bx bx-message-rounded-x'></i></a>`;
                    }
                }
                var rowArray = [
                    "", // dtr-control
                    actionsHtml,
                    item.No,
                    item.Description,
                    item.Quantity,
                    item.Unit_of_Measure_Code,
                    item.PCPL_Packing_Style_Code,
                    item.PCPL_MRP,
                    item.Unit_Price,
                    item.Delivery_Date,
                    item.PCPL_Total_Cost,
                    item.PCPL_Margin,
                    data.PaymentTermsCode,
                    data.ShipmentMethodCode,
                    item.PCPL_Transport_Method,
                    item.PCPL_Transport_Cost,
                    item.PCPL_Sales_Discount,
                    item.PCPL_Commission_Type,
                    item.PCPL_Commission,
                    item.PCPL_Commission_Amount,
                    item.PCPL_Credit_Days,
                    item.PCPL_Interest,
                    `<label id="${item.No}_DropShipment">${item.Drop_Shipment}</label>`,
                    "",
                    "",
                    `<label id="${item.No}_MarginPercent">${item.PCPL_Margin_Percent} %</label>`,

                    item.PCPL_Commission_Payable,
                    //item.PCPL_Commission_Payable_Name,
                    "",
                    item.PCPL_Liquid,
                    item.PCPL_Concentration_Rate_Percent,
                    item.Net_Weight,
                    item.PCPL_Liquid_Rate,
                    item.PCPL_Vendor_No,  
                    item.PCPL_Packing_MRP_Price,
                    `<label id="${item.No}_SQLineNo" style='display:none'>${item.Line_No}</label>`,
                ];

                var colCount = $('#dataList thead th').length;
                while (rowArray.length < colCount) rowArray.push('');

                var newNode = dtable.row.add(rowArray).draw(false).node();
                $(newNode).attr('id', 'ProdTR_' + item.No);
                $(newNode).find("td").eq(0).addClass("dtr-control");
            });

            dtable.responsive.recalc();
            itemLineNo = data.QuoteNo + "," + itemLineNo;
            $('#hfSalesQuoteResDetails').val(itemLineNo);

            if (SQFor == "ApproveReject") {

                if (LoggedInUserRole != "Admin" && LoggedInUserRole != "Salesperson") {
                    $('#dvSQApproveRejectBtn').css('display', 'block');

                    if (LoggedInUserRole == "Finance") {
                        $('#dvSQAprJustificationDetails').css('display', 'block');
                        $('#dvAprDetailsFinanceUser').css('display', 'block');
                        $('#lblOutstandingValue').css('display', 'block');
                        $('#dvcustomerOutStanding').css('display', 'block');
                        $('#lblJustificationTitle').text("Last 10 Sales Quote Justification Details");

                        $('#AprDetailsCustName').text(data.ContactCompanyName);
                        $('#AprDetailsApprovalFor').text(data.ApprovalFor);
                        $('#AprDetailsJustificationReason').text(data.WorkDescription);
                        $('#tdDetailsStatus').text(data.Status);
                        $('#tdDetailsLocationCode').text(data.LocationCode);
                        GetAndFillCompanyIndustry(data.ContactCompanyNo);
                    }
                    else {
                        $('#dvSQAprJustificationDetails').css('display', 'none');
                        $('#dvAprDetailsFinanceUser').css('display', 'none');
                        $('#lblOutstandingValue').css('display', 'none');
                        $('#dvcustomerOutStanding').css('display', 'none');
                        $('#lblJustificationTitle').text("Last 3 Sales Quote Justification Details");
                    }

                    $('#dvSQJustificationDetails').css('display', 'block');
                    BindJustificationDetails(LoggedInUserRole, data.ContactCompanyNo);
                    BindCSOutstandingDuelist();
                }

            }
            else {

                $('#dvSQApproveRejectBtn').css('display', 'none');
                $('#dvSQAprJustificationDetails').css('display', 'none');
                $('#dvAprDetailsFinanceUser').css('display', 'none');
                $('#lblOutstandingValue').css('display', 'none');
                $('#dvcustomerOutStanding').css('display', 'none');
                $('#dvSQJustificationDetails').css('display', 'none');
                $('#lblJustificationTitle').text("");

            }

        }

    });

}

function GetProductDetails(productName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetProductDetails?productName=' + productName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#hfProdNo').val(data.No);

                    $('#ddlPackingStyle option').remove();
                    var packingstyleOpts = "<option value='-1'>---Select---</option>";
                    var inqProdPackingStyle = "";

                    for (var i = 0; i < data.ProductPackingStyle.length; i++) {

                        if ($('#hfInqProdPackingStyle').val() != "" && $('#hfInqProdPackingStyle').val() != null) {
                            if ($('#hfInqProdPackingStyle').val() == data.ProductPackingStyle[i].Packing_Style_Code) {
                                inqProdPackingStyle = data.ProductPackingStyle[i].PCPL_Purchase_Cost + "_" + data.ProductPackingStyle[i].Packing_Style_Code + "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days + "_" + data.ProductPackingStyle[i].PCPL_MRP_Price;
                            }
                        }
                        packingstyleOpts +=
                            "<option value='" + data.ProductPackingStyle[i].PCPL_Purchase_Cost + "_" + data.ProductPackingStyle[i].Packing_Style_Code + "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days + "_" + data.ProductPackingStyle[i].PCPL_MRP_Price + "'>" + data.ProductPackingStyle[i].Packing_Style_Description +
                            "</option>";
                    }

                    $('#ddlPackingStyle').append(packingstyleOpts);

                    if ($('#hfInqProdPackingStyle').val() != "" && $('#hfInqProdPackingStyle').val() != null) {
                        $('#ddlPackingStyle').val(inqProdPackingStyle);
                        $('#ddlPackingStyle').change();
                    }
                    else {
                        $('#ddlPackingStyle').val('-1');
                    }

                    $('#hfNetWeight').val(data.Net_Weight);
                    $('#hfGrossWeight').val(data.Gross_Weight);
                    $('#lblGrossWeight').text(data.Gross_Weight);
                    $('#txtNetWeight').val(data.Net_Weight);

                    $('#hfUOM').val(data.Base_Unit_of_Measure);
                    $('#txtUOM').val(data.Base_Unit_of_Measure);

                    if ($('#hfSavedPackingStyle').val() != "" && $('#hfSavedPackingStyle').val() != null) {

                        $('#ddlPackingStyle option').each(function () {
                            var optionValue = $(this).val();

                            if (optionValue.includes($('#hfSavedPackingStyle').val())) {
                                $('#ddlPackingStyle').val(optionValue);
                                $('#ddlPackingStyle').change();
                            }
                        });
                    }

                    if ($('#hfiteminvstatus').val() == "NotInInventory") {

                    }

                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function EditSQProd(Line_No, ProdTR) {

    ResetQuoteLineDetails();

    var prodNo = $("#" + ProdTR).find("TD").eq(2).html();

    $('#hfProdNo').val(prodNo);

    $('#hfProdNoEdit').val(prodNo);

    var product = $("#" + ProdTR).find("TD").eq(6).html();
    var packingStyle = ($("#" + ProdTR).find("TD").eq(3).html());

    GetProductDetails(packingStyle);

    $('#ddlPackingStyle').val(product);

    $('#hfSavedPackingStyle').val(product);

    $('#txtProductName').val($("#" + ProdTR).find("TD").eq(3).html());

    $('#hfSQProdLineNo').val($("#" + prodNo + "_SQLineNo").text());

    $('#txtProdQty').val($("#" + ProdTR).find("TD").eq(4).html());

    $('#txtProdMRP').val($("#" + ProdTR).find("TD").eq(7).html());

    $('#ddlTransportMethod option:selected').text($("#" + ProdTR).find("TD").eq(14).html());

    $('#txtSalesPrice').val($("#" + ProdTR).find("TD").eq(8).html());

    $('#txtTransportCost').val($("#" + ProdTR).find("TD").eq(15).html());

    $('#txtSalesDiscount').val($("#" + ProdTR).find("TD").eq(16).html());

    $('#txtDeliveryDate').val($("#" + ProdTR).find("TD").eq(9).html());

    //var Isliquidprod = $("#hfIsLiquidProd").val();

    var isLiquidProd = $("#" + ProdTR).find("TD").eq(28).html();

    var isChkCommissionChecked = $("#" + ProdTR).find("TD").eq(17).html();

    var dropShipHtml = $("#" + ProdTR).find("TD").eq(22).text().trim();

    var VendorDrop = $("#" + ProdTR).find("TD").eq(32).html();

    if (dropShipHtml == 'Yes' || dropShipHtml == 'true') {

        $("#chkDropShipment").prop('checked', true);

        $('#chkDropShipment').change();

        $("#dvVendors").show();

        $('#hfSavedItemVendorNo').val(VendorDrop);

    } else {

        $("#chkDropShipment").prop('checked', false);

    }

    if (isChkCommissionChecked == false) {


        $('#chkIsCommission').prop('checked', false);

        $('#txtSalesPrice').prop('readonly', false);

        $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(19).html());

        $('#txtCommissionAmt').prop('disabled', false);

        $('#ddlCommissionPayable').prop('disabled', false);

    } else {

        $('#txtSalesPrice').prop('readonly', false);

        $('#chkIsCommission').prop('checked', true);

        $('#chkIsCommission').change();

        $('#ddlCommissionPerUnitPercent').val($("#" + ProdTR).find("TD").eq(17).html());

        $('#txtCommissionPercent').val($("#" + ProdTR).find("TD").eq(18).html());

        $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(19).html());

        //$('#ddlCommissionPayable').val($("#" + ProdTR).find("TD").eq(26).html());

    }



    if (isLiquidProd == "true") {

        $('#dvLiquidProdFields').css('display', 'block');

        $('#chkIsLiquidProd').prop('checked', true);

        $('#hfIsLiquidProd').val($("#" + ProdTR).find("TD").eq(28).html());

        $('#txtConcentratePercent').val($("#" + ProdTR).find("TD").eq(29).html());

        $('#txtNetWeight').val($("#" + ProdTR).find("TD").eq(30).html());

        $('#txtLiquidRate').val($("#" + ProdTR).find("TD").eq(31).html());

    }

    else {

        $('#chkIsLiquidProd').prop('checked', false);

        //$('#txtNetWeight').val('');

    }

    var commissionPayable = $("#" + ProdTR).find("TD").eq(26).html();

    if (commissionPayable != "") {

        $('#ddlCommissionPayable').val(commissionPayable);

    }

    else {

        $('#ddlCommissionPayable').val('-1');

    }

    $('#txtCreditDays').val($("#" + ProdTR).find("TD").eq(20).html());

    $('#txtMargin').val($("#" + ProdTR).find("TD").eq(11).html());

    $('#spnMarginPercent').val($("#" + ProdTR).find("TD").eq(30).html());

    $('#txtInterest').val($("#" + ProdTR).find("TD").eq(21).html());

    // Set InqProdLineNo from DataTable
    var inqProdLineNo = $("#" + prodNo + "_InqProdLineNo").text();
    $('#hfProdLineNo').val(inqProdLineNo || Line_No);
    $('#txtTotalCost').val($("#" + ProdTR).find("TD").eq(10).html());
    if ($("#" + prodNo + "_InqProdLineNo").val() != "") {
        $('#hfProdLineNo').val($("#" + prodNo + "_InqProdLineNo").val());
    }

    //$("#txtMRPprice" + ProdTR).find("TD").eq(33).text().trim();
    $('#txtMRPPrice').val($("#" + ProdTR).find("TD").eq(33).html());

}


function DeleteSQProd(ProdTR) {

    $("#" + ProdTR).remove();

}

function CalculateFormula() {
    if ($('#txtProductName').val() == "") {
    }
    else {
        var CostPrice = 0, Interest = 0, BasicPurchaseCost = 0, SalesPrice = 0;
        $('#txtTotalCost').val("0");
        if ($('#txtSalesDiscount').val() == "") {
            $('#txtSalesDiscount').val("0");
        }
        if ($('#txtCommissionAmt').val() == "") {
            $('#txtCommissionAmt').val("0");
        }
        if ($('#txtTransportCost').val() == "") {
            $('#txtTransportCost').val("0");
        }

        if ($('#txtProdQty').val() == "") {
            $('#txtProdQty').val("0");
        }
        if ($('#txtBasicPurchaseCost').val() == "") {
            BasicPurchaseCost = parseFloat("0");
        }
        else {
            BasicPurchaseCost = parseFloat($('#txtBasicPurchaseCost').val());
        }

        if ($('#txtSalesPrice').val() == "") {
            $('#txtSalesPrice').val("0");
            SalesPrice = parseFloat("0");
        }
        else {
            SalesPrice = parseFloat($('#txtSalesPrice').val());
        }

        if ($('#hfiteminvstatus').val() == "InInventory") {
            if ($('#ddlTransportMethod').val() == "TOPAY") {
                CostPrice = parseFloat($('#txtBasicPurchaseCost').val()) + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtInterest').val()); // + parseFloat($('#txtInsurance').val());
            }
            else if ($('#ddlTransportMethod').val() == "PAID") {
                CostPrice = parseFloat($('#txtBasicPurchaseCost').val()) + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtTransportCost').val()) + parseFloat($('#txtInterest').val()); // + parseFloat($('#txtInsurance').val());
            }

            $('#txtTotalCost').val(CostPrice.toFixed(2));
            $('#txtTotalCost').prop('disabled', true);

        }
        else {
            var TempCreditDays = 10;

            var creditDays = GetCreditDaysForNotInInventory();
            $('#txtCreditDays').val(creditDays);
            $('#txtCreditDays').prop('disabled', true);
            var InterestRate = parseFloat($('#hfInterestRate').val()) / 100;
            Interest = (BasicPurchaseCost * InterestRate) * (parseInt(creditDays) / 365);
            $('#txtInterest').val(parseFloat(Interest).toFixed(2));
            $('#txtInterest').prop('disabled', true);
            $('#spnInterestRate').text("");
            $('#spnInterestRate').text($('#hfInterestRate').val());
            if ($('#ddlTransportMethod').val() == "TOPAY") {

                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val())  + parseFloat($('#txtInsurance').val());

            }
            else if ($('#ddlTransportMethod').val() == "PAID") {

                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtTransportCost').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val()) + parseFloat($('#txtInsurance').val());

            }

            $('#txtTotalCost').val(CostPrice.toFixed(2));
            $('#txtTotalCost').prop('disabled', true);
        }

        $('#txtMargin').val((parseFloat($('#txtSalesPrice').val()) - parseFloat($('#txtTotalCost').val())).toFixed(2));
        $('#txtMargin').prop('disabled', true);
        if (SalesPrice > 0) {
            if ($('#txtTotalCost').val().length > 0 && parseFloat($('#txtTotalCost').val()) > 0) {
                $('#spnMarginPercent').text(((parseFloat($('#txtMargin').val()) * 100) / parseFloat($('#txtTotalCost').val())).toFixed(2) + " %");
            }
            else {
                $('#spnMarginPercent').text("0 %");
            }
        }
        else {
            $('#spnMarginPercent').text("0 %");
        }

    }
}

function ShortcloseSQProd(SQProdLineNo) {
    $('#modalShortclose').css('display', 'block');
    $('#ShortcloseTitle').text("Shortclose Sales Quote Product");
    $('#hfShortcloseType').val("SalesQuoteProd");
    $('#hfSQProdLineNoForShortclose').val(SQProdLineNo);
    BindShortcloseReason();
}

var interestRate = 0;

function GetInterestRate() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetInterestRate',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {
                    $('#lblInterestRate').text(data.PCPL_Interest_Rate_Percent);
                    $('#hfInterestRate').val(data.PCPL_Interest_Rate_Percent);
                    $('#spnInterestRate').text(data.PCPL_Interest_Rate_Percent);
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function AdditionalQtyChange() {

    if ($('#hfiteminvstatus').val() == "InInventory") {

        if ($('#txtAdditionalQty').val() > 0) {
            $('#txtAdditionalQty').attr("readonly", true);
        }

        let salesQuoteDetails = {};

        salesQuoteDetails.Qty = $('#txtProdQty').val();
        salesQuoteDetails.AdditionalQty = $('#txtAdditionalQty').val();

        if ($('#txtAdditionalQty').val() > 0) {
            salesQuoteDetails.MRPAmt = ($('#txtProdMRP').val() * $ ('#txtAdditionalQty').val());
            salesQuoteDetails.BasicPriceAmt = ($('#txtBasicPurchaseCost').val() * $('#txtAdditionalQty').val());
            //salesQuoteDetails.BasicPriceAmt = ($('#txtMRPPrice').val() * $('#txtAdditionalQty').val());
            salesQuoteDetails.ExciseAmt = ($('#txtExcise').val() * $('#txtAdditionalQty').val());
            salesQuoteDetails.QuantityDiscount = ($('#txtAdditionalQty').val() * $('#hfQuantityDiscount').val());
            salesQuoteDetails.TradeDiscount = ($('#txtAdditionalQty').val() * $('#hfTradeDiscount').val());
            salesQuoteDetails.ConsigneeDiscount = ($('#txtAdditionalQty').val() * $('#hfConsigneeDiscount').val());
            salesQuoteDetails.AdditionalSalesDiscount = ($('#txtAdditionalQty').val() * $('#txtSalesDiscount').val());
            salesQuoteDetails.AdditionalCommission = ($('#txtAdditionalQty').val() * $('#txtCommissionAmt').val());
            salesQuoteDetails.AdditionalTransportCost = ($('#txtAdditionalQty').val() * $('#txtTransportCost').val());

            $('#hfAdditionalCreditDays').val($('#txtAdditionalQty').val() * $('#hfCreditDays').val());
            $('#hfAdditionalAED').val($('#txtAdditionalQty').val() * $('#hfAEDAmt').val());
            salesQuoteDetails.InterestAdditionalAmt = ($('#txtAdditionalQty').val() * $('#lblInterest').val());
            salesQuoteDetails.AdditionalInsurance = ($('#txtAdditionalQty').val() * $('#txtInsurance').val());
            salesQuoteDetails.AdditionalTotalCost = (salesQuoteDetails.BasicPriceAmt + salesQuoteDetails.AdditionalSalesDiscount + salesQuoteDetails.AdditionalCommission + salesQuoteDetails.InterestAdditionalAmt + salesQuoteDetails.AdditionalInsurance + salesQuoteDetails.AdditionalTransportCost);

            if ($('#ddlExcise').val() == "Exclusive") {
                salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost - salesQuoteDetails.ExciseAmt;
            }
            else if ($('#ddlExcise').val() == "Inclusive") {

                if ($('#ddlSCVD').val() == "YES") {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost - $('#hfAdditionalAED').val();
                }
                else if ($('#ddlSCVD').val() == "NO") {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
                }
                else {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
                }
            }
            else {
                salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
            }

            salesQuoteDetails.AdditionalMargin = ($('#txtAdditionalQty').val() * $('#txtMargin').val());
            salesQuoteDetails.AdditionalSalesPrice = ($('#txtAdditionalQty').val() * $('#txtSalesPrice').val());
            salesQuoteDetails.AdditionalTaxGroupAmount = ($('#txtAdditionalQty').val() * $('#lblTaxGroupAmount').val());
            salesQuoteDetails.AdditionalTotalSalesPrice = ($('#txtAdditionalQty').val() * $('#txtTotalSales').val());

            if ($('#ddlNoSeries').val() == "-1") {
                salesQuoteDetails.LocationName = "";
                salesQuoteDetails.Location_Code = "";
            }
            else {
                salesQuoteDetails.LocationName = $("#ddlNoSeries option:selected").text();
                salesQuoteDetails.Location_Code = $('#ddlNoSeries').val();
            }

            salesQuoteDetails.EntryNo = "";
            salesQuoteDetails.SupplierName = "";
            salesQuoteDetails.ManufactureName = "";
            salesQuoteDetails.ManufacturingExciseAmount = 0;
            salesQuoteDetails.DealerName = "";
            salesQuoteDetails.DealerExciseAmount = 0;
            salesQuoteDetails.AvailableQty = 0;
            salesQuoteDetails.RequiredQty = $('#txtAdditionalQty').val();
            salesQuoteDetails.UOM = $('#hfUOM').val();
            salesQuoteDetails.Order_Date = "";
            salesQuoteDetails.DocumentNo = "";
            salesQuoteDetails.ManufacturerQty = 0;
            salesQuoteDetails.Prod_No = $('#hfProdNo').val();
            salesQuoteDetails.ProductName = $('#txtProductName').val();

            if ($('#ddlPackingStyle').val() == "-1") {
                salesQuoteDetails.PackagingStyle = "";
            }
            else {
                salesQuoteDetails.PackagingStyle = $('#ddlPackingStyle').val();
            }

            salesQuoteDetails.SalesDiscount = salesQuoteDetails.AdditionalSalesDiscount;

            if ($('#ddlCommissionPayable').val() == "-1") {
                salesQuoteDetails.Commission = "";
            }
            else {
                salesQuoteDetails.Commission = $('#ddlCommissionPayable').val();
            }

            salesQuoteDetails.CommissionAmount = salesQuoteDetails.AdditionalCommission;

            if ($('#ddlIncoTerms').val() == "-1") {
                salesQuoteDetails.IncoTerms = "";
            }
            else {
                salesQuoteDetails.IncoTerms = $('#ddlIncoTerms').val();
            }

            if ($('#ddlSCVD').val() == "-1") {
                salesQuoteDetails.SVCD = "";
            }
            else {
                salesQuoteDetails.SVCD = $('#ddlSCVD').val();
            }

            if ($('#ddlTransportMethod').val() == "-1") {
                salesQuoteDetails.Transport = "";
            }
            else {
                salesQuoteDetails.Transport = $('#ddlTransportMethod').val();
            }

            salesQuoteDetails.TransportAmount = salesQuoteDetails.AdditionalTransportCost;
            salesQuoteDetails.TotalCost = salesQuoteDetails.AddTotalCost;
            salesQuoteDetails.PaymentTerms = $('#ddlPaymentTerms').val();
            salesQuoteDetails.Margin = $('#txtMargin').val();
            salesQuoteDetails.CreditDays = $('#hfAdditionalCreditDays').val();
            salesQuoteDetails.SalesPrice = salesQuoteDetails.AdditionalSalesPrice;
            salesQuoteDetails.Insurance = salesQuoteDetails.AdditionalInsurance;

            if ($('#ddlTaxGroup').val() == "-1") {
                salesQuoteDetails.TaxGroups = "";
            }
            else {
                salesQuoteDetails.TaxGroups = $('#ddlTaxGroup').val();
            }

            salesQuoteDetails.Interest = salesQuoteDetails.InterestAdditionalAmt;

            if ($('#spnInterestRate').text() == "") {
                salesQuoteDetails.InterestRate = "0"
            }
            else {
                salesQuoteDetails.InterestRate = $('#spnInterestRate').text();
            }

            salesQuoteDetails.TaxGroupAmount = salesQuoteDetails.AdditionalTaxGroupAmount;
            salesQuoteDetails.TotalSalesPrice = salesQuoteDetails.AdditionalTotalSalesPrice;
            salesQuoteDetails.Type = "InInventoryAdditionalQty";

            if ($('#ddlExcise').val() == "-1") {
                salesQuoteDetails.ExciseType = "";
            }
            else {
                salesQuoteDetails.ExciseType = $('#ddlExcise').val();
            }

            salesQuoteDetails.MRP = salesQuoteDetails.MRPAmt;

            salesQuoteDetails.InquiryType = $('#ddlInquiryType').val();
            //salesQuoteDetails.CustomerName = $('#txtCustomerName').val();
            salesQuoteDetails.Sell_to_Customer_No = $('#hfCustomerNo').val();

            if ($('#ddlContactName').val() == "-1") {
                salesQuoteDetails.Sell_to_Contact = "";
            }
            else {
                salesQuoteDetails.Sell_to_Contact = $('#ddlContactName').val();
            }

            salesQuoteDetails.SalesQuoteDate = $('#txtSalesQuoteDate').val();
            salesQuoteDetails.ConsigneeAddress = $("#ddlConsigneeAddress option:selected").text();
            salesQuoteDetails.DeliveryDate = $('#txtDeliveryDate').val();
            salesQuoteDetails.Remarks = "";


            var jsonSalesQuoteDetails = JSON2.stringify(salesQuoteDetails);

            $.ajax({
                type: "POST",
                url: "/SPSalesQuotes/AddUpdateOnAddQtyChange",
                data: jsonSalesQuoteDetails,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    alert('In Ajax');
                }
            });

        }
    }
}

function PaymentTermsChange() {

    if ($('#ddlPaymentTerms').val() != "-1") {

        if ($('#hfiteminvstatus').val() == "InInventory") {

            var IntRate = 0, Interest = 0, InterestAmt = 0, TotalRequiredQty = 0;
            var CreditNoOfDays = 0;
            var CreditDays = 0;

            IntRate = parseFloat($('#hfInterestRate').val());

            $.ajax(
                {
                    url: '/SPSalesQuotes/GetDetailsOfInInventoryLineItems',
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (data) {

                        $.each(data, function (index, item) {

                            CreditNoOfDays = parseFloat(GetCreditDaysForINInventory(item.EntryNo));
                            CreditDays = CreditDays + parseInt(parseInt(CreditNoOfDays) * parseFloat(item.RequiredQty));
                            Interest = parseFloat(item.BasicPrice) * (IntRate / 100) * (parseFloat(parseInt(CreditNoOfDays)) / 365);
                            InterestAmt = InterestAmt + parseFloat(Interest);
                            TotalRequiredQty = TotalRequiredQty + parseFloat(item.RequiredQty);
                        });

                        $('#txtCreditDays').val(parseInt(CreditDays / parseFloat(TotalRequiredQty)));
                        $('#txtInterest').val(parseFloat(InterestAmt) / parseFloat(TotalRequiredQty));
                        $('#spnInterestRate').text(IntRate);

                    },
                    error: function () {
                        //alert("error");
                    }
                }
            );
        }
        if ($('#ddlPaymentTerms').val() != null) {
            const PaymentTermsDetails = $('#ddlPaymentTerms').val().split('_');
            if (PaymentTermsDetails[1] != "") {
                var PaymentTermsDays = PaymentTermsDetails[1].substring(0, PaymentTermsDetails[1].length - 1);
                $('#hfPaymentTermsDays').val(parseInt(PaymentTermsDays));
            }
        }
        CalculateFormula();
        ////UpdateValueForTotalCost();

    }
    else {
        $('#hfInterestAmt').val("0");
        $('#txtCreditDays').val("0");
        $('#txtInterest').val("0");
    }
}

function GetCurrentDate() {

    const date = new Date();

    let day = date.getDate();
    if (day <= 9) {
        day = "0" + day;
    }

    let month = date.getMonth() + 1;
    if (month <= 9) {
        month = "0" + month;
    }

    let year = date.getFullYear();

    let curDate = year + '-' + month + '-' + day;
    return curDate;

}
function commaSeparateNumber(val) {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
    }
    return val;
}

function updateInquiryNotifcationStatus() {

    if ($('#lblInqNo').text() != "") {

        $.post(
            apiUrl + 'updateInquiryNotifcationStatus?InqNo=' + $('#lblInqNo').text(),
            function (data) {

            }
        );
    }
}

function GetSQNoFromInqNo(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    var SQNo = "";

    $.get(apiUrl + 'GetSQNoFromInqNo?InqNo=' + InqNo, function (data) {

        if (data != "") {
            SQNo = data;
        }

    });

    return SQNo;
}

function CheckFieldValues() {

    var errMsg = "";

    if ($('#ddlLocations').val() == "-1" || $('#txtSalesQuoteDate').val() == "" || $('#txtCustomerName').val() == "" || $('#ddlContactName').val() == "-1" ||
        $('#ddlPaymentTerms').val() == "-1" || $('#ddlIncoTerms').val() == "-1" || $('#txtSQValidUntillDate').val() == "") {
        errMsg = "Please Fill Details";
    }
    else if ($('#txtSalesQuoteDate').val() > $('#txtSQValidUntillDate').val()) {
        errMsg = "Sales quote valid untill date should be date after sales quote date";
    }
    else if ($('#tblProducts').text().trim() == "") {

        errMsg = "Please Add Product Details";
    }

    return errMsg;
}

function CheckCPersonFieldValues() {

    var errMsg = "";

    if ($('#txtCPersonName').val() == "" || $('#txtCPersonMobile').val() == "" || $('#txtCPersonEmail').val() == "" || $('#ddlDepartment').val() == "-1" ||
        $('#txtJobResponsibility').val() == "") {
        errMsg = "Please Fill Details";
    }

    return errMsg;
}

function CheckNewBilltoAddressValues() {

    var errMsg = "";

    if ($('#txtNewShiptoAddCode').val() == "" || $('#txtNewShiptoAddress').val() == "" || $('#txtNewShiptoAddPostCode').val() == "" ||
        $('#ddlNewShiptoAddArea').val() == "-1" || $('#txtNewShiptoAddState').val() == "" || $('#txtNewShiptoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewShiptoAddGSTNo').val().length > 0 && $('#txtNewShiptoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewShiptoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function CheckNewDeliverytoAddressValues() {

    var errMsg = "";

    if ($('#txtNewJobtoAddCode').val() == "" || $('#txtNewJobtoAddress').val() == "" || $('#txtNewJobtoAddPostCode').val() == "" ||
        $('#ddlNewJobtoAddArea').val() == "-1" || $('#txtNewJobtoAddState').val() == "" || $('#txtNewJobtoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewJobtoAddGSTNo').val().length > 0 && $('#txtNewJobtoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewJobtoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function ResetQuoteLineDetails() {

    if ($('#tblProducts tr').length > 0) {

        $('#ddlIncoTerms').prop('disabled', true);

        $('#ddlPaymentTerms').prop('disabled', true);

    }
    $('#txtProductName').val("");
    $('#ddlPackingStyle').empty();
    $('#ddlPackingStyle').append("<option value='-1'>---Select---</option>");
    $('#ddlPackingStyle').val("-1");
    $('#chkIsLiquidProd').prop('checked', false);
    $('#txtConcentratePercent').val("");
    $('#txtNetWeight').val("");
    $('#txtLiquidRate').val("");
    $('#lblGrossWeight').text("");
    $('#dvLiquidProdFields').css('display', 'none');
    $('#hfIsLiquidProd').val("false");
    $('#txtProdQty').val("");
    $('#txtBasicPurchaseCost').val("");
    $('#txtBasicPurchaseCost').prop('disabled', false);
    $('#ddlIncoTerms').change();
    $('#txtUOM').val("");
    $('#txtTransportCost').val("");
    $("#hfProdNo").val('');
    $('#txtDeliveryDate').val("");
    $('#txtSalesDiscount').val("");
    $('#ddlCommissionPerUnitPercent').val("-1");
    $('#txtCommissionPercent').val("");
    $('#txtCommissionAmt').val("");
    $('#chkIsCommission').prop('checked', false);
    $('#ddlCommissionPerUnitPercent, #txtCommissionPercent, #txtCommissionAmt').prop('disabled', true);
    $('#ddlCommissionPayable').val("-1");
    $('#ddlCommissionPayable').prop('disabled', true);
    $('#txtMargin').val("");
    $('#txtMargin').prop('disabled', false);
    $('#spnMarginPercent').text("");
    $('#txtInterest').val("");
    $('#spnInterestRate').val("0");
    $('#txtInterest').prop('disabled', false);
    $('#txtTotalCost').val("");
    $('#txtCreditDays').val("0");
    $('#txtCreditDays').prop('disabled', false);
    $('#txtSalesPrice').val("");
    $('#chkDropShipment').prop('checked', false);
    $('#dvVendors').css('display', 'none');
    $('#ddlItemVendors').empty();
    $('#ddlItemVendors').append("<option value='-1'>---Select---</option>");
    $('#ddlItemVendors').val('-1');
    $('#txtMRPPrice').val("");
    $('#txtMRPPrice').prop('disabled', true);


    $('#hfProdNoEdit').val("");
    $('#hfUnitPriceEdit').val("");
    $('#hfSavedPackingStyle').val("");
    $('#hfSavedTotalUnitPrice').val("");
    $('#hfSavedMargin').val("");

}



function AddReqQty() {

    $('#txtProdQty').val($('#txtReqQty').val());
    $('#txtBasicPurchaseCost').val($('#txtMrpFromILE').val());
    $('#txtMRPPrice').val($('#txtMrpFromILE').val());
    //alert($('#txtReqQty').val());
    $('#btnClose,.btn-close').click();

}

function BindVendors() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetVendorsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCommissionPayable option').remove();
                var vendorsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    vendorsOpts += "<option value='" + item.No + "'>" + item.Name + "</option>";
                });

                $('#ddlCommissionPayable').append(vendorsOpts);
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function GenerateCostSheet(ItemNo, ItemName) {

    if ($('#hfSalesQuoteResDetails').val() == "") {

        var errMsg = "Fill cost sheet details after create sales quote";
        ShowErrMsg(errMsg);
    }
    else {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        const SQDetails = $('#hfSalesQuoteResDetails').val().split(',');
        var ItemLineNo;

        var SalesQuoteNo = SQDetails[0];

        for (var a = 0; a < SQDetails.length; a++) {

            if (SQDetails[a].includes(ItemNo)) {

                const SQLineDetails = SQDetails[a].split('_');
                ItemLineNo = parseInt(SQLineDetails[1]);
            }

        }

        $('#divImage').show();
        $.get(apiUrl + "GenerateCostSheet?SQNo=" + SalesQuoteNo + "&ItemLineNo=" + ItemLineNo, function (data) {

            var responseMsg = data;
            $('#divImage').hide();
            $('#modalCostSheetMsg').css('display', 'block');
            $('#dvCostSheetMsg').css('display', 'block');
            if (responseMsg.includes("Error : ")) {

                const errDetails = responseMsg.split(':');
                $('#resMsg').text(errDetails[1].trim());
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-2.png');
                $('#hfCostSheetFlag').val("false");
            }
            else {

                $('#hfItemNo').val(ItemNo);
                $('#hfItemName').val(ItemName);
                $('#resMsg').text(responseMsg);
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
                $('#hfCostSheetFlag').val("true");
            }

        });

    }

}

function showCostSheetDetails(ItemNo, ItemName) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    if ($('#hfSalesQuoteResDetails').val() == "") {

        var errMsg = "Fill cost sheet details after create sales quote";
        ShowErrMsg(errMsg);
    }
    else {

        const SQDetails = $('#hfSalesQuoteResDetails').val().split(',');
        var ItemLineNo;

        var SalesQuoteNo = SQDetails[0];

        for (var a = 0; a < SQDetails.length; a++) {

            if (SQDetails[a].includes(ItemNo)) {

                const SQLineDetails = SQDetails[a].split('_');
                ItemLineNo = parseInt(SQLineDetails[1]);
            }

        }

        var salesprice, totalunitprice, margin;

        $.get(apiUrl + "GetCostSheetDetails?SQNo=" + SalesQuoteNo + "&ItemLineNo=" + ItemLineNo, function (data) {

            var detailsOpt = "";
            $('#tblCostSheetDetails').empty();
            if (data.length == 0) {
                GenerateCostSheet(ItemNo, ItemName);
            }
            else {

                $('#modalCostSheet').css('display', 'block');
                $('#lblCostSheetSQNo').text(SalesQuoteNo);
                $('#lblCostSheetItemLineNo').text(ItemLineNo);
                $('#lblCostSheetItemName').text(ItemName);

                $.each(data, function (index, item) {

                    detailsOpt += "<tr><td hidden>" + item.TPTPL_Line_No + "</td><td>" + item.TPTPL_Item_Charge + "</td><td>" + item.TPTPL_Description + "</td><td id=\"" + item.TPTPL_Line_No + "_Qty\">" + item.TPTPL_Quantity + "</td>" +
                        "<td><input id=\"" + item.TPTPL_Line_No + "_CostUnitPrice\" onchange='calculateCostSheetDetails(\"" + item.TPTPL_Line_No + "_Qty\",\"" + item.TPTPL_Line_No + "_CostUnitPrice\",\"" + item.TPTPL_Line_No + "_Amount\",\"" + item.TPTPL_Line_No + "_Revenue\")' type='text' value=\"" + item.TPTPL_Rate_per_Unit + "\" class='form-control' /></td>" +
                        "<td id=\"" + item.TPTPL_Line_No + "_Amount\">" + item.TPTPL_Amount + "</td><td hidden id=\"" + item.TPTPL_Line_No + "_Revenue\">" + item.TPTPL_Revenue + "</td><td hidden><input type='checkbox' id=\"" + item.TPTPL_Line_No + "_PostToGL\" /></td>" +
                        "<td hidden><input type='checkbox' id=\"" + item.TPTPL_Line_No + "_Revenue\" /></td></tr >";

                    salesprice = item.SalesPrice;
                    totalunitprice = item.TotalUnitPrice;
                    margin = item.Margin;

                });

                $('#tblCostSheetDetails').append(detailsOpt);
                $('#lblCostSheetSalesPrice').text(salesprice);
                $('#lblCostSheetTotalUnitPrice').text(totalunitprice);
                $('#lblCostSheetMargin').text(margin);

            }

        });

    }

}

function calculateCostSheetDetails(Qty, UnitPrice, Amount, ItemRevenue) {

    var TotalUnitPrice = parseFloat($('#lblCostSheetTotalUnitPrice').text());
    if ($("#" + ItemRevenue).text() == true || $("#" + ItemRevenue).text() == "true") {

        TotalUnitPrice -= parseFloat($("#" + UnitPrice).val());
    }
    else if ($("#" + ItemRevenue).text() == false || $("#" + ItemRevenue).text() == "false") {

        TotalUnitPrice += parseFloat($("#" + UnitPrice).val());
    }

    $("#" + Amount).text($("#" + Qty).text() * $("#" + UnitPrice).val());
    $('#lblCostSheetTotalUnitPrice').text(parseFloat(TotalUnitPrice));
    $('#lblCostSheetMargin').text(parseFloat($('#lblCostSheetSalesPrice').text()) - parseFloat($('#lblCostSheetTotalUnitPrice').text()));
}

function CheckCostSheetDetails() {

    var errMsg = "";
    $('#tblCostSheetDetails tr').each(function () {

        var row = $(this)[0];
        var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();
        if (RatePerUnit == "") {
            errMsg = "Please Fill Cost Per Unit In All Charge Item";
        }

    });
    return errMsg;
}

function GetCreditDaysForNotInInventory() {

    if ($('#ddlPackingStyle').val() != "-1") {
        const packingStyleDetails = $('#ddlPackingStyle').val().split('_');
        $('#hfPurchaseDays').val(parseInt(packingStyleDetails[2]));
    }

    return ($('#hfPaymentTermsDays').val() - $('#hfPurchaseDays').val());

}

function GetDetailsByCode(pincode) {

    pincode = jQuery("#hfPostCode").val();

    if (pincode != "") {

        $.ajax(
            {
                url: '/SPInquiry/GetAreasByPincodeForDDL?Pincode=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    if (data.length > 0) {

                        if ($('#hfAddNewDetails').val() == "BillToAddress") {

                            $('#ddlNewShiptoAddArea').empty();
                            $('#ddlNewShiptoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewShiptoAddArea");
                            });

                        }

                        if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

                            $('#ddlNewJobtoAddArea').empty();
                            $('#ddlNewJobtoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewJobtoAddArea");
                            });

                        }

                    }
                    else {
                        $('#ddlNewShiptoAddArea, #ddlNewJobtoAddArea').empty();
                        BindArea();
                    }
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );

    }
}

function ResetCPersonDetails() {

    $('#txtCPersonName').val("");
    $('#txtCPersonMobile').val("");
    $('#txtCPersonEmail').val("");
    $('#txtJobResponsibility').val("");
    BindDepartment();
    $('#chkAllowLogin').prop('checked', false);
    $('#chkEnableOTPOnLogin').prop('checked', false);

}

function ResetNewBillToAddressDetails() {

    $('#txtNewShiptoAddCode').val("");
    $('#txtNewShiptoAddress').val("");
    $('#txtNewShiptoAddress2').val("");
    $('#txtNewShiptoAddPostCode').val("");
    $('#txtNewShiptoAddGSTNo').val("");
    $('#txtNewShiptoAddCountryCode').val("");
    $('#txtNewShiptoAddState').val("");
    $('#txtNewShiptoAddDistrict').val("");
    $('#txtNewShiptoAddCity').val("");
    $('#ddlNewShiptoAddArea').val("-1");

}

function ResetNewDeliveryToAddressDetails() {

    $('#txtNewJobtoAddCode').val("");
    $('#txtNewJobtoAddress').val("");
    $('#txtNewJobtoAddress2').val("");
    $('#txtNewJobtoAddPostCode').val("");
    $('#txtNewJobtoAddGSTNo').val("");
    $('#txtNewJobtoAddCountryCode').val("");
    $('#txtNewJobtoAddState').val("");
    $('#txtNewJobtoAddDistrict').val("");
    $('#txtNewJobtoAddCity').val("");
    $('#ddlNewJobtoAddArea').val("-1");

}

function BindShortcloseReason() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetShortcloseReasons',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var opts = "<option value='-1'>---Select---</option>";

                $.each(data, function (index, item) {
                    opts += "<option value='" + item.Entry_No + "'>" + item.Short_Close_Reason + "</option>";
                });

                $('#ddlShortcloseReason').append(opts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function BindJustificationDetails(LoggedInUserRole, ContactCompanyNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesQuoteJustificationDetails?LoggedInUserRole=' + LoggedInUserRole + '&CCompanyNo=' + ContactCompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var TROpts = "";

                $.each(data, function (index, item) {
                    TROpts += "<tr><td>" + item.PCPL_Target_Date + "</td><td>" + item.No + "</td><td>" + item.WorkDescription + "</td></tr>";
                });

                $('#tblSQJustificationDetails').append(TROpts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function GetAndFillCompanyIndustry(ContactCompanyNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetCompanyIndustry?CCompanyNo=' + ContactCompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {
                    $('#tdTraderMfg').text(data);
                }

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function ApproveRejectSQ(Action, RejectRemarks) {

    if (RejectRemarks == null || RejectRemarks == "") {
        $('#btnApproveSpinner').show();
    }
    else {
        $('#btnRejectSpinner').show();
    }

    let SQNos = new Array();
    var a = 0;
    var str = $('#hfSalesQuoteNo').val() + ":" + $('#hdnApprovalFor').val() + ":" + $('#hdnSalespersonEmail').val() + ",";
    SQNosAndApprovalFor_ = str;

    var UserRoleORReportingPerson = "";

    if ($('#hdnLoggedInUserRole').val() == "Finance") {
        UserRoleORReportingPerson = "Finance";
    }
    else {
        UserRoleORReportingPerson = "ReportingPerson";
    }

    $.post(apiUrl + "SQApproveReject?SQNosAndApprovalFor=" + SQNosAndApprovalFor_ + "&LoggedInUserNo=" + $('#hdnLoggedInUserNo').val() + "&Action=" + Action + "&UserRoleORReportingPerson=" + UserRoleORReportingPerson + "&RejectRemarks=" + RejectRemarks + "&LoggedInUserEmail=" + $('#hdnLoggedInUserEmail').val(), function (data) {

        var resMsg = data;

        if (RejectRemarks == null || RejectRemarks == "") {
            $('#btnApproveSpinner').hide();
        }
        else {
            $('#btnRejectSpinner').hide();
        }

        if (resMsg == "True") {

            $('#modalApproveRejectMsg').css('display', 'block');
            if (Action == "Approve") {
                $('#lblApproveRejectMsg').text("Sales Quote Approved Successfully");
            }
            else if (Action == "Reject") {
                $('#modalRejectRemarks').css('display', 'none');
                $('#lblApproveRejectMsg').text("Sales Quote Rejected Successfully");
            }

        }
        else if (resMsg.includes("Error:")) {
            const resMsgDetails = resMsg.split(':');

            $('#modalErrMsg').css('display', 'block');
            $('#modalErrDetails').text(resMsgDetails[1]);
        }

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

function BindCSOutstandingDuelist() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetCSOutstandingDuelist',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                $("#tblCSOutstandingDuelist").empty();
                var TROpts = "";

                $.each(data, function (index, item) {
                    TROpts += "<tr>"+ "<td>" + item.Document_Type + "</td>"+ "<td>" + item.Bill_No + "</td>"+ "<td>" + item.Bill_Date + "</td>"+ "<td>" + item.Product_Name + "</td>"+ "<td>" + item.Terms + "</td>"+ "<td>" + item.Due_Date + "</td>"+ "<td>" + item.Invoice_Amount + "</td>"+ "<td>" + item.Remain_Amount + "</td>"+ "<td>" + item.Total_Days + "</td>"+ "<td>" + item.Overdue_Days + "</td>"+ "</tr>";
                });

                $('#tblCSOutstandingDuelist').append(TROpts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}