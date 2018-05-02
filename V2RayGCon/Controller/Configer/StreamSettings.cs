﻿using Newtonsoft.Json.Linq;
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
        private string _kcpType;

        public string kcpType
        {
            get { return _kcpType; }
            set { SetField(ref _kcpType, value); }
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
        public void SetSecurity(string security)
        {
            tls = Lib.Utils.GetIndexIgnoreCase(Model.Data.Table.streamSecurity, security);
        }

        public JToken GetKCPSetting()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken stream = configTemplate["kcp"];
            stream["kcpSettings"]["header"]["type"] = kcpType;
            stream["security"] = GetSecuritySetting();
            return stream;
        }

        public JToken GetWSSetting()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken stream = configTemplate["ws"];
            stream["wsSettings"]["path"] = wsPath;
            stream["security"] = GetSecuritySetting();
            return stream;
        }

        public JToken GetTCPSetting()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken stream = configTemplate["tcp"];
            stream["security"] = GetSecuritySetting();
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

            kcpType = GetStr(prefix, "kcpSettings.header.type");
            wsPath = GetStr(prefix, "wsSettings.path");
            SetSecurity(GetStr(prefix, "security"));
        }
        #endregion

        #region private method
        string GetSecuritySetting()
        {
            var streamSecurity = Model.Data.Table.streamSecurity;
            return tls <= 0 ? string.Empty : streamSecurity[tls];
        }
        #endregion

    }

}
