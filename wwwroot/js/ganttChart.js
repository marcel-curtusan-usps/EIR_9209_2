let chart = null;
let Ldata = null;
let engStandardDataset = [];

function GanttChart(chartID, chartdata, mpeName) {
    // Check if a chart instance already exists and destroy it
    if (chart) {
        chart.destroy();
    }
    $("#moddatedisplay").html(MPEdefaultMindate.toFormat('L/dd/yyyy T') + " To " + MPEdefaultMaxdate.toFormat('L/dd/yyyy T'));

    let runColor = 'rgb(0, 103, 244)';
    let planColor = 'rgb(33, 37, 41)';
    let standardColor = 'rgb(128, 0, 128)';
    let standardSetupTimeColor = 'rgb(51, 51, 255)';
    let standardpulldownTimeColor = 'rgb(204, 0, 102)';
    let standardchangeoverTimeColor = 'rgb(96, 96, 96)';

    let currentTimeLine = {
        id: 'currentTimeLine',
        afterDatasetsDraw(chart, args, pluginOptions) {
            const { ctx, data, chartArea: { top, bottom, left, right }, scales: { x, y } } = chart;
            ctx.save();
            ctx.beginPath();
            ctx.lineWidth = 3;
            ctx.strokeStyle = 'rgba(0,0,0,0.9)';
            ctx.moveTo(x.getPixelForValue(localdateTime.setZone("system", { keepLocalTime: true }).ts), top - 30);
            ctx.lineTo(x.getPixelForValue(localdateTime.setZone("system", { keepLocalTime: true }).ts), bottom);
            ctx.stroke();
            ctx.restore();

            // creating the ball on top of the line
            ctx.beginPath();
            ctx.arc(x.getPixelForValue(localdateTime.setZone("system", { keepLocalTime: true }).ts), top - 30, 7, 0, 2 * Math.PI);
            ctx.fillStyle = 'rgba(0,0,0,0.9)';
            ctx.fill();
            ctx.restore();
        }
    }
    let volumelabel = 115;
    let volumelabelvalue = 175;
    let throughputlabel = 255;
    let throughputlabelvalue = 325;
    const tourstimes = [];
    const { tour1Start, tour1End, tour2Start, tour2End, tour3Start, tour3End } = siteTours;
    let currentDate = localdateTime.minus({ days: 1 }).startOf('day');
    for (let i = 0; i < 5; i++) {
        const toursWithinMinMaxDateTime = [];

        function addToArrayIfWithinMinMaxDateTime(array, tourName, tourStartTime, tourEndTime) {
            const tourStart = luxon.DateTime.fromISO(tourStartTime);
            const tourEnd = luxon.DateTime.fromISO(tourEndTime);

            tourstimes.push({ name: tourName, startTime: tourStart, endTime: tourEnd });

        }

        addToArrayIfWithinMinMaxDateTime(
            toursWithinMinMaxDateTime,
            "TOUR 1",
            currentDate.minus({ day: 1 }).toFormat('yyyy-LL-dd') + "T" + tour1Start,
            currentDate.toFormat('yyyy-LL-dd') + "T" + tour1End,
        );

        addToArrayIfWithinMinMaxDateTime(
            toursWithinMinMaxDateTime,
            "TOUR 2",
            currentDate.toFormat('yyyy-LL-dd') + "T" + tour2Start,
            currentDate.toFormat('yyyy-LL-dd') + "T" + tour2End


        );

        addToArrayIfWithinMinMaxDateTime(
            toursWithinMinMaxDateTime,
            "TOUR 3",

            currentDate.toFormat('yyyy-LL-dd') + "T" + tour3Start,
            currentDate.toFormat('yyyy-LL-dd') + "T" + tour3End,
        );



        currentDate = currentDate.plus({ days: i })
    }

    const toursPlugin = {
        id: 'labelPlugin',
        afterDraw(chart) {
            const ctx = chart.ctx;
            const chartArea = chart.chartArea;
            ctx.save();
            ctx.font = 'bolder 14px  Roboto';
            ctx.padding = 10;
            ctx.textAlign = 'center';
            ctx.fillStyle = 'black';

            tourstimes.forEach(tours => {
                if ((tours.startTime.ts >= MPEdefaultMindate.ts && tours.endTime.ts <= MPEdefaultMaxdate.ts) ||
                    (tours.endTime.ts >= MPEdefaultMindate.ts && tours.endTime.ts <= MPEdefaultMaxdate.ts)
                ) {
                    const xStart = chart.scales.x.getPixelForValue(tours.startTime.ts);
                    const xEnd = chart.scales.x.getPixelForValue(tours.endTime.ts);
                    let xMiddle = (xStart + xEnd) / 2;
                    let yPosition = 20;

                    const tourDate = tours.startTime.toFormat('yyyy-LL-dd');

                    const labelWidth = ctx.measureText(tours.name).width;

                    if (xMiddle - labelWidth / 2 < chartArea.left) {
                        xMiddle = chartArea.left + labelWidth / 2;
                        ctx.textAlign = 'left';
                    }
                    else if
                        (xMiddle + labelWidth / 2 > chartArea.right) {
                        xMiddle = chartArea.right - labelWidth / 2;
                        ctx.textAlign = 'right';
                    }
                    else {
                        ctx.textAlign = 'center';
                    }

                    ctx.fillText(tours.name, xMiddle, yPosition + 5);
                    // ctx.fillText(tourDate, xMiddle, yPosition - 8);
                }
            });

            ctx.restore();
        }
    };

    let tourTimeLine = {
        id: 'tourTimeLine',
        afterDatasetsDraw(chart) {
            const { ctx, scales: { x, y } } = chart;
            const drawnLines = [];
            const tolerance = 5;
            ctx.save();
            tourstimes.forEach(tours => {
                if ((tours.startTime.ts >= MPEdefaultMindate.setZone("system", { keepLocalTime: true }).ts && tours.endTime.ts <= MPEdefaultMaxdate.setZone("system", { keepLocalTime: true }).ts) ||
                    (tours.endTime.ts >= MPEdefaultMindate.setZone("system", { keepLocalTime: true }).ts && tours.endTime.ts <= MPEdefaultMaxdate.setZone("system", { keepLocalTime: true }).ts)
                ) {
                    const xPos = chart.scales.x.getPixelForValue(tours.startTime.ts);
                    const lineExists = drawnLines.some(drawnX => Math.abs(drawnX - xPos) < tolerance);
                    if (!isNaN(xPos) && xPos >= x.left && xPos <= x.right && !lineExists) {
                        drawnLines.push(xPos);
                        ctx.beginPath();
                        ctx.lineWidth = 1;
                        ctx.strokeStyle = 'rgba(0,0,0,0.2)';
                        ctx.moveTo(xPos, y.top);
                        ctx.lineTo(xPos, y.top - 50);
                        ctx.stroke();

                    }
                }
            });
            ctx.restore();
        }
    }
    const Plandata = []
    const Rundata = []
    const Standarddata = []
    chartdata.forEach((key, value) => {
        if (key.type === "Plan") {
            Plandata.push(key);
        }
    });
    chartdata.forEach((key, value) => {
        if (key.type === "Run") {
            Rundata.push(key);
        }
    });
    chartdata.forEach((key, value) => {
        if (key.type === "Standard") {
            Standarddata.push(key);
        }
    });
    engStandardDataset = {
        label: 'Standard',
        data: Standarddata,
        backgroundColor: getSortplanColor(Standarddata),
        borderColor: [
            'rgba(0,0,0,5)'
        ],
        barPercentage: 0.7,
        borderWidth: 1,
        borderRadius: 5,
        order: 3
    }

    // setup 
    Ldata = {
        datasets: [{
            label: 'Plan',
            data: Plandata,
            backgroundColor: getSortplanColor(Plandata),
            borderColor: [
                'rgba(54, 162, 235, 5)'
            ],
            barPercentage: 0.7,
            borderSkipped: false,
            borderRadius: 5,
            order: 1
        },
        {
            label: 'Run',
            data: Rundata,
            backgroundColor: getSortplanColor(Rundata),
            borderColor: [
                'rgba(0,0,0,5)'
            ],
            barPercentage: 0.7,
            borderSkipped: false,
            borderRadius: 5,
            order: 2
        }

        ]
    };
    function createDiagonalPattern(color) {
        let shape = document.createElement('canvas')
        shape.width = 10
        shape.height = 10
        let c = shape.getContext('2d')
        c.strokeStyle = color
        c.beginPath()
        c.moveTo(2, 0)
        c.lineTo(10, 8)
        c.stroke()
        c.beginPath()
        c.moveTo(0, 8)
        c.lineTo(2, 10)
        c.stroke()
        return c.createPattern(shape, 'repeat');
    }

    function getSortplanColor(data) {
        let bgresult = [];


        data.forEach((item) => {

            //standard
            if (/standard/i.test(item.type)) {
                if (/Setup Time/i.test(item.sortPlanName)) {
                    bgresult.push(standardSetupTimeColor);
                }
                else if (/Teardown Time/i.test(item.sortPlanName)) {
                    bgresult.push(standardpulldownTimeColor);
                }
                else if (/ChangeOver Time/i.test(item.sortPlanName)) {
                    bgresult.push(standardchangeoverTimeColor);
                }
                else {
                    bgresult.push(standardColor);
                }

            }
            //plan
            if (/plan/i.test(item.type) && item.opn === 750) {
                bgresult.push(createDiagonalPattern(planColor));
            }
            else if (/plan/i.test(item.type) && item.opn !== 750) {
                bgresult.push(planColor);
            }
            //run
            else if (/run/i.test(item.type) && item.opn === 750) {
                bgresult.push(createDiagonalPattern(runColor));
            }
            else if (/run/i.test(item.type) && item.opn !== 750) {
                bgresult.push(runColor);
            }


        });
        return bgresult;
    }

    // config 
    const config = {
        type: 'bar',
        data: Ldata,
        options: {
            responsive: true,
            animation: false,
            parsing: {
                xAxisKey: 'startToEndtime',
                yAxisKey: 'type'
            },
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: -0,
                    top: 45,
                    right: 50
                }
            },
            indexAxis: 'y',
            scales: {
                x: {
                    position: 'top',
                    min: MPEdefaultMindate.setZone("system", { keepLocalTime: true }).ts,
                    max: MPEdefaultMaxdate.setZone("system", { keepLocalTime: true }).ts,
                    type: 'time',
                    time: {
                        displayFormats: {
                            hour: "HH:mm",
                        },
                        unit: 'hour'
                    }
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    afterFit: function (scaleInstance) {
                        scaleInstance.width = 60 // sets the width to 100px
                    }
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {

                        beforeTitle: function (context) {
                            let label = "Type: " +
                                context[0].dataset.data[context[0].dataIndex].type
                            return label;
                        },
                        title: function (context) {
                            let label = "Sortplan: " +
                                context[0].dataset.data[context[0].dataIndex].sortPlanName
                            return label;
                        },
                        afterTitle: function (context) {
                            let label = "OPN: " +
                                context[0].dataset.data[context[0].dataIndex].opn
                            return label;
                        },
                        beforeLabel: function (context) {
                            let label = "Expected Fed: " + parseFloat(context.dataset.data[context.dataIndex].expectedPiecesFed).toLocaleString('en-US')

                            return label;
                        },
                        label: function () {
                            let label = ""

                            return label;
                        },
                        afterLabel: function (context) {
                            let label = "Actual Fed: " + parseFloat(context.dataset.data[context.dataIndex].actualVolume).toLocaleString('en-US')

                            return label;
                        },
                        beforeFooter: function (context) {
                            let label = "Start Time: " +
                                luxon.DateTime.fromJSDate(new Date(context[0].dataset.data[context[0].dataIndex].startToEndtime[0])).toFormat('L/dd/yyyy T');
                            return label;
                        },
                        afterFooter: function (context) {
                            let label = "End Time: " +
                                luxon.DateTime.fromJSDate(new Date(context[0].dataset.data[context[0].dataIndex].startToEndtime[1])).toFormat('L/dd/yyyy T');
                            return label;
                        }
                    }
                },
                datalabels: {
                    clip: true,
                    color: function (context) {
                        let opn = context.dataset.data[context.dataIndex].opn;
                        let type = context.dataset.data[context.dataIndex].type;
                        if (/standard/i.test(type)) {
                            return 'white';
                        }
                        if (/plan/i.test(type) && opn === 750) {
                            return runColor;
                        }
                        else if (/run/i.test(type) && opn === 750) {
                            return planColor;
                        }
                        else {
                            return 'white';
                        }

                    },
                    formatter: function (value, context) {
                        let lt = '';
                        let barlength = context.chart.getDatasetMeta(context.datasetIndex).data[context.dataIndex].width;
                        if ((barlength > 50)) {
                            lt = '';
                            if (/standard/i.test(context.dataset.data[context.dataIndex].type)) {
                                lt = ' (OPN # ' + context.dataset.data[context.dataIndex].opn + ') ' + context.dataset.data[context.dataIndex].sortPlanName;
                                return lt;
                            }

                            else {
                                lt = context.dataset.data[context.dataIndex].sortPlanName
                            }
                            return lt;
                        } else {
                            return lt
                        }
                    }
                }
            },

            events: ['mousemove', 'mouseout', 'click', 'touchstart', 'touchmove'],

            onClick: (e) => {
                if (e.chart._active.length > 0) {
                    let dataIndex = e.chart._active[0].datasetIndex;
                    let activeIndex = e.chart._active[0].index;
                    let chartData = chart.data.datasets[dataIndex].data[activeIndex]
                    if (chartData.type === 'Run') {
                        const startTime = luxon.DateTime.fromISO(chartData.startToEndtime[0]);
                        const stopTime = luxon.DateTime.fromISO(chartData.startToEndtime[1]);
                        Promise.all([loadSpecificTimeData(startTime, stopTime)]);
                    }
                }
                else {
                    Promise.all([loadData("")]);
                }

            }
        },
        plugins: [currentTimeLine, toursPlugin, ChartDataLabels, tourTimeLine]
    };
    chart = new Chart(chartID, config);
}
function updateChart() {
    let standardDataset = chart.data.datasets.find(dataset => dataset.label === 'Standard');

    if ($.type(standardDataset) === "undefined") {
        chart.data.datasets.push(engStandardDataset)

    }
    else {
        const indexToRemove = chart.data.datasets.findIndex(dataset => dataset.label === 'Standard');
        if (indexToRemove !== -1) {
            chart.data.datasets.splice(indexToRemove, 1);
        }
    }

    chart.update();

}

