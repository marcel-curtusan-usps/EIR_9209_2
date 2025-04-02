//side bar setup
let sidebar = L.control.sidebar({
  autopan: false, // whether to maintain the centered map point when opening the sidebar
  closeButton: true, // whether t add a close button to the panes
  container: 'sidebar', // the DOM container or #ID of a predefined sidebar container that should be used
  position: 'left' // left or right
});
let mainfloorOverlays = L.layerGroup();

let mainfloor = L.imageOverlay(null, [0, 0], { id: -1, zIndex: -1 }).addTo(mainfloorOverlays);
let baseLayers = {
  'Main Floor': mainfloor
};

let overlayMaps = {};
let layersSelected = [mainfloor];
//var CRSPixel = L.Util.extend(L.CRS.Simple, {
//	transformation: new L.Transformation(1,0,1,0)
//});
//setup map
let OSLmap = L.map('map', {
  crs: L.CRS.Simple,
  renderer: L.canvas({ padding: 0.5 }),
  preferCanvas: true,
  pmIgnore: false,
  markerZoomAnimation: true,
  minZoom: 0,
  maxZoom: 18,
  zoomControl: false,
  measureControl: true,
  tap: false,
  layers: layersSelected
});
var lastZoom;
OSLmap.on('zoomend', function() {
  var zoom = OSLmap.getZoom();

  // Close tooltips if zoom level is between 1 and 4
  if (zoom >= 0 && zoom <= 2) {
    OSLmap.eachLayer(function(l) {
      if (l.getTooltip) {
        var toolTip = l.getTooltip();
        if (toolTip) {
          OSLmap.closeTooltip(toolTip);
        }
      }
    });
  }
  // Add tooltips if zoom level is above 5
  if (zoom >= 3) {
    OSLmap.eachLayer(function(l) {
      if (l.getTooltip) {
        var toolTip = l.getTooltip();
        if (toolTip) {
          OSLmap.addLayer(toolTip);
        }
      }
    });
  }
  lastZoom = zoom;
});

sidebar
  .on('content', function(ev) {
    sidebar.options.autopan = false;
    $('div[id=machine_div]').attr('data-id', '');
    switch (ev.id) {
      case 'autopan':
        break;
      case 'setting':
        break;
      case 'reports':
        break;
      case 'userprofile':
        break;
      case 'assetinventory':
        break;
      case 'vehicleinfo':
        break;
      case 'notificationsetup':
        break;
      case 'tripsnotificationinfo':
        break;
      default:
        sidebar.options.autopan = false;
        break;
    }
  })
  .addTo(OSLmap);
sidebar.on('closing', function(e) {
  // e.id contains the id of the opened panel
  $('div[id=machine_div]').attr('data-id', '');
});

// Add Layer Popover - Proposed
let layersControl = L.control
  .layers(baseLayers, overlayMaps, {
    sortLayers: true,
    sortFunction: function(layerA, layerB, nameA, nameB) {
      if (/FLOOR/i.test(nameA)) {
        if (/MAIN/i.test(nameA)) {
          return -1;
        } else {
          return nameA < nameB ? -1 : nameB < nameA ? 1 : 0;
        }
      }
    },
    position: 'bottomright',
    collapsed: false,
    autoZIndex: false // Disable expand on hover
  })
  .addTo(OSLmap);
