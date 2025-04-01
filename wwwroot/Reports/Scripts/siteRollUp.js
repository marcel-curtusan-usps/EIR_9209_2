/// A simple template method for replacing placeholders enclosed in curly braces.
let appData = {};
let siteInfo = {};
let siteTours = {};
let connecttimer = 0;
let connectattp = 0;
let MPEName = '';
let RequestDate = '';
let DateTimeNow = new Date();
let CurrentTripMin = 0;
let CountTimer = 0;
let TimerID = -1;
let Timerinterval = 1;
let timezone = {};
let localdateTime = null;
let defaultMaxdate = null;
let defaultMindate = null;
$.urlParam = function(name) {
  let results = new RegExp('[?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
  return results !== null ? results[1] || 0 : '';
};
// $.extend(fotfmanager.client, {
//   updateSiteStatus: updatedata => {
//     Promise.all([updateSitePerformanceSummaryStatus(updatedata, RequestDate, '', '')]);
//   },
//   initSiteStatus: async initdata => {
//     Promise.all([init_siteStatus(initdata)]);
//   },
//   siteInfo: async data => {
//     if (data.hasOwnProperty('timeZoneName')) {
//       timezone = data.timeZoneName;
//       localdateTime = luxon.DateTime.local().setZone(timezone);

//       defaultMaxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
//       defaultMindate = defaultMaxdate.minus({ hours: 24 }).startOf('hour');
//       Promise.all([LoadData()]);
//     }
//   }
// });
$(function() {
  setHeight();
  RequestDate = $.urlParam('Date');
});
async function sitePerformanceStart() {
  try {
    await fetch('../api/ApplicationConfiguration/Configuration')
      .then(response => response.json())
      .then(data => {
        appData = data;
        if (appData != null) {
          siteInfo = appData;
          siteTours = appData.tours;

          // Use the mapping function to get the correct IANA time zone
          ianaTimeZone = getIANATimeZone(getPostalTimeZone(data.timeZoneAbbr));
          localdateTime = luxon.DateTime.local().setZone(ianaTimeZone).setZone('system', { keepLocalTime: true });
          defaultMaxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
          defaultMindate = defaultMaxdate.minus({ hours: 24 }).startOf('hour');
          setTours();
        }
      })
      .then(async () => {
        init_signalRConnection(appData).then(async () => {
          connection.on('updateSiteStatus', async data => {
            Promise.all([updateSitePerformanceSummaryStatus(data, RequestDate, '', '')]);
          });
          await addGroupToList('SitePerformance');
        });
      })
      .then(() => {
        initializeSitePerformance();
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (err) {
    console.log('Connection failed: ', err);
  }
}
async function initializeSitePerformance() {
  console.log('Connected time: ' + new Date($.now()));
  await fetch(`../api/MPESummary/MPENameDatetime?mpe=${MPEName}&startDateTime=${defaultMaxdate}&endDateTime=${defaultMindate}`)
    .then(response => {
      if (!response.ok) {
        throw new Error('Network response was not ok ' + response.statusText);
      }
      return response.json();
    })
    .then(data => {
      if (data) {
        Promise.all([updateSitePerformanceSummaryStatus(data, '', defaultMaxdate, defaultMindate)]);
      }
    })
    .catch(error => {
      console.error('Error:', error);
    });
}

async function setTours() {
  try {
    if (!!siteTours) {
      const tourlist = [];
      if (siteTours.tour1Start) {
        tourlist.push({ name: 'TOUR 1', compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + siteTours.tour1Start), tourStart: siteTours.tour1Start, tourEnd: siteTours.tour1End });
      }
      if (siteTours.tour2Start) {
        tourlist.push({ name: 'TOUR 2', compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + siteTours.tour2Start), tourStart: siteTours.tour2Start, tourEnd: siteTours.tour2End });
      }
      if (siteTours.tour3Start) {
        tourlist.push({ name: 'TOUR 3', compareDate: luxon.DateTime.fromISO(defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + siteTours.tour3Start), tourStart: siteTours.tour3Start, tourEnd: siteTours.tour3End });
      }
      tourlist.sort(compareDates);

      const tours = [];
      if (tourlist[0].compareDate < defaultMaxdate.minus({ hours: 3 })) {
        tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourEnd });
        tours.push({ name: tourlist[1].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourEnd });
        tours.push({ name: tourlist[0].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourStart, endTime: defaultMaxdate.plus({ days: 1 }).toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourEnd });
      } else if (tourlist[1].compareDate < defaultMaxdate.minus({ hours: 3 })) {
        tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourEnd });
        tours.push({ name: tourlist[1].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourEnd });
        tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourEnd });
      } else if (tourlist[2].compareDate < defaultMaxdate.minus({ hours: 3 })) {
        tours.push({ name: tourlist[2].name, startTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourEnd });
        tours.push({ name: tourlist[1].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourEnd });
        tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourEnd });
      } else {
        tours.push({ name: tourlist[2].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[2].tourEnd });
        tours.push({ name: tourlist[1].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourStart, endTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[1].tourEnd });
        tours.push({ name: tourlist[0].name, startTime: defaultMindate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourStart, endTime: defaultMaxdate.toFormat('yyyy-LL-dd') + 'T' + tourlist[0].tourEnd });
      }
      console.log(tours);
      //create the tours dropdown values
      $('#ddlTourSelect').html('');
      $('#ddlTourSelect').append($('<option/>', { value: ',', text: 'Current MODS' }));
      Object.keys(tours).forEach((key, num) => {
        let valueString = tours[key].startTime + ',' + tours[key].endTime;
        $('#ddlTourSelect').append($('<option/>', { value: valueString, text: tours[key].name }));
      });

      $('#ddlTourSelect').off().on('change', function() {
        // if ($(this).val() === ',') {
        //   fotfmanager.server.getMPEPerformanceSummaryList('').done(async data => {
        //     Promise.all([updateSitePerformanceSummaryStatus(data, '', defaultMaxdate, defaultMindate)]);
        //   });
        // } else {
        //   let startEndTime = $(this).val().split(',');
        //   fotfmanager.server.getMPEPerformanceSummaryList('').done(async data => {
        //     Promise.all([updateSitePerformanceSummaryStatus(data, RequestDate, luxon.DateTime.fromISO(startEndTime[1]), luxon.DateTime.fromISO(startEndTime[0]))]);
        //   });
        // }
      });
    }
  } catch (error) {
    console.error('Error:', error);
  }
}
// start site performance summary
sitePerformanceStart();
function capitalize_Words(str) {
  return str.replace(/\w\S*/g, function(txt) {
    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
  });
}
function setHeight() {
  let height = (this.window.innerHeight > 0 ? this.window.innerHeight : this.screen.height) - 1;
  let screenTop = (this.window.screenTop > 0 ? this.window.screenTop : 1) - 1;
  let pageBottom = height - screenTop;
  $('body').css('min-height', pageBottom + 'px');
}

function compareDates(a, b) {
  return b['compareDate'].toMillis() - a['compareDate'].toMillis();
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
