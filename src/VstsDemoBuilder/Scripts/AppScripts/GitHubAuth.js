$(document).ready(function () {
    $('#githubAuth').click(function () {
        var reqorigon = window.location.origin;
        window.open(reqorigon + "/GitHub/GitOauth", "Azure DevOps Demo Generator", "width=500,height=500",
            "width=300,height=400,scrollbars=yes");
    });
    $('input[id="gitHubCheckbox"]').click(function () {
        if ($(this).prop("checked") === true) {
            $('#gitHubAuthDiv').removeClass('d-none');
        }
        if ($(this).prop("checked") === false) {
            $('#gitHubAuthDiv').addClass('d-none');
        }
    });
});

function checkSession() {
    $.ajax({
        url: '../Environment/CheckSession',
        type: "GET",
        success: function (res) {
            console.log(res);
            if (res !== "") {
                alert(res);
                $('#ghkey').val(res);
            }
            else {
                alert("Please authenticate github to fork repo");
            }
        },
        error: function (er) {
        }

    });
}