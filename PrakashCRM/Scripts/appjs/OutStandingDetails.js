var apiUrl = $('#getServiceApiUrl').val() + 'SPOutstandingPayment/';

$(document).ready(function () {
        loadOutstandingDetails();
});

function loadOutstandingDetails() {
    $.ajax({
        url: '/SPOutstandingPayment/GetOutstandingDetails',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            var tbody = $('#outstandingBody');
            tbody.empty();

            data.forEach(function (Item, index) {
                var ItemId = "Item" + Item.ItemId + "Rec"; // Dynamic Item Row ID
                var spRowClass = "Item" + Item.ItemId + "SPRecs"; // Dynamic SP Row Class

                // Item row
                var rowData = `
                <tr>
                    <td>${index + 1}</td>
                    <td><a id="${ItemId}" class="clsPointer"><i class="bx bx-plus-circle"></i></a></td><td>${Item.Customer_Name}</td><td>${Item.Document_No}</td><td>${Item.OverDays}</td><td>${Item.Original_Amount}</td><td>${Item.Remaining_Amount}</td><td>${Item.Received_Amount}</td><td>${Item.Total_Customer_Amt}</td><td>${Item.ACD_Amt}</td><td>${Item.Total_Collection_Amt}</td><td>${Item.LastSixMonths_Customer_No}</td>
                </tr>
            `;
                tbody.append(rowData);

                // Salesperson rows
                Item.Salespersons.forEach(function (sp) {
                    var spRow = `
                    <tr class="${spRowClass}" style="display:none">
                        <td></td>
                        <td></td>
                        <td>${sp.Name}</td>
                        <td>${sp.ColUptoMTD1}</td>
                        <td>${sp.DateOnTitle}</td>
                        <td>${sp.TotalCollection}</td>
                        <td>${sp.OverdueCM1}</td>
                        <td>${sp.First10Due}</td>
                        <td>${sp.Eleventh20Due}</td>
                        <td>${sp.TwentyFirst30Due}</td>
                        <td>${sp.TotalDueCurMonth}</td>
                        <td>${sp.Achievement}</td>
                    </tr>
                `;
                    tbody.append(spRow);
                });

                // Toggle click event
                $(document).off('click', '#' + ItemId).on('click', '#' + ItemId, function () {
                    $('.' + spRowClass).toggle();
                });
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading data:', error);
        }
    });
}
