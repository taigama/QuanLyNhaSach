
function showSnackbar(id, error) {
    // Get the snackbar DIV
    var x = document.getElementById(id);

    $("#" + id).html(error);

    // Add the "show" class to DIV
    x.className = "show";

    // After 3 seconds, remove the show class from DIV
    setTimeout(function () { x.className = x.className.replace("show", ""); }, 3000);
}

function checkBoolean(value) {

    if (value && value == "True") {
        return true;
    }
    return false;
}

function GenerateRowTacGia(id = '', index = '', addFunction = '', deleteFunction = '') {
    var name = id + index;
    var row =
        '<div id="row' + name + '" class="form-group">' +
        '<select id="' + name + '" name="' + name + '" class="selectpicker" data-live-search="true">' +
        '<option value="" > Đang tải...</option>' +
        '</select >' +
        '<button type="button" style="margin-left:5px" class="btn btn-default" onclick="' + deleteFunction + '">-</button>' +
        '</div>';
    return row;
}

function PopulateDropdown(path, id, index = '', value = -1) {
    $.ajax({
        url: path,
        type: 'GET',
        datatype: 'json',
        success: function (data) {
            var rows = '';
            $.each(data, function (i, item) {
                rows += '<option value="' + item.Key + '">' + item.Value + '</option>';
            });


            $("#" + id + index).html(rows);
            $("#" + id + index).attr('name', id + index);
            if (value > -1) {
                $("#" + id + index).val(value);
            }
            $("#" + id + index).selectpicker('refresh');
        },
        error: function (err) {
            alert(err.responseText);
        }
    });
    return $("#" + id + index);
}

function PopulateDropdownWithCallback(path, id, index = '', value = -1, callback) {
    $.ajax({
        url: path,
        type: 'GET',
        datatype: 'json',
        success: function (data) {
            var rows = '';
            $.each(data, function (i, item) {
                rows += '<option value="' + item.Key + '">' + item.Value + '</option>';
            });


            $("#" + id + index).html(rows);
            $("#" + id + index).attr('name', id + index);
            if (value > -1) {
                $("#" + id + index).val(value);
            }
            $("#" + id + index).selectpicker('refresh');

            if (callback) {
                callback();
            }
        },
        error: function (err) {
            alert(err.responseText);
        }
    });
    return $("#" + id + index);
}

function AjaxList(path, result, callback) {
    $.ajax({
        url: path,
        type: 'GET',
        datatype: 'json',
        success: function (data) {
            $.each(data, function (i, item) {
                result.push(item.Value);
            });
            if (callback) {
                callback();
            }
        },
        error: function (err) {
            alert("Error");
        }
    });
}

function RestricInputNumber(id, min = null, max = null, defaultValue = null) {
    var target = $('#' + id);

    if (defaultValue) {
        target.val(parseInt(defaultValue));
    }
    
    target.attr('min', min);
    target.attr('max', max);

    target.change(function () {
        if (!target.val()) {
            target.val(min);
        }
        else if (parseInt(target.val()) < min) {
            target.val(min);
        }
        else if (parseInt(target.val()) > max) {
            target.val(max);
        }
    });

    target.trigger('change');
}

function RestrictInputText(id, defautValue) {
    var target = $('#' + id);

    if (!target.val() || target.val() == '') {
        target.val(defautValue);
    }

    target.change(function () {
        if (!target.val() || target.val() == '') {
            target.val(defautValue);
        }
    });
}

function RestrictInputDate(id, min, max) {
    var target = $('#' + id);

    target.attr('min', ParseDate(min));
    target.attr('max', ParseDate(max));

    var func = function () {
        var current = DOMStringToDate(target.val());
        if (!target.val() || current < min) {
            target.val(ParseDate(min));
        }
        else if (current > max) {
            target.val(ParseDate(max));
        }
    }

    target.focusout(function () {
        func();
    });

    func();
}

function RestrictAndBindInputDate(id, min, max, bindId, month) {
    var target = $('#' + id);
    var bind = $('#' + bindId);

    target.attr('min', ParseDate(min));
    target.attr('max', ParseDate(max));

    var func = function () {
        var current = DOMStringToDate(target.val());

        if (!target.val() || current < min) {
            target.val(ParseDate(min));
        }
        else if (current > max) {
            target.val(ParseDate(max));
        }

        var currentBind = DOMStringToDate(target.val());
        currentBind.setMonth(currentBind.getMonth() + parseInt(month));
        bind.val(ParseDate(currentBind));
    }

    target.focusout(function () {
        func();
    });

    func();
}

function ParseDate(date) {
    var strYear = '' + date.getFullYear();
    var strMonth = '' + (date.getMonth() + 1);
    var strDate = '' + date.getDate();

    while (strYear.length < 4) {
        strYear = '0' + strYear;
    }

    while (strMonth.length < 2) {
        strMonth = '0' + strMonth;
    }

    while (strDate.length < 2) {
        strDate = '0' + strDate;
    }

    return strYear + '-' + strMonth + '-' + strDate;
}

function ParseDateStd(date) {
    var strYear = '' + date.getFullYear();
    var strMonth = '' + (date.getMonth() + 1);
    var strDate = '' + date.getDate();

    while (strYear.length < 4) {
        strYear = '0' + strYear;
    }

    while (strMonth.length < 2) {
        strMonth = '0' + strMonth;
    }

    while (strDate.length < 2) {
        strDate = '0' + strDate;
    }

    return strDate + '-' + strMonth + '-' + strYear;
}

function MoneyToString(money) {
    var str = '' + money;
    var result = '';
    var count = 1;
    while (count <= str.length) {
        result = str.charAt(str.length - count) + result;
        if (count % 3 === 0 && count != str.length) {
            result = ',' + result;
        }
        count++;
    }
    return result;
}

function DOMStringToDate(data) {
    var arrayString = data.split("-");

    if (arrayString.length == 3) {
        var year = parseInt(arrayString[0]);
        var month = parseInt(arrayString[1])
        var date = parseInt(arrayString[2]);
        return new Date(year, month - 1, date);
    }
    else {
        return new Date();
    }

}

function CheckNull(input, trueResult, falseResult) {
    if (input) return trueResult;
    else return falseResult;
}