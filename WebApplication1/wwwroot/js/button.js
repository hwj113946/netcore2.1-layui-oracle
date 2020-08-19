$(function ($) {
    var menu, tab, menu_id, jsondata;
    var _html = "";
    $.each(window.parent.$("#LAY-system-side-menu .layui-this a"), function () {
        menu = $(this).attr("lay-href");
        menu_id = $(this).attr("data-id");
    });
    $.each(window.parent.$("#LAY_app_tabsheader .layui-this"), function () {
        tab = $(this).attr("lay-id")
    });
    if (menu == tab) {
        $.ajax({
            type: 'post',
            dataType: 'json',
            traditional: true,
            url: '/Public/GetButton',
            data: {
                menu_id: menu_id
            },
            success: function (res) {
                if (res.code == "0") {
                    jsondata = eval(res.data);
                    $.each(jsondata, function (i) {
                        var row = jsondata[i];
                            _html += '<a  id="' + row.ATTRIBUTE2 + '" class="layui-btn layui-btn-sm ' + row.ATTRIBUTE1 + '" onclick="' + row.BUTTON_EVENT + '"  style="text-decoration:none;">' + row.BUTTON_NAME + '</a>';                 
                    });
                    $("#button").append(_html);
                } else {
                    layer.msg(res.msg, {icon: 5});
                }                
            },
            error: function (res) {
                layer.msg("获取角色菜单按钮接口出错，请联系管理员", {
                    icon: 5
                });
            }
        });
        
    }
})