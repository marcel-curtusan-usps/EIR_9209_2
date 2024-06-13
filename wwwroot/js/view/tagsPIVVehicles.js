let tagsPIVVehicles = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        let vehicleIcon = L.divIcon({
            id: feature.properties.id,
            className: get_pi_icon(feature.properties.name, feature.properties.Tag_Type) + ' iconXSmall',
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
            //makea ajax call to get the employee details
            $.ajax({
                url: '/api/Tag/' + feature.properties.id,
                type: 'GET',
                success: function (data) {
                    //$('button[name="tagEdit"]').attr('data-id', feature.properties.id);
                    //Promise.all([hidestafftables()]);
                    //$('div[id=div_taginfo]').css('display', '');
                    //data.properties.posAge = feature.properties.posAge;
                    //data.properties.locationMovementStatus = feature.properties.locationMovementStatus;
                    //updateTagDataTable(formattagdata(data.properties), "tagInfotable");
                    sidebar.open('reports');

                },
                error: function (error) {
                    console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                }
            });
        });
        layer.bindTooltip(feature.properties.name.replace(/[^0-9.]/g, '').replace(/^0+/, ''), {
            permanent: true,
            interactive: true,
            direction: 'top',
            opacity: 0.9,
            className: 'vehiclenumber ' + obstructedState
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
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
async function init_tagsPIV(data) {
    return new Promise((resolve, reject) => {
        try {
       
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^badges$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "PIVTags").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "PIVTags").catch(function (err) {
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

function getmarkerType(type) {
    try {
        if (/^supervisor/ig.test(type)) {
            return 'persontag_supervisor ';
        }
        else if (/^maintenance/ig.test(type)) {
            return 'persontag_maintenance ';
        }
        else if (/^(LABORER CUSTODIAL|CUSTODIAN|CUTODIAN|Custodian)/ig.test(type)) {
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
            return 'persontag_unknown ';
        }
        else {
            return 'persontag_unknown ';
        }

    } catch (e) {
        return 'persontag ';
    }

}
