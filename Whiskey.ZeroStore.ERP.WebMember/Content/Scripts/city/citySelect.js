/*
Copyright 2012 Igor Vaynberg

Version: 3.5.2 Timestamp: Sat Nov  1 14:43:36 EDT 2014

This software is licensed under the Apache License, Version 2.0 (the "Apache License") or the GNU
General Public License version 2 (the "GPL License"). You may choose either license to govern your
use of this software only upon the condition that you accept all of the terms of either the Apache
License or the GPL License.

You may obtain a copy of the Apache License and the GPL License at:

    http://www.apache.org/licenses/LICENSE-2.0
    http://www.gnu.org/licenses/gpl-2.0.html

Unless required by applicable law or agreed to in writing, software distributed under the
Apache License or the GPL License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied. See the Apache License and the GPL License for
the specific language governing permissions and limitations under the Apache License and the GPL License.
*/
(function ($) {
    if(typeof $.fn.each2 == "undefined") {
        $.extend($.fn, {
            /*
            * 4-10 times faster .each replacement
            * use it carefully, as it overrides jQuery context of element on each iteration
            */
            each2 : function (c) {
                var j = $([0]), i = -1, l = this.length;
                while (
                    ++i < l
                    && (j.context = j[0] = this[i])
                    && c.call(j[0], i, j) !== false //"this"=DOM, i=index, j=jQuery object
                );
                return this;
            }
        });
    }
})(jQuery);

