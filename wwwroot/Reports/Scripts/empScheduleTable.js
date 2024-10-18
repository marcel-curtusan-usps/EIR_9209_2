
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
            $('#empScheduleData').DataTable().clear().draw(); // Empty the DOM element which contained DataTable
            Promise.all([updateEmpScheduleDataTable(formatData, 'empScheduleData')]);
        }
        else {
            $('#empScheduleData').DataTable().clear().draw(); // Empty the DOM element which contained DataTable
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
                name: employeeData[0].lastName + ' ' +employeeData[0].firstName,
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
                hourstotalpercent: 0
            };

            // Map days to the correct fields
            employeeData.forEach(dayData => {
                const dayIndex = parseInt(dayData.day) ;
                employee[`day${dayIndex}`] = dayData;
                // Accumulate the HrsSchedule
                if (dayData.hrsMove) {
                    employee.totalScheduleHours += dayData.hrsMove;
                }
                // Accumulate the dailyQREhr
                if (dayData.dailyQREhr) {
                    employee.totalSelsHr += parseFloat(dayData.dailyQREhr) || 0;
                }
                // Accumulate the dailyTACShr
                if (dayData.dailyTACShr) {
                    employee.totalTACSHr += parseFloat(dayData.dailyTACShr) || 0;
                }
                //get weekdates
                if (!weekDates[dayIndex]) {
                    let dayname = dayData.dayName.charAt(0).toUpperCase() + dayData.dayName.slice(1); 
                    let splitdate = dayData.date.split('-');
                    let dispdate = splitdate[1] + '/' + splitdate[2];
                    weekDates[dayIndex] = dayname + '<br>' + dispdate;
                }
            });

            result.push(employee);
        }

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
                    "data": 'totalScheduleHours',
                    mRender: function (full, data) {
                       
                        return Math.round(full * 100) / 100;
                    }
                },
                {
                    "title": 'TACS vs SELS %',
                    "width": "10%",
                    "data": 'hourstotalpercent'
                }
            ]
            $('#' + table).DataTable({
                searching: true,
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
                order: [[3, 'desc']],
                columnDefs: [
                    {
                        orderable: false, // Disable sorting on all columns
                        targets: '_all'
                    },
                    {
                        targets: [3,4,5,6,7,8,9],
                        createdCell: function (td, cellData, rowData, row, col) {
                            if (/off/ig.test(cellData.workStatus)) {
                                $(td).addClass('off');
                            } else if (/Leave/ig.test(cellData.workStatus)) {
                                $(td).addClass('leave');
                            } else if (/holoff/ig.test(cellData.workStatus)) {
                                $(td).addClass('holoff');
                            } else {
                                $(td).addClass('innertbl top work');
                            }
                        }
                    }
                ],
                headerCallback: function headerCallback(thead, data, start, end, display) {
                    for (var i = 1; i <= 7; i++) {
                        $(thead)
                            .find('th')
                            .eq(i+2)
                            .html(weekDates[i]);
                    }
                }
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
        if (/(OFF|HOLOFF|'')/i.test(dayhr.workStatus)) {
            curday = dayhr.workStatus;
        } else if (/Leave/i.test(dayhr.workStatus)) {
            curday = 'LV';
        } else {
            // Add a CSS class to the <tbody> element to center its content
            curday = '<table width="100%"><tbody>';
            curday += '<tr><td width="50%" class="bt work">' + dayhr.beginTourHour + '</td><td width="50%" class="et work">' + dayhr.endTourHour + '</td></tr>';
            curday += '<tr class="multi"><td colspan="2" class="section work">' + dayhr.sectionName + '</td></tr>';
            if (dayhr.hrsLeave > 0) {
                curday += '<tr class="multi"><td colspan="2" class="section leave">LV</td></tr>';
                curday += '<tr class="multi"><td colspan="2" class="section leave">' + dayhr.hrsLeave + '</td></tr>';
            }
            curday += '<tr><td class="bt section tacshr">' + Math.round(dayhr.dailyTACShr * 100) / 100 + '</td><td class="et section selshr">' + Math.round(dayhr.dailyQREhr * 100) / 100 + '</td></tr>';
            curday += '</tbody></table>';
        }
        if (/(HOLOFF|Leave|OFF)/i.test(dayhr.workStatus)) {
            if (parseFloat(dayhr.dailyQREhr) > 0 || parseFloat(dayhr.dailyQREhr) > 0) {
                curday = '<table width="100%"><tbody>';
                if (/(OFF|'')/i.test(dayhr.workStatus)) {
                    curday += '<tr class="multi"><td colspan="2" class="off">' + dayhr.workStatus + '</td></tr>';
                } else if (/HOLOFF/i.test(dayhr.workStatus)) {
                    curday += '<tr class="multi"><td colspan="2" class="holoff">' + dayhr.workStatus + '</td></tr>';
                } else if (/Leave/i.test(dayhr.workStatus)) {
                    curday += '<tr class="multi"><td colspan="2" class="leave">LV</td></tr>';
                }
                curday += '<tr><td class="bt section tacshr">' + Math.round(dayhr.dailyTACShr * 100) / 100 + '</td><td class="et section selshr">' + Math.round(dayhr.dailyQREhr * 100) / 100 + '</td></tr>';
                curday += '</tbody></table>';
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
            $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();

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

