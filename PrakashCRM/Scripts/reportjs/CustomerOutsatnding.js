var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    /*$('#btnGenerate').on('click', function () {
        GenerateOutstandingList();
    });*/

    BindOutstandingCustomerList();
});

// Outstanding Customer list
//function GenerateOutstandingList() {
//    $('#divImage').show();
//    var fromDate = $('#txtOutsFDate').val();
//    var toDate = $('#txtOutsTDate').val();

//    if (fromDate !== "" && toDate !== "") {
//        $.post(apiUrl + 'GenerateOutstandingList?FromDate=' + fromDate + '&ToDate=' + toDate,
//            function (data) {
//                if (data) {
//                    BindOutstandingCustomerList();
//                }
//            }
//        );
//    }
//}

function BindOutstandingCustomerList() {
    $.ajax({
        url: '/SPReports/GetOutstandingList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $('#tblOutstanding').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td></td><td>" + item.Location_Code + "</td><td>" + item.Salesperson_Code + "</td><td>" + item.CollectionuptoMTD + "</td><td>" + item.CollRecdforthePeriod + "</td><td>" + item.TotalCollectionRecdtilltoday + "</td><td>" + item.Overdueuptopreviousmonthdue + "</td></tr>";
                });
            } else {
                rowData = "<tr><td colspan='11' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblOutstanding').append(rowData);
        },
        error: function (xhr, status, error) {
            alert("Error fetching data: " + xhr.responseText);
        }
    });
}
