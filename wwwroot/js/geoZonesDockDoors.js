let geoZoneDockDoor = new L.GeoJSON(null, {
  style: function(feature) {
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
  onEachFeature: function(feature, layer) {
    layer.zoneId = feature.properties.id;
    //extract number from feature.properties.name and remove leading zeros
    let dockDoorNumber = parseInt(feature.properties.doorNumber.match(/\d+/g)[0], 10);
    let dockdookflash = GetDockDoorFlash(feature.properties);
    if (feature.properties.tripDirectionInd !== '') {
      dockDoorNumber += '-' + feature.properties.tripDirectionInd;
    }
    layer.on('click', function(e) {
      if (e.sourceTarget.hasOwnProperty('_content')) {
        OSLmap.setView(e.sourceTarget._latlng, 4);
      } else {
        OSLmap.setView(e.sourceTarget.getCenter(), 4);
      }
      Promise.all([LoadDockDoorTable(feature.properties)]);
    });
    layer
      .bindTooltip(dockDoorNumber.toString(), {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 0.9,
        className: 'dockdooknumber ' + dockdookflash
      })
      .openTooltip();
  },
  filter: function(feature, layer) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneDockDooroverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneDockDooroverlayLayer, 'Dock Door');
geoZoneDockDoor.addTo(geoZoneDockDooroverlayLayer);
connection.on('addDockDoorzone', async zoneDate => {
  addDockDoorFeature(zoneDate);
});
connection.on('deleteDockDoorzone', async zoneDate => {
  deleteDockDoorFeature(zoneDate);
});
connection.on('updateDockDoorzone', async zoneDate => {
  Promise.all([updateDockDoorFeature(zoneDate)]);
});
connection.on('updateDockDoorTrip', async data => {
  await findDockDoorZoneLeafletIds(data.zoneId).then(leafletIds => {
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
    await findDockDoorZoneLeafletIds(data.properties.id).then(leafletIds => {}).catch(error => {
      geoZoneDockDoor.addData(data);
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteDockDoorFeature(data) {
  try {
    await findDockDoorZoneLeafletIds(data.properties.id)
      .then(leafletIds => {
        geoZoneDockDoor.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function updateDockDoorFeature(data) {
  await findDockDoorZoneLeafletIds(data.id).then(leafletIds => {
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
      if (geoZoneDockDoor._layers[leafletIds].feature.properties.tripDirectionInd !== '') {
        dockDoorNumber += '-' + geoZoneDockDoor._layers[leafletIds].feature.properties.tripDirectionInd;
      }
      geoZoneDockDoor._layers[leafletIds].setTooltipContent(dockDoorNumber.toString()); // Ensure dockDoorNumber is a string
      if (data.tripDirectionInd === 'O' && data.tripMin > 0 && data.tripMin <= 30) {
        if (geoZoneDockDoor._layers[leafletIds].feature.properties.containersNotLoaded > 0) {
          if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty('_tooltip')) {
            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty('_container')) {
              if (!geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.add('doorflash');
              }
            }
          }
        } else {
          if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty('_tooltip')) {
            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty('_container')) {
              if (geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.remove('doorflash');
              }
            }
          }
        }
      }
      if (data.tripDirectionInd === 'I' && data.tripMin > 0 && data.tripMin <= 30) {
        if (geoZoneDockDoor._layers[leafletIds].feature.properties.containersNotLoaded > 0) {
          if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty('_tooltip')) {
            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty('_container')) {
              if (!geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.add('doorflash');
              }
            }
          }
        } else {
          if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty('_tooltip')) {
            if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty('_container')) {
              if (geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.contains('doorflash')) {
                geoZoneDockDoor._layers[leafletIds]._tooltip._container.classList.remove('doorflash');
              }
            }
          }
        }
      } else {
        if (geoZoneDockDoor._layers[leafletIds].hasOwnProperty('_tooltip')) {
          if (geoZoneDockDoor._layers[leafletIds]._tooltip.hasOwnProperty('_container')) {
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
    geoZoneDockDoor.eachLayer(function(layer) {
      if (layer.zoneId === zoneId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given DockDoor Zone Id'));
  });
}
async function init_geoZoneDockDoor(floorId) {
  try {
    //load the table
    createDockdoorDataTable('dockdoortable');

    await $.ajax({
      url: `${SiteURLconstructor(window.location)}/api/Zone/ZonesTypeByFloorId?floorId=${floorId}&type=DockDoor`,
      contentType: 'application/json',
      type: 'GET',
      success: function(data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addDockDoorFeature(data[i])]);
        }
      }
    });

    $(document).on('change', '.leaflet-control-layers-selector', function(e) {
      let sp = this.nextElementSibling;
      if (/^(Dock Door)/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'DockDoor').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'DockDoor').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    connection.invoke('JoinGroup', 'DockDoor').catch(function(err) {
      return console.error(err.toString());
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function LoadDockDoorTable(data) {
  try {
    dockdoorloaddata = [];
    hideSidebarLayerDivs();
    $('div[id=dockdoor_div]').attr('data-id', data.id);
    $('div[id=dockdoor_div]').css('display', 'block');
    $('div[id=dockdoor_tripdiv]').css('display', 'block');
    $('div[id=ctstabs_div]').css('display', 'none');
    $('div[id=trailer_div]').css('display', 'block');
    $('button[name=container_counts]').text(0 + '/' + 0);
    $('span[name=doornumberid]').text(data.doorNumber);
    $('select[id=tripSelector]').val('');
    $('span[name=doorview]').empty();
    $('button[name=dockdoorinfoedit]').attr('id', data.id);
    $('span[name=doorstatus]').text(data.status === '' ? 'Unknown' : data.status);
    if (data.externalUrl) {
      $('<a/>').attr({ target: '_blank', href: data.externalUrl, style: 'color:white;' }).html('View').appendTo($('span[name=doorview]'));
    } else {
      $('<a/>').attr({ target: '_blank', href: SiteURLconstructor(window.location) + '/Dockdoor/Dockdoor.aspx?DockDoor=' + data.doorNumber, style: 'color:white;' }).html('View').appendTo($('span[name=doorview]'));
    }
    let dataArray = [];
    $.each(data, function(key) {
      let tabledataObject = {};
      if (data.hasOwnProperty('legSiteName')) {
        if (/^legSiteName$/i.test(key)) {
          tabledataObject = {
            order: 1,
            Name: 'Site Name',
            Value: '(' + data.legSiteId + ') -- ' + data.legSiteName
          };
          dataArray.push(tabledataObject);
        }
      }
      if (data.hasOwnProperty('route')) {
        if (/^route$/i.test(key)) {
          tabledataObject = {
            order: 2,
            Name: 'Rout Trip',
            Value: data.route !== '' ? data.route + '-' + data.trip : ''
          };
          dataArray.push(tabledataObject);
        }
      }
      if (data.hasOwnProperty('tripDirectionInd')) {
        if (/^tripDirectionInd$/i.test(key)) {
          tabledataObject = {
            order: 3,
            Name: 'Direction',
            Value: data.tripDirectionInd === '' ? '' : data.tripDirectionInd === 'O' ? 'Out-bound' : 'In-bound'
          };
          dataArray.push(tabledataObject);
        }
      }
      if (data.hasOwnProperty('trailerBarcode')) {
        if (/^trailerBarcode/i.test(key)) {
          tabledataObject = {
            order: 3,
            Name: 'Trailer arcode',
            Value: data.trailerBarcode
          };
          dataArray.push(tabledataObject);
        }
      }
      if (data.hasOwnProperty('scheduledDtm')) {
        if (/^scheduledDtm/i.test(key)) {
          tabledataObject = {
            order: 4,
            Name: 'Scheduled',
            Value: vaildateTime(data.scheduledDtm)
          };
          dataArray.push(tabledataObject);
        }
      }
    });
    Promise.all([updateDockdoorDataTable(dataArray, 'dockdoortable')]);
    sidebar.open('home');
  } catch (e) {
    throw new Error(e.toString());
  }
}
function GetDockDoorFlash(data) {
  try {
    if (data.isTripAtDoor) {
      if (data.tripDirectionInd === 'O' && data.tripMin <= 30 && data.containersNotLoaded > 0) {
        return 'doorflash';
      } else {
        return '';
      }
    } else {
      return '';
    }
  } catch (e) {
    return '';
  }
}
function GetDockDoorZoneColor(data) {
  let activeTrip30MissingContainer = '#dc3545'; //red
  let activeTrip = '#3573b1'; //blue
  let notTrip = '#989ea4'; //clear
  try {
    if (data.isTripAtDoor)
      if (data.tripDirectionInd === 'O' && data.tripMin <= 30) {
        return activeTrip30MissingContainer;
      } else {
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
async function loadDockdoorDatatable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateDockdoorDataTable(newdata, table) {
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
          loadDockdoorDatatable(newdata, table);
        }
      }
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
function vaildateTime(data) {
  try {
    let est = luxon.DateTime.fromISO(data);
    //if (est._isValid && est.year === luxon.DateTime.local().year) {
    if (est.year && est.year === luxon.DateTime.local().year) {
      return est.toFormat('yyyy-MM-dd HH:mm:ss');
    } else {
      return 'Estimate Not Available';
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
