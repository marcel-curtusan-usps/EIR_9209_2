$(function () {

    $("#targetfupload").on("keyup", function (e) {
        if (this.value.length === 0) {
            $("button[id=targetUploadbtn]").prop("disabled", true);
        }
        else {
            $("button[id=targetUploadbtn]").prop("disabled", false);
        }

    });
    $('button[name=addTarget]').off().on('click', function () {
        Promise.all([Add_TargetData()]);
    });

});
//on open set rules
$('#target_Modal').on('shown.bs.modal', function () {
    $("#targetfupload").on("change", function (e) {
        if (document.getElementById("targetfupload").files.length > 0) {
            fileName = document.getElementById("targetfupload").files[0].name;
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = fileName
        }
        else {
            fileName = "";
            nextSibling = e.target.nextElementSibling
            nextSibling.innerText = 'Choose file'
        }

        $('span[id=error_targetUploadbtn]').text("");

    });
});
$('#target_Modal').on('hidden.bs.modal', function () {
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
    $('#targetfile_upload_progressbar').css('width', progress + '%');
    $('input[id=fupload][type=file]').val("");
    document.getElementById("targetfupload").nextElementSibling.innerText = 'Choose file';
    $('span[id=error_targetUploadbtn]').text("");
    $('button[id=targetUploadbtn]').prop("disabled", false);
    sidebar.open('setting');
});
function Add_TargetData() {
    $('#targetmodalHeader_ID').text('Upload Target');
    $("div[id=targetUploadForm]").css("display", "block");
    $('button[id=targetUploadbtn]').on('click', function () {
        let progress = 10;
        $('button[id=targetUploadbtn]').prop("disabled", true);
        let fileUpload = $("#targetfupload").get(0);
        let files = fileUpload.files;
        if (files.length > 0) {
            let data = new FormData();
            for (const element of files) {
                data.append("file", element);

            }

            $.ajax({
                url: SiteURLconstructor(window.location) + '/api/MPETragets/UploadTarget',
                type: 'POST',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () {
                    $('button[id=targetUploadbtn]').prop("disabled", true);
                    $('#progresbarrow').css('display', 'block');
                    $('span[id=error_targetUploadbtn]').text("Loading File Please stand by");

                    $('#targetfile_upload_progressbar').css('width', progress + '%');
                },
                xhr: function () {
                    let xhr = $.ajaxSettings.xhr();
                    if (xhr.upload) {
                        xhr.upload.addEventListener("progress", function (evt) {
                            if (evt.lengthComputable) {
                                let percentComplete = evt.loaded / evt.total;
                                percentComplete = parseInt(percentComplete * 100, 10);
                                $('#targetfile_upload_progressbar').attr('aria-valuenow', percentComplete).css('width', percentComplete + '%');
                                if (percentComplete === 100) {
                                    $('span[id=error_targetUploadbtn]').text("File Transfer Complete -->> Processing File ");
                                }
                            }
                        }, false);
                    }
                    return xhr;
                },
                success: function (response) {
                    if (response !== "") {
                        try {
                            $('span[id=error_targetUploadbtn]').text("File Processing Completed");
                            setTimeout(function () { $("#API_Connection_Modal").modal('hide'); Clear(); }, 500);
                        }
                        catch (e) {
                            $('span[id=error_targetUploadbtn]').text(e);
                        }
                    }
                },
                error: function (response) {
                    $('span[id=error_targetUploadbtn]').text(response.statusText);
                    $('button[id=targetUploadbtn]').prop("disabled", false);
                    $('#targetprogresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                },
                failure: function (response) {
                    $('span[id=error_targetUploadbtn]').text(response.statusText);
                    $('#targetprogresbarrow').css('display', 'none');
                    setTimeout(function () { Clear(); }, 10000);
                }
            })

        }
    });
    $('#target_Modal').modal('show');
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