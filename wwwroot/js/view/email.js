//email table name
let EmailListtable = "emailListtable";
//on close clear all inputs
$('#Email_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
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
    sidebar.open('setting');

});
$('#RemoveEmailModal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .prop('disabled', false)
        .end()
        .find("input[type=radio]")
        .prop('disabled', false)
        .find("span[class=text-info]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end();
    $('span[id=error_remove]').text("");
    sidebar.open('setting');

});

//on open set rules
$('#Email_Modal').on('shown.bs.modal', function () {
    sidebar.close('setting');
    Promise.all([emailSubmitBtn()]);
    $('select[name=reportName]').on("change", function () {
        if (!checkValue($('select[name=reportName] option:selected').html())) {
            $('select[name=reportName]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_reportName]').text("Please Select Report Name");
        }
        else {
            $('select[name=reportName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_reportName]').text("");
        }
        Promise.all([emailSubmitBtn()]);
    });
    $('input[type=text][name=aceId]').on("keyup", function () {
        if (!checkValue($('input[type=text][name=aceId]').val())) {
            $('input[type=text][name=aceId]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_aceId]').text("Please Enter ACE ID");
        }
        else {
            $('input[type=text][name=aceId]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_aceId]').text("");
        }
        Promise.all([emailSubmitBtn()]);
    });
    $('input[type=text][name=emailFirstName]').on("keyup", function () {
        if (!checkValue($('input[type=text][name=emailFirstName]').val())) {
            $('input[type=text][name=emailFirstName]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_emailFirstName]').text("Please Enter First Name");
        }
        else {
            $('input[type=text][name=emailFirstName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_emailFirstName]').text("");
        }
        Promise.all([emailSubmitBtn()]);
    })
    $('input[type=text][name=emailLastName]').on("keyup", function () {
        if (!checkValue($('input[type=text][name=emailLastName]').val())) {
            $('input[type=text][name=emailLastName]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_emailLastName]').text("Please Enter Last Name");
        }
        else {
            $('input[type=text][name=emailLastName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_emailLastName]').text("");
        }
        Promise.all([emailSubmitBtn()]);
    })
    //check if email valid in emailAddres 
    $('input[type=text][name=emailAddress]').on("keyup", function () {
        if (!validateEmail($('input[type=text][name=emailAddress]').val())) {
            $('input[type=text][name=emailAddress]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_emailAddress]').text("Please Enter Email Address");
        }
        else {
            $('input[type=text][name=emailAddress]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_emailAddress]').text("");
        }
        Promise.all([emailSubmitBtn()]);
    })

    // when user select reportName show the MPE name
    $('select[name=reportName]').on("change", function () {
        filterReportType("", "");
        if (!checkValue($('select[name=reportName] option:selected').html())) {
            $('select[name=reportName]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_reportName]').text("Please Select Report Name");
        }
        else {
            $('select[name=reportName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_reportName]').text("");
        }
        Promise.all([emailSubmitBtn()]);

    });
    $('select[name=mpeNameList]').on("change", function () {
        if (!checkValue($('select[name=mpeNameList] option:selected').html())) {
            $('select[name=mpeNameList]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_mpeNameList]').text("Please Select Zone Name");
        }
        else {
            $('select[name=mpeNameList]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_mpeNameList]').text("");
        }
        Promise.all([emailSubmitBtn()]);

    });
});
async function init_emailList() {
    try {
        return new Promise((resolve, reject) => {

            if (!$.fn.dataTable.isDataTable("#" + EmailListtable)) {
                $('button[name=addconnection]').off().on('click', function () {
                    /* close the sidebar */
                    sidebar.close();
                    Promise.all([Add_Email()]);
                });
                Promise.all([createEmailListDataTable(EmailListtable)]);
            }
            else {
                $('#' + EmailListtable).DataTable().clear().draw();
            }
            $.ajax({
                url: SiteURLconstructor(window.location) + "/api/EmailAgent/AllEmail",
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        Promise.all([loadEmailListDatatable(data.sort(), EmailListtable)]);
                    }

                },
                error: function (error) {
                    console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                }
            });
            resolve();
            return false;
        });

    } catch (e) {
        throw new Error(e.toString());
    }
}
async function createEmailListDataTable(table) {
    try {
        let Actioncolumn = true;
        let arrayColums = [{
            "emailAddress": "",
            "reportName": "",
            "mpeName": "",
            "enabled": "",
            "action": ""
        }]
        let columns = [];
        let tempc = {};
        $.each(arrayColums[0], function (key) {
            tempc = {};
            if (/emailAddress/i.test(key)) {
                tempc = {
                    "title": 'Email',
                    "width": "60%",
                    "mDataProp": key
                }
            }
            else if (/reportName/i.test(key)) {
                tempc = {
                    "title": "Report Type",
                    "width": "12%",
                    "mDataProp": key
                }
            }
            else if (/mPEName/i.test(key)) {
                tempc = {
                    "title": "Zone Name",
                    "width": "12%",
                    "mDataProp": key
                }
            }
            else if (/enabled/i.test(key)) {
                tempc = {
                    "title": "Enabled",
                    "width": "25%",
                    "mDataProp": key
                }
            }
            else if (/Action/i.test(key)) {
                tempc = {
                    "title": "Action",
                    "width": "25%",
                    "mDataProp": key,
                    "mRender": function (data, full) {
                        Actioncolumn = true;
                        return '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="editEmail" data-id="' + full.id +
                            '"title="Edit Email"></button>' +
                            ' <button type="button" class="btn btn-light btn-sm mx-1 pi-trashFill" data-toggle="modal" name="deleteEmail" data-id="' + full.id +
                            '"title="Delete Email"></button>';
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
                target: 3,
                visible: Actioncolumn
            }],
            sorting: [[0, "asc"]]

        })

        // Edit/remove record
        $('#' + table + ' tbody').on('click', 'button', function () {
            let td = $(this);
            let table = $(td).closest('table');
            let row = $(table).DataTable().row(td.closest('tr'));
            if (/editEmail/ig.test(this.name)) {
                Edit_Email(row.data());
            }
            if (/deleteEmail/ig.test(this.name)) {
                Delete_Email(row.data());
            }

        });


    } catch (e) {
        throw new Error(e.toString());
    }
}
async function loadEmailListDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateEmailListDataTable(newdata, table) {
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
                    loadEmailListDatatable(newdata, table);
                }
            } resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function deleteEmailList(deletedata) {
    if ($.fn.dataTable.isDataTable("#" + EmailListtable)) {
        $('#' + EmailListtable).DataTable().rows(function (idx, data, node) {
            if (data.id === deletedata.id) {
                $('#' + EmailListtable).DataTable().row(node).remove().draw();
            }
        });
    }
}
async function Add_Email() {
    try {
        Promise.all([loadMpeName()]);
        $('#modalHeader_ID').text('Add New Email Subscription');
        Promise.all([onEmailVaildation()]);

        $('button[id=emailsubmitBtn]').off().on('click', function () {
            $('button[id=emailsubmitBtn]').prop('disabled', true);
            let jsonObject = {
                enabled: $('input[type=checkbox][name=enabled_email]').prop('checked'),
                firstName: $('input[type=text][name=emailFirstName]').val(),
                lastName: $('input[type=text][name=emailLastName]').val(),
                emailAddress: $('input[type=text][name=emailAddress]').val(),
                ace: $('input[type=text][name=aceId]').val(),
                reportName: $('select[name=reportName] option:selected').val(),
                mPEName: $('select[name=mpeNameList] option:selected').val()
            };
            if (!$.isEmptyObject(jsonObject)) {
                //make a ajax call to get the employee details
                $.ajax({
                    url: SiteURLconstructor(window.location) + '/api/EmailAgent/Add',
                    data: JSON.stringify(jsonObject),
                    contentType: 'application/json',
                    type: 'POST',
                    success: function (data) {
                        if (data.length > 0) {
                            Promise.all([updateEmailListDataTable(data, EmailListtable)]);
                        }
                        else {
                            Promise.all([updateEmailListDataTable([data], EmailListtable)]);
                        }
                        setTimeout(function () { $("#Email_Modal").modal('hide'); sidebar.open('setting'); }, 500);
                    },
                    error: function (error) {
                        $('span[id=error_emailsubmitBtn]').text(error);
                        $('button[id=emailsubmitBtn]').prop('disabled', false);
                        //console.log(error);
                    },
                    faulure: function (fail) {
                        console.log(fail);
                    }
                });
            }
        });
        $('#Email_Modal').modal('show');
    } catch (e) {
        throw new Error(e.toString());
    }

}
// edit email 
async function Edit_Email(data) {
    try {
        Promise.all([loadMpeName()]).then(() => {
            $('#modalHeader_ID').text('Edit Email Subscription');
            $('input[type=checkbox][id=enabled_email]').prop('checked', data.enabled);
            $('input[type=text][name=emailFirstName]').val(data.firstName);
            $('input[type=text][name=emailLastName]').val(data.lastName);
            $('input[type=text][name=emailAddress]').val(data.emailAddress);
            $('input[type=text][name=aceId]').val(data.ace);
            $('select[name=reportName]').val(data.reportName);
            $('select[name=mpeNameList]').val(data.mpeName);
            Promise.all([onEmailVaildation()]);
            $('button[id=emailsubmitBtn]').off().on('click', function () {
                $('button[id=emailsubmitBtn]').prop('disabled', true);
                let jsonObject = {
                    id: data.id,
                    enabled: $('input[type=checkbox][name=enabled_email]').prop('checked'),
                    firstName: $('input[type=text][name=emailFirstName]').val(),
                    lastName: $('input[type=text][name=emailLastName]').val(),
                    emailAddress: $('input[type=text][name=emailAddress]').val(),
                    ace: $('input[type=text][name=aceId]').val(),
                    reportName: $('select[name=reportName] option:selected').val(),
                    mPEName: $('select[name=mpeNameList] option:selected').val()
                };
                if (!$.isEmptyObject(jsonObject)) {
                    //make a ajax call to get the employee details
                    $.ajax({
                        url: SiteURLconstructor(window.location) + '/api/EmailAgent/Edit?id=' + data.id,
                        data: JSON.stringify(jsonObject),
                        contentType: 'application/json',
                        type: 'PUT',
                        success: function (data) {
                            Promise.all([updateEmailListDataTable(data, EmailListtable)]);
                            setTimeout(function () { $("#Email_Modal").modal('hide'); sidebar.open('setting'); }, 500);
                        },
                        error: function (error) {
                            $('span[id=error_emailsubmitBtn]').text(error);
                            $('button[id=emailsubmitBtn]').prop('disabled', false);
                            //console.log(error);
                        },
                        faulure: function (fail) {
                            console.log(fail);
                        },
                        complete: function (complete) {
                            $('button[id=emailsubmitBtn]').prop('disabled', false);
                        }
                    });
                }
            });
            $('#Email_Modal').modal('show');
        });
    } catch (e) {
        throw new Error(e.toString());
    }

}
// delete email
async function Delete_Email(data) {
    $('#removeEmailmodalHeader_ID').text('Remove ' + data.emailAddress);
    $('button[id=removebtn]').prop('disabled', false);
    $('button[id=removebtn]').off().on('click', function () {
        $('button[id=removebtn]').prop('disabled', true);
        let jsonObject = {
            id: data.id
        };
        // ajax call to delete email
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/EmailAgent/Delete?id=' + data.id,
            data: JSON.stringify(jsonObject),
            contentType: 'application/json',
            type: 'DELETE',
            success: function (responcedata) {
                $('span[id=error_remove]').text(responcedata);
                setTimeout(function () { $("#RemoveEmailModal").modal('hide'); sidebar.open('setting'); }, 500);
            },
            error: function (error) {
                $('span[id=error_remove]').text(error);
                //console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                Promise.all([deleteEmailList(data)]);
            }
        });
    });
    sidebar.close('setting');
    $('#RemoveEmailModal').modal('show');
}
async function emailSubmitBtn() {
    if ($('input[type=text][name=emailAddress]').hasClass('is-valid') &&
        $('select[name=reportName]').hasClass('is-valid')
    ) {
        $('button[id=emailsubmitBtn]').prop('disabled', false);
    }
    else {
        $('button[id=emailsubmitBtn]').prop('disabled', true);
    }
}
async function onEmailVaildation() {
    if (!checkValue($('select[name=reportName] option:selected').html())) {
        $('select[name=reportName]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_reportName]').text("Please Select Report Name");
    }
    else {
        $('select[name=reportName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_reportName]').text("");
    }
    if (!checkValue($('select[name=mpeNameList] option:selected').html())) {
        $('select[name=mpeNameList]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_mpeNameList]').text("Please Select Zone Name");
    }
    else {
        $('select[name=mpeNameList]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_mpeNameList]').text("");
    }
    if (!checkValue($('input[type=text][name=emailFirstName]').val())) {
        $('input[type=text][name=emailFirstName]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_emailFirstName]').text("Please Enter First Name");
    }
    else {
        $('input[type=text][name=emailFirstName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_emailFirstName]').text("");
    }
    if (!checkValue($('input[type=text][name=emailLastName]').val())) {
        $('input[type=text][name=emailLastName]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_emailLastName]').text("Please Enter Last Name");
    }
    else {
        $('input[type=text][name=emailLastName]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_emailLastName]').text("");
    }
    if (!checkValue($('input[type=text][name=emailAddress]').val())) {
        $('input[type=text][name=emailAddress]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_emailAddress]').text("Please Enter Email Address");
    }
    else {
        $('input[type=text][name=emailAddress]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_emailAddress]').text("");
    }
    if (!checkValue($('input[type=text][name=aceId]').val())) {
        $('input[type=text][name=aceId]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_aceId]').text("Please Enter ACE Id");
    }
    else {
        $('input[type=text][name=aceId]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_aceId]').text("");
    }
}
function validateEmail(email) {
    const regex = /^(?:(?:\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*))$/;
    return regex.test(email);
}
async function loadMpeName() {
    // load the MPE name from API   
    try {
        return new Promise((resolve, reject) => {
            //makea ajax call to get the list of MPE name
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        //clear the select list
                        $('select[id=mpeNameList]').empty();
                        //for each data and set the MPE name

                        let name = "MPE";
                        $('<option data-reportType=' + name + '>').val("").html("").appendTo('#mpeNameList');
                        $.each(data.sort(), function (index, value) {
                            //replace all instance of dot in value

                            let messageType = value.replace(/\./g, '_');
                            if ($('#mpeNameList option[value=' + messageType + ']').length == 0) {
                                $('<option data-reportType=' + name + '>').val(messageType).html(messageType).appendTo('#mpeNameList');
                            }
                        })

                        name = "Site";
                        let messageType = "SiteSummary";
                        if ($('#mpeNameList option[value=' + messageType + ']').length == 0) {
                            $('<option data-reportType=' + name + '>').val(messageType).html(messageType).appendTo('#mpeNameList');
                        }
                    }
                },
                error: function (error) {
                    console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                }
            });
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function filterReportType(name, type) {
    let conectionName = !!name ? name : $("#reportName").find('option:selected').val();
    $("#mpeNameList").children().appendTo("#option-container");
    let toMove = $("#option-container").children("[data-reportType='" + conectionName + "']");
    toMove.appendTo("#mpeNameList");
    let toBlankMove = $("#option-container").children("[data-reportType='blank']");
    toBlankMove.appendTo("#mpeNameList");
    $("#mpeNameList").removeAttr("disabled");
    if (!!name) {
        $('select[id=reportName]').val(name);
        $('select[id=reportName]').prop('disabled', true);
    }
    else {
        $('select[id=reportName]').prop('disabled', false);
    }
    if (!!type) {
        $('select[id=mpeNameList]').val(type);
        $('select[id=mpeNameList]').prop('disabled', true);
    }
    else {
        if (type === "") {
            $('select[id=mpeNameList]').val("blank");
        }
        else {
            $('select[id=mpeNameList]').val(type);
        }

        $('select[id=mpeNameList]').prop('disabled', false);
    }
};