/// <reference path="../jquery-1.12.4.min.js" />

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

    $.ajax({
        url: "../Environment/CheckSession",
        type: "GET",
        success: function (data) {
            if (data.length > 0) {
                if ((data[0] != "" || data[0] != null) && (data[1] != "" || data[1] != null)) {
                    var templateNameExt = "";
                    var templateIdExt = "";
                    templateNameExt = data[0];
                    templateIdExt = data[1];

                    if ((templateNameExt != null || typeof templateNameExt != "undefined" || templateNameExt != "") && (templateIdExt != "" || templateIdExt != null || typeof templateIdExt != "undefined")) {
                        $("#templateselection").addClass('d-none');
                        $('#ddlTemplates').val(templateNameExt);
                        GetTemplates(templateNameExt);
                        $('#lblDefaultDescription').addClass('d-none');
                    }
                }
            }
            else {
                $('#ddlTemplates').val("SmartHotel360");
            }
        }
    });


    $("input[id=Random]").attr('disabled', true);
    $("input[id=Select]").attr('disabled', true);
    $("#btnUserShow").attr('disabled', true);

    ga('send', 'event', 'Create page', 'visited');
});

$(function () {

    $("#Default").prop("checked", "checked");

    $("input").on("keypress", function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });

    $('.dropdown-container')
        .on('input', '.dropdown-search', function () {
            var target = $(this);
            var search = target.val().toLowerCase();

            if (!search) {
                $('li').show();
                return false;
            }

            $('li').each(function () {
                var text = $(this).text().toLowerCase();
                var match = text.indexOf(search) > -1;
                $(this).toggle(match);
            });
        })
        .on('change', '[type="checkbox"]', function () {
            var numChecked = $('[type="checkbox"]:checked').length;
            $('.quantity').text(numChecked || '0');
        });

    //clear searchBox
    $("#clrSearch").click(function () {
        $(".dropdown-search").val('');
        $('li').show();
    });

    $("#DeselectChk").click(function () {
        $(".checkbox").each(function () {
            this.checked = false;
        });
        $('.quantity').text('0');
    });

    //RadioButtons
    $('input:radio').change(function () {
        var changedRadio = this;

        $('input:radio').each(function () {
            this.checked = false;
        });
        $("#ddlUserContainer").hide();
        $("#userModal").modal('hide');
        //$('.quantity').text('0');
        changedRadio.checked = true;
    });

    $(".checkbox").click(function () {

        var checkCount = $(":checkbox:checked").length;
        if (checkCount > 5) {
            this.checked = false;
            alert("Maximum 5 users can be selected");
        }
    });

    $("#btnUserShow").click(function () {
        var radioVal = $("input[type='radio']:checked").val();
        if (radioVal == "Select") {

            $("#ddlUserContainer").show();
            $("#userModal").modal('show');
        }

    });

});

var messageList = [];
/**/
/**/
var ID = function () {
    // Math.random should be unique because of its seeding algorithm.
    // Convert it to base 36 (numbers + letters), and grab the first 9 characters
    // after the decimal.
    return '_' + Math.random().toString(36).substr(2, 9);
};
var uniqueId = "";
/**/
/**/
var ErrorData = '';
var statusCount = 0;
var selectedTemplate = "";
var messagesCount = 13;
var percentForMessage = Math.floor(100 / messagesCount);
var currentPercentage = 0;
var projectNameForLink = '';
var isExtensionNeeded = false;
var isAgreedTerms = false;
var microsoft = "";
var ThirdParty = "";

