﻿using Newtonsoft.Json.Linq;

using static V2RayGCon.Lib.StringResource;


namespace V2RayGCon.Controller.Configer
{
    class SSClient :
        Model.BaseClass.NotifyComponent,
        Model.BaseClass.IConfigerComponent
    {
        private string _addr;
        private string _ip;
        private int _port;

        public string addr
        {
            get { return string.Join(":", _ip, _port); }
            set
            {
                Lib.Utils.TryParseIPAddr(value, out _ip, out _port);
                SetField(ref _addr, value);
            }
        }

        private string _pass;

        public string pass
        {
            get { return _pass; }
            set { SetField(ref _pass, value); }
        }

        private int _method;

        public int method
        {
            get { return _method; }
            set { SetField(ref _method, value); }
        }

        private bool _OTA;

        public bool OTA
        {
            get { return _OTA; }
            set { SetField(ref _OTA, value); }
        }

        public void SetMethod(string selectedMethod)
        {
            method = Lib.Utils.LookupDict(Model.Data.Table.ssMethods, selectedMethod);
        }

        public JToken GetSettings()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken client = configTemplate["ssClient"];
            var ssMethods = Model.Data.Table.ssMethods;

            client["servers"][0]["address"] = _ip;
            client["servers"][0]["port"] = _port;
            client["servers"][0]["method"] = method < 0 ? ssMethods[0] : ssMethods[method];
            client["servers"][0]["password"] = pass;
            client["servers"][0]["ota"] = OTA;

            return client;
        }

        public void UpdateData(JObject config)
        {
            var GetStr = Lib.Utils.ClosureGetStringFromJToken(config);
            var GetAddrStr = Lib.Utils.ClosureGetAddrFromJToken(config);

            var prefix = "outbound.settings.servers.0.";
            pass = GetStr(prefix, "password");
            addr = GetAddrStr(prefix, "address", "port");
            OTA = Lib.Utils.GetBoolFromJToken(config, prefix + "ota");
            SetMethod(GetStr(prefix, "method"));
        }
    }
}
