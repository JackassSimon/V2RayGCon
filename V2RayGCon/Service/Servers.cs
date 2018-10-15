﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Service
{
    public class Servers : Model.BaseClass.SingletonService<Servers>
    {
        Setting setting = null;
        PacServer pacServer = null;
        Cache cache = null;

        public event EventHandler<Model.Data.StrEvent>
            OnRequireNotifierUpdate;

        public event EventHandler
            OnRequireMenuUpdate,
            OnRequireStatusBarUpdate,
            OnRequireFlyPanelUpdate;


        List<Controller.CoreServerCtrl> serverList = null;
        List<string> markList = null;

        Lib.Sys.CancelableTimeout
            lazySaveServerListTimer = null,
            lazyUpdateNotifyTextTimer = null;

        object serverListWriteLock = new object();

        Servers()
        {
            isTesting = false;
        }

        public void Run(
           Setting setting,
           PacServer pacServer,
           Cache cache)
        {
            this.cache = cache;
            this.setting = setting;
            this.pacServer = pacServer;
            this.serverList = setting.LoadServerList();

            foreach (var server in serverList)
            {
                server.Run(cache, setting, pacServer, this);
                BindEventsTo(server);
            }
        }

        #region property
        bool _isTesting;
        object _isTestingLock = new object();
        bool isTesting
        {
            get
            {
                lock (_isTestingLock)
                {
                    return _isTesting;
                }
            }
            set
            {
                lock (_isTestingLock)
                {
                    _isTesting = value;
                }
            }
        }
        #endregion

        #region private method

        /// <summary>
        /// update running servers list
        /// </summary>
        /// <param name="includeCurServer"></param>
        Model.Data.ServerTracker GenCurTrackerSetting(string curServer, bool isStart)
        {
            var trackerSetting = setting.GetServerTrackerSetting();
            var tracked = trackerSetting.serverList;

            var running = GetServerList()
                .Where(s => s.isServerOn)
                .Select(s => s.config)
                .ToList();

            tracked.RemoveAll(c => !running.Any(r => r == c));  // remove stopped
            running.RemoveAll(r => tracked.Any(t => t == r));
            tracked.AddRange(running);
            tracked.Remove(curServer);

            if (isStart)
            {
                tracked.Insert(0, curServer);
            }

            trackerSetting.serverList = tracked;
            return trackerSetting;
        }

        void OnRequireKeepTrackHandler(object sender, Model.Data.BoolEvent isServerStart)
        {
            if (!setting.isServerTrackerOn)
            {
                return;
            }

            SetLazyServerTrackerUpdater(() =>
                LazyServerTrackUpdateWorker(
                    sender as Controller.CoreServerCtrl,
                    isServerStart.Data));
        }

        void LazyServerTrackUpdateWorker(
            Controller.CoreServerCtrl servCtrl,
            bool isStart)
        {
            var curTrackerSetting = GenCurTrackerSetting(servCtrl.config, isStart);
            var isGlobal = false;
            curTrackerSetting.curServer = null;

            switch (Service.PacServer.DetectSystemProxyMode(
                setting.GetSysProxySetting()))
            {
                case Model.Data.Enum.SystemProxyMode.None:
                    setting.SaveServerTrackerSetting(curTrackerSetting);
                    return;
                case Model.Data.Enum.SystemProxyMode.Global:
                    isGlobal = true;
                    break;
                case Model.Data.Enum.SystemProxyMode.PAC:
                    isGlobal = false;
                    break;
            }

            foreach (var c in curTrackerSetting.serverList)
            {
                // 按trackerList的顺序来试
                var serv = serverList.FirstOrDefault(s => s.config == c);
                if (serv == null)
                {
                    continue;
                }

                if (serv.BecomeSystemProxy(isGlobal))
                {
                    curTrackerSetting.curServer = serv.config;
                    break;
                }
            }

            // 没有可用服务器时不要清空代理设置
            // 否则全部重启时会丢失代理设置
            if (isStart && curTrackerSetting.curServer != servCtrl.config)
            {
                Task.Factory.StartNew(() => MessageBox.Show(I18N.SetSysProxyFail));
            }

            setting.SaveServerTrackerSetting(curTrackerSetting);
        }

        Lib.Sys.CancelableTimeout lazyServerTrackerTimer = null;
        void SetLazyServerTrackerUpdater(Action onTimeout)
        {
            lazyServerTrackerTimer?.Release();
            lazyServerTrackerTimer = null;
            lazyServerTrackerTimer = new Lib.Sys.CancelableTimeout(onTimeout, 2000);
            lazyServerTrackerTimer.Start();
        }

        int GetServerIndexByConfig(string config)
        {
            for (int i = 0; i < serverList.Count; i++)
            {
                if (serverList[i].config == config)
                {
                    return i;
                }
            }
            return -1;
        }

        List<Controller.CoreServerCtrl> GetSelectedServerList(bool descending = false)
        {
            var list = serverList.Where(s => s.isSelected);
            if (descending)
            {
                return list.OrderByDescending(s => s.index).ToList();
            }

            return list.OrderBy(s => s.index).ToList();
        }

        string[] GenImportResult(string link, bool success, string reason, string mark)
        {
            return new string[]
            {
                string.Empty,  // reserve for index
                link,
                mark,
                success?"√":"×",
                reason,
            };
        }

        JObject SSLink2Config(string ssLink)
        {
            Model.Data.Shadowsocks ss = Lib.Utils.SSLink2SS(ssLink);
            if (ss == null)
            {
                return null;
            }

            Lib.Utils.TryParseIPAddr(ss.addr, out string ip, out int port);

            var config = cache.tpl.LoadTemplate("tplImportSS");

            var setting = config["outbound"]["settings"]["servers"][0];
            setting["address"] = ip;
            setting["port"] = port;
            setting["method"] = ss.method;
            setting["password"] = ss.pass;

            return config.DeepClone() as JObject;
        }

        Tuple<bool, List<string[]>> ImportSSLinks(string text, string mark = "")
        {
            var isAddNewServer = false;
            var links = Lib.Utils.ExtractLinks(text, Model.Data.Enum.LinkTypes.ss);
            List<string[]> result = new List<string[]>();

            foreach (var link in links)
            {
                var config = SSLink2Config(link);

                if (config == null)
                {
                    result.Add(GenImportResult(link, false, I18N.DecodeFail, mark));
                    continue;
                }

                if (AddServer(Lib.Utils.Config2String(config), mark, true))
                {
                    isAddNewServer = true;
                    result.Add(GenImportResult(link, true, I18N.Success, mark));
                }
                else
                {
                    result.Add(GenImportResult(link, false, I18N.DuplicateServer, mark));
                }
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, result);
        }

        Tuple<bool, List<string[]>> ImportV2RayLinks(string text, string mark = "")
        {
            bool isAddNewServer = false;
            var links = Lib.Utils.ExtractLinks(text, Model.Data.Enum.LinkTypes.v2ray);
            List<string[]> result = new List<string[]>();

            foreach (var link in links)
            {
                try
                {
                    var config = JObject.Parse(
                        Lib.Utils.Base64Decode(
                            Lib.Utils.GetLinkBody(link)));

                    if (config != null)
                    {
                        if (AddServer(Lib.Utils.Config2String(config), mark, true))
                        {
                            isAddNewServer = true;
                            result.Add(GenImportResult(link, true, I18N.Success, mark));
                        }
                        else
                        {
                            result.Add(GenImportResult(link, false, I18N.DuplicateServer, mark));
                        }
                    }
                }
                catch
                {
                    // skip if error occured
                    result.Add(GenImportResult(link, false, I18N.DecodeFail, mark));
                }
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, result);
        }

        JObject Vmess2Config(Model.Data.Vmess vmess)
        {
            if (vmess == null)
            {
                return null;
            }

            // prepare template
            var config = cache.tpl.LoadTemplate("tplImportVmess");
            config["v2raygcon"]["alias"] = vmess.ps;

            var cPos = config["outbound"]["settings"]["vnext"][0];
            cPos["address"] = vmess.add;
            cPos["port"] = Lib.Utils.Str2Int(vmess.port);
            cPos["users"][0]["id"] = vmess.id;
            cPos["users"][0]["alterId"] = Lib.Utils.Str2Int(vmess.aid);

            // insert stream type
            string[] streamTypes = { "ws", "tcp", "kcp", "h2" };
            string streamType = vmess.net.ToLower();

            if (!streamTypes.Contains(streamType))
            {
                return config.DeepClone() as JObject;
            }

            config["outbound"]["streamSettings"] =
                cache.tpl.LoadTemplate(streamType);

            try
            {
                switch (streamType)
                {
                    case "kcp":
                        config["outbound"]["streamSettings"]["kcpSettings"]["header"]["type"] = vmess.type;
                        break;
                    case "ws":
                        config["outbound"]["streamSettings"]["wsSettings"]["path"] =
                            string.IsNullOrEmpty(vmess.v) ? vmess.host : vmess.path;
                        if (vmess.v == "2" && !string.IsNullOrEmpty(vmess.host))
                        {
                            config["outbound"]["streamSettings"]["wsSettings"]["headers"]["Host"] = vmess.host;
                        }
                        break;
                    case "h2":
                        config["outbound"]["streamSettings"]["httpSettings"]["path"] = vmess.path;
                        config["outbound"]["streamSettings"]["httpSettings"]["host"] = Lib.Utils.Str2JArray(vmess.host);
                        break;
                }

            }
            catch { }

            try
            {
                // must place at the end. cos this key is add by streamSettings
                config["outbound"]["streamSettings"]["security"] = vmess.tls;
            }
            catch { }
            return config.DeepClone() as JObject;
        }

        Tuple<bool, List<string[]>> ImportVmessLinks(string text, string mark = "")
        {
            var links = Lib.Utils.ExtractLinks(text, Model.Data.Enum.LinkTypes.vmess);
            var result = new List<string[]>();
            var isAddNewServer = false;

            foreach (var link in links)
            {
                var vmess = Lib.Utils.VmessLink2Vmess(link);
                var config = Vmess2Config(vmess);

                if (config == null)
                {
                    result.Add(GenImportResult(link, false, I18N.DecodeFail, mark));
                    continue;
                }

                if (AddServer(Lib.Utils.Config2String(config), mark, true))
                {
                    result.Add(GenImportResult(link, true, I18N.Success, mark));
                    isAddNewServer = true;
                }
                else
                {
                    result.Add(GenImportResult(link, false, I18N.DuplicateServer, mark));
                }
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, result);
        }

        void LazySaveServerList()
        {
            // create on demand
            if (lazySaveServerListTimer == null)
            {
                var delay = Lib.Utils.Str2Int(StrConst.LazySaveServerListDelay);

                lazySaveServerListTimer =
                    new Lib.Sys.CancelableTimeout(
                        () => setting.SaveServerList(serverList),
                        delay * 1000);
            }

            lazySaveServerListTimer.Start();
        }

        void LazyUpdateNotifyTextHandler(object sender, EventArgs args)
        {
            if (lazyUpdateNotifyTextTimer == null)
            {
                lazyUpdateNotifyTextTimer =
                    new Lib.Sys.CancelableTimeout(
                        UpdateNotifierText,
                        2000);
            }

            lazyUpdateNotifyTextTimer.Start();
        }

        void InvokeEventOnRequireNotifierUpdate(string text)
        {
            try
            {
                OnRequireNotifierUpdate?.Invoke(this, new Model.Data.StrEvent(text));
            }
            catch { }
        }

        void UpdateNotifierText()
        {
            var list = serverList
                .Where(s => s.isServerOn)
                .OrderBy(s => s.index)
                .ToList();

            var count = list.Count;

            if (count <= 0 || count > 3)
            {
                InvokeEventOnRequireNotifierUpdate(
                    count <= 0 ?
                    I18N.Description :
                    count.ToString() + I18N.ServersAreRunning);
                return;
            }

            var texts = new List<string>();

            Action done = () =>
            {
                InvokeEventOnRequireNotifierUpdate(
                    string.Join(Environment.NewLine, texts));
            };

            Action<int, Action> worker = (index, next) =>
            {
                list[index].GetterInboundInfoFor((s) =>
                {
                    texts.Add(s);
                    next?.Invoke();
                });
            };

            Lib.Utils.ChainActionHelperAsync(count, worker, done);
        }

        void InvokeEventOnRequireStatusBarUpdate(object sender, EventArgs args)
        {
            try
            {
                OnRequireStatusBarUpdate?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        void InvokeEventOnRequireMenuUpdate(object sender, EventArgs args)
        {
            try
            {
                OnRequireMenuUpdate?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        void OnSendLogHandler(object sender, Model.Data.StrEvent arg)
        {
            setting.SendLog(arg.Data);
        }

        void ServerItemPropertyChangedHandler(object sender, EventArgs arg)
        {
            LazySaveServerList();
        }

        void RemoveServerItemFromListThen(int index, Action next = null)
        {
            var server = serverList[index];
            server.CleanupThen(() =>
            {
                lock (serverListWriteLock)
                {
                    ReleaseEventsFrom(server);
                    serverList.RemoveAt(index);
                }
                next?.Invoke();
            });
        }

        JObject ExtractOutboundInfoFromConfig(string configString, string id, int portBase, int index, string tagPrefix)
        {
            var pkg = cache.tpl.LoadPackage("package");
            var config = Lib.ImportParser.Parse(configString);

            var tagin = tagPrefix + "in" + index.ToString();
            var tagout = tagPrefix + "out" + index.ToString();
            var port = portBase + index;

            pkg["routing"]["settings"]["rules"][0]["inboundTag"][0] = tagin;
            pkg["routing"]["settings"]["rules"][0]["outboundTag"] = tagout;

            pkg["inboundDetour"][0]["port"] = port;
            pkg["inboundDetour"][0]["tag"] = tagin;
            pkg["inboundDetour"][0]["settings"]["port"] = port;
            pkg["inboundDetour"][0]["settings"]["clients"][0]["id"] = id;

            pkg["outboundDetour"][0]["protocol"] = config["outbound"]["protocol"];
            pkg["outboundDetour"][0]["tag"] = tagout;
            pkg["outboundDetour"][0]["settings"] = config["outbound"]["settings"];
            pkg["outboundDetour"][0]["streamSettings"] = config["outbound"]["streamSettings"];

            return pkg;
        }

        JObject GenVnextConfigPart(int index, int basePort, string id)
        {
            var vnext = cache.tpl.LoadPackage("vnext");
            vnext["outbound"]["settings"]["vnext"][0]["port"] = basePort + index;
            vnext["outbound"]["settings"]["vnext"][0]["users"][0]["id"] = id;
            return vnext;
        }
        #endregion

        #region public method
        public void Cleanup()
        {
            setting.isServerTrackerOn = false;
            lazySaveServerListTimer?.Timeout();
            DisposeLazyTimers();
            AutoResetEvent sayGoodbye = new AutoResetEvent(false);
            StopAllServersThen(() => sayGoodbye.Set());
            sayGoodbye.WaitOne();
        }

        public int GetTotalSelectedServerCount()
        {
            return serverList.Count(s => s.isSelected);
        }

        public int GetTotalServerCount()
        {
            return serverList.Count;
        }

        public void InvokeEventOnRequireFlyPanelUpdate(object sender, EventArgs args)
        {
            try
            {
                OnRequireFlyPanelUpdate?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        public ReadOnlyCollection<string> GetMarkList()
        {
            if (this.markList == null)
            {
                UpdateMarkList();
            }
            return markList.AsReadOnly();
        }

        public void SetAllServerIsSelected(bool isSelected)
        {
            serverList
                .Select(s =>
                {
                    s.SetIsSelected(isSelected);
                    return true;
                })
                .ToList();
        }

        public void UpdateMarkList()
        {
            markList = serverList
                .Select(s => s.mark)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        public void RestartInjectImportServers()
        {
            var list = serverList
                .Where(s => s.isInjectImport && s.isServerOn)
                .OrderBy(s => s.index)
                .ToList();

            RestartServersByListThen(list);
        }

        public ReadOnlyCollection<Controller.CoreServerCtrl> GetServerList()
        {
            return serverList.OrderBy(s => s.index).ToList().AsReadOnly();
        }

        public bool IsEmpty()
        {
            return !(this.serverList.Any());
        }

        public void ImportLinks(List<string[]> linkList)
        {
            // linkList:[ linksString, mark] 
            var taskList = new List<Task<Tuple<bool, List<string[]>>>>();

            foreach (var link in linkList)
            {
                taskList.Add(new Task<Tuple<bool, List<string[]>>>(
                    () => ImportVmessLinks(link[0], link[1])));
                taskList.Add(new Task<Tuple<bool, List<string[]>>>(
                    () => ImportSSLinks(link[0], link[1])));
                taskList.Add(new Task<Tuple<bool, List<string[]>>>(
                    () => ImportV2RayLinks(link[0], link[1])));
            }

            var tasks = taskList.ToArray();
            Task.Factory.StartNew(() =>
            {
                foreach (var task in tasks)
                {
                    task.Start();
                }
                Task.WaitAll(tasks);

                var results = GetterImportLinksResult(tasks);
                UpdateMarkList();
                ShowImportLinksResult(results);
            });
        }

        private void ShowImportLinksResult(Tuple<bool, List<string[]>> results)
        {
            var isAddNewServer = results.Item1;
            var allResults = results.Item2;

            if (isAddNewServer)
            {
                UpdateAllServersSummary();
                LazySaveServerList();
            }

            setting.LazyGC();

            if (allResults.Count > 0)
            {
                new Views.WinForms.FormImportLinksResult(allResults);
                Application.Run();
            }
            else
            {
                MessageBox.Show(I18N.NoLinkFound);
            }
        }

        private static Tuple<bool, List<string[]>> GetterImportLinksResult(Task<Tuple<bool, List<string[]>>>[] tasks)
        {
            var allResults = new List<string[]>();
            var isAddNewServer = false;
            foreach (var task in tasks)
            {
                isAddNewServer = isAddNewServer || task.Result.Item1;
                allResults.AddRange(task.Result.Item2);
                task.Dispose();
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, allResults);
        }

        public void ImportLinks(string links)
        {
            var tasks = new Task<Tuple<bool, List<string[]>>>[] {
                new Task<Tuple<bool, List<string[]>>>(
                    ()=>ImportVmessLinks(links)),

                new Task<Tuple<bool, List<string[]>>>(
                    ()=>ImportV2RayLinks(links)),

                new Task<Tuple<bool, List<string[]>>>(
                    ()=>ImportSSLinks(links)),
            };

            Task.Factory.StartNew(() =>
            {
                foreach (var task in tasks)
                {
                    task.Start();
                }
                Task.WaitAll(tasks);

                var results = GetterImportLinksResult(tasks);
                ShowImportLinksResult(results);
            });
        }

        public void DisposeLazyTimers()
        {
            lazyServerTrackerTimer?.Release();
            lazySaveServerListTimer?.Release();
            lazyUpdateNotifyTextTimer?.Release();
        }

        public bool IsSelecteAnyServer()
        {
            return serverList.Any(s => s.isSelected);
        }

        public void PackSelectedServers()
        {
            var list = GetSelectedServerList(true);

            var packages = JObject.Parse(@"{}");
            var serverNameList = new List<string>();

            var id = Guid.NewGuid().ToString();
            var port = Lib.Utils.Str2Int(StrConst.PacmanInitPort);
            var tagPrefix = StrConst.PacmanTagPrefix;

            Action done = () =>
            {
                var config = cache.tpl.LoadPackage("main");
                config["v2raygcon"]["description"] = string.Join(" ", serverNameList);
                Lib.Utils.UnionJson(ref config, packages);
                OnSendLogHandler(this, new Model.Data.StrEvent(I18N.PackageDone));
                AddServer(config.ToString(Formatting.None), "Package");
                UpdateMarkList();
                Lib.UI.ShowMessageBoxDoneAsync();
            };

            Action<int, Action> worker = (index, next) =>
            {
                var server = list[index];
                try
                {
                    var package = ExtractOutboundInfoFromConfig(server.config, id, port, index, tagPrefix);
                    Lib.Utils.UnionJson(ref packages, package);
                    var vnext = GenVnextConfigPart(index, port, id);
                    Lib.Utils.UnionJson(ref packages, vnext);
                    serverNameList.Add(server.name);
                    OnSendLogHandler(this, new Model.Data.StrEvent(I18N.PackageSuccess + ": " + server.name));
                }
                catch
                {
                    OnSendLogHandler(this, new Model.Data.StrEvent(I18N.PackageFail + ": " + server.name));
                }
                next();
            };

            Lib.Utils.ChainActionHelperAsync(list.Count, worker, done);
        }

        public bool RunSpeedTestOnSelectedServers()
        {
            if (isTesting)
            {
                return false;
            }
            isTesting = true;

            var list = GetSelectedServerList(false);

            Task.Factory.StartNew(() =>
            {
                Lib.Utils.ExecuteInParallel<Controller.CoreServerCtrl, bool>(list, (server) =>
                {
                    server.RunSpeedTest();
                    return true;
                });

                isTesting = false;
                MessageBox.Show(I18N.SpeedTestFinished);
            });

            return true;
        }

        public List<Controller.CoreServerCtrl> GetActiveServersList()
        {
            return serverList.Where(s => s.isServerOn).ToList();
        }

        public void RestartServersByListThen(List<Controller.CoreServerCtrl> servers, Action done = null)
        {
            var list = servers;
            Action<int, Action> worker = (index, next) =>
            {
                list[index].RestartCoreThen(next);
            };

            Lib.Utils.ChainActionHelperAsync(list.Count, worker, done);
        }

        public void WakeupServers()
        {
            List<Controller.CoreServerCtrl> bootList = GenBootServerList();

            Action<int, Action> worker = (index, next) =>
            {
                bootList[index].RestartCoreThen(next);
            };

            Lib.Utils.ChainActionHelperAsync(bootList.Count, worker);
        }

        private List<Controller.CoreServerCtrl> GenBootServerList()
        {
            List<Controller.CoreServerCtrl> result = null;

            var trackerSetting = setting.GetServerTrackerSetting();
            if (trackerSetting.isTrackerOn)
            {
                setting.isServerTrackerOn = true;

                var trackList = trackerSetting.serverList;
                result = serverList.Where(
                    s => s.isAutoRun || trackList.Contains(s.config))
                    .ToList();

                if (trackerSetting.curServer != null)
                {
                    result.RemoveAll(s => s.config == trackerSetting.curServer);
                    var lastServer = serverList.FirstOrDefault(s => s.config == trackerSetting.curServer);
                    if (lastServer != null)
                    {
                        result.Insert(0, lastServer);
                    }
                }
            }
            else
            {
                result = serverList.Where(s => s.isAutoRun).ToList();
            }

            return result;
        }

        public void RestartAllSelectedServersThen(Action done = null)
        {
            Action<int, Action> worker = (index, next) =>
            {
                if (serverList[index].isSelected)
                {
                    serverList[index].RestartCoreThen(next);
                }
                else
                {
                    next();
                }
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, done);
        }

        public void StopAllSelectedThen(Action lambda = null)
        {
            Action<int, Action> worker = (index, next) =>
            {
                if (serverList[index].isSelected)
                {
                    serverList[index].server.StopCoreThen(next);
                }
                else
                {
                    next();
                }
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, lambda);
        }

        public void StopAllServersThen(Action lambda = null)
        {
            Action<int, Action> worker = (index, next) =>
            {
                if (serverList[index].server.isRunning)
                {
                    serverList[index].server.StopCoreThen(next);
                }
                else
                {
                    next();
                }
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, lambda);
        }

        public void DeleteSelectedServersThen(Action done = null)
        {
            if (isTesting)
            {
                MessageBox.Show(I18N.LastTestNoFinishYet);
                return;
            }

            Action<int, Action> worker = (index, next) =>
            {
                if (!serverList[index].isSelected)
                {
                    next();
                    return;
                }

                RemoveServerItemFromListThen(index, next);
            };

            Action finish = () =>
            {
                LazySaveServerList();
                UpdateMarkList();
                InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
                done?.Invoke();
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, finish);
        }

        public void DeleteAllServersThen(Action done = null)
        {
            if (isTesting)
            {
                MessageBox.Show(I18N.LastTestNoFinishYet);
                return;
            }

            Action finish = () =>
            {
                LazySaveServerList();
                UpdateMarkList();
                InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
                done?.Invoke();
            };

            Lib.Utils.ChainActionHelperAsync(
                serverList.Count,
                RemoveServerItemFromListThen,
                finish);
        }

        public void UpdateAllServersSummary()
        {
            Action<int, Action> worker = (index, next) =>
            {
                try
                {
                    serverList[index].UpdateSummaryThen(next);
                }
                catch
                {
                    // skip if something goes wrong
                    next();
                }
            };

            Action done = () =>
            {
                setting.LazyGC();
                LazySaveServerList();
                InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, done);
        }

        public void DeleteServerByConfig(string config)
        {
            if (isTesting)
            {
                MessageBox.Show(I18N.LastTestNoFinishYet);
                return;
            }

            var index = GetServerIndexByConfig(config);
            if (index < 0)
            {
                MessageBox.Show(I18N.CantFindOrgServDelFail);
                return;
            }

            Task.Factory.StartNew(
                () => RemoveServerItemFromListThen(index, () =>
                {
                    LazySaveServerList();
                    UpdateMarkList();
                    InvokeEventOnRequireMenuUpdate(serverList, EventArgs.Empty);
                    InvokeEventOnRequireFlyPanelUpdate(serverList, EventArgs.Empty);
                }));
        }

        public bool IsServerItemExist(string config)
        {
            return serverList.Any(s => s.config == config);
        }

        public void BindEventsTo(Controller.CoreServerCtrl server)
        {
            server.OnRequireKeepTrack += OnRequireKeepTrackHandler;
            server.OnLog += OnSendLogHandler;
            server.OnPropertyChanged += ServerItemPropertyChangedHandler;
            server.OnRequireMenuUpdate += InvokeEventOnRequireMenuUpdate;
            server.OnRequireStatusBarUpdate += InvokeEventOnRequireStatusBarUpdate;
            server.OnRequireNotifierUpdate += LazyUpdateNotifyTextHandler;
        }

        public void ReleaseEventsFrom(Controller.CoreServerCtrl server)
        {
            server.OnRequireKeepTrack -= OnRequireKeepTrackHandler;
            server.OnLog -= OnSendLogHandler;
            server.OnPropertyChanged -= ServerItemPropertyChangedHandler;
            server.OnRequireMenuUpdate -= InvokeEventOnRequireMenuUpdate;
            server.OnRequireStatusBarUpdate -= InvokeEventOnRequireStatusBarUpdate;
            server.OnRequireNotifierUpdate -= LazyUpdateNotifyTextHandler;
        }

        public bool AddServer(string config, string mark, bool quiet = false)
        {
            // duplicate
            if (IsServerItemExist(config))
            {
                return false;
            }

            var newServer = new Controller.CoreServerCtrl()
            {
                config = config,
                mark = mark,
            };

            lock (serverListWriteLock)
            {
                serverList.Add(newServer);
            }

            newServer.Run(cache, setting, pacServer, this);
            BindEventsTo(newServer);

            if (!quiet)
            {
                newServer.UpdateSummaryThen(() =>
                {
                    InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
                    InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                });
            }

            setting.LazyGC();
            LazySaveServerList();
            return true;
        }

        public bool ReplaceServerConfig(string orgConfig, string newConfig)
        {
            var index = GetServerIndexByConfig(orgConfig);

            if (index < 0)
            {
                return false;
            }

            serverList[index].ChangeConfig(newConfig);
            return true;
        }

        #endregion

        #region debug
#if DEBUG
        public void DbgFastRestartTest(int round)
        {
            var list = serverList.ToList();
            var rnd = new Random();

            var count = list.Count;
            Task.Factory.StartNew(() =>
            {
                var taskList = new List<Task>();
                for (int i = 0; i < round; i++)
                {
                    var index = rnd.Next(0, count);
                    var isStopCore = rnd.Next(0, 2) == 0;
                    var server = list[index];

                    var task = new Task(() =>
                    {
                        AutoResetEvent sayGoodbye = new AutoResetEvent(false);
                        if (isStopCore)
                        {
                            server.StopCoreThen(() => sayGoodbye.Set());
                        }
                        else
                        {
                            server.RestartCoreThen(() => sayGoodbye.Set());
                        }
                        sayGoodbye.WaitOne();
                    });

                    taskList.Add(task);
                    task.Start();
                }

                Task.WaitAll(taskList.ToArray());
                MessageBox.Show(I18N.Done);
            });
        }
#endif
        #endregion
    }
}
