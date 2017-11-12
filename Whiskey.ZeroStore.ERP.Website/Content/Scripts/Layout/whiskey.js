
(function ($) {
    $.whiskey = $.whiskey || { version: 2.1 };
})(jQuery);
(function ($) {
    var jsonstr = null;
    var otherstr = null;
    var statustr = null;
    var _arr = [];
    var _arr_other = null;
    $.whiskey.tools = {
        url: {
            encode: function (url) {
                return encodeURIComponent(url);
            },
            decode: function (url) {
                return decodeURIComponent(url);
            }
        },
        arraylist: {
            add: function (da) {
                _arr.push(da)
            },
            get: function (inde) {
                return _arr[inde];
            },
            getRang: function (min, max) {
                if (min > max) {
                    var t = min;
                    min = max;
                    max = t;
                }
                var temArr = [];
                for (var i = 0; i < _arr.length; i++) {
                    if (i >= min && i < max) {
                        temArr.push(_arr[i]);
                    }
                }
                return temArr;
            },
            dele: function (index) {
                _arr.splice(index, 1);
            },
            clear: function () {
                _arr = [];
            },
            leng: function () {
                return _arr.length;
            },
            other: function (da) {
                if (da != undefined) {
                    _arr_other = da;
                }
                return _arr_other;
            }
        },
        json: function (da) {

            if (da != undefined) {
                jsonstr = da;
            }
            else
                return jsonstr;
        },
        other: function (da) {
            if (da != undefined) {
                otherstr = da;
            }
            else
                return otherstr;
        },
        status: function (da) {
            if (da != undefined) {
                statustr = da;
            }
            else
                return statustr;
        },
        dateFormat: function (value, format, loc) {
            var time = {};
            var datetime = new Date();
            if (typeof (value) == "string") {
                var timestamp = value.replace(/\/Date\((\d+)\)\//gi, '$1');
                if (format == undefined || format.length <= 0) {
                    format = "yyyy年MM月dd日";
                }
                datetime.setTime(timestamp);
            } else {
                datetime = value;
            }
            time.Year = datetime.getFullYear();
            time.TYear = ("" + time.Year).substr(2);
            time.Month = datetime.getMonth() + 1;
            time.TMonth = time.Month < 10 ? "0" + time.Month : time.Month;
            time.Day = datetime.getDate();
            time.TDay = time.Day < 10 ? "0" + time.Day : time.Day;
            time.Hour = datetime.getHours();
            time.THour = time.Hour < 10 ? "0" + time.Hour : time.Hour;
            time.hour = time.Hour < 13 ? time.Hour : time.Hour - 12;
            time.Thour = time.hour < 10 ? "0" + time.hour : time.hour;
            time.Minute = datetime.getMinutes();
            time.TMinute = time.Minute < 10 ? "0" + time.Minute : time.Minute;
            time.Second = datetime.getSeconds();
            time.TSecond = time.Second < 10 ? "0" + time.Second : time.Second;
            time.Millisecond = datetime.getMilliseconds();
            time.Week = datetime.getDay();

            var MMMArrEn = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            var MMMArr = ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"];
            var WeekArrEn = ["Sun", "Mon", "Tue", "Web", "Thu", "Fri", "Sat"];
            var WeekArr = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

            var oNumber = time.Millisecond / 1000;

            if (format != undefined && format.replace(/\s/g, "").length > 0) {
                if (loc != undefined && loc == "en") {
                    MMMArr = MMMArrEn.slice(0);
                    WeekArr = WeekArrEn.slice(0);
                }
                format = format
                    .replace(/yyyy/ig, time.Year)
                    .replace(/yyy/ig, time.Year)
                    .replace(/yy/ig, time.TYear)
                    .replace(/y/ig, time.TYear)
                    .replace(/MMM/g, MMMArr[time.Month - 1])
                    .replace(/MM/g, time.TMonth)
                    .replace(/M/g, time.Month)
                    .replace(/dd/ig, time.TDay)
                    .replace(/d/ig, time.Day)
                    .replace(/HH/g, time.THour)
                    .replace(/H/g, time.Hour)
                    .replace(/hh/g, time.Thour)
                    .replace(/h/g, time.hour)
                    .replace(/mm/g, time.TMinute)
                    .replace(/m/g, time.Minute)
                    .replace(/ss/ig, time.TSecond)
                    .replace(/s/ig, time.Second)
                    .replace(/fff/ig, time.Millisecond)
                    .replace(/ff/ig, oNumber.toFixed(2) * 100)
                    .replace(/f/ig, oNumber.toFixed(1) * 10)
                    .replace(/EEE/g, WeekArr[time.Week]);
            }
            else {
                format = time.Year + "-" + time.Month + "-" + time.Day + " " + time.Hour + ":" + time.Minute + ":" + time.Second;
            }
            return format;
        },
        getOneDay: function (day, format) {
            //获取前后某一天时间
            if (isNaN(day)) {
                var daytemp = parseInt(day);
                if (isNaN(daytemp)) {
                    throw new TypeError("getOneDay,参数无效");
                }
                day = daytemp;
            }
            var sdate = new Date().getTime();
            var edate = new Date(sdate + (day * 24 * 60 * 60 * 1000));
            edate = this.dateFormat(edate, format || "yyyy-MM-dd");
            return edate;
        },
        dateAddDay: function dateAddDays(dataStr, dayCount, formart) {
            var strdate = dataStr; //日期字符串
            var isdate = new Date(strdate.replace(/-/g, "/"));  //把日期字符串转换成日期格式
            isdate = new Date((isdate / 1000 + (86400 * dayCount)) * 1000);  //日期加1天
            var pdate = $.whiskey.tools.dateFormat(isdate, formart || "yyyy/MM/dd");
            return pdate;
        },
        numberToChinese: function NumberToChinese(number, isZero) {
            var result = "";
            number = number.toString();
            if (number == 100) {
                return "原价";
            }
            for (var i = 0; i < number.length; i++) {
                switch (number.charAt(i)) {
                    case "0":
                        if (isZero) result += "零";
                        break;
                    case "1":
                        result += "一";
                        break;
                    case "2":
                        result += "二";
                        break;
                    case "3":
                        result += "三";
                        break;
                    case "4":
                        result += "四";
                        break;
                    case "5":
                        result += "五";
                        break;
                    case "6":
                        result += "六";
                        break;
                    case "7":
                        result += "七";
                        break;
                    case "8":
                        result += "八";
                        break;
                    case "9":
                        result += "九";
                        break;
                }
            }
            return result + "折";
        },
        chineseToUTF8: function (chinese) {
            var out, i, len, c;
            out = "";
            len = chinese.length;
            for (i = 0; i < len; i++) {
                c = chinese.charCodeAt(i);
                if ((c >= 0x0001) && (c <= 0x007F)) {
                    out += chinese.charAt(i);
                } else if (c > 0x07FF) {
                    out += String.fromCharCode(0xE0 | ((c >> 12) & 0x0F));
                    out += String.fromCharCode(0x80 | ((c >> 6) & 0x3F));
                    out += String.fromCharCode(0x80 | ((c >> 0) & 0x3F));
                } else {
                    out += String.fromCharCode(0xC0 | ((c >> 6) & 0x1F));
                    out += String.fromCharCode(0x80 | ((c >> 0) & 0x3F));
                }
            }
            return out;
        },
        fixedLength: function (str, num) {
            var len = str.replace(/[^\x00-\xff]/g, "rr").length;
            if (len < num) {
                for (var i = 0; i < (num - len) ; i++) {
                    str += " ";
                }
            } else if (num > len) {
                str = str.substring(0, num);
            }
            return str;
        },
        virtEquals: function (obj, other) {
            if (obj === null || other === null) {
                return (obj === null) && (other === null);
            }
            if (typeof (obj) === "string") {
                return obj === other;
            }
            if (typeof (obj) !== "object") {
                return obj === other;
            }
            if (obj.equals instanceof Function) {
                return obj.equals(other);
            }
            return obj === other;
        },
        UUID: function (len, radix) {//进制
            var chars = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz'.split('');
            var uuid = [], i;
            radix = radix || chars.length;

            if (len) {
                // Compact form
                for (i = 0; i < len; i++) uuid[i] = chars[0 | Math.random() * radix];
            } else {
                // rfc4122, version 4 form
                var r;

                // rfc4122 requires these characters
                uuid[8] = uuid[13] = uuid[18] = uuid[23] = '-';
                uuid[14] = '4';

                // Fill in random data.  At i==19 set the high bits of clock sequence as
                // per rfc4122, sec. 4.1.5
                for (i = 0; i < 36; i++) {
                    if (!uuid[i]) {
                        r = 0 | Math.random() * 16;
                        uuid[i] = chars[(i == 19) ? (r & 0x3) | 0x8 : r];
                    }
                }
            }

            return uuid.join('');
        },
        rgbToHex: function (r, g, b) {
            var rh = r.toString(16);
            var gh = g.toString(16);
            var bh = b.toString(16);
            return "#" + (rh.length == 1 ? "0" + rh : rh) + (gh.length == 1 ? "0" + gh : gh) + (bh.length == 1 ? "0" + bh : bh);
        },
        hexToRgb: function (hex) {
            var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : null;
        },
        rgbToHsl: function (r, g, b) {
            r /= 255, g /= 255, b /= 255;
            var max = Math.max(r, g, b), min = Math.min(r, g, b);
            var h, s, l = (max + min) / 2;

            if (max == min) {
                h = s = 0; // achromatic
            } else {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                switch (max) {
                    case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                    case g: h = (b - r) / d + 2; break;
                    case b: h = (r - g) / d + 4; break;
                }
                h /= 6;
            }
            return [h, s, l];
        },
        hslToRgb: function (h, s, l) {
            var r, g, b;

            if (s == 0) {
                r = g = b = l;
            } else {
                var hue2rgb = function hue2rgb(p, q, t) {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6) return p + (q - p) * 6 * t;
                    if (t < 1 / 2) return q;
                    if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                    return p;
                }

                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = hue2rgb(p, q, h + 1 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3);
            }

            return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
        },
        //yky 2016-8-10
        showVerify: function () {
            if (verifyType == 0) {
                return "审核中 /";
            } else if (verifyType == 1) {
                return "通过";
            } else {
                return "未通过";
            }
        },

        verifyType: function (verifyType, id) {
            if (verifyType == 0) {
                var str = '<a style="cursor:pointer" data-id="' + id + '" href="javascript:void(0)" onclick="updateVerify(this)">审核</a>';
                return "审核中 /" + str;
            } else if (verifyType == 1) {
                return "通过";
            } else {
                return "未通过";
            }
        },
    };

    $.whiskey.arraylist = (function (a) {

        function ArrayList(a) {
            var array = [];

            if (a && a.toArray) {
                array = a.toArray();
            }

            this.get = function (i) {
                return array[i];
            };

            this.contains = function (item) {
                return this.indexOf(item) > -1;
            };

            this.indexOf = function (item) {
                for (var i = 0, len = array.length; i < len; ++i) {
                    if ($.whiskey.tools.virtEquals(item, array[i])) {
                        return i;
                    }
                }
                return -1;
            };

            this.lastIndexOf = function (item) {
                for (var i = array.length - 1; i >= 0; --i) {
                    if ($.whiskey.tools.virtEquals(item, array[i])) {
                        return i;
                    }
                }
                return -1;
            };

            this.getLastOne = function () {
                var o = "";
                for (i = 0; i < array.length; i++) {
                    if (array[i] != null) {
                        o = array[i];
                    }
                }
                return o;
            }

            this.add = function () {
                if (arguments.length === 1) {
                    array.push(arguments[0]); // for add(Object)
                } else if (arguments.length === 2) {
                    var arg0 = arguments[0];
                    if (typeof arg0 === 'number') {
                        if (arg0 >= 0 && arg0 <= array.length) {
                            array.splice(arg0, 0, arguments[1]); // for add(i, Object)
                        } else {
                            throw (arg0 + " is not a valid index");
                        }
                    } else {
                        throw (typeof arg0 + " is not a number");
                    }
                } else {
                    throw ("Please use the proper number of parameters.");
                }
            };

            this.addAll = function (arg1, arg2) {
                // addAll(int, Collection)
                var it;
                if (typeof arg1 === "number") {
                    if (arg1 < 0 || arg1 > array.length) {
                        throw ("Index out of bounds for addAll: " + arg1 + " greater or equal than " + array.length);
                    }
                    it = new ObjectIterator(arg2);
                    while (it.hasNext()) {
                        array.splice(arg1++, 0, it.next());
                    }
                }
                    // addAll(Collection)
                else {
                    it = new ObjectIterator(arg1);
                    while (it.hasNext()) {
                        array.push(it.next());
                    }
                }
            };

            this.set = function () {
                if (arguments.length === 2) {
                    var arg0 = arguments[0];
                    if (typeof arg0 === 'number') {
                        if (arg0 >= 0 && arg0 < array.length) {
                            array.splice(arg0, 1, arguments[1]);
                        } else {
                            throw (arg0 + " is not a valid index.");
                        }
                    } else {
                        throw (typeof arg0 + " is not a number");
                    }
                } else {
                    throw ("Please use the proper number of parameters.");
                }
            };

            this.size = function () {
                return array.length;
            };

            this.clear = function () {
                array.length = 0;
            };

            this.remove = function (item) {
                if (typeof item === 'number') {
                    return array.splice(item, 1)[0];
                }
                item = this.indexOf(item);
                if (item > -1) {
                    array.splice(item, 1);
                    return true;
                }
                return false;
            };

            this.removeAll = function (c) {
                var i, x, item,
                    newList = new ArrayList();
                newList.addAll(this);
                this.clear();
                // For every item that exists in the original ArrayList and not in the c ArrayList
                // copy it into the empty 'this' ArrayList to create the new 'this' Array.
                for (i = 0, x = 0; i < newList.size() ; i++) {
                    item = newList.get(i);
                    if (!c.contains(item)) {
                        this.add(x++, item);
                    }
                }
                if (this.size() < newList.size()) {
                    return true;
                }
                return false;
            };

            this.isEmpty = function () {
                return !array.length;
            };

            this.clone = function () {
                return new ArrayList(this);
            };

            this.toArray = function () {
                return array.slice(0);
            };

            this.iterator = function () {
                return new Iterator(array);
            };

        }

        return ArrayList;
    })();

    $.whiskey.hashtable = (function (UNDEFINED) {
        var FUNCTION = "function", STRING = "string", UNDEF = "undefined";

        // Require Array.prototype.splice, Object.prototype.hasOwnProperty and encodeURIComponent. In environments not
        // having these (e.g. IE <= 5), we bail out now and leave Hashtable null.
        if (typeof encodeURIComponent == UNDEF ||
                Array.prototype.splice === UNDEFINED ||
                Object.prototype.hasOwnProperty === UNDEFINED) {
            return null;
        }

        function toStr(obj) {
            return (typeof obj == STRING) ? obj : "" + obj;
        }

        function hashObject(obj) {
            var hashCode;
            if (typeof obj == STRING) {
                return obj;
            } else if (typeof obj.hashCode == FUNCTION) {
                // Check the hashCode method really has returned a string
                hashCode = obj.hashCode();
                return (typeof hashCode == STRING) ? hashCode : hashObject(hashCode);
            } else {
                return toStr(obj);
            }
        }

        function merge(o1, o2) {
            for (var i in o2) {
                if (o2.hasOwnProperty(i)) {
                    o1[i] = o2[i];
                }
            }
        }

        function equals_fixedValueHasEquals(fixedValue, variableValue) {
            return fixedValue.equals(variableValue);
        }

        function equals_fixedValueNoEquals(fixedValue, variableValue) {
            return (typeof variableValue.equals == FUNCTION) ?
                variableValue.equals(fixedValue) : (fixedValue === variableValue);
        }

        function createKeyValCheck(kvStr) {
            return function (kv) {
                if (kv === null) {
                    throw new Error("null is not a valid " + kvStr);
                } else if (kv === UNDEFINED) {
                    throw new Error(kvStr + " must not be undefined");
                }
            };
        }

        var checkKey = createKeyValCheck("key"), checkValue = createKeyValCheck("value");

        /*----------------------------------------------------------------------------------------------------------------*/

        function Bucket(hash, firstKey, firstValue, equalityFunction) {
            this[0] = hash;
            this.entries = [];
            this.addEntry(firstKey, firstValue);

            if (equalityFunction !== null) {
                this.getEqualityFunction = function () {
                    return equalityFunction;
                };
            }
        }

        var EXISTENCE = 0, ENTRY = 1, ENTRY_INDEX_AND_VALUE = 2;

        function createBucketSearcher(mode) {
            return function (key) {
                var i = this.entries.length, entry, equals = this.getEqualityFunction(key);
                while (i--) {
                    entry = this.entries[i];
                    if (equals(key, entry[0])) {
                        switch (mode) {
                            case EXISTENCE:
                                return true;
                            case ENTRY:
                                return entry;
                            case ENTRY_INDEX_AND_VALUE:
                                return [i, entry[1]];
                        }
                    }
                }
                return false;
            };
        }

        function createBucketLister(entryProperty) {
            return function (aggregatedArr) {
                var startIndex = aggregatedArr.length;
                for (var i = 0, entries = this.entries, len = entries.length; i < len; ++i) {
                    aggregatedArr[startIndex + i] = entries[i][entryProperty];
                }
            };
        }

        Bucket.prototype = {
            getEqualityFunction: function (searchValue) {
                return (typeof searchValue.equals == FUNCTION) ? equals_fixedValueHasEquals : equals_fixedValueNoEquals;
            },

            getEntryForKey: createBucketSearcher(ENTRY),

            getEntryAndIndexForKey: createBucketSearcher(ENTRY_INDEX_AND_VALUE),

            removeEntryForKey: function (key) {
                var result = this.getEntryAndIndexForKey(key);
                if (result) {
                    this.entries.splice(result[0], 1);
                    return result[1];
                }
                return null;
            },

            addEntry: function (key, value) {
                this.entries.push([key, value]);
            },

            keys: createBucketLister(0),

            values: createBucketLister(1),

            getEntries: function (destEntries) {
                var startIndex = destEntries.length;
                for (var i = 0, entries = this.entries, len = entries.length; i < len; ++i) {
                    // Clone the entry stored in the bucket before adding to array
                    destEntries[startIndex + i] = entries[i].slice(0);
                }
            },

            containsKey: createBucketSearcher(EXISTENCE),

            containsValue: function (value) {
                var entries = this.entries, i = entries.length;
                while (i--) {
                    if (value === entries[i][1]) {
                        return true;
                    }
                }
                return false;
            }
        };

        /*----------------------------------------------------------------------------------------------------------------*/

        // Supporting functions for searching hashtable buckets

        function searchBuckets(buckets, hash) {
            var i = buckets.length, bucket;
            while (i--) {
                bucket = buckets[i];
                if (hash === bucket[0]) {
                    return i;
                }
            }
            return null;
        }

        function getBucketForHash(bucketsByHash, hash) {
            var bucket = bucketsByHash[hash];

            // Check that this is a genuine bucket and not something inherited from the bucketsByHash's prototype
            return (bucket && (bucket instanceof Bucket)) ? bucket : null;
        }

        /*----------------------------------------------------------------------------------------------------------------*/

        function Hashtable() {
            var buckets = [];
            var bucketsByHash = {};
            var properties = {
                replaceDuplicateKey: true,
                hashCode: hashObject,
                equals: null
            };

            var arg0 = arguments[0], arg1 = arguments[1];
            if (arg1 !== UNDEFINED) {
                properties.hashCode = arg0;
                properties.equals = arg1;
            } else if (arg0 !== UNDEFINED) {
                merge(properties, arg0);
            }

            var hashCode = properties.hashCode, equals = properties.equals;

            this.properties = properties;

            this.put = function (key, value) {
                checkKey(key);
                checkValue(value);
                var hash = hashCode(key), bucket, bucketEntry, oldValue = null;

                // Check if a bucket exists for the bucket key
                bucket = getBucketForHash(bucketsByHash, hash);
                if (bucket) {
                    // Check this bucket to see if it already contains this key
                    bucketEntry = bucket.getEntryForKey(key);
                    if (bucketEntry) {
                        // This bucket entry is the current mapping of key to value, so replace the old value.
                        // Also, we optionally replace the key so that the latest key is stored.
                        if (properties.replaceDuplicateKey) {
                            bucketEntry[0] = key;
                        }
                        oldValue = bucketEntry[1];
                        bucketEntry[1] = value;
                    } else {
                        // The bucket does not contain an entry for this key, so add one
                        bucket.addEntry(key, value);
                    }
                } else {
                    // No bucket exists for the key, so create one and put our key/value mapping in
                    bucket = new Bucket(hash, key, value, equals);
                    buckets.push(bucket);
                    bucketsByHash[hash] = bucket;
                }
                return oldValue;
            };

            this.get = function (key) {
                checkKey(key);

                var hash = hashCode(key);

                // Check if a bucket exists for the bucket key
                var bucket = getBucketForHash(bucketsByHash, hash);
                if (bucket) {
                    // Check this bucket to see if it contains this key
                    var bucketEntry = bucket.getEntryForKey(key);
                    if (bucketEntry) {
                        // This bucket entry is the current mapping of key to value, so return the value.
                        return bucketEntry[1];
                    }
                }
                return null;
            };

            this.containsKey = function (key) {
                checkKey(key);
                var bucketKey = hashCode(key);

                // Check if a bucket exists for the bucket key
                var bucket = getBucketForHash(bucketsByHash, bucketKey);

                return bucket ? bucket.containsKey(key) : false;
            };

            this.containsValue = function (value) {
                checkValue(value);
                var i = buckets.length;
                while (i--) {
                    if (buckets[i].containsValue(value)) {
                        return true;
                    }
                }
                return false;
            };

            this.clear = function () {
                buckets.length = 0;
                bucketsByHash = {};
            };

            this.isEmpty = function () {
                return !buckets.length;
            };

            var createBucketAggregator = function (bucketFuncName) {
                return function () {
                    var aggregated = [], i = buckets.length;
                    while (i--) {
                        buckets[i][bucketFuncName](aggregated);
                    }
                    return aggregated;
                };
            };

            this.keys = createBucketAggregator("keys");
            this.values = createBucketAggregator("values");
            this.entries = createBucketAggregator("getEntries");

            this.remove = function (key) {
                checkKey(key);

                var hash = hashCode(key), bucketIndex, oldValue = null;

                // Check if a bucket exists for the bucket key
                var bucket = getBucketForHash(bucketsByHash, hash);

                if (bucket) {
                    // Remove entry from this bucket for this key

                    oldValue = bucket.removeEntryForKey(key);

                    if (oldValue !== null) {
                        // Entry was removed, so check if bucket is empty
                        if (bucket.entries.length == 0) {
                            // Bucket is empty, so remove it from the bucket collections

                            bucketIndex = searchBuckets(buckets, hash);
                            buckets.splice(bucketIndex, 1);
                            delete bucketsByHash[hash];
                        }
                    }
                }
                return oldValue;
            };

            this.size = function () {
                var total = 0, i = buckets.length;
                while (i--) {
                    total += buckets[i].entries.length;
                }
                return total;
            };

            this.getFirst = function (index) {
                var o;
                if (typeof (index) != "number") {
                    index = 1;
                }
                if (buckets[0].entries[0][index] != null) {
                    o = buckets[0].entries[0][index];
                }
                return o;
            }

            this.getLast = function (index) {
                var o;
                if (typeof (index) != "number") {
                    index = 1;
                }
                for (i = 0; i < buckets.length; i++) {
                    if (buckets[i].entries[0][index] != null) {
                        o = buckets[i].entries[0][index];
                    }
                }
                return o;
            }

        }

        Hashtable.prototype = {
            each: function (callback) {
                var entries = this.entries(), i = entries.length, entry;
                while (i--) {
                    entry = entries[i];
                    callback(entry[0], entry[1]);
                }
            },

            equals: function (hashtable) {
                var keys, key, val, count = this.size();
                if (count == hashtable.size()) {
                    keys = this.keys();
                    while (count--) {
                        key = keys[count];
                        val = hashtable.get(key);
                        if (val === null || val !== this.get(key)) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            },

            putAll: function (hashtable, conflictCallback) {
                var entries = hashtable.entries();
                var entry, key, value, thisValue, i = entries.length;
                var hasConflictCallback = (typeof conflictCallback == FUNCTION);
                while (i--) {
                    entry = entries[i];
                    key = entry[0];
                    value = entry[1];

                    // Check for a conflict. The default behaviour is to overwrite the value for an existing key
                    if (hasConflictCallback && (thisValue = this.get(key))) {
                        value = conflictCallback(key, thisValue, value);
                    }
                    this.put(key, value);
                }
            },

            clone: function () {
                var clone = new Hashtable(this.properties);
                clone.putAll(this);
                return clone;
            }
        };

        Hashtable.prototype.toQueryString = function () {
            var entries = this.entries(), i = entries.length, entry;
            var parts = [];
            while (i--) {
                entry = entries[i];
                parts[i] = encodeURIComponent(toStr(entry[0])) + "=" + encodeURIComponent(toStr(entry[1]));
            }
            return parts.join("&");
        };

        Hashtable.prototype.toString = function () {
            var entries = this.entries(), i = entries.length, entry;
            var strings = "";
            while (i--) {
                entry = entries[i];
                strings += toStr(entry[0]) + "=" + toStr(entry[1]);
            }
            return strings;
        };

        return Hashtable;
    })();

    $.whiskey.filter = {
        rule: function (field, value, operate) {
            this.Field = field;
            this.Value = value;
            this.Operate = operate || "equal";
        },
        group: function (operate) {
            this.Rules = [];
            this.Operate = operate || "and";
            this.Operates = [];
            this.Groups = [];
        },
        conditions: function () {

        }

    };
    $.whiskey.waterfall = {
        reset: function (container) {
            $(container).waterfall('refresh');
        }
    };
    //给reset方法添加了第二个参数，表示需要重新加载的datatable对象  yxk 2015-9-18
    $.whiskey.datatable = {
        instance: undefined,
        instances: [],
        reset: function (saveStatus, ntable) {
            var oTable = $.whiskey.datatable.instance;

            //除了传递进来的对象，其余的datatable全部移除 yxk
            if (ntable != undefined) {
                oTable = ntable;
                //for (var i = 0; i < oTable.length; i++) {
                //    if ($(oTable[i]).attr("id") != $(ntable).attr("id")) {
                //        oTable.splice(i,1);
                //    } }
            }
            if (typeof (oTable) == "object") {
                if (saveStatus) {
                    var start = oTable.fnSettings()._iDisplayStart;
                    var total = oTable.fnSettings().fnRecordsDisplay();
                    var setting = oTable.fnSettings();
                    setting._iDisplayStart = start;
                    oTable.fnDraw(setting);
                    if ((total - start) == 1) {
                        if (start > 0) {
                            oTable.fnPageChange('previous', true);
                        }
                    }
                } else {

                    oTable.fnDraw();
                }
            }
            $(oTable).find("input").not(':button, :submit, :reset').val('').removeAttr('checked');
        },
        controller: function (data) {
            var controller = "";
            if (typeof (data) == "function") {
                controller = data();
            } else if (typeof (data) == "object") {
                var isDeleted = data.IsDeleted;
                var isEnabled = data.IsEnabled;
                if (isDeleted == false) {
                    controller += $.whiskey.datatable.tplView(data.Id);
                    try {
                        if (data.Id.indexOf("childStore") < 0) {
                            //店铺搭配 子列表处理
                            controller += $.whiskey.datatable.tplUpdate(data.Id);
                        }
                    } catch (e) {

                        controller += $.whiskey.datatable.tplUpdate(data.Id);
                    }
                    if (!isEnabled) {
                        controller += $.whiskey.datatable.tplEnable(data.Id);
                    } else {
                        controller += $.whiskey.datatable.tplDisable(data.Id);
                    }
                    controller += $.whiskey.datatable.tplRemove(data.Id);
                } else {
                    controller += $.whiskey.datatable.tplView(data.Id);
                    controller += $.whiskey.datatable.tplRecovery(data.Id);
                    //controller += $.whiskey.datatable.tplDelete(data.Id);
                }
            }
            return controller;
        },
        notEditController: function (data) {
            var controller = "";
            if (typeof (data) == "function") {
                controller = data();
            } else if (typeof (data) == "object") {
                var isDeleted = data.IsDeleted;
                var isEnabled = data.IsEnabled;
                if (isDeleted == false) {
                    controller += $.whiskey.datatable.tplView(data.Id);
                    if (!isEnabled) {
                        controller += $.whiskey.datatable.tplEnable(data.Id);
                    } else {
                        controller += $.whiskey.datatable.tplDisable(data.Id);
                    }
                    controller += $.whiskey.datatable.tplRemove(data.Id);
                } else {
                    controller += $.whiskey.datatable.tplView(data.Id);
                    controller += $.whiskey.datatable.tplRecovery(data.Id);
                    controller += $.whiskey.datatable.tplDelete(data.Id);
                }
            }
            return controller;
        },

        lblColor: function (value, calc, onclick) {
            var lbl = "info";
            if (calc) {
                if (typeof calc == 'boolean' && !isNaN(value))
                    lbl = value > 0 ? "success" : value == 0 ? "info" : "danger";
                else
                    lbl = calc;
            }
            var prex = "<label class='label label-" + lbl;
            if (onclick) {
                prex += "' onclick='" + onclick;
            }
            return prex + "'>" + value + "</label>";
        },
        //审核
        auditController: function (idorfun, funNames) {
            var controller = "";
            if (typeof (idorfun) == "function") {
                controller = idorfun();
            } else {

                controller += $.whiskey.datatable.tplauditOk(funNames.auditOk, idorfun);

                controller += $.whiskey.datatable.tplauditNo(funNames.auditNo, idorfun);
                controller += $.whiskey.datatable.tplViewDa(funNames.view, idorfun);
                controller += $.whiskey.datatable.tplEdit(funNames.edit, idorfun);



            }
            return controller;
        },
        //修改和预览，但是不带审核
        viewAndEditController: function (idorfun, funNames) {
            var controller = "";
            if (typeof (idorfun) == "function") {
                controller = idorfun();
            } else {

                controller += $.whiskey.datatable.tplViewDa(funNames.view, idorfun);
                controller += $.whiskey.datatable.tplEdit(funNames.edit, idorfun);



            }
            return controller;
        },
        tplTitleCheckbox: function (flg) {
            if (flg == undefined || flg == null)
                flg = "checked-all";
            return '<label style="display:inline" class="px-single"><input type="checkbox" class="px ' + flg + '" checked="checked"><span class="tal" style="点击全选">全选</span></label>';
        },
        //预览数据
        tplViewDa: function (funName, value) {
            return "<button id=\"View\"  title=\"查看详细信息\" type=\"button\"  onclick=" + funName + "(this,'" + value + "'); class=\"btn btn-xs btn-padding-right\"><i class=\"fa fa-eye\"></i> </button>";
        },
        //编辑
        tplEdit: function (funName, value) {
            return "<button id=\"Update\"  title='修改' type=\"button\"  onclick=" + funName + "(this,'" + value + "'); class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-pencil\"></i> </button>";
        },
        //审核通过
        tplauditOk: function (funName, value, IdName) {
            return "<button id=\"" + (IdName || 'Update') + "\" style='margin-right:2px'  title=\"审核通过\" type=\"button\"  onclick=" + funName + "(this,'" + value + "'); class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-clipboard \"></i> </button>";
        },
        //审核不通过
        tplauditNo: function (funName, value, IdName) {
            return "<button id=\"" + (IdName || 'Update') + "\" style='margin-right:2px'  title=\"不通过\" type=\"button\"  onclick=" + funName + "(this,'" + value + "'); class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-life-ring\"></i> </button>";
        },

        //拒绝配货
        tplRefuse: function (value) {
            return "<button id=\"Refuse\" style='margin-right:2px'  title=\"拒绝配货\" type=\"button\"  onclick=Refuse(this,'" + value + "'); class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-life-ring\"></i> </button>";
        },

        tplListCheckbox: function (value, flg, ische) {

            if (flg == undefined || flg == null)
                flg = "";
            if (ische == undefined || ische == null)
                return '<label class="px-single"><input type="checkbox" value="' + value + '" class="px te_1_che ' + flg + '" checked="checked"><span class="lbl"></span></label>';
            else
                return '<label class="px-single"><input type="checkbox" value="' + value + '" class="px te_1_che ' + flg + '"><span class="lbl"></span></label>';



        },
        tplThumbnail: function (url) {
            return '<div style="display:block;max-width: 60px;border: 1px solid #eaeaea;margin: 0 auto 0 auto;position: relative;padding-bottom: 60px;height: 0;overflow:hidden;"><img class="popimg" src="' + url + '" style="width: 100%;left:0;padding: 2px;height: 100%;position: absolute;" onerror="imgloaderror(this);" /></div>';
        },
        tplView: function (value, title, funcName) {
            return "<button id=\"View\"  title=" + (title || "查看详细信息") + " type=\"button\"  onclick=\"" + (funcName || "View") + "(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-eye\"></i> </button>";
        },
        tplUpdate: function (value, title) {
            return "<button id=\"Update\"  title=" + (title || "修改数据") + " type=\"button\"  onclick=\"Update(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-pencil\"></i> </button>";
        },
        tplUpdatePart: function (value, title) {
            return "<button id=\"UpdateP\"  title=" + (title || "指派") + " type=\"button\"  onclick=\"Update(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-icon-beaker\"></i> </button>";
        },

        tplAdd: function (value, title, funcName) {
            return "<button id=\"Add\" title=" + (title || "添加数据") + " type=\"button\"  onclick=\"" + (funcName || "Add") + "(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-plus\"></i> </button>";
        },
        //撤销信息
        tplCancel: function (value) {
            return "<button id=\"Cancel\"  title=\"撤销信息\" type=\"button\"  onclick=\"Cancel(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-cancel-icon\"></i> </button>";
        },
        //恢复撤销信息
        tplRecoveryCancel: function (value) {
            return "<button id=\"RecoveryCancel\"  title=\"恢复撤销信息\" type=\"button\"  onclick=\"RecoveryCancel(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-recoverycancel\"></i> </button>";
        },

        tplRemove: function (value, title, funcName) {
            return "<button id=\"Remove\"  title=" + (title || "将数据移动至回收站") + " type=\"button\"  onclick=\"" + (funcName || "Remove") + "(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-trash-o\"></i> </button>";
        },
        //通过
        tplAdopt: function (value,title,funcName) {
            return "<button id=\"Adopt\"  title=" + (title || "通过") + " type=\"button\"  onclick=\"" + (funcName || "Remove") + "(this,'" + value + "',1);\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-key\"></i> </button>";
        },
        //拒绝通过
        tplDisAdopt: function (value, title, funcName) {
            return "<button id=\"DisAdopt\"  title=" + (title || "拒绝") + " type=\"button\"  onclick=\"" + (funcName || "Remove") + "(this,'" + value + "',0);\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-ban\"></i> </button>";
        },
        tplVerify: function (value) {
            return "<button id=\"Verify\"  title=\"审核通过\" type=\"button\"  onclick=\"Verify(this,'" + value + "',1);\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-key\"></i> </button>";
        },
        tplDisVerify: function (value) {
            return "<button id=\"Verify\"  title=\"拒绝审核\" type=\"button\"  onclick=\"Verify(this,'" + value + "',0);\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-life-ring\"></i> </button>";
        },
        tplPublish: function (value) {
            return "<button id=\"Publish\"  title=\"发布\" type=\"button\"  onclick=\"Publish(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-key\"></i> </button>";
        },
        tplReject: function (value) {
            return "<button id=\"Reject\"  title=\"驳回审核的数据\" type=\"button\"  onclick=\"Reject(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-undo\"></i> </button>";
        },
        tplRecovery: function (value, title, funcName) {
            return "<button id=\"Recovery\" title=" + (title || "将回收站中的数据恢复") + " type=\"button\"  onclick=\"" + (funcName || "Recovery") + "(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-reply\"></i> </button>";
        },
        tplDelete: function (value, title, funcName) {
            return "<button id=\"Delete\"  title=" + (title || "将数据从库中彻底抹去") + " type=\"button\"  onclick=\"" + (funcName || "Delete") + "(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-remove\"></i> </button>";
        },
        tplEnable: function (value, title, funcName) {
            return "<button id=\"Enable\"  title=" + (title || "启用数据") + " type=\"button\"  onclick=\"" + (funcName || "Enable") + "(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-check\"></i> </button>";
        },
        tplDisable: function (value, title, funcName) {
            return "<button id=\"Disable\"  title=" + (title || "禁用数据") + " type=\"button\"  onclick=\"" + (funcName || "Disable") + "(this,'" + value + "');\" class=\"btn btn-xs  tn-xs_color btn-padding-right\"><i class=\"fa fa-ban\"></i> </button>";
        },
        //充值  yky 2015-11-9
        tplRecharge: function (value, title) {
            return "<button id=\"Recharge\"   title=\"" + (title || '充值') + "\" type=\"button\"  onclick=\"Recharge(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa  fa-credit-card\"></i> </button>";
        },

        //修改头像  yky 2016-2-22
        tplAvatar: function (value) {
            return "<button id=\"Avatar\"   title=\"头像\" type=\"button\"  onclick=\"Avatar(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-camera\"></i> </button>";
        },

        //修改密码  yky 2016-2-22
        tplPass: function (value) {
            return "<button id=\"Recharge\"   title=\"修改密码\" type=\"button\"  onclick=\"UpdatePass(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-pencil\"></i> </button>";
        },

        //申请修改储值  yky 2016-2-22
        tplChangeRecharge: function (value) {
            return "<button id=\"Recharge\"   title=\"申请修改储值积分\" type=\"button\"  onclick=\"ChangeRecharge(this,'" + value + "');\" class=\"btn btn-xs  btn-padding-right\"><i class=\"fa fa-minus-square\"></i> </button>";
        },

        //评论列表 yky 2015-12-3
        tplComment: function (value) {
            return "<button id=\"Comment\" title=\"获取评论列表\" type=\"button\" onclick=\"Comment(this,'" + value + "');\" class=\"btn btn-xs btn-padding-right\"><i class=\"fa fa-list-ol\"></i></button>";
        },
        //评论列表 yky 2016-1-13
        tplApproval: function (value) {
            return "<button id=\"Approval\" title=\"获取点赞列表\" type=\"button\" onclick=\"Approval(this,'" + value + "');\" class=\"btn btn-xs btn-padding-right\"><i class=\"fa fa-heart\"></i></button>";
        },
        //选择 yky 2016-1-20
        tplChoose: function (value) {
            return "<button  title=\"选择本条数据\" type=\"button\" onclick=\"SetHeader(this,'" + value + "')\" class=\"btn btn-success btn-padding-right\" ><i class=\"fa fa-send\"></i> <span>选择</span></button>";
        },
        tplSearch: function () {
            return "<button id=\"Search\" title=\"按条件搜索数据\" type=\"button\" onclick=\"Search(this);\" class=\"btn btn-primary btn-padding-right\"><i class=\"fa fa-search\"></i> <span>搜索</span></button>";
        },
        tplClear: function () {
            return "<button id=\"Clear\" title=\"重置搜索栏的各项输入\" type=\"button\" onclick=\"Clear(this);\" class=\"btn btn-default btn-padding-right\"><i class=\"fa fa-refresh\"></i> <span>清除</span></button>";
        },

        tplCreate: function () {
            return "<button id=\"Create\" title=\"创建一条新数据\" type=\"button\" onclick=\"Create(this);\" class=\"btn btn-success btn-padding-right\"><i class=\"fa fa-plus\"></i> <span>添加数据</span></button>";
        },
        tplPrint: function () {
            return "<button id=\"Print\" title=\"打印预览\" type=\"button\" onclick=\"Print(this);\" class=\"btn btn-info btn-padding-right\"><i class=\"fa fa-print\"></i> <span>打印预览</span></button>";
        },
        tplBarcode: function () {
            return "<button id=\"Barcode\" title=\"打印商品条码\" type=\"button\" onclick=\"Barcode(this);\" class=\"btn btn-info btn-padding-right\"><i class=\"fa fa-barcode\"></i> <span>条码打印</span></button>";
        },
        tplExport: function () {
            return "<button id=\"Export\" title=\"导出文件\" type=\"button\" onclick=\"Export(this);\" class=\"btn btn-warning btn-padding-right\"><i class=\"fa fa-save\"></i> <span>导出文件</span></button>";
        },
        tplRemoveAll: function () {
            return "<button id=\"RemoveAll\" title=\"将选择的项移至回收站\" onclick=\"RemoveAll(this);\" type=\"button\" class=\"btn btn-danger btn-padding-right\"><i class=\"fa fa-trash-o\"></i> <span>移除所选</span></button>";
        },
        tplRecoveryAll: function () {
            return "<button id=\"RecoveryAll\" title=\"将选择的项恢复至正常列表\" onclick=\"RecoveryAll(this);\" type=\"button\" class=\"btn btn-warning btn-padding-right\"><i class=\"fa fa-reply\"></i> <span>恢复所选</span></button>";
        },
        tplDeleteAll: function () {
            return "<button id=\"DeleteAll\" title=\"将选择的项从数据库彻底删除\" onclick=\"DeleteAll(this);\" type=\"button\" class=\"btn btn-danger btn-padding-right\"><i class=\"fa fa-remove\"></i> <span>删除所选</span></button>";
        },
        tplReset: function (value) {
            return "<button id=\"Reset\" title=\"重置已选商品\" onclick=\"Reset(this,'" + value + "');\" type=\"button\" class=\"btn btn-danger btn-padding-right\"><i class=\"fa icon-retweet\"></i> </button>";
        },

    };

    $.whiskey.ajaxLoading = function (options, wrapper) {
        var loading = $.Loading(options, wrapper);
        !options.hideLoading ? loading.show() : {};
        var complete = options.complete;
        options.complete = function (httpRequest, status) {
            !options.hideLoading ? loading.dispose() : {};
            if (complete) {
                complete(httpRequest, status);
            }
        };
        options.error = function (XMLHttpRequest, textStatus, errorThrown) {
            $.whiskey.web.alert({
                type: "danger",
                content: "服务器请求发生错误：" + errorThrown + "，错误代码：" + XMLHttpRequest.status,
                callback: function () {
                }
            });
        };
        options.async = true;
        $.ajax(options);
    };

    $.whiskey.web = {
        init: function () {

            $('.switcher').switcher({
                //theme: 'square',
                on_state_content: "展开搜索",
                off_state_content: "隐藏搜索"
            }).on("click", function () {
                var panel_body = $(this).parents('.panel-heading').siblings(".panel-body");
                if (panel_body.is(":hidden")) {
                    panel_body.slideDown('fast');
                } else {
                    panel_body.slideUp('fast');
                }
                //$(".panel-body").toggle("slow");
            });
            //yxk
            $(".panel-search #Search").on("click", function () {
                if ($(".panel-body").is(":hidden")) {
                    $(".panel-body").slideDown("fast");
                    return false;
                }
            });
            $(".panel-body").hide();
            $('.trusher').switcher({
                //theme: 'square',
                on_state_content: "未删除",
                off_state_content: "已删除"
            }).on("click", function () {
                $.whiskey.datatable.reset(false);
            });
            $('.verifier').switcher({
                //theme: 'square',
                on_state_content: "未审核",
                off_state_content: "已审核"
            }).on("click", function () {
                $.whiskey.datatable.reset(false);
            });
            $('.publisher').switcher({
                //theme: 'square',
                on_state_content: "未发布",
                off_state_content: "已发布"
            }).on("click", function () {
                $.whiskey.datatable.reset(false);
            });
            $('.enabler').switcher({
                //theme: 'square',
                on_state_content: "已启用",
                off_state_content: "未启用"
            }).on("click", function () {
                $.whiskey.datatable.reset(false);
            });
            $('.yearandmonth').switcher({
                //theme: 'square',
                on_state_content: "周",
                off_state_content: "年"
            }).on("click", function () {

            });
            $('.saleReturn').switcher({
                //theme: 'square',
                on_state_content: "售",
                off_state_content: "退"
            }).on("click", function () {

            });
            $(".input-daterange").datepicker();
            $(".input-daterange_sale").datepicker({
                autoclose: true,
                pickerPosition: "bottom-left"
            });
            //fsf  LAYER.TIP
            $("#main-wrapper [tipContent]").attr("tipDirection", function (i, val) {
                if (val) {
                    $(this).removeAttr("tipDirection").attr("data-placement", val);
                }
            }).attr("tipContent", function (i, val) {
                $(this).removeAttr("tipContent").attr("data-original-title", val);
            }).tooltip({ container: "#main-wrapper" });
            $("#main-wrapper .panel-search:has(.panel-footer #Search)").find(".panel-body :text").unbind("keyup.search").bind("keyup.search", function (e) {//:visible:not(:disabled)
                if (e.keyCode == 13) {
                    $(this).closest(".panel-search").find(".panel-footer #Search").click();
                }
            });
            $("body").on("mouseover", ".popimg", function () {
                if (this.tagName == "IMG") {
                    var strImg = "<img src='" + this.src + "' style='width:200px;'>";
                    toolTip(strImg);
                } else {
                    $(this).find("img").each(function () {
                        var strImg = "<img src='" + this.src + "' style='width:200px;'>";
                        toolTip(strImg);
                    });
                }
            }).on("mouseout", function () {
                $("body").find("#toolTipLayer").hide();
            });
            //悬浮图片
            window.onImgMouseOver = function (_this) {
                var _this = _this;
                if (_this.tagName == "IMG") {
                    var strImg = "<img src='" + _this.src + "' style='width:200px;'>";
                    toolTip(strImg)
                } else {
                    $(_this).find("img").each(function () {
                        var strImg = "<img src='" + _this.src + "' style='width:200px;'>";
                        toolTip(strImg);
                    });
                }
            };
            window.onImgMouseOut = function (_this) {
                $("body").find("#toolTipLayer").hide();
            }

            window.suspension = function (option) {
                var _defaultOption = {
                    imgPath: 'imgPath', //图片路径
                    classImg: true, //是否悬浮
                    shape: "square", //是方形
                    host:'https://www.0-fashion.com/'
                };
                option = $.extend(_defaultOption, option);
                //传classImg  表示图片不悬浮
                //	var imgPath=option.imgPath https://www.0-fashion.com/
                var imgUrl = option.imgPath;
                if (imgUrl.indexOf('http') === -1) {
                    imgUrl = option.host + imgUrl
                }
                var str = "";
                var classImg = option.classImg;
                var shape = option.shape;
                var square = option.square;
                if (classImg) {
                    // 悬浮
                    str += '<div class="thumbnail_outer_img" ><div  class="thumbnail_inner_img" ><div class="thumbnail_current_img" ><img  class="suspension_img" onerror="imgloaderror(this);"  src="' + imgUrl + '" onmouseover="onImgMouseOver(this)" onmouseout="onImgMouseOut(this)"> </div></div></div>';
                } else {
                    //不悬浮  
                    str += '<div class="thumbnail_outer_img"><div  class="thumbnail_inner_img" ><div class="thumbnail_current_img" ><img class="suspension_img"  onerror="imgloaderror(this);" src="' + imgUrl + '"> </div></div></div>';
                }
                if (shape !== "circle") { //圆形
                    return str;
                }
                str = $(str).css("border", "0").find(".suspension_img").css("border-radius", "50%").css("width", "45px");;
                return str[0].outerHTML;
            }

            // $.whiskey.web.speak("欢迎光临零时尚ERP管理平台！");
        },
        speak: function (text) {
            var print = $.whiskey.printer.getLodop();
            print.FORMAT("VOICE:0;50", text);
        },
        formVerify: function (formName) {
            $(formName).removeData('validator');
            $(formName).removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(formName);
        },
        formData: function (wrapper, wrapper2, extCallback) {
            var self = this;
            if ($.isFunction(wrapper)) {
                extCallback = wrapper;
                wrapper = null;
            } else if ($.isFunction(wrapper2)) {
                extCallback = wrapper2;
                wrapper2 = null;
            }

            var $wrapper = $(wrapper || ".form-search").find("[name]:not(.notmap)");
            var $wrapper2 = $(wrapper2 || ".panel-list");

            ; self.conditions = function () {
                var conditions = new $.whiskey.filter.group();
                var startDate = $wrapper.filter(".start-date").val();
                var endDate = $wrapper.filter(".end-date").val();

                if (startDate && startDate.length > 0) {
                    conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", startDate + " 00:00:01", "greater"));
                }
                if (endDate && endDate.length > 0) {
                    conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", endDate + " 23:59:59", "less"));
                }

                var isdeleted = $wrapper2.find(".trusher").length > 0 && !$wrapper2.find(".trusher").is(":checked"); conditions.Rules.push(new $.whiskey.filter.rule("IsDeleted", isdeleted, "equal"));
                var isenabled = $wrapper2.find(".enabler").length == 0 || $wrapper2.find(".enabler").is(":checked"); conditions.Rules.push(new $.whiskey.filter.rule("IsEnabled", isenabled, "equal"));

                $wrapper.filter("input[name][name!='StartDate'][name!='EndDate'],select[name]").each(function () {
                    var $item = $(this);
                    var field = $item.attr("name");
                    var value = $item.val();
                    if (typeof value == "string") value = $.trim(value);
                    if (value != null && value.length > 0) {
                        conditions.Rules.push(new $.whiskey.filter.rule(field, value, $item.is("select") ? "equal" : "contains"));
                    }
                });

                if ($.isFunction(extCallback)) extCallback(conditions);

                return conditions;
            }
            ; self.getJSON = function () {
                return JSON.stringify(self.conditions());
            }
            ; self.get = function () {
                return { name: "conditions", value: self.getJSON() };
            }
            return self;
        },
        clearForm: function (formName) {
            $(formName).find("input").not(':button, :submit, :reset').val('').removeAttr('checked');
            $(formName).find("select:not(.selectpicker)").prop("selectedIndex", 0);
            try{
                $(formName).find("select.selectpicker").prop("selectedIndex", 0).selectpicker('refresh')
            }catch(e){}
        },
        getIdByChecked: function (wrapperName) {
            var arrays = [];
            $(wrapperName).each(function () {
                if (this.checked == true) {
                    arrays.push({ name: "Id", value: $(this).val() })
                }
            });
            return arrays;
        },
        ajaxDialog: function (options) {
            var _id = guid();
            $.whiskey.ajaxLoading({
                // cache: false,
                type: "get",
                url: options.actionUrl,// + "?r=" + Math.random()
                data: options.getParams,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                    if (typeof (options.beforeSend) == "function") {
                        var res = options.beforeSend(); //在请求之前发生，如果方法返回为false ,请求终止，yxk
                        if (res != undefined && res == false) return false;
                    }

                },
                complete: function (result) {
                    $(options.lockButton).removeAttr("disabled");
                    //yxk
                    if (typeof (options.complete) == "function") {
                        options.complete();
                    }
                    //if (typeof (options.postComplete) == "function") {
                    //    options.postComplete();
                    //}
                },
                success: function (data) {

                    var formHeader = "<form id='" + _id + "' 0fashion='fashion-team' class=\"modal-form form-horizontal dropzone\" action=\"" + options.actionUrl + "\" enctype=\"multipart\/form-data\">";

                    var formBody = data;
                    var formFooter = "</form>";

                    var mesg = formHeader + formBody + formFooter;
                    if (options.noneheader)
                        mesg = formBody;
                    if (typeof (options.getComplete) == "function") {
                        options.getComplete();
                    }
                    var succe_but_tit = options.successTit;
                    if (succe_but_tit == undefined) {
                        succe_but_tit = "提交";
                    }
                    var succe_but_event = options.successEvent;
                    if (typeof (succe_but_event) != "function") {
                        succe_but_event = success_event;
                    }

                    var button = {
                        success: {
                            label: succe_but_tit,
                            icon: "fa-check",
                            className: "btn-primary",
                            callback: function () {
                                return succe_but_event(options);
                            }
                        },
                        cancel: {
                            label: "关闭",
                            icon: "fa-close",
                            className: "btn-default",
                            callback: function () {
                                if (typeof (options.closeEvent) == "function") {
                                    return options.closeEvent();
                                }
                            }
                        }
                    };
                    if (options.button) {
                        button = $.extend(options.button, button);
                    }

                    options.formModel = bootbox.dialog({
                        message: mesg,
                        // message:formBody,
                        className: options.diacl,
                        title: options.caption,
                        buttons: button,
                        onEscape: options.exit,
                    });
                 
                }
            });
        },
        ajaxConfirm: function (options) {
            var title = options.question.length > 0 ? "<div class='text-danger text-center'><h3>" + options.question + "</h3></div>" : "";
            var description = options.notes.length > 0 ? "<br /><div class='text-center'>" + options.notes + "</div><br />" : "";
            bootbox.dialog({
                message: title + description,
                //yxk 
                closeButton: options.showclosebut,
                onEscape: options.exit,
                animate: true,
                buttons: {
                    success: {
                        label: "确认",
                        icon: "fa-check",
                        className: "btn btn-primary",
                        callback: function () {
                            if (options.success_event != undefined)
                                options.success_event();
                            else {
                                //如果没有传入自定义方法 -yxk：
                                $.whiskey.ajaxLoading({
                                    type: "POST",
                                    url: options.actionUrl,// + "?r=" + Math.random()
                                    data: options.params,
                                    beforeSend: function () {
                                        $(options.lockButton).attr("disabled", true);
                                        $(".modal-footer .btn-primary").attr("disabled", "disabled");
                                    },
                                    complete: function (data) {
                                        $(".modal-footer .btn-primary").removeAttr("disabled");
                                    },
                                    success: function (data) {
                                        $(options.lockButton).removeAttr("disabled");
                                        if (options.success != undefined && typeof (options.success) == "function")
                                            options.success(data);
                                        else {
                                            if (typeof (data) == 'object') {
                                                if (data.ResultType == 3) {
                                                    if (typeof (options.complete) == "function") {
                                                        options.complete(data);
                                                    }
                                                    if (options.showCompletePrompt) {
                                                        $.whiskey.web.alert({
                                                            type: "success",
                                                            content: data.Message,
                                                            callback: function () {
                                                                if (options.returnUrl != undefined) {
                                                                    window.location = options.returnUrl;
                                                                } else {
                                                                    bootbox.hideAll();
                                                                }
                                                            }
                                                        });
                                                    }
                                                    return true;
                                                } else {
                                                    $.whiskey.web.alert({
                                                        type: "danger",
                                                        content: data.Message,
                                                        callback: function () {
                                                        }
                                                    });
                                                }
                                            }
                                            else {
                                                $.whiskey.web.alert({
                                                    type: "info",
                                                    content: data,
                                                    callback: function () {
                                                    }
                                                });
                                            }
                                        }
                                    }
                                });
                            }
                        }
                    },
                    cancel: {
                        label: "取消",
                        icon: "fa-cancel",
                        className: "btn btn-default",
                        callback: function () {//yxk
                            $(options.lockButton).removeAttr("disabled");
                            if (options.cancel_event != undefined) {
                                options.cancel_event();
                            }
                        }
                    }
                }

            });

        },
        ajaxView: function (options) {
            $.whiskey.ajaxLoading({
                type: options.type || "GET",
                url: options.actionUrl,// + "?r=" + Math.random()
                data: options.params,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                },
                complete: function (result) {
                    $(options.lockButton).removeAttr("disabled");
                },
                success: function (data) {
                    if (typeof (options.complete) == "function") {
                        options.complete(data);
                    }
                    var button;
                    if (typeof (options.buttonType) == "undefined" || options.buttonType == 0) {
                        button = {
                            cancel: {
                                label: "关闭",
                                icon: "fa-close",
                                className: "btn-default",
                                callback: function () {
                                    if (typeof (options.close) == "function") {
                                        options.close(data);
                                    }
                                }
                            }
                        };
                    } else {
                        button = {
                            success: {
                                label: "提交",
                                icon: "fa-check",
                                className: "btn-primary",
                                callback: function () {
                                    if (typeof (options.submit) == "function") {
                                        options.submit(data);
                                    }
                                }
                            },
                            cancel: {
                                label: "关闭",
                                icon: "fa-close",
                                className: "btn-default",
                                callback: function () {
                                    if (typeof (options.close) == "function") {
                                        options.close(data);
                                    }
                                }
                            }
                        };
                    }
                    if (options.button) {
                        button = $.extend(options.button, button);
                    }
                    bootbox.dialog({
                        message: data,
                        title: options.caption,
                        className: options.className,
                        buttons: button,
                    });
                    return true;
                }
            });
        },
        ajaxRequest: function (options) {
            $.whiskey.ajaxLoading({
                type: options.method != "undefined" ? options.method : "GET",
                url: options.actionUrl + "?r=" + Math.random(),
                data: options.params,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                    if (typeof (options.beforeSend) == "function") {
                        return options.beforeSend();
                    }
                },
                complete: function (result) {
                    $(options.lockButton).removeAttr("disabled");
                },
                hideLoading:options.hideLoading,
                success: function (data) {
                    if (options.showPrompt) {
                        if (typeof (data) == 'object') {
                            if (data.ResultType == 3) {
                                if (typeof (options.complete) == "function") {
                                    options.complete(data);
                                }
                            } else {
                                $.whiskey.web.alert({
                                    type: "danger",
                                    content: data.Message,
                                    callback: function () {
                                    }
                                });
                            }
                        }
                        else {
                            $.whiskey.web.alert({
                                type: "info",
                                content: data,
                                callback: function () {
                                }
                            });
                        }
                    } else {
                        if (typeof (options.complete) == "function") {
                            options.complete(data);
                        }
                    }
                }
            });
        },
        alert: function (options) {
            var modal;
            switch (options.type) {
                case "success":
                    modal = $("#modals-alerts-success").clone(true);
                    break;
                case "danger":
                    modal = $("#modals-alerts-danger").clone(true);
                    break;
                case "warning":
                    modal = $("#modals-alerts-warning").clone(true);
                    break;
                case "info":
                    modal = $("#modals-alerts-info").clone(true);
                    break;
                case "confirm":
                    modal = $("#modals-alerts-confirm").clone(true);
                    break;
                default:
                    modal = $("#modals-alerts-info").clone(true);
                    break;
            }

            $(modal).find(".modal-body").html("<h4 style='line-height:35px;'>" + options.content + "</h4>");
            if (options.ismodal == true) {
                $(modal).modal({ backdrop: 'static', keyboard: false });
            }
            $(modal).find(".btn").on("click", function () {
                if (typeof (options.callback) == "function") {
                    options.callback();
                }
            });
            //yxk 2015-11
            if (options.par != undefined && options.par != null && options.par != "") {
                $(modal).modal(options.par);
            }
            else
                $(modal).modal("show");
        },
        confirm: function (options) {
            var question = options.question.length > 0 ? "<div class='text-danger text-center'><h3>" + options.question + "</h3></div>" : "";
            var notes = options.notes.length > 0 ? "<br /><div class='text-center'>" + options.notes + "</div><br />" : "";
            bootbox.dialog({
                message: question + notes,
                className: options.className || "cls_configm",
                buttons: {
                    success: {
                        label: "确认",
                        icon: "fa-check",
                        className: "btn btn-primary",
                        callback: function () {
                            if (typeof (options.ok) == "function") {
                                options.ok();
                            }
                        }
                    },
                    cancel: {
                        label: "取消",
                        icon: "fa-remove",
                        className: "btn btn-default",
                        callback: function () {
                            if (typeof (options.cancel) == "function") {
                                options.cancel();
                            }
                        }
                    }
                }
            });
            return false;
        },
        tooltip: function (options) {
            var modal;
            switch (options.type) {
                case "success":
                    modal = $("#modals-alerts-success").clone(true);
                    break;
                case "danger":
                    modal = $("#modals-alerts-danger").clone(true);
                    break;
                case "warning":
                    modal = $("#modals-alerts-warning").clone(true);
                    break;
                case "info":
                    modal = $("#modals-alerts-info").clone(true);
                    break;
                case "confirm":
                    modal = $("#modals-alerts-confirm").clone(true);
                    break;
                default:
                    modal = $("#modals-alerts-info").clone(true);
                    break;
            }

            $(modal).find(".modal-body").html("<h4 style='line-height:35px;'>" + options.content + "</h4>");

            $(modal).find(".btn").on("click", function () {
                if (typeof (options.callback) == "function") {
                    options.callback();
                }
            });

            $(modal).modal("show");
        },
        load: function (options) {
            var loading = $.Loading();
            loading.show();
            $(options.wrapper || "#content-wrapper").load(options.url, options.data, function (da) {
                history.pushState(history.state, "", options.url);
                loading.dispose(true);
            });
        },
        updateBadge: function (changeCount, badgeTag, autoCalc) {
            if (badgeTag) {
                var $this = $("." + badgeTag);
                var oldVal = $this.text(); oldVal = (oldVal && !isNaN(oldVal)) ? Number(oldVal) : 0;
                if (autoCalc) { changeCount = oldVal + changeCount };
                var curtype = changeCount > 0;
                $this.text(changeCount);
                curtype ? $this.show() : $this.hide();
                var $li = $this.closest("li.has-sub");
                var hasBadge = $li.find(".dropdown-menu a span.badge").filter(function (ind, item) {
                    var val = $(this).text();
                    return val && !isNaN(val) && Number(val) > 0;
                }).length > 0;
                hasBadge ? $li.find(".tip_count").css("display", "inline-block") : $li.find(".tip_count").hide();
            }
        }
    };

    $.whiskey.barcode2 = function () {
        this.count = 0;
        this.data = {};
        this.margin = 16;
        this.fontSize = 30;
        this.init = function () {
            ArgoxPrinter.A_EnumUSB();
            ArgoxPrinter.A_CreateUSBPort(1);
            ArgoxPrinter.A_Set_Unit("n");
            ArgoxPrinter.A_Set_Syssetting(2, 0, 0, 0, 2);
            ArgoxPrinter.A_Set_Backfeed(320);
            ArgoxPrinter.A_Set_Darkness(8);
            ArgoxPrinter.A_Set_Speed("E");
            ArgoxPrinter.A_Set_LabelVer(57);
        }
        this.close = function () {
            ArgoxPrinter.A_ClosePrn();
        }
        this.printGoods = function () {
            for (i = 0; i < this.data.length; i++) {
                var item = this.data[i];
                this.count++;
                if (this.count % 2 == 1) {
                    this.margin = 16;
                } else {
                    this.margin = 220;
                }
                ArgoxPrinter.A_Prn_Text_TrueType(this.margin, 5, this.fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A1", item.ProductNumber, 1);
                ArgoxPrinter.A_Prn_Barcode(this.margin, 20, 1, 'o', 2, 1, 20, 'N', 1, item.ProductNumber);
                ArgoxPrinter.A_Prn_Text_TrueType(this.margin, 45, this.fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A2", "尺码：" + $.whiskey.tools.fixedLength(item.SizeName, 10) + "折扣：" + $.whiskey.tools.fixedLength(item.DiscountName, 10), 1);
                ArgoxPrinter.A_Prn_Text_TrueType(this.margin, 60, this.fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A3", "颜色：" + $.whiskey.tools.fixedLength(item.ColorName, 10) + "品牌：" + $.whiskey.tools.fixedLength(item.BrandName, 10), 1);
                ArgoxPrinter.A_Prn_Text_TrueType(this.margin, 75, this.fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A4", "价格：￥" + item.TagPrice, 1);
                ArgoxPrinter.A_Prn_Text_TrueType(this.margin, 90, this.fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A5", "名称：" + item.ProductName, 1);
                if (this.count % 2 == 0) {
                    ArgoxPrinter.A_Print_Out(1, 1, 1, 1);
                }
            }
            if (this.data.length % 2 == 1) {
                ArgoxPrinter.A_Print_Out(1, 1, 1, 1);
            }
        }
        this.printPosition = function () {

        }
        this.status = function (handler) {
            var ret = ArgoxPrinter.A_getPrinterStatus();
            var result;
            switch (ret) {
                case 0:
                    result = "无返回值";
                    break;
                case 1:
                    result = "打印机命令解析器忙碌中！";
                    break;
                case 2:
                    result = "纸张用完或安装错误！";
                    break;
                case 4:
                    result = "碳带用完或安装错误！";
                    break;
                case 8:
                    result = "打印批次文档中！";
                    break;
                case 9:
                    result = "打印机待机中！";
                    break;
                case 16:
                    result = "正在打印文件！";
                    break;
                case 32:
                    result = "打印机暂停！";
                    break;
                case 64:
                    result = "正在送出标签纸！";
                    break;
            }
            if (typeof (handler) != "undefined") {
                $(handler).html(result);
            }
        }
    };
    //var list = new Array();
    //list.push("2012211540101,测试衣服一,2780.00,黄色,兰蔻,M-,二折");
    //list.push("1012211540101,测试衣服二,1780.00,红色,香奈儿,XL+,四折");

    $.whiskey.barcoder = {
        print: function (options) {
            if (typeof (whiskey) == "object") {
                var result = whiskey.barcode(options.type, options.barcodes);
                if (result != 0) {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: "USB端口读取失败！请检查是否正确连接打印机！",
                        callback: function () {
                        }
                    });
                }
            } else {
                $.whiskey.web.alert({
                    type: "info",
                    content: "必须通过零时尚ERP管理平台配套浏览器来打印条码！",
                    callback: function () {
                    }
                });
            }
        },
    };

    $.whiskey.printer = {
        instance: null,
        checkInstall: function () {
            var result = false;
            try {
                var LODOP = $.whiskey.printer.getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
                if ((LODOP != null) && (typeof (LODOP.VERSION) != "undefined")) {
                    result = true;
                } else {
                    result = false;
                }
            } catch (err) {
                result = false;
            }
            return result;
        },
        getLodop: function (oOBJECT, oEMBED) {
            var strHtmInstall = "打印控件未安装!<a href='/Content/Softs/install_lodop32.exe' target='_self' style='color:red'>【点击这里执行安装】</a>,安装后请刷新页面或重新进入。";
            var strHtmUpdate = "打印控件需要升级!<a href='/Content/Softs/install_lodop32.exe' target='_self' style='color:red'>【点击这里执行升级】</a>,升级后请重新进入。";
            var strHtm64_Install = "打印控件未安装!<a href='/Content/Softs/install_lodop64.exe' target='_self' style='color:red'>【点击这里执行安装】</a>,安装后请刷新页面或重新进入。";
            var strHtm64_Update = "打印控件需要升级!<a href='/Content/Softs/install_lodop64.exe' target='_self' style='color:red'>【点击这里执行升级】</a>,升级后请重新进入。";
            var strHtmFireFox = "<br /><br />（注意：若曾安装过此插件,请在【工具】->【附加组件】->【扩展】中卸载）<br />";
            var strHtmChrome = "<br /><br />(注意：若曾安装过此插件，浏览器升级或重装后出问题也请重新安装）<br />";
            var LODOP;
            try {
                //=====判断浏览器类型:===============
                var isIE = (navigator.userAgent.indexOf('MSIE') >= 0) || (navigator.userAgent.indexOf('Trident') >= 0);
                var is64IE = isIE && (navigator.userAgent.indexOf('x64') >= 0);
                //=====如果页面有Lodop就直接使用，没有则新建:==========
                if (oOBJECT != undefined || oEMBED != undefined) {
                    if (isIE)
                        LODOP = oOBJECT;
                    else
                        LODOP = oEMBED;
                } else {
                    if ($.whiskey.printer.instance == null) {
                        LODOP = document.createElement("object");
                        LODOP.setAttribute("width", 0);
                        LODOP.setAttribute("height", 0);
                        LODOP.setAttribute("style", "position:absolute;left:0px;top:-100px;width:0px;height:0px;");
                        if (isIE) LODOP.setAttribute("classid", "clsid:2105C259-1E0C-4534-8141-A753534CB4CA");
                        else LODOP.setAttribute("type", "application/x-print-lodop");
                        document.documentElement.appendChild(LODOP);
                        $.whiskey.printer.instance = LODOP;
                    } else
                        LODOP = $.whiskey.printer.instance;
                };
                //=====判断Lodop插件是否安装过，没有安装或版本过低就提示下载安装:==========
                if ((LODOP == null) || (typeof (LODOP.VERSION) == "undefined")) {

                    var errors = "";
                    if (is64IE) errors += strHtm64_Install; else
                        if (isIE) errors += strHtmInstall; else
                            errors += strHtmInstall;
                    if (navigator.userAgent.indexOf('Chrome') >= 0)
                        errors += strHtmChrome;
                    if (navigator.userAgent.indexOf('Firefox') >= 0)
                        errors += strHtmFireFox;

                    $.whiskey.web.alert({
                        type: "danger",
                        content: errors,
                        callback: function () {
                        }
                    });

                    return LODOP;
                }
                //else
                //    if (LODOP.VERSION < "6.1.9.5") {
                //        if (is64IE) document.write(strHtm64_Update); else
                //            if (isIE) document.write(strHtmUpdate); else
                //$.whiskey.web.alert({
                //    type: "info",
                //    content: strHtmUpdate,
                //    callback: function () {
                //    }
                //});
                //        return LODOP;
                //    };
                //=====如下空白位置适合调用统一功能(如注册码、语言选择等):====	     

                LODOP.SET_LICENSES("北京紫枫科技开发有限公司", "653726269717472919278901905623", "", "");
                //============================================================	     
                return LODOP;
            } catch (err) {
                if (is64IE) {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: strHtm64_Install,
                        callback: function () {
                        }
                    });
                } else {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: strHtmInstall,
                        callback: function () {
                        }
                    });
                }
                return LODOP;
            };
        },
        ajaxPreview: function (options) {
            if ($.whiskey.printer.checkInstall()) {
                $.whiskey.ajaxLoading({
                    type: "GET",
                    url: options.actionUrl,// + "?r=" + Math.random()
                    data: options.params,
                    beforeSend: function () {
                        $(options.lockButton).attr("disabled", "disabled");
                    },
                    complete: function (result) {
                        $(options.lockButton).removeAttr("disabled");
                    },
                    success: function (data) {
                        if (typeof (data) == "object") {

                            if (data.ResultType != undefined) {
                                if (data.ResultType != 3) {
                                    $.whiskey.web.alert({
                                        type: "danger",
                                        content: data.Message,
                                        callback: function () {
                                        }
                                    });
                                }
                            } else {
                                var print = $.whiskey.printer.getLodop();
                                print.SET_PREVIEW_WINDOW(1, 0, 0, "830", "700", "立即打印");
                                print.SET_SHOW_MODE("HIDE_PAPER_BOARD", 1);
                                print.SET_PRINT_PAGESIZE(1, 0, 0, "A4");
                                print.SET_PRINT_MODE("AUTO_CLOSE_PREWINDOW", 1);
                                //print.SET_SHOW_MODE("PREVIEW_NO_MINIMIZE", true);
                                print.ADD_PRINT_HTM(options.topMargin, options.leftMargin, options.contentWidth, options.contentHeight, data.html);
                                print.SET_PRINT_STYLEA(0, "HOrient", 3);
                                print.SET_PRINT_STYLEA(0, "VOrient", 3);
                                print.PREVIEW();
                            }

                        } else {
                            $.whiskey.web.alert({
                                type: "info",
                                content: "打印失败，Ajax返回类型不是一个对象！",
                                callback: function () {
                                }
                            });
                        }
                    }
                });
            }
        }
    };

    $.whiskey.exporter = {
        ajaxExport: function (options) {
            options.version = options.version || 1;
            if (options.version == 1) {
                options.filename=options.filename||"导出文件";
                $.whiskey.ajaxLoading({
                    type: "GET",
                    url: options.actionUrl,// + "?r=" + Math.random()
                    data: options.params,
                    dataType:"text",
                    traditional: true,
                    beforeSend: function () {
                        $(options.lockButton).attr("disabled", "disabled");
                    },
                    complete: function (result) {
                        $(options.lockButton).removeAttr("disabled");
                    },
                    success: function (data) {
                        if (typeof (data) == "object") {
                            if (data.ResultType != undefined) {
                                if (data.ResultType != 3) {
                                    $.whiskey.web.alert({
                                        type: "danger",
                                        content: data.Message,
                                        callback: function () {
                                        }
                                    });
                                }
                            } else {
                                $("<div />").html(data.html).find("table").table2excel({
                                    name: options.filename,
                                    filename: options.fileName + $.whiskey.tools.dateFormat(new Date(), ".yyyy.MM.dd.hh.mm.ss") + ".xls",
                                    exclude_img: false,
                                    exclude_links: false,
                                });
                            }

                        } else if (typeof (data) == "string") {
                            try {
                                var data = JSON.parse(data);
                                if (data.hasOwnProperty("html")) {
                                    data = data.html;
                                }
                            } catch (ex) { }
                            $("<div />").html(data).find("table").table2excel({
                                name: options.filename,
                                filename: options.fileName + $.whiskey.tools.dateFormat(new Date(), ".yyyy.MM.dd.hh.mm.ss") + ".xls",
                                exclude_img: false,
                                exclude_links: false,
                            });
                        } else {
                            $.whiskey.web.alert({
                                type: "info",
                                content: "导出失败，Ajax返回类型暂不支持！",
                                callback: function () {
                                }
                            });
                        }
                        return true;
                    }
                });
            } else {
                var loading = $.Loading();
                $.fileDownload(options.actionUrl, {
                    data: options.params,
                    httpMethod: "GET",
                    successCallback: function (url) {
                        $(options.lockButton).prop("disabled", false);
                        loading.dispose();
                    },
                    abortCallback: function () {
                        loading.dispose();
                        $(options.lockButton).prop("disabled", false);
                        $.whiskey.web.alert({
                            type: "warning",
                            content: "导出被中断【失败】",
                            callback: function () {
                            }
                        });
                    },
                    failCallback: function (responseHtml, url, error) {
                        loading.dispose();
                        $(options.lockButton).prop("disabled", false);
                        $.whiskey.web.alert({
                            type: "danger",
                            content: error || "导出失败",
                            callback: function () {
                            }
                        });
                    },
                    prepareCallback: function (url) {
                        $(options.lockButton).prop("disabled", true);
                        loading.show();
                    }
                });
            }
        }
    };

})(jQuery);

