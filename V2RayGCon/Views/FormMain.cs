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
            return _instant;
        }
        #endregion

        Controller.FormMainCtrl formMainCtrl;
        Service.Setting setting;

        FormMain()
        {
            setting = Service.Setting.Instance;

            InitializeComponent();

#if DEBUG
            this.Icon = Properties.Resources.icon_light;
#endif

            this.Show();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            setting.RestoreFormRect(this, nameof(FormMain));

            this.FormClosed += (s, a) =>
            {
                setting.SaveFormRect(this, nameof(FormMain));
                setting.OnSysProxyChanged -= OnSysProxyChangedHandler;
                formMainCtrl.Cleanup();
                setting.LazyGC();
            };

            // Lib.UI.SetFormLocation<FormMain>(this, Model.Data.Enum.FormLocations.TopLeft);

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

            ctrl.Plug(new Controller.FormMainComponent.FlyServer(
                flyServerListContainer));

            ctrl.Plug(new Controller.FormMainComponent.MenuItems(
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

            ctrl.Plug(new Controller.FormMainComponent.ServerMenuItems(
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
                toolStripMenuItemDeleteAllServer));

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
            if (string.IsNullOrEmpty(setting.curSysProxy))
            {
                str = string.Format("{0}:{1}", str, I18N("NotSet"));
            }
            else
            {
                str = string.Format("{0} http://{1}", str, setting.curSysProxy);
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
