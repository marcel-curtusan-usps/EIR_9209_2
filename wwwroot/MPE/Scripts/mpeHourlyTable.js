let MPEDataTableList = [];
const MPEHourlyTabel = "mpeHourlytable";
const MPERejectTabel = "mpeRejecttable";
mpeHourlyConnection.on("updateMPEzoneRunPerformance", async (data) => {
    if (data.mpeId === MPEName) {
        mpeRunData = data;
        createLoadMPEHourData(getTour(), mpeTartgets, mpeRunData);
        createLoadMPERejectHourData(getTour(), mpeTartgets, mpeRunData);
    }
});
mpeHourlyConnection.on("updateMPEzoneTartgets", async (data) => {
    if (data[0].mpeId === MPEName) {
        mpeTartgets = data;
        createLoadMPEHourData(getTour(), mpeTartgets, mpeRunData);
        createLoadMPERejectHourData(getTour(), mpeTartgets, mpeRunData);
    }
});
function constructTourHoursMpeColumns(tourNumber) {
    let columns = [];
    let tourhours = getTourHours(tourNumber);
    //first column is always the name
    var columnfirst =
    {
        data: 'name',
        title: 'Operations',
        width: '80px'
    };
    columns[0] = columnfirst;
    $.each(tourhours, function (key, value) {
        columns[(key + 1)] =
        {
            title: value,
            data: value,
            width: '40px',
            render: function (data, type, row) {
                if (data === 0 ) {
                    return "-";
                }
                else {
                    return data > 1000 ? formatNumberWithCommas(data) : data < -1000 ? formatNumberWithCommas(data) : data ;
                }
               
            }
        };
    });
    var columnLast =
    {
        data: 'tourTotal',
        title: 'Tour TTL',
        width: '240px',
        render: function (data, type, row) {
            if (data === 0) {
                return "-";
            }
            else {
                return data > 1000 ? formatNumberWithCommas(data) : data < -1000 ? formatNumberWithCommas(data) : data;
            }
        }
    };
    columns[columns.length] = columnLast;

    return columns;
}
function constructTourHoursRejectsColumns(tourNumber) {
    let columns = [];
    let tourhours = getTourHours(tourNumber);
    //first column is always the name
    var columnfirst =
    {
        data: 'name',
        title: 'Rejects',
        width: '240px'
    };
    columns[0] = columnfirst;
    $.each(tourhours, function (key, value) {
        columns[(key + 1)] =
        {
            title: value,
            data: value,
            width: '40px',
            render: function (data, type, row) {
                if (data === 0 || data === '0.0') {
                    return "-";
                }
                else {
                    if (row.name === 'Quantity') {
                        return data > 1000 ? formatNumberWithCommas(data) : data < -1000 ? formatNumberWithCommas(data) : data;
                    } else {
                        return data > 1000 ? formatNumberWithCommas(data) + ' %' : data < -1000 ? formatNumberWithCommas(data) + ' %' : data + ' %';
                    }
                }
            }
        };
    });
    var columnLast =
    {
        data: 'tourTotal',
        title: 'Tour TTL',
        width: '240px',
        render: function (data, type, row) {
            if (data === 0 || data === '0.0') {
                return "-";
            }
            else {
                if (row.name === 'Quantity') {
                    return data > 1000 ? formatNumberWithCommas(data) : data < -1000 ? formatNumberWithCommas(data) : data;
                } else {
                    return data > 1000 ? formatNumberWithCommas(data) + ' %' : data < -1000 ? formatNumberWithCommas(data) + ' %' : data + ' %';
                }
            }
          
        }
    };
    columns[columns.length] = columnLast;
    return columns;
}

