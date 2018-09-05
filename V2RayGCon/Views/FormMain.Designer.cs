﻿using System.Windows.Forms;

namespace V2RayGCon.Views
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.operationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemSimAddVmessServer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemImportLinkFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolMenuItemDeleteAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemExportAllServerToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemImportFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemServer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStopAllServers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRestartAllServers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemRestartAutorunServers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemRefreshSummary = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.systemProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemCurrentSysProxy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemClearSysProxy = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemConfigEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemConfigTester = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemQRCode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.v2rayCoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemDownloadV2rayCore = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemRemoveV2rayCore = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemCheckUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMenuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flyServerListContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.operationToolStripMenuItem,
            this.toolMenuItemServer,
            this.windowToolStripMenuItem,
            this.aboutToolStripMenuItem1});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // operationToolStripMenuItem
            // 
            this.operationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolMenuItemSimAddVmessServer,
            this.toolMenuItemImportLinkFromClipboard,
            this.toolStripSeparator5,
            this.toolMenuItemDeleteAll,
            this.toolMenuItemExportAllServerToFile,
            this.toolMenuItemImportFromFile,
            this.toolStripSeparator8,
            this.exitToolStripMenuItem});
            this.operationToolStripMenuItem.Name = "operationToolStripMenuItem";
            resources.ApplyResources(this.operationToolStripMenuItem, "operationToolStripMenuItem");
            // 
            // toolMenuItemSimAddVmessServer
            // 
            this.toolMenuItemSimAddVmessServer.Name = "toolMenuItemSimAddVmessServer";
            resources.ApplyResources(this.toolMenuItemSimAddVmessServer, "toolMenuItemSimAddVmessServer");
            // 
            // toolMenuItemImportLinkFromClipboard
            // 
            this.toolMenuItemImportLinkFromClipboard.Name = "toolMenuItemImportLinkFromClipboard";
            resources.ApplyResources(this.toolMenuItemImportLinkFromClipboard, "toolMenuItemImportLinkFromClipboard");
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // toolMenuItemDeleteAll
            // 
            this.toolMenuItemDeleteAll.Name = "toolMenuItemDeleteAll";
            resources.ApplyResources(this.toolMenuItemDeleteAll, "toolMenuItemDeleteAll");
            // 
            // toolMenuItemExportAllServerToFile
            // 
            this.toolMenuItemExportAllServerToFile.Name = "toolMenuItemExportAllServerToFile";
            resources.ApplyResources(this.toolMenuItemExportAllServerToFile, "toolMenuItemExportAllServerToFile");
            // 
            // toolMenuItemImportFromFile
            // 
            this.toolMenuItemImportFromFile.Name = "toolMenuItemImportFromFile";
            resources.ApplyResources(this.toolMenuItemImportFromFile, "toolMenuItemImportFromFile");
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolMenuItemServer
            // 
            this.toolMenuItemServer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStopAllServers,
            this.toolStripMenuItemRestartAllServers,
            this.toolMenuItemRestartAutorunServers,
            this.toolMenuItemRefreshSummary,
            this.toolStripSeparator1,
            this.systemProxyToolStripMenuItem});
            this.toolMenuItemServer.Name = "toolMenuItemServer";
            resources.ApplyResources(this.toolMenuItemServer, "toolMenuItemServer");
            // 
            // toolStripMenuItemStopAllServers
            // 
            this.toolStripMenuItemStopAllServers.Name = "toolStripMenuItemStopAllServers";
            resources.ApplyResources(this.toolStripMenuItemStopAllServers, "toolStripMenuItemStopAllServers");
            // 
            // toolStripMenuItemRestartAllServers
            // 
            this.toolStripMenuItemRestartAllServers.Name = "toolStripMenuItemRestartAllServers";
            resources.ApplyResources(this.toolStripMenuItemRestartAllServers, "toolStripMenuItemRestartAllServers");
            // 
            // toolMenuItemRestartAutorunServers
            // 
            this.toolMenuItemRestartAutorunServers.Name = "toolMenuItemRestartAutorunServers";
            resources.ApplyResources(this.toolMenuItemRestartAutorunServers, "toolMenuItemRestartAutorunServers");
            // 
            // toolMenuItemRefreshSummary
            // 
            this.toolMenuItemRefreshSummary.Name = "toolMenuItemRefreshSummary";
            resources.ApplyResources(this.toolMenuItemRefreshSummary, "toolMenuItemRefreshSummary");
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // systemProxyToolStripMenuItem
            // 
            this.systemProxyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolMenuItemCurrentSysProxy,
            this.toolMenuItemClearSysProxy});
            this.systemProxyToolStripMenuItem.Name = "systemProxyToolStripMenuItem";
            resources.ApplyResources(this.systemProxyToolStripMenuItem, "systemProxyToolStripMenuItem");
            // 
            // toolMenuItemCurrentSysProxy
            // 
            resources.ApplyResources(this.toolMenuItemCurrentSysProxy, "toolMenuItemCurrentSysProxy");
            this.toolMenuItemCurrentSysProxy.Name = "toolMenuItemCurrentSysProxy";
            // 
            // toolMenuItemClearSysProxy
            // 
            this.toolMenuItemClearSysProxy.Name = "toolMenuItemClearSysProxy";
            resources.ApplyResources(this.toolMenuItemClearSysProxy, "toolMenuItemClearSysProxy");
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolMenuItemConfigEditor,
            this.toolMenuItemConfigTester,
            this.toolMenuItemQRCode,
            this.toolMenuItemLog,
            this.toolMenuItemOptions,
            this.toolStripSeparator10,
            this.v2rayCoreToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            resources.ApplyResources(this.windowToolStripMenuItem, "windowToolStripMenuItem");
            // 
            // toolMenuItemConfigEditor
            // 
            this.toolMenuItemConfigEditor.Name = "toolMenuItemConfigEditor";
            resources.ApplyResources(this.toolMenuItemConfigEditor, "toolMenuItemConfigEditor");
            // 
            // toolMenuItemConfigTester
            // 
            this.toolMenuItemConfigTester.Name = "toolMenuItemConfigTester";
            resources.ApplyResources(this.toolMenuItemConfigTester, "toolMenuItemConfigTester");
            // 
            // toolMenuItemQRCode
            // 
            this.toolMenuItemQRCode.Name = "toolMenuItemQRCode";
            resources.ApplyResources(this.toolMenuItemQRCode, "toolMenuItemQRCode");
            // 
            // toolMenuItemLog
            // 
            this.toolMenuItemLog.Name = "toolMenuItemLog";
            resources.ApplyResources(this.toolMenuItemLog, "toolMenuItemLog");
            // 
            // toolMenuItemOptions
            // 
            this.toolMenuItemOptions.Name = "toolMenuItemOptions";
            resources.ApplyResources(this.toolMenuItemOptions, "toolMenuItemOptions");
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // v2rayCoreToolStripMenuItem
            // 
            this.v2rayCoreToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolMenuItemDownloadV2rayCore,
            this.toolMenuItemRemoveV2rayCore});
            this.v2rayCoreToolStripMenuItem.Name = "v2rayCoreToolStripMenuItem";
            resources.ApplyResources(this.v2rayCoreToolStripMenuItem, "v2rayCoreToolStripMenuItem");
            // 
            // toolMenuItemDownloadV2rayCore
            // 
            this.toolMenuItemDownloadV2rayCore.Name = "toolMenuItemDownloadV2rayCore";
            resources.ApplyResources(this.toolMenuItemDownloadV2rayCore, "toolMenuItemDownloadV2rayCore");
            // 
            // toolMenuItemRemoveV2rayCore
            // 
            this.toolMenuItemRemoveV2rayCore.Name = "toolMenuItemRemoveV2rayCore";
            resources.ApplyResources(this.toolMenuItemRemoveV2rayCore, "toolMenuItemRemoveV2rayCore");
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolMenuItemCheckUpdate,
            this.toolMenuItemAbout,
            this.toolMenuItemHelp});
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            resources.ApplyResources(this.aboutToolStripMenuItem1, "aboutToolStripMenuItem1");
            // 
            // toolMenuItemCheckUpdate
            // 
            this.toolMenuItemCheckUpdate.Name = "toolMenuItemCheckUpdate";
            resources.ApplyResources(this.toolMenuItemCheckUpdate, "toolMenuItemCheckUpdate");
            // 
            // toolMenuItemAbout
            // 
            this.toolMenuItemAbout.Name = "toolMenuItemAbout";
            resources.ApplyResources(this.toolMenuItemAbout, "toolMenuItemAbout");
            // 
            // toolMenuItemHelp
            // 
            this.toolMenuItemHelp.Name = "toolMenuItemHelp";
            resources.ApplyResources(this.toolMenuItemHelp, "toolMenuItemHelp");
            // 
            // flyServerListContainer
            // 
            this.flyServerListContainer.AllowDrop = true;
            resources.ApplyResources(this.flyServerListContainer, "flyServerListContainer");
            this.flyServerListContainer.BackColor = System.Drawing.Color.White;
            this.flyServerListContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flyServerListContainer.Name = "flyServerListContainer";
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flyServerListContainer);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem operationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemImportLinkFromClipboard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemConfigEditor;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemQRCode;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemSimAddVmessServer;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemCheckUpdate;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemAbout;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemExportAllServerToFile;
        private System.Windows.Forms.ToolStripMenuItem toolMenuItemImportFromFile;
        private ToolStripMenuItem toolMenuItemHelp;
        private ToolStripMenuItem toolMenuItemConfigTester;
        private ToolStripMenuItem v2rayCoreToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem toolMenuItemDownloadV2rayCore;
        private ToolStripMenuItem toolMenuItemRemoveV2rayCore;
        private ToolStripMenuItem toolMenuItemOptions;
        private FlowLayoutPanel flyServerListContainer;
        private ToolStripMenuItem toolMenuItemDeleteAll;
        private ToolStripMenuItem toolMenuItemServer;
        private ToolStripMenuItem toolStripMenuItemStopAllServers;
        private ToolStripMenuItem toolStripMenuItemRestartAllServers;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem systemProxyToolStripMenuItem;
        private ToolStripMenuItem toolMenuItemCurrentSysProxy;
        private ToolStripMenuItem toolMenuItemClearSysProxy;
        private ToolStripMenuItem toolMenuItemRefreshSummary;
        private ToolStripMenuItem toolMenuItemRestartAutorunServers;
    }
}
