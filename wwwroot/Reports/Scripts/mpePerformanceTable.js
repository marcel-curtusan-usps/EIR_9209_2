
let valuesArray = null;
async function updateMPEPerformanceSummaryStatus(data, mindate, maxdate) {

    try {
        Promise.all([processMPEdata(data, mindate, maxdate),
        createMPESummaryDataTable('mpePerformanceData', mindate, maxdate),
        LineChart("lineperfChart", data, mindate, maxdate)]).then(MPEData => {
            loadMPESummaryDatatable(MPEData[0], 'mpePerformanceData');
        });

    } catch (e) {
        console.log(e);
    }
}
function isDateInRange(date, mindate, maxdate) {
    return date >= mindate && date <= maxdate;
}
function processMPEdata(data, mindate, maxdate) {
    try {


        let newresult = [];
        let counter = 0;
        valuesArray = Object.values(data);
        //for (let i = 1; i < 28; i++) {
        let maxDateTemp = MPEdefaultMaxdate;
        let minDateTemp = MPEdefaultMindate
        while (minDateTemp <= maxDateTemp) {
            var newObj = $.extend({}, valuesArray[0]);
            let dateformat = minDateTemp.toFormat('yyyy-LL-dd') + "T" + minDateTemp.toFormat('HH:00:00ZZ')
            if (!Object.keys(data).includes(dateformat)) {

                for (var prop in newObj) {
                    if (!/(mpeName|hour)/ig.test(prop)) {
                        newObj[prop] = 0;
                    }
                    if (/(hour)/ig.test(prop)) {
                        newObj[prop] = minDateTemp.toFormat('yyyy-LL-dd') + "T" + minDateTemp.toFormat('HH:00:00ZZ')
                    }
                    if (/(mpeName)/ig.test(prop)) {
                        newObj[prop] = newObj.mpeName;
                    }
                }
                valuesArray.push(newObj);
            }
            minDateTemp = minDateTemp.plus({ hours: 1 });
        }
        let result = valuesArray.reduce((acc, curr) => {

            Object.keys(curr).forEach(key => {
                if (key !== 'hour') {
                    if (!acc[key]) {
                        acc[key] = {};
                        curr[`_${key}`] = key;
                    }
                    const dateObj = luxon.DateTime.fromISO(curr.hour);

                    if (isDateInRange(dateObj, mindate, maxdate)) {
                        acc[key][curr.hour] = curr[key];
                    } else {
                        acc[key][curr.hour] = 0;
                    }
                }
            });
            return acc;
        }, {});
        for (let key in result) {
            if ("&,actualThroughput,laborCounts,laborHrs,mpeType,sortplan,laborCountsTotal,laborHrsTotal,laborPresent,mpeNumber,mpeName,operationNum,".indexOf(key) <= 0
                && !/^_/.test(key)) {

                if (key.toLowerCase().indexOf("dwell") > 0) {
                    for (let skey in result[key]) {
                        let ms = parseFloat(result[key][skey]); // represents 1 hour in milliseconds
                        let hrs = ms / 3600000;
                        let hrsFormatted = hrs.toFixed(1);
                        result[key][skey] = hrsFormatted; // + ' hrs'
                        result[key][skey] = result[key][skey] == 0 ? "-" : parseFloat(result[key][skey]).toLocaleString('en-US');
                    }
                    newresult[counter] = result[key];
                } else if (key.toLowerCase().indexOf("yield") > 0) {
                    for (let skey in result[key]) {
                        let ms = parseFloat(result[key][skey]); // represents 1 hour in milliseconds
                        result[key][skey] = ms.toFixed(1);
                        result[key][skey] = result[key][skey] == 0 ? "-" : parseFloat(result[key][skey]).toLocaleString('en-US');
                    }
                    newresult[counter] = result[key];
                } else {
                    for (let skey in result[key]) {
                        let ms = parseFloat(result[key][skey]); // represents 1 hour in milliseconds
                        result[key][skey] = result[key][skey] == 0 ? "-" : parseFloat(result[key][skey]).toLocaleString('en-US');
                        //result[key][skey] = ms.toFixed(1);
                    }
                    newresult[counter] = result[key];
                }
                result[key]["name"] = key;
                result[key]["sortorder"] = tableKeyDisplay[key] == null ? 200 : tableKeyDisplay[key].sortorder;
                counter++;

            }
        }

        return newresult;
    } catch (e) {
        console.log(e);
    }
}

