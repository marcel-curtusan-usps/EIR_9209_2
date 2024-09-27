async function init_geoZone(goeZones) {
    return new Promise((resolve, reject) => {
        try {
            for (let i = 0; i < goeZones.length; i++) {
                const geoZone = goeZones[i];
             if (/^(Dockdoor)/ig.test(geoZone.properties.type)) {
                    Promise.all([addDockDoorFeature(goeZones[i])]);
                    // Handle MPEBinZone geoZone
                } else if (/^(AGVLocation)/ig.test(geoZone.properties.type)) {
                    Promise.all([addAGVLocationFeature(goeZones[i])]);
                    // Handle type3 geoZone
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