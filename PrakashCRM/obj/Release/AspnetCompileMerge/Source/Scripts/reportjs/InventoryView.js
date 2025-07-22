var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {

    /*var today = new Date();
    var currentYear = today.getFullYear();
    var defaultDate = currentYear + "-04-01";
    var formattedToday = today.toISOString().split('T')[0];
    $('#txtInvFDate').val(defaultDate);
    $('#txtInvTDate').val(formattedToday);

    GenerateInvData();*/
    //BindInvBranchWiseTotals();

    $('#btnGenerate').on('click', function (fromDate, toDate) {
        GenerateInvData();
    })

});

function GenerateInvData() {

    $('#divImage').show();
    var fromDate = $('#txtInvFDate').val();
    var toDate = $('#txtInvTDate').val();

    if (fromDate !== "" && toDate !== "") {
        $.post(apiUrl + 'GenerateInvData?FromDate=' + fromDate + '&ToDate=' + toDate,
            function (data) {
                if (data) {

                    BindInvBranchWiseTotals();

                }
            }
        );
    }

}

const ProductGroupsCode = {};
function BindInvBranchWiseTotals() {
    $.ajax({
        url: '/SPReports/GetBranchWiseTotal',
        type: 'GET',
        contentType: 'application/json',
        success: function (branches) {
            const $tbl = $('#tblInventroyDetails');
            $tbl.empty();

            if (!branches || branches.length === 0) {
                $tbl.append("<tr><td colspan='7' style='text-align:center;'>No Records Found</td></tr>");
                return;
            }

            branches.forEach((b, bi) => {
                const branchId = `InvBranch${bi}`;
                const branchRow = $(`
                    <tr data-branchindex="${bi}" class="branch-row">
                        <td><a href="#" class="clsPointer branch-toggle" data-target="${branchId}SPRecs"><i class="bx bx-plus-circle"></i></a></td>
                        <td>${b.LocationCode}</td><td>${b.Opening_LocationCode}</td><td>${b.Inward_LocationCode}</td><td>${b.Outward_LocationCode}</td><td>${b.Reserved_LocationCode}</td><td>${b.CLStock_LocationCode}</td>
                    </tr>
                    <tr id="${branchId}SPRecs" style="display:none;" data-type="product-groups"><td colspan="7"></td></tr>
                `);
                $tbl.append(branchRow);
            });

            $('#divImage').hide();
            // Branch Toggle
            $tbl.on('click', '.branch-toggle', function (e) {
                e.preventDefault();
                const $a = $(this);
                const $icon = $a.find('i');
                const $branchRow = $a.closest('tr');
                const bi = $branchRow.data('branchindex');
                const locCode = branches[bi].LocationCode;
                const targetId = $a.data('target');
                const $subRow = $(`#${targetId}`);
                const $nextRows = $branchRow.nextUntil('tr.branch-row');

                if ($subRow.is(':visible')) {
                    $nextRows.hide();
                    $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
                } else {
                    $subRow.show();
                    $subRow.empty();
                    $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');

                    const $productRows = $subRow.nextUntil('tr.branch-row');
                    if ($productRows.length === 0) {
                        loadProductGroups($branchRow, $subRow, locCode, bi);
                    } else {
                        $productRows.show();
                    }
                }
            });

            function loadProductGroups($branchRow, $subRow, locCode, bi) {
                $.ajax({
                    url: `/SPReports/GetInv_ProductGroupsWise?branchCode=${locCode}`,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (pgs) {
                        ProductGroupsCode[bi] = pgs;

                        if (!pgs || pgs.length === 0) {
                            const noGroupRow = `<tr data-type="product-group-empty"><td colspan="7" style="padding-left:20px;">No Product Groups</td></tr>`;
                            $branchRow.after(noGroupRow);
                            return;
                        }

                        let rowsHtml = '';
                        pgs.forEach((pg, pgi) => {
                            const pgId = `InvBranch${bi}_ProGroup${pgi}SPRecs`;
                            rowsHtml += `
                                <tr class="product-group-row" data-pgi="${pgi}">
                                    <td style="padding-left:20px;">
                                        <a href="#" class="clsPointer pg-toggle" data-target="${pgId}" data-branch="${bi}" data-pg="${pgi}">
                                            <i class="bx bx-plus-circle"></i>
                                        </a>
                                    </td>
                                    <td>${pg.Code}</td><td>${pg.Opening}</td><td>${pg.Inward}</td><td>${pg.Outward}</td><td>${pg.Reserved}</td><td>${pg.CLStock}</td>
                                </tr>
                                <tr id="${pgId}" class="item-row" style="display:none;"><td colspan="7"></td></tr>
                            `;
                        });
                        $branchRow.after(rowsHtml);
                    },
                    error: function () {
                        $subRow.html(`<td colspan="7" style="color:red;">Error loading product groups</td>`);
                    }
                });
            }

            // Product Group toggle handler
            $tbl.on('click', '.pg-toggle', function (e) {
                e.preventDefault();
                const $a = $(this);
                const $icon = $a.find('i');
                const pgId = $a.data('target');
                const branchIndex = $a.data('branch');
                const pgIndex = $a.data('pg');

                const $row = $(`#${pgId}`);
                const $itemRows = $row.nextUntil('tr.product-group-row, tr.branch-row');

                if ($row.is(':visible')) {
                    $itemRows.hide();
                    $row.hide();
                    $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
                } else {
                    $row.show();
                    $row.empty();
                    $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');

                    if ($itemRows.length === 0) {
                        loadItems(branchIndex, pgIndex, $row);
                    } else {
                        $itemRows.show();
                    }
                }
            });

            function loadItems(branchIndex, pgIndex, $itemRow) {
                const branchCode = branches[branchIndex].LocationCode;
                const pgCode = ProductGroupsCode[branchIndex][pgIndex].Code;

                $.ajax({
                    url: `/SPReports/GetInv_ItemWise?branchCode=${branchCode}&pgCode=${pgCode}`,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (items) {
                        if (!items || items.length === 0) {
                            const noItemsRow = `<tr class="item-row-empty"><td colspan="7" style="padding-left:40px;">No Item Records Found</td></tr>`;
                            $(`#InvBranch${branchIndex}_ProGroup${pgIndex}SPRecs`).after(noItemsRow);
                            return;
                        }
                        let itemRows = '';
                        items.forEach((it) => {
                            itemRows += `
                            <tr class="item-row-detail">
                                <td style='padding-left:40px;'>-</td>
                                <td>${it.ItemName}</td><td>${it.Opening_Item}</td><td>${it.Inward_Item}</td><td>${it.Outward_Item}</td><td>${it.Reserved_Item}</td><td>${it.CLStock_Item}</td>
                            </tr>`;
                        });
                        $(`#InvBranch${branchIndex}_ProGroup${pgIndex}SPRecs`).after(itemRows);
                    },
                    error: function () {
                        $itemRow.html("<td colspan='7' style='color:red;'>Error loading item-wise data</td>");
                    }
                });
            }
        },
        error: function (err) {
            alert("Error fetching branch data: " + err.responseText);
        }
    });
}