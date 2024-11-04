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
let DateTime = luxon.DateTime;
let appData = {};
let siteTours = [];
let baselayerid = "";
let siteInfo = {};
let siteTours = {};
let ianaTimeZone = "";
let retryCount = 0;
const maxRetries = 5;
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        retryCount = 0; // Reset retry count on successful connection
        initializeOSL();

    } catch (err) {
        console.log("Connection failed: ", err);
        retryCount++;
        if (retryCount <= maxRetries) {
            const delay = Math.min(5000 * Math.pow(2, retryCount), 30000); // Exponential backoff with a cap
            const jitter = Math.random() * 1000; // Add jitter
            setTimeout(start, delay + jitter);
        } else {
            console.error("Max retries reached. Could not connect to SignalR.");
            showConnectionStatus("Unable to connect. Please check your network.");
        }
    }
};
function initializeOSL() {
    // Load Application Info
    connection.invoke("GetApplicationInfo").then(function (data) {
        appData = JSON.parse(data);
        siteTours = JSON.parse(appData.Tours);
        Promise.all([setUserProfile()]);
        ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.TimeZoneAbbr));
        Promise.all([updateOSLattribution(appData)]);
        if (/^(Admin|OIE)/i.test(appData.Role)) {
            init_geoman_editing();
            sidebar.addPanel({
                id: 'setting',
                tab: '<span class="iconCenter"><i class="pi-iconGearFill"></i></span>',
                position: 'bottom',
            });
            Promise.all([init_applicationConfiguration()]);
            Promise.all([init_SiteInformation()]);
            init_connectiontType();
            init_emailList();
            init_dacodetocraftType();
        }
        init_connection();
        init_backgroundImages();
        init_osl();
        init_TagSearch();
        init_geoZoneArea();  
        init_geoZoneMPE();
        init_geoZoneDockDoor();
        init_accessPoints();
        init_tagsAGV();
        init_tagsPIV();
        init_tagsCamera();
        init_tags();
        $(`span[id="fotf-site-facility-name"]`).text(appData.SiteName);
    }).catch(function (err) {
        console.error("Error loading application info: ", err);
    });
    
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
// Start the connection.
start();
async function setUserProfile() {
    if (!$.isEmptyObject(appData)) {
        var userid = appData.User;
        $('#userfullname').text(userid);
        $('#useremail').text(appData.EmailAddress);
        $('#userphone').text(appData.Phone);
        $('#usertitel').text(appData.Role);
    }
}
function checkValue(value) {
    switch (value) {
        case "": return false;
        case null: return false;
        case "undefined": return false;
        case undefined: return false;
        default: return true;
    }
}
function SortByNumber(a, b) {
    return a - b;
}
function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}
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
function hideSidebarLayerDivs() {

    $('div[id=agvlocation_div]').css('display', 'none');
    $('div[id=area_div]').css('display', 'none');
    $('div[id=bullpen_div]').css('display', 'none');
    $('div[id=dockdoor_div]').css('display', 'none');
    $('div[id=trailer_div]').css('display', 'none');
    $('div[id=machine_div]').css('display', 'none');
    $('div[id=staff_div]').css('display', 'none');
    $('div[id=ctstabs_div]').css('display', 'none');
    $('div[id=vehicle_div]').css('display', 'none');
    $('div[id=dps_div]').css('display', 'none');
    $('div[id=layer_div]').css('display', 'none');
    $('div[id=dockdoor_tripdiv]').css('display', 'none');

}
function get_pi_icon(name, type) {
    if (/Vehicle$/i.test(type)) {
        if (checkValue(name)) {
            if (/^(wr|walkingrider)/i.test(name)) {
                return "pi-iconLoader_wr ml--24";
            }
            if (/^(fl|forklift)/i.test(name)) {
                return "pi-iconLoader_forklift ml--8";
            }
            if (/^(t|tug|mule)/i.test(name)) {
                return "pi-iconLoader_tugger ml--16";
            }
            if (/^agv_t/i.test(name)) {
                return "pi-iconLoader_avg_t ml--8";
            }
            if (/^agv_p/i.test(name)) {
                return "pi-iconLoader_avg_pj ml--16";
            }
            if (/^ss/i.test(name)) {
                return "pi-iconVh_ss ml--16";
            }
            if (/^bf/i.test(name)) {
                return "pi-iconVh_bss ml--16";
            }
            if (/^Surfboard/i.test(name)) {
                return "pi-iconSurfboard ml--32";
            }
            return "pi-iconVh_ss ml--16";
        }
        else {
            return "pi-iconVh_ss ml--16";
        }
    }
    else {
        return "pi-iconVh_ss ml--16";
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
function IPAddress_validator(value) {
    let ipPattern = /^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/;
    let ipArray = value.match(ipPattern);
    if (value === "0.0.0.0" || value === "255.255.255.255" || ipArray === null) {
        return "Invalid IP Address";
    } else {
        for (let i = 1; i < ipArray.length; i++) {
            let thisSegment = ipArray[i];
            if (thisSegment > 255) {
                return "Invalid IP Address";
            }
            if (i === 0 && thisSegment > 255) {
                return "Invalid IP Address";
            }
        }
    }
    return value;
}
function isValidGeoJSON(data) {
    if (!data || typeof data !== 'object') return false;
    if (!data.type || typeof data.type !== 'string') return false;

    if (data.type === 'Feature') {
        if (!data.geometry || typeof data.geometry !== 'object') return false;
        if (!data.properties || typeof data.properties !== 'object') return false;
        if (!isValidGeoJSONGeometry(data.geometry)) return false;
    } else if (data.type === 'FeatureCollection') {
        if (!Array.isArray(data.features)) return false;
        for (let feature of data.features) {
            if (!isValidGeoJSON(feature)) return false;
        }
    } else {
        if (!isValidGeoJSONGeometry(data)) return false;
    }

    return true;
}
function isValidGeoJSONGeometry(geometry) {
    if (!geometry.type || typeof geometry.type !== 'string') return false;
    if (!Array.isArray(geometry.coordinates)) return false;

    const validTypes = ['Point', 'MultiPoint', 'LineString', 'MultiLineString', 'Polygon', 'MultiPolygon', 'GeometryCollection'];
    if (!validTypes.includes(geometry.type)) return false;

    if (geometry.type === 'GeometryCollection') {
        if (!Array.isArray(geometry.geometries)) return false;
        for (let geom of geometry.geometries) {
            if (!isValidGeoJSONGeometry(geom)) return false;
        }
    }

    return true;
}
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
function formatNumberWithCommas(number) {
    const isNegative = number < 0;
    const absoluteNumber = Math.abs(number);
    const formattedNumber = absoluteNumber.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return isNegative ? `-${formattedNumber}` : formattedNumber;
}