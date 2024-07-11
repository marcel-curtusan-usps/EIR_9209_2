
let valuesArray = null;
let weekDates = [];

async function updateEmployeeSchedule(data) {

    try {
        //Get dates for the week
        let daynum = data[0]["weekSchedule"][0]["day"];
        let daydate = data[0]["weekSchedule"][0]["endTourDtm"];
        const DateTime = luxon.DateTime;
        for (let i = 0; i < daynum - 1; i++) {
            let curdate = DateTime.fromFormat(daydate, 'MMMM, dd yyyy h:mm:ss').plus({ "days": i }).toFormat("MMMM dd");
            weekDates.push(curdate);
        }
        for (let i = daynum - 1; i < 7; i++) {
            let curdate = DateTime.fromFormat(daydate, 'MMMM, dd yyyy h:mm:ss').plus({ "days": i }).toFormat("MMMM dd");
            weekDates.push(curdate);
        }

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
            var day1 = '';
            var day2 = '';
            var day3 = '';
            var day4 = '';
            var day5 = '';
            var day6 = '';
            var day7 = '';
            var hourst = 0;
            Object.keys(curr).forEach(key => {
                if (key === 'weekSchedule') {
                    day1 = getDaySchedule(curr[key], 1);
                    day2 = getDaySchedule(curr[key], 2);
                    day3 = getDaySchedule(curr[key], 3);
                    day4 = getDaySchedule(curr[key], 4);
                    day5 = getDaySchedule(curr[key], 5);
                    day6 = getDaySchedule(curr[key], 6);
                    day7 = getDaySchedule(curr[key], 7);
                    $.each(curr[key], function (key, value) {
                        //hourst = hourst + parseFloat(value.hrSched);
                        hourst = hourst + parseFloat(value.hrMove);
                    });
                }
            });
            let totalhrs = Math.round(hourst * 10) / 10;
            let tacshrtotal = '100%';
            let selshrtotal = '100%';
            totalhrs += '<br><span class="tacshrSpan">' + tacshrtotal + '</span><span class="selshrSpan">' + selshrtotal + '</span>';
            let employee = {
                employee: curr.lastName + ', ' + curr.firstName + '<br>' + curr.ein,
                tour: curr.tourNumber,
                day1: day1,
                day2: day2,
                day3: day3,
                day4: day4,
                day5: day5,
                day6: day6,
                day7: day7,
                hourstotal: totalhrs
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
            "title": 'Employee Name',
            "width": "15%",
            "data": 'employee'
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
        }
      ]

      let EmpScheduleDataTable = $('#' + table).DataTable({
          dom: '<"search"f>Brtip',
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
            { targets: [1, 9], className: 'dt-center' }
        ],
        sorting: [[1, "asc"]],
      })
      $('#' + table + ' thead').attr("class", "thead-dark");

      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  } 
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
                let tacshr = '100%';
                let selshr = '100%';
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

