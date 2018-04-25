﻿using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Controller
{
    class FormMainCtrl
    {
        Service.Setting setting;

        public FormMainCtrl()
        {
            setting = Service.Setting.Instance;
        }

        public void CopyV2RayLink(int index)
        {
            var server = setting.GetServer(index);
            string v2rayLink = string.Empty;

            if (!string.IsNullOrEmpty(server))
            {
                v2rayLink = Lib.Utils.LinkAddPrefix(server, Model.Data.Enum.LinkTypes.v2ray);
            }

            Lib.UI.ShowMsgboxSuccFail(
                Lib.Utils.CopyToClipboard(v2rayLink),
                I18N("LinksCopied"),
                I18N("CopyFail"));
        }

        public void CopyVmessLink(int index)
        {
            var server = setting.GetServer(index);
            string vmessLink = string.Empty;

            if (!string.IsNullOrEmpty(server))
            {
                var config = Lib.Utils.Base64Decode(server);
                var vmess = Lib.Utils.ConfigString2Vmess(config);
                vmessLink = Lib.Utils.Vmess2VmessLink(vmess);
            }

            Lib.UI.ShowMsgboxSuccFail(
                Lib.Utils.CopyToClipboard(vmessLink),
                I18N("LinksCopied"),
                I18N("CopyFail"));
        }

        public void ImportLinks()
        {
            string links = Lib.Utils.GetClipboardText();

            Lib.UI.ShowMsgboxSuccFail(
                setting.ImportLinks(links),
                I18N("ImportLinkSuccess"),
                I18N("ImportLinkFail"));
        }

        public void CopyAllV2RayLink()
        {
            var servers = setting.GetAllServers();
            string s = string.Empty;

            foreach (var server in servers)
            {
                s += "v2ray://" + server + "\r\n";
            }

            Lib.UI.ShowMsgboxSuccFail(
                Lib.Utils.CopyToClipboard(s),
                I18N("LinksCopied"),
                I18N("CopyFail"));
        }

        public void CopyAllVmessLink()
        {
            var servers = setting.GetAllServers();
            string s = string.Empty;

            foreach (var server in servers)
            {
                var config = Lib.Utils.Base64Decode(server);
                var vmess = Lib.Utils.ConfigString2Vmess(config);
                var vmessLink = Lib.Utils.Vmess2VmessLink(vmess);

                if (!string.IsNullOrEmpty(vmessLink))
                {
                    s += vmessLink + "\r\n";
                }
            }

            Lib.UI.ShowMsgboxSuccFail(
                Lib.Utils.CopyToClipboard(s),
                I18N("LinksCopied"),
                I18N("CopyFail"));
        }
    }
}