$(document).ready(function (event) {
    uniqueId = ID();
    $('.rmverror').click(function () {
        var errID = this.nextElementSibling.getAttribute('id');
        $('#' + errID).removeClass("d-block").addClass("d-none");
    });

    $('#templateselection').click(function () {
        $('.VSTemplateSelection').removeClass('d-none').addClass('d-block');
        $('#ddlTemplates_Error').removeClass("d-block").addClass("d-none");
    });

    //ON CHANGE OF ACCOUNT- VALIDATE EXTENSION
    $('#ddlAcccountName').change(function () {
        $('#status-messages').empty().hide();
        $('#textMuted').removeClass("d-block").addClass("d-none");
        $('#dvProgress').removeClass("d-block").addClass("d-none");
        $('#accountLink').empty();
        $('#finalLink').removeClass("d-block").addClass("d-none");
        $('#errorNotify').removeClass("d-block").addClass("d-none");

        var accountNameExt = $('#ddlAcccountName option:selected').val();
        var selectedTemplateForExtension = $('#ddlTemplates').val();

        if (selectedTemplateForExtension == "SonarQube") {
            $("#SoanrQubeDiv").show();
        }
        else {
            $("#SoanrQubeDiv").hide();
        }
        if (accountNameExt == "" || accountNameExt == "Select Account") {
            return false;
        }
        else if (selectedTemplateForExtension == "") {
            return;
        }
        else {
            GetRequiredExtension();
        }
    });

    //ON CHANGE OF TEMPLATE- VALIDATE EXTENSION
    $('#selecttmplate').click(function () {
        //Added
        $('#lblDefaultDescription').hide();
        var GroputempSelected = $(".template.selected").data('template');
        if (GroputempSelected != "") {
            $('#ddlTemplates').val(GroputempSelected);
            $(".VSTemplateSelection").fadeOut('fast');
        }

        $(".VSTemplateSelection").removeClass('d-block').addClass('d-none');
        //till here
        $('#status-messages').empty().hide();
        $('#textMuted').removeClass("d-block").addClass("d-none");
        $('#dvProgress').removeClass("d-block").addClass("d-none");
        $('#accountLink').empty();
        $('#finalLink').removeClass("d-block").addClass("d-none");
        $('#errorNotify').removeClass("d-block").addClass("d-none");

        //Added
        $("#projectParameters").hide();
        $("#projectParameters").html('');
        $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").removeClass("d-block").addClass("d-none");
        var TemplateName = $('#ddlTemplates').val();
        if (TemplateName == "MyShuttle-Java") {
            $("#NotificationModal").modal('show');
        }
        if (TemplateName == "SonarQube") {
            $("#SoanrQubeDiv").show();
        }
        else {
            $("#SoanrQubeDiv").hide();
        }
        var Url = 'GetTemplate/';
        $.get(Url, { "TemplateName": TemplateName }, function (data) {
            if (data != "") {
                var ParsedData = JSON.parse(data);
                var Description = ParsedData.Description;
                var parameters = ParsedData.Parameters;

                if (Description != "") {
                    $("#descContainer").html('');
                    $("#descContainer").html(Description);
                    $("#lblDescription").removeClass("d-none").addClass("d-block");

                }
                else { $("#lblDescription").removeClass("d-block").addClass("d-none"); }
                if (parameters != undefined) {
                    if (parameters.length > 0) {
                        $.each(parameters, function (key, value) {
                            console.log(key, value)
                        });

                        $.each(parameters, function (key, value) {
                            $('<div class="form-group row projParameters"><label for="sonarqubeurl" class="col-lg-4 col-form-label" style="font-weight:400">' + value.label + ':</label><div class="col-lg-8"><input type="text" class="form-control project-parameters rmverror" id="txt' + value.fieldName + '"  proj-parameter-name="' + value.fieldName + '" placeholder="' + value.fieldName + '"><div class="alert alert-danger d-none" role="alert" id="txt' + value.fieldName + '_Error"></div></div>').appendTo("#projectParameters");
                        });
                        $("#projectParameters").show();
                    }
                    else { $("#projectParameters").html(''); }
                }
            }
            else {
                $("#lblDescription").removeClass("d-block").addClass("d-none");
                $("#projectParameters").html('');

            }
        });
        if (TemplateName != "") {
            checkForInstalledExtensions(TemplateName, function callBack(extensions) {
                if (extensions.message != "no extensions required" && extensions.message != "" && extensions.message != undefined && extensions.message.indexOf("Error") == -1 && extensions.message != "Template not found") {

                    $("#extensionError").empty().append(extensions.message);//html(extensions.message);
                    $("#extensionError").show();
                    $("#lblextensionError").removeClass("d-none").addClass("d-block");

                    if (extensions.status != "true") {
                        $("#btnSubmit").prop("disabled", true);
                        isExtensionNeeded = true;
                        microsoft = $('#agreeTermsConditions').attr('placeholder');
                        if (microsoft != "microsoft") {
                            microsoft = "";
                        }
                        ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                        if (ThirdParty != "thirdparty") {
                            ThirdParty = "";
                        }
                    } else { $("#btnSubmit").prop("disabled", false); }
                }
                else { $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").removeClass("d-block").addClass("d-none"); $("#btnSubmit").prop("disabled", false); }

            });
        }
        //Till here

        var accountNameExt = $('#ddlAcccountName option:selected').val();
        var selectedTemplateForExtension = $('#ddlTemplates').val();

        if (accountNameExt == "" || accountNameExt == "--select account--") {
            return false;
        }
        else if (selectedTemplateForExtension == "") {
            return;
        }
        else {
            GetRequiredExtension();
        }

    });

    $("body").on("click", "#EmailPopup", function () {
        $("#EmailModal").modal('show');
    });

    $("#sendEmail").click(function () {
        var pattern = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
        //var pattern = /^[a-zA-Z0-9._-]+@@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
        var emailAddress = $("#toEmail").val();
        var AccountName = $("#toAccountName").val();
        var errorLog = $("#errorMail").text();
        if (emailAddress == '') { $("#toEmail_Error").empty().append("Please enter email address"); $("#toEmail_Error").removeClass('d-none').addClass('d-block'); return false; }
        else if (!pattern.test(emailAddress)) {
            $("#toEmail_Error").removeClass('d-none').addClass('d-block');
            $("#toEmail_Error").empty().append("Please enter valid email address");
            $("#toEmail").focus();
            return false;
        }
        if (AccountName == '') { $("#toAccountName_Error").empty().append("Please enter VSTS account name"); $("#toAccountName_Error").removeClass('d-none').addClass('d-block'); return false; }

        $("#sendEmail").prop('disabled', true);

        var modelData = { "EmailAddress": emailAddress, "AccountName": AccountName, "ErrorLog": errorLog };
        $.post("SendEmail", modelData, function (data) {
            $("#EmailModal").modal('hide');
            $("#infoModel").modal('show');
            $("#sendEmail").removeAttr("disabled");
        });
    });
    //checking for extenisoin start
    var isMicrosoftAgreement = "";
    var isThirdparty = "";
    $('#extensionError').click(function () {
        if (microsoft == "microsoft" && ThirdParty == "thirdparty") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isMicrosoftAgreement == "on" && isThirdparty == "on") {
                $("#btnSubmit").prop("disabled", false);
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true);
                isAgreedTerms = false;
            }
        }
        else if (microsoft == "microsoft" && ThirdParty == "") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isMicrosoftAgreement == "on") {
                $("#btnSubmit").prop("disabled", false);
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true);
                isAgreedTerms = false;

            }
        }
        else if (microsoft == "" && ThirdParty == "thirdparty") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isThirdparty == "on") {
                $("#btnSubmit").prop("disabled", false);
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true);
                isAgreedTerms = false;
            }
        }
    });

    $("#projectParameters").html('');
    var selectedTemplate = $("#ddlTemplates").val();

    if (selectedTemplate == "MyShuttle-Java") {
        $("#NotificationModal").modal('show');
    }
    if (selectedTemplate == "SonarQube") {
        $("#SoanrQubeDiv").show();
    }
    else {
        $("#SoanrQubeDiv").hide();
    }

    if (selectedTemplate != "") {
        $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").hide();
        var Url = 'GetTemplate/';
        $.get(Url, { "TemplateName": selectedTemplate }, function (data) {
            if (data != "") {
                var ParsedData = JSON.parse(data);
                var Description = ParsedData.Description;
                var parameters = ParsedData.Parameters;
                $("#btnSubmit").prop("disabled", false);
                if (Description != "") {
                    $("#descContainer").html('');
                    $("#descContainer").html(Description);
                    $("#lblDescription").removeClass("d-none").addClass("d-block");

                }
                else { $("#lblDescription").removeClass("d-block").addClass("d-none"); }
                if (parameters != undefined) {
                    if (parameters.length > 0) {
                        $.each(parameters, function (key, value) {
                            $('<div class="form-group row projParameters"><label for="sonarqubeurl" class="col-lg-4 col-form-label" style="font-weight:400">' + value.label + ':</label><div class="col-lg-8"><input type="text" class="form-control project-parameters rmverror" id="txt' + value.fieldName + '"  proj-parameter-name="' + value.fieldName + '" placeholder="' + value.fieldName + '"><div class="alert alert-danger d-none" role="alert" id="txt' + value.fieldName + '_Error"></div></div>').appendTo("#projectParameters");

                            // $('<div class="form-group"><label style="font-size:14px; width:30%;" class="col-sm-3 control-label">' + value.label + ':</label><div style="width:70%;" class="col-sm-4"><input type="text"  class ="form-control project-parameters"  id = "txt' + value.fieldName + '" proj-parameter-name="' + value.fieldName + '"  placeholder = "' + value.fieldName + '"></div></div>').appendTo("#projectParameters");
                        });
                        $("#projectParameters").show();
                    }
                    else { $("#projectParameters").html(''); }
                }
            }
            else {
                $("#lblDescription").removeClass("d-lock").addClass("d-none");
                $("#projectParameters").html('');

            }
        });
        if (selectedTemplate != "" && typeof selectedTemplate != "undefined") {
            checkForInstalledExtensions(selectedTemplate, function callBack(extensions) {

                if (extensions.message != "no extensions required" && extensions.message != "" && extensions.message != undefined && extensions.message.indexOf("Error") == -1 && extensions.message != "Template not found") {

                    $("#extensionError").empty().append(extensions.message);
                    $("#extensionError").show();
                    $("#lblextensionError").removeClass("d-none").addClass("d-block");

                    if (extensions.status != "true") {

                        $("#btnSubmit").prop("disabled", true);
                        isExtensionNeeded = true;
                        microsoft = $('#agreeTermsConditions').attr('placeholder');
                        if (microsoft != "microsoft") {
                            microsoft = "";
                        }
                        ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                        if (ThirdParty != "thirdparty") {
                            ThirdParty = "";
                        }

                    } else { $("#btnSubmit").prop("disabled", false); }
                }
                else { $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").removeClass("d-block").addClass("d-none"); $("#btnSubmit").prop("disabled", false); }

            });
        }
    }

    $(document).keypress(function (e) {
        if (e.which == 13) {
            $('#btnSubmit').click();
            return false;
        }
    });
});
$('#btnSubmit').click(function () {
    statusCount = 0;
    $("#txtALertContainer").hide();
    $('#status-messages').hide();
    $("#finalLink").removeClass("d-block").addClass("d-none");

    var projectName = $("#txtProjectName").val();
    var template = $("#ddlTemplates").val();
    //var groupSelected = $('#ddlGroups option:selected').text();
    var accountName = $('#ddlAcccountName option:selected').val();
    var token = $('#hiddenAccessToken').val();
    var regex = /^[A-Za-z0-9 -_]*[A-Za-z0-9][A-Za-z0-9 -_]*$/;
    if (template == "Octopus") {
        var octopusURL = $('#txtOctopusURL').val();
        var octopusAPIkey = $('#txtAPIkey').val();
        if (octopusURL != "") {
            var pattern = /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]\$&'\(\)\*\+,;=.]+$/

            if (!(pattern.test(octopusURL))) {
                $("#txtAlert").text("Please enter a valid URL.");
                $("#txtALertContainer").show();
                return false;
            }
        }
        else {
            $("#txtAlert").text("Please enter a valid URL.");
            $("#txtALertContainer").show();
            return false;
        }
        if (octopusAPIkey == "") {
            $("#txtAlert").text("Please enter a valid Octopus Key.");
            $("#txtALertContainer").show();
            return false;
        }
    }
    if (accountName == "" || accountName == "Select Account") {
        $("#ddlAcccountName_Error").text("Please choose an account first!");
        $("#ddlAcccountName_Error").removeClass("d-none").addClass("d-block");
        $("#ddlAcccountName").focus();
        return false;
    }
    //checking for session templatename and templateID


    if (projectName == "") {
        $("#txtProjectName_Error").text("Please provide a project name");
        $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
        return false;
    }
    if (!(regex.test(projectName))) {
        $("#txtAlert").text("Special characters are not allowed for project name");
        $("#txtALertContainer").show();
        $("#txtProjectName").focus();
        return false;
    }
    if (template == "") {
        $("#ddlTemplates_Error").text("Please select Project template");
        $("#ddlTemplates_Error").removeClass("d-none").addClass("d-block");
        return false;
    }
    if (template == "SonarQube") {
        var ServerDNS = $("#txtSonarServerDNSName").val();
        if (ServerDNS == "") {
            $("#txtSonarServerDNSName_Error").text("Please enter sonar server DNS name");
            $("#txtSonarServerDNSName_Error").removeClass("d-none").addClass("d-block");
            return false;
        }
        //var URLPattern = /^(http|https)?:\/\/[a-zA-Z0-9-\.]+\:[0-9]/;
        //if (!(URLPattern.test(ServerDNS))) {
        //    $("#txtAlert").text("Please enter valid Server DNS name");
        //    $("#txtSonarServerDNSName_Error").removeClass("d-none").addClass("d-block");
        //    return false;
        //}
    }

    //get userMethod and selected users
    var SelectedUsers = '';
    var userMethod = $("input[type='radio']:checked").val();
    if (userMethod == "Select") {
        $(".checkbox").each(function () {
            if (this.checked) {
                SelectedUsers = SelectedUsers + this.value + ',';
            }
        });

        if (SelectedUsers.length == 0) {
            $("#txtAlert").text("Please select account users");
            $("#txtALertContainer").show();
            return false;
        }
    }

    $('#status-messages').html('');
    $('#status-messages').show();

    var Parameters = {};
    $.each($('.project-parameters'), function (index, item) {
        Parameters[$("#" + item['id']).attr('proj-parameter-name')] = item["value"];
    });
    selectedTemplate = template;
    var websiteUrl = window.location.href;
    var projData = { "ProjectName": projectName, "SelectedTemplate": template, "id": uniqueId, "Parameters": Parameters, "selectedUsers": SelectedUsers, "UserMethod": userMethod, "SonarQubeDNS": ServerDNS, "isExtensionNeeded": isExtensionNeeded, "isAgreeTerms": isAgreedTerms, "websiteUrl": websiteUrl, "accountName": accountName, "accessToken": token };
    $.post("StartEnvironmentSetupProcess", projData, function (data) {

        if (data != "True") {
            var queryTemplate = '@Request.QueryString["queryTemplate"]';
            window.location.href = "~/Account/Verify?template=" + queryTemplate;
            return;
        }

        appInsights.trackEvent("Create button clicked");
        appInsights.trackEvent("Created project using" + selectedTemplate + " template");
        ga('send', 'event', selectedTemplate, 'selected');
        appInsights.trackEvent("User method" + userMethod);

        $('#ddlGroups').attr("disabled", "disabled");

        $("#ddlAcccountName").attr("disabled", "disabled");
        $("#txtProjectName").attr("disabled", "disabled");


        $("#templateselection").prop("disabled", true);
        $("#btnSubmit").prop("disabled", true);

        $("input.terms").attr("disabled", true);
        $("#txtALertContainer").hide();
        $("#accountLink").html('');
        $("#errorNotify").removeClass("d-block").addClass("d-none");
        projectNameForLink = projectName;
        ErrorData = '';
        getStatus();
        $('#dvProgress').removeClass("d-none").addClass("d-block");
        $('#textMuted').removeClass("d-none").addClass("d-block");
    });
    event.preventDefault;
});


