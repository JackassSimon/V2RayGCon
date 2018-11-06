﻿using System;
using System.Reflection;
using System.Windows.Forms;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Service
{
    class Notifier : Model.BaseClass.SingletonService<Notifier>
    {
        NotifyIcon ni;
        Setting setting;
        Servers servers;
        PacServer pacServer;


        Notifier() { }

        public void Run(Setting setting, Servers servers, PacServer pacServer)
        {
            this.setting = setting;
            this.servers = servers;
            this.pacServer = pacServer;

            CreateNotifyIcon();
            setting.OnRequireNotifyTextUpdate += OnRequireNotifyTextUpdateHandler;
            pacServer.OnPACServerStateChanged += OnRequireNotifyTextUpdateHandler;

            ni.MouseClick += (s, a) =>
            {
                if (a.Button == MouseButtons.Left)
                {
                    Views.WinForms.FormMain.GetForm();
                }
            };

            OnRequireNotifyTextUpdateHandler(this, EventArgs.Empty);
        }

        #region public method
#if DEBUG
        public void InjectDebugMenuItem(ToolStripMenuItem menu)
        {
            ni.ContextMenuStrip.Items.Insert(0, new ToolStripSeparator());
            ni.ContextMenuStrip.Items.Insert(0, menu);
        }
#endif

        ToolStripMenuItem pluginMenu = null;
        public void UpdatePluginMenu(ToolStripMenuItem pluginMenu)
        {
            if (pluginMenu == null)
            {
                throw new ArgumentException("Plugin menu must not null!");
            }

            if (this.pluginMenu == null)
            {
                ni.ContextMenuStrip.Items.Insert(0, new ToolStripSeparator());
            }
            else
            {
                ni.ContextMenuStrip.Items.Remove(pluginMenu);
            }

            this.pluginMenu = pluginMenu;
            ni.ContextMenuStrip.Items.Insert(0, pluginMenu);
        }

        public void Cleanup()
        {
            ni.Visible = false;
            setting.OnRequireNotifyTextUpdate -= OnRequireNotifyTextUpdateHandler;
            pacServer.OnPACServerStateChanged -= OnRequireNotifyTextUpdateHandler;
        }
        #endregion

        #region private method
        string GetterSysProxyInfo()
        {
            var proxy = Lib.Sys.ProxySetter.GetProxySetting();
            var pacUrl = proxy.autoConfigUrl;

            if (!string.IsNullOrEmpty(pacUrl))
            {
                var param = Lib.Utils.GetProxyParamsFromUrl(pacUrl);
                if (param == null)
                {
                    return string.Format("{0} {1}",
                        I18N.PacProxy,
                        Lib.Utils.CutStr(pacUrl, 32));
                }

                return string.Format(
                    "{0} {1}://{2}:{3}",
                    I18N.PacProxy,
                    (param.isSocks ? "socks5" : "http"),
                    param.ip,
                    param.port.ToString());
            }

            if (proxy.proxyEnable)
            {
                return string.Format(
                    "{0} http://{1}",
                    I18N.GlobalProxy,
                    proxy.proxyServer);
            }

            return string.Empty;
        }

        void OnRequireNotifyTextUpdateHandler(object sender, EventArgs args)
        {
            var proxyInfo = GetterSysProxyInfo();
            var servInfo = setting.runningServerSummary;
            var text = string.Empty;

            if (!string.IsNullOrEmpty(proxyInfo))
            {
                text += proxyInfo + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(servInfo))
            {
                text += servInfo;
            }

            UpdateNotifyText(text);
        }

        private void UpdateNotifyText(string rawText)
        {
            // https://stackoverflow.com/questions/579665/how-can-i-show-a-systray-tooltip-longer-than-63-chars
            var text = string.IsNullOrEmpty(rawText) ?
                I18N.Description :
                Lib.Utils.CutStr(rawText, 127);

            if (ni.Text == text)
            {
                return;
            }

            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }

        void CreateNotifyIcon()
        {
            ni = new NotifyIcon
            {
                Text = I18N.Description,
#if DEBUG
                Icon = Properties.Resources.icon_light,
#else
                Icon = Properties.Resources.icon_dark,
#endif
                BalloonTipTitle = Properties.Resources.AppName,

                ContextMenuStrip = CreateMenu(),
                Visible = true
            };
        }

        ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var factor = Lib.UI.GetScreenScalingFactor();
            if (factor > 1)
            {
                menu.ImageScalingSize = new System.Drawing.Size(
                    (int)(menu.ImageScalingSize.Width * factor),
                    (int)(menu.ImageScalingSize.Height * factor));
            }

            menu.Items.AddRange(new ToolStripMenuItem[] {
                new ToolStripMenuItem(
                    I18N.MainWin,
                    Properties.Resources.WindowsForm_16x,
                    (s,a)=>Views.WinForms.FormMain.GetForm()),

                new ToolStripMenuItem(
                    I18N.OtherWin,
                    Properties.Resources.CPPWin32Project_16x,
                    new ToolStripMenuItem[]{
                        new ToolStripMenuItem(
                            I18N.ConfigEditor,
                            Properties.Resources.EditWindow_16x,
                            (s,a)=>new Views.WinForms.FormConfiger() ),
                        new ToolStripMenuItem(
                            I18N.GenQRCode,
                            Properties.Resources.AzureVirtualMachineExtension_16x,
                            (s,a)=>Views.WinForms.FormQRCode.GetForm()),
                        new ToolStripMenuItem(
                            I18N.Log,
                            Properties.Resources.FSInteractiveWindow_16x,
                            (s,a)=>Views.WinForms.FormLog.GetForm() ),
                        new ToolStripMenuItem(
                            I18N.Options,
                            Properties.Resources.Settings_16x,
                            (s,a)=>Views.WinForms.FormOption.GetForm() ),
                    }),

                new ToolStripMenuItem(
                    I18N.ScanQRCode,
                    Properties.Resources.ExpandScope_16x,
                    (s,a)=>{
                        void Success(string link)
                        {
                            // no comment ^v^
                            if (link == StrConst.Nobody3uVideoUrl)
                            {
                                Lib.UI.VisitUrl(I18N.VisitWebPage, StrConst.Nobody3uVideoUrl);
                                return;
                            }

                            var msg=Lib.Utils.CutStr(link,90);
                            setting.SendLog($"QRCode: {msg}");
                            servers.ImportLinks(link);
                        }

                        void Fail()
                        {
                            MessageBox.Show(I18N.NoQRCode);
                        }

                        Lib.QRCode.QRCode.ScanQRCode(Success,Fail);
                    }),

                new ToolStripMenuItem(
                    I18N.ImportLink,
                    Properties.Resources.CopyLongTextToClipboard_16x,
                    (s,a)=>{
                        string links = Lib.Utils.GetClipboardText();
                        servers.ImportLinks(links);
                    }),

                new ToolStripMenuItem(
                    I18N.DownloadV2rayCore,
                    Properties.Resources.ASX_TransferDownload_blue_16x,
                    (s,a)=>Views.WinForms.FormDownloadCore.GetForm()),
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.AddRange(new ToolStripMenuItem[] {
                new ToolStripMenuItem(
                    I18N.Help,
                    Properties.Resources.StatusHelp_16x,
                    (s,a)=>Lib.UI.VisitUrl(I18N.VistWikiPage,Properties.Resources.WikiLink)),

                new ToolStripMenuItem(
                    I18N.Exit,
                    Properties.Resources.CloseSolution_16x,
                    (s,a)=>{
                        if (Lib.UI.Confirm(I18N.ConfirmExitApp)){
                            Application.Exit();
                        }
                    })
            });

            return menu;
        }
        #endregion
    }
}
