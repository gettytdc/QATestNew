const LogLevel = Object.freeze({ "Trace": 1, "Info": 2, "Error": 3 })

function log(message, level = LogLevel.Trace) {
  if (level >= loggingLevel || level === LogLevel.Error) {
    const e = new Error;
    var callerLine = e.stack.substring(e.stack.lastIndexOf("\n"));
    callerLine = callerLine.substring(callerLine.lastIndexOf(`/`) + 1);
    callerLine = callerLine.substring(0, callerLine.lastIndexOf(`:`));
    executeAsync(function () {
      const entry = `${timeStamp()}\t${message}\n(${callerLine})`;
      console.log(entry);
    });
  }
}

function executeAsync(func) {
  setTimeout(func, 0);
}

function timeStamp() {
  const d = new Date;
  return `${d.getHours().toString().padStart(2, 0)}:${d.getMinutes().toString().padStart(2, 0)}:${
    d.getSeconds().toString().padStart(2, 0)}.${d.getMilliseconds().toString().padStart(3, 0)}`;
}

try {
  module.exports = log;
}
catch (e) { /* Used for unit testing purposes */ }
