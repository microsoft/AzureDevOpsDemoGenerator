$(document).ready(function () {
    $('body').on('click', '#btnUpload', function () {
        $("#fileError").remove();
        var controlID = this.id;
        disableButton(controlID);
        // Checking whether FormData is available in browser
        if (window.FormData !== undefined) {
            $("#urlerror").empty();
            var fileUpload = $("#FileUpload1").get(0);
            var files = fileUpload.files;
            if (files.length === 0) {
                $("#btnContainer").append('<span id="fileError" class="msgColor">Please select a zip file.</span>');
                enableButton(controlID);
                return;
            }
            else {
                $("#fileError").html('');
            }
            // Create FormData object
            var fileData = new FormData();

            // Looping over all files and add it to FormData object
            for (var i = 0; i < files.length; i++) {
                fileData.append(files[i].name, files[i]);
            }

            //$('#gitHubCheckboxDiv').addClass('d-none');
            //$('input[id="gitHubCheckbox"]').prop('checked', false);

            $.ajax({
                url: '/Environment/UploadFiles',
                type: "POST",
                contentType: false, // Not to set any content header
                processData: false, // Not to process data
                data: fileData,
                success: function (result) {

                    if (result[0] !== "") {
                        console.log("succesfully uploaded file: " + result[0]);
                        $.post("UnzipFile", { "fineName": result[0] }, function (Data) {
                            if (Data.responseMessage === "SUCCESS") {
                                console.log("succesfully unzipped file: " + files[0].name);

                                var NewTemplateName = files[0].name.replace(".zip", "");

                                $('#ddlTemplates', parent.document).val(NewTemplateName);
                                $('#selectedTemplateFolder', parent.document).val(Data.privateTemplateName);
                                $(".template-close", parent.document).click();
                                $(".VSTemplateSelection", parent.document).removeClass('d-block').addClass('d-none');
                                $("#lblextensionError", parent.document).removeClass('d-block').addClass('d-none');
                                $("#lblDefaultDescription", parent.document).removeClass('d-block').addClass('d-none');
                                $("#lblDescription", parent.document).removeClass('d-block').addClass('d-none');
                                $("#ddlAcccountName", parent.document).prop('selectedIndex', 0);
                                //$('#gitHubCheckboxDiv', parent.document).addClass('d-none');

                                $('#PrivateTemplateName', parent.document).val(Data.privateTemplateName);
                                $('#PrivateTemplatePath', parent.document).val(Data.privateTemplatePath);
                                enableButton(controlID);
                                $('#FileUpload1').val('');
                            }
                            else if (Data.responseMessage !== null && Data.responseMessage !== "") {

                                $("#urlerror").empty().append(Data.responseMessage);
                                enableButton(controlID);
                                $('#FileUpload1').val('');
                                return;
                            }
                        });
                    }
                },
                error: function (err) {
                }
            });
        } else {
            alert("FormData is not supported.");
        }
    });

    $('body').on('click', '#btnURLUpload, #btnGitHubUpload', function () {
        var isUrlValid = false;
        var URL = '';
        if ($('#GitHubUrl').val() !== '') {
            URL = $('#GitHubUrl').val().trim();
        } else if ($('#FileURL').val() !== '') {
            URL = $('#FileURL').val().trim();
        }
        if (URL === '') {
            $("#urlerror").empty().append('URL should not be empty');
            return false;
        }
        var controlID = this.id;

        var GitHubtoken = $('#GitHubToken').val().trim();
        var userId = $('#UserId').val().trim();
        var password = $('#Password').val().trim();
        $("#urlerror").empty();
        var fileurlSplit = URL.split('/');
        var filename = fileurlSplit[fileurlSplit.length - 1];

        filename = filename.split('.');
        if (filename.length > 1) {
            if (filename[filename.length - 1].toLowerCase().trim() !== "zip") {
                $("#urlerror").empty().append('Invalid URL, please provide the URL which ends with .zip extension'); isUrlValid = false;
            } else {
                isUrlValid = true;
            }
        }
        else {
            $("#urlerror").empty().append('Invalid URL, please provide the URL which ends with .zip extension'); isUrlValid = false;
        }
        if (controlID === 'btnGitHubUpload') {

            if (fileurlSplit[2].toLowerCase() !== "raw.githubusercontent.com" && fileurlSplit[2].toLowerCase() !== "github.com") {
                $("#urlerror").empty().append('Please provide GitHub URL, which should starts with domain name raw.githubusercontent.com or github.com '); isUrlValid = false;
            }
            else if ($('#privateGitHubRepo').prop("checked") === true && GitHubtoken === '') {
                $("#urlerror").empty().append('Please provide GitHub access token for authentication'); isUrlValid = false;
            }
        }
        else if (controlID === 'btnURLUpload' && $('#privateurl').prop("checked") === true && (userId === '' || password === '')) {
            $("#urlerror").empty().append('Please provide username and password for authentication'); isUrlValid = false;
        }

        if (isUrlValid) {
            var OldprivateTemplate = "";
            var oldTemplate = $('#PrivateTemplatePath', parent.document).val().split("\\");
            if (oldTemplate.length > 0) {
                OldprivateTemplate = oldTemplate[oldTemplate.indexOf('PrivateTemplates') + 1];
            }
            disableButton(controlID);
            $.ajax({
                url: "../Environment/UploadPrivateTemplateFromURL",
                type: "GET",
                data: { TemplateURL: URL, token: GitHubtoken, userId: userId, password: password, OldPrivateTemplate: OldprivateTemplate },
                success: function (Data) {
                    if (Data.privateTemplatePath !== "" && Data.privateTemplatePath !== undefined) {
                        console.log(Data);
                        var msg = '';
                        if (Data.responseMessage === "SUCCESS") {
                            $('#PrivateTemplateName', parent.document).val(Data.privateTemplateName);
                            $('#PrivateTemplatePath', parent.document).val(Data.privateTemplatePath);
                            var NewTemplateName = filename[0];
                            $('#ddlTemplates', parent.document).val(NewTemplateName);
                            $('#selectedTemplateFolder', parent.document).val(NewTemplateName);
                            $(".template-close", parent.document).click();
                            $(".VSTemplateSelection", parent.document).removeClass('d-block').addClass('d-none');
                            $("#lblextensionError", parent.document).removeClass('d-block').addClass('d-none');
                            $("#lblDefaultDescription", parent.document).removeClass('d-block').addClass('d-none');
                            $("#lblDescription", parent.document).removeClass('d-block').addClass('d-none');
                            $("#ddlAcccountName", parent.document).prop('selectedIndex', 0);
                            enableButton(controlID);
                            //$('#gitHubCheckboxDiv', parent.document).addClass('d-none');
                        }
                        else if (Data.responseMessage !== '' && Data.responseMessage !== 'SUCCESS') {
                            $("#urlerror").empty().append(Data.responseMessage);
                            enableButton(controlID);
                            return;
                        }
                    }
                    else {
                        if (Data.responseMessage !== null && Data.responseMessage !== 'SUCCESS') {
                            $("#urlerror").empty().append(Data.responseMessage);
                            enableButton(controlID);
                            return;
                        }
                    }

                }

            });
        }

    });
});
function disableButton(button) {
    $('#' + button).attr('disabled', 'disabled').removeClass('btn-primary');
}
function enableButton(button) {
    $('#' + button).attr('disabled', false).addClass('btn-primary');
}
