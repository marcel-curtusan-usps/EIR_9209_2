let AppTable = "app_settingtable"

$('#AppSetting_value_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .end()
        .find("span[class=text-info]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change();

    if (!$('#AppSetting_Modal').hasClass('in')) {
        $('#AppSetting_Modal').addClass('modal-open');
    }
});
$('#AppSetting_value_Modal').on('shown.bs.modal', function () {
    $('span[id=error_appsettingvalue]').text("");

    $('button[id=appsettingvalue]').prop('disabled', false);
    //Connection name Validation
    if (!checkValue($('input[id=modalValueID]').val())) {
        $('input[id=modalValueID]').removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_modalValueID]').text("Please Enter Value");
    }
    else {
        $('input[id=modalValueID]').removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_modalValueID]').text("");
    }
    //IP Address Keyup
    $('input[id=modalValueID]').keyup(function () {
        if (!checkValue($('input[id=modalValueID]').val())) {
            $('input[id=modalValueID]').css("border-color", "#FF0000").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_modalValueID]').text("Please Enter Value");
        }
        else {
            $('input[id=modalValueID]').css({ "border-color": "#2eb82e" }).removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_modalValueID]').text("");
        }
    });
});
connection.on("updateApplicationConfiguration", async (data) => {
    try {
        return new Promise((resolve, reject) => {
            Promise.all([updateAppSettingDataTable(formatdata(data), AppTable)]);
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
});
async function init_applicationConfiguration() {
    return new Promise((resolve, reject) => {
        try {
            createAppSettingDataTable(AppTable);
            fetch('../api/ApplicationConfiguration/Setting')
                .then(response => response.json())
                .then(data => {
                    loadAppSettingDatatable(formatdata(data), AppTable);
                })
                .catch(error => {
                    console.error('Error:', error);
                });
            ////get data from application configuration controller
            connection.invoke("JoinGroup", "ApplicationConfiguration").catch(function (err) {
                return console.error(err.toString());
            });
            resolve();
            return false;
        } catch (e) {
            throw new Error(e.toString());
            reject();
        }
    });
}
//app setting
function Get_Action_State() {
    if (/^Admin/i.test(User.Role)) {
        return '<div class="btn-toolbar" role="toolbar">' +
            '<div class="btn-group mr-2" role="group" >' +
            '<button class="btn btn-light btn-sm mx-1 pi-iconEdit" name="editappsetting"></button>' +
            '</div>';
    }
    else {
        return '';
    }
}
function Edit_AppSetting_Value(Data) {
    $('.valuediv').css("display", "block");
    $('.timezonediv').css("display", "none");
    $('.value_row_toggle').css("display", "none");
    $('#appsettingvaluemodalHeader').text('Edit ' + Data.KEY_NAME + ' Setting');
    $('input[id=modalKeyID]').val(Data.KEY_NAME);
    $('input[id=modalValueID]').val(Data.VALUE);
    if (/TIMEZONE/i.test(Data.KEY_NAME)) {
        fotfmanager.server.getTimeZone().done(function (data) {
            $('.valuediv').css("display", "none");
            $('.timezonediv').css("display", "block");
            $('#timezoneValueID').empty();
            $('<option/>').val("").appendTo('#timezoneValueID');
            $.each(data, function () {
                $('<option/>').val(this).html(this).appendTo('#timezoneValueID');
            })
            $('#timezoneValueID').val(value);
        })
    }
    if (/^(LOG_API_DATA|LOCAL_PROJECT_DATA|REMOTEDB|SERVER_ACTIVE)/i.test(Data.KEY_NAME)) {
        $('.valuediv').css("display", "none");
        $('.value_row_toggle').css("display", "block");
        if (/True/i.test(Data.VALUE)) {
            $('input[type=checkbox][name="appsetting_value"]').prop('checked', true).change();
            $('#modalValueID').val('true');
        }
        //else if (/False/i.test(Data.VALUE)) {
        if (/False/i.test(Data.VALUE)) {
            $('input[type=checkbox][name="appsetting_value"]').prop('checked', false).change();
            $('#modalValueID').val('false');
        }
    }

    $('button[id=appsettingvalue]').off().on('click', function () {
        $('button[id=appsettingvalue]').prop('disabled', true);
        let jsonObject = {};
        if (/TIMEZONE/i.test(Data.KEY_NAME)) {
            jsonObject[Data.KEY_NAME] = $("#timezoneValueID option:selected").val();
        }
        else {
            jsonObject[Data.KEY_NAME] = Get_value(Data);
        }
        if (!$.isEmptyObject(jsonObject)) {
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/ApplicationConfiguration/Update',
                data: JSON.stringify(jsonObject),
                contentType: 'application/json',
                type: 'POST',
                success: function (data) {
                    $('span[id=error_appsettingvalue]').text("Data has been updated");
                    connection.invoke("GetApplicationInfo").then(function (data) {
                        appData = JSON.parse(data);
                        Promise.all([updateOSLattribution(appData)]);
                    });
                },
                error: function (error) {
                    $('span[id=error_appsettingvalue]').text(error);
                    $('button[id=apisubmitBtn]').prop('disabled', false);
                    //console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {

                    setTimeout(function () {
                        $("#AppSetting_value_Modal").modal('hide');

                    }, 800);
                }
            });
        }
    });
    $('#AppSetting_value_Modal').modal('show');
}
function createAppSettingDataTable(table) {
    let arrayColums = [{
        "KEY_NAME": "",
        "VALUE": "",
        "Action": ""
    }]
    let columns = [];
    let tempc = {};
    //$.each(arrayColums[0], function (key, value) {
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/KEY_NAME/i.test(key)) {
            tempc = {
                "title": 'Name',
                "width": "30%",
                "mDataProp": key
            }
        }
        //else if (/VALUE/i.test(key)) {
        else if (/VALUE/i.test(key)) {
            tempc = {
                "title": "Value",
                "width": "50%",
                "mDataProp": key
            }
        }
        //else if (/Action/i.test(key)) {
        else if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "width": "10%",
                "mDataProp": key,
                //"mRender": function (data, type, full) {
                "mRender": function () {
                    if (/^Admin/i.test(appData.Role)) {
                        return '<button class="btn btn-light btn-sm mx-1 pi-iconEdit editappsetting" name="editappsetting"></button>';
                    }

                }
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
            zeroRecords: "No Data"
        },
        aoColumns: columns,
        columnDefs: [{
            orderable: false, // Disable sorting on all columns
            targets: '_all'
        }
        ],
        sorting: [[0, "asc"]]

    })
    // Edit/remove record
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/editappsetting/ig.test(this.name)) {

            Edit_AppSetting_Value(row.data());
        }
    });
}
function loadAppSettingDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
function updateAppSettingDataTable(newdata, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            
            for (const element of newdata) {
                if (data.KEY_NAME.toLowerCase() === element.KEY_NAME.toLowerCase()) {
                    if (data.VALUE !== element.VALUE) {
                        $('#' + table).DataTable().row(node).data(element).draw().invalidate();
                    }
                }
            }
        })
    }
}

function Get_value(Data) {
    try {
        if (/^(True)$|^(False)$/i.test(Data.VALUE)) {
            let return_val = false;
            $('input[type=checkbox][name="appsetting_value"]').each(function () {
                if (this.checked) {
                    return_val = true;
                }
            });
            return return_val;
        }
        else {
            return $('input[id=modalValueID]').val();
        }

    } catch (e) {
        console.error(e.toString());
    }
}
function formatAppSetting(key, value, index) {
    return $.extend(key, value, index, {
        number: index,
        id: key,
        value: value,
        action: Get_Action_State()
    });
}
function formatdata(result) {
    let reformatdata = [];
    try {
        for (let key in result) {
            if (result.hasOwnProperty(key)) {
                let temp = {
                    "KEY_NAME": "",
                    "VALUE": ""
                };
                temp['KEY_NAME'] = key;
                temp['VALUE'] = result[key];
                reformatdata.push(temp);
            }
        }

    } catch (e) {
        console.log(e);
    }

    return reformatdata;
}