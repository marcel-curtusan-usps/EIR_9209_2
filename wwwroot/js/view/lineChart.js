let linechart = null;
let linechartlist = [];
function LineChart(chartID, chartdata, mindate, maxdate) {
    if (linechartlist[chartID]) {
        linechartlist[chartID].destroy();
    }

    let fedColor = 'rgb(0, 103, 244)';
    let standardfedColor = 'rgb(237, 187, 153)';
    let standardstaffingColor = 'rgb(142, 68, 173)';
    let throuColor = 'rgb(33, 37, 41)';
    let staffingColor = 'rgb(48, 208, 116)';
    let chartPadding = {};
    if (chartID === "lineperfChart") {
        chartPadding = {
            "left": 15,
            "top": 5,
            "right": 0
        }
    }
    if (/^(site)/.test(chartID)) {
        chartPadding = {
            "left": 0,
            "top": 10,
            "right": 0
        }
    }
    const zdata = {
        datasets: [
            {
                label: 'Pcs Fed',
                type: 'line',
                data: Object.values(chartdata).sort((a, b) => luxon.DateTime.fromISO(a.hour) - luxon.DateTime.fromISO(b.hour)).map(od => ({ x: luxon.DateTime.fromISO(od.hour), y: od.piecesFeed })),
                borderColor: fedColor,
                backgroundColor: fedColor,
                yAxisID: 'yPeicesFed',
                order: 1
            },
            {
                label: 'Labor Hrs',
                type: 'bar',
                data: Object.values(chartdata).sort((a, b) => luxon.DateTime.fromISO(a.hour) - luxon.DateTime.fromISO(b.hour)).map(od => ({ x: luxon.DateTime.fromISO(od.hour), y: parseFloat((od.totalDwellTime / 3600000).toFixed(1)).toLocaleString('en-US') })),
                borderColor: staffingColor,
                backgroundColor: staffingColor,
                borderRadius: 5,
                barPercentage: 0.50,
                yAxisID: 'yStaffing',
                order: 2
            },
            {
                label: 'Std. Throughput',
                type: 'line',
                data: Object.values(chartdata).sort((a, b) => luxon.DateTime.fromISO(a.hour) - luxon.DateTime.fromISO(b.hour)).map(od => ({ x: luxon.DateTime.fromISO(od.hour), y: od.standardPiecseFeed })),
                borderColor: standardfedColor,
                backgroundColor: standardfedColor,
                yAxisID: 'yStandardPeicesFed',
                order: 4
            }, {
                label: 'Std. Operational Staffing',
                type: 'line',
                data: Object.values(chartdata).sort((a, b) => luxon.DateTime.fromISO(a.hour) - luxon.DateTime.fromISO(b.hour)).map(od => ({ x: luxon.DateTime.fromISO(od.hour), y: od.standardStaffHour })),
                borderColor: standardstaffingColor,
                backgroundColor: standardstaffingColor,
                yAxisID: 'ySandardStaffing',
                order: 4
            }
        ]
    };
    // config 
    const config = {
        data: zdata,
        options: {
            animation: false,
            responsive: true,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        beforeTitle: function () {
                            let label = ""

                            return label;
                        },
                        label: function (context) {

                            let label = "Time: " + context.dataset.data[context.dataIndex].x.toFormat("yyyy-MM-dd HH:mm");
                            return label;

                        },
                        beforeFooter: function (context) {
                            let label = "";
                            if (context[0].dataset.type === 'bar') {
                                label = "Labor Hrs: " + context[0].dataset.data[context[0].dataIndex].y + " Hrs";

                            }
                            if (context[0].dataset.type === 'line') {
                                if (context[0].dataset.yAxisID === "ySandardStaffing") {
                                    label = "Standard Staffing: " + parseFloat(context[0].dataset.data[context[0].dataIndex].y).toLocaleString('en-US') + " Hrs";
                                }
                                else if (context[0].dataset.yAxisID === "yStandardPeicesFed") {
                                    label = "Standard Throughput: " + parseFloat(context[0].dataset.data[context[0].dataIndex].y).toLocaleString('en-US');
                                }
                                else if (context[0].dataset.yAxisID === "yPeicesFed") {
                                    label = "Actual Fed: " + parseFloat(context[0].dataset.data[context[0].dataIndex].y).toLocaleString('en-US');
                                }

                            }
                            return label;
                        }
                    }
                },
                datalabels: {
                    clip: true,
                    anchor: 'end',
                    align: 'top',
                    offset: 8,
                    padding: {
                        top: -18,
                    },
                    color: function () {
                        return 'black';
                    },
                    formatter: function (value, context) {
                        if (context.dataset.type === 'line') {
                            //if (context.dataset.data[context.dataIndex].y > 15) {
                            //    let lt = parseFloat(context.dataset.data[context.dataIndex].y).toLocaleString('en-US');
                            //    return lt;
                            //} else {
                            return ''
                            //}

                        }
                        if (context.dataset.type === 'bar') {
                            let barlength = context.chart.getDatasetMeta(context.datasetIndex).data[context.dataIndex].x;
                            if ((barlength > 50)) {
                                let lt = context.dataset.data[context.dataIndex].y;
                                return lt;
                            } else {
                                return ''
                            }

                        }

                    }
                }
            },
            scales: {
                x: {
                    min: mindate.ts,
                    max: maxdate.ts,

                    type: 'time',
                    time: {
                        displayFormats: {
                            hour: "HH:mm",
                        },
                        unit: 'hour'
                    }
                },
                yPeicesFed: {
                    position: 'left',
                    beginAtZero: true,
                    grid: {
                        display: false
                    },
                    ticks: {
                        maxTicksLimit: 100000,
                    }
                },
                yStaffing: { // line chart scale
                    position: 'right',
                    grace: '20%',
                    beginAtZero: true,
                    ticks: {
                        callback: (duration) => String(duration) + ' Hrs'
                    }

                },
                ySandardStaffing: {
                    position: 'right',
                    grace: '80%',
                    beginAtZero: true,
                    display: false,
                    ticks: {
                        max: 50,
                        stepSize: 5,
                        callback: (duration) => String(duration) + ' Hrs'
                    }
                },
                yStandardPeicesFed: {
                    type: 'linear',
                    beginAtZero: true,
                    display: false,
                    grid: {
                        display: false
                    },
                    ticks: {
                        max: 500000
                    }
                }
            },
            layout: {
                padding: chartPadding
            },
            maintainAspectRatio: false
        },
        plugins: [ChartDataLabels]

    };

    linechartlist[chartID] = new Chart(chartID, config);

}
function updateLineChart(chartID, chartdata, mindate, maxdate) {
    //update chart if the line chart with the name of standard is present

    if (linechartlist[chartID]) {

        linechartlist[chartID].update();
    }
}