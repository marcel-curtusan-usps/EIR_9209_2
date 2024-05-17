

let tagsEmployees = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        return new L.circleMarker(latlng, {
            class: "persontag",
            radius: 4,
            opacity: 2,
            fillOpacity: 3
        });

    },
    onEachFeature: function (feature, layer) {
        layer.markerId = feature.properties.id;
        let VisiblefillOpacity = feature.properties.visible ? "" : "tooltip-hidden";

        let classname = getmarkerType(feature.properties.craftName) + VisiblefillOpacity;
        layer.on('click', function (e) {
            sidebar.open('reports');
        });
        layer.bindTooltip("", {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1,
            className: classname,
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let overlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(overlayLayer, "Badges");
tagsEmployees.addTo(overlayLayer);
async function findLeafletIds(markerId) {
    return new Promise((resolve, reject) => {
        tagsEmployees.eachLayer(function (layer) {
            if (layer.markerId === markerId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_tagsEmployees(data) {
    return new Promise((resolve, reject) => {
        try {
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^badges$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "Tags").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "Tags").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                }
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
async function deleteFeature(data, floorId) {
    try {

        await findLeafletIds(data.properties.id)
            .then(leafletIds => {
                //remove from tagsEmployees
                tagsEmployees.removeLayer(leafletIds);
            })
            .catch(error => {
            });
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function addFeature(data) {
    try {
        await findLeafletIds(data.properties.id)
            .then(leafletIds => {
                Promise.all([positionUpdate(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
            })
            .catch(error => {
                tagsEmployees.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function positionUpdate(leafletId, lat, lag) {
    return new Promise((resolve, reject) => {

        if (tagsEmployees._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
            tagsEmployees._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
            resolve();
            return false;
        }
        else {
            tagsEmployees._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
            resolve();
            return false;
        }
    });
}
function getmarkerType(type) {
    try {
        if (/^supervisor/ig.test(type)) {
            return 'persontag_supervisor ';
        }
        else if (/^maintenance/ig.test(type)) {
            return 'persontag_maintenance ';
        }
        else if (/^(LABORER CUSTODIAL|CUSTODIAN|CUTODIAN)/ig.test(type)) {
            return 'persontag_custodial ';
        }
        //else if (/pse/ig.test(type)) {
        //    return 'persontag_pse ';
        //}
        else if (/inplantsupport/ig.test(type)) {
            return 'persontag_inplantsupport ';
        }
        else if (/^(clerk|mailhandler|mha|mail|pse)/ig.test(type)) {
            return 'persontag ';
        }
        else if (type.length === 0) {
            return 'persontag_blank ';
        }
        else {
            return 'persontag_unknown ';
        }

    } catch (e) {
        return 'persontag ';
    }

}
