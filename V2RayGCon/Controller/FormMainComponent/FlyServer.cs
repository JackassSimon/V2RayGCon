﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Controller.FormMainComponent
{
    class FlyServer : FormMainComponentController
    {
        FlowLayoutPanel flyPanel;
        Service.Servers servers;
        ComboBox cboxMark;
        int preSelectedMarkFilterIndex;

        public FlyServer(
            FlowLayoutPanel panel,
            ComboBox cboxMarkeFilter)
        {
            this.servers = Service.Servers.Instance;
            this.flyPanel = panel;
            this.cboxMark = cboxMarkeFilter;

            InitMarkFilter(cboxMarkeFilter);
            BindDragDropEvent();
            servers.OnRequireFlyPanelUpdate += OnRequireFlyPanelUpdateHandler;
        }

        #region public method
        public void SelectAutorun()
        {
            LoopThroughAllServerItemControl((s) => s.SetSelected(s.GetAutorunStatus()));
        }

        public void SelectAll()
        {
            LoopThroughAllServerItemControl((s) => s.SetSelected(true));
        }

        public void SelectNone()
        {
            LoopThroughAllServerItemControl((s) => s.SetSelected(false));
        }

        public void SelectInvert()
        {
            LoopThroughAllServerItemControl((s) => s.SetSelected(!s.GetSelectStatus()));
        }

        public override void Cleanup()
        {
            servers.OnRequireFlyPanelUpdate -= OnRequireFlyPanelUpdateHandler;
            RemoveAllConrols(true);
        }

        public void RemoveAllConrols(bool blocking = false)
        {
            var controlList = GetAllControls();
            flyPanel.SuspendLayout();
            flyPanel.Controls.Clear();
            flyPanel.ResumeLayout();

            if (blocking)
            {
                DisposeFlyPanelControlByList(controlList);
            }
            else
            {
                Task.Factory.StartNew(
                    () => DisposeFlyPanelControlByList(
                        controlList));
            }
        }

        public override bool RefreshUI()
        {
            ResetIndex();
            var serverList = this.GetFilteredList();

            flyPanel.Invoke((MethodInvoker)delegate
            {
                if (serverList == null || serverList.Count > 0)
                {
                    RemoveWelcomeItem();
                }
                else
                {
                    RemoveAllConrols();
                    if (preSelectedMarkFilterIndex <= 0)
                    {
                        LoadWelcomeItem();
                    }
                    return;
                }

                RemoveDeletedServerItems(ref serverList);
                AddNewServerItems(serverList);
            });
            return true;
        }
        #endregion

        #region private method
        private void InitMarkFilter(ComboBox cboxMarkFilter)
        {
            UpdateMarkFilterItemList(cboxMarkFilter);
            cboxMarkFilter.DropDown += (s, e) =>
            {
                cboxMarkFilter.Invoke((MethodInvoker)delegate
                {
                    UpdateMarkFilterItemList(cboxMarkFilter);
                    Lib.UI.ResetComboBoxDropdownMenuWidth(cboxMarkFilter);
                });
            };
            cboxMarkFilter.SelectedIndexChanged += MarkFilterChangeHandler;
            cboxMarkFilter.TextChanged += MarkFilterTextChangeHandler;
            preSelectedMarkFilterIndex = -1;
            cboxMarkFilter.SelectedIndex = 0;
        }

        void MarkFilterTextChangeHandler(object sender, EventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(cboxMark.Text)
                    && !string.IsNullOrEmpty(cboxMark.Items[preSelectedMarkFilterIndex].ToString()))
                {
                    cboxMark.SelectedIndex = preSelectedMarkFilterIndex;
                }
            }
            catch { }
        }

        void MarkFilterChangeHandler(object sernder, EventArgs args)
        {
            var index = cboxMark.SelectedIndex;
            if (preSelectedMarkFilterIndex == index)
            {
                return;
            }

            preSelectedMarkFilterIndex = index;
            SelectNone();
            RemoveAllConrols();
            RefreshUI();
        }

        List<Model.Data.ServerItem> GetFilteredList()
        {
            var list = servers.GetServerList();
            if (preSelectedMarkFilterIndex < 0)
            {
                return list.ToList();
            }
            switch (preSelectedMarkFilterIndex)
            {
                case 0:
                    return list.ToList();
                case 1:
                    return list.Where(s => string.IsNullOrEmpty(s.mark)).ToList();
            }

            var markList = servers.GetMarkList();
            return list.Where(s => s.mark == markList[preSelectedMarkFilterIndex - 2]).ToList();
        }

        void UpdateMarkFilterItemList(ComboBox marker)
        {
            var itemList = servers.GetMarkList().ToList();
            itemList.Insert(0, I18N("NoMark"));
            itemList.Insert(0, I18N("All"));

            marker.Items.Clear();
            foreach (var item in itemList)
            {
                marker.Items.Add(item);
            }
        }

        void AddNewServerItems(List<Model.Data.ServerItem> serverList)
        {
            var controls = new List<Model.UserControls.ServerListItem>();

            foreach (var server in serverList)
            {
                controls.Add(new Model.UserControls.ServerListItem(server));
            }

            flyPanel.Controls.AddRange(controls.ToArray());
        }

        void DisposeFlyPanelControlByList(List<UserControl> controlList)
        {
            foreach (var control in controlList)
            {
                switch (control)
                {
                    case Model.UserControls.ServerListItem serv:
                        serv.Cleanup();
                        break;
                    case Model.UserControls.WelcomeFlyPanelComponent welcome:
                        welcome.Cleanup();
                        break;
                }
            }

            flyPanel.Invoke((MethodInvoker)delegate
            {
                foreach (var control in controlList)
                {
                    control.Dispose();
                }
            });
        }

        void RemoveDeletedServerItems(ref List<Model.Data.ServerItem> serverList)
        {
            var deletedControlList = GetDeletedControlList(serverList);

            flyPanel.SuspendLayout();
            foreach (var control in deletedControlList)
            {
                flyPanel.Controls.Remove(control);
            }
            flyPanel.ResumeLayout();
            Task.Factory.StartNew(() => DisposeFlyPanelControlByList(deletedControlList));
        }

        List<UserControl> GetDeletedControlList(List<Model.Data.ServerItem> serverList)
        {
            var controlList = GetAllControls();
            var result = new List<UserControl>();

            foreach (Model.UserControls.ServerListItem control in controlList)
            {
                var config = control.GetConfig();
                if (serverList.Where(s => s.config == config)
                    .FirstOrDefault() == null)
                {
                    result.Add(control);
                }
                serverList.RemoveAll(s => s.config == config);
            }

            return result;
        }

        void RemoveWelcomeItem()
        {
            var welcome = GetAllControls()
                .Where(c => c is Model.UserControls.WelcomeFlyPanelComponent)
                .FirstOrDefault();

            if (welcome == null)
            {
                return;
            }

            flyPanel.Controls.Remove(welcome);
            if (!welcome.IsDisposed)
            {
                welcome.Dispose();
            }
        }

        void LoopThroughAllServerItemControl(Action<Model.UserControls.ServerListItem> worker)
        {
            foreach (Model.BaseClass.IFormMainFlyPanelComponent control in flyPanel.Controls)
            {
                if (!(control is Model.UserControls.ServerListItem))
                {
                    continue;
                }
                worker(control as Model.UserControls.ServerListItem);
            }
        }

        List<UserControl> GetAllControls()
        {
            List<UserControl> controlList = new List<UserControl>();
            foreach (Model.BaseClass.IFormMainFlyPanelComponent control in flyPanel.Controls)
            {
                switch (control)
                {
                    case Model.UserControls.ServerListItem serv:
                        controlList.Add(serv);
                        break;
                    case Model.UserControls.WelcomeFlyPanelComponent welcome:
                        controlList.Add(welcome);
                        break;
                }
            }
            return controlList;
        }

        void OnRequireFlyPanelUpdateHandler(object sender, EventArgs args)
        {
            RefreshUI();
        }

        private void LoadWelcomeItem()
        {
            flyPanel.Controls.Add(
                new Model.UserControls.WelcomeFlyPanelComponent());
        }

        private void ResetIndex()
        {
            var list = servers.GetServerList().ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var index = i + 1.0; // closure
                var item = list[i];
                item.ChangeIndex(index);
            }
        }

        private void BindDragDropEvent()
        {
            flyPanel.DragEnter += (s, a) =>
            {
                a.Effect = DragDropEffects.Move;
            };

            flyPanel.DragDrop += (s, a) =>
            {
                // https://www.codeproject.com/Articles/48411/Using-the-FlowLayoutPanel-and-Reordering-with-Drag

                var serverItemMoving = a.Data.GetData(typeof(Model.UserControls.ServerListItem))
                    as Model.UserControls.ServerListItem;

                if (serverItemMoving == null)
                {
                    return;
                }

                var panel = s as FlowLayoutPanel;
                Point p = panel.PointToClient(new Point(a.X, a.Y));
                var controlDest = panel.GetChildAtPoint(p);
                int index = panel.Controls.GetChildIndex(controlDest, false);
                panel.Controls.SetChildIndex(serverItemMoving, index);
                var serverItemDest = controlDest as Model.UserControls.ServerListItem;
                MoveServerListItem(ref serverItemMoving, ref serverItemDest);
            };
        }

        void MoveServerListItem(ref Model.UserControls.ServerListItem moving, ref Model.UserControls.ServerListItem destination)
        {
            var indexDest = destination.GetIndex();
            var indexMoving = moving.GetIndex();

            if (indexDest == indexMoving)
            {
                return;
            }

            moving.SetIndex(
                indexDest < indexMoving ?
                indexDest - 0.5 :
                indexDest + 0.5);

            RefreshUI();
        }
        #endregion
    }
}
