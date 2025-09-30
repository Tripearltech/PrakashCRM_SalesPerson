var apiUrl = $('#getServiceApiUrl').val() + 'SPFeedback/';
$(document).ready(function () {
    FeedBackQuestionList();
});

function FeedBackQuestionList() {

    $.ajax({
        url: '/SPFeedback/GetFeedBackQuestionList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblFeedBackQuestionList").empty();
            //$("#FeedbackLinesModal").modal('show');
            /*   $("#tblFeedbackLine").empty();*/
            if (data.length > 0) {
                var rowData = "";

                $.each(data, function (index, item) {

                    rowData += "<tr><td>" + item.Feedback_Question_No + "</td><td>" + item.Feedback_Question + "</td><td>" + item.Rating + "</td><td>" + item.Comments + "</td></tr>";
                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblFeedBackQuestionList").append(rowData);
        }
    });
}
           