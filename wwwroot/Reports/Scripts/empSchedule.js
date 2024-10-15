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
            if (data.length > 0) {
                //sort 
                data.sort();
                $.each(data, function () {
                    $('<option/>').val(this).html(this).appendTo('select[id=payweek]');
                });
                let currentpayweek = data[0];
                $('select[name=payweek]').val(currentpayweek);
                Promise.all([LoadPayWeekList(currentpayweek)]);
            }
        },
        error: function (error) {
            console.log(error);
        },
        faulure: function (fail) {
            console.log(fail);
        }
    });
    $('select[name=payweek]').on("change", function () {
        let payweekSelected = $('select[name=payweek] option:selected').val();
     
        Promise.all([LoadPayWeekList(payweekSelected)]);
    })

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
    let pathname = winLoc.pathname;
    let match = pathname.match(/^\/([^\/]*)/);
    let urlPath = match[1];
    if (/^(.CF)/i.test(urlPath)) {
        return winLoc.origin + "/" + urlPath;
    }
    else {
        return winLoc.origin;
    }
}
// Start the connection.
//start();