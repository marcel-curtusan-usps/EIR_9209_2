let MPEExternalURL = "";
$.urlParam = function (name) {
    let results = new RegExp('[\?&]' + name + '=([^&#]*)', 'i').exec(window.location.search);
    MPEExternalURL = (results !== null) ? results[1] || 0 : "";
    return MPEExternalURL;
}
$(function () {
    $("#slidePreviewIFrame").prop("src", $.urlParam("MPEExternalUrl"));
});

