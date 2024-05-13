//side bar setup
let sidebar = L.control.sidebar({
    container: 'sidebar', position: 'left', autopan: false
});

let mainfloorOverlays = L.layerGroup();

let mainfloor = L.imageOverlay(null, [0, 0], { id: -1, zIndex: -1 }).addTo(mainfloorOverlays);
let baseLayers = {
    "Main Floor": mainfloor
};

let overlayMaps = {};
let layersSelected = [mainfloor];
//setup map
let OSLmap = L.map('map', {
    crs: L.CRS.Simple,
    renderer: L.canvas({ padding: 0.5 }),
    preferCanvas: true,
    pmIgnore: false,
    markerZoomAnimation: false,
    minZoom: 0,
    maxZoom: 18,
    zoomControl: false,
    measureControl: true,
    tap: false,
    layers: layersSelected
});

sidebar.on('content', function (ev) {
    sidebar.options.autopan = false;
    switch (ev.id) {
        case 'autopan':
            break;
        case 'setting':
            /* Edit_AppSetting("app_settingtable");*/

            break;
        case 'reports':
            //Promise.all([getStaffInfo()]);
            //GetUserInfo();
            break;
        case 'userprofile':
            //GetUserProfile();
            break;
        case 'agvnotificationinfo':
            //LoadNotification("vehicle", "agvnotificationtable");
            break;
        case 'notificationsetup':
            // LoadNotificationsetup({}, "notificationsetuptable");
            break;
        case 'tripsnotificationinfo':
            // LoadNotification("routetrip", "tripsnotificationtable");
            break;
        default:
            sidebar.options.autopan = false;
            break;
    }
}).addTo(OSLmap);
// Add Layer Popover - Proposed
let layersControl = L.control.layers(baseLayers, overlayMaps, {
    sortLayers: true, sortFunction: function (layerA, layerB, nameA, nameB) {
        if (/FLOOR/i.test(nameA)) {
            if (/MAIN/i.test(nameA)) {
                return -1;
            }
            else {
                return nameA < nameB ? -1 : (nameB < nameA ? 1 : 0);
            }
        }
    }, position: 'bottomright', collapsed: false
}).addTo(OSLmap);
function UpdateOSLattribution() {
    OSLmap.attributionControl.setPrefix("USPS " + ApplicationInfo.name + " (" + ApplicationInfo.version + ") | " + ApplicationInfo.siteName);
}
async function init_backgroundImages(MapData) {
    try {
        if (MapData.length > 0) {

            $.each(MapData, function (index, backgroundImages) {
                if (!!backgroundImages) {
                    //Promise.all([loadFloorPlanDatatable([this], "backgroundimagetable")]);
                    //set new image
                    let trackingarea = L.polygon([[100, 150]], [[500, 5000]]);
                    let img = new Image();
                    //load Base64 image
                    img.src = backgroundImages.base64;
                    //create he bound of the image.
                    let bounds = [[backgroundImages.yMeter, backgroundImages.xMeter], [backgroundImages.heightMeter + backgroundImages.yMeter, backgroundImages.widthMeter + backgroundImages.xMeter]];
                    trackingarea = L.polygon(bounds, {});

                    if (index === 0) {
                        baselayerid = backgroundImages.coordinateSystemId;
                        mainfloor.options.id = backgroundImages.coordinateSystemId;
                        mainfloor.setUrl(img.src);
                        mainfloor.setZIndex(index);
                        mainfloor.setBounds(trackingarea.getBounds());
                        //center image
                        OSLmap.setView(trackingarea.getBounds().getCenter(), 1.5);
                        //Promise.all([init_zones(this.zones, baselayerid)]);
                        //Promise.all([init_locators(this.locators, baselayerid)]);
                    }
                    else if (!!this.backgroundImages) {
                        layersControl.addBaseLayer(L.imageOverlay(img.src, trackingarea.getBounds(), { id: this.id, zindex: index }), this.backgroundImages.name);

                    }
                }
            });
            //join Tags group 
            connection.invoke("AddToGroup", "Tags").catch(function (err) {
                return console.error(err.toString());
            });

        }
        else {
            let trackingarea = L.polygon([[100, 150]], [[500, 5000]]);
            let img = new Image();
            img.src = "";
            mainfloor.setUrl(img.src);
            mainfloor.setBounds(trackingarea.getBounds());
            OSLmap.setView(trackingarea.getBounds().getCenter(), 1.5);
        }
    } catch (e) {
        console.log(e)
    }
}