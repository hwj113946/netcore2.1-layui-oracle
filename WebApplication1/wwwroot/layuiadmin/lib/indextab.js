$(function ($) {
    GetLoadNav();
});

function UseMenuTab(_html, n) {
    _html += '<dl class="layui-nav-child">';
    $.each(n, function (i) {
        var subrow = n[i];
        _html += '<dd  data-name="' + subrow.MENU_NAME + '">';

        var childNodes = subrow.ChildNodes;
        if (childNodes.length > 0) {
            _html += '<a href="javascript:;" lay-tips="' + subrow.MENU_NAME + '"><i class="layui-icon ' + subrow.MENU_ICON + '"></i>' + subrow.MENU_NAME + '</a>';

            _html = UseMenuTab(_html, childNodes);
        }
        else {
            _html += '<a  data-id="' + subrow.MENU_ID + '" lay-tips="' + subrow.MENU_NAME + '" lay-href="' + subrow.MENU_URL + '" ><i class="layui-icon ' + subrow.MENU_ICON + '" style="font-size: 16px;"></i>' + subrow.MENU_NAME + '</a>';
        }
        _html += '</dd>';
    })
    _html += '</dl>';
    return _html;
};


function GetLoadNav() {
    var MenuData;
    var BtnData
    $.ajax({
        url: "/Main/GetMenuData",
        type: "get",
        dataType: "json",
        async: false,
        success: function (data) {
            MenuData = eval(data.UseMenuDatas);
        }
    });
    var Usedata = eval(MenuData.Result);
    var _html = "";
    $.each(MenuData, function (i) {
        var row = MenuData[i];
        if (row.PARENT_MENU_ID == "0") {
            _html += '<li data-name="' + row.MENU_NAME + '" class="layui-nav-item">';
            _html += '<a  href="javascript:;" lay-tips="' + row.MENU_NAME + '" lay-direction="2"><i class="layui-icon ' + row.MENU_ICON + '"></i><cite>' + row.MENU_NAME + '</cite></a>';
            var childNodes = row.ChildNodes;
            if (childNodes.length > 0) {
                _html = UseMenuTab(_html, childNodes);
            }

            _html += '</li>';
        }
    });
    $("#LAY-system-side-menu").prepend(_html);
};