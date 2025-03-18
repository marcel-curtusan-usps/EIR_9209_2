let cubeAssignInfoTable = 'cubeAssignInfoTable';

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
      .openTooltip();
  },
  filter: function(feature, layer) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneCubeoverlayLayer = L.layerGroup().addTo(OSLmap);
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
    connection.invoke('JoinGroup', 'Cube').catch(function(err) {
      return console.error(err.toString());
    });
    if (/^(admin)/i.test(appData.Role)) {
      $('button[name=editCube]').off().on('click', function() {
        var id = $(this).attr('id');
        if (checkValue(id)) {
          Promise.all([EditCube(id)]);
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
    if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
      $('button[name=editCube]').css('display', 'block');
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
          //sort
          empdata.sort();
          $.each(empdata, function() {
            $('<option/>').val(this.id).html(this.name).appendTo('select[id=assignedEmp]');
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
