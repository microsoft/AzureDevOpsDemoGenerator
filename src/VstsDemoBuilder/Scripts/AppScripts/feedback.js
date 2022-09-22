$(document).ready(function () {

    $('#formdiv').removeClass('d-none');
    $('#responsediv').addClass('d-none')

    $('#feedbacksubmit').click(function () {

        var noofyears = $("input[type='radio'][name='noofyears']:checked").val();

        // come to know
        var blog = $("input[type='checkbox'][name='blog']:checked").val();
        var social = $("input[type='checkbox'][name='social']:checked").val();
        var Emails = $("input[type='checkbox'][name='Emails']:checked").val();
        var friendscolleagues = $("input[type='checkbox'][name='friendscolleagues']:checked").val();
        var other = $("input[type='checkbox'][name='other']:checked").val();

        // purpose
        var noofyears = $("input[type='radio'][name='noofyears']:checked").val();

        // template section
        var general = $("input[type='checkbox'][name='general']:checked").val();
        var devopslabs = $("input[type='checkbox'][name='devopslabs']:checked").val();
        var mslearn = $("input[type='checkbox'][name='mslearn']:checked").val();
        var caf = $("input[type='checkbox'][name='caf']:checked").val();
        var azurecommunity = $("input[type='checkbox'][name='azurecommunity']:checked").val();
        var FastTrackonAzure = $("input[type='checkbox'][name='FastTrackonAzure']:checked").val();
        var private = $("input[type='checkbox'][name='private']:checked").val();

        // most user template
        var usedtemplatenames = $('#usedtemplatenames').val();

        // kind of template
        var kindoftemplates = $('#kindoftemplates').val();

        // other feedback
        var otherfeedback = $('#otherfeedback').val();

        var state = true;
        if (typeof noofyears == 'undefined') {
            state = false;
        }
        else {
            $('#responsediv').removeClass('d-none')
            $('#formdiv').addClass('d-none');
        }
    }); 
});