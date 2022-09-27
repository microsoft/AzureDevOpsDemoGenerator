$(document).ready(function () {

    $('#formdiv').removeClass('d-none');
    $('#responsediv').addClass('d-none')

    $('#feedbacksubmit').click(function () {
        var name = $('#user-name').val();
        if (name == "" || name == null || name == undefined) { alert("Please enter valid User Name"); return; }
        var email = $('#user-email').val();
        if (!validateEmail(email)) {
            alert("please enter valid email");
            return;
        }
        
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
                },
            failure: function () {
                alert("Something went wrong");
            }
        });
    }); 
});

function validateEmail(email) {
    var re = /\S+@\S+\.\S+/;
    return re.test(email);
}