// Function to handle baselayerchange event
async function onBaseLayerChange(e) {
  baselayerid = e.layer.options.id;
  geoZoneCube.clearLayers();
  await init_geoZoneCube(baselayerid);
  geoZoneArea.clearLayers();
  init_geoZoneArea(baselayerid);
  geoZoneMPE.clearLayers();
  init_geoZoneMPE(baselayerid);
  geoZoneKiosk.clearLayers();
  init_geoZoneKiosk(baselayerid);
  geoZoneDockDoor.clearLayers();
  init_geoZoneDockDoor(baselayerid);
  markerCameras.clearLayers();
  init_tagsCamera(baselayerid);
  OSLmap.setView(e.layer.getBounds().getCenter(), 1.5);
  console.info(`floorId: ${baselayerid}`);
}
// Add the event listener
OSLmap.on('baselayerchange', onBaseLayerChange);
// Add onclick event listener
layersControl.getContainer().onclick = function() {
  if (layersControl._container.classList.contains('leaflet-control-layers-expanded')) {
    layersControl._container.classList.remove('leaflet-control-layers-expanded');
  } else {
    layersControl._container.classList.add('leaflet-control-layers-expanded');
  }
};
//Add zoom button
new L.Control.Zoom({ position: 'bottomright' }).addTo(OSLmap);
//add View Ports
//L.easyButton({
//    position: 'bottomright',
//    states: [{
//        stateName: 'viewport',
//        icon: '<div id="viewportsToggle" data-toggle="popover"><i class="pi-iconViewport align-self-center" title="Viewports"></i></div>'
//    }]
//}).addTo(OSLmap);
async function updateOSLattribution(data) {
  OSLmap.attributionControl.setPrefix('USPS ' + data.ApplicationName + ' (' + data.ApplicationVersion + ') | ' + data.name);
}
// Add Layer Control Button
L.easyButton({
  position: 'bottomright',
  states: [
    {
      stateName: 'layer',
      icon: '<div id="layersToggle" data-toggle="layerPopover"><i class="pi-iconLayer align-self-center" title="Layer Controls"></i></div>'
    }
  ]
}).addTo(OSLmap);
//Add Site Summary Button
//createSiteSummaryDataTable('sitePerformanceData');
L.easyButton({
  position: 'topcenter',

  states: [
    {
      icon: '<strong class="bi bi-bar-chart-steps"></strong>',
      onClick: () => {
        if (/^(localhost)/i.test(window.location.hostname)) {
          window.open(window.location.origin + '/Reports/SitePerformance.html', '_blank');
        } else {
          window.open(window.location.origin + '/CF/SitePerformance.html', '_blank');
        }
      }
    }
  ]
}).addTo(OSLmap);
//Add staffing button
var staffBtn = L.easyButton({
  position: 'topcenter',
  states: [
    {
      stateName: 'openstaffing',
      icon: '<div class="row staffing-row">' + '<div class="col-sm-6 no-top-border">Schedule</div>' + '<div class="col-sm-6 no-top-border">WorkZone</div>' + '<div class="col-sm-6 no-bottom-border" id="schstaffingbutton" style="font-size:1.2vw">0</div>' + '<div class="col-sm-6 no-bottom-border" id="staffingbutton" style="font-size:1.2vw">0</div>' + '</div>',
      onClick: function(control) {
        Promise.all([showstaffdiv()]);
        sidebar.open('reports');
        control.state('closestaffing');
        // $('div[id=schstaffingbutton]').text(tagsscheduled);
      }
    },
    {
      stateName: 'closestaffing',
      icon: '<div class="row staffing-row">' + '<div class="col-sm-6 no-top-border">Schedule</div>' + '<div class="col-sm-6 no-top-border">WorkZone</div>' + '<div class="col-sm-6 no-bottom-border" id="schstaffingbutton" style="font-size:1.2vw">0</div>' + '<div class="col-sm-6 no-bottom-border" id="staffingbutton" style="font-size:1.2vw">0</div>' + '</div>',
      onClick: function(control) {
        Promise.all([hideTagdiv()]);
        sidebar.close('reports');
        control.state('openstaffing');
        // $('div[id=schstaffingbutton]').text(tagsscheduled);
      }
    }
  ]
});
async function hideTagdiv() {
  $('div[id=div_taginfo]').css('display', 'none');
}
async function showstaffdiv() {
  $('div[id=div_taginfo]').css('display', 'none');
  $('div[id=div_userinfo]').css('display', '');
  $('div[id=div_overtimeinfo]').css('display', '');
  $('div[id=div_staffinfo]').css('display', '');
}
staffBtn.button.style.width = '150px';
staffBtn.button.style.height = '70px';
staffBtn.addTo(OSLmap);
$('.leaflet-control-layers').addClass('layerPopover');
$('.layerPopover').attr('id', 'layersContent');
$('#layersContent').prepend('<div class="layersArrow"></div>');
$('.leaflet-control-layers').hide();
$('#layersToggle').on('click', function() {
  //Toggle layer Popover
  $('#layersContent').toggle();
  // close the sidebar
  sidebar.close();
  // close other popover
  $('[data-toggle=popover]').popover('hide');
  $('#twentyfourmessage').popover('hide');
});
let layerCheckboxIds = [];
function setLayerCheckboxId(thisCheckBox, innerHTML) {
  let name = innerHTML.replace(/ /g, '');
  thisCheckBox.id = name;
  layerCheckboxIds.push(thisCheckBox.id);
  return name;
}
async function init_backgroundImages() {
  try {
    $.ajax({
      url: SiteURLconstructor(window.location) + '/api/BackgroundImage/GetAllImages',
      contentType: 'application/json',
      type: 'GET',
      success: function(MapData) {
        if (MapData.length > 0) {
          // Sort MapData by index
          MapData.sort((a, b) => a.index - b.index);
          $.each(MapData, function(index, backgroundImages) {
            if (backgroundImages.source === 'Cisco') {
              L.Util.extend(L.CRS.Simple, {
                transformation: new L.Transformation(1, 0, 1, 0)
              });
            }

            loadOslDatatable([backgroundImages], 'osltable');
            if (!!backgroundImages) {
              //Promise.all([loadOslDatatable([this], "osltable")]);
              //set new image
              let trackingarea = L.polygon([[100, 150]], [[500, 5000]]);
              let img = new Image();
              //load Base64 image
              img.src = backgroundImages.base64;
              //create he bound of the image.
              let bounds = [[backgroundImages.yMeter, backgroundImages.xMeter], [backgroundImages.heightMeter + backgroundImages.yMeter, backgroundImages.widthMeter + backgroundImages.xMeter]];
              trackingarea = L.polygon(bounds, {});

              // Temporarily remove the event listener
              OSLmap.off('baselayerchange', onBaseLayerChange);

              if (index === 0) {
                baselayerid = backgroundImages.coordinateSystemId;
                mainfloor.options.id = backgroundImages.coordinateSystemId;
                mainfloor.setUrl(img.src);
                mainfloor.setZIndex(index);
                mainfloor.setBounds(trackingarea.getBounds());
                // Remove the old base layer from the layersControl
                layersControl.removeLayer(mainfloor);

                // Add the updated base layer to the layersControl
                layersControl.addBaseLayer(mainfloor, backgroundImages.name);
                //center image
                OSLmap.setView(trackingarea.getBounds().getCenter(), 1.5);

                geoZoneCube.clearLayers();
                init_geoZoneCube(baselayerid);
                geoZoneArea.clearLayers();
                init_geoZoneArea(baselayerid);
                geoZoneMPE.clearLayers();
                init_geoZoneMPE(baselayerid);
                geoZoneKiosk.clearLayers();
                init_geoZoneKiosk(baselayerid);
                geoZoneDockDoor.clearLayers();
                init_geoZoneDockDoor(baselayerid);
                markerCameras.clearLayers();
                init_tagsCamera(baselayerid);
              } else if (index > 0) {
                layersControl.addBaseLayer(
                  L.imageOverlay(img.src, trackingarea.getBounds(), {
                    id: this.id,
                    zindex: index
                  }),
                  this.name
                );
              }
              // Re-add the event listener
              OSLmap.on('baselayerchange', onBaseLayerChange);
            }
          });
        } else {
          let trackingarea = L.polygon([[100, 150]], [[500, 5000]]);
          let img = new Image();
          img.src = '';
          mainfloor.setUrl(img.src);
          mainfloor.setBounds(trackingarea.getBounds());
          OSLmap.setView(trackingarea.getBounds().getCenter(), 1.5);
        }
      },
      error: function(error) {
        console.info(error);
      },
      faulure: function(fail) {
        console.info(fail);
      },
      complete: async function(complete) {
        await addGroupToList('BackgroundImage');
      }
    });
  } catch (e) {
    console.info(e);
  }
}
