
let ApplicationInfo = {};
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubServics")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start().then(function () {
            console.log("SignalR Connected.");
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
    Promise.all([init_backgroundImages($.parseJSON(data))]);
});
connection.on("getTagData", async (id, data) => {
    console.log(data);
    // Promise.all([init_backgroundImages($.parseJSON(data))]);
});
connection.on("applicationInfo", async (id, data) => {
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
