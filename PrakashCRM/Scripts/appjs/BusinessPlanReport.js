var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

$(document).ready(function () {
    BindSalespersonDropDwon();

    // Default report bind (optional)
    BindBusinessPlanReport('');

    // Search button click
    $('#btnSearch').on('click', function () {
        var selectedSalesPerson = $('#txtddlSalesPerson').val();

        if (selectedSalesPerson == "-1" || selectedSalesPerson == "" || selectedSalesPerson == null) {
            $('#lblDepartmentMsg').text('Please select a Sales Person.').show();
            return;
        } else {
            $('#lblDepartmentMsg').hide();
        }
        BindBusinessPlanReport(selectedSalesPerson);
    });
});



function BindBusinessPlanReport(salesPersonCode) {
    $.ajax({
        url: '/SPBusinessPlan/GetBusinessReport',
        type: 'GET',
        data: { salesPersonCode: salesPersonCode },
        traditional: true, // 🔹 ensures parameter passes correctly
        success: function (data) {
            $("#tblbusinessreport").empty();
            var TROpts = "";

            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    TROpts += "<tr>"
                        + "<td>" + item.SalesPerson_Name + "</td>"
                        + "<td>" + item.Demand_Qty + "</td>"
                        + "<td>" + item.Target_Qty + "</td>"
                        + "<td>" + item.Sales_Qty + "</td>"
                        + "<td>" + item.Sales_Percentage_Qty + "%</td>"
                        + "<td>" + item.Target_Amt + "</td>"
                        + "<td>" + item.Sales_Amt + "</td>"
                        + "<td>" + item.Sales_Percentage_Amt + "%</td>"
                        + "</tr>";
                });
            } else {
                TROpts = "<tr><td colspan='8' style='text-align:center;color:red;'>No records found</td></tr>";
            }

            $('#tblbusinessreport').append(TROpts);
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
            alert("Error while fetching data");
        }
    });
}

function BindSalespersonDropDwon() {
	$.ajax({
		url: '/SPBusinessPlan/GetSalespersonDropDwon',
		type: 'GET',
		contentType: 'application/json',
		success: function (data) {
			if (data.length > 0) {
				$('#txtddlSalesPerson').append($('<option value="-1">---Select---</option>'));
				$.each(data, function (i, data) {
					$('<option>',
						{
							value: data.Sales_PersonCode,
							text: data.SalesPerson_Name
						})
						.html(data.SalesPerson_Name).appendTo('#txtddlSalesPerson');
					});
				if ($("#hdnddlSalesPerson").val() != "") {
					$("#txtddlSalesPerson").val($("#hdnddlSalesPerson").val());
				}
			}
		},
		error: function (data1) {
			alert(data1);
		}
	});
}