
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
            Promise.all([loadMachineData(feature.properties, 'machinetable')]);
            //makea ajax call to get the employee details
            //$.ajax({
            //    url: '/api/Zone/' + feature.properties.id,
            //    type: 'GET',
            //    success: function (data) {
            //        //$('#content').html(data);
            //        sidebar.open('home');
            //    },
            //    error: function (error) {
            //        console.log(error);
            //    },
            //    faulure: function (fail) {
            //        console.log(fail);
            //    },
            //    complete: function (complete) {
            //        $('div[id=machine_div]').attr("data-id", feature.properties.id);
            //        $('div[id=machine_div]').css('display', 'block');
            //        $('div[id=ctstabs_div]').css('display', 'block');

            //        sidebar.open('home');


            //    }
            //});

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
        reject(new Error('No layer found with the given MPE Zone Id'));
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

async function loadMachineData(data, table) {
    try {
        hideSidebarLayerDivs();
        let mpeData = data.mpeRunPerformance;
        console.error(mpeData);
        $('span[name=mpeview]').empty();
        $('span[name=mpePerfomance]').empty();
        $('span[name=mpeSDO]').empty();
        $('div[id=machine_div]').attr("data-id", data.id);
        $('div[id=machine_div]').css('display', 'block');
        $('div[id=ctstabs_div]').css('display', 'block');
        $('button[name=machineinfoedit]').attr('id', data.id);
        $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + 'MPE/MPE.html?MPEStatus=' + data.name, style: 'color:white;' }).html("View").appendTo($('span[name=mpeview]'));
        $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + 'Reports/MPEPerformance.html?MPEStatus=' + data.name, style: 'color:white;' }).html("MPE Synopsis").appendTo($('span[name=mpePerfomance]'));
        if (!!mpeData.mpeGroup) {
            $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + 'MPESDO/MPESDO.html?MPEGroupName=' + mpeData.mpeGroup, style: 'color:white;' }).html("SDO View").appendTo($('span[name=mpeSDO]'));
        }
        if (/machinetable/i.test(table)) {
            $('div[id=dps_div]').css('display', 'none');
            let machinetop_Table = $('table[id=' + table + ']');
            let machinetop_Table_Body = machinetop_Table.find('tbody');
            machinetop_Table_Body.empty();
            machinetop_Table_Body.append(machinetop_row_template.supplant(formatmachinetoprow(data)));

            if (mpeData.hasOwnProperty("bin_full_bins")) {
                if (mpeData.bin_full_bins !== "") {
                    var result_style = document.getElementById('fullbin_tr').style;
                    result_style.display = 'table-row';
                    $("tr:visible").each(function (index) {
                        var curcolor = $(this).css("background-color");
                        if (curcolor === "" || curcolor === "rgba(0, 0, 0, 0)" || curcolor === "rgba(0, 0, 0, 0.05)") {
                            $(this).css("background-color", !!(index & 1) ? "rgba(0, 0, 0, 0)" : "rgba(0, 0, 0, 0.05)");
                        }
                    });
                }
            }

            if (mpeData.curOperationId === "918" || mpeData.curOperationId === "919") {
                Promise.all([LoadMachineDPSTables(dataproperties, "dpstable")]);
            }

            if (mpeData.hasOwnProperty("currentRunEnd")) {
                if (mpeData.currentRunEnd === "" || mpeData.currentRunEnd === "0") {
                    var runEndTR = document.getElementById('endtime_tr').style;
                    runEndTR.display = 'none';

                    $("tr:visible").each(function (index) {
                        var curcolor = $(this).css("background-color");
                        if (curcolor === "" || curcolor === "rgba(0, 0, 0, 0)" || curcolor == "rgba(0, 0, 0, 0.05)") {
                            $(this).css("background-color", !!(index & 1) ? "rgba(0, 0, 0, 0)" : "rgba(0, 0, 0, 0.05)");
                        }
                    });

                }
            }
            if (mpeData.hasOwnProperty("arsRecrej3")) {
                if (mpeData.arsRecrej3 !== "0" && mpeData.arsRecrej3 !== "") {
                    var arsrec_tr = document.getElementById('arsrec_tr').style;
                    arsrec_tr.display = 'table-row';
                    $("tr:visible").each(function (index) {
                        var curcolor = $(this).css("background-color");
                        if (curcolor === "" || curcolor === "rgba(0, 0, 0, 0)" || curcolor === "rgba(0, 0, 0, 0.05)") {
                            $(this).css("background-color", !!(index & 1) ? "rgba(0, 0, 0, 0)" : "rgba(0, 0, 0, 0.05)");
                        }
                    });
                }
            }
            if (mpeData.hasOwnProperty("sweepRecrej3")) {
                if (mpeData.sweepRecrej3 !== "0" && mpeData.sweepRecrej3 !== "") {
                    var sweeprec_tr = document.getElementById('sweeprec_tr').style;
                    sweeprec_tr.display = 'table-row';
                    $("tr:visible").each(function (index) {
                        var curcolor = $(this).css("background-color");
                        if (curcolor === "" || curcolor === "rgba(0, 0, 0, 0)" || curcolor === "rgba(0, 0, 0, 0.05)") {
                            $(this).css("background-color", !!(index & 1) ? "rgba(0, 0, 0, 0)" : "rgba(0, 0, 0, 0.05)");
                        }
                    });
                }
            }
            document.getElementById('machineChart_tr').style.backgroundColor = 'rgba(0,0,0,0)';
            //if (dataproperties.MPEWatchData.hasOwnProperty("hourly_data")) {
            //    GetMachinePerfGraph(dataproperties);
            //}
            //else {
            var mpgtrStyle = document.getElementById('machineChart_tr').style;
            mpgtrStyle.display = 'none';
            //}
            FormatMachineRowColors(mpeData);
        }
        sidebar.open('home');
    }
    catch (e) {
        // handle error
        console.error(e);
    }
}
let machinetop_row_template =
    '<tr data-id="{zoneId}"><td>{zoneType}</td><td>{zoneName}</td><td><span class="badge badge-pill {stateBadge}" style="font-size: 12px;">{stateText}</span></td></tr>' +
    '<tr id="SortPlan_tr"><td>OPN / Sort Plan</td><td colspan="2">{opNum} / {sortPlan}</td></tr>' +
    '<tr id="StartTime_tr"><td>Start</td><td colspan="2">{sortPlanStart}</td></tr>' +
    '<tr id="endtime_tr"><td>End</td><td colspan="2">{sortPlanEnd}</td></tr>' +
    '<tr id="EstComp_tr"><td>Estimated Completion</td><td colspan="2">{estComp}</td>/tr>' +
    '<tr><td>Pieces Fed / RPG Vol.</td><td>{peicesFed} / {rpgVol}</td>' +
    '<td style="display:none" >Yield:<span class="badge badge-pill badge badge-success" id="yieldNum" style="font-size: 12px;">{yieldCalNumber}</span></td > '
    + '</tr>' +
    '<tr id="Throughput_tr"><td>Throughput Act. / Exp.</td><td colspan="2">{throughput} / {expThroughput}</td></tr>' +
    '<tr id="fullbin_tr" style="display: none;"><td>Bin\'s That are Full</td><td colspan="2" style="white-space: normal; word-wrap:break-word;">{fullBins}</td></tr>' +
    '<tr id="arsrec_tr" style="display: none;"><td>ARS Recirc. Rejects</td><td colspan="2">{arsRecirc}</td></tr>' +
    '<tr id="sweeprec_tr" style="display: none;"><td>Sweep Recirc. Rejects</td><td colspan="2">{sweepRecirc}</td></tr>' +
    '<tr id="machineChart_tr"><td colspan="3"><canvas id="machinechart" width="470" height="200"></canvas></td></tr>';

