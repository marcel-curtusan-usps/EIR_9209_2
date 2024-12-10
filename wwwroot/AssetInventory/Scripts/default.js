let inventoryDataTable = "assetsTable";
let appData = {};
let currentTime = null;
let ianaTimeZone = "";
$('#checkInOutAssetModal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .end()
        .find("span[class=text]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false);
});
//on open set rules
$('#checkInOutAssetModal').on('shown.bs.modal', function () {
    $('span[id=error_assetSubmit]').text("");
    $('button[id=assetSubmit]').prop('disabled', true);
    //serialNumber Validation
    if ($('input[type=text][id=serialNumber]').val() === "") {
        $('input[type=text][id=serialNumber]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_serialNumber]').text("Please Enter Serial Number");
    }
    else {
        $('input[type=text][id=serialNumber]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_serialNumber]').text("");
    }
    //serialNumber Keyup
    $('input[type=text][id=serialNumber]').on("keyup", () => {
        if ($('input[type=text][id=serialNumber]').val() === "") {
            $('input[type=text][id=serialNumber]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_serialNumber]').text("Please Enter Serial Number");
        } else {
            $('input[type=text][id=serialNumber]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_serialNumber]').text("");
        }

        validateForm();
    });
    //ein Validation
    if ($('input[type=text][id=ein]').val() === "") {
        $('input[type=text][id=ein]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_ein]').text("Please Enter EIN");
    }
    else {
        $('input[type=text][id=ein]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_ein]').text("");
    }
    $('input[type=text][id=ein]').on("keyup", () => {
        if ($('input[type=text][id=ein]').val() === "") {
            $('input[type=text][id=ein]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_ein]').text("Please Enter EIN");
        } else {
            $('input[type=text][id=ein]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_ein]').text("");
        }

        validateForm();
    });

    validateForm();
});

connection.on("AddInventoryTracking", async (date) => {
    loadAssetsTrackingDatatable(date, inventoryDataTable);

});
connection.on("UpdateInventoryTracking", async (date) => {
    updateAssetsTrackingDataTable(date, inventoryDataTable);
});
connection.on("DeleteInventoryTracking", async (date) => {
    deleteMPEFeature(date, inventoryDataTable);
});
$(function () {
    
    createInventoryDataTable(inventoryDataTable);
    fetch('../api/ApplicationConfiguration/Configuration')
        .then(response => response.json())
        .then(data => {
            document.title = data.displayName;
            $('span[id=sitename]').text(data.displayName);
            ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
            $('span[id=moddatedisplay]').text(luxon.DateTime.local().setZone(ianaTimeZone).toFormat('yyyy-LL-dd T:ss'));
            if (/^(Admin|Maintenance|OIE)/i.test(data.Role)) {
                $('div[id=cardInventory]').css('display', 'block');
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
    fetch('../api/Inventory/InventoryCount')
        .then(response => response.json())
        .then(data => {
            $("#totalInvetory").text(data.availableItems)
        })
        .catch(error => {
            console.error('Error:', error);
        });
    $('button[id=checkinbtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadcheckin_outModal("in")]);

    });
    $('button[id=checkoutbtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadcheckin_outModal("Out")]);
        
    });
    $('button[id=addInventorybtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadAddInventoryModal("add")]);

    });
});
async function loadAssetsTrackingDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateAssetsTrackingDataTable(newdata, table) {
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
                loadAssetsTrackingDatatable(newdata, table);
            }
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}

