$(function () {

    $("#webEORfupload").on("keyup", function (e) {
        if (this.value.length === 0) {
            $("button[id=WebEORUploadbtn]").prop("disabled", true);
        }
        else {
            $("button[id=WebEORUploadbtn]").prop("disabled", false);
        }

    });
    $('button[name=addendofrun]').off().on('click', function () {
        Promise.all([Add_webEORData()]);
    });

});
//on open set rules
$('#webEOR_Modal').on('shown.bs.modal', function () {
    $("#webEORfupload").on("change", function (e) {
        if (document.getElementById("webEORfupload").files.length > 0) {
            fileName = document.getElementById("webEORfupload").files[0].name;
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = fileName
        }
        else {
            fileName = "";
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = 'Choose file'
        }

        $('span[id=error_WebEORUploadbtn]').text("");

    });
});
$('#webEOR_Modal').on('hidden.bs.modal', function () {
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
    $('#webEORfile_upload_progressbar').css('width', progress + '%');
    $('input[id=fupload][type=file]').val("");
    document.getElementById("webEORfupload").nextElementSibling.innerText = 'Choose file';
    $('span[id=error_WebEORUploadbtn]').text("");
    $('button[id=WebEORUploadbtn]').prop("disabled", false);
    sidebar.open('setting');
});
function Add_webEORData() {
    $('#webEORmodalHeader_ID').text('Upload WebEOR End of Run');
    $("div[id=webEORUploadForm]").css("display", "block");
    $('button[id=WebEORUploadbtn]').on('click', function () {
        let progress = 10;
        $('button[id=WebEORUploadbtn]').prop("disabled", true);
        let fileUpload = $("#webEORfupload").get(0);
        let files = fileUpload.files;
        if (files.length > 0) {
            let data = new FormData();
            for (const element of files) {
                data.append("file", element);

            }

            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/MPERunActivity/UploadWebEOR',
                type: 'POST',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () {
                    $('button[id=WebEORUploadbtn]').prop("disabled", true);
                    $('#progresbarrow').css('display', 'block');
                    $('span[id=error_WebEORUploadbtn]').text("Loading File Please stand by");

                    $('#webEORfile_upload_progressbar').css('width', progress + '%');
                },
                xhr: function () {
                    let xhr = $.ajaxSettings.xhr();
                    if (xhr.upload) {
                        xhr.upload.addEventListener("progress", function (evt) {
                            if (evt.lengthComputable) {
                                let percentComplete = evt.loaded / evt.total;
                                percentComplete = parseInt(percentComplete * 100, 10);
                                $('#webEORfile_upload_progressbar').attr('aria-valuenow', percentComplete).css('width', percentComplete + '%');
                                if (percentComplete === 100) {
                                    $('span[id=error_WebEORUploadbtn]').text("File Transfer Complete -->> Processing File ");
                                }
                            }
                        }, false);
                    }
                    return xhr;
                },
                success: function (response) {
                    if (response !== "") {
                        try {
                            $('span[id=error_WebEORUploadbtn]').text("File Processing Completed");
                            setTimeout(function () { $("#API_Connection_Modal").modal('hide'); Clear(); }, 500);
                        }
                        catch (e) {
                            $('span[id=error_WebEORUploadbtn]').text(e);
                        }
                    }
                },
                error: function (response) {
                    $('span[id=error_WebEORUploadbtn]').text(response.statusText);
                    $('button[id=WebEORUploadbtn]').prop("disabled", false);
                    $('#webEORprogresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                },
                failure: function (response) {
                    $('span[id=error_WebEORUploadbtn]').text(response.statusText);
                    $('#webEORprogresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                }
            })

        }
    });
    $('#webEOR_Modal').modal('show');
}
function Clear() {
    let progress = 0;
    $('#progresbarrow').css('display', 'none');
    $('#file_upload_progressbar').css('width', progress + '%');
    $("#metersPerPixel").val("");
    $('input[type=file]').val('');
    $('input[type=radio]').prop("checked", "");
    $('span[id=error_btnUpload]').text("");
    $('button[id=btnUpload]').prop("disabled", false);
}