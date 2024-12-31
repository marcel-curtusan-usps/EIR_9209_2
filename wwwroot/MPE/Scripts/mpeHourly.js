let DateTime = luxon.DateTime;
let appData = {};
let siteTours = {};
let siteInfo = {};
let ianaTimeZone = "";
let MPEName = "";
let TourNumber = "";
let DateProvided = "";
let tourHours = [];
let mpeTartgets = {};
let mpeRunData = {};
let tourDates = null;
let colHourcount = 0;
let retryCount = 0;
let currentTime = null;
let currentTour = null;


const maxRetries = 5;
const mpeHourlyConnection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();
/**
* Extracts a URL parameter by name from the current window location.
* @param {string} name - The name of the URL parameter to extract.
* @returns {string} The value of the URL parameter.
*/
$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    return (results !== null) ? results[1] || 0 : "";
}

$(function () {
    MPEName = $.urlParam("MpeName");
    document.title = MPEName;  
    TourNumber = parseInt($.urlParam("TourNumber"), 10);
    DateProvided = $.urlParam("Date");
    setHeight();
    $('span[id=headerIdSpan]').text(MPEName + " Machine Performance");
});

/**
 * Starts the SignalR connection with retry logic using exponential backoff and jitter.
 */
async function mpeHourlyStart() {
    try {
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
            setTimeout(mpeHourlyStart, delay + jitter);
        } else {
            console.error("Max retries reached. Could not connect to SignalR.");
            showConnectionStatus("Unable to connect. Please check your network.");
        }
    }
};
/**
 * Initializes the MPE hourly data by invoking various methods on the SignalR connection.
 */
