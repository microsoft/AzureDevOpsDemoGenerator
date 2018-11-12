$(document).ready(function () {
    $('body').on('click', '#btnUpload', function () {

        $("#fileError").remove();

        // Checking whether FormData is available in browser
        if (window.FormData !== undefined) {

            var fileUpload = $("#FileUpload1").get(0);
            var files = fileUpload.files;

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
                    if (result == "1") {
                        alert("succesfully uploaded file: " + files[0].name);

                        $.post("UnzipFile", { "fineName": files[0].name }, function (respose) {

                            if (respose == "SUCCESS") {

                                alert("succesfully unzipped file: " + files[0].name);
                                var NewTemplateName = files[0].name.replace(".zip", "");
                                $('#ddlTemplates').val(NewTemplateName);
                                $(".VSTemplateSelection").removeClass('d-block').addClass('d-none');
                                $("#lblextensionError").removeClass('d-block').addClass('d-none');
                                $("#lblDefaultDescription").removeClass('d-block').addClass('d-none');
                                $("#lblDescription").removeClass('d-block').addClass('d-none');
                                $("#ddlAcccountName").prop('selectedIndex', 0);
                            } else {
                                if (respose == "PROJECTANDSETTINGNOTFOUND") {

                                    $("#btnContainer").append('<span id="fileError" class="bg-warning">ProjectSetting and ProjectTemplate files not found! plase include the files in zip and try again</span>')
                                }
                                if (respose == "SETTINGNOTFOUND") {
                                    $("#btnContainer").append('<span id="fileError" class="bg-warning">ProjectSetting file not found! plase include the files in zip and try again</span>')
                                }
                                if (respose == "PROJECTFILENOTFOUND") {
                                    $("#btnContainer").append('<span id="fileError" class="bg-warning">ProjectTemplate file not found! plase include the files in zip and try again</span>')
                                }
                                if (respose == "ISPRIVATEERROR") {
                                    $("#btnContainer").append('<span id="fileError" class="bg-warning">IsPrivate flag is not set to true inProjectTemplate file, update the flag and try again.</span>')
                                }
                            }
                        })
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
