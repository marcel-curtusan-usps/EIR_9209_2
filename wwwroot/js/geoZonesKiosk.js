let geoZoneKiosk = new L.GeoJSON(null, {
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
    layer.on('click', function(e) {
      if (e.sourceTarget.hasOwnProperty('_content')) {
        OSLmap.setView(e.sourceTarget._latlng, 4);
      } else {
        OSLmap.setView(e.sourceTarget.getCenter(), 4);
      }
      Promise.all([loadKioskData(feature.properties)]);
    });
    layer
      .bindTooltip(feature.properties.name + '-' + feature.properties.number, {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 1,
        className: 'location '
      })
      .openTooltip();
  },
  filter: function(feature) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneKioskoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneKioskoverlayLayer, 'Kiosk Zones');
geoZoneKiosk.addTo(geoZoneKioskoverlayLayer);

async function findKioskZoneLeafletIds(zoneId) {
  return new Promise((resolve, reject) => {
    geoZoneKiosk.eachLayer(function(layer) {
      if (layer.zoneId === zoneId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given MPE Zone Id: ' + zoneId));
  });
}
async function init_geoZoneKiosk(floorId) {
  try {
    //load Kiosk Zones
    const KioskZonedata = await $.ajax({
      url: `${SiteURLconstructor(window.location)}/api/Zone/ZonesTypeByFloorId?floorId=${floorId}&type=Kiosk`,
      contentType: 'application/json',
      type: 'GET',
      success: function(data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addKioskFeature(data[i])]);
        }
      }
    });
    $(document).on('change', '.leaflet-control-layers-selector', function(_e) {
      let sp = this.nextElementSibling;
      if (/^(Kiosk Zones)$/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'Kiosk').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'Kiosk').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    addGroupToList('Kiosk');
    if (/^(admin)/i.test(appData.Role)) {
      $('button[name=editcrsKiosk]').off().on('click', function() {
        var id = $(this).attr('id');
        if (checkValue(id)) {
          Promise.all([EditCrsKioskInfo(id)]);
        }
      });
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
connection.on('addKioskzone', async zoneDate => {
  addKioskFeature(zoneDate);
});
connection.on('deleteKioskzone', async zoneDate => {
  deleteKioskFeature(zoneDate);
});
connection.on('updateKioskzone', async KioskZonedata => {
  try {
    await findKioskZoneLeafletIds(KioskZonedata.properties.id).then(leafletIds => {
      // Assuming mpeZonedata is the object containing the new properties
      const newProperties = KioskZonedata.properties;

      // Loop through the properties and update the values
      for (const key in newProperties) {
        if (key !== 'id') {
          if (key == 'name') {
            // Check if the property name is different and update the tooltip
            geoZoneKiosk._layers[leafletIds].feature.properties[key] = newProperties[key];
            geoZoneKiosk._layers[leafletIds].setTooltipContent(newProperties[key] + '-' + newProperties['number']);
          } else {
            geoZoneKiosk._layers[leafletIds].feature.properties[key] = newProperties[key];
          }
        }
      }
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});
async function addKioskFeature(data) {
  try {
    await findKioskZoneLeafletIds(data.properties.id).then(leafletIds => {}).catch(error => {
      geoZoneKiosk.addData(data);
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteKioskFeature(data) {
  try {
    await findKioskZoneLeafletIds(data.properties.id)
      .then(leafletIds => {
        geoZoneKiosk.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function EditCrsKioskInfo(id) {
  try {
    /* close the sidebar */
    sidebar.close();
    const leafletIds = await findKioskZoneLeafletIds(id);
    Data = geoZoneKiosk._layers[leafletIds].feature.properties;
    $('input[id=zone_id]').val(Data.id);
    $('input[id=crsioskName]').val(Data.name);
    $('input[id=crsKioskNumber]').val(Data.number);
    $('input[id=crsDeviceId]').val(Data.deviceId);
    $('select[id=zone_Type]').val(Data.type);
    $('button[id=CrsKioskSubmitBtn]').off().on('click', function() {
      let jsonObject = {
        id: $('input[id=zone_id]').val(),
        name: $('input[id=crsioskName]').val(),
        number: $('input[id=crsKioskNumber]').val(),
        deviceId: $('input[id=crsDeviceId]').val(),
        type: $('select[name=zone_Type] option:selected').val()
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
          //await updateCubeDataTable(await formatCubeData(data.properties), cubeAssignInfoTable);
          await findKioskZoneLeafletIds(data.properties.id).then(leafletIds => {
            geoZoneKiosk._layers[leafletIds].properties = data.properties;
          });
          //update leaflet layer tooltip
          geoZoneKiosk._layers[leafletIds].bindTooltip(`${data.properties.name} - ${data.properties.number}`, {
            permanent: true,
            interactive: true,
            direction: 'center',
            opacity: 1,
            className: 'fixed-tooltip location '
          });
          setTimeout(function() {
            $('#CRSKiosk_Modal').modal('hide');
            sidebar.open('home');
          }, 500);
        })
        .catch(error => {
          $('span[id=errorCrsKioskSubmitBtn]').text(error.message);
          console.error('Error:', error);
        });
    });

    $('#CRSKiosk_Modal').modal('show');
  } catch (e) {
    console.error('Error fetching machine info: ', error);
  }
}
async function loadKioskData(data) {
  hideSidebarLayerDivs();
  $('span[name=viewcrsKiosk]').empty();
  $('div[id=crsKiosk_div]').attr('data-id', data.id);
  $('button[name=editcrsKiosk]').attr('id', data.id);
  $('div[id=crsKiosk_div]').css('display', 'block');
  $('#CRSKiosk_ModalHeader_ID').text('Edit CRS Kiosk: ' + data.name);

  if (/^(Admin|Maintenance)/i.test(appData.Role)) {
    $('button[name=editcrsKiosk]').css('display', 'block');
  }
  let KioskName = data.name + '-' + data.number;
  $('<a/>').attr({ target: '_blank', href: SiteURLconstructor(window.location) + '/CRS/default.html?kioskId=' + KioskName, style: 'color:white;' }).html('View').appendTo($('span[name=viewcrsKiosk]'));
  sidebar.open('home');
}
