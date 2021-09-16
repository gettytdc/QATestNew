

// **********************************************************
// 					SessionCreator Class

SessionCreator.prototype._ProcessName;
SessionCreator.prototype._UserName;
SessionCreator.prototype._http;
SessionCreator.prototype._SessionID;

//Constructor.
function SessionCreator(UserName, ProcessName) {
	this._ProcessName = ProcessName;
	this._UserName = UserName;
	this._http=new XMLHttpRequest();
}


SessionCreator.prototype.createSession = function() {
	try {
		//Synchronous request, to create session
		http.open("GET", "http://localhost:8181/user%20name%20"+escape(this._UserName)+"&create%20name%20"+escape(this._ProcessName), false);
		http.send(null);
		
		//Synchronous request to start the new session
		if(http.readyState==4) {
			if(http.status==200) {
				response=http.responseText.split(/\r\n|\r|\n/);
				if(!response[0].startsWith("USER SET")) {
					throw("Failed to set user - "+response[0]);
				}
				if(!response[1].startsWith("SESSION CREATED")) {
					throw("Did not create session - "+response[1]);
				}
				this._SessionID=response[1].substring(18);
			} else {
				throw("Start request failed:"+http.status);
			}
		}
		
		//Make sure that the session was properly started
		http.open("GET", "http://localhost:8181/user%20name%20"+escape(this._UserName)+"&start%20"+escape(this._SessionID), false);
		http.send(null);
		if(http.readyState==4) {
			if(http.status==200) {
				response=http.responseText.split(/\r\n|\r|\n/);
				if(!response[0].startsWith("USER SET")) {
					throw("Failed to set user - "+response[0]);
				}
				if(!response[1].startsWith("STARTED")) {
					throw("Did not start - "+response[1]);
				}
				
			} else {
				throw("Start request failed:"+http.status);
			}
		}
		
	}
	catch(e) {
		throw("Unexpected error: "+ e)
	}
}


SessionCreator.prototype.checkSessionStatus = function() {
	//Ask Automate for the status
	var Status
	http.open("GET", "http://localhost:8181/status", false);
	http.send(null);
	if(http.readyState==4) {
		if(http.status==200) {
			response=http.responseText.split(/\r\n|\r|\n/);
			for (var i=0; i<response.length; i++) {
				if (response[i].match(this._SessionID)) {
					Status= response[i].split(" ")[2];
				}
			}
		} else {
			throw("Start request failed - http status: "+http.status);
		}
	}
	
	if (Status) {
		return Status
	}
	else {
		throw("Status call was successful, but Session of interest was not listed - " & this._SessionID)
	}
}

SessionCreator.prototype.getSessionID = function() {
	return this._SessionID;
}

SessionCreator.prototype.getProcessName = function() {
	return this._ProcessName;
}

// **********************************************************







