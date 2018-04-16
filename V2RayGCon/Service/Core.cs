﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Service
{

    class Core
    {
        #region Singleton
        static readonly Core instV2RCore = new Core();
        public static Core Instance
        {
            get
            {
                return instV2RCore;
            }
        }
        #endregion

        Process v2rayCore;
        Setting setting;

        public event EventHandler OnCoreStatChange;

        Core()
        {
            v2rayCore = null;
            setting = Setting.Instance;
            setting.OnRequireCoreRestart += (s, a) => RestartCore();
        }

        void OverwriteProxySettings(JObject config)
        {
            Lib.Utils.TryParseIPAddr(setting.proxyAddr, out string ip, out int port);
            try
            {
                config["inbound"]["protocol"] = setting.proxyType;
                config["inbound"]["listen"] = ip;
                config["inbound"]["port"] = port;
            }
            catch
            {
                Debug.WriteLine("Core: Can not set local proxy address");
                MessageBox.Show(I18N("CoreCantSetLocalAddr"));
            }

        }

        void RestartCore()
        {
            var index = setting.GetSelectedServerIndex();
            var b64Config = setting.GetServer(index);

            if (string.IsNullOrEmpty(b64Config))
            {
                return;
            }

            string plainText = Lib.Utils.Base64Decode(b64Config);
            JObject config = JObject.Parse(plainText);
            OverwriteProxySettings(config);
            RestartCore(config.ToString());
        }

        public bool IsRunning()
        {
            return v2rayCore != null;
        }

        public void RestartCore(string config)
        {
            StopCore();

            if (File.Exists(resData("Executable")))
            {
                StartCore(config);
            }
            else
            {
                MessageBox.Show(I18N("ExeNotFound"));
            }
        }

        void StartCore(string config)
        {
            if (v2rayCore != null)
            {
                Debug.WriteLine("v2ray core is running, skip.");
                return;
            }

            Debug.WriteLine("start v2ray core");

            v2rayCore = new Process();
            v2rayCore.StartInfo.FileName = resData("Executable");
            v2rayCore.StartInfo.Arguments = "-config=stdin: -format=json";
            v2rayCore.StartInfo.CreateNoWindow = true;
            v2rayCore.StartInfo.UseShellExecute = false;
            v2rayCore.StartInfo.RedirectStandardOutput = true;
            v2rayCore.StartInfo.RedirectStandardError = true;
            v2rayCore.StartInfo.RedirectStandardInput = true;

            v2rayCore.EnableRaisingEvents = true;
            v2rayCore.Exited += (s, e) =>
            {
                setting.SendLog(I18N("CoreExit"));
            };
            v2rayCore.ErrorDataReceived += (s, e) => setting.SendLog(e.Data);
            v2rayCore.OutputDataReceived += (s, e) => setting.SendLog(e.Data);

            try
            {
                v2rayCore.Start();
                v2rayCore.StandardInput.WriteLine(config);
                v2rayCore.StandardInput.Close();

                // Add to JOB object support win8+ only
                Lib.ChildProcessTracker.AddProcess(v2rayCore);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Excep: {0}", e);
                StopCore();
                MessageBox.Show(I18N("CantLauchCore"));
                return;
            }

            v2rayCore.BeginErrorReadLine();
            v2rayCore.BeginOutputReadLine();
            OnCoreStatChange?.Invoke(this, null);
        }

        public void StopCore()
        {
            if (v2rayCore == null)
            {
                Debug.WriteLine("v2ray-core is not runnig!");
                return;
            }

            Debug.WriteLine("kill v2ray core");
            try
            {
                Lib.Utils.KillProcessAndChildrens(v2rayCore.Id);
            }
            catch { }
            v2rayCore = null;
            OnCoreStatChange?.Invoke(this, null);
        }
    }
}
