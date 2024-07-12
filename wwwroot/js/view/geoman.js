$('#Zone_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .val('')
        .end()
        .find("span[class=text]")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change()
        .end();
});
//remove layers
$('#Remove_Layer_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .val('')
        .end()
        .find("span[class=text]")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change()
        .end();
});
$('#Remove_Layer_Modal').on('shown.bs.modal', function () {
});

async function init_geoman_editing() {
    let draw_options = {
        position: 'bottomright',
        oneBlock: false,
        snappingOption: false,
        drawRectangle: true,
        drawMarker: true,
        drawPolygon: true,
        drawPolyline: false,
        drawCircleMarker: false,
        drawCircle: false,
        editMode: false,
        cutPolygon: false,
        dragMode: false,
        removalMode: true
    };
    OSLmap.pm.addControls(draw_options);
    OSLmap.on('pm:create', (e) => {
        hideSidebarLayerDivs();
        $('div[id=layer_div]').css('display', 'block');
        $('select[name=zone_type]').prop('disabled', false);
        VaildateForm("");
        $('select[name=zone_type]').empty();
        $('<option/>').val("").html("").appendTo('select[id=zone_type]');
        if (/(Polygon|Rectangle)/i.test(e.shape)) {
            if (/(Polygon)/i.test(e.shape)) {
                $('<option/>').val("Area").html("Area Zone").appendTo('select[id=zone_type]');
                $('<option/>').val("AGVLocation").html("AGV Location Zone").appendTo('select[id=zone_type]');
                $('<option/>').val("MPE").html("MPE Zone").appendTo('select[id=zone_type]');
                $('<option/>').val("ViewPorts").html("View Ports").appendTo('select[id=zone_type]');
                $('<option/>').val("Bullpen").html("Bullpen Zone").appendTo('select[id=zone_type]')
            }
            if (/(Rectangle)/i.test(e.shape)) {
                $('<option/>').val("Bin").html("Bin Zone").appendTo('select[id=zone_type]');
                $('<option/>').val("DockDoor").html("Dock Door Zone").appendTo('select[id=zone_type]');
            }
            CreateZone(e);
            sidebar.open('home');
        }
        if (e.shape === "Marker") {
            $('select[name=zone_type]').empty();
            $('<option/>').val("CameraMarker").html("Camera").appendTo('select[id=zone_type]');
            CreateCamera(e);
            sidebar.open('home');
        }
        $('button[id=zonecloseBtn][type=button]').off().on('click', function () {
            sidebar.close();
            e.layer.remove();
        })
    });
    OSLmap.on('pm:edit', (e) => {
        if (e.shape === 'Marker') {
            VaildateForm("");
            e.layer.bindPopup().openPopup();
        }
    });
    OSLmap.on('pm:remove', (e) => {
        if (e.shape === 'Marker') {
            VaildateForm("");
            RemoveMarkerItem(e);
        }
        else {
            VaildateForm("");
            RemoveZoneItem(e);
        }
    });
    //zone type
    $('select[name=zone_type]').change(function () {
        if (!checkValue($('select[name=zone_type]').val())) {
            $('select[name=zone_type]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_zone_type]').text("Please Select Type");
            VaildateForm('');
        }
        else {
            $('select[name=zone_type]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_zone_type]').text("");
            VaildateForm($('select[name=zone_type] option:selected').val());
        }
        if (!checkValue($('select[name=zone_select_name]').val())) {
            $('select[name=zone_select_name]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_zone_select_name]').text("Please Select Name");
        }
        else {
            $('select[name=zone_select_name]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_zone_select_name]').text("");
        }
    });
    $('select[id=zone_select_name]').change(function () {
        if (/Not Listed$/.test($('select[name=zone_select_name] option:selected').val())) {
            $('#manual_row').css('display', 'block');
            if (!checkValue($('input[type=text][name=manual_name]').val())) {
                $('input[type=text][name=manual_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
                $('span[id=error_manual_name]').text("Please Enter Name");
            }
            else {
                $('input[type=text][name=manual_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
                $('span[id=error_manual_name]').text("");
            }
            if (!checkValue($('input[type=text][name=manual_number]').val())) {
                $('input[type=text][name=manual_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
                $('span[id=error_manual_number]').text("Please Enter Number");
            }
            else {
                $('input[type=text][name=manual_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
                $('span[id=error_manual_number]').text("");
            }
        }
        else {
            $('#manual_row').css('display', 'none');
        }
        if ($('select[name=zone_select_name] option:selected').val() === '') {
            $('button[id=zonesubmitBtn]').prop('disabled', true);
            $('select[id=zone_select_name]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_zone_select_name]').text("Please Select Name");
        }
        else {
            $('select[id=zone_select_name]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_zone_select_name]').text("");
            if (/Camera/i.test($('select[name=zone_type] option:selected').val())) {

                enableCameraSubmit();
            }
            else if (/Bin/i.test($('select[name=zone_type] option:selected').val())) {

                enableBinZoneSubmit();
            }
            else {
                enableZoneSubmit();
            }
        }

    });
    //bins name
    $('textarea[id=bin_bins]').keyup(function () {
        if (!checkValue($('textarea[id=bin_bins]').val())) {
            $('textarea[id=bin_bins]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_bin_bins]').text("Please Enter Bin Numbers");
        }
        else {
            $('textarea[id=bin_bins]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_bin_bins]').text("");
        }
        enableBinZoneSubmit();
    });


    //Camera URL
    $('select[name=cameraLocation]').change(function () {
        if (!checkValue($('select[name=cameraLocation]').val())) {
            $('select[name=cameraLocation]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_cameraLocation]').text("Please Select Type");
            ;
        }
        else {
            $('select[name=cameraLocation]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_cameraLocation]').text("");
        }
        enableCameraSubmit();
    });
    //manual type keyup
    $('input[type=text][name=manual_name]').keyup(function () {
        if (!checkValue($('input[type=text][name=manual_name]').val())) {
            $('input[type=text][name=manual_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_manual_name]').text("Please Enter Machine Name");
        }
        else {
            $('input[type=text][name=manual_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_manual_name]').text("");
        }
        if ($('select[name=zone_type] option:selected').val() === "Camera") {

            enableCameraSubmit();
        }
        else if ($('select[name=zone_type] option:selected').val() === "Bin") {

            enableBinZoneSubmit();
        }
        else {
            enableZoneSubmit();
        }
    });
    $('input[type=text][name=manual_number]').keyup(function () {
        if (!checkValue($('input[type=text][name=manual_number]').val())) {
            $('input[type=text][name=manual_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_manual_number]').text("Please Enter Machine Name");
        }
        else {
            $('input[type=text][name=manual_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_manual_number]').text("");
        }
        if ($('select[name=zone_type] option:selected').val() === "Camera") {

            enableCameraSubmit();
        }
        else if ($('select[name=zone_type] option:selected').val() === "Bin") {

            enableBinZoneSubmit();
        }
        else {
            enableZoneSubmit();
        }
    });
}
function enableBinZoneSubmit() {
    if ($('select[name=zone_type]').hasClass('is-valid') &&
        $('select[id=zone_select_name]').hasClass('is-valid') &&
        $('textarea[id=bin_bins]').hasClass('is-valid')
    ) {
        $('button[id=zonesubmitBtn]').prop('disabled', false);
    }
    else {
        $('button[id=zonesubmitBtn]').prop('disabled', true);
    }
}
function enableCameraSubmit() {
    if ($('select[name=cameraLocation]').hasClass('is-valid') &&
        $('select[name=zone_type]').hasClass('is-valid')
    ) {
        $('button[id=zonesubmitBtn]').prop('disabled', false);
    }
    else {
        $('button[id=zonesubmitBtn]').prop('disabled', true);
    }
}
function enableZoneSubmit() {
    if ($('select[name=zone_type]').hasClass('is-valid') &&
        /Not Listed$/i.test($('select[name=zone_select_name] option:selected').val()) &&
        $('input[type=text][name=manual_name]').hasClass('is-valid') &&
        $('input[type=text][name=manual_number]').hasClass('is-valid')) {
        $('button[id=zonesubmitBtn]').prop('disabled', false);
    }
    else if ($('select[name=zone_type]').hasClass('is-valid') && $('select[name=zone_select_name]').hasClass('is-valid') && !/(Not Listed$)/i.test($('select[name=zone_select_name] option:selected').val())) {
        $('button[id=zonesubmitBtn]').prop('disabled', false);
    }
    else {
        $('button[id=zonesubmitBtn]').prop('disabled', true);
    }
}
function CreateZone(newlayer) {
    try {
        //map.setView(newlayer.layer._bounds.getCenter());
        var togeo = newlayer.layer.toGeoJSON();
        var geoProp = {
            zoneType: "",
            floorid: baselayerid,
            name: "",
            bins: "",
            visible: true
        }
        $('button[id=zonesubmitBtn][type=button]').off().on('click', function () {
            togeo.properties = geoProp;
            togeo.properties.zoneType = $('select[name=zone_type] option:selected').val();
            if (/Bin/i.test($('select[name=zone_type] option:selected').val())) {
                togeo.properties.bins = $('textarea[id="bin_bins"]').val();
            }
            if (/Not Listed$/.test($('select[name=zone_select_name] option:selected').val())) {
                togeo.properties.name = $('input[id=manual_name]').val() + "-" + $('input[id=manual_number]').val().padStart(3, '0');
            }
            else {
                togeo.properties.name = $('select[name=zone_select_name] option:selected').val();
            }
            if (!$.isEmptyObject(togeo)) {
                //make a ajax call to get the employee details
                $.ajax({
                    url: SiteURLconstructor(window.location) + '/api/Zone/Add',
                    data: JSON.stringify(togeo),
                    contentType: 'application/json',
                    type: 'POST',
                    success: function (data) {
                        Promise.all([init_geoZone(data)]);
                        setTimeout(function () { sidebar.close(); }, 500);
                    },
                    error: function (error) {
                        $('span[id=error_zonesubmitBtn]').text(error);
                        $('button[id=zonesubmitBtn]').prop('disabled', false);
                        //console.log(error);
                    },
                    faulure: function (fail) {
                        console.log(fail);
                    },
                    complete: function (complete) {
                        newlayer.layer.remove();
                    }
                });
            }
        });

    } catch (e) {

    }
}
function CreateCamera(newlayer) {
    VaildateForm("Camera");
    sidebar.open('home');
    var togeo = newlayer.layer.toGeoJSON();
    map.setView(newlayer.layer._latlng, 3);
    var geoProp = {
        name: "",
        floorid: baselayerid,
        tagType: "",
        visible: true
    }

    $('button[id=zonesubmitBtn][type=button]').off().on('click', function () {
        togeo.properties = geoProp;
        togeo.properties.name = $('select[name=cameraLocation] option:selected').val();
        togeo.properties.tagType = $('select[name=zone_type] option:selected').val();
        //Camera Direction
        togeo.properties.cameraDirection = $('input[id=cameraDirection]').val();
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Tag/GetTagTypeList?TagType=' + $('select[name=zone_type] option:selected').val(),
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Machine Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })
                }
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });
        //$.connection.FOTFManager.server.addMarker(JSON.stringify(togeo)).done(function (Data) {
        //    if (!$.isEmptyObject(Data)) {
        //        setTimeout(function () { sidebar.close('home'); }, 500);
        //        newlayer.layer.remove();
        //    }
        //    else {
        //        newlayer.layer.remove();
        //    }
        //});
    });
}
function RemoveZoneItem(removeLayer) {
    try {
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/Delete?id=' + removeLayer.layer.feature.properties.id,
            contentType: 'application/json',
            type: 'DELETE',
            success: function (data) {
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                removeLayer.layer.remove();
                //console.log(complete);
            }
        });
    } catch (e) {
        console.log();
    }
}
function RemoveMarkerItem(removeLayer) {
    try {
        sidebar.close();
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Machine Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })
                }
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });
        //if (removeLayer.layer.feature.properties.Tag_Type === "CameraMarker") {
        //    fotfmanager.server.removeCameraMarker(removeLayer.layer.feature.properties.id).done(function (Data) {
        //        //if modal is open close it
        //        if (($('#Camera_Modal').data('bs.modal') || {})._isShown) {
        //            $('#Camera_Modal').modal('hide');
        //        }
        //        setTimeout(function () { $("#Remove_Layer_Modal").modal('hide'); $('#Camera_Modal').modal('hide'); }, 500);
        //    });
        //}
        //fotfmanager.server.removeMarker(removeLayer.layer.feature.properties.id).done(function (Data) {

        //    setTimeout(function () { $("#Remove_Layer_Modal").modal('hide'); }, 500);
        //});
    } catch (e) {
        console.log();
    }
}
function removeFromMapView(id) {
    $.map(OSLmap._layers, function (layer, i) {
        if (layer.hasOwnProperty("feature")) {
            if (layer._leaflet_id === id) {
                OSLmap.removeLayer(layer)
            }
        }
    });
}
function VaildateForm(FormType) {
    $('input[type=text][name=manual_name]').val("");
    $('input[type=text][name=manual_number]').val("");
    $('select[name=zone_type]').prop('disabled', false);
    $('div[id=div_zone_select_name]').css('display', 'none');
    $('select[id=zone_select_name]').empty();
    $('<option/>').val("").html("").appendTo('select[id=zone_select_name]');
    $('select[id=zone_select_name]').val("");
    $('#camerainfo').css("display", "none");
    $('#binzoneinfo').css("display", "none");
    $('#manual_row').css('display', 'none');
    if (!checkValue($('select[name=zone_type]').val())) {
        $('select[name=zone_type]').removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_zone_type]').text("Please Select Zone Type");
    }
    else {
        $('select[name=zone_type]').removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_zone_type]').text("");
    }
    if (/(MPE)/i.test(FormType)) {
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Machine Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })
                }
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
    }
    else if (/(DockDoor)/i.test(FormType)) {
        $('textarea[id="bin_bins"]').val("");

        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Dock Door Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })
                }
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });
        //fotfmanager.server.getDockDoorList().done(function (DockDoordata) {
        //    if (DockDoordata.length > 0) {
        //        //sort 
        //        DockDoordata.sort(SortByNumber);
        //        DockDoordata.push('**Dock Door Not Listed');
        //        $.each(DockDoordata, function () {
        //            $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
        //        })
        //    }
        //    else {
        //        DockDoordata.push('**Dock Door Not Listed');
        //        $.each(DockDoordata, function () {
        //            $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
        //        })
        //    }
        //});
    }
    else if (/(Bin)/i.test(FormType)) {

        $('#binzoneinfo').css("display", "block");
        $('textarea[id="bin_bins"]').val("");
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Machine Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })
                }
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //  console.log(complete);
            }
        });
        if (!checkValue($('textarea[id=bin_bins]').val())) {
            $('textarea[id=bin_bins]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_bin_bins]').text("Please Bin Numbers");
        }
        else {
            $('textarea[id=bin_bins]').removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_bin_bins]').text("");
        }
        enableBinZoneSubmit();
    }
    else if (/(Bullpen)/i.test(FormType)) {

        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort();
                    mpedata.push('**Bullpen Not Listed');
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })

                    $('select[name=zone_type]').prop('disabled', true);
                    enableZoneSubmit();
                }
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });
        //fotfmanager.server.getSVZoneNameList().done(function (svdata) {

        //    if (svdata.length > 0) {
        //        //sort 
        //        svdata.sort(SortByName);
        //        svdata.push('**Bullpen Not Listed');
        //        $.each(svdata, function () {
        //            $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
        //        })
        //    }
        //    $('select[name=zone_type]').prop('disabled', true);
        //    enableZoneSubmit();
        //});
    }
    else if (/(AGVLocation)/i.test(FormType)) {
        $('<option/>').val('**AGVLocation Not Listed').html('**AGVLocation Not Listed').appendTo('select[id=zone_select_name]');
        enableZoneSubmit();
    }

    if (/(Camera)/i.test(FormType)) {
        //fotfmanager.server.getCameraList().done(function (cameradata) {
        //    if (cameradata.length > 0) {
        //        $('select[id=cameraLocation]').empty();
        //        $('<option/>').val("").html("").appendTo('select[id=cameraLocation]');
        //        $.each(cameradata, function () {
        //            $('<option/>').val(this.CAMERA_NAME).html(this.CAMERA_NAME + "/" + this.DESCRIPTION).appendTo('select[id=cameraLocation]');
        //        })
        //    }
        //});
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?ZoneType=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    //sort 
                    mpedata.sort(SortByName);
                    mpedata.push('**Bullpen Not Listed');
                    $.each(svdata, function () {
                        $('<option/>').val(this).html(this).appendTo('select[id=zone_select_name]');
                    })

                    $('select[name=zone_type]').prop('disabled', true);
                    enableZoneSubmit();
                } s
            },
            error: function (error) {

                console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                //console.log(complete);
            }
        });
        $('#camerainfo').css("display", "block");
        $('#binzoneinfo').css("display", "none");
        $('select[name=zone_type]').prop('disabled', true);
        //Camera URL
        if ($('select[name=cameraLocation]').val() === "") {
            $('select[name=cameraLocation]').removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_cameraLocation]').text("Please Select Camera URL");
        } else {
            $('input[type=text][name=cameraLocation]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_cameraLocation]').text("");
        }

        //Camera Direction display arrow
        $('#cameraDirectionvalue').html($('input[id=cameraDirection]').val() + '&#176;');
        document.getElementById("cameraDirectionArrow").style.transform = 'rotate(' + $('input[id=cameraDirection]').val() + 'deg)';
        $('input[id=cameraDirection]').on('input change', () => {
            $('#cameraDirectionvalue').html($('input[id=cameraDirection]').val() + '&#176;');
            document.getElementById("cameraDirectionArrow").style.transform = 'rotate(' + $('input[id=cameraDirection]').val() + 'deg)';
        });

        enableCameraSubmit();
    }
    else {
        $('div[id=div_zone_select_name]').css('display', 'block');
    }

}