
$('#UserTag_Modal').on('hidden.bs.modal', function () {
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
    sidebar.open('userprofile');
    $('#personform').css("display", "none");
});
$('#UserTag_Modal').on('shown.bs.modal', function () {
    $('select[id=tagCraftName_select]').on('change', function () {
        if ($(this).val() === "") {
            $('span[id=error_tagCraftName_select]').text("Pleas select a Category");
        }
        else {
            $('span[id=error_tagCraftName_select]').text("");
        }
    });

    // on change of the tagType_select


    $('#tagType_select').on('change', function () {
        if ($(this).val() === "") {
            $('#error_tagType_select').text("Please select a Category");
        }
        else {
            $('#error_tagType_select').text("");

            if (/Badge/ig.test($(this).val())) {
                //display the person form
                $('#personform').css("display", "block");
            }
            else {
                $('#personform').css("display", "none");
            }
        }
    });
});
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

});
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
            opacity: 1
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let overlayCameraLayer = L.layerGroup();
layersControl.addOverlay(overlayCameraLayer, "Cameras");
markerCameras.addTo(overlayCameraLayer);
async function hidestafftables() {
    $('div[id=div_taginfo]').css('display', '');
    $('div[id=div_userinfo]').css('display', 'none');
    $('div[id=div_overtimeinfo]').css('display', 'none');
    $('div[id=div_staffinfo]').css('display', 'none');

}
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
                markerCameras._layers[leafletIds].bindTooltip("", {
                    permanent: true,
                    interactive: true,
                    direction: 'center',
                    opacity: 1,
                    className: classname,
                }).openTooltip();
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
function getmarkerType(type) {
    try {
        if (/^supervisor/ig.test(type)) {
            return 'persontag_supervisor ';
        }
        else if (/^maintenance/ig.test(type)) {
            return 'persontag_maintenance ';
        }
        else if (/^(LABORER CUSTODIAL|CUSTODIAN|CUTODIAN|Custodian|Custodian)/ig.test(type)) {
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
function createTagDataTable(table) {
    let arrayColums = [{
        "INDEX": "",
        "KEY_NAME": "",
        "VALUE": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/INDEX/i.test(key)) {
            tempc = {
                "title": "index",
                "width": "5%",
                "mDataProp": key
            }
        }
        else if (/KEY_NAME/i.test(key)) {
            tempc = {
                "title": 'Name',
                "width": "35%",
                "mDataProp": key
            }
        }
        else if (/VALUE/i.test(key)) {
            tempc = {
                "title": "Value",
                "width": "60%",
                "mDataProp": key
            }
        }

        columns.push(tempc);

    });
    $('#' + table).DataTable({
        dom: 'Bfrtip',
        bFilter: false,
        bdeferRender: true,
        bpaging: false,
        bPaginate: false,
        autoWidth: false,
        bInfo: false,
        destroy: true,
        language: {
            zeroRecords: "No Data"
        },
        aoColumns: columns,
        columnDefs: [
            {
                target: 0,
                visible: false,
                searchable: false
            }
        ],
        sorting: [[0, "asc"], [1, "asc"]]

    });
}
function updateTagDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            for (const element of newdata) {
                if (data.KEY_NAME === element.KEY_NAME) {
                    $('#' + table).DataTable().row(node).data(element).draw().invalidate();
                }
            }
        })
        if (loadnew) {
            loadStaffingDatatable(newdata, table);
        }
    }
}
function createStaffingDataTable(table) {
    let arrayColums = [{
        "icon": "",
        "type": "",
        "sche": "",
        "in_building": "",
        "epacs": ""
    }];
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key) {
        tempc = {};

        if (/type/i.test(key)) {
            tempc = {
                "title": 'Type',
                "width": "40%",
                "mDataProp": key
            };
        }
        else if (/sche/i.test(key)) {
            tempc = {
                "title": "Scheduled",
                "width": "15%",
                "mDataProp": key
            };
        }
        else if (/in_building/i.test(key)) {
            tempc = {
                "title": "WorkZone",
                "width": "15%",
                "mDataProp": key
            };
}
        else if (/epacs/i.test(key)) {
            tempc = {
                "title": "ePACS",
                "width": "15%",
                "mDataProp": key
            };
        }
        else if (/icon/i.test(key)) {
            tempc = {
                "title": 'Icon',
                "width": "5%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    return '<i class="leaflet-tooltip ' + getmarkerType(full.type) + '"></i>';

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
                });

        sidebar.close('');

    } catch (e) {
        $("#error_camera").text(e);
    }
        } catch (error) {
            $('span[id=error_usertagsubmitBtn]').text(error);
}
    });

    // Show the modal
    $('#UserTag_Modal').modal('show');
}