var position_active = 0;
$(document).ready(function(){
    $(".list-inline-item-link").each(function(index,element){
        if($(element).hasClass("active"))
            position_active = index;
    });

    //$(".list-inline-item-link").hover(
    //    function () {
    //        $(".list-inline-item-link").removeClass("active");
    //        $(".list-inline-item").removeClass("active");
    //    }, 
    //    function () {
    //        var elementActive = $(".list-inline-item-link")[position_active];
    //        $(elementActive).addClass("active");
    //        $(elementActive).parent(".list-inline-item").addClass("active");
    //    }
    //);

    $(".img-hover-zoom").hover(
        function () {
            $(this).find("a.learn-more").show();
        }, 
        function () {
            $(this).find("a.learn-more").hide();
        }
    );

    $(".menu-bars button").on("click", function(){
        $(".list-menu").show("slow");
        $(".list-menu .call-us").show();
        $("body").css("overflow","hidden");
    });

    $(".list-menu .btn-close").on("click", function(){
        $(".list-menu").hide();
        $("body").css("overflow","auto");
    });
});

function ActiveMenu(ElementId){
    $("#" + ElementId).addClass("active");
    $("#" + ElementId).parent(".list-inline-item").addClass("active");
}

function ReloadSliderImageMobile(){
    var windowWidth = $(window).width();
    if (windowWidth < 768) {
        $("img", ".slider-4").attr("src", "/Content/images/banner/home/home_banner_4_mobile.jpg");
        $("img", ".slider-5").attr("src", "/Content/images/banner/home/home_banner_5_mobile.jpg");
    }
}