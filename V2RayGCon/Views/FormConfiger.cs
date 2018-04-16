﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using ScintillaNET;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Views
{
    public partial class FormConfiger : Form
    {
        JObject configTemplate, configEditing, configDefault;
        int perSectionIndex, perServIndex;

        Dictionary<int, string> sectionTable, ssrMethodTable;
        int sectionSeparator;
        Service.Setting settings;
        Scintilla tboxConfig;

        public FormConfiger()
        {
            settings = Service.Setting.Instance;

            InitializeComponent();
            InitScintilla();
            InitForm();

            this.Show();

        }

        void InitScintilla()
        {
            tboxConfig = new Scintilla();
            panelScintilla.Controls.Add(tboxConfig);

            tboxConfig.Dock = DockStyle.Fill;
            tboxConfig.WrapMode = WrapMode.None;
            tboxConfig.IndentationGuides = IndentView.LookBoth;

            var scintilla = tboxConfig;
            // Configure the JSON lexer styles
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 11;
            scintilla.Styles[Style.Json.Default].ForeColor = Color.Silver;
            scintilla.Styles[Style.Json.BlockComment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Json.LineComment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Json.Number].ForeColor = Color.Olive;
            scintilla.Styles[Style.Json.PropertyName].ForeColor = Color.Blue;
            scintilla.Styles[Style.Json.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Json.StringEol].BackColor = Color.Pink;
            scintilla.Styles[Style.Json.Operator].ForeColor = Color.Purple;
            scintilla.Lexer = Lexer.Json;

            // folding
            // Instruct the lexer to calculate folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                scintilla.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        bool CheckValid()
        {
            try
            {
                if (perSectionIndex >= sectionSeparator)
                {
                    JArray.Parse(tboxConfig.Text);
                }
                else
                {
                    JObject.Parse(tboxConfig.Text);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool Confirm(string content)
        {
            return Lib.Utils.Confirm(content);
        }

        void ShowConfigSection(int sectionIndex)
        {
            // Debug.WriteLine("showConfigById: " + selIdx);

            if (sectionIndex == 0)
            {
                tboxConfig.Text = configEditing.ToString();
                return;
            }

            var part = configEditing[sectionTable[sectionIndex]];
            if (part == null)
            {
                if (sectionIndex >= sectionSeparator)
                {
                    part = new JArray();
                }
                else
                {
                    part = new JObject();
                }
                configEditing[sectionTable[sectionIndex]] = part;
            }
            tboxConfig.Text = part.ToString();
            UpdateElement();
        }

        void SaveSectionChanges()
        {
            var content = tboxConfig.Text;
            if (perSectionIndex >= sectionSeparator)
            {
                configEditing[sectionTable[perSectionIndex]] =
                    JArray.Parse(content);
                return;
            }
            if (perSectionIndex == 0)
            {
                configEditing = JObject.Parse(content);
                return;
            }
            configEditing[sectionTable[perSectionIndex]] =
                JObject.Parse(content);
        }

        bool CheckSectionChange()
        {
            var content = tboxConfig.Text;

            if (perSectionIndex >= sectionSeparator)
            {
                return !(JToken.DeepEquals(JArray.Parse(content),
                    configEditing[sectionTable[perSectionIndex]]));
            }

            var obj = JObject.Parse(content);

            // config.json
            if (perSectionIndex == 0)
            {
                return !(JToken.DeepEquals(obj, configEditing));
            }

            return !(JToken.DeepEquals(obj,
                configEditing[sectionTable[perSectionIndex]]));
        }

        private void cboxConfigSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            int curSectionIndex = cboxConfigSection.SelectedIndex;
            // Debug.WriteLine("Select id:" + selIdx);

            if (curSectionIndex != perSectionIndex)
            {
                if (CheckValid())
                {
                    if (CheckSectionChange() && Confirm(I18N("EditorSaveChange")))
                    {
                        SaveSectionChanges();
                    }

                    perSectionIndex = curSectionIndex;
                    ShowConfigSection(curSectionIndex);
                }
                else
                {
                    if (Confirm(I18N("CannotParseJson")))
                    {
                        perSectionIndex = curSectionIndex;
                        ShowConfigSection(curSectionIndex);
                    }
                    else
                    {
                        cboxConfigSection.SelectedIndex = perSectionIndex;
                        return;
                    }
                }
            }

            UpdateElement();
        }

        void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string content = tboxConfig.Text;

            try
            {
                if (perSectionIndex >= sectionSeparator)
                {
                    configEditing[sectionTable[perSectionIndex]] =
                        JArray.Parse(content);
                }
                else
                {
                    var obj = JObject.Parse(content);
                    if (perSectionIndex == 0)
                    {
                        configEditing = obj;
                    }
                    else
                    {
                        configEditing[sectionTable[perSectionIndex]] = obj;
                    }
                }
            }
            catch
            {
                MessageBox.Show(I18N("PleaseCheckConfig"));
            }

            UpdateElement();
        }

        void FillTextBox(TextBox tbox, string perfix, string key)
        {
            tbox.Text = Lib.Utils.GetStrFromJToken(configEditing, perfix + key);
        }

        void FillTextBox(TextBox tbox, string perfix, string keyIP, string keyPort)
        {
            string ip = Lib.Utils.GetStrFromJToken(configEditing, perfix + keyIP);
            string port = Lib.Utils.GetStrFromJToken(configEditing, perfix + keyPort);
            tbox.Text = string.Join(":", ip, port);
        }

        void UpdateElement()
        {

            string perfix;

            // vmess
            perfix = "outbound.settings.vnext.0.users.0.";
            FillTextBox(tboxVMessID, perfix, "id");
            FillTextBox(tboxVMessLevel, perfix, "level");
            FillTextBox(tboxVMessAid, perfix, "alterId");

            perfix = "outbound.settings.vnext.0.";
            FillTextBox(tboxVMessIPaddr, perfix, "address", "port");

            // SS outbound.settings.servers.0.
            perfix = "outbound.settings.servers.0.";
            FillTextBox(tboxSSREmail, perfix, "email");
            FillTextBox(tboxSSRPass, perfix, "password");
            FillTextBox(tboxSSRAddr, perfix, "address", "port");

            bool ssrOTA = Lib.Utils.GetBoolFromJToken(configEditing, perfix + "ota");
            cboxSSROTA.SelectedIndex = ssrOTA ? 1 : 0;

            string ssrMethod = Lib.Utils.GetStrFromJToken(configEditing, perfix + "method");
            cboxSSRMethod.SelectedIndex = 0;
            foreach (var item in ssrMethodTable)
            {
                if (item.Value.Equals(ssrMethod))
                {
                    cboxSSRMethod.SelectedIndex = item.Key;
                    break;
                }
            }

            // kcp ws tls
            perfix = "outbound.streamSettings.";
            FillTextBox(tboxKCPType, perfix, "kcpSettings.header.type");
            FillTextBox(tboxWSPath, perfix, "wsSettings.path");

            var security = Lib.Utils.GetStrFromJToken(configEditing, perfix + "security");
            cboxStreamSecurity.SelectedIndex = security.Equals("tls") ? 1 : 0;
        }

        void SetStreamSecurity()
        {
            string sec = cboxStreamSecurity.SelectedIndex == 1 ? "tls" : "";
            try
            {
                configEditing["outbound"]["streamSettings"]["security"] = sec;
            }
            catch { }
            Debug.WriteLine("FormConfiger: can not set stream security!");
        }

        private void btnDropChanges_Click(object sender, EventArgs e)
        {
            tboxConfig.Text =
                perSectionIndex == 0 ?
                configEditing.ToString() :
                configEditing[sectionTable[perSectionIndex]].ToString();
        }

        private void btnOverwriteServConfig_Click(object sender, EventArgs e)
        {
            if (CheckSectionChange() && !Confirm(I18N("EditorDiscardChange")))
            {
                return;
            }

            string cfgString = configEditing.ToString();
            settings.ReplaceServer(Lib.Utils.Base64Encode(cfgString), perServIndex);
            MessageBox.Show(I18N("Done"));
        }

        private void btnLoadDefaultConfig_Click(object sender, EventArgs e)
        {
            string defConfig =
                perSectionIndex == 0 ?
                configDefault.ToString() :
                configDefault[sectionTable[perSectionIndex]]?.ToString();

            if (string.IsNullOrEmpty(defConfig))
            {
                MessageBox.Show(I18N("EditorNoExample"));
            }
            else
            {
                tboxConfig.Text = defConfig;
            }
        }

        private void btnVMessInsertClient_Click(object sender, EventArgs e)
        {
            string id = tboxVMessID.Text;
            int level = Lib.Utils.Str2Int(tboxVMessLevel.Text);
            int aid = Lib.Utils.Str2Int(tboxVMessAid.Text);

            Lib.Utils.TryParseIPAddr(tboxVMessIPaddr.Text, out string ip, out int port);

            JToken o = configTemplate["vmessClient"];
            o["vnext"][0]["address"] = ip;
            o["vnext"][0]["port"] = port;
            o["vnext"][0]["users"][0]["id"] = id;
            o["vnext"][0]["users"][0]["alterId"] = aid;
            o["vnext"][0]["users"][0]["level"] = level;

            ModifyOutBoundSetting(o, "vmess");

        }

        void ModifyOutBoundSetting(JToken set, string protocol)
        {
            try
            {
                configEditing["outbound"]["settings"] = set;
            }
            catch
            {
                Debug.WriteLine("FormConfiger: can not insert outbound.setting");
            }
            try
            {
                configEditing["outbound"]["protocol"] = protocol;
            }
            catch
            {
                Debug.WriteLine("FormConfiger: can not insert outbound.protocol");
            }

            ShowConfigSection(perSectionIndex);
        }

        private void btnSSRInsertClient_Click(object sender, EventArgs e)
        {

            Lib.Utils.TryParseIPAddr(tboxSSRAddr.Text, out string ip, out int port);

            JToken o = configTemplate["ssrClient"];
            o["servers"][0]["email"] = tboxSSREmail.Text;
            o["servers"][0]["address"] = ip;
            o["servers"][0]["port"] = port;
            o["servers"][0]["method"] = ssrMethodTable[cboxSSRMethod.SelectedIndex];
            o["servers"][0]["password"] = tboxSSRPass.Text;
            o["servers"][0]["ota"] = cboxSSROTA.SelectedIndex == 1;

            ModifyOutBoundSetting(o, "shadowsocks");
        }

        private void btnStreamInsertKCP_Click(object sender, EventArgs e)
        {
            try
            {
                configEditing["outbound"]["streamSettings"] = configTemplate["kcp"];
                configEditing["outbound"]["streamSettings"]["kcpSettings"]["header"]["type"] = tboxKCPType.Text;
            }
            catch { }
            SetStreamSecurity();
            ShowConfigSection(perSectionIndex);
        }

        private void btnStreamInsertWS_Click(object sender, EventArgs e)
        {
            try
            {
                configEditing["outbound"]["streamSettings"] = configTemplate["ws"];
                configEditing["outbound"]["streamSettings"]["wsSettings"]["path"] = tboxWSPath.Text;
            }
            catch { }
            SetStreamSecurity();
            ShowConfigSection(perSectionIndex);
        }

        private void btnStreamInsertTCP_Click(object sender, EventArgs e)
        {
            try
            {
                configEditing["outbound"]["streamSettings"] = configTemplate["tcp"];
            }
            catch { };
            SetStreamSecurity();
            ShowConfigSection(perSectionIndex);
        }

        private void btnVMessGenUUID_Click(object sender, EventArgs e)
        {
            tboxVMessID.Text = Guid.NewGuid().ToString();
        }

        private void btnInsertNewServ_Click(object sender, EventArgs e)
        {
            if (CheckSectionChange() && !Confirm(I18N("EditorDiscardChange")))
            {
                return;
            }

            string cfgString = configEditing.ToString();
            settings.AddServer(Lib.Utils.Base64Encode(cfgString));
            MessageBox.Show(I18N("Done"));
        }

        private void cboxShowPassWord_CheckedChanged(object sender, EventArgs e)
        {
            if (cboxShowPassWord.Checked == true)
            {
                tboxSSRPass.PasswordChar = '\0';

            }
            else
            {
                tboxSSRPass.PasswordChar = '*';
            }
        }

        JObject LoadServerConfig()
        {
            JObject o = null;
            string b64str = settings.GetServer(settings.curEditingIndex);
            if (!string.IsNullOrEmpty(b64str))
            {
                o = JObject.Parse(Lib.Utils.Base64Decode(b64str));
            }

            if (o == null)
            {
                o = JObject.Parse(resData("config_min"));
                MessageBox.Show(I18N("EditorCannotLoadServerConfig"));
            }

            return o;
        }

        void InitForm()
        {
            sectionTable = new Dictionary<int, string>
            {
                { 0, "config.json"},
                { 1, "log"},
                { 2, "api"},
                { 3, "dns"},
                { 4, "stats"},
                { 5, "routing"},
                { 6, "policy"},
                { 7, "inbound"},
                { 8, "outbound"},
                { 9, "transport"},
                { 10,"v2raygcon" },
                { 11,"inboundDetour"},
                { 12,"outboundDetour"},

            };

            // separate between dictionary or array
            sectionSeparator = 11;

            ssrMethodTable = new Dictionary<int, string>
            {
                { 0,"aes-128-cfb"},
                { 1,"aes-128-gcm"},
                { 2,"aes-256-cfb"},
                { 3,"aes-256-gcm"},
                { 4,"chacha20"},
                { 5,"chacha20-ietf"},
                { 6,"chacha20-poly1305"},
                { 7,"chacha20-ietf-poly1305"},
            };


            configTemplate = JObject.Parse(resData("config_tpl"));
            configDefault = JObject.Parse(resData("config_def"));
            configEditing = LoadServerConfig();

            cboxServList.Items.Clear();
            for (int i = 0; i < settings.GetServerCount(); i++)
            {
                cboxServList.Items.Add(i + 1);
            }

            perServIndex = settings.curEditingIndex;
            cboxServList.SelectedIndex = perServIndex;

            perSectionIndex = 0;
            cboxConfigSection.SelectedIndex = perSectionIndex;
            ShowConfigSection(perSectionIndex);
        }
    }
}
