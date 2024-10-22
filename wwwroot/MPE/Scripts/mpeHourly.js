let DateTime = luxon.DateTime;
let appData = {};
let siteTours = {};
let siteInfo = {};
let ianaTimeZone = "";
let CurrentTripMin = 0;
let MPEName = "";
let TourNumber = "";
let tourHours = [];
let MPEhourlyMaxdate = null;
let MPEhourlyMindate = null;
let colHourcount = 0;
let retryCount = 0;
const MPETabel = "mpeHourlytable";
const maxRetries = 5;
const mpeHourlyConnection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();


mpeHourlyConnection.on("updateMPEzoneRunPerformance", async (data) => {
    if (data.mpeId === MPEName) {
        Promise.all([buildDataTable(data)]);
    }
   
});
$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    MPEName = (results !== null) ? results[1] || 0 : "";
    return MPEName;
}
$(function () {
    setHeight();
    document.title = $.urlParam("mpeStatus");
    RequestDate = getUrlParameter("Date");
    $(document).prop('title', " Machine Hourly Report" + ' (' + MPEName + ')');
    $.ajax({
        url: SiteURLconstructor(window.location) + '/api/SiteInformation/SiteInfo',
        type: 'GET',
        success: function (data) {
            //if data is not null
            if (data != null) {
                siteInfo = data;
                siteTours = data.tours;
                // Use the mapping function to get the correct IANA time zone
                ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
                //localdateTime = luxon.DateTime.local().setZone(ianaTimeZone).setZone("system", { keepLocalTime: true });
                
            }
        },
        error: function (error) {
            console.log(error);
        },
        failure: function (fail) {
            console.log(fail);
        }
    });
});

async function mpeHourlySignalRstart() {
    try {
        //createMPEDataTable(MPETabel);
        await mpeHourlyConnection.start();
        console.log("SignalR Connected.");
        retryCount = 0; // Reset retry count on successful connection
        initializeMpeHourly();

    } catch (err) {
        console.log("Connection failed: ", err);
        retryCount++;
        if (retryCount <= maxRetries) {
            const delay = Math.min(5000 * Math.pow(2, retryCount), 30000); // Exponential backoff with a cap
            const jitter = Math.random() * 1000; // Add jitter
            setTimeout(mpeHourlySignalRstart, delay + jitter);
        } else {
            console.error("Max retries reached. Could not connect to SignalR.");
            showConnectionStatus("Unable to connect. Please check your network.");
        }
    }
};

function initializeMpeHourly() {
    try {
        // Start the connection
        //createMPEDataTable("mpeHourlytable");
        mpeHourlyConnection.invoke("GetApplicationInfo").then(function (data) {
            appData = JSON.parse(data);

            //init_ApplicationConfiguration();

        }).catch(function (err) {
            console.error("Error loading application info: ", err);
        });
        mpeHourlyConnection.invoke("GetGeoZoneMPEData", MPEName).then(function (mpeData) {
            $('label[id=mpeName]').text(mpeData.mpeId);
            Promise.all([buildDataTable(mpeData)]);
        }).catch(function (err) {
            console.error("Error Fetching data for MPE " + MPEName , err);
        });

        mpeHourlyConnection.invoke("JoinGroup", "MPE").then(function (data) {
            console.log("Connected to Group:", "MPEZones");
        }).catch(function (err) {
            return console.error(err.toString());
        });
    } catch (err) {
        console.log("Connection failed: ", err);
    }
}
mpeHourlyConnection.onclose(async () => {
    console.log("Connection closed. Attempting to reconnect...");
    showConnectionStatus("Connection lost. Attempting to reconnect...");
    await start();
});

mpeHourlyConnection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
    showConnectionStatus("Reconnecting...");
});

mpeHourlyConnection.onreconnected((connectionId) => {
    console.log("Reconnected. Connection ID: ", connectionId);
    showConnectionStatus("Reconnected.");
});


