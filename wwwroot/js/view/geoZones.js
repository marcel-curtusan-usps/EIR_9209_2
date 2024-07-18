async function init_geoZone(goeZones) {
    return new Promise((resolve, reject) => {
        try {
            for (let i = 0; i < goeZones.length; i++) {
                const geoZone = goeZones[i];
                if (/^(MPE)/ig.test(geoZone.properties.zoneType)) {
                    Promise.all([addMPEFeature(goeZones[i])]);
                    // Handle DockDoorZone geoZone
                } else if (/^(Dockdoor)/ig.test(geoZone.properties.zoneType)) {
                    Promise.all([addDockDoorFeature(goeZones[i])]);
                    // Handle MPEBinZone geoZone
                } else if (/^(Bin)/ig.test(geoZone.properties.zoneType)) {
                    Promise.all([addBinFeature(goeZones[i])]);
                    // Handle type3 geoZone
                } else if (/^(AGVLocation)/ig.test(geoZone.properties.zoneType)) {
                    Promise.all([addAGVLocationFeature(goeZones[i])]);
                    // Handle type3 geoZone
                }
                else if (/^(Area)/ig.test(geoZone.properties.zoneType)) {
                    Promise.all([addAreaFeature(goeZones[i])]);
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