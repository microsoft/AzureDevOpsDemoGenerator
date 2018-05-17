$(document).ready(function () {

    var grpSelected = $(".active").attr("href");
    console.log(grpSelected);
    createTemplates(grpSelected);

    $(".nav-link").on("click", function () {
        debugger;
        grpSelected = $(this).attr("href");
        console.log(grpSelected);
        $.ajax({
            url: "../Environment/GetGroups",
            type: "GET",
            success: function (groups) {
                debugger;
                var grp = "";
                if (groups.GroupwiseTemplates.length > 0) {
                    grp += '<div class="tab-pane show active" id="' + grpSelected + '" role="tabpanel" aria-labelledby="pills-' + grpSelected + '-tab">'
                    grp += '<div class="templates d-flex align-items-center flex-wrap">';
                    for (var g = 0; g < groups.GroupwiseTemplates.length; g++) {
                        if (groups.GroupwiseTemplates[g].Groups == grpSelected) {
                            var MatchedGroup = groups.GroupwiseTemplates[g];
                            for (var i = 0; i < MatchedGroup.Template.length; i++) {
                                if (i == 0) {
                                    grp += '<div class="template selected" data-template=' + MatchedGroup.Template[i] + '>';
                                    grp += '<div class="template-header"><i class="fab fa-github fa-4x"></i><strong class="title">' + MatchedGroup.Template[i] + '</strong></div>'
                                    grp += '<p class="description">Lorem ipsum dolor sit amet consectetur adipisicing elit. Asperiores commodi, sapiente a delectus distinctio voluptatum suscipit. Dolorem hic quae mollitia laudantium eius. Iure, eveniet vel exercitationem reiciendis mollitia at! Mollitia.</p>';
                                    grp += '</div>';
                                }
                                else {
                                    grp += '<div class="template" data-template=' + MatchedGroup.Template[i] + '>';
                                    grp += '<div class="template-header"><i class="fab fa-github fa-4x"></i><strong class="title">' + MatchedGroup.Template[i] + '</strong></div>'
                                    grp += '<p class="description">Lorem ipsum dolor sit amet consectetur adipisicing elit. Asperiores commodi, sapiente a delectus distinctio voluptatum suscipit. Dolorem hic quae mollitia laudantium eius. Iure, eveniet vel exercitationem reiciendis mollitia at! Mollitia.</p>';
                                    grp += '</div>';
                                }
                                //grp += '<option value=' + MatchedGroup.Template[i] + '>' + MatchedGroup.Template[i] + '</option>'
                            }
                        }
                    }
                    grp += '</div></div>';

                    $('#pills-tabContent').empty().append(grp);
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
    $('.nav-link').click(function () {
        console.log($(this).attr('href'));
    });

    // SHOW MODAL ON EVENT
    $(".template-invoke").on("click", function () {
        $(".VSTemplateSelection").fadeIn('fast');
    });

    // CLOSE MODAL ON EVENT
    $(".template-close").on("click", function () {
        $(".VSTemplateSelection").fadeOut('fast');
    });

    // TOGGLING ACTIVE CLASS TO TEMPLATE GROUP
    $(".template-group-item").on('click', function (e) {
        e.preventDefault();
        $(".template-group-item").removeClass('active');
        $(this).addClass('active');
    });

    // TOGGLING SELECTED CLASS TO TEMPLATE
    $(".template").on("click", function () {
        $(".template").removeClass("selected");
        $(this).addClass("selected");
        console.log($(".template.selected").data('template'));
    });

    // GET ID TO BE SHOWN
    let showId = $(".template-group-item.active").attr('href');
    console.log(showId);
    $(`.template-body .templates${showId}`).show();
});

function createTemplates(grpSelected) {
    grpSelected = "OSSDevOps"
    $.ajax({
        url: "../Environment/GetGroups",
        type: "GET",
        success: function (groups) {
            debugger;
            var grp = "";
            if (groups.GroupwiseTemplates.length > 0) {
                grp += '<div class="tab-pane show active" id="' + grpSelected + '" role="tabpanel" aria-labelledby="pills-' + grpSelected + '-tab">'
                grp += '<div class="templates d-flex align-items-center flex-wrap">';
                for (var g = 0; g < groups.GroupwiseTemplates.length; g++) {
                    if (groups.GroupwiseTemplates[g].Groups == grpSelected) {
                        var MatchedGroup = groups.GroupwiseTemplates[g];
                        for (var i = 0; i < MatchedGroup.Template.length; i++) {
                            if (i == 0) {
                                grp += '<div class="template selected" data-template=' + MatchedGroup.Template[i] + '>';
                                grp += '<div class="template-header"><i class="fab fa-github fa-4x"></i><strong class="title">' + MatchedGroup.Template[i] + '</strong></div>'
                                grp += '<p class="description">Lorem ipsum dolor sit amet consectetur adipisicing elit. Asperiores commodi, sapiente a delectus distinctio voluptatum suscipit. Dolorem hic quae mollitia laudantium eius. Iure, eveniet vel exercitationem reiciendis mollitia at! Mollitia.</p>';
                                grp += '</div>';
                            }
                            else {
                                grp += '<div class="template" data-template=' + MatchedGroup.Template[i] + '>';
                                grp += '<div class="template-header"><i class="fab fa-github fa-4x"></i><strong class="title">' + MatchedGroup.Template[i] + '</strong></div>'
                                grp += '<p class="description">Lorem ipsum dolor sit amet consectetur adipisicing elit. Asperiores commodi, sapiente a delectus distinctio voluptatum suscipit. Dolorem hic quae mollitia laudantium eius. Iure, eveniet vel exercitationem reiciendis mollitia at! Mollitia.</p>';
                                grp += '</div>';
                            }
                            //grp += '<option value=' + MatchedGroup.Template[i] + '>' + MatchedGroup.Template[i] + '</option>'
                        }
                    }
                }
                grp += '</div></div>';

                $('#pills-tabContent').empty().append(grp);
            }
        }
    });
}