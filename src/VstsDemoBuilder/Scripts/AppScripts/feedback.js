$(document).ready(function () {

    $('#formdiv').removeClass('d-none');
    $('#responsediv').addClass('d-none');
    $('#user-name').change(function () {
        var name = $('#user-name').val();
        if (name == "" || name == null || name == undefined) {
            $("#name_Error").empty();
            $("#name_Error").text("Please enter valid User Name");
            $("#name_Error").removeClass("d-none").addClass("d-block");
            $("#user-name").focus();
            return false;
        }
        else {
            $("#name_Error").removeClass("d-block").addClass("d-none");
        }
    });
    $('#user-email').change(function () {
        var email = $('#user-email').val();
        if (email == "" || email == null || email == undefined) {
            $("#email_Error").empty();
            $("#email_Error").text("Please enter valid Email");
            $("#email_Error").removeClass("d-none").addClass("d-block");
            $("#user_email").focus();
            return false;
        }
        else {
            if (!validateEmail(email)) {
                $("#email_Error").empty();
                $("#email_Error").text("Please enter valid Email");
                $("#name_Error").removeClass("d-block").addClass("d-none");
                $("#email_Error").removeClass("d-none").addClass("d-block");
                $("#user-email").focus();
                return false;
            }
            else {
                $("#email_Error").removeClass("d-block").addClass("d-none");
            }
        }
    });
    $('#feedbacksubmit').click(function () {
        var name = $('#user-name').val();
        if (name == "" || name == null || name == undefined) {
            $("#name_Error").empty();
            $("#name_Error").text("Please enter valid User Name");
            $("#name_Error").removeClass("d-none").addClass("d-block");
            $("#user-name").focus();
            return false;
        }
        var email = $('#user-email').val();
        if (!validateEmail(email)) {
            $("#email_Error").empty();
            $("#email_Error").text("Please enter valid Email");
            $("#name_Error").removeClass("d-block").addClass("d-none");
            $("#email_Error").removeClass("d-none").addClass("d-block");
            $("#user-email").focus();
            return false;            
        }
        $("#name_Error").removeClass("d-block").addClass("d-none");
        $("#email_Error").removeClass("d-block").addClass("d-none");
        var noofyears = $("input[type='radio'][name='noofyears']:checked").val();

        // come to know
        var know = $("input[type='checkbox'][name='know']:checked").map(function () {
            return this.value;
        }).get().join(',');
        
        // purpose
        var purpose = $("input[type='radio'][name='purpose']:checked").val();

        // template section
        var used = $("input[type='checkbox'][name='used']:checked").map(function () {
            return this.value;
        }).get().join(',');
        
        // most user template
        var usedtemplatenames = $('#usedtemplatenames').val();

        // kind of template
        var kindoftemplates = $('#kindoftemplates').val();

        // other feedback
        var otherfeedback = $('#otherfeedback').val();
               
        var auth = {
            Name: name,            
            Email: email,
            Noofyears: noofyears,
            Know: know,
            Purpose: purpose,
            Used: used,
            Usedtemplatenames: usedtemplatenames,
            Kindoftemplates: kindoftemplates,
            Otherfeedback: otherfeedback
        };
        $.ajax({
            url: "/feedback/storefeedback",
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(auth),            
            success:
                function (data) {
                    if (data == "True") {
                        $('#responsediv').removeClass('d-none');
                        $('#formdiv').addClass('d-none');
                    }
                    else {
                        $('#storage_Error').empty();
                        $('#storage_Error').text("<p>Unable to store data : "+data+"</p>");
                        $('#storage_Error').removeClass('d-none');
                        $('#formdiv').addClass('d-none');
                    }
                },
            failure: function () {
                $('#storage_Error').empty();
                $('#storage_Error').text("<p>Something went wrong</p>");
                $('#storage_Error').removeClass('d-none');
                $('#formdiv').addClass('d-none');
               //alert("Something went wrong");
            }
        });
    }); 
});

function validateEmail(email) {
    var re = /\S+@\S+\.\S+/;
    return re.test(email);
}