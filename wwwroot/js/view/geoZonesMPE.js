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
    $('span[id=error_machinesubmitBtn]').text("");
    $('button[id=machinesubmitBtn]').prop('disabled', true);
    //Request Type Keyup
    $('input[type=text][name=machine_name]').keyup(function () {
        if (!checkValue($('input[type=text][name=machine_name]').val())) {
            $('input[type=text][name=machine_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_machine_name]').text("Please Enter Machine Name");
        }
        else {
            $('input[type=text][name=machine_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_machine_name]').text("");
        }

        enablezoneSubmit();
    });
    //Connection name Validation
    if (!checkValue($('input[type=text][name=machine_name]').val())) {
        $('input[type=text][name=machine_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_machine_name]').text("Please Enter Machine Name");
    }
    else {
        $('input[type=text][name=machine_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_machine_name]').text("");
    }
    //Request Type Keyup
    $('input[type=text][name=machine_number]').keyup(function () {
        if (!checkValue($('input[type=text][name=machine_number]').val())) {
            $('input[type=text][name=machine_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_machine_number]').text("Please Enter Machine Number");
        }
        else {
            $('input[type=text][name=machine_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_machine_number]').text("");
        }

        enablezoneSubmit();
    });
    //ipaddress 
    $('input[type=text][name=machine_ip]').keyup(function () {
        if (IPAddress_validator($('input[type=text][name=machine_ip]').val()) === 'Invalid IP Address') {
            $('input[type=text][name=machine_ip]').css("border-color", "#FF0000");
            $('span[id=error_machine_ip]').text("Please Enter Valid IP Address!");
        }
        else {
            $('input[type=text][name=machine_ip]').css({ "border-color": "#2eb82e" }).removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_machine_ip]').text("");
        }

        enablezoneSubmit();
    });
    $('select[name=zonePayLocationColor]').change(function () {
        if ($('select[name=zonePayLocationColor]').val() === "") {
            $('span[id=error_zonePayLocationColor]').text("Pleas select a Color");
            $('select[name=zonePayLocationColor]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        }
        else {
            $('span[id=error_zonePayLocationColor]').text("");
            $('select[name=zonePayLocationColor]').css({ "border-color": "#2eb82e" }).removeClass('is-invalid').addClass('is-valid');
        }
        enablezoneSubmit();
    });
    if ($('select[name=zonePayLocationColor]').val() === "") {
        $('select[name=zonePayLocationColor]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_zonePayLocationColor]').text("Please Enter select color");
    }
    else {
        $('select[name=zonePayLocationColor]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_zonePayLocationColor]').text("");
    }

    //GPIO
    //$('input[type=text][name=GPIO]').keyup(function () {
    //    if (!checkValue($('input[type=text][name=GPIO]').val())) {
    //        $('input[type=text][name=GPIO]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
    //        $('span[id=errorgpio]').text("Please Enter GPIO ");
    //    }
    //    else {
    //        $('input[type=text][name=GPIO]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
    //        $('span[id=errorgpio]').text("");
    //    }

    //    enablezoneSubmit();
    //});
    //if (!checkValue($('input[type=text][name=GPIO]').val())) {
    //    $('input[type=text][name=GPIO]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
    //    $('span[id=errorgpio]').text("Please Enter Machine Number");
    //}
    //else {
    //    $('input[type=text][name=GPIO]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
    //    $('span[id=errorgpio]').text("");
    //}
    //Request Type Validation
    if (!checkValue($('input[type=text][name=machine_number]').val())) {
        $('input[type=text][name=machine_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_machine_number]').text("Please Enter Machine Number");
    }
    else {
        $('input[type=text][name=machine_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_machine_number]').text("");
    }
    //zone LDC Validation
    if (checkValue($('input[type=text][name=zone_ldc]').val())) {
        $('input[type=text][name=zone_ldc]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=errorzone_ldc]').text("");
    }
    else {
        $('input[type=text][name=zone_ldc]').css("border-color", "#D3D3D3").removeClass('is-invalid').removeClass('is-valid');
        $('span[id=errorzone_ldc]').text("");
    }
    //Request zone LDC Keyup
    $('input[type=text][name=zone_ldc]').keyup(function () {
        if (checkValue($('input[type=text][name=zone_ldc]').val())) {
            $('input[type=text][name=zone_ldc]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=errorzone_ldc]').text("");
        }
        else {
            $('input[type=text][name=zone_ldc]').css("border-color", "#D3D3D3").removeClass('is-invalid').removeClass('is-valid');
            $('span[id=errorzone_ldc]').text("");
        }
    });
    //Zone Pay Location Validation
    if (!checkValue($('input[type=text][name=zone_paylocation]').val())) {
        $('input[type=text][name=zone_paylocation]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=errorzone_paylocation]').text("Please Enter Pay Location");
    }
    else {
        $('input[type=text][name=zone_paylocation]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=errorzone_paylocation]').text("");
    }
    //Request zone LDC Keyup
    $('input[type=text][name=zone_paylocation]').keyup(function () {
        if (!checkValue($('input[type=text][name=zone_paylocation]').val())) {
            $('input[type=text][name=zone_paylocation]').css("border-color", "#FF0000").removeClass('is-invalid').addClass('is-valid');
            $('span[id=errorzone_paylocation]').text("");
        }
        else {
            $('input[type=text][name=zone_paylocation]').css("border-color", "#2eb82e").removeClass('is-invalid').removeClass('is-valid');
            $('span[id=errorzone_paylocation]').text("");
        }
        enablezoneSubmit();
    });
});


