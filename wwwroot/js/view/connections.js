//start table function
async function Add_Connection() {
    try {
        sidebar.open('connection');
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function Edit_Connection(data) { };
async function updateConnection(data) {
    try {
        return new Promise((resolve, reject) => {
            Promise.all([updateConnectionDataTable(data, "connectiontable")]);
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
};
async function init_connection(ConnectionList) {
    try {
        return new Promise((resolve, reject) => {
            createConnectionDataTable("connectiontable");
            if (ConnectionList.length > 0) {
                loadConnectionDatatable(ConnectionList.sort(SortByConnectionName), "connectiontable");
            }
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function createConnectionDataTable(table) {
    let Actioncolumn = true;
    let arrayColums = [{
        "name": "",
        "messageType": "",
        "port": "",
        "status": "",
        "action": ""
    }];
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key, value) {
        tempc = {};
        if (/Name/i.test(key)) {
            tempc = {
                "title": 'Name',
                "width": "20%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    if (full.apiConnection) {
                        return full.name + ' <span class="badge badge-pill float-right badge-info">API</span>';
                    }
                    else if (full.UdpConnection) {
                        return full.name + ' <span class="badge badge-pill float-right badge-info">UDP</span>';
                    }
                    else if (full.TcpIpConnection) {
                        return full.name + ' <span class="badge badge-pill float-right badge-info">TCP/IP</span>';
                    }
                    else if (full.WsConnection) {
                        return full.name + ' <span class="badge badge-pill float-right badge-info">WebScoket</span>';
                    }
                }
            }
        }
        else if (/MessageType/i.test(key)) {
            tempc = {
                "title": "Message Type",
                "width": "10%",
                "mDataProp": key
            }
        }
        else if (/Port/i.test(key)) {
            tempc = {
                "title": "Port",
                "width": "10%",
                "mDataProp": key
            }
        }
        else if (/Status/i.test(key)) {
            tempc = {
                "title": "Status",
                "width": "30%",
                "mDataProp": key

            }
        }
        else if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "width": "30%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    /* if (/^Admin/i.test(User.Role)) {*/
                    Actioncolumn = true;
                    return '<button class="btn btn-light btn-sm mx-1 pi-iconEdit connectionedit" name="connectionedit"></button>' +
                        '<button class="btn btn-light btn-sm mx-1 pi-trashFill connectiondelete" name="connectiondelete"></button>'
                    //}
                    //else {
                    //    Actioncolumn = false;
                    //    return "";
                    //}
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
        buttons: {
            buttons:
                [
                    {
                        text: "Add",
                        action: function () { Add_Connection(); }
                    }
                ]
        },
        bFilter: false,
        bdeferRender: true,
        bpaging: false,
        bPaginate: false,
        autoWidth: false,
        bInfo: false,
        destroy: true,
        language: {
            zeroRecords: "No Data"
        },
        aoColumns: columns,
        columnDefs: [{
            target: 4,
            visible: Actioncolumn
        }
        ],
        sorting: [[0, "asc"]],
        rowCallback: function (row, data) {
            $(row).find('td:eq(0)').css('text-align', 'left');
            if (data.activeConnection) {
                if (data.apiConnected) {
                    $(row).find('td:eq(3)').css('background-color', '#17C671');
                }
                else if (data.tcpIpConnection) {
                    $(row).find('td:eq(3)').css('background-color', '#17C671');
                }
                else if (data.udpConnection) {
                    $(row).find('td:eq(3)').css('background-color', '#17C671');
                }
                else {
                    $(row).find('td:eq(3)').css('background-color', '#FF604E');
                }
            }
            else {
                $(row).find('td:eq(3)').css('background-color', '#FFB400');
            }
            Promise.all([addSideButton(data.name)]);


        }
    })
    // Edit/remove record
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/connectionedit/ig.test(this.name)) {
            sidebar.close();
            Edit_Connection(row.data());
        }
        else if (/connectiondelete/ig.test(this.name)) {
            sidebar.close();
            Remove_Connection(row.data());
        }
    });
}

async function loadConnectionDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateConnectionDataTable(newdata, table) {
    try {
        return new Promise((resolve, reject) => {
            let loadnew = true;
            if ($.fn.dataTable.isDataTable("#" + table)) {
                $('#' + table).DataTable().rows(function (idx, data, node) {

                    if (data.id === newdata.id) {
                        loadnew = false;
                        $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
                    }
                });
                if (loadnew) {
                    loadConnectionDatatable([newdata], table);
                }
            } resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function removeConnectionDataTable(removedata, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (data, node) {
            if (data.id === removedata.id) {
                $('#' + table).DataTable().row(node).remove().draw();
            }
        });
    }
}
function SortByConnectionName(a, b) {
    return a.name < b.name ? -1 : a.name > b.name ? 1 : 0;
}
async function addSideButton(name) {
    if (/^MPE/i.test(name)) {
        $('#MPESideButton').css("display", "block");
    }
    else if (/^AGV/i.test(name)) {
        $('#AGVSideButton').css("display", "block");
    } else if (/^SV/i.test(name)) {
        $('#TripSideButton').css("display", "block");
    }
}