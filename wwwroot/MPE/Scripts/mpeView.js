let DateTime = luxon.DateTime;
let appData = {};
let siteInfo = {};
let ianaTimeZone = "";
let CurrentTripMin = 0;
let MPEName = "";
let retryCount = 0;
const MPETabel = "mpeStatustable";
const maxRetries = 5;
const mpeViewConnection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();


mpeViewConnection.on("updateMPEzoneRunPerformance", async (data) => {
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
    $(document).prop('title', " Machine View" + ' (' + MPEName + ')');
});


async function mpeViewSignalRstart() {
    try {
        createMPEDataTable(MPETabel);
        await mpeViewConnection.start();
        console.log("SignalR Connected.");
        retryCount = 0; // Reset retry count on successful connection
        initializeMpeView();

    } catch (err) {
        console.log("Connection failed: ", err);
        retryCount++;
        if (retryCount <= maxRetries) {
            const delay = Math.min(5000 * Math.pow(2, retryCount), 30000); // Exponential backoff with a cap
            const jitter = Math.random() * 1000; // Add jitter
            setTimeout(mpeViewSignalRstart, delay + jitter);
        } else {
            console.error("Max retries reached. Could not connect to SignalR.");
            showConnectionStatus("Unable to connect. Please check your network.");
        }
    }
};
function createMPEDataTable(table) {
    let arrayColums = [{
        "order": "",
        "Name": "",
        "Planned": "",
        "Actual": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/Planned/i.test(key)) {
            tempc = {
                "title": 'Planned',
                "mDataProp": key,
                "class": "col-planned text-center"
            }
        }
        else if (/Actual/i.test(key)) {
            tempc = {
                "title": "Actual",
                "mDataProp": key,
                "class": "col-actual text-center"
            }
        }
        else if (/Name/i.test(key)) {
            tempc = {
                "title": "",
                "mDataProp": key,
                "class": "col-name text-right"
            }
        }
        else {
            tempc = {
                "title": capitalize_Words(key.replace(/\_/, ' ')),
                "mDataProp": key
            }
        }
        columns.push(tempc);
    });
    $('#' + table).DataTable({
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
        aoColumns: columns,
        sorting: [[0, "asc"]],
        columnDefs: [{
            visible: false,
            targets: 0,
        }],
        //rowCallback: function (row, data, index) {
        rowCallback: function (row) {
            $(row).find('td').css('font-size', 'calc(0.1em + 2.6vw)');
        }
    });
}
function initializeMpeView() {
    try {
        // Start the connection
        createMPEDataTable("mpeStatustable");
        mpeViewConnection.invoke("GetApplicationInfo").then(function (data) {
            appData = JSON.parse(data);

            //init_ApplicationConfiguration();

        }).catch(function (err) {
            console.error("Error loading application info: ", err);
        });
        mpeViewConnection.invoke("GetGeoZoneMPEData", MPEName).then(function (mpeData) {
            $('label[id=mpeName]').text(mpeData.mpeId);
            $('label[id=opn]').text(mpeData.curOperationId);
            $('label[id=sortplan_name_text]').text(mpeData.curSortplan);
            $('label[id=mpe_status]').text(MPEStatus(mpeData));
            Promise.all([buildDataTable(mpeData)]);
        }).catch(function (err) {
            console.error("Error Fetching data for MPE " + MPEName , err);
        });

        mpeViewConnection.invoke("JoinGroup", "MPE").then(function (data) {
            console.log("Connected to Group:", "MPEZones");
        }).catch(function (err) {
            return console.error(err.toString());
        });
    } catch (err) {
        console.log("Connection failed: ", err);
    }
}
mpeViewConnection.onclose(async () => {
    console.log("Connection closed. Attempting to reconnect...");
    showConnectionStatus("Connection lost. Attempting to reconnect...");
    await start();
});

mpeViewConnection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
    showConnectionStatus("Reconnecting...");
});

mpeViewConnection.onreconnected((connectionId) => {
    console.log("Reconnected. Connection ID: ", connectionId);
    showConnectionStatus("Reconnected.");
});


