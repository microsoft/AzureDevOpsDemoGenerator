var messageList = [];
var uniqueId = $('#uid').val();
var ErrorData = [];
var statusCount = 0;

var messagesCount = 10;
var percentForMessage = Math.floor(100 / messagesCount);
var currentPercentage = 0;

$(document).ready(function () {
    $(window).scroll(function () {
        var scroll = $(window).scrollTop();
        if (scroll > 50) {
            $(".navbar").css("background-color", "#2279C3");
        }
        else {
            $(".navbar").css("background-color", "transparent");
        }
    });
    $('#ext_load').addClass('d-none');

    $('.redC').focusin(function () {
        $('.redC').css('border-color', '');
    })
    $('#close').click(function () {
        $("#msgSource").hide();
        return;
    });
    $('#closeStatus').click(function () {
        $("#statusmessages").hide();
        return;
    });
    $('#closeErrList').click(function () {
        $("#errrListDiv").hide();
        return;
    });

    //on project list change

    $('#isExtension').change(function () {
        var extension = $("input[id=isExtension]:checked").val()
        if (extension == "true") {
            $('#ext_load').removeClass('d-none');

            GetExtensions();
        } else {
            $('#extensionlist').empty();
        }
    })

    //$("#projectSelect").change(function () {
    //    document.getElementById("isExtension").checked = false;
    //    $('#extensionlist').empty();
    //});

    $("#submitForm").click(function () {
        $('#statusmessages').empty();
        $("#msg").empty();
        $("#msgSource").hide();

        messageList = [];
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        var templateName = $("#txttemplateName").val();

        var email = $('#useremail').val();
        if (templateName == "") {
            $("#msg").text("Enter Template Name");
            $("#msgSource").show();
        }
        if (email == "") {
            $("#msg").text("Enter valid userID");
            $("#msgSource").show();
            return;
        }
        else if (project == '' || project == '--select account--' || project == 0) {
            $("#msg").text("Please select Source Project");
            $("#msgSource").show();
            return;
        }
        else {
            $('#loader').show();
            $('#selectedAcc').val(project);
            $('#srcProjectName').val(projectName);

            //checking for extension
            var checkedExtension = [];
            var isextension = $("input[id=isExtension]:checked").val();
            debugger;
            if (isextension == "true") {
                $("input[class=extension]:checked").each(function () {
                    var values = $(this).val();
                    checkedExtension.push(values);
                })
            }
            var token = $('#hidaccessToken').val();
            var srcProjectID = $('#selectedAcc').val();
            var srcProjectname = $('#srcProjectName').val();
            var useremail = $('#useremail').val();
            var projectParam = {
                accessToken: token,
                SelectedID: srcProjectID,
                accountName: SourceAcc,
                SrcProjectName: srcProjectname,
                uid: uniqueId,
                Email: useremail,
                extensions: checkedExtension,
                TemplateName: templateName
            }
            var WITCount = 0;
            $('#loader').hide();

            $.post("../ProjectSetup/StartEnvironmentSetupProcess", projectParam, function (data) {
                if (data != "True") {
                    window.location.href = "~/Account/Verify";
                    return;
                }
            });
            $("#ddlAcccountName").prop('disabled', true);
            $('#projectSelect').prop('disabled', true);
            $('#TargetAcccountName').prop('disabled', true);
            $('#NewProjectName').prop('disabled', true);
            $("#submitForm").removeClass('btn-primary');
            $("#submitForm").prop('disabled', true);
            $('#useremail').prop('disabled', true);
            $('#dvProgress').show();
            getStatus();
            $('#generatebtnloader').removeClass('d-none');

        }
    });

    $('#ddlAcccountName').change(function () {
        var accSelected = $('#ddlAcccountName').val();
        $('#projectSelect').empty();
        var options = $("#projectSelect option");
        options.appendTo("#projectSelect", "Select Project");
        options.appendTo("#projectSelect");
        if (accSelected == '' || accSelected == '--select account--') {
            $("#msg").text("Please select a Account");
            $("#msgSource").show();
            return;
        }
        else {
            $('#projectloader').removeClass('d-none');

            var token = $('#hiddenAccessToken').val();
            var param = {
                accname: accSelected,
                pat: token
            }
            $.ajax({
                url: '../Extractor/GetprojectList',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(param),
                success: function (da) {
                    if (da.count > 0) {
                        $('#projectSelect').empty();
                        var opt = "";
                        //opt += "<option value='0'>Select Project</option>";
                        for (var i = 0; i < da.count; i++) {
                            opt += ' <option value="' + da.value[i].id + '">' + da.value[i].name + '</option>';
                        }
                        $("#projectSelect").append(opt);
                        var options = $("#projectSelect option");                    // Collect options         
                        options.detach().sort(function (a, b) {               // Detach from select, then Sort
                            var at = $(a).text();
                            var bt = $(b).text();
                            return (at > bt) ? 1 : ((at < bt) ? -1 : 0);            // Tell the sort function how to order
                        });
                        options.appendTo("#projectSelect", "Select Project");
                        options.appendTo("#projectSelect");
                        $('#projectloader').addClass('d-none');
                    }
                    else {
                        $("#msg").text("Some error occured :" + da.errmsg);
                        $("#msgSource").show();
                        $('#projectloader').addClass('d-none');
                        return;
                    }
                },
                error: function () {
                    $('#projectloader').addClass('d-none');
                }
            });
        }
    });

    $('#sendmailbtn').click(function () {
        var emailid = $('#emailid').val();
        var custname = $('#Custname').val();
        var checkValid = true;
        if (custname == "") {
            $('#Custname').css('border-color', 'red');
            checkValid = false;
            return;
        }
        else {
            $('#Custname').css('border-color', '');
        }
        if (emailid != "") {
            $('#emailerror').empty();
            if ($('#emailid').val().length > 0) {
                var email = $('#emailid').val();
                var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

                if (!filter.test(email)) {
                    $('#emailerror').empty();
                    $('#emailerror').append('Please provide a valid email address');
                    $('#emailid').focus;
                    $('#emailid').css('border-color', 'red');
                    checkValid = false;
                    return;
                }
            }
        }
        else if (emailid == "") {
            $('#emailid').css('border-color', 'red');
            checkValid = false;
            return;
        }
        else {
            $('#emailid').css('border-color', '');
        }
        if (checkValid == true) {
            $('#sendmailbtn').prop('disabled', false);
            $('#mailloader').css('visibility', 'visible');
            $.ajax({
                url: '../ProjectSetup/SendAdminMail',
                type: 'POST',
                data: { mailid: emailid, name: custname },
                success: function (data) {
                    if (data == 'success') {
                        $('#mailloader').hide();
                        myFunction();
                    }
                    else {
                        $('#mailloader').hide();
                    }
                }
            });
        }

    });
    $("#emailid").on('focusout', function () {
        $('#emailerror').empty();
        if ($('#emailid').val().length > 0) {
            var email = $('#emailid').val();
            var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

            if (!filter.test(email)) {
                $('#emailerror').empty();
                $('#emailerror').append('Please provide a valid email address');
                email.focus;
                return false;
            }
        }
    });

    //Teams Table
    $('#import').click(function () {
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        var templateName = $("#txttemplateName").val();
        var key = $('#hiddenAccessToken').val();

        if (templateName == "") {
            $("#msg").text("Enter Template Name");
            $("#msgSource").show();
            return;
        }
        if (project == '' || projectName == '--select account--' || project == 0) {
            $("#msg").text("Please select Source Project");
            $("#msgSource").show();
            return;
        }
        $('#generatebtnloader').removeClass('d-none');
        $('#import').removeClass('btn-primary').attr('disabled', 'disabled');

    });

    $('#teamstable').on('click', '.add', function () {
        var markup = "<tr><td width='300px'><input type='text' class='form-control teams' /></td><td width='10%;'><button type='button' class='btn btn-primary btn-sm add'>&#10010</button></td></tr>";
        var rmvbttn = "<td><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";

        //no of rows
        var x = $("table #teamstable tr").length;
        if (x > 0) {
            var y = x - 1;
            //taking its input field value
            var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
            var inputData = $('table #teamstable tr:eq(' + y + ') input').val();
            if (!(regex.test(inputData))) {
                $('#errorDiv').html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                return;
            }
            if (inputData == "") {
                //if empty, focusing it
                $('table #teamstable tr:eq(' + y + ') input').focus();
                return;
            }
            else {
                //else removing add button, disabling text box, adding remove button
                this.closest('td').remove();
                $('table #teamstable tr:eq(' + y + ') input').attr('disabled', 'disabled');

                $("table #teamstable tr:eq(" + y + ")").append(rmvbttn);
            }
        }
        $("table #teamstable").append(markup);
    });

    $('#teamstable').on('click', '.edit', function () {
        $(this).parent().parent().find('td:eq(0)').find('input').attr('disabled', false);
        $(this).parent().parent().find('td:eq(0)').find('input').addClass('edited');
    });

    $('#teamstable').on('focusout', '.edited', function () {
        debugger;
        var txtval = $(this).parent().parent().find('td:eq(0)').find('input').val();
        if (txtval != "") {
            $(this).parent().parent().find('td:eq(0)').find('input').attr('disabled', 'disabled');
        }
    });

    $('#teamstable').on('click', '.remove', function () {
        this.closest('tr').remove();
    });

    $("table #teamstable").on('focus', '.teams', function () {
        $('#errorDiv').empty();
    });

    $('#TeamGenerate').click(function () {
        debugger;
        var x = $("table #teamstable tr").length;
        var templatename = $('#txttemplateName').val();

        var inputData = new Array();
        if (x > 0) {
            $("table #teamstable tr").each(function (y) {
                var tdData = $("table #teamstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    inputData.push(tdData);
                }
            });
        }
        if (inputData.length > 0) {//&& templatename != ""
            $.ajax({
                url: "../Extractor/GenerateTeamJson",
                type: "POST",
                data: { teams: inputData, TemplateName: templatename },
                success: function (res) {
                    GetTeams();
                },
                error: function (xhr) {
                    alert(xhr.responseText);
                }
            });
        }

    });

    //Iteration Table
    $('#iterationstable').on('click', '.add', function () {
        var markup = "<tr><td width='300px'><input type='text' class='form-control teams' /></td><td width='10%'><button type='button' class='btn btn-primary btn-sm add'>&#10010</button></td></tr>";
        var rmvbttn = "<td><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";

        //no of rows
        var x = $("table #iterationstable tr").length;
        if (x > 0) {
            var y = x - 1;
            //taking its input field value
            var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
            var inputData = $('table #iterationstable tr:eq(' + y + ') input').val();
            if (!(regex.test(inputData))) {
                $('#errorDiv').html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                return;
            }
            if (inputData == "") {
                //if empty, focusing it
                $('table #iterationstable tr:eq(' + y + ') input').focus();
                return;
            }
            else {
                //else removing add button, disabling text box, adding remove button
                this.closest('td').remove();
                $('table #iterationstable tr:eq(' + y + ') input').attr('disabled', 'disabled');
                $("table #iterationstable tr:eq(" + y + ")").append(rmvbttn);
            }
        }
        $("table #iterationstable").append(markup);
    });

    $('#iterationstable').on('click', '.edit', function () {
        $(this).parent().parent().find('td:eq(0)').find('input').attr('disabled', false);
        $(this).parent().parent().find('td:eq(0)').find('input').addClass('edited');
    });

    $('#iterationstable').on('focusout', '.edited', function () {
        debugger;
        var txtval = $(this).parent().parent().find('td:eq(0)').find('input').val();
        if (txtval != "") {
            $(this).parent().parent().find('td:eq(0)').find('input').attr('disabled', 'disabled');
        }
    });

    $('#iterationstable').on('focusout', 'input', function () {
        var txtval = $(this).parent().parent().find('td:eq(0)').find('input').val();
        if (txtval != "") {
            $(this).parent().parent().find('td:eq(0)').find('input').attr('disabled', 'disabled');
        }
    });

    $('#iterationstable').on('click', '.remove', function () {
        this.closest('tr').remove();
    });

    $("table #iterationstable").on('focus', '.teams', function () {
        $('#errorDiv').empty();
    });

    $('#IterationTeamGenerate').click(function () {
        debugger;
        var x = $("table #iterationstable tr").length;
        var templatename = $('#txttemplateName').val();
        var inputData = new Array();
        if (x > 0) {
            $("table #iterationstable tr").each(function (y) {
                var tdData = $("table #iterationstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    inputData.push(tdData);
                }
            });
        }
        if (inputData.length > 0) {//&& templatename != ""
            $.ajax({
                url: "../Extractor/GenerateIterationJson",
                type: "POST",
                data: { Iteration: inputData, TemplateName: templatename },
                success: function (res) {
                    GetIterations();
                },
                error: function (xhr) {
                    alert(xhr.responseText);
                }
            });
        }

    });

    //Radio button
    var extractor = $('input:radio[id="extractor"]:checked').val();
    var brandnew = $('input:radio[id="brandnew"]:checked').val();

    $('input:radio[id="extractor"]').click(function () {
        var extractor = $('input:radio[id="extractor"]:checked').val();
        if (extractor == "on") {
            $('.listNewTemplateDivs').addClass('d-none');
            $('#tabs').addClass('d-none');
            $('.listDivs').removeClass('d-none');
            $('#TeamGenerate').removeClass('d-none');
            $('#TeamGenerate1').addClass('d-none');
            $('#IterationTeamGenerate').removeClass('d-none');
            $('#IterationTeamGenerate1').addClass('d-none');
        }
    });
    $('input:radio[id="brandnew"]').click(function () {
        var brandnew = $('input:radio[id="brandnew"]:checked').val();
        if (brandnew == "on") {
            $('.listNewTemplateDivs').removeClass('d-none');
            $('#tabs').removeClass('d-none');
            $('.listDivs').addClass('d-none');
            $('#TeamGenerate').addClass('d-none');
            $('#TeamGenerate1').removeClass('d-none');
            $('#IterationTeamGenerate').addClass('d-none');
            $('#IterationTeamGenerate1').removeClass('d-none');
            $('#updateTemplate').addClass('d-none');
            $('#importTemplate').removeClass('d-none');
            $('#txttemplateName').attr('disabled', false);
        }
        var row = ""; row += '<tr><td><input class="form-control teams" type="text" /></td><td><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
        $('#teamstable').empty().append(row);

        var iterow = ""; iterow += '<tr><td><input class="form-control iteration" type="text" /></td><td><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
        $('#tabs').removeClass('d-none');
        $('#iterationstable').empty().append(iterow);
    });
    //Importing at one time
    $('#importTemplate').click(function () {
        debugger;
        var t = $("table #teamstable tr").length;
        var i = $("table #iterationstable tr").length;
        var templatename = $('#txttemplateName').val();
        var processtemplate = $('#processtemplate').empty();
        if (processtemplate == "") {
            processtemplate = "Scrum";
        }
        var teamData = new Array();
        var iterationData = new Array();
        if (templatename == "") {
            $('#txttemplateName').css('border-color', 'red');
            return;
        }
        if (t > 0) {
            $("table #iterationstable tr").each(function (y) {
                var tdData = $("table #iterationstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
                    if (!(regex.test(tdData))) {
                        $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                        $("#msgSource").show();
                        return;
                    }
                    iterationData.push(tdData);
                }
            });
        }
        if (i > 0) {
            $("table #teamstable tr").each(function (y) {
                var tdData = $("table #teamstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
                    if (!(regex.test(tdData))) {
                        $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                        $("#msgSource").show();
                        return;
                    }
                    teamData.push(tdData);
                }
            });
        }
        if ((teamData.length > 0 || iterationData.length > 0) && templatename != "") {
            debugger;
            $('#updateTemplate').removeClass('d-none');
            $('#txttemplateName').attr('disabled', 'disabled');
            $('#importTemplate').removeClass('btn-primary').attr('disabled', 'disabled');
            var parms = {
                Iteration: iterationData, Teams: teamData, TemplateName: templatename,
                ProcessTemplateName: processtemplate
            }
            $.ajax({
                url: "../Extractor/GenerateTeamIterationJson",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(parms),
                success: function (res) {
                    $('#importTemplate').addClass('d-none');
                    console.log("https://demogen.azurewebsites.net/?" + res);
                    alert("https://demogen.azurewebsites.net/?" + res);
                    GetTeams();
                    GetIterations();
                },
                error: function (xhr) {
                    $('#importTemplate').removeClass('d-none');
                    alert(xhr.responseText);
                }
            });
        }
    });

    //validation on key up
    $('table #teamstable').on('keyup', '.teams', function () {
        var teamtext = this.value;
        var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
        if (!(regex.test(teamtext))) {
            $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
            $("#msgSource").show();
            $('#importTemplate').removeClass('btn-primary');
            $('#importTemplate').attr('disabled', 'disabled');
            return;
        }
        else {
            $("#msgSource").hide();
            $('#importTemplate').addClass('btn-primary');
            $('#importTemplate').attr('disabled', false);
        }
    });

    $('table #iterationstable').on('keyup', '.iteration', function () {
        var teamtext = this.value;
        var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
        if (!(regex.test(teamtext))) {
            $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
            $("#msgSource").show();
            $('#importTemplate').removeClass('btn-primary');
            $('#importTemplate').attr('disabled', 'disabled');
            return;
        }
        else {
            $("#msgSource").hide();
            $('#importTemplate').addClass('btn-primary');
            $('#importTemplate').attr('disabled', false);
        }
    });

    //Update Template
    $('#updateTemplate').click(function () {
        var t = $("table #teamstable tr").length;
        var i = $("table #iterationstable tr").length;
        var templatename = $('#txttemplateName').val();
        var teamData = new Array();
        var iterationData = new Array();

        if (t > 0) {
            $("table #iterationstable tr").each(function (y) {
                var tdData = $("table #iterationstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
                    if (!(regex.test(tdData))) {
                        $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                        $("#msgSource").show();
                        return;
                    }
                    iterationData.push(tdData);
                }
            });
        }
        if (i > 0) {
            $("table #teamstable tr").each(function (y) {
                var tdData = $("table #teamstable tr:eq(" + y + ") td:eq(0) input").val();
                if (tdData != "" && typeof (tdData) != undefined) {
                    var regex = /^[a-zA-Z0-9(){}_\s-]*$/;
                    if (!(regex.test(tdData))) {
                        $("#msg").empty().html('<p class="teamvalidation">verify that the name does not exceed the maximum character limit, only contains valid characters, and is not a reserved name. The following characters are not valid: ~ ; \' + = , < > | / \ ? : & $ * " # [ ]</p>');
                        $("#msgSource").show();
                        return;
                    }
                    teamData.push(tdData);
                }
            });
        }
        if ((teamData.length > 0 || iterationData.length > 0) && templatename != "") {
            $('#txttemplateName').attr('disabled', 'disabled');
            var parms = {
                Iteration: iterationData, Teams: teamData, TemplateName: templatename
            }
            $.ajax({
                url: "../Extractor/UpdateTeamIterationJson",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(parms),
                success: function (res) {
                    GetTeams();
                    GetIterations();
                },
                error: function (xhr) {
                    alert(xhr.responseText);
                }
            });
        }
    });

    //on project change
    $('#projectSelect').change(function () {
        $("#msgSource").hide();
        var project = $('#projectSelect option:selected').val();
        var accSelected = $('#ddlAcccountName').val();
        var key = $('#hiddenAccessToken').val();
        $('#processtemplate').empty();
        $('#processTemplateLoader').removeClass('d-none');
        if (project == 0 || project == "") {
            return;
        }
        if (accSelected == '' || accSelected == '--select account--') {
            return;
        }
        else {
            $.ajax({
                url: '../Extractor/GetProjectPropertirs',
                type: 'POST',
                data: { accname: accSelected, project: project, _credentials: key },
                success: function (res) {
                    $('#processtemplate').empty().append(res.value[4].value);
                    $('#processTemplateLoader').addClass('d-none');
                    console.log(res);
                    var p = res.value[4].value;
                    if (p !== "Scrum" && p !== "Agile") {
                        $('#processTemplateLoader').addClass('d-none');
                        $("#msg").text("Note: Process templates other than Agile and Scrum are not supported! If the Process Template is inherited from any Standard Process Templates, tool will map the project to standard process template.");
                        $("#msgSource").show();
                        return;
                    }
                },
                error: function (e) {
                    $('#processTemplateLoader').addClass('d-none');
                    $("#msg").text(e);
                    $("#msgSource").show();
                }
            });
        }
    });
});


