if (typeof jQuery !== 'undefined') {
    if (!jQuery.trim) {
        jQuery.trim = function(text) {
            return text == null ? "" : String.prototype.trim.call(text);
        };
    }
    if (!jQuery.isFunction) {
        jQuery.isFunction = function(obj) {
            return typeof obj === "function";
        };
    }
    if (!jQuery.isArray) {
        jQuery.isArray = Array.isArray;
    }
    if (!jQuery.isWindow) {
        jQuery.isWindow = function(obj) {
            return obj != null && obj === obj.window;
        };
    }
}