function formatdpstoprow(properties) {
    return $.extend(properties, {
        dpssortplans: properties.sortplan_name_perf,
        piecesfedfirstpass: properties.pieces_fed_1st_cnt,
        piecesrejectedfirstpass: properties.pieces_rejected_1st_cnt,
        piecestosecondpass: properties.pieces_to_2nd_pass,
        piecesfedsecondpass: properties.pieces_fed_2nd_cnt,
        piecesrejectedsecondpass: properties.pieces_rejected_2nd_cnt,
        piecesremainingsecondpass: properties.pieces_remaining,
        timetocompleteactual: properties.time_to_comp_actual,
        timeleftsecondpassactual: properties.time_to_2nd_pass_actual,
        recomendedstartactual: properties.rec_2nd_pass_start_actual,
        completiondateTime: properties.time_to_comp_actual_DateTime
    });
}
let dpstop_row_template =
    '<tr><td>DPS Sort Plans</td><td>{dpssortplans}</td><td></td></tr>' +
    '<tr><td><b>First Pass</b></td><td></td><td></td></tr>' +
    '<tr><td>Pieces Fed</td><td>{piecesfedfirstpass}</td><td></td></tr>' +
    '<tr><td>Pieces Rejected</td><td>{piecesrejectedfirstpass}</td><td></td></tr>' +
    '<tr><td>Pieces To Second Pass</td><td>{piecestosecondpass}</td><td></td></tr>' +
    '<tr><td>Rec. 2nd Pass Start Time</td><td>{recomendedstartactual}</td><td></td></tr>' +
    '<tr><td><b>Second Pass</b></td><td></td><td></td></tr>' +
    '<tr><td>Pieces Fed</td><td>{piecesfedsecondpass}</td><td></td></tr>' +
    '<tr><td>Pieces Rejected</td><td>{piecesrejectedsecondpass}</td><td></td></tr>' +
    '<tr><td>Pieces Remaining</td><td>{piecesremainingsecondpass}</td><td></td></tr>' +
    '<tr><td>Est. Completion Time</td><td>{completiondateTime}</td><td></td></tr>'
    ;

