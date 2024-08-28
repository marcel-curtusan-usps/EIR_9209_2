//connection types
let dacodetocrafttypetable = "dacodetocrafttypetable";

$('#DacodetocraftType_value_Modal').on('hidden.bs.modal', function () {
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
$('#DacodetocraftType_value_Modal').on('shown.bs.modal', function () {

    if (!checkValue($('input[id=craftValueID]').val())) {
        $('input[id=craftValueID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_craftValueID]').text("Please Enter Value");
    }
    else {
        $('input[id=craftValueID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_craftValueID]').text("");
    }
    if (!checkValue($('input[id=modalDacodeTypeID]').val())) {
        $('input[id=modalDacodeTypeID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_modalDacodeTypeID]').text("Please Enter Value");
    }
    else {
        $('input[id=modalDacodeTypeID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_modalDacodeTypeID]').text("");
    }
    //modalConnTypeID
    $('input[type=text][id=modalDacodeTypeID]').keyup(function () {
        if (!checkValue($('input[type=text][id=modalDacodeTypeID]').val())) {
            $('input[type=text][id=modalDacodeTypeID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_modalDacodeTypeID]').text("Please Enter value");
        }
        else {
            $('input[type=text][id=modalDacodeTypeID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_modalDacodeTypeID]').text("");
        }
    });
    //craftValueID
    $('input[type=text][id=craftValueID]').keyup(function () {
        if (!checkValue($('input[type=text][id=craftValueID]').val())) {
            $('input[type=text][id=craftValueID]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_craftValueID]').text("Please Enter Value");
        }
        else {
            $('input[type=text][id=craftValueID]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_craftValueID]').text("");
        }
    });
});
async function init_dacodetocraftType(data) {
    try {
        return new Promise((resolve, reject) => {
            $('button[name=dacodebtn]').off().on('click', function () {
                Promise.all([Add_Dacodetocrafttype()]);
            });
            createDacodetocrafttypeDataTable(dacodetocrafttypetable);
            if (data.length > 0) {
                loadDacodetocrafttypeDatatable(data, "dacodetocrafttypetable")
            }
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}

async function createDacodetocrafttypeDataTable(table) {
    try {
        return new Promise((resolve, reject) => {
            let arrayColums = [{
                "designationActivity": "",
                "craftType": "",
                "Action": ""
            }]
            let columns = [];
            $.each(arrayColums[0], function (key) {
                tempc = {};
                if (/Activity/i.test(key)) {
                    tempc = {
                        "title": 'Designation Activity',
                        "width": "30%",
                        "mDataProp": key
                    }
                }
                if (/Craft/i.test(key)) {
                    tempc = {
                        "title": "Craft Type",
                        "width": "50%",
                        "mDataProp": key
                    }
                }
                if (/Action/i.test(key)) {
                    tempc = {
                        "title": "Action",
                        "width": "20%",
                        "mDataProp": key,
                        "mRender": function (data, full) {
                            return '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="editdacodetocrafttype" data-id="' + full.id +
                                '"title="Edit Connection Type"></button>' +
                                ' <button type="button" class="btn btn-light btn-sm mx-1 pi-trashFill" data-toggle="modal" name="deletedacodetocrafttype" data-id="' + full.id +
                                '"title="Delete Connection Type"></button>';
                        }
                    }
                }
                columns.push(tempc);

            });
            let DacodetocrafttypeTable = $('#' + table).DataTable({
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
                columnDefs: [{
                    orderable: false, // Disable sorting on all columns
                    targets: 2
                }
                ],
                sorting: [[0, "asc"]]

            })

            // Edit/remove record
            $('#' + table + ' tbody').on('click', 'button', function () {
                let td = $(this);
                let table = $(td).closest('table');
                let row = $(table).DataTable().row(td.closest('tr'));
                //let rowData = row.data();
                if (/editdacodetocrafttype/ig.test(this.name)) {
                    Promise.all([Edit_Dacodetocrafttype(row.data())]);
                }
                if (/deletedacodetocrafttype/ig.test(this.name)) {
                    //Delete_Dacodetocrafttype(row.data());
                    Promise.all([Delete_Dacodetocrafttype(row.data())]);
                }
            });

            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}

async function Add_Dacodetocrafttype() {
    try {
        $('#modalDacodeTypeID').prop("disabled", false);
        $('input[id=modalDacodeTypeID]').val("");
        $('input[id=craftValueID]').val("");
        $('#dacodetypevaluemodalHeader').text('Add Designation Activity to Craft Type');

        $('button[id=dacodetypebtn]').off().on('click', function () {
            $('button[id=dacodetypebtn]').prop('disabled', true);
            let jsonObject = {
                designationActivity: $('input[id=modalDacodeTypeID]').val(),
                craftType: $('input[id=craftValueID]').val(),
            };
            if (!$.isEmptyObject(jsonObject)) {
                //make a ajax call to get the employee details
                $.ajax({
                    url: SiteURLconstructor(window.location) + '/api/Dacodetocrafttypes/Add',
                    data: JSON.stringify(jsonObject),
                    contentType: 'application/json',
                    type: 'POST',
                    success: function (data) {
                        setTimeout(function () {
                            $("#DacodetocraftType_value_Modal").modal('hide');
                            $('button[id=dacodetypebtn]').prop('disabled', false);
                            updateDacodetocrafttypeDataTable(data, dacodetocrafttypetable);
                        }, 500);
                    },
                    error: function (error) {
                        $('span[id=error_dacodetypevalue]').text("Designation Activity was not Updated");
                        $('button[id=dacodetypebtn]').prop('disabled', false);
                        //console.log(error);
                    },
                    failure: function (fail) {
                        console.log(fail);
                    },
                    complete: function (complete) {
                        console.log(complete);
                    }
                });
            }
        });
        $('#DacodetocraftType_value_Modal').modal('show');
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function Edit_Dacodetocrafttype(data) {
    $('#modalDacodeTypeID').prop("disabled", true);
    $('.valuediv').css("display", "block");

    $('input[id=modalDacodeTypeID]').val(data.designationActivity);
    $('input[id=craftValueID]').val(data.craftType);
    $('#dacodetypevaluemodalHeader').text('Edit Designation Activity to Craft Type');

    $('button[id=dacodetypebtn]').off().on('click', function () {
        try {
            $('button[id=dacodetypebtn]').prop('disabled', true);
            let jsonObject = {
                designationActivity: $('input[id=modalDacodeTypeID]').val(),
                craftType: $('input[id=craftValueID]').val(),
            };
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/Dacodetocrafttypes/Update?id=' + data.designationActivity,
                contentType: 'application/json-patch+json',
                type: 'PUT',
                data: JSON.stringify(jsonObject),
                success: function (successdata) {
                    setTimeout(function () {
                        $("#DacodetocraftType_value_Modal").modal('hide');
                        $('button[id=dacodetypebtn]').prop('disabled', false);
                        updateDacodetocrafttypeDataTable(successdata, dacodetocrafttypetable);
                    }, 500);
                },
                error: function (error) {
                    $('span[id=error_dacodetypevalue]').text("Designation Activity " + data.designationActivity + " was not Updated");
                    //console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                    console.log(complete);
                }
            });
        } catch (e) {
            $('span[id=error_apisubmitBtn]').text(e);
        }
    });

    $('#DacodetocraftType_value_Modal').modal('show');
}

async function loadDacodetocrafttypeDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}

async function updateDacodetocrafttypeDataTable(newdata, table) {
    try {
        return new Promise((resolve, reject) => {
            let loadnew = true;
            if ($.fn.dataTable.isDataTable("#" + table)) {
                $('#' + table).DataTable().rows(function (idx, data, node) {

                    if (data.designationActivity === newdata.designationActivity) {
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

async function Delete_Dacodetocrafttype(data) {
    try {
        $('#removeDacodeHeader_ID').text('Removing Designation Activity To Craft Type: ' + data.designationActivity + " (" + data.craftType + ")");
        $('button[id=remove_Dacode]').off().on('click', function () {
            //make a ajax call to get the Connection details
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/Dacodetocrafttypes/Delete?id=' + data.designationActivity,
                type: 'DELETE',
                success: function (data) {
                    //$('#content').html(data);
                    //sidebar.open('connections');
                    setTimeout(function () {
                        $("#RemoveConfirmationDacodeModal").modal('hide');
                        removeDacodetypeDataTable(data, dacodetocrafttypetable)
                    }, 500);
                },
                error: function (error) {
                    $('span[id=error_removeDcode]').text(data.designationActivity + " (" + data.craftType + ") was not removed");
                    //console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                    console.log(complete);
                }
            });
        });
        $('#RemoveConfirmationDacodeModal').modal('show');
    } catch (e) {

    }
}
function removeDacodetypeDataTable(ldata, table) {
    $('#' + table).DataTable().rows(function (idx, data, node) {
        if (data.designationActivity === ldata.designationActivity) {
            $('#' + table).DataTable().row(node).remove().draw();
        }
    })
}

