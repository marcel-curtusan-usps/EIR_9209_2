//on close clear all inputs
$('#Zone_Modal').on('hidden.bs.modal', function() {
  $(this).find('input[type=text],textarea,select').css({ 'border-color': '#D3D3D3' }).val('').end().find('span[class=text]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).change();
});
//on open set rules
$('#Zone_Modal').on('shown.bs.modal', function() {});
let geoZoneAGVLocation = new L.GeoJSON(null, {
  style: function(feature) {
    let inmission = '#989ea4';
    return {
      weight: 1,
      opacity: 1,
      color: '#3573b1',
      fillOpacity: 0.2,
      fillColor: inmission,
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
      Promise.all([LoadAGVLocationTables(feature.properties)]);
    });

    layer
      .bindTooltip(Get_location_Code(feature.properties.name), {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 1,
        className: 'location '
      })
      .openTooltip();
  },
  filter: function(feature, layer) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneAGVoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneAGVoverlayLayer, 'AGV Location Zones');
geoZoneAGVLocation.addTo(geoZoneAGVoverlayLayer);

async function findAGVZoneLeafletIds(zoneId) {
  return new Promise((resolve, reject) => {
    geoZoneAGVLocation.eachLayer(function(layer) {
      if (layer.zoneId === zoneId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given MPE Zone Id: ' + zoneId));
  });
}
async function init_geoZoneAGV() {
  try {
    const agvZonedata = await $.ajax({
      url: `${SiteURLconstructor(window.location)}/api/Zone/ZoneType?type=AGVLocation`,
      contentType: 'application/json',
      type: 'GET',
      success: function(data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addAGVLocationFeature(data[i])]);
        }
      }
    });
    $(document).on('change', '.leaflet-control-layers-selector', function(e) {
      let sp = this.nextElementSibling;
      if (/^(AGV Location)/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'AGVLocation').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'AGVLocation').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    await addGroupToList('AGVLocation');
  } catch (e) {
    throw new Error(e.toString());
  }
}
connection.on('addAGVLocationzone', async zoneDate => {
  Promise.all([addAGVLocationFeature(zoneDate)]);
});
connection.on('deleteAGVLocationzone', async zoneDate => {
  Promise.all([deleteAGVLocationFeature(zoneDate)]);
});
connection.on('updateAGVLocationzone', async mpeZonedata => {
  await findAGVZoneLeafletIds(mpeZonedata.properties.id).then(leafletIds => {
    geoZoneAGVLocation._layers[leafletIds].properties = mpeZonedata.properties;
  });
});
async function LoadAGVLocationTables(dataproperties) {
  try {
    hideSidebarLayerDivs();
    $('div[id=agvlocation_div]').attr('data-id', dataproperties.id);
    $('div[id=agvlocation_div]').css('display', 'block');
    $('span[name=locationid]').text(Get_location_Code(dataproperties.name));
    $('button[name=agvlocationinfoedit]').css('display', 'block');
    agvlocationinmission_Table_Body.empty();
    if (!$.isEmptyObject(dataproperties.MissionList)) {
      $.each(dataproperties.MissionList, function() {
        agvlocationinmission_Table_Body.append(agvlocation_row_inmission_template.supplant(formatagvlocationinmissionrow(this)));
      });
    }
    sidebar.open('home');
  } catch (e) {
    console.log(e);
  }
}
async function addAGVLocationFeature(data) {
  try {
    await findAGVZoneLeafletIds(data.properties.id).then(leafletIds => {}).catch(error => {
      geoZoneAGVLocation.addData(data);
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteAGVLocationFeature(data) {
  try {
    await findAGVZoneLeafletIds(data.properties.id)
      .then(leafletIds => {
        geoZoneAGVLocation.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
function Get_location_Code(location) {
  let Post = '';
  if (locationReg.test(location)) {
    let arr = location.match(/.{1,3}/g);
    if (arr.length === 4) {
      let a = checkValue(arr[0].replace(/^0+/, '')) ? arr[0].replace(/^0+/, '') : '0';
      let b = checkValue(arr[1].replace(/^0+/, '')) ? arr[1].replace(/^0+/, '') : '0';
      let c = checkValue(arr[2].replace(/^0+/, '')) ? arr[2].replace(/^0+/, '') : '0';
      let d = checkValue(arr[3].replace(/^0+/, '')) ? arr[3].replace(/^0+/, '') : '0';

      return a + b + ' - ' + c + ',' + d;
    }
  }
  return Post;
}
let locationReg = new RegExp('^[A-Z0-9]{3}[A-Z0-9]{3}[0-9]{3}[0-9]{3}$', 'i');

let agvlocationinmission_Table = $('table[id=locationinmissiontable]');
let agvlocationinmission_Table_Body = agvlocationinmission_Table.find('tbody');

let agvlocation_row_inmission_template =
  '<tr class="text-center" id={RequestId}>' +
  '<td class="font-weight-bold" colspan="2">' +
  '<h6 class="control-label sectionHeader ml-1 mb-1 d-flex justify-content-between">' +
  'Mission Status' +
  '<span class="d-flex justify-content-between">' +
  'Request ID:' +
  '<span class="btn btn-secondary border-0 badge-dark badge">{RequestId}</span>  ' +
  '</span>' +
  '</h6 >' +
  '</td>' +
  '<tr id=pickupLocation_{RequestId}><td>Vehicle Name:</td><td>{VehicleName}</td></tr>' +
  '<tr id=pickupLocation_{RequestId}><td>Pickup Location:</td><td>{PickupLocation}</td></tr>' +
  '<tr id=pickupLocationEta_{RequestId}><td>ETA to Pickup:</td><td>{PickupLocationEta}</td></tr>' +
  '<tr id=dropoffLocation_{RequestId}><td>Drop-Off Location:</td><td>{DropoffLocation}</td></tr>' +
  '<tr id=endLocation_{RequestId}><td>End Location:</td><td>{EndLocation}</td></tr>' +
  '<tr id=door_{RequestId}><td>Door:</td><td>{Door}</td></tr>' +
  '<tr id=placard_{RequestId}><td>Placard:</td><td>{Placard}</td></tr>' +
  '</tr>';
function formatagvlocationinmissionrow(properties) {
  return $.extend(properties, {
    RequestId: properties.RequestId,
    VehicleName: properties.Vehicle !== null ? properties.Vehicle : 'N/A',
    PickupLocation: properties.hasOwnProperty('Pickup_Location') ? Get_location_Code(properties.Pickup_Location) : 'N/A',
    DropoffLocation: properties.hasOwnProperty('Dropoff_Location') ? Get_location_Code(properties.Dropoff_Location) : 'N/A',
    EndLocation: properties.hasOwnProperty('End_Location') ? Get_location_Code(properties.End_Location) : 'N/A',
    PickupLocationEta: properties.ETA !== null ? properties.ETA : 'N/A',
    Door: properties.Door !== null ? properties.Door : 'N/A',
    Placard: properties.Placard !== null ? properties.Placard : 'N/A'
  });
}