function GetTeams() {
    var srcProjectname = $('#txttemplateName').val();
    $.ajax({
        url: "../Extractor/GetTeam",
        type: "POST",
        data: { Templatename: srcProjectname },
        success: function (res) {
            if (res.length > 0) {
                var row = "";
                res.forEach(function (x) {
                    row += '<tr>';
                    row += "<td width=300px'><input type='text' class='form-control teams' value='" + x.name + "' disabled/></td><td width='40px'><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";
                    row += '</tr>';
                });
                row += '<tr><td><input class="form-control teams" type="text" /></td><td width="10%"><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
                $('#tabs').removeClass('d-none');
                $('#teamstable').empty().append(row);
            }

        }, error: function () { }
    });
}

function GetIterations() {
    var srcProjectname = $('#txttemplateName').val();
    $.ajax({
        url: "../Extractor/GetIterations",
        type: "POST",
        data: { Templatename: srcProjectname },
        success: function (res) {
            if (res.length > 0) {
                var row = "";
                res.forEach(function (x) {
                    row += '<tr>';
                    row += "<td width=300px'><input type='text' class='form-control iteration' value='" + x.name + "' disabled/></td><td width='40px'><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";
                    row += '</tr>';
                });
                row += '<tr><td><input class="form-control iteration" type="text" /></td><td width="10%"><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
                $('#tabs').removeClass('d-none');
                $('#iterationstable').empty().append(row);
            }
            console.log(res)
        }, error: function () { }
    });
}

