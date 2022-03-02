chrome.runtime.onConnect.addListener(handleOnConnect);

chrome.tabs.onRemoved.addListener(handleTabRemoved);

chrome.runtime.onMessage.addListener(handleRuntimeMessage);

chrome.webNavigation.onCreatedNavigationTarget.addListener(handleWebNavigationOnCreatedTarget);

// The default level for a log call is LogLevel.Trace
// LogLevel.Error  - Always be recorded in the console (regardless of declared level below)
// LogLevel.Info   - Not very verbose logging
// LogLevel.Trace  - Very verbose, all messages are logged to the console, this will affect performance
var loggingLevel = LogLevel.Error;

// Communication between web pages and this background script is done through ports (See the https://developer.chrome.com/apps/runtime api).
// Each page has it's own port. They are stored in here, indexed via their page ID.
const pages = {};

//The queue of outgoing Native Messages
var messageQueue = [];

const extensionVersion = chrome.runtime.getManifest().version;

var sourcePageId;

var nmPort = null;
var clearToSendFlag = true;
var transmitTimeout = 10; //milliseconds

connectNativeMessaging();

function connectNativeMessaging() {
  if (nmPort === null) {
    const hostName = "com.blueprism.messaging";
   
    nmPort = chrome.runtime.connectNative(hostName);
    nmPort.onMessage.addListener(onNativeMessage);
    nmPort.onDisconnect.addListener(onNativeMessagingDisconnect);
    log("Native messaging connected", LogLevel.Info);
    setTimeout(transmitQueuedNativeMessages, transmitTimeout); 
  }
}

function transmitQueuedNativeMessages() {
  try {
    if (clearToSendFlag && nmPort !== null && messageQueue.length > 0) {
      clearToSendFlag = false;
      var messageToSend = messageQueue.shift();
      log("Sending message: " + JSON.stringify(messageToSend), LogLevel.Trace);
      nmPort.postMessage({ data: messageToSend });
    }
  } finally {
    setTimeout(transmitQueuedNativeMessages, transmitTimeout);
  }
}

function sendNativeMessage(message) {
  log("Queueing message: " + JSON.stringify(message), LogLevel.Trace);
  messageQueue.push(message);
}

function onNativeMessagingDisconnect() {
  log("disconnected",  LogLevel.Error);
  log(`Last runtime error: ${JSON.stringify(chrome.runtime.lastError)}`, LogLevel.Error);
  if (nmPort.error) {
    log(`Native messaging port error: ${nmPort.error.message}`, LogLevel.Error);
  }
  nmPort = null;
}

function onNativeMessage(message) {
  const messageData = JSON.parse(message.message);
  switch (messageData.name) {
    case "Attach":
      sendPagesMessage(messageData);
      break;
    case "Launch":
      openNewWindow(messageData);
      break;
    case "Acknowledge":
      clearToSendFlag = true;
      break;
    default:
      postMessage(messageData);
      break;
  }
}

function openNewWindow(messageData) {
  const urls = messageData.urls.split(" ");
  chrome.windows.create({ url: urls, type: "normal" });
}

function postMessage(message) {
  const pageId = message.pageId;
  if (pageId) {
    const port = pages[pageId];
    if (port)
      port.postMessage({ message: message });
  }
}

function sendPagesMessage(messageData) {
  if (Object.keys(pages).length !== 0) {
    sendNativeMessage({
      data: "attached",
      bpClientId: messageData.bpClientId,
      title: messageData.title,
      trackingId: messageData.trackingId,
      pages: Object.keys(pages),
      extensionVersion: extensionVersion,
      conversationId: messageData.conversationId,
      attempt: messageData.attempt
    });
  } else {
      sendNativeMessage({ data: "Session Started", extensionVersion: extensionVersion });
  }
}

function handleOnConnect(port) {
  port.onMessage.addListener(function (msg) {

    completeOnConnect(port, msg);
  });

  port.onDisconnect.addListener(handleOnDisconnect);
}

function handleOnDisconnect(p) {
  if (!p.name) return;
  sendNativeMessage({ pageId: p.name, data: "Page disconnected" });
  delete pages[p.name];
}

function handleTabRemoved(tabId, removedEvents) {
  log(`The Tab with ID: ${tabId} was removed`, LogLevel.Info);
  sendNativeMessage({ tabId: tabId, data: "Tab removed" });
}

function completeOnConnect(port, msg) {
  const tabId = msg.tabId;
  const pageId = port.name;
  if (msg.name === "makeConnection") {
    pages[pageId] = port;

    getSessionForPage(tabId, function (session) {
      if (session) {
        log(`Connecting page: ${pageId}, tab: ${tabId}, SessionID: ${session.sessionId}, Tab Title:`, LogLevel.Info);
        sendNativeMessage({ sessionId: session.sessionId, tabID: tabId, parentTabId: parentToChild[tabId], pageId: pageId, tabTitle: port.sender.tab.title, url: port.sender.tab.url, data: "Page connected", extensionVersion: extensionVersion });
      }
    });
  } else if (msg.name === "sendMessage") {
    sendNativeMessage({ pageId: pageId, data: msg.message });
  } else if (msg.name === "gethtmlsource") {
    sourcePageId = pageId;
    chrome.tabs.query({ active: true },
      function (tabArray) {
        chrome.tabs.executeScript(tabArray[0].id, { file: "getHtmlSource.js" });
      });
  } else if (msg.name === "closePageReceived") {
    chrome.tabs.remove(msg.tabId);
  }
}

function handleRuntimeMessage(request, sender, sendResponse) {
  if (request.action === "getSource") {
    const sourcePage = pages[sourcePageId];
    if (sourcePage) {
      sourcePage.postMessage({ message: request.source, name: "htmlSourceRetrieved" });
    }
  };

  if (request === "Register Tab") {
    sendResponse(sender.tab.id);
  }
}

function getSessionForPage(tabId, callback) {
  listSession(function (tabList) {
    if (callback)
      callback(tabList.find(element => element.tabId === tabId));
  });
}

function listSession(callback) {
  chrome.windows.getAll({ populate: true }, function (windowList) {
    const list = [];
    for (let i = 0; i < windowList.length; i++) {
      for (let ii = 0; ii < windowList[i].tabs.length; ii++) {
        list.push({ sessionId: windowList[i].id, tabId: windowList[i].tabs[ii].id });
      }
    }
    if (callback) {
      callback(list);
    }
  });
}

var parentToChild = {};

function handleWebNavigationOnCreatedTarget(details) {

  var parent = details.sourceTabId;
  var child = details.tabId;

  parentToChild[child] = parent;
}
