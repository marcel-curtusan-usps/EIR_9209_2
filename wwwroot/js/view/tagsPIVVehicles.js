

connection.on("updatePIVVehicleTagPosition", async (tagdata) => {
    try {
        Promise.all([addPIVFeature(tagdata)]);
    } catch (e) {
        throw new Error(e.toString());
    }
});
let tagsPIVVehicles = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        let vehicleIcon = L.divIcon({
            id: feature.properties.id,
            className: get_pi_icon(feature.properties.name, feature.properties.tagType) + ' iconXSmall',
            html: '<i>' +
                '<span class="path1"></span>' +
                '<span class="path2"></span>' +
                '<span class="path3"></span>' +
                '<span class="path4"></span>' +
                '<span class="path5"></span>' +
                '<span class="path6"></span>' +
                '<span class="path7"></span>' +
                '<span class="path8"></span>' +
                '<span class="path9"></span>' +
                '<span class="path10"></span>' +
                '</i>'
        });
        return L.marker(latlng, {
            icon: vehicleIcon,
            title: feature.properties.name,
            riseOnHover: true,
            bubblingMouseEvents: true,
            popupOpen: true
        })

    },
    onEachFeature: function (feature, layer) {
        layer.markerId = feature.properties.id;
        let VisiblefillOpacity = feature.properties.visible ? "" : "tooltip-hidden";
        let obstructedState = '';
        layer.on('click', function (e) {

        });
        layer.bindTooltip(feature.properties.name.replace(/[^0-9.]/g, '').replace(/^0+/, ''), {
            permanent: true,
            interactive: true,
            direction: 'top',
            opacity: 0.9,
            className: 'vehiclenumber ' + obstructedState
        }).openTooltip();
    }
});
// add to the map and layers control
let overPivLayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(overPivLayLayer, "PIV Vehicles");
tagsPIVVehicles.addTo(overPivLayLayer);
async function findPIVLeafletIds(markerId) {
    return new Promise((resolve, reject) => {
        tagsPIVVehicles.eachLayer(function (layer) {
            if (layer.markerId === markerId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_tagsPIV() {
    return new Promise((resolve, reject) => {
        try {
            //load PIV Tags
            connection.invoke("GetPIVTags").then(function (data) {
                //add PIV markers to the layer
                for (let i = 0; i < data.length; i++) {
                    Promise.all([addPIVFeatures(data[i])]);
                }
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
            
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^PIV Vehicles$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "PIVVehicle").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "PIVVehicle").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                }
            });
            connection.invoke("JoinGroup", "PIVVehicle").catch(function (err) {
                return console.error(err.toString());
            });
            resolve();
            return false;
        }
        catch (e) {
            throw new Error(e.toString());
            reject();
        }
    });
}
async function deletePIVFeature(data) {
    try {

        await findPIVLeafletIds(data.properties.id)
            .then(leafletIds => {
                //remove from tagsEmployees
                tagsPIVVehicles.removeLayer(leafletIds);
            })
            .catch(error => {
            });
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function addPIVFeature(data) {
    try {
        await findPIVLeafletIds(data.properties.id)
            .then(leafletIds => {
                Promise.all([PIVpositionUpdate(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
            })
            .catch(error => {
                tagsPIVVehicles.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function PIVpositionUpdate(leafletId, lat, lag) {
    return new Promise((resolve, reject) => {

        if (tagsPIVVehicles._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
            tagsPIVVehicles._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
            resolve();
            return false;
        }
        else {
            tagsPIVVehicles._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
            resolve();
            return false;
        }
    });
}

