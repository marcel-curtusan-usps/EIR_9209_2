connection.on('updateAPPosition', async data => {
  let apdata = JSON.parse(data);
  if (tagdata.properties.visible) {
    Promise.all([addAPFeature(apdata)]);
  } else {
    Promise.all([deleteAPFeature(apdata)]);
  }
});
connection.on('updateAPInfo', async apdata => {
  if (apdata.properties.visible) {
    Promise.all([updateAPFeature(apdata)]);
  } else {
    Promise.all([deleteAPFeature(apdata)]);
  }
});
connection.on('deleteAPInfo', async apdata => {
  if (apdata.properties.visible) {
    Promise.all([updateAPFeature(apdata)]);
  } else {
    Promise.all([deleteAPFeature(apdata)]);
  }
});
let accessPoints = new L.geoJson(null, {
  pointToLayer: function(feature, latlng) {
    let icon = L.divIcon({
      className: 'bi-wifi text-primary h3',
      iconSize: [20, 42],
      iconAnchor: [15, 0]
    });
    return L.marker(latlng, {
      icon: icon,
      riseOnHover: true,
      bubblingMouseEvents: true,
      popupOpen: true
    });
  },
  onEachFeature: function(feature, layer) {
    layer.markerId = feature.properties.id;
    layer.on('click', function(e) {
      //makea ajax call to get the employee details
      $.ajax({
        url: SiteURLconstructor(window.location) + '/api/Tag/GetTagByTagId?tagId=' + feature.properties.id,
        type: 'GET',
        success: function(data) {
          if ('Message' in data) {
            //display error message
          } else {
            $('button[name="tagEdit"]').attr('data-id', feature.properties.id);
            Promise.all([hidestafftables()]);
            $('div[id=div_taginfo]').css('display', '');
            data.properties.posAge = feature.properties.posAge;
            data.properties.locationMovementStatus = feature.properties.locationMovementStatus;
            updateTagDataTable(formattagdata(data.properties), 'tagInfotable');
            sidebar.open('reports');
          }
        },
        error: function(error) {
          console.log(error);
        },
        faulure: function(fail) {
          console.log(fail);
        },
        complete: function(complete) {}
      });
    });
    layer
      .bindTooltip('', {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 0
      })
      .openTooltip();
  }
});
// add to the map and layers control
let overlayAPLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(overlayAPLayer, 'AP');
accessPoints.addTo(overlayAPLayer);
async function hidestafftables() {
  $('div[id=div_taginfo]').css('display', '');
  $('div[id=div_userinfo]').css('display', 'none');
  $('div[id=div_overtimeinfo]').css('display', 'none');
  $('div[id=div_staffinfo]').css('display', 'none');
}
async function findAPLeafletIds(markerId) {
  return new Promise((resolve, reject) => {
    accessPoints.eachLayer(function(layer) {
      if (layer.markerId === markerId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given markerId'));
  });
}
async function init_accessPoints() {
  try {
    connection
      .invoke('GetAccessPoints')
      .then(function(data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addAPFeature(data[i])]);
        }
      })
      .catch(function(err) {
        // handle error
        console.error(err);
      });

    $(document).on('change', '.leaflet-control-layers-selector', function() {
      let sp = this.nextElementSibling;
      if (/^(AP)$/ig.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'AP').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'AP').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    await addGroupToList('AP');
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteAPFeature(data, floorId) {
  try {
    await findAPLeafletIds(data.properties.id)
      .then(leafletIds => {
        //remove from accessPoints
        accessPoints.removeLayer(leafletIds);
      })
      .catch(error => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function addAPFeature(data) {
  try {
    await findAPLeafletIds(data.properties.id)
      .then(leafletIds => {
        Promise.all([UpdateAPposition(leafletIds, data.geometry.coordinates[1], data.geometry.coordinates[0])]);
      })
      .catch(error => {
        accessPoints.addData(data);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function updateAPFeature(data) {
  try {
    let tag = data;
    await findAPLeafletIds(tag.properties.id)
      .then(leafletIds => {
        accessPoints._layers[leafletIds].feature.properties = tag.properties;
        accessPoints._layers[leafletIds]
          .bindTooltip('', {
            permanent: true,
            interactive: true,
            direction: 'center'
          })
          .openTooltip();
      })
      .catch(error => {
        //
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function UpdateAPposition(leafletId, lat, lag) {
  return new Promise((resolve, reject) => {
    if (accessPoints._layers[leafletId].getLatLng().distanceTo(new L.LatLng(lat, lag)) > 3000000) {
      accessPoints._layers[leafletId].setLatLng(new L.LatLng(lat, lag));
      resolve();
      return false;
    } else {
      accessPoints._layers[leafletId].slideTo(new L.LatLng(lat, lag), { duration: 10000 });
      resolve();
      return false;
    }
  });
}
function getmarkerType(type) {
  try {
    if (/^supervisor/ig.test(type)) {
      return 'persontag_supervisor ';
    } else if (/^maintenance/ig.test(type)) {
      return 'persontag_maintenance ';
    } else if (/^(LABORER CUSTODIAL|CUSTODIAN|CUTODIAN|Custodian|Custodian)/ig.test(type)) {
      return 'persontag_custodial ';
    } else if (/inplantsupport/ig.test(type)) {
      //else if (/pse/ig.test(type)) {
      //    return 'persontag_pse ';
      //}
      return 'persontag_inplantsupport ';
    } else if (/^(clerk|mailhandler|mha|mail|pse)/ig.test(type)) {
      return 'persontag ';
    } else if (type.length === 0) {
      return 'persontag_unknown ';
    } else {
      return 'persontag_unknown ';
    }
  } catch (e) {
    return 'persontag ';
  }
}
