﻿using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Lib
{
    public class Utils
    {
        public static Service.Cache cache = Service.Cache.Instance;

        #region Json

        static bool Contain(JProperty main, JProperty sub)
        {
            return Contain(main.Value, sub.Value);
        }

        static bool Contain(JArray main, JArray sub)
        {
            foreach (var sItem in sub)
            {
                foreach (var mItem in main)
                {
                    if (Contain(mItem, sItem))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        static bool Contain(JObject main, JObject sub)
        {
            foreach (var item in sub)
            {
                var key = item.Key;
                if (!main.ContainsKey(key))
                {
                    return false;
                }

                if (!Contain(main[key], sub[key]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Contain(JValue main, JValue sub)
        {
            return main.Equals(sub);
        }

        public static bool Contain(JToken main, JToken sub)
        {
            if (main.Type != sub.Type)
            {
                return false;
            }

            switch (sub.Type)
            {
                case JTokenType.Property:
                    return Contain(main as JProperty, sub as JProperty);
                case JTokenType.Object:
                    return Contain(main as JObject, sub as JObject);
                case JTokenType.Array:
                    return Contain(main as JArray, sub as JArray);
                default:
                    return Contain(main as JValue, sub as JValue);
            }
        }

        public static Tuple<string, string> ParsePathIntoParentAndKey(string path)
        {
            var index = path.LastIndexOf('.');
            string key;
            string parent = string.Empty;
            if (index < 0)
            {
                key = path;
            }
            else if (index == 0)
            {
                key = path.Substring(1);
            }
            else
            {
                key = path.Substring(index + 1);
                parent = path.Substring(0, index);
            }

            return new Tuple<string, string>(parent, key);
        }

        public static JObject CreateJObject(string path)
        {
            var result = JObject.Parse(@"{}");

            if (string.IsNullOrEmpty(path))
            {
                return result;
            }
            var curNode = result as JToken;
            foreach (var p in path.Split('.'))
            {
                if (string.IsNullOrEmpty(p))
                {
                    throw new KeyNotFoundException("Parent contain empty key");
                }

                if (int.TryParse(p, out int num))
                {
                    throw new KeyNotFoundException("All parents must be JObject");
                }

                curNode[p] = JObject.Parse(@"{}");
                curNode = curNode[p];
            }

            return result;
        }

        public static bool SetValue<T>(JObject json, string path, T value)
        {
            var parts = ParsePathIntoParentAndKey(path);
            var r = json;

            var key = parts.Item2;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var parent = parts.Item1;
            if (!string.IsNullOrEmpty(parent))
            {
                var p = GetKey(json, parent);
                if (p == null || !(p is JObject))
                {
                    return false;
                }
                r = p as JObject;
            }

            r[key] = new JValue(value);
            return true;
        }

        public static JObject ExtractJObjectPart(JObject source, string path)
        {
            var parts = ParsePathIntoParentAndKey(path);
            var key = parts.Item2;
            var parentPath = parts.Item1;

            if (string.IsNullOrEmpty(key))
            {
                throw new KeyNotFoundException("key is empty");
            }

            var node = GetKey(source, path);
            if (node == null)
            {
                throw new KeyNotFoundException();
            }

            var result = CreateJObject(parentPath);

            var parent = string.IsNullOrEmpty(parentPath) ?
                result : GetKey(result, parentPath);

            if (parent == null || !(parent is JObject))
            {
                throw new KeyNotFoundException("Create parent JObject fail!");
            }

            parent[key] = node.DeepClone();

            return result;
        }

        public static void RemoveKeyFromJObject(JObject json, string path)
        {
            var parts = ParsePathIntoParentAndKey(path);

            var parent = parts.Item1;
            var key = parts.Item2;

            if (string.IsNullOrEmpty(key))
            {
                throw new KeyNotFoundException();
            }

            var node = string.IsNullOrEmpty(parent) ?
                json : GetKey(json, parent);

            if (node == null || !(node is JObject))
            {
                throw new KeyNotFoundException();
            }

            (node as JObject).Property(key)?.Remove();
        }

        static void ConcatJson(ref JObject body, JObject mixin)
        {
            body.Merge(mixin, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,
                MergeNullValueHandling = MergeNullValueHandling.Ignore,
            });
        }

        public static void CombineConfig(ref JObject body, JObject mixin)
        {
            // in(out)Dtr

            JObject backup = JObject.Parse(@"{}");

            foreach (var key in new string[] {
                    "inboundDetour",
                    "outboundDetour",
                    "routing.settings.rules"})
            {
                JObject nodeBody = null;
                JObject nodeMixin = null;
                try
                {
                    nodeBody = ExtractJObjectPart(body, key);
                    RemoveKeyFromJObject(body, key);
                }
                catch (KeyNotFoundException) { }

                try
                {
                    nodeMixin = ExtractJObjectPart(mixin, key);
                    ConcatJson(ref backup, nodeMixin);
                    RemoveKeyFromJObject(mixin, key);
                    ConcatJson(ref body, nodeMixin);
                }
                catch (KeyNotFoundException) { }

                if (nodeBody != null)
                {
                    ConcatJson(ref body, nodeBody);
                }
            }

            MergeJson(ref body, mixin);

            // restore mixin
            ConcatJson(ref mixin, backup);
        }

        public static void MergeJson(ref JObject body, JObject mixin)
        {
            body.Merge(mixin, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });
        }

        public static JToken GetKey(JToken json, string path)
        {
            var curPos = json;
            var keys = path.Split('.');

            int depth;
            for (depth = 0; depth < keys.Length; depth++)
            {
                if (curPos == null || !curPos.HasValues)
                {
                    break;
                }

                if (int.TryParse(keys[depth], out int n))
                {
                    curPos = curPos[n];
                }
                else
                {
                    curPos = curPos[keys[depth]];
                }
            }

            return depth < keys.Length ? null : curPos;
        }

        public static T GetValue<T>(JToken json, string prefix, string key)
        {
            return GetValue<T>(json, $"{prefix}.{key}");
        }

        public static T GetValue<T>(JToken json, string path)
        {
            var key = GetKey(json, path);

            var def = default(T) == null && typeof(T) == typeof(string) ?
                (T)(object)string.Empty :
                default(T);

            if (key == null)
            {
                return def;
            }
            try
            {
                return key.Value<T>();
            }
            catch { }
            return def;
        }

        public static Func<string, string, string> GetStringByPrefixAndKeyHelper(JObject json)
        {
            var o = json;
            return (prefix, key) =>
            {
                return GetValue<string>(o, $"{prefix}.{key}");
            };
        }

        public static Func<string, string> GetStringByKeyHelper(JObject json)
        {
            var o = json;
            return (key) =>
            {
                return GetValue<string>(o, $"{key}");
            };
        }

        public static string GetAddr(JObject json, string prefix, string keyIP, string keyPort)
        {
            var ip = GetValue<String>(json, prefix, keyIP) ?? "127.0.0.1";
            var port = GetValue<string>(json, prefix, keyPort);
            return string.Join(":", ip, port);
        }

        #endregion

        #region convert

        public static string Config2Base64String(JObject config)
        {
            return Base64Encode(config.ToString(Formatting.None));
        }

        public static List<string> Str2ListStr(string serial)
        {
            var list = new List<string> { };
            var items = serial.Split(',');
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    list.Add(item);
                }

            }
            return list;
        }

        public static List<string> ExtractLinks(string text, Model.Data.Enum.LinkTypes linkType)
        {
            string pattern = GenPattern(linkType);
            var matches = Regex.Matches("\n" + text, pattern, RegexOptions.IgnoreCase);
            var links = new List<string>();
            foreach (Match match in matches)
            {
                links.Add(match.Value.Substring(1));
            }
            return links;
        }

        public static string Vmess2VmessLink(Model.Data.Vmess vmess)
        {
            if (vmess == null)
            {
                return string.Empty;
            }

            string content = JsonConvert.SerializeObject(vmess);
            return AddLinkPrefix(
                Base64Encode(content),
                Model.Data.Enum.LinkTypes.vmess);
        }

        public static Model.Data.Vmess VmessLink2Vmess(string link)
        {
            try
            {
                string plainText = Base64Decode(GetLinkBody(link));
                var vmess = JsonConvert.DeserializeObject<Model.Data.Vmess>(plainText);
                if (!string.IsNullOrEmpty(vmess.add)
                    && !string.IsNullOrEmpty(vmess.port)
                    && !string.IsNullOrEmpty(vmess.aid))
                {

                    return vmess;
                }
            }
            catch { }
            return null;
        }

        public static Model.Data.Shadowsocks SSLink2SS(string ssLink)
        {
            string b64 = GetLinkBody(ssLink);

            try
            {
                var ss = new Model.Data.Shadowsocks();
                var plainText = Base64Decode(b64);
                var parts = plainText.Split('@');
                var mp = parts[0].Split(':');
                if (parts[1].Length > 0 && mp[0].Length > 0 && mp[1].Length > 0)
                {
                    ss.method = mp[0];
                    ss.pass = mp[1];
                    ss.addr = parts[1];
                }
                return ss;
            }
            catch { }
            return null;
        }

        public static JObject SSLink2Config(string ssLink)
        {
            Model.Data.Shadowsocks ss = SSLink2SS(ssLink);
            if (ss == null)
            {
                return null;
            }

            TryParseIPAddr(ss.addr, out string ip, out int port);

            var config = cache.tpl.LoadTemplate("tplImportSS");

            var setting = config["outbound"]["settings"]["servers"][0];
            setting["address"] = ip;
            setting["port"] = port;
            setting["method"] = ss.method;
            setting["password"] = ss.pass;

            return config.DeepClone() as JObject;
        }

        public static Model.Data.Vmess ConfigString2Vmess(string config)
        {
            JObject json;
            try
            {
                json = JObject.Parse(config);
            }
            catch
            {
                return null;
            }

            var GetStr = GetStringByPrefixAndKeyHelper(json);

            Model.Data.Vmess vmess = new Model.Data.Vmess();
            vmess.v = "2";
            vmess.ps = GetStr("v2raygcon", "alias");

            var prefix = "outbound.settings.vnext.0";
            vmess.add = GetStr(prefix, "address");
            vmess.port = GetStr(prefix, "port");
            vmess.id = GetStr(prefix, "users.0.id");
            vmess.aid = GetStr(prefix, "users.0.alterId");

            prefix = "outbound.streamSettings";
            vmess.net = GetStr(prefix, "network");
            vmess.type = GetStr(prefix, "kcpSettings.header.type");
            vmess.tls = GetStr(prefix, "security");

            switch (vmess.net)
            {
                case "ws":
                    vmess.path = GetStr(prefix, "wsSettings.path");
                    vmess.host = GetStr(prefix, "wsSettings.headers.Host");
                    break;
                case "h2":
                    try
                    {
                        vmess.path = GetStr(prefix, "httpSettings.path");
                        var hosts = json["outbound"]["streamSettings"]["httpSettings"]["host"];
                        vmess.host = JArray2Str(hosts as JArray);
                    }
                    catch { }
                    break;
            }


            return vmess;
        }

        public static JObject Vmess2Config(Model.Data.Vmess vmess)
        {
            if (vmess == null)
            {
                return null;
            }

            // prepare template
            var config = cache.tpl.LoadTemplate("tplImportVmess");
            config["v2raygcon"]["alias"] = vmess.ps;

            var cPos = config["outbound"]["settings"]["vnext"][0];
            cPos["address"] = vmess.add;
            cPos["port"] = Lib.Utils.Str2Int(vmess.port);
            cPos["users"][0]["id"] = vmess.id;
            cPos["users"][0]["alterId"] = Lib.Utils.Str2Int(vmess.aid);

            // insert stream type
            string[] streamTypes = { "ws", "tcp", "kcp", "h2" };
            string streamType = vmess.net.ToLower();

            if (!streamTypes.Contains(streamType))
            {
                return config.DeepClone() as JObject;
            }

            config["outbound"]["streamSettings"] =
                cache.tpl.LoadTemplate(streamType);

            try
            {
                switch (streamType)
                {
                    case "kcp":
                        config["outbound"]["streamSettings"]["kcpSettings"]["header"]["type"] = vmess.type;
                        break;
                    case "ws":
                        config["outbound"]["streamSettings"]["wsSettings"]["path"] =
                            string.IsNullOrEmpty(vmess.v) ? vmess.host : vmess.path;
                        if (vmess.v == "2" && !string.IsNullOrEmpty(vmess.host))
                        {
                            config["outbound"]["streamSettings"]["wsSettings"]["headers"]["Host"] = vmess.host;
                        }
                        break;
                    case "h2":
                        config["outbound"]["streamSettings"]["httpSettings"]["path"] = vmess.path;
                        config["outbound"]["streamSettings"]["httpSettings"]["host"] = Str2JArray(vmess.host);
                        break;
                }

            }
            catch { }

            try
            {
                // must place at the end. cos this key is add by streamSettings
                config["outbound"]["streamSettings"]["security"] = vmess.tls;
            }
            catch { }
            return config.DeepClone() as JObject;
        }

        public static JArray Str2JArray(string content)
        {
            var arr = new JArray();
            var items = content.Replace(" ", "").Split(',');
            foreach (var item in items)
            {
                if (item.Length > 0)
                {
                    arr.Add(item);
                }
            }
            return arr;
        }

        public static string JArray2Str(JArray array)
        {
            if (array == null)
            {
                return string.Empty;
            }
            List<string> s = new List<string>();

            foreach (var item in array.Children())
            {
                try
                {
                    var v = item.Value<string>();
                    if (!string.IsNullOrEmpty(v))
                    {
                        s.Add(v);
                    }
                }
                catch { }
            }

            if (s.Count <= 0)
            {
                return string.Empty;
            }
            return string.Join(",", s);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        static string Base64PadRight(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        }

        public static string Base64Decode(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return string.Empty;
            }
            var padded = Base64PadRight(base64EncodedData);
            var base64EncodedBytes = System.Convert.FromBase64String(padded);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        #endregion

        #region net
        public static string Fetch(string url, int timeout = -1)
        {
            var html = string.Empty;

            Lib.Utils.SupportProtocolTLS12();
            using (WebClient wc = new TimedWebClient
            {
                Encoding = System.Text.Encoding.UTF8,
                Timeout = timeout,
            })
            {
                /* 如果用抛出异常的写法
                 * task中调用此函数时
                 * 会弹出用户未处理异常警告
                 */
                try
                {
                    html = wc.DownloadString(url);
                }
                catch { }
            }
            return html;
        }

        public static string GetLatestVGCVersion()
        {
            string html = Fetch(StrConst("UrlLatestVGC"));

            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            string p = StrConst("PatternLatestVGC");
            var match = Regex.Match(html, p, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public static List<string> GetCoreVersions()
        {
            List<string> versions = new List<string> { };

            string html = Fetch(StrConst("ReleasePageUrl"));

            if (string.IsNullOrEmpty(html))
            {
                return versions;
            }

            string pattern = StrConst("PatternDownloadLink");
            var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var v = match.Groups[1].Value;
                versions.Add(v);
            }

            return versions;
        }
        #endregion

        #region files
        public static string GetAppDataFolder()
        {
            var appData = System.Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(appData, Properties.Resources.AppName);
        }

        public static void CreateAppDataFolder()
        {
            var path = GetAppDataFolder();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteAppDataFolder()
        {
            Directory.Delete(GetAppDataFolder(), recursive: true);
        }
        #endregion

        #region Miscellaneous
        public static string RandomHex(int length)
        {
            //  https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
            if (length <= 0)
            {
                return string.Empty;
            }

            Random random = new Random();
            const string chars = "0123456789abcdef";
            return new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(Math.Min(value, max - 1), min);
        }

        public static int GetIndexIgnoreCase(Dictionary<int, string> dict, string value)
        {
            foreach (var data in dict)
            {
                if (!string.IsNullOrEmpty(data.Value)
                    && data.Value.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return data.Key;
                }
            }
            return -1;
        }

        public static string CutStr(string s, int len)
        {
            if (len >= s.Length || len < 1)
            {
                return s;
            }

            return s.Substring(0, len) + " ...";
        }

        public static int Str2Int(string value)
        {
            if (float.TryParse(value, out float f))
            {
                return (int)Math.Round(f);
            };
            return 0;
        }

        public static bool TryParseIPAddr(string address, out string ip, out int port)
        {
            ip = "127.0.0.1";
            port = 1080;

            string[] parts = address.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            ip = parts[0];
            port = Clamp(Str2Int(parts[1]), 0, 65536);
            return true;
        }

        static string GetLinkPrefix(Model.Data.Enum.LinkTypes linkType)
        {
            return Model.Data.Table.linkPrefix[(int)linkType];
        }

        public static string GenPattern(Model.Data.Enum.LinkTypes linkType)
        {
            return string.Format(
               "{0}{1}{2}",
               StrConst("PatternNonAlphabet"), // vme[ss]
               GetLinkPrefix(linkType),
               StrConst("PatternBase64"));
        }

        public static string AddLinkPrefix(string b64Content, Model.Data.Enum.LinkTypes linkType)
        {
            return GetLinkPrefix(linkType) + b64Content;
        }

        public static string GetLinkBody(string link)
        {
            Regex re = new Regex("[a-zA-Z0-9]+://");
            return re.Replace(link, string.Empty);
        }

        public static void ZipFileDecompress(string zipFile, string outFolder)
        {
            // let downloader handle exception
            using (ZipFile zip = ZipFile.Read(zipFile))
            {
                var flattenFoldersOnExtract = zip.FlattenFoldersOnExtract;
                zip.FlattenFoldersOnExtract = true;
                zip.ExtractAll(outFolder, ExtractExistingFileAction.OverwriteSilently);
                zip.FlattenFoldersOnExtract = flattenFoldersOnExtract;
            }
        }
        #endregion

        #region UI related
        public static void CopyToClipboardAndPrompt(string content)
        {
            MessageBox.Show(
                Lib.Utils.CopyToClipboard(content) ?
                I18N("CopySuccess") :
                I18N("CopyFail"));
        }

        public static bool CopyToClipboard(string content)
        {
            try
            {
                Clipboard.SetText(content);
                return true;
            }
            catch { }
            return false;
        }

        public static string GetAppDir()
        {
            // The code below will fail in test.
            // return Path.GetDirectoryName(Application.ExecutablePath);

            // https://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path/35295609
            return System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void SupportProtocolTLS12()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }

        public static string GetClipboardText()
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                return Clipboard.GetText(TextDataFormat.Text);

            }
            return string.Empty;
        }
        #endregion

        #region process

        public static List<TResult> ExecuteInParallel<TParam, TResult>(List<TParam> values, Func<TParam, TResult> lamda)
        {
            var result = new List<TResult>();

            if (values.Count <= 0)
            {
                return result;
            }

            var taskList = new List<Task<TResult>>();
            foreach (var value in values)
            {
                var task = new Task<TResult>(() => lamda(value));
                taskList.Add(task);
                task.Start();
            }
            try
            {
                Task.WaitAll(taskList.ToArray());
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    throw e;
                }
            }

            foreach (var task in taskList)
            {
                result.Add(task.Result);
                task.Dispose();
            }

            return result;
        }

        public static void RunAsSTAThread(Action lamda)
        {
            // https://www.codeproject.com/Questions/727531/ThreadStateException-cant-handeled-in-ClipBoard-Se
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    lamda();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }

        public static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            // We must kill child processes first!
            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }

            // Then kill parents.
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited)
                {
                    proc.Kill();
                    proc.WaitForExit(1000);
                }
            }
            catch
            {
                // Process already exited.
            }
        }
        #endregion
    }
}
