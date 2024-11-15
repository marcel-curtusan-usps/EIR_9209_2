
//side bar setup
let sidebar = L.control.sidebar({
    autopan: false,       // whether to maintain the centered map point when opening the sidebar
    closeButton: true,    // whether t add a close button to the panes
    container: 'sidebar', // the DOM container or #ID of a predefined sidebar container that should be used
    position: 'left',     // left or right
});
let mainfloorOverlays = L.layerGroup();

let mainfloor = L.imageOverlay(null, [0, 0], { id: -1, zIndex: -1 }).addTo(mainfloorOverlays);
let baseLayers = {
    "Main Floor": mainfloor
};

let overlayMaps = {};
let layersSelected = [mainfloor];
//var CRSPixel = L.Util.extend(L.CRS.Simple, {
//	transformation: new L.Transformation(1,0,1,0)
//});
//setup map
let OSLmap = L.map('map', {
    crs: L.CRS.Simple,
    renderer: L.canvas({ padding: 0.5 }),
    preferCanvas: true,
    pmIgnore: false,
    markerZoomAnimation: true,
    minZoom: 0,
    maxZoom: 18,
    zoomControl: false,
    measureControl: true,
    tap: true,
    layers: layersSelected
});

sidebar.on('content', function (ev) {
    sidebar.options.autopan = false;
    $('div[id=machine_div]').attr("data-id", "");
    switch (ev.id) {
        case 'autopan':
            break;
        case 'setting':
            break;
        case 'reports':
            break;
        case 'userprofile':
            break;
        case 'vehicleinfo':
            break;
        case 'notificationsetup':
            break;
        case 'tripsnotificationinfo':
            break;
        default:
            sidebar.options.autopan = false;
            break;
    }
}).addTo(OSLmap);
sidebar.on('closing', function (e) {
    // e.id contains the id of the opened panel
    $('div[id=machine_div]').attr("data-id","");
})

// Add Layer Popover - Proposed
let layersControl = L.control.layers(baseLayers, overlayMaps, {
    sortLayers: true,
    sortFunction: function (layerA, layerB, nameA, nameB) {
        if (/FLOOR/i.test(nameA)) {
            if (/MAIN/i.test(nameA)) {
                return -1;
            }
            else {
                return nameA < nameB ? -1 : (nameB < nameA ? 1 : 0);
            }
        }
    },
    position: 'bottomright',
    collapsed: true,
    autoZIndex: false // Disable expand on hover
}).addTo(OSLmap);

// Add onclick event listener
layersControl.getContainer().onclick = function () {
    if (layersControl._container.classList.contains('leaflet-control-layers-expanded')) {
        layersControl._container.classList.remove('leaflet-control-layers-expanded');
    } else {
        layersControl._container.classList.add('leaflet-control-layers-expanded');
    }
};
//Add zoom button
new L.Control.Zoom({ position: 'bottomright' }).addTo(OSLmap);
//add View Ports
//L.easyButton({
//    position: 'bottomright',
//    states: [{
//        stateName: 'viewport',
//        icon: '<div id="viewportsToggle" data-toggle="popover"><i class="pi-iconViewport align-self-center" title="Viewports"></i></div>'
//    }]
//}).addTo(OSLmap);
async function updateOSLattribution(data) {
    return new Promise((resolve, reject) => {
        OSLmap.attributionControl.setPrefix("USPS " + data.ApplicationName + " (" + data.ApplicationVersion + ") | " + data.SiteName);
        resolve();
        return false;
    });
}
async function init_backgroundImages() {
    try {
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/BackgroundImage/GetAllImages',
            contentType: 'application/json',
            type: 'GET',
            success: function (MapData) {
                if (MapData.length > 0) {
                    $.each(MapData, function (index, backgroundImages) {
                        if (backgroundImages.source === "Cisco") {
                            L.Util.extend(L.CRS.Simple, {
                            	transformation: new L.Transformation(1,0,1,0)
                            });
                        }

                        loadOslDatatable([backgroundImages],"osltable")
                        if (!!backgroundImages) {
                            //Promise.all([loadOslDatatable([this], "osltable")]);
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
                            }
                            else if (!!this.backgroundImages) {
                                layersControl.addBaseLayer(L.imageOverlay(img.src, trackingarea.getBounds(), { id: this.id, zindex: index }), this.backgroundImages.name);

                            }
                        }
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
            },
            error: function (error) {
                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                connection.invoke("JoinGroup", "BackgroundImage").catch(function (err) {
                    return console.error(err.toString());
                });

            }
        });

    } catch (e) {
        console.log(e)
    }
}