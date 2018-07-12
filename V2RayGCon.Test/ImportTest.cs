﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static V2RayGCon.Lib.StringResource;


namespace V2RayGCon.Test
{
    [TestClass]
    public class ImportTest
    {
        [DataTestMethod]
        [DataRow("a.b.", "a.b", "")]
        [DataRow("a.b.c", "a.b", "c")]
        [DataRow(".", "", "")]
        [DataRow(".b", "", "b")]
        [DataRow("", "", "")]
        public void PathParseTest(string path, string parent, string key)
        {
            var v = Lib.Utils.ParsePathIntoParentAndKey(path);
            Assert.AreEqual<string>(parent, v.Item1);
            Assert.AreEqual<string>(key, v.Item2);

        }

        [DataTestMethod]
        [DataRow(
            @"{'routing':{'a':1,'settings':{'c':1,'rules':[{'b':2}]}}}",
            @"{'routing':{'b':2,'settings':{'c':2,'rules':[{'a':1}]}}}",
            @"{'routing':{'a':1,'b':2,'settings':{'c':2,'rules':[{'a':1},{'b':2}]}}}")]
        [DataRow(
            @"{'routing':{'a':1,'settings':{'c':1,'rules':[{'a':1}]}}}",
            @"{'routing':{'b':2,'settings':{'c':2,'rules':[]}}}",
            @"{'routing':{'a':1,'b':2,'settings':{'c':2,'rules':[{'a':1}]}}}")]
        [DataRow(
            @"{'routing':{'a':1,'settings':{'c':1,'rules':null}}}",
            @"{'routing':{'b':2,'settings':{'c':2,'rules':[{'a':1}]}}}",
            @"{'routing':{'a':1,'b':2,'settings':{'c':2,'rules':[{'a':1}]}}}")]
        [DataRow(
            @"{'routing':{'a':1,'settings':{'rules':[]}}}",
            @"{'routing':{'b':2,'settings':{'rules':[]}}}",
            @"{'routing':{'a':1,'b':2,'settings':{'rules':[]}}}")]
        [DataRow(
            @"{'routing':{'a':1}}",
            @"{'routing':{'b':2,'settings':{'rules':[{'b':1}]}}}",
            @"{'routing':{'a':1,'b':2,'settings':{'rules':[{'b':1}]}}}")]
        [DataRow(
            @"{'routing':{'a':1,'settings':{'rules':[{'b':2}]}}}",
            @"{'routing':{'a':2,'settings':{'rules':[{'b':1}]}}}",
            @"{'routing':{'a':2,'settings':{'rules':[{'b':1},{'b':2}]}}}")]
        [DataRow(
            @"{'inboundDetour':[{'a':1}],'outboundDetour':null}",
            @"{'inboundDetour':null,'outboundDetour':[{'b':1}]}",
            @"{'inboundDetour':[{'a':1}],'outboundDetour':[{'b':1}]}")]
        [DataRow(
            @"{'inboundDetour':[{'a':1}]}",
            @"{'inboundDetour':[{'b':1}]}",
            @"{'inboundDetour':[{'b':1},{'a':1}]}")]
        [DataRow(@"{'a':1,'b':1}", @"{'b':2}", @"{'a':1,'b':2}")]
        [DataRow(@"{'a':1}", @"{'b':1}", @"{'a':1,'b':1}")]
        [DataRow(@"{}", @"{}", @"{}")]
        public void MergeConfigTest(string left, string right, string expect)
        {
            // outboundDetour inboundDetour
            var l = JObject.Parse(left);
            var r = JObject.Parse(right);
            var e = JObject.Parse(expect);
            var result = Lib.Utils.MergeConfig(l, r);

            Assert.AreEqual(true, JObject.DeepEquals(e, result));
        }

        [TestMethod]
        public void DetectJArrayTest()
        {
            var config = JObject.Parse(resData("config_def"));
            var domainOverride = Lib.Utils.GetKey(config, "inTpl.domainOverride");
            var isArray = domainOverride is JArray;
            var isObject = domainOverride is JObject;

            string[] list = null;
            if (isArray)
            {
                list = domainOverride.ToObject<string[]>();
            }

            Assert.AreEqual(true, isArray);
            Assert.AreEqual(false, isObject);
            Assert.AreNotEqual(null, list);
        }

