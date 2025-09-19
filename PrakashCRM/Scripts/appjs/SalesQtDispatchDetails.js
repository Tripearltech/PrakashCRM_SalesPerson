var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

$(document).ready(function () {
    $('.order-details-link').on('click', function (e) {
        e.preventDefault();

        // Get data attributes
        var invoice = $(this).data('invoice');
        var customer = $(this).data('customer');
        var vehicle = $(this).data('vehicle');
        var date = $(this).data('date');

        // Populate modal form fields
        $('#modalInvoiceNo').val(invoice);
        $('#modalCustomerName').val(customer);
        $('#modalVehicleNo').val(vehicle);
        $('#modalDate').val(date);

        // Show modal
        var myModal = new bootstrap.Modal(document.getElementById('orderDetailsModal'));
        myModal.show();
    });
});