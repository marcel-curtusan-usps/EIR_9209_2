let connecttimer = 0;
let connectattp = 0;
let MPEName = "";
let RequestDate = "";
let statDate = "";
let endDate = "";
let DateTimeNow = new Date();
let CurrentTripMin = 0;
let CountTimer = 0;
let TimerID = -1;
let Timerinterval = 1;
let siteTours = {};
let siteInfo = {};
let hourlyMPEdata = {};
let sortPlandata = {};
let timezone = {};
let localdateTime = null;
let MPEdefaultMaxdate = null;
let MPEdefaultMindate = null;
let ianaTimeZone = null;
let formattedDate = "";

$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    MPEName = (results !== null) ? results[1] || 0 : "";
    return MPEName;
}
$(function () {
    document.title = $.urlParam("mpeStatus");
    setHeight();
    RequestDate = getUrlParameter("Date");
    $('span[id=headerIdSpan]').text(MPEName + " Machine Performance");

    $.ajax({
        url: SiteURLconstructor(window.location) + '/api/SiteInformation/SiteInfo',
        type: 'GET',
        success: function (data) {
            //if data is not null
            if (data != null) {
                siteTours = data.tours;
                siteInfo = data;
                // Use the mapping function to get the correct IANA time zone
                ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
                localdateTime = luxon.DateTime.local().setZone(ianaTimeZone);

                MPEdefaultMaxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
                MPEdefaultMindate = MPEdefaultMaxdate.minus({ hours: 24 }).startOf('hour');
                $('#datepicker').datepicker({
                    format: 'yyyy-mm-dd',
                    autoclose: true,
                    todayBtn: true,
                    todayHighlight:true,
                    clearBtn: true,
                    endDate: '+1d' // Limit selection to 1 day in the future
                }).on('changeDate', function (e) {
                    let selectedDate = luxon.DateTime.fromJSDate(e.date); // Convert JS Date to Luxon DateTime
                    formattedDate = selectedDate.toFormat('yyyy-MM-dd'); // Format the date
                    loadData(formattedDate);
                });
                //js date with timezone

                let today = localdateTime.toFormat('yyyy-MM-dd');
                $('#datepicker').datepicker('setDate', today);


            }
        },
        error: function (error) {
            console.log(error);
        },
        faulure: function (fail) {
            console.log(fail);
        }
    });
});
function loadData(date) {
    if (date === "") {
        date = formattedDate;
    }
    let MPESummaryURL = "";
    let MPERunActivityURL = ""
    // Get today's date in 'yyyy-mm-dd' format
    let ianaTimeZone = getIANATimeZone(getPostalTimeZone(siteInfo.timeZoneAbbr));
    localdateTime = luxon.DateTime.local().setZone(ianaTimeZone);
    let todayFormatted = localdateTime.toFormat('yyyy-MM-dd');
    let startTime = date + "T00:00:00";
    let endTime = date + "T23:59:59";
    // Check if the selected date is today
    if (date === todayFormatted) {
        MPEdefaultMaxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
        MPEdefaultMindate = MPEdefaultMaxdate.minus({ hours: 24 }).startOf('hour');
        endTime = MPEdefaultMaxdate.toFormat('yyyy-LL-dd') + "T" + MPEdefaultMaxdate.toFormat('HH:00:00');
        startTime = MPEdefaultMindate.toFormat('yyyy-LL-dd') + "T" + MPEdefaultMindate.toFormat('HH:00:00');
        MPESummaryURL = SiteURLconstructor(window.location) + '/api/MPESummary/MPENameDatetime?mpe=' + MPEName + '&' + 'startDateTime=' + startTime + '&' + 'endDateTime=' + endTime;
        MPERunActivityURL = SiteURLconstructor(window.location) + '/api/MPERunActivity/GetByMPEName?mpe=' + MPEName + '&' + 'startDateTime=' + startTime + '&' + 'endDateTime=' + endTime;
    }
    else {
        MPEdefaultMaxdate = luxon.DateTime.fromISO(endTime).startOf('hour');
        MPEdefaultMindate = MPEdefaultMaxdate.minus({ hours: 24 }).startOf('hour');
        MPESummaryURL = SiteURLconstructor(window.location) + '/api/MPESummary/MPENameDatetime?mpe=' + MPEName + '&' + 'startDateTime=' + startTime + '&' + 'endDateTime=' + endTime;
        MPERunActivityURL = SiteURLconstructor(window.location) + '/api/MPERunActivity/GetByMPEName?mpe=' + MPEName + '&' + 'startDateTime=' + startTime + '&' + 'endDateTime=' + endTime;
    }

    Promise.all([getMPESummary(MPESummaryURL)]);
    Promise.all([getMPERunActivity(MPERunActivityURL)]);
}
async function getMPESummary(url) {
    try {
        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
               // hourlyMPEdata = data.length > 0 ? data[0] : [];
                Promise.all([updateMPEPerformanceSummaryStatus(data, MPEdefaultMindate, MPEdefaultMaxdate)]);

            },
            error: function (error) {
                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            }
        });
    } catch (e) {
        console.log(e);
    }
}
async function getMPERunActivity(url) {
    try {
        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
                sortPlandata = data;
                Promise.all([updateRunVsPlan(data, 'ganttRunVsPlan')]);
                Promise.all([loadCurrentRun(data)]);

            },
            error: function (error) {
                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            }
        });
    } catch (e) {
        console.log(e);
    }
}
//async function LoadData() {
//    //console.log("Connected time: " + new Date($.now()));
//    if (!!MPEName) {
//        //load tours
//        fotfmanager.server.getSiteTours().done(async (data) => {
//            siteTours = data;
//        }).then(() => {

//            //load gantt chart
//            fotfmanager.server.getMPErunVsPlan(MPEName).done(async (data) => {
//                sortPlandata = data;
//                Promise.all([updateRunVsPlan(data, 'ganttRunVsPlan')]);
//                Promise.all([loadCurrentRun(data)]);
//            });

//            //load hourly table
//            fotfmanager.server.getMPEPerformanceSummaryList(MPEName).done(async (data) => {

//                hourlyMPEdata = data.length > 0 ? data[0] : [];
//                Promise.all([updateMPEPerformanceSummaryStatus(hourlyMPEdata, MPEdefaultMindate, MPEdefaultMaxdate)]);

//            });
//            //join the Mpe Performance group to get updates
//            fotfmanager.server.joinGroup("MPEPerformance_" + MPEName)
//        });
//    }
//};
async function updateRunVsPlan(data, chartId) {
    GanttChart(chartId, data, MPEName)
}
async function loadSpecificTimeData(StartTime, EndTime) {
    let endTime = EndTime.toFormat('yyyy-LL-dd') + "T" + EndTime.toFormat('HH:59:59');
    let startTime = StartTime.toFormat('yyyy-LL-dd') + "T" + StartTime.toFormat('HH:00:00');
   let MPESummaryURL = SiteURLconstructor(window.location) + '/api/MPESummary/MPENameDatetime?mpe=' + MPEName + '&' + 'startDateTime=' + startTime + '&' + 'endDateTime=' + endTime;
    Promise.all([getMPESummary(MPESummaryURL)]);
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
    'PST2': 'PDT',
    'MST1': 'MDT',
    'MST2': 'MDT',
    'CST1': 'CDT',
    'CST2': 'CDT',
    'EST1': 'EDT',
    'EST2': 'EDT'
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