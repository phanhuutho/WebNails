var position_active = 0;
$(document).ready(function(){
    $(".list-inline-item-link").each(function(index,element){
        if($(element).hasClass("active"))
        position_active = index;
    });

    $(".list-inline-item-link").hover(
        function () {
            $(".list-inline-item-link").removeClass("active");
        }, 
        function () {
            var elementActive = $(".list-inline-item-link")[position_active];
            $(elementActive).addClass("active");
        }
    );

    $(".menu-bars button").on("click", function(){
        $(".list-menu").show("slow");
        $(".menu-mobile-tablet, .call-us").hide();
        $(".list-menu .call-us").show();
        $("body").css("overflow","hidden");
    });

    $(".list-menu .btn-close").on("click", function(){
        $(".list-menu").hide();
        $(".menu-mobile-tablet, .call-us").show();
        $("body").css("overflow","auto");
    });
});

function ActiveMenu(ElementId){
    $("#" + ElementId).addClass("active");
}

function ReloadSliderImageMobile(){
    var windowWidth = $(window).width();
    if (windowWidth < 768) {
        $("img", ".slider-4").attr("src", "/Content/images/banner/home/home_banner_4_mobile.jpg");
        $("img", ".slider-5").attr("src", "/Content/images/banner/home/home_banner_5_mobile.jpg");
    }
}

function InitValidateContact() {
    $.validator.addMethod('phone', function (value, element) {
        if (value == '') return true;
        return value.match(/\(?([0-9]{3})\)?([ .-]?)([0-9]{3})\2([0-9]{4})/);
    }, 'Enter Valid  phone number');

    $("#frmSendMessage").validate({
        rules: {
            YourName: "required",
            YourEmail: { required: true, email: true },
            YourPhone: { required: true, phone: true},
            Subject: "required",
            YourMessage: "required",
            Agreement: "required"
        },
        messages: {
            YourName: { required: "Require input Your name" },
            YourEmail: { required: "Require input Your email", email: "Invalid Your email" },
            YourPhone: { required: "Require input Your phone", phone: "Invalid Your phone" },
            Subject: { required: "Require Subject" },
            YourMessage: { required: "Require Your message" },
            Agreement: { required: "Require agree with the Terms & Conditions and the Privacy & Cookies Policy" }
        },
        submitHandler: function (form) {
            $("body").addClass("loader");
            $("body").append("<div class='loading'></div>");
            $.ajax({
                type: "POST",
                url: "/contact.html",
                data: $(form).serialize(),
                success: function (result) {
                    if (result.messages == "OK") {
                        $(".loading", "body").remove();
                        $("body").removeClass("loader");
                        document.forms[0].reset();
                        $(".modal-body", "#ModalMessage").html("Thank you for your message. It has been sent. We will get back with you shortly.");
                        $("#ModalMessage").modal("show");
                    }
                },
            });
            return false;
        }
    });
}