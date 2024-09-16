let geoZoneDockDoor = new L.GeoJSON(null, {
    style: function (feature) {
        let ZoneColor = GetDockDoorZoneColor(feature.properties);
        return {
            weight: 1,
            opacity: 1,
            color: '#989ea4',
            fillColor: ZoneColor,
            fillOpacity: 0.2,
            lastOpacity: 0.2
        };
    },
    onEachFeature: function (feature, layer) {

        layer.zoneId = feature.properties.id;
        //extract number from feature.properties.name and remove leading zeros
        let dockDoorNumber = parseInt(feature.properties.doorNumber.match(/\d+/g)[0], 10);
        let dockdookflash = GetDockDoorFlash(feature.properties);
        if (feature.properties.tripDirectionInd !== "") {
            dockDoorNumber += "-" + feature.properties.tripDirectionInd
        }
        layer.on('click', function (e) {

            //if sourceTarget is not available, use target
            if (e.sourceTarget.getCenter) {
               
                OSLmap.setView(e.sourceTarget.getCenter(), 4);
                Promise.all([LoadDockDoorTable(feature.properties)]);
            }
        });
        layer.bindTooltip(dockDoorNumber.toString(), {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 0.9,
            className: 'dockdooknumber ' + dockdookflash
        }).openTooltip();
    },
    filter: function (feature, layer) {
        return feature.properties.visible;
    }
});

