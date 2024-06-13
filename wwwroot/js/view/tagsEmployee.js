

let tagsEmployees = new L.GeoJSON(null, {
    pointToLayer: function (feature, latlng) {
        return new L.circleMarker(latlng, {
            class: "persontag",
            radius: 0,
            opacity: 0,
            fillOpacity: 0
        });

    },
    onEachFeature: function (feature, layer) {
        layer.markerId = feature.properties.id;
        let VisiblefillOpacity = feature.properties.visible ? "" : "tooltip-hidden";

        let classname = getmarkerType(feature.properties.craftName) + VisiblefillOpacity;
        layer.on('click', function (e) {
            //makea ajax call to get the employee details
            $.ajax({
                url: '/api/Tag/' + feature.properties.id,
                type: 'GET',
                success: function (data) {
                    $('button[name="tagEdit"]').attr('data-id', feature.properties.id);
                    Promise.all([hidestafftables()]);
                    $('div[id=div_taginfo]').css('display', '');
                    data.properties.posAge = feature.properties.posAge;
                    data.properties.locationMovementStatus = feature.properties.locationMovementStatus;
                    updateTagDataTable(formattagdata(data.properties), "tagInfotable");
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
        layer.bindTooltip("", {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1,
            className: classname,
        }).openTooltip();
    }, filter: function (feature, layer) {
        return feature.properties.visible;
    }
});
// add to the map and layers control
let overlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(overlayLayer, "Badges");
tagsEmployees.addTo(overlayLayer);
async function hidestafftables() {
    $('div[id=div_taginfo]').css('display', '');
    $('div[id=div_userinfo]').css('display', 'none');
    $('div[id=div_overtimeinfo]').css('display', 'none');
    $('div[id=div_staffinfo]').css('display', 'none');

}
async function findLeafletIds(markerId) {
    return new Promise((resolve, reject) => {
        tagsEmployees.eachLayer(function (layer) {
            if (layer.markerId === markerId) {
                resolve(layer._leaflet_id);
                return false;
            }
        });
        reject(new Error('No layer found with the given markerId'));
    });
}
async function init_tagsEmployees(data) {
    return new Promise((resolve, reject) => {
        try {
            createStaffingDataTable("staffingtable");
            createTagDataTable('tagInfotable');
            $(document).on('change', '.leaflet-control-layers-selector', function () {
                let sp = this.nextElementSibling;
                if (/^badges$/ig.test(sp.innerHTML.trim())) {
                    if (this.checked) {
                        connection.invoke("JoinGroup", "Tags").catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                    else {
                        connection.invoke("LeaveGroup", "Tags").catch(function (err) {
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
async function positionUpdate(leafletId, lat, lag) {
    return new Promise((resolve, reject) => {

        if (tagsEmployees._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
            tagsEmployees._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
            resolve();
            return false;
        }
        else {
            tagsEmployees._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
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

                }
            };
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
        sorting: [[1, "asc"]],
        rowCallback: function (row, data, index) {
            $(row).find('td:eq(2)').css('text-align', 'center');
            $(row).find('td:eq(3)').css('text-align', 'center');
            $(row).find('td:eq(4)').css('text-align', 'center');
        },
        footerCallback: function (row, data, start, end, display) {
            let api = this.api();
            // converting to interger to find total
            let intVal = function (i) {
                return typeof i === 'string' ?
                    i.replace(/[$,]/g, '') * 1 :
                    typeof i === 'number' ?
                        i : 0;
            };
            // computing column Total of the complete result 
            let schetotal = api
                .column(2)
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0);
            let in_buildingtotal = api
                .column(3)
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0);
            let epacstotal = api
                .column(4)
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0);
            // Update footer by showing the total with the reference of the column index 
            $(api.column(2).footer()).html(schetotal).css('text-align', 'center');
            $(api.column(3).footer()).html(in_buildingtotal).css('text-align', 'center');
            $(api.column(4).footer()).html(epacstotal).css('text-align', 'center');
        }
    });
}
function loadStaffingDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
function updateStaffingDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            for (const element of newdata) {
                if (data.type === element.type) {
                    $('#' + table).DataTable().row(node).data(element).draw().invalidate();
                }
            }

        })
        if (loadnew) {
            loadStaffingDatatable(newdata, table);
        }
    }
}
function formattagdata(result) {
    let reformatdata = [];
    try {
        for (let key in result) {
            let temp = {
                "INDEX": "",
                "KEY_NAME": "",
                "VALUE": ""
            };
            if (/^(admin)/i.test(appData.role) && !$.isPlainObject(result[key]) && /^(empFirstName|empLastName)/ig.test(key)) {
                switch (key) {
                    case "empFirstName":
                        temp['INDEX'] = 0;
                        break;
                    case "empLastName":
                        temp['INDEX'] = 0;
                        break;
                    default:
                        temp['INDEX'] = 10;
                        break;
                }
                temp['KEY_NAME'] = key;
                temp['VALUE'] = result[key];
                reformatdata.push(temp);
            }

            if (!$.isPlainObject(result[key]) && /^(id|ein|encodedID|craftName|tourNumber|daysOff|floorId|posAge|locationMovementStatus|payLocation|title|designationActivity)/ig.test(key)) {
                switch (key) {
                    case "craftName":
                        temp['INDEX'] = 2;
                        break;
                    case "id":
                        temp['INDEX'] = 1;
                        break;
                    case "ein":
                        temp['INDEX'] = 1;
                        break;
                    case "encodedID":
                        temp['INDEX'] = 1;
                        break;
                    default:
                        temp['INDEX'] = 10;
                        break;
                }
                temp['KEY_NAME'] = key;
                temp['VALUE'] = result[key];
                reformatdata.push(temp);
            }
        }

    } catch (e) {
        throw new Error(e.toString());
    }

    return reformatdata;
}

async function tagEditInfo() {

    //get tag info from the tagEdit data-id

    let tagid = $('button[name="tagEdit"]').data('id');
    //makea ajax call to get the employee details
    $.ajax({
        url: '/api/Tag/' + tagid,
        type: 'GET',
        success: function (data) {
            Promise.all([EditUserInfo(data.properties)]);
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
async function EditUserInfo(properties) {
    //move to tag location 

    // Set the header text of the modal
    $('#modaluserHeader_ID').text('Edit Tag Info');
    $('button[id=usertagsubmitBtn]').prop('disabled', false);
    $('input[name=tag_id]').prop('disabled', true);
    // Close the sidebar
    sidebar.close();
    // Populate the input fields with the feature properties
    if (/Person/ig.test(properties.tagType)) {
        $('#personform').css("display", "block");
    }
    $('input[id=employeeEIN]').val(properties.eIN);
    $('input[id=empFirstName]').val(properties.empFirstName);
    $('input[id=empLastName]').val(properties.empLastName);
    $('input[id=tagEncodedID]').val(properties.encodedId);
    $('input[id=paylocation]').val(properties.empPayLocation);
    $('select[id=tagType_select]').val(properties.tagType);
    if (!/^(Clerk|Supervisor|Maintenance|Mail Handler|Custodial)$/ig.test(properties.craftName)) {
        $('select[id=tagCraftName_select]').val("");
    }
    else {
        $('select[id=tagCraftName_select]').val(capitalize_Words(properties.craftName));
    }

    $('input[name=tag_name]').val(properties.name);
    $('input[name=tag_id]').val(properties.id);

    // Set up the click event for the submit button
    $('button[id=usertagsubmitBtn]').off().on('click', function () {
        try {
            // Disable the submit button to prevent multiple submissions
            $('button[id=usertagsubmitBtn]').prop('disabled', true);

            // Create an object to store the updated properties
            let updatedProperties = {};
            updatedProperties.tagId = $('input[name=tag_id]').val();
            updatedProperties.name = $('input[name=tag_name]').val();
            if ($('select[name=tagType_select] option:selected').val() !== properties.tagType) {
                updatedProperties.tagtype = $('select[name=tagType_select] option:selected').val();
            }
            if (/Person/ig.test($('select[id=tagType_select]').val())) {
                updatedProperties.ein = $('input[name=empId]').val();
                updatedProperties.empFirstName = $('input[name=empFirstName]').val();
                updatedProperties.empLastName = $('input[name=empLastName]').val();
                updatedProperties.encodedId = $('input[name=tagEncodedID]').val();
                updatedProperties.title = $('select[name=tagCraftName_select] option:selected').val();
                updatedProperties.empPayLocation = $('input[id=paylocation]').val();
            }
            else {
                updatedProperties.ein = "";
                updatedProperties.empFirstName = "";
                updatedProperties.empLastName = "";
                updatedProperties.encodedId = "";
                updatedProperties.title = "";
                updatedProperties.payLocation = "";
            }
            // Send the updated properties to the server

            if (!$.isEmptyObject(updatedProperties)) {

                let uri = APIURLconstructor(window.location) + "Tags/TagId";
                $.ajax({
                    url: uri,
                    headers:
                    {
                        'APIAuthorization': APIAuth
                    },
                    type: 'POST',
                    data: JSON.stringify([updatedProperties]),
                    contentType: 'application/json',
                    success: function (properties) {
                        $('span[id=error_usertagsubmitBtn]').text("Tag Info has been updated");
                    },
                    error: function (error) {
                        // Display user-friendly error message on the webpage
                        $('span[id=error_usertagsubmitBtn]').text("Error retrieving data: " + error.responseText);

                    },
                    failure: function (response) {
                        // Display user-friendly error message on the webpage
                        $('span[id=error_usertagsubmitBtn]').text("Request failed: " + response.statusText);
                    },
                    complete: function (data) {
                        setTimeout(function () {
                            $("#UserTag_Modal").modal('hide');
                            sidebar.open('userprofile');
                        }, 1000);
                    }
                });

            } else {
                $('span[id=error_usertagsubmitBtn]').text("No Tag Data has been Updated");

            }
        } catch (error) {
            $('span[id=error_usertagsubmitBtn]').text(error);
        }
    });

    // Show the modal
    $('#UserTag_Modal').modal('show');
}