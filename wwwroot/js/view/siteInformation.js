
//connection types
let siteInfotable = "siteInfotable";
async function init_SiteInformation(siteNassCode) {
    try {
        createSiteInfoDataTable(siteInfotable);

        //get data from application configuration controller

        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/SiteInformation/GetSiteInfo?NASSCode=' + siteNassCode,

            type: 'GET',
            success: function (data) {
                loadSiteInfoDatatable(formatSiteInfodata(data), siteInfotable);
            },
            error: function (error) {
                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });


    } catch (e) {
        throw new Error(e.toString());
    }
}
function createSiteInfoDataTable(table) {
    let arrayColums = [{
        "KEY_NAME": "",
        "VALUE": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/KEY_NAME/i.test(key)) {
            tempc = {
                "title": 'Name',
                "width": "30%",
                "mDataProp": key
            }
        }
        //else if (/VALUE/i.test(key)) {
        else if (/VALUE/i.test(key)) {
            tempc = {
                "title": "Value",
                "width": "50%",
                "mDataProp": key
            }
        }
        columns.push(tempc);

    });
    $('#' + table).DataTable({
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
        columnDefs: [
        ],
        sorting: [[0, "asc"]]

    })
}
function loadSiteInfoDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
function updateSiteInfoDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            for (const element of newdata) {
                if (data.KEY_NAME === element.KEY_NAME) {
                    $('#' + table).DataTable().row(node).data(element).draw().invalidate();
                }
            }
        })
        if (loadnew) {
            loadSiteInfoDatatable(newdata, table);
        }
    }
}
function formatSiteInfodata(result) {
    let reformatdata = [];
    try {
        for (let key in result) {
            if (result.hasOwnProperty(key) && key !== "tours" && key !== "nassCode") {
                let temp = {
                    "KEY_NAME": "",
                    "VALUE": ""
                };
                temp['KEY_NAME'] = key;
                temp['VALUE'] = result[key];
                reformatdata.push(temp);
            }
            //check if the key is an object
            if (typeof result[key] === 'object') {
                let tours = result[key];
                for (let key in tours) {
                    if (tours.hasOwnProperty(key) && key !== "siteId") {
                        let temp = {
                            "KEY_NAME": "",
                            "VALUE": ""
                        };
                        temp['KEY_NAME'] = key;
                        temp['VALUE'] = tours[key];
                        reformatdata.push(temp);
                    }
                }
            }
           
        }

    } catch (e) {
        throw new Error(e.toString());
    }

    return reformatdata;
}