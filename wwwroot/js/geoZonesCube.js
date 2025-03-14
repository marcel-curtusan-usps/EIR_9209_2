//on close clear all inputs
$('#Zone_Modal').on('hidden.bs.modal', function () {
  $(this).find('input[type=text],textCube,select').css({ 'border-color': '#D3D3D3' }).val('').end().find('span[class=text]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).change();
});
//on open set rules
$('#Zone_Modal').on('shown.bs.modal', function () {});
let geoZoneCube = new L.GeoJSON(null, {
  style: function (feature) {
    return {
      weight: 1,
      opacity: 1,
      color: '#989ea4',
      fillOpacity: 0.2,
      lastOpacity: 0.2
    };
  },
  onEachFeature: function (feature, layer) {
    layer.zoneId = feature.properties.id;
    layer.on('click', async function (e) {
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
        className: 'location '
      })
      .openTooltip();
  },
  filter: function (feature, layer) {
    return feature.properties.visible;
  }
});

// add to the map and layers control
let geoZoneCubeoverlayLayer = L.layerGroup().addTo(OSLmap);
layersControl.addOverlay(geoZoneCubeoverlayLayer, 'Cube Zones');
geoZoneCube.addTo(geoZoneCubeoverlayLayer);

async function findCubeLeafletIds(zoneId) {
  return new Promise((resolve, reject) => {
    geoZoneCube.eachLayer(function (layer) {
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
    await $.ajax({
      url: `${SiteURLconstructor(window.location)}/api/Zone/ZonesTypeByFloorId?floorId=${floorId}&type=Cube`,
      contentType: 'application/json',
      type: 'GET',
      success: function (data) {
        for (let i = 0; i < data.length; i++) {
          Promise.all([addCubeFeature(data[i])]);
        }
      }
    });
    $(document).on('change', '.leaflet-control-layers-selector', function (e) {
      let sp = this.nextElementSibling;
      if (/^(Cube Zones)$/gi.test(sp.innerHTML.trim())) {
        if (this.checked) {
          connection.invoke('JoinGroup', 'Cube').catch(function (err) {
            return console.error(err.toString());
          });
        } else {
          connection.invoke('LeaveGroup', 'Cube').catch(function (err) {
            return console.error(err.toString());
          });
        }
      }
    });
    connection.invoke('JoinGroup', 'Cube').catch(function (err) {
      return console.error(err.toString());
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
connection.on('addCubezone', async (zoneDate) => {
  Promise.all([addCubeFeature(zoneDate)]);
});
connection.on('deleteCubezone', async (zoneDate) => {
  Promise.all([deleteCubeFeature(zoneDate)]);
});
connection.on('updateCubezone', async (mpeZonedata) => {
  await findCubeLeafletIds(mpeZonedata.properties.id).then((leafletIds) => {
    geoZoneCube._layers[leafletIds].properties = mpeZonedata.properties;
  });
});
async function addCubeFeature(data) {
  try {
    await findCubeLeafletIds(data.properties.id)
      .then((leafletIds) => {})
      .catch((error) => {
        geoZoneCube.addData(data);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteCubeFeature(data) {
  try {
    await findCubeLeafletIds(data.properties.id)
      .then((leafletIds) => {
        geoZoneCube.removeLayer(leafletIds);
      })
      .catch((error) => {});
  } catch (e) {
    throw new Error(e.toString());
  }
}

async function loadCubeInfo(data) {
  try {
    hideSidebarLayerDivs();
    $('div[id=cubeAssignment_div]').css('display', 'block');
    sidebar.open('home');
  } catch (error) {
    // handle error
    console.error(e);
  }
}