let geoZoneMPE = new L.GeoJSON(null, {
    style: function (feature) {
        return {
            weight: 1,
            opacity: 1,
            color: '#989ea4',
            fillOpacity: 0.2,
            fillColor: GetMacineBackground(feature.properties.mpeRunPerformance),
            lastOpacity: 0.2
        };
    },
    onEachFeature: function (feature, layer) {

        layer.zoneId = feature.properties.id;

        layer.on('click', function (e) {
            OSLmap.setView(e.sourceTarget.getCenter(), 3);
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
                    $('div[id=machine_div]').attr("data-id", feature.properties.id);
                    $('div[id=machine_div]').css('display', 'block');
                    $('div[id=ctstabs_div]').css('display', 'block');

                    sidebar.open('home');


                }
            });

        });
        layer.bindTooltip(feature.properties.name + "<br/>" + "Staffing: " + (feature.properties.hasOwnProperty("CurrentStaff") ? feature.properties.CurrentStaff : "0"), {
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
connection.on("UpdateGeoZone", async (mpeZonedata) => {
    await findMpeZoneLeafletIds(mpeZonedata.properties.id)
        .then(leafletIds => {
            geoZoneMPE._layers[leafletIds].properties = mpeZonedata.properties;
        });

});
connection.on("MPEPerformanceUpdateGeoZone", async (mpeZonedata) => {
    await findMpeZoneLeafletIds(mpeZonedata.zoneId)
        .then(leafletIds => {
            geoZoneMPE._layers[leafletIds].feature.properties.mpeRunPerformance = mpeZonedata;
            geoZoneMPE._layers[leafletIds].setStyle({
                weight: 1,
                opacity: 1,
                fillOpacity: 0.2,
                fillColor: GetMacineBackground(mpeZonedata),
                lastOpacity: 0.2
            });
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
        if (mpeWatchData.curSortplan === "0" || mpeWatchData.curSortplan === '') {
            return NotRunningbkColor;
        }
        else {

            if (mpeWatchData.throughputStatus === 3) {
                return AlertColor;
            }
            if (mpeWatchData.unplanMaintSpStatus === 2 || mpeWatchData.opStartedLateStatus === 2 || mpeWatchData.opRunningLateStatus === 2 || mpeWatchData.sortplanWrongStatus === 2 || mpeWatchData.throughputStatus === 2) {
                return AlertColor;
            }
            if (mpeWatchData.unplanMaintSpStatus === 1 || mpeWatchData.opStartedLateStatus === 1 || mpeWatchData.opRunningLateStatus === 1 || mpeWatchData.sortplanWrongStatus === 1) {
                return WarningColor;
            }
            return RunningColor;
        }
    }
    catch (e) {

    }

}