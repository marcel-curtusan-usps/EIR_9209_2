let inventoryDataTable = "assetsTable";
let appData = {};
let currentTime = null;
let ianaTimeZone = "";
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
    $('button[id=checkinbtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadcheckin_outModal("in")]);

    });
    $('button[id=checkoutbtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadcheckin_outModal("in")]);
        
    });
    $('button[id=addInventorybtn]').off().on('click', function () {
        /* close the sidebar */

        Promise.all([loadAddInventoryModal("add")]);

    });
});
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
    if (type === "out") {
        // Unhide the EIN Type input field
        $('h5[id=assetModalLabel]').text('Check-Out');
        $('div[id=einContainer]').css('display', 'block');
        $('div[id=notesContainer]').css('display', 'none');
        $('button[id=assetModalsubmit]').text('Check-Out');
    }
    if (type === "in") {
        // Hide the EIN Type input field
        $('h5[id=assetModalLabel]').text('Check-In');
        $('div[id=einContainer]').css('display', 'none');
        $('div[id=notesContainer]').css('display', 'block');
        $('button[id=assetModalsubmit]').text('Check-In');
    }
    $('#checkInOutAssetModal').modal('show');
}
function createInventoryDataTable(table) {
    let arrayColums = [{
        "Name": "",
        "EIN": "",
        "Tour": "",
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
                "title": "Date",
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
