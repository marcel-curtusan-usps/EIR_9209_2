
let valuesArray = null;
let weekDates = [];

async function updateEmployeeSchedule(data) {
    try {
        if (data.length > 0) {
            //Get dates for the week
            weekDates = [data[0].weekDate1, data[0].weekDate2, data[0].weekDate3, data[0].weekDate4, data[0].weekDate5, data[0].weekDate6, data[0].weekDate7];
            data = data[0].scheduleList;

            Promise.all([
                createEmpScheduleDataTable('empScheduleData'),
                loadEmpScheduleDatatable(processScheduledata(data), 'empScheduleData')
            ]);
        }
        else {
        }
    } catch (e) {
        console.log(e);
    }
}
function processScheduledata(data) {
    try {
        valuesArray = Object.values(data);
        var result = valuesArray.reduce((acc, curr) => {
            let tacshr = '9.86';
            let tacshrtotal = '38';
            let selshrtotal = curr.totalselshr;
            let totalhrs = curr.totalhr;
            totalhrs += '<br><span class="tacshrSpan">' + tacshrtotal + '</span><span class="selshrSpan">' + selshrtotal + '</span>';
            if (selshrtotal == 0) {
                totalhrspercent = '';
            } else {
                totalhrspercent = '<br>' + Math.round(parseFloat(tacshrtotal) / parseFloat(selshrtotal) * 100 * 1) / 1 + '%';
            }
            let employee = {
                ein: curr.ein,
                name: curr.firstName + ' ' + curr.lastName, 
                tour: curr.tourNumber,
                day1: getDayFormat(curr.day1hr, curr.day1selshr, tacshr),
                day2: getDayFormat(curr.day2hr, curr.day2selshr, tacshr),
                day3: getDayFormat(curr.day3hr, curr.day3selshr, tacshr),
                day4: getDayFormat(curr.day4hr, curr.day4selshr, tacshr),
                day5: getDayFormat(curr.day5hr, curr.day5selshr, tacshr),
                day6: getDayFormat(curr.day6hr, curr.day6selshr, tacshr),
                day7: getDayFormat(curr.day7hr, curr.day7selshr, tacshr),
                hourstotal: totalhrs,
                hourstotalpercent: totalhrspercent
            };
            acc.push(employee);
            return acc;
        }, []);
        return result;
    } catch (e) {
        console.log(e);
    }
}

async function createEmpScheduleDataTable(table) {
    try {
        if ($.fn.dataTable.isDataTable('#' + table)) {
            table.destroy(); // destroy the dataTableObject
            // For new version use table.destroy();
            $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
            // The line above is needed if number of columns change in the Data
        }
        return new Promise((resolve, reject) => {

            let columns = [
                {
                    "title": 'EIN',
                    "width": "10%",
                    "data": 'ein'
                },
                {
                    "title": 'Name',
                    "width": "15%",
                    "data": 'name'
                },
                {
                    "title": 'Tour',
                    "width": "5%",
                    "data": 'tour'
                },
                {
                    "title": 'Saturday<br>' + weekDates[0],
                    "width": "10%",
                    "data": 'day1'
                },
                {
                    "title": 'Sunday<br>' + weekDates[1],
                    "width": "10%",
                    "data": 'day2'
                },
                {
                    "title": 'Monday<br>' + weekDates[2],
                    "width": "10%",
                    "data": 'day3'
                },
                {
                    "title": 'Tuesday<br>' + weekDates[3],
                    "width": "10%",
                    "data": 'day4'
                },
                {
                    "title": 'Wednesday<br>' + weekDates[4],
                    "width": "10%",
                    "data": 'day5'
                },
                {
                    "title": 'Thursday<br>' + weekDates[5],
                    "width": "10%",
                    "data": 'day6'
                },
                {
                    "title": 'Friday<br>' + weekDates[6],
                    "width": "10%",
                    "data": 'day7'
                },
                {
                    "title": 'Hours Total',
                    "width": "10%",
                    "data": 'hourstotal'
                },
                {
                    "title": 'TACS vs SELS %',
                    "width": "10%",
                    "data": 'hourstotalpercent'
                }
            ]

            let EmpScheduleDataTable = $('#' + table).DataTable({
                searching: true,
                //dom: "flrtipB",
                bFilter: true,
                bdeferRender: true,
                bpaging: true,
                bPaginate: false,
                autoWidth: false,
                bInfo: false,
                destroy: true,
                scrollY: 600,
                scrollx: true,
                scroller: true,
                language: {
                    zeroRecords: "No Data"
                },
                aoColumns: columns,
                columnDefs: [
                    { targets: [2, 10, 11], className: 'dt-center' }
                ],
                sorting: [[2, "asc"], [11, "asc"]],
            })
            $('#' + table + ' thead').attr("class", "thead-dark");

            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}

function getDayFormat(dayhr, selshr, tacshr) {
    let curday = '';
    if (dayhr == 'OFF') {
        curday = '<span class="offSpan">OFF</span>';
    } else if (dayhr == 'HOLOFF') {
        curday = '<span class="holoffSpan">HOLOFF</span>';
    } else if (dayhr == 'LV') {
        curday = '<span class="leaveSpan">LV</span>';
    } else {
        curday = '<span class="tourhrSpan">' + dayhr + '</span >';
    }
    if (dayhr == 'OFF' || dayhr == 'HOLOFF' || dayhr == 'LV') {
        if (parseFloat(selshr) > 0) {
            curday += '<br><span class="tacshrSpan">' + tacshr + '</span><span class="selshrSpan">' + selshr + '</span>';
        }
    } else {
        curday += '<br><span class="tacshrSpan">' + tacshr + '</span><span class="selshrSpan">' + selshr + '</span>';
    }
    return curday;
}
async function loadEmpScheduleDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
function padLeft(number, length) {
    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }
    return str;
}

