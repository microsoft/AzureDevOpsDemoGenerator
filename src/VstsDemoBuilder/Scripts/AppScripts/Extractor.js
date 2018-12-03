
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
            $('#projectloader').removeClass('d-none');
            $('#analyseDiv').addClass('d-none'); $('#analytics').html("");
            $('#genArtDiv').addClass('d-none'); $('#artifactProgress').html("");
            $('#GenerateArtifact').addClass('d-none');
            $('#finalLink').addClass('d-none');
            $('#Analyse').attr('disabled', 'disabled');
            $('#Analyse').removeClass('btn-primary');

            var token = $('#key').val();
            //var param = {
            //    accname: accSelected,
            //    pat: token
            //};
            $.ajax({
                url: '../Extractor/GetprojectList',
                type: 'POST',
                data: { accname: accSelected, pat: token },
                success: function (da) {
                    if (da.count > 0) {
                        $('#Analyse').addClass('btn-primary').attr('disabled', false);

                        console.log(da);
                        $('#projectSelect').empty();
                        var opt = "";
                        opt += ' <option value="0" selected="selected">Select Project</option>';
                        for (var i = 0; i < da.count; i++) {
                            opt += ' <option value="' + da.value[i].id + '">' + da.value[i].name + '</option>';
                        }
                        $("#projectSelect").append(opt);
                        //var options = $("#projectSelect option");                    // Collect options         
                        //options.detach().sort(function (a, b) {               // Detach from select, then Sort
                        //    var at = $(a).text();
                        //    var bt = $(b).text();
                        //    return (at > bt) ? 1 : ((at < bt) ? -1 : 0);            // Tell the sort function how to order
                        //});
                        //options.appendTo("#projectSelect");
                        $('#projectloader').addClass('d-none');
                        $('#Analyse').attr('disabled', false);
                        $('#Analyse').addClass('btn-primary');
                    }
                    else {
                        $('#projectloader').addClass('d-none');
                        $("#projectSelect_Error").text(da.errmsg);
                        $("#projectSelect_Error").removeClass('d-none');
                        $('#Analyse').attr('disabled', false);
                        $('#Analyse').addClass('btn-primary');
                        return;
                    }
                },
                error: function () {
                    $('#projectloader').addClass('d-none');
                    $('#Analyse').attr('disabled', false);
                    $('#Analyse').addClass('btn-primary');
                }
            });
        }
    });

    $('#projectSelect').change(function () {
        $('#projectloader').removeClass('d-none');
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
        if (project === 0 || project === "") {
            return;
        }
        if (accSelected === '' || accSelected === 'Select Organization') {
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
                    $('#projectloader').addClass('d-none');
                    $('#processtemplate').empty().val(res.value[4].value);
                    $('#TemplateClass').empty().val(res.TypeClass);
                    $('#processTemplateLoader').addClass('d-none');
                    var p = res.value[4].value;
                    if (p !== "Scrum" && p !== "Agile") {
                        $('#processTemplateLoader').addClass('d-none');
                        $("#projectSelect_Error").text("Note: Please select a project that uses the standard Scrum or Agile process template.");
                        $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
                        $("#projectSelect_Error").removeClass('d-none');
                        return;
                    }
                    $('#Analyse').addClass('btn-primary').attr('disabled', false);
                },
                error: function (e) {
                    $('#processTemplateLoader').addClass('d-none');
                    $("#projectSelect_Error").text(e);
                    $("#projectSelect_Error").removeClass('d-none');
                    $('#projectloader').addClass('d-none');
                    $('#Analyse').addClass('btn-primary').attr('disabled', false);
                }
            });
        }
    });

    $('#Analyse').click(function () {
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
        if (project === "" || projectName === '--select project--' || project === 0 || typeof project === undefined) {
            $("#projectSelect_Error").text("Please select a Project");
            $("#projectSelect_Error").removeClass('d-none');
            return;
        }
        var projectN = {
            ProjectName: projectName,
            accountName: SourceAcc, accessToken: key
        };
        $('#Analyse').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#imgLoading').removeClass('d-none');

        $('#genArtDiv').addClass('d-none');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');
        $('#ExStatus-messages').empty();
        $('#accountLink').empty(); $('#finalLink').addClass('d-none');
        $.ajax({
            url: '../Extractor/AnalyzeProject',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(projectN),
            success: function (res) {
                var row = "";
                if (res !== "") {
                    var processTemplate = $('#processtemplate').val();
                    var templaetClass = $('#TemplateClass').val();
                    if (templaetClass !== "system") {
                        $('#GenerateArtifacts').hide();
                        $('#templateError').empty().append("Tool doesn't support for projects based on custom or derived process templates");
                    }
                    else {
                        $('#templateError').empty().append("Everything looks good.Click the button below to proceed.");
                        $('#GenerateArtifacts').show();
                    }
                    row += ' Selected Project: ' + projectName + ', &nbsp; Process Template: ' + processTemplate + ' (' + templaetClass + ') <br />';

                    if (res.teamCount !== 0 && res.teamCount !== null)
                        row += '<i class="fas fa-check-circle"></i>' + ' Teams: ' + res.teamCount + '<br />';
                    if (res.IterationCount !== 0 && res.IterationCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Iteration: ' + res.IterationCount + '<br />';
                    if (res.fetchedEpics !== 0 && res.fetchedEpics !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Epics: ' + res.fetchedEpics + '<br />';
                    if (res.fetchedFeatures !== 0 && res.fetchedFeatures !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Features: ' + res.fetchedFeatures + '<br />';
                    if (res.fetchedPBIs !== 0 && res.fetchedPBIs !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' PBIs: ' + res.fetchedPBIs + '<br />';
                    if (res.fetchedTasks !== 0 && res.fetchedTasks !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Tasks: ' + res.fetchedTasks + '<br />';

                    if (res.fetchedBugs !== 0 && res.fetchedBugs !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Bugs: ' + res.fetchedBugs + '<br />';
                    if (res.fetchedUserStories !== 0 && res.fetchedUserStories !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' User Stories: ' + res.fetchedUserStories + '<br />';

                    if (res.fetchedTestSuits !== 0 && res.fetchedTestSuits !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Test Suites: ' + res.fetchedTestSuits + '<br />';

                    if (res.fetchedTestPlan !== 0 && res.fetchedTestPlan !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Test Plans: ' + res.fetchedTestPlan + '<br />';

                    if (res.fetchedTestCase !== 0 && res.fetchedTestCase !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Test Cases: ' + res.fetchedTestCase + '<br />';

                    if (res.fetchedFeedbackRequest !== 0 && res.fetchedFeedbackRequest !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Feedback Requests: ' + res.fetchedFeedbackRequest + '<br />';

                    if (res.BuildDefCount !== 0 && res.BuildDefCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Build Definitions: ' + res.BuildDefCount + '<br />';

                    if (res.ReleaseDefCount !== 0 && res.ReleaseDefCount !== null)
                        row += '<i class="fas fa-check-circle" ></i >' + ' Release Definitions: ' + res.ReleaseDefCount + '<br />';
                }
                $('#analyseDiv').removeClass('d-none');
                $('#analytics').empty().append(row);
                $('#imgLoading').addClass('d-none');
                $('#Analyse').addClass('btn-primary').attr('disabled', false);

                $('#genArtDiv').removeClass('d-none');
                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
            },
            error: function (er) {
                $('#Analyse').addClass('btn-primary').attr('disabled', false);
                $('#imgLoading').addClass('d-none');
            }
        });
        $('#generatebtnloader').removeClass('d-none');

    });

    $('#GenerateArtifacts').click(function () {
        $('.accorDetails').removeClass('show');
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        var key = $('#key').val();
        var processTemplate = $('#processtemplate').val();
        if (SourceAcc === "" || SourceAcc === "Select Account") {
            $("#ddlAcccountName_Error").text("Please select Source Account Name");
            $("#ddlAcccountName_Error").removeClass('d-none');
            return;
        }
        if (project === '' || projectName === '--select account--' || project === 0) {
            $("#projectSelect_Error").text("Please select Source Project");
            $("#projectSelect_Error").removeClass('d-none');
            return;
        }
        var projects = {
            ProjectName: projectName,
            accountName: SourceAcc, accessToken: key, id: uniqueId, ProcessTemplate: processTemplate
        };
        $('#ExStatus-messages').html('');
        $('#ExStatus-messages').show();
        $('#GenerateArtifact').removeClass('d-none');
        $('#ExdvProgress').removeClass('d-none');
        $('#GenerateArtifacts').removeClass('btn-primary').attr('disabled', 'disabled');

        $.ajax({
            url: '../Extractor/StartEnvironmentSetupProcess',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(projects),
            success: function (res) {
                if (res === "True") {
                    getStatus();
                }
            },
            error: function (er) {
                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);

            }
        });
    });

});

function getStatus() {
    $.ajax({
        url: 'GetCurrentProgress/' + uniqueId,
        type: 'GET',
        success: function (data) {
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
                        if (response === "100") {
                            $('#artifactProgress').removeClass('d-none');

                            $('#ExdvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            currentPercentage = 0;
                            var link = "../ExtractedTemplate/" + finalprojectName + ".zip";
                            $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                            $('.genArtifacts').removeClass('show');
                            $('<b style="display: block;">Congratulations! Your template is ready. Click <a href="' + link + '" target="_blank" style="font-weight:700;text-decoration:underline;" download>here</a> to download the Zip file</b>').appendTo("#accountLink");

                            $('#ExtractorProgressBar').width(currentPercentage++ + '%');
                            $("#finalLink").removeClass("d-none").addClass("d-block");
                            $("#btnSubmit").prop("disabled", false);
                            $("#txtProjectName").val("");

                            $('#ddlAcccountName').prop('selectedIndex', 0);
                            $("#templateselection").prop("disabled", false);

                            $('#ddlGroups').removeAttr("disabled");
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");

                        }
                        else {
                            ErrorData = response;
                            if (ErrorData !== '') {
                                $('#artifactProgress').removeClass('d-none');

                                currentPercentage = 0;
                                var links = "../ExtractedTemplate/" + finalprojectName + ".zip";
                                $('#GenerateArtifacts').addClass('btn-primary').attr('disabled', false);
                                $('.genArtifacts').removeClass('show');
                                $('<b style="display: block;"> Click <a href="' + links + '" target="_blank" style="font-weight:700;text-decoration:underline;" download>here</a> to download the Zip file</b>').appendTo("#accountLink");

                                $("#projCreateMsg").hide();
                                $('<b style="display: block;">We ran into some issues and we are sorry about that!</b><p> The log below will provide you insights into why the provisioning failed. You can email us the log  to <a id="EmailPopup"><i>devopsdemos@microsoft.com</i></a> and we will try to help you.</p><p>Click on View Diagnostics button to share logs with us.</p>').appendTo("#errorDescription");
                                $('#ExdvProgress').removeClass("d-block").addClass("d-none");
                                $("#errorNotify").removeClass("d-none").addClass("d-block");

                                $("#errorMail").empty().append(ErrorData);
                                $("#errorNotify").show();

                                $("#btnSubmit").prop("disabled", false);
                                $("#txtProjectName").val("");
                                $('#ddlAcccountName').prop('selectedIndex', 0);
                                $('#ddlGroups').removeAttr("disabled");
                                $("#ddlAcccountName").removeAttr("disabled");
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