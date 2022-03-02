using System;
using System.Collections.Generic;
using System.IO;
using BluePrism.ApplicationManager.AppMan.Service.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace BluePrism.ApplicationManager.AppMan.Service
{
    public class AppManController
    {
        private readonly Stream _controllerStream;
        private readonly object _controllerStreamLock = new object();
        // An event which is set if we are disconnected from the target application
        public Dictionary<Guid, AppMan> AppMans { get; } = new Dictionary<Guid, AppMan>();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public AppManController(Stream controllerStream)
        {
            _controllerStream = controllerStream;
            Log.Debug("Ica Channel Created");
        }

        public void BeginReadThread(string[] args)
        {
            try
            {
                Log.Debug("Beginning Read Thread");
                using (var reader = new StreamReader(_controllerStream))
                {
                    var canRead = true;
                    var handshakeComplete = false;
                    while (canRead)
                    {
                        var line = reader.ReadLine();
                        canRead = !string.IsNullOrWhiteSpace(line);

                        if (canRead && line == "AppmanInit")
                        {
                            if (handshakeComplete)
                            {
                                continue;
                            }

                            using (var _controllerWriter = new StreamWriter(_controllerStream, System.Text.Encoding.UTF8, 13, true) { AutoFlush = true })
                            {
                                _controllerWriter.WriteLine("AppmanInit OK");
                                handshakeComplete = true;
                                Log.Debug("Handshake Complete");
                                continue;
                            }
                        }

                        var message = GetMessageObjectFromLine(line);
                        if (!canRead || message == null)
                        {
                            AttemptProcessForceKillMessage(line);
                            continue;
                        }

                        var citrixMessage = message.ToObject<CitrixAppManMessage>();
                        Log.Debug($"citrixMessage Successfully converted: {citrixMessage}");

                        Log.Debug(citrixMessage.Id);
                        CreateAppMan(args, citrixMessage);

                        Log.Debug($"Writing to AppMan writer: {citrixMessage.Id}");
                        AppMans[citrixMessage.Id].AppManWriter.WriteLine(citrixMessage.Query);
                    }

                    Log.Info("Exited Read loop");
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"Error: {ex}");
            }
        }

        private JObject GetMessageObjectFromLine(string line)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<JObject>(line);
                Log.Debug($"message successfully deserialized: {message}");
                return message;
            }
            catch
            {
                Log.Debug($"message failed to be deserialized: {line}");
                return null;
            }
        }

        private void OnAppManDisconnect(object sender, DisconnectedEventArgs args)
        {
            AppMans.Remove(args.AppManId);
            ((AppMan)sender).Dispose();
            Log.Debug($"{args.AppManId} removed from dictionary");
        }

        private void AttemptProcessForceKillMessage(string line)
        {
            if (AppManKillHeader.ContainsHeader(line))
            {
                ProcessForceKillMessage(line);
            }
        }

        private void ProcessForceKillMessage(string line)
        {
            Log.Debug($"ForceKill received - {line}");
            var appManId = AppManKillHeader.Parse(line).Id;

            if (!AppMans.ContainsKey(appManId))
            {
                return;
            }

            AppMans[appManId].Close();
            AppMans.Remove(appManId);
        }

        private void CreateAppMan(string[] args, CitrixAppManMessage citrixMessage)
        {
            if (!AppMans.ContainsKey(citrixMessage.Id))
            {
                var argString = string.Join(" ", args);
                Log.Debug($"Creating AppMan: {citrixMessage.Id}");
                var appMan = new AppMan(citrixMessage.Id, _controllerStream, argString,
                    _controllerStreamLock, citrixMessage.Mode);
                appMan.Disconnected += OnAppManDisconnect;
                AppMans.Add(citrixMessage.Id, appMan);
            }
        }
    }
}
