/// A simple template method for replacing placeholders enclosed in curly braces.

let connecttimer = 0;
let connectattp = 0;
let MPEName = "";
let RequestDate = "";
let fotfmanager = $.connection.FOTFManager;
let DateTimeNow = new Date();
let CurrentTripMin = 0;
let CountTimer = 0;
let TimerID = -1;
let Timerinterval = 1;
let timezone = {};
let localdateTime = null;
let defaultMaxdate = null;
let defaultMindate = null;
$.extend(fotfmanager.client, {
    updateSiteStatus: (updatedata) => { Promise.all([updateSitePerformanceSummaryStatus(updatedata, RequestDate, "", "")]) },
    initSiteStatus: async (initdata) => {
        Promise.all([init_siteStatus(initdata)]);
    },
    siteInfo: async (data) => {
        if (data.hasOwnProperty("timeZoneName")) {
            timezone = data.timeZoneName;
            localdateTime = luxon.DateTime.local().setZone(timezone);

            defaultMaxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
            defaultMindate = defaultMaxdate.minus({ hours: 24 }).startOf('hour');
            Promise.all([LoadData()])
        }
    }
});
$(function () {
    setHeight();;
    RequestDate = getUrlParameter("Date");
    //start connection 
    $.connection.hub.qs = { 'page_type': "SitePerformance".toUpperCase() };
    $.connection.hub.start({ waitForPageLoad: false })
        .done(() => {
        }).catch(
            function (err) {
             throw new Error(err.toString());
            });
    //handling Disconnect
    $.connection.hub.disconnected(function () {
        connecttimer = setTimeout(function () {
            if (connectattp > 10) {
                clearTimeout(connecttimer);
            }
            connectattp += 1;
            $.connection.hub.start({ waitForPageLoad: false })
                .done(() => {
         
                 
                })
                .catch(function (err) {
                    throw new Error(err.toString());
                    //console.log(err.toString());
                });
        }, 10000); // Restart connection after 10 seconds.
    });
    //Raised when the underlying transport has reconnected.
    $.connection.hub.reconnecting(function () {
        clearTimeout(connecttimer);
        console.log("reconnected at time: " + new Date($.now()));
        fotfmanager.server.joinGroup("SitePerformance");
    });
});
async function LoadData() {
    console.log("Connected time: " + new Date($.now()));
    fotfmanager.server.getMPEPerformanceSummaryList("").done(async (data) => {
        Promise.all([updateSitePerformanceSummaryStatus(data, "",defaultMaxdate, defaultMindate)])
    });

    fotfmanager.server.getSiteTours().done(async (data) => {
        if (!!data) {
            const tourlist = [];
            if (data.tour1Start) {
                tourlist.push({ name: "TOUR 1", compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + data.tour1Start), tourStart: data.tour1Start, tourEnd: data.tour1End });
            }
            if (data.tour2Start) {
                tourlist.push({ name: "TOUR 2", compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + data.tour2Start), tourStart: data.tour2Start, tourEnd: data.tour2End });
            }
            if (data.tour3Start) {
                tourlist.push({ name: "TOUR 3", compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + data.tour3Start), tourStart: data.tour3Start, tourEnd: data.tour3End });
            }
            tourlist.sort(compareDates);

            const tours = [];
            if (tourlist[0].compareDate < defaultMaxdate.minus({ hours: 3 })) {
                tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourEnd });
                tours.push({ name: tourlist[1].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourEnd });
                tours.push({ name: tourlist[0].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourStart, endTime: defaultMaxdate.plus({ days: 1 }).toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourEnd });
            } else if (tourlist[1].compareDate < defaultMaxdate.minus({ hours: 3 })) {
                tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourEnd });
                tours.push({ name: tourlist[1].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourEnd });
                tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourEnd });
            } else if (tourlist[2].compareDate < defaultMaxdate.minus({ hours: 3 })) {
                tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourEnd });
                tours.push({ name: tourlist[1].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourEnd });
                tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourEnd });
            } else {
                tours.push({ name: tourlist[2].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[2].tourEnd });
                tours.push({ name: tourlist[1].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[1].tourEnd });
                tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + "T" + tourlist[0].tourEnd });
            }
            console.log(tours);
            //create the tours dropdown values
            $("#ddlTourSelect").html("");
            $("#ddlTourSelect").append($("<option/>", { value: ",", text: "Current MODS" }));
            Object.keys(tours).forEach((key, num) => {
                let valueString =tours[key].startTime + "," + tours[key].endTime;
                $("#ddlTourSelect").append($("<option/>", { value: valueString, text: tours[key].name }));
            });

            $('#ddlTourSelect').off().on('change', function () {;
                if ($(this).val() === ",") {
                    fotfmanager.server.getMPEPerformanceSummaryList("").done(async (data) => {
                        Promise.all([updateSitePerformanceSummaryStatus(data, "", defaultMaxdate, defaultMindate)])
                    });
                } else {
                    let startEndTime = $(this).val().split(',');
                    fotfmanager.server.getMPEPerformanceSummaryList("").done(async (data) => {
                        Promise.all([updateSitePerformanceSummaryStatus(data, RequestDate, luxon.DateTime.fromISO(startEndTime[1]), luxon.DateTime.fromISO(startEndTime[0]))])
                    });

                }
            });
        }
    });
};


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

function compareDates(a, b) {
    return b['compareDate'].toMillis() - a['compareDate'].toMillis()
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