﻿/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar
(https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World.
If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCSWorld.Campaign;
using System;
using System.IO;
using System.Windows.Forms;

namespace BriefingRoom4DCSWorld.Forms
{
    public partial class CampaignCreatorForm : Form
    {
        private readonly CampaignTemplate Template;

        public CampaignCreatorForm()
        {
            InitializeComponent();

            Template = new CampaignTemplate();
        }

        private void CampaignCreatorForm_Load(object sender, EventArgs e)
        {
            TemplatePropertyGrid.SelectedObject = Template;
            Icon = GUITools.GetIconFromResource("CampaignIcon.ico");

            T_New.Image = GUITools.GetImageFromResource("Icons.New.png");
            T_Open.Image = GUITools.GetImageFromResource("Icons.Open.png");
            T_SaveAs.Image = GUITools.GetImageFromResource("Icons.SaveAs.png");
            T_ExportCampaign.Image = GUITools.GetImageFromResource("Icons.ExportCampaign.png");
            T_Close.Image = GUITools.GetImageFromResource("Icons.Exit.png");
        }

        private void OnMenuClick(object sender, EventArgs e)
        {
            switch (((ToolStripButton)sender).Name)
            {
                case "T_New":
                    if (MessageBox.Show(
                        "Reset all values to their defauts?\r\nUnsaved changes will be lost.", "New campaign",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                    Template.Clear();
                    TemplatePropertyGrid.Refresh();
                    return;
                case "T_Open":
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "BriefingRoom campaign templates (*.brc)|*.brc";
                        ofd.RestoreDirectory = true;
                        ofd.InitialDirectory = BRPaths.TEMPLATES;
                        if ((ofd.ShowDialog() == DialogResult.OK) && File.Exists(ofd.FileName))
                        {
                            Template.LoadFromFile(ofd.FileName);
                            TemplatePropertyGrid.Refresh();
                        }
                    }
                    return;
                case "T_SaveAs":
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "BriefingRoom campaign templates (*.brc)|*.brc";
                        sfd.RestoreDirectory = true;
                        sfd.InitialDirectory = BRPaths.TEMPLATES;
                        if (sfd.ShowDialog() == DialogResult.OK)
                            Template.SaveToFile(sfd.FileName);
                    }
                    return;
                case "T_ExportCampaign":
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "DCS World campaign (*.cmp)|*.cmp";
                        sfd.RestoreDirectory = true;
                        sfd.InitialDirectory = Toolbox.GetDCSCampaignPath();
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            CampaignToolStripMenu.Enabled = false;
                            using (CampaignGenerator generator = new CampaignGenerator())
                            {
                                generator.Generate(Template, sfd.FileName);
                                MessageBox.Show("Campaign saved successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            CampaignToolStripMenu.Enabled = true;
                        }
                    }
                    return;
                case "T_Close":
                    Close();
                    return;
            }
        }
    }
}
