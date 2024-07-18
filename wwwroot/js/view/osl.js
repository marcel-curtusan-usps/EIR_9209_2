
let nextSibling, fileName = "", checked;
$('#OSL_Modal').on('shown.bs.modal', function () {
    if (fileName === "" && $('select[id=metersPerPixel] option:selected').val() === "") {
        $("button[id=oslUploadbtn]").prop("disabled", true);
    }
    else {
        $("button[id=oslUploadbtn]").prop("disabled", false);
    }
    $('select[name=metersPerPixel]').change(function () {
        if (fileName !== "" && $('select[id=metersPerPixel] option:selected').val() !== "") {
            $("button[id=oslUploadbtn]").prop("disabled", false);
        }
        else {
            $("button[id=oslUploadbtn]").prop("disabled", true);
        }
    });
    $("#fupload").change(function (e) {
        if (document.getElementById("fupload").files.length > 0) {
            fileName = document.getElementById("fupload").files[0].name;
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = fileName
        }
        else {
            fileName = "";
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = 'Choose file'
        }

        $('span[id=error_oslUploadbtn]').text("");
        if (fileName === "" && $('select[id=metersPerPixel] option:selected').val() !== "") {
            $("button[id=oslUploadbtn]").prop("disabled", false);
        }
        else {
            $("button[id=oslUploadbtn]").prop("disabled", true);
        }
    });
});
$('#OSL_Modal').on('hidden.bs.modal', function () {
    $(this)
        .find("input[type=text],textarea,select")
        .css({ "border-color": "#D3D3D3" })
        .val('')
        .end()
        .find("span[class=text]")
        .css("border-color", "#FF0000")
        .val('')
        .text("")
        .end()
        .find('input[type=checkbox]')
        .prop('checked', false).change();
    let progress = 0;
    $('#progresbarrow').css('display', 'none');
    $('#file_upload_progressbar').css('width', progress + '%');
    $('input[id=fupload][type=file]').val("");
    document.getElementById("fupload").nextElementSibling.innerText = 'Choose file';
    $('span[id=error_oslUploadbtn]').text("");
    $('button[id=oslUploadbtn]').prop("disabled", true);
    sidebar.open('setting');
});
async function init_osl() {
    try {
        createOslDataTable("osltable");
        $('button[name=addOslPan]').off().on('click', function () {
            /* close the sidebar */
            sidebar.close();
            Add_OSL();
        });
        connection.invoke("JoinGroup", "OSL").catch(function (err) {
            return console.error(err.toString());
        });
    } catch (e) {
        console.log(e)
    }
}
connection.on("addOSL", async (osldata) => {
    addOSLFeature(osldata);

});
connection.on("deleteOSL", async (osldata) => {
    deleteOSLFeature(osldata);

});
connection.on("updateOSL", async (osldata) => {


});
async function deleteOSLFeature(data) {
    try {
        removeOslDatatable(data, "osltable");
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
async function addOSLFeature(data) {
    try {

        loadOslDatatable([data], "osltable")
    }
    catch (e) {
        throw new Error(e.toString());
    }
}
function Add_OSL() {
    $('#floormodalHeader_ID').text('Add New Floor Plan');
    $("button[id=oslUploadbtn]").css("display", "block");
    $("button[id=oslSubmitbtn]").css("display", "none");
    $("div[id=oslUploadForm]").css("display", "block");
    $("div[id=oslDetailsForm]").css("display", "none");
    $('button[id=oslUploadbtn]').on('click', function () {
        let progress = 10;
        $('button[id=oslUploadbtn]').prop("disabled", true);
        const myObject = {
            name: $('input[type=text][id=oslName_txt]').val(),
            metersPerPixelY: $("#metersPerPixel option:selected").val(),
            metersPerPixelX: $("#metersPerPixel option:selected").val()
        };
        const jsonString = JSON.stringify(myObject);
        let fileUpload = $("#fupload").get(0);
        let files = fileUpload.files;
        if (files.length > 0) {
            let data = new FormData();
            //for (const element of files) {
            //    data.append(element.name, element);

            //}
            data.append("Store", jsonString);
            data.append('file', files[0]); // Use 'file' as the key for the file

            //data.append("name", $('input[type=text][id=oslName_txt]').val());
            //data.append("metersPerPixel", $("#metersPerPixel option:selected").val());
            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/BackgroundImage/Add',
                type: "POST",
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () {
                    $('button[id=oslUploadbtn]').prop("disabled", true);
                    $('#progresbarrow').css('display', 'block');
                    $('span[id=error_oslUploadbtn]').text("Loading File Please stand by");

                    $('#file_upload_progressbar').css('width', progress + '%');
                },
                xhr: function () {
                    let xhr = $.ajaxSettings.xhr();
                    if (xhr.upload) {
                        xhr.upload.addEventListener("progress", function (evt) {
                            if (evt.lengthComputable) {
                                let percentComplete = evt.loaded / evt.total;
                                percentComplete = parseInt(percentComplete * 100, 10);
                                $('#file_upload_progressbar').attr('aria-valuenow', percentComplete).css('width', percentComplete + '%');
                                if (percentComplete === 100) {
                                    $('span[id=error_oslUploadbtn]').text("File Transfer Complete -->> Processing File ");
                                }
                            }
                        }, false);
                    }
                    return xhr;
                },
                success: function (response) {
                    if (response !== "") {
                        try {
                            $('span[id=error_oslUploadbtn]').text("File Processing Completed");
                            setTimeout(function () { Clear(); }, 500);
                        }
                        catch (e) {
                            $('span[id=error_oslUploadbtn]').text(e);
                        }
                    }
                },
                error: function (response) {
                    $('span[id=error_oslUploadbtn]').text(response.statusText);
                    $('#progresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                },
                failure: function (response) {
                    $('span[id=error_oslUploadbtn]').text(response.statusText);
                    $('#progresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                }
            })

        }
    });
    $('#OSL_Modal').modal('show');
}
function Edit_OSL(data) {
    $('#floormodalHeader_ID').text('Edit Floor Plan ' + data.name);
    $("button[id=oslUploadbtn]").prop("disabled", true);
    $("button[id=oslUploadbtn]").css("display", "none");
    $("button[id=oslSubmitbtn]").css("display", "block");
    $("div[id=oslUploadForm]").css("display", "none");
    $("div[id=oslDetailsForm]").css("display", "block");
    $("input[id=oslName_txt]").val(data.name);
    $("input[id=BckImageId_txt]").prop("disabled", true);
    $("input[id=BckImageId_txt]").val(data.id);
    $("input[id=CoordinateSystemId_txt]").val(data.coordinateSystemId);

    $("input[id=WidthMeter_txt]").val(data.widthMeter);
    $("input[id=HeightMeter_txt]").val(data.heightMeter);
    $("input[id=YMeter_txt]").val(data.yMeter);
    $("input[id=XMeter_txt]").val(data.xMeter);
    $("input[id=OrigoY_txt]").val(data.origoY);
    $("input[id=OrigoX_txt]").val(data.origoX);
    $("input[id=MetersPerPixelY_txt]").val(data.metersPerPixelY);
    $("input[id=MetersPerPixelX_txt]").val(data.metersPerPixelX);
    $('button[id=oslSubmitbtn]').on('click', function () {

        let jsonObject = {};
        jsonObject["id"] = $("input[id=BckImageId_txt]").val();
        jsonObject["name"] = $("input[id=oslName_txt]").val();
        jsonObject["coordinateSystemId"] = $("input[id=CoordinateSystemId_txt]").val();
        jsonObject["widthMeter"] = $("input[id=WidthMeter_txt]").val();
        jsonObject["heightMeter"] = $("input[id=HeightMeter_txt]").val();
        jsonObject["yMeter"] = $("input[id=YMeter_txt]").val();
        jsonObject["xMeter"] = $("input[id=XMeter_txt]").val();
        jsonObject["origoY"] = $("input[id=OrigoY_txt]").val();
        jsonObject["origoX"] = $("input[id=OrigoX_txt]").val();
        jsonObject["metersPerPixelY"] = $("input[id=MetersPerPixelY_txt]").val();
        jsonObject["metersPerPixelX"] = $("input[id=MetersPerPixelX_txt]").val();
        if (!$.isEmptyObject(jsonObject)) {
            //fotfmanager.server.editFloorPlanData(JSON.stringify(jsonObject)).done(function (data) {

            //    $('span[id=error_oslUploadbtn]').text("Background Images has been updated");
            //    setTimeout(function () {
            //        $("#OSL_Modal").modal('hide');
            //        updateOslDatatable(data[0], "osltable");
            //    }, 800);

            //});
        }
    });
    $('#OSL_Modal').modal('show');
}
function createOslDataTable(table) {
    let arrayColums = [{
        "name": "",
        "id": "",
        "Action": ""
    }]
    let columns = [];
    let tempc = {};
    $.each(arrayColums[0], function (key, value) {
        tempc = {};
        if (/name/i.test(key)) {
            tempc = {
                "title": "Name",
                "width": "25%",
                "mDataProp": key,
                "mRender": function (data, type, full) {
                    if (!!full.backgroundImages) {
                        return full.backgroundImages.name
                    }
                    else {
                        return data;
                    }
                }
            }
        }
        else if (/id/i.test(key)) {
            tempc = {
                "title": 'Id',
                "width": "55%",
                "mDataProp": key
            }
        }
        else if (/Action/i.test(key)) {
            tempc = {
                "title": "Action",
                "mDataProp": key,
                "width": "15%",
                "mRender": function (data, type, full) {

                    return '<button type="button" class="btn btn-light btn-sm mx-1 pi-iconEdit ml-p5rem" data-toggle="modal" name="oslEdit" title="Edit OSL"></button>' +
                        '<button type="button" class="btn btn-light btn-sm mx-1 pi-trashFill ml-p5rem" data-toggle="modal" name="oslDelete" title="Delete OSL"</button>'

                }
            }
        }
        else {
            tempc = {
                "title": capitalize_Words(key.replace(/\_/, ' ')),
                "mDataProp": key
            }
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
            zeroRecords: "No Data"
        },
        aoColumns: columns,
        columnDefs: [],
        sorting: [[0, "asc"]],

    })
    $('#' + table + ' tbody').on('click', 'button', function () {
        let td = $(this);
        let table = $(td).closest('table');
        let row = $(table).DataTable().row(td.closest('tr'));
        if (/oslDelete/ig.test(this.name)) {
            Remove_OSL(row.data());
        }
        if (/oslEdit/ig.test(this.name)) {
            Edit_OSL(row.data());
        }
    });
}
function loadOslDatatable(data, table) {
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows.add(data).draw();
    }
}
function updateOslDatatable(newdata, table) {
    let loadnew = true;
    if ($.fn.dataTable.isDataTable("#" + table)) {
        $('#' + table).DataTable().rows(function (idx, data, node) {
            loadnew = false;
            if (data.id === newdata.id) {
                $('#' + table).DataTable().row(node).data(newdata).draw().invalidate();
            }
        })
        if (loadnew) {
            loadOslDatatable(newdata, table);
        }
    }
}
function removeOslDatatable(ldata, table) {
    $('#' + table).DataTable().rows(function (idx, data, node) {
        if (data.id === ldata.id) {
            $('#' + table).DataTable().row(node).remove().draw();
        }
    })
}
function Remove_OSL(data) {
    try {
        //fotfmanager.server.removeFloorPlanData(data.id).done(function (ImageId) {
        //    removeOslDatatable({ id: ImageId }, "osltable");

        //})
        $.ajax({
            url: SiteURLconstructor(window.location) + '/api/BackgroundImage/Delete?id=' + data.id,
            type: 'DELETE',
            success: function (data) {

                setTimeout(function () { $("#API_Connection_Modal").modal('hide'); sidebar.open('setting'); }, 500);
            },
            error: function (error) {

                $('span[id=error_apisubmitBtn]').text(data.name + " " + data.messageType + " Connection was not Updated");
                //console.log(error);
            },
            faulure: function (fail) {
                console.log(fail);
            },
            complete: function (complete) {
                console.log(complete);
            }
        });
    }
    catch (e) {

    }
}