function createLoadMPEHourData(tourNumber,targets,mpedata) {

    let tourhours = getTourHours(tourNumber);
    let dataArray = [];
    let dataTarget = {};
    let dataTargetActual = {};
    let dataTargetDelta = {};
    let targetTTL = 0;
    let mpeActualTTL = 0;
    let deltaTTL = 0;
    dataTarget = {
        "order": 1,
        "name": "Target"
    }
    dataTargetActual = {
        "order": 5,
        "name": "Actual"
    }
    dataTargetDelta = {
        "order": 10,
        "name": "Delta"
    }
    for (let i = 0; i < tourhours.length; i++) {
        const hourTarget = targets.find(target => target.targetHour === tourhours[i]);
        let curtDayandHour = ""
        if (tourNumber === 1 && new RegExp('^(00|01|02|03|04|05|06|07|08)').test(tourhours[i])) {
            curtDayandHour = currentTime.plus({ day: 1 }).day.toString().padStart(2, '0') + " " + tourhours[i];
        }
        else {
          curtDayandHour = currentTime.day.toString().padStart(2, '0') + " " + tourhours[i];
        }
         
        const hourMPE = mpedata.hourlyData.find(hourlyData => hourlyData.hour.endsWith(curtDayandHour));
        let targetHourlyVol = parseInt(hourTarget?.hourlyTargetVol) || 0;
        let mpeCount = parseInt(hourMPE?.count) || 0;
        dataTarget[tourhours[i]] = targetHourlyVol
        targetTTL += targetHourlyVol;
       
        dataTargetActual[tourhours[i]] = mpeCount;
        mpeActualTTL += mpeCount;
        dataTargetDelta[tourhours[i]] = mpeCount - targetHourlyVol;
        deltaTTL += (mpeCount - targetHourlyVol);
    }
    dataTarget["tourTotal"] = targetTTL;
    dataTargetActual["tourTotal"] = mpeActualTTL;
    dataTargetDelta["tourTotal"] = deltaTTL;
    dataArray.push(dataTarget);
    dataArray.push(dataTargetActual);
    dataArray.push(dataTargetDelta);
    $('#' + MPEHourlyTabel).DataTable().clear().draw();
    Promise.all([updateMpeDataTable(dataArray, MPEHourlyTabel)]);
}
function createLoadMPERejectHourData(tourNumber, targets, mpedata) {

    let tourhours = getTourHours(tourNumber);
    let dataArray = [];
    let dataQuantity = {};
    let dataTargetPercent = {};
    let dataActualPercent = {};
    let quantityTTL = 0;
    let targetPercentTTL = 0;
    let actualPercentTTL = 0;
    let mpeActualTTL = 0;
    dataQuantity = {
        "order": 1,
        "name": "Quantity"
    }
    dataTargetPercent = {
        "order": 5,
        "name": "Target %"
    }
    dataActualPercent = {
        "order": 10,
        "name": "Actual %"
    }
    for (let i = 0; i < tourhours.length; i++) {
        const hourTarget = targets.find(target => target.targetHour === tourhours[i]);
        let curtDayandHour = ""
        if (tourNumber === 1 && new RegExp('^(00|01|02|03|04|05|06|07|08)').test(tourhours[i])) {
            curtDayandHour = currentTime.plus({ day: 1 }).day.toString().padStart(2, '0') + " " + tourhours[i];
        }
        else {
            curtDayandHour = currentTime.day.toString().padStart(2, '0') + " " + tourhours[i];
        }
        const hourMPE = mpedata.hourlyData.find(hourlyData => hourlyData.hour.endsWith(curtDayandHour));
  
        let mpeCount = parseInt(hourMPE?.count) || 0;
        mpeActualTTL += mpeCount;
        let mpeReject = parseInt(hourMPE?.rejected) || 0;
        let targetReject = parseInt(hourTarget?.hourlyRejectRatePercent) || 0;
        dataQuantity[tourhours[i]] = mpeReject;
        quantityTTL += dataQuantity[tourhours[i]];
    
        dataTargetPercent[tourhours[i]] = targetReject || 0 ;
        targetPercentTTL += targetReject;

        dataActualPercent[tourhours[i]] = mpeCount > 0 ? ((mpeReject / mpeCount * 100).toFixed(1)) : 0;
        actualPercentTTL = quantityTTL > 0 ? ((quantityTTL / mpeActualTTL * 100).toFixed(1)) : 0;
    }
    dataQuantity["tourTotal"] = quantityTTL;
    dataTargetPercent["tourTotal"] = targetPercentTTL; 
    dataActualPercent["tourTotal"] = actualPercentTTL;
    dataArray.push(dataQuantity);
    dataArray.push(dataTargetPercent);
    dataArray.push(dataActualPercent);
    $('#' + MPERejectTabel).DataTable().clear().draw();
    Promise.all([updateMpeDataTable(dataArray, MPERejectTabel)]);
}
function createMPEDataTable(tourNumber) {
    $('#' + MPEHourlyTabel).DataTable({
        dom: 'Bfrtip',
        bFilter: false,
        bdeferRender: true,
        paging: false,
        bPaginate: false,
        bAutoWidth: true,
        bInfo: false,
        destroy: true,
        columns: constructTourHoursMpeColumns(tourNumber),
        order: [[]],
        rowCallback: function (row, data, index) {
            $(row).find('td').css({
                'font-size': 'calc(0.1em + 1.9vw)',
                'text-align': 'center'
            });
            if (/Delta/i.test(data.name)) {
                Object.keys(data).forEach((key, index) => {
                    if (key !== 'name' && key !== 'tourTotal' && key !== 'order') {
                        let colindex = index -1  ;
                        let tkey = data[key];
                        if (parseInt(data[key]) >= 1) {
                            $(row).find('td:eq(' + colindex + ')').addClass('green');
                        } else {
                            $(row).find('td:eq(' + colindex + ')').addClass('red');
                        }
                    }
                });
                let coltindex = row.childElementCount - 1;
                if (parseInt(data["tourTotal"]) >= 1) {
                    $(row).find('td:eq(' + coltindex + ')').addClass('green');
                } else {
                    $(row).find('td:eq(' + coltindex + ')').addClass('red');
                }
            }
        }
    });
}
function createRejectDataTable(tourNumber) {
    $('#' + MPERejectTabel).DataTable({
        dom: 'Bfrtip',
        bFilter: false,
        bdeferRender: true,
        paging: false,
        bPaginate: false,
        bAutoWidth: true,
        bInfo: false,
        destroy: true,
        columns: constructTourHoursRejectsColumns(tourNumber),
        order: [[]],
        rowCallback: function (row, data, index) {
            $(row).find('td').css({
                'font-size': 'calc(0.1em + 1.9vw)',
                'text-align': 'center'
            });
            if (/Target/i.test(data.name)) {
                tgrtrow = data;
            }
            if (/Actual/i.test(data.name)) {

                Object.keys(data).forEach((key, index) => {
                    if (key !== 'name' && key !== 'tourTotal' && key !== 'order') {
                        let colindex = index - 1;
                        let tkey = data[key];
                        if (parseInt(data[key]) >= parseFloat(tgrtrow[key])) {
                            $(row).find('td:eq(' + colindex + ')').addClass('red');
                        } else {
                            $(row).find('td:eq(' + colindex + ')').addClass('green');
                        }
                    }
                });
                let coltindex = row.childElementCount - 1;
                if (parseFloat(data["tourTotal"]) >= parseFloat(tgrtrow["tourTotal"])) {
                    $(row).find('td:eq(' + coltindex + ')').addClass('red');
                } else {
                    $(row).find('td:eq(' + coltindex + ')').addClass('green');
                 
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
async function updateMpeDataTable(newdata, table) {
    try {
        return new Promise((resolve, reject) => {
            if ($.fn.dataTable.isDataTable("#" + table)) {
                if ($('#' + table).DataTable().rows().count() > 0) {
                    $('#' + table).DataTable().rows(function (idx, data, node) {
                        $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
                        resolve();
                        return true;
                    });
                }
                else {
                    loadMpeDataTable(newdata, table);
                }
            }
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function formatNumberWithCommas(number) {
    const isNegative = number < 0;
    const absoluteNumber = Math.abs(number);
    const formattedNumber = absoluteNumber.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return isNegative ? `-${formattedNumber}` : formattedNumber;
}