async function init_geoZone(goeZones) {
    return new Promise((resolve, reject) => {
        try {
            for (let i = 0; i < goeZones.length; i++) {
                const geoZone = goeZones[i];
                if (geoZone.properties.zoneType === 'MPEZone') {
                    Promise.all([addMPEFeature(goeZones[i])]);
                    // Handle DockDoorZone geoZone
                } else if (geoZone.properties.zoneType === 'DockDoorZone') {
                    Promise.all([addDockDoorFeature(goeZones[i])]);
                    // Handle MPEBinZone geoZone
                } else if (geoZone.properties.zoneType === 'MPEBinZone') {
                    Promise.all([addMPEBinFeature(goeZones[i])]);
                    // Handle type3 geoZone
                } else {
                    // Handle other types of geoZone
                }
            }
            resolve();
            return false;
        } catch (e) {
            reject();
            return false;
        }
    });
}