mpeHourlyConnection.on("updateMPEView" + MPEName, async (data) => {

    await findMpeZoneLeafletIds(data.zoneId)
        .then(leafletIds => {
            geoZoneMPE._layers[leafletIds].feature.properties.mpeRunPerformance = data;
            geoZoneMPE._layers[leafletIds].setStyle({
                weight: 1,
                opacity: 1,
                fillOpacity: 0.2,
                fillColor: GetMacineBackground(data),
                lastOpacity: 0.2
            });
        });
});
async function buildDataTable(data) {
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
        "Name": "Target",
        "TourTotal": 29000
    }
    let tabledataTargetReject = {};
    tabledataTargetReject = {
        "order": 12,
        "Name": "Target %",
        "TourTotal": "4%"
    }
    for (let i = 0; i < colHourcount; i++) {
        tabledataTarget["Hour" + i] = 4500;
        tabledataTargetReject["Hour" + i] = "4%";
    }
    dataArray.push(tabledataTarget);
    //rejectArray.push(tabledataTargetReject);

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
            deltadiff = parseInt(tabledataObject["Hour" + i]) - parseInt(tabledataTarget["Hour" + i]);
            totaldelta += deltadiff;
            if (parseInt(tabledataObject["Hour" + i]) > 0) {
                rejectrate = parseInt(tabledataObjectReject["Hour" + i]) / parseInt(tabledataObject["Hour" + i]) * 100;
            }
        } else {
            deltadiff = "";
        }
        tabledataDelta["Hour" + i] = deltadiff;
        tabledataRejectRate["Hour" + i] = rejectrate + '%';
    }
    tabledataDelta["TourTotal"] = totaldelta;
    let totalrejectrate = 0;
    if (parseInt(tabledataObject["TourTotal"]) > 0) {
        totalrejectrate = parseInt(tabledataObjectReject["TourTotal"]) / parseInt(tabledataObject["TourTotal"]) * 100;
    }
    tabledataRejectRate["TourTotal"] = totalrejectrate + '%';
    dataArray.push(tabledataDelta);
    rejectArray.push(tabledataRejectRate);
    //if (TourNumber == curtour) {
    //    updateMpeDataTable("mpeHourlytable", dataArray);
    //    updateMpeDataTable("mpeRejecttable", rejectArray);
    //} else {
        TourNumber = curtour;
        $('label[id=tourNumber]').text(TourNumber);
        createMPEDataTable("mpeHourlytable", dataArray);
        createRejectDataTable("mpeRejecttable", rejectArray);
    //}
}
function getTour() {
    let curtour = "";
    let tourstlist = [];
    $.each(siteTours, function (key, value) {
        if (/tour1Start|tour2Start|tour3Start/i.test(key)) {
            let tourstart = value.split(":")[0];
            tourstlist.push(tourstart);
        }
    });
    tourstlist.sort();
    let now = luxon.DateTime.local().setZone(ianaTimeZone);
    let starthour = "";
    let endhour = "";

    if (now.hour >= tourstlist[2] || now.hour < tourstlist[0]) {
        curtour = 1;
        let nowtmp = now;
        if (nowtmp.hour < tourstlist[0]) {
            nowtmp = nowtmp.minus({ hours: 24 });
            starthour = nowtmp.year + '-' + nowtmp.month + '-' + nowtmp.day + ' ' + siteTours.tour1Start;
            endhour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour1End;
        } else if (nowtmp.hour >= tourstlist[2]) {
            nowtmp = nowtmp.plus({ hours: 24 });
            starthour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour1Start;
            endhour = nowtmp.year + '-' + nowtmp.month + '-' + nowtmp.day + ' ' + siteTours.tour1End;
        }
    } else if (now.hour < tourstlist[1]) {
        curtour = 2;
        //start time
        starthour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour2Start;
        //end time
        endhour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour2End;
    } else {
        curtour = 3;
        //start time
        starthour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour3Start;
        //end time
        endhour = now.year + '-' + now.month + '-' + now.day + ' ' + siteTours.tour3End;
    }
    MPEhourlyMindate = luxon.DateTime.fromFormat(starthour, 'yyyy-MM-dd hh:mm', { zone: ianaTimeZone });
    MPEhourlyMaxdate = luxon.DateTime.fromFormat(endhour, 'yyyy-MM-dd hh:mm', { zone: ianaTimeZone });
    return curtour;
}
function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}
function setHeight() {
    let height = (this.window.innerHeight > 0 ? this.window.innerHeight : this.screen.height) - 1;
    let screenTop = (this.window.screenTop > 0 ? this.window.screenTop : 1) - 1;
    let pageBottom = (height - screenTop);
    $("div.card").css("min-height", pageBottom + "px");
}
function showConnectionStatus(message) {
    const statusElement = document.getElementById('connection-status');
    if (statusElement) {
        statusElement.textContent = message;
        statusElement.style.display = 'block';
    }
}
// Mapping of standard time zone abbreviations to IANA time zones
const timeZoneMapping = {
    'PST': 'America/Los_Angeles',
    'PDT': 'America/Los_Angeles',
    'MST': 'America/Denver',
    'MDT': 'America/Denver',
    'CST': 'America/Chicago',
    'CDT': 'America/Chicago',
    'EST': 'America/New_York',
    'EDT': 'America/New_York',
    'HST': 'Pacific/Honolulu',
    'AKST': 'America/Anchorage',
    'AKDT': 'America/Anchorage',
    'AEST': 'Australia/Sydney',
    'AEDT': 'Australia/Sydney',
    'ACST': 'Australia/Adelaide',
    'ACDT': 'Australia/Adelaide',
    'AWST': 'Australia/Perth',
    'JST': 'Asia/Tokyo'
};
const postaltimeZoneMapping = {
    'PST1': 'PDT',
    'MST1': 'MDT',
    'CST1': 'CDT',
    'EST1': 'EDT'
};

