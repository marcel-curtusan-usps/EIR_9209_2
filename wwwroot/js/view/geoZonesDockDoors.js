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
    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});

// add to the map and layers control
let geoZoneDockDooroverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneDockDooroverlayLayer, "Dock Door");
geoZoneDockDoor.addTo(geoZoneDockDooroverlayLayer);

async function findDockDoorZoneLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneDockDoor.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_geoZoneDockDoor() {
    $(document).on('change', '.leaflet-control-layers-selector', function (e) {
        let sp = this.nextElementSibling;
        if (/^(Dock Door)$/ig.test(sp.innerHTML.trim())) {
            if (this.checked) {
                connection.invoke("AddToGroup", "DockDoorZones").catch(function (err) {
                    return console.error(err.toString());
                });
            }
            else {
                connection.invoke("RemoveFromGroup", "DockDoorZones").catch(function (err) {
                    return console.error(err.toString());
                });
            }
        }

    });
    connection.invoke("JoinGroup", "DockDoorZones").catch(function (err) {
        return console.error(err.toString());
    });
}
async function addDockDoorFeature(data) {
    try {
        await findMpeZoneLeafletIds(data.properties.id)
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