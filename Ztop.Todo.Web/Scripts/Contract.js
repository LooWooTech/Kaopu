﻿
$(function () {
    $(".btn-group>button").click(function () {
        var val = $(this).text();
        if (val == "全部") {
            val = "";
        }
        var targetName = $(this).attr("name");
        $(".btn-group>button[name='" + targetName + "']").removeClass("btn-success").addClass("btn-default");
        $(this).removeClass("btn-default").addClass("btn-success");
        $("input[name='" + targetName + "']").val(val);
    });
    $("input.datetimepicker").datetimepicker({
        timepicker: false,
        format: 'Y-m-d'
    });

    $(".input-group-btn>button").click(function () {
        var targetName = $(this).attr("name");
        $(this).removeClass("btn-default").addClass("btn-success");
        $("input[name='" + targetName + "']").val("");
    });


    $("input[name='OtherSide']").focus(function () {
        $(this).prev().children().removeClass("btn-success").addClass("btn-default");
    });

    $("input[name='Name']").focus(function () {
        $(this).prev().children().removeClass("btn-success").addClass("btn-default");
    });
})

