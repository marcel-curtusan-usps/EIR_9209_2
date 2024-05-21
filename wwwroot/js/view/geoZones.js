async function init_geoZone(goeZones) {
    return new Promise((resolve, reject) => {
        try {
            for (let i = 0; i < goeZones.length; i++) {
                const geoZone = goeZones[i];
                if (geoZone.properties.zoneType === 'MPEZone') {
                    Promise.all([addMPEFeature(goeZones[i])]);
                
                    // Handle type1 geoZone
                } else if (geoZone.properties.zoneType === 'DockDoorZone') {
                    Promise.all([addDockDoorFeature(goeZones[i])]);
                    // Handle type2 geoZone
                } else if (geoZone.type === 'type3') {
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