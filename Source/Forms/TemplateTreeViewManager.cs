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

            NodeMenu = new ContextMenuStrip { BackColor = BACKGROUND_COLOR, Font = FONT, ForeColor = FOREGROUND_COLOR };
            NodeMenu.ItemClicked += OnNodeMenuItemClicked;
            NodeMenu.ShowCheckMargin = false;
            NodeMenu.ShowImageMargin = false;
            NodeMenu.ShowItemToolTips = true;

            RefreshAll();
        }

        public void RefreshAll()
        {
            TreeNode node;

            TemplateTreeView.Nodes.Clear();
            
            node = TemplateTreeView.Nodes.Add("theater", "");
            node.Nodes.Add("theaterCountriesCoalitions", "");
            node.Nodes.Add("theaterHomeAirbase", "");

            node = TemplateTreeView.Nodes.Add("environment", "Environment");
            node.Nodes.Add("environmentSeason", "");
            node.Nodes.Add("environmentTimeOfDay", "");

            node = TemplateTreeView.Nodes.Add("weather", "");
            node.Nodes.Add("weatherWind", "");

            UpdateNodeValues();
            TemplateTreeView.ExpandAll();
        }

        private void UpdateNodeValues()
        {
            foreach (TreeNode n in TemplateTreeView.Nodes.GetAllNodes())
            {
                switch (n.Name)
                {
                    case "environmentSeason": n.Text = $"Season: {Template.EnvironmentSeason}"; break;
                    case "environmentTimeOfDay": n.Text = $"Time of day: {Template.EnvironmentTimeOfDay}"; break;
                    case "theater": n.Text = $"Theater: {Template.TheaterID}"; break;
                    case "theaterCountriesCoalitions": n.Text = $"Countries coalitions: {Template.TheaterRegionsCoalitions}"; break;
                    case "theaterHomeAirbase": n.Text = $"Home airbase: {(string.IsNullOrEmpty(Template.TheaterStartingAirbase) ? "(Random)" : Template.TheaterStartingAirbase)}"; break;
                    case "weather": n.Text = $"Weather: {Template.EnvironmentWeather}"; break;
                    case "weatherWind": n.Text = $"Wind: {Template.EnvironmentWind}"; break;
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
                case "environmentSeason": ShowDropDownMenuEnum<Season>(e.Location); return;
                case "environmentTimeOfDay": ShowDropDownMenuEnum<TimeOfDay>(e.Location); return;
                case "theater": ShowDropDownMenuDB<DBEntryTheater>(e.Location); return;
                case "theaterCountriesCoalitions": ShowDropDownMenuEnum<CountryCoalition>(e.Location); return;
                case "theaterHomeAirbase":
                    ShowDropDownMenuString(e.Location,
                        (from DBEntryTheaterAirbase ab in Database.Instance.GetEntry<DBEntryTheater>(Template.TheaterID).Airbases select ab.Name).ToArray());
                    return;
                case "weather": ShowDropDownMenuEnum<Weather>(e.Location); return;
                case "weatherWind": ShowDropDownMenuEnum<Wind>(e.Location); return;
            }
        }

        private void OnNodeMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if ((TemplateTreeView.SelectedNode == null) || (e.ClickedItem == null) || (e.ClickedItem.Tag == null)) return;

            switch (TemplateTreeView.SelectedNode.Name)
            {
                default: return; // Invalid value, return now because there's no need to call UpdateNodeValues()
                case "environmentSeason": Template.EnvironmentSeason = (Season)e.ClickedItem.Tag; break;
                case "environmentTimeOfDay": Template.EnvironmentTimeOfDay = (TimeOfDay)e.ClickedItem.Tag; break;
                case "theater": Template.TheaterID = (string)e.ClickedItem.Tag; break;
                case "theaterCountriesCoalitions": Template.TheaterRegionsCoalitions = (CountryCoalition)e.ClickedItem.Tag; break;
                case "theaterHomeAirbase": Template.TheaterStartingAirbase = (string)e.ClickedItem.Tag; break;
                case "weather": Template.EnvironmentWeather = (Weather)e.ClickedItem.Tag; break;
                case "weatherWind": Template.EnvironmentWind = (Wind)e.ClickedItem.Tag; break;
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

        private void ShowDropDownMenuString(Point location, params string[] values)
        {
            NodeMenu.Items.Clear();
            foreach (string value in values)
                NodeMenu.Items.Add(value).Tag = value;

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