function myFunction() {
    var x = document.getElementById("snackbar")
    x.className = "show";
    setTimeout(function () { x.className = x.className.replace("show", ""); }, 3000);
}

function emailvalidation() {
    $('#emailerror').empty();
    if ($('#emailid').val().length > 0) {
        var email = $('#emailid').val();
        var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

        if (!filter.test(email)) {
            $('#emailerror').empty();
            $('#emailerror').append('Please provide a valid email address');
            email.focus;
            return false;
        }
        return true;
    }
}

function getStatus() {
    $.ajax({
        url: "../ProjectSetup/GetCurrentProgress",
        type: 'GET',
        async: false,
        data: { id: uniqueId },
        success: function (data) {
            var isMessageShown = true;
            $('#dvProgress').css('display', 'inline');
            $('#statusmessages').css('display', 'inline');
            if (jQuery.inArray(data, messageList) == -1) {
                messageList.push(data);
                isMessageShown = false;
            }
            if (data != "end" && data != "100") {
                if (isMessageShown == false) {
                    if (messageList.length == 1) {

                        $('#progressBar').width(currentPercentage++ + '%');
                        if (data.length > 0) {
                            $('#statusmessages').append('<img src="../Images/check-10.png" style="padding:4px;"/> ' + data + ' <br />');
                        }
                    }
                    else {
                        if (data.indexOf("TF200019") == -1) {
                            $('#progressBar').width(currentPercentage++ + '%');
                        }
                        $('#statusmessages').append('<img src="../Images/check-10.png" style="padding:4px;"/>' + data + ' <br />');
                    }
                }
                else if (currentPercentage <= ((messageList.length + 1) * percentForMessage) && currentPercentage <= 100) {
                    $('#progressBar').width(currentPercentage++ + '%');
                }
                window.setTimeout("getStatus()", 500);
            }
            else {
                stopProcess();
                var erruniqueId = uniqueId + "_Errors";
                GetErrormsg();
                $('#useremail').prop('disabled', false);
                GetTeams();
                GetIterations();
                //$('#statusmessages').append('<a href="https://' + $('#TargetAcccountName').val() + '.visualstudio.com/' + $('#NewProjectName').val() + '" target="_blank"> Here is your new project</a><br />');
            }

        }
    });

}

