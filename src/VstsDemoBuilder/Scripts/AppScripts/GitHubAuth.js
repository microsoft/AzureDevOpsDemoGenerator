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