function getStatus() {

    $.ajax({
        url: 'GetCurrentProgress/' + uniqueId,
        type: 'GET',
        success: function (data) {
            var isMessageShown = true;

            if (jQuery.inArray(data, messageList) == -1) {
                messageList.push(data);
                isMessageShown = false;
            }

            if (data != "100") {

                if (isMessageShown == false) {
                    if (messageList.length == 1) {
                        $('#progressBar').width(currentPercentage++ + '%');
                        if (data.length > 0) {
                            $('#status-messages').append('<i class="fas fa-clipboard-check"></i> &nbsp;' + data + '<br/>');
                        }
                    }
                    else {
                        if (data.indexOf("TF50309") == 0) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-clipboard-check"></i> &nbsp;' + data + '<br/>');
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#txtProjectName").val("");
                            $('#ddlAcccountName').prop('selectedIndex', 0);

                            $("#btnSubmit").prop("disabled", false);
                            $("#templateselection").prop("disabled", false);
                            $('#dvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            return;
                        }
                        else if (data.indexOf("TF200019") == 0) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-clipboard-check"></i> &nbsp;' + data + '<br/>');
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#txtProjectName").val("");
                            $('#ddlAcccountName').prop('selectedIndex', 0);

                            $("#btnSubmit").prop("disabled", false);
                            $("#templateselection").prop("disabled", false);
                            $('#dvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            return;

                        }
                        else if (data.indexOf("TF200019") == -1) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-clipboard-check"></i> &nbsp;' + data + '<br/>');
                        }
                        else {
                            $('#status-messages').append('<i class="fas fa-clipboard-check"></i> &nbsp;' + data + '<br/>');
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#txtProjectName").val("");
                            $('#ddlAcccountName').prop('selectedIndex', 0);

                            $("#btnSubmit").prop("disabled", false);
                            $("#templateselection").prop("disabled", false);
                            $('#dvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");

                        }

                    }

                }
                else if (currentPercentage <= ((messageList.length + 1) * percentForMessage) && currentPercentage <= 100) {
                    $('#progressBar').width(currentPercentage++ + '%');
                }
                window.setTimeout("getStatus()", 500);
            }
            else {
                if (messageList.length != 3) {
                    var ID = uniqueId + "_Errors";
                    var url2 = 'GetCurrentProgress/' + ID;
                    $.get(url2, function (response) {
                        if (response == "100" || response == "") {
                            $.ajax({
                                url: "../Account/GetAccountName/",
                                type: "GET",
                                async: false,
                                success: function (data) {
                                    var accountName = data;
                                    $("#projCreateMsg").hide();
                                    var link = "https://" + accountName + ".visualstudio.com/" + projectNameForLink;

                                    if (selectedTemplate == "SmartHotel360") {
                                        $('<b style="display: block;">Congratulations! Your project is successfully provisioned. Here is the URL to your project</b> <a href="' + link + '" target="_blank" style="font-weight:400;font-size:Medium;color:#0074d0">' + link + '</a><br><br><b>Note that the code for the SmartHotel360 project is not imported but being referred to the GitHub repo in the build definition. Before you run a release, you will first need to create an Azure service endpoint</b>').appendTo("#accountLink");
                                    }
                                    else {
                                        $('<b style="display: block;">Congratulations! Your project is successfully provisioned. Here is the URL to your project</b> <a href="' + link + '" target="_blank" style="font-weight:400;font-size:Medium;color:#0074d0">' + link + '</a>').appendTo("#accountLink");
                                    }
                                    $('#dvProgress').removeClass("d-block").addClass("d-none");
                                    $('#textMuted').removeClass("d-block").addClass("d-none");
                                    currentPercentage = 0;

                                    $('#progressBar').width(currentPercentage++ + '%');
                                    $("#finalLink").removeClass("d-none").addClass("d-block");
                                    $("#btnSubmit").prop("disabled", false);
                                    $("#txtProjectName").val("");

                                    $('#ddlAcccountName').prop('selectedIndex', 0);
                                    $("#templateselection").prop("disabled", false);

                                    $('#ddlGroups').removeAttr("disabled");
                                    $("#ddlAcccountName").removeAttr("disabled");
                                    $("#txtProjectName").removeAttr("disabled");
                                }
                            });
                        }
                        else {
                            ErrorData = response;
                            if (ErrorData != '') {
                                $("#projCreateMsg").hide();
                                $('<b style="display: block;">We ran into some issues and we are sorry about that!</b><p> The log below will provide you insights into why the provisioning failed. You can email us the log  to <a id="EmailPopup"><i>devopsdemos@microsoft.com</i></a> and we will try to help you.</p><p>Click on View Diagnostics button to share logs with us.</p>').appendTo("#errorDescription");
                                $('#dvProgress').removeClass("d-block").addClass("d-none");
                                $("#errorNotify").removeClass("d-none").addClass("d-block");
                                $('#textMuted').removeClass("d-block").addClass("d-none");

                                currentPercentage = 0;
                                $('#progressBar').width(currentPercentage++ + '%');
                                $("#errorMail").empty().append(ErrorData);
                                $("#errorNotify").show();

                                $("#btnSubmit").prop("disabled", false);
                                $("#txtProjectName").val("");
                                $('#ddlAcccountName').prop('selectedIndex', 0);

                                $("#templateselection").prop("disabled", false);

                                $('#ddlGroups').removeAttr("disabled");

                                $("#ddlAcccountName").removeAttr("disabled");
                                $("#txtProjectName").removeAttr("disabled");

                            }
                        }
                    });
                    messageList = [];
                }
                if ('@Request.QueryString["queryTemplate"]' == '') {

                    $('#ddlGroups').removeAttr("disabled");

                    $("#ddlAcccountName").removeAttr("disabled");
                    $("#txtProjectName").removeAttr("disabled");

                }

            };
        },
        error: function (xhr) {
            getStatus();
        }
    });
}

