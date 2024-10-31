let MPEDataTableList = [];
const MPEHourlyTabel = "mpeHourlytable";
const MPERejectTabel = "mpeRejecttable";
mpeHourlyConnection.on("updateMPEzoneRunPerformance", async (data) => {
    if (data.mpeId === MPEName) {
        Promise.all([buildDataTable(data, mpeTartgets)]);
    }
});
async function buildDataTable(data, targetdata) {
    let curtour = getTour();
    const interval = luxon.Interval.fromDateTimes(
        MPEhourlyMindate,
        MPEhourlyMaxdate
    );

    const desiredArray = interval.splitBy({ minutes: 60 }).map((d) => d.start);
    colHourcount = desiredArray.length;
    tourHours = [];
    for (let i = 0; i < colHourcount; i++) {
        let displayHour = desiredArray[i].hour.toString().padStart(2, '0') + ':00';
        tourHours.push(displayHour);
    }

    let dataArray = [];
    let rejectArray = [];

    let tabledataTarget = {};
    tabledataTarget = {
        "order": 1,
        "Name": "Target"
    }
    let tabledataTargetReject = {};
    tabledataTargetReject = {
        "order": 12,
        "Name": "Target %"
    }
    let tourtotaltarget = 0;
    let tourtotalreject = 0;
    for (let i = 0; i < colHourcount; i++) {
        let found = 0;
        $.each(targetdata, function (key, value) {
            if (tourHours[i] == value.targetHour) {
                found++;
                tabledataTarget["Hour" + i] = value.hourlyTargetVol;
                tourtotaltarget += parseInt(value.hourlyTargetVol);
                tabledataTargetReject["Hour" + i] = value.hourlyRejectRatePercent + '%';
                tourtotalreject += parseFloat(value.hourlyRejectRatePercent);
            }
        });
        if (found == 0) {
            tabledataTarget["Hour" + i] = "";
            tabledataTargetReject["Hour" + i] = "";
        }
    }
    tabledataTarget["TourTotal"] = tourtotaltarget;
    tabledataTargetReject["TourTotal"] = (tourtotalreject / colHourcount).toFixed(2) + '%';
    dataArray.push(tabledataTarget);

    let tabledataObject = {};
    tabledataObject = {
        "order": 2,
        "Name": "Actual",
        "TourTotal": 0
    }
    let tabledataObjectReject = {};
    tabledataObjectReject = {
        "order": 10,
        "Name": "Quantity",
        "TourTotal": 0
    }
    for (let i = 0; i < colHourcount; i++) {
        tabledataObject["Hour" + i] = "";
        tabledataObjectReject["Hour" + i] = "";
    }
    $.each(data, function (key, value) {

        if (/hourlyData/i.test(key)) {
            let totalpcs = 0;
            let totalreject = 0;
            $.each(value, function (hrkey) {
                console.log(data.hourlyData[hrkey].hour);
                let tmpdate = luxon.DateTime.fromFormat(data.hourlyData[hrkey].hour, 'yyyy-MM-dd hh:mm', { zone: ianaTimeZone });
                if (tmpdate >= MPEhourlyMindate && tmpdate < MPEhourlyMaxdate) {
                    for (let i = 0; i < colHourcount; i++) {
                        let hrtmp = data.hourlyData[hrkey].hour.split(" ")[1];
                        if (hrtmp == tourHours[i]) {
                            tabledataObject["Hour" + i] = data.hourlyData[hrkey].count;
                            tabledataObjectReject["Hour" + i] = data.hourlyData[hrkey].rejected;
                        }
                    }
                    totalpcs += parseInt(data.hourlyData[hrkey].count);
                    totalreject += parseInt(data.hourlyData[hrkey].rejected);
                }
            });
            tabledataObject["TourTotal"] = totalpcs;
            tabledataObjectReject["TourTotal"] = totalreject;
            dataArray.push(tabledataObject);
            rejectArray.push(tabledataObjectReject);
        }
    });
    rejectArray.push(tabledataTargetReject);
    let tabledataDelta = {};
    let tabledataRejectRate = {};
    tabledataDelta = {
        "order": 3,
        "Name": "Delta",
        "TourTotal": 0
    }
    tabledataRejectRate = {
        "order": 13,
        "Name": "Actual %",
        "TourTotal": 0
    }
    let totaldelta = 0;
    let deltadiff = 0;
    let rejectrate = 0;
    for (let i = 0; i < colHourcount; i++) {
        if (tabledataObject["Hour" + i].toString() != "") {
            if (tabledataTarget["Hour" + i].toString() != "") {
                deltadiff = parseInt(tabledataObject["Hour" + i]) - parseInt(tabledataTarget["Hour" + i]);
                totaldelta += deltadiff;
            } else {
                deltadiff = "";
            }
            if (parseInt(tabledataObject["Hour" + i]) > 0) {
                rejectrate = (parseInt(tabledataObjectReject["Hour" + i]) / parseInt(tabledataObject["Hour" + i]) * 100).toFixed(2);
            }
            tabledataDelta["Hour" + i] = deltadiff;
            tabledataRejectRate["Hour" + i] = rejectrate + '%';
        } else {
            tabledataDelta["Hour" + i] = "";
            tabledataRejectRate["Hour" + i] = "";
        }
    }
    tabledataDelta["TourTotal"] = totaldelta;
    let totalrejectrate = 0;
    if (parseInt(tabledataObject["TourTotal"]) > 0) {
        totalrejectrate = (parseInt(tabledataObjectReject["TourTotal"]) / parseInt(tabledataObject["TourTotal"]) * 100).toFixed(2);
    }
    tabledataRejectRate["TourTotal"] = totalrejectrate + '%';
    dataArray.push(tabledataDelta);
    rejectArray.push(tabledataRejectRate);
    if (TourNumber == curtour) {
        updateMpeDataTable("mpeHourlytable", dataArray);
        updateMpeDataTable("mpeRejecttable", rejectArray);
    } else {
        TourNumber = curtour;
        $('label[id=tourNumber]').text(TourNumber);
        createMPEDataTable("mpeHourlytable", dataArray);
        createRejectDataTable("mpeRejecttable", rejectArray);
    }
}
function createMPEDataTable(table, data) {
    if (MPEDataTableList[table]) { // Check if DataTable has been previously created and therefore needs to be flushed
        MPEDataTableList[table].destroy(); // destroy the dataTableObject
        // For new version use table.destroy();
        $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
        // The line above is needed if number of columns change in the Data
    }
    var columns = [];
    columns.push({
        data: 'Name',
        title: 'Operations',
        width: '240px'
    });

    columnNames = tourHours;
    for (var i in columnNames) {
        columns.push({
            data: "Hour" + i,
            title: columnNames[i],
            render: function (data, type, row) {
                return data > 1000 ? formatNumberWithCommas(data) : data;
            }
        });
    }
    columns.push({
        data: 'TourTotal',
        title: 'Tour TTL',
        render: function (data, type, row) {
            return data > 1000 ? formatNumberWithCommas(data) : data;
        }
    });
    MPEDataTableList[table] = $('#' + table).DataTable({
        fnInitComplete: function () {
            if ($(this).find('tbody tr').length <= 1) {
                $('.odd').hide()
            }
        },
        dom: 'Bfrtip',
        bFilter: false,
        bdeferRender: true,
        paging: false,
        bPaginate: false,
        bAutoWidth: true,
        bInfo: false,
        destroy: true,
        sorting: [[data.order, "asc"]],
        data: data,
        columns: columns,
        rowCallback: function (row, data, index) {
            $(row).find('td').css({
                'font-size': 'calc(0.1em + 1.9vw)',
                'text-align': 'center'
            });
            if (/Delta/i.test(data.Name)) {
                for (let i = 0; i < row.childElementCount - 2; i++) {
                    let colname = 'Hour' + i;
                    let colindex = i + 1;
                    if (parseInt(data[colname]) >= 0) {
                        $(row).find('td:eq(' + colindex + ')').addClass('green');
                    } else {
                        $(row).find('td:eq(' + colindex + ')').addClass('red');
                    }
                }
                let coltindex = row.childElementCount - 1;
                if (parseInt(data["TourTotal"]) >= 0) {
                    $(row).find('td:eq(' + coltindex + ')').addClass('green');
                } else {
                    $(row).find('td:eq(' + coltindex + ')').addClass('red');
                }
            }
        }
    });
}
function createRejectDataTable(table, data) {
    if (MPEDataTableList[table]) { // Check if DataTable has been previously created and therefore needs to be flushed
        MPEDataTableList[table].destroy(); // destroy the dataTableObject
        // For new version use table.destroy();
        $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
        // The line above is needed if number of columns change in the Data
    }

    var columns = [];
    columns.push({
        data: 'Name',
        title: 'Rejects',
        width: '240px'
    });

    columnNames = tourHours;
    for (var i in columnNames) {
        columns.push({
            data: "Hour" + i,
            title: columnNames[i],
            render: function (data, type, row) {
                return data > 1000 ? formatNumberWithCommas(data) : data;
            }
        });
    }
    columns.push({
        data: 'TourTotal',
        title: 'Tour TTL',
        render: function (data, type, row) {
            return data > 1000 ? formatNumberWithCommas(data) : data;
        }
    });
    MPEDataTableList[table] = $('#' + table).DataTable({
        fnInitComplete: function () {
            if ($(this).find('tbody tr').length <= 1) {
                $('.odd').hide()
            }
        },
        dom: 'Bfrtip',
        bFilter: false,
        bdeferRender: true,
        paging: false,
        bPaginate: false,
        bAutoWidth: true,
        bInfo: false,
        destroy: true,
        sorting: [[data.order, "asc"]],
        data: data,
        columns: columns,
        rowCallback: function (row, data, index) {
            $(row).find('td').css({
                'font-size': 'calc(0.1em + 1.9vw)',
                'text-align': 'center'
            });
            if (/Target/i.test(data.Name)) {
                tgrtrow = data;
            }
            if (/Actual/i.test(data.Name)) {
                for (let i = 0; i < row.childElementCount - 2; i++) {
                    let colname = 'Hour' + i;
                    let colindex = i + 1;
                    if (tgrtrow[colname] != "" && parseFloat(tgrtrow[colname]) >= parseFloat(data[colname])) {
                        $(row).find('td:eq(' + colindex + ')').addClass('green');
                    } else {
                        $(row).find('td:eq(' + colindex + ')').addClass('red');
                    }
                }
                let coltindex = row.childElementCount - 1;
                if (parseFloat(tgrtrow["TourTotal"]) >= parseFloat(data["TourTotal"])) {
                    $(row).find('td:eq(' + coltindex + ')').addClass('green');
                } else {
                    $(row).find('td:eq(' + coltindex + ')').addClass('red');
                }
            }
        }
    });
}
function loadMpeDataTable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table) && !$.isEmptyObject(data)) {
        /*if (!$.isEmptyObject(data)) {*/
        $('#' + table).DataTable().rows.add(data).draw();
        //}
    }
}
function updateMpeDataTable(table, ldata) {
    let load = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            load = false;
            if (ldata.length > 0) {
                $.each(ldata, function () {
                    if (data.Name === this.Name) {
                        $('#' + table).DataTable().row(node).data(this).draw().invalidate();
                    }
                })
            }
        })
        if (load) {
            loadMpeDataTable(ldata, table);
        }
    }
}
function formatNumberWithCommas(number) {
    const isNegative = number < 0;
    const absoluteNumber = Math.abs(number);
    const formattedNumber = absoluteNumber.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return isNegative ? `-${formattedNumber}` : formattedNumber;
}