function GetErrormsg() {
    var erruniqueId = uniqueId + "_Errors";
    console.log(erruniqueId);
    $.ajax({
        url: "../ProjectSetup/GetCurrentProgress",
        type: 'POST',
        data: { id: erruniqueId },
        success: function (dat) {
            if (dat != "") {
                $('#errrListDiv').css('display', 'inline');
                $('#errrListDiv').append('<br/> <img src="../Images/error.png" style="padding:4px; width:15px; height:15px;"/>' + dat + ' <br />');
            }
        }
    });
}

function stopProcess() {
    $('#generatebtnloader').addClass('d-none');
    $('#dvProgress').css('display', 'none');
    $('#dvProgress').hide();
    $("#ddlAcccountName").prop('disabled', false);
    $('#projectSelect').prop('disabled', false);
    $('#TargetAcccountName').prop('disabled', false);
    $('#NewProjectName').prop('disabled', false);
    $("#submitForm").prop('disabled', false);
    $("#submitForm").addClass('btn-primary');
    $("#statusmessages").hide();

    currentPercentage = 0;
    messageList = [];
}

function GetExtensions() {
    var accname = $('#ddlAcccountName').val();
    var pat = $('#hidaccessToken').val();
    $.ajax({
        url: "../Extractor/CheckForInstalledExtensions",
        type: "POST",
        data: { AccountName: accname, PAT: pat },
        success: function (data) {
            console.log(data);
            if (data.length > 0) {
                var str = "<p style='color: #1335da;'>Select the extension required for the project</p>";
                $.each(data, function (i, exten) {
                    str = str + '<input type="checkbox" class="extension" value="' + exten.name + "$" + exten.PublisherId + "$" + exten.ExtensionId + '"/>&nbsp;&nbsp;<label>' + exten.name + '</label><br/>';
                });
                $('#ext_load').addClass('d-none');
                $('#extensionlist').empty().append(str);
            } else {
                $('#ext_load').addClass('d-none');
            }

        }
    })
}

