let geoZoneMPE = new L.GeoJSON(null, {
    style: function (feature) {
        return {
            weight: 1,
            opacity: 1,
            color: '#3573b1',
            fillOpacity: 0.2,
            fillColor: GetMacineBackground(feature.properties.MPEWatchData),
            lastOpacity: 0.2
        };
    },
    onEachFeature: function (feature, layer) {

        layer.zoneId = feature.properties.id;
        layer.on('click', function (e) {
            //makea ajax call to get the employee details
            $.ajax({
                url: '/api/Zone/' + feature.properties.id,
                type: 'GET',
                success: function (data) {
                    //$('#content').html(data);
                    sidebar.open('reports');
                },
                error: function (error) {
                    console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                    console.log(complete);
                }
            });
            sidebar.open('reports');
        });

    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});

// add to the map and layers control
let geoZoneMPEoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneMPEoverlayLayer, "MPE Zones");
geoZoneMPE.addTo(geoZoneMPEoverlayLayer);

async function findMpeZoneLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneMPE.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_geoZoneMPE() {
    $(document).on('change', '.leaflet-control-layers-selector', function (e) {
        let sp = this.nextElementSibling;
        if (/^(MPE Zones)$/ig.test(sp.innerHTML.trim())) {
            if (this.checked) {
                connection.invoke("AddToGroup", "MPEZones").catch(function (err) {
                    return console.error(err.toString());
                });
            }
            else {
                connection.invoke("RemoveFromGroup", "MPEZones").catch(function (err) {
                    return console.error(err.toString());
                });
            }
        }

    });
    connection.invoke("JoinGroup", "MPEZones").catch(function (err) {
        return console.error(err.toString());
    });
}
connection.on("UpdateGeoZone", async (data) => {
    let mpeZonedata = JSON.parse(data);
    await findMpeZoneLeafletIds(mpeZonedata.properties.id)
        .then(leafletIds => {
            geoZoneMPE._layers[leafletIds].properties = mpeZonedata.properties;
        });

});
async function addMPEFeature(data) {
    try {
        await findMpeZoneLeafletIds(data.properties.id)
            .then(leafletIds => {

            })
            .catch(error => {
                geoZoneMPE.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
function GetMacineBackground(mpeWatchData) {
    let NotRunningbkColor = '#989ea4';
    let RunningColor = '#3573b1';
    let WarningColor = '#ffc107';
    let AlertColor = '#dc3545';
    try {
        if (mpeWatchData.cur_sortplan === "0" || mpeWatchData.cur_sortplan === '') {
            return NotRunningbkColor;
        }
        else {

            if (mpeWatchData.throughput_status === 3) {
                return AlertColor;
            }
            if (mpeWatchData.unplan_maint_sp_status === 2 || mpeWatchData.op_started_late_status === 2 || mpeWatchData.op_running_late_status === 2 || mpeWatchData.sortplan_wrong_status === 2 || mpeWatchData.throughput_status === 2) {
                return AlertColor;
            }
            if (mpeWatchData.unplan_maint_sp_status === 1 || mpeWatchData.op_started_late_status === 1 || mpeWatchData.op_running_late_status === 1 || mpeWatchData.sortplan_wrong_status === 1) {
                return WarningColor;
            }
            return RunningColor;
        }
    }
    catch (e) {

    }

}