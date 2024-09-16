let connecttimer = 0;
let connectattp = 0;
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "hubServics")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

$(function () {
    setHeight();
    //makea ajax call to get the employee details
    $.ajax({
        url: SiteURLconstructor(window.location) + '/api/EmpSchedule/PayWeekList',
        type: 'GET',
        success: function (data) {

            Promise.all([LoadPayWeekList(data[0])]);

        },
        error: function (error) {
            console.log(error);
        },
        faulure: function (fail) {
            console.log(fail);
        }
    });


});
async function LoadPayWeekList(payweek) {
    try {
        //makea ajax call to get the employee details
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/EmpSchedule/EmployeesSchedule?payWeek='+ payweek,
            type: 'GET',
            success: function (data) {
                console.log(data);
                Promise.all([updateEmployeeSchedule(data)]);
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

//async function start() {
//    try {
//        //await connection.start().then(async () => {
//        //    //load siteinfo
//        //    await connection.invoke("GetSiteInformation").then(function (data) {
//        //        siteTours = data.tours;
//        //    }).catch(function (err) {
//        //        console.error(err);
//        //    });
//        //    await connection.invoke("GetMPESynopsis").then(async (data) => {
//        //        hourlyMPEdata = data.length > 0 ? data[0] : [];
//        //        Promise.all([updateMPEPerformanceSummaryStatus(data)]).then(function () {
//        //            connection.invoke("JoinGroup", "MPESynopsis").catch(function (err) {
//        //                return console.error(err.toString());
//        //            });

//        //        });
//        //    }).catch(function (err) {
//        //        console.error(err);
//        //    });

//        //});
//    } catch (err) {
//        console.log(err);
//        setTimeout(start, 5000);
//    }
//};

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
        return winLoc.origin + "/CF";
    }
    else {
        return winLoc.origin;
    }
}
// Start the connection.
//start();