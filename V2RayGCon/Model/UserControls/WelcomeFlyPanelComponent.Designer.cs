﻿namespace V2RayGCon.Model.UserControls
{
    partial class WelcomeFlyPanelComponent
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeFlyPanelComponent));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbDownloadV2rayCore = new System.Windows.Forms.LinkLabel();
            this.lbV2rayCoreGitHub = new System.Windows.Forms.LinkLabel();
            this.lbCopyFromClipboard = new System.Windows.Forms.LinkLabel();
            this.lbSimAddVmessWin = new System.Windows.Forms.LinkLabel();
            this.lbScanQRCode = new System.Windows.Forms.LinkLabel();
            this.lbConfigEditor = new System.Windows.Forms.LinkLabel();
            this.lbWiki = new System.Windows.Forms.LinkLabel();
            this.lbIssue = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // lbDownloadV2rayCore
            // 
            resources.ApplyResources(this.lbDownloadV2rayCore, "lbDownloadV2rayCore");
            this.lbDownloadV2rayCore.Name = "lbDownloadV2rayCore";
            this.lbDownloadV2rayCore.TabStop = true;
            this.lbDownloadV2rayCore.UseCompatibleTextRendering = true;
            // 
            // lbV2rayCoreGitHub
            // 
            resources.ApplyResources(this.lbV2rayCoreGitHub, "lbV2rayCoreGitHub");
            this.lbV2rayCoreGitHub.Name = "lbV2rayCoreGitHub";
            this.lbV2rayCoreGitHub.TabStop = true;
            this.lbV2rayCoreGitHub.UseCompatibleTextRendering = true;
            // 
            // lbCopyFromClipboard
            // 
            resources.ApplyResources(this.lbCopyFromClipboard, "lbCopyFromClipboard");
            this.lbCopyFromClipboard.Name = "lbCopyFromClipboard";
            this.lbCopyFromClipboard.TabStop = true;
            this.lbCopyFromClipboard.UseCompatibleTextRendering = true;
            // 
            // lbSimAddVmessWin
            // 
            resources.ApplyResources(this.lbSimAddVmessWin, "lbSimAddVmessWin");
            this.lbSimAddVmessWin.Name = "lbSimAddVmessWin";
            this.lbSimAddVmessWin.TabStop = true;
            this.lbSimAddVmessWin.UseCompatibleTextRendering = true;
            // 
            // lbScanQRCode
            // 
            resources.ApplyResources(this.lbScanQRCode, "lbScanQRCode");
            this.lbScanQRCode.Name = "lbScanQRCode";
            this.lbScanQRCode.TabStop = true;
            this.lbScanQRCode.UseCompatibleTextRendering = true;
            // 
            // lbConfigEditor
            // 
            resources.ApplyResources(this.lbConfigEditor, "lbConfigEditor");
            this.lbConfigEditor.Name = "lbConfigEditor";
            this.lbConfigEditor.TabStop = true;
            this.lbConfigEditor.UseCompatibleTextRendering = true;
            // 
            // lbWiki
            // 
            resources.ApplyResources(this.lbWiki, "lbWiki");
            this.lbWiki.Name = "lbWiki";
            this.lbWiki.TabStop = true;
            this.lbWiki.UseCompatibleTextRendering = true;
            // 
            // lbIssue
            // 
            resources.ApplyResources(this.lbIssue, "lbIssue");
            this.lbIssue.Name = "lbIssue";
            this.lbIssue.TabStop = true;
            this.lbIssue.UseCompatibleTextRendering = true;
            // 
            // WelcomeFlyPanelComponent
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbIssue);
            this.Controls.Add(this.lbWiki);
            this.Controls.Add(this.lbConfigEditor);
            this.Controls.Add(this.lbScanQRCode);
            this.Controls.Add(this.lbSimAddVmessWin);
            this.Controls.Add(this.lbCopyFromClipboard);
            this.Controls.Add(this.lbV2rayCoreGitHub);
            this.Controls.Add(this.lbDownloadV2rayCore);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "WelcomeFlyPanelComponent";
            this.Load += new System.EventHandler(this.WelcomeFlyPanelComponent_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel lbDownloadV2rayCore;
        private System.Windows.Forms.LinkLabel lbV2rayCoreGitHub;
        private System.Windows.Forms.LinkLabel lbCopyFromClipboard;
        private System.Windows.Forms.LinkLabel lbSimAddVmessWin;
        private System.Windows.Forms.LinkLabel lbScanQRCode;
        private System.Windows.Forms.LinkLabel lbConfigEditor;
        private System.Windows.Forms.LinkLabel lbWiki;
        private System.Windows.Forms.LinkLabel lbIssue;
    }
}
