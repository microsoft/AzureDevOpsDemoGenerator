$(document).ready(function () {
    $('body').on('click', '#btnUpload', function () {

        $("#fileError").remove();
        disableButton();
        // Checking whether FormData is available in browser
        if (window.FormData !== undefined) {
            var fileUpload = $("#FileUpload1").get(0);
            var files = fileUpload.files;
            if (files.length === 0) {
                $("#btnContainer").append('<span id="fileError" class="msgColor">Please select a zip file.</span>');
                enableButton();
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

            $.ajax({
                url: '/Environment/UploadFiles',
                type: "POST",
                contentType: false, // Not to set any content header
                processData: false, // Not to process data
                data: fileData,
                success: function (result) {
                    if (result === "1") {
                        //alert("succesfully uploaded file: " + files[0].name);
                        console.log("succesfully uploaded file: " + files[0].name);
                        $.post("UnzipFile", { "fineName": files[0].name }, function (respose) {
                            if (respose === "SUCCESS") {
                                //alert("succesfully unzipped file: " + files[0].name);
                                console.log("succesfully unzipped file: " + files[0].name);

                                var NewTemplateName = files[0].name.replace(".zip", "");
                                $('#ddlTemplates').val(NewTemplateName);
                                $(".VSTemplateSelection").removeClass('d-block').addClass('d-none');
                                $("#lblextensionError").removeClass('d-block').addClass('d-none');
                                $("#lblDefaultDescription").removeClass('d-block').addClass('d-none');
                                $("#lblDescription").removeClass('d-block').addClass('d-none');
                                $("#ddlAcccountName").prop('selectedIndex', 0);
                                enableButton();

                            }
                            else if (respose === "PROJECTANDSETTINGNOTFOUND") {
                                $("#btnContainer").append('<span id="fileError" class="msgColor">ProjectSetting and ProjectTemplate files not found! plase include the files in zip and try again</span>');
                                enableButton();
                                return;
                            }
                            else if (respose === "SETTINGNOTFOUND") {
                                $("#btnContainer").append('<span id="fileError" class="msgColor">ProjectSetting file not found! plase include the files in zip and try again</span>');
                                enableButton();
                                return;
                            }
                            else if (respose === "PROJECTFILENOTFOUND") {
                                $("#btnContainer").append('<span id="fileError" class="msgColor">ProjectTemplate file not found! plase include the files in zip and try again</span>');
                                enableButton();
                                return;

                            }
                            else if (respose === "ISPRIVATEERROR") {
                                $("#btnContainer").append('<span id="fileError" class="msgColor">IsPrivate flag is not set to true inProjectTemplate file, update the flag and try again.</span>');
                                enableButton();
                                return;
                            }
                            else {                                
                                $("#btnContainer").append('<span id="fileError" class="msgColor">' + respose + "\r\n" + '</span>');
                                enableButton();
                                return;
                            }
                        });
                    }
                },
                error: function (err) {
                    alert(err.statusText);
                }
            });
        } else {
            alert("FormData is not supported.");
        }
    });
});

function disableButton() {
    $('#btnUpload').attr('disabled', 'disabled').removeClass('btn-primary');
}
function enableButton() {
    $('#btnUpload').attr('disabled', false).addClass('btn-primary');
}