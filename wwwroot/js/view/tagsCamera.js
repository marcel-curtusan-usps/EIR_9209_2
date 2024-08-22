
connection.on("updateCamera", async (data) => {
    let tagdata = JSON.parse(data);
    Promise.all([addCameraFeature(tagdata)]);
});
let defaultTime = 90;
let count = 0;
let timer;
function startTimer() {
    timer = setInterval(function () {
        let newcount = count * 1 - 1;
        count = newcount
        $('#counter').html(count);
        if (count <= 0) {
            $('#counter').html('0');
            $('#countDownView').hide();
            $('#timeOutView').show();
            $('div[id=Camera_Modal]').attr("data-id", "0");
            $('div[id=camera_modalbody]').empty();
        }
    }, 1000);
}

let markerCameras = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        if (feature.properties.cameraDirection) {
            //camera cone for markers with a cameraDirection set
            let icon = L.divIcon({
                className: 'custom-div-icon',
                html: "<div style='transform:rotate(" + feature.properties.cameraDirection + "deg)' class='marker-camera-cone'></div><div class='marker-pin'></div><i class='bi-camera-fill'></i>",
                iconSize: [30, 42],
                iconAnchor: [15, 42]
            });
            return L.marker(latlng, {
                icon: icon,
                riseOnHover: true,
                bubblingMouseEvents: true,
                popupOpen: true
            });
        } else {
            let icon = L.divIcon({
                className: 'custom-div-icon',
                html: "<div class='marker-pin'></div><i class='bi-camera-fill'></i>",
                iconSize: [30, 42],
                iconAnchor: [15, 42]
            });
            return L.marker(latlng, {
                icon: icon,
                riseOnHover: true,
                bubblingMouseEvents: true,
                popupOpen: true
            });
        }

    },
    onEachFeature: function (feature, layer) {
        layer.markerId = feature.properties.id;
        layer.on('click', function (e) {
            $('#timeOutView').hide();
            $('#countDownView').show();
            clearInterval(timer);
            count = defaultTime;
            $('#counter').html(count);
            setTimeout(startTimer, 300);

            LoadWeb_CameraImage(feature.properties.cameraData, feature.properties.id, feature.properties.cameraDirection, feature.properties.base64Image);
        });
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let overlayCameraLayer = L.layerGroup();
layersControl.addOverlay(overlayCameraLayer, "Cameras");
markerCameras.addTo(overlayCameraLayer);
async function findCameraLeafletIds(markerId) {
    return new Promise((resolve, reject) => {
        markerCameras.eachLayer(function (layer) {
            if (layer.markerId === markerId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_markerCameras(data) {
    return new Promise((resolve, reject) => {
        try {
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^Cameras$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "Camera").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "Camera").catch(function (err) {
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
async function deleteCameraFeature(data) {
    try {

        await findCameraLeafletIds(data.properties.id)
            .then(leafletIds => {
                //remove from markerCameras
                markerCameras.removeLayer(leafletIds);
            })
            .catch(error => {
            });
    } catch (e) {
        throw new Error(e.toString());
    }
}
async function addCameraFeature(data) {
    try {
        await findCameraLeafletIds(data.properties.id)
            .then(leafletIds => {
                Promise.all([positionCameraUpdate(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
                markerCameras._layers[leafletIds].feature.properties = data.properties;
                if (($('#Camera_Modal').hasClass('show')) && $('div[id=Camera_Modal]').attr("data-id") === data.properties.id) {
                    camera_modal_body = $('div[id=camera_modalbody]');
                    camera_modal_body.empty();
                    camera_modal_body.append(ImageLayout.supplant(formatImageLayout(data.properties.base64Image)));
                }
            })
            .catch(error => {
                markerCameras.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function updateCameraFeature(data) {
    try {
        let tag = data;
        await findCameraLeafletIds(tag.properties.id)
            .then(leafletIds => {
                //let VisiblefillOpacity = tag.properties.visible ? "" : "tooltip-hidden";
                //let classname = getmarkerType(tag.properties.craftName) + VisiblefillOpacity;

                markerCameras._layers[leafletIds].feature.properties = tag.properties;
            })
            .catch(error => {
                //
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function positionCameraUpdate(leafletId, lat, lag) {
    return new Promise((resolve, reject) => {

        if (markerCameras._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
            markerCameras._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
            resolve();
            return false;
        }
        else {
            markerCameras._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
            resolve();
            return false;
        }
    });
}

let ImageLayout = '<div>{image}</div>';
function formatImageLayout(img) {
    let newimg = new Image();
    newimg.src = img;
    return $.extend(newimg, {
        image: newimg.outerHTML
    });
}
var webCameraViewData = null;
function LoadWeb_CameraImage(Data, id, direction, image) {
    try {
        $('#cameramodalHeader').text('View Web Camera');
        //$('#cameradescription').text(Data.DESCRIPTION);
        camera_modal_body = $('div[id=camera_modalbody]');
        camera_modal_body.empty();
        camera_modal_body.append(ImageLayout.supplant(formatImageLayout(image)));
        $('div[id=Camera_Modal]').attr("data-id", id);
        $('#Camera_Modal').modal('show');

        if (direction) {
            document.getElementById("cameraViewDirectionArrow").style.transform = 'rotate(' + direction + 'deg)';
            document.getElementById("cameraViewDirectionArrow").style.display = 'block';
            document.getElementById("cameraViewDirectionNotice").style.display = 'none';
        } else {
            document.getElementById("cameraViewDirectionArrow").style.display = 'none';
            document.getElementById("cameraViewDirectionNotice").style.display = 'block';
        }

        sidebar.close('');

    } catch (e) {
        $("#error_camera").text(e);
    }
}

