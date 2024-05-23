let linechart = null;
let linechartlist = [];
function LineChart(chartID, chartdata, mindate, maxdate) {
    if (linechartlist[chartID]) {
        linechartlist[chartID].destroy();
    }


    let fedColor = 'rgb(0, 103, 244)';
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
    let currentTimeLine = {
        id: 'currentTimeLine',
        afterDatasetsDraw(chart) {
            const { ctx, data, chartArea: { top, bottom, left, right }, scales: { x, y } } = chart;
            ctx.save();
            ctx.beginPath();
            ctx.lineWidth = 3;
            ctx.strokeStyle = 'rgba(0,0,0,0.9)';
            ctx.moveTo(x.getPixelForValue(new Date()), top - 30);
            ctx.lineTo(x.getPixelForValue(new Date()), bottom);
            ctx.stroke();
            ctx.restore();

        }
    }

    const zdata = {
        datasets: [
            {
                label: 'Pcs Fed',
                type: 'line',
                data: Object.values(chartdata).sort((a, b) => new Date(a.hour) - new Date(b.hour)).map(od => ({ x: new Date(od.hour), y: od.piecesFeed })),
                borderColor: fedColor,
                backgroundColor: fedColor,
                yAxisID: 'yPeicesFed',
                order: 1
            },
            {
                label: 'Labor Hrs',
                type: 'bar',
                data: Object.values(chartdata).sort((a, b) => new Date(a.hour) - new Date(b.hour)).map(od => ({ x: new Date(od.hour), y: parseFloat((od.totalDwellTime / 3600000).toFixed(1)).toLocaleString('en-US') })),
                borderColor: staffingColor,
                backgroundColor: staffingColor,
                borderRadius: 5,
                barPercentage: 0.50,
                yAxisID: 'yStaffing',
                order: 2
            }
        ]
    };
    //plugins
    const topLine = {
        id: 'topLine',
        afterDatasetsDraw(chart, args, plugins) {
            const { ctx, data } = chart;
            ctx.save();
            chart.getDatasetMeta(0).data.forEach((datapoint, index) => {
                ctx.beginPath();
                ctx.strokeStyle = data.datasets[0].borderColor[index];
                ctx.lineWidth = 3;
                const halfWidth = datapoint.x / 2;
                ctx.moveTo(datapoint.x - halfWidth, datapoint.y - 6);
                ctx.lineTo(datapoint.x - halfWidth, datapoint.y - 6);
                ctx.stroke();

                //text
                ctx.font = 'bold 12px sans-serif';
                ctx.fillStyle = data.datasets[0].borderColor[index];
                ctx.textAlign = 'center';
                ctx.fillText(data.datasets[0].data[index].y + ' Hrs', datapoint.x, datapoint.y - 15);

            });
        }
    }
    // config 
    const config = {
        data: zdata,
        options: {
            animation: {
                duration: 0
            },
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

                            let label = "Time: " + luxon.DateTime.fromJSDate(new Date(context.dataset.data[context.dataIndex].x)).toFormat('L/dd/yyyy T');
                            return label;

                        },
                        beforeFooter: function (context) {
                            let label = "";
                            if (context[0].dataset.type === 'bar') {
                                label = "Labor Hrs: " + context[0].dataset.data[context[0].dataIndex].y + " Hrs";
                            }
                            if (context[0].dataset.type === 'line') {
                                if (context[0].dataset.yAxisID === "yPeicesFed") {
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