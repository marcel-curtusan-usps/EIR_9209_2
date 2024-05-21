//on close clear all inputs
$('#API_Connection_Modal').on('hidden.bs.modal', function () {
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
        .find("span[class=text]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change()
        .end();
    sidebar.open('connections');
});
//on open set rules
$('#API_Connection_Modal').on('shown.bs.modal', function () {
    sidebar.close('connections');
    $('span[id=error_apisubmitBtn]').text("");
    $('button[id=apisubmitBtn]').prop('disabled', true);
    $('select[name=message_type]').prop('disabled', true);

    $('.hoursforwardvalue').html($('input[id=hoursforward_range]').val());
    $('input[id=hoursforward_range]').on('input change', () => {
        $('.hoursforwardvalue').html($('input[id=hoursforward_range]').val());
    });
    $('.hoursbackvalue').html($('input[id=hoursback_range]').val());
    $('input[id=hoursback_range]').on('input change', () => {
        $('.hoursbackvalue').html($('input[id=hoursback_range]').val());
    });

    //Connection name Keyup
    if (!checkValue($('select[name=connection_name] option:selected').html())) {
        $('select[name=connection_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_connection_name]').text("Please Select Connection Name");
    }
    else {
        $('select[name=connection_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_connection_name]').text("");
    }

    $('select[name=connection_name]').change(function () {
        filtermessage_type("", "");
        if (!checkValue($('select[name=connection_name] option:selected').html())) {
            $('select[name=connection_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_connection_name]').text("Please Select Connection Name");
            $('select[name=message_type]').prop('disabled', true);
            enableMessagetype();
        }
        else {
            $('select[name=connection_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_connection_name]').text("");
            enableMessagetype();
        }

        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });

    //message Type Validation
    if (!checkValue($('select[name=message_type] option:selected').val())) {
        $('select[name=message_type]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_message_type]').text("Select Connection Name Frist");
    }
    else {
        $('select[name=message_type]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_message_type]').text("");
    }
    $('select[name=message_type]').change(function () {
        if (!checkValue($('select[name=message_type] option:selected').val())) {
            $('select[name=message_type]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_message_type]').text("Please Enter Message Type");
        }
        else {
            $('select[name=message_type]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_message_type]').text("");
        }
        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });
    //Data Retrieve Occurrences Validation
    if (!checkValue($('select[name=data_retrieve] option:selected').val())) {
        $('select[name=data_retrieve]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_data_retrieve]').text("Select Data Retrieve Occurrences");
    }
    else {
        $('select[name=data_retrieve]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_data_retrieve]').text("");
    }
    //Data Retrieve Occurrences Keyup
    $('select[name=data_retrieve]').change(function () {
        if (!checkValue($('select[name=data_retrieve] option:selected').val())) {
            $('select[name=data_retrieve]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_data_retrieve]').text("Select Data Retrieve Occurrences");
        }
        else {
            $('select[name=data_retrieve]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_data_retrieve]').text("");
        }
        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });
    // Address Validation
    if (!checkValue($('input[type=text][name=ip_address]').val())) {
        $('input[type=text][name=ip_address]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_ip_address]').text("Please Enter Valid IP address");
    }
    else {
        $('input[type=text][name=ip_address]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_ip_address]').text("");
    }
    //IP Address Keyup
    $('input[type=text][name=ip_address]').keyup(function () {
        if (IPAddress_validator($('input[type=text][name=ip_address]').val()) === 'Invalid IP Address') {
            $('input[type=text][name=ip_address]').css("border-color", "#FF0000");
            $('span[id=error_ip_address]').text("Please Enter Valid IP Address!");
        }
        else {
            $('input[type=text][name=ip_address]').css({ "border-color": "#2eb82e" }).removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_ip_address]').text("");
        }
        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });
    //port Validation
    if (!checkValue($('input[type=text][name=port_number]').val())) {
        $('input[type=text][name=port_number]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_port_number]').text("Please Enter Port Number");
    }
    else {
        $('input[type=text][name=port_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_port_number]').text("");
    }
    //Port Keyup
    $('input[type=text][name=port_number]').keyup(function () {
        if ($.isNumeric($('input[type=text][name=port_number]').val())) {
            if ($('input[type=text][name=port_number]').val().length > 65535) {
                $('input[type=text][name=port_number]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
                $('span[id=error_port_number]').text("Please Enter Port Number!");
            }
            else if ($('input[type=text][name=port_number]').val().length === 0) {
                $('input[type=text][name=port_number]').css({ "border-color": "#FF0000" }).addClass('is-valid').removeClass('is-invalid');
                $('span[id=error_port_number]').text("Please Enter Port Number!");
            }
            else {
                $('input[type=text][name=port_number]').css({ "border-color": "#2eb82e" }).removeClass('is-invalid').addClass('is-valid');
                $('span[id=error_port_number]').text("");
            }
        }
        else {
            $('input[type=text][name=port_number]').css({ "border-color": "#FF0000" }).removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_port_number]').text("Please Enter Port Number!");
        }
        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });
    //Vendor URL
    if (!checkValue($('input[type=text][name=url]').val())) {
        $('input[type=text][name=url]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_url]').text("Please Enter API URL");
    } else {
        $('input[type=text][name=url]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_url]').text("");
    }
    //URL Keyup
    $('input[type=text][name=url]').keyup(function () {
        if (!checkValue($('input[type=text][name=url]').val())) {
            $('input[type=text][name=url]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_url]').text("Please Enter API URL");
        }
        else {
            $('input[type=text][name=url]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_url]').text("");
        }
        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            enabletcpipudpSubmit();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            enableaipSubmit();
        }
    });

    //Hour 
    $('input[type=checkbox][name=hour_range]').change(() => {
        if (!$('input[type=checkbox][id=hour_range]').is(':checked')) {
            $('.hours_range_row').css("display", "none");
            $('input[id=hoursback_range]').val(0);
            $('.hoursbackvalue').html(0);
            $('input[id=hoursforward_range]').val(0);
            $('.hoursforwardvalue').html(0);
        }
        else {
            $('.hours_range_row').css("display", "");
        }
    });
    //radio check
    if ($("input[type=radio][name='connectionType']").change(() => {
        connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');

        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            onudptcpipConnection();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            onAPIConnection();
        }
    }));
    if ($("input[type=checkbox][name='active_connection']").change(() => {
        connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');

        if (/^(udp|tcp)/i.test(connTypeRadio)) {
            onudptcpipConnection();
        }
        else if (/^(api)/i.test(connTypeRadio)) {
            onAPIConnection();
        }
    }));

    //$('input[type=checkbox][name=ws_connection]').change(() => {
    //    onUpdateWS();
    //});
});
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
            $('button[name=addconnection]').off().on('click', function () {
                /* close the sidebar */
                sidebar.close();
                Add_Connection();
            });
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