mpeViewConnection.on("updateMPEView" + MPEName, async (data) => {

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
    let dataArray = [];
    $.each(data, function (key) {
        let tabledataObject = {};
        if (/curOperationId/i.test(key)) {
            tabledataObject = {
                "order": 3,
                "Name": "Throughput",
                "Planned": data.expectedThroughput,
                "Actual": data.curThruputOphr,
            }
            dataArray.push(tabledataObject);
        }
        if (/totSortplanVol/i.test(key)) {
            tabledataObject = {
                "order": 2,
                "Name": "Volume",
                "Planned": data.rpgEstVol,
                "Actual": data.totSortplanVol,
            }
            dataArray.push(tabledataObject);
        }
        if (/arsRecrej3/i.test(key)) {
            tabledataObject = {
                "order": 4,
                "Name": "Reject Rate",
                "Planned": 0,
                "Actual": data.arsRecrej3,
            }
            dataArray.push(tabledataObject);
        }
        if (/rpgEndDtm/i.test(key)) {
            tabledataObject = {
                "order": 5,
                "Name": "End Time",
                "Planned": VaildateEstComplete(data.rpgEndDtm,"Plan"),
                "Actual": VaildateEstComplete(data.rpgEstimatedCompletion,"Actual"),
            }
            if (CurrentTripMin === 0) {
                CountTimer = 0;
                dataArray.push(tabledataObject);
            }
        }
        if (/curSortplan/i.test(key)) {
            tabledataObject = {
                "order": 1,
                "Name": "Sort Program",
                "Planned": data.curSortplan,
                "Actual": data.curSortplan,
            }
            dataArray.push(tabledataObject);
        }
        //if (true) {
        //    tabledataObject = {
        //        "Volume": '',
        //        "Throughput": "",
        //        "Reject Rate": "",
        //        "End Time": "",
        //    }
        //}
    });
    updateMpeDataTable(dataArray, "mpeStatustable");
}
function MPEStatus(data) {
    if (/^0$/i.test(data.curSortplan)) {
        return "Idle";
    }
    else {
        return "Running";
    }
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
function startTimer(SVdtm) {
    if (!!SVdtm) {
        let duration = calculatescheduledDuration(SVdtm);
        let timer = setInterval(function () {
            if (!!duration && duration._isValid) {
                CurrentTripMin = duration.asMinutes();

                duration = moment.duration(duration.asSeconds() - Timerinterval, 'seconds');
                $('label[id=countdown]').html(duration.format("d [days] hh:mm:ss ", { trunc: true }));

            }
            else {
                stopTimer()
            }

        }, 1000);

        TimerID = timer;

        return timer;
    }
    else {
        stopTimer()
    }
};
function calculatescheduledDuration(t) {
    if (!!t) {
        let timenow = moment(DateTimeNow);
        let conditiontime = moment(t);
        if (conditiontime._isValid && conditiontime.year() === timenow.year()) {
            if (timenow > conditiontime) {
                return moment.duration(timenow.diff(conditiontime));
            }
            else {
                return moment.duration(conditiontime.diff(timenow));
            }
        }
        else {
            return "";
        }
    }
}
function VaildateEstComplete(estComplet, type) {

    try {
        let est = luxon.DateTime.fromISO(estComplet);
        //if (est._isValid && est.year === luxon.DateTime.local().year) {
        if (est.year && est.year === luxon.DateTime.local().year) {
            return est.toFormat("yyyy-MM-dd HH:mm:ss");
        }
        else {
            if (/Plan/ig.test(type)) {
                return type + " Not Available";
            }
            else if (/Actual/ig.test(type)) {
                return type +" Not Available";
            }
            else {
                return "Not Available";
            }
          
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}
function SiteURLconstructor(winLoc) {
    if (/^(.CF)/i.test(winLoc.pathname)) {
        return winLoc.origin + "/CF";
    }
    else {
        return winLoc.origin;
    }
}
function createMPEDataTable(table) {
    let arrayColums = [{
        "order": "",
        "Name": "",
        "Planned": "",
        "Actual": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/Planned/i.test(key)) {
            tempc = {
                "title": 'Planned',
                "mDataProp": key,
                "class": "col-planned text-center"
            }
        }
        else if (/Actual/i.test(key)) {
            tempc = {
                "title": "Actual",
                "mDataProp": key,
                "class": "col-actual text-center"
            }
        }
        else if (/Name/i.test(key)) {
            tempc = {
                "title": "",
                "mDataProp": key,
                "class": "col-name text-right"
            }
        }
        else {
            tempc = {
                "title": capitalize_Words(key.replace(/\_/, ' ')),
                "mDataProp": key
            }
        }
        columns.push(tempc);
    });
    $('#' + table).DataTable({
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
        aoColumns: columns,
        sorting: [[0, "asc"]],
        columnDefs: [{
            visible: false,
            targets: 0,
        }],
        //rowCallback: function (row, data, index) {
        rowCallback: function (row) {
            $(row).find('td').css('font-size', 'calc(0.1em + 2.6vw)');
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
function updateMpeDataTable(ldata, table) {
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
mpeViewSignalRstart();