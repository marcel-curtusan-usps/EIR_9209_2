let inventoryDataTable = "assetsTable";
$(function () {
    createInventoryDataTable(inventoryDataTable);
    fetch('../api/ApplicationConfiguration/Configuration')
        .then(response => response.json())
        .then(data => {
            document.title = data.displayName;
            $('span[id=sitename]').text(data.displayName);
            $('span[id=moddatedisplay]').text(data.displayName);
            if (/^(Admin|OIE)/i.test(data.Role)) {
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

        Promise.all([loadcheckin_outModal("out")]);

    });
});
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
    $('#assetModal').modal('show');
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