console.log("BluePrism plugin loaded");
(function () {
    const uuidZero = "00000000-0000-0000-0000-000000000000";
    const knownElements = {};

    const cryptographyProvider = new CryptographyProvider(
        new HashAlgorithm(CryptoJS.SHA256),
        new SymmetricAlgorithm(CryptoJS.AES),
        CryptoJS.lib.WordArray.random);
				
    function secureMathRandom() {
      var rng = window.crypto || window.msCrypto;
      if (rng === undefined)
        return Math.random();

      //result between 0 and 1 (inclusive of 0, but not 1)
      return rng.getRandomValues(new Uint32Array(1))[0] / 4294967296;
    }
	
    function uuidv4() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = secureMathRandom() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    // generate a unique id to identify this page to the background script.
    const pageId = uuidv4();

    
    // port used to communicate with the background script.
    const bgscriptCommsPort = chrome.runtime.connect({ name: pageId });

    (function attemptConnectWebSocket() {
        const messageHandlers = {
            "GetAddress": handleGetAddress,
            "SetAddress": handleSetAddress,
            "GetCursorPosition": handleGetCursorPosition,
            "ClickElement": handleClickElement,
            "FocusElement": handleFocusElement,
            "HoverElement": handleHoverElement,
            "GetElementBounds": handleGetElementBounds,
            "GetElementId": handleGetElementId,
            "GetElementName": handleGetElementName,
            "GetElementType": handleGetElementType,
            "GetElementClass": handleGetElementClass,
            "GetElementPath": handleGetElementPath,
            "GetElementValue": handleGetElementValue,
            "GetElementText": handleGetElementText,
            "GetElementHtml": handleGetElementHtml,
            "GetElementByPath": handleGetElementByPath,
            "GetElementByCssSelector": handleGetElementByCssSelector,
            "GetElementsByType": handleGetElementsByType,
            "GetElementsByClass": handleGetElementsByClass,
            "GetElementsByName": handleGetElementsByName,
            "GetElementById": handleGetElementById,
            "GetDescendants": handleGetDescendants,
            "GetElementIsChecked": handleGetElementIsChecked,
            "SetElementValue": handleSetElementValue,
            "SetCheckedState": handleSetCheckedState,
            "HighlightElement": handleHighlightElement,
            "RemoveHighlighting": handleRemoveHighlighting,
            "GetRootElement": handleGetRootElement,
            "GetElementClientBounds": handleGetElementClientBounds,
            "GetElementOffsetBounds": handleGetElementOffsetBounds,
            "GetElementScrollBounds": handleGetElementScrollBounds,
            "GetElementChildCount": handleGetElementChildCount,
            "GetElementIsEditable": handleGetElementIsEditable,
            "GetElementStyle": handleGetElementStyle,
            "GetElementTabIndex": handleGetElementTabIndex,
            "GetElementAttribute": handleGetElementAttribute,
            "SelectElement": handleSelectElement,
            "SetElementAttribute": handleSetElementAttribute,
            "ScrollToElement": handleScrollToElement,
            "SubmitElement": handleSubmitElement,
            "GetLinkAddressText": handleGetLinkAddressText,
            "GetElementIsVisible": handleGetIsVisible,
            "UpdateCookie": handleUpdateCookie,
            "GetElementIsOnScreen": handleGetElementIsOnScreen,
            "GetListItems": handleGetListItems,
            "SelectListItem": handleSelectListItem,
            "AddToListSelection": handleAddToListSelection,
            "RemoveFromListSelection": handleRemoveFromListSelection,
            "InjectJavascript": handleInjectJavascript,
            "InvokeJavascript": handleInvokeJavascript,
            "GetElementLabel": handleGetElementLabel,
            "SetElementTextRange": handleSetElementTextRange,
            "GetElementSelectedText": handleGetElementSelectedText,
            "GetTableColumnCount": handleGetTableColumnCount,
            "GetTableRowCount": handleGetTableRowCount,
            "GetTableCellText": handleGetTableCellText,
            "GetElementAccessKey": handleGetElementAccessKey,
            "GetIsListItemSelected": handleGetIsListItemSelected,
            "GetSliderRange": handleGetSliderRange,
            "DoubleClickElement": handleDoubleClickElement,
            "GetHTMLSource": handleGetHTMLSource,
            "ClosePage": handleClosePage,
            "CheckParentDocumentLoaded": handleCheckParentDocumentLoaded,
            "GetElementScreenLocationAndBounds": handleGetElementScreenLocationAndBounds
        };

        let hoveredElement = undefined;
        let cursorPosition = undefined;
        let highlightFrame;

        let encryptionKey;
        let connectionInterval;
        let getHtmlSourceMessage;

        connectionInterval = setInterval(connectToBluePrism, 1000);

        //Send a message to the background script asking for the tabId 
        let myTabId = "";
        chrome.runtime.sendMessage(
          "Register Tab",
          function (response) {
            myTabId = response;
            console.log(`Setting myTabId to:${response}`);
          }
      );

      function connectToBluePrism() {
          bgscriptCommsPort.postMessage({ name: 'makeConnection', pageId: pageId, tabId: myTabId });
        }

        bgscriptCommsPort.onMessage.addListener((msg) => {
          let message = msg.message;
          if (message.message === "acknowledged") {
            encryptionKey = pageId;
            setupEnvironment();
            clearInterval(connectionInterval);
          } else if (msg.name === "htmlSourceRetrieved") {
            sendReply(getHtmlSourceMessage, message);
          } else if (message.message === "ClosePage") {
            handleClosePage();
          } else {
            receiveClientMessage(message);
          }
        });

        function receiveClientMessage(message) {
            const decryptedMessage = cryptographyProvider.decrypt(message.message, encryptionKey);
            const messageData = JSON.parse(decryptedMessage);
            messageHandlers[messageData.MessageType](messageData);
        }

        function handleGetAddress(message) {
            sendReply(message, document.location.href);
        }

        function handleGetHTMLSource(message) {
          getHtmlSourceMessage = message;
          bgscriptCommsPort.postMessage({ name: 'gethtmlsource', pageId: pageId, tabId: myTabId  });
        }

        function handleClosePage() {
          bgscriptCommsPort.postMessage({ name: 'closePageReceived', pageId: pageId, tabId: myTabId });
        }

        function handleCheckParentDocumentLoaded(message) {
          sendReply(message, document.readyState === 'complete')
        }

      function handleGetElementScreenLocationAndBounds(message) {
        const elementBounds = tryGetFromElement(message.Data,
          e => {
            const bounds = e.getBoundingClientRect();
            return {
              x: Math.floor(bounds.x),
              y: Math.floor(bounds.y),
              width: Math.floor(bounds.width),
              height: Math.floor(bounds.height)
            };
          });
        const navHeight = window.outerHeight - window.innerHeight;
        const result = {
          x: Math.floor(window.screenX + elementBounds.x),
          y: Math.floor(window.screenY + navHeight + elementBounds.y),
          width: Math.floor(elementBounds.width),
          height: Math.floor(elementBounds.height)
        };
        sendReply(message, result);
      }

      function handleSetAddress(message) {
            document.location.href = message.Data;
        }

        function handleGetCursorPosition(message) {
            sendReply(message, cursorPosition);
        }

        function handleClickElement(message) {
            tryExecuteOnElement(message.Data,
                e => {
                    if (e.fireEvent) {
                        e.fireEvent('onclick');
                    } else {
                        const event = document.createEvent('MouseEvents');
                        event.initMouseEvent('click', true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                        e.dispatchEvent(event);
                    }
                });
        }

        function handleDoubleClickElement(message) {
            tryExecuteOnElement(message.Data,
                e => {
                    if (e.fireEvent) {
                        e.fireEvent('ondblclick');
                    } else {
                        const event = document.createEvent('MouseEvents');
                        event.initMouseEvent('dblclick', true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                        e.dispatchEvent(event);
                    }
                });
        }


        function handleFocusElement(message) {
            tryExecuteOnElement(message.Data, e => e.focus());
        }

      function handleHoverElement(message) {
          tryExecuteOnElement(message.Data,
            e => {
              if (e.fireEvent) {
                e.fireEvent('onmouseover');
              } else {
                const event = document.createEvent('MouseEvents');
                console.log(`mouseover fired for ${e}`);
                event.initMouseEvent('mouseover', true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                e.dispatchEvent(event);
              }
            });
        }
      function handleGetElementBounds(message) {
            let result = tryGetFromElement(message.Data,
                e => {
                    const bounds = e.getBoundingClientRect();
                    return {
                        x: Math.floor(bounds.x),
                        y: Math.floor(bounds.y),
                        width: Math.floor(bounds.width),
                        height: Math.floor(bounds.height)
                    };
                });
            if (result === null) {
                result = { x: -1, y: -1, width: 0, height: 0 };
            }
            sendReply(message, result);
        }

        function handleGetElementId(message) {
            const result = tryGetFromElement(message.Data, e => e.id);
            sendReply(message, result);
        }

        function handleGetElementSelectedText(message) {
            const result = tryGetFromElement(message.Data, e => {
                const selectionHelper = new ElementSelectedText(e, window.getSelection());
                const text = selectionHelper.getSelectedText();
                return text;
            });
            sendReply(message, result);
        }

        const ElementSelectedText = function (selectedElement, documentSelection) {

            const textFromElement = function (element) {
                const result = element.innerText || element.wholeText;
                return result.replace(/[\t\r]/g, " ").replace(/[\n]/g, "").trim();
            };

            const rootContainsNode = function (root, nodeToCheck) {

                if (root.isEqualNode(nodeToCheck))
                    return true;

                for (let childNode of root.childNodes) {
                    const result = rootContainsNode(childNode, nodeToCheck);
                    if (result)
                        return true;
                }
                return false;
            };

            const getElementSelectedText = function (node) {

                let result = "";
                if (node.nodeType === nodeTextType && documentSelection.containsNode(node)) {
                    const text = textFromElement(node);
                    result = text > "" ? text + " " : "";
                }

                for (let childNode of node.childNodes) {
                    const childNodeText = getElementSelectedText(childNode);
                    result = result !== undefined ? result.concat(childNodeText) : "";
                }
                return result;
            };

            const getFirstNodeInSelection = function (node) {

                if (documentSelection.containsNode(node))
                    return node;

                for (let childNode of node.childNodes) {
                    const resultNode = getFirstNodeInSelection(childNode);
                    if (resultNode)
                        return resultNode;
                }
                return null;
            };

            const determineTextFromSelection = function () {

                if (documentSelection.type === "None" || !elementWithinSelection)
                    return "";

                // The selection is within our element.
                if (startNodeInElement && endNodeInElement)
                    return documentSelection.toString();

                // The selection encompasses our element.
                if (!startNodeInElement && !endNodeInElement)
                    return textFromElement(selectedElement);

                const elementSelectedText = getElementSelectedText(selectedElement).trim();

                const replacementNode = startNodeInElement ? startNode : endNode;
                if (!replacementNode.nodeType === nodeTextType)
                    return elementSelectedText;

                const offset = startNodeInElement ? documentSelection.anchorOffset : documentSelection.focusOffset;
                const replacementNodeText = textFromElement(replacementNode);

                const firstNodeInSelection = getFirstNodeInSelection(selectedElement);
                const nodeIsAtStart = firstNodeInSelection.isEqualNode(replacementNode);

                const result = nodeIsAtStart ? elementSelectedText.substring(offset, elementSelectedText.length)
                    : elementSelectedText.substring(0, (elementSelectedText.length - replacementNodeText.length) + offset)

                return result;
            };

            const determineTextFromInputElement = function () {

                const parent = selectedElement.parentNode;
                if (parent && documentSelection.containsNode(parent))
                    return selectedElement.value || selectedElement.defaultValue

                if (!endNode)
                    return "";

                const selectedElementHasAnInput = endNode.firstChild !== null ? nodeInputNames.indexOf(endNode.children[0].nodeName) : -1;

                if (startNode.isEqualNode(endNode) && selectedElementHasAnInput !== -1)
                    return documentSelection.toString();

                return "";
            };

            const nodeTextType = 3;
            const nodeInputNames = ["INPUT", "TEXTAREA"];
            const startNode = documentSelection.anchorNode;
            const endNode = documentSelection.focusNode;
            const startNodeInElement = rootContainsNode(selectedElement, startNode);
            const endNodeInElement = rootContainsNode(selectedElement, endNode);
            const elementWithinSelection = getFirstNodeInSelection(selectedElement) !== null;

            this.getSelectedText = function () {

                const noValueFoundIndex = -1;
                const inputIndex = nodeInputNames.indexOf(selectedElement.nodeName);
                if (inputIndex !== noValueFoundIndex)
                    return determineTextFromInputElement();

                const result = determineTextFromSelection();
                return result.replace(/[\t\n\r]/g, " ");
            };
        };

        function handleGetElementName(message) {
            const result = tryGetFromElement(message.Data, e => e.name);
            sendReply(message, result);
        }

        function handleGetElementType(message) {
            const result = tryGetFromElement(message.Data, e => e.tagName);
            sendReply(message, result);
        }

        function handleGetElementClass(message) {
            const result = tryGetFromElement(message.Data, e => e.className.toString());
            sendReply(message, result);
        }

        function handleGetElementPath(message) {
            const result = tryGetFromElement(message.Data, getPathTo);
            sendReply(message, result);
        }

        function handleGetElementValue(message) {
            const result = tryGetFromElement(message.Data, e => e.value);
            sendReply(message, result);
        }

        function handleGetElementText(message) {
            const result = tryGetFromElement(message.Data, e => e.tagName === "INPUT" ? e.value : e.innerText);
            sendReply(message, result);
        }

        function handleGetElementHtml(message) {
            const result = tryGetFromElement(message.Data, e => e.outerHTML);
            sendReply(message, result);
        }

        function handleGetLinkAddressText(message) {
            const result = tryGetFromElement(message.Data, e => e.href);
            sendReply(message, result);
        }

        function handleGetElementByPath(message) {
            const element = getElementByXPath(message.Data);
            sendReply(message, element === null ? uuidZero : getOrAssignId(element));
        }

        function handleGetElementByCssSelector(message) {
            const element = getElementByCssSelector(message.Data);
            sendReply(message, element === null ? uuidZero : getOrAssignId(element));
        }

        function handleGetElementsByType(message) {
            const elementIds =
                [].slice.call(
                    document.getElementsByTagName(message.Data))
                    .map(getOrAssignId);

            sendReply(message, elementIds);
        }

        function handleGetElementsByClass(message) {
            const elementIds =
                [].slice.call(
                    document.getElementsByClassName(message.Data))
                    .map(getOrAssignId);

            sendReply(message, elementIds);
        }

        function handleGetElementById(message) {
            const element = document.getElementById(message.Data);
            sendReply(message, element === null ? uuidZero : getOrAssignId(element));
        }

        function handleGetElementsByName(message) {
            const elementIds =
                [].slice.call(
                    document.getElementsByName(message.Data))
                    .map(getOrAssignId);

            sendReply(message, elementIds);
        }

        function handleGetDescendants(message) {

            let elementIds = tryGetFromElement(message.Data,
                e =>
                    [].slice.call(
                        e.querySelectorAll("*"))
                        .map(getOrAssignId));

            if (elementIds === null) {
                elementIds = [];
            }

            sendReply(message, elementIds);
        }

        function handleSetElementValue(message) {
            tryExecuteOnElement(message.Data.ElementId,
                e => { if (e.value !== undefined) e.value = message.Data.Value; });
        }

        function handleSetCheckedState(message) {
            tryExecuteOnElement(message.Data.ElementId,
                e => { e.checked = message.Data.Value; });
        }

        function handleHighlightElement(message) {
            const element = knownElements[message.Data.ElementId];
            if (element === undefined || element === null) {
                removeElementHighlighting();
                return;
            }
            highlightFrame.style.display = "block";
            const bounds = element.getBoundingClientRect();
            highlightFrame.style.left = `${bounds.x}px`;
            highlightFrame.style.top = `${bounds.y + window.pageYOffset}px`;
            highlightFrame.style.width = `${bounds.width}px`;
            highlightFrame.style.height = `${bounds.height}px`;
            highlightFrame.style.border = `2px solid ${message.Data.Color}`;
            highlightFrame.style.pointerEvents = "None";
        }

        function handleRemoveHighlighting() {
            removeElementHighlighting();
        }

        function handleGetRootElement(message) {
            const element = document.documentElement;
            sendReply(message, getOrAssignId(element));
        }

        function handleGetElementClientBounds(message) {
            let bounds = tryGetFromElement(message.Data,
                e => ({
                    x: Math.floor(e.clientLeft || 0),
                    y: Math.floor(e.clientTop || 0),
                    width: Math.floor(e.clientWidth || 0),
                    height: Math.floor(e.clientHeight || 0)
                }));
            if (bounds === null) {
                bounds = { x: -1, y: -1, width: 0, height: 0 };
            }
            sendReply(message, bounds);
        }

        function handleGetElementOffsetBounds(message) {
            let bounds = tryGetFromElement(message.Data,
                e => ({
                    x: Math.floor(e.offsetLeft || 0),
                    y: Math.floor(e.offsetTop || 0),
                    width: Math.floor(e.offsetWidth || 0),
                    height: Math.floor(e.offsetHeight || 0)
                }));
            if (bounds === null) {
                bounds = { x: -1, y: -1, width: 0, height: 0 };
            }
            sendReply(message, bounds);
        }

        function handleGetElementScrollBounds(message) {
            let bounds = tryGetFromElement(message.Data,
                e => ({
                    x: Math.floor(e.scrollLeft || 0),
                    y: Math.floor(e.scrollTop || 0),
                    width: Math.floor(e.scrollWidth || 0),
                    height: Math.floor(e.scrollHeight || 0)
                }));
            if (bounds === null) {
                bounds = { x: -1, y: -1, width: 0, height: 0 };
            }
            sendReply(message, bounds);
        }

        function handleGetTableCellText(message) {
            const rowNumber = message.Data.RowNumber;
            const columnNumber = message.Data.ColumnNumber;
            const elementId = message.Data.ElementId;
            const result = tryGetFromElement(elementId, e => e.rows[rowNumber].cells[columnNumber].innerText);
            sendReply(message, result);
        }

        function handleGetTableColumnCount(message) {
            const result = tryGetFromElement(message.Data.ElementId, e => e.rows[message.Data.RowIndex].cells.length);
            sendReply(message, result);
        }

        function handleGetTableRowCount(message) {
            const result = tryGetFromElement(message.Data, e => e.rows.length);
            sendReply(message, result);
        }

        function handleGetElementIsChecked(message) {
            const result = tryGetFromElement(message.Data, e => e.checked);
            sendReply(message, result || false);
        }

        function handleGetElementChildCount(message) {
            const result = tryGetFromElement(message.Data, e => e.childElementCount);
            sendReply(message, result);
        }

        function handleGetElementIsEditable(message) {
            const result = tryGetFromElement(message.Data, e => e.isContentEditable);
            sendReply(message, result);
        }

        function handleGetElementStyle(message) {
            const result = tryGetFromElement(message.Data, e => JSON.stringify(e.style));
            sendReply(message, result);
        }

        function handleGetElementTabIndex(message) {
            const result = tryGetFromElement(message.Data, e => e.tabIndex);
            sendReply(message, result);
        }

        function handleGetElementAttribute(message) {
            const result = tryGetFromElement(message.Data.ElementId,
                e => e.getAttribute(message.Data.AttributeName));
            sendReply(message, result);
        }

        function handleSetElementTextRange(message) {
            const elementId = message.Data.ElementId;
            const startIndex = message.Data.StartIndex;
            const length = message.Data.Length;
            tryExecuteOnElement(elementId, e => {

                const nodeInputNames = ["INPUT", "TEXTAREA"];
                const noValueFoundIndex = -1;
                const inputIndex = nodeInputNames.indexOf(e.nodeName);
                if (inputIndex !== noValueFoundIndex) {
                    e.focus();
                    e.setSelectionRange(startIndex - 1, (startIndex - 1) + length);
                    return;
                }

                const getFirstTextNodeInSelection = function (node) {

                    const nodeTextType = 3;
                    if (node.nodeType === nodeTextType)
                        return node;

                    for (let childNode of node.childNodes) {
                        const resultNode = getFirstTextNodeInSelection(childNode);
                        if (resultNode)
                            return resultNode;
                    }
                    return null;
                };

                const firstTextNode = getFirstTextNodeInSelection(e);
                if (!firstTextNode)
                    return;

                const range = document.createRange();
                range.setStart(firstTextNode, startIndex - 1);
                range.setEnd(firstTextNode, (startIndex - 1) + length);
                const selection = window.getSelection();
                selection.removeAllRanges();
                selection.addRange(range);
            });
        }

        function handleSelectElement(message) {
            tryExecuteOnElement(message.Data,
                e => {

                    if (e.tagName === "OPTION") {
                        e.selected = "selected";
                    } else {
                        const range = document.createRange();
                        range.setStartBefore(e);
                        range.setEndAfter(e);
                        const selection = window.getSelection();
                        selection.removeAllRanges();
                        selection.addRange(range);
                    }
                });
        }

        function handleSetElementAttribute(message) {
            tryExecuteOnElement(message.Data.ElementId,
                e => e.setAttribute(message.Data.AttributeName, message.Data.Value));
        }

        function handleScrollToElement(message) {
            tryExecuteOnElement(message.Data, e => e.scrollIntoView(true));
        }

        function handleSubmitElement(message) {
            tryExecuteOnElement(message.Data, e => {
                if (e.tagName.toLowerCase() === "form" && e.submit !== undefined)
                    e.submit();
            });
        }

        function handleGetIsVisible(message) {
            const isVisible = tryGetFromElement(message.Data, e => window.getComputedStyle(e).display !== 'none');
            sendReply(message, isVisible);
        }

        function handleUpdateCookie(message) {
            document.cookie = message.Data;
        }

        function handleGetElementIsOnScreen(message) {
            const isOnScreen =
                tryGetFromElement(message.Data, e => {
                    const rect = e.getBoundingClientRect();
                    const viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight);
                    return !(rect.bottom < 0 || rect.top - viewHeight >= 0);
                });
            sendReply(message, isOnScreen);
        }

        function handleGetListItems(message) {
            const items = tryGetFromElement(message.Data, e =>
                [...e.querySelectorAll("option,li")]
                    .map(x => ({
                        Text: x.innerText,
                        Value: x.value || null,
                        IsSelected: x.selected ? true : false
                    })));

            sendReply(message, items);
        }

        function handleSelectListItem(message) {
            tryExecuteOnElement(message.Data.ElementId, e => {
                const listItems = [...e.querySelectorAll("option")];
                for (let item of listItems) {
                    item.selected = '';
                }
                if (message.Data.Index > 0) {
                    listItems[message.Data.Index - 1].selected = 'selected';
                } else {
                    const matchingItems = listItems.filter(item => item.innerText === message.Data.Name);
                    if (matchingItems[0]) {
                        matchingItems[0].selected = 'selected';
                    }
                }
            });
        }

        function handleAddToListSelection(message) {
            tryExecuteOnElement(message.Data.ElementId, e => {
                const listItems = [...e.querySelectorAll("option")];
                if (message.Data.Index > 0) {
                    listItems[message.Data.Index - 1].selected = 'selected';
                } else {
                    for (let item of listItems.filter(item => item.innerText === message.Data.Name)) {
                        item.selected = 'selected';
                    }
                }
            });
        }

        function handleRemoveFromListSelection(message) {
            tryExecuteOnElement(message.Data.ElementId, e => {
                const listItems = [...e.querySelectorAll("option")];
                if (message.Data.Index > 0) {
                    listItems[message.Data.Index - 1].selected = '';
                } else {
                    for (let item of listItems.filter(item => item.innerText === message.Data.Name)) {
                        item.selected = '';
                    }
                }
            });
        }

        function handleInjectJavascript(message) {
            const script = document.createElement("script");
            const code = document.createTextNode(message.Data);
            script.appendChild(code);
            document.getElementsByTagName("BODY")[0].appendChild(script);
        }

        function handleInvokeJavascript(message) {
            const script = document.createElement("script");
            const code = document.createTextNode(`${message.Data.FunctionName}.apply(this,${message.Data.Parameters});`);
            script.appendChild(code);
            document.getElementsByTagName("BODY")[0].appendChild(script);
        }

        function handleGetElementLabel(message) {
            const labelText = tryGetFromElement(message.Data,
                e => {
                    if (e.id) {
                        const labels = document.querySelectorAll(`label[for="${e.id}"]`);
                        if (labels.length === 1)
                            return labels[0].innerText;
                    }

                    while (e.parentElement) {
                        e = e.parentElement;
                        if (e.tagName === "LABEL") {
                            return e.innerText;
                        }
                    }
                    return "";
                });
            sendReply(message, labelText);
        }


        function handleGetIsListItemSelected(message) {
            const selected = tryGetFromElement(message.Data, e => {
                return e.selected;
            });

            sendReply(message, selected);
        }

        function handleGetElementAccessKey(message) {
            const accessKey = tryGetFromElement(message.Data, e => e.accessKey);
            sendReply(message, accessKey);
        }

        function handleGetSliderRange(message) {
            const range = tryGetFromElement(message.Data, e => ({ Minimum: e.min, Maximum: e.max }));
            sendReply(message, range);
        }


        function tryExecuteOnElement(elementId, action) {
            const element = knownElements[elementId];
            if (element !== undefined && element !== null) {
                action(element);
            }
        }

        function tryGetFromElement(elementId, getProperty) {
            const element = knownElements[elementId];
            let result = null;
            if (element !== undefined && element !== null) {
                result = getProperty(element);
            }
            return result;
        }

        function sendReply(message, data) {
          sendMessageToBackground(getEncryptedMessage(message.MessageType, data, message.ConversationId));
        }

      function sendMessageToBackground(message) {
        bgscriptCommsPort.postMessage({ name: 'sendMessage', pageId: pageId, message: message, tabId: myTabId });
        }

        function removeElementHighlighting() {
            highlightFrame.style = "position: absolute; display: none";
        }

        function getElementByXPath(path) {
            return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
        }

        function getElementByCssSelector(selector) {
            return document.querySelector(selector);
        }

        let isSetup = false;
        function setupEnvironment() {
            if (isSetup) return;

            window.onmouseout = e => {
                hoveredElement = undefined;
                cursorPosition = undefined;
                sendMessageToBackground(getEncryptedMessage(
                    "MouseHover",
                    uuidZero));
            };

            window.onmousemove = e => {

                var clientX = (typeof e.x === 'undefined') ? e.clientX: e.x;
                var clientY = (typeof e.y === 'undefined') ? e.clientY : e.y;

                hoveredElement = document.elementsFromPoint(clientX, clientY).filter(function (x) { return x !== highlightFrame; })[0];
                cursorPosition = { x: clientX, y: clientY };
                sendMessageToBackground(getEncryptedMessage(
                    "MouseHover",
                    getOrAssignId(hoveredElement)));
            };

            highlightFrame = document.createElement("div");
            highlightFrame.style = "position: absolute; display: none";

            window.setInterval(function () { highlightFrame.style.zIndex = 16777271; });

            document.getElementsByTagName("body")[0].appendChild(highlightFrame);

            isSetup = true;
        }

        function getEncryptedMessage(messageType, data, id = uuidv4()) {
            return cryptographyProvider.encrypt(getMessage(messageType, data, id), encryptionKey);
        }

        function getMessage(messageType, data, id = uuidv4()) {
            return JSON.stringify({
                ConversationId: id,
                MessageType: messageType,
                Data: data,
                pageId: pageId,
            });
        }

        function getOrAssignId(element) {
            const matchingElement =
                Object.keys(knownElements)
                    .map(k => ({ element: knownElements[k], key: k }))
                    .filter(x => x.element === element);

            if (matchingElement.length > 0) {
                return matchingElement[0].key;
            } else {
                const key = uuidv4();
                knownElements[key] = element;
                return key;
            }
        }

        // Taken from https://stackoverflow.com/questions/2631820/how-do-i-ensure-saved-click-coordinates-can-be-reloaed-to-the-same-place-even-i/2631931#2631931
        function getPathTo(element) {
            if (element.tagName === 'HTML')
                return '/HTML[1]';
            if (element === document.body)
                return '/HTML[1]/BODY[1]';

            let ix = 0;
            const siblings = element.parentNode.childNodes;
            for (let i = 0; i < siblings.length; i++) {
                const sibling = siblings[i];
                if (sibling === element)
                    return getPathTo(element.parentNode) + '/' + element.tagName + '[' + (ix + 1) + ']';
                if (sibling.nodeType === 1 && sibling.tagName === element.tagName)
                    ix++;
            }

            return '';
        }
    })();
})();

