//email table name
let ConnectionListtable = 'connectiontable';
let connTypeRadio = null;
//on close clear all inputs

$('#API_Connection_Modal').on('hidden.bs.modal', function() {
  $(this).find('input[type=text],textarea,select').css({ 'border-color': '#D3D3D3' }).val('').prop('disabled', false).end().find('input[type=radio]').prop('disabled', false).prop('checked', false).trigger('change').end().find('span[class=text]').css('border-color', '#FF0000').val('').text('').end().find('input[type=checkbox]').prop('checked', false).trigger('change').end();
  offOAuthConnection();
  sidebar.open('connections');
});

//on open set rules
$('#API_Connection_Modal').on('shown.bs.modal', function() {
  sidebar.close('connections');
  $('span[id=error_apisubmitBtn]').text('');
  $('button[id=apisubmitBtn]').prop('disabled', true);
  $('select[name=message_type]').prop('disabled', true);
  // Set the radio button with the value 'api' to checked
  $('input[type=radio][name=connectionType][id=api_connection]').prop('checked', true).trigger('change');
  connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
  $('.hoursforwardvalue').html($('input[id=hoursforward_range]').val());
  $('input[id=hoursforward_range]').on('input change', () => {
    $('.hoursforwardvalue').html($('input[id=hoursforward_range]').val());
  });
  $('.hoursbackvalue').html($('input[id=hoursback_range]').val());
  $('input[id=hoursback_range]').on('input change', () => {
    $('.hoursbackvalue').html($('input[id=hoursback_range]').val());
  });

  //Connection name Keyup
  if (!checkValue($('select[name=connection_name] option:selected').html())) {
    $('select[name=connection_name]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_connection_name]').text('Please Select Connection Name');
  } else {
    $('select[name=connection_name]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_connection_name]').text('');
  }

  $('select[name=connection_name]').on('change', function() {
    filtermessage_type('', '');
    $('input[type=text][name=url]').val('');
    // Set the baseUrl to the URL input text box and trigger keyup
    $('input[type=text][name=url]').val('').trigger('keyup');

    if (!checkValue($('select[name=connection_name] option:selected').html())) {
      $('select[name=connection_name]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_connection_name]').text('Please Select Connection Name');
      $('select[name=message_type]').prop('disabled', true);
      enableMessagetype();
    } else {
      $('select[name=connection_name]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_connection_name]').text('');
      enableMessagetype();
    }
    let connName = $('select[name=connection_name] option:selected').val();
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }

    if (/^(CiscoSpaces)/i.test(connName)) {
      $('div[id="CiscoSpacesmenu"]').css('display', '');
    } else {
      $('div[id="CiscoSpacesmenu"]').css('display', 'none');
      $('input[type=text][name=bearerToken]').val('');
      $('input[type=text][name=ciscoSpaceMapId]').val('');
      $('input[type=text][name=ciscoSpacetenantId]').val('');
    }
  });
  $('select[name=message_type]').on('change', function() {
    if (!checkValue($('select[name=message_type] option:selected').val())) {
      $('select[name=message_type]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_message_type]').text('Please Enter Message Type');
      // Set the baseUrl to the URL input text box
      $('input[type=text][name=url]').val('');
      // Set the baseUrl to the URL input text box and trigger keyup
      $('input[type=text][name=url]').val('').trigger('keyup');
    } else {
      let selectedOption = $('select[name=message_type] option:selected');
      let baseUrl = selectedOption.attr('data-baseUrl'); // Get the data-baseUrl attribute value using .attr()
      $('select[name=message_type]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_message_type]').text('');
      // Set the baseUrl to the URL input text box
      $('input[type=text][name=url]').val(baseUrl);
      // Set the baseUrl to the URL input text box and trigger keyup
      $('input[type=text][name=url]').val(baseUrl).trigger('keyup');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });
  //Data Retrieve Occurrences Validation
  if (!checkValue($('select[name=data_retrieve] option:selected').val())) {
    $('select[name=data_retrieve]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_data_retrieve]').text('Select Data Retrieve Occurrences');
  } else {
    $('select[name=data_retrieve]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_data_retrieve]').text('');
  }
  //Data Retrieve Occurrences Keyup
  $('select[name=data_retrieve]').on('change', function() {
    if (!checkValue($('select[name=data_retrieve] option:selected').val())) {
      $('select[name=data_retrieve]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_data_retrieve]').text('Select Data Retrieve Occurrences');
    } else {
      $('select[name=data_retrieve]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_data_retrieve]').text('');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });
  //connection Timeout Validation
  if (!checkValue($('select[name=connectionTimeout] option:selected').val())) {
    $('select[name=connectionTimeout]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_connectionTimeout]').text('Select Timeout Duration');
  } else {
    $('select[name=connectionTimeout]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_connectionTimeout]').text('');
  }
  //Data Retrieve Occurrences Keyup
  $('select[name=connectionTimeout]').on('change', function() {
    if (!checkValue($('select[name=connectionTimeout] option:selected').val())) {
      $('select[name=connectionTimeout]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_connectionTimeout]').text('Select Connection');
    } else {
      $('select[name=connectionTimeout]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_connectionTimeout]').text('');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });
  $('input[type=text][name=hostname], input[type=text][name=ip_address]').on('keyup change', checkAndDisableInputs);

  //Hostname Keyup
  $('input[type=text][name=hostname]').on('keyup', () => {
    checkAndDisableInputs();
    if (!checkValue($('input[type=text][name=hostname]').val())) {
      $('input[type=text][name=hostname]').css('border-color', '#D3D3D3').removeClass('is-valid').removeClass('is-valid');
      $('span[id=error_hostname]').text('');
    } else {
      $('input[type=text][name=hostname]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_hostname]').text('');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });

  //IP Address Keyup
  $('input[type=text][name=ip_address]').on('keyup', function() {
    checkAndDisableInputs();
    if (IPAddress_validator($('input[type=text][name=ip_address]').val()) === 'Invalid IP Address') {
      $('input[type=text][name=ip_address]').css('border-color', '#FF0000');
      $('span[id=error_ip_address]').text('Please Enter Valid IP Address!');
    } else {
      $('input[type=text][name=ip_address]').css({ 'border-color': '#2eb82e' }).removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_ip_address]').text('');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });

  //Port Keyup
  $('input[type=text][name=port_number]').on('keyup', () => {
    if (!isNaN(parseFloat($('input[type=text][name=port_number]').val()))) {
      if ($('input[type=text][name=port_number]').val().length > 65535) {
        $('input[type=text][name=port_number]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
        $('span[id=error_port_number]').text('Enter Port Number!');
      } else if ($('input[type=text][name=port_number]').val().length === 0) {
        $('input[type=text][name=port_number]').css({ 'border-color': '#FF0000' }).addClass('is-valid').removeClass('is-invalid');
        $('span[id=error_port_number]').text('Enter Port Number!');
      } else {
        $('input[type=text][name=port_number]').css({ 'border-color': '#2eb82e' }).removeClass('is-invalid').addClass('is-valid');
        $('span[id=error_port_number]').text('');
      }
    } else {
      $('input[type=text][name=port_number]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_port_number]').text('Enter Port Number!');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });
  //Vendor URL
  if (!checkValue($('input[type=text][name=url]').val())) {
    $('input[type=text][name=url]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_url]').text('Please Enter API URL');
  } else {
    $('input[type=text][name=url]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_url]').text('');
  }
  //URL Keyup
  $('input[type=text][name=url]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=url]').val())) {
      $('input[type=text][name=url]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_url]').text('Please Enter API URL');
    } else {
      $('input[type=text][name=url]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_url]').text('');
    }
    if (/^(udp|tcp)/i.test(connTypeRadio)) {
      enabletcpipudpSubmit();
    } else if (/^(api)/i.test(connTypeRadio)) {
      enableaipSubmit();
    }
  });

  //oAuth
  $('input[type=text][name=idrequesturl]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=idrequesturl]').val())) {
      $('input[type=text][name=idrequesturl]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_idrequesturl]').text('Please Enter Id Request URL');
    } else {
      $('input[type=text][name=idrequesturl]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_idrequesturl]').text('');
    }
  });
  $('input[type=text][name=tokenurl]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=tokenurl]').val())) {
      $('input[type=text][name=tokenurl]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_tokenurl]').text('Please Enter Token URL');
    } else {
      $('input[type=text][name=tokenurl]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_tokenurl]').text('');
    }
  });
  $('input[type=text][name=tokenusername]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=tokenusername]').val())) {
      $('input[type=text][name=tokenusername]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_tokenusername]').text('Please Enter UserName');
    } else {
      $('input[type=text][name=tokenusername]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_tokenusername]').text('');
    }
  });
  $('input[type=text][name=tokenpassword]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=tokenpassword]').val())) {
      $('input[type=text][name=tokenpassword]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_tokenpassword]').text('Please Enter Password');
    } else {
      $('input[type=text][name=tokenpassword]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_tokenpassword]').text('');
    }
  });
  $('input[type=text][name=tokenclientId]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=tokenclientId]').val())) {
      $('input[type=text][name=tokenclientId]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_tokenclientId]').text('Please Enter Client Id');
    } else {
      $('input[type=text][name=tokenclientId]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_tokenclientId]').text('');
    }
  });

  $('input[type=text][name=bearerToken]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=bearerToken]').val())) {
      $('input[type=text][name=bearerToken]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_bearerToken]').text('Please Enter Token');
    } else {
      $('input[type=text][name=bearerToken]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_bearerToken]').text('');
    }
  });
  $('input[type=text][name=ciscoSpaceMapId]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=ciscoSpaceMapId]').val())) {
      $('input[type=text][name=ciscoSpaceMapId]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_ciscoSpaceMapId]').text('Please Enter Map Id');
    } else {
      $('input[type=text][name=ciscoSpaceMapId]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_ciscoSpaceMapId]').text('');
    }
  });
  $('input[type=text][name=ciscoSpacetenantId]').on('keyup', () => {
    if (!checkValue($('input[type=text][name=ciscoSpacetenantId]').val())) {
      $('input[type=text][name=ciscoSpacetenantId]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_ciscoSpacetenantId]').text('Please Enter Tenant Id');
    } else {
      $('input[type=text][name=ciscoSpacetenantId]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_ciscoSpacetenantId]').text('');
    }
  });
  //Hour
  $('input[type=checkbox][name=hour_range]').on('change', () => {
    if (!$('input[type=checkbox][id=hour_range]').is(':checked')) {
      $('.hours_range_row').css('display', 'none');
      $('input[id=hoursback_range]').val(0);
      $('.hoursbackvalue').html(0);
      $('input[id=hoursforward_range]').val(0);
      $('.hoursforwardvalue').html(0);
    } else {
      $('.hours_range_row').css('display', '');
    }
  });
  //logdata
  $('input[type=checkbox][name=logdata]').on('change', () => {
    connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
    if (/^(api)/i.test(connTypeRadio)) {
      onAPIConnection();
    }
  });
  //radio check
  if (
    $("input[type=radio][name='connectionType']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');

      if (/^(udp|tcp)/i.test(connTypeRadio)) {
        onudptcpipConnection();
      } else if (/^(api)/i.test(connTypeRadio)) {
        onAPIConnection();
      }
    })
  );
  if (
    $("input[type=checkbox][name='active_connection']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');

      if (/^(udp|tcp)/i.test(connTypeRadio)) {
        onudptcpipConnection();
      } else if (/^(api)/i.test(connTypeRadio)) {
        onAPIConnection();
      }
    })
  );
  if (
    $("input[type=checkbox][id='oAuth2']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
      if (/^(api)/i.test(connTypeRadio)) {
        if ($('input[type=checkbox][id=oAuth2]').is(':checked')) {
          onOAuthConnection();
        } else {
          offOAuthConnection();
        }
      }
    })
  );
  if (
    $("input[type=checkbox][id='basicAuth']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
      if (/^(api)/i.test(connTypeRadio)) {
        if ($('input[type=checkbox][id=basicAuth]').is(':checked')) {
          onBasicAuthConnection();
        } else {
          offBasicAuthConnection();
        }
      }
    })
  );
  if (
    $("input[type=checkbox][id='bearerToken']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
      if (/^(api)/i.test(connTypeRadio)) {
        if ($('input[type=checkbox][id=bearerToken]').is(':checked')) {
          onBearerConnection();
        } else {
          offBearerConnection();
        }
      }
    })
  );
  if (
    $("input[type=checkbox][id='idRequest']").on('change', () => {
      connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
      if (/^(api)/i.test(connTypeRadio)) {
        if ($('input[type=checkbox][id=idRequest]').is(':checked')) {
          onidRequestConnection();
        } else {
          offidRequestConnection();
        }
      }
    })
  );

  checkAndDisableInputs();
  $("input[type='checkbox'][name='authType']").on('change', function() {
    const checkboxes = document.querySelectorAll('input[type="checkbox"][name="authType"]');
    checkboxes.forEach(cb => {
      if (cb !== this) {
        cb.checked = false;
        if (cb.id === 'bearerToken') {
          offBearerConnection();
        }
        if (cb.id === 'basicAuth') {
          offBasicAuthConnection();
        }
        if (cb.id === 'oAuth2') {
          offOAuthConnection();
        }
        if (cb.id === 'idRequest') {
          offidRequestConnection();
        }
      }
    });
    if (this.checked) {
      if (this.id === 'bearerToken') {
        onBearerConnection();
      }
      if (this.id === 'basicAuth') {
        onBasicAuthConnection();
      }
      if (this.id === 'oAuth2') {
        onOAuthConnection();
      }
      if (this.id === 'idRequest') {
        onidRequestConnection();
      }
    } else {
      if (this.id === 'idRequest') {
        offidRequestConnection();
      }
      if (this.id === 'bearerToken') {
        offBearerConnection();
      }
      if (this.id === 'basicAuth') {
        offBasicAuthConnection();
      }
      if (this.id === 'oAuth2') {
        offOAuthConnection();
      }
    }
  });
  //$('input[type=checkbox][name=ws_connection]').change(() => {
  //    onUpdateWS();
  //});
});
function checkAndDisableInputs() {
  const hostnameInput = $('input[type=text][name=hostname]');
  const ipAddressInput = $('input[type=text][name=ip_address]');

  if (hostnameInput.val().trim()) {
    ipAddressInput.prop('disabled', true);
  } else if (ipAddressInput.val().trim()) {
    hostnameInput.prop('disabled', true);
  } else {
    hostnameInput.prop('disabled', false);
    ipAddressInput.prop('disabled', false);
  }
}
/// receive messages from server
connection.on('addConnection', async data => {
  try {
    return new Promise((resolve, reject) => {
      Promise.all([updateConnectionDataTable(data, ConnectionListtable)]);
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});
connection.on('deleteConnection', async data => {
  try {
    return new Promise((resolve, reject) => {
      Promise.all([removeConnectionDataTable(data, ConnectionListtable)]);
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});
connection.on('updateConnection', async data => {
  try {
    return new Promise((resolve, reject) => {
      Promise.all([updateConnectionDataTable(data, ConnectionListtable)]);
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
});

async function Add_Connection() {
  try {
    if (appData.SiteName === 'No Site Configured') {
      $('#SiteNotConfiguredModal').modal('show');
    } else {
      $('div[id="serveripmenu"]').css('display', '');
      $('div[id="endpointurl"]').css('display', '');
      $('#modalHeader_ID').text('Add Connection');
      $('select[name=connectionTimeout]').val(60000).trigger('change');
      $('button[id=apisubmitBtn]').off().on('click', function() {
        $('button[id=apisubmitBtn]').prop('disabled', true);
        $('input[type=checkbox][name=ws_connection]').prop('disabled', false);
        $('input[type=checkbox][name=udp_connection]').prop('disabled', false);
        $('input[type=checkbox][name=tcpip_connection]').prop('disabled', false);
        let jsonObject = {
          ActiveConnection: $('input[type=checkbox][name=active_connection]').is(':checked'),
          ApiConnection: $('input[type=radio][id=api_connection]').is(':checked'),
          UdpConnection: $('input[type=radio][id=udp_connection]').is(':checked'),
          TcpIpConnection: $('input[type=radio][id=tcpip_connection]').is(':checked'),
          WsConnection: $('input[type=radio][id=ws_connection]').is(':checked'),
          HoursBack: parseInt($('input[id=hoursback_range]').val(), 10),
          HoursForward: parseInt($('input[id=hoursforward_range]').val(), 10),
          MillisecondsInterval: $('select[name=data_retrieve] option:selected').val(),
          MillisecondsTimeout: $('select[name=connectionTimeout] option:selected').val(),
          Name: $('select[name=connection_name] option:selected').val(),
          Hostname: $('input[type=text][name=hostname]').val(),
          IpAddress: $('input[type=text][name=ip_address]').val(),
          Port: Number.isNaN(Number($('input[type=text][name=port_number]').val())) ? parseInt($('input[id=hoursback_range]').val(), 10) : 0,
          Url: $('input[type=text][name=url]').val(),
          MessageType: $('select[name=message_type] option:selected').val(),
          OAuthUrl: $('input[type=text][name=tokenurl]').val(),
          LogData: $('input[type=checkbox][name=logdata]').is(':checked')
          //CreatedByUsername: User.UserId,
          // NassCode: User.Facility_NASS_Code
        };
        //check if the Basic Auth is checked
        if ($('input[type=checkbox][id=idRequest]').is(':checked')) {
          jsonObject.AuthType = 'idRequest';
          jsonObject.OAuthUrl = $('input[type=text][name=tokenurl]').val();
        }
        //check if the Basic Auth is checked
        if ($('input[type=checkbox][id=basicAuth]').is(':checked')) {
          jsonObject.AuthType = 'basicAuth';
          jsonObject.OAuthUserName = $('input[type=text][name=tokenusername]').val();
          jsonObject.OAuthPassword = $('input[type=text][name=tokenpassword]').val();
          jsonObject.OAuthClientId = $('input[type=text][name=tokenclientId]').val();
        }
        //check if the Bearer Token is checked
        if ($('input[type=checkbox][id=bearerToken]').is(':checked')) {
          jsonObject.AuthType = 'bearerToken';
          jsonObject.OutgoingApikey = $('input[type=text][name=bearerToken]').val();
          jsonObject.MapId = $('input[type=text][name=ciscoSpaceMapId]').val();
          jsonObject.TenantId = $('input[type=text][name=ciscoSpacetenantId]').val();
          jsonObject.OAuthUrl = $('input[type=text][name=tokenurl]').val();
          jsonObject.OAuthUserName = $('input[type=text][name=tokenusername]').val();
          jsonObject.OAuthPassword = $('input[type=text][name=tokenpassword]').val();
          jsonObject.OAuthClientId = $('input[type=text][name=tokenclientId]').val();
        }
        //check if the OAuth is checked
        if ($('input[type=checkbox][id=oAuth2]').is(':checked')) {
          jsonObject.AuthType = 'oAuth2';
          jsonObject.OAuthUrl = $('input[type=text][name=tokenurl]').val();
          jsonObject.OAuthUserName = $('input[type=text][name=tokenusername]').val();
          jsonObject.OAuthPassword = $('input[type=text][name=tokenpassword]').val();
          jsonObject.OAuthClientId = $('input[type=text][name=tokenclientId]').val();
        }
        if (!$.isEmptyObject(jsonObject)) {
          //make a ajax call to get the employee details
          $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Connections/Add',
            data: JSON.stringify(jsonObject),
            contentType: 'application/json',
            type: 'POST',
            success: function(data) {
              $('#content').html(data);
              sidebar.open('connections');
              setTimeout(function() {
                $('#API_Connection_Modal').modal('hide');
                sidebar.open('connections');
              }, 500);
            },
            error: function(error) {
              $('span[id=error_apisubmitBtn]').text(error);
              $('button[id=apisubmitBtn]').prop('disabled', false);
              //console.log(error);
            },
            faulure: function(fail) {
              console.log(fail);
            },
            complete: function(complete) {
              console.log(complete);
            }
          });
        }
      });
      $('#API_Connection_Modal').modal('show');
    }
  } catch (e) {
    throw new Error(e.toString());
  }
}
async function Edit_Connection(data) {
  if (appData.SiteName === 'No Site Configured') {
    $('#SiteNotConfiguredModal').modal('show');
  } else {
    $('#modalHeader_ID').text('Edit Connection');
    $('input[type=checkbox][id=active_connection]').prop('checked', data.activeConnection);
    $('input[type=checkbox][name=logdata]').prop('checked', data.logData);
    if (checkValue(data.ipAddress)) {
      $('input[type=text][id=ip_address]').val(data.ipAddress).trigger('keyup');
    }
    if (checkValue(data.hostname)) {
      $('input[type=text][id=hostname]').val(data.hostname).trigger('keyup');
    }
    $('input[type=text][id=port_number]').val(data.port);
    $('input[type=text][id=url]').val(data.url);
    filtermessage_type(data.name, data.messageType);
    $('select[name=connectionTimeout]').val(data.millisecondsTimeout);
    $('select[name=data_retrieve]').val(data.millisecondsInterval);
    $('input[type=radio]').prop('disabled', true);

    if (/^(CiscoSpaces)/i.test(data.name)) {
      $('input[type=text][name=ciscoSpaceMapId]').val(data.mapId);
      $('input[type=text][name=ciscoSpacetenantId]').val(data.tenantId);
      $('div[id="CiscoSpacesmenu"]').css('display', '');
    }

    if (/^(idRequest)/i.test(data.authType)) {
      $('input[type=checkbox][id=idRequest]').prop('checked', true);
      $('input[type=text][name=idrequesturl]').prop('disabled', false).val(data.oAuthUrl);
      onidRequestConnection();
    }
    if (/^(oAuth2)/i.test(data.authType)) {
      $('input[type=checkbox][id=oAuth2]').prop('checked', true);
      $('input[type=text][name=tokenurl]').prop('disabled', false).val(data.oAuthUrl);
      $('input[type=text][name=tokenusername]').prop('disabled', false).val(data.oAuthUserName);
      $('input[type=text][name=tokenpassword]').prop('disabled', false).val(data.oAuthPassword);
      $('input[type=text][name=tokenclientId]').prop('disabled', false).val(data.oAuthClientId);
      onOAuthConnection();
    }
    if (/^(bearerToken)/i.test(data.authType)) {
      $('input[type=checkbox][id=bearerToken]').prop('checked', true);
      $('input[type=text][name=bearerToken]').prop('disabled', false).val(data.outgoingApikey);
      onBearerConnection();
    }
    if (/^(basicAuth)/i.test(data.authType)) {
      $('input[type=checkbox][id=basicAuth]').prop('checked', true);
      $('input[type=text][name=tokenusername]').prop('disabled', false).val(data.oAuthUserName);
      $('input[type=text][name=tokenpassword]').prop('disabled', false).val(data.oAuthPassword);
      $('input[type=text][name=tokenclientId]').prop('disabled', false).val(data.oAuthClientId);
      onBasicAuthConnection();
    }
    if (data.apiConnection) {
      $('input[type=radio][id=api_connection]').prop('checked', data.apiConnection);
      onAPIConnection();
    }
    if (data.udpConnection) {
      $('input[type=radio][id=udp_connection]').prop('checked', data.udpConnection);
      onudptcpipConnection();
    }
    if (data.tcpIpConnection) {
      $('input[type=radio][id=tcpip_connection]').prop('checked', data.tcpIpConnection);
      onudptcpipConnection();
    }
    if (data.wsConnection) {
      $('input[type=radio][id=ws_connection]').prop('checked', data.wsConnection);
    }
    connTypeRadio = $('input[type=radio][name=connectionType]:checked').attr('id');
    if (data.hoursBack > 0 || data.hoursForward > 0) {
      $('.hoursbackvalue').html($.isNumeric(data.hoursBack) ? parseInt(data.hoursBack, 10) : 0);
      $('input[id=hoursback_range]').val($.isNumeric(data.hoursBack) ? parseInt(data.hoursBack, 10) : 0);
      $('.hours_range_row').css('display', '');
      $('.hoursforwardvalue').html($.isNumeric(data.hoursForward) ? parseInt(data.hoursForward, 10) : 0);
      $('input[id=hoursforward_range]').val($.isNumeric(data.hoursForward) ? parseInt(data.hoursForward, 10) : 0);
      $('.hours_range_row').css('display', '');
      $('input[type=checkbox][id=hour_range]').prop('checked', true);
    } else {
      $('.hoursbackvalue').html($.isNumeric(data.hoursBack) ? parseInt(data.hoursBack, 10) : 0);
      $('input[id=hoursback_range]').val($.isNumeric(data.hoursBack) ? parseInt(data.hoursBack, 10) : 0);
      $('.hours_range_row').css('display', 'none');
      $('.hoursforwardvalue').html($.isNumeric(data.hoursForward) ? parseInt(data.hoursForward, 10) : 0);
      $('input[id=hoursforward_range]').val($.isNumeric(data.hoursForward) ? parseInt(data.hoursForward, 10) : 0);
      $('.hours_range_row').css('display', 'none');
      $('input[type=checkbox][id=hour_range]').prop('checked', false);
    }
    $('button[id=apisubmitBtn]').prop('disabled', true);
    $('button[id=apisubmitBtn]').off().on('click', function() {
      try {
        $('button[id=apisubmitBtn]').prop('disabled', true);
        let jsonObject = {
          activeConnection: $('input[type=checkbox][name=active_connection]').is(':checked'),
          apiConnection: $('input[type=radio][id=api_connection]').is(':checked'),
          udpConnection: $('input[type=radio][id=udp_connection]').is(':checked'),
          tcpIpConnection: $('input[type=radio][id=tcpip_connection]').is(':checked'),
          wsConnection: $('input[type=radio][name=ws_connection]').is(':checked'),
          hoursBack: $('input[type=checkbox][id=hour_range]').is(':checked') ? parseInt($('input[id=hoursback_range]').val(), 10) : 0,
          hoursForward: $('input[type=checkbox][id=hour_range]').is(':checked') ? parseInt($('input[id=hoursforward_range]').val(), 10) : 0,
          millisecondsInterval: $('select[name=data_retrieve] option:selected').val(),
          millisecondsTimeout: $('select[name=connectionTimeout] option:selected').val(),
          name: $('select[name=connection_name] option:selected').val(),
          hostname: $('input[type=text][id=hostname]').val(),
          ipAddress: $('input[type=text][id=ip_address]').val(),
          port: $('input[type=text][name=port_number]').val(),
          url: $('input[type=text][name=url]').val(),
          messageType: $('select[name=message_type] option:selected').val(),
          logData: $('input[type=checkbox][name=logdata]').is(':checked'),
          //CreatedByUsername: User.UserId,
          //lastupdateByUsername: User.UserId,
          id: data.id
        };
        //check if the Id Request is checked
        if ($('input[type=checkbox][id=idRequest]').is(':checked')) {
          jsonObject.authType = 'idRequest';
          jsonObject.oAuthUrl = $('input[type=text][name=idrequesturl]').val();
        }
        //check if the Basic Auth is checked
        if ($('input[type=checkbox][id=basicAuth]').is(':checked')) {
          jsonObject.authType = 'basicAuth';
          jsonObject.oAuthUserName = $('input[type=text][name=tokenusername]').val();
          jsonObject.oAuthPassword = $('input[type=text][name=tokenpassword]').val();
          jsonObject.oAuthClientId = $('input[type=text][name=tokenclientId]').val();
        }
        //check if the Bearer Token is checked
        let ty = $('input[type=checkbox][id=bearerToken]').prop('checked');
        if ($('input[type=checkbox][id=bearerToken]').prop('checked')) {
          jsonObject.authType = 'bearerToken';
          jsonObject.outgoingApikey = $('input[type=text][name=bearerToken]').val();

          if (/^(CiscoSpaces)/i.test(data.name)) {
            jsonObject.mapId = $('input[type=text][name=ciscoSpaceMapId]').val();
            jsonObject.tenantId = $('input[type=text][name=ciscoSpacetenantId]').val();
          }
        }
        //check if the OAuth is checked
        if ($('input[type=checkbox][id=oAuth2]').is(':checked')) {
          jsonObject.authType = 'oAuth2';
          jsonObject.oAuthUrl = $('input[type=text][name=tokenurl]').val();
          jsonObject.oAuthUserName = $('input[type=text][name=tokenusername]').val();
          jsonObject.oAuthPassword = $('input[type=text][name=tokenpassword]').val();
          jsonObject.oAuthClientId = $('input[type=text][name=tokenclientId]').val();
        }

        if (!$.isEmptyObject(jsonObject)) {
          //make a ajax call to get the Connection details
          $.ajax({
            url: SiteURLconstructor(window.location) + '/api/Connections/Update',
            contentType: 'application/json-patch+json',
            type: 'POST',
            data: JSON.stringify(jsonObject),
            success: function(data) {
              sidebar.open('connections');
              setTimeout(function() {
                $('#API_Connection_Modal').modal('hide');
                sidebar.open('connections');
              }, 500);
            },
            error: function(error) {
              $('span[id=error_apisubmitBtn]').text(data.name + ' ' + data.messageType + ' Connection was not Updated');
              //console.log(error);
            },
            faulure: function(fail) {
              console.log(fail);
            },
            complete: function(complete) {
              // console.log(complete);
            }
          });
        }
      } catch (e) {
        $('span[id=error_apisubmitBtn]').text(e);
      }
    });
    if (checkValue(data.ipAddress)) {
      $('input[id=ip_address]').trigger('keyup');
    }
    if (checkValue(data.hostname)) {
      $('input[id=hostname]').trigger('keyup');
    }
    $('#API_Connection_Modal').modal('show');
  }
}
async function Remove_Connection(data) {
  try {
    $('#removeAPImodalHeader_ID').text('Removing Connection: ' + data.name + ' With Message Type : ' + data.messageType);
    $('button[id=remove_server_connection]').off().on('click', function() {
      //make a ajax call to get the Connection details
      $.ajax({
        url: SiteURLconstructor(window.location) + '/api/Connections/Delete/?id=' + data.id,
        type: 'DELETE',
        success: function(data) {
          $('#content').html(data);
          sidebar.open('connections');
          setTimeout(function() {
            $('#RemoveConfirmationModal').modal('hide');
            sidebar.open('connections');
          }, 500);
        },
        error: function(error) {
          $('span[id=error_apisubmitBtn]').text(data.name + ' ' + data.messageType + ' Connection has been Removed');
          //console.log(error);
        },
        faulure: function(fail) {
          console.log(fail);
        },
        complete: function(complete) {
          console.log(complete);
        }
      });
      //fotfmanager.server.removeAPI(JSON.stringify(ConnectionRemove)).done(function (Data) {
      //    setTimeout(function () {
      //        $("#RemoveConfirmationModal").modal('hide');
      //        sidebar.open('connections');
      //    }, 800);
      //})
    });
    $('#RemoveConfirmationModal').modal('show');
  } catch (e) {}
}
async function init_connection() {
  try {
    createConnectionDataTable(ConnectionListtable);
    if (/^(Admin|OIE)/i.test(appData.Role)) {
      $('button[name=connectionbtn]').css('display', 'block');
      $('button[name=addconnection]').off().on('click', function() {
        /* close the sidebar */
        sidebar.close();
        Promise.all([Add_Connection()]);
      });
    }
    //loading connections
    fetch('../api/Connections/AllConnection')
      .then(response => response.json())
      .then(data => {
        if (data.length > 0) {
          Promise.all([loadConnectionDatatable(data.sort(SortByConnectionName), ConnectionListtable)]);
        }
      })
      .catch(error => {
        console.error('Error:', error);
      });

    await addGroupToList('Connections');
  } catch (e) {
    throw new Error(e.toString());
  }
}
function createConnectionDataTable(table) {
  let Actioncolumn = false;
  if (/^(Admin|OIE)/i.test(appData.Role)) {
    Actioncolumn = true;
  }
  let arrayColums = [
    {
      name: '',
      messageType: '',
      port: '',
      status: '',
      action: ''
    }
  ];
  let columns = [];
  let tempc = {};
  $.each(arrayColums[0], function(key, value) {
    tempc = {};
    if (/Name/i.test(key)) {
      tempc = {
        title: 'Name',
        width: '20%',
        mDataProp: key,
        mRender: function(data, type, full) {
          if (full.apiConnection) {
            return full.name + ' <span class="badge badge-pill float-right badge-info">API</span>';
          } else if (full.UdpConnection) {
            return full.name + ' <span class="badge badge-pill float-right badge-info">UDP</span>';
          } else if (full.TcpIpConnection) {
            return full.name + ' <span class="badge badge-pill float-right badge-info">TCP/IP</span>';
          } else if (full.WsConnection) {
            return full.name + ' <span class="badge badge-pill float-right badge-info">WebScoket</span>';
          }
        }
      };
    } else if (/MessageType/i.test(key)) {
      tempc = {
        title: 'Message Type',
        width: '20%',
        mDataProp: key
      };
    } else if (/Port/i.test(key)) {
      tempc = {
        title: 'Port',
        width: '10%',
        mDataProp: key
      };
    } else if (/Status/i.test(key)) {
      tempc = {
        title: 'Status',
        width: '20%',
        mDataProp: key,
        mRender: function(data, full, row) {
          switch (data) {
            case 0:
              return 'Stopped';
            case 1:
              return 'Starting';
            case 2:
              return 'StartFailedWaitingToRestart';
            case 3:
              return 'Running';
            case 4:
              return 'Stopping';
            case 5:
              return 'ErrorPullingData';
            case 6:
              return 'InActive';
            case 7:
              return 'Idle';
            default:
              return 'No Status';
          }
        }
      };
    } else if (/Action/i.test(key)) {
      tempc = {
        title: 'Action',
        width: '20%',
        mDataProp: key,
        mRender: function(data, type, full) {
          if (/^(Admin|OIE)/i.test(appData.Role)) {
            Actioncolumn = true;
            return '<button class="btn btn-light btn-sm mx-1 pi-iconEdit connectionedit" name="connectionedit"></button>' + '<button class="btn btn-light btn-sm mx-1 pi-trashFill connectiondelete" name="connectiondelete"></button>';
          } else {
            return '';
          }
        }
      };
    } else {
      tempc = {
        title: capitalize_Words(key.replace(/\_/, ' ')),
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
        target: 4,
        visible: Actioncolumn
      },
      {
        orderable: false, // Disable sorting on all columns
        targets: '_all'
      }
    ],
    sorting: [[0, 'asc']],
    rowCallback: function(row, data) {
      $(row).find('td:eq(0)').css('text-align', 'left');
      $(row).find('td:eq(3)').css('text-align', 'center');
      if (data.activeConnection) {
        if (data.apiConnected) {
          if (/(0|4|5|6)/i.test(data.status)) {
            $(row).find('td:eq(3)').css('background-color', '#FF604E');
          } else {
            $(row).find('td:eq(3)').css('background-color', '#17C671');
          }
        } else if (data.tcpIpConnection) {
          $(row).find('td:eq(3)').css('background-color', '#17C671');
        } else if (data.udpConnection) {
          $(row).find('td:eq(3)').css('background-color', '#17C671');
        } else {
          $(row).find('td:eq(3)').css('background-color', '#FF604E');
        }
      } else {
        $(row).find('td:eq(3)').css('background-color', '#FFB400');
      }
    }
  });
  // Edit/remove record
  $('#' + table + ' tbody').on('click', 'button', function() {
    let td = $(this);
    let table = $(td).closest('table');
    let row = $(table).DataTable().row(td.closest('tr'));
    if (/connectionedit/gi.test(this.name)) {
      sidebar.close();
      Promise.all([Edit_Connection(row.data())]);
    } else if (/connectiondelete/gi.test(this.name)) {
      sidebar.close();
      Promise.all([Remove_Connection(row.data())]);
    }
  });
}

async function loadConnectionDatatable(data, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows.add(data).draw();
  }
}
async function updateConnectionDataTable(newdata, table) {
  try {
    return new Promise((resolve, reject) => {
      let loadnew = true;
      if ($.fn.dataTable.isDataTable('#' + table)) {
        $('#' + table).DataTable().rows(function(idx, data, node) {
          if (data.id === newdata.id) {
            loadnew = false;
            $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
          }
        });
        if (loadnew) {
          loadConnectionDatatable([newdata], table);
        }
      }
      resolve();
      return false;
    });
  } catch (e) {
    throw new Error(e.toString());
  }
}
function removeConnectionDataTable(removedata, table) {
  if ($.fn.dataTable.isDataTable('#' + table)) {
    $('#' + table).DataTable().rows(function(idx, data, node) {
      if (data.id === removedata) {
        $('#' + table).DataTable().row(node).remove().draw();
      }
    });
  }
}
function SortByConnectionName(a, b) {
  return a.name < b.name ? -1 : a.name > b.name ? 1 : 0;
}
async function addSideButton(name) {
  if (/^MPE/i.test(name)) {
    $('#MPESideButton').css('display', 'block');
  } else if (/^AGV/i.test(name)) {
    $('#AGVSideButton').css('display', 'block');
  } else if (/^SV/i.test(name)) {
    $('#TripSideButton').css('display', 'block');
  }
}

function enabletcpipudpSubmit() {
  if ($('select[name=message_type]').hasClass('is-valid') && $('select[name=data_retrieve]').hasClass('is-valid') && $('input[type=text][name=port_number]').hasClass('is-valid') && $('select[name=connection_name]').hasClass('is-valid')) {
    $('button[id=apisubmitBtn]').prop('disabled', false);
  } else {
    $('button[id=apisubmitBtn]').prop('disabled', true);
  }
}
function enableaipSubmit() {
  if ($('select[name=connection_name]').hasClass('is-valid') && $('select[name=message_type]').hasClass('is-valid') && $('input[type=text][name=url]').hasClass('is-valid') && ($('input[type=text][name=hostname]').hasClass('is-valid') || $('input[type=text][name=ip_address]').hasClass('is-valid')) && $('select[name=data_retrieve]').hasClass('is-valid') && $('select[name=connectionTimeout]').hasClass('is-valid')) {
    $('button[id=apisubmitBtn]').prop('disabled', false);
  } else {
    $('button[id=apisubmitBtn]').prop('disabled', true);
  }
}
function enableMessagetype() {
  if (!checkValue($('select[name=connection_name] option:selected').val())) {
    $('select[name=message_type]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_message_type]').text('Select Connection Name First');
  } else {
    if (checkValue($('select[name=message_type] option:selected').val())) {
      $('select[name=message_type]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
      $('span[id=error_message_type]').text('Select Message Type');
    } else {
      $('select[name=message_type]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
      $('span[id=error_message_type]').text('');
    }
  }
}
function filtermessage_type(name, type) {
  let conectionName = !!name ? name : $('#connection_name').find('option:selected').val();
  $('#message_type').children().appendTo('#option-container');
  let toMove = $('#option-container').children("[data-messagetype='" + conectionName + "']");
  toMove.appendTo('#message_type');
  let toBlankMove = $('#option-container').children("[data-messagetype='blank']");
  toBlankMove.appendTo('#message_type');
  $('#message_type').removeAttr('disabled');
  if (!!name) {
    $('select[id=connection_name]').val(name);
    $('select[id=connection_name]').prop('disabled', true);
  } else {
    $('select[id=connection_name]').prop('disabled', false);
  }
  if (!!type) {
    $('select[id=message_type]').val(type);
    $('select[id=message_type]').prop('disabled', true);
  } else {
    if (type === '') {
      $('select[id=message_type]').val('blank');
    } else {
      $('select[id=message_type]').val(type);
    }

    $('select[id=message_type]').prop('disabled', false);
  }
}
function onAPIConnection() {
  $('div[id="endpointurl"]').css('display', '');
  $('input[type=text][name=url]').prop('disabled', false);
  $('select[name=data_retrieve]').prop('disabled', false);
  $('input[type=text][name=url]').trigger('keyup');
  $('input[type=text][name=ip_address]').trigger('keyup');
  $('select[name=data_retrieve]').trigger('change');
  $('select[name=message_type]').trigger('change');
  enableaipSubmit();
}
function onudptcpipConnection() {
  $('div[id="endpointurl"]').css('display', 'none');
  $('div[id="OAuthmenu"]').css('display', 'none');
  $('div[id="serveripmenu"]').css('display', '');
  $('input[type=text][name=url]').prop('disabled', false);
  $('input[type=text][name=hostname]').prop('disabled', false);
  $('input[type=text][name=hostname]').val('');
  $('input[type=text][name=hostname]').css('border-color', '#D3D3D3').removeClass('is-valid').removeClass('is-invalid');
  $('span[id=error_hostname]').text('');
  if (!checkValue($('input[type=text][name=ip_address]').val())) {
    $('input[type=text][name=ip_address]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_ip_address]').text('Please Enter Valid IP address');
  } else {
    $('input[type=text][name=ip_address]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_ip_address]').text('');
  }
  if (!checkValue($('input[type=text][name=port_number]').val())) {
    $('input[type=text][name=port_number]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_port_number]').text('Please Enter Port Number');
  } else {
    $('input[type=text][name=port_number]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_port_number]').text('');
  }
  $('select[name=data_retrieve]').prop('disabled', true);
  $('select[name=data_retrieve]').val('0');
  if (!checkValue($('select[name=data_retrieve] option:selected').val())) {
    $('select[name=data_retrieve]').css('border-color', '#FF0000').removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_data_retrieve]').text('Select Data Retrieve Occurrences');
  } else {
    $('select[name=data_retrieve]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_data_retrieve]').text('');
  }
  enabletcpipudpSubmit();
}
function onidRequestConnection() {
  if (/^(api)/i.test(connTypeRadio)) {
    onAPIConnection();
  }
  $('div[id="IdRequest"]').css('display', '');
  $('input[type=text][name=tokenurl]').prop('disabled', false).css('display', 'block').val('');
  if (!checkValue($('input[type=text][name=idrequesturl]').val())) {
    $('input[type=text][name=idrequesturl]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_idrequesturl]').text('Please Enter Request Id URL');
  } else {
    $('input[type=text][name=idrequesturl]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_idrequesturl]').text('');
  }
  enableaipSubmit();
}
function offidRequestConnection() {
  $('div[id="IdRequest"]').css('display', 'none');
  $('input[type=text][name=idrequesturl]').prop('disabled', false).val('');
}

function onOAuthConnection() {
  $('div[id="OAuthmenu"]').css('display', '');
  $('div[id="divtokenurl"]').css('display', '');
  $('input[type=text][name=tokenurl]').prop('disabled', false).css('display', 'block').val('');
  if (!checkValue($('input[type=text][name=tokenurl]').val())) {
    $('input[type=text][name=tokenurl]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenurl]').text('Please Enter Token URL');
  } else {
    $('input[type=text][name=tokenurl]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenurl]').text('');
  }
  if (!checkValue($('input[type=text][name=tokenusername]').val())) {
    $('input[type=text][name=tokenusername]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenusername]').text('Please Enter UserName');
  } else {
    $('input[type=text][name=tokenusername]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenusername]').text('');
  }
  if (!checkValue($('input[type=text][name=tokenpassword]').val())) {
    $('input[type=text][name=tokenpassword]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenpassword]').text('Please Enter Password');
  } else {
    $('input[type=text][name=tokenpassword]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenpassword]').text('');
  }
  if (!checkValue($('input[type=text][name=tokenclientId]').val())) {
    $('input[type=text][name=tokenclientId]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenclientId]').text('Please Enter Client Id');
  } else {
    $('input[type=text][name=tokenclientId]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenclientId]').text('');
  }
}
function offOAuthConnection() {
  $('div[id="OAuthmenu"]').css('display', 'none');
  $('input[type=text][name=tokenurl]').prop('disabled', false).val('');
  $('input[type=text][name=tokenusername]').prop('disabled', false).val('');
  $('input[type=text][name=tokenpassword]').prop('disabled', false).val('');
  $('input[type=text][name=tokenclientId]').prop('disabled', false).val('');
}
function onBasicAuthConnection() {
  $('div[id="OAuthmenu"]').css('display', '');
  $('div[id="divtokenurl"]').css('display', 'none');
  $('input[type=text][name=tokenurl]').prop('disabled', true).css('display', 'none').val('');
  if (!checkValue($('input[type=text][name=tokenusername]').val())) {
    $('input[type=text][name=tokenusername]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenusername]').text('Please Enter UserName');
  } else {
    $('input[type=text][name=tokenusername]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenusername]').text('');
  }
  if (!checkValue($('input[type=text][name=tokenpassword]').val())) {
    $('input[type=text][name=tokenpassword]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenpassword]').text('Please Enter Password');
  } else {
    $('input[type=text][name=tokenpassword]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenpassword]').text('');
  }
  if (!checkValue($('input[type=text][name=tokenclientId]').val())) {
    $('input[type=text][name=tokenclientId]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_tokenclientId]').text('Please Enter Client Id');
  } else {
    $('input[type=text][name=tokenclientId]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_tokenclientId]').text('');
  }
}
function offBasicAuthConnection() {
  $('div[id="OAuthmenu"]').css('display', 'none');
  $('input[type=text][name=tokenurl]').prop('disabled', false).val('').removeClass('is-invalid').removeClass('is-valid');
  $('input[type=text][name=tokenusername]').prop('disabled', false).val('').removeClass('is-invalid').removeClass('is-valid');
  $('input[type=text][name=tokenpassword]').prop('disabled', false).val('').removeClass('is-invalid').removeClass('is-valid');
  $('input[type=text][name=tokenclientId]').prop('disabled', false).val('').removeClass('is-invalid').removeClass('is-valid');
}
function onBearerConnection() {
  $('div[id="BearerTokenmenu"]').css('display', '');
  if (!checkValue($('input[type=text][name=bearerToken]').val())) {
    $('input[type=text][name=bearerToken]').css({ 'border-color': '#FF0000' }).removeClass('is-valid').addClass('is-invalid');
    $('span[id=error_bearerToken]').text('Please Enter Token');
  } else {
    $('input[type=text][name=bearerToken]').css('border-color', '#2eb82e').removeClass('is-invalid').addClass('is-valid');
    $('span[id=error_bearerToken]').text('');
  }
}
function offBearerConnection() {
  $('div[id="BearerTokenmenu"]').css('display', 'none');
  $('input[type=text][name=bearerToken]').val('');
  $('input[type=text][name=bearerToken]').prop('disabled', false).val('');
}
