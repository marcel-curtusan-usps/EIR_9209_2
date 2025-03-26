//connection types
let siteInfotable = 'siteInfotable';
connection.on('updateSiteInformation', async data => {
  try {
    return new Promise((resolve, reject) => {
      loadSiteInfoDatatable(formatSiteInfodata(data), siteInfotable);
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});
async function init_SiteInformation() {
  try {
    createSiteInfoDataTable(siteInfotable);
    fetch('../api/SiteInformation/SiteInfo')
      .then(response => response.json())
      .then(data => {
        loadSiteInfoDatatable(formatSiteInfodata(data), siteInfotable);
      })
      .catch(error => {
        console.error('Error:', error);
      });
    await addGroupToList('GetSiteInfo');
    await addGroupToList('SiteInformation');
  } catch (e) {
    console.log(e);
  }
}
function createSiteInfoDataTable(table) {
  let arrayColumns = {
    displayName: '',
    value: ''
  };
  let columns = [];
  let tempc = {};
  $.each(arrayColumns, function(key) {
    tempc = {};
    if (/displayName/i.test(key)) {
      tempc = {
        title: 'Name',
        width: '30%',
        mDataProp: key
      };
    } else if (/value/i.test(key)) {
      //else if (/VALUE/i.test(key)) {
      tempc = {
        title: 'Value',
        width: '50%',
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
        orderable: false, // Disable sorting on all columns
        targets: '_all'
      }
    ],
    sorting: [[0, 'asc']]
  });
}
function loadSiteInfoDatatable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    // For new version use table.destroy();
    $('#' + table).DataTable().clear().draw(); // Empty the DOM element which contained DataTable
    // The line above is needed if number of columns change in the Data
  }
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
function updateSiteInfoDataTable(newdata, table) {
  let loadnew = true;
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows(function(idx, data, node) {
      loadnew = false;
      for (const element of newdata) {
        if (data.value === element.value) {
          $('#' + table).DataTable().row(node).data(element).draw().invalidate();
        }
      }
    });
    if (loadnew) {
      loadSiteInfoDatatable(newdata, table);
    }
  }
}
function formatSiteInfodata(data) {
  let reformattedData = [];
  for (let key in data) {
    if (data.hasOwnProperty(key)) {
      reformattedData.push({
        displayName: insertSpaceBeforeCapitalLetters(key),
        name: key,
        value: data[key]
      });
    }
  }
  return reformattedData;
}
