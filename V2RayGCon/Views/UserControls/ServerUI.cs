﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Views.UserControls
{
    public partial class ServerUI : UserControl, Model.BaseClass.IFormMainFlyPanelComponent
    {
        Service.Setting setting;
        Service.PacServer pacServer;
        Controller.CoreServerCtrl serverItem;

        int[] formHeight;
        Bitmap[] foldingButtonIcons;
        string[] keywords = null;

        public ServerUI(Controller.CoreServerCtrl serverItem)
        {
            setting = Service.Setting.Instance;
            pacServer = Service.PacServer.Instance;

            this.serverItem = serverItem;
            InitializeComponent();

            this.foldingButtonIcons = new Bitmap[] {
                Properties.Resources.StepBackArrow_16x,
                Properties.Resources.StepOverArrow_16x,
            };

            this.formHeight = new int[] {
                this.Height,  // collapseLevel= 0
                this.cboxInbound.Top,
            };
        }

        private void ServerListItem_Load(object sender, EventArgs e)
        {
            SetStatusThen(string.Empty);
            RefreshUI(this, EventArgs.Empty);
            this.serverItem.OnPropertyChanged += RefreshUI;
            rtboxServerTitle.BackColor = this.BackColor;
        }

        #region private method
        private void HighLightTitleWithKeywords()
        {
            if (keywords == null)
            {
                return;
            }

            rtboxServerTitle?.Invoke((MethodInvoker)delegate
            {
                var box = rtboxServerTitle;
                var title = box.Text.ToLower();
                var keyword = keywords.FirstOrDefault(
                    s => !string.IsNullOrEmpty(s)
                    && Lib.Utils.PartialMatch(title, s))?.ToLower();

                if (keyword == null)
                {
                    return;
                }

                var highlight = Color.DeepPink;

                int idxTitle = 0, idxKeyword = 0;
                while (idxTitle < title.Length && idxKeyword < keyword.Length)
                {
                    if (title[idxTitle] == keyword[idxKeyword])
                    {
                        box.SelectionStart = idxTitle;
                        box.SelectionLength = 1;
                        box.SelectionColor = highlight;
                        idxKeyword++;
                    }
                    idxTitle++;
                }
                box.SelectionStart = 0;
                box.SelectionLength = 0;
                box.DeselectAll();
            });
        }

        private void SetSysProxyToPACMode(bool isWhiteList)
        {
            var index = cboxInbound.SelectedIndex;
            if (index == (int)Model.Data.Enum.ProxyTypes.Config)
            {
                MessageBox.Show(I18N.SysProxyRequireHttpOrSocksMode);
                return;
            }

            Lib.Utils.TryParseIPAddr(tboxInboundAddr.Text, out string ip, out int port);
            var p = new Model.Data.PacUrlParams
            {
                ip = ip,
                port = port,
                isSocks = index == (int)Model.Data.Enum.ProxyTypes.SOCKS,
                isWhiteList = isWhiteList,
            };

            pacServer.SetPACProx(p);
            Lib.UI.ShowMessageBoxDoneAsync();
        }

        void CopyPACUrlToClipboard(bool isWhiteList)
        {
            var index = cboxInbound.SelectedIndex;
            if (index == (int)Model.Data.Enum.ProxyTypes.Config)
            {
                MessageBox.Show(I18N.SysProxyRequireHttpOrSocksMode);
                return;
            }


            Lib.Utils.TryParseIPAddr(tboxInboundAddr.Text, out string ip, out int port);
            var p = new Model.Data.PacUrlParams
            {
                ip = ip,
                port = port,
                isWhiteList = isWhiteList,
                isSocks = index == (int)Model.Data.Enum.ProxyTypes.SOCKS,
            };

            var pacUrl = pacServer.GenPacUrl(p);
            pacServer.StartPacServer();

            MessageBox.Show(
                Lib.Utils.CopyToClipboard(pacUrl) ?
                I18N.LinksCopied :
                I18N.CopyFail);
        }

        void RestartServer()
        {
            var server = this.serverItem;
            server.parent.StopAllServersThen(
                () => server.RestartCoreThen());
        }

        void RefreshUI(object sender, EventArgs arg)
        {

            rtboxServerTitle.Invoke((MethodInvoker)delegate
            {
                Lib.UI.UpdateControlOnDemand(
                    cboxInbound, serverItem.overwriteInboundType);

                Lib.UI.UpdateControlOnDemand(
                    rtboxServerTitle, serverItem.GetTitle());

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
                UpdateAicLable();
                UpdateChkSelected();
                UpdateOnOffLabel(serverItem.server.isRunning);
                UpdateServerMark();
                UpdateFormFoldingMode();
                UpdateToolsTip();
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

        private void UpdateToolsTip()
        {
            var status = serverItem.status;
            if (toolTip1.GetToolTip(lbStatus) != status)
            {
                toolTip1.SetToolTip(lbStatus, status);
            }

            var title = rtboxServerTitle.Text;
            if (toolTip1.GetToolTip(rtboxServerTitle) != title)
            {
                toolTip1.SetToolTip(rtboxServerTitle, title);
            }
        }

        private void UpdateAicLable()
        {
            var text = serverItem.isAutoRun ? "A" : "";
            text += serverItem.isInjectImport ? "I" : "";
            text += serverItem.isInjectSkipCNSite ? "C" : "";

            if (lbIsAutorun.Text != text)
            {
                lbIsAutorun.Text = text;
            }
        }

        void UpdateFormFoldingMode()
        {
            var level = Lib.Utils.Clamp(serverItem.foldingLevel, 0, foldingButtonIcons.Length);

            if (btnIsCollapse.BackgroundImage != foldingButtonIcons[level])
            {
                btnIsCollapse.BackgroundImage = foldingButtonIcons[level];
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
            var fontStyle = new Font(rtboxServerTitle.Font, selected ? FontStyle.Bold : FontStyle.Regular);
            var colorRed = selected ? Color.OrangeRed : Color.Black;
            rtboxServerTitle.Font = fontStyle;
            lbStatus.Font = fontStyle;
            lbStatus.ForeColor = colorRed;
        }

        private void UpdateOnOffLabel(bool isServerOn)
        {
            var text = isServerOn ? "ON" : "OFF";

            if (tboxInboundAddr.ReadOnly != isServerOn)
            {
                tboxInboundAddr.ReadOnly = isServerOn;
            }

            if (lbRunning.Text != text)
            {
                lbRunning.Text = text;
                lbRunning.ForeColor = isServerOn ? Color.DarkOrange : Color.Green;
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
        public void SetKeywords(string keywords)
        {
            this.keywords = (keywords ?? "").Split(
               new char[] { ' ', ',' },
               StringSplitOptions.RemoveEmptyEntries);

            Task.Factory.StartNew(() =>
            {
                // control may be desposed, the sun may explode while this function is running
                try
                {
                    HighLightTitleWithKeywords();
                }
                catch { }
            });
        }

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
                           I18N.LinksCopied :
                           I18N.CopyFail);
        }

        private void v2rayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                           Lib.Utils.CopyToClipboard(
                               Lib.Utils.AddLinkPrefix(
                                   Lib.Utils.Base64Encode(this.serverItem.config),
                                   Model.Data.Enum.LinkTypes.v2ray)) ?
                           I18N.LinksCopied :
                           I18N.CopyFail);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Lib.UI.Confirm(I18N.ConfirmDeleteControl))
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
            var level = (serverItem.foldingLevel + 1) % 2;
            serverItem.SetPropertyOnDemand(ref serverItem.foldingLevel, level);
        }

        private void lbIsAutorun_MouseDown(object sender, MouseEventArgs e)
        {
            ServerListItem_MouseDown(this, e);
        }

        private void rtboxServerTitle_Click(object sender, EventArgs e)
        {
            chkSelected.Checked = !chkSelected.Checked;
        }

        private void pACBlackListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSysProxyToPACMode(false);
        }

        private void pACWhiteListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSysProxyToPACMode(true);
        }

        private void copyPACBlackListLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPACUrlToClipboard(false);
        }

        private void copyPACWhiteListLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPACUrlToClipboard(true);
        }

        private void globalProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cboxInbound.SelectedIndex != (int)Model.Data.Enum.ProxyTypes.HTTP)
            {
                MessageBox.Show(I18N.SysProxyRequireHTTPServer);
                return;
            }

            Lib.Utils.TryParseIPAddr(tboxInboundAddr.Text, out string ip, out int port);
            pacServer.SetGlobalProxy(ip, port);

            // issue #9
            MessageBox.Show(I18N.SetSysProxyDone);
        }
        #endregion


    }
}
