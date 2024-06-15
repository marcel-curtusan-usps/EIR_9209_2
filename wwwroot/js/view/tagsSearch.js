let TagSearchtable = "tagresulttable";
let search_Table = $('table[id=tagresulttable]');

$(function () {
    // Search Tag Name
    $('input[id=inputsearchbtn').on("keyup", function () {
        startSearch(this.value)
    }).on("keydown", function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });
});
async function init_TagSearch() {
    Promise.all([createTagSearchDataTable(TagSearchtable)]);
}
async function startSearch(searchValue) {
    //clear layer tooltip class list for all tags

    if (checkValue(searchValue)) {
        let uri = SiteURLconstructor(window.location) + "api/Tag/Search?id=" + searchValue;

        if (!!searchValue) {
            //makea ajax call to get the employee details
            $.ajax({
                url: uri,
                type: 'GET',
                success: function (data) {
                    Promise.all([clearLayerTooltip()]);

                    if ($.fn.dataTable.isDataTable("#" + TagSearchtable)) {
                        $('#' + TagSearchtable).DataTable().clear().draw();
                    }
                    // Empty the DOM element which contained DataTable

                    loadTagSearchDataTable(data, TagSearchtable);
                    //set layer tooltip class list for tags in properties list.
                    Promise.all([setLayerTooltip(data)]);

                },
                error: function (error) {
                    console.log(error);
                },
                faulure: function (fail) {
                    console.log(fail);
                },
                complete: function (complete) {
                }
            });
        }
    }
    else {
        Promise.all([clearLayerTooltip()]);
        if ($.fn.dataTable.isDataTable("#" + TagSearchtable)) {
            $('#' + TagSearchtable).DataTable().clear().draw();
        }
    }
}
async function setLayerTooltip(tagid) {
    return new Promise((resolve, reject) => {
        if (map.hasOwnProperty("_layers")) {
            //loop trough tagid list 
            tagid.forEach(function (item) {
                $.map(map._layers, function (layer, i) {
                    if (layer.hasOwnProperty("feature") && layer.feature.properties.hasOwnProperty("Tag_Type")) {
                        if (/(person)|(Vehicle$)/i.test(layer.feature.properties.Tag_Type)) {
                            if (item.id === layer.feature.properties.id) {
                                if (layer.hasOwnProperty("_tooltip") && layer._tooltip.hasOwnProperty("_container")) {
                                    if (!layer._tooltip._container.classList.contains('searchflash')) {
                                        layer._tooltip._container.classList.add('searchflash');
                                    }
                                }
                            }
                        }
                    }
                })
            });
        }
        resolve();
        return;
    })

}
async function clearLayerTooltip() {
    return new Promise((resolve, reject) => {
        if (map.hasOwnProperty("_layers")) {
            $.map(map._layers, function (layer, i) {
                if (layer.hasOwnProperty("feature") && layer.feature.properties.hasOwnProperty("Tag_Type")) {
                    if (/(person)|(Vehicle$)/i.test(layer.feature.properties.Tag_Type)) {
                        if (layer.hasOwnProperty("_tooltip") && layer._tooltip.hasOwnProperty("_container")) {
                            if (layer._tooltip._container.classList.contains('searchflash')) {
                                layer._tooltip._container.classList.remove('searchflash');
                            }
                        }
                    }
                }
            })
        }
        resolve();
        return;
    })

}
async function createTagSearchDataTable(table) {
    let arrayColums = [{
        "id": "",
        "eIN": "",
        "craftName": "",
        "Action": ""
    }]
    let columns = [];
    let tempc = {};

    $.each(arrayColums[0], function (key) {
        tempc = {};
        if (/id/i.test(key)) {
            tempc = {
                "title": 'Id',
                "width": "30%",
                "mDataProp": key
            }
        }
        if (/ein/i.test(key)) {
            tempc = {
                "title": "EIN",
                "width": "30%",
                "mDataProp": key
            }
        }
        if (/craftName/i.test(key)) {
            tempc = {
                "title": "Type",
                "width": "30%",
                "mDataProp": key
            }
        }
        if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "width": "10%",
                "mRender": function (data, full) {
                    return '<button type="button" class="btn btn-light btn-sm mx-1 bi bi-bullseye" data-toggle="modal" name="tagnav" "title="Edit Tag Info"></button>' +
                        '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit" data-toggle="modal" name="tagedit" "title="Edit Tag Info"></button>';
                }
            }
        }
        columns.push(tempc);

    });
    $('#' + table).DataTable({
        dom: 'Bfrtip',
        filter: false,
        deferRender: true,
        paging: false,
        paginate: false,
        autoWidth: false,
        info: false,
        destroy: true,
        fixedHeader: true,
        language: {
            emptyTable: "Please Enter Tag / EIN / Name / EncodedID"
        },
        columns: columns,
        scrollY: "250px",
        columnDefs: [
        ],
        sorting: false, // Disable sorting
        buttons: [
            'copy', 'csv', 'excel', 'pdf', 'print'
        ]
    });
    // Edit/remove record
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/tagedit/ig.test(this.name)) {
            Promise.all([EditUserInfo(row.data())]);
        }
        if (/tagnav/ig.test(this.name)) {
            Promise.all([moveToTagLocation(row.data().id)]);
        }
    });
}
function loadTagSearchDataTable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
async function updateTagSearchDataTable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            if (data.Id === newdata.Id) {
                loadnew = false;
                $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
            }
        })
        if (loadnew) {
            loadTagSearchDataTable(newdata, table);
        }
    }
}
function removeTagSearchDataTable(ldata, table) {
    $('#' + table).DataTable().rows(function (idx, data, node) {
        if (data.Id === ldata.Id) {
            $('#' + table).DataTable().row(node).remove().draw();
        }
    });
}
async function moveToTagLocation(id) {

    $.map(map._layers, function (layer, i) {
        if (layer.hasOwnProperty("feature") && layer.feature.properties.hasOwnProperty("Tag_Type")) {
            if (/(person)|(Vehicle$)/i.test(layer.feature.properties.Tag_Type)) {
                if (id === layer.feature.properties.id) {
                    map.setView(layer._latlng, 5);
                }
            }
        }
    })

}