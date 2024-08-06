
let valuesArray = null;
let weekDates = [];

async function updateEmployeeSchedule(data) {

    try {
        //Get dates for the week
        weekDates = data['weekdate'];
        data = Object.keys(data).filter(objKey =>
            objKey !== 'weekdate').reduce((newObj, key) => {
                newObj[key] = data[key];
                return newObj;
            }, {}
        );

        Promise.all([
            createEmpScheduleDataTable('empScheduleData'),
            loadEmpScheduleDatatable(processScheduledata(data), 'empScheduleData')
        ]);
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
            let selshrtotal = curr[17];
            let totalhrs = curr[9];
            totalhrs += '<br><span class="tacshrSpan">' + tacshrtotal + '</span><span class="selshrSpan">' + selshrtotal + '</span>';
            if (selshrtotal == 0) {
                totalhrspercent = '';
            } else {
                totalhrspercent = '<br>' + Math.round(parseFloat(tacshrtotal) / parseFloat(selshrtotal) * 100 * 1) / 1 + '%';
            }

            let employee = {
                employee: curr[0],
                tour: curr[1],
                day1: getDayFormat(curr[2], curr[10], tacshr),
                day2: getDayFormat(curr[3], curr[11], tacshr),
                day3: getDayFormat(curr[4], curr[12], tacshr),
                day4: getDayFormat(curr[5], curr[13], tacshr),
                day5: getDayFormat(curr[6], curr[14], tacshr),
                day6: getDayFormat(curr[7], curr[15], tacshr),
                day7: getDayFormat(curr[8], curr[16], tacshr),
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
    if ($.fn.dataTable.isDataTable('#'+table)) {
       table.destroy(); // destroy the dataTableObject
       // For new version use table.destroy();
       $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
       // The line above is needed if number of columns change in the Data
    }
    return new Promise((resolve, reject) => {
    
      let columns = [
        {
            "title": 'EIN',
            "width": "15%",
            "data": 'employee'
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
            { targets: [1, 9, 10], className: 'dt-center' }
        ],
          sorting: [[1, "asc"], [10, "asc"]],
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
function getDaySchedule(row, day) {
    let curday = '<span class="offSpan">OFF</span>';
    //let curday = 'OFF';
    $.each(row, function (key, value) {
        if (value.day == day) {
            if (value.groupName == 'Holiday Off') {
                curday = '<span class="holoffSpan">HOLOFF</span>';
                //curday = 'HOLOFF';
            } else if (value.hrLeave == value.hrSched) {
                curday = '<span class="leaveSpan">LV</span>';
                //curday = 'LV';
            } else {
                curday = '<span class="tourhrSpan">' + value.btour + '-' + value.etour + '</span >';
                let tacshr = '9.86';
                let selshr = '6.85';
                curday += '<br><span class="tacshrSpan">' + tacshr + '</span><span class="selshrSpan">' + selshr + '</span>';
            }
        }
    });
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