function success_event(options) {
    $.whiskey.web.formVerify(".modal-form");
    if (typeof (options.formValidator) == "function") {
        if (!options.formValidator()) {
            return false;
        }
    }
    $.whiskey.ajaxLoading({
        type: "POST",
        url: options.actionUrl,
        data: $(".modal-form").serialize() + "&" + JSON.stringify(options.postParams),
        hideLoading: options.hideLoading,
        beforeSend: function () {
            $(".modal-footer .btn-primary").attr("disabled", "disabled");
        },
        complete: function (data) {
            $(".modal-footer .btn-primary").removeAttr("disabled");
        },
        success: function (data) {
            if (typeof (data) == 'object') {
                if (data.ResultType == 3) {
                    if (typeof (options.postComplete) == "function") {
                        if (options.postComplete(data)) {
                            options.formModel.modal('hide');
                        }
                    } else {
                        options.formModel.modal('hide');
                    }
                    if (options.showCompletePrompt) {
                        $.whiskey.web.alert({
                            type: "success",
                            content: data.Message,
                            callback: function () {
                                if (options.returnUrl != undefined) {
                                    window.location = options.returnUrl;
                                } else {
                                    bootbox.hideAll();
                                }
                            }
                        });

                    }

                    return true;
                } else {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: data.Message,
                        callback: function () {
                        }
                    });
                }
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: data,
                    callback: function () {
                    }
                });
            }
        }
    });
    return false;
}

