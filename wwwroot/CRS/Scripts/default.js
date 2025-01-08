let dateTime = luxon.DateTime;
let appData = {};
let siteTours = {};
let siteInfo = {};
let ianaTimeZone = "";
let tourNumber = "";
let tourHours = [];
let retryCount = 0;
let kioskId = "";
var suffixKeyCodes = [9, 13];
var prefixKeyCodes = [];
let errorTimeout;
let currentUser = {};
const errorTimeLimit = 2000; // 15 seconds
let inactivityTimeout;
const inactivityTimeLimit = 1500000; // should be 1500 seconds
const modalTimeout = 1000; // 1 second
const tacsDataTable = "tacsdatatable";
const maxRetries = 5;
let TranCode = "";
let isKioskInUse = false;
const events = ["mousemove", "keydown", "scroll", "click", "touchstart"];
const crsConnection = new signalR.HubConnectionBuilder()
  .withUrl(SiteURLconstructor(window.location) + "/hubServics")
  .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
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
async function tacsTime() {
  const currentTime = dateTime.local(); // Get the current local time using luxon

  // TACS time shows military hour, and minutes as a decimal out of 100
  // 30 minutes would be HR.50
  const hours = currentTime.hour.toString().padStart(2, "0");
  const minutesDecimal = currentTime.minute / 60;
  const minutes = Math.round(minutesDecimal * 100)
    .toString()
    .padStart(2, "0");
  const currentDate = currentTime.toLocaleString(dateTime.DATE_SHORT);

  $("span[id=tacsTime]").text(currentDate + " " + hours + "." + minutes);
}
// Update the date and time every second
setInterval(updateDateTime, 1000);
setInterval(tacsTime, 30000);
function resetInactivityTimer() {
  clearTimeout(inactivityTimeout);
  inactivityTimeout = setTimeout(async () => {
    await restEIN();
  }, inactivityTimeLimit);
}
function setupInactivityListener() {
  events.forEach((event) => {
    document.addEventListener(event, resetInactivityTimer, false);
  });
}
function removeInactivityListener() {
  events.forEach((event) => {
    document.removeEventListener(event, resetInactivityTimer, false);
  });
  clearTimeout(inactivityTimeout);
}
// Function to focus on the barcodeScan input
function focusBarcodeScan() {
  document.getElementById("barcodeScan").focus();
}

// Add event listeners for each event to trigger the focus
function addFocusEventListeners() {
  events.forEach((event) => {
    document.addEventListener(event, focusBarcodeScan);
  });
}

// Function to remove event listeners for focusing on the barcodeScan input
function removeFocusEventListeners() {
  events.forEach((event) => {
    document.removeEventListener(event, focusBarcodeScan);
  });
}
/**
 * Extracts a URL parameter by name from the current window location.
 * @param {string} name - The name of the URL parameter to extract.
 * @returns {string} The value of the URL parameter.
 */
$.urlParam = function (name) {
  let results = new RegExp("[?&]" + name + "=([^&#]*)", "i").exec(
    window.location.search
  );
  kioskId = results !== null ? results[1] || 0 : "";
  return kioskId;
};

