
$('#Camera_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .prop('disabled', false)
        .end()
        .find("input[type=radio]")
        .prop('disabled', false)
        .prop('checked', false).change()
        .end()
        .find("span[class=text-info]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change()
        .end();

    //stop the startTimer when closing the modal

    clearInterval(timer);
    connection.invoke("LeaveGroup", "CamerasStill").catch(function (err) {
        return console.error(err.toString());
    });
   
});
$('#Camera_Modal').on('shown.bs.modal', function () {
    connection.invoke("JoinGroup", "CamerasStill").catch(function (err) {
        return console.error(err.toString());
    });
});
connection.on("addCameras", async (data) => {
   
    Promise.all([addCameraFeature(data)]);
});
connection.on("updateCameras", async (data) => {
    Promise.all([updateCameraFeature(data)]);
});
connection.on("deleteCameras", async (data) => {
    Promise.all([deleteCameraFeature(data)]);
});
connection.on("updateCamerasStill", async (data) => {
    Promise.all([updateCameraStillFeature(data)]);
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
            connection.invoke("LeaveGroup", "CamerasStill").catch(function (err) {
                return console.error(err.toString());
            });
        }
    }, 1000);
};
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
        layer.bindTooltip("", {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 0
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let overlayCameraLayer = L.layerGroup().addTo(OSLmap);
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
async function init_tagsCamera() {
    return new Promise((resolve, reject) => {
        try {
            //load cameras
            connection.invoke("GetCameras").then(function (data) {
                //add camera markers to the layer
                for (let i = 0; i < data.length; i++) {
                    ;
                    Promise.all([addCameraFeature(data[i])]);
                }
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
     
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^Cameras$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "Cameras").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "Cameras").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                }
            });
            connection.invoke("JoinGroup", "Cameras").catch(function (err) {
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
        let Camera = data;
        await findCameraLeafletIds(Camera.properties.id)
            .then(leafletIds => {
                markerCameras._layers[leafletIds].feature.properties = data.properties;
            })
            .catch(error => {
                //
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function updateCameraStillFeature(data) {
    try {
        let Camera = data;
        await findCameraLeafletIds(Camera.properties.id)
            .then(leafletIds => {
                markerCameras._layers[leafletIds].feature.properties.base64Image = data.properties.base64Image;
                if (($('#Camera_Modal').hasClass('show')) && $('div[id=Camera_Modal]').attr("data-id") === data.properties.id) {
                    camera_modal_body = $('div[id=camera_modalbody]');
                    camera_modal_body.empty();
                    camera_modal_body.append(ImageLayout.supplant(formatImageLayout(data.properties.base64Image)));
                }
            })
            .catch(error => {
                //
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}


let ImageLayout = '<div class="row">{image}</div>';
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