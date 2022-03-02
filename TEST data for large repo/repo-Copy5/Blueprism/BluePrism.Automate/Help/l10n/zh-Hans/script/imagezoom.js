var imgID = 0;

// Currently, this is the only initialising function in the help.
// If we add any more, this will have to play a bit nicer.
// Course, it'd be nice if we had jquery instead...
window.onload = function()
{
	var divs = document.getElementsByTagName("div");
	var maxheight = 0;
	for (var i=0; i<divs.length; i++)
	{
		var div = divs[i];
		var cls = div.className.toLowerCase();
		if (cls.indexOf("captioned-image") != -1)
		{
			if (cls.indexOf("nozoom")!=-1) // cancelled
				continue;
			
			var thumbs = div.getElementsByTagName("img");
			if (typeof(thumbs)!="undefined" && thumbs!=null && thumbs.length > 0)
			{	
				var thumb = thumbs[0];
				
				thumb.alt = ""; // the alt text is a pain when zooming an image when hovering 
				thumb.name = "img" + (++imgID);
				
				var d = document.createElement("div");
				d.className = "imgzoom";
				d.id = "_"+thumb.name;
				d.style.position = "absolute";
				d.style.left = "-5000px";

				var img = document.createElement("img");
				img.src = thumb.src;
				d.appendChild(img);
				
				document.body.appendChild(d);
				if (d.clientHeight && d.clientHeight>maxheight)
					maxheight = d.clientHeight;
				
				var showimg = function(evt)
				{
					if (typeof(evt)=="undefined")
						evt = window.event;

					var zoomer = document.getElementById("_"+this.name);
					
					var x = 0; var y = 0;
					
					if (evt.pageX || evt.pageY) // firefox
					{
						x = evt.pageX;
						y = evt.pageY;
					}
					else if (evt.clientX || evt.clientY) // ie
					{
						x = evt.clientX + document.body.scrollLeft
							+ document.documentElement.scrollLeft;
						y = evt.clientY + document.body.scrollTop
							+ document.documentElement.scrollTop;
					}

					var clientWidth = document.documentElement.clientWidth;
					var zoomerWidth = zoomer.clientWidth;
					if (x + zoomerWidth > clientWidth)
						x = x - zoomerWidth - 20;
					
					zoomer.style.left = (x+10) + "px";
					zoomer.style.top = (y+10) + "px";
				}
				
				var hideimg = function(evt)
				{
					var zoomer = document.getElementById("_"+this.name);
					//zoomer.style.top = "0px";
					zoomer.style.left = "-5000px";
				}
				
				thumb.onmouseover = showimg;
				thumb.onmousemove = showimg;
				thumb.onmouseout = hideimg;
			}
			
		}
	}
	// make space at the bottom of the page for the largest image
	// to be zoomed without causing unsettling scrolling to occur
	if (maxheight > 0)
	{
		var spacer = document.createElement("div");
		spacer.style.position = "absolute";
		spacer.style.height=maxheight + "px";
		spacer.style.width="1px";
		spacer.style.left = "-5000px";
		document.body.appendChild(spacer);
	}
}
