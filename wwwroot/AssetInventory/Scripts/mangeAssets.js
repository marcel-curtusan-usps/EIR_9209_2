let assetsListTable = "assetsListTable";
let appData = {};
let currentTime = null;
let ianaTimeZone = "";
$(function () {

    createAssetsInventoryDataTable(assetsListTable);
    fetch('../api/ApplicationConfiguration/Configuration')
        .then(response => response.json())
        .then(data => {
            document.title = data.displayName;
            appData = data
            $('span[id=sitename]').text(data.displayName);
            ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
            $('span[id=moddatedisplay]').text(luxon.DateTime.local().setZone(ianaTimeZone).toFormat('yyyy-LL-dd T:ss'));

        })
        .catch(error => {
            console.error('Error:', error);
        });
    fetch('../api/Inventory/Inventory')
        .then(response => response.json())
        .then(data => {
            updateAssetsInventoryDataTable(data, assetsListTable);

        })
        .catch(error => {
            console.error('Error:', error);
        });
});
function createAssetsInventoryDataTable(table) {
    let arrayColums = [{
        "name": "",
        "barcode": "",
        "category": "",
        "status": "",
        "createDateTime": "",
        "notes": "",
        "action":""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/Name/i.test(key)) {
            tempc = {
                "title": 'Name',
                "mDataProp": key,
                "class": "text-center"
            }
        }
        else if (/Item/i.test(key)) {
            tempc = {
                "title": "Item Name",
                "mDataProp": key,
                "class": "text-center"
            }
        }
        else if (/category/i.test(key)) {
            tempc = {
                "title": "Category Type",
                "mDataProp": key,
                "class": "text-center"
            }
        }
        else if (/createDateTime/i.test(key)) {
            tempc = {
                "title": "Date",
                "width": "9%",
                "mDataProp": key,
                "class": "text-right",
                "mRender": function (data, type, full) {
                    return luxon.DateTime.fromISO(full.createDateTime).toFormat('yyyy-LL-dd T:ss');
                }
            }
        }
        else if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "width": "6%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    return '<button class="btn btn-light btn-sm mx-1 bi-pencil-square assetEdit" name="assetEdit"></button>' +
                        '<button class="btn btn-light btn-sm mx-1 bi-trash assetdelete" name="assetdelete"></button>'

                }
            }
        }
        else {
            tempc = {
                "title": capitalize_Words(key.replace(/\_/, ' ')),
                "mDataProp": key
            }
        }
        columns.push(tempc);
    });
    $('#' + table).DataTable({
        dom: 'Bfrtip',
        bFilter: false,
        buttons: [
            {
                text: '+ Add',
                className: 'btn btn-success m-2 float-end',
                action: function (e, dt, node, config) {
                    Promise.all([loadAddInventoryModal("","add")]);
                }
            }
        ],
        bdeferRender: true,
        paging: false,
        bPaginate: false,
        bAutoWidth: true,
        bInfo: false,
        destroy: true,
        aoColumns: columns,
        sorting: [[0, "asc"]],
        columnDefs: []
    });
    // Edit/remove record
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/assetEdit/ig.test(this.name)) {
            Promise.all([loadAddInventoryModal(row.data(),"update")]);
        }
        else if (/assetdelete/ig.test(this.name)) {
            Promise.all([removeInventory(row.data())]);
        }
    });
}
function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}
async function loadAssetsInventoryDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateAssetsInventoryDataTable(newdata, table) {
    try {
        let loadnew = true;
        if ($.fn.dataTable.isDataTable("#" + table)) {
            $('#' + table).DataTable().rows(function (idx, data, node) {
                if (data.id === newdata.id) {
                    loadnew = false;
                    $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
                }
            })
            if (loadnew) {
                loadAssetsInventoryDatatable(newdata, table);
            }
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}

function removeAssetsInventoryDataTable(removedata, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.id === removedata.id) {
                $('#' + table).DataTable().row(node).remove().draw();
            }
        });
    }
}
async function removeInventory(data) {
    try {
        $('#removeInventoryModalId').text('Removing Inventory Item: ' + data.name );
        $('button[id=removeInventory]').off().on('click', function () {
            // Make a fetch call to add the inventory
            fetch(`../api/Inventory/Delete?id=${data.id}`, {
                method: "DELETE"
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok ' + response.statusText);
                    }
                    return response.json();
                })
                .then(successdata => {
                    $('span[id=error_inTakeInventory]').text("Data has been updated");
                    setTimeout(function () {
                        $("#RemoveInventoryModal").modal('hide');
                        $('button[id=removeInventory]').prop('disabled', false);
                        removeAssetsInventoryDataTable(successdata, assetsListTable);
                    }, 500);
                })
                .catch(error => {
                    $('span[id=error_removeInventory]').text(error);
                    $('button[id=removeInventory]').prop('disabled', false);
                    console.log(error);
                });
        });
        $('#RemoveInventoryModal').modal('show');
    } catch (e) {

    }
}
async function loadAddInventoryModal(data, type) {
    let callmethod = "";
    if (type === "add") {
        callmethod = "POST",
        $('h5[id=inTakeInventoryLabel]').text('Add New Inventory');
        $('#inTakeInventoryModal').modal('show');
    }
    else if (type === "update") {
        callmethod = "PUT"
        $('h5[id=inTakeInventoryLabel]').text('Edit Inventory');
        $('#inTakeInventoryModal').modal('show');
    }
    $('input[id=itemName]').val(data.name);
    $('input[id=itemDescription]').val(data.description);
    $('input[id=itemCategory]').val(data.category),
        $('input[id=itemserialNumber]').val(data.serialNumber),
        $('input[id=notes]').val(data.notes),
        $('input[id=itemBarcode]').val(data.barcode),
        $('input[id=itemBleTag]').val(data.bleTag)
    $('button[id=inTakeInventorysubmit]').off().on('click', function () {
        $(this).prop('disabled', true);
        /* $('button[id=conntypevalue]')*/
        let jsonObject = {
            id: data.id,
            createDateTime: data.createDateTime,
            moddifyDateTime: data.moddifyDateTime,
            name: $('input[id=itemName]').val(),
            description: $('textarea[id=itemDescription]').val(),
            category: $('input[id=descValueID]').val(),
            serialNumber: $('input[id=itemserialNumber]').val(),
            notes: $('textarea[id=notes]').val(),
            barcode: $('input[id=itemBarcode]').val(),
            bleTag: $('input[id=itemBleTag]').val()
        };

        if (!$.isEmptyObject(jsonObject)) {
            // Make a fetch call to add the inventory
            fetch(`../api/Inventory/${type}`, {
                method: callmethod,
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(jsonObject)
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok ' + response.statusText);
                    }
                    return response.json();
                })
                .then(successdata => {
                    $('span[id=error_inTakeInventory]').text("Data has been updated");
                    setTimeout(function () {
                        $("#inTakeInventoryModal").modal('hide');
                        $('button[id=inTakeInventorysubmit]').prop('disabled', false);
                        updateAssetsInventoryDataTable(successdata, assetsListTable);
                    }, 500);
                })
                .catch(error => {
                    $('span[id=inTakeInventorysubmit]').text(error);
                    $('button[id=inTakeInventorysubmit]').prop('disabled', false);
                    console.log(error);
                });
        }
    });
}
// Mapping of standard time zone abbreviations to IANA time zones
const timeZoneMapping = {
    'PST': 'America/Los_Angeles',
    'PDT': 'America/Los_Angeles',
    'MST': 'America/Denver',
    'MDT': 'America/Denver',
    'CST': 'America/Chicago',
    'CDT': 'America/Chicago',
    'EST': 'America/New_York',
    'EDT': 'America/New_York',
    'HST': 'Pacific/Honolulu',
    'AKST': 'America/Anchorage',
    'AKDT': 'America/Anchorage',
    'AEST': 'Australia/Sydney',
    'AEDT': 'Australia/Sydney',
    'ACST': 'Australia/Adelaide',
    'ACDT': 'Australia/Adelaide',
    'AWST': 'Australia/Perth',
    'JST': 'Asia/Tokyo'
};
const postaltimeZoneMapping = {
    'PST1': 'PDT',
    'PST2': 'PDT',
    'MST1': 'MDT',
    'MST2': 'MST',
    'CST1': 'CDT',
    'CST2': 'CDT',
    'EST1': 'EDT',
    'EST2': 'EDT'
};

/**
 * Maps standard time zone abbreviations to IANA time zones.
 * @param {string} abbreviation - The standard time zone abbreviation.
 * @returns {string} The corresponding IANA time zone.
 */
function getIANATimeZone(abbreviation) {
    return timeZoneMapping[abbreviation] || abbreviation;
}
/**
 * Maps postal time zone abbreviations to standard time zone abbreviations.
 * @param {string} abbreviation - The postal time zone abbreviation.
 * @returns {string} The corresponding standard time zone abbreviation.
 */
function getPostalTimeZone(abbreviation) {
    return postaltimeZoneMapping[abbreviation] || abbreviation;
}