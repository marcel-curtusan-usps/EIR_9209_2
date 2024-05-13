
let tagsEmployees = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        return new L.marker(latlng, {
            radius: 8,
            opacity: 2,
            fillOpacity: 3
        });

    },
    onEachFeature: function (feature, layer) {
        layer.markerId = feature.properties.id;
        layer.on('click', function (e) {

        });
        layer.bindTooltip("", {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
let tagsEmployeesoverlays = {
    "Badges": tagsEmployees // replace "Overlay Name" with your actual overlay name
};
L.control.layers(null, tagsEmployeesoverlays).addTo(OSLmap);
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

async function addFeature(data) {
    try {
        await findLeafletIds(data.properties.id)
            .then(leafletIds => {
                Promise.all([positionUpdate(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
            })
            .catch(error => {
                tagsEmployees.addData(data);
            });
        //if (data.properties.floorId === baselayerid) {
        //    let updateId = markerList[data.properties.id];
        //    if (typeof updateId !== 'undefined' && OSLmap.hasLayer(updateId)) {
        //        Promise.all([positionUpdate(updateId._leaflet_id, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
        //    }
        //    else {
        //        tagsEmployees.addData(data);
        //    }
        //}
        //return true;
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function positionUpdate(leafletId, lat, lag) {
    return new Promise((resolve, reject) => {

        tagsEmployees._layers[leafletId].setLatLng(new L.LatLng(lat, lag));

        resolve();
    });
}

