var mywindown;
$(document).ready(function () {
    $('#githubAuth').click(function () {
        var reqorigon = window.location.origin;
        mywindown = window.open(reqorigon + "/GitHub/GitOauth", "Azure DevOps Demo Generator", "width=500,height=500",
            "width=300,height=400,scrollbars=yes");
        checkSession();
        ga('send', 'event', 'GitHubAuthorize', 'clicked');
    });
    $('input[id="gitHubCheckbox"]').click(function () {
        if ($(this).prop("checked") === true) {
            $('#btnSubmit').prop('disabled', true).removeClass('btn-primary');
            //$('#gitHubAuthDiv').removeClass('d-none');
            $('#txtRepoNamediv').removeClass('d-none');
            $('#githubAuth').addClass('btn-primary').prop('disabled', false);
        }
        if ($(this).prop("checked") === false) {
            $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
            //$('#gitHubAuthDiv').addClass('d-none');
            $('#txtRepoNamediv').addClass('d-none');
            $('#githubAuth').removeClass('btn-primary').prop('disabled', true);
        }
    });

    //setTimeout(function () {
    //    $('#buildYourTemplate').removeClass('icon2');
    //}, 10000);

    $('#buildYourTemplate').hover(function () {
        $('#buildYourTemplate').removeClass('icon2');
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
                localStorage.setItem("gToken", res);
                $('input[id="gitHubCheckbox"]').prop('checked', true).prop('disabled', true);
                $('#githubAuth').removeClass('btn-primary').prop('disabled', true);
                $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
                mywindown.close();
                $('#githubAuth').css('border-color', 'initial');
                GetOrganizations();
            }
            else {
                window.setTimeout("checkSession()", 500);
            }
        },
        error: function (er) {
        }
    });
}
//function checkTokenInSession() {
//    $.ajax({
//        url: '../Environment/CheckSession',
//        type: "GET",
//        async: false,
//        cache: false,
//        success: function (res) {
//            if (res !== "") {
//                $('input[id="gitHubCheckbox"]').prop('checked', true).prop('disabled', true);
//                $('#githubAuth').removeClass('btn-primary').prop('disabled', true);
//                $('#btnSubmit').prop('disabled', false).addClass('btn-primary');
//                $('#githubAuth').css('border-color', 'initial');
//            }
//            else {
//                $('#btnSubmit').prop('disabled', true).removeClass('btn-primary');
//            }
//        },
//        error: function (er) {
//        }
//    });
//}

function GetOrganizations() {
    var gToken = localStorage.getItem("gToken");
    if (gToken != "") {
        $.ajax({
            url: '../Environment/GetOrganizaton',
            type: 'GET',
            success: function (data) {
                debugger;
                var orgs = "";
                if (data.length > 0) {
                    for (var o in data) {
                        orgs += "<option value=" + data[o].login + ">" + data[o].login + "</option>"
                    }
                    $(orgs).appendTo('#ghOrgs');
                }
            },
            error: function (er) {
                console.log(er);
            }
        });
    }
}