let TagSearchtable = 'tagresulttable';
let search_Table = $('table[id=tagresulttable]');

$(function() {
  // Search Tag Name
  $('input[id=inputsearchbtn')
    .on('keyup', function() {
      startSearch(this.value);
    })
    .on('keydown', function(event) {
      if (event.which === 13) {
        event.preventDefault();
      }
    });
});
async function init_tagsSearch() {
  Promise.all([createTagSearchDataTable(TagSearchtable)]);
}
async function startSearch(searchValue) {
  //clear layer tooltip class list for all tags

  if (checkValue(searchValue)) {
    if (!!searchValue) {
      //makea ajax call to get the employee details
      $.ajax({
        url: SiteURLconstructor(window.location) + '/api/Tag/Search?value=' + searchValue,
        type: 'GET',
        success: function(data) {
          Promise.all([clearLayerTooltip()]);

          if ($.fn.dataTable.isDataTable('#' + TagSearchtable)) {
            $('#' + TagSearchtable).DataTable().clear().draw();
          }
          // Empty the DOM element which contained DataTable

          loadTagSearchDataTable(data, TagSearchtable);
          //set layer tooltip class list for tags in properties list.
          Promise.all([setLayerTooltip(data)]);
        },
        error: function(error) {
          console.log(error);
        },
        faulure: function(fail) {
          console.log(fail);
        },
        complete: function(complete) {}
      });
    }
  } else {
    Promise.all([clearLayerTooltip()]);
    if ($.fn.dataTable.isDataTable('#' + TagSearchtable)) {
      $('#' + TagSearchtable).DataTable().clear().draw();
    }
  }
}
async function setLayerTooltip(tagid) {
  return new Promise((resolve, reject) => {
    if (OSLmap.hasOwnProperty('_layers')) {
      //loop trough tagid list
      tagid.forEach(function(item) {
        $.map(OSLmap._layers, function(layer, i) {
          if (layer.hasOwnProperty('feature') && layer.feature.properties.hasOwnProperty('type')) {
            if (/(badge)|(Vehicle$)/i.test(layer.feature.properties.type)) {
              if (item.tagid === layer.feature.properties.id) {
                if (layer.hasOwnProperty('_tooltip') && layer._tooltip.hasOwnProperty('_container')) {
                  if (!layer._tooltip._container.classList.contains('searchflash')) {
                    layer._tooltip._container.classList.add('searchflash');
                  }
                }
              }
            }
          }
        });
      });
    }
    resolve();
    return;
  });
}
async function clearLayerTooltip() {
  return new Promise((resolve, reject) => {
    if (OSLmap.hasOwnProperty('_layers')) {
      $.map(OSLmap._layers, function(layer, i) {
        if (layer.hasOwnProperty('feature') && layer.feature.properties.hasOwnProperty('Tag_Type')) {
          if (/(badge)|(Vehicle$)/i.test(layer.feature.properties.Tag_Type)) {
            if (layer.hasOwnProperty('_tooltip') && layer._tooltip.hasOwnProperty('_container')) {
              if (layer._tooltip._container.classList.contains('searchflash')) {
                layer._tooltip._container.classList.remove('searchflash');
              }
            }
          }
        }
      });
    }
    resolve();
    return;
  });
}
async function createTagSearchDataTable(table) {
  let arrayColums = [
    {
      tagid: '',
      ein: '',
      craftName: '',
      Action: ''
    }
  ];
  let columns = [];
  let tempc = {};

  $.each(arrayColums[0], function(key) {
    tempc = {};
    if (/tagid/i.test(key)) {
      tempc = { title: 'Id', width: '15%', mDataProp: key };
    }
    if (/ein/i.test(key)) {
      tempc = {
        title: 'EIN',
        width: '15%',
        mDataProp: key
      };
    }
    if (/craftName/i.test(key)) {
      tempc = {
        title: 'Type',
        width: '50%',
        mDataProp: key
      };
    }
    if (/Action/i.test(key)) {
      tempc = {
        title: 'Action',
        width: '10%',
        mRender: function(data, type, full) {
          return '<button type="button" class="btn btn-light btn-sm mx-1 bi bi-bullseye ' + (full.presence ? 'green-presence text-warning' : 'red-presence text-light') + '" data-toggle="modal" name="tagnav" "title="Edit Tag Info"></button>' + '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="tagedit" "title="Edit Tag Info"></button>';
        }
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
      emptyTable: 'Please Enter Tag / EIN / Name / EncodedID'
    },
    columns: columns,
    columnDefs: [
      {
        orderable: false, // Disable sorting on all columns
        targets: '_all'
      }
    ],
    sorting: false // Disable sorting
  });
  // Edit/remove record
  $('#' + table + ' tbody').on('click', 'button', function() {
    let td = $(this);
    let table = $(td).closest('table');
    let row = $(table).DataTable().row(td.closest('tr'));
    let rowData = row.data();
    if (/tagedit/ig.test(this.name)) {
      Promise.all([tagEditInfo(rowData.tagid)]);
    }
    if (/tagnav/ig.test(this.name)) {
      Promise.all([moveToTagLocation(rowData.tagid)]);
    }
  });
}
function loadTagSearchDataTable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateTagSearchDataTable(newdata, table) {
  let loadnew = true;
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows(function(idx, data, node) {
      if (data.Id === newdata.Id) {
        loadnew = false;
        $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
      }
    });
    if (loadnew) {
      loadTagSearchDataTable(newdata, table);
    }
  }
}
function removeTagSearchDataTable(ldata, table) {
  $('#' + table).DataTable().rows(function(idx, data, node) {
    if (data.Id === ldata.Id) {
      $('#' + table).DataTable().row(node).remove().draw();
    }
  });
}
async function moveToTagLocation(id) {
  try {
    $.map(OSLmap._layers, function(layer, i) {
      if (layer.hasOwnProperty('feature') && layer.feature.properties.hasOwnProperty('type')) {
        if (/(badge)|(Vehicle$)/i.test(layer.feature.properties.type)) {
          if (id === layer.feature.properties.id) {
            OSLmap.setView(layer._latlng, 5);
          }
        }
      }
    });
  } catch (e) {
    console.log(e);
  }
}
