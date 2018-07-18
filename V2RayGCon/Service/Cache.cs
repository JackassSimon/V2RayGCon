﻿namespace V2RayGCon.Service
{
    public class Cache : Model.BaseClass.SingletonService<Cache>
    {
        // special
        public Caches.HTML html;
        public Caches.Template tpl;
        public Caches.CoreCache core;

        Cache()
        {
            html = new Caches.HTML();
            tpl = new Caches.Template();
            core = new Caches.CoreCache();
        }

        #region public method

        #endregion

        #region private method

        #endregion
    }
}
