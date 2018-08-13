﻿using System.Diagnostics;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Service
{
    class Notifier : Model.BaseClass.SingletonService<Notifier>
    {
        NotifyIcon ni;
        Core core;
        Setting setting;

        Notifier()
        {
            setting = Setting.Instance;
            core = Core.Instance;
            CreateNotifyIcon();

            Application.ApplicationExit += (s, a) => Cleanup();
            Microsoft.Win32.SystemEvents.SessionEnding += (s, a) => Application.Exit();
            core.OnCoreStatChange += (s, a) => UpdateNotifyText();

#if DEBUG
            This_function_do_some_tedious_stuff();
#else
            ni.MouseClick += (s, a) =>
            {
                if (a.Button == MouseButtons.Left)
                {
                    Views.FormMain.GetForm();
                }
            };

            if (setting.GetServerCount() > 0)
            {
                setting.ActivateServer();
            }
            else
            {
                Views.FormMain.GetForm();
            }
#endif
        }

        #region DEBUG code TL;DR
#if DEBUG
        void This_function_do_some_tedious_stuff()
        {
            ni.DoubleClick += (s, a) =>
            {
                Debug.WriteLine("Some test code:");
            };


            new Views.FormConfiger(0);

            // new Views.FormConfigTester();
            // Views.FormMain.GetForm();
            // Views.FormLog.GetForm();
            // Views.FormSimAddVmessClient.GetForm();
            // Views.FormDownloadCore.GetForm();

        }
#endif
        #endregion

        #region private method
        void UpdateNotifyText()
        {
            var type = setting.proxyType;
            var protocol = Model.Data.Table.proxyTypesString[type];

            var proxy = string.Empty;
            if (type == (int)Model.Data.Enum.ProxyTypes.config)
            {
                proxy = setting.GetInbountInfo();
            }
            else
            {
                proxy = string.Format("{0}://{1}", protocol, setting.proxyAddr);
            }

            var title = string.Format("{0} {1}", setting.GetCurAlias(), proxy);

            ni.Text = core.isRunning ? title : I18N("Description");
        }

        void CreateNotifyIcon()
        {
            ni = new NotifyIcon();
            ni.Text = I18N("Description");
            ni.Icon = Properties.Resources.icon_dark;
            ni.BalloonTipTitle = Properties.Resources.AppName;
            ni.ContextMenu = CreateMenu();
            ni.Visible = true;
        }

        ContextMenu CreateMenu()
        {
            return new ContextMenu(new MenuItem[] {

                new MenuItem(I18N("MainWin"),(s,a)=>Views.FormMain.GetForm()),

                new MenuItem(I18N("OtherWin"),new MenuItem[]{
                    new MenuItem(I18N("ConfigEditor"),(s,a)=>new Views.FormConfiger() ),
                    new MenuItem(I18N("ConfigTester"),(s,a)=>new Views.FormConfigTester() ),
                    new MenuItem(I18N("GenQRCode"),(s,a)=>Views.FormQRCode.GetForm() ),
                    new MenuItem(I18N("Log"),(s,a)=>Views.FormLog.GetForm() ),
                }),

                new MenuItem(I18N("ScanQRCode"),(s,a)=>{
                    void Success(string link)
                    {
                        var msg=Lib.Utils.CutStr(link,90);
                        setting.SendLog($"QRCode: {msg}");
                        setting.ImportLinks(link);
                    }

                    void Fail()
                    {
                        MessageBox.Show(I18N("NoQRCode"));
                    }

                    Lib.QRCode.QRCode.ScanQRCode(Success,Fail);
                }),

                new MenuItem(I18N("ImportLink"),(s,a)=>{
                    string links = Lib.Utils.GetClipboardText();
                    setting.ImportLinks(links);
                }),

                new MenuItem(I18N("DownloadV2rayCore"),(s,a)=>Views.FormDownloadCore.GetForm()),

                new MenuItem("-"),

                new MenuItem(I18N("Help"),(s,a)=>Lib.UI.VisitUrl(I18N("VistWikiPage"),Properties.Resources.WikiLink)),

                new MenuItem(I18N("Exit"),(s,a)=>{
                    if(Lib.UI.Confirm(I18N("ConfirmExitApp"))){
                        Application.Exit();
                    }
                })
            });
        }

        void Cleanup()
        {
            Debug.WriteLine("Call cleanup");
            ni.Visible = false;
            if (setting.isSysProxyHasSet)
            {
                Lib.ProxySetter.setProxy("", false);
                setting.isSysProxyHasSet = false;
            }
            core.StopCoreThen(null);
        }
        #endregion
    }
}
