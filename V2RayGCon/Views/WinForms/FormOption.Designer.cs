﻿namespace V2RayGCon.Views.WinForms
{
    partial class FormOption
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOption));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageImport = new System.Windows.Forms.TabPage();
            this.btnImportAdd = new System.Windows.Forms.Button();
            this.flyImportPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPageSubscribe = new System.Windows.Forms.TabPage();
            this.btnUpdateViaSubscription = new System.Windows.Forms.Button();
            this.btnAddSubsUrl = new System.Windows.Forms.Button();
            this.flySubsUrlContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPageSetting = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.chkSetUseV4 = new System.Windows.Forms.CheckBox();
            this.chkSetSysPortable = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkSetServAutotrack = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cboxSettingPageSize = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboxSettingLanguage = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPagePACServ = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rtboxPacServWhiteList = new System.Windows.Forms.RichTextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rtboxPacServBlackList = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnPacBrowseFile = new System.Windows.Forms.Button();
            this.tboxPacFilePath = new System.Windows.Forms.TextBox();
            this.chkPacCustomFile = new System.Windows.Forms.CheckBox();
            this.chkPacServIsAutorun = new System.Windows.Forms.CheckBox();
            this.tboxPacServPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBakBackup = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnOptionSave = new System.Windows.Forms.Button();
            this.btnBakRestore = new System.Windows.Forms.Button();
            this.btnOptionExit = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageImport.SuspendLayout();
            this.tabPageSubscribe.SuspendLayout();
            this.tabPageSetting.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPagePACServ.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPageImport);
            this.tabControl1.Controls.Add(this.tabPageSubscribe);
            this.tabControl1.Controls.Add(this.tabPageSetting);
            this.tabControl1.Controls.Add(this.tabPagePACServ);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPageImport
            // 
            this.tabPageImport.Controls.Add(this.btnImportAdd);
            this.tabPageImport.Controls.Add(this.flyImportPanel);
            resources.ApplyResources(this.tabPageImport, "tabPageImport");
            this.tabPageImport.Name = "tabPageImport";
            this.tabPageImport.UseVisualStyleBackColor = true;
            // 
            // btnImportAdd
            // 
            resources.ApplyResources(this.btnImportAdd, "btnImportAdd");
            this.btnImportAdd.Name = "btnImportAdd";
            this.toolTip1.SetToolTip(this.btnImportAdd, resources.GetString("btnImportAdd.ToolTip"));
            this.btnImportAdd.UseVisualStyleBackColor = true;
            // 
            // flyImportPanel
            // 
            this.flyImportPanel.AllowDrop = true;
            resources.ApplyResources(this.flyImportPanel, "flyImportPanel");
            this.flyImportPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flyImportPanel.Name = "flyImportPanel";
            // 
            // tabPageSubscribe
            // 
            this.tabPageSubscribe.Controls.Add(this.btnUpdateViaSubscription);
            this.tabPageSubscribe.Controls.Add(this.btnAddSubsUrl);
            this.tabPageSubscribe.Controls.Add(this.flySubsUrlContainer);
            resources.ApplyResources(this.tabPageSubscribe, "tabPageSubscribe");
            this.tabPageSubscribe.Name = "tabPageSubscribe";
            this.tabPageSubscribe.UseVisualStyleBackColor = true;
            // 
            // btnUpdateViaSubscription
            // 
            resources.ApplyResources(this.btnUpdateViaSubscription, "btnUpdateViaSubscription");
            this.btnUpdateViaSubscription.Name = "btnUpdateViaSubscription";
            this.toolTip1.SetToolTip(this.btnUpdateViaSubscription, resources.GetString("btnUpdateViaSubscription.ToolTip"));
            this.btnUpdateViaSubscription.UseVisualStyleBackColor = true;
            // 
            // btnAddSubsUrl
            // 
            resources.ApplyResources(this.btnAddSubsUrl, "btnAddSubsUrl");
            this.btnAddSubsUrl.Name = "btnAddSubsUrl";
            this.toolTip1.SetToolTip(this.btnAddSubsUrl, resources.GetString("btnAddSubsUrl.ToolTip"));
            this.btnAddSubsUrl.UseVisualStyleBackColor = true;
            // 
            // flySubsUrlContainer
            // 
            this.flySubsUrlContainer.AllowDrop = true;
            resources.ApplyResources(this.flySubsUrlContainer, "flySubsUrlContainer");
            this.flySubsUrlContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flySubsUrlContainer.Name = "flySubsUrlContainer";
            // 
            // tabPageSetting
            // 
            this.tabPageSetting.Controls.Add(this.groupBox6);
            this.tabPageSetting.Controls.Add(this.groupBox5);
            this.tabPageSetting.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.tabPageSetting, "tabPageSetting");
            this.tabPageSetting.Name = "tabPageSetting";
            this.tabPageSetting.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.chkSetUseV4);
            this.groupBox6.Controls.Add(this.chkSetSysPortable);
            resources.ApplyResources(this.groupBox6, "groupBox6");
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.TabStop = false;
            // 
            // chkSetUseV4
            // 
            resources.ApplyResources(this.chkSetUseV4, "chkSetUseV4");
            this.chkSetUseV4.Name = "chkSetUseV4";
            this.toolTip1.SetToolTip(this.chkSetUseV4, resources.GetString("chkSetUseV4.ToolTip"));
            this.chkSetUseV4.UseVisualStyleBackColor = true;
            // 
            // chkSetSysPortable
            // 
            resources.ApplyResources(this.chkSetSysPortable, "chkSetSysPortable");
            this.chkSetSysPortable.Name = "chkSetSysPortable";
            this.toolTip1.SetToolTip(this.chkSetSysPortable, resources.GetString("chkSetSysPortable.ToolTip"));
            this.chkSetSysPortable.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Controls.Add(this.chkSetServAutotrack);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // chkSetServAutotrack
            // 
            resources.ApplyResources(this.chkSetServAutotrack, "chkSetServAutotrack");
            this.chkSetServAutotrack.Name = "chkSetServAutotrack";
            this.toolTip1.SetToolTip(this.chkSetServAutotrack, resources.GetString("chkSetServAutotrack.ToolTip"));
            this.chkSetServAutotrack.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.cboxSettingPageSize);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cboxSettingLanguage);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cboxSettingPageSize
            // 
            this.cboxSettingPageSize.FormattingEnabled = true;
            this.cboxSettingPageSize.Items.AddRange(new object[] {
            resources.GetString("cboxSettingPageSize.Items"),
            resources.GetString("cboxSettingPageSize.Items1"),
            resources.GetString("cboxSettingPageSize.Items2"),
            resources.GetString("cboxSettingPageSize.Items3"),
            resources.GetString("cboxSettingPageSize.Items4")});
            resources.ApplyResources(this.cboxSettingPageSize, "cboxSettingPageSize");
            this.cboxSettingPageSize.Name = "cboxSettingPageSize";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.toolTip1.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // cboxSettingLanguage
            // 
            this.cboxSettingLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSettingLanguage.FormattingEnabled = true;
            this.cboxSettingLanguage.Items.AddRange(new object[] {
            resources.GetString("cboxSettingLanguage.Items"),
            resources.GetString("cboxSettingLanguage.Items1"),
            resources.GetString("cboxSettingLanguage.Items2")});
            resources.ApplyResources(this.cboxSettingLanguage, "cboxSettingLanguage");
            this.cboxSettingLanguage.Name = "cboxSettingLanguage";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tabPagePACServ
            // 
            this.tabPagePACServ.Controls.Add(this.panel1);
            this.tabPagePACServ.Controls.Add(this.groupBox2);
            resources.ApplyResources(this.tabPagePACServ, "tabPagePACServ");
            this.tabPagePACServ.Name = "tabPagePACServ";
            this.tabPagePACServ.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Name = "panel1";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rtboxPacServWhiteList);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // rtboxPacServWhiteList
            // 
            resources.ApplyResources(this.rtboxPacServWhiteList, "rtboxPacServWhiteList");
            this.rtboxPacServWhiteList.Name = "rtboxPacServWhiteList";
            // 
            // groupBox4
            // 
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Controls.Add(this.rtboxPacServBlackList);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // rtboxPacServBlackList
            // 
            resources.ApplyResources(this.rtboxPacServBlackList, "rtboxPacServBlackList");
            this.rtboxPacServBlackList.Name = "rtboxPacServBlackList";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.btnPacBrowseFile);
            this.groupBox2.Controls.Add(this.tboxPacFilePath);
            this.groupBox2.Controls.Add(this.chkPacCustomFile);
            this.groupBox2.Controls.Add(this.chkPacServIsAutorun);
            this.groupBox2.Controls.Add(this.tboxPacServPort);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnPacBrowseFile
            // 
            resources.ApplyResources(this.btnPacBrowseFile, "btnPacBrowseFile");
            this.btnPacBrowseFile.Name = "btnPacBrowseFile";
            this.toolTip1.SetToolTip(this.btnPacBrowseFile, resources.GetString("btnPacBrowseFile.ToolTip"));
            this.btnPacBrowseFile.UseVisualStyleBackColor = true;
            // 
            // tboxPacFilePath
            // 
            resources.ApplyResources(this.tboxPacFilePath, "tboxPacFilePath");
            this.tboxPacFilePath.Name = "tboxPacFilePath";
            this.toolTip1.SetToolTip(this.tboxPacFilePath, resources.GetString("tboxPacFilePath.ToolTip"));
            // 
            // chkPacCustomFile
            // 
            resources.ApplyResources(this.chkPacCustomFile, "chkPacCustomFile");
            this.chkPacCustomFile.Name = "chkPacCustomFile";
            this.toolTip1.SetToolTip(this.chkPacCustomFile, resources.GetString("chkPacCustomFile.ToolTip"));
            this.chkPacCustomFile.UseVisualStyleBackColor = true;
            // 
            // chkPacServIsAutorun
            // 
            resources.ApplyResources(this.chkPacServIsAutorun, "chkPacServIsAutorun");
            this.chkPacServIsAutorun.Name = "chkPacServIsAutorun";
            this.toolTip1.SetToolTip(this.chkPacServIsAutorun, resources.GetString("chkPacServIsAutorun.ToolTip"));
            this.chkPacServIsAutorun.UseVisualStyleBackColor = true;
            // 
            // tboxPacServPort
            // 
            resources.ApplyResources(this.tboxPacServPort, "tboxPacServPort");
            this.tboxPacServPort.Name = "tboxPacServPort";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // btnBakBackup
            // 
            resources.ApplyResources(this.btnBakBackup, "btnBakBackup");
            this.btnBakBackup.Name = "btnBakBackup";
            this.toolTip1.SetToolTip(this.btnBakBackup, resources.GetString("btnBakBackup.ToolTip"));
            this.btnBakBackup.UseVisualStyleBackColor = true;
            this.btnBakBackup.Click += new System.EventHandler(this.btnBakBackup_Click);
            // 
            // btnOptionSave
            // 
            resources.ApplyResources(this.btnOptionSave, "btnOptionSave");
            this.btnOptionSave.Name = "btnOptionSave";
            this.toolTip1.SetToolTip(this.btnOptionSave, resources.GetString("btnOptionSave.ToolTip"));
            this.btnOptionSave.UseVisualStyleBackColor = true;
            this.btnOptionSave.Click += new System.EventHandler(this.btnOptionSave_Click);
            // 
            // btnBakRestore
            // 
            resources.ApplyResources(this.btnBakRestore, "btnBakRestore");
            this.btnBakRestore.Name = "btnBakRestore";
            this.toolTip1.SetToolTip(this.btnBakRestore, resources.GetString("btnBakRestore.ToolTip"));
            this.btnBakRestore.UseVisualStyleBackColor = true;
            this.btnBakRestore.Click += new System.EventHandler(this.btnBakRestore_Click);
            // 
            // btnOptionExit
            // 
            resources.ApplyResources(this.btnOptionExit, "btnOptionExit");
            this.btnOptionExit.Name = "btnOptionExit";
            this.btnOptionExit.UseVisualStyleBackColor = true;
            this.btnOptionExit.Click += new System.EventHandler(this.btnOptionExit_Click);
            // 
            // FormOption
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnBakRestore);
            this.Controls.Add(this.btnBakBackup);
            this.Controls.Add(this.btnOptionExit);
            this.Controls.Add(this.btnOptionSave);
            this.Controls.Add(this.tabControl1);
            this.Name = "FormOption";
            this.Shown += new System.EventHandler(this.FormOption_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPageImport.ResumeLayout(false);
            this.tabPageSubscribe.ResumeLayout(false);
            this.tabPageSetting.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPagePACServ.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSubscribe;
        private System.Windows.Forms.FlowLayoutPanel flySubsUrlContainer;
        private System.Windows.Forms.Button btnAddSubsUrl;
        private System.Windows.Forms.Button btnUpdateViaSubscription;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnOptionSave;
        private System.Windows.Forms.Button btnOptionExit;
        private System.Windows.Forms.TabPage tabPageImport;
        private System.Windows.Forms.Button btnImportAdd;
        private System.Windows.Forms.FlowLayoutPanel flyImportPanel;
        private System.Windows.Forms.Button btnBakBackup;
        private System.Windows.Forms.Button btnBakRestore;
        private System.Windows.Forms.TabPage tabPageSetting;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cboxSettingLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxSettingPageSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPagePACServ;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RichTextBox rtboxPacServBlackList;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RichTextBox rtboxPacServWhiteList;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkPacServIsAutorun;
        private System.Windows.Forms.TextBox tboxPacServPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkSetServAutotrack;
        private System.Windows.Forms.Button btnPacBrowseFile;
        private System.Windows.Forms.TextBox tboxPacFilePath;
        private System.Windows.Forms.CheckBox chkPacCustomFile;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox chkSetSysPortable;
        private System.Windows.Forms.CheckBox chkSetUseV4;
    }
}