(function ($, undefined) {
    "use strict";
    /*global document, window, jQuery, console */

    if (window.Select2 !== undefined) {
        return;
    }

    var AbstractSelect2, SingleSelect2, MultiSelect2, nextUid, sizer,
        lastMousePosition={x:0,y:0}, $document, scrollBarDimensions,

    KEY = {
        TAB: 9,
        ENTER: 13,
        ESC: 27,
        SPACE: 32,
        LEFT: 37,
        UP: 38,
        RIGHT: 39,
        DOWN: 40,
        SHIFT: 16,
        CTRL: 17,
        ALT: 18,
        PAGE_UP: 33,
        PAGE_DOWN: 34,
        HOME: 36,
        END: 35,
        BACKSPACE: 8,
        DELETE: 46,
        isArrow: function (k) {
            k = k.which ? k.which : k;
            switch (k) {
            case KEY.LEFT:
            case KEY.RIGHT:
            case KEY.UP:
            case KEY.DOWN:
                return true;
            }
            return false;
        },
        isControl: function (e) {
            var k = e.which;
            switch (k) {
            case KEY.SHIFT:
            case KEY.CTRL:
            case KEY.ALT:
                return true;
            }

            if (e.metaKey) return true;

            return false;
        },
        isFunctionKey: function (k) {
            k = k.which ? k.which : k;
            return k >= 112 && k <= 123;
        }
    },
    MEASURE_SCROLLBAR_TEMPLATE = "<div class='select2-measure-scrollbar'></div>",

    DIACRITICS = {"\u24B6":"A","\uFF21":"A","\u00C0":"A","\u00C1":"A","\u00C2":"A","\u1EA6":"A","\u1EA4":"A","\u1EAA":"A","\u1EA8":"A","\u00C3":"A","\u0100":"A","\u0102":"A","\u1EB0":"A","\u1EAE":"A","\u1EB4":"A","\u1EB2":"A","\u0226":"A","\u01E0":"A","\u00C4":"A","\u01DE":"A","\u1EA2":"A","\u00C5":"A","\u01FA":"A","\u01CD":"A","\u0200":"A","\u0202":"A","\u1EA0":"A","\u1EAC":"A","\u1EB6":"A","\u1E00":"A","\u0104":"A","\u023A":"A","\u2C6F":"A","\uA732":"AA","\u00C6":"AE","\u01FC":"AE","\u01E2":"AE","\uA734":"AO","\uA736":"AU","\uA738":"AV","\uA73A":"AV","\uA73C":"AY","\u24B7":"B","\uFF22":"B","\u1E02":"B","\u1E04":"B","\u1E06":"B","\u0243":"B","\u0182":"B","\u0181":"B","\u24B8":"C","\uFF23":"C","\u0106":"C","\u0108":"C","\u010A":"C","\u010C":"C","\u00C7":"C","\u1E08":"C","\u0187":"C","\u023B":"C","\uA73E":"C","\u24B9":"D","\uFF24":"D","\u1E0A":"D","\u010E":"D","\u1E0C":"D","\u1E10":"D","\u1E12":"D","\u1E0E":"D","\u0110":"D","\u018B":"D","\u018A":"D","\u0189":"D","\uA779":"D","\u01F1":"DZ","\u01C4":"DZ","\u01F2":"Dz","\u01C5":"Dz","\u24BA":"E","\uFF25":"E","\u00C8":"E","\u00C9":"E","\u00CA":"E","\u1EC0":"E","\u1EBE":"E","\u1EC4":"E","\u1EC2":"E","\u1EBC":"E","\u0112":"E","\u1E14":"E","\u1E16":"E","\u0114":"E","\u0116":"E","\u00CB":"E","\u1EBA":"E","\u011A":"E","\u0204":"E","\u0206":"E","\u1EB8":"E","\u1EC6":"E","\u0228":"E","\u1E1C":"E","\u0118":"E","\u1E18":"E","\u1E1A":"E","\u0190":"E","\u018E":"E","\u24BB":"F","\uFF26":"F","\u1E1E":"F","\u0191":"F","\uA77B":"F","\u24BC":"G","\uFF27":"G","\u01F4":"G","\u011C":"G","\u1E20":"G","\u011E":"G","\u0120":"G","\u01E6":"G","\u0122":"G","\u01E4":"G","\u0193":"G","\uA7A0":"G","\uA77D":"G","\uA77E":"G","\u24BD":"H","\uFF28":"H","\u0124":"H","\u1E22":"H","\u1E26":"H","\u021E":"H","\u1E24":"H","\u1E28":"H","\u1E2A":"H","\u0126":"H","\u2C67":"H","\u2C75":"H","\uA78D":"H","\u24BE":"I","\uFF29":"I","\u00CC":"I","\u00CD":"I","\u00CE":"I","\u0128":"I","\u012A":"I","\u012C":"I","\u0130":"I","\u00CF":"I","\u1E2E":"I","\u1EC8":"I","\u01CF":"I","\u0208":"I","\u020A":"I","\u1ECA":"I","\u012E":"I","\u1E2C":"I","\u0197":"I","\u24BF":"J","\uFF2A":"J","\u0134":"J","\u0248":"J","\u24C0":"K","\uFF2B":"K","\u1E30":"K","\u01E8":"K","\u1E32":"K","\u0136":"K","\u1E34":"K","\u0198":"K","\u2C69":"K","\uA740":"K","\uA742":"K","\uA744":"K","\uA7A2":"K","\u24C1":"L","\uFF2C":"L","\u013F":"L","\u0139":"L","\u013D":"L","\u1E36":"L","\u1E38":"L","\u013B":"L","\u1E3C":"L","\u1E3A":"L","\u0141":"L","\u023D":"L","\u2C62":"L","\u2C60":"L","\uA748":"L","\uA746":"L","\uA780":"L","\u01C7":"LJ","\u01C8":"Lj","\u24C2":"M","\uFF2D":"M","\u1E3E":"M","\u1E40":"M","\u1E42":"M","\u2C6E":"M","\u019C":"M","\u24C3":"N","\uFF2E":"N","\u01F8":"N","\u0143":"N","\u00D1":"N","\u1E44":"N","\u0147":"N","\u1E46":"N","\u0145":"N","\u1E4A":"N","\u1E48":"N","\u0220":"N","\u019D":"N","\uA790":"N","\uA7A4":"N","\u01CA":"NJ","\u01CB":"Nj","\u24C4":"O","\uFF2F":"O","\u00D2":"O","\u00D3":"O","\u00D4":"O","\u1ED2":"O","\u1ED0":"O","\u1ED6":"O","\u1ED4":"O","\u00D5":"O","\u1E4C":"O","\u022C":"O","\u1E4E":"O","\u014C":"O","\u1E50":"O","\u1E52":"O","\u014E":"O","\u022E":"O","\u0230":"O","\u00D6":"O","\u022A":"O","\u1ECE":"O","\u0150":"O","\u01D1":"O","\u020C":"O","\u020E":"O","\u01A0":"O","\u1EDC":"O","\u1EDA":"O","\u1EE0":"O","\u1EDE":"O","\u1EE2":"O","\u1ECC":"O","\u1ED8":"O","\u01EA":"O","\u01EC":"O","\u00D8":"O","\u01FE":"O","\u0186":"O","\u019F":"O","\uA74A":"O","\uA74C":"O","\u01A2":"OI","\uA74E":"OO","\u0222":"OU","\u24C5":"P","\uFF30":"P","\u1E54":"P","\u1E56":"P","\u01A4":"P","\u2C63":"P","\uA750":"P","\uA752":"P","\uA754":"P","\u24C6":"Q","\uFF31":"Q","\uA756":"Q","\uA758":"Q","\u024A":"Q","\u24C7":"R","\uFF32":"R","\u0154":"R","\u1E58":"R","\u0158":"R","\u0210":"R","\u0212":"R","\u1E5A":"R","\u1E5C":"R","\u0156":"R","\u1E5E":"R","\u024C":"R","\u2C64":"R","\uA75A":"R","\uA7A6":"R","\uA782":"R","\u24C8":"S","\uFF33":"S","\u1E9E":"S","\u015A":"S","\u1E64":"S","\u015C":"S","\u1E60":"S","\u0160":"S","\u1E66":"S","\u1E62":"S","\u1E68":"S","\u0218":"S","\u015E":"S","\u2C7E":"S","\uA7A8":"S","\uA784":"S","\u24C9":"T","\uFF34":"T","\u1E6A":"T","\u0164":"T","\u1E6C":"T","\u021A":"T","\u0162":"T","\u1E70":"T","\u1E6E":"T","\u0166":"T","\u01AC":"T","\u01AE":"T","\u023E":"T","\uA786":"T","\uA728":"TZ","\u24CA":"U","\uFF35":"U","\u00D9":"U","\u00DA":"U","\u00DB":"U","\u0168":"U","\u1E78":"U","\u016A":"U","\u1E7A":"U","\u016C":"U","\u00DC":"U","\u01DB":"U","\u01D7":"U","\u01D5":"U","\u01D9":"U","\u1EE6":"U","\u016E":"U","\u0170":"U","\u01D3":"U","\u0214":"U","\u0216":"U","\u01AF":"U","\u1EEA":"U","\u1EE8":"U","\u1EEE":"U","\u1EEC":"U","\u1EF0":"U","\u1EE4":"U","\u1E72":"U","\u0172":"U","\u1E76":"U","\u1E74":"U","\u0244":"U","\u24CB":"V","\uFF36":"V","\u1E7C":"V","\u1E7E":"V","\u01B2":"V","\uA75E":"V","\u0245":"V","\uA760":"VY","\u24CC":"W","\uFF37":"W","\u1E80":"W","\u1E82":"W","\u0174":"W","\u1E86":"W","\u1E84":"W","\u1E88":"W","\u2C72":"W","\u24CD":"X","\uFF38":"X","\u1E8A":"X","\u1E8C":"X","\u24CE":"Y","\uFF39":"Y","\u1EF2":"Y","\u00DD":"Y","\u0176":"Y","\u1EF8":"Y","\u0232":"Y","\u1E8E":"Y","\u0178":"Y","\u1EF6":"Y","\u1EF4":"Y","\u01B3":"Y","\u024E":"Y","\u1EFE":"Y","\u24CF":"Z","\uFF3A":"Z","\u0179":"Z","\u1E90":"Z","\u017B":"Z","\u017D":"Z","\u1E92":"Z","\u1E94":"Z","\u01B5":"Z","\u0224":"Z","\u2C7F":"Z","\u2C6B":"Z","\uA762":"Z","\u24D0":"a","\uFF41":"a","\u1E9A":"a","\u00E0":"a","\u00E1":"a","\u00E2":"a","\u1EA7":"a","\u1EA5":"a","\u1EAB":"a","\u1EA9":"a","\u00E3":"a","\u0101":"a","\u0103":"a","\u1EB1":"a","\u1EAF":"a","\u1EB5":"a","\u1EB3":"a","\u0227":"a","\u01E1":"a","\u00E4":"a","\u01DF":"a","\u1EA3":"a","\u00E5":"a","\u01FB":"a","\u01CE":"a","\u0201":"a","\u0203":"a","\u1EA1":"a","\u1EAD":"a","\u1EB7":"a","\u1E01":"a","\u0105":"a","\u2C65":"a","\u0250":"a","\uA733":"aa","\u00E6":"ae","\u01FD":"ae","\u01E3":"ae","\uA735":"ao","\uA737":"au","\uA739":"av","\uA73B":"av","\uA73D":"ay","\u24D1":"b","\uFF42":"b","\u1E03":"b","\u1E05":"b","\u1E07":"b","\u0180":"b","\u0183":"b","\u0253":"b","\u24D2":"c","\uFF43":"c","\u0107":"c","\u0109":"c","\u010B":"c","\u010D":"c","\u00E7":"c","\u1E09":"c","\u0188":"c","\u023C":"c","\uA73F":"c","\u2184":"c","\u24D3":"d","\uFF44":"d","\u1E0B":"d","\u010F":"d","\u1E0D":"d","\u1E11":"d","\u1E13":"d","\u1E0F":"d","\u0111":"d","\u018C":"d","\u0256":"d","\u0257":"d","\uA77A":"d","\u01F3":"dz","\u01C6":"dz","\u24D4":"e","\uFF45":"e","\u00E8":"e","\u00E9":"e","\u00EA":"e","\u1EC1":"e","\u1EBF":"e","\u1EC5":"e","\u1EC3":"e","\u1EBD":"e","\u0113":"e","\u1E15":"e","\u1E17":"e","\u0115":"e","\u0117":"e","\u00EB":"e","\u1EBB":"e","\u011B":"e","\u0205":"e","\u0207":"e","\u1EB9":"e","\u1EC7":"e","\u0229":"e","\u1E1D":"e","\u0119":"e","\u1E19":"e","\u1E1B":"e","\u0247":"e","\u025B":"e","\u01DD":"e","\u24D5":"f","\uFF46":"f","\u1E1F":"f","\u0192":"f","\uA77C":"f","\u24D6":"g","\uFF47":"g","\u01F5":"g","\u011D":"g","\u1E21":"g","\u011F":"g","\u0121":"g","\u01E7":"g","\u0123":"g","\u01E5":"g","\u0260":"g","\uA7A1":"g","\u1D79":"g","\uA77F":"g","\u24D7":"h","\uFF48":"h","\u0125":"h","\u1E23":"h","\u1E27":"h","\u021F":"h","\u1E25":"h","\u1E29":"h","\u1E2B":"h","\u1E96":"h","\u0127":"h","\u2C68":"h","\u2C76":"h","\u0265":"h","\u0195":"hv","\u24D8":"i","\uFF49":"i","\u00EC":"i","\u00ED":"i","\u00EE":"i","\u0129":"i","\u012B":"i","\u012D":"i","\u00EF":"i","\u1E2F":"i","\u1EC9":"i","\u01D0":"i","\u0209":"i","\u020B":"i","\u1ECB":"i","\u012F":"i","\u1E2D":"i","\u0268":"i","\u0131":"i","\u24D9":"j","\uFF4A":"j","\u0135":"j","\u01F0":"j","\u0249":"j","\u24DA":"k","\uFF4B":"k","\u1E31":"k","\u01E9":"k","\u1E33":"k","\u0137":"k","\u1E35":"k","\u0199":"k","\u2C6A":"k","\uA741":"k","\uA743":"k","\uA745":"k","\uA7A3":"k","\u24DB":"l","\uFF4C":"l","\u0140":"l","\u013A":"l","\u013E":"l","\u1E37":"l","\u1E39":"l","\u013C":"l","\u1E3D":"l","\u1E3B":"l","\u017F":"l","\u0142":"l","\u019A":"l","\u026B":"l","\u2C61":"l","\uA749":"l","\uA781":"l","\uA747":"l","\u01C9":"lj","\u24DC":"m","\uFF4D":"m","\u1E3F":"m","\u1E41":"m","\u1E43":"m","\u0271":"m","\u026F":"m","\u24DD":"n","\uFF4E":"n","\u01F9":"n","\u0144":"n","\u00F1":"n","\u1E45":"n","\u0148":"n","\u1E47":"n","\u0146":"n","\u1E4B":"n","\u1E49":"n","\u019E":"n","\u0272":"n","\u0149":"n","\uA791":"n","\uA7A5":"n","\u01CC":"nj","\u24DE":"o","\uFF4F":"o","\u00F2":"o","\u00F3":"o","\u00F4":"o","\u1ED3":"o","\u1ED1":"o","\u1ED7":"o","\u1ED5":"o","\u00F5":"o","\u1E4D":"o","\u022D":"o","\u1E4F":"o","\u014D":"o","\u1E51":"o","\u1E53":"o","\u014F":"o","\u022F":"o","\u0231":"o","\u00F6":"o","\u022B":"o","\u1ECF":"o","\u0151":"o","\u01D2":"o","\u020D":"o","\u020F":"o","\u01A1":"o","\u1EDD":"o","\u1EDB":"o","\u1EE1":"o","\u1EDF":"o","\u1EE3":"o","\u1ECD":"o","\u1ED9":"o","\u01EB":"o","\u01ED":"o","\u00F8":"o","\u01FF":"o","\u0254":"o","\uA74B":"o","\uA74D":"o","\u0275":"o","\u01A3":"oi","\u0223":"ou","\uA74F":"oo","\u24DF":"p","\uFF50":"p","\u1E55":"p","\u1E57":"p","\u01A5":"p","\u1D7D":"p","\uA751":"p","\uA753":"p","\uA755":"p","\u24E0":"q","\uFF51":"q","\u024B":"q","\uA757":"q","\uA759":"q","\u24E1":"r","\uFF52":"r","\u0155":"r","\u1E59":"r","\u0159":"r","\u0211":"r","\u0213":"r","\u1E5B":"r","\u1E5D":"r","\u0157":"r","\u1E5F":"r","\u024D":"r","\u027D":"r","\uA75B":"r","\uA7A7":"r","\uA783":"r","\u24E2":"s","\uFF53":"s","\u00DF":"s","\u015B":"s","\u1E65":"s","\u015D":"s","\u1E61":"s","\u0161":"s","\u1E67":"s","\u1E63":"s","\u1E69":"s","\u0219":"s","\u015F":"s","\u023F":"s","\uA7A9":"s","\uA785":"s","\u1E9B":"s","\u24E3":"t","\uFF54":"t","\u1E6B":"t","\u1E97":"t","\u0165":"t","\u1E6D":"t","\u021B":"t","\u0163":"t","\u1E71":"t","\u1E6F":"t","\u0167":"t","\u01AD":"t","\u0288":"t","\u2C66":"t","\uA787":"t","\uA729":"tz","\u24E4":"u","\uFF55":"u","\u00F9":"u","\u00FA":"u","\u00FB":"u","\u0169":"u","\u1E79":"u","\u016B":"u","\u1E7B":"u","\u016D":"u","\u00FC":"u","\u01DC":"u","\u01D8":"u","\u01D6":"u","\u01DA":"u","\u1EE7":"u","\u016F":"u","\u0171":"u","\u01D4":"u","\u0215":"u","\u0217":"u","\u01B0":"u","\u1EEB":"u","\u1EE9":"u","\u1EEF":"u","\u1EED":"u","\u1EF1":"u","\u1EE5":"u","\u1E73":"u","\u0173":"u","\u1E77":"u","\u1E75":"u","\u0289":"u","\u24E5":"v","\uFF56":"v","\u1E7D":"v","\u1E7F":"v","\u028B":"v","\uA75F":"v","\u028C":"v","\uA761":"vy","\u24E6":"w","\uFF57":"w","\u1E81":"w","\u1E83":"w","\u0175":"w","\u1E87":"w","\u1E85":"w","\u1E98":"w","\u1E89":"w","\u2C73":"w","\u24E7":"x","\uFF58":"x","\u1E8B":"x","\u1E8D":"x","\u24E8":"y","\uFF59":"y","\u1EF3":"y","\u00FD":"y","\u0177":"y","\u1EF9":"y","\u0233":"y","\u1E8F":"y","\u00FF":"y","\u1EF7":"y","\u1E99":"y","\u1EF5":"y","\u01B4":"y","\u024F":"y","\u1EFF":"y","\u24E9":"z","\uFF5A":"z","\u017A":"z","\u1E91":"z","\u017C":"z","\u017E":"z","\u1E93":"z","\u1E95":"z","\u01B6":"z","\u0225":"z","\u0240":"z","\u2C6C":"z","\uA763":"z","\u0386":"\u0391","\u0388":"\u0395","\u0389":"\u0397","\u038A":"\u0399","\u03AA":"\u0399","\u038C":"\u039F","\u038E":"\u03A5","\u03AB":"\u03A5","\u038F":"\u03A9","\u03AC":"\u03B1","\u03AD":"\u03B5","\u03AE":"\u03B7","\u03AF":"\u03B9","\u03CA":"\u03B9","\u0390":"\u03B9","\u03CC":"\u03BF","\u03CD":"\u03C5","\u03CB":"\u03C5","\u03B0":"\u03C5","\u03C9":"\u03C9","\u03C2":"\u03C3"};

    $document = $(document);

    nextUid=(function() { var counter=1; return function() { return counter++; }; }());


    function reinsertElement(element) {
        var placeholder = $(document.createTextNode(''));

        element.before(placeholder);
        placeholder.before(element);
        placeholder.remove();
    }

    function stripDiacritics(str) {
        // Used 'uni range + named function' from http://jsperf.com/diacritics/18
        function match(a) {
            return DIACRITICS[a] || a;
        }

        return str.replace(/[^\u0000-\u007E]/g, match);
    }

    function indexOf(value, array) {
        var i = 0, l = array.length;
        for (; i < l; i = i + 1) {
            if (equal(value, array[i])) return i;
        }
        return -1;
    }

    function measureScrollbar () {
        var $template = $( MEASURE_SCROLLBAR_TEMPLATE );
        $template.appendTo(document.body);

        var dim = {
            width: $template.width() - $template[0].clientWidth,
            height: $template.height() - $template[0].clientHeight
        };
        $template.remove();

        return dim;
    }

    /**
     * Compares equality of a and b
     * @param a
     * @param b
     */
    function equal(a, b) {
        if (a === b) return true;
        if (a === undefined || b === undefined) return false;
        if (a === null || b === null) return false;
        // Check whether 'a' or 'b' is a string (primitive or object).
        // The concatenation of an empty string (+'') converts its argument to a string's primitive.
        if (a.constructor === String) return a+'' === b+''; // a+'' - in case 'a' is a String object
        if (b.constructor === String) return b+'' === a+''; // b+'' - in case 'b' is a String object
        return false;
    }

    /**
     * Splits the string into an array of values, transforming each value. An empty array is returned for nulls or empty
     * strings
     * @param string
     * @param separator
     */
    function splitVal(string, separator, transform) {
        var val, i, l;
        if (string === null || string.length < 1) return [];
        val = string.split(separator);
        for (i = 0, l = val.length; i < l; i = i + 1) val[i] = transform(val[i]);
        return val;
    }

    function getSideBorderPadding(element) {
        return element.outerWidth(false) - element.width();
    }

    function installKeyUpChangeEvent(element) {
        var key="keyup-change-value";
        element.on("keydown", function () {
            if ($.data(element, key) === undefined) {
                $.data(element, key, element.val());
            }
        });
        element.on("keyup", function () {
            var val= $.data(element, key);
            if (val !== undefined && element.val() !== val) {
                $.removeData(element, key);
                element.trigger("keyup-change");
            }
        });
    }


    /**
     * filters mouse events so an event is fired only if the mouse moved.
     *
     * filters out mouse events that occur when mouse is stationary but
     * the elements under the pointer are scrolled.
     */
    function installFilteredMouseMove(element) {
        element.on("mousemove", function (e) {
            var lastpos = lastMousePosition;
            if (lastpos === undefined || lastpos.x !== e.pageX || lastpos.y !== e.pageY) {
                $(e.target).trigger("mousemove-filtered", e);
            }
        });
    }

    /**
     * Debounces a function. Returns a function that calls the original fn function only if no invocations have been made
     * within the last quietMillis milliseconds.
     *
     * @param quietMillis number of milliseconds to wait before invoking fn
     * @param fn function to be debounced
     * @param ctx object to be used as this reference within fn
     * @return debounced version of fn
     */
    function debounce(quietMillis, fn, ctx) {
        ctx = ctx || undefined;
        var timeout;
        return function () {
            var args = arguments;
            window.clearTimeout(timeout);
            timeout = window.setTimeout(function() {
                fn.apply(ctx, args);
            }, quietMillis);
        };
    }

    function installDebouncedScroll(threshold, element) {
        var notify = debounce(threshold, function (e) { element.trigger("scroll-debounced", e);});
        element.on("scroll", function (e) {
            if (indexOf(e.target, element.get()) >= 0) notify(e);
        });
    }

    function focus($el) {
        if ($el[0] === document.activeElement) return;

        /* set the focus in a 0 timeout - that way the focus is set after the processing
            of the current event has finished - which seems like the only reliable way
            to set focus */
        window.setTimeout(function() {
            var el=$el[0], pos=$el.val().length, range;

            $el.focus();

            /* make sure el received focus so we do not error out when trying to manipulate the caret.
                sometimes modals or others listeners may steal it after its set */
            var isVisible = (el.offsetWidth > 0 || el.offsetHeight > 0);
            if (isVisible && el === document.activeElement) {

                /* after the focus is set move the caret to the end, necessary when we val()
                    just before setting focus */
                if(el.setSelectionRange)
                {
                    el.setSelectionRange(pos, pos);
                }
                else if (el.createTextRange) {
                    range = el.createTextRange();
                    range.collapse(false);
                    range.select();
                }
            }
        }, 0);
    }

    function getCursorInfo(el) {
        el = $(el)[0];
        var offset = 0;
        var length = 0;
        if ('selectionStart' in el) {
            offset = el.selectionStart;
            length = el.selectionEnd - offset;
        } else if ('selection' in document) {
            el.focus();
            var sel = document.selection.createRange();
            length = document.selection.createRange().text.length;
            sel.moveStart('character', -el.value.length);
            offset = sel.text.length - length;
        }
        return { offset: offset, length: length };
    }

    function killEvent(event) {
        event.preventDefault();
        event.stopPropagation();
    }
    function killEventImmediately(event) {
        event.preventDefault();
        event.stopImmediatePropagation();
    }

    function measureTextWidth(e) {
        if (!sizer){
            var style = e[0].currentStyle || window.getComputedStyle(e[0], null);
            sizer = $(document.createElement("div")).css({
                position: "absolute",
                left: "-10000px",
                top: "-10000px",
                display: "none",
                fontSize: style.fontSize,
                fontFamily: style.fontFamily,
                fontStyle: style.fontStyle,
                fontWeight: style.fontWeight,
                letterSpacing: style.letterSpacing,
                textTransform: style.textTransform,
                whiteSpace: "nowrap"
            });
            sizer.attr("class","select2-sizer");
            $(document.body).append(sizer);
        }
        sizer.text(e.val());
        return sizer.width();
    }

    function syncCssClasses(dest, src, adapter) {
        var classes, replacements = [], adapted;

        classes = $.trim(dest.attr("class"));

        if (classes) {
            classes = '' + classes; // for IE which returns object

            $(classes.split(/\s+/)).each2(function() {
                if (this.indexOf("select2-") === 0) {
                    replacements.push(this);
                }
            });
        }

        classes = $.trim(src.attr("class"));

        if (classes) {
            classes = '' + classes; // for IE which returns object

            $(classes.split(/\s+/)).each2(function() {
                if (this.indexOf("select2-") !== 0) {
                    adapted = adapter(this);

                    if (adapted) {
                        replacements.push(adapted);
                    }
                }
            });
        }

        dest.attr("class", replacements.join(" "));
    }


    function markMatch(text, term, markup, escapeMarkup) {
        var match=stripDiacritics(text.toUpperCase()).indexOf(stripDiacritics(term.toUpperCase())),
            tl=term.length;

        if (match<0) {
            markup.push(escapeMarkup(text));
            return;
        }

        markup.push(escapeMarkup(text.substring(0, match)));
        markup.push("<span class='select2-match'>");
        markup.push(escapeMarkup(text.substring(match, match + tl)));
        markup.push("</span>");
        markup.push(escapeMarkup(text.substring(match + tl, text.length)));
    }

    function defaultEscapeMarkup(markup) {
        var replace_map = {
            '\\': '&#92;',
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            "/": '&#47;'
        };

        return String(markup).replace(/[&<>"'\/\\]/g, function (match) {
            return replace_map[match];
        });
    }

    /**
     * Produces an ajax-based query function
     *
     * @param options object containing configuration parameters
     * @param options.params parameter map for the transport ajax call, can contain such options as cache, jsonpCallback, etc. see $.ajax
     * @param options.transport function that will be used to execute the ajax request. must be compatible with parameters supported by $.ajax
     * @param options.url url for the data
     * @param options.data a function(searchTerm, pageNumber, context) that should return an object containing query string parameters for the above url.
     * @param options.dataType request data type: ajax, jsonp, other datatypes supported by jQuery's $.ajax function or the transport function if specified
     * @param options.quietMillis (optional) milliseconds to wait before making the ajaxRequest, helps debounce the ajax function if invoked too often
     * @param options.results a function(remoteData, pageNumber, query) that converts data returned form the remote request to the format expected by Select2.
     *      The expected format is an object containing the following keys:
     *      results array of objects that will be used as choices
     *      more (optional) boolean indicating whether there are more results available
     *      Example: {results:[{id:1, text:'Red'},{id:2, text:'Blue'}], more:true}
     */
    function ajax(options) {
        var timeout, // current scheduled but not yet executed request
            handler = null,
            quietMillis = options.quietMillis || 100,
            ajaxUrl = options.url,
            self = this;

        return function (query) {
            window.clearTimeout(timeout);
            timeout = window.setTimeout(function () {
                var data = options.data, // ajax data function
                    url = ajaxUrl, // ajax url string or function
                    transport = options.transport || $.fn.select2.ajaxDefaults.transport,
                    // deprecated - to be removed in 4.0  - use params instead
                    deprecated = {
                        type: options.type || 'GET', // set type of request (GET or POST)
                        cache: options.cache || false,
                        jsonpCallback: options.jsonpCallback||undefined,
                        dataType: options.dataType||"json"
                    },
                    params = $.extend({}, $.fn.select2.ajaxDefaults.params, deprecated);

                data = data ? data.call(self, query.term, query.page, query.context) : null;
                url = (typeof url === 'function') ? url.call(self, query.term, query.page, query.context) : url;

                if (handler && typeof handler.abort === "function") { handler.abort(); }

                if (options.params) {
                    if ($.isFunction(options.params)) {
                        $.extend(params, options.params.call(self));
                    } else {
                        $.extend(params, options.params);
                    }
                }

                $.extend(params, {
                    url: url,
                    dataType: options.dataType,
                    data: data,
                    success: function (data) {
                        // TODO - replace query.page with query so users have access to term, page, etc.
                        // added query as third paramter to keep backwards compatibility
                        var results = options.results(data, query.page, query);
                        query.callback(results);
                    },
                    error: function(jqXHR, textStatus, errorThrown){
                        var results = {
                            hasError: true,
                            jqXHR: jqXHR,
                            textStatus: textStatus,
                            errorThrown: errorThrown
                        };

                        query.callback(results);
                    }
                });
                handler = transport.call(self, params);
            }, quietMillis);
        };
    }

    /**
     * Produces a query function that works with a local array
     *
     * @param options object containing configuration parameters. The options parameter can either be an array or an
     * object.
     *
     * If the array form is used it is assumed that it contains objects with 'id' and 'text' keys.
     *
     * If the object form is used it is assumed that it contains 'data' and 'text' keys. The 'data' key should contain
     * an array of objects that will be used as choices. These objects must contain at least an 'id' key. The 'text'
     * key can either be a String in which case it is expected that each element in the 'data' array has a key with the
     * value of 'text' which will be used to match choices. Alternatively, text can be a function(item) that can extract
     * the text.
     */
    function local(options) {
        var data = options, // data elements
            dataText,
            tmp,
            text = function (item) { return ""+item.text; }; // function used to retrieve the text portion of a data item that is matched against the search

         if ($.isArray(data)) {
            tmp = data;
            data = { results: tmp };
        }

         if ($.isFunction(data) === false) {
            tmp = data;
            data = function() { return tmp; };
        }

        var dataItem = data();
        if (dataItem.text) {
            text = dataItem.text;
            // if text is not a function we assume it to be a key name
            if (!$.isFunction(text)) {
                dataText = dataItem.text; // we need to store this in a separate variable because in the next step data gets reset and data.text is no longer available
                text = function (item) { return item[dataText]; };
            }
        }

        return function (query) {
            var t = query.term, filtered = { results: [] }, process;
            if (t === "") {
                query.callback(data());
                return;
            }

            process = function(datum, collection) {
                var group, attr;
                datum = datum[0];
                if (datum.children) {
                    group = {};
                    for (attr in datum) {
                        if (datum.hasOwnProperty(attr)) group[attr]=datum[attr];
                    }
                    group.children=[];
                    $(datum.children).each2(function(i, childDatum) { process(childDatum, group.children); });
                    if (group.children.length || query.matcher(t, text(group), datum)) {
                        collection.push(group);
                    }
                } else {
                    if (query.matcher(t, text(datum), datum)) {
                        collection.push(datum);
                    }
                }
            };

            $(data().results).each2(function(i, datum) { process(datum, filtered.results); });
            query.callback(filtered);
        };
    }

    // TODO javadoc
    function tags(data) {
        var isFunc = $.isFunction(data);
        return function (query) {
            var t = query.term, filtered = {results: []};
            var result = isFunc ? data(query) : data;
            if ($.isArray(result)) {
                $(result).each(function () {
                    var isObject = this.text !== undefined,
                        text = isObject ? this.text : this;
                    if (t === "" || query.matcher(t, text)) {
                        filtered.results.push(isObject ? this : {id: this, text: this});
                    }
                });
                query.callback(filtered);
            }
        };
    }

    /**
     * Checks if the formatter function should be used.
     *
     * Throws an error if it is not a function. Returns true if it should be used,
     * false if no formatting should be performed.
     *
     * @param formatter
     */
    function checkFormatter(formatter, formatterName) {
        if ($.isFunction(formatter)) return true;
        if (!formatter) return false;
        if (typeof(formatter) === 'string') return true;
        throw new Error(formatterName +" must be a string, function, or falsy value");
    }

  /**
   * Returns a given value
   * If given a function, returns its output
   *
   * @param val string|function
   * @param context value of "this" to be passed to function
   * @returns {*}
   */
    function evaluate(val, context) {
        if ($.isFunction(val)) {
            var args = Array.prototype.slice.call(arguments, 2);
            return val.apply(context, args);
        }
        return val;
    }

    function countResults(results) {
        var count = 0;
        $.each(results, function(i, item) {
            if (item.children) {
                count += countResults(item.children);
            } else {
                count++;
            }
        });
        return count;
    }

    /**
     * Default tokenizer. This function uses breaks the input on substring match of any string from the
     * opts.tokenSeparators array and uses opts.createSearchChoice to create the choice object. Both of those
     * two options have to be defined in order for the tokenizer to work.
     *
     * @param input text user has typed so far or pasted into the search field
     * @param selection currently selected choices
     * @param selectCallback function(choice) callback tho add the choice to selection
     * @param opts select2's opts
     * @return undefined/null to leave the current input unchanged, or a string to change the input to the returned value
     */
    function defaultTokenizer(input, selection, selectCallback, opts) {
        var original = input, // store the original so we can compare and know if we need to tell the search to update its text
            dupe = false, // check for whether a token we extracted represents a duplicate selected choice
            token, // token
            index, // position at which the separator was found
            i, l, // looping variables
            separator; // the matched separator

        if (!opts.createSearchChoice || !opts.tokenSeparators || opts.tokenSeparators.length < 1) return undefined;

        while (true) {
            index = -1;

            for (i = 0, l = opts.tokenSeparators.length; i < l; i++) {
                separator = opts.tokenSeparators[i];
                index = input.indexOf(separator);
                if (index >= 0) break;
            }

            if (index < 0) break; // did not find any token separator in the input string, bail

            token = input.substring(0, index);
            input = input.substring(index + separator.length);

            if (token.length > 0) {
                token = opts.createSearchChoice.call(this, token, selection);
                if (token !== undefined && token !== null && opts.id(token) !== undefined && opts.id(token) !== null) {
                    dupe = false;
                    for (i = 0, l = selection.length; i < l; i++) {
                        if (equal(opts.id(token), opts.id(selection[i]))) {
                            dupe = true; break;
                        }
                    }

                    if (!dupe) selectCallback(token);
                }
            }
        }

        if (original!==input) return input;
    }

    function cleanupJQueryElements() {
        var self = this;

        $.each(arguments, function (i, element) {
            self[element].remove();
            self[element] = null;
        });
    }

    /**
     * Creates a new class
     *
     * @param superClass
     * @param methods
     */
    function clazz(SuperClass, methods) {
        var constructor = function () {};
        constructor.prototype = new SuperClass;
        constructor.prototype.constructor = constructor;
        constructor.prototype.parent = SuperClass.prototype;
        constructor.prototype = $.extend(constructor.prototype, methods);
        return constructor;
    }

    AbstractSelect2 = clazz(Object, {

        // abstract
        bind: function (func) {
            var self = this;
            return function () {
                func.apply(self, arguments);
            };
        },

        // abstract
        init: function (opts) {
            var results, search, resultsSelector = ".select2-results";

            // prepare options
            this.opts = opts = this.prepareOpts(opts);

            this.id=opts.id;

            // destroy if called on an existing component
            if (opts.element.data("select2") !== undefined &&
                opts.element.data("select2") !== null) {
                opts.element.data("select2").destroy();
            }

            this.container = this.createContainer();

            this.liveRegion = $('.select2-hidden-accessible');
            if (this.liveRegion.length == 0) {
                this.liveRegion = $("<span>", {
                        role: "status",
                        "aria-live": "polite"
                    })
                    .addClass("select2-hidden-accessible")
                    //.appendTo(document.body);
            }

            this.containerId="s2id_"+(opts.element.attr("id") || "autogen"+nextUid());
            this.containerEventName= this.containerId
                .replace(/([.])/g, '_')
                .replace(/([;&,\-\.\+\*\~':"\!\^#$%@\[\]\(\)=>\|])/g, '\\$1');
            this.container.attr("id", this.containerId);

            this.container.attr("title", opts.element.attr("title"));

            this.body = $(document.body);

            syncCssClasses(this.container, this.opts.element, this.opts.adaptContainerCssClass);

            this.container.attr("style", opts.element.attr("style"));
            this.container.css(evaluate(opts.containerCss, this.opts.element));
            this.container.addClass(evaluate(opts.containerCssClass, this.opts.element));

            this.elementTabIndex = this.opts.element.attr("tabindex");

            // swap container for the element
            this.opts.element
                .data("select2", this)
                .attr("tabindex", "-1")
                .before(this.container)
                .on("click.select2", killEvent); // do not leak click events

            this.container.data("select2", this);

            this.dropdown = this.container.find(".select2-drop");

            syncCssClasses(this.dropdown, this.opts.element, this.opts.adaptDropdownCssClass);

            this.dropdown.addClass(evaluate(opts.dropdownCssClass, this.opts.element));
            this.dropdown.data("select2", this);
            this.dropdown.on("click", killEvent);

            this.results = results = this.container.find(resultsSelector);
            this.search = search = this.container.find("input.select2-input");

            this.queryCount = 0;
            this.resultsPage = 0;
            this.context = null;

            // initialize the container
            this.initContainer();

            this.container.on("click", killEvent);

            installFilteredMouseMove(this.results);

            this.dropdown.on("mousemove-filtered", resultsSelector, this.bind(this.highlightUnderEvent));
            this.dropdown.on("touchstart touchmove touchend", resultsSelector, this.bind(function (event) {
                this._touchEvent = true;
                this.highlightUnderEvent(event);
            }));
            this.dropdown.on("touchmove", resultsSelector, this.bind(this.touchMoved));
            this.dropdown.on("touchstart touchend", resultsSelector, this.bind(this.clearTouchMoved));

            // Waiting for a click event on touch devices to select option and hide dropdown
            // otherwise click will be triggered on an underlying element
            this.dropdown.on('click', this.bind(function (event) {
                if (this._touchEvent) {
                    this._touchEvent = false;
                    this.selectHighlighted();
                }
            }));

            installDebouncedScroll(80, this.results);
            this.dropdown.on("scroll-debounced", resultsSelector, this.bind(this.loadMoreIfNeeded));

            // do not propagate change event from the search field out of the component
            $(this.container).on("change", ".select2-input", function(e) {e.stopPropagation();});
            $(this.dropdown).on("change", ".select2-input", function(e) {e.stopPropagation();});

            // if jquery.mousewheel plugin is installed we can prevent out-of-bounds scrolling of results via mousewheel
            if ($.fn.mousewheel) {
                results.mousewheel(function (e, delta, deltaX, deltaY) {
                    var top = results.scrollTop();
                    if (deltaY > 0 && top - deltaY <= 0) {
                        results.scrollTop(0);
                        killEvent(e);
                    } else if (deltaY < 0 && results.get(0).scrollHeight - results.scrollTop() + deltaY <= results.height()) {
                        results.scrollTop(results.get(0).scrollHeight - results.height());
                        killEvent(e);
                    }
                });
            }

            installKeyUpChangeEvent(search);
            search.on("keyup-change input paste", this.bind(this.updateResults));
            search.on("focus", function () { search.addClass("select2-focused"); });
            search.on("blur", function () { search.removeClass("select2-focused");});

            this.dropdown.on("mouseup", resultsSelector, this.bind(function (e) {
                if ($(e.target).closest(".select2-result-selectable").length > 0) {
                    this.highlightUnderEvent(e);
                    this.selectHighlighted(e);
                }
            }));

            // trap all mouse events from leaving the dropdown. sometimes there may be a modal that is listening
            // for mouse events outside of itself so it can close itself. since the dropdown is now outside the select2's
            // dom it will trigger the popup close, which is not what we want
            // focusin can cause focus wars between modals and select2 since the dropdown is outside the modal.
            this.dropdown.on("click mouseup mousedown touchstart touchend focusin", function (e) { e.stopPropagation(); });

            this.nextSearchTerm = undefined;

            if ($.isFunction(this.opts.initSelection)) {
                // initialize selection based on the current value of the source element
                this.initSelection();

                // if the user has provided a function that can set selection based on the value of the source element
                // we monitor the change event on the element and trigger it, allowing for two way synchronization
                this.monitorSource();
            }

            if (opts.maximumInputLength !== null) {
                this.search.attr("maxlength", opts.maximumInputLength);
            }

            var disabled = opts.element.prop("disabled");
            if (disabled === undefined) disabled = false;
            this.enable(!disabled);

            var readonly = opts.element.prop("readonly");
            if (readonly === undefined) readonly = false;
            this.readonly(readonly);

            // Calculate size of scrollbar
            scrollBarDimensions = scrollBarDimensions || measureScrollbar();

            this.autofocus = opts.element.prop("autofocus");
            opts.element.prop("autofocus", false);
            if (this.autofocus) this.focus();

            this.search.attr("placeholder", opts.searchInputPlaceholder);
        },

        // abstract
        destroy: function () {
            var element=this.opts.element, select2 = element.data("select2"), self = this;

            this.close();

            if (element.length && element[0].detachEvent && self._sync) {
                element.each(function () {
                    if (self._sync) {
                        this.detachEvent("onpropertychange", self._sync);
                    }
                });
            }
            if (this.propertyObserver) {
                this.propertyObserver.disconnect();
                this.propertyObserver = null;
            }
            this._sync = null;

            if (select2 !== undefined) {
                select2.container.remove();
                select2.liveRegion.remove();
                select2.dropdown.remove();
                element
                    .show()
                    .removeData("select2")
                    .off(".select2")
                    .prop("autofocus", this.autofocus || false);
                if (this.elementTabIndex) {
                    element.attr({tabindex: this.elementTabIndex});
                } else {
                    element.removeAttr("tabindex");
                }
                element.show();
            }

            cleanupJQueryElements.call(this,
                "container",
                "liveRegion",
                "dropdown",
                "results",
                "search"
            );
        },

        // abstract
        optionToData: function(element) {
            if (element.is("option")) {
                return {
                    id:element.prop("value"),
                    text:element.text(),
                    element: element.get(),
                    css: element.attr("class"),
                    disabled: element.prop("disabled"),
                    locked: equal(element.attr("locked"), "locked") || equal(element.data("locked"), true)
                };
            } else if (element.is("optgroup")) {
                return {
                    text:element.attr("label"),
                    children:[],
                    element: element.get(),
                    css: element.attr("class")
                };
            }
        },

        // abstract
        prepareOpts: function (opts) {
            var element, select, idKey, ajaxUrl, self = this;

            element = opts.element;

            if (element.get(0).tagName.toLowerCase() === "select") {
                this.select = select = opts.element;
            }

            if (select) {
                // these options are not allowed when attached to a select because they are picked up off the element itself
                $.each(["id", "multiple", "ajax", "query", "createSearchChoice", "initSelection", "data", "tags"], function () {
                    if (this in opts) {
                        throw new Error("Option '" + this + "' is not allowed for Select2 when attached to a <select> element.");
                    }
                });
            }

            opts = $.extend({}, {
                populateResults: function(container, results, query) {
                    var populate, id=this.opts.id, liveRegion=this.liveRegion;

                    populate=function(results, container, depth) {

                        var i, l, result, selectable, disabled, compound, node, label, innerContainer, formatted;

                        results = opts.sortResults(results, container, query);

                        // collect the created nodes for bulk append
                        var nodes = [];
                        for (i = 0, l = results.length; i < l; i = i + 1) {

                            result=results[i];

                            disabled = (result.disabled === true);
                            selectable = (!disabled) && (id(result) !== undefined);

                            compound=result.children && result.children.length > 0;

                            node=$("<li></li>");
                            node.addClass("select2-results-dept-"+depth);
                            node.addClass("select2-result");
                            node.addClass(selectable ? "select2-result-selectable" : "select2-result-unselectable");
                            if (disabled) { node.addClass("select2-disabled"); }
                            if (compound) { node.addClass("select2-result-with-children"); }
                            node.addClass(self.opts.formatResultCssClass(result));
                            node.attr("role", "presentation");

                            label=$(document.createElement("div"));
                            label.addClass("select2-result-label");
                            label.attr("id", "select2-result-label-" + nextUid());
                            label.attr("role", "option");

                            formatted=opts.formatResult(result, label, query, self.opts.escapeMarkup);
                            if (formatted!==undefined) {
                                label.html(formatted);
                                node.append(label);
                            }


                            if (compound) {

                                innerContainer=$("<ul></ul>");
                                innerContainer.addClass("select2-result-sub");
                                populate(result.children, innerContainer, depth+1);
                                node.append(innerContainer);
                            }

                            node.data("select2-data", result);
                            nodes.push(node[0]);
                        }

                        // bulk append the created nodes
                        container.append(nodes);
                        liveRegion.text(opts.formatMatches(results.length));
                    };

                    populate(results, container, 0);
                }
            }, $.fn.select2.defaults, opts);

            if (typeof(opts.id) !== "function") {
                idKey = opts.id;
                opts.id = function (e) { return e[idKey]; };
            }

            if ($.isArray(opts.element.data("select2Tags"))) {
                if ("tags" in opts) {
                    throw "tags specified as both an attribute 'data-select2-tags' and in options of Select2 " + opts.element.attr("id");
                }
                opts.tags=opts.element.data("select2Tags");
            }

            if (select) {
                opts.query = this.bind(function (query) {
                    var data = { results: [], more: false },
                        term = query.term,
                        children, placeholderOption, process;

                    process=function(element, collection) {
                        var group;
                        if (element.is("option")) {
                            if (query.matcher(term, element.text(), element)) {
                                collection.push(self.optionToData(element));
                            }
                        } else if (element.is("optgroup")) {
                            group=self.optionToData(element);
                            element.children().each2(function(i, elm) { process(elm, group.children); });
                            if (group.children.length>0) {
                                collection.push(group);
                            }
                        }
                    };

                    children=element.children();

                    // ignore the placeholder option if there is one
                    if (this.getPlaceholder() !== undefined && children.length > 0) {
                        placeholderOption = this.getPlaceholderOption();
                        if (placeholderOption) {
                            children=children.not(placeholderOption);
                        }
                    }

                    children.each2(function(i, elm) { process(elm, data.results); });

                    query.callback(data);
                });
                // this is needed because inside val() we construct choices from options and their id is hardcoded
                opts.id=function(e) { return e.id; };
            } else {
                if (!("query" in opts)) {

                    if ("ajax" in opts) {
                        ajaxUrl = opts.element.data("ajax-url");
                        if (ajaxUrl && ajaxUrl.length > 0) {
                            opts.ajax.url = ajaxUrl;
                        }
                        opts.query = ajax.call(opts.element, opts.ajax);
                    } else if ("data" in opts) {
                        opts.query = local(opts.data);
                    } else if ("tags" in opts) {
                        opts.query = tags(opts.tags);
                        if (opts.createSearchChoice === undefined) {
                            opts.createSearchChoice = function (term) { return {id: $.trim(term), text: $.trim(term)}; };
                        }
                        if (opts.initSelection === undefined) {
                            opts.initSelection = function (element, callback) {
                                var data = [];
                                $(splitVal(element.val(), opts.separator, opts.transformVal)).each(function () {
                                    var obj = { id: this, text: this },
                                        tags = opts.tags;
                                    if ($.isFunction(tags)) tags=tags();
                                    $(tags).each(function() { if (equal(this.id, obj.id)) { obj = this; return false; } });
                                    data.push(obj);
                                });

                                callback(data);
                            };
                        }
                    }
                }
            }
            if (typeof(opts.query) !== "function") {
                throw "query function not defined for Select2 " + opts.element.attr("id");
            }

            if (opts.createSearchChoicePosition === 'top') {
                opts.createSearchChoicePosition = function(list, item) { list.unshift(item); };
            }
            else if (opts.createSearchChoicePosition === 'bottom') {
                opts.createSearchChoicePosition = function(list, item) { list.push(item); };
            }
            else if (typeof(opts.createSearchChoicePosition) !== "function")  {
                throw "invalid createSearchChoicePosition option must be 'top', 'bottom' or a custom function";
            }

            return opts;
        },

        /**
         * Monitor the original element for changes and update select2 accordingly
         */
        // abstract
        monitorSource: function () {
            var el = this.opts.element, observer, self = this;

            el.on("change.select2", this.bind(function (e) {
                if (this.opts.element.data("select2-change-triggered") !== true) {
                    this.initSelection();
                }
            }));

            this._sync = this.bind(function () {

                // sync enabled state
                var disabled = el.prop("disabled");
                if (disabled === undefined) disabled = false;
                this.enable(!disabled);

                var readonly = el.prop("readonly");
                if (readonly === undefined) readonly = false;
                this.readonly(readonly);

                if (this.container) {
                    syncCssClasses(this.container, this.opts.element, this.opts.adaptContainerCssClass);
                    this.container.addClass(evaluate(this.opts.containerCssClass, this.opts.element));
                }

                if (this.dropdown) {
                    syncCssClasses(this.dropdown, this.opts.element, this.opts.adaptDropdownCssClass);
                    this.dropdown.addClass(evaluate(this.opts.dropdownCssClass, this.opts.element));
                }

            });

            // IE8-10 (IE9/10 won't fire propertyChange via attachEventListener)
            if (el.length && el[0].attachEvent) {
                el.each(function() {
                    this.attachEvent("onpropertychange", self._sync);
                });
            }

            // safari, chrome, firefox, IE11
            observer = window.MutationObserver || window.WebKitMutationObserver|| window.MozMutationObserver;
            if (observer !== undefined) {
                if (this.propertyObserver) { delete this.propertyObserver; this.propertyObserver = null; }
                this.propertyObserver = new observer(function (mutations) {
                    $.each(mutations, self._sync);
                });
                this.propertyObserver.observe(el.get(0), { attributes:true, subtree:false });
            }
        },

        // abstract
        triggerSelect: function(data) {
            var evt = $.Event("select2-selecting", { val: this.id(data), object: data, choice: data });
            this.opts.element.trigger(evt);
            return !evt.isDefaultPrevented();
        },

        /**
         * Triggers the change event on the source element
         */
        // abstract
        triggerChange: function (details) {

            details = details || {};
            details= $.extend({}, details, { type: "change", val: this.val() });
            // prevents recursive triggering
            this.opts.element.data("select2-change-triggered", true);
            this.opts.element.trigger(details);
            this.opts.element.data("select2-change-triggered", false);

            // some validation frameworks ignore the change event and listen instead to keyup, click for selects
            // so here we trigger the click event manually
            this.opts.element.click();

            // ValidationEngine ignores the change event and listens instead to blur
            // so here we trigger the blur event manually if so desired
            if (this.opts.blurOnChange)
                this.opts.element.blur();
        },

        //abstract
        isInterfaceEnabled: function()
        {
            return this.enabledInterface === true;
        },

        // abstract
        enableInterface: function() {
            var enabled = this._enabled && !this._readonly,
                disabled = !enabled;

            if (enabled === this.enabledInterface) return false;

            this.container.toggleClass("select2-container-disabled", disabled);
            this.close();
            this.enabledInterface = enabled;

            return true;
        },

        // abstract
        enable: function(enabled) {
            if (enabled === undefined) enabled = true;
            if (this._enabled === enabled) return;
            this._enabled = enabled;

            this.opts.element.prop("disabled", !enabled);
            this.enableInterface();
        },

        // abstract
        disable: function() {
            this.enable(false);
        },

        // abstract
        readonly: function(enabled) {
            if (enabled === undefined) enabled = false;
            if (this._readonly === enabled) return;
            this._readonly = enabled;

            this.opts.element.prop("readonly", enabled);
            this.enableInterface();
        },

        // abstract
        opened: function () {
            return (this.container) ? this.container.hasClass("select2-dropdown-open") : false;
        },

        // abstract
        positionDropdown: function() {
            var $dropdown = this.dropdown,
                container = this.container,
                offset = container.offset(),
                height = container.outerHeight(false),
                width = container.outerWidth(false),
                dropHeight = $dropdown.outerHeight(false),
                $window = $(window),
                windowWidth = $window.width(),
                windowHeight = $window.height(),
                viewPortRight = $window.scrollLeft() + windowWidth,
                viewportBottom = $window.scrollTop() + windowHeight,
                dropTop = offset.top + height,
                dropLeft = offset.left,
                enoughRoomBelow = dropTop + dropHeight <= viewportBottom,
                enoughRoomAbove = (offset.top - dropHeight) >= $window.scrollTop(),
                dropWidth = $dropdown.outerWidth(false),
                enoughRoomOnRight = function() {
                    return dropLeft + dropWidth <= viewPortRight;
                },
                enoughRoomOnLeft = function() {
                    return offset.left + viewPortRight + container.outerWidth(false)  > dropWidth;
                },
                aboveNow = $dropdown.hasClass("select2-drop-above"),
                bodyOffset,
                above,
                changeDirection,
                css,
                resultsListNode;

            // always prefer the current above/below alignment, unless there is not enough room
            if (aboveNow) {
                above = true;
                if (!enoughRoomAbove && enoughRoomBelow) {
                    changeDirection = true;
                    above = false;
                }
            } else {
                above = false;
                if (!enoughRoomBelow && enoughRoomAbove) {
                    changeDirection = true;
                    above = true;
                }
            }

            //if we are changing direction we need to get positions when dropdown is hidden;
            if (changeDirection) {
                $dropdown.hide();
                offset = this.container.offset();
                height = this.container.outerHeight(false);
                width = this.container.outerWidth(false);
                dropHeight = $dropdown.outerHeight(false);
                viewPortRight = $window.scrollLeft() + windowWidth;
                viewportBottom = $window.scrollTop() + windowHeight;
                dropTop = offset.top + height;
                dropLeft = offset.left;
                dropWidth = $dropdown.outerWidth(false);
                $dropdown.show();

                // fix so the cursor does not move to the left within the search-textbox in IE
                this.focusSearch();
            }

            if (this.opts.dropdownAutoWidth) {
                resultsListNode = $('.select2-results', $dropdown)[0];
                $dropdown.addClass('select2-drop-auto-width');
                $dropdown.css('width', '');
                // Add scrollbar width to dropdown if vertical scrollbar is present
                dropWidth = $dropdown.outerWidth(false) + (resultsListNode.scrollHeight === resultsListNode.clientHeight ? 0 : scrollBarDimensions.width);
                dropWidth > width ? width = dropWidth : dropWidth = width;
                dropHeight = $dropdown.outerHeight(false);
            }
            else {
                this.container.removeClass('select2-drop-auto-width');
            }

            //console.log("below/ droptop:", dropTop, "dropHeight", dropHeight, "sum", (dropTop+dropHeight)+" viewport bottom", viewportBottom, "enough?", enoughRoomBelow);
            //console.log("above/ offset.top", offset.top, "dropHeight", dropHeight, "top", (offset.top-dropHeight), "scrollTop", this.body.scrollTop(), "enough?", enoughRoomAbove);

            // fix positioning when body has an offset and is not position: static
            if (this.body.css('position') !== 'static') {
                bodyOffset = this.body.offset();
                dropTop -= bodyOffset.top;
                dropLeft -= bodyOffset.left;
            }

            if (!enoughRoomOnRight() && enoughRoomOnLeft()) {
                dropLeft = offset.left + this.container.outerWidth(false) - dropWidth;
            }

            css =  {
                left: dropLeft,
                width: width
            };

            if (above) {
                css.top = offset.top - dropHeight;
                css.bottom = 'auto';
                this.container.addClass("select2-drop-above");
                $dropdown.addClass("select2-drop-above");
            }
            else {
                css.top = dropTop;
                css.bottom = 'auto';
                this.container.removeClass("select2-drop-above");
                $dropdown.removeClass("select2-drop-above");
            }
            css = $.extend(css, evaluate(this.opts.dropdownCss, this.opts.element));

            $dropdown.css(css);
        },

        // abstract
        shouldOpen: function() {
            var event;

            if (this.opened()) return false;

            if (this._enabled === false || this._readonly === true) return false;

            event = $.Event("select2-opening");
            this.opts.element.trigger(event);
            return !event.isDefaultPrevented();
        },

        // abstract
        clearDropdownAlignmentPreference: function() {
            // clear the classes used to figure out the preference of where the dropdown should be opened
            this.container.removeClass("select2-drop-above");
            this.dropdown.removeClass("select2-drop-above");
        },

        /**
         * Opens the dropdown
         *
         * @return {Boolean} whether or not dropdown was opened. This method will return false if, for example,
         * the dropdown is already open, or if the 'open' event listener on the element called preventDefault().
         */
        // abstract
        open: function () {

            if (!this.shouldOpen()) return false;

            this.opening();

            // Only bind the document mousemove when the dropdown is visible
            $document.on("mousemove.select2Event", function (e) {
                lastMousePosition.x = e.pageX;
                lastMousePosition.y = e.pageY;
            });

            return true;
        },

        /**
         * Performs the opening of the dropdown
         */
        // abstract
        opening: function() {
            var cid = this.containerEventName,
                scroll = "scroll." + cid,
                resize = "resize."+cid,
                orient = "orientationchange."+cid,
                mask;

            this.container.addClass("select2-dropdown-open").addClass("select2-container-active");

            this.clearDropdownAlignmentPreference();

            if(this.dropdown[0] !== this.body.children().last()[0]) {
                this.dropdown.detach().appendTo(this.body);
            }

            // create the dropdown mask if doesn't already exist
            mask = $("#select2-drop-mask");
            if (mask.length === 0) {
                mask = $(document.createElement("div"));
                mask.attr("id","select2-drop-mask").attr("class","select2-drop-mask");
                mask.hide();
                mask.appendTo(this.body);
                mask.on("mousedown touchstart click", function (e) {
                    // Prevent IE from generating a click event on the body
                    reinsertElement(mask);

                    var dropdown = $("#select2-drop"), self;
                    if (dropdown.length > 0) {
                        self=dropdown.data("select2");
                        if (self.opts.selectOnBlur) {
                            self.selectHighlighted({noFocus: true});
                        }
                        self.close();
                        e.preventDefault();
                        e.stopPropagation();
                    }
                });
            }

            // ensure the mask is always right before the dropdown
            if (this.dropdown.prev()[0] !== mask[0]) {
                this.dropdown.before(mask);
            }

            // move the global id to the correct dropdown
            $("#select2-drop").removeAttr("id");
            this.dropdown.attr("id", "select2-drop");

            // show the elements
            mask.show();

            this.positionDropdown();
            this.dropdown.show();
            this.positionDropdown();

            this.dropdown.addClass("select2-drop-active");

            // attach listeners to events that can change the position of the container and thus require
            // the position of the dropdown to be updated as well so it does not come unglued from the container
            var that = this;
            this.container.parents().add(window).each(function () {
                $(this).on(resize+" "+scroll+" "+orient, function (e) {
                    if (that.opened()) that.positionDropdown();
                });
            });


        },

        // abstract
        close: function () {
            if (!this.opened()) return;

            var cid = this.containerEventName,
                scroll = "scroll." + cid,
                resize = "resize."+cid,
                orient = "orientationchange."+cid;

            // unbind event listeners
            this.container.parents().add(window).each(function () { $(this).off(scroll).off(resize).off(orient); });

            this.clearDropdownAlignmentPreference();

            $("#select2-drop-mask").hide();
            this.dropdown.removeAttr("id"); // only the active dropdown has the select2-drop id
            this.dropdown.hide();
            this.container.removeClass("select2-dropdown-open").removeClass("select2-container-active");
            this.results.empty();

            // Now that the dropdown is closed, unbind the global document mousemove event
            $document.off("mousemove.select2Event");

            this.clearSearch();
            this.search.removeClass("select2-active");
            this.opts.element.trigger($.Event("select2-close"));
        },

        /**
         * Opens control, sets input value, and updates results.
         */
        // abstract
        externalSearch: function (term) {
            this.open();
            this.search.val(term);
            this.updateResults(false);
        },

        // abstract
        clearSearch: function () {

        },

        //abstract
        getMaximumSelectionSize: function() {
            return evaluate(this.opts.maximumSelectionSize, this.opts.element);
        },

        // abstract
        ensureHighlightVisible: function () {
            var results = this.results, children, index, child, hb, rb, y, more, topOffset;

            index = this.highlight();

            if (index < 0) return;

            if (index == 0) {

                // if the first element is highlighted scroll all the way to the top,
                // that way any unselectable headers above it will also be scrolled
                // into view

                results.scrollTop(0);
                return;
            }

            children = this.findHighlightableChoices().find('.select2-result-label');

            child = $(children[index]);

            topOffset = (child.offset() || {}).top || 0;

            hb = topOffset + child.outerHeight(true);

            // if this is the last child lets also make sure select2-more-results is visible
            if (index === children.length - 1) {
                more = results.find("li.select2-more-results");
                if (more.length > 0) {
                    hb = more.offset().top + more.outerHeight(true);
                }
            }

            rb = results.offset().top + results.outerHeight(false);
            if (hb > rb) {
                results.scrollTop(results.scrollTop() + (hb - rb));
            }
            y = topOffset - results.offset().top;

            // make sure the top of the element is visible
            if (y < 0 && child.css('display') != 'none' ) {
                results.scrollTop(results.scrollTop() + y); // y is negative
            }
        },

        // abstract
        findHighlightableChoices: function() {
            return this.results.find(".select2-result-selectable:not(.select2-disabled):not(.select2-selected)");
        },

        // abstract
        moveHighlight: function (delta) {
            var choices = this.findHighlightableChoices(),
                index = this.highlight();

            while (index > -1 && index < choices.length) {
                index += delta;
                var choice = $(choices[index]);
                if (choice.hasClass("select2-result-selectable") && !choice.hasClass("select2-disabled") && !choice.hasClass("select2-selected")) {
                    this.highlight(index);
                    break;
                }
            }
        },

        // abstract
        highlight: function (index) {
            var choices = this.findHighlightableChoices(),
                choice,
                data;

            if (arguments.length === 0) {
                return indexOf(choices.filter(".select2-highlighted")[0], choices.get());
            }

            if (index >= choices.length) index = choices.length - 1;
            if (index < 0) index = 0;

            this.removeHighlight();

            choice = $(choices[index]);
            choice.addClass("select2-highlighted");

            // ensure assistive technology can determine the active choice
            this.search.attr("aria-activedescendant", choice.find(".select2-result-label").attr("id"));

            this.ensureHighlightVisible();

            this.liveRegion.text(choice.text());

            data = choice.data("select2-data");
            if (data) {
                this.opts.element.trigger({ type: "select2-highlight", val: this.id(data), choice: data });
            }
        },

        removeHighlight: function() {
            this.results.find(".select2-highlighted").removeClass("select2-highlighted");
        },

        touchMoved: function() {
            this._touchMoved = true;
        },

        clearTouchMoved: function() {
          this._touchMoved = false;
        },

        // abstract
        countSelectableResults: function() {
            return this.findHighlightableChoices().length;
        },

        // abstract
        highlightUnderEvent: function (event) {
            var el = $(event.target).closest(".select2-result-selectable");
            if (el.length > 0 && !el.is(".select2-highlighted")) {
                var choices = this.findHighlightableChoices();
                this.highlight(choices.index(el));
            } else if (el.length == 0) {
                // if we are over an unselectable item remove all highlights
                this.removeHighlight();
            }
        },

        // abstract
        loadMoreIfNeeded: function () {
            var results = this.results,
                more = results.find("li.select2-more-results"),
                below, // pixels the element is below the scroll fold, below==0 is when the element is starting to be visible
                page = this.resultsPage + 1,
                self=this,
                term=this.search.val(),
                context=this.context;

            if (more.length === 0) return;
            below = more.offset().top - results.offset().top - results.height();

            if (below <= this.opts.loadMorePadding) {
                more.addClass("select2-active");
                this.opts.query({
                        element: this.opts.element,
                        term: term,
                        page: page,
                        context: context,
                        matcher: this.opts.matcher,
                        callback: this.bind(function (data) {

                    // ignore a response if the select2 has been closed before it was received
                    if (!self.opened()) return;


                    self.opts.populateResults.call(this, results, data.results, {term: term, page: page, context:context});
                    self.postprocessResults(data, false, false);

                    if (data.more===true) {
                        more.detach().appendTo(results).html(self.opts.escapeMarkup(evaluate(self.opts.formatLoadMore, self.opts.element, page+1)));
                        window.setTimeout(function() { self.loadMoreIfNeeded(); }, 10);
                    } else {
                        more.remove();
                    }
                    self.positionDropdown();
                    self.resultsPage = page;
                    self.context = data.context;
                    this.opts.element.trigger({ type: "select2-loaded", items: data });
                })});
            }
        },

        /**
         * Default tokenizer function which does nothing
         */
        tokenize: function() {

        },

        /**
         * @param initial whether or not this is the call to this method right after the dropdown has been opened
         */
        // abstract
        updateResults: function (initial) {
            var search = this.search,
                results = this.results,
                opts = this.opts,
                data,
                self = this,
                input,
                term = search.val(),
                lastTerm = $.data(this.container, "select2-last-term"),
                // sequence number used to drop out-of-order responses
                queryNumber;

            // prevent duplicate queries against the same term
            if (initial !== true && lastTerm && equal(term, lastTerm)) return;

            $.data(this.container, "select2-last-term", term);

            // if the search is currently hidden we do not alter the results
            if (initial !== true && (this.showSearchInput === false || !this.opened())) {
                return;
            }

            function postRender() {
                search.removeClass("select2-active");
                self.positionDropdown();
                if (results.find('.select2-no-results,.select2-selection-limit,.select2-searching').length) {
                    self.liveRegion.text(results.text());
                }
                else {
                    self.liveRegion.text(self.opts.formatMatches(results.find('.select2-result-selectable:not(".select2-selected")').length));
                }
            }

            function render(html) {
                results.html(html);
                postRender();
            }

            queryNumber = ++this.queryCount;

            var maxSelSize = this.getMaximumSelectionSize();
            if (maxSelSize >=1) {
                data = this.data();
                if ($.isArray(data) && data.length >= maxSelSize && checkFormatter(opts.formatSelectionTooBig, "formatSelectionTooBig")) {
                    render("<li class='select2-selection-limit'>" + evaluate(opts.formatSelectionTooBig, opts.element, maxSelSize) + "</li>");
                    return;
                }
            }

            if (search.val().length < opts.minimumInputLength) {
                if (checkFormatter(opts.formatInputTooShort, "formatInputTooShort")) {
                    render("<li class='select2-no-results'>" + evaluate(opts.formatInputTooShort, opts.element, search.val(), opts.minimumInputLength) + "</li>");
                } else {
                    render("");
                }
                if (initial && this.showSearch) this.showSearch(true);
                return;
            }

            if (opts.maximumInputLength && search.val().length > opts.maximumInputLength) {
                if (checkFormatter(opts.formatInputTooLong, "formatInputTooLong")) {
                    render("<li class='select2-no-results'>" + evaluate(opts.formatInputTooLong, opts.element, search.val(), opts.maximumInputLength) + "</li>");
                } else {
                    render("");
                }
                return;
            }

            if (opts.formatSearching && this.findHighlightableChoices().length === 0) {
                render("<li class='select2-searching'>" + evaluate(opts.formatSearching, opts.element) + "</li>");
            }

            search.addClass("select2-active");

            this.removeHighlight();

            // give the tokenizer a chance to pre-process the input
            input = this.tokenize();
            if (input != undefined && input != null) {
                search.val(input);
            }

            this.resultsPage = 1;

            opts.query({
                element: opts.element,
                    term: search.val(),
                    page: this.resultsPage,
                    context: null,
                    matcher: opts.matcher,
                    callback: this.bind(function (data) {
                var def; // default choice

                // ignore old responses
                if (queryNumber != this.queryCount) {
                  return;
                }

                // ignore a response if the select2 has been closed before it was received
                if (!this.opened()) {
                    this.search.removeClass("select2-active");
                    return;
                }

                // handle ajax error
                if(data.hasError !== undefined && checkFormatter(opts.formatAjaxError, "formatAjaxError")) {
                    render("<li class='select2-ajax-error'>" + evaluate(opts.formatAjaxError, opts.element, data.jqXHR, data.textStatus, data.errorThrown) + "</li>");
                    return;
                }

                // save context, if any
                this.context = (data.context===undefined) ? null : data.context;
                // create a default choice and prepend it to the list
                if (this.opts.createSearchChoice && search.val() !== "") {
                    def = this.opts.createSearchChoice.call(self, search.val(), data.results);
                    if (def !== undefined && def !== null && self.id(def) !== undefined && self.id(def) !== null) {
                        if ($(data.results).filter(
                            function () {
                                return equal(self.id(this), self.id(def));
                            }).length === 0) {
                            this.opts.createSearchChoicePosition(data.results, def);
                        }
                    }
                }

                if (data.results.length === 0 && checkFormatter(opts.formatNoMatches, "formatNoMatches")) {
                    render("<li class='select2-no-results'>" + evaluate(opts.formatNoMatches, opts.element, search.val()) + "</li>");
                    return;
                }

                results.empty();
                self.opts.populateResults.call(this, results, data.results, {term: search.val(), page: this.resultsPage, context:null});

                if (data.more === true && checkFormatter(opts.formatLoadMore, "formatLoadMore")) {
                    results.append("<li class='select2-more-results'>" + opts.escapeMarkup(evaluate(opts.formatLoadMore, opts.element, this.resultsPage)) + "</li>");
                    window.setTimeout(function() { self.loadMoreIfNeeded(); }, 10);
                }

                this.postprocessResults(data, initial);

                postRender();

                this.opts.element.trigger({ type: "select2-loaded", items: data });
            })});
        },

        // abstract
        cancel: function () {
            this.close();
        },

        // abstract
        blur: function () {
            // if selectOnBlur == true, select the currently highlighted option
            if (this.opts.selectOnBlur)
                this.selectHighlighted({noFocus: true});

            this.close();
            this.container.removeClass("select2-container-active");
            // synonymous to .is(':focus'), which is available in jquery >= 1.6
            if (this.search[0] === document.activeElement) { this.search.blur(); }
            this.clearSearch();
            this.selection.find(".select2-search-choice-focus").removeClass("select2-search-choice-focus");
        },

        // abstract
        focusSearch: function () {
            focus(this.search);
        },

        // abstract
        selectHighlighted: function (options) {
            if (this._touchMoved) {
              this.clearTouchMoved();
              return;
            }
            var index=this.highlight(),
                highlighted=this.results.find(".select2-highlighted"),
                data = highlighted.closest('.select2-result').data("select2-data");

            if (data) {
                this.highlight(index);
                this.onSelect(data, options);
            } else if (options && options.noFocus) {
                this.close();
            }
        },

        // abstract
        getPlaceholder: function () {
            var placeholderOption;
            return this.opts.element.attr("placeholder") ||
                this.opts.element.attr("data-placeholder") || // jquery 1.4 compat
                this.opts.element.data("placeholder") ||
                this.opts.placeholder ||
                ((placeholderOption = this.getPlaceholderOption()) !== undefined ? placeholderOption.text() : undefined);
        },

        // abstract
        getPlaceholderOption: function() {
            if (this.select) {
                var firstOption = this.select.children('option').first();
                if (this.opts.placeholderOption !== undefined ) {
                    //Determine the placeholder option based on the specified placeholderOption setting
                    return (this.opts.placeholderOption === "first" && firstOption) ||
                           (typeof this.opts.placeholderOption === "function" && this.opts.placeholderOption(this.select));
                } else if ($.trim(firstOption.text()) === "" && firstOption.val() === "") {
                    //No explicit placeholder option specified, use the first if it's blank
                    return firstOption;
                }
            }
        },

        /**
         * Get the desired width for the container element.  This is
         * derived first from option `width` passed to select2, then
         * the inline 'style' on the original element, and finally
         * falls back to the jQuery calculated element width.
         */
        // abstract
        initContainerWidth: function () {
            function resolveContainerWidth() {
                var style, attrs, matches, i, l, attr;

                if (this.opts.width === "off") {
                    return null;
                } else if (this.opts.width === "element"){
                    return this.opts.element.outerWidth(false) === 0 ? 'auto' : this.opts.element.outerWidth(false) + 'px';
                } else if (this.opts.width === "copy" || this.opts.width === "resolve") {
                    // check if there is inline style on the element that contains width
                    style = this.opts.element.attr('style');
                    if (style !== undefined) {
                        attrs = style.split(';');
                        for (i = 0, l = attrs.length; i < l; i = i + 1) {
                            attr = attrs[i].replace(/\s/g, '');
                            matches = attr.match(/^width:(([-+]?([0-9]*\.)?[0-9]+)(px|em|ex|%|in|cm|mm|pt|pc))/i);
                            if (matches !== null && matches.length >= 1)
                                return matches[1];
                        }
                    }

                    if (this.opts.width === "resolve") {
                        // next check if css('width') can resolve a width that is percent based, this is sometimes possible
                        // when attached to input type=hidden or elements hidden via css
                        style = this.opts.element.css('width');
                        if (style.indexOf("%") > 0) return style;

                        // finally, fallback on the calculated width of the element
                        return (this.opts.element.outerWidth(false) === 0 ? 'auto' : this.opts.element.outerWidth(false) + 'px');
                    }

                    return null;
                } else if ($.isFunction(this.opts.width)) {
                    return this.opts.width();
                } else {
                    return this.opts.width;
               }
            };

            var width = resolveContainerWidth.call(this);
            if (width !== null) {
                this.container.css("width", width);
            }
        }
    });

    SingleSelect2 = clazz(AbstractSelect2, {

        // single

        createContainer: function () {
            var container = $(document.createElement("div")).attr({
                "class": "select2-container"
            }).html([
                "<a href='javascript:void(0)' class='select2-choice' tabindex='-1'>",
                "   <span class='select2-chosen'>&#160;</span><abbr class='select2-search-choice-close'></abbr>",
               // "   <span class='select2-arrow' role='presentation'><b role='presentation'></b></span>",
                "</a>",
                "<label for='' class='select2-offscreen'></label>",
                "<input class='select2-focusser select2-offscreen' type='text' aria-haspopup='true' role='button' />",
                "<div class='select2-drop select2-display-none'>",
                "   <div class='select2-search'>",
                "       <label for='' class='select2-offscreen'></label>",
                "       <input type='text' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' class='select2-input' role='combobox' aria-expanded='true'",
                "       aria-autocomplete='list' />",
                "   </div>",
                "   <ul class='select2-results' role='listbox'>",
                "   </ul>",
                "</div>"].join(""));
            return container;
        },

        // single
        enableInterface: function() {
            if (this.parent.enableInterface.apply(this, arguments)) {
                this.focusser.prop("disabled", !this.isInterfaceEnabled());
            }
        },

        // single
        opening: function () {
            var el, range, len;

            if (this.opts.minimumResultsForSearch >= 0) {
                this.showSearch(true);
            }

            this.parent.opening.apply(this, arguments);

            if (this.showSearchInput !== false) {
                // IE appends focusser.val() at the end of field :/ so we manually insert it at the beginning using a range
                // all other browsers handle this just fine

                this.search.val(this.focusser.val());
            }
            if (this.opts.shouldFocusInput(this)) {
                this.search.focus();
                // move the cursor to the end after focussing, otherwise it will be at the beginning and
                // new text will appear *before* focusser.val()
                el = this.search.get(0);
                if (el.createTextRange) {
                    range = el.createTextRange();
                    range.collapse(false);
                    range.select();
                } else if (el.setSelectionRange) {
                    len = this.search.val().length;
                    el.setSelectionRange(len, len);
                }
            }

            // initializes search's value with nextSearchTerm (if defined by user)
            // ignore nextSearchTerm if the dropdown is opened by the user pressing a letter
            if(this.search.val() === "") {
                if(this.nextSearchTerm != undefined){
                    this.search.val(this.nextSearchTerm);
                    this.search.select();
                }
            }

            this.focusser.prop("disabled", true).val("");
            this.updateResults(true);
            this.opts.element.trigger($.Event("select2-open"));
        },

        // single
        close: function () {
            if (!this.opened()) return;
            this.parent.close.apply(this, arguments);

            this.focusser.prop("disabled", false);

            if (this.opts.shouldFocusInput(this)) {
                this.focusser.focus();
            }
        },

        // single
        focus: function () {
            if (this.opened()) {
                this.close();
            } else {
                this.focusser.prop("disabled", false);
                if (this.opts.shouldFocusInput(this)) {
                    this.focusser.focus();
                }
            }
        },

        // single
        isFocused: function () {
            return this.container.hasClass("select2-container-active");
        },

        // single
        cancel: function () {
            this.parent.cancel.apply(this, arguments);
            this.focusser.prop("disabled", false);

            if (this.opts.shouldFocusInput(this)) {
                this.focusser.focus();
            }
        },

        // single
        destroy: function() {
            $("label[for='" + this.focusser.attr('id') + "']")
                .attr('for', this.opts.element.attr("id"));
            this.parent.destroy.apply(this, arguments);

            cleanupJQueryElements.call(this,
                "selection",
                "focusser"
            );
        },

        // single
        initContainer: function () {

            var selection,
                container = this.container,
                dropdown = this.dropdown,
                idSuffix = nextUid(),
                elementLabel;

            if (this.opts.minimumResultsForSearch < 0) {
                this.showSearch(false);
            } else {
                this.showSearch(true);
            }

            this.selection = selection = container.find(".select2-choice");

            this.focusser = container.find(".select2-focusser");

            // add aria associations
            selection.find(".select2-chosen").attr("id", "select2-chosen-"+idSuffix);
            this.focusser.attr("aria-labelledby", "select2-chosen-"+idSuffix);
            this.results.attr("id", "select2-results-"+idSuffix);
            this.search.attr("aria-owns", "select2-results-"+idSuffix);

            // rewrite labels from original element to focusser
            this.focusser.attr("id", "s2id_autogen"+idSuffix);

            elementLabel = $("label[for='" + this.opts.element.attr("id") + "']");
            this.opts.element.focus(this.bind(function () { this.focus(); }));

            this.focusser.prev()
                .text(elementLabel.text())
                .attr('for', this.focusser.attr('id'));

            // Ensure the original element retains an accessible name
            var originalTitle = this.opts.element.attr("title");
            this.opts.element.attr("title", (originalTitle || elementLabel.text()));

            this.focusser.attr("tabindex", this.elementTabIndex);

            // write label for search field using the label from the focusser element
            this.search.attr("id", this.focusser.attr('id') + '_search');

            this.search.prev()
                .text($("label[for='" + this.focusser.attr('id') + "']").text())
                .attr('for', this.search.attr('id'));

            this.search.on("keydown", this.bind(function (e) {
                if (!this.isInterfaceEnabled()) return;

                // filter 229 keyCodes (input method editor is processing key input)
                if (229 == e.keyCode) return;

                if (e.which === KEY.PAGE_UP || e.which === KEY.PAGE_DOWN) {
                    // prevent the page from scrolling
                    killEvent(e);
                    return;
                }

                switch (e.which) {
                    case KEY.UP:
                    case KEY.DOWN:
                        this.moveHighlight((e.which === KEY.UP) ? -1 : 1);
                        killEvent(e);
                        return;
                    case KEY.ENTER:
                        this.selectHighlighted();
                        killEvent(e);
                        return;
                    case KEY.TAB:
                        this.selectHighlighted({noFocus: true});
                        return;
                    case KEY.ESC:
                        this.cancel(e);
                        killEvent(e);
                        return;
                }
            }));

            this.search.on("blur", this.bind(function(e) {
                // a workaround for chrome to keep the search field focussed when the scroll bar is used to scroll the dropdown.
                // without this the search field loses focus which is annoying
                if (document.activeElement === this.body.get(0)) {
                    window.setTimeout(this.bind(function() {
                        if (this.opened()) {
                            this.search.focus();
                        }
                    }), 0);
                }
            }));

            this.focusser.on("keydown", this.bind(function (e) {
                if (!this.isInterfaceEnabled()) return;

                if (e.which === KEY.TAB || KEY.isControl(e) || KEY.isFunctionKey(e) || e.which === KEY.ESC) {
                    return;
                }

                if (this.opts.openOnEnter === false && e.which === KEY.ENTER) {
                    killEvent(e);
                    return;
                }

                if (e.which == KEY.DOWN || e.which == KEY.UP
                    || (e.which == KEY.ENTER && this.opts.openOnEnter)) {

                    if (e.altKey || e.ctrlKey || e.shiftKey || e.metaKey) return;

                    this.open();
                    killEvent(e);
                    return;
                }

                if (e.which == KEY.DELETE || e.which == KEY.BACKSPACE) {
                    if (this.opts.allowClear) {
                        this.clear();
                    }
                    killEvent(e);
                    return;
                }
            }));


            installKeyUpChangeEvent(this.focusser);
            this.focusser.on("keyup-change input", this.bind(function(e) {
                if (this.opts.minimumResultsForSearch >= 0) {
                    e.stopPropagation();
                    if (this.opened()) return;
                    this.open();
                }
            }));

            selection.on("mousedown touchstart", "abbr", this.bind(function (e) {
                if (!this.isInterfaceEnabled()) {
                    return;
                }

                this.clear();
                killEventImmediately(e);
                this.close();

                if (this.selection) {
                    this.selection.focus();
                }
            }));

            selection.on("mousedown touchstart", this.bind(function (e) {
                // Prevent IE from generating a click event on the body
                reinsertElement(selection);

                if (!this.container.hasClass("select2-container-active")) {
                    this.opts.element.trigger($.Event("select2-focus"));
                }

                if (this.opened()) {
                    this.close();
                } else if (this.isInterfaceEnabled()) {
                    this.open();
                }

                killEvent(e);
            }));

            dropdown.on("mousedown touchstart", this.bind(function() {
                if (this.opts.shouldFocusInput(this)) {
                    this.search.focus();
                }
            }));

            selection.on("focus", this.bind(function(e) {
                killEvent(e);
            }));

            this.focusser.on("focus", this.bind(function(){
                if (!this.container.hasClass("select2-container-active")) {
                    this.opts.element.trigger($.Event("select2-focus"));
                }
                this.container.addClass("select2-container-active");
            })).on("blur", this.bind(function() {
                if (!this.opened()) {
                    this.container.removeClass("select2-container-active");
                    this.opts.element.trigger($.Event("select2-blur"));
                }
            }));
            this.search.on("focus", this.bind(function(){
                if (!this.container.hasClass("select2-container-active")) {
                    this.opts.element.trigger($.Event("select2-focus"));
                }
                this.container.addClass("select2-container-active");
            }));

            this.initContainerWidth();
            this.opts.element.hide();
            this.setPlaceholder();

        },

        // single
        clear: function(triggerChange) {
            var data=this.selection.data("select2-data");
            if (data) { // guard against queued quick consecutive clicks
                var evt = $.Event("select2-clearing");
                this.opts.element.trigger(evt);
                if (evt.isDefaultPrevented()) {
                    return;
                }
                var placeholderOption = this.getPlaceholderOption();
                this.opts.element.val(placeholderOption ? placeholderOption.val() : "");
                this.selection.find(".select2-chosen").empty();
                this.selection.removeData("select2-data");
                this.setPlaceholder();

                if (triggerChange !== false){
                    this.opts.element.trigger({ type: "select2-removed", val: this.id(data), choice: data });
                    this.triggerChange({removed:data});
                }
            }
        },

        /**
         * Sets selection based on source element's value
         */
        // single
        initSelection: function () {
            var selected;
            if (this.isPlaceholderOptionSelected()) {
                this.updateSelection(null);
                this.close();
                this.setPlaceholder();
            } else {
                var self = this;
                this.opts.initSelection.call(null, this.opts.element, function(selected){
                    if (selected !== undefined && selected !== null) {
                        self.updateSelection(selected);
                        self.close();
                        self.setPlaceholder();
                        self.nextSearchTerm = self.opts.nextSearchTerm(selected, self.search.val());
                    }
                });
            }
        },

        isPlaceholderOptionSelected: function() {
            var placeholderOption;
            if (this.getPlaceholder() === undefined) return false; // no placeholder specified so no option should be considered
            return ((placeholderOption = this.getPlaceholderOption()) !== undefined && placeholderOption.prop("selected"))
                || (this.opts.element.val() === "")
                || (this.opts.element.val() === undefined)
                || (this.opts.element.val() === null);
        },

        // single
        prepareOpts: function () {
            var opts = this.parent.prepareOpts.apply(this, arguments),
                self=this;

            if (opts.element.get(0).tagName.toLowerCase() === "select") {
                // install the selection initializer
                opts.initSelection = function (element, callback) {
                    var selected = element.find("option").filter(function() { return this.selected && !this.disabled });
                    // a single select box always has a value, no need to null check 'selected'
                    callback(self.optionToData(selected));
                };
            } else if ("data" in opts) {
                // install default initSelection when applied to hidden input and data is local
                opts.initSelection = opts.initSelection || function (element, callback) {
                    var id = element.val();
                    //search in data by id, storing the actual matching item
                    var match = null;
                    opts.query({
                        matcher: function(term, text, el){
                            var is_match = equal(id, opts.id(el));
                            if (is_match) {
                                match = el;
                            }
                            return is_match;
                        },
                        callback: !$.isFunction(callback) ? $.noop : function() {
                            callback(match);
                        }
                    });
                };
            }

            return opts;
        },

        // single
        getPlaceholder: function() {
            // if a placeholder is specified on a single select without a valid placeholder option ignore it
            if (this.select) {
                if (this.getPlaceholderOption() === undefined) {
                    return undefined;
                }
            }

            return this.parent.getPlaceholder.apply(this, arguments);
        },

        // single
        setPlaceholder: function () {
            var placeholder = this.getPlaceholder();

            if (this.isPlaceholderOptionSelected() && placeholder !== undefined) {

                // check for a placeholder option if attached to a select
                if (this.select && this.getPlaceholderOption() === undefined) return;

                this.selection.find(".select2-chosen").html(this.opts.escapeMarkup(placeholder));

                this.selection.addClass("select2-default");

                this.container.removeClass("select2-allowclear");
            }
        },

        // single
        postprocessResults: function (data, initial, noHighlightUpdate) {
            var selected = 0, self = this, showSearchInput = true;

            // find the selected element in the result list

            this.findHighlightableChoices().each2(function (i, elm) {
                if (equal(self.id(elm.data("select2-data")), self.opts.element.val())) {
                    selected = i;
                    return false;
                }
            });

            // and highlight it
            if (noHighlightUpdate !== false) {
                if (initial === true && selected >= 0) {
                    this.highlight(selected);
                } else {
                    this.highlight(0);
                }
            }

            // hide the search box if this is the first we got the results and there are enough of them for search

            if (initial === true) {
                var min = this.opts.minimumResultsForSearch;
                if (min >= 0) {
                    this.showSearch(countResults(data.results) >= min);
                }
            }
        },

        // single
        showSearch: function(showSearchInput) {
            if (this.showSearchInput === showSearchInput) return;

            this.showSearchInput = showSearchInput;

            this.dropdown.find(".select2-search").toggleClass("select2-search-hidden", !showSearchInput);
            this.dropdown.find(".select2-search").toggleClass("select2-offscreen", !showSearchInput);
            //add "select2-with-searchbox" to the container if search box is shown
            $(this.dropdown, this.container).toggleClass("select2-with-searchbox", showSearchInput);
        },

        // single
        onSelect: function (data, options) {

            if (!this.triggerSelect(data)) { return; }

            var old = this.opts.element.val(),
                oldData = this.data();

            this.opts.element.val(this.id(data));
            this.updateSelection(data);

            this.opts.element.trigger({ type: "select2-selected", val: this.id(data), choice: data });

            this.nextSearchTerm = this.opts.nextSearchTerm(data, this.search.val());
            this.close();

            if ((!options || !options.noFocus) && this.opts.shouldFocusInput(this)) {
                this.focusser.focus();
            }

            if (!equal(old, this.id(data))) {
                this.triggerChange({ added: data, removed: oldData });
            }
        },

        // single
        updateSelection: function (data) {

            var container=this.selection.find(".select2-chosen"), formatted, cssClass;

            this.selection.data("select2-data", data);

            container.empty();
            if (data !== null) {
                formatted=this.opts.formatSelection(data, container, this.opts.escapeMarkup);
            }
            if (formatted !== undefined) {
                container.append(formatted);
            }
            cssClass=this.opts.formatSelectionCssClass(data, container);
            if (cssClass !== undefined) {
                container.addClass(cssClass);
            }

            this.selection.removeClass("select2-default");

            if (this.opts.allowClear && this.getPlaceholder() !== undefined) {
                this.container.addClass("select2-allowclear");
            }
        },

        // single
        val: function () {
            var val,
                triggerChange = false,
                data = null,
                self = this,
                oldData = this.data();

            if (arguments.length === 0) {
                return this.opts.element.val();
            }

            val = arguments[0];

            if (arguments.length > 1) {
                triggerChange = arguments[1];
            }

            if (this.select) {
                this.select
                    .val(val)
                    .find("option").filter(function() { return this.selected }).each2(function (i, elm) {
                        data = self.optionToData(elm);
                        return false;
                    });
                this.updateSelection(data);
                this.setPlaceholder();
                if (triggerChange) {
                    this.triggerChange({added: data, removed:oldData});
                }
            } else {
                // val is an id. !val is true for [undefined,null,'',0] - 0 is legal
                if (!val && val !== 0) {
                    this.clear(triggerChange);
                    return;
                }
                if (this.opts.initSelection === undefined) {
                    throw new Error("cannot call val() if initSelection() is not defined");
                }
                this.opts.element.val(val);
                this.opts.initSelection(this.opts.element, function(data){
                    self.opts.element.val(!data ? "" : self.id(data));
                    self.updateSelection(data);
                    self.setPlaceholder();
                    if (triggerChange) {
                        self.triggerChange({added: data, removed:oldData});
                    }
                });
            }
        },

        // single
        clearSearch: function () {
            this.search.val("");
            this.focusser.val("");
        },

        // single
        data: function(value) {
            var data,
                triggerChange = false;

            if (arguments.length === 0) {
                data = this.selection.data("select2-data");
                if (data == undefined) data = null;
                return data;
            } else {
                if (arguments.length > 1) {
                    triggerChange = arguments[1];
                }
                if (!value) {
                    this.clear(triggerChange);
                } else {
                    data = this.data();
                    this.opts.element.val(!value ? "" : this.id(value));
                    this.updateSelection(value);
                    if (triggerChange) {
                        this.triggerChange({added: value, removed:data});
                    }
                }
            }
        }
    });

    MultiSelect2 = clazz(AbstractSelect2, {

        // multi
        createContainer: function () {
            var container = $(document.createElement("div")).attr({
                "class": "select2-container select2-container-multi"
            }).html([
                "<ul class='select2-choices'>",
                "  <li class='select2-search-field'>",
                "    <label for='' class='select2-offscreen'></label>",
                "    <input type='text' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' class='select2-input'>",
                "  </li>",
                "</ul>",
                "<div class='select2-drop select2-drop-multi select2-display-none'>",
                "   <ul class='select2-results'>",
                "   </ul>",
                "</div>"].join(""));
            return container;
        },

        // multi
        prepareOpts: function () {
            var opts = this.parent.prepareOpts.apply(this, arguments),
                self=this;

            // TODO validate placeholder is a string if specified
            if (opts.element.get(0).tagName.toLowerCase() === "select") {
                // install the selection initializer
                opts.initSelection = function (element, callback) {

                    var data = [];

                    element.find("option").filter(function() { return this.selected && !this.disabled }).each2(function (i, elm) {
                        data.push(self.optionToData(elm));
                    });
                    callback(data);
                };
            } else if ("data" in opts) {
                // install default initSelection when applied to hidden input and data is local
                opts.initSelection = opts.initSelection || function (element, callback) {
                    var ids = splitVal(element.val(), opts.separator, opts.transformVal);
                    //search in data by array of ids, storing matching items in a list
                    var matches = [];
                    opts.query({
                        matcher: function(term, text, el){
                            var is_match = $.grep(ids, function(id) {
                                return equal(id, opts.id(el));
                            }).length;
                            if (is_match) {
                                matches.push(el);
                            }
                            return is_match;
                        },
                        callback: !$.isFunction(callback) ? $.noop : function() {
                            // reorder matches based on the order they appear in the ids array because right now
                            // they are in the order in which they appear in data array
                            var ordered = [];
                            for (var i = 0; i < ids.length; i++) {
                                var id = ids[i];
                                for (var j = 0; j < matches.length; j++) {
                                    var match = matches[j];
                                    if (equal(id, opts.id(match))) {
                                        ordered.push(match);
                                        matches.splice(j, 1);
                                        break;
                                    }
                                }
                            }
                            callback(ordered);
                        }
                    });
                };
            }

            return opts;
        },

        // multi
        selectChoice: function (choice) {

            var selected = this.container.find(".select2-search-choice-focus");
            if (selected.length && choice && choice[0] == selected[0]) {

            } else {
                if (selected.length) {
                    this.opts.element.trigger("choice-deselected", selected);
                }
                selected.removeClass("select2-search-choice-focus");
                if (choice && choice.length) {
                    this.close();
                    choice.addClass("select2-search-choice-focus");
                    this.opts.element.trigger("choice-selected", choice);
                }
            }
        },

        // multi
        destroy: function() {
            $("label[for='" + this.search.attr('id') + "']")
                .attr('for', this.opts.element.attr("id"));
            this.parent.destroy.apply(this, arguments);

            cleanupJQueryElements.call(this,
                "searchContainer",
                "selection"
            );
        },

        // multi
        initContainer: function () {

            var selector = ".select2-choices", selection;

            this.searchContainer = this.container.find(".select2-search-field");
            this.selection = selection = this.container.find(selector);

            var _this = this;
            this.selection.on("click", ".select2-container:not(.select2-container-disabled) .select2-search-choice:not(.select2-locked)", function (e) {
                _this.search[0].focus();
                _this.selectChoice($(this));
            });

            // rewrite labels from original element to focusser
            this.search.attr("id", "s2id_autogen"+nextUid());

            this.search.prev()
                .text($("label[for='" + this.opts.element.attr("id") + "']").text())
                .attr('for', this.search.attr('id'));
            this.opts.element.focus(this.bind(function () { this.focus(); }));

            this.search.on("input paste", this.bind(function() {
                if (this.search.attr('placeholder') && this.search.val().length == 0) return;
                if (!this.isInterfaceEnabled()) return;
                if (!this.opened()) {
                    this.open();
                }
            }));

            this.search.attr("tabindex", this.elementTabIndex);

            this.keydowns = 0;
            this.search.on("keydown", this.bind(function (e) {
                if (!this.isInterfaceEnabled()) return;

                ++this.keydowns;
                var selected = selection.find(".select2-search-choice-focus");
                var prev = selected.prev(".select2-search-choice:not(.select2-locked)");
                var next = selected.next(".select2-search-choice:not(.select2-locked)");
                var pos = getCursorInfo(this.search);

                if (selected.length &&
                    (e.which == KEY.LEFT || e.which == KEY.RIGHT || e.which == KEY.BACKSPACE || e.which == KEY.DELETE || e.which == KEY.ENTER)) {
                    var selectedChoice = selected;
                    if (e.which == KEY.LEFT && prev.length) {
                        selectedChoice = prev;
                    }
                    else if (e.which == KEY.RIGHT) {
                        selectedChoice = next.length ? next : null;
                    }
                    else if (e.which === KEY.BACKSPACE) {
                        if (this.unselect(selected.first())) {
                            this.search.width(10);
                            selectedChoice = prev.length ? prev : next;
                        }
                    } else if (e.which == KEY.DELETE) {
                        if (this.unselect(selected.first())) {
                            this.search.width(10);
                            selectedChoice = next.length ? next : null;
                        }
                    } else if (e.which == KEY.ENTER) {
                        selectedChoice = null;
                    }

                    this.selectChoice(selectedChoice);
                    killEvent(e);
                    if (!selectedChoice || !selectedChoice.length) {
                        this.open();
                    }
                    return;
                } else if (((e.which === KEY.BACKSPACE && this.keydowns == 1)
                    || e.which == KEY.LEFT) && (pos.offset == 0 && !pos.length)) {

                    this.selectChoice(selection.find(".select2-search-choice:not(.select2-locked)").last());
                    killEvent(e);
                    return;
                } else {
                    this.selectChoice(null);
                }

                if (this.opened()) {
                    switch (e.which) {
                    case KEY.UP:
                    case KEY.DOWN:
                        this.moveHighlight((e.which === KEY.UP) ? -1 : 1);
                        killEvent(e);
                        return;
                    case KEY.ENTER:
                        this.selectHighlighted();
                        killEvent(e);
                        return;
                    case KEY.TAB:
                        this.selectHighlighted({noFocus:true});
                        this.close();
                        return;
                    case KEY.ESC:
                        this.cancel(e);
                        killEvent(e);
                        return;
                    }
                }

                if (e.which === KEY.TAB || KEY.isControl(e) || KEY.isFunctionKey(e)
                 || e.which === KEY.BACKSPACE || e.which === KEY.ESC) {
                    return;
                }

                if (e.which === KEY.ENTER) {
                    if (this.opts.openOnEnter === false) {
                        return;
                    } else if (e.altKey || e.ctrlKey || e.shiftKey || e.metaKey) {
                        return;
                    }
                }

                this.open();

                if (e.which === KEY.PAGE_UP || e.which === KEY.PAGE_DOWN) {
                    // prevent the page from scrolling
                    killEvent(e);
                }

                if (e.which === KEY.ENTER) {
                    // prevent form from being submitted
                    killEvent(e);
                }

            }));

            this.search.on("keyup", this.bind(function (e) {
                this.keydowns = 0;
                this.resizeSearch();
            })
            );

            this.search.on("blur", this.bind(function(e) {
                this.container.removeClass("select2-container-active");
                this.search.removeClass("select2-focused");
                this.selectChoice(null);
                if (!this.opened()) this.clearSearch();
                e.stopImmediatePropagation();
                this.opts.element.trigger($.Event("select2-blur"));
            }));

            this.container.on("click", selector, this.bind(function (e) {
                if (!this.isInterfaceEnabled()) return;
                if ($(e.target).closest(".select2-search-choice").length > 0) {
                    // clicked inside a select2 search choice, do not open
                    return;
                }
                this.selectChoice(null);
                this.clearPlaceholder();
                if (!this.container.hasClass("select2-container-active")) {
                    this.opts.element.trigger($.Event("select2-focus"));
                }
                this.open();
                this.focusSearch();
                e.preventDefault();
            }));

            this.container.on("focus", selector, this.bind(function () {
                if (!this.isInterfaceEnabled()) return;
                if (!this.container.hasClass("select2-container-active")) {
                    this.opts.element.trigger($.Event("select2-focus"));
                }
                this.container.addClass("select2-container-active");
                this.dropdown.addClass("select2-drop-active");
                this.clearPlaceholder();
            }));

            this.initContainerWidth();
            this.opts.element.hide();

            // set the placeholder if necessary
            this.clearSearch();
        },

        // multi
        enableInterface: function() {
            if (this.parent.enableInterface.apply(this, arguments)) {
                this.search.prop("disabled", !this.isInterfaceEnabled());
            }
        },

        // multi
        initSelection: function () {
            var data;
            if (this.opts.element.val() === "" && this.opts.element.text() === "") {
                this.updateSelection([]);
                this.close();
                // set the placeholder if necessary
                this.clearSearch();
            }
            if (this.select || this.opts.element.val() !== "") {
                var self = this;
                this.opts.initSelection.call(null, this.opts.element, function(data){
                    if (data !== undefined && data !== null) {
                        self.updateSelection(data);
                        self.close();
                        // set the placeholder if necessary
                        self.clearSearch();
                    }
                });
            }
        },

        // multi
        clearSearch: function () {
            var placeholder = this.getPlaceholder(),
                maxWidth = this.getMaxSearchWidth();

            if (placeholder !== undefined  && this.getVal().length === 0 && this.search.hasClass("select2-focused") === false) {
                this.search.val(placeholder).addClass("select2-default");
                // stretch the search box to full width of the container so as much of the placeholder is visible as possible
                // we could call this.resizeSearch(), but we do not because that requires a sizer and we do not want to create one so early because of a firefox bug, see #944
                this.search.width(maxWidth > 0 ? maxWidth : this.container.css("width"));
            } else {
                this.search.val("").width(10);
            }
        },

        // multi
        clearPlaceholder: function () {
            if (this.search.hasClass("select2-default")) {
                this.search.val("").removeClass("select2-default");
            }
        },

        // multi
        opening: function () {
            this.clearPlaceholder(); // should be done before super so placeholder is not used to search
            this.resizeSearch();

            this.parent.opening.apply(this, arguments);

            this.focusSearch();

            // initializes search's value with nextSearchTerm (if defined by user)
            // ignore nextSearchTerm if the dropdown is opened by the user pressing a letter
            if(this.search.val() === "") {
                if(this.nextSearchTerm != undefined){
                    this.search.val(this.nextSearchTerm);
                    this.search.select();
                }
            }

            this.updateResults(true);
            if (this.opts.shouldFocusInput(this)) {
                this.search.focus();
            }
            this.opts.element.trigger($.Event("select2-open"));
        },

        // multi
        close: function () {
            if (!this.opened()) return;
            this.parent.close.apply(this, arguments);
        },

        // multi
        focus: function () {
            this.close();
            this.search.focus();
        },

        // multi
        isFocused: function () {
            return this.search.hasClass("select2-focused");
        },

        // multi
        updateSelection: function (data) {
            var ids = [], filtered = [], self = this;

            // filter out duplicates
            $(data).each(function () {
                if (indexOf(self.id(this), ids) < 0) {
                    ids.push(self.id(this));
                    filtered.push(this);
                }
            });
            data = filtered;

            this.selection.find(".select2-search-choice").remove();
            $(data).each(function () {
                self.addSelectedChoice(this);
            });
            self.postprocessResults();
        },

        // multi
        tokenize: function() {
            var input = this.search.val();
            input = this.opts.tokenizer.call(this, input, this.data(), this.bind(this.onSelect), this.opts);
            if (input != null && input != undefined) {
                this.search.val(input);
                if (input.length > 0) {
                    this.open();
                }
            }

        },

        // multi
        onSelect: function (data, options) {

            if (!this.triggerSelect(data) || data.text === "") { return; }

            this.addSelectedChoice(data);

            this.opts.element.trigger({ type: "selected", val: this.id(data), choice: data });

            // keep track of the search's value before it gets cleared
            this.nextSearchTerm = this.opts.nextSearchTerm(data, this.search.val());

            this.clearSearch();
            this.updateResults();

            if (this.select || !this.opts.closeOnSelect) this.postprocessResults(data, false, this.opts.closeOnSelect===true);

            if (this.opts.closeOnSelect) {
                this.close();
                this.search.width(10);
            } else {
                if (this.countSelectableResults()>0) {
                    this.search.width(10);
                    this.resizeSearch();
                    if (this.getMaximumSelectionSize() > 0 && this.val().length >= this.getMaximumSelectionSize()) {
                        // if we reached max selection size repaint the results so choices
                        // are replaced with the max selection reached message
                        this.updateResults(true);
                    } else {
                        // initializes search's value with nextSearchTerm and update search result
                        if(this.nextSearchTerm != undefined){
                            this.search.val(this.nextSearchTerm);
                            this.updateResults();
                            this.search.select();
                        }
                    }
                    this.positionDropdown();
                } else {
                    // if nothing left to select close
                    this.close();
                    this.search.width(10);
                }
            }

            // since its not possible to select an element that has already been
            // added we do not need to check if this is a new element before firing change
            this.triggerChange({ added: data });

            if (!options || !options.noFocus)
                this.focusSearch();
        },

        // multi
        cancel: function () {
            this.close();
            this.focusSearch();
        },

        addSelectedChoice: function (data) {
            var enableChoice = !data.locked,
                enabledItem = $(
                    "<li class='select2-search-choice'>" +
                    "    <div></div>" +
                    "    <a href='#' class='select2-search-choice-close' tabindex='-1'></a>" +
                    "</li>"),
                disabledItem = $(
                    "<li class='select2-search-choice select2-locked'>" +
                    "<div></div>" +
                    "</li>");
            var choice = enableChoice ? enabledItem : disabledItem,
                id = this.id(data),
                val = this.getVal(),
                formatted,
                cssClass;

            formatted=this.opts.formatSelection(data, choice.find("div"), this.opts.escapeMarkup);
            if (formatted != undefined) {
                choice.find("div").replaceWith($("<div></div>").html(formatted));
            }
            cssClass=this.opts.formatSelectionCssClass(data, choice.find("div"));
            if (cssClass != undefined) {
                choice.addClass(cssClass);
            }

            if(enableChoice){
              choice.find(".select2-search-choice-close")
                  .on("mousedown", killEvent)
                  .on("click dblclick", this.bind(function (e) {
                  if (!this.isInterfaceEnabled()) return;

                  this.unselect($(e.target));
                  this.selection.find(".select2-search-choice-focus").removeClass("select2-search-choice-focus");
                  killEvent(e);
                  this.close();
                  this.focusSearch();
              })).on("focus", this.bind(function () {
                  if (!this.isInterfaceEnabled()) return;
                  this.container.addClass("select2-container-active");
                  this.dropdown.addClass("select2-drop-active");
              }));
            }

            choice.data("select2-data", data);
            choice.insertBefore(this.searchContainer);

            val.push(id);
            this.setVal(val);
        },

        // multi
        unselect: function (selected) {
            var val = this.getVal(),
                data,
                index;
            selected = selected.closest(".select2-search-choice");

            if (selected.length === 0) {
                throw "Invalid argument: " + selected + ". Must be .select2-search-choice";
            }

            data = selected.data("select2-data");

            if (!data) {
                // prevent a race condition when the 'x' is clicked really fast repeatedly the event can be queued
                // and invoked on an element already removed
                return;
            }

            var evt = $.Event("select2-removing");
            evt.val = this.id(data);
            evt.choice = data;
            this.opts.element.trigger(evt);

            if (evt.isDefaultPrevented()) {
                return false;
            }

            while((index = indexOf(this.id(data), val)) >= 0) {
                val.splice(index, 1);
                this.setVal(val);
                if (this.select) this.postprocessResults();
            }

            selected.remove();

            this.opts.element.trigger({ type: "select2-removed", val: this.id(data), choice: data });
            this.triggerChange({ removed: data });

            return true;
        },

        // multi
        postprocessResults: function (data, initial, noHighlightUpdate) {
            var val = this.getVal(),
                choices = this.results.find(".select2-result"),
                compound = this.results.find(".select2-result-with-children"),
                self = this;

            choices.each2(function (i, choice) {
                var id = self.id(choice.data("select2-data"));
                if (indexOf(id, val) >= 0) {
                    choice.addClass("select2-selected");
                    // mark all children of the selected parent as selected
                    choice.find(".select2-result-selectable").addClass("select2-selected");
                }
            });

            compound.each2(function(i, choice) {
                // hide an optgroup if it doesn't have any selectable children
                if (!choice.is('.select2-result-selectable')
                    && choice.find(".select2-result-selectable:not(.select2-selected)").length === 0) {
                    choice.addClass("select2-selected");
                }
            });

            if (this.highlight() == -1 && noHighlightUpdate !== false && this.opts.closeOnSelect === true){
                self.highlight(0);
            }

            //If all results are chosen render formatNoMatches
            if(!this.opts.createSearchChoice && !choices.filter('.select2-result:not(.select2-selected)').length > 0){
                if(!data || data && !data.more && this.results.find(".select2-no-results").length === 0) {
                    if (checkFormatter(self.opts.formatNoMatches, "formatNoMatches")) {
                        this.results.append("<li class='select2-no-results'>" + evaluate(self.opts.formatNoMatches, self.opts.element, self.search.val()) + "</li>");
                    }
                }
            }

        },

        // multi
        getMaxSearchWidth: function() {
            return this.selection.width() - getSideBorderPadding(this.search);
        },

        // multi
        resizeSearch: function () {
            var minimumWidth, left, maxWidth, containerLeft, searchWidth,
                sideBorderPadding = getSideBorderPadding(this.search);

            minimumWidth = measureTextWidth(this.search) + 10;

            left = this.search.offset().left;

            maxWidth = this.selection.width();
            containerLeft = this.selection.offset().left;

            searchWidth = maxWidth - (left - containerLeft) - sideBorderPadding;

            if (searchWidth < minimumWidth) {
                searchWidth = maxWidth - sideBorderPadding;
            }

            if (searchWidth < 40) {
                searchWidth = maxWidth - sideBorderPadding;
            }

            if (searchWidth <= 0) {
              searchWidth = minimumWidth;
            }

            this.search.width(Math.floor(searchWidth));
        },

        // multi
        getVal: function () {
            var val;
            if (this.select) {
                val = this.select.val();
                return val === null ? [] : val;
            } else {
                val = this.opts.element.val();
                return splitVal(val, this.opts.separator, this.opts.transformVal);
            }
        },

        // multi
        setVal: function (val) {
            var unique;
            if (this.select) {
                this.select.val(val);
            } else {
                unique = [];
                // filter out duplicates
                $(val).each(function () {
                    if (indexOf(this, unique) < 0) unique.push(this);
                });
                this.opts.element.val(unique.length === 0 ? "" : unique.join(this.opts.separator));
            }
        },

        // multi
        buildChangeDetails: function (old, current) {
            var current = current.slice(0),
                old = old.slice(0);

            // remove intersection from each array
            for (var i = 0; i < current.length; i++) {
                for (var j = 0; j < old.length; j++) {
                    if (equal(this.opts.id(current[i]), this.opts.id(old[j]))) {
                        current.splice(i, 1);
                        if(i>0){
                            i--;
                        }
                        old.splice(j, 1);
                        j--;
                    }
                }
            }

            return {added: current, removed: old};
        },


        // multi
        val: function (val, triggerChange) {
            var oldData, self=this;

            if (arguments.length === 0) {
                return this.getVal();
            }

            oldData=this.data();
            if (!oldData.length) oldData=[];

            // val is an id. !val is true for [undefined,null,'',0] - 0 is legal
            if (!val && val !== 0) {
                this.opts.element.val("");
                this.updateSelection([]);
                this.clearSearch();
                if (triggerChange) {
                    this.triggerChange({added: this.data(), removed: oldData});
                }
                return;
            }

            // val is a list of ids
            this.setVal(val);

            if (this.select) {
                this.opts.initSelection(this.select, this.bind(this.updateSelection));
                if (triggerChange) {
                    this.triggerChange(this.buildChangeDetails(oldData, this.data()));
                }
            } else {
                if (this.opts.initSelection === undefined) {
                    throw new Error("val() cannot be called if initSelection() is not defined");
                }

                this.opts.initSelection(this.opts.element, function(data){
                    var ids=$.map(data, self.id);
                    self.setVal(ids);
                    self.updateSelection(data);
                    self.clearSearch();
                    if (triggerChange) {
                        self.triggerChange(self.buildChangeDetails(oldData, self.data()));
                    }
                });
            }
            this.clearSearch();
        },

        // multi
        onSortStart: function() {
            if (this.select) {
                throw new Error("Sorting of elements is not supported when attached to <select>. Attach to <input type='hidden'/> instead.");
            }

            // collapse search field into 0 width so its container can be collapsed as well
            this.search.width(0);
            // hide the container
            this.searchContainer.hide();
        },

        // multi
        onSortEnd:function() {

            var val=[], self=this;

            // show search and move it to the end of the list
            this.searchContainer.show();
            // make sure the search container is the last item in the list
            this.searchContainer.appendTo(this.searchContainer.parent());
            // since we collapsed the width in dragStarted, we resize it here
            this.resizeSearch();

            // update selection
            this.selection.find(".select2-search-choice").each(function() {
                val.push(self.opts.id($(this).data("select2-data")));
            });
            this.setVal(val);
            this.triggerChange();
        },

        // multi
        data: function(values, triggerChange) {
            var self=this, ids, old;
            if (arguments.length === 0) {
                 return this.selection
                     .children(".select2-search-choice")
                     .map(function() { return $(this).data("select2-data"); })
                     .get();
            } else {
                old = this.data();
                if (!values) { values = []; }
                ids = $.map(values, function(e) { return self.opts.id(e); });
                this.setVal(ids);
                this.updateSelection(values);
                this.clearSearch();
                if (triggerChange) {
                    this.triggerChange(this.buildChangeDetails(old, this.data()));
                }
            }
        }
    });

    $.fn.select2 = function () {

        var args = Array.prototype.slice.call(arguments, 0),
            opts,
            select2,
            method, value, multiple,
            allowedMethods = ["val", "destroy", "opened", "open", "close", "focus", "isFocused", "container", "dropdown", "onSortStart", "onSortEnd", "enable", "disable", "readonly", "positionDropdown", "data", "search"],
            valueMethods = ["opened", "isFocused", "container", "dropdown"],
            propertyMethods = ["val", "data"],
            methodsMap = { search: "externalSearch" };

        this.each(function () {
            if (args.length === 0 || typeof(args[0]) === "object") {
                opts = args.length === 0 ? {} : $.extend({}, args[0]);
                opts.element = $(this);

                if (opts.element.get(0).tagName.toLowerCase() === "select") {
                    multiple = opts.element.prop("multiple");
                } else {
                    multiple = opts.multiple || false;
                    if ("tags" in opts) {opts.multiple = multiple = true;}
                }

                select2 = multiple ? new window.Select2["class"].multi() : new window.Select2["class"].single();
                select2.init(opts);
            } else if (typeof(args[0]) === "string") {

                if (indexOf(args[0], allowedMethods) < 0) {
                    throw "Unknown method: " + args[0];
                }

                value = undefined;
                select2 = $(this).data("select2");
                if (select2 === undefined) return;

                method=args[0];

                if (method === "container") {
                    value = select2.container;
                } else if (method === "dropdown") {
                    value = select2.dropdown;
                } else {
                    if (methodsMap[method]) method = methodsMap[method];

                    value = select2[method].apply(select2, args.slice(1));
                }
                if (indexOf(args[0], valueMethods) >= 0
                    || (indexOf(args[0], propertyMethods) >= 0 && args.length == 1)) {
                    return false; // abort the iteration, ready to return first matched value
                }
            } else {
                throw "Invalid arguments to select2 plugin: " + args;
            }
        });
        return (value === undefined) ? this : value;
    };

    // plugin defaults, accessible to users
    $.fn.select2.defaults = {
        width: "copy",
        loadMorePadding: 0,
        closeOnSelect: true,
        openOnEnter: true,
        containerCss: {},
        dropdownCss: {},
        containerCssClass: "",
        dropdownCssClass: "",
        formatResult: function(result, container, query, escapeMarkup) {
            var markup=[];
            markMatch(this.text(result), query.term, markup, escapeMarkup);
            return markup.join("");
        },
        transformVal: function(val) {
            return $.trim(val);
        },
        formatSelection: function (data, container, escapeMarkup) {
            return data ? escapeMarkup(this.text(data)) : undefined;
        },
        sortResults: function (results, container, query) {
            return results;
        },
        formatResultCssClass: function(data) {return data.css;},
        formatSelectionCssClass: function(data, container) {return undefined;},
        minimumResultsForSearch: 0,
        minimumInputLength: 0,
        maximumInputLength: null,
        maximumSelectionSize: 0,
        id: function (e) { return e == undefined ? null : e.id; },
        text: function (e) {
          if (e && this.data && this.data.text) {
            if ($.isFunction(this.data.text)) {
              return this.data.text(e);
            } else {
              return e[this.data.text];
            }
          } else {
            return e.text;
          }
        },
        matcher: function(term, text) {
            return stripDiacritics(''+text).toUpperCase().indexOf(stripDiacritics(''+term).toUpperCase()) >= 0;
        },
        separator: ",",
        tokenSeparators: [],
        tokenizer: defaultTokenizer,
        escapeMarkup: defaultEscapeMarkup,
        blurOnChange: false,
        selectOnBlur: false,
        adaptContainerCssClass: function(c) { return c; },
        adaptDropdownCssClass: function(c) { return null; },
        nextSearchTerm: function(selectedObject, currentSearchTerm) { return undefined; },
        searchInputPlaceholder: '',
        createSearchChoicePosition: 'top',
        shouldFocusInput: function (instance) {
            // Attempt to detect touch devices
            var supportsTouchEvents = (('ontouchstart' in window) ||
                                       (navigator.msMaxTouchPoints > 0));

            // Only devices which support touch events should be special cased
            if (!supportsTouchEvents) {
                return true;
            }

            // Never focus the input if search is disabled
            if (instance.opts.minimumResultsForSearch < 0) {
                return false;
            }

            return true;
        }
    };

    $.fn.select2.locales = [];

    $.fn.select2.locales['en'] = {
         formatMatches: function (matches) { if (matches === 1) { return "One result is available, press enter to select it."; } return matches + " results are available, use up and down arrow keys to navigate."; },
         formatNoMatches: function () { return "No matches found"; },
         formatAjaxError: function (jqXHR, textStatus, errorThrown) { return "Loading failed"; },
         formatInputTooShort: function (input, min) { var n = min - input.length; return "Please enter " + n + " or more character" + (n == 1 ? "" : "s"); },
         formatInputTooLong: function (input, max) { var n = input.length - max; return "Please delete " + n + " character" + (n == 1 ? "" : "s"); },
         formatSelectionTooBig: function (limit) { return "You can only select " + limit + " item" + (limit == 1 ? "" : "s"); },
         formatLoadMore: function (pageNumber) { return "Loading more results…"; },
         formatSearching: function () { return "Searching…"; }
    };

    $.extend($.fn.select2.defaults, $.fn.select2.locales['en']);

    $.fn.select2.ajaxDefaults = {
        transport: $.ajax,
        params: {
            type: "GET",
            cache: false,
            dataType: "json"
        }
    };

    // exports
    window.Select2 = {
        query: {
            ajax: ajax,
            local: local,
            tags: tags
        }, util: {
            debounce: debounce,
            markMatch: markMatch,
            escapeMarkup: defaultEscapeMarkup,
            stripDiacritics: stripDiacritics
        }, "class": {
            "abstract": AbstractSelect2,
            "single": SingleSelect2,
            "multi": MultiSelect2
        }
    };

}(jQuery));













function Location() {
	this.items	= {
	'0':{1:'北京市',22:'天津市',44:'上海市',66:'重庆市',108:'河北省',406:'山西省',622:'内蒙古',804:'辽宁省',945:'吉林省',1036:'黑龙江省',1226:'江苏省',1371:'浙江省',1500:'安徽省',1679:'福建省',1812:'江西省',1992:'山东省',2197:'河南省',2456:'湖北省',2613:'湖南省',2822:'广东省',3015:'广西',3201:'海南省',3235:'四川省',3561:'贵州省',3728:'云南省',3983:'西藏',4136:'陕西省',4334:'甘肃省',4499:'青海省',4588:'宁夏',4624:'新疆',4802:'香港',4822:'澳门',4825:'台湾省'},
	'0,1':{2:'北京市'},
	'0,1,2':{3:'东城区',4:'西城区',5:'崇文区',6:'宣武区',7:'朝阳区',8:'丰台区',9:'石景山区',10:'海淀区',11:'门头沟区',12:'房山区',13:'通州区',14:'顺义区',15:'昌平区',16:'大兴区',17:'怀柔区',18:'平谷区',19:'密云县',20:'延庆县',21:'延庆镇'},
	'0,22':{23:'天津市'},
	'0,22,23':{24:'和平区',25:'河东区',26:'河西区',27:'南开区',28:'河北区',29:'红桥区',30:'塘沽区',31:'汉沽区',32:'大港区',33:'东丽区',34:'西青区',35:'津南区',36:'北辰区',37:'武清区',38:'宝坻区',39:'蓟县',40:'宁河县',41:'芦台镇',42:'静海县',43:'静海镇'},
	'0,44':{45:'上海市'},
	'0,44,45':{46:'黄浦区',47:'卢湾区',48:'徐汇区',49:'长宁区',50:'静安区',51:'普陀区',52:'闸北区',53:'虹口区',54:'杨浦区',55:'闵行区',56:'宝山区',57:'嘉定区',58:'浦东新区',59:'金山区',60:'松江区',61:'青浦区',62:'南汇区',63:'奉贤区',64:'崇明县',65:'城桥镇'},
	'0,66':{67:'重庆市'},
	'0,66,67':{68:'渝中区',69:'大渡口区',70:'江北区',71:'沙坪坝区',72:'九龙坡区',73:'南岸区',74:'北碚区',75:'万盛区',76:'双桥区',77:'渝北区',78:'巴南区',79:'万州区',80:'涪陵区',81:'黔江区',82:'长寿区',83:'合川市',84:'永川区市',85:'江津市',86:'南川市',87:'綦江县',88:'潼南县',89:'铜梁县',90:'大足县',91:'荣昌县',92:'璧山县',93:'垫江县',94:'武隆县',95:'丰都县',96:'城口县',97:'梁平县',98:'开县',99:'巫溪县',100:'巫山县',101:'奉节县',102:'云阳县',103:'忠县',104:'石柱土家族自治县',105:'彭水苗族土家族自治县',106:'酉阳土家族苗族自治县',107:'秀山土家族苗族自治县'},
	'0,108':{109:'石家庄市',145:'张家口市',176:'承德市',196:'秦皇岛市',208:'唐山市',229:'廊坊市',246:'保定市',290:'衡水市',310:'沧州市',337:'邢台市',372:'邯郸市'},
	'0,108,109':{110:'长安区',111:'桥东区',112:'桥西区',113:'新华区',114:'裕华区',115:'井陉矿区',116:'辛集市',117:'藁城市',118:'晋州市',119:'新乐市',120:'鹿泉市',121:'井陉县',122:'微水镇',123:'正定县',124:'正定镇',125:'栾城县',126:'栾城镇',127:'行唐县',128:'龙州镇',129:'灵寿县',130:'灵寿镇',131:'高邑县',132:'高邑镇',133:'深泽县',134:'深泽镇',135:'赞皇县',136:'赞皇镇',137:'无极县',138:'无极镇',139:'平山县',140:'平山镇',141:'元氏县',142:'槐阳镇',143:'赵县',144:'赵州镇'},
	'0,108,145':{146:'桥西区',147:'桥东区',148:'宣化区',149:'下花园区',150:'宣化县',151:'张家口市宣化区',152:'张北县',153:'张北镇',154:'康保县',155:'康保镇',156:'沽源县',157:'平定堡镇',158:'尚义县',159:'南壕堑镇',160:'蔚县',161:'蔚州镇',162:'阳原县',163:'西城镇',164:'怀安县',165:'柴沟堡镇',166:'万全县',167:'孔家庄镇',168:'怀来县',169:'沙城镇',170:'涿鹿县',171:'涿鹿镇',172:'赤城县',173:'赤城镇',174:'崇礼县',175:'西湾子镇'},
	'0,108,176':{177:'双桥区',178:'双滦区',179:'鹰手营子矿区',180:'承德县',181:'下板城镇',182:'兴隆县',183:'兴隆镇',184:'平泉县',185:'平泉镇',186:'滦平县',187:'滦平镇',188:'隆化县',189:'隆化镇',190:'丰宁满族自治县',191:'大阁镇',192:'宽城满族自治县',193:'宽城镇',194:'围场满族蒙古族自治县',195:'围场镇'},
	'0,108,196':{197:'海港区',198:'山海关区',199:'北戴河区',200:'昌黎县',201:'昌黎镇',202:'抚宁县',203:'抚宁镇',204:'卢龙县',205:'卢龙镇',206:'青龙满族自治县',207:'青龙镇'},
	'0,108,208':{209:'路北区',210:'路南区',211:'古冶区',212:'开平区',213:'丰润区',214:'丰南区',215:'遵化市',216:'迁安市',217:'滦县',218:'滦州镇',219:'滦南县',220:'倴城镇',221:'乐亭县',222:'乐亭镇',223:'迁西县',224:'兴城镇',225:'玉田县',226:'玉田镇',227:'唐海县',228:'唐海镇'},
	'0,108,229':{230:'安次区',231:'广阳区',232:'霸州市',233:'三河市',234:'固安县',235:'固安镇',236:'永清县',237:'永清镇',238:'香河县',239:'淑阳镇',240:'大城县',241:'平舒镇',242:'文安县',243:'文安镇',244:'大厂回族自治县',245:'大厂镇'},
	'0,108,246':{247:'新市区',248:'北市区',249:'南市区',250:'定州市',251:'涿州市',252:'安国市',253:'高碑店市',254:'满城县',255:'满城镇',256:'清苑县',257:'清苑镇',258:'易县',259:'易州镇',260:'徐水县',261:'安肃镇',262:'涞源县',263:'涞源镇',264:'定兴县',265:'定兴镇',266:'顺平县',267:'蒲阳镇',268:'唐县',269:'仁厚镇',270:'望都县',271:'望都镇',272:'涞水县',273:'涞水镇',274:'高阳县',275:'高阳镇',276:'安新县',277:'安新镇',278:'雄县',279:'雄州镇',280:'容城县',281:'容城镇',282:'曲阳县',283:'恒州镇',284:'阜平县',285:'阜平镇',286:'博野县',287:'博陵镇',288:'蠡县',289:'蠡吾镇'},
	'0,108,290':{291:'桃城区',292:'冀州市',293:'深州市',294:'枣强县',295:'枣强镇',296:'武邑县',297:'武邑镇',298:'武强县',299:'武强镇',300:'饶阳县',301:'饶阳镇',302:'安平县',303:'安平镇',304:'故城县',305:'郑口镇',306:'景县',307:'景州镇',308:'阜城县',309:'阜城镇'},
	'0,108,310':{311:'运河区',312:'新华区',313:'泊头市',314:'任丘市',315:'黄骅市',316:'河间市',317:'沧县',318:'沧州市新华区',319:'青县',320:'清州镇',321:'东光县',322:'东光镇',323:'海兴县',324:'苏基镇',325:'盐山县',326:'盐山镇',327:'肃宁县',328:'肃宁镇',329:'南皮县',330:'南皮镇',331:'吴桥县',332:'桑园镇',333:'献县',334:'乐寿镇',335:'孟村回族自治县',336:'孟村镇'},
	'0,108,337':{338:'桥东区',339:'桥西区',340:'南宫市',341:'沙河市',342:'邢台县',343:'邢台市桥东区',344:'临城县',345:'临城镇',346:'内丘县',347:'内丘镇',348:'柏乡县',349:'柏乡镇',350:'隆尧县',351:'隆尧镇',352:'任县',353:'任城镇',354:'南和县',355:'和阳镇',356:'宁晋县',357:'凤凰镇',358:'巨鹿县',359:'巨鹿镇',360:'新河县',361:'新河镇',362:'广宗县',363:'广宗镇',364:'平乡县',365:'丰州镇',366:'威县',367:'洺州镇',368:'清河县',369:'葛仙庄镇',370:'临西县',371:'临西镇'},
	'0,108,372':{373:'丛台区',374:'邯山区',375:'复兴区',376:'峰峰矿区',377:'武安市',378:'邯郸县',379:'南堡乡东小屯村',380:'临漳县',381:'临漳镇',382:'成安县',383:'成安镇',384:'大名县',385:'大名镇',386:'涉县',387:'涉城镇',388:'磁县',389:'磁州镇',390:'肥乡县',391:'肥乡镇',392:'永年县',393:'临洺关镇',394:'邱县',395:'新马头镇',396:'鸡泽县',397:'鸡泽镇',398:'广平县',399:'广平镇',400:'馆陶县',401:'馆陶镇',402:'魏县',403:'魏城镇',404:'曲周县',405:'曲周镇'},
	'0,406':{407:'太原市',421:'朔州市',432:'大同市',451:'阳泉市',459:'长治市',483:'晋城市',494:'忻州市',521:'晋中市',542:'临汾市',574:'吕梁市',598:'运城市'},
	'0,406,407':{408:'杏花岭区',409:'小店区',410:'迎泽区',411:'尖草坪区',412:'万柏林区',413:'晋源区',414:'古交市',415:'清徐县',416:'清源镇',417:'阳曲县',418:'黄寨镇',419:'娄烦县',420:'娄烦镇'},
	'0,406,421':{422:'朔城区',423:'平鲁区',424:'山阴县',425:'岱岳乡',426:'应县',427:'金城镇',428:'右玉县',429:'新城镇',430:'怀仁县',431:'云中镇'},
	'0,406,432':{433:'城区',434:'矿区',435:'南郊区',436:'新荣区',437:'阳高县',438:'龙泉镇',439:'天镇县',440:'玉泉镇',441:'广灵县',442:'壶泉镇',443:'灵丘县',444:'武灵镇',445:'浑源县',446:'永安镇',447:'左云县',448:'云兴镇',449:'大同县',450:'西坪镇'},
	'0,406,451':{452:'城区',453:'矿区',454:'郊区',455:'平定县',456:'冠山镇',457:'盂县',458:'秀水镇'},
	'0,406,459':{460:'城区',461:'郊区',462:'潞城市',463:'长治县',464:'韩店镇',465:'襄垣县',466:'古韩镇',467:'屯留县',468:'麟绛镇',469:'平顺县',470:'青羊镇',471:'黎城县',472:'黎侯镇',473:'壶关县',474:'龙泉镇',475:'长子县',476:'丹朱镇',477:'武乡县',478:'丰州镇',479:'沁县',480:'定昌镇',481:'沁源县',482:'沁河镇'},
	'0,406,483':{484:'城区',485:'高平市',486:'泽州县',487:'南村镇',488:'沁水县',489:'龙港镇',490:'阳城县',491:'凤城镇',492:'陵川县',493:'崇文镇'},
	'0,406,494':{495:'忻府区',496:'原平市',497:'定襄县',498:'晋昌镇',499:'五台县',500:'台城镇',501:'代县',502:'上馆镇',503:'繁峙县',504:'繁城镇',505:'宁武县',506:'凤凰镇',507:'静乐县',508:'鹅城镇',509:'神池县',510:'龙泉镇',511:'五寨县',512:'砚城镇',513:'岢岚县',514:'岚漪镇',515:'河曲县',516:'文笔镇',517:'保德县',518:'东关镇',519:'偏关县',520:'新关镇'},
	'0,406,521':{522:'榆次区',523:'介休市',524:'榆社县',525:'箕城镇',526:'左权县',527:'辽阳镇',528:'和顺县',529:'义兴镇',530:'昔阳县',531:'乐平镇',532:'寿阳县',533:'朝阳镇',534:'太谷县',535:'明星镇',536:'祁县',537:'昭余镇',538:'平遥县',539:'古陶镇',540:'灵石县',541:'翠峰镇'},
	'0,406,542':{543:'尧都区',544:'侯马市',545:'霍州市',546:'曲沃县',547:'乐昌镇',548:'翼城县',549:'唐兴镇',550:'襄汾县',551:'新城镇',552:'洪洞县',553:'大槐树镇',554:'古县',555:'岳阳镇',556:'安泽县',557:'府城镇',558:'浮山县',559:'天坛镇',560:'吉县',561:'吉昌镇',562:'乡宁县',563:'昌宁镇',564:'蒲县',565:'蒲城镇',566:'大宁县',567:'昕水镇',568:'永和县',569:'芝河镇',570:'隰县',571:'龙泉镇',572:'汾西县',573:'永安镇'},
	'0,406,574':{575:'离石区',576:'孝义市',577:'汾阳市',578:'文水县',579:'凤城镇',580:'中阳县',581:'宁乡镇',582:'兴县',583:'蔚汾镇',584:'临县',585:'临泉镇',586:'方山县',587:'圪洞镇',588:'柳林县',589:'柳林镇',590:'岚县',591:'东村镇',592:'交口县',593:'水头镇',594:'交城县',595:'天宁镇',596:'石楼县',597:'灵泉镇'},
	'0,406,598':{599:'盐湖区',600:'永济市',601:'河津市',602:'芮城县',603:'古魏镇',604:'临猗县',605:'猗氏镇',606:'万荣县',607:'解店镇',608:'新绛县',609:'龙兴镇',610:'稷山县',611:'稷峰镇',612:'闻喜县',613:'桐城镇',614:'夏县',615:'瑶峰镇',616:'绛县',617:'古绛镇',618:'平陆县',619:'圣人涧镇',620:'垣曲县',621:'新城镇'},
	'0,622':{623:'呼和浩特市',638:'包头市',651:'乌海市',655:'赤峰市',677:'通辽市',692:'呼伦贝尔市',713:'鄂尔多斯市',729:'乌兰察布市',750:'巴彦淖尔市',764:'兴安盟',775:'锡林郭勒盟',798:'阿拉善盟'},
	'0,622,623':{624:'回民区',625:'新城区',626:'玉泉区',627:'赛罕区',628:'托克托县',629:'双河镇',630:'武川县',631:'可可以力更镇',632:'和林格尔县',633:'城关镇',634:'清水河县',635:'城关镇',636:'土默特左旗',637:'察素齐镇'},
	'0,622,638':{639:'昆都仑区',640:'东河区',641:'青山区',642:'石拐区',643:'白云矿区',644:'九原区',645:'固阳县',646:'金山镇',647:'土默特右旗',648:'萨拉齐镇',649:'达尔罕茂明安联合旗',650:'百灵庙镇'},
	'0,622,651':{652:'海勃湾区',653:'海南区',654:'乌达区'},
	'0,622,655':{656:'红山区',657:'元宝山区',658:'松山区',659:'宁城县',660:'天义镇',661:'林西县',662:'林西镇',663:'阿鲁科尔沁旗',664:'天山镇',665:'巴林左旗',666:'林东镇',667:'巴林右旗',668:'大板镇',669:'克什克腾旗',670:'经棚镇',671:'翁牛特旗',672:'乌丹镇',673:'喀喇沁旗',674:'锦山镇',675:'敖汉旗',676:'新惠镇'},
	'0,622,677':{678:'科尔沁区',679:'霍林郭勒市',680:'开鲁县',681:'开鲁镇',682:'库伦旗',683:'库伦镇',684:'奈曼旗',685:'大沁他拉镇',686:'扎鲁特旗',687:'鲁北镇',688:'科尔沁左翼中旗',689:'保康镇',690:'科尔沁左翼后旗',691:'甘旗卡镇'},
	'0,622,692':{693:'海拉尔区',694:'满洲里市',695:'扎兰屯市',696:'牙克石市',697:'根河市',698:'额尔古纳市',699:'阿荣旗',700:'那吉镇',701:'新巴尔虎右旗',702:'阿拉坦额莫勒镇',703:'新巴尔虎左旗',704:'阿穆古郎镇',705:'陈巴尔虎旗',706:'巴彦库仁镇',707:'鄂伦春自治旗',708:'阿里河镇',709:'鄂温克族自治旗',710:'巴彦托海镇',711:'莫力达瓦达斡尔族自治旗',712:'尼尔基镇'},
	'0,622,713':{714:'东胜区',715:'达拉特旗',716:'树林召镇',717:'准格尔旗',718:'薛家湾镇',719:'鄂托克前旗',720:'敖勒召其镇',721:'鄂托克旗',722:'乌兰镇',723:'杭锦旗',724:'锡尼镇',725:'乌审旗',726:'嘎鲁图镇',727:'伊金霍洛旗',728:'阿勒腾席热镇'},
	'0,622,729':{730:'集宁区',731:'丰镇市',732:'卓资县',733:'卓资山镇',734:'化德县',735:'长顺镇',736:'商都县',737:'商都镇',738:'兴和县',739:'城关镇',740:'凉城县',741:'岱海镇',742:'察哈尔右翼前旗',743:'土贵乌拉镇',744:'察哈尔右翼中旗',745:'科布尔镇',746:'察哈尔右翼后旗',747:'白音察干镇',748:'四子王旗',749:'乌兰花镇'},
	'0,622,750':{751:'临河区',752:'五原县',753:'隆兴昌镇',754:'磴口县',755:'巴彦高勒镇',756:'乌拉特前旗',757:'乌拉山镇',758:'乌拉特中旗',759:'海流图镇',760:'乌拉特后旗',761:'巴音宝力格镇',762:'杭锦后旗',763:'陕坝镇'},
	'0,622,764':{765:'乌兰浩特市',766:'阿尔山市',767:'突泉县',768:'突泉镇',769:'科尔沁右翼前旗',770:'大坝沟镇',771:'科尔沁右翼中旗',772:'巴彦呼硕镇',773:'扎赉特旗',774:'音德尔镇'},
	'0,622,775':{776:'锡林浩特市',777:'二连浩特市',778:'多伦县',779:'多伦淖尔镇',780:'阿巴嘎旗',781:'别力古台镇',782:'苏尼特左旗',783:'满都拉图镇',784:'苏尼特右旗',785:'赛汉塔拉镇',786:'东乌珠穆沁旗',787:'乌里雅斯太镇',788:'西乌珠穆沁旗',789:'巴拉嘎尔郭勒镇',790:'太仆寺旗',791:'宝昌镇',792:'镶黄旗',793:'新宝拉格镇',794:'正镶白旗',795:'明安图镇',796:'正蓝旗',797:'上都镇'},
	'0,622,798':{799:'巴彦浩特镇',800:'阿拉善右旗',801:'额肯呼都格镇',802:'额济纳旗',803:'达来呼布镇'},
	'0,804':{805:'沈阳市',822:'朝阳市',832:'阜新市',842:'铁岭市',853:'抚顺市',864:'本溪市',873:'辽阳市',882:'鞍山市',892:'丹东市',900:'大连市',912:'营口市',919:'盘锦市',926:'锦州市',936:'葫芦岛市'},
	'0,804,805':{806:'沈河区',807:'和平区',808:'大东区',809:'皇姑区',810:'铁西区',811:'苏家屯区',812:'东陵区',813:'新城子区',814:'于洪区',815:'新民市',816:'辽中县',817:'辽中镇',818:'康平县',819:'康平镇',820:'法库县',821:'法库镇'},
	'0,804,822':{823:'双塔区',824:'龙城区',825:'北票市',826:'凌源市',827:'朝阳县',828:'朝阳市双塔区',829:'建平县',830:'喀喇沁左翼蒙古族自治县',831:'大城子镇'},
	'0,804,832':{833:'海州区',834:'新邱区',835:'太平区',836:'清河门区',837:'细河区',838:'彰武县',839:'彰武镇',840:'阜新蒙古族自治县',841:'阜新镇'},
	'0,804,842':{843:'银州区',844:'清河区',845:'调兵山市',846:'开原市',847:'铁岭县',848:'铁岭市银州区',849:'西丰县',850:'西丰镇',851:'昌图县',852:'昌图镇'},
	'0,804,853':{854:'顺城区',855:'新抚区',856:'东洲区',857:'望花区',858:'抚顺县',859:'抚顺市顺城区',860:'新宾满族自治县',861:'新宾镇',862:'清原满族自治县',863:'清原镇'},
	'0,804,864':{865:'平山区',866:'溪湖区',867:'明山区',868:'南芬区',869:'本溪满族自治县',870:'小市镇',871:'桓仁满族自治县',872:'桓仁镇'},
	'0,804,873':{874:'白塔区',875:'文圣区',876:'宏伟区',877:'弓长岭区',878:'太子河区',879:'灯塔市',880:'辽阳县',881:'首山镇'},
	'0,804,882':{883:'铁东区',884:'铁西区',885:'立山区',886:'千山区',887:'海城市',888:'台安县',889:'台安镇',890:'岫岩满族自治县',891:'岫岩镇'},
	'0,804,892':{893:'振兴区',894:'元宝区',895:'振安区',896:'凤城市',897:'东港市',898:'宽甸满族自治县',899:'宽甸镇'},
	'0,804,900':{901:'西岗区',902:'中山区',903:'沙河口区',904:'甘井子区',905:'旅顺口区',906:'金州区',907:'瓦房店市',908:'普兰店市',909:'庄河市',910:'长海县',911:'大长山岛镇'},
	'0,804,912':{913:'站前区',914:'西市区',915:'鲅鱼圈区',916:'老边区',917:'大石桥市',918:'盖州市'},
	'0,804,919':{920:'兴隆台区',921:'双台子区',922:'大洼县',923:'大洼镇',924:'盘山县',925:'盘锦市双台子区'},
	'0,804,926':{927:'太和区',928:'古塔区',929:'凌河区',930:'凌海市',931:'北宁市',932:'黑山县',933:'黑山镇',934:'义县',935:'义州镇'},
	'0,804,936':{937:'龙港区',938:'连山区',939:'南票区',940:'兴城市',941:'绥中县',942:'绥中镇',943:'建昌县',944:'建昌镇'},
	'0,945':{946:'长春市',958:'白城市',966:'松原市',976:'吉林市',987:'四平市',996:'辽源市',1003:'通化市',1014:'白山市',1025:'延边州'},
	'0,945,946':{947:'朝阳区',948:'南关区',949:'宽城区',950:'二道区',951:'绿园区',952:'双阳区',953:'德惠市',954:'九台市',955:'榆树市',956:'农安县',957:'农安镇'},
	'0,945,958':{959:'洮北区',960:'大安市',961:'洮南市',962:'镇赉县',963:'镇赉镇',964:'通榆县',965:'开通镇'},
	'0,945,966':{967:'宁江区',968:'扶余县',969:'三岔河镇',970:'长岭县',971:'长岭镇',972:'乾安县',973:'乾安镇',974:'前郭尔罗斯蒙古族自治县',975:'前郭镇'},
	'0,945,976':{977:'船营区',978:'龙潭区',979:'昌邑区',980:'丰满区',981:'磐石市',982:'蛟河市',983:'桦甸市',984:'舒兰市',985:'永吉县',986:'口前镇'},
	'0,945,987':{988:'铁西区',989:'铁东区',990:'双辽市',991:'公主岭市',992:'梨树县',993:'梨树镇',994:'伊通满族自治县',995:'伊通镇'},
	'0,945,996':{997:'龙山区',998:'西安区',999:'东丰县',1000:'东丰镇',1001:'东辽县',1002:'白泉镇'},
	'0,945,1003':{1004:'东昌区',1005:'二道江区',1006:'梅河口市',1007:'集安市',1008:'通化县',1009:'快大茂镇',1010:'辉南县',1011:'朝阳镇',1012:'柳河县',1013:'柳河镇'},
	'0,945,1014':{1015:'八道江区',1016:'临江市',1017:'江源县',1018:'孙家堡子镇',1019:'抚松县',1020:'抚松镇',1021:'靖宇县',1022:'靖宇镇',1023:'长白朝鲜族自治县',1024:'长白镇'},
	'0,945,1025':{1026:'延吉市',1027:'图们市',1028:'敦化市',1029:'珲春市',1030:'龙井市',1031:'和龙市',1032:'汪清县',1033:'汪清镇',1034:'安图县',1035:'明月镇'},
	'0,1036':{1037:'哈尔滨市',1064:'齐齐哈尔市',1089:'七台河市',1095:'黑河市',1105:'大庆市',1119:'鹤岗市',1130:'伊春市',1149:'佳木斯市',1165:'双鸭山市',1178:'鸡西市',1189:'牡丹江市',1202:'绥化市',1219:'大兴安岭地区'},
	'0,1036,1037':{1038:'松北区',1039:'道里区',1040:'南岗区',1041:'道外区',1042:'香坊区',1043:'动力区',1044:'平房区',1045:'呼兰区',1046:'双城市',1047:'尚志市',1048:'五常市',1049:'阿城市',1050:'依兰县',1051:'依兰镇',1052:'方正县',1053:'方正镇',1054:'宾县',1055:'宾州镇',1056:'巴彦县',1057:'巴彦镇',1058:'木兰县',1059:'木兰镇',1060:'通河县',1061:'通河镇',1062:'延寿县',1063:'延寿镇'},
	'0,1036,1064':{1065:'建华区',1066:'龙沙区',1067:'铁锋区',1068:'昂昂溪区',1069:'富拉尔基区',1070:'碾子山区',1071:'梅里斯达斡尔族区',1072:'讷河市',1073:'龙江县',1074:'龙江镇',1075:'依安县',1076:'依安镇',1077:'泰来县',1078:'泰来镇',1079:'甘南县',1080:'甘南镇',1081:'富裕县',1082:'富裕镇',1083:'克山县',1084:'克山镇',1085:'克东县',1086:'克东镇',1087:'拜泉县',1088:'拜泉镇'},
	'0,1036,1089':{1090:'桃山区',1091:'新兴区',1092:'茄子河区',1093:'勃利县',1094:'勃利镇'},
	'0,1036,1095':{1096:'爱辉区',1097:'北安市',1098:'五大连池市',1099:'嫩江县',1100:'嫩江镇',1101:'逊克县',1102:'边疆镇',1103:'孙吴县',1104:'孙吴镇'},
	'0,1036,1105':{1106:'萨尔图区',1107:'龙凤区',1108:'让胡路区',1109:'大同区',1110:'红岗区',1111:'肇州县',1112:'肇州镇',1113:'肇源县',1114:'肇源镇',1115:'林甸县',1116:'林甸镇',1117:'杜尔伯特蒙古族自治县',1118:'泰康镇'},
	'0,1036,1119':{1120:'兴山区',1121:'向阳区',1122:'工农区',1123:'南山区',1124:'兴安区',1125:'东山区',1126:'萝北县',1127:'凤翔镇',1128:'绥滨县',1129:'绥滨镇'},
	'0,1036,1130':{1131:'伊春区',1132:'南岔区',1133:'友好区',1134:'西林区',1135:'翠峦区',1136:'新青区',1137:'美溪区',1138:'金山屯区',1139:'五营区',1140:'乌马河区',1141:'汤旺河区',1142:'带岭区',1143:'乌伊岭区',1144:'红星区',1145:'上甘岭区',1146:'铁力市',1147:'嘉荫县',1148:'朝阳镇'},
	'0,1036,1149':{1150:'前进区',1151:'永红区',1152:'向阳区',1153:'东风区',1154:'郊区',1155:'同江市',1156:'富锦市',1157:'桦南县',1158:'桦南镇',1159:'桦川县',1160:'悦来镇',1161:'汤原县',1162:'汤原镇',1163:'抚远县',1164:'抚远镇'},
	'0,1036,1165':{1166:'尖山区',1167:'岭东区',1168:'四方台区',1169:'宝山区',1170:'集贤县',1171:'福利镇',1172:'友谊县',1173:'友谊镇',1174:'宝清县',1175:'宝清镇',1176:'饶河县',1177:'饶河镇'},
	'0,1036,1178':{1179:'鸡冠区',1180:'恒山区',1181:'滴道区',1182:'梨树区',1183:'城子河区',1184:'麻山区',1185:'虎林市',1186:'密山市',1187:'鸡东县',1188:'鸡东镇'},
	'0,1036,1189':{1190:'爱民区',1191:'东安区',1192:'阳明区',1193:'西安区',1194:'穆棱市',1195:'绥芬河市',1196:'海林市',1197:'宁安市',1198:'东宁县',1199:'东宁镇',1200:'林口县',1201:'林口镇'},
	'0,1036,1202':{1203:'北林区',1204:'安达市',1205:'肇东市',1206:'海伦市',1207:'望奎县',1208:'望奎镇',1209:'兰西县',1210:'兰西镇',1211:'青冈县',1212:'青冈镇',1213:'庆安县',1214:'庆安镇',1215:'明水县',1216:'明水镇',1217:'绥棱县',1218:'绥棱镇'},
	'0,1036,1219':{1220:'呼玛县',1221:'呼玛镇',1222:'塔河县',1223:'塔河镇',1224:'漠河县',1225:'西林吉镇'},
	'0,1226':{1227:'南京市',1243:'徐州市',1259:'连云港市',1271:'宿迁市',1280:'淮安市',1293:'盐城市',1308:'扬州市',1317:'泰州市',1324:'南通市',1335:'镇江市',1342:'常州市',1350:'无锡市',1359:'苏州市'},
	'0,1226,1227':{1228:'玄武区',1229:'白下区',1230:'秦淮区',1231:'建邺区',1232:'鼓楼区',1233:'下关区',1234:'浦口区',1235:'六合区',1236:'栖霞区',1237:'雨花台区',1238:'江宁区',1239:'溧水县',1240:'永阳镇',1241:'高淳县',1242:'淳溪镇'},
	'0,1226,1243':{1244:'云龙区',1245:'鼓楼区',1246:'九里区',1247:'贾汪区',1248:'泉山区',1249:'邳州市',1250:'新沂市',1251:'铜山县',1252:'铜山镇',1253:'睢宁县',1254:'睢城镇',1255:'沛县',1256:'沛城镇',1257:'丰县',1258:'凤城镇'},
	'0,1226,1259':{1260:'新浦区',1261:'连云区',1262:'海州区',1263:'赣榆县',1264:'青口镇',1265:'灌云县',1266:'伊山镇',1267:'东海县',1268:'牛山镇',1269:'灌南县',1270:'新安镇'},
	'0,1226,1271':{1272:'宿城区',1273:'宿豫区',1274:'沭阳县',1275:'沭城镇',1276:'泗阳县',1277:'众兴镇',1278:'泗洪县',1279:'青阳镇'},
	'0,1226,1280':{1281:'清河区',1282:'清浦区',1283:'楚州区',1284:'淮阴区',1285:'金湖县',1286:'黎城镇',1287:'盱眙县',1288:'盱城镇',1289:'洪泽县',1290:'高良涧镇',1291:'涟水县',1292:'涟城镇'},
	'0,1226,1293':{1294:'亭湖区',1295:'盐都区',1296:'东台市',1297:'大丰市',1298:'射阳县',1299:'合德镇',1300:'阜宁县',1301:'阜城镇',1302:'滨海县',1303:'东坎镇',1304:'响水县',1305:'响水镇',1306:'建湖县',1307:'近湖镇'},
	'0,1226,1308':{1309:'维扬区',1310:'广陵区',1311:'邗江区',1312:'仪征市',1313:'江都市',1314:'高邮市',1315:'宝应县',1316:'安宜镇'},
	'0,1226,1317':{1318:'海陵区',1319:'高港区',1320:'靖江市',1321:'泰兴市',1322:'姜堰市',1323:'兴化市'},
	'0,1226,1324':{1325:'崇川区',1326:'港闸区',1327:'海门市',1328:'启东市',1329:'通州市',1330:'如皋市',1331:'如东县',1332:'掘港镇',1333:'海安县',1334:'海安镇'},
	'0,1226,1335':{1336:'京口区',1337:'润州区',1338:'丹徒区',1339:'扬中市',1340:'丹阳市',1341:'句容市'},
	'0,1226,1342':{1343:'钟楼区',1344:'天宁区',1345:'戚墅堰区',1346:'新北区',1347:'武进区',1348:'金坛市',1349:'溧阳市'},
	'0,1226,1350':{1351:'崇安区',1352:'南长区',1353:'北塘区',1354:'滨湖区',1355:'惠山区',1356:'锡山区',1357:'江阴市',1358:'宜兴市'},
	'0,1226,1359':{1360:'金阊区',1361:'沧浪区',1362:'平江区',1363:'虎丘区',1364:'吴中区',1365:'相城区',1366:'吴江市',1367:'昆山市',1368:'太仓市',1369:'常熟市',1370:'张家港市'},
	'0,1371':{1372:'杭州市',1387:'湖州市',1396:'嘉兴市',1406:'舟山市',1413:'宁波市',1425:'绍兴市',1433:'衢州市',1442:'金华市',1453:'台州市',1465:'温州市',1483:'丽水市'},
	'0,1371,1372':{1373:'拱墅区',1374:'上城区',1375:'下城区',1376:'江干区',1377:'西湖区',1378:'滨江区',1379:'余杭区',1380:'萧山区',1381:'临安市',1382:'富阳市',1383:'建德市',1384:'桐庐县',1385:'淳安县',1386:'千岛湖镇'},
	'0,1371,1387':{1388:'吴兴区',1389:'南浔区',1390:'长兴县',1391:'雉城镇',1392:'德清县',1393:'武康镇',1394:'安吉县',1395:'递铺镇'},
	'0,1371,1396':{1397:'南湖区',1398:'秀洲区',1399:'平湖市',1400:'海宁市',1401:'桐乡市',1402:'嘉善县',1403:'魏塘镇',1404:'海盐县',1405:'武原镇'},
	'0,1371,1406':{1407:'定海区',1408:'普陀区',1409:'岱山县',1410:'高亭镇',1411:'嵊泗县',1412:'菜园镇'},
	'0,1371,1413':{1414:'海曙区',1415:'江东区',1416:'江北区',1417:'北仑区',1418:'镇海区',1419:'鄞州区',1420:'慈溪市',1421:'余姚市',1422:'奉化市',1423:'宁海县',1424:'象山县'},
	'0,1371,1425':{1426:'越城区',1427:'诸暨市',1428:'上虞市',1429:'嵊州市',1430:'绍兴县',1431:'新昌县',1432:'城关镇'},
	'0,1371,1433':{1434:'柯城区',1435:'衢江区',1436:'江山市',1437:'常山县',1438:'天马镇',1439:'开化县',1440:'城关镇',1441:'龙游县'},
	'0,1371,1442':{1443:'婺城区',1444:'金东区',1445:'兰溪市',1446:'永康市',1447:'义乌市',1448:'东阳市',1449:'武义县',1450:'浦江县',1451:'磐安县',1452:'安文镇'},
	'0,1371,1453':{1454:'椒江区',1455:'黄岩区',1456:'路桥区',1457:'临海市',1458:'温岭市',1459:'三门县',1460:'海游镇',1461:'天台县',1462:'仙居县',1463:'玉环县',1464:'珠港镇'},
	'0,1371,1465':{1466:'鹿城区',1467:'龙湾区',1468:'瓯海区',1469:'瑞安市',1470:'乐清市',1471:'永嘉县',1472:'上塘镇',1473:'文成县',1474:'大峃镇',1475:'平阳县',1476:'昆阳镇',1477:'泰顺县',1478:'罗阳镇',1479:'洞头县',1480:'北岙镇',1481:'苍南县',1482:'灵溪镇'},
	'0,1371,1483':{1484:'莲都区',1485:'龙泉市',1486:'缙云县',1487:'五云镇',1488:'青田县',1489:'鹤城镇',1490:'云和县',1491:'云和镇',1492:'遂昌县',1493:'妙高镇',1494:'松阳县',1495:'西屏镇',1496:'庆元县',1497:'松源镇',1498:'景宁畲族自治县',1499:'鹤溪镇'},
	'0,1500':{1501:'合肥市',1512:'宿州市',1522:'淮北市',1528:'亳州市',1536:'阜阳市',1549:'蚌埠市',1560:'淮南市',1568:'滁州市',1581:'马鞍山市',1587:'芜湖市',1598:'铜陵市',1604:'安庆市',1623:'黄山市',1635:'六安市',1648:'巢湖市',1658:'池州市',1666:'宣城市'},
	'0,1500,1501':{1502:'庐阳区',1503:'瑶海区',1504:'蜀山区',1505:'包河区',1506:'长丰县',1507:'水湖镇',1508:'肥东县',1509:'店埠镇',1510:'肥西县',1511:'上派镇'},
	'0,1500,1512':{1513:'埇桥区',1514:'砀山县',1515:'砀城镇',1516:'萧县',1517:'龙城镇',1518:'灵璧县',1519:'灵城镇',1520:'泗县',1521:'泗城镇'},
	'0,1500,1522':{1523:'相山区',1524:'杜集区',1525:'烈山区',1526:'濉溪县',1527:'濉溪镇'},
	'0,1500,1528':{1529:'谯城区',1530:'涡阳县',1531:'城关镇',1532:'蒙城县',1533:'城关镇',1534:'利辛县',1535:'城关镇'},
	'0,1500,1536':{1537:'颍州区',1538:'颍东区',1539:'颍泉区',1540:'界首市',1541:'临泉县',1542:'城关镇',1543:'太和县',1544:'城关镇',1545:'阜南县',1546:'城关镇',1547:'颍上县',1548:'慎城镇'},
	'0,1500,1549':{1550:'蚌山区',1551:'龙子湖区',1552:'禹会区',1553:'淮上区',1554:'怀远县',1555:'城关镇',1556:'五河县',1557:'城关镇',1558:'固镇县',1559:'城关镇'},
	'0,1500,1560':{1561:'田家庵区',1562:'大通区',1563:'谢家集区',1564:'八公山区',1565:'潘集区',1566:'凤台县',1567:'城关镇'},
	'0,1500,1568':{1569:'琅区',1570:'南谯区',1571:'明光市',1572:'天长市',1573:'来安县',1574:'新安镇',1575:'全椒县',1576:'襄河镇',1577:'定远县',1578:'定城镇',1579:'凤阳县',1580:'府城镇'},
	'0,1500,1581':{1582:'雨山区',1583:'花山区',1584:'金家庄区',1585:'当涂县',1586:'姑孰镇'},
	'0,1500,1587':{1588:'镜湖区',1589:'弋江区',1590:'三山区',1591:'鸠江区',1592:'芜湖县',1593:'湾镇',1594:'繁昌县',1595:'繁阳镇',1596:'南陵县',1597:'籍山镇'},
	'0,1500,1598':{1599:'铜官山区',1600:'狮子山区',1601:'郊区',1602:'铜陵县',1603:'五松镇'},
	'0,1500,1604':{1605:'迎江区',1606:'大观区',1607:'宜秀区',1608:'桐城市',1609:'怀宁县',1610:'高河镇',1611:'枞阳县',1612:'枞阳镇',1613:'潜山县',1614:'梅城镇',1615:'太湖县',1616:'晋熙镇',1617:'宿松县',1618:'孚玉镇',1619:'望江县',1620:'雷阳镇',1621:'岳西县',1622:'天堂镇'},
	'0,1500,1623':{1624:'屯溪区',1625:'黄山区',1626:'徽州区',1627:'歙县',1628:'徽城镇',1629:'休宁县',1630:'海阳镇',1631:'黟县',1632:'碧阳镇',1633:'祁门县',1634:'祁山镇'},
	'0,1500,1635':{1636:'金安区',1637:'裕安区',1638:'寿县',1639:'寿春镇',1640:'霍邱县',1641:'城关镇',1642:'舒城县',1643:'城关镇',1644:'金寨县',1645:'梅山镇',1646:'霍山县',1647:'衡山镇'},
	'0,1500,1648':{1649:'居巢区',1650:'庐江县',1651:'庐城镇',1652:'无为县',1653:'无城镇',1654:'含山县',1655:'环峰镇',1656:'和县',1657:'历阳镇'},
	'0,1500,1658':{1659:'贵池区',1660:'东至县',1661:'尧渡镇',1662:'石台县',1663:'七里镇',1664:'青阳县',1665:'蓉城镇'},
	'0,1500,1666':{1667:'宣州区',1668:'宁国市',1669:'郎溪县',1670:'建平镇',1671:'广德县',1672:'桃州镇',1673:'泾县',1674:'泾川镇',1675:'旌德县',1676:'旌阳镇',1677:'绩溪县',1678:'华阳镇'},
	'0,1679':{1680:'福州市',1699:'南平市',1713:'莆田市',1719:'三明市',1740:'泉州市',1758:'厦门市',1765:'漳州市',1785:'龙岩市',1798:'宁德市'},
	'0,1679,1680':{1681:'鼓楼区',1682:'台江区',1683:'仓山区',1684:'马尾区',1685:'晋安区',1686:'福清市',1687:'长乐市',1688:'闽侯县',1689:'连江县',1690:'凤城镇',1691:'罗源县',1692:'凤山镇',1693:'闽清县',1694:'梅城镇',1695:'永泰县',1696:'樟城镇',1697:'平潭县',1698:'潭城镇'},
	'0,1679,1699':{1700:'延平区',1701:'邵武市',1702:'武夷山市',1703:'建瓯市',1704:'建阳市',1705:'顺昌县',1706:'浦城县',1707:'光泽县',1708:'杭川镇',1709:'松溪县',1710:'松源镇',1711:'政和县',1712:'熊山镇'},
	'0,1679,1713':{1714:'城厢区',1715:'涵江区',1716:'荔城区',1717:'秀屿区',1718:'仙游县'},
	'0,1679,1719':{1720:'梅列区',1721:'三元区',1722:'永安市',1723:'明溪县',1724:'雪峰镇',1725:'清流县',1726:'龙津镇',1727:'宁化县',1728:'翠江镇',1729:'大田县',1730:'均溪镇',1731:'尤溪县',1732:'城关镇',1733:'沙县',1734:'将乐县',1735:'古镛镇',1736:'泰宁县',1737:'杉城镇',1738:'建宁县',1739:'濉城镇'},
	'0,1679,1740':{1741:'鲤城区',1742:'丰泽区',1743:'洛江区',1744:'泉港区',1745:'石狮市',1746:'晋江市',1747:'南安市',1748:'惠安县',1749:'螺城镇',1750:'安溪县',1751:'凤城镇',1752:'永春县',1753:'桃城镇',1754:'德化县',1755:'浔中镇',1756:'金门县',1757:'☆'},
	'0,1679,1758':{1759:'思明区',1760:'海沧区',1761:'湖里区',1762:'集美区',1763:'同安区',1764:'翔安区'},
	'0,1679,1765':{1766:'芗城区',1767:'龙文区',1768:'龙海市',1769:'云霄县',1770:'云陵镇',1771:'漳浦县',1772:'绥安镇',1773:'诏安县',1774:'南诏镇',1775:'长泰县',1776:'武安镇',1777:'东山县',1778:'西埔镇',1779:'南靖县',1780:'山城镇',1781:'平和县',1782:'小溪镇',1783:'华安县',1784:'华丰镇'},
	'0,1679,1785':{1786:'新罗区',1787:'漳平市',1788:'长汀县',1789:'汀州镇',1790:'永定县',1791:'凤城镇',1792:'上杭县',1793:'临江镇',1794:'武平县',1795:'平川镇',1796:'连城县',1797:'莲峰镇'},
	'0,1679,1798':{1799:'蕉城区',1800:'福安市',1801:'福鼎市',1802:'寿宁县',1803:'鳌阳镇',1804:'霞浦县',1805:'柘荣县',1806:'双城镇',1807:'屏南县',1808:'古峰镇',1809:'古田县',1810:'周宁县',1811:'狮城镇'},
	'0,1812':{1813:'南昌市',1827:'九江市',1849:'景德镇市',1855:'鹰潭市',1860:'新余市',1864:'萍乡市',1873:'赣州市',1907:'上饶市',1930:'抚州市',1952:'宜春市',1967:'吉安市'},
	'0,1812,1813':{1814:'东湖区',1815:'西湖区',1816:'青云谱区',1817:'湾里区',1818:'青山湖区',1819:'南昌县',1820:'莲塘镇',1821:'新建县',1822:'长堎镇',1823:'安义县',1824:'龙津镇',1825:'进贤县',1826:'民和镇'},
	'0,1812,1827':{1828:'浔阳区',1829:'庐山区',1830:'瑞昌市',1831:'九江县',1832:'沙河街镇',1833:'武宁县',1834:'新宁镇',1835:'修水县',1836:'义宁镇',1837:'永修县',1838:'涂埠镇',1839:'德安县',1840:'蒲亭镇',1841:'星子县',1842:'南康镇',1843:'都昌县',1844:'都昌镇',1845:'湖口县',1846:'双钟镇',1847:'彭泽县',1848:'龙城镇'},
	'0,1812,1849':{1850:'珠山区',1851:'昌江区',1852:'乐平市',1853:'浮梁县',1854:'浮梁镇'},
	'0,1812,1855':{1856:'月湖区',1857:'贵溪市',1858:'余江县',1859:'邓埠镇'},
	'0,1812,1860':{1861:'渝水区',1862:'分宜县',1863:'分宜镇'},
	'0,1812,1864':{1865:'安源区',1866:'湘东区',1867:'莲花县',1868:'琴亭镇',1869:'上栗县',1870:'上栗镇',1871:'芦溪县',1872:'芦溪镇'},
	'0,1812,1873':{1874:'章贡区',1875:'瑞金市',1876:'南康市',1877:'赣县',1878:'梅林镇',1879:'信丰县',1880:'嘉定镇',1881:'大余县',1882:'南安镇',1883:'上犹县',1884:'东山镇',1885:'崇义县',1886:'横水镇',1887:'安远县',1888:'欣山镇',1889:'龙南县',1890:'龙南镇',1891:'定南县',1892:'历市镇',1893:'全南县',1894:'城厢镇',1895:'宁都县',1896:'梅江镇',1897:'于都县',1898:'贡江镇',1899:'兴国县',1900:'潋江镇',1901:'会昌县',1902:'文武坝镇',1903:'寻乌县',1904:'长宁镇',1905:'石城县',1906:'琴江镇'},
	'0,1812,1907':{1908:'信州区',1909:'德兴市',1910:'上饶县',1911:'旭日镇',1912:'广丰县',1913:'永丰镇',1914:'玉山县',1915:'冰溪镇',1916:'铅山县',1917:'河口镇',1918:'横峰县',1919:'岑阳镇',1920:'弋阳县',1921:'弋江镇',1922:'余干县',1923:'玉亭镇',1924:'鄱阳县',1925:'鄱阳镇',1926:'万年县',1927:'陈营镇',1928:'婺源县',1929:'紫阳镇'},
	'0,1812,1930':{1931:'临川区',1932:'南城县',1933:'建昌镇',1934:'黎川县',1935:'日峰镇',1936:'南丰县',1937:'琴城镇',1938:'崇仁县',1939:'巴山镇',1940:'乐安县',1941:'鳌溪镇',1942:'宜黄县',1943:'凤冈镇',1944:'金溪县',1945:'秀谷镇',1946:'资溪县',1947:'鹤城镇',1948:'东乡县',1949:'孝岗镇',1950:'广昌县',1951:'旴江镇'},
	'0,1812,1952':{1953:'袁州区',1954:'丰城市',1955:'樟树市',1956:'高安市',1957:'奉新县',1958:'冯川镇',1959:'万载县',1960:'上高县',1961:'宜丰县',1962:'新昌镇',1963:'靖安县',1964:'双溪镇',1965:'铜鼓县',1966:'永宁镇'},
	'0,1812,1967':{1968:'吉州区',1969:'青原区',1970:'井冈山市',1971:'厦坪镇',1972:'吉安县',1973:'敦厚镇',1974:'吉水县',1975:'文峰镇',1976:'峡江县',1977:'水边镇',1978:'新干县',1979:'金川镇',1980:'永丰县',1981:'恩江镇',1982:'泰和县',1983:'澄江镇',1984:'遂川县',1985:'泉江镇',1986:'万安县',1987:'芙蓉镇',1988:'安福县',1989:'平都镇',1990:'永新县',1991:'禾川镇'},
	'0,1992':{1993:'济南市',2006:'青岛市',2019:'聊城市',2029:'德州市',2047:'东营市',2056:'淄博市',2068:'潍坊市',2081:'烟台市',2095:'威海市',2100:'日照市',2107:'临沂市',2129:'枣庄市',2136:'济宁市',2154:'泰安市',2163:'莱芜市',2166:'滨州市',2179:'菏泽市'},
	'0,1992,1993':{1994:'市中区',1995:'历下区',1996:'槐荫区',1997:'天桥区',1998:'历城区',1999:'长清区',2000:'章丘市',2001:'平阴县',2002:'平阴镇',2003:'济阳县',2004:'济阳镇',2005:'商河县'},
	'0,1992,2006':{2007:'市南区',2008:'市北区',2009:'四方区',2010:'黄岛区',2011:'崂山区',2012:'城阳区',2013:'李沧区',2014:'胶州市',2015:'即墨市',2016:'平度市',2017:'胶南市',2018:'莱西市'},
	'0,1992,2019':{2020:'东昌府区',2021:'临清市',2022:'阳谷县',2023:'莘县',2024:'茌平县',2025:'东阿县',2026:'冠县',2027:'冠城镇',2028:'高唐县'},
	'0,1992,2029':{2030:'德城区',2031:'乐陵市',2032:'禹城市',2033:'陵县',2034:'陵城镇',2035:'平原县',2036:'夏津县',2037:'夏津镇',2038:'武城县',2039:'武城镇',2040:'齐河县',2041:'晏城镇',2042:'临邑县',2043:'宁津县',2044:'宁津镇',2045:'庆云县',2046:'庆云镇'},
	'0,1992,2047':{2048:'东营区',2049:'河口区',2050:'垦利县',2051:'垦利镇',2052:'利津县',2053:'利津镇',2054:'广饶县',2055:'广饶镇'},
	'0,1992,2056':{2057:'张店区',2058:'淄川区',2059:'博山区',2060:'临淄区',2061:'周村区',2062:'桓台县',2063:'索镇',2064:'高青县',2065:'田镇',2066:'沂源县',2067:'南麻镇'},
	'0,1992,2068':{2069:'潍城区',2070:'寒亭区',2071:'坊子区',2072:'奎文区',2073:'安丘市',2074:'昌邑市',2075:'高密市',2076:'青州市',2077:'诸城市',2078:'寿光市',2079:'临朐县',2080:'昌乐县'},
	'0,1992,2081':{2082:'莱山区',2083:'芝罘区',2084:'福山区',2085:'牟平区',2086:'栖霞市',2087:'海阳市',2088:'龙口市',2089:'莱阳市',2090:'莱州市',2091:'蓬莱市',2092:'招远市',2093:'长岛县',2094:'南长山镇'},
	'0,1992,2095':{2096:'环翠区',2097:'荣成市',2098:'乳山市',2099:'文登市'},
	'0,1992,2100':{2101:'东港区',2102:'岚山区',2103:'五莲县',2104:'洪凝镇',2105:'莒县',2106:'城阳镇'},
	'0,1992,2107':{2108:'兰山区',2109:'罗庄区',2110:'河东区',2111:'郯城县',2112:'郯城镇',2113:'苍山县',2114:'卞庄镇',2115:'莒南县',2116:'十字路镇',2117:'沂水县',2118:'沂水镇',2119:'蒙阴县',2120:'蒙阴镇',2121:'平邑县',2122:'平邑镇',2123:'费县',2124:'费城镇',2125:'沂南县',2126:'界湖镇',2127:'临沭县',2128:'临沭镇'},
	'0,1992,2129':{2130:'薛城区',2131:'市中区',2132:'峄城区',2133:'台儿庄区',2134:'山亭区',2135:'滕州市'},
	'0,1992,2136':{2137:'市中区',2138:'任城区',2139:'曲阜市',2140:'兖州市',2141:'邹城市',2142:'微山县',2143:'鱼台县',2144:'谷亭镇',2145:'金乡县',2146:'金乡镇',2147:'嘉祥县',2148:'嘉祥镇',2149:'汶上县',2150:'汶上镇',2151:'泗水县',2152:'梁山县',2153:'梁山镇'},
	'0,1992,2154':{2155:'泰山区',2156:'岱岳区',2157:'新泰市',2158:'肥城市',2159:'宁阳县',2160:'宁阳镇',2161:'东平县',2162:'东平镇'},
	'0,1992,2163':{2164:'莱城区',2165:'钢城区'},
	'0,1992,2166':{2167:'滨城区',2168:'惠民县',2169:'惠民镇',2170:'阳信县',2171:'阳信镇',2172:'无棣县',2173:'无棣镇',2174:'沾化县',2175:'富国镇',2176:'博兴县',2177:'博兴镇',2178:'邹平县'},
	'0,1992,2179':{2180:'牡丹区',2181:'曹县',2182:'曹城镇',2183:'定陶县',2184:'定陶镇',2185:'成武县',2186:'成武镇',2187:'单县',2188:'单城镇',2189:'巨野县',2190:'巨野镇',2191:'郓城县',2192:'郓城镇',2193:'鄄城县',2194:'鄄城镇',2195:'东明县',2196:'城关镇'},
	'0,2197':{2198:'郑州市',2212:'开封市',2228:'三门峡市',2238:'洛阳市',2262:'焦作市',2277:'新乡市',2296:'鹤壁市',2304:'安阳市',2318:'濮阳市',2330:'商丘市',2346:'许昌市',2356:'漯河市',2364:'平顶山市',2379:'南阳市',2400:'信阳市',2417:'周口市',2436:'驻马店市',2455:'济源市'},
	'0,2197,2198':{2199:'中原区',2200:'二七区',2201:'管城回族区',2202:'金水区',2203:'上街区',2204:'惠济区',2205:'新郑市',2206:'登封市',2207:'新密市',2208:'巩义市',2209:'荥阳市',2210:'中牟县',2211:'城关镇'},
	'0,2197,2212':{2213:'鼓楼区',2214:'龙亭区',2215:'顺河回族区',2216:'禹王台区',2217:'金明区',2218:'杞县',2219:'城关镇',2220:'通许县',2221:'城关镇',2222:'尉氏县',2223:'城关镇',2224:'开封县',2225:'城关镇',2226:'兰考县',2227:'城关镇'},
	'0,2197,2228':{2229:'湖滨区',2230:'义马市',2231:'灵宝市',2232:'渑池县',2233:'城关镇',2234:'陕县',2235:'大营镇',2236:'卢氏县',2237:'城关镇'},
	'0,2197,2238':{2239:'西工区',2240:'老城区',2241:'瀍河回族区',2242:'涧西区',2243:'吉利区',2244:'洛龙区',2245:'偃师市',2246:'孟津县',2247:'城关镇',2248:'新安县',2249:'城关镇',2250:'栾川县',2251:'城关镇',2252:'嵩县',2253:'城关镇',2254:'汝阳县',2255:'城关镇',2256:'宜阳县',2257:'城关镇',2258:'洛宁县',2259:'城关镇',2260:'伊川县',2261:'城关镇'},
	'0,2197,2262':{2263:'解放区',2264:'山阳区',2265:'中站区',2266:'马村区',2267:'孟州市',2268:'沁阳市',2269:'修武县',2270:'城关镇',2271:'博爱县',2272:'清化镇',2273:'武陟县',2274:'木城镇',2275:'温县',2276:'温泉镇'},
	'0,2197,2277':{2278:'卫滨区',2279:'红旗区',2280:'凤泉区',2281:'牧野区',2282:'卫辉市',2283:'辉县市',2284:'新乡县',2285:'新乡市红旗区',2286:'获嘉县',2287:'城关镇',2288:'原阳县',2289:'城关镇',2290:'延津县',2291:'城关镇',2292:'封丘县',2293:'城关镇',2294:'长垣县',2295:'城关镇'},
	'0,2197,2296':{2297:'淇滨区',2298:'山城区',2299:'鹤山区',2300:'浚县',2301:'城关镇',2302:'淇县',2303:'朝歌镇'},
	'0,2197,2304':{2305:'北关区',2306:'文峰区',2307:'殷都区',2308:'龙安区',2309:'林州市',2310:'安阳县',2311:'安阳市北关区',2312:'汤阴县',2313:'城关镇',2314:'滑县',2315:'道口镇',2316:'内黄县',2317:'城关镇'},
	'0,2197,2318':{2319:'华龙区',2320:'清丰县',2321:'城关镇',2322:'南乐县',2323:'城关镇',2324:'范县',2325:'城关镇',2326:'台前县',2327:'城关镇',2328:'濮阳县',2329:'城关镇'},
	'0,2197,2330':{2331:'梁园区',2332:'睢阳区',2333:'永城市',2334:'虞城县',2335:'城关镇',2336:'民权县',2337:'城关镇',2338:'宁陵县',2339:'城关镇',2340:'睢县',2341:'城关镇',2342:'夏邑县',2343:'城关镇',2344:'柘城县',2345:'城关镇'},
	'0,2197,2346':{2347:'魏都区',2348:'禹州市',2349:'长葛市',2350:'许昌县',2351:'许昌市魏都区',2352:'鄢陵县',2353:'安陵镇',2354:'襄城县',2355:'城关镇'},
	'0,2197,2356':{2357:'源汇区',2358:'郾城区',2359:'召陵区',2360:'舞阳县',2361:'舞泉镇',2362:'临颍县',2363:'城关镇'},
	'0,2197,2364':{2365:'新华区',2366:'卫东区',2367:'湛河区',2368:'石龙区',2369:'舞钢市',2370:'汝州市',2371:'宝丰县',2372:'城关镇',2373:'叶县',2374:'昆阳镇',2375:'鲁山县',2376:'鲁阳镇',2377:'郏县',2378:'城关镇'},
	'0,2197,2379':{2380:'卧龙区',2381:'宛城区',2382:'邓州市',2383:'南召县',2384:'城关镇',2385:'方城县',2386:'城关镇',2387:'西峡县',2388:'镇平县',2389:'城关镇',2390:'内乡县',2391:'城关镇',2392:'淅川县',2393:'社旗县',2394:'赊店镇',2395:'唐河县',2396:'新野县',2397:'城关镇',2398:'桐柏县',2399:'城关镇'},
	'0,2197,2400':{2401:'河区',2402:'平桥区',2403:'息县',2404:'城关镇',2405:'淮滨县',2406:'城关镇',2407:'潢川县',2408:'光山县',2409:'固始县',2410:'城关镇',2411:'商城县',2412:'城关镇',2413:'罗山县',2414:'城关镇',2415:'新县',2416:'新集镇'},
	'0,2197,2417':{2418:'川汇区',2419:'项城市',2420:'扶沟县',2421:'城关镇',2422:'西华县',2423:'城关镇',2424:'商水县',2425:'城关镇',2426:'太康县',2427:'城关镇',2428:'鹿邑县',2429:'城关镇',2430:'郸城县',2431:'城关镇',2432:'淮阳县',2433:'城关镇',2434:'沈丘县',2435:'槐店镇'},
	'0,2197,2436':{2437:'驿城区',2438:'确山县',2439:'盘龙镇',2440:'泌阳县',2441:'泌水镇',2442:'遂平县',2443:'灈阳镇',2444:'西平县',2445:'上蔡县',2446:'蔡都镇',2447:'汝南县',2448:'汝宁镇',2449:'平舆县',2450:'古槐镇',2451:'新蔡县',2452:'古吕镇',2453:'正阳县',2454:'真阳镇'},
	'0,2456':{2457:'武汉市',2471:'十堰市',2485:'襄樊市',2498:'荆门市',2506:'孝感市',2517:'黄冈市',2535:'鄂州市',2539:'黄石市',2547:'咸宁市',2558:'荆州市',2570:'宜昌市',2589:'随州市',2592:'省直辖县级行政单位',2598:'恩施州'},
	'0,2456,2457':{2458:'江岸区',2459:'江汉区',2460:'硚口区',2461:'汉阳区',2462:'武昌区',2463:'青山区',2464:'洪山区',2465:'东西湖区',2466:'汉南区',2467:'蔡甸区',2468:'江夏区',2469:'黄陂区',2470:'新洲区'},
	'0,2456,2471':{2472:'张湾区',2473:'茅箭区',2474:'丹江口市',2475:'郧县',2476:'城关镇',2477:'竹山县',2478:'城关镇',2479:'房县',2480:'城关镇',2481:'郧西县',2482:'城关镇',2483:'竹溪县',2484:'城关镇'},
	'0,2456,2485':{2486:'襄城区',2487:'樊城区',2488:'襄阳区',2489:'老河口市',2490:'枣阳市',2491:'宜城市',2492:'南漳县',2493:'城关镇',2494:'谷城县',2495:'城关镇',2496:'保康县',2497:'城关镇'},
	'0,2456,2498':{2499:'东宝区',2500:'掇刀区',2501:'钟祥市',2502:'沙洋县',2503:'沙洋镇',2504:'京山县',2505:'新市镇'},
	'0,2456,2506':{2507:'孝南区',2508:'应城市',2509:'安陆市',2510:'汉川市',2511:'孝昌县',2512:'花园镇',2513:'大悟县',2514:'城关镇',2515:'云梦县',2516:'城关镇'},
	'0,2456,2517':{2518:'黄州区',2519:'麻城市',2520:'武穴市',2521:'红安县',2522:'城关镇',2523:'罗田县',2524:'凤山镇',2525:'英山县',2526:'温泉镇',2527:'浠水县',2528:'清泉镇',2529:'蕲春县',2530:'漕河镇',2531:'黄梅县',2532:'黄梅镇',2533:'团风县',2534:'团风镇'},
	'0,2456,2535':{2536:'鄂城区',2537:'梁子湖区',2538:'华容区'},
	'0,2456,2539':{2540:'黄石港区',2541:'西塞山区',2542:'下陆区',2543:'铁山区',2544:'大冶市',2545:'阳新县',2546:'兴国镇'},
	'0,2456,2547':{2548:'咸安区',2549:'赤壁市',2550:'嘉鱼县',2551:'鱼岳镇',2552:'通城县',2553:'隽水镇',2554:'崇阳县',2555:'天城镇',2556:'通山县',2557:'通羊镇'},
	'0,2456,2558':{2559:'沙市区',2560:'荆州区',2561:'石首市',2562:'洪湖市',2563:'松滋市',2564:'江陵县',2565:'郝穴镇',2566:'公安县',2567:'斗湖堤镇',2568:'监利县',2569:'容城镇'},
	'0,2456,2570':{2571:'西陵区',2572:'伍家岗区',2573:'点军区',2574:'猇亭区',2575:'夷陵区',2576:'枝江市',2577:'宜都市',2578:'当阳市',2579:'远安县',2580:'鸣凤镇',2581:'兴山县',2582:'古夫镇',2583:'秭归县',2584:'茅坪镇',2585:'长阳土家族自治县',2586:'龙舟坪镇',2587:'五峰土家族自治县',2588:'五峰镇'},
	'0,2456,2589':{2590:'曾都区',2591:'广水市'},
	'0,2456,2592':{2593:'仙桃市',2594:'天门市',2595:'潜江市',2596:'神农架林区',2597:'松柏镇'},
	'0,2456,2598':{2599:'恩施市',2600:'利川市',2601:'建始县',2602:'业州镇',2603:'巴东县',2604:'信陵镇',2605:'宣恩县',2606:'珠山镇',2607:'咸丰县',2608:'高乐山镇',2609:'来凤县',2610:'翔凤镇',2611:'鹤峰县',2612:'容美镇'},
	'0,2613':{2614:'长沙市',2628:'张家界市',2635:'常德市',2651:'益阳市',2661:'岳阳市',2675:'株洲市',2689:'湘潭市',2696:'衡阳市',2714:'郴州市',2734:'永州市',2755:'邵阳市',2776:'怀化市',2799:'娄底市',2807:'湘西州'},
	'0,2613,2614':{2615:'长沙市',2616:'岳麓区',2617:'芙蓉区',2618:'天心区',2619:'开福区',2620:'雨花区',2621:'浏阳市',2622:'长沙县',2623:'星沙镇',2624:'望城县',2625:'高塘岭镇',2626:'宁乡县',2627:'玉潭镇'},
	'0,2613,2628':{2629:'永定区',2630:'武陵源区',2631:'慈利县',2632:'零阳镇',2633:'桑植县',2634:'澧源镇'},
	'0,2613,2635':{2636:'武陵区',2637:'鼎城区',2638:'津市市',2639:'安乡县',2640:'城关镇',2641:'汉寿县',2642:'龙阳镇',2643:'澧县',2644:'澧阳镇',2645:'临澧县',2646:'安福镇',2647:'桃源县',2648:'漳江镇',2649:'石门县',2650:'楚江镇'},
	'0,2613,2651':{2652:'赫山区',2653:'资阳区',2654:'沅江市',2655:'南县',2656:'南洲镇',2657:'桃江县',2658:'桃花江镇',2659:'安化县',2660:'东坪镇'},
	'0,2613,2661':{2662:'岳阳楼区',2663:'君山区',2664:'云溪区',2665:'汨罗市',2666:'临湘市',2667:'岳阳县',2668:'荣家湾镇',2669:'华容县',2670:'城关镇',2671:'湘阴县',2672:'文星镇',2673:'平江县',2674:'汉昌镇'},
	'0,2613,2675':{2676:'天元区',2677:'荷塘区',2678:'芦淞区',2679:'石峰区',2680:'醴陵市',2681:'株洲县',2682:'渌口镇',2683:'攸县',2684:'城关镇',2685:'茶陵县',2686:'城关镇',2687:'炎陵县',2688:'霞阳镇'},
	'0,2613,2689':{2690:'岳塘区',2691:'雨湖区',2692:'湘乡市',2693:'韶山市',2694:'湘潭县',2695:'易俗河镇'},
	'0,2613,2696':{2697:'雁峰区',2698:'珠晖区',2699:'石鼓区',2700:'蒸湘区',2701:'南岳区',2702:'常宁市',2703:'耒阳市',2704:'衡阳县',2705:'西渡镇',2706:'衡南县',2707:'云集镇',2708:'衡山县',2709:'开云镇',2710:'衡东县',2711:'城关镇',2712:'祁东县',2713:'洪桥镇'},
	'0,2613,2714':{2715:'北湖区',2716:'苏仙区',2717:'资兴市',2718:'桂阳县',2719:'城关镇',2720:'永兴县',2721:'城关镇',2722:'宜章县',2723:'城关镇',2724:'嘉禾县',2725:'城关镇',2726:'临武县',2727:'城关镇',2728:'汝城县',2729:'城关镇',2730:'桂东县',2731:'城关镇',2732:'安仁县',2733:'城关镇'},
	'0,2613,2734':{2735:'冷水滩区',2736:'零陵区',2737:'东安县',2738:'白牙市镇',2739:'道县',2740:'道江镇',2741:'宁远县',2742:'舜陵镇',2743:'江永县',2744:'潇浦镇',2745:'蓝山县',2746:'塔峰镇',2747:'新田县',2748:'龙泉镇',2749:'双牌县',2750:'泷泊镇',2751:'祁阳县',2752:'浯溪镇',2753:'江华瑶族自治县',2754:'沱江镇'},
	'0,2613,2755':{2756:'双清区',2757:'大祥区',2758:'北塔区',2759:'武冈市',2760:'邵东县',2761:'两市镇',2762:'邵阳县',2763:'塘渡口镇',2764:'新邵县',2765:'酿溪镇',2766:'隆回县',2767:'桃洪镇',2768:'洞口县',2769:'洞口镇',2770:'绥宁县',2771:'长铺镇',2772:'新宁县',2773:'金石镇',2774:'城步苗族自治县',2775:'儒林镇'},
	'0,2613,2776':{2777:'鹤城区',2778:'洪江市',2779:'沅陵县',2780:'沅陵镇',2781:'辰溪县',2782:'辰阳镇',2783:'溆浦县',2784:'卢峰镇',2785:'中方县',2786:'中方镇',2787:'会同县',2788:'林城镇',2789:'麻阳苗族自治县',2790:'高村镇',2791:'新晃侗族自治县',2792:'新晃镇',2793:'芷江侗族自治县',2794:'芷江镇',2795:'靖州苗族侗族自治县',2796:'渠阳镇',2797:'通道侗族自治县',2798:'双江镇'},
	'0,2613,2799':{2800:'娄星区',2801:'冷水江市',2802:'涟源市',2803:'双峰县',2804:'永丰镇',2805:'新化县',2806:'上梅镇'},
	'0,2613,2807':{2808:'吉首市',2809:'泸溪县',2810:'白沙镇',2811:'凤凰县',2812:'沱江镇',2813:'花垣县',2814:'花垣镇',2815:'保靖县',2816:'迁陵镇',2817:'古丈县',2818:'古阳镇',2819:'永顺县',2820:'灵溪镇',2821:'龙山县'},
	'0,2822':{2823:'广州市',2836:'深圳市',2843:'清远市',2857:'韶关市',2872:'河源市',2884:'梅州市',2899:'潮州市',2905:'汕头市',2914:'揭阳市',2923:'汕尾市',2930:'惠州市',2937:'东莞市',2938:'深圳市',2945:'珠海市',2949:'中山市',2950:'江门市',2958:'佛山市',2964:'肇庆市',2976:'云浮市',2985:'阳江市',2992:'茂名市',3000:'湛江市'},
	'0,2822,2823':{2824:'越秀区',2825:'荔湾区',2826:'海珠区',2827:'天河区',2828:'白云区',2829:'黄埔区',2830:'番禺区',2831:'花都区',2832:'南沙区',2833:'萝岗区',2834:'增城市',2835:'从化市'},
	'0,2822,2836':{2837:'福田区',2838:'罗湖区',2839:'南山区',2840:'宝安区',2841:'龙岗区',2842:'盐田区'},
	'0,2822,2843':{2844:'清城区',2845:'英德市',2846:'连州市',2847:'佛冈县',2848:'石角镇',2849:'阳山县',2850:'阳城镇',2851:'清新县',2852:'太和镇',2853:'连山壮族瑶族自治县',2854:'吉田镇',2855:'连南瑶族自治县',2856:'三江镇'},
	'0,2822,2857':{2858:'浈江区',2859:'武江区',2860:'曲江区',2861:'乐昌市',2862:'南雄市',2863:'始兴县',2864:'太平镇',2865:'仁化县',2866:'仁化镇',2867:'翁源县',2868:'龙仙镇',2869:'新丰县',2870:'乳源瑶族自治县',2871:'乳城镇'},
	'0,2822,2872':{2873:'源城区',2874:'紫金县',2875:'紫城镇',2876:'龙川县',2877:'老隆镇',2878:'连平县',2879:'元善镇',2880:'和平县',2881:'阳明镇',2882:'东源县',2883:'仙塘镇'},
	'0,2822,2884':{2885:'梅江区',2886:'兴宁市',2887:'梅县',2888:'程江镇',2889:'大埔县',2890:'湖寮镇',2891:'丰顺县',2892:'汤坑镇',2893:'五华县',2894:'水寨镇',2895:'平远县',2896:'大柘镇',2897:'蕉岭县',2898:'蕉城镇'},
	'0,2822,2899':{2900:'湘桥区',2901:'潮安县',2902:'庵埠镇',2903:'饶平县',2904:'黄冈镇'},
	'0,2822,2905':{2906:'金平区',2907:'濠江区',2908:'龙湖区',2909:'潮阳区',2910:'潮南区',2911:'澄海区',2912:'南澳县',2913:'后宅镇'},
	'0,2822,2914':{2915:'榕城区',2916:'普宁市',2917:'揭东县',2918:'曲溪镇',2919:'揭西县',2920:'河婆镇',2921:'惠来县',2922:'惠城镇'},
	'0,2822,2923':{2924:'城区',2925:'陆丰市',2926:'海丰县',2927:'海城镇',2928:'陆河县',2929:'河田镇'},
	'0,2822,2930':{2931:'惠城区',2932:'惠阳区',2933:'博罗县',2934:'罗阳镇',2935:'惠东县',2936:'龙门县'},
	'0,2822,2938':{2939:'福田区',2940:'罗湖区',2941:'南山区',2942:'宝安区',2943:'龙岗区',2944:'盐田区'},
	'0,2822,2945':{2946:'香洲区',2947:'斗门区',2948:'金湾区'},
	'0,2822,2950':{2951:'江海区',2952:'蓬江区',2953:'新会区',2954:'恩平市',2955:'台山市',2956:'开平市',2957:'鹤山市'},
	'0,2822,2958':{2959:'禅城区',2960:'南海区',2961:'顺德区',2962:'三水区',2963:'高明区'},
	'0,2822,2964':{2965:'端州区',2966:'鼎湖区',2967:'高要市',2968:'四会市',2969:'广宁县',2970:'南街镇',2971:'怀集县',2972:'怀城镇',2973:'封开县',2974:'江口镇',2975:'德庆县'},
	'0,2822,2976':{2977:'云城区',2978:'罗定市',2979:'云安县',2980:'六都镇',2981:'新兴县',2982:'新城镇',2983:'郁南县',2984:'都城镇'},
	'0,2822,2985':{2986:'江城区',2987:'阳春市',2988:'阳西县',2989:'织镇',2990:'阳东县',2991:'东城镇'},
	'0,2822,2992':{2993:'茂南区',2994:'茂港区',2995:'化州市',2996:'信宜市',2997:'高州市',2998:'电白县',2999:'水东镇'},
	'0,2822,3000':{3001:'赤坎区',3002:'霞山区',3003:'坡头区',3004:'麻章区',3005:'吴川市',3006:'廉江市',3007:'雷州市',3008:'遂溪县',3009:'遂城镇',3010:'徐闻县',3011:'撤销广州市东山区',3012:'芳村区',3013:'设立广州市南沙区',3014:'萝岗区'},
	'0,3015':{3016:'南宁市',3029:'桂林市',3059:'柳州市',3076:'梧州市',3087:'贵港市',3094:'玉林市',3105:'钦州市',3112:'北海市',3118:'防城港市',3124:'崇左市',3137:'百色市',3161:'河池市',3182:'来宾市',3193:'贺州市'},
	'0,3015,3016':{3017:'青秀区',3018:'兴宁区',3019:'江南区',3020:'西乡塘区',3021:'良庆区',3022:'邕宁区',3023:'武鸣县',3024:'横县',3025:'宾阳县',3026:'上林县',3027:'隆安县',3028:'马山县'},
	'0,3015,3029':{3030:'象山区',3031:'叠彩区',3032:'秀峰区',3033:'七星区',3034:'雁山区',3035:'阳朔县',3036:'阳朔镇',3037:'临桂县',3038:'临桂镇',3039:'灵川县',3040:'灵川镇',3041:'全州县',3042:'全州镇',3043:'兴安县',3044:'兴安镇',3045:'永福县',3046:'永福镇',3047:'灌阳县',3048:'灌阳镇',3049:'资源县',3050:'资源镇',3051:'平乐县',3052:'平乐镇',3053:'荔浦县',3054:'荔城镇',3055:'龙胜各族自治县',3056:'龙胜镇',3057:'恭城瑶族自治县',3058:'恭城镇'},
	'0,3015,3059':{3060:'城中区',3061:'鱼峰区',3062:'柳南区',3063:'柳北区',3064:'柳江县',3065:'拉堡镇',3066:'柳城县',3067:'大埔镇',3068:'鹿寨县',3069:'鹿寨镇',3070:'融安县',3071:'长安镇',3072:'三江侗族自治县',3073:'古宜镇',3074:'融水苗族自治县',3075:'融水镇'},
	'0,3015,3076':{3077:'万秀区',3078:'蝶山区',3079:'长洲区',3080:'岑溪市',3081:'苍梧县',3082:'龙圩镇',3083:'藤县',3084:'藤州镇',3085:'蒙山县',3086:'蒙山镇'},
	'0,3015,3087':{3088:'港北区',3089:'港南区',3090:'覃塘区',3091:'桂平市',3092:'平南县',3093:'平南镇'},
	'0,3015,3094':{3095:'玉州区',3096:'北流市',3097:'兴业县',3098:'石南镇',3099:'容县',3100:'容州镇',3101:'陆川县',3102:'陆城镇',3103:'博白县',3104:'博白镇'},
	'0,3015,3105':{3106:'钦南区',3107:'钦北区',3108:'灵山县',3109:'灵城镇',3110:'浦北县',3111:'小江镇'},
	'0,3015,3112':{3113:'海城区',3114:'银海区',3115:'铁山港区',3116:'合浦县',3117:'廉州镇'},
	'0,3015,3118':{3119:'港口区',3120:'防城区',3121:'东兴市',3122:'上思县',3123:'思阳镇'},
	'0,3015,3124':{3125:'江州区',3126:'凭祥市',3127:'扶绥县',3128:'新宁镇',3129:'大新县',3130:'桃城镇',3131:'天等县',3132:'天等镇',3133:'宁明县',3134:'城中镇',3135:'龙州县',3136:'龙州镇'},
	'0,3015,3137':{3138:'右江区',3139:'田阳县',3140:'田州镇',3141:'田东县',3142:'平马镇',3143:'平果县',3144:'马头镇',3145:'德保县',3146:'城关镇',3147:'靖西县',3148:'新靖镇',3149:'那坡县',3150:'城厢镇',3151:'凌云县',3152:'泗城镇',3153:'乐业县',3154:'同乐镇',3155:'西林县',3156:'八达镇',3157:'田林县',3158:'乐里镇',3159:'隆林各族自治县',3160:'新州镇'},
	'0,3015,3161':{3162:'金城江区',3163:'宜州市',3164:'南丹县',3165:'城关镇',3166:'天峨县',3167:'六排镇',3168:'凤山县',3169:'凤城镇',3170:'东兰县',3171:'东兰镇',3172:'巴马瑶族自治县',3173:'巴马镇',3174:'都安瑶族自治县',3175:'安阳镇',3176:'大化瑶族自治县',3177:'大化镇',3178:'罗城仫佬族自治县',3179:'东门镇',3180:'环江毛南族自治县',3181:'思恩镇'},
	'0,3015,3182':{3183:'兴宾区',3184:'合山市',3185:'象州县',3186:'象州镇',3187:'武宣县',3188:'武宣镇',3189:'忻城县',3190:'城关镇',3191:'金秀瑶族自治县',3192:'金秀镇'},
	'0,3015,3193':{3194:'八步区',3195:'昭平县',3196:'昭平镇',3197:'钟山县',3198:'钟山镇',3199:'富川瑶族自治县',3200:'富阳镇'},
	'0,3201':{3202:'海口市',3207:'三亚市',3208:'省直辖行政单位'},
	'0,3201,3202':{3203:'龙华区',3204:'秀英区',3205:'琼山区',3206:'美兰区'},
	'0,3201,3208':{3209:'文昌市',3210:'琼海市',3211:'万宁市',3212:'五指山市',3213:'东方市',3214:'儋州市',3215:'临高县',3216:'临城镇',3217:'澄迈县',3218:'金江镇',3219:'定安县',3220:'定城镇',3221:'屯昌县',3222:'屯城镇',3223:'昌江黎族自治县',3224:'石碌镇',3225:'白沙黎族自治县',3226:'牙叉镇',3227:'琼中黎族苗族自治县',3228:'营根镇',3229:'陵水黎族自治县',3230:'椰林镇',3231:'保亭黎族苗族自治县',3232:'保城镇',3233:'乐东黎族自治县',3234:'抱由镇'},
	'0,3235':{3236:'成都市',3261:'广元市',3273:'绵阳市',3289:'德阳市',3298:'南充市',3313:'广安市',3322:'遂宁市',3331:'内江市',3340:'乐山市',3358:'自贡市',3367:'泸州市',3379:'宜宾市',3399:'攀枝花市',3407:'巴中市',3415:'达州市',3428:'资阳市',3435:'眉山市',3447:'雅安市',3463:'阿坝州',3490:'甘孜州',3527:'凉山州'},
	'0,3235,3236':{3237:'青羊区',3238:'锦江区',3239:'金牛区',3240:'武侯区',3241:'成华区',3242:'龙泉驿区',3243:'青白江区',3244:'新都区',3245:'温江区',3246:'都江堰市',3247:'彭州市',3248:'邛崃市',3249:'崇州市',3250:'金堂县',3251:'赵镇',3252:'双流县',3253:'郫县',3254:'郫筒镇',3255:'大邑县',3256:'晋原镇',3257:'蒲江县',3258:'鹤山镇',3259:'新津县',3260:'五津镇'},
	'0,3235,3261':{3262:'市中区',3263:'元坝区',3264:'朝天区',3265:'旺苍县',3266:'东河镇',3267:'青川县',3268:'乔庄镇',3269:'剑阁县',3270:'下寺镇',3271:'苍溪县',3272:'陵江镇'},
	'0,3235,3273':{3274:'涪城区',3275:'游仙区',3276:'江油市',3277:'三台县',3278:'潼川镇',3279:'盐亭县',3280:'云溪镇',3281:'安县',3282:'花荄镇',3283:'梓潼县',3284:'文昌镇',3285:'北川羌族自治县',3286:'曲山镇',3287:'平武县',3288:'龙安镇'},
	'0,3235,3289':{3290:'旌阳区',3291:'什邡市',3292:'广汉市',3293:'绵竹市',3294:'罗江县',3295:'罗江镇',3296:'中江县',3297:'凯江镇'},
	'0,3235,3298':{3299:'顺庆区',3300:'高坪区',3301:'嘉陵区',3302:'阆中市',3303:'南部县',3304:'南隆镇',3305:'营山县',3306:'朗池镇',3307:'蓬安县',3308:'周口镇',3309:'仪陇县',3310:'新政镇',3311:'西充县',3312:'晋城镇'},
	'0,3235,3313':{3314:'广安区',3315:'华蓥市',3316:'岳池县',3317:'九龙镇',3318:'武胜县',3319:'沿口镇',3320:'邻水县',3321:'鼎屏镇'},
	'0,3235,3322':{3323:'船山区',3324:'安居区',3325:'蓬溪县',3326:'赤城镇',3327:'射洪县',3328:'太和镇',3329:'大英县',3330:'蓬莱镇'},
	'0,3235,3331':{3332:'市中区',3333:'东兴区',3334:'威远县',3335:'严陵镇',3336:'资中县',3337:'重龙镇',3338:'隆昌县',3339:'金鹅镇'},
	'0,3235,3340':{3341:'市中区',3342:'沙湾区',3343:'五通桥区',3344:'金口河区',3345:'峨眉山市',3346:'犍为县',3347:'玉津镇',3348:'井研县',3349:'研城镇',3350:'夹江县',3351:'漹城镇',3352:'沐川县',3353:'沐溪镇',3354:'峨边彝族自治县',3355:'沙坪镇',3356:'马边彝族自治县',3357:'民建镇'},
	'0,3235,3358':{3359:'自流井区',3360:'大安区',3361:'贡井区',3362:'沿滩区',3363:'荣县',3364:'旭阳镇',3365:'富顺县',3366:'富世镇'},
	'0,3235,3367':{3368:'江阳区',3369:'纳溪区',3370:'龙马潭区',3371:'泸县',3372:'福集镇',3373:'合江县',3374:'合江镇',3375:'叙永县',3376:'叙永镇',3377:'古蔺县',3378:'古蔺镇'},
	'0,3235,3379':{3380:'翠屏区',3381:'宜宾县',3382:'柏溪镇',3383:'南溪县',3384:'南溪镇',3385:'江安县',3386:'江安镇',3387:'长宁县',3388:'长宁镇',3389:'高县',3390:'庆符镇',3391:'筠连县',3392:'筠连镇',3393:'珙县',3394:'巡场镇',3395:'兴文县',3396:'中城镇',3397:'屏山县',3398:'屏山镇'},
	'0,3235,3399':{3400:'东区',3401:'西区',3402:'仁和区',3403:'米易县',3404:'攀莲镇',3405:'盐边县',3406:'桐子林镇'},
	'0,3235,3407':{3408:'巴州区',3409:'通江县',3410:'诺江镇',3411:'南江县',3412:'南江镇',3413:'平昌县',3414:'江口镇'},
	'0,3235,3415':{3416:'通川区',3417:'万源市',3418:'达县',3419:'南外镇',3420:'宣汉县',3421:'东乡镇',3422:'开江县',3423:'新宁镇',3424:'大竹县',3425:'竹阳镇',3426:'渠县',3427:'渠江镇'},
	'0,3235,3428':{3429:'雁江区',3430:'简阳市',3431:'乐至县',3432:'天池镇',3433:'安岳县',3434:'岳阳镇'},
	'0,3235,3435':{3436:'东坡区',3437:'仁寿县',3438:'文林镇',3439:'彭山县',3440:'凤鸣镇',3441:'洪雅县',3442:'洪川镇',3443:'丹棱县',3444:'丹棱镇',3445:'青神县',3446:'城厢镇'},
	'0,3235,3447':{3448:'雨城区',3449:'名山县',3450:'蒙阳镇',3451:'荥经县',3452:'严道镇',3453:'汉源县',3454:'富林镇',3455:'石棉县',3456:'新棉镇',3457:'天全县',3458:'城厢镇',3459:'芦山县',3460:'芦阳镇',3461:'宝兴县',3462:'穆坪镇'},
	'0,3235,3463':{3464:'马尔康县',3465:'马尔康镇',3466:'汶川县',3467:'威州镇',3468:'理县',3469:'杂谷脑镇',3470:'茂县',3471:'凤仪镇',3472:'松潘县',3473:'进安镇',3474:'九寨沟县',3475:'永乐镇',3476:'金川县',3477:'金川镇',3478:'小金县',3479:'美兴镇',3480:'黑水县',3481:'芦花镇',3482:'壤塘县',3483:'壤柯镇',3484:'阿坝县',3485:'阿坝镇',3486:'若尔盖县',3487:'达扎寺镇',3488:'红原县',3489:'邛溪镇'},
	'0,3235,3490':{3491:'康定县',3492:'炉城镇',3493:'泸定县',3494:'泸桥镇',3495:'丹巴县',3496:'章谷镇',3497:'九龙县',3498:'呷尔镇',3499:'雅江县',3500:'河口镇',3501:'道孚县',3502:'鲜水镇',3503:'炉霍县',3504:'新都镇',3505:'甘孜县',3506:'甘孜镇',3507:'新龙县',3508:'茹龙镇',3509:'德格县',3510:'更庆镇',3511:'白玉县',3512:'建设镇',3513:'石渠县',3514:'尼呷镇',3515:'色达县',3516:'色柯镇',3517:'理塘县',3518:'高城镇',3519:'巴塘县',3520:'夏邛镇',3521:'乡城县',3522:'桑披镇',3523:'稻城县',3524:'金珠镇',3525:'得荣县',3526:'松麦镇'},
	'0,3235,3527':{3528:'西昌市',3529:'盐源县',3530:'盐井镇',3531:'德昌县',3532:'德州镇',3533:'会理县',3534:'城关镇',3535:'会东县',3536:'会东镇',3537:'宁南县',3538:'披砂镇',3539:'普格县',3540:'普基镇',3541:'布拖县',3542:'特木里镇',3543:'金阳县',3544:'天地坝镇',3545:'昭觉县',3546:'新城镇',3547:'喜德县',3548:'光明镇',3549:'冕宁县',3550:'城厢镇',3551:'越西县',3552:'越城镇',3553:'甘洛县',3554:'新市坝镇',3555:'美姑县',3556:'巴普镇',3557:'雷波县',3558:'锦城镇',3559:'木里藏族自治县',3560:'乔瓦镇'},
	'0,3561':{3562:'贵阳市',3576:'六盘水市',3583:'遵义市',3608:'安顺市',3620:'毕节地区',3636:'铜仁地区',3656:'黔东南州',3688:'黔南州',3711:'黔西南州'},
	'0,3561,3562':{3563:'乌当区',3564:'南明区',3565:'云岩区',3566:'花溪区',3567:'白云区',3568:'小河区',3569:'清镇市',3570:'开阳县',3571:'城关镇',3572:'修文县',3573:'龙场镇',3574:'息烽县',3575:'永靖镇'},
	'0,3561,3576':{3577:'钟山区',3578:'盘县',3579:'红果镇',3580:'六枝特区',3581:'平寨镇',3582:'水城县'},
	'0,3561,3583':{3584:'红花岗区',3585:'汇川区',3586:'赤水市',3587:'仁怀市',3588:'遵义县',3589:'南白镇',3590:'桐梓县',3591:'娄山关镇',3592:'绥阳县',3593:'洋川镇',3594:'正安县',3595:'凤仪镇',3596:'凤冈县',3597:'龙泉镇',3598:'湄潭县',3599:'湄江镇',3600:'余庆县',3601:'白泥镇',3602:'习水县',3603:'东皇镇',3604:'道真仡佬族苗族自治县',3605:'玉溪镇',3606:'务川仡佬族苗族自治县',3607:'都濡镇'},
	'0,3561,3608':{3609:'西秀区',3610:'平坝县',3611:'城关镇',3612:'普定县',3613:'城关镇',3614:'关岭布依族苗族自治县',3615:'关索镇',3616:'镇宁布依族苗族自治县',3617:'城关镇',3618:'紫云苗族布依族自治县',3619:'松山镇'},
	'0,3561,3620':{3621:'毕节市',3622:'大方县',3623:'大方镇',3624:'黔西县',3625:'城关镇',3626:'金沙县',3627:'城关镇',3628:'织金县',3629:'城关镇',3630:'纳雍县',3631:'雍熙镇',3632:'赫章县',3633:'城关镇',3634:'威宁彝族回族苗族自治县',3635:'草海镇'},
	'0,3561,3636':{3637:'铜仁市',3638:'江口县',3639:'双江镇',3640:'石阡县',3641:'汤山镇',3642:'思南县',3643:'思唐镇',3644:'德江县',3645:'青龙镇',3646:'玉屏侗族自治县',3647:'平溪镇',3648:'印江土家族苗族自治县',3649:'峨岭镇',3650:'沿河土家族自治县',3651:'和平镇',3652:'松桃苗族自治县',3653:'蓼皋镇',3654:'万山特区',3655:'万山镇'},
	'0,3561,3656':{3657:'凯里市',3658:'黄平县',3659:'新州镇',3660:'施秉县',3661:'城关镇',3662:'三穗县',3663:'八弓镇',3664:'镇远县',3665:'阳镇',3666:'岑巩县',3667:'思旸镇',3668:'天柱县',3669:'凤城镇',3670:'锦屏县',3671:'三江镇',3672:'剑河县',3673:'革东镇',3674:'台江县',3675:'台拱镇',3676:'黎平县',3677:'德凤镇',3678:'榕江县',3679:'古州镇',3680:'从江县',3681:'丙妹镇',3682:'雷山县',3683:'丹江镇',3684:'麻江县',3685:'杏山镇',3686:'丹寨县',3687:'龙泉镇'},
	'0,3561,3688':{3689:'都匀市',3690:'福泉市',3691:'荔波县',3692:'玉屏镇',3693:'贵定县',3694:'城关镇',3695:'瓮安县',3696:'雍阳镇',3697:'独山县',3698:'城关镇',3699:'平塘县',3700:'平湖镇',3701:'罗甸县',3702:'龙坪镇',3703:'长顺县',3704:'长寨镇',3705:'龙里县',3706:'龙山镇',3707:'惠水县',3708:'和平镇',3709:'三都水族自治县',3710:'三合镇'},
	'0,3561,3711':{3712:'兴义市',3713:'兴仁县',3714:'城关镇',3715:'普安县',3716:'盘水镇',3717:'晴隆县',3718:'莲城镇',3719:'贞丰县',3720:'珉谷镇',3721:'望谟县',3722:'复兴镇',3723:'册亨县',3724:'者楼镇',3725:'安龙县',3726:'新安镇',3727:'剑河县人民政府驻地由柳川镇迁至革东镇'},
	'0,3728':{3729:'昆明市',3752:'曲靖市',3769:'玉溪市',3787:'保山市',3797:'昭通市',3819:'丽江市',3829:'思茅市',3849:'临沧市',3865:'德宏州',3874:'怒江州',3884:'迪庆州',3891:'大理州',3915:'楚雄州',3935:'红河州',3960:'文山州',3977:'西双版纳州'},
	'0,3728,3729':{3730:'盘龙区',3731:'五华区',3732:'官渡区',3733:'西山区',3734:'东川区',3735:'安宁市',3736:'呈贡县',3737:'龙城镇',3738:'晋宁县',3739:'昆阳镇',3740:'富民县',3741:'永定镇',3742:'宜良县',3743:'匡远镇',3744:'嵩明县',3745:'嵩阳镇',3746:'石林彝族自治县',3747:'鹿阜镇',3748:'禄劝彝族苗族自治县',3749:'屏山镇',3750:'寻甸回族彝族自治县',3751:'仁德镇'},
	'0,3728,3752':{3753:'麒麟区',3754:'宣威市',3755:'马龙县',3756:'通泉镇',3757:'沾益县',3758:'西平镇',3759:'富源县',3760:'中安镇',3761:'罗平县',3762:'罗雄镇',3763:'师宗县',3764:'丹凤镇',3765:'陆良县',3766:'中枢镇',3767:'会泽县',3768:'金钟镇'},
	'0,3728,3769':{3770:'红塔区',3771:'江川县',3772:'大街镇',3773:'澄江县',3774:'凤麓镇',3775:'通海县',3776:'秀山镇',3777:'华宁县',3778:'宁州镇',3779:'易门县',3780:'龙泉镇',3781:'峨山彝族自治县',3782:'双江镇',3783:'新平彝族傣族自治县',3784:'桂山镇',3785:'元江哈尼族彝族傣族自治县',3786:'澧江镇'},
	'0,3728,3787':{3788:'隆阳区',3789:'施甸县',3790:'甸阳镇',3791:'腾冲县',3792:'腾越镇',3793:'龙陵县',3794:'龙山镇',3795:'昌宁县',3796:'田园镇'},
	'0,3728,3797':{3798:'昭阳区',3799:'鲁甸县',3800:'文屏镇',3801:'巧家县',3802:'新华镇',3803:'盐津县',3804:'盐井镇',3805:'大关县',3806:'翠华镇',3807:'永善县',3808:'溪落渡镇',3809:'绥江县',3810:'中城镇',3811:'镇雄县',3812:'乌峰镇',3813:'彝良县',3814:'角奎镇',3815:'威信县',3816:'扎西镇',3817:'水富县',3818:'向家坝镇'},
	'0,3728,3819':{3820:'古城区',3821:'永胜县',3822:'永北镇',3823:'华坪县',3824:'中心镇',3825:'玉龙纳西族自治县',3826:'黄山镇',3827:'宁蒗彝族自治县',3828:'大兴镇'},
	'0,3728,3829':{3830:'翠云区',3831:'普洱哈尼族彝族自治县',3832:'宁洱镇',3833:'墨江哈尼族自治县',3834:'联珠镇',3835:'景东彝族自治县',3836:'锦屏镇',3837:'景谷傣族彝族自治县',3838:'威远镇',3839:'镇沅彝族哈尼族拉祜族自治县',3840:'恩乐镇',3841:'江城哈尼族彝族自治县',3842:'勐烈镇',3843:'孟连傣族拉祜族佤族自治县',3844:'娜允镇',3845:'澜沧拉祜族自治县',3846:'勐朗镇',3847:'西盟佤族自治县',3848:'勐梭镇'},
	'0,3728,3849':{3850:'临翔区',3851:'凤庆县',3852:'凤山镇',3853:'云县',3854:'爱华镇',3855:'永德县',3856:'德党镇',3857:'镇康县',3858:'南伞镇',3859:'双江拉祜族佤族布朗族傣族自治县',3860:'勐勐镇',3861:'耿马傣族佤族自治县',3862:'耿马镇',3863:'沧源佤族自治县',3864:'勐董镇'},
	'0,3728,3865':{3866:'潞西市',3867:'瑞丽市',3868:'梁河县',3869:'遮岛镇',3870:'盈江县',3871:'平原镇',3872:'陇川县',3873:'章凤镇'},
	'0,3728,3874':{3875:'泸水县六库镇',3876:'泸水县',3877:'六库镇',3878:'福贡县',3879:'上帕镇',3880:'贡山独龙族怒族自治县',3881:'茨开镇',3882:'兰坪白族普米族自治县',3883:'金顶镇'},
	'0,3728,3884':{3885:'香格里拉县',3886:'建塘镇',3887:'德钦县',3888:'升平镇',3889:'维西傈僳族自治县',3890:'保和镇'},
	'0,3728,3891':{3892:'大理市',3893:'祥云县',3894:'祥城镇',3895:'宾川县',3896:'金牛镇',3897:'弥渡县',3898:'弥城镇',3899:'永平县',3900:'博南镇',3901:'云龙县',3902:'诺邓镇',3903:'洱源县',3904:'茈碧湖镇',3905:'剑川县',3906:'金华镇',3907:'鹤庆县',3908:'云鹤镇',3909:'漾濞彝族自治县',3910:'苍山西镇',3911:'南涧彝族自治县',3912:'南涧镇',3913:'巍山彝族回族自治县',3914:'南诏镇'},
	'0,3728,3915':{3916:'楚雄市',3917:'双柏县',3918:'妥甸镇',3919:'牟定县',3920:'共和镇',3921:'南华县',3922:'龙川镇',3923:'姚安县',3924:'栋川镇',3925:'大姚县',3926:'金碧镇',3927:'永仁县',3928:'永定镇',3929:'元谋县',3930:'元马镇',3931:'武定县',3932:'狮山镇',3933:'禄丰县',3934:'金山镇'},
	'0,3728,3935':{3936:'蒙自县',3937:'文澜镇',3938:'个旧市',3939:'开远市',3940:'绿春县',3941:'大兴镇',3942:'建水县',3943:'临安镇',3944:'石屏县',3945:'异龙镇',3946:'弥勒县',3947:'弥阳镇',3948:'泸西县',3949:'中枢镇',3950:'元阳县',3951:'南沙镇',3952:'红河县',3953:'迤萨镇',3954:'金平苗族瑶族傣族自治县',3955:'金河镇',3956:'河口瑶族自治县',3957:'河口镇',3958:'屏边苗族自治县',3959:'玉屏镇'},
	'0,3728,3960':{3961:'文山县',3962:'开化镇',3963:'砚山县',3964:'江那镇',3965:'西畴县',3966:'西洒镇',3967:'麻栗坡县',3968:'麻栗镇',3969:'马关县',3970:'马白镇',3971:'丘北县',3972:'锦屏镇',3973:'广南县',3974:'莲城镇',3975:'富宁县',3976:'新华镇'},
	'0,3728,3977':{3978:'景洪市',3979:'勐海县',3980:'勐海镇',3981:'勐腊县',3982:'勐腊镇'},
	'0,3983':{3984:'拉萨市',4000:'那曲地区',4021:'昌都地区',4044:'林芝地区',4059:'山南地区',4084:'日喀则地区',4120:'阿里地区'},
	'0,3983,3984':{3985:'城关区',3986:'林周县',3987:'甘丹曲果镇',3988:'当雄县',3989:'当曲卡镇',3990:'尼木县',3991:'塔荣镇',3992:'曲水县',3993:'曲水镇',3994:'堆龙德庆县',3995:'东嘎镇',3996:'达孜县',3997:'德庆镇',3998:'墨竹工卡县',3999:'工卡镇'},
	'0,3983,4000':{4001:'那曲县',4002:'那曲镇',4003:'嘉黎县',4004:'阿扎镇',4005:'比如县',4006:'比如镇',4007:'聂荣县',4008:'聂荣镇',4009:'安多县',4010:'帕那镇',4011:'申扎县',4012:'申扎镇',4013:'索县',4014:'亚拉镇',4015:'班戈县',4016:'普保镇',4017:'巴青县',4018:'拉西镇',4019:'尼玛县',4020:'尼玛镇'},
	'0,3983,4021':{4022:'昌都县',4023:'城关镇',4024:'江达县',4025:'江达镇',4026:'贡觉县',4027:'莫洛镇',4028:'类乌齐县',4029:'桑多镇',4030:'丁青县',4031:'丁青镇',4032:'察雅县',4033:'烟多镇',4034:'八宿县',4035:'白玛镇',4036:'左贡县',4037:'旺达镇',4038:'芒康县',4039:'嘎托镇',4040:'洛隆县',4041:'孜托镇',4042:'边坝县',4043:'草卡镇'},
	'0,3983,4044':{4045:'林芝县',4046:'八一镇',4047:'工布江达县',4048:'工布江达镇',4049:'米林县',4050:'米林镇',4051:'墨脱县',4052:'墨脱镇',4053:'波密县',4054:'扎木镇',4055:'察隅县',4056:'竹瓦根镇',4057:'朗县',4058:'朗镇'},
	'0,3983,4059':{4060:'乃东县',4061:'泽当镇',4062:'扎囊县',4063:'扎塘镇',4064:'贡嘎县',4065:'吉雄镇',4066:'桑日县',4067:'桑日镇',4068:'琼结县',4069:'琼结镇',4070:'曲松县',4071:'曲松镇',4072:'措美县',4073:'措美镇',4074:'洛扎县',4075:'洛扎镇',4076:'加查县',4077:'安绕镇',4078:'隆子县',4079:'隆子镇',4080:'错那县',4081:'错那镇',4082:'浪卡子县',4083:'浪卡子镇'},
	'0,3983,4084':{4085:'日喀则市',4086:'南木林县',4087:'南木林镇',4088:'江孜县',4089:'江孜镇',4090:'定日县',4091:'协格尔镇',4092:'萨迦县',4093:'萨迦镇',4094:'拉孜县',4095:'曲下镇',4096:'昂仁县',4097:'卡嘎镇',4098:'谢通门县',4099:'卡嘎镇',4100:'白朗县',4101:'洛江镇',4102:'仁布县',4103:'德吉林镇',4104:'康马县',4105:'康马镇',4106:'定结县',4107:'江嘎镇',4108:'仲巴县',4109:'拉让乡',4110:'亚东县',4111:'下司马镇',4112:'吉隆县',4113:'宗嘎镇',4114:'聂拉木县',4115:'聂拉木镇',4116:'萨嘎县',4117:'加加镇',4118:'岗巴县',4119:'岗巴镇'},
	'0,3983,4120':{4121:'噶尔县',4122:'狮泉河镇',4123:'普兰县',4124:'普兰镇',4125:'札达县',4126:'托林镇',4127:'日土县',4128:'日土镇',4129:'革吉县',4130:'革吉镇',4131:'改则县',4132:'改则镇',4133:'措勤县',4134:'措勤镇',4135:'林芝县人民政府驻地由林芝镇迁至八一镇'},
	'0,4136':{4137:'西安市',4155:'延安市',4181:'铜川市',4187:'渭南市',4207:'咸阳市',4232:'宝鸡市',4254:'汉中市',4276:'榆林市',4300:'安康市',4320:'商洛市'},
	'0,4136,4137':{4138:'莲湖区',4139:'新城区',4140:'碑林区',4141:'灞桥区',4142:'未央区',4143:'雁塔区',4144:'阎良区',4145:'临潼区',4146:'长安区',4147:'蓝田县',4148:'蓝关镇',4149:'周至县',4150:'二曲镇',4151:'户县',4152:'甘亭镇',4153:'高陵县',4154:'鹿苑镇'},
	'0,4136,4155':{4156:'宝塔区',4157:'延长县',4158:'七里村镇',4159:'延川县',4160:'延川镇',4161:'子长县',4162:'瓦窑堡镇',4163:'安塞县',4164:'真武洞镇',4165:'志丹县',4166:'保安镇',4167:'吴起县',4168:'吴旗镇',4169:'甘泉县',4170:'城关镇',4171:'富县',4172:'富城镇',4173:'洛川县',4174:'凤栖镇',4175:'宜川县',4176:'丹州镇',4177:'黄龙县',4178:'石堡镇',4179:'黄陵县',4180:'桥山镇'},
	'0,4136,4181':{4182:'耀州区',4183:'王益区',4184:'印台区',4185:'宜君县',4186:'城关镇'},
	'0,4136,4187':{4188:'临渭区',4189:'华阴市',4190:'韩城市',4191:'华县',4192:'华州镇',4193:'潼关县',4194:'城关镇',4195:'大荔县',4196:'城关镇',4197:'蒲城县',4198:'城关镇',4199:'澄城县',4200:'城关镇',4201:'白水县',4202:'城关镇',4203:'合阳县',4204:'城关镇',4205:'富平县',4206:'窦村镇'},
	'0,4136,4207':{4208:'秦都区',4209:'杨陵区',4210:'渭城区',4211:'兴平市',4212:'三原县',4213:'城关镇',4214:'泾阳县',4215:'泾干镇',4216:'乾县',4217:'城关镇',4218:'礼泉县',4219:'城关镇',4220:'永寿县',4221:'监军镇',4222:'彬县',4223:'城关镇',4224:'长武县',4225:'昭仁镇',4226:'旬邑县',4227:'城关镇',4228:'淳化县',4229:'城关镇',4230:'武功县',4231:'普集镇'},
	'0,4136,4232':{4233:'渭滨区',4234:'金台区',4235:'陈仓区',4236:'凤翔县',4237:'城关镇',4238:'岐山县',4239:'凤鸣镇',4240:'扶风县',4241:'城关镇',4242:'眉县',4243:'首善镇',4244:'陇县',4245:'城关镇',4246:'千阳县',4247:'城关镇',4248:'麟游县',4249:'九成宫镇',4250:'凤县',4251:'双石铺镇',4252:'太白县',4253:'嘴头镇'},
	'0,4136,4254':{4255:'汉台区',4256:'南郑县',4257:'城关镇',4258:'城固县',4259:'博望镇',4260:'洋县',4261:'洋州镇',4262:'西乡县',4263:'城关镇',4264:'勉县',4265:'勉阳镇',4266:'宁强县',4267:'汉源镇',4268:'略阳县',4269:'城关镇',4270:'镇巴县',4271:'泾洋镇',4272:'留坝县',4273:'城关镇',4274:'佛坪县',4275:'袁家庄镇'},
	'0,4136,4276':{4277:'榆阳区',4278:'神木县',4279:'神木镇',4280:'府谷县',4281:'府谷镇',4282:'横山县',4283:'横山镇',4284:'靖边县',4285:'张家畔镇',4286:'定边县',4287:'定边镇',4288:'绥德县',4289:'名州镇',4290:'米脂县',4291:'银州镇',4292:'佳县',4293:'佳芦镇',4294:'吴堡县',4295:'宋家川镇',4296:'清涧县',4297:'宽洲镇',4298:'子洲县',4299:'双湖峪镇'},
	'0,4136,4300':{4301:'汉滨区',4302:'汉阴县',4303:'城关镇',4304:'石泉县',4305:'城关镇',4306:'宁陕县',4307:'城关镇',4308:'紫阳县',4309:'城关镇',4310:'岚皋县',4311:'城关镇',4312:'平利县',4313:'城关镇',4314:'镇坪县',4315:'城关镇',4316:'旬阳县',4317:'城关镇',4318:'白河县',4319:'城关镇'},
	'0,4136,4320':{4321:'商州区',4322:'洛南县',4323:'城关镇',4324:'丹凤县',4325:'龙驹寨镇',4326:'商南县',4327:'城关镇',4328:'山阳县',4329:'城关镇',4330:'镇安县',4331:'永乐镇',4332:'柞水县',4333:'乾佑镇'},
	'0,4334':{4335:'兰州市',4347:'嘉峪关市',4352:'白银市',4361:'天水市',4374:'武威市',4382:'酒泉市',4394:'张掖市',4406:'庆阳市',4422:'平凉市',4436:'定西市',4450:'陇南市',4467:'临夏州',4483:'甘南州'},
	'0,4334,4335':{4336:'城关区',4337:'七里河区',4338:'西固区',4339:'安宁区',4340:'红古区',4341:'永登县',4342:'城关镇',4343:'皋兰县',4344:'石洞镇',4345:'榆中县',4346:'城关镇'},
	'0,4334,4347':{4348:'金昌市',4349:'金川区',4350:'永昌县',4351:'城关镇'},
	'0,4334,4352':{4353:'白银区',4354:'平川区',4355:'靖远县',4356:'乌兰镇',4357:'会宁县',4358:'会师镇',4359:'景泰县',4360:'一条山镇'},
	'0,4334,4361':{4362:'秦州区',4363:'麦积区',4364:'清水县',4365:'永清镇',4366:'秦安县',4367:'兴国镇',4368:'甘谷县',4369:'大像山镇',4370:'武山县',4371:'城关镇',4372:'张家川回族自治县',4373:'张家川镇'},
	'0,4334,4374':{4375:'凉州区',4376:'民勤县',4377:'城关镇',4378:'古浪县',4379:'古浪镇',4380:'天祝藏族自治县',4381:'华藏寺镇'},
	'0,4334,4382':{4383:'肃州区',4384:'玉门市',4385:'敦煌市',4386:'金塔县',4387:'金塔镇',4388:'安西县',4389:'渊泉镇',4390:'肃北蒙古族自治县',4391:'党城湾镇',4392:'阿克塞哈萨克族自治县',4393:'红柳湾镇'},
	'0,4334,4394':{4395:'甘州区',4396:'民乐县',4397:'洪水镇',4398:'临泽县',4399:'沙河镇',4400:'高台县',4401:'城关镇',4402:'山丹县',4403:'清泉镇',4404:'肃南裕固族自治县',4405:'红湾寺镇'},
	'0,4334,4406':{4407:'西峰区',4408:'庆城县',4409:'庆城镇',4410:'环县',4411:'环城镇',4412:'华池县',4413:'柔远镇',4414:'合水县',4415:'西华池镇',4416:'正宁县',4417:'山河镇',4418:'宁县',4419:'新宁镇',4420:'镇原县',4421:'城关镇'},
	'0,4334,4422':{4423:'崆峒区',4424:'泾川县',4425:'城关镇',4426:'灵台县',4427:'中台镇',4428:'崇信县',4429:'锦屏镇',4430:'华亭县',4431:'东华镇',4432:'庄浪县',4433:'水洛镇',4434:'静宁县',4435:'城关镇'},
	'0,4334,4436':{4437:'安定区',4438:'通渭县',4439:'平襄镇',4440:'临洮县',4441:'洮阳镇',4442:'漳县',4443:'武阳镇',4444:'岷县',4445:'岷阳镇',4446:'渭源县',4447:'清源镇',4448:'陇西县',4449:'巩昌镇'},
	'0,4334,4450':{4451:'武都区',4452:'成县',4453:'城关镇',4454:'宕昌县',4455:'城关镇',4456:'康县',4457:'文县',4458:'城关镇',4459:'西和县',4460:'汉源镇',4461:'礼县',4462:'城关镇',4463:'两当县',4464:'城关镇',4465:'徽县',4466:'城关镇'},
	'0,4334,4467':{4468:'临夏市',4469:'临夏县',4470:'韩集镇',4471:'康乐县',4472:'附城镇',4473:'永靖县',4474:'刘家峡镇',4475:'广河县',4476:'城关镇',4477:'和政县',4478:'城关镇',4479:'东乡族自治县',4480:'锁南坝镇',4481:'积石山保安族东乡族撒拉族自治县',4482:'吹麻滩镇'},
	'0,4334,4483':{4484:'合作市',4485:'临潭县',4486:'城关镇',4487:'卓尼县',4488:'柳林镇',4489:'舟曲县',4490:'城关镇',4491:'迭部县',4492:'电尕镇',4493:'玛曲县',4494:'尼玛镇',4495:'碌曲县',4496:'玛艾镇',4497:'夏河县',4498:'拉卜楞镇'},
	'0,4499':{4500:'西宁市',4511:'海东地区',4524:'海北州',4533:'海南州',4544:'黄南州',4553:'果洛州',4566:'玉树州',4579:'海西州'},
	'0,4499,4500':{4501:'城中区',4502:'城东区',4503:'城西区',4504:'城北区',4505:'大通回族土族自治县',4506:'桥头镇',4507:'湟源县',4508:'城关镇',4509:'湟中县',4510:'鲁沙尔镇'},
	'0,4499,4511':{4512:'平安县',4513:'平安镇',4514:'乐都县',4515:'碾伯镇',4516:'民和回族土族自治县',4517:'川口镇',4518:'互助土族自治县',4519:'威远镇',4520:'化隆回族自治县',4521:'巴燕镇',4522:'循化撒拉族自治县',4523:'积石镇'},
	'0,4499,4524':{4525:'海晏县',4526:'三角城镇',4527:'祁连县',4528:'八宝镇',4529:'刚察县',4530:'沙柳河镇',4531:'门源回族自治县',4532:'浩门镇'},
	'0,4499,4533':{4534:'共和县',4535:'恰卜恰镇',4536:'同德县',4537:'尕巴松多镇',4538:'贵德县',4539:'河阴镇',4540:'兴海县',4541:'子科滩镇',4542:'贵南县',4543:'茫曲镇'},
	'0,4499,4544':{4545:'同仁县',4546:'隆务镇',4547:'尖扎县',4548:'马克唐镇',4549:'泽库县',4550:'泽曲镇',4551:'河南蒙古族自治县',4552:'优干宁镇'},
	'0,4499,4553':{4554:'玛沁县',4555:'大武镇',4556:'班玛县',4557:'赛来塘镇',4558:'甘德县',4559:'柯曲镇',4560:'达日县',4561:'吉迈镇',4562:'久治县',4563:'智青松多镇',4564:'玛多县',4565:'黄河乡'},
	'0,4499,4566':{4567:'玉树县',4568:'结古镇',4569:'杂多县',4570:'萨呼腾镇',4571:'称多县',4572:'称文镇',4573:'治多县',4574:'加吉博洛镇',4575:'囊谦县',4576:'香达镇',4577:'曲麻莱县',4578:'约改镇'},
	'0,4499,4579':{4580:'德令哈市',4581:'格尔木市',4582:'乌兰县',4583:'希里沟镇',4584:'都兰县',4585:'察汗乌苏镇',4586:'天峻县',4587:'新源镇'},
	'0,4588':{4589:'银川市',4598:'石嘴山市',4603:'吴忠市',4610:'固原市',4620:'中卫市'},
	'0,4588,4589':{4590:'兴庆区',4591:'金凤区',4592:'西夏区',4593:'灵武市',4594:'永宁县',4595:'杨和镇',4596:'贺兰县',4597:'习岗镇'},
	'0,4588,4598':{4599:'大武口区',4600:'惠农区',4601:'平罗县',4602:'城关镇'},
	'0,4588,4603':{4604:'利通区',4605:'青铜峡市',4606:'盐池县',4607:'花马池镇',4608:'同心县',4609:'豫海镇'},
	'0,4588,4610':{4611:'原州区',4612:'西吉县',4613:'吉强镇',4614:'隆德县',4615:'城关镇',4616:'泾源县',4617:'香水镇',4618:'彭阳县',4619:'白阳镇'},
	'0,4588,4620':{4621:'沙坡头区',4622:'中宁县',4623:'海原县'},
	'0,4624':{4625:'乌鲁木齐市',4635:'克拉玛依市',4640:'自治区直辖县级行政单位',4645:'喀什地区',4669:'阿克苏地区',4687:'和田地区',4702:'吐鲁番地区',4708:'哈密地区',4714:'克孜勒苏柯州',4722:'博尔塔拉州',4728:'昌吉州',4742:'巴音郭楞州',4760:'伊犁州',4779:'塔城地区',4792:'阿勒泰地区'},
	'0,4624,4625':{4626:'天山区',4627:'沙依巴克区',4628:'新市区',4629:'水磨沟区',4630:'头屯河区',4631:'达坂城区',4632:'东山区',4633:'乌鲁木齐县',4634:'乌鲁木齐市水磨沟区'},
	'0,4624,4635':{4636:'克拉玛依区',4637:'独山子区',4638:'白碱滩区',4639:'乌尔禾区'},
	'0,4624,4640':{4641:'石河子市',4642:'阿拉尔市',4643:'图木舒克市',4644:'五家渠市'},
	'0,4624,4645':{4646:'喀什市',4647:'疏附县',4648:'托克扎克镇',4649:'疏勒县',4650:'疏勒镇',4651:'英吉沙县',4652:'英吉沙镇',4653:'泽普县',4654:'泽普镇',4655:'莎车县',4656:'莎车镇',4657:'叶城县',4658:'喀格勒克镇',4659:'麦盖提县',4660:'麦盖提镇',4661:'岳普湖县',4662:'岳普湖镇',4663:'伽师县',4664:'巴仁镇',4665:'巴楚县',4666:'巴楚镇',4667:'塔什库尔干塔吉克自治县',4668:'塔什库尔干镇'},
	'0,4624,4669':{4670:'阿克苏市',4671:'温宿县',4672:'温宿镇',4673:'库车县',4674:'库车镇',4675:'沙雅县',4676:'沙雅镇',4677:'新和县',4678:'新和镇',4679:'拜城县',4680:'拜城镇',4681:'乌什县',4682:'乌什镇',4683:'阿瓦提县',4684:'阿瓦提镇',4685:'柯坪县',4686:'柯坪镇'},
	'0,4624,4687':{4688:'和田市',4689:'和田县',4690:'墨玉县',4691:'喀拉喀什镇',4692:'皮山县',4693:'固玛镇',4694:'洛浦县',4695:'洛浦镇',4696:'策勒县',4697:'策勒镇',4698:'于田县',4699:'木尕拉镇',4700:'民丰县',4701:'尼雅镇'},
	'0,4624,4702':{4703:'吐鲁番市',4704:'鄯善县',4705:'鄯善镇',4706:'托克逊县',4707:'托克逊镇'},
	'0,4624,4708':{4709:'哈密市',4710:'伊吾县',4711:'伊吾镇',4712:'巴里坤哈萨克自治县',4713:'巴里坤镇'},
	'0,4624,4714':{4715:'阿图什市',4716:'阿克陶县',4717:'阿克陶镇',4718:'阿合奇县',4719:'阿合奇镇',4720:'乌恰县',4721:'乌恰镇'},
	'0,4624,4722':{4723:'博乐市',4724:'精河县',4725:'精河镇',4726:'温泉县',4727:'博格达尔镇'},
	'0,4624,4728':{4729:'昌吉市',4730:'阜康市',4731:'米泉市',4732:'呼图壁县',4733:'呼图壁镇',4734:'玛纳斯县',4735:'玛纳斯镇',4736:'奇台县',4737:'奇台镇',4738:'吉木萨尔县',4739:'吉木萨尔镇',4740:'木垒哈萨克自治县',4741:'木垒镇'},
	'0,4624,4742':{4743:'库尔勒市',4744:'轮台县',4745:'轮台镇',4746:'尉犁县',4747:'尉犁镇',4748:'若羌县',4749:'若羌镇',4750:'且末县',4751:'且末镇',4752:'和静县',4753:'和静镇',4754:'和硕县',4755:'特吾里克镇',4756:'博湖县',4757:'博湖镇',4758:'焉耆回族自治县',4759:'焉耆镇'},
	'0,4624,4760':{4761:'伊宁市',4762:'奎屯市',4763:'伊宁县',4764:'吉里于孜镇',4765:'霍城县',4766:'水定镇',4767:'巩留县',4768:'巩留镇',4769:'新源县',4770:'新源镇',4771:'昭苏县',4772:'昭苏镇',4773:'特克斯县',4774:'特克斯镇',4775:'尼勒克县',4776:'尼勒克镇',4777:'察布查尔锡伯自治县',4778:'察布查尔镇'},
	'0,4624,4779':{4780:'塔城市',4781:'乌苏市',4782:'额敏县',4783:'额敏镇',4784:'沙湾县',4785:'三道河子镇',4786:'托里县',4787:'托里镇',4788:'裕民县',4789:'哈拉布拉镇',4790:'和布克赛尔蒙古自治县',4791:'和布克赛尔镇'},
	'0,4624,4792':{4793:'阿勒泰市',4794:'布尔津县',4795:'布尔津镇',4796:'富蕴县',4797:'库额尔齐斯镇',4798:'福海县',4799:'哈巴河县',4800:'青河县',4801:'吉木乃县'},
	'0,4802':{4803:'香港特别行政区'},
	'0,4802,4803':{4804:'中西区',4805:'东区',4806:'九龙城区',4807:'观塘区',4808:'南区',4809:'深水埗区',4810:'湾仔区',4811:'黄大仙区',4812:'油尖旺区',4813:'离岛区',4814:'葵青区',4815:'北区',4816:'西贡区',4817:'沙田区',4818:'屯门区',4819:'大埔区',4820:'荃湾区',4821:'元朗区'},
	'0,4822':{4823:'澳门特别行政区'},
	'0,4822,4823':{4824:'澳门特别行政区'},
	'0,4825':{4826:'台北',4827:'高雄',4828:'台中',4829:'花莲',4830:'基隆',4831:'嘉义',4832:'金门',4833:'连江',4834:'苗栗',4835:'南投',4836:'澎湖',4837:'屏东',4838:'台东',4839:'台南',4840:'桃园',4841:'新竹',4842:'宜兰',4843:'云林',4844:'彰化'}
	};
}

Location.prototype.find	= function(id) {
	if(typeof(this.items[id]) == "undefined")
		return false;
	return this.items[id];
}

Location.prototype.fillOption	= function(el_id , loc_id , selected_id) {
	var el	= $('#'+el_id); 
	var json	= this.find(loc_id); 
	if (json) {
		var index	= 1;
		var selected_index	= 0;
		$.each(json , function(k , v) {
			var option	= '<option value="'+k+'">'+v+'</option>';
			el.append(option);
			
			if (k == selected_id) {
				selected_index	= index;
			}
			
			index++;
		})
		//el.attr('selectedIndex' , selected_index); 
	}
	el.select2("val", "");
}