        [DataTestMethod]
        [DataRow(@"{}", @"{}", @"{}")]
        [DataRow(@"{a:'123',b:null}", @"{a:null,b:'123'}", @"{a:null,b:'123'}")]
        [DataRow(@"{a:[1,2],b:{}}", @"{a:[3],b:{a:[1,2,3]}}", @"{a:[3,2],b:{a:[1,2,3]}}")]
        public void MergeJson(string first, string second, string expect)
        {
            var v = Lib.Utils.MergeJson(JObject.Parse(first), JObject.Parse(second));
            var e = JObject.Parse(expect);

            Assert.AreEqual(true, JObject.DeepEquals(v, e));
        }

        [TestMethod]
        public void ParseImportTest()
        {
            var data = new Dictionary<string, string>();

            void kv(string name, string key, string val)
            {
                var json = JObject.Parse(@"{}");
                if (data.ContainsKey(name))
                {
                    json = JObject.Parse(data[name]);
                }
                json[key] = val;
                data[name] = json.ToString(Newtonsoft.Json.Formatting.None);
            }

            void import(string name, string url)
            {
                var json = JObject.Parse(@"{}");
                if (data.ContainsKey(name))
                {
                    json = JObject.Parse(data[name]);
                }
                var imp = Lib.Utils.GetKey(json, "v2raygcon.import");
                if (imp == null || !(imp is JObject))
                {
                    json["v2raygcon"] = JObject.Parse(@"{'import':{}}");

                }
                json["v2raygcon"]["import"][url] = "";
                data[name] = json.ToString(Newtonsoft.Json.Formatting.None);
            }

            List<string> fetcher(List<string> keys)
            {
                var result = new List<string>();

                foreach (var key in keys)
                {
                    try
                    {
                        // Debug.WriteLine(key);
                        result.Add(data[key]);
                    }
                    catch
                    {
                        throw new System.Net.WebException();
                    }
                }

                return result;
            }

            bool eq(JObject left, JObject right)
            {
                var jleft = left.DeepClone() as JObject;
                var jright = right.DeepClone() as JObject;
                jleft["v2raygcon"] = null;
                jright["v2raygcon"] = null;
                return JObject.DeepEquals(jleft, jright);
            }

            JObject parse(string key, int depth = 3)
            {
                var config = JObject.Parse(data[key]);
                return Lib.ImportParser.ParseImportRecursively(fetcher, config, depth);
            }

            void check(string expect, string value)
            {
                Assert.AreEqual(true, eq(JObject.Parse(expect), parse(value)));
            }

            data["base"] = "{'v2raygcon':{}}";
            kv("a", "a", "1");
            kv("b", "b", "1");
            kv("baser", "r", "1");
            import("baser", "baser");
            import("mixAB", "a");
            import("mixAB", "b");
            import("mixC", "mixAB");
            kv("mixC", "a", "2");
            kv("mixC", "c", "1");
            import("mixCAb", "mixC");
            import("mixCAb", "mixAB");
            kv("mixCAb", "c", "2");
            import("mixABC", "a");
            import("mixABC", "b");
            import("mixABC", "mixC");
            import("final", "mixAB");
            import("final", "mixC");
            import("final", "mixCAb");
            import("final", "baser");
            kv("final", "msg", "omg");

            check(@"{'a':'2','b':'1','c':'2','r':'1','msg':'omg'}", "final");
            check(@"{'a':'2','b':'1','c':'1'}", "mixABC");
            check(@"{'a':'1','b':'1','c':'2'}", "mixCAb");
            check(@"{'a':'2','c':'1','b':'1'}", "mixC");
            check(@"{'a':'1','b':'1'}", "mixAB");
            check(data["base"], "base");
            check(data["baser"], "baser");
        }
    }
}