function removeAssetsTrackingDataTable(removedata, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.id === removedata.id) {
                $('#' + table).DataTable().row(node).remove().draw();
            }
        });
    }
}
async function loadInventoryData() {
    let data = await fetch('../api/Inventory/GetInventoryData')
        .then(response => response.json())
        .then(data => {
            return data;
        })
        .catch(error => {
            console.error('Error:', error);
        });
    return data;
    $('#assetModal').modal('show');
}
async function loadAddInventoryModal(type) {
    if (type === "add") {
        $('h5[id=inTakeInventoryLabel]').text('Add New Inventory');
        $('#inTakeInventoryModal').modal('show');
    }
    else {
        $('h5[id=inTakeInventoryLabel]').text('Edit Inventory');
        $('#inTakeInventoryModal').modal('show');
    }
}
async function loadcheckin_outModal(type) {
    if (/^(out)$/i.test(type)) {
        // Unhide the EIN Type input field
        $('h5[id=assetModalLabel]').text('Check-Out');
        $('div[id=einContainer]').css('display', 'block');
        $('div[id=notesContainer]').css('display', 'none');
        $('button[id=assetModalsubmit]').text('Check-Out');
    }
    if (/^(in)$/i.test(type)) {
        // Hide the EIN Type input field
        $('h5[id=assetModalLabel]').text('Check-In');
        $('div[id=einContainer]').css('display', 'none');
        $('div[id=notesContainer]').css('display', 'block');
        $('button[id=assetModalsubmit]').text('Check-In');
    }

    $('button[id=assetSubmit]').off().on('click', function () {
        try {
            let type = "";
            let jsonObject = {};
            if (/^(out)$/i.test(type)) {
                type = "AddTracking";
                jsonObject["serialNumber"] = $("#serialNumber").val();
                jsonObject["ein"] = $("#ein").val();
                jsonObject["type"] = type;
            }
            if (/^(in)$/i.test(type)) {
                type = "UpdateTracking";
                jsonObject["type"] = type;
                jsonObject["serialNumber"] = $("#serialNumber").val();
            }
            // Make a fetch call to add the inventory
            fetch(`../api/Inventory/${type}`, {
                method: 'PUT',
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
                    $('span[id=error_assetSubmit]').text("Data has been updated");
                    setTimeout(function () {
                        $("#checkInOutAssetModal").modal('hide');
                        $('button[id=assetSubmit]').prop('disabled', false);
                        updateAppRoleGroupsDataTable(formatRoledata(successdata), AppRoleTable);
                    }, 500);
                })
                .catch(error => {
                    $('span[id=error_assetSubmit]').text(error);
                    $('button[id=assetSubmit]').prop('disabled', false);
                    console.log(error);
                });
        }
        catch (e) {
            $('span[id=error_machinesubmitBtn]').text(e);
        }
    });
    $('#checkInOutAssetModal').modal('show');
}
function validateForm() {
    let isSerialNumberValid = $('input[type=text][id=serialNumber]').hasClass('is-valid');
    let isEINValid = $('input[type=text][id=ein]').hasClass('is-valid');
    let isCheckOut = $('h5[id=assetModalLabel]').text() === 'Check-Out';
    let isCheckIn = $('h5[id=assetModalLabel]').text() === 'Check-In';

    if ((isCheckOut && isSerialNumberValid && isEINValid) || (isCheckIn && isSerialNumberValid)) {
        $('button[id=assetSubmit]').prop('disabled', false);
    } else {
        $('button[id=assetSubmit]').prop('disabled', true);
    }
}
function createInventoryDataTable(table) {
    let arrayColums = [{
        "Name": "",
        "EIN": "",
        "Tour": "",
        "Date":"",
        "Duration": "",
        "EquipmentType": "",
        "Status": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/Name/i.test(key)) {
            tempc = {
                "title": 'Name',
                "mDataProp": key,
                "class": "col-planned text-center"
            }
        }
        else if (/Item/i.test(key)) {
            tempc = {
                "title": "Item Name",
                "mDataProp": key,
                "class": "col-actual text-center"
            }
        }
        else if (/EquipmentType/i.test(key)) {
            tempc = {
                "title": "Equipment Type",
                "mDataProp": key,
                "class": "col-actual text-center"
            }
        }
        else if (/Date/i.test(key)) {
            tempc = {
                "title": "Check-Out Date",
                "mDataProp": key,
                "class": "col-name text-right"
            }
        }
        else if (/Duration/i.test(key)) {
            tempc = {
                "title": "Check-Out Duration",
                "mDataProp": key,
                "class": "col-name text-right"
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
}
function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
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
