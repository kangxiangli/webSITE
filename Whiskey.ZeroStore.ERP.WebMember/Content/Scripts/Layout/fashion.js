(function ($) {
    $.fashion = $.fashion || { version: 2.1 };
})(jQuery);

(function ($) {
    ; $.fashion.tools = {
        url: {
            encode: function (url) {
                return encodeURIComponent(url);
            },
            decode: function (url) {
                return decodeURIComponent(url);
            }
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
    };

    ; $.fashion.arraylist = (function (a) {

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
                    if ($.fashion.tools.virtEquals(item, array[i])) {
                        return i;
                    }
                }
                return -1;
            };

            this.lastIndexOf = function (item) {
                for (var i = array.length - 1; i >= 0; --i) {
                    if ($.fashion.tools.virtEquals(item, array[i])) {
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

    ; $.fashion.hashtable = (function (UNDEFINED) {
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

    ; $.fashion.filter = {
        rule: function (field, value, operate) {
            this.Field = field;
            this.Value = value;
            this.Operate = operate || "equal";
        },
        group: function () {
            this.Rules = [];
            this.Operate = "and";
            this.Operates = [];
            this.Groups = [];
        },
        conditions: function () {

        }

    };

    ; $.fashion.datatable = {
        instance: undefined,
        instances: [],
        reset: function (saveStatus, ntable) {
            var oTable = $.fashion.datatable.instance;

            if (ntable != undefined) {
                oTable = ntable;
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
        lblColor: function (value, calc) {
            var lbl = "info";
            if (calc) {
                if (typeof calc == 'boolean' && !isNaN(value))
                    lbl = value > 0 ? "success" : value == 0 ? "info" : "danger";
                else
                    lbl = calc;
            }
            return "<label class='label label-" + lbl + "'>" + value + "</label>";
        },
        tplTitleCheckbox: function (flg) {
            if (flg == undefined || flg == null)
                flg = "checked-all";
            return '<label style="display:inline" class="px-single"><input type="checkbox" class="px ' + flg + '" checked="checked"><span class="tal" style="点击全选">全选</span></label>';
        },
        tplListCheckbox: function (value, flg, ische) {
            if (flg == undefined || flg == null)
                flg = "";
            if (ische == undefined || ische == null)
                return '<label class="px-single"><input type="checkbox" value="' + value + '" class="px te_1_che ' + flg + '" checked="checked"><span class="lbl"></span></label>';
            else
                return '<label class="px-single"><input type="checkbox" value="' + value + '" class="px te_1_che ' + flg + '"><span class="lbl"></span></label>';
        },
    };

    ; $.fashion.ajaxLoading = function (options, wrapper) {
        var loading = $.Loading();

        var complete = options.complete;
        options.complete = function (httpRequest, status) {
            loading.close();
            if (typeof (complete) == "function") {
                complete(httpRequest, status);
            }
        };
        options.error = function (XMLHttpRequest, textStatus, errorThrown) {
            var content = "服务器请求发生错误：" + errorThrown + "，错误代码：" + XMLHttpRequest.status;
            $.fashion.web.alert({ content: content, type: "error" });
        };
        options.async = true;

        var beforeSend = options.beforeSend;
        options.beforeSend = function () {
            loading.show();
            if (typeof (beforeSend) == "function") {
                var retu = beforeSend();
                if (retu == false) {
                    loading.close();
                    return false;
                }
            }
        };

        $.ajax(options);
    };

    ; $.fashion.web = {
        init: function () {

            // $.fashion.web.speak("欢迎光临零时尚ERP管理平台！");
        },
        speak: function (text) {
            var print = $.fashion.printer.getLodop();
            print.FORMAT("VOICE:0;50", text);
        },
        formVerify: function (formName) {
            $(formName).removeData('validator');
            $(formName).removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(formName);
        },
        clearForm: function (formName) {
            $(formName).find("input").not(':button, :submit, :reset').val('').removeAttr('checked');
            $(formName).find("select option[value='']").attr("selected", true);
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
            $.fashion.ajaxLoading({
                // cache: false,
                type: "get",
                url: options.url,// + "?r=" + Math.random()
                data: options.data,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                    if (typeof (options.beforeSend) == "function") {
                        var res = options.beforeSend(); //在请求之前发生，如果方法返回为false ,请求终止
                        if (res != undefined && res == false) return false;
                    }
                },
                complete: function (result) {
                    $(options.lockButton).removeAttr("disabled");
                    if (typeof (options.complete) == "function") {
                        options.complete();
                    }
                },
                success: function (data) {
                    var formHeader = "<form id='" + _id + "' 0fashion='fashion-team' class=\"modal-form form-horizontal dropzone\" action=\"" + options.url + "\" enctype=\"multipart\/form-data\">";

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
                    var succe_but_event = options.success;
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
                                if (typeof (options.close) == "function") {
                                    return options.close();
                                }
                            }
                        }
                    };
                    if (options.button) {
                        button = $.extend(options.button, button);
                    }

                    options.formModel = bootbox.dialog({
                        message: mesg,
                        className: options.className,
                        title: options.caption,
                        buttons: button
                    });
                }
            });
        },
        ajaxConfirm: function (options) {
            var title = options.question.length > 0 ? "<div class='text-danger text-center'><h3>" + options.question + "</h3></div>" : "";
            var description = options.notes.length > 0 ? "<br /><div class='text-center'>" + options.notes + "</div><br />" : "";
            bootbox.dialog({
                message: title + description,
                closeButton: options.showclosebut,
                onEscape: options.exit,
                animate: true,
                buttons: {
                    success: {
                        label: "确认",
                        icon: "fa-check",
                        className: "btn btn-primary",
                        callback: function () {
                            if (options.successEvent != undefined)
                                options.successEvent();
                            else {
                                //如果没有传入自定义方法
                                $.fashion.ajaxLoading({
                                    type: "POST",
                                    url: options.url,// + "?r=" + Math.random()
                                    data: options.data,
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
                                                        $.fashion.web.alert({
                                                            content: data.Message,
                                                            type: "success",
                                                            ok: function () {
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
                                                    $.fashion.web.alert({ content: data.Message, type: "error" });
                                                }
                                            }
                                            else {
                                                $.fashion.web.alert({ content: data, type: "info" });
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
                            if (options.cancel != undefined && typeof (options.cancel) == "function") {
                                options.cancel();
                            }
                        }
                    }
                }

            });

        },
        ajaxView: function (options) {
            $.fashion.ajaxLoading({
                type: options.type || "GET",
                url: options.url,// + "?r=" + Math.random()
                data: options.data,
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
                                    if (typeof (options.success) == "function") {
                                        options.success(data);
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
        ajax: function (options) {
            $.fashion.ajaxLoading({
                type: options.type != "undefined" ? options.type : "GET",
                url: options.url + "?r=" + Math.random(),
                data: options.data,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                    if (typeof (options.beforeSend) == "function")
                        return options.beforeSend();
                },
                complete: function (httpRequest, status) {
                    $(options.lockButton).removeAttr("disabled");
                    if (typeof (options.complete) == "function")
                        options.complete(httpRequest, status);
                },
                success: function (data) {
                    if (typeof (options.success) == "function") {
                        options.success(data);
                    }

                    if (!options.disDialog) {
                        if (data.ResultType == OperationResultType.Success) {
                            layer.msg(data.Message, { icon: 6, time: options.closeDialogTimer || 1000 }, function () {
                                if (typeof (options.closeDialogEvent) == "function") {
                                    options.closeDialogEvent(data);
                                }
                            });
                        } else {
                            if (data.ResultType != undefined) {
                                if (!options.errorIsMsg) {
                                    var stype = (data.ResultType == OperationResultType.ValidError || data.ResultType == OperationResultType.Error) ? "error" : "info";
                                    $.fashion.web.alert({
                                        type: stype,
                                        content: data.Message,
                                    });
                                } else {
                                    layer.msg(data.Message, { icon: 2 });
                                }
                            }
                        }
                    } else {
                        layer.closeAll();
                    }
                }, error: function (data) {
                    $.fashion.web.alert({
                        type: "error",
                        content: "请求失败,请稍候在试",
                    });
                }
            });
        },
        alert: function (options) {
            var iconArray = ["warning", "success", "error", "info"];
            options.type = options.type || "warning";

            var header = "<div class='fashion-alert-div'>";
            var footer = "</div>";
            var msg = header + options.content + footer;
            layer.open({
                title: options.title,
                content: msg,
                btnAlign: "c",
                yes: function (ind) {
                    if ($.isFunction(options.ok)) {
                        options.ok();
                    }
                    layer.close(ind);
                },
                id: guid(),
                cancel: options.cancel,
                move: options.move != false,
                shadeClose: options.shadeClose != true,
                area: '360px',
                icon: iconArray.indexOf(options.type)
            });
        },
        load: function (options) {
            options.data = options.data || {};
            options.data._p_flag = options._p_flag == undefined ? 1 : options._p_flag;
            if (options.data._p_flag == -1) {
                options.wrapper = options.wrapper || "#main-wrapper";
            }

            $.fashion.web.ajax({
                url: options.url,
                data: options.data,
                disDialog: true,
                success: function (data) {
                    if (typeof data == "object" && data.ResultType && data.ResultType !== 3) {
                        $.fashion.web.alert({
                            type: "error",
                            content: data.Message,
                        });
                    } else {
                        $(options.wrapper || "#content-wrapper").empty().append(data);
                        history.pushState(history.state, "", options.url);
                    }
                }
            });
        },
    };

    ; $.fashion.buildLoad = function (userOptions) {
        var options = {
            wrapper: "#menu-wrapper",
            selector: "ul.menu_main",
            contentwrapper: "#content-wrapper",
            flag: 1//0=nav,1=menu
        };

        $.extend(options, userOptions);

        $(options.wrapper).find(options.selector).find("a[href]").filter(":not([href*='javascript'],[href^='#'],[href=''],.notmapper)").each(function (index, item) {
            var $item = $(item);
            $item.attr("fashion-href", item.href).removeAttr("href").attr("style", "cursor:pointer;");
            var _flag = $item.attr("fashion-flag");
            _flag = _flag == undefined ? options.flag : _flag;
            $item.unbind("click.load").bind('click.load', function () {
                var itemurl = $(this).attr("fashion-href");
                _loadPartialPage(itemurl, _flag);
                history.pushState(history.state, "", itemurl);
            });
        });
        function _loadPartialPage(itemurl, _flag) {
            $.fashion.web.load({
                url: itemurl,
                _p_flag: _flag,
                wrapper: options.contentwrapper,
            });
        }
    }
    ; $.fashion.web.msg = function (options){
    	var icon = options.icon || 0;
    	var time = options.time || 2000;
    	layer.msg(options.msg,{"icon":icon,"time":time})
    }

})(jQuery);

function success_event(options) {
    $.fashion.web.formVerify(".modal-form");
    if (typeof (options.formValidator) == "function") {
        if (!options.formValidator()) {
            return false;
        }
    }
    $.fashion.ajaxLoading({
        type: "POST",
        url: options.url,
        data: $(".modal-form").serialize() + "&" + JSON.stringify(options.data),
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
                        $.fashion.web.alert({
                            content: data.Message,
                            type: "success",
                            ok: function () {
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
                    $.fashion.web.alert({ content: data.Message, type: "error" });
                }
            }
            else {
                $.fashion.web.alert({ content: data });
            }
        }
    });
    return false;
}

$(document).ready(function () {
    $.fashion.web.init();
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
        var self = this;
        self.layindex = null;

        self.close = function () {
            if (self.layindex) {
                layer.close(self.layindex);
            }

            if (options && typeof (options.closeEvent) == 'function') {
                options.closeEvent();
            }
        }

        self.show = function () {
            //self.layindex = layer.load('加载中...', {
            //    icon: 16,
            //    shade: [0.1, '#fff'] //0.1透明度的白色背景
            //});
            self.layindex = layer.load(2, {
                shade: [0.1] //0.1透明度的白色背景
            });
        }
    };

    $.extend({
        Loading: function (options, wrapper) {
            return new Loading(options, wrapper);
        },
    });
})(jQuery);