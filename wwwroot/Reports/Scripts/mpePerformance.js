/// A simple template method for replacing placeholders enclosed in curly braces.
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                let r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}
let connecttimer = 0;
let connectattp = 0;
let MPEName = "";
let RequestDate = "";
let statDate = "";
let endDate = "";
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "hubServics")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
//let fotfmanager = $.connection.FOTFManager;
let DateTimeNow = new Date();
let CurrentTripMin = 0;
let CountTimer = 0;
let TimerID = -1;
let Timerinterval = 1;
let siteTours = {};
let hourlyMPEdata = {};
let sortPlandata = {};
let timezone = {};
let localdateTime = null;
let MPEdefaultMaxdate = null;
let MPEdefaultMindate = null;

$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    MPEName = (results !== null) ? results[1] || 0 : "";
    return MPEName;
}
$(function () {
    setHeight();

    document.title = $.urlParam("mpeStatus");
    RequestDate = getUrlParameter("Date");
    $('span[id=headerIdSpan]').text(MPEName + " Machine Performance");
    /*    document.title = $.urlParam("SitePerformance");*/
    //start connection 
    //$.connection.hub.qs = { 'page_type': "MPEPerformance".toUpperCase() };
    //$.connection.hub.start({ waitForPageLoad: false })
    //    .done(() => {


    //    }).catch(
    //        function (err) {
    //            /* console.log(err.toString());*/
    //            throw new Error(err.toString());
    //        });
    ////handling Disconnect
    //$.connection.hub.disconnected(function () {
    //    connecttimer = setTimeout(function () {
    //        if (connectattp > 10) {
    //            clearTimeout(connecttimer);
    //        }
    //        connectattp += 1;
    //        $.connection.hub.start({ waitForPageLoad: false })
    //            .done(() => {
    //                Promise.all([LoadData()])
    //            })
    //            .catch(function (err) {
    //                throw new Error(err.toString());
    //                //console.log(err.toString());
    //            });
    //    }, 10000); // Restart connection after 10 seconds.
    //});
    ////Raised when the underlying transport has reconnected.
    //$.connection.hub.reconnecting(function () {
    //    clearTimeout(connecttimer);
    //    fotfmanager.server.joinGroup("MPEPerformance");
    //});
});

async function start() {
    try {
        await connection.start().then(async () => {
            //load siteinfo
            await connection.invoke("GetSiteInformation").then(function (data) {
                siteTours = data.tours;
            }).catch(function (err) {
                console.error(err);
            });
            await connection.invoke("GetMPESynopsis").then(async (data) => {
                hourlyMPEdata = data.length > 0 ? data[0] : [];
                Promise.all([updateMPEPerformanceSummaryStatus(data)]).then(function () {
                    connection.invoke("JoinGroup", "MPESynopsis").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                console.error(err);
            });

        });
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};
async function LoadData() {
    //console.log("Connected time: " + new Date($.now()));
    if (!!MPEName) {
        //load tours
        fotfmanager.server.getSiteTours().done(async (data) => {
            siteTours = data;
        }).then(() => {

            //load gantt chart
            fotfmanager.server.getMPErunVsPlan(MPEName).done(async (data) => {
                sortPlandata = data;
                Promise.all([updateRunVsPlan(data, 'ganttRunVsPlan')]);
                Promise.all([loadCurrentRun(data)]);
            });

            //load hourly table
            fotfmanager.server.getMPEPerformanceSummaryList(MPEName).done(async (data) => {

                hourlyMPEdata = data.length > 0 ? data[0] : [];
                Promise.all([updateMPEPerformanceSummaryStatus(hourlyMPEdata, MPEdefaultMindate, MPEdefaultMaxdate)]);

            });
            //join the Mpe Performance group to get updates
            fotfmanager.server.joinGroup("MPEPerformance_" + MPEName)
        });
    }
};
async function updateRunVsPlan(data, chartId) {
    GanttChart(chartId, data, MPEName)
}
async function loadSpecificTimeData(startTime, endTime) {
    let valuesArray = Object.values(hourlyMPEdata);
    let tempSpecificTimeData = [];

    let baseendTime = endTime.plus({ hours: 1 }).startOf('hour').setZone(timezone);
    let basestartTime = startTime.minus({ hours: 1 }).startOf('hour').setZone(timezone);

    $.each(valuesArray, (key, value) => {
        let keyDate = luxon.DateTime.fromISO(value.hour).setZone(timezone);
        if ((keyDate.ts >= basestartTime.ts && keyDate.ts <= baseendTime.ts)
        ) {
            let keyDateVal = keyDate.toFormat('yyyy-LL-dd') + "T" + keyDate.toFormat('HH:00:00')
            tempSpecificTimeData[keyDateVal] = value;
        }
        if (isDateInRange(keyDate, basestartTime, baseendTime)) {

            /*  tempSpecificTimeData.push([keyDate] = value);*/
        }
    });

    Promise.all([updateMPEPerformanceSummaryStatus(tempSpecificTimeData, basestartTime, baseendTime)])
}
async function loadDefaultData() {
    fotfmanager.server.getMPEPerformanceSummaryList(MPEName).done(async (data) => {
        hourlyMPEdata = data.length > 0 ? data[0] : [];
        Promise.all([updateMPEPerformanceSummaryStatus(hourlyMPEdata, MPEdefaultMindate, MPEdefaultMaxdate)])
    });

}

async function loadCurrentRun(data) {
    let SortPlan = "N/A";
    let OPNumber = 0;
    let actualthroughput_sum_run = 0;
    let actualthroughput_sum_plan = 0;
    let actualvolume_sum_run = 0;
    let actualvolume_sum_plan = 0;
    let expectedpiecesfed_sum_plan = 0;
    let expectedthroughput_sum_plan = 0;

    let RPGData = data.sort((a, b) => a.type.localeCompare(b.type)).reduce(function (a, b) {

        if (b.type === "Run" && b.activeRun) {
            OPNumber = b.opn;
            SortPlan = b.sortPlanName;
            expectedpiecesfed_sum_plan = b.expectedPiecesFed;
            actualvolume_sum_run = b.actualVolume;
            return a + b.actualThroughput;
        } else if (b.type === "Plan") {
            if (OPNumber !== 0) {
                actualthroughput_sum_plan = actualthroughput_sum_plan + b.actualThroughput;
                // actualvolume_sum_plan = actualvolume_sum_plan + b.actualVolume;
                //expectedpiecesfed_sum_plan = expectedpiecesfed_sum_plan + b.expectedPiecesFed;
                expectedthroughput_sum_plan = expectedthroughput_sum_plan + b.expectedThruput;
            }
            return a + b.actualThroughput;
        } else {
            return a;
        }
    }, 0);

    $("#operation_number").html(OPNumber + "/" + SortPlan);
    $("#machine_volume_number").html(parseFloat(actualvolume_sum_run).toLocaleString('en-US') + " / " + parseFloat(expectedpiecesfed_sum_plan).toLocaleString('en-US'));
    $("#machine_throughput_number").html(parseFloat(actualthroughput_sum_run).toLocaleString('en-US') + " / " + parseFloat(expectedthroughput_sum_plan).toLocaleString('en-US'));

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
    $("body").css("min-height", pageBottom + "px");
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
        return winLoc.origin + "/CF/";
    }
    else {
        return winLoc.origin + "/";
    }
}
// Start the connection.
start();