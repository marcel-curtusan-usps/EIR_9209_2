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
      $.ajax({
        url: SiteURLconstructor(window.location) + '/api/Tag/GetTagByTagId?tagId=' + feature.properties.id,
        type: 'GET',
        success: function(data) {
          //$('button[name="tagEdit"]').attr('data-id', feature.properties.id);
          //Promise.all([hidestafftables()]);
          //$('div[id=div_taginfo]').css('display', '');
          //data.properties.posAge = feature.properties.posAge;
          //data.properties.locationMovementStatus = feature.properties.locationMovementStatus;
          //updateTagDataTable(formattagdata(data.properties), "tagInfotable");
          sidebar.open('vehicleinfo');
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
    //load PIV Tags
    // connection
    //   .invoke('GetAGVTags')
    //   .then(function(data) {
    //     //add AGV markers to the layer
    //     for (let i = 0; i < data.length; i++) {
    //       Promise.all([addAGVFeature(data[i])]);
    //     }
    //   })
    //   .catch(function(err) {
    //     // handle error
    //     console.error(err);
    //   });
    $(document).on('change', '.leaflet-control-layers-selector', function() {
      let sp = this.nextElementSibling;
      if (/^AGV Vehicles/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'AutonomousVehicle').catch(function(err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'AutonomousVehicle').catch(function(err) {
            return console.error(err.toString());
          });
        }
      }
    });
    await addGroupToList('AutonomousVehicle');
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
        //
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