let sampleData = [
    {
        "sortPlanName": "MAINT",
        "startToEndtime": [
            "2023-12-16T03:00:00",
            "2023-12-16T07:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 750,
        "type": "Plan"
    },
    {
        "sortPlanName": "MAINT",
        "startToEndtime": [
            "2023-12-17T03:00:00",
            "2023-12-17T07:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 750,
        "type": "Plan"
    },
    {
        "sortPlanName": "118XDOGP",
        "startToEndtime": [
            "2023-12-15T23:00:00",
            "2023-12-16T03:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 118,
        "type": "Plan"
    },
    {
        "sortPlanName": "118XDOGP",
        "startToEndtime": [
            "2023-12-14T23:00:00",
            "2023-12-15T03:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 118,
        "type": "Plan"
    },
    {
        "sortPlanName": "118XDOGP",
        "startToEndtime": [
            "2023-12-16T23:00:00",
            "2023-12-17T03:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 118,
        "type": "Plan"
    },
    {
        "sortPlanName": "MAINT",
        "startToEndtime": [
            "2023-12-15T03:00:00",
            "2023-12-15T07:00:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 750,
        "type": "Plan"
    },
    {
        "sortPlanName": "118XDOGP",
        "startToEndtime": [
            "2023-12-15T14:52:11",
            "2023-12-15T16:07:49.5945212-05:00"
        ],
        "mpeName": "HOPS-002",
        "mpeNumber": 2,
        "opn": 118,
        "type": "Run"
    }
]