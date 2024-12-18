let dateTime = luxon.DateTime;
let appData = {};
let siteTours = {};
let siteInfo = {};
let ianaTimeZone = "";
let tourNumber = "";
let tourHours = [];
let retryCount = 0;
let kioskId = "";
const maxRetries = 5;
const crsConnection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
    .configureLogging(signalR.LogLevel.Information)
    .configureLogging(signalR.LogLevel.Information)
    .withHubProtocol(new signalR.JsonHubProtocol())
    .build();

// Get the current date and time
function updateDateTime() {
    var currentDate = new Date();
    var formattedDate = currentDate.toLocaleString().replace(",", " | ");

    // Set the date and time in the footer
    var footerDateElement = document.getElementById("footerDate");
    footerDateElement.textContent = formattedDate;
}

// Update the date and time every second
setInterval(updateDateTime, 1000);

async function tacsTime() {

    // TACS time shows military hour, and minutes as a decimal out of 100
    // 30 minutes would be HR.50
    const hours = padZero(currentTime.getHours());
    const minutesDecimal = currentTime.getMinutes() / 60;
    const minutes = padZero(Math.round(minutesDecimal * 100));
    const currentDate = currentTime.toLocaleDateString();

    $('span[id=tacsTime]').text(currentDate + currentTime);
}

/**
* Extracts a URL parameter by name from the current window location.
* @param {string} name - The name of the URL parameter to extract.
* @returns {string} The value of the URL parameter.
*/
$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    kioskId = (results !== null) ? results[1] || 0 : "";
    return kioskId;
}

$(function () {
    document.title = $.urlParam("kioskId");
    if (!kioskId) {
        Promise.all([kioskConfig()]);
    }
    else {
        Promise.all([restEIN()]);
        $('input[type=text][name=encodedId]').on("keyup", () => {
            if (($('input[type=text][name=encodedId]').val() === '')) {
                $('input[type=text][name=encodedId]').css("border-color", "#2eb82e").removeClass('is-valid').removeClass('is-valid');
                $('span[id=error_encodedId]').text("");
                $('button[id=submitBtn]').prop('disabled', false);
            }
            else {
                $('input[type=text][name=encodedId]').css("border-color", "#D3D3D3").removeClass('is-invalid').addClass('is-valid');
                $('span[id=error_encodedId]').text("");
                $('button[id=submitBtn]').prop('disabled', true);
            }
        });
      
        setHeight();
        $('span[id=kisokIdSpan]').text(kioskId);
        $('button[id=encodedIdBtn]').off().on('click', () => {
            Promise.all([loadEIN($('input[type=text][id=encodedId]').val())]);
        });
        $('button[id=cancelBtn]').off().on('click', () => {
            Promise.all([restEIN()]);
        });

        // Check and set focus every 3 seconds
        setInterval(checkAndSetFocus, 3000);
    }
});
async function kioskConfig() {
    $('div[id=landing]').css('display', 'none');
    $('div[id=root]').css('display', 'none');
    $('div[id=kioskSelection]').css('display', 'block');
    $('select[id=kioskSelect]').empty();
    $.ajax({
        url: SiteURLconstructor(window.location) + "/api/Kiosk/KioskList",
        type: 'GET',
        success: function (kioskdata) {
            kioskdata.push('Select Kiosk');
            if (kioskdata.length > 0) {
                //sort 
                kioskdata.sort();
                $.each(kioskdata, function () {
                    $('<option/>').val(this).html(this).appendTo('select[id=kioskSelect]');
                })
            }
        },
        error: function (error) {
            console.log(error);
        },
        faulure: function (fail) {
            console.log(fail);
        },
        complete: function (complete) {
        }
    });
}
async function loadEIN(data) {
    $('div[id=landing]').css('display', 'none');
    $('div[id=kioskSelection]').css('display', 'none');
    $('div[id=root]').css('display', 'block');
};
async function restEIN() {
    $('div[id=root]').css('display', 'none');
    $('div[id=kioskSelection]').css('display', 'none');
    $('div[id=landing]').css('display', 'block');
    $('input[id=encodedId]').val('').trigger('keyup').focus();
    
}
/**
 * Starts the SignalR connection with retry logic using exponential backoff and jitter.
 */
async function crsStart() {
    try {
        await crsConnection.start();
        console.log("SignalR Connected.");
        retryCount = 0; // Reset retry count on successful connection
        initializeCRSKiosk();

    } catch (err) {
        console.log("Connection failed: ", err);
        retryCount++;
        if (retryCount <= maxRetries) {
            const delay = Math.min(5000 * Math.pow(2, retryCount), 30000); // Exponential backoff with a cap
            const jitter = Math.random() * 1000; // Add jitter
            setTimeout(crsStart, delay + jitter);
        } else {
            console.error("Max retries reached. Could not connect to SignalR.");
            showConnectionStatus("Unable to connect. Please check your network.");
        }
    }
};
/**
 * Initializes the MPE hourly data by invoking various methods on the SignalR connection.
 */
async function initializeCRSKiosk() {
    try {
        // Start the connection
        //load Kiosk Zones
        await $.ajax({
            url: `${SiteURLconstructor(window.location)}/api/Kiosk/GetKiosk?id=${kioskId}`,
            contentType: 'application/json',
            type: 'GET',
            success: function (data) {
                for (let i = 0; i < data.length; i++) {
                    restEIN();
                }
            }
        })
        crsConnection.invoke("GetApplicationInfo").then(function (data) {
            appData = JSON.parse(data);
            //if data is not null
            if (appData != null) {
                siteInfo = appData;
                siteTours = JSON.parse(appData.Tours);
                // Use the mapping function to get the correct IANA time zone
                ianaTimeZone = getIANATimeZone(getPostalTimeZone(appData.TimeZoneAbbr));
            }
        }).catch(function (err) {
            console.error("Error loading application info: ", err);
        });

        crsConnection.invoke("JoinGroup", "CRS").then(function (data) {
            console.log("Connected to Group:", "CRS Zones");
        }).catch(function (err) {
            return console.error(err.toString());
        });
    } catch (err) {
        console.log("Connection failed: ", err);
    }
}
crsConnection.onclose(async () => {
    console.log("Connection closed. Attempting to reconnect...");
    showConnectionStatus("Connection lost. Attempting to reconnect...");
    await start();
});

crsConnection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
    showConnectionStatus("Reconnecting...");
});

crsConnection.onreconnected((connectionId) => {
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
function checkAndSetFocus() {
    if (!$('input[id=encodedId]').is(':focus')) {
        $('input[id=encodedId]').focus();
    }
}
crsStart();

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
    'MST2': 'MDT',
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

// Function to handle keypad button clicks
function handleKeypadClick(event) {
    const display = document.querySelector('.display');
    const buttonValue = event.target.textContent;

    if (buttonValue === '←') {
        // Handle backspace
        display.textContent = display.textContent.slice(0, -1);
    } else {
        // Append the button value to the display if it's not backspace and length is less than 3
        if (display.textContent.length < 3) {
            display.textContent += buttonValue;
        }
    }
}

// Add event listeners to keypad buttons
document.querySelectorAll('#keypad button').forEach(button => {
    button.addEventListener('click', handleKeypadClick);
});

// Function to handle top code button clicks
function handleTopCodeClick(event) {
    const display = document.querySelector('.display');
    const buttonValue = event.target.textContent;

    // Set the display to the button value
    display.textContent = buttonValue;
}

// Add event listeners to top code buttons
document.querySelectorAll('.topCodeButton').forEach(button => {
    button.addEventListener('click', handleTopCodeClick);
});