var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

$(document).ready(function () {
    BindSalespersonDropDwon();
    BindFinancialYears();

    // Refresh report when FY or salesperson changes
    $('#txtddlYear, #txtddlSalesPerson').on('change', function () {
        BindBusinessPlanReport();
    });
});

function getFinancialYears(financialYear) {
    let currentFinancialYear;

    if (financialYear) {
        const startYear = parseInt(financialYear.split('-')[0]);
        currentFinancialYear = `${startYear}-${startYear + 1}`;
    } else {
        const currentDate = new Date();
        const currentMonth = currentDate.getMonth();
        const currentYear = currentDate.getFullYear();

        if (currentMonth < 3) {
            currentFinancialYear = `${currentYear - 1}-${currentYear}`;
        } else {
            currentFinancialYear = `${currentYear}-${currentYear + 1}`;
        }
    }

    const baseYear = parseInt(currentFinancialYear.split('-')[0]);

    return {
        current: `${baseYear}-${baseYear + 1}`,
        previous: `${baseYear - 1}-${baseYear}`,
        next: `${baseYear + 1}-${baseYear + 2}`
    };
}

function bindYearDropdown(financialYearData) {
    $('#txtddlYear').empty();
    const yearOpts = `
        <option value='${financialYearData.previous}'>${financialYearData.previous}</option>
        <option value='${financialYearData.current}' selected>${financialYearData.current}</option>
        <option value='${financialYearData.next}'>${financialYearData.next}</option>
    `;
    $('#txtddlYear').append(yearOpts);
}

function bindFYHeader(financialYearData) {
    $('#pre-FY').text(financialYearData.previous);
    $('#curr-FY').text(financialYearData.current);
}
function BindFinancialYears() {
    const fyData = getFinancialYears();
    bindYearDropdown(fyData);
    bindFYHeader(fyData);
    BindBusinessPlanReport();

    function BindBusinessPlanReport() {
        var selectedFY = $('#txtddlYear').val();
        var selectedSalesperson = $('#txtddlSalesPerson').val();

        $.ajax({
            url: '/SPBusinessPlan/GetBusinessReport',
            type: 'GET',
            data: { year: selectedFY, salesperson: selectedSalesperson },
            success: function (data) {
                var $tbody = $("#tblbusinessreport tbody");
                $tbody.empty();

                if (data && data.length > 0) {
                    $.each(data, function (index, item) {
                        var row = "<tr>"
                            + "<td>" + item.SalesPerson_Name + "</td>"
                            + "<td>" + item.Demand_Qty + "</td>"
                            + "<td>" + item.Target_Qty + "</td>"
                            + "<td>" + item.Sales_Qty + "</td>"
                            + "<td>" + item.Sales_Percentage_Qty + "%</td>"
                            + "<td>" + item.Target_Amt + "</td>"
                            + "<td>" + item.Sales_Amt + "</td>"
                            + "<td>" + item.Sales_Percentage_Amt + "%</td>"
                            + "</tr>";
                        $tbody.append(row);
                    });
                } else {
                    $tbody.append("<tr><td colspan='8' style='text-align:center;color:red;'>No records found</td></tr>");
                }
            },
            error: function () {
                alert("Error while fetching data.");
            }
        });
    }
    function BindSalespersonDropDwon() {
        $.ajax({
            url: '/SPBusinessPlan/GetSalespersonDropDwon',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                var $ddl = $('#txtddlSalesPerson');
                $ddl.empty();
                $ddl.append('<option value="-1">---Select---</option>');

                if (data && data.length > 0) {
                    $.each(data, function (i, item) {
                        $('<option>', {
                            value: item.Sales_PersonCode,
                            text: item.SalesPerson_Name
                        }).appendTo($ddl);
                    });

                    if ($("#hdnddlSalesPerson").val() != "") {
                        $ddl.val($("#hdnddlSalesPerson").val());
                    }
                }
            },
            error: function () {
                alert("Error while fetching salesperson data.");
            }
        });
    }
}