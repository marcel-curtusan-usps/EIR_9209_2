let geoZoneDockDoor = new L.GeoJSON(null, {
    style: function (feature) {
        return {
            weight: 1,
            opacity: 1,
            color: '#989ea4',
            fillOpacity: 0.2,
            lastOpacity: 0.2
        };
    },
    onEachFeature: function (feature, layer) {

        layer.zoneId = feature.properties.id;
        //extract number from feature.properties.name and remove leading zeros
        let dockDoorNumber = parseInt(feature.properties.name.match(/\d+/g)[0], 10);

        layer.on('click', function (e) {

            //if sourceTarget is not available, use target
            if (e.sourceTarget.getCenter) {
               
                OSLmap.setView(e.sourceTarget.getCenter(), 3);
                Promise.all([LoadDockDoorTable(feature.properties)]);
            }
        });
        layer.bindTooltip(dockDoorNumber.toString(), {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 0.9,
            className: 'dockdooknumber '
        }).openTooltip();

    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});

// add to the map and layers control
let geoZoneDockDooroverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneDockDooroverlayLayer, "Dock Door Zones");
geoZoneDockDoor.addTo(geoZoneDockDooroverlayLayer);

async function findDockDoorZoneLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneDockDoor.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given DockDoor Zone Id'));
    });
}
async function init_geoZoneDockDoor(data) {
    //load dockdoor data
    for (let i = 0; i < data.length; i++) {
        ;
        Promise.all([addDockDoorFeature(data[i])]);
    }
    $(document).on('change', '.leaflet-control-layers-selector', function (e) {
        let sp = this.nextElementSibling;
        if (/^(Dock Door)/ig.test(sp.innerHTML.trim())) {
            if (this.checked) {
                connection.invoke("JoinGroup", "DockDoor").catch(function (err) {
                    return console.error(err.toString());
                });
            }
            else {
                connection.invoke("LeaveGroup", "DockDoor").catch(function (err) {
                    return console.error(err.toString());
                });
            }
        }

    });
    connection.invoke("JoinGroup", "DockDoor").catch(function (err) {
        return console.error(err.toString());
    });
}
async function addDockDoorFeature(data) {
    try {
        await findDockDoorZoneLeafletIds(data.properties.id)
            .then(leafletIds => {

            })
            .catch(error => {
                geoZoneDockDoor.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function LoadDockDoorTable(data) {
    try {
        dockdoorloaddata = [];
        hideSidebarLayerDivs();
        $('div[id=dockdoor_div]').attr("data-id", data.id);
        $('div[id=dockdoor_div]').css('display', 'block');
        $('div[id=dockdoor_tripdiv]').css('display', 'block');
        $('div[id=ctstabs_div]').css('display', 'none');
        $('div[id=trailer_div]').css('display', 'block');
        $('button[name=container_counts]').text(0 + "/" + 0);
        $('span[name=doornumberid]').text(data.doorNumber);
        $('select[id=tripSelector]').val("");
        $('span[name=doorview]').empty();
        $('button[name=dockdoorinfoedit]').attr('id', data.id);
        $('span[name=doorstatus]').text("Unknown");
        if (data.externalUrl) {
            $("<a/>").attr({ target: "_blank", href: data.externalUrl, style: 'color:white;' }).html("View").appendTo($('span[name=doorview]'));
        }
        else {
            $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + 'Dockdoor/Dockdoor.aspx?DockDoor=' + data.doorNumber, style: 'color:white;' }).html("View").appendTo($('span[name=doorview]'));
        }
        sidebar.open('home');


    }
    catch (e) {
        throw new Error(e.toString());
    }
}