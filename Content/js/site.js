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

    $(".menu-bars button").on("click", function(){
        $(".list-menu").show("slow");
        $(".list-menu .call-us").show();
        $("body").css("overflow","hidden");
    });

    $(".list-menu .btn-close").on("click", function(){
        $(".list-menu").hide();
        $("body").css("overflow","auto");
    });

    $("img[img-hover]").hover(
        function () {
            var img_hover = $(this).attr("img-hover");
            $(this).attr("src", img_hover);
        },
        function () {
            var img_default = $(this).attr("img-default");
            $(this).attr("src", img_default);
        }
    )
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

function ModalDetail(obj, title) {
    var $this = $(obj);
    var strHtmlDetail = $this.find(".box-prices", ".box-modal").html();
    var $modal = $("#ModalDetail");
    $modal.find(".modal-title").html("").append(title);
    $modal.find(".modal-body").html("").append("<div class='box-prices'>" + strHtmlDetail + "</div>");
    $modal.modal('show');
}

function OpenDetail(name) {
    location.href = "/services/" + name + ".html";
}

function ModalServiceDetail(obj) {
    var $this = $(obj);
    var strHtmlDetail = $this.find(".box-modal").html();
    var $modal = $("#ModalServiceDetail");
    $modal.find(".modal-body").html("").append(strHtmlDetail);
    $modal.modal('show');
}

function ShowImageDetail(obj) {
    var $this = $(obj);
    var $boxImages = $this.parents(".box-images");
    var src = $this.attr("src");
    $boxImages.find(".image-show img").attr("src", src);
    $boxImages.find(".image-items img").removeClass("opacity-100");
    $boxImages.find(".image-items img").removeClass("opacity-50");
    $boxImages.find(".image-items img").addClass("opacity-50");
    $this.addClass("opacity-100");
}