$(function () {
  kioskId = $.urlParam("kioskId");
  document.title = kioskId;
  tacsTime();
  createTacsDatatable(tacsDataTable);

  if (!kioskId) {
    Promise.all([kioskConfig()]);
  } else {
    focusBarcodeScan();
    Promise.all([restEIN()]);
    $("input[type=text][name=barcodeScan]").on("keyup", () => {
      if ($("input[name=barcodeScan]").val() === "") {
        $("input[name=barcodeScan]")
          .css("border-color", "#2eb82e")
          .removeClass("is-valid")
          .removeClass("is-valid");
        $("span[id=errorBarcodeScan]").text("");
        $("button[id=barcodeScanBtn]").prop("disabled", false);
      } else {
        $("input[id=barcodeScan]")
          .css("border-color", "#D3D3D3")
          .removeClass("is-invalid")
          .addClass("is-valid");
        $("span[id=errorBarcodeScan]").text("");
        $("button[id=barcodeScanBtn]").prop("disabled", true);
      }
    });

    $("#barcodeScan").on("keydown", function (event) {
      if (event.keyCode === 13) {
        // Check if Enter key is pressed
        $("#barcodeScanBtn").click(); // Trigger click event on barcodeScanBtn
      }
    });

    setHeight();
    $("span[id=kisokIdSpan]").text(kioskId);
    $('button[id=barcodeScanBtn]').off().on('click', async () => {
        let scanValue = $('input[id=barcodeScan]').val()
        if (scanValue !== "") {
           // Trim to 9 characters from the end if payLoadData starts with 4 zeros
            if (scanValue.startsWith("0000")) {
                scanValue = scanValue.slice(-9);
            }
            $('span[id=errorBarcodeScan]').text("");
            await loadEIN(scanValue);
        }
        else {
            $('span[id=errorBarcodeScan]').text("Please Scan USPS Badge");
            clearTimeout(errorTimeout);
            errorTimeout = setTimeout(async () => {
                await restEIN();
            }, errorTimeLimit);
        }
    });
    $("button[id=bt]")
      .off()
      .on("click", () => {
        TranCode = $("#bt").data("trancode");
        $("div[id=Keypad-display]").text(TranCode);
        $("button[id=confirmBtn]").prop("disabled", false);
      });
    $("button[id=ol]")
      .off()
      .on("click", () => {
        TranCode = $("#ol").data("trancode");
        $("div[id=Keypad-display]").text(TranCode);
        $("button[id=confirmBtn]").prop("disabled", false);
      });
    $("button[id=il]")
      .off()
      .on("click", () => {
        TranCode = $("#il").data("trancode");
        $("div[id=Keypad-display]").text(TranCode);
        $("button[id=confirmBtn]").prop("disabled", false);
      });
    $("button[id=et]")
      .off()
      .on("click", () => {
        TranCode = $("#et").data("trancode");
        $("div[id=Keypad-display]").text(TranCode);
        $("button[id=confirmBtn]").prop("disabled", false);
      });
    $("button[id=mvBtn]")
      .off()
      .on("click", () => {
        TranCode = $("#il").data("trancode");
        $("div[id=Keypad-display]").text("");
        $("button[id=bt]").prop("disabled", true);
        $("button[id=ol]").prop("disabled", true);
        $("button[id=il]").prop("disabled", true);
        $("button[id=et]").prop("disabled", true);
        $("button[id=keypadButton]").prop("disabled", false);
      });
    $("button[id=cancelBtn]")
      .off()
      .on("click", async () => {
        await restEIN();
      });

    $("button[id=confirmBtn]")
      .off()
      .on("click", async () => {
        try {
          await confirmBtnSubmit();
          $("span[id=crsEvent]").text(TranCode);
          $("#confirmModal").modal("show");
          setTimeout(async function () {
            $("#confirmModal").modal("hide");
            await restEIN();
          }, modalTimeLimit); // Adjusted the timeout to match the modal
        } catch (error) {
          console.error("Error during confirm button submission:", error);
        }
      });

    // Initialize
    onScan.attachTo(document, {
      timeBeforeScanTest: 100,
      avgTimeByChar: 30,
      minLength: 8,
      suffixKeyCodes: suffixKeyCodes,
      prefixKeyCodes: prefixKeyCodes,
      scanButtonLongPressTime: 500,
      stopPropagation: false,
      preventDefault: false,
      reactToKeyDown: true,
      singleScanQty: 1,
      reactToPaste: true, // Compatibility to built-in scanners in paste-mode (as opposed to keyboard-mode)
      onScan: async function (sCode, iQty) {
        console.log("Scanned: " + iQty + "x " + sCode);
        if (sCode !== "") {
          let scanValue = sCode;
          // Trim to 9 characters from the end if payLoadData starts with 4 zeros
          if (scanValue.startsWith("0000")) {
            scanValue = scanValue.slice(-9);
          }
          $("span[id=errorBarcodeScan]").text("");
          await loadEIN(scanValue);
        } else {
          $("span[id=errorBarcodeScan]").text("Invalid Scan");
          clearTimeout(errorTimeout);
          errorTimeout = setTimeout(async () => {
            await restEIN();
          }, errorTimeLimit);
        }
      },
      onPaste: async function (isPasteString) {
        console.log("Paste: " + isPasteString);
        if (isPasteString !== "") {
          $("span[id=errorBarcodeScan]").text("");
          await loadEIN(isPasteString);
        } else {
          $("span[id=errorBarcodeScan]").text("Invalid Scan");
          clearTimeout(errorTimeout);
          errorTimeout = setTimeout(async () => {
            await restEIN();
          }, errorTimeLimit);
        }
      },
      onKeyDetect: async function (iKey, keyDetect) {
        if (iKey === 13) {
          console.log("Paste: " + keyDetect.key);
          if (keyDetect.key !== "") {
            $("span[id=errorBarcodeScan]").text("");
            let scanValue = $("input[id=barcodeScan]").val();
            await loadEIN(scanValue);
          } else {
            $("span[id=errorBarcodeScan]").text("Invalid Scan");
            clearTimeout(errorTimeout);
            errorTimeout = setTimeout(async () => {
              await restEIN();
            }, errorTimeLimit);
          }
        }
      },
    });
  }
});
async function confirmBtnSubmit() {
  // {
  //"empInfo": {
  //"empId": "02953255",
  //"empIdType": "EIN"
  //},
  //"tranInfo":{
  //"tranCode": "010",
  //"ringReasonCode": "00",
  //"tranDate": "2020-07-30",
  //"tranTime": "07.00",
  //"timeZoneCode": "UTC",
  //"utcOffset": "-06:00",
  //"dstObserved": true,
  //"ringTypeCode": "000",
  //"financeNo": "353340"
  //},
  //"ringInfo":{
  //"financeNoUnitId": null,
  //"rsc": null,
  //"rscSuffix": null,
  //"operationId": null,
  //"localUnitNo": null,
  //"routeNo": null,
  //"activityDurationQty": null,
  //"scheduledInd": null,
  //"positionLevelNo": null,
  //"facilityId": null,
  //"vehicleId": null
  //},
  //"deviceInfo": {
  //"deviceId": "MDD-12345",
  //"deviceType": "MDD",
  //"latitude": "44.818870",
  //"longitude": "-93.167220"
  //}
  //}
  let RawRingObject = {
    empInfo: {
      empId: currentUser.employeeId,
      empIdType: "EIN",
    },
    tranInfo: {
      tranCode: TranCode,
      ringReasonCode: "00",
      tranDate: "",
      tranTime: "",
      timeZoneCode: "",
      utcOffset: "",
      dstObserved: "",
      ringTypeCode: "000",
      financeNo: "",
    },
    ringInfo: {
      OperationId:
        TranCode === "011"
          ? $("div[id=Keypad-display]").text()
          : currentUser.baseOp,
    },
    deviceInfo: {
      deviceId: kioskId,
      deviceType: "CRS",
      latitude: "",
      longitude: "",
    },
  };

  //create tacs event
  fetch(`../api/ClockRingStation/AddRawRings`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json-patch+json",
    },
    body: JSON.stringify(RawRingObject),
  })
    .then((response) => {
      if (!response.ok) {
        errorTimeout = setTimeout(function () {
          Promise.all([restEIN()]);
        }, errorTimeLimit);
        throw new Error(`HTTP error! status: ${response.status}`);
      } else if (response.ok && response.status != 200) {
        $("span[id=errorBarcodeScan]").text(
          `Employee ${payLoadData} Not Found`
        );
        clearTimeout(errorTimeout);
        errorTimeout = setTimeout(() => {
          Promise.all([restEIN()]);
        }, errorTimeLimit);
        throw new Error();
      } else {
        return response.json();
      }
    })
    .then(async (data) => {})
    .catch((error) => {
      console.error("Error:", error);
    });
}
async function kioskConfig() {
  $("div[id=landing]").css("display", "none");
  $("div[id=root]").css("display", "none");
  $("div[id=kioskSelection]").css("display", "block");
  $("select[id=kioskSelect]").empty();
  $.ajax({
    url: SiteURLconstructor(window.location) + "/api/Kiosk/KioskList",
    type: "GET",
    success: function (kioskdata) {
      kioskdata.push("Select Kiosk");
      if (kioskdata.length > 0) {
        //sort
        kioskdata.sort();
        $.each(kioskdata, function () {
          $("<option/>")
            .val(this)
            .html(this)
            .appendTo("select[id=kioskSelect]");
        });
      }
    },
    error: function (error) {
      console.log(error);
    },
    faulure: function (fail) {
      console.log(fail);
    },
    complete: function (complete) {},
  });
}
async function loadEIN(payLoadData) {
  let payLoad = payLoadData;
  $("input[id=barcodeScan]").val("");
  await fetch(`../api/ClockRingStation/GetByEIN?code=${payLoad}`)
    .then((response) => {
      if (!response.ok) {
        $("span[id=errorBarcodeScan]").text(
          `Employee ${payLoadData} Not Found`
        );
        clearTimeout(errorTimeout);
        errorTimeout = setTimeout(async () => {
          await restEIN();
        }, errorTimeLimit);
        throw new Error();
      } else {
        return response.json();
      }
    })
    .then(async (data) => {
      // If load is successful, remove the focus event listeners
      removeFocusEventListeners();
      currentUser = data.employee;
      clearTimeout(errorTimeout);
      $("div[id=landing]").css("display", "none");
      $("div[id=kioskSelection]").css("display", "none");
      $("div[id=root]").css("display", "block");
      $("p[id=profId]").text(await formatProfId(data.employee));

      if (data.hasOwnProperty("topOpnCodes") && data.topOpnCodes.length > 0) {
        await loadTopCodes(data.topOpnCodes);
      }
      if (data.hasOwnProperty("rawRings") && data.rawRings.length > 0) {
        await loadRawRingsLogs(data.rawRings);
      }
      // Start the inactivity listener
      setupInactivityListener();
      resetInactivityTimer();
      isKioskInUse = true;
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}
async function loadTopCodes(codesList) {
  try {
    const topCodeListDiv = $("div[id=topCodeList]");
    topCodeListDiv.empty(); // Clear any existing content
    
    // Limit the displayed codes to 6
    const limitedCodes = codesList.slice(0, 6);
    
    limitedCodes.forEach((code) => {
      const codeElement = $("<div>")
        .addClass("col-2")
        .append(
          $("<button>")
            .addClass("topCodeButton btn btn-light")
            .text(code)
            .attr("data-tranCode", "011")
        );
      topCodeListDiv.append(codeElement);
    });
    
    document.querySelectorAll(".topCodeButton").forEach((button) => {
      button.addEventListener("click", handleTopCodeClick);
    });
  } catch (e) {
    console.error("Error:", e);
  }
}
async function loadRawRingsLogs(rawRingList) {
    try {
        // Create an array to hold the new data objects
        let newData = rawRingList.map(rawRing => {
            return {
                id: rawRing.sourceTranId,
                tranDateTime: `${rawRing.tranInfo.tranDate} ${rawRing.tranInfo.tranTime}`,
                tranTime: rawRing.tranInfo.tranTime,
                tranCode: rawRing.tranInfo.tranCode,
                operationId: rawRing.ringInfo.operationId
            };
        });

        // Pass the new data to updateRawRingsDataTable
        await updateRawRingsDataTable(newData, tacsDataTable);
    } catch (e) {
        console.error("Error:", e);
    }
}
async function loadRawRingsDataTable(data, table) {
  if ($.fn.dataTable.isDataTable("#" + table)) {
    $("#" + table)
      .DataTable()
      .rows.add(data)
      .draw();
  }
}
async function updateRawRingsDataTable(newData, table) {
    try {
        return new Promise((resolve, reject) => {
            if ($.fn.dataTable.isDataTable("#" + table)) {
                // Clear the existing data before adding new data
                $("#" + table).DataTable().clear().draw();

                // Add the new data
                $("#" + table).DataTable().rows.add(newData).draw();
            } else {
                // If the table is not initialized, initialize it with the new data
                loadRawRingsDataTable(newData, table);
            }
            resolve();
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}

function constructTacsColumns() {

    let columns = [];
    var column0 =
    {
        //first column is always the name
        title: "OPN Code",
        data: "operationId",
        width: '20%'
    };
    var column1 =
    {
        //first column is always the name
        title: "Event",
        data: "tranCode",
        width: '15%',
        mRender: function (data, type, full) {
            if (data === "010") {
                return `BT (${data})`;
            } else if (data === "012") {
                return `OL (${data})`;
            } else if (data === "013") {
                return `IL (${data})`;
            } else if (data === "014") {
                return `ET (${data})`;
            } else if (data === "011") {
                return `MV (${data})`;
            } else {
                return data;
            }
        }
    };
    var column2 =
    {
        //first column is always the name
        title: "Date & Time",
        data: "tranDateTime",
        width: '30%'
    };
    var column3 =
    {
        //first column is always the name
        title: "Duration (hours)",
        data: null,
        width: '30%'
    };
    columns[0] = column0;
    columns[1] = column1;
    columns[2] = column2;
    columns[3] = column3;

  return columns;
}
function createTacsDatatable(table) {
    try {
        $('#' + table).DataTable({
            dom: 'Bfrtip',
            bFilter: false,
            bdeferRender: true,
            bpaging: false,
            bPaginate: false,
            autoWidth: false,
            bInfo: false,
            ordering: false, // Disable sorting for all columns
            destroy: true,
            scroller: true,
            language: {
                zeroRecords: "No Data",
                emptyTable: "No TACS Log"
            },
            order: [[]],
            aoColumns: constructTacsColumns(),
            rowCallback: function (row, data, index) {
              const tableApi = this.api();
              const totalRows = tableApi.data().count();

              // Retrieve the text from the second column (index 1)
              const secondColumnText = $('td:eq(1)', row).text().trim().toUpperCase();

              // Check if the second column contains "BT (010)"
              if (secondColumnText === "BT (010)") {
                  // Leave the duration column empty
                  $('td:eq(3)', row).html('');
              } else if (index < totalRows - 1) {
                  // Get the next row's data (since data is reversed)
                  const nextRowData = tableApi.row(index + 1).data();

                  if (nextRowData) {
                      // Parse tranTime values
                      const nextTranTime = parseFloat(nextRowData.tranTime);
                      const currentTranTime = parseFloat(data.tranTime);

                      // Calculate duration: currentTranTime - nextTranTime
                      const duration = currentTranTime - nextTranTime;

                      // Check if duration is a valid number
                      if (!isNaN(duration)) {
                          // Display the duration with two decimal places
                          $('td:eq(3)', row).html(duration.toFixed(2));
                      } else {
                          // If duration is not a number, leave it blank
                          $('td:eq(3)', row).html('');
                      }
                  } else {
                      // If next row data is not available, leave duration blank
                      $('td:eq(3)', row).html('');
                  }
              } else {
                  // For the last row, no next row exists, leave duration blank
                  $('td:eq(3)', row).html('');
              }
          }
        });
    } catch (e) {
        console.log("Error fetching machine info: ", e);
    }
}


//rule: display the profId in the format of first 3 letters of last name and the last 4 digits of the EIN
async function formatProfId(data) {
  //data.EmployeeId
  //data.LastName
  return (
    data.lastName.substring(0, 2).toUpperCase() +
    data.employeeId.substring(data.employeeId.length - 4)
  );
}
async function restEIN() {
  // Remove the inactivity listener
  isKioskInUse = false;
  removeInactivityListener();
  currentUser = {};
  $("button[id=confirmBtn]").prop("disabled", true);
  $("button[id=clockButtons]").prop("disabled", true);
  $("button[id=keypadButton]").prop("disabled", true);
  $("button[id=confirmBtn]").prop("disabled", true);
  $("div[id=root]").css("display", "none");
  $("div[id=kioskSelection]").css("display", "none");
  $("div[id=landing]").css("display", "block");
  $("span[id=errorBarcodeScan]").text("");
  $("input[id=barcodeScan]").val("").trigger("keyup").focus();
  $("div[id=Keypad-display]").text("");
  $("button[id=bt]").prop("disabled", false);
  $("button[id=ol]").prop("disabled", false);
  $("button[id=il]").prop("disabled", false);
  $("button[id=et]").prop("disabled", false);
  $("button[id=mvBtn]").prop("disabled", false);
  $("div[id=topCodeList]").empty();
  if ($.fn.dataTable.isDataTable("#" + tacsDataTable)) {
    // Check if DataTable has been previously created and therefore needs to be flushed
    // For new version use table.destroy();
    $("#" + tacsDataTable)
      .DataTable()
      .clear()
      .draw(); // Empty the DOM element which contained DataTable
    // The line above is needed if number of columns change in the Data
  }
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
}
/**
 * Initializes the MPE hourly data by invoking various methods on the SignalR connection.
 */
async function initializeCRSKiosk() {
  try {
    // Start the connection
    //load Kiosk Zones
    await $.ajax({
      url: `${SiteURLconstructor(
        window.location
      )}/api/Kiosk/GetKiosk?id=${kioskId}`,
      contentType: "application/json",
      type: "GET",
      success: function (data) {
        for (let i = 0; i < data.length; i++) {
          restEIN();
        }
      },
    });
    crsConnection
      .invoke("GetApplicationInfo")
      .then(function (data) {
        appData = JSON.parse(data);
        //if data is not null
        if (appData != null) {
          siteInfo = appData;
          siteTours = JSON.parse(appData.Tours);
          // Use the mapping function to get the correct IANA time zone
          ianaTimeZone = getIANATimeZone(
            getPostalTimeZone(appData.TimeZoneAbbr)
          );
        }
      })
      .catch(function (err) {
        console.error("Error loading application info: ", err);
      });

    crsConnection
      .invoke("JoinGroup", "CRS")
      .then(function (data) {
        console.log("Connected to Group:", "CRS Zones");
      })
      .catch(function (err) {
        return console.error(err.toString());
      });
  } catch (err) {
    console.log("Connection failed: ", err);
  }
}
crsConnection.on("epacScan", async (scanDate) => {
  await handelIncomingScan(scanDate);
});
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

async function handelIncomingScan(incomingScan) {
  try {
    if (
      isKioskInUse === false &&
      incomingScan.hasOwnProperty("kioskId") &&
      incomingScan.kioskId === kioskId
    ) {
      if (incomingScan.hasOwnProperty("id")) {
        await loadEIN(incomingScan.id);
      }
    }
  } catch (error) {
    console.log("error: ", error);
  }
}
/**
 * Displays the connection status message to the user.
 * @param {string} message - The message to display.
 */
function showConnectionStatus(message) {
  const statusElement = document.getElementById("connection-status");
  if (statusElement) {
    statusElement.textContent = message;
    statusElement.style.display = "block";
  }
}
function checkAndSetFocus() {
  if (!$("input[id=encodedId]").is(":focus")) {
    $("input[id=encodedId]").focus();
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
  let height =
    (this.window.innerHeight > 0
      ? this.window.innerHeight
      : this.screen.height) - 1;
  let screenTop = (this.window.screenTop > 0 ? this.window.screenTop : 1) - 1;
  let pageBottom = height - screenTop;
  $("div.card").css("min-height", pageBottom + "px");
}

// Mapping of standard time zone abbreviations to IANA time zones
const timeZoneMapping = {
  PST: "America/Los_Angeles",
  PDT: "America/Los_Angeles",
  MST: "America/Denver",
  MDT: "America/Denver",
  CST: "America/Chicago",
  CDT: "America/Chicago",
  EST: "America/New_York",
  EDT: "America/New_York",
  HST: "Pacific/Honolulu",
  AKST: "America/Anchorage",
  AKDT: "America/Anchorage",
  AEST: "Australia/Sydney",
  AEDT: "Australia/Sydney",
  ACST: "Australia/Adelaide",
  ACDT: "Australia/Adelaide",
  AWST: "Australia/Perth",
  JST: "Asia/Tokyo",
};
const postaltimeZoneMapping = {
  PST1: "PDT",
  PST2: "PDT",
  MST1: "MDT",
  MST2: "MDT",
  CST1: "CDT",
  CST2: "CDT",
  EST1: "EDT",
  EST2: "EDT",
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
    sURLVariables = sPageURL.split("&"),
    sParameterName,
    i;

  for (i = 0; i < sURLVariables.length; i++) {
    sParameterName = sURLVariables[i].split("=");

    if (sParameterName[0] === sParam) {
      return typeof sParameterName[1] === undefined
        ? true
        : decodeURIComponent(sParameterName[1]);
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
  } else {
    return winLoc.origin;
  }
}

// Function to handle keypad button clicks
function handleKeypadClick(event) {
  const KeypadDisplay = document.querySelector(".Keypad-display");
  const buttonValue = event.target.textContent;
  TranCode = event.target.dataset.trancode;
  if (buttonValue === "←") {
    // Handle backspace
    KeypadDisplay.textContent = KeypadDisplay.textContent.slice(0, -1);
    if (KeypadDisplay.textContent.length <= 0) {
      $("button[id=bt]").prop("disabled", false);
      $("button[id=ol]").prop("disabled", false);
      $("button[id=il]").prop("disabled", false);
      $("button[id=et]").prop("disabled", false);
      $("button[id=mvBtn]").prop("disabled", false);
      $("button[id=confirmBtn]").prop("disabled", true);
    }
  } else {
    // Append the button value to the display if it's not backspace and length is less than 3
    if (KeypadDisplay.textContent.length < 3) {
      KeypadDisplay.textContent += buttonValue;
      if (KeypadDisplay.textContent.length === 3) {
        $("button[id=bt]").prop("disabled", true);
        $("button[id=ol]").prop("disabled", true);
        $("button[id=il]").prop("disabled", true);
        $("button[id=et]").prop("disabled", true);
        $("button[id=mvBtn]").prop("disabled", true);
        $("button[id=confirmBtn]").prop("disabled", false);
      }
    }
  }
}

// Add event listeners to keypad buttons
document.querySelectorAll("#keypad button").forEach((button) => {
  button.addEventListener("click", handleKeypadClick);
});

// Function to handle top code button clicks
function handleTopCodeClick(event) {
  const buttonValue = event.target.textContent;
  TranCode = event.target.dataset.trancode;
  $("div[id=Keypad-display]").text("");
  $("button[id=bt]").prop("disabled", true);
  $("button[id=ol]").prop("disabled", true);
  $("button[id=il]").prop("disabled", true);
  $("button[id=et]").prop("disabled", true);
  $("button[id=mvBtn]").prop("disabled", true);
  $("button[id=confirmBtn]").prop("disabled", false);
  // Set the display to the button value
  $("div[id=Keypad-display]").text(buttonValue);
}

// Function to handle modal timeout
function startModalTimeout() {
  modalTimeLimit = setTimeout(async () => {
    $("#confirmModal").modal("hide");
    await restEIN();
  }, modalTimeout);
}

// Event listener for modal show event
$("#confirmModal").on("shown.bs.modal", function () {
  startModalTimeout();
});

// Event listener for modal close button
$("#modalClose").on("click", function () {
  clearTimeout(modalTimeout);
  restEIN();
});