function DisplayErrors() {
    $("#errorBody").html('<pre>' + ErrorData + '</pre>');
    $("#errorModal").modal('show');
}


function checkForInstalledExtensions(selectedTemplate, callBack) {
    var accountNam = $('#ddlAcccountName option:selected').val();
    var Oauthtoken = $('#hiddenAccessToken').val();
    var selectedTemplate = selectedTemplate;
    if (accountNam != "" && selectedTemplate != "") {
        $("#btnSubmit").prop("disabled", true);

        $.ajax({
            url: "../Environment/CheckForInstalledExtensions",
            type: "GET",
            data: { selectedTemplate: selectedTemplate, token: Oauthtoken, Account: accountNam },
            success: function (InstalledExtensions) {

                callBack(InstalledExtensions);
            }
        });
    }

}

function checkForExtensions(callBack) {
    var accountNam = $('#ddlAcccountName option:selected').val();
    var Oauthtoken = $('#hiddenAccessToken').val();
    var selectedTemplate = $("#ddlTemplates").val();
    if (selectedTemplate != "" && accountNam != "") {
        $("#btnSubmit").prop("disabled", true);
        $.ajax({
            url: "../Environment/CheckForInstalledExtensions",
            type: "GET",
            data: { selectedTemplate: selectedTemplate, token: Oauthtoken, Account: accountNam },
            success: function (InstalledExtensions) {
                callBack(InstalledExtensions);
            }
        });
    }

}

