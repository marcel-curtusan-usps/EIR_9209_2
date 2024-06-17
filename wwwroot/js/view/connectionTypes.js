//connection types
let connectiontypetable = "connectiontypetable";
$('#ConnectionType_value_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css("border-color", "#D3D3D3")
        .val('')
        .prop('disabled', false)
        .end()
        .find("input[type=radio]")
        .prop('disabled', false)
        .prop('checked', false).change()
        .end()
        .find("span[class=text-info]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change()
        .end();
});
//on open set rules
$('#ConnectionType_value_Modal').on('shown.bs.modal', function () {

    if (!checkValue($('input[id=descValueID]').val())) {
        $('input[id=descValueID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_descValueID]').text("Please Enter Value");
    }
    else {
        $('input[id=descValueID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_descValueID]').text("");
    }
    if (!checkValue($('input[id=modalConnTypeID]').val())) {
        $('input[id=modalConnTypeID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_modalConnTypeID]').text("Please Enter Value");
    }
    else {
        $('input[id=modalConnTypeID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_modalConnTypeID]').text("");
    }
    //modalConnTypeID
    $('input[type=text][id=modalConnTypeID]').keyup(function () {
        if (!checkValue($('input[type=text][id=modalConnTypeID]').val())) {
            $('input[type=text][id=modalConnTypeID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_modalConnTypeID]').text("Please Enter value");
        }
        else {
            $('input[type=text][id=modalConnTypeID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_modalConnTypeID]').text("");
        }
    });
    //descValueID
    $('input[type=text][id=descValueID]').keyup(function () {
        if (!checkValue($('input[type=text][id=descValueID]').val())) {
            $('input[type=text][id=descValueID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_descValueID]').text("Please Enter Value");
        }
        else {
            $('input[type=text][id=descValueID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_descValueID]').text("");
        }
    });
});
async function init_connectiontType(data) {
    try {
        Promise.all([createConnectiontypeDataTable(connectiontypetable)]);
        if (data.length > 0) {
            Promise.all([connectionTypeLoad(data), updateConnectiontypeDataTable(data, connectiontypetable)]);
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}
function format(rowData) {
    return '<table id="' + rowData.id + '_subconntypetable" >' + '</table>';
}


async function createConnectiontypeDataTable(table) {
    try {
        return new Promise((resolve, reject) => {
            let arrayColums = [{
                "name": "",
                "description": "",
                "Action": ""
            }]
            let columns = [];
            let tempc = {};
            tempc = {
                "className": 'details-control',
                "orderable": false,
                "defaultContent": '',
                "data": null
            }
            columns.push(tempc);
            $.each(arrayColums[0], function (key) {
                tempc = {};
                if (/Name/i.test(key)) {
                    tempc = {
                        "title": 'Name',
                        "width": "25%",
                        "mDataProp": key
                    }
                }
                if (/Description/i.test(key)) {
                    tempc = {
                        "title": "Description",
                        "width": "40%",
                        "mDataProp": key
                    }
                }
                if (/Action/i.test(key)) {
                    tempc = {
                        "title": "Action",
                        "width": "25%",
                        "mDataProp": key,
                        "mRender": function (data, full) {
                            return '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="editconnectiontype" data-id="' + full.id +
                                '"title="Edit Connection Type"></button>' +
                                ' <button type="button" class="btn btn-light btn-sm mx-1 pi-trashFill" data-toggle="modal" name="deleteconnectiontype" data-id="' + full.id +
                                '"title="Delete Connection Type"></button>' +
                                ' <button type="button" class="btn btn-light btn-sm mx-1 bi-plus" data-toggle="modal" name="addconnectionsubtype" data-id="' + full.id +
                                '"title="Add New Connection Subtype"></button>';
                        }
                    }
                }
                columns.push(tempc);

            });
            let ConnectiontypeTable = $('#' + table).DataTable({
                dom: 'Bfrtip',
                bFilter: false,
                bdeferRender: true,
                bpaging: false,
                bPaginate: false,
                autoWidth: false,
                bInfo: false,
                destroy: true,
                language: {
                    emptyTable: "No Data Available"
                },
                aoColumns: columns,
                columnDefs: [
                ],
                sorting: [[1, "asc"]]

            })
            // Add event listener for opening and closing first level child details
            $('#' + table + ' tbody').off().on('click', 'td.details-control', function () {
                let tr = $(this).closest('tr');
                let row = ConnectiontypeTable.row(tr);
                let rowData = row.data();

                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                }
                else {
                    row.child(format(rowData)).show();

                    let table_id = rowData.id + "_subconntypetable";
                    createConnectiontypeSubtable(rowData.id, table_id, rowData.messageTypes);
                    tr.addClass('shown');


                }
            });
            // Edit/remove record
            $('#' + table + ' tbody').on('click', 'button', function () {
                let td = $(this);
                let table = $(td).closest('table');
                let row = $(table).DataTable().row(td.closest('tr'));
                let rowData = row.data();
                if (/editconnectiontype/ig.test(this.name)) {
                    Edit_Connectiontype(row.data());
                }
                if (/deleteconnectiontype/ig.test(this.name)) {
                    Delete_Connectiontype(row.data());
                }
                if (/addconnectionsubtype/ig.test(this.name)) {
                    Add_ConnectionSubtype(rowData);
                }

            });

            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function Add_ConnectionType() {
    $('#modalConnTypeID').prop("disabled", false);
    $('.valuediv').css("display", "block");

    $('#conntype_label').text('Name');
    $('#conntypevaluemodalHeader').text('Add New Connection Type');

    $('button[id=conntypevalue]').off().on('click', function () {
        $(this).prop('disabled', true);
        /* $('button[id=conntypevalue]')*/
        let jsonObject = {
            name: $('input[id=modalConnTypeID]').val(),
            description: $('input[id=descValueID]').val()
        };

        fotfmanager.server.addConnTypesdata(JSON.stringify(jsonObject)).done(function (Data) {
            $('span[id=error_conntypevalue]').text("Data has been updated");
            setTimeout(function () {
                $("#ConnectionType_value_Modal").modal('hide');
                $('button[id=conntypevalue]').prop('disabled', false);

                updateConnectiontypeDataTable([Data], "connectiontypetable");
            }, 800);
        });

    });

    $('#ConnectionType_value_Modal').modal();
}
function Edit_Connectiontype(Data) {
    $('#modalConnTypeID').prop("disabled", true);
    $('.valuediv').css("display", "block");

    $('#conntype_label').text('Name');
    $('#conntypevaluemodalHeader').text('Edit Connection Type ' + Data.name);

    $('input[id=modalConnTypeID]').val(Data.name);
    $('input[id=descValueID]').val(Data.description);

    $('button[id=conntypevalue]').off().on('click', function () {
        $('button[id=conntypevalue]').prop('disabled', true);
        let jsonObject = {
            id: Data.id,
            name: $('input[id=modalConnTypeID]').val(),
            description: $('input[id=descValueID]').val(),
        };

        fotfmanager.server.editConnTypesdata(JSON.stringify(jsonObject)).done(function (connData) {
            $('span[id=error_conntypevalue]').text("Data has been updated");
            setTimeout(function () {
                $("#ConnectionType_value_Modal").modal('hide');
                $('button[id=conntypevalue]').prop('disabled', false);
                updateConnectiontypeDataTable(connData, connectiontypetable);
                ConnectionTypeUpdate(connData)
            }, 800);

            //reload data

            // Edit_AppSetting(table);

        });

    });

    $('#ConnectionType_value_Modal').modal('show');
}
function loadConnectiontypeDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateConnectiontypeDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.id === newdata.id) {
                loadnew = false;
                $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
            }
        })
        if (loadnew) {
            loadConnectiontypeDatatable(newdata, table);
        }
    }
}
function Delete_Connectiontype(Data) {
    $('#removeConTypeHeader_ID').text('Remove ' + Data.Name);
    $('button[id=remove_ConType]').off().on('click', function () {
        let jsonObject = {
            id: Data.id
        };
        fotfmanager.server.deleteConnTypesdata(JSON.stringify(jsonObject)).done(function (newData) {
            $('span[id=error_remove_ConType]').text("Data has been Remove");
            setTimeout(function () {
                $("#RemoveConfirmationConTypeModal").modal('hide');
                removeConnectiontypeDataTable(newData, "connectiontypetable")
                ConnectionTypeRemove(newData)
            }, 800);
        });
    });

    $('#RemoveConfirmationConTypeModal').modal();
}
async function createConnectiontypeSubtable(parentid, table, row_data) {
    try {
        let arrayColums = [{
            "name": "",
            "description": "",
            "Action": ""
        }]
        let columns = [];
        let tempc = {};
        $("#error_subconntypetable").text("");
        $.each(arrayColums[0], function (key, value) {
            tempc = {};
            if (/Description/i.test(key)) {
                tempc = {
                    "title": "Description",
                    "mDataProp": key
                }
            }
            if (/Name/i.test(key)) {
                tempc = {
                    "title": "Name",
                    "mDataProp": key
                }
            }
            if (/Action/i.test(key)) {
                tempc = {
                    "title": "Action",
                    "width": "10%",
                    "mDataProp": key,
                    "mRender": function () {
                        return '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="editconnectionsubtype" data-id="'
                            + parentid + '"title="Edit Connection Subtype"></button>' +
                            '<button type="button" class="btn btn-light btn-sm mx-1 pi-trashFill" data-toggle="modal" name="deleteconnectionsubtype" data-id="' + parentid + '"title="Delete Connection Subtype"></button>';
                    }
                }
            }
            columns.push(tempc);
        });
        $('#' + table).DataTable({
            data: row_data,
            searching: false,
            info: false,
            autoWidth: true,
            destroy: true,
            bpaging: false,
            bPaginate: false,
            select: false,
            aoColumns: columns,
            columnDefs: [],
            sorting: [[0, "asc"]],
        });
        $('#' + table + ' tbody').on('click', 'button', function () {
            let td = $(this);
            let table = $(td).closest('table');
            let row = $(table).DataTable().row(td.closest('tr'));
            let parentid = $(this).data("id");
            if (/editconnectionsubtype/ig.test(this.name)) {
                Edit_ConnectionSubtype(row.data(), parentid);
            }
            if (/deleteconnectionsubtype/ig.test(this.name)) {

                Delete_ConnectionSubtype(row.data(), parentid);
            }
        });

    } catch (e) {

    }
}
function Add_ConnectionSubtype(Data) {
    $('#modalConnTypeID').prop("disabled", false);
    $('.valuediv').css("display", "block");

    $('#conntype_label').text('Code');
    $('#conntypevaluemodalHeader').text('Add New Connection Subtype');

    $('button[id=conntypevalue]').off().on('click', function () {

        $('button[id=conntypevalue]').prop('disabled', true);
        let jsonObject = {
            name: $('input[id=modalConnTypeID]').val(),
            description: $('input[id=descValueID]').val(),
            id: Data.id
        };

        fotfmanager.server.addConnSubtypesdata(JSON.stringify(jsonObject)).done(function (newData) {
            $('span[id=error_conntypevalue]').text("Data has been updated");
            setTimeout(function () {
                $("#ConnectionType_value_Modal").modal('hide');
                $('button[id=conntypevalue]').prop('disabled', false);
                if ($.fn.dataTable.isDataTable("#" + Data.id + "_subconntypetable")) {
                    updateConnectionSubtypeDataTable(newData[0], Data.id + "_subconntypetable");
                    Data.MessageTypes.push(newData[0][0])
                    connectionTypeLoad([Data])
                }
                else {
                    createConnectiontypeSubtable(Data.id, Data.id + "_subconntypetable", newData[0]);
                    Data.MessageTypes.push(newData[0][0])
                    connectionTypeLoad([Data])
                }
            }, 800);
        });
    });

    $('#ConnectionType_value_Modal').modal();
}
function Edit_ConnectionSubtype(Data, connId) {
    $('.valuediv').css("display", "block");

    $('#conntype_label').text('Code');
    $('#conntypevaluemodalHeader').text('Edit Connection Subtype ' + Data.name);

    $('input[id=modalConnTypeID]').val(Data.name);
    $('input[id=descValueID]').val(Data.Description);

    $('button[id=conntypevalue]').off().on('click', function () {
        $('button[id=conntypevalue]').prop('disabled', true);
        let jsonObject = {
            id: connId,
            subId: Data.Id,
            name: $('input[id=modalConnTypeID]').val(),
            description: $('input[id=descValueID]').val()
        };
        fotfmanager.server.editConnSubtypesdata(JSON.stringify(jsonObject)).done(function (newData) {
            $('span[id=error_conntypevalue]').text("Data has been updated");
            setTimeout(function () {
                $("#ConnectionType_value_Modal").modal('hide');
                $('button[id=conntypevalue]').prop('disabled', false);
                updateConnectionSubtypeDataTable(newData[0], connId + "_subconntypetable");
                ConnectionSubtypeUpdate(newData[0])
            }, 800);
        });
    });

    $('#ConnectionType_value_Modal').modal('show');
}
function Delete_ConnectionSubtype(Data, connId) {
    $('#removeConTypeHeader_ID').text('Remove ' + Data.Name);
    $('button[id=remove_ConType]').off().on('click', function () {
        let jsonObject = {
            id: connId,
            subId: Data.id
        };
        fotfmanager.server.deleteConnSubtypesdata(JSON.stringify(jsonObject)).done(function (newData) {
            $('span[id=error_remove_ConType]').text("Data has been Remove");
            setTimeout(function () {
                $("#RemoveConfirmationConTypeModal").modal('hide');
                removeConnectiontypeDataTable(Data, connId + "_subconntypetable")
                ConnectionSubtypeRemove(Data)
            }, 800);
        });
    });

    $('#RemoveConfirmationConTypeModal').modal();
}
async function connectionTypeLoad(connName) {
    try {
        $(document).ready(function () {
            if (!$.isEmptyObject(connName)) {
                if ($('#connection_name option[value="blank"]').length == 0) {
                    $('<option/>').val("blank").html("").appendTo('#connection_name');
                    $('<option data-messagetype=blank>').val("blank").html("").appendTo('#message_type');
                }
                $.each(connName, function (key, value) {
                    let name = this.name;
                    if ($('#connection_name option[value=' + name + ']').length == 0) {
                        $('<option/>').val(this.name).html(this.description + " (" + this.name + ")").appendTo('#connection_name');
                    }
                    $(this.messageTypes).each(function (key, value) {
                        let messagetype = this.name;

                        if ($('#message_type option[value=' + messagetype + ']').length == 0) {
                            $('<option data-messagetype=' + name + '>').val(messagetype).html(this.description).appendTo('#message_type');
                        }
                    });
                });
            }
        });

    }
    catch (e) {

    }
}
function ConnectionTypeUpdate(connData) {
    if (!$.isEmptyObject(connData)) {
        let name = connData.name;
        if (!($('#connection_name option[value=' + name + ']').length === 0)) {
            $('#connection_name option[value=' + name + ']').html(connData.description + " (" + connData.name + ")");
        }
    }
}
function ConnectionTypeRemove(connData) {
    if (!$.isEmptyObject(connData)) {
        let name = connData.name;
        if (!($('#connection_name option[value=' + name + ']').length === 0)) {
            $('#connection_name option[value=' + name + ']').remove();
        }
        $('#message_type > option').each(function () {
            if ($(this).data("messagetype") === name) {
                $(this).remove();
            }
        });
    }
}
function ConnectionSubtypeUpdate(connData) {
    if (!$.isEmptyObject(connData)) {
        let messagetype = connData.name;
        if (!($('#message_type option[value=' + messagetype + ']').length === 0)) {
            $('#message_type option[value=' + messagetype + ']').html(connData.description);
        }
    }
}
function ConnectionSubtypeRemove(connData) {
    if (!$.isEmptyObject(connData)) {
        let name = connData.name;
        if (!($('#message_type option[value=' + name + ']').length === 0)) {
            $('#message_type option[value=' + name + ']').remove();
        }
    }
}
function removeConnectiontypeDataTable(ldata, table) {
    $('#' + table).DataTable().rows(function (idx, data, node) {
        if (data.id === ldata.id) {
            $('#' + table).DataTable().row(node).remove().draw();
        }
    })
}
async function updateConnectionSubtypeDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.id === newdata.id) {
                loadnew = false;
                $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
            }
        })
        if (loadnew) {
            loadConnectiontypeDatatable(newdata, table);
        }
    }
}
async function addConnectiontypeDatatable(newdata, table) {
    loadConnectiontypeDatatable(newdata, table);
}