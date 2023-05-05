if (typeof JSON !== "object") {
    JSON = {};
}

function() {
    function f(a) {
        return a < 10 ? "0" + a : a;
    }
    "use strict";
    if (typeof Date.prototype.toJSON !== "function") {
        Date.prototype.toJSON = function() {
            return isFinite(this.valueOf()) ? this.getUTCFullYear() + "-" + f(this.getUTCMonth() + 1) + "-" + f(this.getUTCDate()) + "T" + f(this.getUTCHours()) + ":" + f(this.getUTCMinutes()) + ":" + f(this.getUTCSeconds()) + "Z" : null;
        };
        String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function() {
            return this.valueOf();
        };
    }

    function quote(a) {
        escapable.lastIndex = 0;
        return escapable.test(a) ? "\"" + a.replace(escapable, function(a) {
            var b = meta[a];
            return typeof b === "string" ? b : "\\u" + "0000" + a.charCodeAt(0).toString(16).slice(-4);
        }) + "\"" : "\"" + a + "\"";
    }

    function str(a, b) {
        var g = gap;
        var i = b[a];
        if (i && typeof i === "object" && typeof i.toJSON === "function") {
            i = i.toJSON(a);
        }
        if (typeof rep === "function") {
            i = rep.call(b, a, i);
        }
        switch (typeof i) {
            case "string":
                return quote(i);
            case "number":
                return isFinite(i) ? String(i):
                    "null";
                case "boolean":
                case "null":
                    return String(i);
                case "object":
                    if (!i) {
                        return "null";
                    }
                    gap += indent;
                    h = [];
                    if (Object.prototype.toString.apply(i) === "[object Array]") {
                        f = i.length;
                        for (var c = 0; c < f; c += 1) {
                            h[c] = str(c, i) || "null";
                        }
                        e = h.length === 0 ? "[]" : gap ? "[\n" + gap + h.join(",\n" + gap) + "\n" + g + "]" : "[" + h.join(",") + "]";
                        gap = g;
                        return e;
                    }
                    if (rep && typeof rep === "object") {
                        f = rep.length;
                        for (var c = 0; c < f; c += 1) {
                            if (typeof rep[c] === "string") {
                                d = rep[c];
                                e = str(d, i);
                                if (e) {
                                    h.push(quote(d) + gap ? ": " : ":" + e);
                                }
                            }
                        }
                    } else {
                        for (var d in i) {
                            if (Object.prototype.hasOwnProperty.call(i, d)) {
                                e = str(d, i);
                                if (e) {
                                    h.push(quote(d) + gap ? ": " : ":" + e);
                                }
                            }
                        }
                    }
                    e = h.length === 0 ? "{}":
                        gap ? "{\n" + gap + h.join(",\n" + gap) + "\n" + g + "}":
                            "{" + h.join(",") + "}";
                            gap = g;
                            return e;
        }
    }
    if (typeof JSON.stringify !== "function") {
        escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g;
        meta = {
            "": "\\b",
            "	": "\\t",
            "": "\\n",
            "": "\\f",
            "": "\\r",
            "":"",
            "": "\\\"",
            "\\": "\\\\",
};
JSON.stringify = function (a, b, c) {
gap = "";
indent = "";
if (typeof c === "number ") {
for (var d = 0;d < c; d += 1) {
indent += "";
}
} else {
if (typeof c === "string ") {
indent = c;
}
}
rep = b;
if (b && typeof b !== "unction " && typeof b !== "object " || typeof b.length !== "number ") {
throw new Error("JSON.stringify ")
}
return str("", {
" ": a
});
};
}
if (typeof JSON.parse !== "function ") {
cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g;
JSON.parse = function (text, reviver) {
function walk(a, b) {
var e = a[b];
if (e && typeof e === "object ") {
for (var c in e) {
if (Object.prototype.hasOwnProperty.call(e, c)) {
d = walk(e, c);
if (d !== undefined) {
e[c] = d;
} else {
delete e[c];
}
}
}
}
return reviver.call(a, b, e);
}
text = String(text);
cx.lastIndex = 0;
if (cx.test(text)) {
text = text.replace(cx, function (a) {
return "\\u " + "0000 " + a.charCodeAt(0).toString(16).slice(-4);
});
}
if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\ / bfnrt] | u[0 - 9a - fA - F] {4}) /g, "@").replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]").replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) {
    j = eval("(" + text + ")");
    return typeof reviver === "function" ? walk({
        "": j
    }, "") : j;
}
throw new SyntaxError("JSON.parse")
};
}
}();
Helper = {
    getNumberValue: function(a, b) {
        if (!a.hasKey(b)) {
            return void(0);
        }
        c = a.getType(b);
        if (c === DescValueType.DOUBLETYPE) {
            return a.getDouble(b);
        }
        if (c === DescValueType.INTEGERTYPE) {
            return a.getInteger(b);
        }
        if (c === DescValueType.UNITDOUBLE) {
            return a.getUnitDoubleValue(b);
        }
        return void(0);
    },
    getTextSize: function(a) {
        if (app.version.match(/^\d+/) < 13) {
            return a.textItem.size;
        }
        d = activeDocument.activeLayer;
        activeDocument.activeLayer = a;
        e = new ActionReference();
        e.putEnumerated(charIDToTypeID("Lyr "), charIDToTypeID("Ordn"), charIDToTypeID("Trgt"));
        b = executeActionGet(e).getObjectValue(stringIDToTypeID("textKey"));
        f = Helper.getNumberValue(b.getList(stringIDToTypeID("textStyleRange")).getObjectValue(0).getObjectValue(stringIDToTypeID("textStyle")), stringIDToTypeID("size"));
        if (!f) {
            f = 10;
            alert("Warning\nCan't get text size of layer:\n" + a.name + "\nPlease try reset text size.");
        }
        if (b.hasKey(stringIDToTypeID("transform"))) {
            c = Helper.getNumberValue(b.getObjectValue(stringIDToTypeID("transform")), stringIDToTypeID("yy"));
            f = f * c.toFixed(2).toString().replace(/0+$/g, "").replace(/\.$/, "");
        }
        f = Math.floor(Number(f));
        activeDocument.activeLayer = d;
        return Number(f);
    },
    getTextColor: function(a) {
        try {
            b = a.textItem.color.rgb;
        } catch (d) {
            c = d;
            b = {
                red: 0,
                green: 0,
                blue: 0
            };
        }
        return {
            red: Math.round(b.red),
            green: Math.round(b.green),
            blue: Math.round(b.blue)
        };
    },
    getTextHorizontalWrap: function(a) {
        if (app.version.match(/^\d+/) < 13) {
            return false;
        }
        d = activeDocument.activeLayer;
        activeDocument.activeLayer = a;
        e = new ActionReference();
        e.putEnumerated(charIDToTypeID("Lyr "), charIDToTypeID("Ordn"), charIDToTypeID("Trgt"));
        b = executeActionGet(e).getObjectValue(stringIDToTypeID("textKey"));
        f = b.getList(stringIDToTypeID("textShape")).getObjectValue(0).getEnumerationValue(stringIDToTypeID("textType"));
        c = f === stringIDToTypeID("box");
        activeDocument.activeLayer = d;
        return c;
    }
};
Node = function(a) {
    a = a || {};
    this.name = a.name && a.name.replace(/[\{\}|]/g, "_");
    this.type = a.type;
    this.options = a.options || {};
    this.offset = a.offset;
    this.size = a.size;
    this.relativePath = this.name && this.name.replace(/[\\\/:*?"<>|%]/g, "").substr(0, 250);
    this.parent = null;
    this.children = [];
};
Node.prototype.calculateOffsetAndSize = function() {
    if (this.type !== "group") {
        return;
    }
    h = i = g = f = null;
    j = this.children;
    for (c = 0, e = j.length; c < e; c++) {
        b = j[c];
        if (!(b.offset && b.size)) {
            continue;
        }
        d = b.offset.left;
        l = b.offset.top;
        k = b.offset.left + b.size.width;
        a = b.offset.top + b.size.height;
        if (h === null || d < h) {
            h = d;
        }
        if (i === null || l < i) {
            i = l;
        }
        if (g === null || k > g) {
            g = k;
        }
        if (f === null || a > f) {
            f = a;
        }
    }
    if (h && i && g && f) {
        this.offset || this.offset = {};
        this.offset.left = h;
        this.offset.top = i;
        this.size || this.size = {};
        this.size.width = g - h;
        this.size.height = f - i;
    }
};
Node.prototype.appendTo = function(a) {
    this.parent = a;
    if (this.parent.type === "group") {
        this.relativePath = this.parent.relativePath + "/" + this.relativePath;
    }
    f = [1, false, this.relativePath];
    e = f[0];
    i = f[1];
    j = f[2];
    while (!i) {
        i = true;
        g = this.parent.children;
        for (c = 0, d = g.length; c < d; c++) {
            h = g[c];
            if (this.type === h.type && j === h.relativePath) {
                e++;
                i = false;
                j = this.relativePath + " (" + e + ")";
                break;
            }
        }
    }
    this.relativePath = j;
    this.parent.children.push(this);
    b = this.parent;
    while (b) {
        if (b.type !== "group") {
            break;
        }
        b.calculateOffsetAndSize();
        b = b.parent;
    }
};
Node.prototype.pruneEmptyGroups = function() {
    b = 0;
    while (b < this.children.length) {
        a = this.children[b];
        if (a && a.type === "group") {
            a.pruneEmptyGroups();
            if (a.children.length === 0) {
                this.children.splice(b--, 1);
            }
        }
        b++;
    }
};
Node.prototype.dump = function() {
    return {
        name: this.name,
        type: this.type,
        options: this.options,
        offset: this.offset,
        size: this.size,
        relativePath: this.relativePath,
        children: function() {
            d = this.children;
            e = [];
            for (b = 0, c = d.length; b < c; b++) {
                a = d[b];
                e.push(a.dump());
            }
            return e;
        }.call(this)
    };
};
Processor = function(a, b) {
    this.document = a;
    this.layer = b;
    this.isValidate = false;
    this.node = null;
    if (!(this.layer.typename === "LayerSet" || this.layer.typename === "ArtLayer")) {
        return;
    }
    if (!this.layer.visible) {
        return;
    }
    c = this.getInfo();
    if (c) {
        this.isValidate = true;
        this.flatten(c);
        c.offset = this.getOffset(c.options);
        c.size = this.getSize(c.options);
        this.node = new Node(c);
    } else {
        if (this.layer.typename === "LayerSet") {
            this.isValidate = true;
            this.node = new Node({
                type: "group",
                name: this.layer.name
            });
        }
    }
};
Processor.prototype.getInfo = function() {
    g = function(a) {
        return function(a) {
            b = a.match(/^(.+)=(.+?)(\[(.+)\])?$/);
            if (!b) {
                return false;
            }
            return {
                name: b[1],
                typeString: b[2],
                optionsString: b[4]
            };
        };
    }(this);
    i = function(a) {
        return function(b) {
            b = b.toLowerCase();
            if (b === "png") {
                return ["png"];
            }
            h = b.match(/^jpg(:(\d+))?$/);
            if (h) {
                j = parseInt(h[2], 10);
                j = j && 0 < j && j <= 12 ? j : 12;
                return ["jpg", {
                    jpgQuality: j
                }];
            }
            if (b === "text") {
                if (a.layer.kind !== LayerKind.TEXT) {
                    alert("Error\nInvalid text layer.\n" + a.layer.name);
                    return false;
                }
                o = a.layer.textItem;
                l = Helper.getTextSize(a.layer);
                d = Helper.getTextColor(a.layer);
                e = Helper.getTextHorizontalWrap(a.layer);
                return ["text", {
                    opacity: parseFloat(a.layer.opacity),
                    textContents: o.contents,
                    textFont: o.font,
                    textSize: l,
                    textColor: d,
                    textHorizontalWrap: e
                }];
            }
            if (b === "button") {
                if (a.layer.typename !== "LayerSet") {
                    alert("Error\nInvalid button group.\n" + a.layer.name);
                    return false;
                }
                c = {};
                k = a.layer.layers;
                for (f = 0, g = k.length; f < g; f++) {
                    n = k[f];
                    i = n.name.toLowerCase();
                    if (i === "normal" || i === "highlighted" || i === "pressed" || i === "disabled") {
                        c[i] = true;
                        n.name = i;
                    }
                }
                if (!c.normal) {
                    alert("Error\nButton must contains \"normal\" state.\n" + a.layer.name);
                    return false;
                }
                return ["button", {
                    buttonStates: function() {
                        a = [];
                        for (var m in c) {
                            a.push(m);
                        }
                        return a;
                    }()
                }];
            }
            return false;
        };
    }(this);
    d = function(a) {
        return function(b) {
            h = {};
            if (!b) {
                return h;
            }
            c = {
                l: "left",
                c: "center",
                r: "right",
                s: "stretch"
            };
            d = {
                t: "top",
                m: "middle",
                b: "bottom",
                s: "stretch"
            };
            j = {
                l: "left",
                c: "center",
                r: "right"
            };
            k = {
                t: "top",
                m: "middle",
                b: "bottom"
            };
            l = b.split(",");
            for (e = 0, g = l.length; e < g; e++) {
                i = l[e];
                m = i.split(":");
                f = m[0];
                n = m[1];
                f = f.replace(/(^\s+)|(\s+$)/, "").toLowerCase();
                n = n.replace(/(^\s+)|(\s+$)/, "");
                if (!(f && n)) {
                    continue;
                }
                if (f === "anchor") {
                    n = n.toLowerCase();
                    if (!/^[lcrs][tmbs]$/.test(n)) {
                        alert("Error\nInvalid anchor.\n" + a.layer.name);
                        continue;
                    }
                    h.anchor = {
                        x: c[n[0]],
                        y: d[n[1]]
                    };
                    continue;
                }
                if (f === "pivot") {
                    n = n.toLowerCase();
                    if (!/^[lcr][tmb]$/.test(n)) {
                        alert("Error\nInvalid pivot.\n" + a.layer.name);
                        continue;
                    }
                    h.pivot = {
                        x: j[n[0]],
                        y: k[n[1]]
                    };
                    continue;
                }
                if (f === "padding") {
                    n = n.split(/\s+/);
                    h.padding = {
                        top: parseInt(n[0], 10) || 0,
                        right: parseInt(n[1] || n[0], 10) || 0,
                        bottom: parseInt(n[2] || n[0], 10) || 0,
                        left: parseInt(n[3] || n[1] || n[0], 10) || 0
                    };
                    continue;
                }
                if (f === "packingtag") {
                    h.packingTag = n;
                    continue;
                }
            }
            return h;
        };
    }(this);
    e = g(this.layer.name);
    if (!e) {
        return false;
    }
    f = i(e.typeString);
    h = f[0];
    a = f[1];
    if (!h) {
        return false;
    }
    c = d(e && e.optionsString);
    if (a) {
        for (var b in a) {
            j = a[b];
            c[b] = j;
        }
    }
    return {
        name: e.name,
        type: h,
        options: c
    };
};
Processor.prototype.flatten = function(a) {
    b = function(a) {
        return function(b) {
            app.activeDocument = a.document;
            if (b.typename === "LayerSet") {
                b = b.merge();
            } else {
                c = a.document.layerSets.add();
                c.move(b, ElementPlacement.PLACEAFTER);
                b.move(c, ElementPlacement.INSIDE);
                c.name = b.name;
                b = c.merge();
            }
            return b;
        };
    }(this);
    if (a.type === "button") {
        e = a.options.buttonStates;
        for (c = 0, d = e.length; c < d; c++) {
            f = e[c];
            g = this.layer.layers.getByName(f);
            b(g);
        }
    } else {
        this.layer = b(this.layer);
    }
};
Processor.prototype.getOffset = function(a) {
    c = this.layer.bounds;
    for (d = e = 0, g = c.length; e < g; d = ++e) {
        b = c[d];
        c[d] = parseFloat(b);
    }
    f = c[0];
    h = c[1];
    if (a.padding) {
        f -= a.padding.left;
        h -= a.padding.top;
    }
    return {
        left: f,
        top: h
    };
};
Processor.prototype.getSize = function(a) {
    c = this.layer.bounds;
    for (e = f = 0, g = c.length; f < g; e = ++f) {
        b = c[e];
        c[e] = parseFloat(b);
    }
    h = c[2] - c[0];
    d = c[3] - c[1];
    if (a.padding) {
        h += a.padding.left + a.padding.right;
        d += a.padding.top + a.padding.bottom;
    }
    return {
        width: h,
        height: d
    };
};
Processor.prototype.export = function(a) {
    if (this.node.type === "text") {
        return;
    }
    b = function(b) {
        return function(c, d) {
            // e = app.documents.add(b.node.size.width, b.node.size.height, 72, "Layer to Export", NewDocumentMode.RGB, DocumentFill.TRANSPARENT);
            e = app.documents.add(b.node.size.width, b.node.size.height, 72, "Layer to Export", NewDocumentMode.RGB, DocumentFill.NORMAL);
            app.activeDocument = b.document;
            f = c.duplicate(e, ElementPlacement.INSIDE);
            j = -f.bounds[0];
            k = -f.bounds[1];
            if (b.node.options.padding) {
                j += b.node.options.padding.left;
                k += b.node.options.padding.top;
            }
            app.activeDocument = e;
            f.translate(j, k);
            i = void(0);
            if (b.node.type === "png" || b.node.type === "button") {
                i = new PNGSaveOptions();
            } else if (b.node.type === "jpg") {
                i = new JPEGSaveOptions();
                i.quality = b.node.options.jpgQuality || 12;
            } else {
                alert("Error\nInvalid type.\n" + b.layer.name);
                return;
            }
            h = a + "/" + b.node.relativePath;
            if (d) {
                h += d;
            }
            g = Folder(h.replace(/(.+)\/.+$/, "$1"));
            if (!g.exists) {
                g.create();
            }
            e.saveAs(File(h), i, true, Extension.LOWERCASE);
            e.close(SaveOptions.DONOTSAVECHANGES);
        };
    }(this);
    if (this.node.type === "button") {
        e = this.node.options.buttonStates;
        for (c = 0, d = e.length; c < d; c++) {
            f = e[c];
            g = this.layer.layers.getByName(f);
            b(g, "--" + f);
        }
    } else {
        b(this.layer);
    }
};
setting = {
    structureSuffix: "-structure",
    fontsListSuffix: "-fonts",
    assetsFolderSuffix: "-assets"
};
C = {
    documentFilePath: null,
    assetsRootPath: null,
    _document: null,
    digest: function() {
        a = C;
        if (!a.checkEnviroment()) {
            return;
        }
        if (!a.prepareForExport()) {
            return;
        }
        alert("Get Ready\nThe process may take some time. Please be patient.\nClick [OK] to continue.");
        b = app.preferences.rulerUnits;
        c = app.preferences.typeUnits;
        app.preferences.rulerUnits = Units.PIXELS;
        app.preferences.typeUnits = TypeUnits.PIXELS;
        a.process();
        app.preferences.rulerUnits = b;
        app.preferences.typeUnits = c;
        alert("Success\nAll assets has been exported. Open Unity3D to continue.");
    },
    checkEnviroment: function() {
        if (app.documents.length === 0) {
            alert("Error\nPlease open a file.");
            return false;
        }
        if (!app.activeDocument.saved) {
            a = confirm("Warn\nThis file hasn't been saved. Would you like to continue?");
            if (!a) {
                return false;
            }
        }
        return true;
    },
    prepareForExport: function() {
        a = C;
        a.documentFilePath = app.activeDocument.fullName;
        a.assetsRootPath = app.activeDocument.fullName + setting.assetsFolderSuffix;
        b = Folder(a.assetsRootPath);
        if (b.exists) {
            return confirm("Warn\nThe target folder exists. Would you like to overwrite?");
        }
        return true;
    },
    process: function() {
        b = C;
        b._document = app.activeDocument.duplicate();
        c = new Node({
            type: "root",
            size: {
                width: b._document.width.value,
                height: b._document.height.value
            }
        });
        a = {};

        function(c, d) {
            g = arguments.callee;
            if (c.typename === "Layers") {
                for (h = 0, i = c.length; h < i; h++) {
                    k = c[h];
                    g(k, d);
                }
                return;
            }
            j = new Processor(b._document, c);
            if (j.isValidate) {
                e = j.node;
                e.appendTo(d);
                f = e.options.textFont;
                if (f) {
                    a[f] = true;
                }
                if (e.type === "group") {
                    return g(c.layers, e);
                } else {
                    return j.export(b.assetsRootPath);
                }
            }
        }(b._document.layers, c);
        c.pruneEmptyGroups();
        b.saveStructureFile(c);
        b.saveFontsListFile(a);
        b._document.close(SaveOptions.DONOTSAVECHANGES);
    },
    saveStructureFile: function(a) {
        c = C;
        b = new File("" + c.documentFilePath + setting.structureSuffix + ".json");
        b.encoding = "UTF8";
        b.type = "TEXT";
        b.lineFeed = c.getLineFeed();
        b.open("w");
        b.writeln(JSON.stringify(a.dump()));
        b.close();
    },
    saveFontsListFile: function(a) {
        f = C;
        e = function() {
            b = [];
            for (var d in a) {
                b.push(d);
            }
            return b;
        }();
        if (!(e.length > 0)) {
            return;
        }
        c = "" + f.documentFilePath + setting.fontsListSuffix + ".txt";
        b = new File(c);
        b.encoding = "UTF8";
        b.type = "TEXT";
        b.lineFeed = f.getLineFeed();
        b.open("w");
        b.writeln("Please copy these fonts to your project's assets folder, and rename them exactly as below:\n");
        b.writeln(e.join("\n"));
        b.close();
        alert("Font List Gerenated\nPlease check \"" + c + "\" for instruction.");
    },
    getLineFeed: function() {
        if (/windows/i.test($.os)) {
            return "Windows";
        } else {
            return "Unix";
        }
    }
};
C.digest();