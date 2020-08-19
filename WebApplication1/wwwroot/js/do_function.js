//删除方法
function win_del(datas, url) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        traditional: true,
        url: url,
        data: {
            id: datas
        },
        success: function (res) {
            if (res.code == "200") {
                return true;
            } else {
                return false;
            }
        },
        error: function () {
            return false;
        }
    });
}

//新增、修改、查看
function win_show(url,layer,title,width,height) {
    layer.open({
        type: 2,
        area: [''+width+'px', ''+height+'px'],
        title: title,
        content: url
    });
}

//查询：table组件，url地址，参数{id:"123",name:"哦嗬"}
function win_search(table,url,datas) {
    table.reload('table1', {
        url:url,
        where: datas,
        method: 'Post',
        page: {
            curr: 1
        }
    });
}


//导出Excel数据
function win_exportExcel(layer,table,ins,url,datas) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        traditional: true,
        url: url,
        data: datas,
        success: function (res) {
            if (res.code == "0") {
                table.exportFile(ins.config.id, res.data, 'xls');
            } else {
                layer.msg("导出数据失败", {
                    icon: 5
                });
            }
        },
        error: function (res) {
            layer.msg("数据接口出错了，请联系管理员", {
                icon: 5
            });
        }
    });
}

//渲染类型下拉框
function DropDowmRender(form, html_id, url, value) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: url,
        success: function (res) {
            if (res.code == "0") {
                $.each(res.data, function (index, item) {
                    if (index == 0) {
                        if (value == "" || value == null || value == undefined) {
                            value = item.ID;
                        }
                    }
                    $("#" + html_id + "").append('<option value="' + item.ID + '">' + item.NAME + '</option>');
                });
                $("#" + html_id + "").val(value);
                form.render('select');
            } else {
                parent.layer.msg("加载数据出错，请联系管理员", {
                    icon: 5
                });
            }
        },
        error: function (res) {
            parent.layer.msg("加载数据出错，请联系管理员", {
                icon: 5
            });
        }
    });
}