$(document).ready(function () {
    $.whiskey.web.init();
});
function guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

(function (win) {
    win.OperationResultType = {
        ValidError: 0,
        QueryNull: 1,
        NoChanged: 2,
        Success: 3,
        Error: 4,
        NameRepeat: 5,
        LoginError: 6,
        DataRepeat: 7
    };
})(window);

(function ($) {
    /*全局loading fsf 2016-11-24 */
    function Loading(options, wrapper) {
        //->获取当前屏幕的宽度和高度
        var winW = document.documentElement.clientWidth || document.body.clientWidth,
            winH = document.documentElement.clientHeight || document.body.clientHeight;
        //->获取盒子的宽度和高度
        options = options || {};
        var self = this;
        var img = $($.Loading.loadimg || "<div style=\"background-image:url('/content/images/loading_new.gif');padding:3px;\"><img id=\"progressImgage\"  src=\"/content/images/ajax_loader.gif\" /></div>");
        img.addClass("loadingdivnano");
        var mask = $("<div id=\"maskOfProgressImage\"></div>").addClass("mask").hide();
        var PositionStyle = "fixed";
        if (wrapper != null && wrapper != "" && wrapper != undefined) {
            $(wrapper).css("position", "relative").append(img).append(mask);
            PositionStyle = "absolute";
        }
        else {
            $("body").append(img).append(mask);
            var winW = document.documentElement.clientWidth || document.body.clientWidth,
            winH = document.documentElement.clientHeight || document.body.clientHeight;
            var boxW = img.width();
            var boxH = img.height();
            img.css({
                "position ": "absolute",
                "left": (winW - boxW) / 2 + "px",
                "top": (winH - boxH) / 2 + "px",
                "z-index": "9999",
                "display": "none"
            });

        }

        mask.css({
            "position": PositionStyle,
            "top": "0",
            "right": "0",
            "bottom": "0",
            "left": "0",
            "z-index": "8888",
            "display": "none",

        });
        var hideEvent = options.hideEvent;

        self.hide = function () {
            img.hide();
            mask.hide();
            if (typeof (hideEvent) == 'function') {
                hideEvent();
            }
            blurry(false);
        }

        var showEvent = options.showEvent;

        self.show = function () {
            blurry(true);
            var winW = document.documentElement.clientWidth || document.body.clientWidth,
            winH = document.documentElement.clientHeight || document.body.clientHeight;
            var boxW = img.width();
            var boxH = img.height();
            img.show().css({
                "position": PositionStyle,
                "position ": "absolute",
                "left": (winW - boxW) / 2 + "px",
                "top": (winH - boxH) / 2 + "px"
                //              "position": PositionStyle,
                //              "top": "40%",
                //              "left": "50%",
                //              "margin-top": function () { return -1 * img.height() / 2; },
                //              "margin-left": function () { return -1 * img.width() / 2; }
            });
            mask.show();
            if (typeof (showEvent) == 'function') {
                showEvent();
            }
        }

        self.dispose = function (clearAll) {
            if (clearAll != true) {
                img.remove();
                mask.remove();
            } else {
                $("#maskOfProgressImage,.loadingdivnano").remove();
            }
            blurry(false);
        }

        function blurry(enable) {
            var $wrapper = $('#sidebar,#main-wrapper');
            enable ? $wrapper.addClass('add_filterBlur') : $wrapper.removeClass('add_filterBlur');
        }
    };

    //队列请求
    function queueRequest() {
        var me = this;
        var _arr = [];
        var _ind = 0;
        function run() {
            var info = _arr[_ind++];
            if (info) {
                $.ajax({
                    url: info.url, data: info.data,type:"POST", complete: function () { run(); }, success: function (res) {
                        if ($.isFunction(info.callback)) {
                            info.callback(res);
                        }
                    }
                });
            }
        }
        me.start = function () {
            run();
        };

        me.restart = function () {
            _ind = 0;
            me.start();
        }

        me.add = function (url, data, callback) {
            if ($.isFunction(data) && callback == undefined) {
                callback = data; data = {};
            }
            _arr.push({ url: url, data: data, callback: callback });
        };
    };

    $.extend({
        queueRequest: function () {
            return new queueRequest();
        },
        Loading: function (options, wrapper) {
            return new Loading(options, wrapper);
        },
    });


})(jQuery);

; (function ($) {
    //PopLoading fsf 2017-06-29
    function loading(options) {
        var self = this;
        self.show = function () {
            $(options.wrapper || "body").mLoading({ text: options.text });
        }
        self.close = function () {
            $(options.wrapper || "body").children(".mloading").remove();
        }
    }

    $.extend({
        PopLoading: function (options) {
            return new loading(options);
        },
    });
})(jQuery);
