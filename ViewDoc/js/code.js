// TODO:
// test feature "inheritance"
// test two feat. of the same type on top of each other
// colors (order)

var docData
	= {};
var view
	= {};
var treeItems
	= {};
var popoverAnchor;

//function setTitle(title, docTitle) {
//    document.title = title;
//    $(".doc > h2").text(docTitle ? docTitle : title);
//}

function addTreeItem(level, text, id) {
    var ofs = 3 + level * 20;
    $($(".sidebar-body")[0]).append($("<div class='tree-item' style='padding-left: " + ofs + "px;'>" + htmlEncode(text) + "</div>").prepend($("<div>&nbsp;&nbsp;&nbsp;&nbsp;</div>").click(function () {
        if (view[id]) { $(this).removeAttr("class"); } else { $(this).attr("class", className(id)); }
        toggle(id);
    })));
    treeItems[id] = text;
}

function setTreeItems(data) {
    for (var i in data) {
        addTreeItem(data[i][0], data[i][1], data[i][2]);
    }
}

function addFeature(name, value) {
    $($(".sidebar-body")[1]).append("<div><span class='feat-name'>" + htmlEncode(name) + "</span><br/><span class='feat-val'>" + value + "</span></div>");
}

function setFeatures(data) {
    for (var i in data) {
        addFeature(data[i][0], data[i][1]);
    }
}

function setContent(data) {
    var doc = $("#doc-content");
    for (var i in data) {
        var span = $("<span class='a'>" + htmlEncode(data[i][0]) + "</span>").data("data", data[i]);
        doc.append(span);
        span.mousestop(function (e) {
            showPopover(e.pageX, e.pageY, e.sender);
        }, {
            onMouseout: function () {
                hidePopover();
            },
            onStopMove: function () {
                hidePopover();
            }
        });
        data[i].push(span);
        for (var j in data[i][1]) {
            var a = data[i][1][j];
            if (a instanceof Array) { a = a[0]; }
            if (!docData[a]) { docData[a] = []; }
            docData[a].push(data[i]);
        }
    }
}

function popover(data) {
    for (var i in data[1]) {
        var a = data[1][i];
        if (a instanceof Array) { a = a[0]; }
        if (view[a]) {
            var c = "";
            var l = data[1][i].length;
            for (var j = 1; j < l; j += 2) {
                c += "<div><span class='feat-name'>" + htmlEncode(data[1][i][j]) + "</span><br/><span class='feat-val'>" + data[1][i][j + 1] + "</span></div>";
            }
            return { title: treeItems[a], contentHtml: c }
        }
    }
    return null;
}

function showPopover(x, y, sender) {
    var data = $(sender).data("data");
    var p = popover(data);
    if (!p || p.contentHtml == "") { return; }
    var wnd = $(window);
    var hw = wnd.width() / 2;
    var hh = wnd.height() / 2;
    var mx = x - wnd.scrollLeft();
    var my = y - wnd.scrollTop();
    var placement;
    if (my < hh) { placement = "bottom-"; }
    else { placement = "top-"; }
    if (mx < hw) { placement += "right"; }
    else { placement += "left"; }
    popoverAnchor
		.css("left", (x - 5) + "px")
		.css("top", (y - 5) + "px")
		.popover({
		    "animation": true,
		    "html": true,
		    "placement": placement,
		    "trigger": "manual",
		    "title": htmlEncode(p.title),
		    "content": p.contentHtml
		})
		.popover("show");
}

function hidePopover() {
    if ($(".popover").length > 0) {
        popoverAnchor.popover("destroy");
    }
}

function show(a) {
    view[a] = 1;
    update(a);
}

function hide(a) {
    delete view[a];
    update(a);
}

function toggle(a) {
    if (view[a]) {
        hide(a);
    } else {
        show(a);
    }
}

function className(a) {
    return "c" + (a % 10);
}

function update(a) {
    for (var i in docData[a]) {
        var rmvClass = true;
        for (var j in docData[a][i][1]) {
            var an = docData[a][i][1][j];
            if (an instanceof Array) { an = an[0]; }
            if (view[an]) {
                docData[a][i][2].attr("class", "a " + className(an));
                rmvClass = false;
                break;
            }
        }
        if (rmvClass) {
            docData[a][i][2].attr("class", "a");
        }
    }
}

$(document).ready(function () {
    popoverAnchor = $("<div style='width: 10px; height: 10px; position: absolute; pointer-events: none;'></div>").appendTo("body");
    // perfectify
    $(document).perfectify(pfy.a);
});