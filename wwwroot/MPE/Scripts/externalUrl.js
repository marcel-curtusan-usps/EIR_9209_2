let MPEExternalURL = "";
$.urlParam = function () {
    let urlsearch = window.location.search;
    let results = urlsearch.split(/=(.*)/s);

    MPEExternalURL = (results !== null) ? results[1] || 0 : "";
    return MPEExternalURL;
}
$(function () {
    $("#slidePreviewIFrame").prop("src", $.urlParam());
});