// Function to get the IANA time zone from a standard abbreviation
function getIANATimeZone(abbreviation) {
    return timeZoneMapping[abbreviation] || abbreviation;
}
function getPostalTimeZone(abbreviation) {
    return postaltimeZoneMapping[abbreviation] || abbreviation;
}
var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return typeof sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return false;
};
function SiteURLconstructor(winLoc) {
    let pathname = winLoc.pathname;
    let match = pathname.match(/^\/([^\/]*)/);
    let urlPath = match[1];
    if (/^(CF)/i.test(urlPath)) {
        return winLoc.origin + "/" + urlPath;
    }
    else {
        return winLoc.origin;
    }
}

let MPEDataTableList = [];
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
        title: 'Operations'
    });

    columnNames = tourHours;
    for (var i in columnNames) {
        columns.push({
            data: "Hour" + i,
            title: columnNames[i]
        });
    }
    columns.push({
        data: 'TourTotal',
        title: 'Tour TTL'
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
            $(row).find('td').css('font-size', 'calc(0.1em + 1.9vw)');
            if (/Delta/i.test(data.Name)) {
                for (let i = 0; i < row.childElementCount-2; i++) {
                    let colname = 'Hour' + i;
                    let colindex = i + 1;
                    if (parseInt(data[colname]) > 0) {
                        $(row).find('td:eq(' + colindex + ')').addClass('green');
                    } else {
                        $(row).find('td:eq(' + colindex + ')').addClass('red');
                    }
                }
                let coltindex = row.childElementCount - 1;
                if (parseInt(data["TourTotal"]) > 0) {
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
        title: 'Rejects'
    });

    columnNames = tourHours;
    for (var i in columnNames) {
        columns.push({
            data: "Hour" + i,
            title: columnNames[i]
        });
    }
    columns.push({
        data: 'TourTotal',
        title: 'Tour TTL'
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
            $(row).find('td').css('font-size', 'calc(0.1em + 1.9vw)');
            if (/Actual/i.test(data.Name)) {
                for (let i = 1; i < row.childElementCount; i++) {
                    //if (parseInt(data[i]) > 0) {
                        $(row).find('td:eq(' + i + ')').addClass('green');
                    //} else {
                    //    $(row).find('td:eq(' + i + ')').addClass('red');
                    //}
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
mpeHourlySignalRstart();