function GetRequiredExtension() {
    checkForExtensions(function callBack(extensions) {
        if (extensions.message != "no extensions required" && extensions.message != "" && extensions.message != undefined && extensions.message.indexOf("Error") == -1 && extensions.message != "Template not found") {

            $("#extensionError").empty().append(extensions.message);
            $("#extensionError").show();
            $("#lblextensionError").removeClass("d-none").addClass("d-block");

            if (extensions.status != "true") {
                $("#btnSubmit").prop("disabled", true);
                isExtensionNeeded = true;
                microsoft = $('#agreeTermsConditions').attr('placeholder');
                if (microsoft != "microsoft") {
                    microsoft = "";
                }
                ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                if (ThirdParty != "thirdparty") {
                    ThirdParty = "";
                }
            } else { $("#btnSubmit").prop("disabled", false); }
        }
        else { $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").removeClass("d-block").addClass("d-none"); $("#btnSubmit").prop("disabled", false); }

    });
}

//TEMPLATE GROUP CREATION
$(document).ready(function () {
    createTemplates();

    $(document.body).on('click', '.nav-link', function () {
        grpSelected = this.text;
        $.ajax({
            url: "../Environment/GetGroups",
            type: "GET",
            success: function (groups) {
                var grp = "";
                if (groups.GroupwiseTemplates.length > 0) {
                    grp += '<div class="tab-pane show active" id="' + grpSelected + '" role="tabpanel" aria-labelledby="pills-' + grpSelected + '-tab">'
                    grp += '<div class="templates d-flex align-items-center flex-wrap">';
                    for (var g = 0; g < groups.GroupwiseTemplates.length; g++) {
                        if (groups.GroupwiseTemplates[g].Groups == grpSelected) {
                            var MatchedGroup = groups.GroupwiseTemplates[g];
                            for (var i = 0; i < MatchedGroup.Template.length; i++) {
                                if (i == 0) {
                                    grp += '<div class="template selected" data-template=' + MatchedGroup.Template[i].Name + '>';
                                    grp += '<div class="template-header"><i class="fas fa-file-code fa-4x"></i><strong class="title">' + MatchedGroup.Template[i].Name + '</strong></div>'
                                    grp += '<p class="description">' + MatchedGroup.Template[i].Description + '</p>';
                                    grp += '</div>';
                                }
                                else {
                                    grp += '<div class="template" data-template=' + MatchedGroup.Template[i].Name + '>';
                                    grp += '<div class="template-header"><i class="fas fa-file-code fa-4x"></i><strong class="title">' + MatchedGroup.Template[i].Name + '</strong></div>'
                                    grp += '<p class="description">' + MatchedGroup.Template[i].Description + '</p>';
                                    grp += '</div>';
                                }
                            }
                        }
                    }
                    grp += '</div></div>';

                    $('#pills-tabContent').html('').html(grp);
                }
            }
        });
    });

    //Group load
    $.ajax({
        url: "../Environment/GetGroups",
        type: "GET",
        success: function (groups) {
            var grp = "";
            if (groups.Groups.length > 0) {
                for (var g = 0; g < groups.Groups.length; g++) {
                    if (g == 0)
                        grp += '<li class="nav-item"><a class="nav-link active text-white" id="pills-' + groups.Groups[g] + '-tab" id="pills-' + groups.Groups[g] + '-tab" data-toggle="pill" href="#' + groups.Groups[g] + '" role="tab" aria-selected="true">' + groups.Groups[g] + '</a></li>'
                    else
                        grp += '<li class="nav-item"><a class="nav-link text-white" id="pills-' + groups.Groups[g] + '-tab" data-toggle="pill" href="#' + groups.Groups[g] + '" role="tab" aria-controls="pills-' + groups.Groups[g] + '" aria-selected="false">' + groups.Groups[g] + '</a></li>'
                }
                $('#modtemplateGroup').empty().append(grp);

            }
        }
    });

});

$(function () {

    // SHOW MODAL ON EVENT
    $(".template-invoke").on("click", function () {
        $(".VSTemplateSelection").fadeIn('fast');
    });

    // CLOSE MODAL ON EVENT
    $(".template-close").on("click", function () {
        $(".VSTemplateSelection").removeClass('d-block').addClass('d-none');
    });

    // TOGGLING ACTIVE CLASS TO TEMPLATE GROUP
    $(".template-group-item").on('click', function (e) {
        e.preventDefault();
        $(".template-group-item").removeClass('active');
        $(this).addClass('active');
    });

    // TOGGLING SELECTED CLASS TO TEMPLATE

    $(document.body).on("click", '.template', function () {
        $(".template").removeClass("selected");
        $(this).addClass("selected");

    });

    // GET ID TO BE SHOWN
    let showId = $(".template-group-item.active").attr('href');
    console.log(showId);
    $(`.template-body .templates${showId}`).show();
});

function createTemplates() {
    var grpSelected = "General"
    $.ajax({
        url: "../Environment/GetGroups",
        type: "GET",
        success: function (groups) {
            var grp = "";
            if (groups.GroupwiseTemplates.length > 0) {
                grp += '<div class="tab-pane show active" id="' + grpSelected + '" role="tabpanel" aria-labelledby="pills-' + grpSelected + '-tab">'
                grp += '<div class="templates d-flex align-items-center flex-wrap">';
                for (var g = 0; g < groups.GroupwiseTemplates.length; g++) {
                    if (groups.GroupwiseTemplates[g].Groups == grpSelected) {
                        var MatchedGroup = groups.GroupwiseTemplates[g];
                        for (var i = 0; i < MatchedGroup.Template.length; i++) {
                            if (i == 0) {
                                grp += '<div class="template selected" data-template=' + MatchedGroup.Template[i].Name + '>';
                                grp += '<div class="template-header"><i class="fas fa-file-code fa-4x"></i><strong class="title">' + MatchedGroup.Template[i].Name + '</strong></div>'
                                grp += '<p class="description">' + MatchedGroup.Template[i].Description + '</p>';
                                grp += '</div>';
                            }
                            else {
                                grp += '<div class="template" data-template=' + MatchedGroup.Template[i].Name + '>';
                                grp += '<div class="template-header"><i class="fas fa-file-code fa-4x"></i><strong class="title">' + MatchedGroup.Template[i].Name + '</strong></div>'
                                grp += '<p class="description">' + MatchedGroup.Template[i].Description + '</p>';
                                grp += '</div>';
                            }
                        }
                    }
                }
                grp += '</div></div>';

                $('#pills-tabContent').empty().append(grp);
            }
        }
    });
}

//Project name validtaion on keyup
$("#txtProjectName").keyup(function () {

    var projectName = this.value;
    //var regex1 = /^[a-zA-Z_!)(][a-zA-Z0-9_!)(]*(?:\s+[a-zA-Z!)(][a-zA-Z0-9!)(]+)?$/;
    //var regex2 = /^[a-zA-Z0-9!^\-`)( ]*[a-zA-Z0-9!^\-`_)(]*[^.\/\\~@#$*%+=[\]{\}'",:;?<>|](?:\s+[a-zA-Z!)(][a-zA-Z0-9!)(]+)?$/;

    var regex = /^[a-zA-Z0-9!^\-`)(]*[a-zA-Z0-9_!^\.)( ]*[^.\/\\~@#$*%+=[\]{\}'",:;?<>|](?:[a-zA-Z!)(][a-zA-Z0-9!^\-` )(]+)?$/;

    if (projectName != "") {

        if (!(regex.test(projectName))) {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            return false;
        }
        else if (projectName == "COM1" || projectName == "COM2" || projectName == "COM3" || projectName == "COM4" || projectName == "COM5" || projectName == "COM6" || projectName == "COM7" || projectName == "COM8" || projectName == "COM9" || projectName == "COM10") {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            return false;
        }
        else if (projectName == "PRN" || projectName == "LPT1" || projectName == "LPT2" || projectName == "LPT3" || projectName == "LPT4" || projectName == "LPT5" || projectName == "LPT6" || projectName == "LPT7" || projectName == "LPT8" || projectName == "LPT9") {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            return false;
        }
        else if (projectName == "NUL" || projectName == "CON" || projectName == "AUX" || projectName == "SERVER" || projectName == "SignalR" || projectName == "DefaultCollection" || projectName == "Web" || projectName == "App_code" || projectName == "App_Browsers" || projectName == "App_Data") {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            return false;
        }
        else if (projectName == "App_GlobalResources" || projectName == "App_LocalResources" || projectName == "App_Themes" || projectName == "App_WebResources" || projectName == "bin" || projectName == "web.config") {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            return false;
        }
        else {
            $("#txtProjectName_Error").text("");
            $("#txtProjectName_Error").removeClass("d-block").addClass("d-none");
            return false;
        }
    }
    else {
        $("#txtProjectName_Error").text("");
        $("#txtProjectName_Error").removeClass("d-block").addClass("d-none");
        return false;
    }
});

