
//on close clear all inputs
$('#Zone_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .end()
        .find("span[class=text]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change();
});
//on open set rules
$('#Zone_Modal').on('shown.bs.modal', function () {

});
let isAreaZoneRemoved = false;

let geoZoneArea = new L.GeoJSON(null, {
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
        layer.on('click', function (e) {
            OSLmap.setView(e.sourceTarget.getCenter(), 3);
        });

        layer.bindTooltip(feature.properties.name, {
            permanent: true,
            interactive: true,
            direction: "center",
            opacity: 1,
            className: 'location '
        }).openTooltip();

    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});

// add to the map and layers control
let geoZoneAreaoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneAreaoverlayLayer, "Area Zones");
geoZoneArea.addTo(geoZoneAreaoverlayLayer);

async function findAreaLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneArea.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given MPE Zone Id: ' + zoneId));
    });
}
async function init_geoZoneArea() {
    $(document).on('change', '.leaflet-control-layers-selector', function (e) {
        let sp = this.nextElementSibling;
        if (/^(Area Zones)/ig.test(sp.innerHTML.trim())) {
            if (this.checked) {
                connection.invoke("JoinGroup", "Area").catch(function (err) {
                    return console.error(err.toString());
                });
            }
            else {
                connection.invoke("LeaveGroup", "Area").catch(function (err) {
                    return console.error(err.toString());
                });
            }
        }

    });
    connection.invoke("JoinGroup", "Area").catch(function (err) {
        return console.error(err.toString());
    });
}
connection.on("addAreazone", async (zoneDate) => {
    Promise.all([addAreaFeature(zoneDate)]);

});
connection.on("deleteAreazone", async (zoneDate) => {
    Promise.all([deleteAreaFeature(zoneDate)]);

});
connection.on("updateAreazone", async (mpeZonedata) => {
    await findAreaLeafletIds(mpeZonedata.properties.id)
        .then(leafletIds => {
            geoZoneArea._layers[leafletIds].properties = mpeZonedata.properties;
        });

});
async function addAreaFeature(data) {
    try {
        await findAreaLeafletIds(data.properties.id)
            .then(leafletIds => {

            })
            .catch(error => {
                geoZoneArea.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function deleteAreaFeature(data) {
    try {
        await findAreaLeafletIds(data.properties.id)
            .then(leafletIds => {
                geoZoneArea.removeLayer(leafletIds);
            })
            .catch(error => {

            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}