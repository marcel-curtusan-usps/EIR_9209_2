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
let isBinZoneRemoved = false;

let flash = "";
let geoZoneBin = new L.GeoJSON(null, {
    style: function (feature) {
        return {
            weight: 1,
            opacity: 1,
            color: '#3573b1',
            fillOpacity: 0.2,
            fillColor: '#989ea4',
            label: feature.properties.name,
            lastOpacity: 0.2
        };
    },
    onEachFeature: function (feature, layer) {

        layer.zoneId = feature.properties.id;
        if (!isBinZoneRemoved) {
            layer.on('click', function (e) {
                if (e.sourceTarget.hasOwnProperty("_content")) {
                    OSLmap.setView(e.sourceTarget._latlng, 4);
                }
                else {
                    OSLmap.setView(e.sourceTarget.getCenter(), 4);
                }  
                //set to the center of the polygon.
                $('input[type=checkbox][name=followvehicle]').prop('checked', false).change();
              
                if ((' ' + document.getElementById('sidebar').className + ' ').indexOf(' ' + 'collapsed' + ' ') <= -1) {
                    if ($('#zoneselect').val() === feature.properties.id) {
                        sidebar.close('home');
                    }
                    else {
                        sidebar.open('home');
                    }
                }
                else {
                    sidebar.open('home');
                }
                LoadBinZoneTables(feature.properties);
            });
        }
        layer.bindTooltip("", {
            permanent: true,
            interactive: true,
            direction: "center",
            opacity: 1,
            className: 'dockdooknumber ' + flash
        }).openTooltip();

    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let geoZoneBINoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneBINoverlayLayer, "Bin Zones");
geoZoneBin.addTo(geoZoneBINoverlayLayer);

connection.on("addBinzone", async (zoneDate) => {
    addBinFeature(zoneDate);

});
connection.on("deleteBinzone", async (zoneDate) => {
    isBinZoneRemoved = true;
    deleteBinFeature(zoneDate);

});
connection.on("updateBinzone", async (zoneDate) => {
    //need to update bin zone
    updateBinFeature(zoneDate);
});
async function findBinZoneLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneBin.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given MPE Bin Zone Id'));
    });
}
async function init_geoZoneBin() {
    try {
        //load cameras
        //connection.invoke("GetGeoZones","Bin").then(function (data) {
        //    for (let i = 0; i < data.length; i++) {
        //        Promise.all([addBinFeature(data[i])]);
        //    }
        //}).catch(function (err) {
        //    // handle error
        //    console.error(err);
        //});
        const binZonedata = await $.ajax({
            url: `${SiteURLconstructor(window.location)}/api/Zone/ZoneType?type=Bin`,
            contentType: 'application/json',
            type: 'GET',
            success: function (data) {
                for (let i = 0; i < data.length; i++) {
                    Promise.all([addDockDoorFeature(data[i])]);
                }
            }
        });
        $(document).on('change', '.leaflet-control-layers-selector', function (e) {
            let sp = this.nextElementSibling;
            if (/^(Bin Zones)/ig.test(sp.innerHTML.trim())) {
                if (this.checked) {
                    connection.invoke("JoinGroup", "Bin").catch(function (err) {
                        return console.error(err.toString());
                    });
                }
                else {
                    connection.invoke("LeaveGroup", "Bin").catch(function (err) {
                        return console.error(err.toString());
                    });
                }
            }

        });
        connection.invoke("JoinGroup", "Bin").catch(function (err) {
            return console.error(err.toString());
        });

    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function addBinFeature(data) {
    try {
        await findBinZoneLeafletIds(data.properties.id)
            .then(leafletIds => {

            })
            .catch(error => {
                geoZoneBin.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function updateBinFeature(data) {
    try {
        await findBinZoneLeafletIds(data.zoneId)
            .then(leafletIds => {
                if (data.binFullStatus > 0) {
                    geoZoneBin._layers[leafletIds]._tooltip._container.classList.add('doorflash');
                    geoZoneBin._layers[leafletIds].setStyle({
                        weight: 1,
                        opacity: 1,
                        fillOpacity: 0.5,
                        fillColor: "#ff8855",
                        lastOpacity: 0.5
                    });
                }
                else {
                    geoZoneMPEBin._layers[layerindex]._tooltip._container.classList.remove('doorflash');
                    geoZoneMPEBin._layers[layerindex].setStyle({
                        weight: 1,
                        opacity: 1,
                        fillOpacity: 0.2,
                        fillColor: "#989ea4",
                        lastOpacity: 0.2
                    });
                }
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function deleteBinFeature(data) {
    try {
        await findBinZoneLeafletIds(data.properties.id)
            .then(leafletIds => {
                geoZoneBin.removeLayer(leafletIds);
            })
            .catch(error => {

            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function LoadBinZoneTables(data) {
    try {
        hideSidebarLayerDivs();
        $('div[id=area_div]').attr("data-id", data.id);
        $('div[id=area_div]').css('display', 'block');
        czzonetop_Table_Body.empty();
        czzonetop_Table_Body.append(czzonetop_row_template.supplant(formatczzonetoprow(data)));
    } catch (e) {
        console.log(e)
    }
}
let czzonetop_Table = $('table[id=areazonetoptable]');
let czzonetop_Table_Body = czzonetop_Table.find('tbody');
let czzonetop_row_template = '<tr data-id="{zoneId}"><td style="width: 22%;">Bin Zone Name</td><td>{zoneName}</td></tr>' +
    '<tr><td>Bins Configured</td><td>{AssignedBins}</td></tr>' +
    '<tr><td>Full Bins</td><td>{fullbins}</td></tr>';
function formatczzonetoprow(properties) {
    return $.extend(properties, {
        zoneId: properties.id,
        zoneName: properties.name,
        AssignedBins: properties.bins,
        fullbins: properties.mpeRunPerformance.binFullBins
    });
}