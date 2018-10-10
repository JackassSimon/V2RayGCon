﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Service
{
    public class Servers : Model.BaseClass.SingletonService<Servers>
    {
        public event EventHandler
            OnRequireMenuUpdate,
            OnRequireStatusBarUpdate,
            OnRequireFlyPanelUpdate;

        List<Controller.ServerCtrl> serverList = null;
        List<string> markList = null;

        Lib.CancelableTimeout
            lazyGCTimer = null,
            lazySaveServerListTimer = null,
            lazyUpdateNotifyTextTimer = null;

        object serverListWriteLock = new object();

        int preRunningServerCount = 0;

        Setting setting = null;

        Servers()
        {
            isTesting = false;
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

        List<Controller.ServerCtrl> GetSelectedServerList(bool descending = false)
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

        Tuple<bool, List<string[]>> ImportSSLinks(string text, string mark = "")
        {
            var isAddNewServer = false;
            var links = Lib.Utils.ExtractLinks(text, Model.Data.Enum.LinkTypes.ss);
            List<string[]> result = new List<string[]>();

            foreach (var link in links)
            {
                var config = Lib.Utils.SSLink2Config(link);

                if (config == null)
                {
                    result.Add(GenImportResult(link, false, I18N("DecodeFail"), mark));
                    continue;
                }

                if (AddServer(Lib.Utils.Config2String(config), mark, true))
                {
                    isAddNewServer = true;
                    result.Add(GenImportResult(link, true, I18N("Success"), mark));
                }
                else
                {
                    result.Add(GenImportResult(link, false, I18N("DuplicateServer"), mark));
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
                            result.Add(GenImportResult(link, true, I18N("Success"), mark));
                        }
                        else
                        {
                            result.Add(GenImportResult(link, false, I18N("DuplicateServer"), mark));
                        }
                    }
                }
                catch
                {
                    // skip if error occured
                    result.Add(GenImportResult(link, false, I18N("DecodeFail"), mark));
                }
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, result);
        }

        Tuple<bool, List<string[]>> ImportVmessLinks(string text, string mark = "")
        {
            var links = Lib.Utils.ExtractLinks(text, Model.Data.Enum.LinkTypes.vmess);
            var result = new List<string[]>();
            var isAddNewServer = false;

            foreach (var link in links)
            {
                var vmess = Lib.Utils.VmessLink2Vmess(link);
                var config = Lib.Utils.Vmess2Config(vmess);

                if (config == null)
                {
                    result.Add(GenImportResult(link, false, I18N("DecodeFail"), mark));
                    continue;
                }

                if (AddServer(Lib.Utils.Config2String(config), mark, true))
                {
                    result.Add(GenImportResult(link, true, I18N("Success"), mark));
                    isAddNewServer = true;
                }
                else
                {
                    result.Add(GenImportResult(link, false, I18N("DuplicateServer"), mark));
                }
            }

            return new Tuple<bool, List<string[]>>(isAddNewServer, result);
        }

        void LazySaveServerList()
        {
            // create on demand
            if (lazySaveServerListTimer == null)
            {
                var delay = Lib.Utils.Str2Int(StrConst("LazySaveServerListDelay"));

                lazySaveServerListTimer =
                    new Lib.CancelableTimeout(
                        () => setting.SaveServerList(serverList),
                        delay * 1000);
            }

            lazySaveServerListTimer.Start();

            LazyUpdateNotifyText();
        }

        void LazyUpdateNotifyText()
        {
            if (lazyUpdateNotifyTextTimer == null)
            {
                lazyUpdateNotifyTextTimer =
                    new Lib.CancelableTimeout(
                        UpdateNotifierText,
                        1000);
            }

            lazyUpdateNotifyTextTimer.Start();
        }

        void UpdateNotifierText()
        {
            var list = serverList
                .Where(s => s.isServerOn)
                .OrderBy(s => s.index)
                .ToList();

            var count = list.Count;
            if (count == preRunningServerCount)
            {
                return;
            }
            preRunningServerCount = count;

            if (count <= 0)
            {
                setting.UpdateNotifierText();
                return;
            }

            if (count < 4)
            {

                var textList = new List<string>();

                Action done = () =>
                {
                    var text = string.Join(System.Environment.NewLine, textList);
                    setting.UpdateNotifierText(text);
                };

                Action<int, Action> worker = (index, next) =>
                {
                    list[index].GetProxyAddrForNotifierThen((s) =>
                    {
                        textList.Add(s);
                        next();
                    });
                };

                Lib.Utils.ChainActionHelperAsync(count, worker, done);
                return;
            }

            setting.UpdateNotifierText(count.ToString() + I18N("ServersAreRunning"));
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
                    server.parent = null;
                    serverList.RemoveAt(index);
                }
                next?.Invoke();
            });
        }

        JObject ExtractOutboundInfoFromConfig(string configString, string id, int portBase, int index, string tagPrefix)
        {
            var cache = Service.Cache.Instance;
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
            var vnext = Service.Cache.Instance.tpl.LoadPackage("vnext");
            vnext["outbound"]["settings"]["vnext"][0]["port"] = basePort + index;
            vnext["outbound"]["settings"]["vnext"][0]["users"][0]["id"] = id;
            return vnext;
        }
        #endregion

        #region public method
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

        public void LazyGC()
        {
            // Create on demand.
            if (lazyGCTimer == null)
            {
                var delay = Lib.Utils.Str2Int(StrConst("LazyGCDelay"));

                lazyGCTimer = new Lib.CancelableTimeout(
                    () =>
                    {
                        System.GC.Collect();
                    }, delay * 1000);
            }

            lazyGCTimer.Start();
        }

        public ReadOnlyCollection<Controller.ServerCtrl> GetServerList()
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

            LazyGC();

            if (allResults.Count > 0)
            {
                new Views.WinForms.FormImportLinksResult(allResults);
                Application.Run();
            }
            else
            {
                MessageBox.Show(I18N("NoLinkFound"));
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

        public void SaveServerListImmediately()
        {
            lazySaveServerListTimer?.Timeout();
        }

        public void DisposeLazyTimers()
        {
            lazyGCTimer?.Release();
            lazySaveServerListTimer?.Release();
            lazyUpdateNotifyTextTimer?.Release();
        }

        public bool IsSelecteAnyServer()
        {
            return serverList.Any(s => s.isSelected);
        }

        public void PackSelectedServers()
        {
            var cache = Service.Cache.Instance;
            var list = GetSelectedServerList(true);

            var packages = JObject.Parse(@"{}");
            var serverNameList = new List<string>();

            var id = Guid.NewGuid().ToString();
            var port = Lib.Utils.Str2Int(StrConst("PacmanInitPort"));
            var tagPrefix = StrConst("PacmanTagPrefix");

            Action done = () =>
            {
                var config = cache.tpl.LoadPackage("main");
                config["v2raygcon"]["description"] = string.Join(" ", serverNameList);
                Lib.Utils.UnionJson(ref config, packages);
                OnSendLogHandler(this, new Model.Data.StrEvent(I18N("PackageDone")));
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
                    OnSendLogHandler(this, new Model.Data.StrEvent(I18N("PackageSuccess") + ": " + server.name));
                }
                catch
                {
                    OnSendLogHandler(this, new Model.Data.StrEvent(I18N("PackageFail") + ": " + server.name));
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
                Lib.Utils.ExecuteInParallel<Controller.ServerCtrl, bool>(list, (server) =>
                {
                    server.RunSpeedTest();
                    return true;
                });

                isTesting = false;
                MessageBox.Show(I18N("SpeedTestFinished"));
            });

            return true;
        }

        public List<Controller.ServerCtrl> GetActiveServersList()
        {
            return serverList.Where(s => s.isServerOn).ToList();
        }

        public void RestartServersByListThen(List<Controller.ServerCtrl> servers, Action done = null)
        {
            var list = servers;
            Action<int, Action> worker = (index, next) =>
            {
                list[index].RestartCoreThen(next);
            };

            Lib.Utils.ChainActionHelperAsync(list.Count, worker, done);
        }

        public void WakeupAutorunServersThen(Action done = null)
        {
            Action<int, Action> worker = (index, next) =>
            {
                if (serverList[index].isAutoRun)
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
                else { next(); }
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, lambda);
        }

        public void DeleteSelectedServersThen(Action done = null)
        {
            if (isTesting)
            {
                MessageBox.Show(I18N("LastTestNoFinishYet"));
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
                MessageBox.Show(I18N("LastTestNoFinishYet"));
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
                LazyGC();
                LazySaveServerList();
                InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
            };

            Lib.Utils.ChainActionHelperAsync(serverList.Count, worker, done);
        }

        public void Prepare(Setting setting)
        {
            this.setting = setting;
            this.serverList = setting.LoadServerList();

            foreach (var server in serverList)
            {
                server.parent = this;
                BindEventsTo(server);
            }
        }

        public void DeleteServerByConfig(string config)
        {
            if (isTesting)
            {
                MessageBox.Show(I18N("LastTestNoFinishYet"));
                return;
            }

            var index = GetServerIndexByConfig(config);
            if (index < 0)
            {
                MessageBox.Show(I18N("CantFindOrgServDelFail"));
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

        public void BindEventsTo(Controller.ServerCtrl server)
        {
            server.OnLog += OnSendLogHandler;
            server.OnPropertyChanged += ServerItemPropertyChangedHandler;
            server.OnRequireMenuUpdate += InvokeEventOnRequireMenuUpdate;
            server.OnRequireStatusBarUpdate += InvokeEventOnRequireStatusBarUpdate;
        }

        public bool IsServerItemExist(string config)
        {
            return serverList.Any(s => s.config == config);
        }

        public void ReleaseEventsFrom(Controller.ServerCtrl server)
        {
            server.OnLog -= OnSendLogHandler;
            server.OnPropertyChanged -= ServerItemPropertyChangedHandler;
            server.OnRequireMenuUpdate -= InvokeEventOnRequireMenuUpdate;
            server.OnRequireStatusBarUpdate -= InvokeEventOnRequireStatusBarUpdate;
        }

        public bool AddServer(string config, string mark, bool quiet = false)
        {
            // duplicate
            if (IsServerItemExist(config))
            {
                return false;
            }

            var newServer = new Controller.ServerCtrl()
            {
                config = config,
                mark = mark,
            };

            lock (serverListWriteLock)
            {
                serverList.Add(newServer);
            }

            newServer.parent = this;
            BindEventsTo(newServer);

            if (!quiet)
            {
                newServer.UpdateSummaryThen(() =>
                {
                    InvokeEventOnRequireMenuUpdate(this, EventArgs.Empty);
                    InvokeEventOnRequireFlyPanelUpdate(this, EventArgs.Empty);
                });

            }

            LazyGC();
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
                MessageBox.Show(I18N("Done"));
            });
        }
#endif
        #endregion
    }
}
