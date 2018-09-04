﻿using System;
using System.Drawing;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Model.UserControls
{
    public partial class ServerListItem : UserControl
    {
        Model.Data.ServerItem serverItem;
        ContextMenu menu;
        bool isRunning;

        public ServerListItem(int index, Model.Data.ServerItem serverItem)
        {
            menu = CreateMenu();
            this.serverItem = serverItem;
            SetIndex(index);
            InitializeComponent();
        }

        private void ServerListItem_Load(object sender, EventArgs e)
        {
            isRunning = !serverItem.isServerOn;
            RefreshUI(this, EventArgs.Empty);
            this.serverItem.OnPropertyChanged += RefreshUI;
        }

        #region private method
        void RefreshUI(object sender, EventArgs arg)
        {
            lbSummary.Invoke((MethodInvoker)delegate
            {
                Lib.UI.UpdateControlOnDemand<ComboBox, int>(
                    cboxInbound, serverItem.inboundOverwriteType);

                Lib.UI.UpdateControlOnDemand<Label, string>(
                    lbIndex, serverItem.index.ToString());

                Lib.UI.UpdateControlOnDemand<Label, string>(
                    lbSummary, serverItem.summary);

                Lib.UI.UpdateControlOnDemand<TextBox, string>(
                    tboxInboundIP, serverItem.inboundIP);

                Lib.UI.UpdateControlOnDemand<TextBox, string>(
                    tboxInboundPort, serverItem.inboundPort.ToString());

                Lib.UI.UpdateControlOnDemand<CheckBox, bool>(
                    chkAutoRun, serverItem.isAutoRun);

                Lib.UI.UpdateControlOnDemand<CheckBox, bool>(
                    chkImport, serverItem.isInjectImport);

                SetRunning(serverItem.server.isRunning);
            });
        }

        ContextMenu CreateMenu()
        {
            return new ContextMenu(new MenuItem[] {
                new MenuItem(I18N("Start"),(s,a)=>serverItem.RestartCoreThen()),
                new MenuItem(I18N("Stop"),(s,a)=>serverItem.StopCoreThen()),
                new MenuItem("-"),
                new MenuItem(I18N("Edit"),(s,a)=>{
                    var item=this.serverItem;
                    var config=item.config;
                    new Views.FormConfiger(this.serverItem.config);
                }),
                new MenuItem(I18N("Copy"),new MenuItem[]{
                    new MenuItem(I18N("VmessLink"),(s,a)=>{
                        MessageBox.Show(
                            Lib.Utils.CopyToClipboard(
                                Lib.Utils.Vmess2VmessLink(
                                    Lib.Utils.ConfigString2Vmess(
                                        this.serverItem.config)))?
                                        I18N("LinksCopied") :
                                        I18N("CopyFail"));
                    }),
                    new MenuItem(I18N("V2RayLink"),(s,a)=>{
                        MessageBox.Show(
                            Lib.Utils.CopyToClipboard(
                                Lib.Utils.AddLinkPrefix(
                                    Lib.Utils.Base64Encode(this.serverItem.config),
                                    Model.Data.Enum.LinkTypes.v2ray)) ?
                            I18N("LinksCopied") :
                            I18N("CopyFail"));
                    }),
                }),
                new MenuItem("-"),
                new MenuItem(I18N("SetAsSysProxy"),(s,a)=>{
                    if (cboxInbound.SelectedIndex != (int)Model.Data.Enum.ProxyTypes.HTTP)
                    {
                        MessageBox.Show(I18N("SysProxyRequireHTTPServer"));
                        return;
                    }

                    Service.Setting.Instance.SetSystemProxy(
                        string.Format("{0}:{1}",
                        this.tboxInboundIP.Text,
                        this.tboxInboundPort.Text));
                }),
            });
        }

        private void SetRunning(bool isServerOn)
        {
            if (this.isRunning == isServerOn)
            {
                return;
            }

            this.isRunning = isServerOn;

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

        #region public method

        public void SetIndex(int index)
        {
            this.serverItem.SetIndex(index);
        }

        public void Cleanup()
        {
            this.serverItem.OnPropertyChanged -= RefreshUI;
            // this.serverItem = null;
        }
        #endregion

        #region UI event
        private void ServerListItem_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Lib.UI.CreateCursorIconFromUserControl(this);
            DoDragDrop((ServerListItem)sender, DragDropEffects.Move);
        }

        private void btnAction_Click(object sender, System.EventArgs e)
        {
            Button btnSender = sender as Button;
            Point pos = new Point(btnSender.Left, btnSender.Top + btnSender.Height);
            menu.Show(this, pos);
        }

        private void cboxInbound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serverItem.inboundOverwriteType == cboxInbound.SelectedIndex)
            {
                return;
            }
            serverItem.SetInboundType(cboxInbound.SelectedIndex);
        }

        private void chkAutoRun_CheckedChanged(object sender, EventArgs e)
        {
            var check = chkAutoRun.Checked;
            if (serverItem.isAutoRun != check)
            {
                serverItem.SetAutoRun(check);
            }

        }

        private void chkImport_CheckedChanged(object sender, EventArgs e)
        {
            var check = chkImport.Checked;
            if (serverItem.isInjectImport != check)
            {
                serverItem.SetInjectImport(check);
            }

        }

        private void tboxInboundIP_TextChanged(object sender, EventArgs e)
        {
            var txt = tboxInboundIP.Text;
            if (txt == serverItem.inboundIP)
            {
                return;
            }
            serverItem.SetInboundIP(txt);
        }

        private void tboxInboundPort_TextChanged(object sender, EventArgs e)
        {
            var txt = tboxInboundPort.Text;
            if (txt == serverItem.inboundPort.ToString())
            {
                return;
            }
            serverItem.SetInboundPort(txt);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Lib.UI.Confirm(I18N("ConfirmDeleteControl")))
            {
                return;
            }

            Cleanup();

            serverItem.Delete();
        }



        #endregion


    }
}
