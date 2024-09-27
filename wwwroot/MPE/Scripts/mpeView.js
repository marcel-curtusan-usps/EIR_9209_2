let MPEName = "";
let retryCount = 0;
const maxRetries = 5;
$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    MPEName = (results !== null) ? results[1] || 0 : "";
    return MPEName;
}
$(function () {
    document.title = $.urlParam("mpeStatus");
    RequestDate = getUrlParameter("Date");
    $(document).prop('title', " Machine View" + ' (' + MPEName + ')');
});

const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();
async function mpeViewSignalRstart() {
    try {
        await connection.start();
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
        connection.invoke("GetApplicationInfo").then(function (data) {
            appData = JSON.parse(data);

            //init_ApplicationConfiguration();

        }).catch(function (err) {
            console.error("Error loading application info: ", err);
        });
    } catch (err) {
        console.log("Connection failed: ", err);
    }
}
connection.onclose(async () => {
    console.log("Connection closed. Attempting to reconnect...");
    showConnectionStatus("Connection lost. Attempting to reconnect...");
    await start();
});

connection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
    showConnectionStatus("Reconnecting...");
});

connection.onreconnected((connectionId) => {
    console.log("Reconnected. Connection ID: ", connectionId);
    showConnectionStatus("Reconnected.");
});

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
function SiteURLconstructor(winLoc) {
    if (/^(.CF)/i.test(winLoc.pathname)) {
        return winLoc.origin + "/CF";
    }
    else {
        return winLoc.origin;
    }
}
mpeViewSignalRstart();