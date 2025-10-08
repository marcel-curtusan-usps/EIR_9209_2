connection.on('updateAutonomousVehicleTagPosition', async tagdata => {
  try {
    if (tagdata.properties.visible) {
      Promise.all([addAGVFeature(tagdata)]);
    }
  } catch (e) {
    throw new Error(e.toString());
  }
});

connection.on('UpdateAGVTagInfo', async data => {
  if (tagdata.properties.visible) {
    Promise.all([addAGVFeature(tagdata)]);
  } else {
    Promise.all([deleteAGVFeature(tagdata)]);
  }
});
let tagsAGVVehicles = new L.GeoJSON(null, {
  pointToLayer: function(feature, latlng) {
    let vehicleIcon = L.divIcon({
      id: feature.properties.id,
      className: get_pi_icon(feature.properties.name, feature.properties.type) + ' iconXSmall',
      html: '<i>' + '<span class="path1"></span>' + '<span class="path2"></span>' + '<span class="path3"></span>' + '<span class="path4"></span>' + '<span class="path5"></span>' + '<span class="path6"></span>' + '<span class="path7"></span>' + '<span class="path8"></span>' + '<span class="path9"></span>' + '<span class="path10"></span>' + '</i>'
    });
    return L.marker(latlng, {
      icon: vehicleIcon,
      title: feature.properties.name,
      riseOnHover: true,
      bubblingMouseEvents: true,
      popupOpen: true
    });
  },
  onEachFeature: function(feature, layer) {
    layer.markerId = feature.properties.id;
    let VisiblefillOpacity = feature.properties.visible ? '' : 'tooltip-hidden';
    let obstructedState = '';
    layer.on('click', function(e) {
      //makea ajax call to get the employee details
      loadAgvTagData(feature.properties);
    });
    //how do replace using regex
    if (feature.properties.name) {
      layer
        .bindTooltip(feature.properties.name.replace(/[^0-9.]/g, '').replace(/^0+/, ''), {
          permanent: true,
          interactive: true,
          direction: 'top',
          opacity: 0.9,
          className: 'vehiclenumber ' + obstructedState
        })
        .openTooltip();
    }
  }
});
// add to the map and layers control
let overAgvLayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(overAgvLayLayer, 'AGV Vehicles');
tagsAGVVehicles.addTo(overAgvLayLayer);
async function findAGVLeafletIds(markerId) {
  return new Promise((resolve, reject) => {
    tagsAGVVehicles.eachLayer(function(layer) {
      if (layer.markerId === markerId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given markerId'));
  });
}
async function init_tagsAGV() {
  try {
    createAGVDataTable('vehicleInfoTable');
    $(document).on('change', '.leaflet-control-layers-selector', async function() {
      let sp = this.nextElementSibling;
      if (/^AGV Vehicles/ig.test(sp.innerHTML.trim())) {
        if (this.checked) {
          await addGroupToList('AutonomousVehicle');
        } else {
          await removeFromGroupList('AutonomousVehicle');
        }
      }
    });
    await addGroupToList('AutonomousVehicle');
    if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
      $('button[id=vehicleinfoedit]').off().on('click', function() {
        /* close the sidebar */
        sidebar.close();
        var id = $(this).attr('data-id');
        if (checkValue(id)) {
          Promise.all([tagEditInfo(id)]);
        }
      });
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteAGVFeature(data, floorId) {
  try {
    await findAGVLeafletIds(data.properties.id)
      .then(leafletIds => {
        //remove from tagsEmployees
        tagsAGVVehicles.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function addAGVFeature(data) {
  try {
    await findAGVLeafletIds(data.properties.id)
      .then(leafletIds => {
        Promise.all([AGVpositionUpdate(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
      })
      .catch(error => {
        tagsAGVVehicles.addData(data);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function AGVpositionUpdate(leafletId, lat, lag) {
  return new Promise((resolve, reject) => {
    if (tagsAGVVehicles._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
      tagsAGVVehicles._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
      resolve();
      return false;
    } else {
      tagsAGVVehicles._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
      resolve();
      return false;
    }
  });
}
async function updateFeature(data) {
  try {
    let tag = data;
    await findAGVLeafletIds(tag.properties.id)
      .then(leafletIds => {
        let VisiblefillOpacity = tag.properties.visible ? '' : 'tooltip-hidden';
        let classname = getmarkerType(tag.properties.craftName) + VisiblefillOpacity;

        tagsAGVVehicles._layers[leafletIds].feature.properties = tag.properties;
        tagsAGVVehicles._layers[leafletIds]
          .bindTooltip('', {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1,
            className: classname
          })
          .openTooltip();
      })
      .catch(error => {
        console.info('Error:', error);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function loadAgvTagData(tagdata) {
  try {
    if (/^(Admin|Maintenance|OIE)/i.test(appData.Role)) {
      $('button[id=vehicleInfoTable]').css('display', 'block');
      $('button[id="vehicleinfoedit"]').attr('data-id', tagdata.id);
    }
    if (/tagedit/ig.test(this.name)) {
      Promise.all([tagEditInfo(rowData.tagid)]);
    }
    let dataArray = [];
    $.each(tagdata, function(key) {
      let tabledataObject = {};
      if (tagdata.hasOwnProperty('name')) {
        if (/^name$/i.test(key)) {
          tabledataObject = { order: 1, Name: 'Name', Value: tagdata.name };
          dataArray.push(tabledataObject);
        }
      }
      if (tagdata.hasOwnProperty('id')) {
        if (/^id$/i.test(key)) {
          tabledataObject = { order: 2, Name: 'Tag Id', Value: tagdata.id };
          dataArray.push(tabledataObject);
        }
      }
    });
    await updateAGVDataTable(dataArray, 'vehicleInfoTable');
    sidebar.open('vehicleinfo');
    // await $.ajax({
    //   url: SiteURLconstructor(window.location) + '/api/Tag/GetTagByTagId?tagId=' + tagdata.id,
    //   type: 'GET',
    //   success: function(data) {
    //     Promise.all([hidestafftables()]);
    //     //$('div[id=div_taginfo]').css('display', '');
    //     //data.properties.posAge = feature.properties.posAge;
    //     //data.properties.locationMovementStatus = feature.properties.locationMovementStatus;
    //     //updateTagDataTable(formattagdata(data.properties), "tagInfotable");
    //     sidebar.open('vehicleinfo');
    //   },
    //   error: function(error) {
    //     console.log(error);
    //   },
    //   faulure: function(fail) {
    //     console.log(fail);
    //   },
    //   complete: function(complete) {}
    // });
  } catch (error) {
    console.info('Error loading AGV tag data:', error);
    throw new Error(error.toString());
  }
}
async function createAGVDataTable(table) {
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
async function loadAGVDatatable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateAGVDataTable(newdata, table) {
  try {
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
        loadAGVDatatable(newdata, table);
      }
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
