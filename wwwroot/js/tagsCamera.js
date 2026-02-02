$('#Camera_Modal').on('hidden.bs.modal', async function () {
  $(this).find('input[type=text],textarea,select').css({ 'border-color': '#D3D3D3' }).val('').prop('disabled', false).end().find('input[type=radio]').prop('disabled', false).prop('checked', false).change().end().find('span[class=text-info]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).change().end();

  //stop the startTimer when closing the modal
  if (timer) {
    clearInterval(timer);
    await removeFromGroupList('CamerasStill');
  }
  // remove resize handler when modal closes
  $(window).off('resize.cameraModal');
});
$('#Camera_Modal').on('shown.bs.modal', async function () {
  await addGroupToList('CamerasStill');
  // ensure sizing is correct when the modal opens
  resizeCameraModal();
  // keep sizing in sync with window resize while modal is open
  $(window).off('resize.cameraModal').on('resize.cameraModal', function () {
    if ($('#Camera_Modal').hasClass('show')) {
      resizeCameraModal();
    }
  });
});
connection.on('addCameras', async data => {
  await addCameraFeature(data);
});
connection.on('updateCameras', async data => {
  await updateCameraFeature(data);
});
connection.on('deleteCameras', async data => {
  await deleteCameraFeature(data);
});
connection.on('updateCamerasStill', async data => {
  await updateCameraStillFeature(data);
});

let defaultTime = 90;
let count = 0;
let timer;
function startTimer() {
  // ensure we don't have multiple intervals running
  if (timer) {
    clearInterval(timer);
  }
  timer = setInterval(function () {
    try {
      count = count - 1;
      if (count <= 0) {
        count = 0;
        $('#counter').html('0');
        clearInterval(timer);
        $('#countDownView').hide();
        $('#divImageRefresh').addClass('d-none');
        $('#timeOutView').show();
        $('div[id=cameraImageBody]').removeClass('cameraImageBodyRefreshBorder').addClass('cameraImageBodyTimeOutBorder');
        handleGroupChange(false, 'CamerasStill').then(() => {
          $('#divImageRefresh').removeClass('d-none');
        }).catch(err => console.error(err));
      } else {
        $('#counter').html(count);
        if ($('div[id=cameraImageBody]').hasClass('cameraImageBodyTimeOutBorder')) {
          $('div[id=cameraImageBody]').removeClass('cameraImageBodyTimeOutBorder').addClass('cameraImageBodyRefreshBorder');
        }
      }
    } catch (err) {
      console.error(err);
    }
  }, 1000);
}
// Utility to create the same custom divIcon with cone & cameraDirection
function getCameraDivIcon(direction) {
  if (direction >= 359) {
    return L.divIcon({
      className: 'custom-div-icon',
      html: direction ? "<div class='marker-camera-cone' style='transform:rotate(0deg);'></div>" +
        "<div class='marker-camera-cone' style='transform:rotate(90deg);'></div>" +
        "<div class='marker-camera-cone' style='transform:rotate(180deg);'></div>" +
        "<div class='marker-camera-cone' style='transform:rotate(270deg);'></div>" +
        "<div class='marker-pin'></div><i class='bi-camera-fill'></i>" : "<div class='marker-pin'></div><i class='bi-camera-fill'></i>",
      iconSize: [42, 42],
      iconAnchor: [2, 62]
    });
  }
  else {
    return L.divIcon({
      className: 'custom-div-icon',
      html: direction ? "<div style='transform:rotate(" + direction + "deg)' class='marker-camera-cone'></div><div class='marker-pin'></div><i class='bi-camera-fill'></i>" : "<div class='marker-pin'></div><i class='bi-camera-fill'></i>",
      iconSize: [42, 42],
      iconAnchor: [2, 62]
    });
  }
}
function getCameraQuadDivIcon(direction) {

}
// GeoJSON layer for camera markers (ensure global availability for other scripts)
let markerCameras = new L.GeoJSON(null, {
  pointToLayer: function (feature, latlng) {
    return L.marker(latlng, {
      icon: getCameraDivIcon(feature.properties.cameraDirection),
      riseOnHover: true,
      bubblingMouseEvents: true,
      popupOpen: true
    });
  },
  onEachFeature: function (feature, layer) {
    try {
      layer.markerId = feature.properties.id;
      layer.on('click', function (e) {
        $('#timeOutView').hide();
        $('#countDownView').show();
        clearInterval(timer);
        count = defaultTime;
        $('#counter').html(count);
        setTimeout(startTimer, 300);

        LoadWeb_CameraImage(feature.properties, layer);
      });
      layer.bindTooltip('', {
        permanent: true,
        interactive: true,
        direction: 'center',
        opacity: 0
      }).openTooltip();
    } catch (error) {
      console.error('Error in onEachFeature for camera markers:', error);
    }
  },
  filter: function (feature, layer) {
    return feature.properties.visible;
  }
});
// add to the map and layers control
let overlayCameraLayer = L.layerGroup();
layersControl.addOverlay(overlayCameraLayer, 'Cameras');
markerCameras.addTo(overlayCameraLayer);


// add to the map and layers control
// Finds a camera layer by its markerId
// Returns a Promise that resolves with the leaflet_id if found, or rejects if not found
// Note: Rejection is used intentionally to indicate "not found" state
async function findCameraLeafletIds(markerId) {
  return new Promise((resolve, reject) => {
    markerCameras.eachLayer(function (layer) {
      if (layer.markerId === markerId) {
        resolve(layer._leaflet_id);
        return false;
      }
    });
    reject(new Error('No layer found with the given markerId'));
  });
}
async function init_tagsCamera(floorId) {
  try {
    if (!/PMCCUser$/ig.test(appData.User)) {
      //loading connections
      await fetchData(`${await SiteURLconstructor(globalThis.location)}/api/Camera/CameraByFloorId?floorId=${floorId}&type=Cameras`)
        .then(response => response.json())
        .then(data => {
          if (data.length > 0) {
            data.forEach(function (item) {
              if (item.properties.visible) {
                addCameraFeature(item);
              }
            });
          }
        })
        .catch(error => {
          console.error('Error:', error);
        });
      //load cameras
      $(document).on('change', '.leaflet-control-layers-selector', async function () {
        let sp = this.nextElementSibling.innerHTML.trim();
        if (/^Cameras$/ig.test(sp)) {
          await handleGroupChange(this.checked, sp);
        }
      });
      $('#startImageRefresh').off('click').on('click', function (e) {
        e.preventDefault();
        $('#timeOutView').hide();
        $('#countDownView').show();
        if (timer) {
          clearInterval(timer);
        }
        count = defaultTime;
        $('#counter').html(count);
        handleGroupChange(true, 'CamerasStill').then(() => {
          startTimer();
          $('#divImageRefresh').addClass('d-none');
          $('div[id=cameraImageBody]').addClass('cameraImageBodyRefreshBorder').removeClass('cameraImageBodyTimeOutBorder');
        }).catch(err => console.error(err));

      });
    } else {
      console.info('Camera feature is not available for this user.');
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function deleteCameraFeature(data) {
  try {
    await findCameraLeafletIds(data.properties.id)
      .then(leafletIds => {
        //remove from markerCameras
        markerCameras.removeLayer(leafletIds);
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function addCameraFeature(data) {
  try {
    await findCameraLeafletIds(data.properties.id).then(leafletIds => { }).catch(error => {
      markerCameras.addData(data);
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function updateCameraFeature(data) {
  try {
    await findCameraLeafletIds(data.properties.id)
      .then(leafletIds => {
        markerCameras._layers[leafletIds].feature.properties = data.properties;
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function updateCameraStillFeature(data) {
  try {
    await findCameraLeafletIds(data.properties.id)
      .then(leafletIds => {
        markerCameras._layers[leafletIds].feature.properties.base64Image = data.properties.base64Image;
        if ($('#Camera_Modal').hasClass('show') && $('div[id=Camera_Modal]').attr('data-id') === data.properties.id) {
          let camera_modal_body = $('div[id=cameraImageBody]');
          camera_modal_body.empty();
          if (data.properties.base64Image) {
            camera_modal_body.append(ImageLayout.supplant(formatImageLayout(data.properties.base64Image)));
          }
          LoadCameraInfo(data.properties, data.properties.id);
        }
      })
      .catch(error => {
        console.error('Error:', error);
      });
  } catch (e) {
    throw new Error(e.toString());
  }
}

let ImageLayout = '{image}';
function formatImageLayout(img) {
  let newimg = new Image();
  newimg.src = img;
  return $.extend(newimg, {
    image: newimg.outerHTML
  });
}

let webCameraViewData = null;
async function LoadWeb_CameraImage(Data, layer) {
  try {
    if (!Data) throw new Error('No camera data provided');

    webCameraViewData = Data;
    await LoadCameraInfo(Data, Data.id);
    $('#cameramodalHeader').text('View Web Camera');

    const $cameraModal = $('#Camera_Modal');
    const $cameraModalBody = $('#cameraImageBody');
    $cameraModalBody.empty();
    if (Data.base64Image) {
      $cameraModalBody.append(ImageLayout.supplant(formatImageLayout(Data.base64Image)));
    }
    $cameraModal.attr('data-id', Data.id).modal('show');
    $cameraModalBody.removeClass('cameraImageBodyTimeOutBorder').addClass('cameraImageBodyRefreshBorder');
    // adjust sizes after content is in place
    resizeCameraModal();

    const $arrowBtn = $('#cameraViewDirectionArrow');
    const $arrowNeedle = $arrowBtn.find('.camera-direction-component__needle');
    const $compass = $arrowBtn.find('.camera-direction-component__compass');


    // get/set configured north angle (degrees). default 0
    const configuredNorth = Data.northDirection !== undefined && Data.northDirection !== null
      ? Number(Data.northDirection)
      : 0;
    $arrowBtn.data('north', configuredNorth);



    // Treat 0 as a valid rotation value (was previously falsy)
    if (Data.cameraDirection !== undefined && Data.cameraDirection !== null) {
      const cameraDeg = isNaN(Number(Data.cameraDirection)) ? 0 : Number(Data.cameraDirection);


      // Rotate labels so the 'N' points to configuredNorth, and rotate needle relative to that north
      updateCompassLabels($compass, configuredNorth);

      const needleDeg = cameraDeg - configuredNorth;
      // Ensure needle transform preserves the translate that places its bottom at center
      $arrowNeedle.css({ transform: 'translate(-50%,-100%) rotate(' + needleDeg + 'deg)', transition: 'transform 200ms ease' });
      $arrowBtn.show();
      if (cameraDeg <= 359) {
        $('#cameraDirectionRow').removeClass('d-none');
      
      // initialize sliders if present
      if ($('#cameraDirectionSlider').length) {
        $('#cameraDirectionSlider').val(Math.round(cameraDeg));
        $('#cameraDirectionValue').text(Math.round(cameraDeg) + '°');
        // trigger camera input handler to set needle state
        $('#cameraDirectionSlider').trigger('input');
      }
      if ($('#northDirectionSlider').length) {
        $('#northDirectionSlider').val(Math.round(configuredNorth));
        $('#northDirectionValue').text(Math.round(configuredNorth) + '°');
        // trigger north input handler to position labels (does not move needle)
        $('#northDirectionSlider').trigger('input');
      }}
      else {
        $('#cameraDirectionRow').addClass('d-none');
      }
      // Live update icon on slider movement, but only POST on release (change/mouseup)
      $('#cameraDirectionSlider')
        .off('input change mouseup')
        .on('input', function () {
          let cameraDirection = Number($(this).val());
          $('#cameraDirectionValue').text(cameraDirection + "°");
          layer.setIcon(getCameraDivIcon(cameraDirection));
          onCameraInput();
        })
        .on('change mouseup', async function () {
          let cameraDirection = Number($(this).val());
          try {
            await postAddData(`${await SiteURLconstructor(globalThis.location)}/api/Camera/UpdateCameraDirection`, {
              id: Data.id,
              direction: cameraDirection
            });
          } catch (err) {
            console.error(err);
          }
        });
    } else {
      $('#cameraDirectionRow').addClass('d-none');
      $arrowBtn.hide();
    }

    sidebar.close('');
  } catch (e) {
    $('#error_camera').text(e && e.message ? e.message : e);
  }
}
// update label positions around compass
function updateCompassLabels($compassEl, northDeg) {
  const $labels = {
    n: $compassEl.find('.compass-label--n'),
    e: $compassEl.find('.compass-label--e'),
    s: $compassEl.find('.compass-label--s'),
    w: $compassEl.find('.compass-label--w')
  };
  const w = $compassEl.width();
  const radius = Math.max((w / 2) + 8, 10);
  // offsets: place labels around compass (0 = N, 90 = E, 180 = S, 270 = W)
  const offsets = { n: 0, e: 90, s: 180, w: 270 };
  Object.keys($labels).forEach(key => {
    const offset = offsets[key];
    const angle = (northDeg + offset) % 360;
    // move label out from center along rotated axis, keep label upright by counter-rotating
    const transform = 'translate(-50%,-50%) rotate(' + angle + 'deg) translateY(' + (-radius) + 'px) rotate(' + (-angle) + 'deg)';
    $labels[key].css('transform', transform);
  });
}
async function LoadCameraInfo(Data, id) {
  try {
    let cameraInfobody = $('div[id=cameraImageInfobody]');
    cameraInfobody.empty();
    let infoTable = '<table class="table table-bordered"><thead><tr><th>Property</th><th>Value</th></tr></thead><tbody>';
    infoTable += '<tr><td scope="row">Camera Name</td><td>' + Data.cameraName + '</td></tr>';
    infoTable += '<tr><td scope="row">Description</td><td>' + Data.description + '</td></tr>';
    infoTable += '<tr><td scope="row">IP Address</td><td>' + Data.ip + '</td></tr>';
    infoTable += '<tr><td scope="row">Type</td><td>' + Data.type + '</td></tr>';
    infoTable += '<tr><td scope="row">Resolution </td><td>' + Data.defaultResolution + '</td></tr>';
    infoTable += '<tr><td scope="row">CameraId </td><td>' + Data.defaultCameraId + '</td></tr>';
    infoTable += '<tr><td scope="row">Compression</td><td>' + Data.compression + '</td></tr>';
    infoTable += '</tbody></table>';
    cameraInfobody.append(infoTable);
  } catch (e) {
    $('#error_camera').text(e);
  }
}


const $btn = $('#cameraViewDirectionArrow');
const $compass = $btn.find('.camera-direction-component__compass');
const $needle = $btn.find('.camera-direction-component__needle');
const $cameraVal = $('#cameraDirectionValue');
const $northVal = $('#northDirectionValue');

function onNorthInput() {
  if (!$btn.length) return;
  const north = Number($('#northDirectionSlider').val() || 0);
  $northVal.text(Math.round(north) + '°');
  // rotate labels only; do NOT change the needle
  updateCompassLabels($compass, north);
  $btn.data('north', north);
}
let currentNeedleDeg = null;
function onCameraInput() {
  if (!$btn.length) return;
  const cam = Number($('#cameraDirectionSlider').val() || 0);
  const north = Number($('#northDirectionSlider').val() || $btn.data('north') || 0);
  $cameraVal.text(Math.round(cam) + '°');
  // update the needle when camera direction changes
  currentNeedleDeg = cam - north;
  $needle.css({ transform: 'translate(-50%,-100%) rotate(' + currentNeedleDeg + 'deg)', transition: 'transform 0ms' });
}

// Dynamically size the camera image area to the available modal space
function resizeCameraModal() {
  try {
    const $modal = $('#Camera_Modal');
    if (!$modal.length) return;
    const $content = $modal.find('.modal-content');
    const $header = $content.find('.modal-header');
    const $footer = $content.find('.modal-footer');
    const $body = $content.find('.modal-body');
    const $rightCol = $('#cameraImageBody');

    // Compute available height inside the modal content minus header/footer
    const contentHeight = $content.innerHeight();
    const headerH = $header.outerHeight(true) || 0;
    const footerH = $footer.outerHeight(true) || 0;
    const gap = 12; // small spacing buffer
    const available = Math.max(contentHeight - headerH - footerH - gap, 200);

    // Apply constraints to the container and media elements
    $rightCol.css({ 'max-height': available + 'px', 'overflow': 'auto' });
    $rightCol.find('img, iframe, video, canvas').css({
      'max-height': (available - 8) + 'px',
      'width': '100%',
      'height': 'auto',
      'object-fit': 'contain'
    });
  } catch (err) {
    console.error('resizeCameraModal error:', err);
  }
}