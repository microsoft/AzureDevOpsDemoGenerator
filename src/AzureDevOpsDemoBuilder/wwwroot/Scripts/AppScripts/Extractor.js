
var ID = function () {
    return '_' + Math.random().toString(36).substr(2, 9);
};
var uniqueId = "";
var messagesCount = 2;
var percentForMessage = Math.floor(100 / messagesCount);
var ErrorData = '';
var statusCount = 0;
var messageList = [];
var currentPercentage = 0;
var finalprojectName = "";

$(document).ready(function () {
    $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');

    window.onbeforeunload = WindowCloseHanlder;
    function WindowCloseHanlder() {
        var fso = new ActiveXObject('Scripting.FileSystemObject');
        return fso.DeleteFile("../ExtractedTemplate/" + finalprojectName + ".zip", true);
    }

    uniqueId = ID();
    $('.rmverror').click(function () {
        var errID = this.nextElementSibling.getAttribute('id');
        $('#' + errID).removeClass("d-block").addClass("d-none");
    });
    $(window).scroll(function () {
        var scroll = $(window).scrollTop();
        if (scroll > 50) {
            $(".navbar").css("background-color", "#2279C3");
        }
        else {
            $(".navbar").css("background-color", "transparent");
        }
    });

    $('#info').hover(function () {
        $('#infoData').removeClass('d-none');
    });
    $('#info').mouseout(function () {
        $('#infoData').addClass('d-none');
    });

    $('#close').click(function () {
        $("#msgSource").hide();
        return;
    });

    $('#ddlAcccountName').change(function () {
        $('#analyseDiv').addClass('d-none'); $('#analytics').html("");

        $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
        $("#errorNotify").hide();
        var accSelected = $('#ddlAcccountName').val();
        var projectName = $("#projectSelect option:selected").text();
        $('#projectSelect').empty();
        var options = $("#projectSelect option");
        options.appendTo("#projectSelect", "Select Project");
        options.appendTo("#projectSelect");
        if (accSelected === '' || accSelected === 'Select Organization') {
            $("#ddlAcccountName_Error").text("Please select an organization");
            $("#ddlAcccountName_Error").removeClass('d-none');
            return;
        }
        else {
            $('#projecctDiv').addClass('lodergif_div');
            //$('#projectloader').removeClass('d-none');
            $('#analyseDiv').addClass('d-none'); $('#analytics').html("");
            $('#genArtDiv').addClass('d-none'); $('#artifactProgress').html("");
            $('#GenerateArtifact').addClass('d-none');
            $('#finalLink').addClass('d-none');
            $('#Analyse').attr('disabled', 'disabled');
            $('#Analyse').removeClass('btn-primary');

            var token = $('#key').val();
            $.ajax({
                url: '../Extractor/GetprojectList',
                type: 'POST',
                data: { accname: accSelected, pat: token },
                success: function (da) {
                    if (da.count > 0) {
                        $('#Analyse').addClass('btn-primary').attr('disabled', false);
                        $('#projectSelect').empty();
                        var opt = "";
                        opt += ' <option value="0" selected="selected">Select Project</option>';
                        for (var i = 0; i < da.count; i++) {
                            opt += ' <option value="' + da.value[i].id + '">' + da.value[i].name + '</option>';
                        }
                        $("#projectSelect").append(opt);
                        var options = $("#projectSelect option");
                        $('#projecctDiv').removeClass('lodergif_div');
                        //$('#projectloader').addClass('d-none');
                    }
                    else {
                        $('#projecctDiv').removeClass('lodergif_div');
                        //$('#projectloader').addClass('d-none');
                        $("#projectSelect_Error").text(da.errmsg);
                        $("#projectSelect_Error").removeClass('d-none');
                        return;
                    }
                },
                error: function () {
                    $('#projecctDiv').removeClass('lodergif_div');
                    //$('#projectloader').addClass('d-none');
                    $('#Analyse').attr('disabled', false);
                    $('#Analyse').addClass('btn-primary');
                }
            });
        }
    });

    $('#projectSelect').change(function () {
        $('#projecctDiv').addClass('lodergif_div');
        //$('#projectloader').removeClass('d-none');
        $('#analyseDiv').addClass('d-none'); $('#analytics').html("");
        $('#genArtDiv').addClass('d-none'); $('#artifactProgress').html("");
        $('#GenerateArtifact').addClass('d-none');
        $("#errorNotify").hide();
        $("#msgSource").hide();
        var project = $('#projectSelect option:selected').val();
        var accSelected = $('#ddlAcccountName').val();
        var key = $('#key').val();
        $('#processtemplate').empty();
        $('#processTemplateLoader').removeClass('d-none');
        if (project === "0" || project === "" || project === 'Select Project') {
            $('#Analyse').attr('disabled', 'disabled');
            $('#Analyse').removeClass('btn-primary');
            $('#projecctDiv').removeClass('lodergif_div');
            //$('#projectloader').addClass('d-none');
            return;
        }
        if (accSelected === '' || accSelected === 'Select Organization') {
            $('#Analyse').attr('disabled', 'disabled');
            $('#Analyse').removeClass('btn-primary');
            $('#projecctDiv').removeClass('lodergif_div');
            //$('#projectloader').addClass('d-none');
            return;
        }
        else {
            $('#Analyse').attr('disabled', 'disabled');
            $('#Analyse').removeClass('btn-primary');

            $.ajax({
                url: '../Extractor/GetProjectProperties',
                type: 'GET',
                data: { accname: accSelected, project: project, _credentials: key },
                success: function (res) {
                    $('#projecctDiv').removeClass('lodergif_div');
                    //$('#projectloader').addClass('d-none');
                    $('#processtemplate').empty().val(res.value[4].refValue);
                    $('#TemplateClass').empty().val(res.typeClass);
                    $('#processTemplateLoader').addClass('d-none');
                    var p = res.value[4].value;
                    //if (p !== "Scrum" && p !== "Agile" && p !== "Basic") {
                    //    $('#processTemplateLoader').addClass('d-none');
                    //    $("#projectSelect_Error").text("Note: Please select a project that uses the standard Scrum, Agile or Basic process template.");
                    //    $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
                    //    $("#projectSelect_Error").removeClass('d-none');
                    //    return;
                    //}
                    $('#Analyse').addClass('btn-primary').attr('disabled', false);
                },
                error: function (e) {
                    $('#processTemplateLoader').addClass('d-none');
                    $("#projectSelect_Error").text(e);
                    $("#projectSelect_Error").removeClass('d-none');
                    $('#projecctDiv').removeClass('lodergif_div');
                    //$('#projectloader').addClass('d-none');
                    $('#Analyse').addClass('btn-primary').attr('disabled', false);
                }
            });
        }
    });

    $('#Analyse').click(function () {
        ga('send', 'event', 'Analyze Button', 'clicked');
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        finalprojectName = projectName;
        var key = $('#key').val();
        if (SourceAcc === "" || SourceAcc === "Select Organization") {
            $("#ddlAcccountName_Error").text("Please select an organization");
            $("#ddlAcccountName_Error").removeClass('d-none');
            return;
        }
        if (project === "" || projectName === 'Select Project' || project === "0" || typeof project === undefined) {
            $("#projectSelect_Error").text("Please select a Project");
            $("#projectSelect_Error").removeClass('d-none');
            return;
        }

        $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
        //$('#imgLoading').removeClass('d-none');
        $('#Analyse').addClass('lodergif');

        $('#genArtDiv').addClass('d-none');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#ExStatus-messages').empty();
        //$('#accountLink').empty();
        $('#finalLink').addClass('d-none');
        $("#ddlAcccountName").prop('disabled', true);
        $("#projectSelect").prop('disabled', true);
        $.ajax({
            url: '../Extractor/AnalyzeProject',
            type: 'POST',
            data: { ProjectName: projectName, accessToken: key, accountName: SourceAcc },
            success: function (res) {
                console.log(res);
                var row = "";
                if (res !== "") {
                    var processTemplate = $('#processtemplate').val();
                    var templaetClass = $('#TemplateClass').val();
                    if (templaetClass !== "system") {
                        //$('#GenerateArtifacts').hide();
                        //$('#templateError').empty().append("Tool doesn't support for projects based on custom or derived process templates");
                    }
                    else {
                        $('#templateError').empty().append("Everything looks good.Click the button below to proceed.");
                        $('#GenerateArtifacts').show();
                    }
                    row += ' Selected Project: ' + projectName + ', &nbsp; Process Template: ' + processTemplate + ' (' + templaetClass + ') <br />';

                    if (res.teamCount !== 0 && res.teamCount !== null)
                        row += '<i class="fas fa-check-circle"></i>' + ' Teams: ' + res.teamCount + '<br />';

                    if (res.iterationCount !== 0 && res.iterationCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Iteration: ' + res.iterationCount + '<br />';

                    if (res.buildDefCount !== 0 && res.buildDefCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Build Definitions: ' + res.buildDefCount + '<br />';

                    if (res.releaseDefCount !== 0 && res.releaseDefCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Release Definitions: ' + res.releaseDefCount + '<br />';

                    $.each(res.workItemCounts, function (x, y) {
                        row += '<i class="fas fa-check-circle" ></i >' + x + ': ' + y + ' <br /> ';
                    });
                    var er = "";
                    $.each(res.errorMessages, function (x, y) {
                        er += '<p>' + y + '</p >';
                    });
                    $('#templateError').empty().append(er);
                }
                $('#analyseDiv').removeClass('d-none');
                $('#collapseOne').addClass('show');
                $('#analytics').empty().append(row);
                //$('#imgLoading').addClass('d-none');
                $('#Analyse').removeClass('lodergif');
                $('#Analyse').addClass('btn-primary').attr('disabled', false);

                $('#genArtDiv').removeClass('d-none');
                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                $("#ddlAcccountName").prop('disabled', false);
                $("#projectSelect").prop('disabled', false);

            },
            error: function (er) {
                $('#Analyse').addClass('btn-primary').attr('disabled', false);
                //$('#imgLoading').addClass('d-none');
                $('#Analyse').removeClass('lodergif');
                $("#ddlAcccountName").prop('disabled', false);
                $("#projectSelect").prop('disabled', false);
            }
        });
        $('#generatebtnloader').removeClass('d-none');

    });

    $('#GenerateArtifacts').click(function () {
        ga('send', 'event', 'Generate Artifact', 'clicked');
        $('.accorDetails').removeClass('show');
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        $('#hdnProjecName').val(projectName);
        var key = $('#key').val();
        var processTemplate = $('#processtemplate').val();
        if (SourceAcc === "" || SourceAcc === "Select Organization") {
            $("#ddlAcccountName_Error").text("Please select Source Account Name");
            $("#ddlAcccountName_Error").removeClass('d-none');
            return;
        }
        if (project === "" || projectName === 'Select Project' || project === "0") {
            $("#projectSelect_Error").text("Please select Source Project");
            $("#projectSelect_Error").removeClass('d-none');
            return;
        }
        var projects = {
            projectName: projectName, SourceAcc: SourceAcc, key: key, uniqueId: uniqueId, processTemplate: processTemplate, project: project
        };
        $('#ExStatus-messages').html('');
        $('#ExStatus-messages').show();
        $('#GenerateArtifact').removeClass('d-none');
        //$('#ExdvProgress').removeClass('d-none');
        $('#GenerateArtifacts').addClass('lodergif');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#collapseTwo').addClass('show');
        $("#ddlAcccountName").prop('disabled', true);
        $("#projectSelect").prop('disabled', true);
        $.ajax({
            url: '../Extractor/StartEnvironmentSetupProcess',
            type: 'POST',
            data: { projectName: projectName, SourceAcc: SourceAcc, key: key, uniqueId: uniqueId, processTemplate: processTemplate, project: project },
            success: function (res) {
                if (res === true) {
                    getStatus();
                }
            },
            error: function (er) {
                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                $('#Analyse').addClass('btn-primary').attr('disabled', false);
                $("#ddlAcccountName").prop('disabled', false);
                $("#projectSelect").prop('disabled', false);
            }
        });
    });
    $('#fileDownload').click(function () {
        ga('send', 'event', 'fileDownload', 'clicked');
    });

});

function getStatus() {
    $.ajax({
        url: 'GetCurrentProgress/' + uniqueId,
        type: 'GET',
        success: function (data) {
            console.log(data);
            var isMessageShown = true;
            if (jQuery.inArray(data, messageList) === -1) {
                messageList.push(data);
                isMessageShown = false;
            }
            if (data !== "100") {
                if (isMessageShown === false) {
                    if (messageList.length === 1) {
                        $('#ExtractorProgressBar').width(currentPercentage++ + '%');
                        if (data.length > 0) {
                            $('#ExStatus-messages').append('<i class="fas fa-check-circle"></i> &nbsp;' + data + '<br/>');
                        }
                    }
                    else {
                        $('#ExStatus-messages').append('<i class="fas fa-check-circle"></i> &nbsp;' + data + '<br/>');
                    }
                }
                else if (currentPercentage <= ((messageList.length + 1) * percentForMessage) && currentPercentage <= 100) {
                    $('#ExtractorProgressBar').width(currentPercentage++ + '%');
                }
                window.setTimeout("getStatus()", 1000);
            }
            else {
                if (messageList.length !== 3) {
                    var ID = uniqueId + "_Errors";
                    var url2 = 'GetCurrentProgress/' + ID;
                    $.get(url2, function (response) {
                        console.log(response);
                        if (response === "100" || response === "") {
                            $('#artifactProgress').removeClass('d-none');
                            $('#GenerateArtifacts').removeClass('lodergif');
                            //$('#ExdvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            currentPercentage = 0;
                            $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                            $('#Analyse').addClass('btn-primary').attr('disabled', false);
                            $('.genArtifacts').removeClass('show');

                            $('#ExtractorProgressBar').width(currentPercentage++ + '%');
                            $("#finalLink").removeClass("d-none").addClass("d-block");
                            $("#btnSubmit").prop("disabled", false);
                            $("#txtProjectName").val("");

                            $('#ddlAcccountName').prop('selectedIndex', 0);
                            $("#templateselection").prop("disabled", false);

                            $('#ddlGroups').removeAttr("disabled");
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#ddlAcccountName").prop('disabled', false);
                            $("#projectSelect").prop('disabled', false);
                        }
                        else {
                            ErrorData = response;
                            if (ErrorData !== '') {
                                $('#artifactProgress').removeClass('d-none');
                                currentPercentage = 0;
                                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                                $('#Analyse').addClass('btn-primary').attr('disabled', false);
                                $('.genArtifacts').removeClass('show');

                                $("#projCreateMsg").hide();
                                $('<b style="display: block;">We ran into some issues and we are sorry about that!</b><p> The log below will provide you insights into why the provisioning failed. You can raise an issue with the error logs <a id="EmailPopup" href="https://github.com/microsoft/AzureDevOpsDemoGenerator/issues/new" target="_blank">here</a> and we will try to help you.</p><p>Click on View Diagnostics button to share logs with us.</p>').appendTo("#errorDescription");
                                $('#GenerateArtifacts').removeClass('lodergif');
                                //$('#ExdvProgress').removeClass("d-block").addClass("d-none");
                                $("#errorNotify").removeClass("d-none").addClass("d-block");
                                $("#finalLink").removeClass("d-none").addClass("d-block");

                                $("#errorMail").empty().append(ErrorData);
                                $("#errorNotify").show();

                                $("#btnSubmit").prop("disabled", false);
                                $("#txtProjectName").val("");
                                $('#ddlAcccountName').prop('selectedIndex', 0);
                                $('#ddlGroups').removeAttr("disabled");
                                $("#ddlAcccountName").removeAttr("disabled");
                                $("#ddlAcccountName").prop('disabled', false);
                                $("#projectSelect").prop('disabled', false);
                            }
                        }
                    });
                    messageList = [];
                }
            }
        },
        error: function (xhr) {
            getStatus();
        }
    });
}

function DeleteFolder() {
    $.ajax({
        url: '../Extractor/RemoveFolder',
        type: 'GET',
        success: function () {

        },
        error: function () {
        }

    });
}