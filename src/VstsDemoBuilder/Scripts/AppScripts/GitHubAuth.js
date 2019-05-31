var mywindown;
$(document).ready(function () {
    $('#githubAuth').click(function () {
        var reqorigon = window.location.origin;
        mywindown = window.open(reqorigon + "/GitHub/GitOauth", "Azure DevOps Demo Generator", "width=500,height=500",
            "width=300,height=400,scrollbars=yes");
        checkSession();
    });
    $('input[id="gitHubCheckbox"]').click(function () {
        if ($(this).prop("checked") === true) {
            $('#btnSubmit').prop('disabled', true).removeClass('btn-primary');
            $('#gitHubAuthDiv').removeClass('d-none');
        }
        if ($(this).prop("checked") === false) {
            $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
            $('#gitHubAuthDiv').addClass('d-none');
        }
    });
});

function checkSession() {
    $.ajax({
        url: '../Environment/CheckSession',
        type: "GET",
        async: false,
        cache: false,
        success: function (res) {
            if (res !== "") {
                $('#hdnGToken').val(res);
                $('input[id="gitHubCheckbox"]').prop('checked', true).prop('disabled', true);
                $('#githubAuth').removeClass('btn-primary').prop('disabled', true);
                $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
                mywindown.close();
            }
            else {
                window.setTimeout("checkSession()", 500);
                //$('input[id="gitHubCheckbox"]').prop('checked', false).prop('disabled', false);
                //$('#gitHubAuthDiv').addClass('d-none');
                //$('#githubAuth').addClass('btn-primary').prop('disabled', false);
            }
        },
        error: function (er) {
        }
    });
}
function checkTokenInSession() {
    $.ajax({
        url: '../Environment/CheckSession',
        type: "GET",
        async: false,
        cache: false,
        success: function (res) {
            if (res !== "") {
                $('input[id="gitHubCheckbox"]').prop('checked', true).prop('disabled', true);
                $('#githubAuth').removeClass('btn-primary').prop('disabled', true);
                $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
            }
            else {
                $('#btnSubmit').prop('disabled', true).removeClass('btn-primary');
            }
        },
        error: function (er) {
        }
    });
}