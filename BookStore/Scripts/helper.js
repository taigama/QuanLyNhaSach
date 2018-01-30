// tạo ra 1 cell trong table, với string nội dung html, và độ rộng bootstrap
function generateCell(data, widthUnit) {
    var cell = '<td class="col-md-' + widthUnit + '">' + data + '</td>';
    return cell;
}

// tạo ra 1 cell trong table, với id, với string nội dung html, và độ rộng bootstrap
function generateCellId(data, widthUnit, id) {
    var cell = '<td id="'+ id +'" class="col-md-' + widthUnit + '">' + data + '</td>';
    return cell;
}

function generateCellHidden(id, data) {
    var cell = '<td id="' + id +'" class="hidden">' + data + '</td>';
    return cell;
}

// tạo html button, nội dung display, style (primary, info, etc), str đại diện cho nội dung onClick
function generateButton(display, style, onClickStr) {
    var btn = '<button type="button" class="btn btn-' + style + ' btn-xs" onclick="' + onClickStr + '">' + display + '</button>';
    return btn;
}

// dạng của nó 'yyyy-MM-dd:time'
function formatJsonNetDate(date) {
    var year = date.substr(0, 4);
    var month = date.substr(5, 2);
    var day = date.substr(8, 2);


    return [day, month, year].join('/');
}// trả về 'dd/MM/yyyy'

function formatJsonNetDateToHtml(date) {
    return date.substr(0, 10);
}// trả về 'yyyy-MM-dd'

// dạng của nó '/01696969696969'
function formatASPDate(aspdate) {
    var d = new Date(parseInt(aspdate.substr(6)));

    month = '' + (d.getMonth() + 1);
    day = '' + d.getDate();
    year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [day, month, year].join('-');
}// format date thành 'dd-MM-yyyy'

// tạo chuỗi có dạng #FFFFFF
function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

// kiểm tra coi có phải số nguyên không?
function isInt(value) {
    if (isNaN(value)) {
        return false;
    }
    var x = parseFloat(value);
    return (x | 0) === x;
}

// from a list item.Key & item.Value
function genDropdownOption(data) {
    var rows = '';
    $.each(data, function (i, item) {
        rows += '<option value="' + item.Key + '">' + item.Value + '</option>';
    });
    return rows;
}

function populateStdDropdown(data, id) {
    id = '#' + id;
    var rows = '<option value="">Toàn bộ</option>'
        + genDropdownOption(data);

    $(id).html(rows);
    $(id).selectpicker('refresh');
}

function setChildHeight(parrent, height) {    
    $.each($(parrent).children(), function (i, child) {
        $(child).css("line-height", height + "px");
    });
}