function GetTeamIteration() {
    $.ajax({
        url: '../Extractor/GetTeamListtoSave',
        type: 'POST',
        data: { projectName: projectName, AccountName: SourceAcc, pat: key },
        success: function (res) {
            if (res.length > 0) {
                var row = "";
                res.forEach(function (x) {
                    row += '<tr>';
                    row += "<td width=300px'><input type='text' class='form-control teams' value='" + x.name + "' disabled/></td><td width='40px'><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";
                    row += '</tr>';
                });
                row += '<tr><td><input class="form-control teams" type="text" /></td><td width="10%;"><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
                $('#import').addClass('btn-primary').attr('disabled', false);
                $('#generatebtnloader').addClass('d-none');
                $('#tabs').removeClass('d-none');
                $('#teamstable').empty().append(row);
            }

        },
        error: function () {
            $('#import').addClass('btn-primary').attr('disabled', false);
            $('#generatebtnloader').addClass('d-none');
            alert("Error");
        }
    });
    $.ajax({
        url: '../Extractor/GetIterationsToSave',
        type: 'POST',
        data: { projectName: projectName, AccountName: SourceAcc, pat: key },
        success: function (res) {
            if (res.length > 0) {
                var row = "";
                res.forEach(function (x) {
                    row += '<tr>';
                    row += "<td width=300px'><input type='text' class='form-control' value='" + x.name + "' disabled/></td><td width='40px'><button type='button' class='btn btn-primary btn-sm remove'>&#10005</button></td><td><button type='button' class='btn btn-primary btn-sm edit'>&#9998</button></td>";
                    row += '</tr>';
                });
                $('#import').addClass('btn-primary').attr('disabled', false);
                $('#generatebtnloader').addClass('d-none');
                row += '<tr><td><input class="form-control" type="text" /></td width="10%;"><td><button type="button" class="btn btn-primary btn-sm add">&#10010</button></td></tr>';
                $('#tabs').removeClass('d-none');
                $('#iterationstable').empty().append(row);
            }
        },
        error: function () {
            $('#import').addClass('btn-primary').attr('disabled', false);
            $('#generatebtnloader').addClass('d-none');
            alert("Error");
        }
    });
}