﻿using System;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Views
{
    public partial class FormMain : Form
    {
        #region Sigleton
        static FormMain _instant;
        public static FormMain GetForm()
        {
            if (_instant == null || _instant.IsDisposed)
            {
                _instant = new FormMain();
            }
            _instant.Activate();
            return _instant;
        }
        #endregion

        Controller.FormMainCtrl formMainCtrl;
        Service.Setting setting;
        Service.Servers servers;

        FormMain()
        {
            setting = Service.Setting.Instance;
            servers = Service.Servers.Instance;

            InitializeComponent();

#if DEBUG
            this.Icon = Properties.Resources.icon_light;
#endif

            this.Show();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            setting.RestoreFormRect(this);

            this.FormClosed += (s, a) =>
            {
                setting.SaveFormRect(this);
                setting.OnSysProxyChanged -= OnSysProxyChangedHandler;
                formMainCtrl.Cleanup();
                servers.LazyGC();
            };

            this.Text = string.Format(
                "{0} v{1}",
                Properties.Resources.AppName,
                Properties.Resources.Version);

            formMainCtrl = InitFormMainCtrl();

            toolMenuItemCurrentSysProxy.Text = GetCurrentSysProxySetting();
            setting.OnSysProxyChanged += OnSysProxyChangedHandler;
        }

        #region private method
        private Controller.FormMainCtrl InitFormMainCtrl()
        {
            var ctrl = new Controller.FormMainCtrl();

            ctrl.Plug(new Controller.FormMainComponent.MarkFilter(
                cboxMarkFilter));

            ctrl.Plug(new Controller.FormMainComponent.FlyServer(
                flyServerListContainer));

            ctrl.Plug(new Controller.FormMainComponent.MenuItemsBasic(
                toolMenuItemSimAddVmessServer,
                toolMenuItemImportLinkFromClipboard,
                toolMenuItemExportAllServerToFile,
                toolMenuItemImportFromFile,
                toolMenuItemCheckUpdate,
                toolMenuItemAbout,
                toolMenuItemHelp,
                toolMenuItemConfigEditor,
                toolMenuItemQRCode,
                toolMenuItemLog,
                toolMenuItemOptions,
                toolStripMenuItemDownLoadV2rayCore,
                toolStripMenuItemRemoveV2rayCore));

            ctrl.Plug(new Controller.FormMainComponent.MenuItemsServer(
                toolStripMenuItemStopSelected,
                toolStripMenuItemRestartSelected,
                toolMenuItemClearSysProxy,
                toolMenuItemRefreshSummary,
                toolMenuItemSelectAutorunServers,
                toolStripMenuItemSelectAll,
                toolStripMenuItemSelectNone,
                toolStripMenuItemSelectInvert,
                toolStripMenuItemSpeedTestOnSelected,
                toolStripMenuItemDeleteSelectedServers,
                toolStripMenuItemCopyAsV2rayLink,
                toolStripMenuItemCopyAsVmessLink,
                toolStripMenuItemCopyAsSubscription,
                toolStripMenuItemDeleteAllServer,
                toolStripMenuItemPackSelectedServers));

            return ctrl;
        }

        void OnSysProxyChangedHandler(object sender, EventArgs args)
        {
            menuStrip1.Invoke((MethodInvoker)delegate
            {
                toolMenuItemCurrentSysProxy.Text = GetCurrentSysProxySetting();
            });
        }

        string GetCurrentSysProxySetting()
        {
            var str = I18N("CurSysProxy");
            var proxy = string.Empty;

            if (Lib.ProxySetter.getProxyState())
            {
                proxy = Lib.ProxySetter.getProxyUrl();
            }

            if (string.IsNullOrEmpty(proxy))
            {
                str = string.Format("{0}:{1}", str, I18N("NotSet"));
            }
            else
            {
                str = string.Format("{0} http://{1}", str, proxy);
            }
            return str;
        }
        #endregion

        #region UI event handler
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
