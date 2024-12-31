async function updateSitePerformanceSummaryStatus(data, requestdate, maxdate, mindate) {
    if (data) {
        console.log(data);
        console.log(requestdate);
        console.log(mindate);
        console.log(maxdate);
        //create sumobject on data object
        let reportdata = [];
        let letterlist = "";
        let packages = "";
        for (let machine in data) {

            let valuesArray = [];
            valuesArray = Object.values(data[machine]);

            for (let i = 1; i < 28; i++) {
                let minDateTemp = maxdate.plus({ hours: i });//new Date(new Date().setHours((maxdate.getHours() + 2) - i, 0, 0, 0))
                var newObj = $.extend({}, valuesArray[0]);
                let dateformat = luxon.DateTime.fromJSDate(minDateTemp).toFormat('yyyy-LL-dd') + "T" + luxon.DateTime.fromJSDate(minDateTemp).toFormat('T:ssZZ')
                if (!Object.keys(data).includes(dateformat)) {
                    for (var prop in newObj) {
                        if (!/(mpeName|hour)/ig.test(prop)) {
                            newObj[prop] = 0;
                        }
                        if (/(hour)/ig.test(prop) && !/(standardStaffHour)/ig.test(prop)) {
                            newObj[prop] = luxon.DateTime.fromJSDate(minDateTemp).toFormat('yyyy-LL-dd') + "T" + luxon.DateTime.fromJSDate(minDateTemp).toFormat('T:ssZZ')
                        }
                        if (/(mpeName)/ig.test(prop)) {
                            newObj[prop] = newObj.mpeName;
                        }
                    }
                    valuesArray.push(newObj);
                }
            }

            let lettersRegex = /^(AFCS200|CIOSS|DBCS|DIOSS|LMS|RIPSLTR|RIPSSVR|HSTS|RCR)/; ///^(AFCS200|AFSM100|CIOSS|DBCS|DIOSS|LMS|RIPSLTR|RIPSSVR|HSTS|RCR)/
            let flatsRegex = /^(AFSM)/; ///^(AFCS200|AFSM100|CIOSS|DBCS|DIOSS|LMS|RIPSLTR|RIPSSVR|HSTS|RCR)/
            let iskip = false;
            let iskipletter = false;
            let iskippackages = false;
            let iskipflats = false;
            if (!reportdata["summary"]) {
                reportdata["summary"] = JSON.parse(JSON.stringify(data[machine]));
                iskip = true;
            }
            if (lettersRegex.test(machine)) {
                if (!reportdata["letters"]) {
                    reportdata["letters"] = JSON.parse(JSON.stringify(data[machine]));
                    iskipletter = true;
                }
            } else if (flatsRegex.test(machine)) {
                if (!reportdata["flats"]) {
                    reportdata["flats"] = JSON.parse(JSON.stringify(data[machine]));
                    iskipflats = true;
                }
            } else {    
                if (!reportdata["packages"]) {
                    reportdata["packages"] = JSON.parse(JSON.stringify(data[machine]));
                    iskippackages = true;
                }
            }

            let result = valuesArray.reduce((acc, curr) => {
                Object.keys(curr).forEach((key, num) => {
                    if ("&,actualThroughput,laborCounts,laborHrs,mpeType,hour,mpeNumber,mpeName,".indexOf(key) <= 0) {

                        if (!iskip) {
                            for (let summarymachine in reportdata["summary"]) {
                                if (reportdata["summary"][summarymachine].hour === curr.hour) {
                                    reportdata["summary"][summarymachine][key] = parseInt(reportdata["summary"][summarymachine][key]) + parseInt(curr[key])
                                }
                            }
                        }

                        if (lettersRegex.test(machine)) {
                            //populate the letters and flats
                            if (!iskipletter) {
                                for (let summarymachine in reportdata["letters"]) {
                                    if (reportdata["letters"][summarymachine].hour === curr.hour) {
                                        reportdata["letters"][summarymachine][key] = parseInt(reportdata["letters"][summarymachine][key]) + parseInt(curr[key])
                                    }
                                }
                            }
                        } else if (flatsRegex.test(machine)) {
                            //populate the letters and flats
                            if (!iskipflats) {
                                for (let summarymachine in reportdata["flats"]) {
                                    if (reportdata["flats"][summarymachine].hour === curr.hour) {
                                        reportdata["flats"][summarymachine][key] = parseInt(reportdata["flats"][summarymachine][key]) + parseInt(curr[key])
                                    }
                                }
                            }
                        } else {
                            if (!iskippackages) {
                                for (let summarymachine in reportdata["packages"]) {
                                    if (reportdata["packages"][summarymachine].hour === curr.hour) {
                                        reportdata["packages"][summarymachine][key] = parseInt(reportdata["packages"][summarymachine][key]) + parseInt(curr[key])
                                    }
                                }

                            }
                            //populate the packages
                        }

                    }
                });
                return acc;
            }, {});
        }

        let summaryresult = processSummaryData(reportdata["summary"], requestdate);
        createSiteSummaryDataTable('sitePerformanceData', mindate, maxdate);
        loadMPESummaryDatatable(summaryresult, 'sitePerformanceData');
        LineChart("sitePerformanceChart", reportdata["summary"], mindate, maxdate);

        let letterresult = processSummaryData(reportdata["letters"], requestdate);
        createSiteSummaryDataTable('siteLettersPerformanceData', mindate, maxdate);
        loadMPESummaryDatatable(letterresult, 'siteLettersPerformanceData');
        LineChart("siteLettersPerformanceChart", reportdata["letters"], mindate, maxdate);

        let packagesresult = processSummaryData(reportdata["packages"], requestdate);
        createSiteSummaryDataTable('sitePackagesPerformanceData', mindate, maxdate);
        loadMPESummaryDatatable(packagesresult, 'sitePackagesPerformanceData');
        LineChart("sitePackagesPerformanceChart", reportdata["packages"], mindate, maxdate);

        let flats = processSummaryData(reportdata["flats"], requestdate);
        createSiteSummaryDataTable('siteflatsPerformanceData', mindate, maxdate);
        loadMPESummaryDatatable(flats, 'siteflatsPerformanceData');
        LineChart("siteflatsPerformanceChart", reportdata["flats"], mindate, maxdate);
    }
}

 function processSummaryData(_datasummary, _requestdate) {

    let rawresult = [];
    let newresult = [];
     let counter = 0;

     let sumArray = Object.values(_datasummary);
     let maxDateTemp = defaultMaxdate;
     let minDateTemp = defaultMindate;
     while (minDateTemp <= maxDateTemp) {
         var newObj = $.extend({}, sumArray[0]);
         let dateformat = minDateTemp.toFormat('yyyy-LL-dd') + "T" + minDateTemp.toFormat('HH:00:00ZZ')
         if (!Object.keys(_datasummary).includes(dateformat)) {

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
             sumArray.push(newObj);
         }
         minDateTemp = minDateTemp.plus({ hours: 1 });
     }
     let result = sumArray.reduce((acc, curr) => {
        Object.keys(curr).forEach(key => {
            if (key !== 'hour') {
                if (!rawresult[key]) {
                    rawresult[key] = {};
                    curr[`_${key}`] = key;
                }
                const dateObj = new Date(curr.hour);
                if (isDateInRange(dateObj, defaultMindate, defaultMaxdate)) {
                    if (!rawresult[key][curr.hour]) {
                        rawresult[key][curr.hour] = curr[key];
                    } else {
                        rawresult[key][curr.hour] = rawresult[key][curr.hour] + curr[key];
                    }
                } else {
                    rawresult[key][curr.hour] = 0;
                }
            }
        });
        return acc;

    }, {});
    //rawresult data processing
    for (let key in rawresult) {
        if (key === "actualThroughput") {
            for (let skey in rawresult[key]) {
                const now = new Date();
                let minutesSpent = 60;
                const time = now.getHours().toString().padStart(2, '0'); //`${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
                if (time.toString() == skey.toString()) {
                    minutesSpent = now.getMinutes() + (now.getSeconds() / 60);
                }
                rawresult[key][skey] = (parseFloat(rawresult["piecesFeed"][skey]) / (60 / minutesSpent))
            }
        }
    }
    for (let key in rawresult) {
        if (key === "actualYield") {
            for (let skey in rawresult[key]) {
                try {

                    let clerkMHspent = rawresult["clerkDwellTime"][skey] + rawresult["mhDwellTime"][skey]
                    if (clerkMHspent > 0) {
                        let ms = parseFloat(clerkMHspent); // represents 1 hour in milliseconds
                        let hrs = ms / 3600000;
                        rawresult[key][skey] = (parseFloat(rawresult["piecesFeed"][skey]) / hrs)
                    }
                } catch (e) {
                    console.log(e);
                }
            }

        }
    }
    //rawresult display processing
    for (let key in rawresult) {
        //if ("&,totalPresent,piecesFeed,actualThroughput,actualYield,totalDwellTime,".indexOf(key) >= 0) {
        if ("&,actualThroughput,laborCounts,laborHrs,mpeType,sortplan,laborCountsTotal,laborHrsTotal,laborPresent,mpeNumber,operationNum,mpeName,piecesSorted,piecesRejected,standardStaffHrs,standardPiecseFeed,".indexOf(key) <= 0 && /^(?!.*_)/.test(key)) {

            if (key.toLowerCase().indexOf("dwell") > 0) {
                for (let skey in rawresult[key]) {
                    let ms = parseFloat(rawresult[key][skey]); // represents 1 hour in milliseconds
                    let hrs = ms / 3600000;
                    let hrsFormatted = hrs.toFixed(1);
                    rawresult[key][skey] = hrsFormatted; // + ' hrs'
                    rawresult[key][skey] = rawresult[key][skey] == 0 ? "-" : parseFloat(rawresult[key][skey]).toLocaleString('en-US');
                }
                newresult[counter] = rawresult[key];
            } else if ("&,actualThroughput,actualYield,,".indexOf(key) >= 0) {
                for (let skey in rawresult[key]) {
                    let ms = parseFloat(rawresult[key][skey]); // represents 1 hour in milliseconds
                    rawresult[key][skey] = ms.toFixed(1);
                    rawresult[key][skey] = rawresult[key][skey] == 0 ? "-" : parseFloat(rawresult[key][skey]).toLocaleString('en-US');
                }
                newresult[counter] = rawresult[key];
            } else {
                for (let skey in rawresult[key]) {
                    let ms = parseFloat(rawresult[key][skey]); // represents 1 hour in milliseconds
                    rawresult[key][skey] = rawresult[key][skey] == 0 ? "-" : parseFloat(rawresult[key][skey]).toLocaleString('en-US');
                    //result[key][skey] = ms.toFixed(1);
                }
                newresult[counter] = rawresult[key];
            }
            rawresult[key]["name"] = key;
            rawresult[key]["sortorder"] = tableKeyDisplay[key] == null ? 200 : tableKeyDisplay[key].sortorder;
            counter++;

        }
     }

    return newresult;
}

