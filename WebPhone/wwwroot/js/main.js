$(function () {
    "use strict";

    /* NAV ADMIN */

    function ToogleClass(element, className) {
        element.hasClass(className)
            ? element.removeClass(className)
            : element.addClass(className);
    }

    $("#main .nav-left .item-top.item-top-ls").click(function (e) {
        ToogleClass($(this), "open");
    });

    const checkBox = $(".content .main-content .table .form-check-input");

    checkBox.first().click(function (e) {
        $(this).is(":checked")
            ? checkBox.prop("checked", true)
            : checkBox.prop("checked", false);
    });

    checkBox.not(checkBox.first())?.click(function (e) {
        if (
            checkBox
                .not(checkBox.first())
                .toArray()
                .every((item) => item.checked)
        ) {
            checkBox.first().prop("checked", true);
        }

        if (
            checkBox
                .not(checkBox.first())
                .toArray()
                .some((item) => !item.checked)
        ) {
            checkBox.first().prop("checked", false);
        }
    });

    checkBox.click(function (e) {
        const btnRemove = $(".content .top-content .btn-remove");
        checkBox.toArray().some((item) => item.checked)
            ? btnRemove.css("display", "inline-block")
            : btnRemove.css("display", "none");
    });
});