// add to the map and layers control
let geoZoneDockDooroverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneDockDooroverlayLayer, "Dock Door");
geoZoneDockDoor.addTo(geoZoneDockDooroverlayLayer);
connection.on("addDockDoorzone", async (zoneDate) => {
    addDockDoorFeature(zoneDate);

});
connection.on("deleteDockDoorzone", async (zoneDate) => {
    deleteDockDoorFeature(zoneDate);

});
connection.on("updateDockDoorzone", async (zoneDate) => {
    //need to update bin zone
    Promise.all([ updateDockDoorFeature(zoneDate)]);
});
connection.on("updateDockDoorTrip", async (data) => {

    await findDockDoorZoneLeafletIds(data.zoneId)
        .then(leafletIds => {
            geoZoneDockDoor._layers[leafletIds].feature.properties.routeTrips = data;
            geoZoneDockDoor._layers[leafletIds].setStyle({
                weight: 1,
                opacity: 1,
                fillOpacity: 0.2,
                fillColor: GetMacineBackground(data),
                lastOpacity: 0.2
            });
        });
});
async function addDockDoorFeature(data) {
    try {
        await findDockDoorZoneLeafletIds(data.properties.id)
            .then(leafletIds => {

            })
            .catch(error => {
                geoZoneDockDoor.addData(data);
            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function deleteDockDoorFeature(data) {
    try {
        await findDockDoorZoneLeafletIds(data.properties.id)
            .then(leafletIds => {
                geoZoneDockDoor.removeLayer(leafletIds);
            })
            .catch(error => {

            });
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function updateDockDoorFeature(data) {
    await findDockDoorZoneLeafletIds(data.id)
        .then(leafletIds => {
            try {
                geoZoneDockDoor._layers[leafletIds].feature.properties = data;
                geoZoneDockDoor._layers[leafletIds].setStyle({
                    weight: 1,
                    opacity: 1,
                    fillColor: GetDockDoorZoneColor(data),
                    fillOpacity: 0.2,
                    lastOpacity: 0.2
                });
                let dockDoorNumber = parseInt(geoZoneDockDoor._layers[leafletIds].feature.properties.doorNumber.match(/\d+/g)[0], 10);
                if (geoZoneDockDoor._layers[leafletIds].feature.properties.tripDirectionInd !== "") {
                    dockDoorNumber += "-" + geoZoneDockDoor._layers[leafletIds].feature.properties.tripDirectionInd;
                }
                geoZoneDockDoor._layers[leafletIds].setTooltipContent(dockDoorNumber.toString()); // Ensure dockDoorNumber is a string
                if (data.tripDirectionInd === "O" && data.tripMin > 0 && data.tripMin <= 30) {
                    if (geoZoneDockDoor._layers[leafletIds].feature.properties.containersNotLoaded > 0) {
                        if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty("_tooltip")) {
                            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty("_container")) {
                                if (!geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                                    geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.add('doorflash');
                                }
                            }
                        }
                    } else {
                        if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty("_tooltip")) {
                            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty("_container")) {
                                if (geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                                    geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.remove('doorflash');
                                }
                            }
                        }
                    }
                }
                if (data.tripDirectionInd === "I" && data.tripMin > 0 && data.tripMin <= 30) {
                    if (geoZoneDockDoor._layers[leafletIds].feature.properties.containersNotLoaded > 0) {
                        if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty("_tooltip")) {
                            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty("_container")) {
                                if (!geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                                    geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.add('doorflash');
                                }
                            }
                        }
                    } else {
                        if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty("_tooltip")) {
                            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty("_container")) {
                                if (geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                                    geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.remove('doorflash');
                                }
                            }
                        }
                    }
                } else {
                    if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty("_tooltip")) {
                        if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty("_container")) {
                            if (geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                                geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.remove('doorflash');
                            }
                        }
                    }
                }
            } catch (e) {
                console.log(e);
            }
        });
}
async function findDockDoorZoneLeafletIds(zoneId) {
    return new Promise((resolve, reject) => {
        geoZoneDockDoor.eachLayer(function (layer) {
            if (layer.zoneId === zoneId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given DockDoor Zone Id'));
    });
}
async function init_geoZoneDockDoor() {
    return new Promise((resolve, reject) => {
        try {
            //load the table 
            createDockdoorDataTable("dockdoortable");
            connection.invoke("GetDockDoorGeoZones").then(function (data) {
                //load dockdoor data
                for (let i = 0; i < data.length; i++) {
                    Promise.all([addDockDoorFeature(data[i])]);
                }
            }).catch(function (err) {
                // handle error
                console.error(err);
            });
       
            $(document).on('change', '.leaflet-control-layers-selector', function (e) {
                let sp = this.nextElementSibling;
                if (/^(Dock Door)/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "DockDoor").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "DockDoor").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                }
            });
            connection.invoke("JoinGroup", "DockDoor").catch(function (err) {
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
async function LoadDockDoorTable(data) {
    try {
        dockdoorloaddata = [];
        hideSidebarLayerDivs();
        $('div[id=dockdoor_div]').attr("data-id", data.id);
        $('div[id=dockdoor_div]').css('display', 'block');
        $('div[id=dockdoor_tripdiv]').css('display', 'block');
        $('div[id=ctstabs_div]').css('display', 'none');
        $('div[id=trailer_div]').css('display', 'block');
        $('button[name=container_counts]').text(0 + "/" + 0);
        $('span[name=doornumberid]').text(data.doorNumber);
        $('select[id=tripSelector]').val("");
        $('span[name=doorview]').empty();
        $('button[name=dockdoorinfoedit]').attr('id', data.id);
        $('span[name=doorstatus]').text(data.status === "" ? "Unknown" : data.status);
        if (data.externalUrl) {
            $("<a/>").attr({ target: "_blank", href: data.externalUrl, style: 'color:white;' }).html("View").appendTo($('span[name=doorview]'));
        }
        else {
            $("<a/>").attr({ target: "_blank", href: SiteURLconstructor(window.location) + 'Dockdoor/Dockdoor.aspx?DockDoor=' + data.doorNumber, style: 'color:white;' }).html("View").appendTo($('span[name=doorview]'));
        }

        //container_Table_Body.empty();
        let reultData = [];
        let newresult = [];
        let counter = 0;
        if (data.hasOwnProperty("legSiteName")) {
            reultData["siteName"] = { "siteName": data.legSiteName };
        }
        if (data.hasOwnProperty("route")) {
            reultData["routTrip"] = { "routTrip": data.route + "-" + data.trip };
        }
        if (data.hasOwnProperty("tripDirectionInd")) {
            reultData["direction"] = { "direction": data.tripDirectionInd };
        }
        if (data.hasOwnProperty("scheduledDtm")) {
            reultData["scheduled"] = { "scheduled": data.scheduledDtm };
        }
        for (let key in reultData) {
            if (/^(siteName|routTrip|direction|scheduled)/.test(key)) {
                newresult[counter] = reultData[key];
                reultData[key]["name"] = key;
                reultData[key]["sortorder"] = dockdoortableKeyDisplay[key] == null ? 200 : dockdoortableKeyDisplay[key].sortorder;
                counter++;
            }
            
        }
        updateDockdoorDataTable(newresult, "dockdoortable");
        sidebar.open('home');

    }
    catch (e) {
        throw new Error(e.toString());
    }
}
function GetDockDoorFlash(data) {
    try {
        
        if (data.isTripAtDoor) {
            if (data.tripDirectionInd === "O" && data.tripMin <= 30 && data.containersNotLoaded > 0) {
                    return "doorflash";
                }
                else {
                    return "";
                }
            }
            else {
                return "";
            }
        
    } catch (e) {
        return "";
    }
}
function GetDockDoorZoneColor(data) {
    let activeTrip30MissingContainer = '#dc3545'; //red
    let activeTrip = '#3573b1'; //blue 
    let notTrip = '#989ea4'; //clear
    try {

        if (data.isTripAtDoor)
            if (data.tripDirectionInd === "O" && data.tripMin <= 30) {
                return activeTrip30MissingContainer;
            }
            else {
                return activeTrip;
            }
        else {
            return notTrip;
        }


    } catch (e) {
        return notTrip;
    }
}
async function createDockdoorDataTable(table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {// Check if DataTable has been previously created and therefore needs to be flushed

        $('#' + table).DataTable().destroy(); // destroy the dataTableObject
        // For new version use table.destroy();
        $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
        // The line above is needed if number of columns change in the Data
    }
    let columns = [
        {
            data: 'sortorder',
            title: 'sortorder'
        },
        {
            data: 'name',
            title: 'Name',
            width: '21%',
            mRender: function (full, data) {
                let displayVal;
                displayVal = dockdoortableKeyDisplay[full] == null ? full : dockdoortableKeyDisplay[full].disp;
                return displayVal;
            }
        }
    ]
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
        columnDefs: [{
            visible: false,
            targets: 0
        },
        {
            orderable: false, // Disable sorting on all columns
            targets: '_all'
        }]
    })

}
async function loadDockdoorDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateDockdoorDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            $('#' + table).DataTable().row(node).data(element).draw().invalidate();

        })
        if (loadnew) {
            loadDockdoorDatatable(newdata, table);
        }
    }
}


//let container_Table = $('table[id=containertable]');
//let container_Table_Body = container_Table.find('tbody');
//function formatctscontainerrow(properties, zoneid) {
//    return $.extend(properties, {
//        zone_id: zoneid,
//        id: properties.placardBarcode,
//        dest: checkValue(properties.dest) ? properties.dest : "",
//        location: checkValue(properties.location) ? properties.location : "",
//        placard: properties.placardBarcode,
//        status: properties.constainerStatus,
//        backgroundcolorstatus: properties.constainerStatus === "Unloaded" ? "table-secondary" : properties.constainerStatus === "Close" ? "table-primary" : properties.constainerStatus === "Loaded" ? "table-success" : "table-danger"
//    });
//}
//let container_row_template = '<tr>' +
//    '<td data-input="dest" class="text-center">{dest}</td>' +
//    '<td data-input="location" class="text-center">{location}</td>' +
//    '<td data-input="found-icon" class="text-center"></td>' +
//    '<td data-input="placard" class="text-center"><a data-doorid={zone_id} data-placardid={placard} class="containerdetails">{placard}</a></td>' +
//    '<td data-input="status" class="text-center {backgroundcolorstatus}">{status}</td>' +
//    '</tr>"';
//let dockdoortop_Table = $('table[id=dockdoortable]');
//let dockdoortop_Table_Body = dockdoortop_Table.find('tbody');
//let dockdoortop_row_template = '<tr data-id={zone_id} >' +
//    '<td class="text-right" style="border-right-style:solid" >{name}</td>' +
//    '<td class="text-left">{value}</td>' +
//    '</tr>';
let dockdoortableKeyDisplay = {
    "siteName": { disp: "Site Name", sortorder: 0 },
    "routTrip": { disp: "Route-Trip", sortorder: 5 },
    "trailerBarcode": { disp: "Trailer Barcode", sortorder: 12 },
    "loadPercent": { disp: "Load Percent", sortorder: 15 },
    "direction": { disp: "Direction", sortorder: 21 },
    "scheduled": { disp: "Scheduled", sortorder: 22 },
    "arriveTime": { disp: "Actual Arrive Time", sortorder: 35 }
};