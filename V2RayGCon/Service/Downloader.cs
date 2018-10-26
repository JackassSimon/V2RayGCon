﻿using System;
using System.IO;
using System.Net;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Service
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class Downloader
    {
        public event EventHandler OnDownloadCompleted, OnDownloadCancelled, OnDownloadFail;
        public event EventHandler<Model.Data.IntEvent> OnProgress;

        string _packageName;
        string _version;
        WebClient client;

        Service.Setting setting;

        public Downloader(Service.Setting setting)
        {
            this.setting = setting;

            SetArchitecture(false);
            _version = StrConst.DefCoreVersion;
            client = null;
        }

        #region public method

        public void SetArchitecture(bool win64 = false)
        {
            _packageName = win64 ? StrConst.PkgWin64 : StrConst.PkgWin32;
        }

        public void SetVersion(string version)
        {
            _version = version;
        }

        public string GetPackageName()
        {
            return _packageName;
        }

        public void DownloadV2RayCore()
        {
            Download();
        }

        public bool UnzipPackage()
        {
            try
            {
                Lib.Utils.ZipFileDecompress(
                    GetLocalFilePath(),
                    GetLocalFolderPath());
            }
            catch
            {
                // some code analizers may complain about empty catch block.
                return false;
            }
            return true;
        }

        public void Cleanup()
        {
            Cancel();
            client?.Dispose();
        }

        public void Cancel()
        {
            client?.CancelAsync();
        }
        #endregion

        #region private method
        void SendProgress(int percentage)
        {
            try
            {
                OnProgress?.Invoke(this,
                    new Model.Data.IntEvent(Math.Max(1, percentage)));
            }
            catch { }
        }

        void NotifyDownloadResults(bool status)
        {
            try
            {
                if (status)
                {
                    OnDownloadCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    OnDownloadFail?.Invoke(this, EventArgs.Empty);
                }
            }
            catch { }
        }

        void UpdateCore()
        {
            var servers = Service.Servers.Instance;

            var activeServers = servers.GetActiveServersList();

            servers.StopAllServersThen(() =>
            {
                var status = UnzipPackage();
                NotifyDownloadResults(status);
                if (activeServers.Count > 0)
                {
                    servers.RestartServersByListThen(activeServers);
                }
            });
        }

        void DownloadCompleted(bool cancelled)
        {
            client.Dispose();
            client = null;

            if (cancelled)
            {
                try
                {
                    OnDownloadCancelled?.Invoke(this, EventArgs.Empty);
                }
                catch { }
                return;
            }

            setting.SendLog(string.Format("{0}", I18N.DownloadCompleted));

            UpdateCore();
        }

        string GetLocalFolderPath()
        {
            return setting.isPortable ?
                Lib.Utils.GetAppDir() :
                Lib.Utils.GetSysAppDataFolder();
        }

        string GetLocalFilePath()
        {
            return Path.Combine(GetLocalFolderPath(), _packageName);
        }

        void Download()
        {
            string tpl = StrConst.DownloadLinkTpl;
            string url = string.Format(tpl, _version, _packageName);

            client = new WebClient();

            client.DownloadProgressChanged += (s, a) =>
            {
                SendProgress(a.ProgressPercentage);
            };

            client.DownloadFileCompleted += (s, a) =>
            {
                DownloadCompleted(a.Cancelled);
            };

            Lib.Utils.CreateAppDataFolder();
            setting.SendLog(string.Format("{0}:{1}", I18N.Download, url));
            client.DownloadFileAsync(new Uri(url), GetLocalFilePath());
        }

        #endregion
    }
}
