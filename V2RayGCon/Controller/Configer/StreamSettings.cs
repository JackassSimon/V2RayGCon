﻿using Newtonsoft.Json.Linq;
using System;
using static V2RayGCon.Lib.StringResource;



namespace V2RayGCon.Controller.Configer
{
    class StreamSettings : Model.BaseClass.NotifyComponent
    {
        public StreamSettings()
        {
            _isServer = false;
        }

        #region properties
        private int _kcpType;
        public int kcpType
        {
            get { return _kcpType; }
            set { SetField(ref _kcpType, value); }
        }

        private int _tcpType;
        public int tcpType
        {
            get { return _tcpType; }
            set { SetField(ref _tcpType, value); }
        }

        private string _wsPath;
        public string wsPath
        {
            get { return _wsPath; }
            set { SetField(ref _wsPath, value); }
        }

        private int _tls;
        public int tls
        {
            get { return _tls; }
            set { SetField(ref _tls, value); }
        }

        private bool _isServer;

        public bool isServer
        {
            get { return _isServer; }
            set { _isServer = value; }
        }
        #endregion

        #region public method
        public JToken GetKCPSetting()
        {
            // 0 -> none -> kcp
            // 1 -> srtp -> kcp_srtp
            // ...

            var key = "kcp";
            if (kcpType > 0)
            {
                key = "kcp_" + Model.Data.Table.kcpTypes[kcpType];
            }

            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken stream = configTemplate[key];
            PlugTlsSettings(stream);
            return stream;
        }

        public JToken GetWSSetting()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));

            JToken stream = configTemplate["ws"];
            stream["wsSettings"]["path"] = wsPath;

            PlugTlsSettings(stream);
            return stream;
        }

        public JToken GetTCPSetting()
        {


            // 0 -> none -> tcp
            // 1 -> http -> tcp_http
            var key = "tcp";
            if (tcpType > 0)
            {
                key = "tcp_" + Model.Data.Table.tcpTypes[tcpType];
            }

            var configTemplate = JObject.Parse(resData("config_tpl"));
            var stream = configTemplate[key];
            PlugTlsSettings(stream);
            return stream;
        }

        public void UpdateData(JObject config)
        {
            var GetStr = Lib.Utils.HelperGetStringByPrefixAndKey(config);

            string prefix;

            if (_isServer)
            {
                prefix = "inbound.streamSettings";
            }
            else
            {
                prefix = "outbound.streamSettings";
            }

            wsPath = GetStr(prefix, "wsSettings.path");

            tls = Math.Max(0, Lib.Utils.GetIndexIgnoreCase(
                Model.Data.Table.streamSecurity,
                GetStr(prefix, "security")));

            kcpType = Lib.Utils.GetIndexIgnoreCase(
                Model.Data.Table.kcpTypes,
                GetStr(prefix, "kcpSettings.header.type"));

            tcpType = Lib.Utils.GetIndexIgnoreCase(
                Model.Data.Table.tcpTypes,
                GetStr(prefix, "tcpSettings.header.type"));
        }
        #endregion

        #region private method
        void PlugTlsSettings(JToken streamSettings)
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            var tlsTpl = configTemplate["tls"];
            if (tls <= 0)
            {
                streamSettings["security"] = string.Empty;
            }
            else
            {
                var streamSecurity = Model.Data.Table.streamSecurity;
                streamSettings["security"] = streamSecurity[tls];
                streamSettings["tlsSettings"] = tlsTpl;
            }
        }
        #endregion

    }

}
