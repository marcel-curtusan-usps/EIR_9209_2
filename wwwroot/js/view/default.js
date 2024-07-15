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
let baselayerid = "";
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "/hubServics")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start().then(function () {
            //load Application Info
            connection.invoke("GetApplicationInfo").then(function (data) {
                appData = JSON.parse(data);
                if (/^(Admin|OIE)/i.test(appData.role)) {
                    Promise.all([init_geoman_editing()]);
                    sidebar.addPanel({
                        id: 'setting',
                        tab: '<span class="iconCenter"><i class="pi-iconGearFill"></i></span>',
                        position: 'bottom',
                    });
                }
                Promise.all([init_ApplicationConfiguration()]);
                Promise.all([UpdateOSLattribution(appData)]);
                Promise.all([init_TagSearch()]);
                Promise.all([init_backgroundImages(data)]);
                Promise.all([init_emailList()]);
                $(`span[id="fotf-site-facility-name"]`).text(appData.name);
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load Connection
            connection.invoke("GetConnectionList").then(function (data) {
                Promise.all([init_connection(data)]).then(function () {
                    connection.invoke("JoinGroup", "Connections").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load Connection Type
            connection.invoke("GetConnectionTypeList").then(function (data) {
                Promise.all([init_connectiontType(data)]).then(function () {
                    connection.invoke("JoinGroup", "ConnectionTypes").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load Designation Activity to Craft Type
            connection.invoke("GetDacodeToCraftTypeList").then(function (data) {
                Promise.all([init_dacodetocraftType(data)]).then(function () {
                    connection.invoke("JoinGroup", "DacodeToCraftTypes").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load background images
            //connection.invoke("GetBackgroundImages").then(function (data) {
            //    Promise.all([init_backgroundImages(data)]).then(function () {
            //        connection.invoke("JoinGroup", "BackgroundImage").catch(function (err) {
            //            return console.error(err.toString());
            //        });
            //    });
            //}).catch(function (err) {
            //    // handle error
            //    console.error(err);
            //});
            //load Person Tags
            connection.invoke("GetBadgeTags").then(function (data) {
                Promise.all([init_tagsEmployees(data)]).then(function () {
                    connection.invoke("JoinGroup", "Badge").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load PIV Tags
            connection.invoke("GetPIVTags").then(function (data) {
                Promise.all([init_tagsPIV(data)]).then(function () {
                    connection.invoke("JoinGroup", "PIVVehicle").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load PIV Tags
            connection.invoke("GetAGVTags").then(function (data) {
                Promise.all([init_tagsAGV(data)]).then(function () {
                    connection.invoke("JoinGroup", "AutonomousVehicle").catch(function (err) {
                        return console.error(err.toString());
                    });

                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load GeoZones MPE
            connection.invoke("GetGeoZones").then(function (data) {
                Promise.all([init_geoZone(data)]).then(function () {
                    Promise.all([init_geoZoneMPE()]);
                    Promise.all([init_geoZoneBin()]);
                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });


        }).catch(function (err) {
            setTimeout(start, 5000);
            return console.error(err.toString());

        });

    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});
connection.on("backgroundImages", async (id, data) => {
    // console.log(data);
    Promise.all([init_backgroundImages(JSON.parse(data))]);
});
connection.on("getTagData", async (id, data) => {
    console.log(data);
    // Promise.all([init_backgroundImages($.parseJSON(data))]);
});
connection.on("connection", async (data) => {
    Promise.all([updateConnection(JSON.parse(data))]);
});


connection.on("applicationInfo", async (id, data) => {
    ApplicationInfo = JSON.parse(data);
    UpdateOSLattribution();
    $(`span[id="fotf-site-facility-name"]`).text(ApplicationInfo.name);
    //add setting icon to the bottom side panel
    if (/^(admin)/i.test(ApplicationInfo.role)) {
        sidebar.addPanel({
            id: 'setting',
            tab: '<span class="iconCenter"><i class="pi-iconGearFill"></i></span>',
            position: 'bottom',
        });
    }

});
connection.on("siteInfo", async (id, data) => {
    ApplicationInfo = $.parseJSON(data);
    UpdateOSLattribution();
    $(`span[id="fotf-site-facility-name"]`).text(ApplicationInfo.name);
    //add setting icon to the bottom side panel
    if (/^(admin)/i.test(ApplicationInfo.role)) {
        sidebar.addPanel({
            id: 'setting',
            tab: '<span class="iconCenter"><i class="pi-iconGearFill"></i></span>',
            position: 'bottom',
        });
    }

});

// Start the connection.
start();
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
    if (/^(.CF)/i.test(winLoc.pathname)) {
        return winLoc.origin + "/CF";
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