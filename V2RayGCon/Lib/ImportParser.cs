﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Lib
{
    public class ImportParser
    {
        static Service.Cache cache;

        public static void Run(Service.Cache cacheServ)
        {
            cache = cacheServ;
        }

        #region public method
        /*
         * exceptions  
         * test<FormatException> base64 decode fail
         * test<System.Net.WebException> url not exist
         * test<Newtonsoft.Json.JsonReaderException> json decode fail
         */
        public static JObject Parse(string configString)
        {
            var maxDepth = Lib.Utils.Str2Int(StrConst.ParseImportDepth);

            var result = ParseImportRecursively(
                GetContentFromCache,
                JObject.Parse(configString),
                maxDepth);

            try
            {
                Lib.Utils.RemoveKeyFromJObject(result, "v2raygcon.import");
            }
            catch (KeyNotFoundException)
            {
                // do nothing;
            }

            return result;
        }

        public static JObject ParseImportRecursively(
            Func<List<string>, List<string>> fetcher,
            JObject config,
            int depth)
        {
            var empty = JObject.Parse(@"{}");

            if (depth <= 0)
            {
                return empty;
            }

            // var config = JObject.Parse(configString);

            var urls = GetImportUrls(config);
            var contents = fetcher(urls);

            if (contents.Count <= 0)
            {
                return config;
            }

            var configList =
                Lib.Utils.ExecuteInParallel<string, JObject>(
                    contents,
                    (content) =>
                    {
                        return ParseImportRecursively(
                            fetcher,
                            JObject.Parse(content),
                            depth - 1);
                    });

            var result = empty;
            foreach (var c in configList)
            {
                Lib.Utils.CombineConfig(ref result, c);
            }
            Lib.Utils.CombineConfig(ref result, config);

            return result;
        }
        #endregion

        #region private method
        static List<string> GetImportUrls(JObject config)
        {
            List<string> urls = null;
            var empty = new List<string>();
            var import = Lib.Utils.GetKey(config, "v2raygcon.import");
            if (import != null && import is JObject)
            {
                urls = (import as JObject).Properties().Select(p => p.Name).ToList();
            }
            return urls ?? new List<string>();
        }

        static List<string> GetContentFromCache(List<string> urls)
        {
            return urls.Count <= 0 ?
                urls :
                Lib.Utils.ExecuteInParallel<string, string>(
                    urls,
                    (url) => cache.html[url]);
        }
        #endregion

    }
}
