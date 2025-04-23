let retryCount = 0;
const maxRetries = 5;
let listofGroups = [];
const connection = new signalR.HubConnectionBuilder()
  .withUrl(SiteURLconstructor(window.location) + '/hubServics')
  .withAutomaticReconnect([0, 2000, 10000, 30000]) // Exponential backoff intervals
  .configureLogging(signalR.LogLevel.Information)
  .withHubProtocol(new signalR.JsonHubProtocol())
  .build();

async function init_signalRConnection(data) {
  try {
    await connection.start().then(async () => {
      // Join each group and remove from list if successful
      await Promise.all(
        listofGroups.map(async (group, index) => {
          try {
            await JoinGroup(group);
            listofGroups.splice(index, 1); // Remove group from list
          } catch (err) {
            console.info(`Error joining group ${group}:`, err);
          }
        })
      );
    });
    console.info('SignalR connection established.');
    retryCount = 0; // Reset retry count on successful connection
  } catch (err) {
    console.info('Error starting SignalR connection:', err);
    retryCount++;
    if (retryCount < maxRetries) {
      setTimeout(init_signalRConnection, 2000); // Retry after 2 seconds
    } else {
      console.info('Max retries reached. Could not connect to SignalR.');
      showConnectionStatus('Unable to connect. Please check your network.');
    }
  }
}
async function JoinGroup(_group) {
  await connection.invoke('JoinGroup', _group);
}
async function LeaveGroup(_group) {
  await connection.invoke('LeaveGroup', _group);
}
async function addGroupToList(group) {
  if (connection.state === signalR.HubConnectionState.Connected) {
    await JoinGroup(group);
  } else {
    if (!listofGroups.includes(group)) {
      listofGroups.push(group);
    }
  }
}
async function handleGroupChange(isChecked, groupName) {
  if (isChecked) {
    await addGroupToList(groupName);
  } else {
    await removeFromGroupList(groupName);
  }
}
async function removeFromGroupList(group) {
  if (connection.state === signalR.HubConnectionState.Connected) {
    await LeaveGroup(group);
  }
}
connection.onclose(async () => {
  console.info('Connection closed. Attempting to reconnect...');
  showConnectionStatus('Connection lost. Attempting to reconnect...');
  await start();
});

connection.onreconnecting(error => {
  console.info('Reconnecting...', error);
  showConnectionStatus('Reconnecting...');
});

connection.onreconnected(connectionId => {
  console.info('Reconnected. Connection ID: ', connectionId);
  showConnectionStatus('Reconnected.');
});
function SiteURLconstructor(winLoc) {
  let pathname = winLoc.pathname;
  let match = pathname.match(/^\/([^\/]*)/);
  let urlPath = match[1];
  if (/^(CF)/i.test(urlPath)) {
    return winLoc.origin + '/' + urlPath;
  } else {
    return winLoc.origin;
  }
}
