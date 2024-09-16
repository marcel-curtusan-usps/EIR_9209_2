
let valuesArray = null;
let weekDates = [];
$(function () {
    Promise.all([createEmpScheduleDataTable('empScheduleData')]);

});

async function updateEmployeeSchedule(data) {
    try {
        if (data.length > 0) {
            let formatData = processScheduledata(data)
            console.log(formatData);
            Promise.all([updateEmpScheduleDataTable(formatData, 'empScheduleData')]);
        }
        else {
        }
    } catch (e) {
        console.log(e);
    }
}
function processScheduledata(data) {
    try {
        const result = [];

        // Group data by EIN
        const groupedData = data.reduce((acc, curr) => {
            if (!acc[curr.ein]) {
                acc[curr.ein] = [];
            }
            acc[curr.ein].push(curr);
            return acc;
        }, {});

        // Transform each group
        for (const ein in groupedData) {
            const employeeData = groupedData[ein];
            const employee = {
                ein: ein, 
                name: employeeData[0].firstName + ' ' + employeeData[0].lastName,
                tour: employeeData[0].tourNumber,
                day1: '',
                day2: '',
                day3: '',
                day4: '',
                day5: '',
                day6: '',
                day7: '',
                totalScheduleHours: 0,
                totalSelsHr: '',
                totalTACSHr: '',
                hourstotalpercent: ''
            };

            // Map days to the correct fields
            employeeData.forEach(dayData => {
                const dayIndex = parseInt(dayData.day) ;
                employee[`day${dayIndex}`] = dayData;
                // Accumulate the HrsSchedule
                if (dayData.hrsSchedule) {
                    employee.totalScheduleHours += Number(dayData.hrsSchedule) || 0;
                }
                // Accumulate the dailyQREhr
                if (dayData.dailyQREhr) {
                    employee.totalSelsHr += parseFloat(dayData.dailyQREhr) || 0;
                }
                // Accumulate the dailyTACShr
                if (dayData.dailyTACShr) {
                    employee.totalTACSHr += parseFloat(dayData.dailyTACShr) || 0;
                }
            });

            result.push(employee);
        }

        return result;
        //let newresult = [];
        //valuesArray = Object.values(data);
        //var result = valuesArray.reduce((acc, curr) => {
        //    let tacshr = '9.86';
        //    let tacshrtotal = '38';
        //    let selshrtotal = curr.totalselshr;
        //    let totalhrs = curr.totalhr;
        //    totalhrs += '<br><span class="tacshrSpan">' + tacshrtotal + '</span><span class="selshrSpan">' + selshrtotal + '</span>';
        //    if (selshrtotal == 0) {
        //        totalhrspercent = '';
        //    } else {
        //        totalhrspercent = '<br>' + Math.round(parseFloat(tacshrtotal) / parseFloat(selshrtotal) * 100 * 1) / 1 + '%';
        //    }
        //    let employee = {
        //        ein: curr.ein,
        //        name: curr.firstName + ' ' + curr.lastName, 
        //        tour: curr.tourNumber,
        //        day1: getDayFormat(curr.day1hr, curr.day1selshr, tacshr),
        //        day2: getDayFormat(curr.day2hr, curr.day2selshr, tacshr),
        //        day3: getDayFormat(curr.day3hr, curr.day3selshr, tacshr),
        //        day4: getDayFormat(curr.day4hr, curr.day4selshr, tacshr),
        //        day5: getDayFormat(curr.day5hr, curr.day5selshr, tacshr),
        //        day6: getDayFormat(curr.day6hr, curr.day6selshr, tacshr),
        //        day7: getDayFormat(curr.day7hr, curr.day7selshr, tacshr),
        //        hourstotal: curr,
        //        hourstotalpercent: totalhrspercent
        //    };
        //    acc.push(employee);
        //    return acc;
        //}, []);
        //return result;
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
                    "data": 'day1',
                    "width": "10%",
                    mRender: function (full) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day2',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day3',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day4',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day5',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day6',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "data": 'day7',
                    "width": "10%",
                    mRender: function (full, data) {
                        let displayVal;
                        displayVal = getDayFormat(full)
                        return displayVal;
                    }
                },
                {
                    "title": 'Hours Total',
                    "width": "10%",
                    "data": 'totalScheduleHours'
                },
                {
                    "title": 'TACS vs SELS %',
                    "width": "10%",
                    "data": 'hourstotalpercent'
                }
            ]
            $('#' + table).DataTable({
                searching: true,
                //dom: "flrtipB",
                bFilter: true,
                bdeferRender: true,
                bpaging: true,
                bPaginate: false,
                autoWidth: false,
                bInfo: false,
                destroy: true,
                scrollY: 690,
                scrollx: true,
                scroller: true,
                language: {
                    zeroRecords: "No Data"
                },
                aoColumns: columns,
                columnDefs: [
                    {
                        orderable: false, // Disable sorting on all columns
                        targets: '_all'
                    }],
                rowCallback: function (row, data, index) {
                    for (let i = 1; i <= 7; i++) {
                        let dayData = data[`day${i}`];
                        if ($.isObject(dayData)) {
                            if (dayData && /off/ig.test(dayData.workStatus)) {
                                $('td', row).eq(i + 2).addClass('off');
                            }
                            else {
                                $('td', row).eq(i + 2).addClass('innertbl top work');
                            }
                        }
                        else {
                            $('td', row).eq(i + 2).addClass('off');
                        }
                    }
                },
            });
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function getDayFormat(dayhr) {
    let curday = '';
    if ($.isObject(dayhr)) {
        if (/(OFF|'')/i.test(dayhr.workStatus)) {
            curday = dayhr.workStatus;
        } else if (/HOLOFF/i.test(dayhr.workStatus)) {
            curday = '<span class="holoffSpan">HOLOFF</span>';
        } else if (/Leave/i.test(dayhr.workStatus)) {
            curday = '<td>LV</td>';
        } else {
            // Add a CSS class to the <tbody> element to center its content
            curday = '<table width="100%"><tbody>';
            curday += '<tr><td width="50%" class="bt">' + dayhr.beginTourHour + '</td><td width="50%" class="et">' + dayhr.endTourHour + '</td></tr>';
            curday += '<tr class="multi"><td colspan="2" class="section">' + dayhr.sectionName + '</td></tr>';
            curday += '<tr><td>' + dayhr.dailyTACShr + '</td><td>' + dayhr.dailyQREhr + '</td></tr>';
            curday += '</tbody></table>';
        }
        if (/(HOLOFF|Leave)/i.test(dayhr.workStatus)) {
            if (parseFloat(dayhr.dailyQREhr) > 0) {
                curday += '<tr><td>' + dayhr.dailyTACShr + '</td><td>' + dayhr.dailyQREhr + '</td></tr>';
            }
        }
    } else {
        curday = 'OFF';
    }
    return curday;
}
$.isObject = function (obj) {
    return obj !== '' && obj !== null && typeof obj === 'object' && !Array.isArray(obj);
};
//function getDayFormat(dayhr) {
//    let curday = '';
//    if ($.Object(dayhr)) {
//        curday = '<td>OFF</td>';
//    }
//    if (/(OFF|'')/i.test(dayhr.workStatus)) {
//        curday = '<td>OFF</td>';
//    } else if (/HOLOFF/i.test(dayhr.workStatus)) {
//        curday = '<span class="holoffSpan">HOLOFF</span>';
//    } else if (/Leave/i.test(dayhr.workStatus)) {
//        curday = '<td>LV</td>';
//    } else {
//        curday = '<tr><td>' + dayhr.beginTourHour + '</td><td>' + dayhr.endTourHour + '</td></tr>';
//        curday += '<tr><td>' + dayhr.dailyTACShr + '</td><td>' + dayhr.dailyQREhr + '</td></tr>';
//    }
//    if (/(HOLOFF|Leave)/i.test(dayhr.workStatus)) {
//        if (parseFloat(dayhr.dailyQREhr) > 0) {
//            curday += '<tr><td>' + dayhr.dailyTACShr + '</td><td>' + dayhr.dailyQREhr + '</td></tr>';
//        }
//    }
//    //else {
//    //    curday += '<br><span class="tacshrSpan">' + dayhr.dailyTACShr + '</span><span class="selshrSpan">' + dayhr.dailyQREhr + '</span>';
//    //}
//    return curday;
//}
async function updateEmpScheduleDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            $('#' + table).DataTable().row(node).data(element).draw().invalidate();

        })
        if (loadnew) {
            loadEmpScheduleDatatable(newdata, table);
        }
    }
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

