/**
 * Get current browser viewpane heigtht
 */
function _get_window_height() {
    return window.innerHeight || 
           document.documentElement.clientHeight ||
           document.body.clientHeight || 0;
}

/**
 * Get current absolute window scroll position
 */
function _get_window_Yscroll() {
    return window.pageYOffset || 
           document.body.scrollTop ||
           document.documentElement.scrollTop || 0;
}

/**
 * Get current absolute document height
 */
function _get_doc_height() {
    return Math.max(
        document.body.scrollHeight || 0, 
        document.documentElement.scrollHeight || 0,
        document.body.offsetHeight || 0, 
        document.documentElement.offsetHeight || 0,
        document.body.clientHeight || 0, 
        document.documentElement.clientHeight || 0
    );
}



function scrollToDiv(idref) {
	/ * get the working element; the last touched */
	var el = document.getElementById(idref);
	
	/* get the working element to the top of the window */
	if (el) el.scrollIntoView(true); else return;
	
	var dh = _get_doc_height();
	var wh = _get_window_height();
	var et = el.offsetTop;

	/* if we're not close to the bottom, scroll up to get
			the working area in the middle of the window */
	if (dh - et > wh/2)
		window.scrollBy(0, -wh/2);
}