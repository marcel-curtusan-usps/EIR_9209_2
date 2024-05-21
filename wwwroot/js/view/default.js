
let appData = {};
let baselayerid = "";
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SiteURLconstructor(window.location) + "hubServics")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start().then(function () {
            //load Application Info
            connection.invoke("GetApplicationInfo").then(function (data) {
                let appData = JSON.parse(data);
                if (/^(Admin|OIE)/i.test(appData.role)) {
                    Promise.all([init_geoman_editing()]);
                    sidebar.addPanel({
                        id: 'setting',
                        tab: '<span class="iconCenter"><i class="pi-iconGearFill"></i></span>',
                        position: 'bottom',
                    });
                }
                Promise.all([UpdateOSLattribution(appData)]);
                $(`span[id="fotf-site-facility-name"]`).text(appData.name);
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load GeoZones MPE
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
            //load background images
            connection.invoke("GetBackgroundImages").then(function (data) {
                Promise.all([init_backgroundImages(data)]).then(function () {
                    connection.invoke("JoinGroup", "BackgroundImage").catch(function (err) {
                        return console.error(err.toString());
                    });
                });
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            //load Person Tags
            connection.invoke("GetPersonTags").then(function (data) {
                Promise.all([init_tagsEmployees(data)]).then(function () {
                    connection.invoke("JoinGroup", "Tags").catch(function (err) {
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
                    Promise.all([init_geoZoneMPE]);

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
connection.on("tags", async (data) => {
    let tagdata = JSON.parse(data);
    if (tagdata.properties.visible) {
        Promise.all([addFeature(tagdata)]);
    }
    else {
        Promise.all([deleteFeature(tagdata)]);
    }

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

function capitalize_Words(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}
function SiteURLconstructor(winLoc) {
    if (/^(.CF)/i.test(winLoc.pathname)) {
        return winLoc.origin + "/CF/";
    }
    else {
        return winLoc.origin + "/";
    }
}