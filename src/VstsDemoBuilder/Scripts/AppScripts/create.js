/// <reference path="../jquery-1.12.4.min.js" />
$(document).ready(function () {

    //$("#privateTemplatepop").removeClass('d-block').addClass('d-none');

    $('#buildYourTemplate').click(function () {
        ga('send', 'event', 'Build Your Template', 'visited');
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

    $("input[id=Random]").attr('disabled', true);
    $("input[id=Select]").attr('disabled', true);
    $("#btnUserShow").attr('disabled', true);

    ga('send', 'event', 'Create page', 'visited');
});

var messageList = [];
/**/
/**/
var ID = function () {
    return '_' + Math.random().toString(36).substr(2, 9);
};
var uniqueId = "";
/**/
/**/
var ErrorData = '';
var statusCount = 0;
var selectedTemplate = "";
var messagesCount = 8;
var percentForMessage = Math.floor(100 / messagesCount);
var currentPercentage = 0;
var projectNameForLink = '';
var isExtensionNeeded = false;
var isAgreedTerms = false;
var microsoft = "";
var ThirdParty = "";
var AccountNameForLink;
var templateFolder = "";
var publicTemplateMsg = "";
var privateTemplateMsg = "";

$(document).ready(function (event) {
    //REMOVE ERROR MESSAGE
    uniqueId = ID();
    $('.rmverror').click(function () {
        var errID = this.nextElementSibling.getAttribute('id');
        $('#' + errID).removeClass("d-block").addClass("d-none");
    });
    $('body').on('click', '.rmverrorOn', function () {
        var errID = this.nextElementSibling.getAttribute('id');
        $('#' + errID).removeClass("d-block").addClass("d-none");
    });

    //ON CLICK OF CHOOSE TEMPLATE BUTTON, SHOW THE TEMPLATE POPUP
    $('#templateselection').click(function () {
        $('.VSTemplateSelection').removeClass('d-none').addClass('d-block');
        $('#ddlTemplates_Error').removeClass("d-block").addClass("d-none");
        ga('send', 'event', 'Choose Template Button', 'Clicked');
        $(".template__block")[0].click();
    });

    //ON CHANGE OF ACCOUNT- VALIDATE EXTENSION
    $('#ddlAcccountName').change(function () {
        $('#status-messages').empty().hide();
        $('#textMuted').removeClass("d-block").addClass("d-none");
        $('#dvProgress').removeClass("d-block").addClass("d-none");
        $('#accountLink').empty();
        $('#finalLink').removeClass("d-block").addClass("d-none");
        $('#errorNotify').removeClass("d-block").addClass("d-none");
        // IF ACCOUNT NAME IS NOT SELECTED, IT WILL VALIDATE THE EXTENSION
        var accountNameToCheckExtension = $('#ddlAcccountName option:selected').val();
        var checkExtensionForSelectedTemplate = templateFolder;

        if (checkExtensionForSelectedTemplate === "SonarQube") {
            $("#SoanrQubeDiv").show();
        }
        else {
            $("#SoanrQubeDiv").hide();
        }
        if (accountNameToCheckExtension === "" || accountNameToCheckExtension === "Select Organiaztion") {
            return false;
        }
        else if (checkExtensionForSelectedTemplate === "") {
            return;
        }
        else {
            GetRequiredExtension();
        }
    });

    //ON CHANGE OF TEMPLATE- VALIDATE EXTENSION - ON CLICK OF Select Template BUTTOON
    $('#selecttmplate').click(function () {
        // related to temlate selection POPUP - modify this
        //.template will change to .template__block 
        $('#lblDefaultDescription').hide();
        var templateFolderSelected = $(".template__block.selected").data('folder'); // taking template folder name - appended from JSON
        var groputempSelected = $(".template__block.selected").data('template'); // taking template name - appended from JSON
        var selectedTemplateDescription = $(".description.descSelected").data('description'); // taking template description  - appended from JSON
        var infoMsg = $(".description.descSelected").data('message'); // taking info message - appended from JSON

        if (infoMsg === "" || typeof infoMsg === "undefined" || infoMsg === null) {
            $('#InfoMessage').html('');
            $('#InfoMessage').removeClass('d-block').addClass('d-none');
        }
        else {
            $('#InfoMessage').html(infoMsg);
            $('#InfoMessage').removeClass('d-none').addClass('d-block');
        }
        if (selectedTemplateDescription !== "") {
            $('#descContainer').html(selectedTemplateDescription);
        }
        else {
            $('#descContainer').html("Azure DevOps Demo Generator");
        }
        if (groputempSelected !== "") {
            templateFolder = templateFolderSelected;
            $('#ddlTemplates').val(groputempSelected);
            $(".VSTemplateSelection").fadeOut('fast');
        }
        $(".VSTemplateSelection").removeClass('d-block').addClass('d-none'); // hiding Popup
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
        $("#extensionError").html('');
        $("#extensionError").hide();
        $("#lblextensionError").removeClass("d-block").addClass("d-none");
        var TemplateName = templateFolder; // COPYING TEMPLATE FOLDER NAME FROM GLOBAL VARIABLE TO LOCAL VARIABLE

        if (TemplateName === "SonarQube") {
            $("#SoanrQubeDiv").show();
        }
        else {
            $("#SoanrQubeDiv").hide();
        }
        // binding projecct parameters
        GetTemplateParameters(TemplateName);
        if (TemplateName !== "") {
            checkForInstalledExtensions(TemplateName, function callBack(extensions) {
                if (extensions.message !== "no extensions required" && extensions.message !== "" && typeof extensions.message !== undefined && extensions.message.indexOf("Error") === -1 && extensions.message !== "Template not found") {

                    $("#extensionError").empty().append(extensions.message);
                    $("#extensionError").show();
                    $("#lblextensionError").removeClass("d-none").addClass("d-block");

                    if (extensions.status !== "true") {
                        $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
                        isExtensionNeeded = true;
                        microsoft = $('#agreeTermsConditions').attr('placeholder');
                        if (microsoft !== "microsoft") {
                            microsoft = "";
                        }
                        ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                        if (ThirdParty !== "thirdparty") {
                            ThirdParty = "";
                        }
                    } else { $("#btnSubmit").prop("disabled", false).addClass('btn-primary'); }
                }
                else {
                    $("#extensionError").html('');
                    $("#extensionError").hide();
                    $("#lblextensionError").removeClass("d-block").addClass("d-none");
                    $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                }

            });
        }
        // taking accout name to check for installed extension.
        var accountNameToCheckExtension = $('#ddlAcccountName option:selected').val();
        var checkExtensionsForSelectedTemplate = templateFolder;
        ga('send', 'event', 'Selected Template : ', checkExtensionsForSelectedTemplate);
        if (accountNameToCheckExtension === "" || accountNameToCheckExtension === "--select organiaztion--") {
            return false;
        }
        else if (checkExtensionsForSelectedTemplate === "") {
            return;
        }
        else {
            GetRequiredExtension();
        }

    });

    $("body").on("click", "#EmailPopup", function () {
        $("#EmailModal").modal('show');
    });

    //checking for extenisoin
    // START - validating extension installation agreement checkbox- based on Microsoft and Third party category
    var isMicrosoftAgreement = "";
    var isThirdparty = "";
    $('#extensionError').click(function () {
        if (microsoft === "microsoft" && ThirdParty === "thirdparty") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isMicrosoftAgreement === "on" && isThirdparty === "on") {
                $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
                isAgreedTerms = false;
            }
        }
        else if (microsoft === "microsoft" && ThirdParty === "") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isMicrosoftAgreement === "on") {
                $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
                isAgreedTerms = false;

            }
        }
        else if (microsoft === "" && ThirdParty === "thirdparty") {
            isMicrosoftAgreement = $('input[id=agreeTermsConditions]:checked').val();
            isThirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
            if (isThirdparty === "on") {
                $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                isAgreedTerms = true;
            }
            else {
                $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
                isAgreedTerms = false;
            }
        }
    });
    // END - validating extension installation agreement checkbox- based on Microsoft and Third party category

    $("#projectParameters").html('');
    var selectedTemplate = templateFolder; // COPYING TEMPLATE FOLDER NAME FROM GLOBAL VARIABLE TO LOCAL VARIABLE

    if (selectedTemplate === "SonarQube") {
        $("#SoanrQubeDiv").show();
    }
    else {
        $("#SoanrQubeDiv").hide();
    }

    if (selectedTemplate !== "") {
        $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").hide();
        var Url = 'GetTemplate/';
        // CHECKING FOR TEMPLATE PATAMETERS
        GetTemplateParameters(selectedTemplate);
        // IF TEMPLATE IS NOT NULL OF UNDEFINED, CHECKING FOR EXTENSION AND CATEGORIZING INTO MICROSOFT AND THIRD PARTY
        if (selectedTemplate !== "" && typeof selectedTemplate !== "undefined") {
            checkForInstalledExtensions(selectedTemplate, function callBack(extensions) {

                if (extensions.message !== "no extensions required" && extensions.message !== "" && typeof extensions.message !== "undefined" && extensions.message.indexOf("Error") === -1 && extensions.message !== "Template not found") {

                    $("#extensionError").empty().append(extensions.message);
                    $("#extensionError").show();
                    $("#lblextensionError").removeClass("d-none").addClass("d-block");

                    if (extensions.status !== "true") {

                        $("#btnSubmit").prop("disabled", true).addClass('btn-primary');
                        isExtensionNeeded = true;
                        microsoft = $('#agreeTermsConditions').attr('placeholder');
                        if (microsoft !== "microsoft") {
                            microsoft = "";
                        }
                        ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                        if (ThirdParty !== "thirdparty") {
                            ThirdParty = "";
                        }

                    } else { $("#btnSubmit").prop("disabled", false).addClass('btn-primary'); }
                }
                else {
                    $("#extensionError").html('');
                    $("#extensionError").hide();
                    $("#lblextensionError").removeClass("d-block").addClass("d-none");
                    $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                }

            });
        }
    }

    $(document).keypress(function (e) {
        if (e.which === 13) {
            $('#btnSubmit').click();
            return false;
        }
    });
    // for private templates AND when the user directly comes form template URL
    // TAKING PRIVATE TEMPLATE DESCRIPTION FROM MODEL - SINCE  these templates are not availabe in selection popup
    var privateTemplateDescription = $('#selectedTemplateDescription').val();
    if (privateTemplateDescription !== "") {
        var templateTxt = $('#descContainer').text();
        if (templateTxt !== "")
            $("#descContainer").html(privateTemplateDescription);
    }
    //If User comes with lab url(private), we will check for PrivatetemplateFolderName in the field
    var publicTemplate = $('#ddlTemplates').val();
    var privateTemplate = $('#selectedTemplateFolder').val(); // taking private template folder name from model
    if (privateTemplate !== "" || typeof privateTemplate !== "undefined") {
        templateFolder = privateTemplate;
    }
    else {
        templateFolder = publicTemplate;
    }
    // add private template info message to info box
    AppendMessage();
    var defaultTemplate = $('#selectedTemplate').val();
    $('#ddlTemplates').val(defaultTemplate);

});
// on click of submit button, validates project name, account name and the selected template
$('#btnSubmit').click(function () {
    statusCount = 0;
    $("#txtALertContainer").hide();
    $('#status-messages').hide();
    $("#finalLink").removeClass("d-block").addClass("d-none");

    var projectName = $.trim($("#txtProjectName").val());
    var template = templateFolder;
    var accountName = $('#ddlAcccountName option:selected').val();
    var token = $('#hiddenAccessToken').val();
    var email = $('#emailID').val();
    var regex = /^[A-Za-z0-9 -_]*[A-Za-z0-9][A-Za-z0-9 -_]*$/;
    if (accountName === "" || accountName === "Select Organiaztion") {
        $("#ddlAcccountName_Error").text("Please choose an organization first!");
        $("#ddlAcccountName_Error").removeClass("d-none").addClass("d-block");
        $("#ddlAcccountName").focus();
        return false;
    }
    if (projectName === "") {
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
    if (template === "") {
        $("#ddlTemplates_Error").text("Please select Project template");
        $("#ddlTemplates_Error").removeClass("d-none").addClass("d-block");
        return false;
    }
    if (template === "Octopus") {
        var octopusURL = $('#txtOctopusURL').val();
        var octopusAPIkey = $('#txtAPIkey').val();
        if (octopusURL !== "") {
            var pattern = /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]\$&'\(\)\*\+,;=.]+$/;

            if (!(pattern.test(octopusURL))) {
                $("#txtOctopusURL_Error").text("Please enter a valid URL.");
                $("#txtOctopusURL_Error").removeClass("d-none").addClass("d-block");
                return false;
            }
        }
        else {
            $("#txtOctopusURL_Error").text("Please enter a valid URL.");
            $("#txtOctopusURL_Error").removeClass("d-none").addClass("d-block");
            return false;
        }
        if (octopusAPIkey === "") {
            $("#txtAPIkey_Error").text("Please enter a valid Octopus Key.");
            $("#txtAPIkey_Error").removeClass("d-none").addClass("d-block");
            return false;
        }
    }

    if (template === "SonarQube") {
        var ServerDNS = $("#txtSonarServerDNSName").val();
        if (ServerDNS === "") {
            $("#txtSonarServerDNSName_Error").text("Please enter sonar server DNS name");
            $("#txtSonarServerDNSName_Error").removeClass("d-none").addClass("d-block");
            return false;
        }
    }

    $('#status-messages').html('');
    $('#status-messages').show();
    $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
    var Parameters = {};
    $.each($('.project-parameters'), function (index, item) {
        Parameters[$("#" + item['id']).attr('proj-parameter-name')] = item["value"];
    });
    selectedTemplate = template;
    var websiteUrl = window.location.href;
    var projData = { "ProjectName": projectName, "SelectedTemplate": template, "id": uniqueId, "Parameters": Parameters, "SonarQubeDNS": ServerDNS, "isExtensionNeeded": isExtensionNeeded, "isAgreeTerms": isAgreedTerms, "websiteUrl": websiteUrl, "accountName": accountName, "accessToken": token, "email": email };
    $.post("StartEnvironmentSetupProcess", projData, function (data) {

        if (data !== "True") {
            var queryTemplate = '@Request.QueryString["queryTemplate"]';
            window.location.href = "~/Account/Verify?template=" + queryTemplate;
            return;
        }

        appInsights.trackEvent("Create button clicked");
        appInsights.trackEvent("Created project using" + selectedTemplate + " template");
        ga('send', 'event', selectedTemplate, 'selected');
        $('#ddlGroups').attr("disabled", "disabled");

        $("#ddlAcccountName").attr("disabled", "disabled");
        $("#txtProjectName").attr("disabled", "disabled");
        $("#templateselection").prop("disabled", true);
        $("input.terms").attr("disabled", true);
        $("#txtALertContainer").hide();
        $("#accountLink").html('');
        $("#errorNotify").removeClass("d-block").addClass("d-none");
        projectNameForLink = projectName;
        AccountNameForLink = accountName;
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

            if (data === "OAUTHACCESSDENIED") {
                $('#progressBar').width(currentPercentage++ + '%');
                $('#status-messages').append('<i class="fas fa-forward"></i> &nbsp;Third Party application access via OAuth is disabled for this Organization,please change OAuth access setting and try again!<br/>');
                $("#ddlAcccountName").removeAttr("disabled");
                $("#txtProjectName").removeAttr("disabled");
                $("#txtProjectName").val("");
                $('#ddlAcccountName').prop('selectedIndex', 0);

                $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                $("#templateselection").prop("disabled", false);
                $('#dvProgress').removeClass("d-block").addClass("d-none");
                $('#textMuted').removeClass("d-block").addClass("d-none");
                return;

            }
            var isMessageShown = true;

            if (jQuery.inArray(data, messageList) === -1) {
                messageList.push(data);
                isMessageShown = false;
            }

            if (data !== "100") {

                if (isMessageShown === false) {
                    if (messageList.length === 1) {
                        $('#progressBar').width(currentPercentage++ + '%');
                        if (data.length > 0) {
                            $('#status-messages').append('<i class="fas fa-check-circle" style="color:green"></i> &nbsp;' + data + '<br/>');
                        }
                    }
                    else {
                        if (data.indexOf("TF50309") === 0) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-check-circle" style="color:green"></i> &nbsp;' + data + '<br/>');
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
                        else if (data.indexOf("TF200019") === 0) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-check-circle" style="color:green"></i> &nbsp;' + data + '<br/>');
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#txtProjectName").val("");
                            $('#ddlAcccountName').prop('selectedIndex', 0);

                            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                            $("#templateselection").prop("disabled", false);
                            $('#dvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            return;

                        }
                        else if (data.indexOf("TF200019") === -1) {
                            $('#progressBar').width(currentPercentage++ + '%');
                            $('#status-messages').append('<i class="fas fa-check-circle" style="color:green"></i> &nbsp;' + data + '<br/>');
                        }
                        else {
                            $('#status-messages').append('<i class="fas fa-check-circle" style="color:green"></i> &nbsp;' + data + '<br/>');
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            $("#txtProjectName").val("");
                            $('#ddlAcccountName').prop('selectedIndex', 0);

                            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
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
                if (messageList.length !== 3) {
                    var ID = uniqueId + "_Errors";
                    var url2 = 'GetCurrentProgress/' + ID;
                    $.get(url2, function (response) {
                        if (response === "100" || response === "") {
                            $.ajax({
                                url: "../Account/GetAccountName/",
                                type: "GET",
                                async: false,
                                success: function (data) {
                                    var accountName = $('#ddlAcccountName option:selected').val();
                                    var projectNameForLink = $("#txtProjectName").val();
                                    var link = "https://dev.azure.com/" + accountName + "/" + projectNameForLink;
                                    var proceedOrg = "<a href='" + link + "' target='_blank'><button type = 'button' class='btn btn-primary btn-sm' id = 'proceedOrg' style = 'margin: 5px;'> Navigate to project</button></a>";
                                    var social = "<p style='color: black; font-weight: 500; margin: 0px;'>Like the tool? Share your feedback &nbsp;";
                                    social += "<script>function fbs_click() { u = 'https://azuredevopsdemogenerator.azurewebsites.net/'; t = +Azure + DevOps + Demo + Generator & window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }</script>";
                                    var twitter = "<a href='https://twitter.com/intent/tweet?url=https://azuredevopsdemogenerator.azurewebsites.net/&amp;text=Azure+DevOps+Demo+Generator&amp;hashtags=azuredevopsdemogenerator' target='_blank'><img src='/Images/twitter.png' style='width:20px;'></a>&nbsp;&nbsp;";
                                    social += twitter;
                                    $('<b style="display: block;">Congratulations! Your project is successfully provisioned.</b>' + proceedOrg + social).appendTo("#accountLink");
                                    $('#dvProgress').removeClass("d-block").addClass("d-none");
                                    $('#textMuted').removeClass("d-block").addClass("d-none");
                                    currentPercentage = 0;

                                    $('#progressBar').width(currentPercentage++ + '%');
                                    $("#finalLink").removeClass("d-none").addClass("d-block");
                                    $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
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
                            var accountName = $('#ddlAcccountName option:selected').val();
                            $("#projCreateMsg").hide();
                            //var link = "https://dev.azure.com/" + accountName + "/" + projectNameForLink;

                            //if (selectedTemplate == "SmartHotel360") {
                            //    $('<b style="display: block;">Congratulations! Your project is successfully provisioned. Here is the URL to your project</b> <a href="' + link + '" target="_blank" style="font-weight:400;font-size:Medium;color:#0074d0">' + link + '</a><br><br><b>Note that the code for the SmartHotel360 project is not imported but being referred to the GitHub repo in the build definition. Before you run a release, you will first need to create an Azure service endpoint</b>').appendTo("#accountLink");
                            //}
                            //else {
                            //    $('<b style="display: block;">Congratulations! Your project is successfully provisioned. Here is the URL to your project</b> <a href="' + link + '" target="_blank" style="font-weight:400;font-size:Medium;color:#0074d0">' + link + '</a>').appendTo("#accountLink");
                            //}
                            $('#dvProgress').removeClass("d-block").addClass("d-none");
                            $('#textMuted').removeClass("d-block").addClass("d-none");
                            currentPercentage = 0;
                            $('#status-messages').empty().hide();
                            $('#progressBar').width(currentPercentage++ + '%');
                            $("#finalLink").addClass("d-none").removeClass("d-block");
                            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
                            $("#txtProjectName").val("");

                            $('#ddlAcccountName').prop('selectedIndex', 0);
                            $("#templateselection").prop("disabled", false);
                            $('#ddlGroups').removeAttr("disabled");
                            $("#ddlAcccountName").removeAttr("disabled");
                            $("#txtProjectName").removeAttr("disabled");
                            if (ErrorData !== '') {
                                $("#projCreateMsg").hide(); $("#errorDescription").html("");
                                $('<b style="display: block;">We ran into some issues and we are sorry about that!</b><p> The log below will provide you insights into why the provisioning failed. You can email us the log  to <a id="EmailPopup"><i>devopsdemos@microsoft.com</i></a> and we will try to help you.</p><p>Click on View Diagnostics button to share logs with us.</p>').appendTo("#errorDescription");
                                $("#errorMail").empty().append(ErrorData);
                                $("#errorNotify").show();
                                $("#errorNotify").removeClass("d-none").addClass("d-block");
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
// check for installed extensions
function checkForInstalledExtensions(selectedTemplate, callBack) {
    var accountNam = $('#ddlAcccountName option:selected').val();
    var Oauthtoken = $('#hiddenAccessToken').val();
    if (accountNam !== "" && selectedTemplate !== "") {
        $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');

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
    var selectedTemplate = templateFolder;
    if (selectedTemplate !== "" && accountNam !== "") {
        $("#imgLoading").show();
        $("#btnSubmit").removeClass('btn-primary').prop("disabled", true);
        $("#ddlAcccountName").prop("disabled", true);
        $("#txtProjectName").prop('disabled', 'disabled');

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
        if (extensions.message !== "no extensions required" && extensions.message !== "" && typeof extensions.message !== "undefined" && extensions.message.indexOf("Error") === -1 && extensions.message !== "Template not found") {
            $("#imgLoading").hide();
            $("#ddlAcccountName").prop("disabled", false);
            $("#extensionError").empty().append(extensions.message);
            $("#extensionError").show();
            $("#lblextensionError").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").prop('disabled', false);
            if (extensions.status !== "true") {
                $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');

                isExtensionNeeded = true;
                microsoft = $('#agreeTermsConditions').attr('placeholder');
                if (microsoft !== "microsoft") {
                    microsoft = "";
                }
                ThirdParty = $('#ThirdPartyagreeTermsConditions').attr('placeholder');
                if (ThirdParty !== "thirdparty") {
                    ThirdParty = "";
                }
            } else { $("#btnSubmit").prop("disabled", false).addClass('btn-primary'); microsoft = ""; ThirdParty = ""; }
        }
        else { $("#imgLoading").hide(); $("#ddlAcccountName").prop("disabled", false); $("#extensionError").html(''); $("#extensionError").hide(); $("#lblextensionError").removeClass("d-block").addClass("d-none"); $("#btnSubmit").addClass('btn-primary').prop("disabled", false); $("#txtProjectName").prop('disabled', false); microsoft = ""; ThirdParty = ""; }

    });
}

//TEMPLATE GROUP CREATION - modify this
$(document).ready(function () {
    createTemplatesNew();

    $(document.body).on('click', '.nav-link', function () {
        grpSelected = this.text;
        LoadTemplates(grpSelected);
    });

    //Group load
    $.ajax({
        url: "../Environment/GetGroups",
        type: "GET",
        success: function (groups) {
            var grp = "";
            if (groups.Groups.length > 0) {
                for (var g = 0; g < groups.Groups.length; g++) {
                    if (g === 0)
                        grp += '<li class="nav-item"><a class="nav-link active text-white" id="pills-' + groups.Groups[g] + '-tab" id="pills-' + groups.Groups[g] + '-tab" data-toggle="pill" href="#' + groups.Groups[g] + '" role="tab" aria-selected="true">' + groups.Groups[g] + '</a></li>'
                    else
                        grp += '<li class="nav-item"><a class="nav-link text-white" id="pills-' + groups.Groups[g] + '-tab" data-toggle="pill" href="#' + groups.Groups[g] + '" role="tab" aria-controls="pills-' + groups.Groups[g] + '" aria-selected="false">' + groups.Groups[g] + '</a></li>'
                }
                $('#modtemplateGroup').empty().append(grp);

            }
        }
    });

});

// template selection 
$(function () {
    let length = $(".template").length;
    for (let i = 0; i < length; i++) {
        $(".template").eq(i).css({
            animationDelay: "0." + i + "s"
        });
    }

    $(".template-invoke").on("click", function () {
        $(".VSTemplateSelection").fadeIn('fast');
    });
    $(".template-close").on("click", function () {
        $(".VSTemplateSelection").removeClass('d-block').addClass('d-none');
    });

    $(".template-group-item").on('click', function (e) {
        e.preventDefault();
        $(".template-group-item").removeClass('active');
        $(this).addClass('active');
    });

    $(document).on('mouseover', ".selected__preview img", function () {
        let src = $(this).attr("src");
        $(".preview__image .img__preview").attr('src', src);
        $(".preview__image").removeClass("d-none");
        $(".template__block").addClass('blur');
    });

    $(document).on('mouseout', ".selected__preview img", function () {
        $(".preview__image").addClass("d-none");
        $(".template__block").removeClass('blur');
    });
    

    $(document).on('click', '.template__block', function () {
        $(".selected__preview").empty();
        $(".selected__preview").addClass("d-none");
        $(".selected").removeClass("selected");
        $(this).addClass("selected");
        let selectedTitle = $(this).find("h4").text();
        let selectedDesc = $(this).data('description');
        $(".selected__title").text(selectedTitle);
        $(".selected__desc").html(selectedDesc);

        $('.description').removeClass('descSelected');
        $(this.lastElementChild).addClass('descSelected');

        let selectedImages = $(this).data('images').split(',');
        if (selectedImages !== null && selectedImages !== "") {
            $(".selected__preview").removeClass("d-none");
            let imagePreviews;
            for (image of selectedImages) {
                const imgElement = $("<img>", {
                    src: image,
                    class: "template_image"
                });
                $(".selected__preview").append(imgElement);
            }
        }
    });
});

function createTemplatesNew() {
    var grpSelected = "General";
    LoadTemplates(grpSelected);
}


//Project name validtaion on keyup
$("#txtProjectName").keyup(function () {

    var projectName = $.trim(this.value);
    var regex = /^(?!_.)[a-zA-Z0-9!^\-`)(]*[a-zA-Z0-9_!^\.)( ]*[^.\/\\~@#$*%+=[\]{\}'",:;?<>|](?:[a-zA-Z!)(][a-zA-Z0-9!^\-` )(]+)?$/;
    if (projectName !== "") {
        var restrictedNames = ["COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "PRN", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LTP", "LTP8", "LTP9", "NUL", "CON", "AUX", "SERVER", "SignalR", "DefaultCollection", "Web", "App_code", "App_Browesers", "App_Data", "App_GlobalResources", "App_LocalResources", "App_Themes", "App_WebResources", "bin", "web.config"];
        if (restrictedNames.find(x => x.toLowerCase() === projectName.toLowerCase())) {
            var link = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + link);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            $('#btnSubmit').removeClass('btn-primary').attr('disabled', 'disabled');
            return false;
        }
        else {
            validateExtensionCheckbox();
        }
        if (!(regex.test(projectName))) {
            var links = "<a href='https://go.microsoft.com/fwlink/?linkid=842564' target='_blank'>Learn more</a>";
            $("#txtProjectName_Error").html("The project name '" + projectName + "' is invalid " + links);
            $("#txtProjectName_Error").removeClass("d-none").addClass("d-block");
            $("#txtProjectName").focus();
            $('#btnSubmit').removeClass('btn-primary').attr('disabled', 'disabled');
            return false;
        }
        else {
            validateExtensionCheckbox();
            return false;
        }
    }
    else {
        $("#txtProjectName_Error").text("");
        $("#txtProjectName_Error").removeClass("d-block").addClass("d-none");
        $('#btnSubmit').addClass('btn-primary').attr('disabled', false);
        return false;
    }
});

// validate the extension agreement checkboxes
function validateExtensionCheckbox() {
    var checkboxMicrosoft = "";
    var checkboxTrirdparty = "";

    $("#txtProjectName_Error").text("");
    $("#txtProjectName_Error").removeClass("d-block").addClass("d-none");

    if (microsoft === "microsoft" && ThirdParty === "thirdparty") {
        checkboxMicrosoft = $('input[id=agreeTermsConditions]:checked').val();
        checkboxTrirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
        if (checkboxMicrosoft === "on" && checkboxTrirdparty === "on") {
            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
            isAgreedTerms = true;
        }
        else {
            $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
            isAgreedTerms = false;
        }
    }
    else if (microsoft === "microsoft" && ThirdParty === "") {
        checkboxMicrosoft = $('input[id=agreeTermsConditions]:checked').val();
        checkboxTrirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
        if (checkboxMicrosoft === "on") {
            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
            isAgreedTerms = true;
        }
        else {
            $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
            isAgreedTerms = false;

        }
    }
    else if (microsoft === "" && ThirdParty === "thirdparty") {
        checkboxMicrosoft = $('input[id=agreeTermsConditions]:checked').val();
        checkboxTrirdparty = $('input[id=ThirdPartyagreeTermsConditions]:checked').val();
        if (checkboxTrirdparty === "on") {
            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
            isAgreedTerms = true;
        }
        else {
            $("#btnSubmit").prop("disabled", true).removeClass('btn-primary');
            isAgreedTerms = false;
        }
    }
}

// Get Template parameters
function GetTemplateParameters(selectedTemplate) {
    var Url = 'GetTemplate/';
    $.get(Url, { "TemplateName": selectedTemplate }, function (data) {
        if (data !== "") {
            var ParsedData = JSON.parse(data);
            var Description = ParsedData.Description;
            var parameters = ParsedData.Parameters;
            $("#btnSubmit").prop("disabled", false).addClass('btn-primary');
            if (typeof parameters !== "undefined") {
                if (parameters.length > 0) {
                    $.each(parameters, function (key, value) {
                        $('<div class="form-group row projParameters"><label for="sonarqubeurl" class="col-lg-3 col-form-label" style="font-weight:400">' + value.label + ':</label><div class="col-lg-8"><input type="text" class="form-control project-parameters rmverrorOn" id="txt' + value.fieldName + '"  proj-parameter-name="' + value.fieldName + '" placeholder="' + value.fieldName + '"><div class="alert alert-danger d-none" role="alert" id="txt' + value.fieldName + '_Error"></div></div>').appendTo("#projectParameters");
                    });
                    $("#projectParameters").show();
                }
                else { $("#projectParameters").html(''); }
            }
        }
    });
}

function openImportPopUp() {
    $("#privateTemplatepop").removeClass('d-none').addClass('d-block');
}

function AppendMessage() {

    privateTemplateMsg = $('#infoMessageTxt').val(); // taking infor message form model

    if (privateTemplateMsg !== "" && privateTemplateMsg !== null && typeof privateTemplateMsg !== "undefined") {
        $('#InfoMessage').html(privateTemplateMsg);
        $('#InfoMessage').removeClass('d-none').addClass('d-block');
    }
    else {
        $('#InfoMessage').html('');
        $('#InfoMessage').removeClass('d-block').addClass('d-none');
    }
}

function LoadTemplates(grpSelected) {
    $.ajax({
        url: "../Environment/GetGroups",
        type: "GET",
        success: function (groups) {
            var grp = "";
            if (groups.GroupwiseTemplates.length > 0) {
                console.log(groups);
                for (var g = 0; g < groups.GroupwiseTemplates.length; g++) {
                    if (groups.GroupwiseTemplates[g].Groups === grpSelected) {
                        var MatchedGroup = groups.GroupwiseTemplates[g];
                        for (var i = 0; i < MatchedGroup.Template.length; i++) {
                            if (i === 0) {
                                var templateImg = MatchedGroup.Template[i].Image;
                                if (templateImg === "" || templateImg === null) {
                                    templateImg = "/Templates/TemplateImages/CodeFile.png";
                                }
                                console.log(MatchedGroup.Template[i]);
                                var imgList = MatchedGroup.Template[i].PreviewImages ? MatchedGroup.Template[i].PreviewImages.join(',') : '';

                                grp += '<div class="template__block selected" data-images="' + imgList + '" data-template="' + MatchedGroup.Template[i].Name + '" data-folder="' + MatchedGroup.Template[i].TemplateFolder + '" data-description="' + MatchedGroup.Template[i].Description + '">';
                                grp += '<div class="d-flex align-items-center">';
                                grp += '<div class="template__logo">';
                                grp += '<img src="' + templateImg + '"/></div>';
                                grp += '<div class="template__intro">';
                                grp += '<h4>' + MatchedGroup.Template[i].Name + '</h4>';

                                if (MatchedGroup.Template[i].Tags !== null) {
                                    for (var l = 0; l < MatchedGroup.Template[i].Tags.length; l++) {
                                        grp += '<span>' + MatchedGroup.Template[i].Tags[l] + '</span>';
                                    }
                                }
                                grp += '</div></div>';

                                grp += '<p class="template__block__description description descSelected" data-description="' + MatchedGroup.Template[i].Description + '" data-message="' + MatchedGroup.Template[i].Message + '">' + MatchedGroup.Template[i].Description + '</p>';
                                grp += '</div>';
                                if (MatchedGroup.Template[i].Name === "SmartHotel360") {
                                    var templateTxt = $('#selectedTemplateDescription').val();
                                    if (templateTxt === "" || typeof templateTxt === "undefined")
                                        $('#descContainer').html(MatchedGroup.Template[i].Description);
                                }
                                $(".selected__title").text(MatchedGroup.Template[i].Name);
                                $(".selected__desc").html(MatchedGroup.Template[i].Description);
                            }
                            else {
                                var templateImgs = MatchedGroup.Template[i].Image;
                                if (templateImgs === "" || templateImgs === null) {
                                    templateImgs = "/Templates/TemplateImages/CodeFile.png";
                                }
                                var imgLists = MatchedGroup.Template[i].PreviewImages ? MatchedGroup.Template[i].PreviewImages.join(',') : '';

                                grp += '<div class="template__block" data-images="' + imgLists + '" data-template="' + MatchedGroup.Template[i].Name + '" data-folder="' + MatchedGroup.Template[i].TemplateFolder + '" data-description="' + MatchedGroup.Template[i].Description + '">';

                                grp += '<div class="d-flex align-items-center">';
                                grp += '<div class="template__logo">';
                                grp += '<img src="' + templateImgs + '"/></div>';
                                grp += '<div class="template__intro">';
                                grp += '<h4>' + MatchedGroup.Template[i].Name + '</h4>';
                                if (MatchedGroup.Template[i].Tags !== null) {
                                    for (var m = 0; m < MatchedGroup.Template[i].Tags.length; m++) {
                                        grp += '<span>' + MatchedGroup.Template[i].Tags[m] + '</span>';
                                    }
                                }
                                grp += '</div></div>';

                                grp += '<p class="template__block__description description" data-description="' + MatchedGroup.Template[i].Description + '" data-message="' + MatchedGroup.Template[i].Message + '">' + MatchedGroup.Template[i].Description + '</p>';
                                grp += '</div>';
                                if (MatchedGroup.Template[i].Name === "SmartHotel360") {
                                    var templateTxtx = $('#selectedTemplateDescription').val();
                                    if (templateTxtx === "" || typeof templateTxt === "undefined")
                                        $('#descContainer').html(MatchedGroup.Template[i].Description);
                                }
                            }
                        }
                    }
                }
                $('#templates__list').empty().append(grp);
            }
        }
    });

}
// if the user uploading his exported template (zip file), we will take that template name as folder name
$('body').on('click', '#btnUpload', function () {
    var fileUpload = $("#FileUpload1").get(0);
    var files = fileUpload.files;
    $('#InfoMessage').removeClass('d-block').addClass('d-none');
    // Create FormData object
    var fileData = new FormData();
    // Looping over all files and add it to FormData object
    for (var i = 0; i < files.length; i++) {
        fileData.append(files[i].name, files[i]);
    }
    templateFolder = files[0].name.replace(".zip", "");
});