function initializeMpeHourly() {
    try {
        // Start the connection
        $('button[id=mpeName]').text(MPEName);
        mpeHourlyConnection.invoke("GetApplicationInfo").then(function (data) {
            appData = JSON.parse(data);
            //if data is not null
            if (appData != null) {
                siteInfo = appData;
                siteTours = JSON.parse(appData.Tours);
                // Use the mapping function to get the correct IANA time zone
                ianaTimeZone = getIANATimeZone(getPostalTimeZone(appData.TimeZoneAbbr));
                if (DateProvided != "") {
                    currentTime = luxon.DateTime.fromFormat(DateProvided, 'yyyy-MM-dd');
                }
                else {
                    currentTime = luxon.DateTime.local().setZone(ianaTimeZone);
                }
                if (TourNumber > 0) {
                    $('button[id=tourNumber]').text(TourNumber);
                    $('button[id=tourDates]').text(getTourDates(TourNumber));
                    createhourlyTargetDataTable(TourNumber);
                    createhourlyRejectDataTable(TourNumber);
                } else
                {
                    $('button[id=tourNumber]').text(getTour());
                    $('button[id=tourDates]').text(tourDates);
                    createhourlyTargetDataTable(getTour());
                    createhourlyRejectDataTable(getTour());
                }
            }
        }).catch(function (err) {
            console.error("Error loading application info: ", err);
        });
        mpeHourlyConnection.invoke("GetMPETargets", MPEName).then((mpeTargetsData) => {
            if (mpeTargetsData) {
                mpeTartgets = mpeTargetsData;
            }

        }).then(() => {
            mpeHourlyConnection.invoke("GetGeoZoneMPEData", MPEName).then((mpeData) => {
                if (mpeData) {
                    mpeRunData = mpeData;
                    if (TourNumber > 0) {
                        createLoadMPEHourData(TourNumber, mpeTartgets, mpeRunData);
                        createLoadMPERejectHourData(TourNumber, mpeTartgets, mpeRunData);
                    }
                    else {
                        createLoadMPEHourData(getTour(), mpeTartgets, mpeRunData);
                        createLoadMPERejectHourData(getTour(), mpeTartgets, mpeRunData);
                    }
                }

            }).catch(function (err) {
                console.error("Error Fetching data for MPE " + MPEName, err);
            });
        }).catch(function (err) {
            console.error("Error Fetching data for MPE " + MPEName, err);
        });


        mpeHourlyConnection.invoke("JoinGroup", "MPE").then(function (data) {
            console.log("Connected to Group:", "MPEZones");
        }).catch(function (err) {
            return console.error(err.toString());
        });
        mpeHourlyConnection.invoke("JoinGroup", "MPETartgets").then(function (data) {
            console.log("Connected to Group:", "MPETartgets");
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

/**
 * Displays the connection status message to the user.
 * @param {string} message - The message to display.
 */
function showConnectionStatus(message) {
    const statusElement = document.getElementById('connection-status');
    if (statusElement) {
        statusElement.textContent = message;
        statusElement.style.display = 'block';
    }
}

mpeHourlyStart();
/**
 * Determines the current tour based on the current time and the tour start times defined in siteTours.
 * @returns {number} The current tour number.
 */
function getTourHours(tournumber) {
    if (tournumber) {
        const DateTime = luxon.DateTime;
        const Duration = luxon.Duration;
        let startTime = siteTours['tour' + tournumber + 'Start'];
        let endTime = siteTours['tour' + tournumber + 'End'];
        const interval = "01:00"

        let dtStart = DateTime.fromFormat(startTime, "HH:mm");
        let dtEnd = DateTime.fromFormat(endTime, "HH:mm");
        if (dtStart > dtEnd) {
            dtStart = dtStart.minus({ hours: 24 });
        }
        const durationInterval = Duration.fromISOTime(interval);

        let tourhours = [];
        let i = dtStart;
        while (i < dtEnd) {
            tourhours.push(i.toFormat("HH:mm"));
            i = i.plus(durationInterval);
        }
        return tourhours;
    }
}
function getTourDates(tournumber) {
    if (tournumber == 1) {
        if (DateProvided != "") {
            let currentTimetmp = currentTime;
            currentTimetmp = currentTimetmp.plus({ days: 1 });
            tourDates = currentTime.month + '/' + currentTime.day + ' ' + siteTours.tour1Start + ' - ' + currentTimetmp.month + '/' + currentTimetmp.day + ' ' + siteTours.tour2Start;
        } else {
            let tour1hour = siteTours.tour1Start.split(":")[0];
            if (currentTime.hour >= tour1hour) {
                let currentTimetmp = currentTime;
                currentTimetmp = currentTimetmp.plus({ days: 1 });
                tourDates = currentTime.month + '/' + currentTime.day + ' ' + siteTours.tour1Start + ' - ' + currentTimetmp.month + '/' + currentTimetmp.day + ' ' + siteTours.tour2Start;
            } else {
                let currentTimetmp = currentTime;
                currentTimetmp = currentTimetmp.minus({ days: 1 });
                tourDates = currentTimetmp.month + '/' + currentTimetmp.day + ' ' + siteTours.tour1Start + ' - ' + currentTime.month + '/' + currentTime.day + ' ' + siteTours.tour2Start;
            }
        }
    } else if (tournumber == 2) {
        tourDates = currentTime.month + '/' + currentTime.day + ' ' + siteTours.tour2Start + ' - ' + siteTours.tour3Start;
    } else {
        tourDates = currentTime.month + '/' + currentTime.day + ' ' + siteTours.tour3Start + ' - ' + siteTours.tour1Start;
    }
    return tourDates;
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
    if (now.hour >= tourstlist[2] || now.hour < tourstlist[0]) {
        curtour = 1;
        let nowtmp = now;
        if (nowtmp.hour < tourstlist[0]) {
            nowtmp = nowtmp.minus({ hours: 24 });
            tourDates = nowtmp.month + '/' + nowtmp.day + ' ' + siteTours.tour1Start + ' - ' + now.month + '/' + now.day + ' ' + siteTours.tour2Start;
        } else if (nowtmp.hour >= tourstlist[2]) {
            nowtmp = nowtmp.plus({ hours: 24 });
            tourDates = now.month + '/' + now.day + ' ' + siteTours.tour1Start + ' - ' + nowtmp.month + '/' + nowtmp.day + ' ' + siteTours.tour2Start;
        }
    } else if (now.hour < tourstlist[1]) {
        curtour = 2;
        tourDates = now.month + '/' + now.day + ' ' + siteTours.tour2Start + ' - ' + siteTours.tour3Start;
    } else {
        curtour = 3;
        tourDates = now.month + '/' + now.day + ' ' + siteTours.tour3Start + ' - ' + siteTours.tour1Start;
    }
    return curtour;
}

/**
 * Capitalizes the first letter of each word in a string.
 * @param {string} str - The string to capitalize.
 * @returns {string} The capitalized string.
 */
function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}
/**
 * Sets the minimum height of elements with the class card to ensure they occupy the full available height of the viewport.
 */
function setHeight() {
    let height = (this.window.innerHeight > 0 ? this.window.innerHeight : this.screen.height) - 1;
    let screenTop = (this.window.screenTop > 0 ? this.window.screenTop : 1) - 1;
    let pageBottom = (height - screenTop);
    $("div.card").css("min-height", pageBottom + "px");
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
    'MST2': 'MST',
    'CST1': 'CDT',
    'CST2': 'CDT',
    'EST1': 'EDT',
    'EST2': 'EDT'
};

/**
 * Maps standard time zone abbreviations to IANA time zones.
 * @param {string} abbreviation - The standard time zone abbreviation.
 * @returns {string} The corresponding IANA time zone.
 */
function getIANATimeZone(abbreviation) {
    return timeZoneMapping[abbreviation] || abbreviation;
}
/**
 * Maps postal time zone abbreviations to standard time zone abbreviations.
 * @param {string} abbreviation - The postal time zone abbreviation.
 * @returns {string} The corresponding standard time zone abbreviation.
 */
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
/**
 * Constructs the site URL based on the window location.
 * @param {Location} winLoc - The window location object.
 * @returns {string} The constructed site URL.
 */
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

