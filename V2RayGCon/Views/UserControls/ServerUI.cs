﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Views.UserControls
{
    public partial class ServerUI : UserControl, Model.BaseClass.IFormMainFlyPanelComponent
    {
        Controller.ServerCtrl serverItem;
        public bool isRunning;
        int[] formHeight;

        public ServerUI(Controller.ServerCtrl serverItem)
        {
            this.serverItem = serverItem;
            InitializeComponent();

            this.formHeight = new int[] {
                this.Height,  // collapseLevel= 0
                this.lbStatus.Top,
                this.cboxInbound.Top,
            };

            isRunning = !serverItem.isServerOn;
        }

        private void ServerListItem_Load(object sender, EventArgs e)
        {
            SetStatusThen(string.Empty);
            RefreshUI(this, EventArgs.Empty);
            this.serverItem.OnPropertyChanged += RefreshUI;
        }

        #region private method
        void RestartServer()
        {
            var server = this.serverItem;
            server.parent.StopAllServersThen(
                () => server.RestartCoreThen());
        }

        void RefreshUI(object sender, EventArgs arg)
        {
            lbServerTitle.Invoke((MethodInvoker)delegate
            {
                Lib.UI.UpdateControlOnDemand(
                    cboxInbound, serverItem.overwriteInboundType);

                Lib.UI.UpdateControlOnDemand(
                    lbServerTitle, serverItem.GetTitle());

                Lib.UI.UpdateControlOnDemand(
                    lbStatus, serverItem.status);

                Lib.UI.UpdateControlOnDemand(
                    toolStripMenuItemIsInjectImport,
                    serverItem.isInjectImport);

                Lib.UI.UpdateControlOnDemand(
                    toolStripMenuItemSkipCNSite,
                    serverItem.isInjectSkipCNSite);

                Lib.UI.UpdateControlOnDemand(
                    toolStripMenuItemIsAutorun,
                    serverItem.isAutoRun);

                UpdateInboundAddrOndemand();
                SetAICLable();
                UpdateChkSelected();
                ShowOnOffStatus(serverItem.server.isRunning);
                UpdateServerMark();
                UpdateFormCollapseMode();
                SetTitleToolTip();
            });
        }

        void UpdateInboundAddrOndemand()
        {
            var text = serverItem.inboundIP + ":" + serverItem.inboundPort.ToString();
            if (tboxInboundAddr.Text != text)
            {
                tboxInboundAddr.Text = text;
            }
        }

        private void SetTitleToolTip()
        {
            var status = serverItem.status;
            if (string.IsNullOrEmpty(status))
            {
                return;
            }

            if (toolTip1.GetToolTip(lbServerTitle) == status)
            {
                return;
            }

            toolTip1.SetToolTip(lbServerTitle, status);
        }

        private void SetAICLable()
        {
            var text = serverItem.isAutoRun ? "A" : "";
            text += serverItem.isInjectImport ? "I" : "";
            text += serverItem.isInjectSkipCNSite ? "C" : "";

            if (lbIsAutorun.Text != text)
            {
                lbIsAutorun.Text = text;
            }
        }

        void UpdateFormCollapseMode()
        {
            var level = Lib.Utils.Clamp(serverItem.collapseLevel, 0, 3);

            if (btnIsCollapse.ImageIndex != level)
            {
                btnIsCollapse.ImageIndex = level;
            }

            var newHeight = this.formHeight[level];
            if (this.Height != newHeight)
            {
                this.Height = newHeight;
            }
        }

        void UpdateServerMark()
        {
            if (cboxMark.Text == serverItem.mark)
            {
                return;
            }

            cboxMark.Text = serverItem.mark;
        }

        void UpdateChkSelected()
        {
            if (serverItem.isSelected == chkSelected.Checked)
            {
                return;
            }

            chkSelected.Checked = serverItem.isSelected;
            HighlightSelectedServerItem(chkSelected.Checked);
        }

        void HighlightSelectedServerItem(bool selected)
        {
            var fontStyle = new Font(lbServerTitle.Font, selected ? FontStyle.Bold : FontStyle.Regular);
            var colorRed = selected ? Color.OrangeRed : Color.Black;
            lbServerTitle.Font = fontStyle;
            lbStatus.Font = fontStyle;
            lbStatus.ForeColor = colorRed;
        }

        private void ShowOnOffStatus(bool isServerOn)
        {
            if (this.isRunning == isServerOn)
            {
                return;
            }

            this.isRunning = isServerOn;
            tboxInboundAddr.ReadOnly = this.isRunning;

            if (isServerOn)
            {
                lbRunning.ForeColor = Color.DarkOrange;
                lbRunning.Text = "ON";
            }
            else
            {
                lbRunning.ForeColor = Color.Green;
                lbRunning.Text = "OFF";
            }
        }
        #endregion

        #region properties
        public bool isSelected
        {
            get
            {
                return serverItem.isSelected;
            }
            private set { }
        }
        #endregion

        #region public method
        public string GetConfig()
        {
            return serverItem.config;
        }

        public void SetStatusThen(string status, Action next = null)
        {
            if (lbStatus.IsDisposed)
            {
                next?.Invoke();
                return;
            }

            try
            {
                lbStatus?.Invoke((MethodInvoker)delegate
                {
                    Lib.UI.UpdateControlOnDemand(lbStatus, status);
                });
            }
            catch { }
            next?.Invoke();
        }

        public void SetSelected(bool selected)
        {
            serverItem.SetIsSelected(selected);
        }

        public double GetIndex()
        {
            return serverItem.index;
        }

        public void SetIndex(double index)
        {
            serverItem.ChangeIndex(index);
        }

        public void Cleanup()
        {
            this.serverItem.OnPropertyChanged -= RefreshUI;
        }
        #endregion

        #region UI event
        private void ServerListItem_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Lib.UI.CreateCursorIconFromUserControl(this);
            DoDragDrop((ServerUI)sender, DragDropEffects.Move);
        }

        private void btnAction_Click(object sender, System.EventArgs e)
        {
            Button btnSender = sender as Button;
            Point pos = new Point(btnSender.Left, btnSender.Top + btnSender.Height);
            ctxMenuStripMore.Show(this, pos);
            //menu.Show(this, pos);
        }

        private void cboxInbound_SelectedIndexChanged(object sender, EventArgs e)
        {
            serverItem.SetOverwriteInboundType(cboxInbound.SelectedIndex);
        }

        private void chkSelected_CheckedChanged(object sender, EventArgs e)
        {
            var selected = chkSelected.Checked;
            if (selected == serverItem.isSelected)
            {
                return;
            }
            serverItem.SetIsSelected(selected);
            HighlightSelectedServerItem(chkSelected.Checked);
        }

        private void tboxInboundAddr_TextChanged(object sender, EventArgs e)
        {
            Lib.Utils.TryParseIPAddr(tboxInboundAddr.Text, out string ip, out int port);
            serverItem.SetPropertyOnDemand(ref serverItem.inboundIP, ip, true);
            serverItem.SetPropertyOnDemand(ref serverItem.inboundPort, port, true);
        }

        private void lbSummary_Click(object sender, EventArgs e)
        {
            chkSelected.Checked = !chkSelected.Checked;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            RestartServer();
        }

        private void multiboxingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serverItem.RestartCoreThen();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = this.serverItem;
            var config = item.config;
            new Views.WinForms.FormConfiger(this.serverItem.config);
        }

        private void vmessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                           Lib.Utils.CopyToClipboard(
                               Lib.Utils.Vmess2VmessLink(
                                   Lib.Utils.ConfigString2Vmess(
                                       this.serverItem.config))) ?
                           I18N("LinksCopied") :
                           I18N("CopyFail"));
        }

        private void v2rayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                           Lib.Utils.CopyToClipboard(
                               Lib.Utils.AddLinkPrefix(
                                   Lib.Utils.Base64Encode(this.serverItem.config),
                                   Model.Data.Enum.LinkTypes.v2ray)) ?
                           I18N("LinksCopied") :
                           I18N("CopyFail"));
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Lib.UI.Confirm(I18N("ConfirmDeleteControl")))
            {
                return;
            }
            Cleanup();
            serverItem.parent.DeleteServerByConfig(serverItem.config);

        }

        private void speedTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(
                        () => serverItem.RunSpeedTest());
        }

        private void logOfThisServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serverItem.ShowLogForm();
        }

        private void setAsSystemProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cboxInbound.SelectedIndex != (int)Model.Data.Enum.ProxyTypes.HTTP)
            {
                MessageBox.Show(I18N("SysProxyRequireHTTPServer"));
                return;
            }

            Service.Setting.Instance.SetSystemProxy(
                this.tboxInboundAddr.Text);

            // issue #9
            MessageBox.Show(I18N("SetSysProxyDone"));
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serverItem.StopCoreThen();
        }

        private void cboxMark_TextChanged(object sender, EventArgs e)
        {
            this.serverItem.SetMark(cboxMark.Text);
        }

        private void cboxMark_DropDown(object sender, EventArgs e)
        {
            var servers = Service.Servers.Instance;
            cboxMark.Items.Clear();
            foreach (var item in servers.GetMarkList())
            {
                cboxMark.Items.Add(item);
            }
            Lib.UI.ResetComboBoxDropdownMenuWidth(cboxMark);
        }

        private void lbStatus_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void label4_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void lbRunning_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void toolStripMenuItemStart_Click(object sender, EventArgs e)
        {
            RestartServer();
        }

        private void toolStripMenuItemIsAutorun_Click(object sender, EventArgs e)
        {
            serverItem.ToggleIsAutorun();
        }

        private void toolStripMenuItemIsInjectImport_Click(object sender, EventArgs e)
        {
            serverItem.ToggleIsInjectImport();
        }

        private void toolStripMenuItemSkipCNSite_Click(object sender, EventArgs e)
        {
            serverItem.ToggleIsInjectSkipCNSite();
        }

        private void btnMultiboxing_Click(object sender, EventArgs e)
        {
            serverItem.RestartCoreThen();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            serverItem.StopCoreThen();
        }

        private void btnIsCollapse_Click(object sender, EventArgs e)
        {
            var level = (serverItem.collapseLevel % 3 + 1) % 3;
            serverItem.SetPropertyOnDemand(ref serverItem.collapseLevel, level);
        }

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void lbIsAutorun_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }
        #endregion
    }
}
