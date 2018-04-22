﻿using Newtonsoft.Json.Linq;
using static V2RayGCon.Lib.StringResource;



namespace V2RayGCon.Controller.Configer
{
    class VmessClient :
        Model.BaseClass.NotifyComponent,
        Model.BaseClass.IConfigerComponent
    {
        private string _ID;

        public string ID
        {
            get { return _ID; }
            set { SetField(ref _ID, value); }
        }

        private string _level;

        public string level
        {
            get { return _level; }
            set { SetField(ref _level, value); }
        }

        private string _altID;

        public string altID
        {
            get { return _altID; }
            set { SetField(ref _altID, value); }
        }

        private string _blackhole;
        private string _ip;
        private int _port;

        public string addr
        {
            get { return string.Join(":", _ip, _port); }
            set
            {
                Lib.Utils.TryParseIPAddr(value, out _ip, out _port);
                SetField(ref _blackhole, value);
            }
        }

        public JToken GetSettings()
        {
            var configTemplate = JObject.Parse(resData("config_tpl"));
            JToken client = configTemplate["vmessClient"];

            client["vnext"][0]["address"] = _ip;
            client["vnext"][0]["port"] = _port;
            client["vnext"][0]["users"][0]["id"] = ID;
            client["vnext"][0]["users"][0]["alterId"] = Lib.Utils.Str2Int(altID);
            client["vnext"][0]["users"][0]["level"] = Lib.Utils.Str2Int(level);

            return client;
        }

        public void UpdateData(JObject config)
        {
            var GetStr = Lib.Utils.ClosureGetStringFromJToken(config);
            var GetAddrStr = Lib.Utils.ClosureGetAddrFromJToken(config);

            var perfix = "outbound.settings.vnext.0.users.0.";
            ID = GetStr(perfix, "id");
            level = GetStr(perfix, "level");
            altID = GetStr(perfix, "alterId");
            perfix = "outbound.settings.vnext.0.";
            addr = GetAddrStr(perfix, "address", "port");
        }
    }
}
