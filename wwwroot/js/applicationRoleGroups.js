let AppRoleTable = 'app_roleGroupsTable';

$('#AppSetting_value_Modal').on('hidden.bs.modal', function() {
  $(this).find('input[type=text],textarea,select').css({ 'border-color': '#D3D3D3' }).val('').end().find('span[class=text-info]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).change();

  if (!$('#AppSetting_Modal').hasClass('in')) {
    $('#AppSetting_Modal').addClass('modal-open');
  }
});
$('#AppSetting_value_Modal').on('shown.bs.modal', function() {
  $('span[id=error_appsettingvalue]').text('');

  $('button[id=appsettingvalue]').prop('disabled', false);
  //Connection name Validation
  if (!checkValue($('input[id=modalValueID]').val())) {
    $('input[id=modalValueID]').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_modalValueID]').text('Please Enter Value');
  } else {
    $('input[id=modalValueID]').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_modalValueID]').text('');
  }
  //IP Address Keyup
  $('input[id=modalValueID]').keyup(function() {
    if (!checkValue($('input[id=modalValueID]').val())) {
      $('input[id=modalValueID]').css('border-color', '#FF0000').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_modalValueID]').text('Please Enter Value');
    } else {
      $('input[id=modalValueID]').css({ 'border-color': '#2eb82e' }).removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_modalValueID]').text('');
    }
  });
});
connection.on('updateApplicationRoleGroups', async data => {
  try {
    return new Promise((resolve, reject) => {
      Promise.all([updateAppRoleGroupsDataTable(formatRoledata(data), AppRoleTable)]);
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});
async function init_applicationRoleGroups() {
  try {
    createAppRoleGroupsDataTable(AppRoleTable);
    fetch('../api/ApplicationConfiguration/UserRole')
      .then(response => response.json())
      .then(data => {
        loadAppRoleGroupsDatatable(formatRoledata(data), AppRoleTable);
      })
      .catch(error => {
        console.error('Error:', error);
      });

    await addGroupToList('ApplicationRoleGroups');
  } catch (e) {
    throw new Error(e.toString());
  }
}
//app RoleGroups
function Edit_AppRoleGroupsValue(roleData) {
  $('.valuediv').css('display', 'block');
  $('.timezonediv').css('display', 'none');
  $('.value_row_toggle').css('display', 'none');
  $('#approlegroupsmodalHeader').text('Edit ' + roleData.displayName + ' Role Group');
  $('input[id=roleNameId]').val(roleData.name);
  $('input[id=roleValue]').val(roleData.value);

  $('button[id=approlegroupsbtn]').off().on('click', function() {
    $('button[id=approlegroupsbtn]').prop('disabled', true);
    let jsonObject = {};

    jsonObject[roleData.name] = $('input[id=roleValue]').val();

    if (!$.isEmptyObject(jsonObject)) {
      // Make a fetch call to add the inventory
      fetch(`../api/ApplicationConfiguration/UpdateUserRole`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(jsonObject)
      })
        .then(response => {
          if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
          }
          return response.json();
        })
        .then(successdata => {
          $('span[id=error_approlegroupsbtn]').text('Data has been updated');
          setTimeout(function() {
            $('#AppRoleGroupsValueModal').modal('hide');
            $('button[id=approlegroupsbtn]').prop('disabled', false);
            updateAppRoleGroupsDataTable(formatRoledata(successdata), AppRoleTable);
          }, 500);
        })
        .catch(error => {
          $('span[id=error_approlegroupsbtn]').text(error);
          $('button[id=approlegroupsbtn]').prop('disabled', false);
          console.log(error);
        });
      //$.ajax({
      //    url: SiteURLconstructor(window.location) + '/api/ApplicationConfiguration/Update',
      //    data: JSON.stringify(jsonObject),
      //    contentType: 'application/json',
      //    type: 'POST',
      //    success: function (data) {
      //        $('span[id=error_approlegroupsbtn]').text("Data has been updated");
      //    },
      //    error: function (error) {
      //        $('span[id=error_approlegroupsbtn]').text(error);
      //        $('button[id=approlegroupsbtn]').prop('disabled', false);
      //        //console.log(error);
      //    },
      //    faulure: function (fail) {
      //        console.log(fail);
      //    },
      //    complete: function (complete) {

      //        setTimeout(function () {
      //            $("#AppSetting_value_Modal").modal('hide');

      //        }, 800);
      //    }
      //});
    }
  });
  $('#AppRoleGroupsValueModal').modal('show');
}
function createAppRoleGroupsDataTable(table) {
  let arrayColumns = {
    displayName: '',
    value: '',
    action: ''
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
    } else if (/Action/i.test(key)) {
      //else if (/Action/i.test(key)) {
      tempc = {
        title: 'Action',
        width: '10%',
        mDataProp: key,
        //"mRender": function (data, type, full) {
        mRender: function() {
          if (/^Admin/i.test(appData.Role)) {
            return '<button class="btn btn-light btn-sm mx-1 pi-iconEdit editappsetting" name="editappsetting"></button>';
          }
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
  // Edit/remove record
  $('#' + table + ' tbody').on('click', 'button', function() {
    let td = $(this);
    let table = $(td).closest('table');
    let row = $(table).DataTable().row(td.closest('tr'));
    if (/editappsetting/ig.test(this.name)) {
      Edit_AppRoleGroupsValue(row.data());
    }
  });
}
async function loadAppRoleGroupsDatatable(data, table) {
  try {
    return new Promise((resolve, reject) => {
      if ($.fn.dataTable.isDataTable('#' + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
      }
      resolve();
      return true;
    });
  } catch (e) {
    throw new Error(e.toString());
    reject();
    return false;
  }
}
function updateAppRoleGroupsDataTable(newdata, table) {
  try {
    return new Promise((resolve, reject) => {
      let loadnew = true;
      if ($.fn.dataTable.isDataTable('#' + table)) {
        $('#' + table).DataTable().rows(function(idx, data, node) {
          loadnew = false;
          for (const element of newdata) {
            if (data.name === element.name) {
              if (data.value !== element.value) {
                $('#' + table).DataTable().row(node).data(element).draw().invalidate();
              }
            }
          }
        });
        if (loadnew) {
          loadAppSettingsDataTable([newdata], table);
        }
      }
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}

function Get_value(Data) {
  try {
    if (/^(True)$|^(False)$/i.test(Data.VALUE)) {
      let return_val = false;
      $('input[type=checkbox][name="appsetting_value"]').each(function() {
        if (this.checked) {
          return_val = true;
        }
      });
      return return_val;
    } else {
      return $('input[id=modalValueID]').val();
    }
  } catch (e) {
    console.error(e.toString());
  }
}
function formatRoledata(data) {
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
function formatdata(result) {
  let reformatdata = [];
  try {
    for (let key in result) {
      if (result.hasOwnProperty(key)) {
        let temp = {
          KEY_NAME: '',
          VALUE: ''
        };
        temp['KEY_NAME'] = key;
        temp['VALUE'] = result[key];
        reformatdata.push(temp);
      }
    }
  } catch (e) {
    console.log(e);
  }

  return reformatdata;
}
