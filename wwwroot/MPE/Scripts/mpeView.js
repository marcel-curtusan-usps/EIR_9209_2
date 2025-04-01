let DateTime = luxon.DateTime;
let appData = {};
let siteInfo = {};
let ianaTimeZone = '';
let CurrentTripMin = 0;
let MPEName = '';
let timer;
const MPETabel = 'mpeStatustable';

$.urlParam = function(name) {
  let results = new RegExp('[?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
  return results !== null ? results[1] || 0 : '';
};
$(function() {
  MPEName = $.urlParam('mpeStatus');
  document.title = MPEName;
  RequestDate = getUrlParameter('Date');
  $(document).prop('title', ' MPE View' + ' (' + MPEName + ')');
  setHeight();
});

async function mpeViewStart() {
  try {
    await fetch('../api/ApplicationConfiguration/Configuration')
      .then(response => response.json())
      .then(data => {
        appData = data;
        if (appData != null) {
          siteInfo = appData;
          siteInfo = data;
          // Use the mapping function to get the correct IANA time zone
          ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
          localdateTime = luxon.DateTime.local().setZone(ianaTimeZone).setZone('system', { keepLocalTime: true });
        }
      })
      .then(async () => {
        init_signalRConnection(appData).then(async () => {
          connection.on('updateMPEzoneRunPerformance', async data => {
            if (data.mpeId === MPEName) {
              Promise.all([buildDataTable(data)]);
            }
          });
          await addGroupToList('MPE');
        });
      })
      .then(() => {
        initializeMpeView();
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (err) {
    console.log('Connection failed: ', err);
  }
}
function createMPEDataTable(table) {
  let arrayColums = [
    {
      order: '',
      Name: '',
      Planned: '',
      Actual: ''
    }
  ];
  let columns = [];
  let tempc = {};
  $.each(arrayColums[0], function(key) {
    tempc = {};
    if (/Planned/i.test(key)) {
      tempc = {
        title: 'Planned',
        mDataProp: key,
        class: 'col-planned text-center'
      };
    } else if (/Actual/i.test(key)) {
      tempc = {
        title: 'Actual',
        mDataProp: key,
        class: 'col-actual text-center'
      };
    } else if (/Name/i.test(key)) {
      tempc = {
        title: '',
        mDataProp: key,
        class: 'col-name text-right'
      };
    } else {
      tempc = {
        title: capitalize_Words(key.replace(/\_/, ' ')),
        mDataProp: key
      };
    }
    columns.push(tempc);
  });
  $('#' + table).DataTable({
    fnInitComplete: function() {
      if ($(this).find('tbody tr').length <= 1) {
        $('.odd').hide();
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
    sorting: [[0, 'asc']],
    columnDefs: [
      {
        visible: false,
        targets: 0
      }
    ],
    //rowCallback: function (row, data, index) {
    rowCallback: function(row) {
      $(row).find('td').css('font-size', 'calc(0.1em + 2.6vw)');
    }
  });
}
async function initializeMpeView() {
  try {
    // Start the connection
    createMPEDataTable(MPETabel);

    await fetch(`../api/MPE/MPEPerformanceData?name=${MPEName}`)
      .then(response => response.json())
      .then(mpeData => {
        if (mpeData) {
          $('label[id=mpeName]').text(mpeData.mpeId);
          $('label[id=opn]').html('&nbsp;' + mpeData.curOperationId);
          $('label[id=sortplan_name_text]').text(mpeData.curSortplan);
          $('label[id=mpe_status]').text(MPEStatus(mpeData));
          Promise.all([buildDataTable(mpeData)]);
        }
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (err) {
    console.log('Connection failed: ', err);
  }
}
async function buildDataTable(data) {
  let dataArray = [];
  $.each(data, function(key) {
    let tabledataObject = {};
    if (/curOperationId/i.test(key)) {
      tabledataObject = {
        order: 3,
        Name: 'Throughput',
        Planned: data.rpgExpectedThruput,
        Actual: data.curThruputOphr
      };
      dataArray.push(tabledataObject);
    }
    if (/totSortplanVol/i.test(key)) {
      tabledataObject = {
        order: 2,
        Name: 'Volume',
        Planned: data.rpgEstVol,
        Actual: data.totSortplanVol
      };
      dataArray.push(tabledataObject);
    }
    if (/arsRecrej3/i.test(key)) {
      tabledataObject = {
        order: 4,
        Name: 'Reject Rate',
        Planned: 0,
        Actual: data.arsRecrej3
      };
      dataArray.push(tabledataObject);
    }
    if (/rpgEndDtm/i.test(key)) {
      tabledataObject = {
        order: 5,
        Name: 'End Time',
        Planned: VaildateEstComplete(data.rpgEndDtm, 'Plan'),
        Actual: VaildateEstComplete(data.rpgEstimatedCompletion, 'Actual')
      };
      if (CurrentTripMin === 0) {
        CountTimer = 0;
        dataArray.push(tabledataObject);
      }
    }
    if (/curSortplan/i.test(key)) {
      tabledataObject = {
        order: 1,
        Name: 'Sort Program',
        Planned: data.curSortplan,
        Actual: data.curSortplan
      };
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
  updateMpeDataTable(dataArray, 'mpeStatustable');
  clearInterval(timer);
  startCountdown(data.rpgEstimatedCompletion, data.nextOperationId, data.nextRPGStartDtm);
}
function startCountdown(targetTime, nextOP, nextStartTime) {
  let targetDate = luxon.DateTime.fromISO(targetTime, { zone: ianaTimeZone });
  let nextDate = luxon.DateTime.fromISO(nextStartTime, { zone: ianaTimeZone });
  if (nextOP && nextOP != '0') {
    $('label[id=nextopnText]').css('display', 'block');
    $('label[id=nextstartText]').css('display', 'block');
    $('label[id=nextopn]').html('&nbsp;' + nextOP);
    $('label[id=nextstart]').html('&nbsp;' + nextDate.toFormat('HH:mm MM/dd'));
  } else {
    $('label[id=nextopnText]').css('display', 'none');
    $('label[id=nextstartText]').css('display', 'none');
    $('label[id=nextopn]').html('');
    $('label[id=nextstart]').html('');
  }

  // Update the countdown every second
  timer = setInterval(() => {
    // Get the current date and time in milliseconds
    let now = luxon.DateTime.local().setZone(ianaTimeZone);

    // Calculate the distance between now and the target date
    let distance = targetDate - now;

    // Time calculations for days, hours, minutes and seconds
    //const days = Math.floor(distance / (1000 * 60 * 60 * 24));
    const hours = Math.floor(distance % (1000 * 60 * 60 * 24) / (1000 * 60 * 60));
    const minutes = Math.floor(distance % (1000 * 60 * 60) / (1000 * 60));
    const seconds = Math.floor(distance % (1000 * 60) / 1000);

    // Display the countdown in an element
    $('label[id=countdownText]').css('display', 'block');
    $('label[id=countdown]').html('&nbsp;' + hours + 'h ' + minutes + 'm ' + seconds + 's ');
    if (!nextOP || nextOP == '0') {
      $('label[id=countdown]').css('color', 'green');
    } else {
      if (nextDate > targetDate) {
        $('label[id=countdown]').css('color', 'green');
      } else {
        $('label[id=countdown]').css('color', 'red');
      }
    }

    // Clear the interval when the countdown reaches 0
    if (distance < 0) {
      clearInterval(timer);
      $('label[id=countdown]').html('');
      $('label[id=countdownText]').css('display', 'none');
    }
  }, 1000);
}
function MPEStatus(data) {
  if (/(^0$|^$)/i.test(data.curSortplan)) {
    return 'Idle';
  } else {
    return 'Running';
  }
}
function capitalize_Words(str) {
  return str.replace(/\w\S*/g, function(txt) {
    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
  });
}
function setHeight() {
  let height = (this.window.innerHeight > 0 ? this.window.innerHeight : this.screen.height) - 1;
  let screenTop = (this.window.screenTop > 0 ? this.window.screenTop : 1) - 1;
  let pageBottom = height - screenTop;
  $('div.card').css('min-height', pageBottom + 'px');
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
  PST: 'America/Los_Angeles',
  PDT: 'America/Los_Angeles',
  MST: 'America/Denver',
  MDT: 'America/Denver',
  CST: 'America/Chicago',
  CDT: 'America/Chicago',
  EST: 'America/New_York',
  EDT: 'America/New_York',
  HST: 'Pacific/Honolulu',
  AKST: 'America/Anchorage',
  AKDT: 'America/Anchorage',
  AEST: 'Australia/Sydney',
  AEDT: 'Australia/Sydney',
  ACST: 'Australia/Adelaide',
  ACDT: 'Australia/Adelaide',
  AWST: 'Australia/Perth',
  JST: 'Asia/Tokyo'
};
const postaltimeZoneMapping = {
  PST1: 'PDT',
  PST2: 'PDT',
  MST1: 'MDT',
  MST2: 'MDT',
  CST1: 'CDT',
  CST2: 'CDT',
  EST1: 'EDT',
  EST2: 'EDT'
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
function VaildateEstComplete(estComplet, type) {
  try {
    let est = luxon.DateTime.fromISO(estComplet, { zone: ianaTimeZone });
    //if (est._isValid && est.year === luxon.DateTime.local().year) {
    if (est.year && est.year === luxon.DateTime.local().year) {
      //return est.toFormat("yyyy-MM-dd HH:mm:ss");
      return est.toFormat('HH:mm MM/dd');
    } else {
      if (/Plan/gi.test(type)) {
        return type + ' Not Available';
      } else if (/Actual/gi.test(type)) {
        return type + ' Not Available';
      } else {
        return 'Not Available';
      }
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
function SiteURLconstructor(winLoc) {
  let pathname = winLoc.pathname;
  let match = pathname.match(/^\/([^\/]*)/);
  let urlPath = match[1];
  if (/^(CF)/i.test(urlPath)) {
    return winLoc.origin + '/' + urlPath;
  } else {
    return winLoc.origin;
  }
}
function createMPEDataTable(table) {
  let arrayColums = [
    {
      order: '',
      Name: '',
      Planned: '',
      Actual: ''
    }
  ];
  let columns = [];
  let tempc = {};
  $.each(arrayColums[0], function(key) {
    tempc = {};
    if (/Planned/i.test(key)) {
      tempc = {
        title: 'Planned',
        mDataProp: key,
        class: 'col-planned text-center'
      };
    } else if (/Actual/i.test(key)) {
      tempc = {
        title: 'Actual',
        mDataProp: key,
        class: 'col-actual text-center'
      };
    } else if (/Name/i.test(key)) {
      tempc = {
        title: '',
        mDataProp: key,
        class: 'col-name text-right'
      };
    } else {
      tempc = {
        title: capitalize_Words(key.replace(/\_/, ' ')),
        mDataProp: key
      };
    }
    columns.push(tempc);
  });
  $('#' + table).DataTable({
    fnInitComplete: function() {
      if ($(this).find('tbody tr').length <= 1) {
        $('.odd').hide();
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
    sorting: [[0, 'asc']],
    columnDefs: [
      {
        visible: false,
        targets: 0
      }
    ],
    //rowCallback: function (row, data, index) {
    rowCallback: function(row) {
      $(row).find('td').css('font-size', 'calc(0.1em + 2.6vw)');
    }
  });
}
function loadMpeDataTable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table) && !$.isEmptyObject(data)) {
    /*if (!$.isEmptyObject(data)) {*/
    $('#' + table).DataTable().rows.add(data).draw();
    //}
  }
}
function updateMpeDataTable(ldata, table) {
  let load = true;
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows(function(idx, data, node) {
      load = false;
      if (ldata.length > 0) {
        $.each(ldata, function() {
          if (data.Name === this.Name) {
            $('#' + table).DataTable().row(node).data(this).draw().invalidate();
          }
        });
      }
    });
    if (load) {
      loadMpeDataTable(ldata, table);
    }
  }
}
mpeViewStart();
