let inventoryDataTable = "assetsTable";
$(function () {
    createInventoryDataTable(inventoryDataTable);
    fetch('../api/SiteInformation/SiteInfo')
        .then(response => response.json())
        .then(data => {
            document.title = data.displayName;
            $('span[id=sitename]').text(data.displayName);
            $('span[id=moddatedisplay]').text(data.displayName);
        })
        .catch(error => {
            console.error('Error:', error);
        });
});
function createInventoryDataTable(table) {
    let arrayColums = [{
        "Name": "",
        "Item": "",
        "Date": "",
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