function GetTemplates(selectedTemplate) {
    var Url = 'GetTemplate/';
    $.get(Url, { "TemplateName": selectedTemplate }, function (data) {
        if (data != "") {
            var ParsedData = JSON.parse(data);
            var Description = ParsedData.Description;
            var parameters = ParsedData.Parameters;
            $("#btnSubmit").prop("disabled", false);
            if (Description != "") {
                $("#descContainer").html('');
                $("#descContainer").html(Description);
                $("#lblDescription").removeClass("d-none").addClass("d-block");

            }
            else { $("#lblDescription").removeClass("d-block").addClass("d-none"); }
            if (parameters != undefined) {
                if (parameters.length > 0) {
                    $.each(parameters, function (key, value) {
                        $('<div class="form-group row projParameters"><label for="sonarqubeurl" class="col-lg-4 col-form-label" style="font-weight:400">' + value.label + ':</label><div class="col-lg-8"><input type="text" class="form-control project-parameters rmverror" id="txt' + value.fieldName + '"  proj-parameter-name="' + value.fieldName + '" placeholder="' + value.fieldName + '"><div class="alert alert-danger d-none" role="alert" id="txt' + value.fieldName + '_Error"></div></div>').appendTo("#projectParameters");
                    });
                    $("#projectParameters").show();
                }
                else { $("#projectParameters").html(''); }
            }
        }
        else {
            $("#lblDescription").removeClass("d-lock").addClass("d-none");
            $("#projectParameters").html('');

        }
    });
}