let MPEDataTableList = [];
async function createMPESummaryDataTable(table, minDate, maxDate) {
    if (MPEDataTableList[table]) { // Check if DataTable has been previously created and therefore needs to be flushed

        MPEDataTableList[table].destroy(); // destroy the dataTableObject
        // For new version use table.destroy();
        $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
        // The line above is needed if number of columns change in the Data
    }
    let maxDateTemp = MPEdefaultMaxdate;
    let minDateTemp = MPEdefaultMindate
    let columns = [
        {
            data: 'sortorder',
            title: 'sortorder'
        },
        {
            data: 'name',
            title: 'Hours',
            width: '21%',
            mRender: function (full, data) {
                let displayVal;
                displayVal = tableKeyDisplay[full] == null ? full : tableKeyDisplay[full].disp;
                return displayVal;
            }
        }]

    while (minDateTemp <= maxDateTemp) {
        columns.push({
            'data': minDateTemp.toFormat('yyyy-LL-dd') + "T" + minDateTemp.toFormat('HH:00:00'),
            'title': minDateTemp.toFormat('T'),
            'defaultContent': ""
        });
        minDateTemp = minDateTemp.plus({ hours: 1 });
    }
    MPEDataTableList[table] = $('#' + table).DataTable({
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
            visible: false,
            targets: 0,

        }],
        sorting: [[0, "asc"]],
        rowCallback: function (row, data, index) {
            if (/^Total/ig.test(data.name)) {
                $(row).css('background-color', '#f2f2f2');
            }
            if (/standard/ig.test(data.name)) {
                $(row).css('background-color', '#cceeff');
            }
            if (/piecesFeed/ig.test(data.name)) {
                $(row).css('background-color', '#e6ffe6');
            }
        },
        headerCallback: function (thead, data, start, end, display) {
            // Add title attribute with tooltip text to column header
            $(thead).find('th').each(function () {
                $(this).attr('title', data);
            });
        }
    })
    $('#' + table + ' thead').attr("class", "thead-dark");
}
async function loadMPESummaryDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateMPESummaryDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            $('#' + table).DataTable().row(node).data(element).draw().invalidate();

        })
        if (loadnew) {
            loadConnectiontypeDatatable(newdata, table);
        }
    }
}

let tableKeyDisplay = {
    "machineOperatingTime": { disp: "Machine Operating Time (hrs)", sortorder: 0 },
    "standardStaffHrs": { disp: "Std. Oprational Staffing (Clerks + MH) ", sortorder: 5 },
    "totalDwellTime": { disp: "Total Hours Spent (Labor) hrs", sortorder: 12 },
    "clerkDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Clerk Time in zone", sortorder: 15 },
    "mhDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Mail Handler Time in zone", sortorder: 21 },
    "supervisorDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Supervisor Time in zone", sortorder: 35 },
    "maintDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Maintenance Time in zone", sortorder: 31 },
    "otherDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Other Time in zone", sortorder: 34 },
    "totalPresent": { disp: "Total Employees Present", sortorder: 38 },
    "clerkPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Clerks Present", sortorder: 41 },
    "mhPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Mail Handlers Present", sortorder: 45 },
    "supervisorPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Supervisor Present", sortorder: 50 },
    "maintPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Maintenance Present", sortorder: 55 },
    "otherPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Other Employees Present", sortorder: 60 },
    "standardPiecseFeed": { disp: "Std. Throughput", sortorder: 63 },
    "piecesFeed": { disp: "Pieces Fed", sortorder: 65 },
    "piecesSorted": { disp: "Pieces Sorted", sortorder: 66 },
    "piecesRejected": { disp: "Pieces Rejected", sortorder: 67 },
    "actualThroughput": { disp: "Throughput (Pcs Fed/ machine window)", sortorder: 70 },
    "actualYield": { disp: "Yield (Pcs Fed/ Time spent)", sortorder: 71 }

};

function padLeft(number, length) {
    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }
    return str;
}

