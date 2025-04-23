let cubeAssignInfoTable = 'cubeAssignInfoTable';
let badgeScanTable = 'scanHistoryTable';

//on close clear all inputs
$('#Zone_Modal').on('hidden.bs.modal', function() {
  $(this).find('input[type=text],textCube,select').css({ 'border-color': '#D3D3D3' }).val('').end().find('span[class=text]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).change();
});
//on open set rules
$('#Zone_Modal').on('shown.bs.modal', function() {});
let geoZoneCube = new L.GeoJSON(null, {
  style: function(feature) {
    return {
      weight: 1,
      opacity: 1,
      color: '#989ea4',
      fillOpacity: 0.2,
      lastOpacity: 0.2
    };
  },
  onEachFeature: function(feature, layer) {
    layer.zoneId = feature.properties.id;
    layer.on('click', async function(e) {
      if (e.sourceTarget.hasOwnProperty('_content')) {
        OSLmap.setView(e.sourceTarget._latlng, 4);
      } else {
        OSLmap.setView(e.sourceTarget.getCenter(), 4);
      }
      await loadCubeInfo(feature.properties);
    });

    layer
      .bindTooltip(`Dept: ${feature.properties.name}<br>Cube #: ${feature.properties.number}<br>${feature.properties.assignTo}`, {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 1,
        className: 'fixed-tooltip location '
      })
      .closeTooltip();
  },
  filter: function(feature, layer) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneCubeoverlayLayer = L.layerGroup();
layersControl.addOverlay(geoZoneCubeoverlayLayer, 'Cube Zones');
geoZoneCube.addTo(geoZoneCubeoverlayLayer);

async function findCubeLeafletIds(zoneId) {
  return new Promise((resolve, reject) => {
    geoZoneCube.eachLayer(function(layer) {
      if (layer.zoneId === zoneId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given MPE Zone Id: ' + zoneId));
  });
}
async function init_geoZoneCube(floorId) {
  try {
    await createCubeDataTable(cubeAssignInfoTable);
    await createBadgeScanDatatable(badgeScanTable);
    await $.ajax({
      url: `${SiteURLconstructor(window.location)}/api/Zone/ZonesTypeByFloorId?floorId=${floorId}&type=Cube`,
      contentType: 'application/json',
      type: 'GET',
      success: async function(data) {
        for (let i = 0; i < data.length; i++) {
          await addCubeFeature(data[i]);
        }
      }
    });
    $(document).on('change', '.leaflet-control-layers-selector', function(e) {
      let sp = this.nextElementSibling;
      if (/^(Cube Zones)$/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'Cube').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'Cube').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    await addGroupToList('Cube');
    if (/^(admin)/i.test(appData.Role)) {
      $('button[name=editCube]').off().on('click', function() {
        var id = $(this).attr('id');
        if (checkValue(id)) {
          Promise.all([EditCube(id)]);
        }
      });
      $('button[name=fetchAssignData]').off().on('click', async function() {
        var id = $(this).attr('id');
        if (checkValue(id)) {
          await fetchBadgeScanHistory(id);
        }
      });
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
connection.on('addCubezone', async zoneDate => {
  Promise.all([addCubeFeature(zoneDate)]);
});
connection.on('deleteCubezone', async zoneDate => {
  Promise.all([deleteCubeFeature(zoneDate)]);
});
connection.on('updateCubezone', async mpeZonedata => {
  await findCubeLeafletIds(mpeZonedata.properties.id).then(leafletIds => {
    geoZoneCube._layers[leafletIds].properties = mpeZonedata.properties;
  });
});
async function addCubeFeature(data) {
  try {
    await findCubeLeafletIds(data.properties.id).then(leafletIds => {}).catch(error => {
      geoZoneCube.addData(data);
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteCubeFeature(data) {
  try {
    await findCubeLeafletIds(data.properties.id)
      .then(leafletIds => {
        geoZoneCube.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function createCubeDataTable(table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    // Check if DataTable has been previously created and therefore needs to be flushed

    $('#' + table).DataTable().destroy(); // destroy the dataTableObject
    // For new version use table.destroy();
    $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
    // The line above is needed if number of columns change in the Data
  }
  let arrayColums = {
    order: '',
    Name: '',
    Value: ''
  };
  let columns = [];
  let tempc = {};
  $.each(arrayColums, function(key) {
    tempc = {};

    if (/Name/i.test(key)) {
      tempc = {
        title: 'Name',
        mDataProp: key,
        width: '25%',
        className: 'display'
      };
    } else if (/Value/i.test(key)) {
      tempc = {
        title: 'Value',
        mDataProp: key,
        width: '75%'
      };
    } else {
      tempc = {
        title: capitalize_Words(key.replace(/\_/, ' ')),
        mDataProp: key
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
      zeroRecords: 'No Data'
    },
    aoColumns: columns,
    columnDefs: [
      {
        visible: false,
        targets: 0
      },
      {
        orderable: false, // Disable sorting on all columns
        targets: '_all'
      }
    ],
    rowCallback: function(row, data) {
      $(row).find('td:eq(0)').css('text-align', 'right');
    }
  });
}

async function loadCubeDatatable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateCubeDataTable(newdata, table) {
  try {
    return new Promise((resolve, reject) => {
      let loadnew = true;
      if ($.fn.dataTable.isDataTable('#' + table)) {
        $('#' + table).DataTable().rows(function(idx, data, node) {
          loadnew = false;
          if (newdata.length > 0) {
            $.each(newdata, function() {
              if (data.Name === this.Name) {
                $('#' + table).DataTable().row(node).data(this).draw().invalidate();
              }
            });
          }
        });
        if (loadnew) {
          loadCubeDatatable(newdata, table);
        }
      }
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function loadCubeInfo(data) {
  try {
    hideSidebarLayerDivs();
    $('div[id=cubeAssignment_div]').css('display', 'block');
    $('div[id=cubeAssignment_div]').attr('data-id', data.id);
    $('button[name=editCube]').attr('id', data.id);
    $('button[name=fetchAssignData]').attr('id', data.ein);
    if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
      $('button[name=editCube]').css('display', 'block');
      $('button[name=fetchAssignData]').css('display', 'block');
    }
    let dataArray = await formatCubeData(data);

    await updateCubeDataTable(dataArray, cubeAssignInfoTable);
    sidebar.open('home');
  } catch (error) {
    // handle error
    console.error(e);
  }
}
async function formatCubeData(data) {
  let dataArray = [];
  $.each(data, function(key) {
    if (data.hasOwnProperty('name') && /^name$/i.test(key)) {
      dataArray.push({ order: 1, Name: 'Dept Name:', Value: data.name });
    }
    if (data.hasOwnProperty('number') && /^number$/i.test(key)) {
      dataArray.push({ order: 2, Name: 'Cube Number:', Value: data.number });
    }
    if (data.hasOwnProperty('assignTo') && /^assignTo$/i.test(key)) {
      dataArray.push({ order: 3, Name: 'Assign To:', Value: data.assignTo });
    }
  });
  return dataArray;
}
async function fetchBadgeScanHistory(id) {
  try {
    let scanList = await $.ajax({
      url: `http://smsmonitor.usps.gov/SMSWrapper/api/LogViewer/ScanHistory?ein=${id}&fdbid=${siteInfo.financeNumber}&limit=10`,
      type: 'GET',
      dataType: 'json',
      headers: {
        Accept: '*/*' // Add this line
      },
      success: async function(data) {
        if (data.length === 0) {
          await loadBadgeScanLogs(scanList);
          return;
        }
      },
      error: function(error) {
        console.log(error);
      },
      faulure: function(fail) {
        console.log(fail);
      }
    });
  } catch (e) {
    console.error('Error fetching badge scan history:', e);
  }
}
async function loadBadgeScanLogs(scanList) {
  try {
    // Create an array to hold the new data objects
    let newData = scanList.map(rawRing => {
      return {
        id: rawRing.sourceTranId,
        tranDateTime: `${rawRing.tranInfo.tranDate} ${rawRing.tranInfo.tranTime}`,
        tranTime: rawRing.tranInfo.tranTime,
        tranCode: rawRing.tranInfo.tranCode,
        operationId: rawRing.ringInfo.operationId
      };
    });

    // Pass the new data to updateRawRingsDataTable
    await updateBadgeScanDataTable(newData, tacsDataTable);
  } catch (e) {
    console.error('Error:', e);
  }
}
async function loadBadgeScanDataTable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateBadgeScanDataTable(newData, table) {
  try {
    return new Promise((resolve, reject) => {
      if ($.fn.dataTable.isDataTable('#' + table)) {
        // Clear the existing data before adding new data
        $('#' + table).DataTable().clear().draw();

        // Add the new data
        $('#' + table).DataTable().rows.add(newData).draw();
      } else {
        // If the table is not initialized, initialize it with the new data
        loadBadgeScanLogs(newData, table);
      }
      resolve();
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}

function constructScans() {
  let columns = [];
  var column0 = {
    //first column is always the name
    title: 'OPN Code',
    data: 'operationId',
    width: '20%'
  };
  var column1 = {
    //first column is always the name
    title: 'Event',
    data: 'tranCode',
    width: '15%',
    mRender: function(data, type, full) {
      if (data === '010') {
        return `BT (${data})`;
      } else if (data === '012') {
        return `OL (${data})`;
      } else if (data === '013') {
        return `IL (${data})`;
      } else if (data === '014') {
        return `ET (${data})`;
      } else if (data === '011') {
        return `MV (${data})`;
      } else {
        return data;
      }
    }
  };
  var column2 = {
    //first column is always the name
    title: 'Date & Time',
    data: 'tranDateTime',
    width: '30%'
  };
  var column3 = {
    //first column is always the name
    title: 'Duration (hours)',
    data: null,
    width: '30%'
  };
  columns[0] = column0;
  columns[1] = column1;
  columns[2] = column2;
  columns[3] = column3;

  return columns;
}
function createBadgeScanDatatable(table) {
  try {
    $('#' + table).DataTable({
      dom: 'Bfrtip',
      bFilter: false,
      bdeferRender: true,
      bpaging: false,
      bPaginate: false,
      autoWidth: false,
      bInfo: false,
      ordering: false,
      destroy: true,
      scroller: true,
      language: {
        zeroRecords: 'No Data',
        emptyTable: 'No Badge Scan Log'
      },
      order: [[]],
      aoColumns: constructScans(),
      rowCallback: function(row, data, index) {
        // Disable sorting for all columns
        const tableApi = this.api();
        const totalRows = tableApi.data().count();

        // Retrieve the text from the second column (index 1)
        const secondColumnText = $('td:eq(1)', row).text().trim().toUpperCase();

        // Check if the second column contains "BT (010)"
        if (secondColumnText === 'BT (010)') {
          // Leave the duration column empty
          $('td:eq(3)', row).html('');
        } else if (index < totalRows - 1) {
          // Get the next row's data (since data is reversed)
          const nextRowData = tableApi.row(index + 1).data();

          if (nextRowData) {
            // Parse tranTime values
            const nextTranTime = parseFloat(nextRowData.tranTime);
            const currentTranTime = parseFloat(data.tranTime);

            // Calculate duration: currentTranTime - nextTranTime
            const duration = currentTranTime - nextTranTime;

            // Check if duration is a valid number
            if (!isNaN(duration)) {
              // Display the duration with two decimal places
              $('td:eq(3)', row).html(duration.toFixed(2));
            } else {
              // If duration is not a number, leave it blank
              $('td:eq(3)', row).html('');
            }
          } else {
            // If next row data is not available, leave duration blank
            $('td:eq(3)', row).html('');
          }
        } else {
          // For the last row, no next row exists, leave duration blank
          $('td:eq(3)', row).html('');
        }
      }
    });
  } catch (e) {
    console.log('Error fetching machine info: ', e);
  }
}
async function EditCube(id) {
  try {
    /* close the sidebar */
    sidebar.close();
    const leafletIds = await findCubeLeafletIds(id);
    let data = geoZoneCube._layers[leafletIds].feature.properties;
    $('input[id=CubeZone_id]').val(data.id);
    $('input[id=cubeDeptName]').val(data.name);
    $('input[id=cubeNumber]').val(data.number);
    $.ajax({
      url: SiteURLconstructor(window.location) + '/api/EmpSchedule/EmployeesList',
      contentType: 'application/json',
      type: 'GET',
      success: function(empdata) {
        if (empdata.length > 0) {
          $('select[id=assignedEmp]').empty();
          //sort
          empdata.sort((a, b) => a.name.localeCompare(b.name));
          $.each(empdata, function() {
            $('<option/>').val(this.id).html(capitalize_Words(this.name)).appendTo('select[id=assignedEmp]');
          });
        }
      },
      error: function(error) {
        console.log(error);
      },
      faulure: function(fail) {
        console.log(fail);
      },
      complete: function(complete) {
        $('select[id=assignedEmp]').val(data.ein);
      }
    });

    $('button[id=CubeSubmitBtn]').off().on('click', function() {
      let jsonObject = {
        id: $('input[id=CubeZone_id]').val(),
        name: $('input[id=cubeDeptName]').val(),
        number: $('input[id=cubeNumber]').val(),
        ein: $('select[name=assignedEmp] option:selected').val(),
        assignTo: $('select[name=assignedEmp] option:selected').text(),
        type: 'Cube'
      };
      //make a ajax call to get the Connection details
      fetch(`../api/Zone/Update`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json-patch+json'
        },
        body: JSON.stringify(jsonObject)
      })
        .then(response => {
          if (!response.ok) {
            return response.json().then(error => {
              throw new Error(error);
            });
          }
          return response.json();
        })
        .then(async data => {
          await updateCubeDataTable(await formatCubeData(data.properties), cubeAssignInfoTable);
          await findCubeLeafletIds(data.properties.id).then(leafletIds => {
            geoZoneCube._layers[leafletIds].properties = data.properties;
          });
          //update leaflet layer tooltip
          geoZoneCube._layers[leafletIds].bindTooltip(`Dept: ${data.properties.name}<br>Cube #: ${data.properties.number}<br>${data.properties.assignTo}`, {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1,
            className: 'fixed-tooltip location '
          });

          setTimeout(function() {
            $('#Cube_Modal').modal('hide');
            sidebar.open('home');
          }, 500);
        })
        .catch(error => {
          $('span[id=errorCrsKioskSubmitBtn]').text(error.message);
          console.error('Error:', error);
        });
    });

    $('#Cube_Modal').modal('show');
  } catch (e) {
    console.error('Error fetching machine info: ', error);
  }
}
