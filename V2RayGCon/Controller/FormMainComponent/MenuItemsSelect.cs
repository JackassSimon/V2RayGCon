﻿using System.Windows.Forms;

namespace V2RayGCon.Controller.FormMainComponent
{
    class MenuItemsSelect : FormMainComponentController
    {


        public MenuItemsSelect(
            ToolStripMenuItem selectAll,
            ToolStripMenuItem selectNone,
            ToolStripMenuItem selectInvert,
            ToolStripMenuItem selectAutorun,
            ToolStripMenuItem selectRunning,
            ToolStripMenuItem selectTimeout,
            ToolStripMenuItem selectNoSpeedTest,
            ToolStripMenuItem selectNoMark)
        {
            // fly panel may not ready while this init
            selectNoMark.Click += (s, a) =>
            {
                GetFlyPanel().SelectNoMark();
            };


            selectNoSpeedTest.Click += (s, a) =>
            {
                GetFlyPanel().SelectNoSpeedTest();
            };

            selectTimeout.Click += (s, a) =>
            {
                GetFlyPanel().SelectTimeout();
            };

            selectRunning.Click += (s, a) =>
            {
                GetFlyPanel().SelectRunning();
            };

            selectAutorun.Click += (s, a) =>
            {
                GetFlyPanel().SelectAutorun();
            };

            selectAll.Click += (s, a) =>
            {
                GetFlyPanel().SelectAll();
            };

            selectNone.Click += (s, a) =>
            {
                GetFlyPanel().SelectNone();
            };

            selectInvert.Click += (s, a) =>
            {
                GetFlyPanel().SelectInvert();
            };
        }

        #region public method
        public override bool RefreshUI()
        {
            return false;
        }

        public override void Cleanup()
        {
        }
        #endregion

        #region private method

        Controller.FormMainComponent.FlyServer GetFlyPanel()
        {
            return this.GetContainer()
                .GetComponent<Controller.FormMainComponent.FlyServer>();
        }
        #endregion
    }
}
