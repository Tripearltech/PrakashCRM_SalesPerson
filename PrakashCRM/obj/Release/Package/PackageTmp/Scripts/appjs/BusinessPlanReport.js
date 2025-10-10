var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

$(document).ready(function () {

    BindBusinessPlanReport();

});


function BindBusinessPlanReport() {
    $.ajax(
        {
            url: '/SPBusinessPlan/GetBusinessReport',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                $("#tblbusinessreport").empty();
                var TROpts = "";

                $.each(data, function (index, item) {
                    TROpts += "<tr>"+ "<td>" + item.SalesPerson_Name + "</td>"+ "<td>" + item.Demand_Qty + "</td>"+ "<td>" + item.Target_Qty + "</td>"+ "<td>" + item.Sales_Qty + "</td>"+ "<td>" + item.Sales_Percentage_Qty + "%</td>"+ "<td>" + item.Target_Amt + "</td>"+ "<td>" + item.Sales_Amt + "</td>"+ "<td>" + item.Sales_Percentage_Amt + "%</td>"+ "</tr>";
                });

                $('#tblbusinessreport').append(TROpts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

//function onYearChange() {
//    const selectedYear = $('#ddlYear').val();
//    console.log("Selected Financial Year:", selectedYear);

//    let previousYear;
//    if (selectedYear === financialYearData.current) {
//        previousYear = financialYearData.previous;
//    } else if (selectedYear === financialYearData.previous) {
//        previousYear = `${parseInt(selectedYear.split('-')[0]) - 1}-${parseInt(selectedYear.split('-')[0])}`;
//    } else {
//        previousYear = financialYearData.current;
//    }

//    $('#pre-FY').text(previousYear);
//    $('#curr-FY').text(selectedYear);

//    BindBusinessPlanReport(previousYear, selectedYear);
//}