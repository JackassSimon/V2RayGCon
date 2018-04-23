﻿using Newtonsoft.Json.Linq;

using static V2RayGCon.Lib.StringResource;


namespace V2RayGCon.Controller.Configer
{
    class SSServer :
        Model.BaseClass.NotifyComponent,
        Model.BaseClass.IConfigerComponent
    {
        private string _port;

        public string port
        {
            get { return _port; }
            set
            {
                SetField(ref _port, value);
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

        private int _network;

        public int network
        {
            get { return _network; }
            set { SetField(ref _network, value); }
        }

        private bool _OTA;

        public bool OTA
        {
            get { return _OTA; }
            set { SetField(ref _OTA, value); }
        }

        public void SetMethod(string selectedMethod)
        {
            method = Lib.Utils.LookupDict(
                Model.Data.Table.ssMethods,
                selectedMethod);
        }

        public void SetNetwork(string selectedNetwork)
        {
            network = Lib.Utils.LookupDict(
                Model.Data.Table.ssNetworks,
                selectedNetwork);
        }

        public JToken GetSettings()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken server = configTemplate["ssServer"];

            var methods = Model.Data.Table.ssMethods;
            var networks = Model.Data.Table.ssNetworks;

            server["port"] = Lib.Utils.Str2Int(port);
            server["settings"]["method"] = method < 0 ? methods[0] : methods[method];
            server["settings"]["password"] = pass;
            server["settings"]["ota"] = OTA;
            server["settings"]["network"] = network < 0 ? networks[0] : networks[network];

            return server;
        }

        public void UpdateData(JObject config)
        {
            var GetStr = Lib.Utils.ClosureGetStringFromJToken(config);
            var prefix = "inbound.settings.";
            port = GetStr("inbound.", "port");
            SetMethod(GetStr(prefix, "method"));
            SetNetwork(GetStr(prefix, "network"));
            pass = GetStr(prefix, "password");
            OTA = Lib.Utils.GetBoolFromJToken(config, prefix + "ota");
        }

    }
}
