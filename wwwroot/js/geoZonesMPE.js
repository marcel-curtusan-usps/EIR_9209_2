const MPETable = "machinetable";
const TargetTable = "mpetargettable";
const StandardTable = "mpestandardtable";
let MpeTargetTable = [];
let MpetargetEditor = [];
let tourlist = [1, 2, 3];
let hoursOptions = {
    "00:00": "00:00",
    "01:00": "01:00",
    "02:00": "02:00",
    "03:00": "03:00",
    "04:00": "04:00",
    "05:00": "05:00",
    "06:00": "06:00",
    "07:00": "07:00",
    "08:00": "08:00",
    "09:00": "09:00",
    "10:00": "10:00",
    "11:00": "11:00",
    "12:00": "12:00",
    "13:00": "13:00",
    "14:00": "14:00",
    "15:00": "15:00",
    "16:00": "16:00",
    "17:00": "17:00",
    "18:00": "18:00",
    "19:00": "19:00",
    "20:00": "20:00",
    "21:00": "21:00",
    "22:00": "22:00",
    "23:00": "23:00",
};
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
    $('div[id=machine_div]').attr("data-id", "");
});
//on open set rules
$('#Zone_Modal').on('shown.bs.modal', function () {
    $('span[id=error_machinesubmitBtn]').text("");
    $('button[id=machinesubmitBtn]').prop('disabled', true);
    //Request Type Keyup
    $('input[type=text][name=machine_name]').on("keyup", () => {
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
    //check zone type
    $('select[name=zone_Type]').on("change", () => {
        if ($('select[name=zone_Type]').val() === "") {
            $('select[name=zone_Type]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_zone_Type]').text("Pleas select a Zone");
        }
        else {
            $('select[name=zone_Type]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_zone_Type]').text("");
        }
        enablezoneSubmit();
    });
    //machine_zone_select_name
    $('select[id=machine_zone_select_name]').on("change", () => {
        if ($('select[name=machine_zone_select_name] option:selected').val() === "") {
            $('select[name=machine_zone_select_name]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_machine_zone_name]').text("Pleas select a MPE Name");
        }
        else {
            $('select[name=machine_zone_select_name]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_machine_zone_name]').text("");
        }
        enablezoneSubmit();
    });
    if ($('select[name=zone_Type]').val() === "") {
        $('select[name=zone_Type]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_zone_Type]').text("Pleas select a Zone");
    }
    else {
        $('select[name=zone_Type]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_zone_Type]').text("");
    }

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
    $('input[type=text][name=machine_number]').on("keyup", () => {
        if (!checkValue($('input[type=text][name=machine_number]').val())) {
            $('input[type=text][name=machine_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=error_machine_number]').text("Please Enter Number");
        }
        else {
            $('input[type=text][name=machine_number]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=error_machine_number]').text("");
        }

        enablezoneSubmit();
    });
    //ipaddress 
    $('input[type=text][name=machine_ip]').on("keyup", () => {
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
    $('select[name=zonePayLocationColor]').on("change", () => {
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
    //Request Type Validation
    if (!checkValue($('input[type=text][name=machine_number]').val())) {
        $('input[type=text][name=machine_number]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_machine_number]').text("Please Enter Number");
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
    $('input[type=text][name=zone_ldc]').on("keyup", () => {
        if (checkValue($('input[type=text][name=zone_ldc]').val())) {
            $('input[type=text][name=zone_ldc]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
            $('span[id=errorzone_ldc]').text("");
        }
        else {
            $('input[type=text][name=zone_ldc]').css("border-color", "#D3D3D3").removeClass('is-invalid').removeClass('is-valid');
            $('span[id=errorzone_ldc]').text("");
        }
    });
    //zone RejectBins Validation
    if (!checkValue($('input[type=text][name=mpeRejectBins]').val())) {
        $('input[type=text][name=mpeRejectBins]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=errormpeRejectBins]').text("");
    }
    else {
        $('input[type=text][name=mpeRejectBins]').css("border-color", "#2eb82e").removeClass('is-invalid').removeClass('is-valid');
        $('span[id=errormpeRejectBins]').text("");
    }
    //Request RejectBins Keyup
    $('input[type=text][name=mpeRejectBins]').on("keyup", () => {
        if (!checkValue($('input[type=text][name=mpeRejectBins]').val())) {
            $('input[type=text][name=mpeRejectBins]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=errormpeRejectBins]').text("");
        }
        else {
            $('input[type=text][name=mpeRejectBins]').css("border-color", "#2eb82e").removeClass('is-invalid').removeClass('is-valid');
            $('span[id=errormpeRejectBins]').text("");
        }
        enablezoneSubmit();
    });
    //zone ExternalUrl Validation
    if (!checkValue($('input[type=text][name=mpeExternalUrl]').val())) {
        $('input[type=text][name=mpeExternalUrl]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
        $('span[id=errormpeExternalUrl]').text("Please Enter URL");
    }
    else {
        $('input[type=text][name=mpeExternalUrl]').css("border-color", "#2eb82e").removeClass('is-invalid').addClass('is-valid');
        $('span[id=errormpeExternalUrl]').text("");
    }
    //Request ExternalUrl Keyup
    $('input[type=text][name=mpeExternalUrl]').on("keyup", () => {
        if (!checkValue($('input[type=text][name=mpeExternalUrl]').val())) {
            $('input[type=text][name=mpeExternalUrl]').css("border-color", "#FF0000").removeClass('is-valid').addClass('is-invalid');
            $('span[id=errormpeExternalUrl]').text("");
        }
        else {
            $('input[type=text][name=mpeExternalUrl]').css("border-color", "#2eb82e").removeClass('is-invalid').removeClass('is-valid');
            $('span[id=errormpeExternalUrl]').text("");
        }
        enablezoneSubmit();
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
    $('input[type=text][name=zone_paylocation]').on("keyup", () => {
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

let isMPEZoneRemoved = false;
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
            if (e.sourceTarget.hasOwnProperty("_content")) {
                OSLmap.setView(e.sourceTarget._latlng, 4);
            }
            else {
                OSLmap.setView(e.sourceTarget.getCenter(), 4);
            }  
            Promise.all([loadMachineData(feature.properties, MPETable)]);

        });

        layer.bindTooltip(feature.properties.name + "<br/>" + "Staffing: " + (feature.properties.hasOwnProperty("CurrentStaff") ? feature.properties.CurrentStaff : "0"), {
            permanent: true,
            interactive: true,
            direction: "center",
            opacity: 1,
            className: 'location '
        }).openTooltip();

    },
    filter: function (feature) {
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
        reject(new Error('No layer found with the given MPE Zone Id: ' + zoneId));
    });
}
async function init_geoZoneMPE(floorId) {
  try {
    //int mpe standard table
    creatMpeStandardDataTable(StandardTable);
    for (var touri in tourlist) {
      createTargetDataTable(TargetTable + tourlist[touri], tourlist[touri]);
    }
    //load MPE Zones
    await $.ajax({
      url: `${SiteURLconstructor(
        window.location
      )}/api/Zone/ZonesTypeByFloorId?floorId=${floorId}&type=MPE`,
      contentType: "application/json",
      type: "GET",
      success: function (data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addMPEFeature(data[i])]);
        }
      },
    });
    $(document).on("change", ".leaflet-control-layers-selector", async function (_e) {
      let sp = this.nextElementSibling.innerHTML.trim();
      if (/^(MPE Zones)$/gi.test(sp)) {
              await handleGroupChange(this.checked, sp);
      }
    });
    await addGroupToList('MPE');
    await addGroupToList('MPETartgets');
    if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
      $("button[name=machineinfoedit]")
        .off()
        .on("click", function () {
          /* close the sidebar */
          sidebar.close();
          var id = $(this).attr("id");
          if (checkValue(id)) {
            Promise.all([Edit_Machine_Info(id)]);
          }
        });
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
connection.on("addMPEzone", async (zoneDate) => {
    addMPEFeature(zoneDate);

});
connection.on("deleteMPEzone", async (zoneDate) => {
    isMPEZoneRemoved = true;
    deleteMPEFeature(zoneDate);

});
connection.on("updateMPEzone", async (mpeZonedata) => {
    try {
    await findMpeZoneLeafletIds(mpeZonedata.properties.id)
        .then(leafletIds => {
            // Assuming mpeZonedata is the object containing the new properties
            const newProperties = mpeZonedata.properties;

            // Loop through the properties and update the values
            for (const key in newProperties) {
                if (key !== 'mpeRunPerformance') {
                    
                    if (key == 'name') {
                        // Check if the property name is different and update the tooltip
                        geoZoneMPE._layers[leafletIds].feature.properties[key] = newProperties[key];
                        geoZoneMPE._layers[leafletIds].setTooltipContent(
                            newProperties[key] + "<br/>" +
                            "Staffing: " +
                            (geoZoneMPE._layers[leafletIds].feature.properties.hasOwnProperty("CurrentStaff")
                                ? geoZoneMPE._layers[leafletIds].feature.properties.CurrentStaff
                                : "0")
                        );
                    }
                    else {

                        geoZoneMPE._layers[leafletIds].feature.properties[key] = newProperties[key];
                    }
                }
            }
        });
    }
    catch (e) {
        throw new Error(e.toString());
    }

});
connection.on("updateMPEzoneTartgets", async (data) => {
    if ($('#Zone_Modal').hasClass('show')) {
        let mpeId = "";
        if ($('select[name=machine_zone_select_name] option:selected').val() !== '') {
            let selectedMachine = $('select[name=machine_zone_select_name] option:selected').val().split(/-(?=[^-]*$)/);
            mpeName = selectedMachine[0];
            mpeNumber = selectedMachine[1];
            mpeId = mpeName + "-" + mpeNumber.padStart(3, '0');
        }
        else {
            mpeName = $('input[type=text][name=machine_name]').val();
            mpeNumber = $('input[type=text][name=machine_number]').val();
            mpeId = mpeName + "-" + mpeNumber.padStart(3, '0');
        }
     
        if (data[0].mpeId === mpeId) {
            // Update the cell data in the DataTable
            for (var tourNumber in tourlist) {
                loadTargetsHourDataTable(tourlist[tourNumber], data);
            }
        }
    }
});
connection.on("updatempezonerunperformance", async (data) => {

    await findMpeZoneLeafletIds(data.id)
        .then(leafletIds => {
            geoZoneMPE._layers[leafletIds].feature.properties.mpeRunPerformance = data;
            geoZoneMPE._layers[leafletIds].setStyle({
                weight: 1,
                opacity: 1,
                fillOpacity: 0.2,
                fillColor: GetMacineBackground(data),
                lastOpacity: 0.2
            });
            // Check if the sidebar with ID 'home' is open
            if ($('#home').hasClass('active')) {
                if ($('div[id=machine_div]').attr("data-id") === data.id) {
                    Promise.all([loadMachineData(geoZoneMPE._layers[leafletIds].feature.properties, MPETable)]);
                }
            }
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
async function deleteMPEFeature(data) {
    try {
        await findMpeZoneLeafletIds(data.properties.id)
            .then(leafletIds => {
                geoZoneMPE.removeLayer(leafletIds);
            })
            .catch(error => {

            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
function enablezoneSubmit() {
    if ($('div[id="machine_select_row"]').is(":visible")
        && $('select[name=machine_zone_select_name]').hasClass('is-valid')) {
        $('button[id=machinesubmitBtn]').prop('disabled', false);
    }
    else if ($('div[id="machine_manual_row"]').is(":visible") &&
        $('input[type=text][name=machine_name]').hasClass('is-valid') &&
        $('input[type=text][name=machine_number]').hasClass('is-valid')) {
        $('button[id=machinesubmitBtn]').prop('disabled', false);
    }
    else {
        $('button[id=machinesubmitBtn]').prop('disabled', true);
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
        $('span[name=mpeview]').empty();
        $('span[name=mpeHRview]').empty();
        $('span[name=mpePerfomance]').empty();
        $('span[name=mpeSDO]').empty();
        $('span[name=mpeExternalURL]').empty();
        $('div[id=machine_div]').attr("data-id", data.id);
        $('div[id=machine_div]').css('display', 'block');
        $('div[id=ctstabs_div]').css('display', 'block');
        $('div[id=machineChart_tr]').css('display', 'none');
        $('button[name=machineinfoedit]').attr('id', data.id);
        if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
            $('button[name=machineinfoedit]').css('display', 'block');
        }
        $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + '/MPE/default.html?MPEStatus=' + data.name, style: 'color:white;' }).html("View").appendTo($('span[name=mpeview]'));
        $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + '/MPE/hourlyreport.html?MpeName=' + data.name, style: 'color:white;' }).html("HR View").appendTo($('span[name=mpeHRview]'));
        $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + '/Reports/MPEPerformance.html?MPEStatus=' + data.name, style: 'color:white;' }).html("MPE Synopsis").appendTo($('span[name=mpePerfomance]'));
        if (data.mpeGroup !== '') {
            $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + '/MPESDO/MPESDO.html?MPEGroupName=' + mpeData.mpeGroup, style: 'color:white;' }).html("SDO View").appendTo($('span[name=mpeSDO]'));
        }
        if (data.externalUrl !== '') {
            $('div[id=div_externalurl]').css('display', '');
            $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + '/MPE/externalurl.html?MPEExternalUrl=' + data.externalUrl, style: 'color:white;' }).html("External URL").appendTo($('span[name=mpeExternalURL]'));
        } else {
            $('div[id=div_externalurl]').css('display', 'none');
        }
        if (/machinetable/i.test(table)) {
            $('div[id=dps_div]').css('display', 'none');
            let machinetop_Table = $('table[id=' + table + ']');
            let machinetop_Table_Body = machinetop_Table.find('tbody');
            machinetop_Table_Body.empty();
            machinetop_Table_Body.append(machinetop_row_template.supplant(formatmachinetoprow(data)));
          
           
            if (mpeData !== null) {
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
                    // Promise.all([LoadMachineDPSTables(dataproperties, "dpstable")]);
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
                if (data.mpeRunPerformance.hasOwnProperty("hourlyData") && data.mpeRunPerformance.hourlyData.length > 0) {
                    Promise.all([GetMachinePerfGraph(data)]);
                }
                FormatMachineRowColors(mpeData);
            } else {
                var mpgtrStyle = document.getElementById('machineChart_tr').style;
                mpgtrStyle.display = 'none';
            }
        }
        sidebar.open('home'); 
    }
    catch (e) {
        // handle error
        console.error(e);
    }
}
let machinetop_row_template =
    '<tr data-id="{zoneId}"><td>{type}</td><td>{zoneName}</td><td><span class="badge badge-pill {stateBadge}" style="font-size: 12px;">{stateText}</span></td></tr>' +
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
    '<tr id="machineChart_tr" ><td colspan="3"><canvas id="machinechart" width="470" height="200"></canvas></td></tr>';

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
function formatmachinetoprow(properties) {
    try {
        return $.extend(properties, {
            zoneId: properties.id,
            zoneName: properties.name,
            type: properties.type,
            sortPlan: properties.mpeRunPerformance !== null ? Vaildatesortplan(properties.mpeRunPerformance) : "N/A",// ? properties.MPEWatchData.cur_sortplan : "N/A",
            opNum: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.curOperationId : 0 ,
            sortPlanStart: properties.mpeRunPerformance !== null ? VaildateMPEtime(properties.mpeRunPerformance.currentRunStart) : "N/A",
            sortPlanEnd: properties.mpeRunPerformance !== null ? VaildateMPEtime(properties.mpeRunPerformance.currentRunEnd) : "N/A",
            peicesFed: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.totSortplanVol : 0,
            throughput: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.curThruputOphr : 0,
            rpgVol: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.rpgEstVol : 0,
            stateBadge: getstatebadge(properties),
            yieldCalNumber: properties.mpeRunPerformance !== null ? getYiedCalNumber(properties.mpeRunPerformance) : 0,
            stateText: getstateText(properties),
            //estComp: VaildateEstComplete(properties.mpeRunPerformance.rpg),// checkValue(properties.MPEWatchData.rpg_est_comp_time) ? properties.MPEWatchData.rpg_est_comp_time : "Estimate Not Available",
            estComp: properties.mpeRunPerformance !== null ? VaildateEstComplete(properties.mpeRunPerformance.rpgEstimatedCompletion) : "N/A",
            rpgStart: properties.mpeRunPerformance !== null ? luxon.DateTime.fromISO(properties.mpeRunPerformance.rpgStartDtm).toFormat("yyyy-LL-dd HH:mm:ss") : 0,
            rpgEnd: properties.mpeRunPerformance !== null ? luxon.DateTime.fromISO(properties.mpeRunPerformance.rpgEndDtm).toFormat("yyyy-LL-dd HH:mm:ss") : 0,
            expThroughput: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.rpgExpectedThruput : 0,
            fullBins: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.binFullBins : 0,
            arsRecirc: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.arsRecrej3 : 0,
            sweepRecirc: properties.mpeRunPerformance !== null ? properties.mpeRunPerformance.sweepRecrej3 : 0
        });
    }
    catch (e) {
        console.log(e);
    }
}
function getstatebadge(properties) {
    try {
        if (properties?.mpeRunPerformance) {
            const { currentRunEnd, currentRunStart, curSortplan } = properties.mpeRunPerformance;
            const endtime = currentRunEnd && currentRunEnd !== '0' ? luxon.DateTime.fromFormat(currentRunEnd, 'yyyy-MM-dd HH:mm:ss') : null;
            const starttime = currentRunStart && currentRunStart !== '0' ? luxon.DateTime.fromFormat(currentRunStart, 'yyyy-MM-dd HH:mm:ss') : null;

            if (starttime?.isValid && !endtime) {
                return curSortplan ? "badge rounded-pill text-bg-success" : "badge rounded-pill text-bg-info";
            } else if (!starttime?.isValid && !endtime?.isValid) {
                return "badge rounded-pill text-bg-danger";
            } else if (starttime?.isValid && endtime?.isValid) {
                return "badge rounded-pill text-bg-info";
            }
        }
        return "badge rounded-pill text-bg-secondary";
    } catch (e) {
        console.log(e);
        return "badge rounded-pill text-bg-danger";
    }

}
function getstateText(properties) {
    try {
        const mpeRunPerformance = properties?.mpeRunPerformance;
        if (mpeRunPerformance) {
            const { curSortplan, currentRunEnd } = mpeRunPerformance;

            if (!curSortplan) {
                return VaildateMPEtime(currentRunEnd) ? "Idle" : "Unknown";
            } else if (curSortplan.length >= 3) {
                return "Running";
            } else {
                return "Idle";
            }
        }
        return "No Data";
    } catch (e) {
        console.log(e);
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
        let time = luxon.DateTime.fromFormat(data, "yyyy-MM-dd HH:mm:ss");

        if (time.isValid && time.year === luxon.DateTime.local().year) {
            return time.toFormat("yyyy-MM-dd HH:mm:ss");
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
        //if (est._isValid && est.year === luxon.DateTime.local().year) {
        if (est.year && est.year === luxon.DateTime.local().year) {
            return est.toFormat("yyyy-MM-dd HH:mm:ss");
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
function padLeft(string, length, paddingCharacter) {
    var paddedString = String(string);

    while (paddedString.length < length) {
        paddedString = paddingCharacter + paddedString;
    }

    return paddedString;
}
async function getlistofMPE() {

    $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Zone/GetZoneNameList?Type=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpedata) {
                if (mpedata.length > 0) {
                    mpedata.sort();
                    mpedata.push('**Machine Not Listed');
           
                    $('select[id=machine_zone_select_name]').css('display', '');
                    $('select[id=machine_zone_select_name]').empty();
                    $('<option/>').val("").html("").appendTo('select[id=machine_zone_select_name]');
                    $('select[id=machine_zone_select_name]').val("");
                    $.each(mpedata, function () {
                        $('<option/>').val(this).html(this).appendTo('#machine_zone_select_name');
                    })
                }
                else {
                    $('select[id=machine_zone_select_name]').empty();
                    $('<option/>').val("").html("").appendTo('select[id=machine_zone_select_name]');
                    $('#machine_manual_row').css('display', 'block');
                    $('#machine_select_row').css('display', 'none');
                    $('select[id=machine_zone_select_name]').css('display', 'none');

                }
        
            },
            error: function (error) {

                console.log(error);
                return false;
            },
            faulure: function (fail) {
                console.log(fail);
                return false;
            },
            complete: function (complete) {
                console.log(complete);
                return false;
            }
    });
}
async function getlistofMPEGroups() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/MPE/MPEGroups?Type=MPE',
            contentType: 'application/json',
            type: 'GET',
            success: function (mpeGroupData) {
                $('select[id=mpe_group_select]').empty();
                if (mpeGroupData.length > 0) {
                    mpeGroupData.push('');
                    mpeGroupData.sort();
                    mpeGroupData.push('**Group Not Listed');
                    $('#mpegroupname_div').css('display', 'none');
                    $('select[id=mpe_group_select]').css('display', '');
                    $.each(mpeGroupData, function () {
                        $('<option/>').val(this).html(this).appendTo('#mpe_group_select');
                    })
                    resolve();
                    return true;

                }
                else {
                    $('<option/>').val("**Group Not Listed").html("**Group Not Listed").appendTo('select[id=mpe_group_select]');
                    $('<option/>').val("").html("").appendTo('select[id=mpe_group_select]');
                    $('select[id=mpe_group_select]').val("");
                    $('#mpegroupname_div').css('display', 'none');
                    /*enableNewGroupName();*/
                    resolve();
                    return true;
                }
            },
            error: function (error) {

                console.log(error);
                return false;
            },
            faulure: function (fail) {
                console.log(fail);
                return false;
            },
            complete: function (complete) {
                //console.log(complete);
                return false;
            }
        });
    });
}
async function Edit_Machine_Info(id) {

    $('button[id=machinesubmitBtn]').prop('disabled', true);
    $('#machine_manual_row').css('display', 'none');
    $('#machine_select_row').css('display', 'block');


    if (!geoZoneMPE.hasOwnProperty("_layers")) return;

    let Data = {};
    let MPEwNUMBER = "";

    try {
        const leafletIds = await findMpeZoneLeafletIds(id);
        Data = geoZoneMPE._layers[leafletIds].feature.properties;
        $('#modalZoneHeader_ID').text('Edit MPE |' + Data.name);
        await Promise.all([await getlistofMPE(), getlistofMPEGroups()]).then(() => {
            $('select[id=machine_zone_select_name]').val(Data.name);
            $('select[id=mpe_group_select]').val(Data.mpeGroup);
        });
        if ($.isEmptyObject(Data)) return;
        MPEwNUMBER = Data.name;

        $('input[id=zone_id]').val(Data.id);
        $('input[id=machine_ip]').val(Data.mpeIpAddress);
        $('select[id=zone_Type]').val(Data.type);
        $('input[id=mpeRejectBins]').val(Data.rejectBins)
        $('input[id=mpeExternalUrl]').val(Data.externalUrl)
        if (MPEwNUMBER !== "") {
            const mpedata = await $.ajax({
                url: `${SiteURLconstructor(window.location)}/api/MPE/MPEStandard?Name=${MPEwNUMBER}`,
                contentType: 'application/json',
                type: 'GET',
                success: function (data) {
                    $('#' + StandardTable).DataTable().clear().draw();
                    if (data.length > 0) {
                        updateMpeStandardDataTable(data, StandardTable);
                    }
                }
            });
            await $.ajax({
                url: `${SiteURLconstructor(window.location)}/api/MPETragets/MPETargets?Name=${MPEwNUMBER}`,
                contentType: 'application/json',
                type: 'GET',
                success: function (data) {
                    $('#' + TargetTable).DataTable().clear().draw();
                    for (var tourNumber in tourlist) {
                        loadTargetsHourDataTable(tourlist[tourNumber], data);
                    }
                }
            });
        }
        let trp = $('select[id=machine_zone_select_name]').val();
        if ($('select[id=machine_zone_select_name]').val()) {
            $('#machine_manual_row').hide();
            $('#machine_select_row').show();
            $('select[id=machine_zone_select_name]').val(Data.name).trigger('change');
        } else {
            if (Data.mpeName === "") {
                let tempSplit = Data.name.split(/-(?=[^-]*$)/);
                $('input[id=machine_name]').val(tempSplit[0]);
                $('input[id=machine_number]').val(tempSplit[1]);
            }
            else {
                $('input[id=machine_name]').val(Data.mpeName);
                $('input[id=machine_number]').val(Data.mpeNumber);
            }
            $('#machine_manual_row').show();
            $('#machine_select_row').hide();
        }
        const $machineZoneSelect = $('select[id=machine_zone_select_name]');
        $machineZoneSelect.on("change", function () {
            if ($('select[name=machine_zone_select_name] option:selected').val() === '**Machine Not Listed') {
                // Handle the case where the machine is not listed
            }
        });
        $('button[id=machinesubmitBtn]').off().on('click', function () {
            try {
                let jsonObject = {};
                $('button[id=machinesubmitBtn]').prop('disabled', true);
                let tyrr = $('select[name=machine_zone_select_name] option:selected').val();
                if ($('select[name=machine_zone_select_name] option:selected').val()) {
                    let selectedMachine = $('select[name=machine_zone_select_name] option:selected').val().split(/-(?=[^-]*$)/);
                    jsonObject.mpeName = selectedMachine[0];
                    jsonObject.mpeNumber =  selectedMachine[1];
                    jsonObject.name = jsonObject.mpeName + "-" + jsonObject.mpeNumber.padStart(3, '0')
                }
                else {
                    jsonObject.mpeName = $('input[type=text][name=machine_name]').val();
                    jsonObject.mpeNumber = $('input[type=text][name=machine_number]').val();
                    jsonObject.name = jsonObject.mpeName + "-" + jsonObject.mpeNumber.padStart(3, '0');
                }
                jsonObject.rejectBins = $('input[type=text][name=mpeRejectBins]').val();
                jsonObject.externalUrl = $('input[type=text][name=mpeExternalUrl]').val();
                jsonObject.floorId = $('input[type=text][name=machine_ip]').val();
                jsonObject.mpeIpAddress = $('input[type=text][name=machine_ip]').val();
                jsonObject.ldc = $('input[type=text][name=zone_ldc]').val();
                jsonObject.payLocation = $('input[type=text][name=zone_paylocation]').val();
                jsonObject.payLocationColor = $('select[name=zonePayLocationColor] option:selected').val();
                jsonObject.type = $('select[name=zone_Type] option:selected').val();
                jsonObject.mpeGroup = $('select[name=mpe_group_select] option:selected').val();
                if (!$.isEmptyObject(jsonObject)) {
                    jsonObject["id"] = Data.id;

                    $.ajax({
                        url: SiteURLconstructor(window.location) + '/api/Zone/Update',
                        contentType: 'application/json',
                        data: JSON.stringify(jsonObject),
                        type: 'POST',
                        success: function (response) {
                            $('span[id=error_machinesubmitBtn]').text("" + jsonObject.name + " Zone has been Updated.");
                            setTimeout(function () {
                                $("#Zone_Modal").modal('hide');
                            }, 1500);
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
                }
            }
            catch (e) {
                $('span[id=error_machinesubmitBtn]').text(e);
            }
        });
        $('#Zone_Modal').modal('show');
        $('select[id=machine_zone_select_name]').trigger('change');
        $('select[id=mpe_group_select]').trigger('change');
        $('select[id=zone_Type]').trigger('change');
        $('input[id=mpeRejectBins]').trigger('keyup');
        $('input[id=mpeExternalUrl]').trigger('keyup');
    } catch (error) {
        console.error("Error fetching machine info: ", error);
    }
}
function constructTargetsHourColumns(tourNumber) {

    let columns = [];
    let tourhours = getTourHours(tourNumber);
    var column0 =
    {
        //first column is always the name
        title: "Tour " + tourNumber,
        data: "name",
        width: '240px'
    };
    columns[0] = column0;
    ////one column for every week_ending entry.
    $.each(tourhours, function (key, value) {
        columns[(key + 1)] =
        {
            title: value,
            data: value,
            width: '80px',
            render: function (data, type, row) {
                return data > 1000 ? formatNumberWithCommas(data) : data < -1000 ? formatNumberWithCommas(data) : data;
            }
        };
    });

    return columns;
}
function loadTargetsHourDataTable(tourNumber, targets) {

    let tourhours = getTourHours(tourNumber);
    let dataArray = [];
    let dataTarget = {};
    let dataTargetReject = {};
    dataTarget = {
        "order": 1,
        "name": "Target Volume"
    }
    dataTargetReject = {
        "order": 12,
        "name": "Target Reject Rate %"
    }
    for (let i = 0; i < tourhours.length; i++) {
        const hourTarget = targets.find(target => target.targetHour === tourhours[i]);
        dataTarget[tourhours[i]] = hourTarget?.hourlyTargetVol || 0;
        dataTargetReject[tourhours[i]] = hourTarget?.hourlyRejectRatePercent || 0;
    }
    dataArray.push(dataTarget);
    dataArray.push(dataTargetReject);
    $('#' + (TargetTable + tourNumber)).DataTable().clear().draw(); 
    Promise.all([updateTargetsHourDataTable(dataArray, TargetTable + tourNumber)]);
}
//async function Edit_Machine_Info(id) {
//    $('#modalZoneHeader_ID').text('Edit Machine Info');

//    sidebar.close('connections');
//   await Promise.all([getlistofMPE(), getlistofMPEGroups()]);
//    $('button[id=machinesubmitBtn]').prop('disabled', true);
//    try {
//        if (geoZoneMPE.hasOwnProperty("_layers")) {
//            let layerindex = -0;
//            let Data = {};
//            let MPEwNUMBER = "";
//            await findMpeZoneLeafletIds(id)
//                .then(leafletIds => {
//                    Data = geoZoneMPE._layers[leafletIds].feature.properties;
//                    MPEwNUMBER = Data.name
             

//                }).then(async () => {
//                    $('input[id=machine_ip]').val(Data.mpeIpAddress)
//                    $('select[id=zone_Type]').val(Data.type);
//                    $('select[id=machine_zone_select_name]').val(Data.name);
//                    $('select[id=mpe_group_select]').val(Data.mpeGroup);


//                    if (!$.isEmptyObject(Data)) {


//                        $.ajax({
//                            url: SiteURLconstructor(window.location) + '/api/MPE/MPEStandard?Name=' + MPEwNUMBER,
//                            contentType: 'application/json',
//                            type: 'GET',
//                            success: function (mpedata) {
//                                if (mpedata.length > 0) {
//                                    mpedata.sort();
//                                    mpedata.push('**Machine Not Listed');
//                                    $('#machine_manual_row').css('display', 'none');
//                                    $('#machine_select_row').css('display', '');
//                                    $('select[id=machine_zone_select_name]').css('display', '');
//                                    $('select[id=machine_zone_select_name]').empty();
//                                    $('<option/>').val("").html("").appendTo('select[id=machine_zone_select_name]');
//                                    $('select[id=machine_zone_select_name]').val("");
//                                    $.each(mpedata, function () {
//                                        $('<option/>').val(this).html(this).appendTo('#machine_zone_select_name');
//                                    })
//                                    $('select[id=machine_zone_select_name]').val(Data.name.toString());
//                                }
//                                else {
//                                    $('<option/>').val("").html("").appendTo('select[id=machine_zone_select_name]');
//                                    $('<option/>').val("**Machine Not Listed").html("**Machine Not Listed").appendTo('select[id=machine_zone_select_name]');
//                                    $('select[id=machine_zone_select_name]').val("**Machine Not Listed");
//                                    $('#machine_manual_row').css('display', '');
//                                    $('#machine_select_row').css('display', 'none');
//                                    $('select[id=machine_zone_select_name]').css('display', 'none');
//                                }
//                            },
//                            error: function (error) {

//                                console.log(error);
//                            },
//                            faulure: function (fail) {
//                                console.log(fail);
//                            },
//                            complete: function (complete) {
//                                console.log(complete);
//                            }
//                        });

//                        $('select[id=machine_zone_select_name]').on("change", function () {
//                            if ($('select[name=machine_zone_select_name] option:selected').val() === '**Machine Not Listed') {
//                                $('#machine_manual_row').css('display', '');
//                                $('#machine_select_row').css('display', 'none');
//                            }
//                            else {
//                                $('#machine_manual_row').css('display', 'none');
//                                $('#machine_select_row').css('display', '');
//                            }
//                            if ($('select[name=machine_zone_select_name] option:selected').val() === '') {
//                                $('button[id=machinesubmitBtn]').prop('disabled', true);
//                            }
//                            else {
//                                $('button[id=machinesubmitBtn]').prop('disabled', false);
//                            }
//                        });

//                        /*Onchange Validate Machine Group Name*/
//                        $('select[id=mpe_group_select]').on("change", function () {
//                            if ($('select[name=mpe_group_select] option:selected').val() === '**Group Not Listed') {
//                                $('#mpegroupname_div').css('display', '');
//                                enableNewGroupName();
//                            }
//                            else {
//                                $('#mpegroupname_div').css('display', 'none');
//                                $('input[id=mpegroupname]').val("");
//                                $('button[id=machinesubmitBtn]').prop('disabled', false);
//                            }
//                        });

//                        /*Validate new group name textbox not empty*/
//                        $('input[type=text][name=mpegroupname]').keyup(function () {
//                            enableNewGroupName();
//                        });

//                        $('input[type=text][name=machine_name]').val(Data.MPE_Type);
//                        $('input[type=text][name=machine_number]').val(Data.MPE_Number);
//                        $('input[type=text][name=zone_ldc]').val(Data.Zone_LDC);
//                        $('input[type=text][name=machine_id]').val(Data.id);
//                        $('input[type=text][name=machine_ip]').val(Data.MPE_IP);
//                        $('input[type=text][name=zone_paylocation]').val(Data.zonePayLocation);
//                        $('select[id=zonePayLocationColor]').val(Data.zonePayLocationColor);

//                        //get MPEENGStandard
//                        //empty values of the div.
//                        $('#mpestandard_div').html("");

$('.targetTour').off().on('click', function () {
    $("#Zone_Modal").modal('hide');
    $("#HourlyTarget_Modal").modal('show');
    $("#modalHourlyTargetHeader_ID").text($('input[id=targetmpeid]').val());
    let targettour = $(this).data("addtarget");

    //get tour hours
    let tourhours = getTourHours(targettour);

    $('#targethdlabel').text('Tour ' + targettour);
    $('#targethdlabel').css('fontWeight', 'bold');

    //get data
    $.ajax({
        url: SiteURLconstructor(window.location) + "/api/MPETragets/GetByMPE?MpeId=" + $('input[id=targetmpeid]').val(),
        contentType: 'application/json',
        type: 'GET',
        success: function (mpeTargetData) {
            const row = document.getElementById('mpetarget_div');
            row.textContent = "";
            const row1 = document.getElementById('mpetarget_div1');
            row1.textContent = "";
            const row2 = document.getElementById('mpetarget_div2');
            row2.textContent = "";
            const hrtemplate = document.querySelector('#mpetarget_hour_template');
            const cnttemplate = document.querySelector('#mpetarget_count_template');
            const rejtemplate = document.querySelector('#mpetarget_reject_template');
            let rowdata = new Array();
            let rowdata1 = new Array();
            let rowdata2 = new Array();
            for (let i = 0; i < tourhours.length; i++) {
                let curdiv = document.createElement("div");
                rowdata[i] = hrtemplate.content.cloneNode(true);
                const clonedElement = rowdata[i].querySelector('.hourtext');
                clonedElement.id = 'hourtext' + i;
                clonedElement.textContent = tourhours[i];
                curdiv.appendChild(rowdata[i]);
                curdiv.classList.add('col');
                row.appendChild(curdiv);

                curdiv = document.createElement("div");
                rowdata1[i] = cnttemplate.content.cloneNode(true);
                const hourid = rowdata1[i].getElementById("targethourid");
                hourid.id = 'hour' + i;
                $.each(mpeTargetData, function (key, value) {
                    if (value.hour==tourhours[i]) {
                        hourid.value = value.hourlyTargetVol;
                    }
                });
                curdiv.appendChild(rowdata1[i]);
                curdiv.classList.add('col');
                row1.appendChild(curdiv);

                curdiv = document.createElement("div");
                rowdata2[i] = rejtemplate.content.cloneNode(true);
                const rejectrateid = rowdata2[i].getElementById("rejectratehourid");
                rejectrateid.id = 'rejecthour' + i;
                $.each(mpeTargetData, function (key, value) {
                    if (value.hour==tourhours[i]) {
                        rejectrateid.value = value.hourlyRejectRatePercent;
                    }
                });
                curdiv.appendChild(rowdata2[i]);
                curdiv.classList.add('col');
                row2.appendChild(curdiv);
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
    addMPETargets(tourhours.length);           
    deleteMPETargets(tourhours.length);
});

function addMPETargets(hourcnt) {
    $('button[id=mpetargetedit]').off().on('click', function () {
        $('button[id=mpetargetedit]').prop('disabled', true);
        let arrayObject = [];
        for (let i = 0; i < hourcnt; i++) {
            let jsonObject = {
                mpeType: $('input[id=targetmpetype]').val(),
                mpeNumber: $('input[id=targetmpenumber]').val(),
                mpeId: $('input[id=targetmpeid]').val(),
                hour: $('#hourtext' + i).text(),
                hourlyTargetVol: $('input[id=hour' + i + ']').val() ? $('input[id=hour' + i + ']').val() : 0,
                hourlyRejectRatePercent: $('input[id=rejecthour' + i + ']').val() ? $('input[id=rejecthour' + i + ']').val() : 0
            };
            arrayObject.push(jsonObject);
        }

        if (arrayObject.length > 0) {
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/MPETragets/Add',
                data: JSON.stringify(arrayObject),
                contentType: 'application/json',
                type: 'POST',
                success: function (data) {
                    setTimeout(function () {
                        $("#HourlyTarget_Modal").modal('hide');
                        $('button[id=mpetargetedit]').prop('disabled', false);
                        $('#mpetarget_div').text('');
                        $("#Zone_Modal").modal('show');
                        displayMPETarget($('input[id=targetmpeid]').val());
                    }, 500);
                },
                error: function (error) {
                    //console.log(error);
                },
                failure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                    console.log(complete);
                }
            });
        }
    });
}
function deleteMPETargets(hourcnt) {
    $('button[id=mpetargetdelete]').off().on('click', function () {
        $('button[id=mpetargetdelete]').prop('disabled', true);
        let arrayObject = [];
        for (let i = 0; i < hourcnt; i++) {
            arrayObject.push($('input[id=targetmpeid]').val() + $('#hourtext' + i).text());
        }

        if (arrayObject.length > 0) {
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/MPETragets/DeleteTour',
                data: JSON.stringify(arrayObject),
                contentType: 'application/json',
                type: 'POST',
                success: function (data) {
                    setTimeout(function () {
                        $("#HourlyTarget_Modal").modal('hide');
                        $('button[id=mpetargetdelete]').prop('disabled', false);
                        $('#mpetarget_div').text('');
                        $("#Zone_Modal").modal('show');
                        displayMPETarget($('input[id=targetmpeid]').val());
                    }, 500);
                },
                error: function (error) {
                    //console.log(error);
                },
                failure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                    console.log(complete);
                }
            });
        }
    });
}
function getTourHours(tournumber) {
    const DateTime = luxon.DateTime;
    const Duration = luxon.Duration;
    let startTime = siteTours['tour' + tournumber + 'Start'];
    let endTime = siteTours['tour' + tournumber + 'End'];
    const interval = "01:00"

    let dtStart = DateTime.fromFormat(startTime, "HH:mm");
    let dtEnd = DateTime.fromFormat(endTime, "HH:mm");
    if (dtStart > dtEnd) {
        dtStart = dtStart.minus({ hours: 24 });
    }
    const durationInterval = Duration.fromISOTime(interval);

    let tourhours = [];
    let i = dtStart;
    while (i < dtEnd) {
        tourhours.push(i.toFormat("HH:mm"));
        i = i.plus(durationInterval);
    }
    return tourhours;
}
function createTargetDataTable(table, tour) {
    try {
        $('#' + table).DataTable({
            dom: 'Bfrtip',
            select: 'single',
            responsive: true,
            bFilter: false,
            bdeferRender: true,
            bpaging: false,
            bPaginate: false,
            autoWidth: false,
            bInfo: false,
            ordering: false, // Disable sorting for all columns
            destroy: true,
            language: {
                zeroRecords: "No Data"
            },
            order: [[]],
            aoColumns: constructTargetsHourColumns(tour),
            // Other DataTable options...
            initComplete: function () {
                var api = this.api();
                // Add click event listener to header cells
                $(api.table().header()).on('click', 'th', function () {
                    $('#targetHourZone').val($('select[name=machine_zone_select_name] option:selected').val())
                    // Get the column index of the clicked header
                    var columnIndex = $(this).index();
                    //get tour number from header in column 0 row 0
                    var tourText = $(api.column(0).header()).text();
                    var tourNumber = tourText.match(/\d+/)[0];
                    // Get the header text of the clicked cell's column
                    var headerText = $(api.column(columnIndex).header()).text();
                    // Get all data for the clicked column
                    let columnData = api.column(columnIndex).data().toArray();      
                    let mpeName = "";  
                    let mpeNumber = "";
                    if ($('select[name=machine_zone_select_name] option:selected').val() !== '') {
                        let selectedMachine = $('select[name=machine_zone_select_name] option:selected').val().split(/-(?=[^-]*$)/);
                        mpeName = selectedMachine[0];
                        mpeNumber = selectedMachine[1];
                    }
                    else {
                        mpeName = $('input[type=text][name=machine_name]').val();
                        mpeNumber = $('input[type=text][name=machine_number]').val();
                    }
                    // Optionally, you can trigger a modal or any other action with the column data
                    $('#Targets_Modal').modal('show');
                    // Pass data to the modal
                    $('#Targets_Modal').find('input[name="targetHour"]').val(headerText);
                    // Pass data to the modal
                    $('#Targets_Modal').find('input[name="hourlyTargetVol"]').val(columnData[0]);
                    // Pass data to the modal
                    $('#Targets_Modal').find('input[name="hourlyRejectRatePercent"]').val(columnData[1]);
                    // Handle the save button click event
                    $('#targetHoursubmitBtn').off('click').on('click', function () {
                        let jsonObject = {
                            "id": $('input[id=zone_id]').val(),
                            "mpeName": mpeName,
                            "mpeNumber": mpeNumber,
                            "targetHour": headerText,
                            "mpeId": mpeName + "-" + mpeNumber.padStart(3, '0'),
                            "hourlyTargetVol": $('input[name="hourlyTargetVol"]').val(),
                            "hourlyRejectRatePercent": $('input[name="hourlyRejectRatePercent"]').val()
                        }
                        //make a ajax call to get the Connection details
                        $.ajax({
                            url: SiteURLconstructor(window.location) + '/api/MPETragets/Update',
                            contentType: 'application/json-patch+json',
                            type: 'PUT',
                            data: JSON.stringify(jsonObject),
                            success: function (data) {
                               
                                // Update the cell data in the DataTable
                                //loadTargetsHourDataTable(tourNumber,data);
                                setTimeout(function () { $('#Targets_Modal').modal('hide'); }, 500);
                            },
                            error: function (error) {
                                $('span[id=error_targetHoursubmitBtn]').text(error);
                                //console.log(error);
                            },
                            faulure: function (fail) {
                                console.log(fail);
                            },
                           
                        });
              
                    });
                });
                api.on('draw', function () {
                    api.cells().every(function () {
                        var cell = this;
                        $(cell.node()).off('click').on('click', function () {
                            $('#targetHourZone').val($('select[name=machine_zone_select_name] option:selected').val());

                            //get tour number from header in column 0 row 0
                            var tourText = $(api.column(0).header()).text();
                            var tourNumber = tourText.match(/\d+/)[0];
                            // Retrieve data from the clicked cell
                            var cellData = cell.data();
                            // Get the column index of the clicked cell
                            var columnIndex = cell.index().column;
                            // Get the header text of the clicked cell's column
                            var headerText = $(api.column(columnIndex).header()).text();
                            // Get all data for the clicked column
                            var columnData = api.column(columnIndex).data().toArray();
                            // Trigger the modal popup
                            $('#Targets_Modal').modal('show');
                            let mpeName = "";
                            let mpeNumber = "";
                            if ($('select[name=machine_zone_select_name] option:selected').val() !== '') {
                                let selectedMachine = $('select[name=machine_zone_select_name] option:selected').val().split(/-(?=[^-]*$)/);
                                mpeName = selectedMachine[0];
                                mpeNumber = selectedMachine[1];
                            }
                            else {
                                mpeName = $('input[type=text][name=machine_name]').val();
                                mpeNumber = $('input[type=text][name=machine_number]').val();
                            }
                            // Pass data to the modal
                            $('#Targets_Modal').find('input[name="targetHour"]').val(headerText);
                            // Pass data to the modal
                            $('#Targets_Modal').find('input[name="hourlyTargetVol"]').val(columnData[0]);
                            // Pass data to the modal
                            $('#Targets_Modal').find('input[name="hourlyRejectRatePercent"]').val(columnData[1]);
                            // Handle the save button click event
                            $('#targetHoursubmitBtn').off('click').on('click', function () {
                                let jsonObject = {
                                    "id": $('input[id=zone_id]').val(),
                                    "mpeName": mpeName,
                                    "mpeNumber": mpeNumber,
                                    "targetHour": headerText,
                                    "mpeId": mpeName + "-"+ mpeNumber.padStart(3, '0'),
                                    "hourlyTargetVol": $('input[name="hourlyTargetVol"]').val(),
                                    "hourlyRejectRatePercent": $('input[name="hourlyRejectRatePercent"]').val()
                                }
                                //make a ajax call to get the Connection details
                                $.ajax({
                                    url: SiteURLconstructor(window.location) + '/api/MPETragets/Update',
                                    contentType: 'application/json-patch+json',
                                    type: 'PUT',
                                    data: JSON.stringify(jsonObject),
                                    success: function (data) {

                                        // Update the cell data in the DataTable
                                        //loadTargetsHourDataTable(tourNumber,data);
                                        setTimeout(function () { $('#Targets_Modal').modal('hide'); }, 500);
                                    },
                                    error: function (error) {
                                        $('span[id=error_targetHoursubmitBtn]').text(error);
                                        //console.log(error);
                                    },
                                    faulure: function (fail) {
                                        console.log(fail);
                                    },

                                });

                            });
                        });
                    });
                });
            }
        });

        $('#' + TargetTable + ' tbody td:first-child').css('pointer-events', 'none');

    } catch (e) {
        console.log("Error fetching machine info: ", e);
    }
}
async function updateTargetsHourDataTable(newdata, table) {
    try {
        return new Promise((resolve, reject) => {
            if ($.fn.dataTable.isDataTable("#" + table)) {
                if ($('#' + table).DataTable().rows().count() > 0) {
                    $('#' + table).DataTable().rows(function (idx, data, node) {
                        $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
                        resolve();
                        return true;
                    });
                }
                else {
                    $('#' + table).DataTable().rows.add(newdata).draw();
                }
            }
            resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function creatMpeStandardDataTable(table) {
    let Actioncolumn = true;
    let arrayColums = [{
        "name": "",
        "opn": "",
        "starttime": "",
        "endtime": "",
        "setuptime": "",
        "teardown": "",
        "chnageover": "",
        "pcsperhr": "",
        "staffperhour": "",
        "action": ""
    }];
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key, value) {
        tempc = {};
        if (/Name/i.test(key)) {
            tempc = {
                "title": 'Name',
                "width": "20%",
                "mDataProp": key,
            }
        }
        else if (/MessageType/i.test(key)) {
            tempc = {
                "title": "Message Type",
                "width": "20%",
                "mDataProp": key
            }
        }
        else if (/Port/i.test(key)) {
            tempc = {
                "title": "Port",
                "width": "10%",
                "mDataProp": key
            }
        }
        else if (/Status/i.test(key)) {
            tempc = {
                "title": "Status",
                "width": "20%",
                "mDataProp": key
            }
        }
        else if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "width": "20%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    Actioncolumn = true;
                    return '<button class="btn btn-light btn-sm mx-1 pi-iconEdit mpestadardedit" name="connectionedit"></button>' +
                        '<button class="btn btn-light btn-sm mx-1 pi-trashFill mpestadarddelete" name="connectiondelete"></button>'
                }
            }
        }
        else {
            tempc = {
                "title": capitalize_Words(key.replace(/\_/, ' ')),
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
        columnDefs: [],
        sorting: [[0, "asc"]]
    })
    // Edit/remove record
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/mpestadardedit/ig.test(this.name)) {
            sidebar.close();
            //Promise.all([Edit_Connection(row.data())]);
        }
        else if (/mpestadarddelete/ig.test(this.name)) {
            sidebar.close();
          //  Promise.all([Remove_Connection(row.data())]);
        }
    });
}
async function loadMpeStandardDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateMpeStandardDataTable(newdata, table) {
    try {
        return new Promise((resolve, reject) => {
            let loadnew = true;
            if ($.fn.dataTable.isDataTable("#" + table)) {
                $('#' + table).DataTable().rows(function (idx, data, node) {

                    if (data.id === newdata.id) {
                        loadnew = false;
                        $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
                    }
                });
                if (loadnew) {
                    loadMpeStandardDatatable([newdata], table);
                }
            } resolve();
            return false;
        });
    } catch (e) {
        throw new Error(e.toString());
    }
}
function removeMpeStandardDataTable(removedata, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.id === removedata) {
                $('#' + table).DataTable().row(node).remove().draw();
            }
        });
    }
}
async function GetMachinePerfGraph(dataproperties) {
    try {
        //replace the space with the letter T
        let localdateTime = luxon.DateTime.local().setZone(ianaTimeZone);
        let maxdate = localdateTime.plus({ hours: 2 }).startOf('hour');
        let mindate = maxdate.minus({ hours: 24 }).startOf('hour');

        //let mindate = luxon.DateTime.fromISO(dataproperties.mpeRunPerformance.hourlyData[0].hour.replace('\s',"T"));
        //let maxdate = luxon.DateTime.fromISO(dataproperties.mpeRunPerformance.hourlyData[23].hour).plus({ hours: 1 }).startOf('hour');;
        let total = $.map(dataproperties.mpeRunPerformance.hourlyData, obj => obj.count).reduce((total, count) => total + count);
        let totalSorted = $.map(dataproperties.mpeRunPerformance.hourlyData, obj => obj.sorted).reduce((total, sorted) => total + sorted);
        let totalRejected = $.map(dataproperties.mpeRunPerformance.hourlyData, obj => obj.rejected).reduce((total, rejected) => total + rejected);
        if (total > 0) {
            let MPEdataSet = {
                datasets: [
                    {
                        label: 'Pcs Fed',
                        type: "line",
                        hidden: false,
                        backgroundColor: "rgb(0,0,255)",
                        borderColor: "rgb(0,0,255)",
                        data: Object.values(dataproperties.mpeRunPerformance.hourlyData).sort((a, b) => luxon.DateTime.fromISO(a.hour.replace(/\s/g, 'T')) - luxon.DateTime.fromISO(b.hour.replace(/\s/g, 'T'))).map(od => ({ x: luxon.DateTime.fromISO(od.hour.replace(/\s/g, 'T')), y: od.count })),
                        yAxisID: 'yPeicesFed'
                    },
                    {
                        label: 'Sorted Pcs',
                        type: "line",
                        hidden: true,
                        backgroundColor: "rgb(48, 208, 116)",
                        borderColor: "rgb(48, 208, 116)",
                        data: Object.values(dataproperties.mpeRunPerformance.hourlyData).sort((a, b) => luxon.DateTime.fromISO(a.hour.replace(/\s/g, 'T')) - luxon.DateTime.fromISO(b.hour.replace(/\s/g, 'T'))).map(od => ({ x: luxon.DateTime.fromISO(od.hour.replace(/\s/g, 'T')), y: od.sorted })),
                        yAxisID: 'ySorted'
                    },
                    {
                        label: 'Rejected Pcs',
                        type: "line",
                        hidden: true,
                        backgroundColor: "rgb(237, 187, 153)",
                        borderColor: "rgb(237, 187, 153)",
                        data: Object.values(dataproperties.mpeRunPerformance.hourlyData).sort((a, b) => luxon.DateTime.fromISO(a.hour.replace(/\s/g, 'T')) - luxon.DateTime.fromISO(b.hour.replace(/\s/g, 'T'))).map(od => ({ x: luxon.DateTime.fromISO(od.hour.replace(/\s/g, 'T')), y: od.rejected })),
                        yAxisID: 'yRejected'
                    }
                ]
            };
            let MPEconfig = {
                data: MPEdataSet,
                options: {
                    animation: false,
                    responsive: false,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        },
                        tooltip: {
                            callbacks: {
                                beforeTitle: function () {
                                    return "";
                                },
                                label: function (context) {
                                    let label = "Time: " + luxon.DateTime.fromJSDate(new Date(context.dataset.data[context.dataIndex].x)).toFormat('L/dd/yyyy T');
                                    return label;
                                }, 
                                beforeFooter: function (context) {
                                    let label = "";
                                    label = context[0].dataset.label+": " + parseFloat(context[0].dataset.data[context[0].dataIndex].y).toLocaleString('en-US');
                                    return label;
                                }
                            }
                        }
                    },

                    scales: {
                        x: {
                            min: mindate.setZone("system", { keepLocalTime: true }).ts,
                            max: maxdate.setZone("system", { keepLocalTime: true }).ts,
                            type: 'time',
                            time: {
                                displayFormats: {
                                    hour: "HH:mm",
                                },
                                unit: 'hour'
                            }
                        },
                        yPeicesFed: {
                            beginAtZero: true
                        },
                        ySorted: {
                            beginAtZero: true,
                            display:false
                        },
                        yRejected: {
                            beginAtZero: true,
                            display: false
                        }
                    }
                }
            };
          
            let MpeChart = new Chart("machinechart", MPEconfig)

            const SortedIndex = MpeChart.data.datasets.findIndex(dataset => dataset.yAxisID === 'ySorted');
            const RejectedIndex = MpeChart.data.datasets.findIndex(dataset => dataset.yAxisID === 'yRejected');
            const Sortedcounts = MpeChart.data.datasets[SortedIndex];
            const Rejectedcounts = MpeChart.data.datasets[RejectedIndex];
            if (totalSorted > 0) {
                Sortedcounts.hidden = false;
            }
            if (totalRejected > 0) {
                Rejectedcounts.hidden = false;
            }
            MpeChart.update();
        }
        else {
            var mpgtrStyle = document.getElementById('machineChart_tr').style;
            mpgtrStyle.display = 'none';
        }
    } catch (e) {
        console.log(e)
    }
}