async function LoadMachineDetails(selcValue) {
    try {
        if (polygonMachine.hasOwnProperty("_layers")) {
            $.map(polygonMachine._layers, function (layer, i) {
                if (layer.hasOwnProperty("feature")) {
                    if (layer.feature.properties.id === selcValue) {
                        var Center = new L.latLng(
                            (layer._bounds._southWest.lat + layer._bounds._northEast.lat) / 2,
                            (layer._bounds._southWest.lng + layer._bounds._northEast.lng) / 2);
                        map.setView(Center, 3);
                        if (/Machine/i.test(layer.feature.properties.zoneType)) {
                            LoadMachineTables(layer.feature.properties, 'machinetable');
                        }
                        return false;
                    }
                }
            });
        }
    } catch (e) {
        throw new Error(e.toString());
    }

}
function formatmachinetoprow(properties) {
    return $.extend(properties, {
        zoneId: properties.id,
        zoneName: properties.name,
        zoneType: properties.zoneType,
        sortPlan: Vaildatesortplan(properties.mpeRunPerformance),// ? properties.MPEWatchData.cur_sortplan : "N/A",
        opNum: properties.mpeRunPerformance.curOperationId,
        sortPlanStart: VaildateMPEtime(properties.mpeRunPerformance.currentRunStart),
        sortPlanEnd: Vaildatesortplan(properties.mpeRunPerformance) !== "N/A" ? "" : VaildateMPEtime(properties.mpeRunPerformance.currentRunEnd),
        peicesFed: properties.mpeRunPerformance.totSortplanVol,
        throughput: properties.mpeRunPerformance.curThruputOphr,
        rpgVol: properties.mpeRunPerformance.rpgEstVol,
        stateBadge: getstatebadge(properties),
        yieldCalNumber: getYiedCalNumber(properties.mpeRunPerformance),
        stateText: getstateText(properties),
        estComp: VaildateEstComplete(properties.mpeRunPerformance.rpg),// checkValue(properties.MPEWatchData.rpg_est_comp_time) ? properties.MPEWatchData.rpg_est_comp_time : "Estimate Not Available",
        rpgStart: luxon.DateTime.fromISO(properties.mpeRunPerformance.rpgStartDtm).toFormat("yyyy-LL-dd HH:mm:ss"),
        rpgEnd: luxon.DateTime.fromISO(properties.mpeRunPerformance.rpgEndDtm).toFormat("yyyy-LL-dd HH:mm:ss"),
        expThroughput: properties.mpeRunPerformance.expectedThroughput,
        fullBins: properties.mpeRunPerformance.binFullBins,
        arsRecirc: properties.mpeRunPerformance.arsRecrej3,
        sweepRecirc: properties.mpeRunPerformance.sweepRecrej3
    });
}
function getstatebadge(properties) {
    if (properties.hasOwnProperty("MPEWatchData")) {
        if (properties.MPEWatchData.hasOwnProperty("currentRunEnd")) {
            var endtime = properties.MPEWatchData.currentRunEnd == "0" ? "" : luxon.DateTime.fromISO(properties.MPEWatchData.currentRunEnd);

            var starttime = function () {
                if (data.length < 8) {
                    return " ";
                }
                return luxon.DateTime.fromISO(properties.MPEWatchData.currentRunStart)
            }
            var sortPlan = properties.MPEWatchData.curSortplan;

            if (starttime._isValid && !endtime._isValid) {
                if (sortPlan !== "") {
                    return "badge badge-success";
                }
                else {
                    return "badge badge-info";
                }
            }
            else if (!starttime._isValid && !endtime._isValid) {
                return "badge badge-info";
            }
            else if (starttime._isValid && endtime._isValid) {
                return "badge badge-info";
            }
        }
        else {
            return "badge badge-secondary";
        }
    }
    else {
        return "badge badge-secondary";
    }
}
function getstateText(properties) {
    if (properties.hasOwnProperty("mpeRunPerformance")) {
        if (properties.mpeRunPerformance.hasOwnProperty("currentRunEnd")) {
            //var endtime = moment(properties.MPEWatchData.current_run_end);
            
            var endtime = properties.mpeRunPerformance.currentRunEnd === "0" ? "" : luxon.DateTime.fromISO(properties.mpeRunPerformance.currentRunEnd);
            var starttime = properties.mpeRunPerformance.currentRunStart === "0" ? "" : luxon.DateTime.fromISO(properties.mpeRunPerformance.currentRunStart);
            var sortPlan = properties.mpeRunPerformance.curSortplan;

            if (sortPlan.length > 3) {
                return "Running";
            }
            else if (!starttime._isValid && !endtime._isValid) {
                return "Unknown";
            }
            else if (starttime._isValid && endtime._isValid) {
                return "Idle";
            }
        }
        else {
            return "No Data";
        }
    }
    else {
        return "No Data";
    }
}
function Vaildatesortplan(data) {
    try {
        if (!!data && data.curSortplan.length > 3) {
            return data.curSortplan;
        }
        else {
            return "N/A"
        }
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
function VaildateMPEtime(data) {
    try {
        if (data.length < 8) {
            return " ";
        }
        //how do use luxon to check if the date is valid
        let time = luxon.DateTime.fromFormat(data, "yyyy-LL-MM hh:mm:ss"); 
    
        if (time.isValid && time.year() === luxon.DateTime.local().year()) {
            return time.toFormat("yyyy-LL-MM hh:mm:ss");
        }
        else {
            return " ";
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}
function getYiedCalNumber(data) {
    if (!!data) {

        return 0;
    }
    else {
        return "NA";
    }
}
function VaildateEstComplete(estComplet) {

    try {
        let est = luxon.DateTime.fromISO(estComplet);
        if (est._isValid && est.year() === luxon.DateTime.local().year()) {
            return est.toFormat("MM/DD/YYYY hh:mm:ss A");
        }
        else {
            return "Estimate Not Available";
        }
    } catch (e) {
        throw new Error(e.toString());
    }
}
function FormatMachineRowColors(mpeRunPerformance, starttime) {
    let Throughput_tr_style = document.getElementById('Throughput_tr').style;
    let SortPlan_tr_style = document.getElementById('SortPlan_tr').style;
    let StartTime_tr_style = document.getElementById('StartTime_tr').style;
    let EstComp_tr_style = document.getElementById('EstComp_tr').style;
    let WarningColor = "rgba(255, 193, 7, 0.5)";
    let AlertColor = "rgba(220, 53, 69, 0.5)";
    try {
        if (mpeRunPerformance.throughputStatus === 3) {
            Throughput_tr_style.backgroundColor = AlertColor;
        }

        if (mpeRunPerformance.opStartedLateStatus === "2") {
            StartTime_tr_style.backgroundColor = AlertColor;
        }
        else if (mpeRunPerformance.opStartedLateStatus === "1") {
            StartTime_tr_style.backgroundColor = WarningColor;
        }
        if (mpeRunPerformance.opRunningLateStatus === "2") {
            EstComp_tr_style.backgroundColor = AlertColor;
        }
        else if (mpeRunPerformance.opRunningLateStatus === "1") {
            EstComp_tr_style.backgroundColor = WarningColor;
        }
        if (mpeRunPerformance.unplanMaintSpStatus === "2" || mpeRunPerformance.sortplanWrongStatus === "2") {
            SortPlan_tr_style.backgroundColor = AlertColor;
        }
        else if (mpeRunPerformance.unplanMaintSpStatus === "1" || mpeRunPerformance.sortplanWrongStatus === "1") {
            SortPlan_tr_style.backgroundColor = WarningColor;
        }
    }
    catch (e) {

        throw new Error(e.toString());
    }
}