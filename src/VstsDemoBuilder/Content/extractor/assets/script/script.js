
$(function(){

    let length = $(".template").length;

    for(let i=0;i<length;i++)
       {
          $(".template").eq(i).css({
             animationDelay:"0."+i+"s"
          });
       };



    $(".template-invoke").on("click", function(){
        $(".VSTemplateSelection").fadeIn('fast');
    });
    $(".template-close").on("click", function(){
        $(".VSTemplateSelection").fadeOut('fast');
    });

    $(".template-group-item").on('click', function(e){
        e.preventDefault();
        $(".template-group-item").removeClass('active');
        $(this).addClass('active');
    });

    $(".template").on("click", function(){
        $(".template").removeClass("selected");
        $(this).addClass("selected");
    });
});