let siteSummaryDataTableList = [];

function createSiteSummaryDataTable(table, minDate, maxDate) {

    if (siteSummaryDataTableList[table]) { // Check if DataTable has been previously created and therefore needs to be flushed
        siteSummaryDataTableList[table].destroy(); // destroy the dataTableObject
        // For new version use table.destroy();
        $('#' + table).empty(); // Empty the DOM element which contained DataTable
        // The line above is needed if number of columns change in the Data
    }

    let maxDateTemp = defaultMaxdate;
    let minDateTemp = defaultMindate;
    let columns = [
        {
            data: 'sortorder',
            title: 'sortorder'
        },
        {
            data: 'name',
            title: 'Hours',
            width: '229px',
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
            'defaultContent' : ""
        });
        minDateTemp = minDateTemp.plus({ hours: 1 });
    }
    siteSummaryDataTableList[table] = $('#' + table).DataTable({
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
    "totalPresent": { disp: "Total Employees Present", sortorder: 12 },
    "clerkPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Clerks Present", sortorder: 15 },
    "mhPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Mail Handlers Present", sortorder: 21 },
    "supervisorPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Supervisor Present", sortorder: 25 },
    "maintPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Maintenance Present", sortorder: 31 },
    "otherPresent": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Other Employees Present", sortorder: 34 },
    "totalDwellTime": { disp: "Total Hours Spent (Labor) hrs", sortorder: 38 },
    "clerkDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Clerk Time in zone", sortorder: 41},
    "mhDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Mail Handler Time in zone", sortorder: 45},
    "supervisorDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Supervisor Time in zone", sortorder: 50},
    "maintDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Maintenance Time in zone", sortorder: 55},
    "otherDwellTime": { disp: "&nbsp;&nbsp;&nbsp;&nbsp; Other Time in zone", sortorder: 60 },
    "standardPiecseFeed": { disp: "Std. Throughput", sortorder: 64 },
    "piecesFeed": { disp: "Pieces Fed", sortorder: 65 },
    "piecesSorted": { disp: "Pieces Sorted", sortorder: 66 },
    "piecesRejected": { disp: "Pieces Rejected", sortorder: 67 },
    "actualThroughput": { disp: "Throughput (Pcs Fed/ machine window)", sortorder: 70},
    "actualYield": { disp: "Yield (Pcs Fed/ Work Hours)", sortorder: 71}

};

function isDateInRange(date, mindate, maxdate) {
    return date > mindate && date < maxdate;
}
function padLeft(number, length) {
    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }
    return str;
}