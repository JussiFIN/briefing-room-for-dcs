/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCSWorld.DB;
using BriefingRoom4DCSWorld.Template;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public class TemplateTreeViewManager : IDisposable
    {
        private static readonly Color BACKGROUND_COLOR = Color.FromArgb(255, 20, 92, 168);
        private static readonly Color FOREGROUND_COLOR = Color.White;
        private static readonly Font FONT = new Font("Courier New", 11f, FontStyle.Bold);

        private readonly ContextMenuStrip NodeMenu;
        private readonly MissionTemplate Template;
        private readonly TreeView TemplateTreeView;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TemplateTreeViewManager(MissionTemplate template, TreeView templateTreeView)
        {
            Template = template;

            TemplateTreeView = templateTreeView;
            TemplateTreeView.BackColor = BACKGROUND_COLOR;
            TemplateTreeView.Font = FONT;
            TemplateTreeView.ForeColor = FOREGROUND_COLOR;
            TemplateTreeView.NodeMouseClick += OnNodeMouseClick;
            TemplateTreeView.ShowNodeToolTips = true;

            NodeMenu = new ContextMenuStrip();
            NodeMenu.BackColor = BACKGROUND_COLOR;
            NodeMenu.Font = FONT;
            NodeMenu.ForeColor = FOREGROUND_COLOR;
            NodeMenu.ItemClicked += OnNodeMenuItemClicked;
            NodeMenu.ShowCheckMargin = false;
            NodeMenu.ShowImageMargin = false;
            NodeMenu.ShowItemToolTips = true;

            RefreshAll();
        }

        public void RefreshAll()
        {
            TemplateTreeView.Nodes.Clear();
            TemplateTreeView.Nodes.Add("theater", "");
            TemplateTreeView.Nodes.Add("weather", "");
            UpdateNodeValues();
        }

        private void UpdateNodeValues()
        {
            foreach (TreeNode n in TemplateTreeView.Nodes)
            {
                switch (n.Name)
                {
                    case "theater": n.Text = $"Theater: {Template.TheaterID}"; break;
                    case "weather": n.Text = $"Weather: {Template.EnvironmentWeather}"; break;
                }
            }

            TemplateTreeView.Sort();
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;
            TemplateTreeView.SelectedNode = e.Node;

            switch (e.Node.Name)
            {
                case "theater": ShowDropDownMenuDB<DBEntryTheater>(e.Location); return;
                case "weather": ShowDropDownMenuEnum<Weather>(e.Location); return;
            }
        }

        private void OnNodeMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if ((TemplateTreeView.SelectedNode == null) || (e.ClickedItem == null) || (e.ClickedItem.Tag == null)) return;

            switch (TemplateTreeView.SelectedNode.Name)
            {
                default: return; // Invalid value, return now because there's no need to call UpdateNodeValues()
                case "theater": Template.TheaterID = (string)e.ClickedItem.Tag; break;
                case "weather": Template.EnvironmentWeather = (Weather)e.ClickedItem.Tag; break;
            }

            UpdateNodeValues();
        }

        private void ShowDropDownMenuDB<T>(Point location) where T : DBEntry
        {
            NodeMenu.Items.Clear();
            foreach (string id in Database.Instance.GetAllEntriesIDs<T>())
                NodeMenu.Items.Add(id).Tag = id;

            NodeMenu.Show(TemplateTreeView, location);
        }

        private void ShowDropDownMenuEnum<T>(Point location) where T : struct
        {
            NodeMenu.Items.Clear();
            foreach (T value in Toolbox.GetEnumValues<T>())
                NodeMenu.Items.Add(value.ToString()).Tag = value;

            NodeMenu.Show(TemplateTreeView, location);
        }

        /// <summary>
        /// <see cref="IDisposable"/> implementation.
        /// </summary>
        public void Dispose()
        {

        }
    }
}
