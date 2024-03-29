﻿// Form1.cs
// Copyright 2022 Thomas W. Shanley
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonClassesLib;
using DocxAdapterLib;
using G2RModel.Model;
using Ged2Reg.Model;
using GedcomObfuscationLib;
using OdtAdapterLib;

namespace Ged2Reg
{
    public partial class Form1 : Form, ILocalLogger
    {
        private RegisterReportModel _rrm;
        private CitationStrategyChoices _csss = new CitationStrategyChoices();
        private CitationStrategyChoices _csssFill = new CitationStrategyChoices();
        private BaptismOptionChoices _baptismOptionChoices = new BaptismOptionChoices();
        private AsyncActionDelegates _aad = new AsyncActionDelegates();
        private frmSettingsSets.ListOfNamedSettingSets _settingsSetsBound = new frmSettingsSets.ListOfNamedSettingSets();

        private Uri _githubUrl = new Uri("https://github.com/spacecat56/Ged2Reg");
        private bool _forceClose = false;

        public bool ForceLicense { get; set; }

        #region WinformsDesigner Fails - Workaround 

        // the dotnet core / dotnet5 winforms designer is unusable (tends to lose it
        // and has damaged our code, including once, by deleting the ENTIRE FORM layout. having
        // moved ahead to 5.0.x we were stuck.  here we bypass the unreliable designer
        // and instead add the needed changes at runtime

        // new bound bools in settings:
        // AncestorsReport
        // SuppressGenNbrs
        // AllFamilies
        // GenerationPrefix
        // GenerationHeadings
        // ...etc.

        private Button pbOpenLastRunOutput;
        private TabPage tpAncestry;
        private TabPage tpIndexes;
        private TabPage tpIo;
        private TabPage tpCitation;
        private Label lbAncestry;
        private Button pbListAncestors;
        private Button pbConform;
        private Button pbConformAncestry;
        private ComboBox cbAncestorChoices;
        private Button pbPickFocus;
        private Button pbTest;
        private int rowStep = 22;
        private int yPos = 32;
        private int lbColPos = 40;
        private int kbColPos = 320;
        private CheckBox kbItalicsLineage;
        private CheckBox kbStdBriefContd;
        private CheckBox kbGenNbrAll;
        private CheckBox kbIndexMarriedNames;
        private CheckBox kbMinusIndexChild;
        private CheckBox kbPlaceFirst;
        private CheckBox kbDownshift;
        private CheckBox kbEditDescr;
        private TextBox teFirstGen;
        private ToolStripMenuItem miTools;
        private ToolStripMenuItem miObfuscate;
        private CheckBox kbOpenAfter;
        private CheckBox kbPlaceholderCites;
        private CheckBox kbPreferEditedCites;
        private CheckBox kbAbrreviations;
        private CheckBox kbAppendTitle;

        private void AdjustForm ()
        {
            nudGenerations.Maximum = 999;

            tabControl1.TabPages.Remove(tabPage4);

            //
            // CHANGE TO the Notes tab
            //
            label49.Text = "Include Each Main Person\'s Notes";

            //
            // CHANGES TO the Citations tab
            //
            tpCitation = tabPage3;
            tpCitation.SuspendLayout();
            yPos = label21.Bottom + 2;
            lbColPos = label21.Left;
            kbColPos = checkBox15.Left;
            kbPlaceholderCites = AddBoundCheckBox(tpCitation, 
                "Placeholders for missing citations", 
                nameof(G2RSettings.CitationPlaceholders));
            checkBox15.Top++;
            Control[] tweaks = new Control[]{label13, label14, dgShortCited, dgFullCitations};
            foreach (Control control in tweaks)
            {
                control.Top += 2;
            }

            // make room!
            label13.Text = "Full citations (change Seq and sort by column to re-order)";
            yPos = label13.Location.Y + 4;
            lbColPos = label13.Right + 40;
            kbColPos = dgFullCitations.Right - kbPlaceholderCites.Width;
            kbPreferEditedCites = AddBoundCheckBox(tpCitation,
                "Prefer edited citations",
                nameof(G2RSettings.PreferEditedCitations));

            tpCitation.ResumeLayout();

            //
            // CHANGES TO the Input/Output tab
            //
            tpIo = tabPage1;
            tpIo.SuspendLayout();
            //sigh.  this works...
            yPos = label5.Bottom + 21;
            kbColPos = lbColPos = pbGo.Left;
            // ...this fails.  being dull today, I guess.
            //yPos = label5.Location.Y;
            //kbColPos = lbColPos = nudGenerations.Right + 16;
            kbOpenAfter = AddUnboundCheckbox(tpIo, "Open After Create", nameof(kbOpenAfter));

            label57.Left = cbSettingsSet.Left;
            label57.Top = cbSettingsSet.Top - (label57.Height + 2);
            label57.Text = "Current settings set:";
            label57.Visible = true;

            teGedcom.Validated += TeGedcom_Validated;

            tpIo.ResumeLayout();

            //
            // CHANGES TO the Content tab
            //
            TabPage tpContentOptions = tabPage2;

            yPos = teLastRunOutput.Top;
            lbColPos = textBox4.Right - 36;
            pbOpenLastRunOutput = AddButton(tpContentOptions, "...", 36, nam: nameof(pbOpenLastRunOutput), h:teLastRunOutput.Height);
            pbOpenLastRunOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pbOpenLastRunOutput.Click += PbOpenLastRunOutput_Click;

            teLastRunOutput.Width = textBox4.Width - 40;
            teLastRunOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;

            yPos = label30.Top - 6;
            lbColPos = label9.Left;
            kbColPos = checkBox4.Left;
            tpContentOptions.SuspendLayout();
            kbItalicsLineage = AddBoundCheckBox(tpContentOptions, 
                "Italicize names in lineage list", 
                nameof(G2RSettings.ItalicsNamesInLineageList));

            yPos = label61.Top + 4;
            lbColPos = label61.Left;
            kbColPos = kbIncludeEvents.Left;
            kbStdBriefContd = AddBoundCheckBox(tpContentOptions,
                "'Standard' output for children / abbrev.",
                nameof(G2RSettings.StandardBriefContinued));
            yPos = label61.Top + 4;
            kbColPos += kbStdBriefContd.Width;
            kbAbrreviations = AddBoundCheckBox(tpContentOptions,
                null,
                nameof(G2RSettings.AbbreviateChildEvents),
                nameof(kbAbrreviations));

            yPos = label45.Bottom + 36;
            pbConform = AddButton(tpContentOptions, "Conform settings", 180, label45.Left);
            pbConform.Click += pbConform_Click;


            yPos = (int) (label7.Bottom + (1.5 * rowStep));
            lbColPos = label7.Left;
            kbColPos = checkBox2.Left;
            kbGenNbrAll = AddBoundCheckBox(tpContentOptions,
                "Generation numbers on all children",
                nameof(G2RSettings.GenNbrAllChildren));


            lbColPos = label18.Left;
            kbColPos = checkBox13.Left;
            yPos = label19.Top + (label19.Top - label18.Top) + 4;
            kbPlaceFirst = AddBoundCheckBox(tpContentOptions,
                "Place before Date",
                nameof(G2RSettings.PlaceFirst));

            //yPos += 2;
            yPos--;
            teFirstGen = AddBoundTextBox(tpContentOptions, 
                "Starter generation 'number' (optional)", 
                nameof(G2RSettings.FirstGenNbr), 60, RightToLeft.No);


            lbColPos = label27.Left;
            kbColPos = checkBox21.Left;
            yPos = label27.Top - (label19.Top - label18.Top);
            kbDownshift = AddBoundCheckBox(tpContentOptions,
                "Shift all-caps names to mixed case",
                nameof(G2RSettings.DownshiftNames));

            label8.Text = "Include (event descriptions) / edited";
            kbColPos = checkBox3.Right + 4;
            yPos = checkBox3.Top;
            kbEditDescr = AddBoundCheckBox(tpContentOptions,
                null,
                nameof(G2RSettings.EditDescriptions),
                nameof(kbEditDescr));

            comboBox2.Top -= 2;


            lbColPos = label27.Left;
            kbColPos = checkBox21.Left;
            yPos = label35.Bottom + (label35.Height / 2);
            kbAppendTitle = AddBoundCheckBox(tpContentOptions,
                "Append Title to main person names",
                nameof(G2RSettings.AppendTitle));



            // extra copy of this setting, not needed
            kbAncestorsReport.Visible = label65.Visible = false;
            // hide grandkids - not implemented
            label6.Visible = checkBox1.Visible = false;
            // additional events - not implemented
            label61.Visible = kbIncludeEvents.Visible = false;
            tpContentOptions.ResumeLayout();

            //
            // CHANGES TO the Indexes tab
            //
            tpIndexes = tabPage6;
            tpIndexes.SuspendLayout();

            yPos = pbDefaultNameIndex.Bottom + rowStep / 2;
            lbColPos = label44.Left;
            kbColPos = pbDefaultNameIndex.Left;
            kbIndexMarriedNames = AddBoundCheckBox(tpIndexes,
                "+ married names",
                nameof(G2RSettings.IndexMarriedNames));

            //yPos = pbDefaultNameIndex.Bottom + rowStep / 2;
            //lbColPos = label44.Left;
            //kbColPos = pbDefaultNameIndex.Left;
            kbMinusIndexChild = AddBoundCheckBox(tpIndexes,
                "- continued child",
                nameof(G2RSettings.MinusChild));

            label43.Top += label43.Height * 2;

            tpIndexes.ResumeLayout();

            //
            // ADDED TAB for ancestry report settings
            //
            tpAncestry = new TabPage("Ancestry Report");
            tpAncestry.SuspendLayout();
            yPos = 32;
            lbColPos = 40;
            kbColPos = 320;

            //tabControl1.TabPages.Add(tpAncestry);
            // the next line is the "secret" workaround to the 
            // utterly bullshit, 15-year-old defect in 
            // TabPageCollection.Insert whereby it just plain has 
            // never worked.  force creation of the handle:
            IntPtr h = tabControl1.Handle;
            // ONLY THEN does Insert() actually work.  WTF, Microsoft?
            tabControl1.TabPages.Insert(2, tpAncestry);

            AddLabel(tpAncestry, "Options unique to Ancestry report:", xOffset: -20);
            yPos += rowStep;
            AddBoundCheckBox(tpAncestry, "Output Ancestors Report", nameof(G2RSettings.AncestorsReport));
            AddBoundCheckBox(tpAncestry, "Omit generation superscripts", nameof(G2RSettings.SuppressGenNbrs));
            AddBoundCheckBox(tpAncestry, "Allow multiple appearances", nameof(G2RSettings.AllowMultipleAppearances));
            AddBoundCheckBox(tpAncestry, "Placeholders for unknowns", nameof(G2RSettings.Placeholders));
            AddBoundCheckBox(tpAncestry, "Space between couples", nameof(G2RSettings.SpaceBetweenCouples));
            yPos += rowStep / 2;

            AddBoundCheckBox(tpAncestry, "Include back references", nameof(G2RSettings.IncludeBackRefs));
            AddBoundCheckBox(tpAncestry, "    Also list siblings", nameof(G2RSettings.IncludeSiblings));

            //AddBoundCheckBox(tpAncestry, "All families of ancestors (non-standard)", nameof(G2RSettings.AllFamilies));
            AddBoundCheckBox(tpAncestry, "Generation prefix numbers", nameof(G2RSettings.GenerationPrefix));
            yPos += rowStep / 2;

            AddBoundCheckBox(tpAncestry, "Focus on one ancestor (see below*)", nameof(G2RSettings.Focus));
            AddBoundCheckBox(tpAncestry, "Omit focus spouse(s)", nameof(G2RSettings.OmitFocusSpouses));
            AddBoundCheckBox(tpAncestry, "Continue past focal ancestor", nameof(G2RSettings.ContinuePastFocus));
            AddBoundCheckBox(tpAncestry, "'Merge' duplicate ancestors", nameof(G2RSettings.FindDuplicates));
            AddBoundTextBox(tpAncestry, "Minimize from generation", nameof(G2RSettings.MinimizeFromGeneration));
            yPos += rowStep / 4;
            AddBoundCheckBox(tpAncestry, "    Also drop back-references", nameof(G2RSettings.OmitBackRefsLater));

            int yPosLater = yPos + rowStep;
            int lbPosLater = lbColPos;
            int kbPosLater = kbColPos;
            yPos = 32;
            lbColPos = kbPosLater + 96;
            kbColPos = lbColPos + 280;

            AddLabel(tpAncestry, "Options that also affect Register report:", xOffset: -20);
            yPos += rowStep;
            AddBoundCheckBox(tpAncestry, "Generation headings", nameof(G2RSettings.GenerationHeadings));
            AddBoundCheckBox(tpAncestry, "Reduced margins", nameof(G2RSettings.ReducedMargins));
            AddBoundCheckBox(tpAncestry, "Use host name for link text", nameof(G2RSettings.UseHostName));

            yPos = yPosLater;
            lbColPos = lbPosLater;
            kbColPos = kbPosLater;
            AddLabel(tpAncestry, "*Focused ancestor", xOffset: -20);
            yPos += rowStep;
            var tb = AddBoundTextBox(tpAncestry, "Currently selected ancestor", nameof(G2RSettings.FocusName), 320, RightToLeft.No);
            tb.ReadOnly = true;
            tb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            yPos += 4;

            pbListAncestors = AddButton(tpAncestry, "List choices", 120);
            pbListAncestors.Click += pbListAncestors_Click;
            pbPickFocus = AddButton(tpAncestry, "Apply selection:", 120, lbColPos+pbListAncestors.Width+8);
            pbPickFocus.Click += pbPickFocus_Click;
            yPos += 4;
            cbAncestorChoices = AddComboBox(tpAncestry, nameof(cbAncestorChoices), 320);
            cbAncestorChoices.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            yPos = pbListAncestors.Bottom + pbListAncestors.Height;
            pbConformAncestry = AddButton(tpAncestry, "Conform settings", 180);
            pbConformAncestry.Click += pbConform_Click;

            tpAncestry.Location = new Point(4, 35);
            tpAncestry.Margin = new Padding(2);
            tpAncestry.Name = "tpAncestry";
            tpAncestry.Padding = new Padding(2);
            tpAncestry.Size = new Size(759, 623);
            // the following line throws from tab control, invalid index, some bullshit 
            //tpAncestry.Text = "Ancestry report";
            tpAncestry.UseVisualStyleBackColor = true;

            tpAncestry.PerformLayout();
            tpAncestry.ResumeLayout(false);


            //
            // Scaling related tweaks to Rewriters tab
            //
            pbNameRex.Top = teFinderNames.Top - (2 + pbNameRex.Height);
            pbEventRex.Top = teFinderEvents.Top - (2 + pbEventRex.Height);

            //
            // CHANGES TO the menu
            //
            miTools = new ToolStripMenuItem()
            {
                Name = nameof(miTools),
                Size = new Size(65, 29),
                Text = "Tools"
            };
            menuStrip1.Items.Insert(1, miTools);
            miObfuscate = new ToolStripMenuItem()
            {
                Name = nameof(miObfuscate),
                Size = new Size(150, 29),
                Text = "Create Obscured GEDCOM...",
                Enabled = false
            };
            miTools.DropDownItems.Add(miObfuscate);
            miObfuscate.Click += miObfuscate_Click;

            miLicenseInfo.Click += MiLicenseInfo_Click;
            miSampleInput.Click += MiSampleInput_Click;
            miUpdates.Click += MiUpdates_Click;
            miUserGuide.Click += MiUserGuide_Click;

        }

        private void TeGedcom_Validated(object sender, EventArgs e)
        {
            if (teGedcom.Text != "sample") return;
            _rrm.Settings.GedcomFile = teGedcom.Text = RegisterReportModel.PathToFileResource("sample.ged");
        }

        private ComboBox AddComboBox(TabPage tp, string name, int w = 200, int x = -1)
        {
            ComboBox cb = new ComboBox();

            //cb.DataBindings.Add(new Binding("SelectedValue", bsG2RSettings, "CitationStrategy", true));
            //cb.DataSource = citationStrategyChoicesBindingSource;
            //cb.DisplayMember = "Name";
            //cb.ValueMember = "Value";
            cb.FormattingEnabled = true;
            cb.Location = new Point(x >= 0 ? x : kbColPos, yPos);
            cb.Margin = new Padding(2);
            cb.Name = name;
            cb.Size = new Size(w, 34);
            cb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //cb.TabIndex = 61;
            tp.Controls.Add(cb);
            return cb;
        }

        private Button AddButton(TabPage tp, string txt, int w = 100, int x = -1, string nam = null, int h = 33)
        {
            Button pb = new Button();

            pb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pb.Location = new Point(x>=0?x:lbColPos, yPos);
            pb.Margin = new Padding(2);
            pb.Name = nam ?? $"pb{txt.Replace(" ", "")}";
            pb.Size = new Size(w, h);
            pb.Text = txt;
            pb.UseVisualStyleBackColor = true;
            tp.Controls.Add(pb);
            return pb;
        }

        private CheckBox AddUnboundCheckbox(TabPage tp, string lbl, string name = null)
        {
            CheckBox kb = new CheckBox
            {
                AutoSize = true,
                Checked = false,
                CheckState = CheckState.Unchecked,
                Location = new Point(kbColPos, yPos),
                Margin = new Padding(2),
                Name = name ?? $"kb{lbl.Replace(" ", "")}",
                RightToLeft = RightToLeft.Yes,
                Size = new Size(120, 21),
                Text = lbl,
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            tp.Controls.Add(kb);
            return kb;
        }
        private CheckBox AddBoundCheckBox(TabPage tp, string lbl, string boundSetting, string nam = null)
        {
            if (lbl != null)
                AddLabel(tp, lbl);

            CheckBox kb = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                CheckState = CheckState.Checked,
                Location = new Point(kbColPos, yPos),
                Margin = new Padding(2),
                Name = nam??$"kb{lbl.Replace(" ", "")}",
                RightToLeft = RightToLeft.Yes,
                Size = new Size(22, 21),
                TabIndex = 83,
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            kb.DataBindings.Add(new Binding("Checked", bsG2RSettings, boundSetting, true));
            tp.Controls.Add(kb);

            yPos += rowStep;
            return kb;
        }

        private TextBox AddBoundTextBox(TabPage tp, string lbl, string boundSetting, int w = 60, RightToLeft rtl = RightToLeft.Yes)
        {
            AddLabel(tp, lbl, 2);

            TextBox textBox = new TextBox
            {
                AutoSize = true,
                Location = new Point(kbColPos, yPos),
                Margin = new Padding(2),
                Name = $"te{lbl.Replace(" ", "")}",
                RightToLeft = rtl,
                Size = new Size(w, 21),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            textBox.DataBindings.Add(new Binding("Text", bsG2RSettings, boundSetting, true));
            tp.Controls.Add(textBox);

            yPos += rowStep;
            return textBox;
        }

        private void AddLabel(TabPage tp, string txt, int lblOffset = -3, int xOffset = 0)
        {
            tp.Controls.Add(new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(lbColPos+xOffset, yPos + lblOffset),
                Name = $"lbl{txt.Replace(" ", "")}",
                Text = txt,
                Visible = true,
            });
        }

        #endregion

        #region formMechanics
        public Form1()
        {
            InitializeComponent();
            //tpStyles.Visible = false; // does not work...
            //tabControl1.TabPages.Remove(tpStyles);
            InitLogDelegates();
            AdjustForm();
            Application.DoEvents();

            bsSettingsSets.DataSource = _settingsSetsBound;
            bsSettingsSets.ResetBindings(true);

            //bsSettingsSets.

            teSepNi.Height = teSepPi.Height = textBox1.Height; // Winforms can be SO STUPID

            citationStrategyChoicesBindingSource.DataSource = _csss;
            citationStrategyChoicesBindingSource1.DataSource = _csssFill;
            baptismOptionChoicesBindingSource.DataSource = _baptismOptionChoices;

            _rrm = new RegisterReportModel
            {
                Logger = this, 
                DocFactory = new OoxDocFactory()
            };

            LoadSettingsSets(true);

            _aad.CancelEnable = ena => { pbCancel.Enabled = ena; };
            _aad.PostStatusReport = txt => { Log(txt); };

            teLog.Height = panel1.Height - (teLog.Top + 10);
        }

        #region Overrides of Form

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Agreement agreed = null;
            frmAgreement fa = null;
            bool didAgree = false;
            try
            {
                try
                {
                    agreed = Agreement.Load();
                    didAgree = agreed.AgreedOn != null;
                }
                catch (FileNotFoundException fex)
                {
                    didAgree = false;
                }
                didAgree &= !ForceLicense;
                if (didAgree)
                    return;
                agreed = new Agreement();
                string path = RegisterReportModel.PathToFileResource("LICENSE");
                agreed.AgreedEul = File.ReadAllText(path);
                path = RegisterReportModel.PathToFileResource("NOTICE");
                agreed.AgreedAuthorship = File.ReadAllText(path);
                if (ForceLicense)
                    agreed.Reset();
                fa = new frmAgreement().Init(agreed);
                DialogResult dra = fa.ShowDialog(this);
                if (dra != DialogResult.OK || agreed.Status != StateOfPlay.Authorship)
                {
                    MessageBox.Show($"User '{agreed.AgreedUser}' has not accepted the license and terms.", "Ged2Red - Closing", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    _forceClose = true;
                    Application.Exit();
                }

                agreed.Save();
            }
            catch (Exception ex)
            {
                //
                Log(ex);
            }
        }

        #endregion

        private void LoadSettingsSets(bool preferLastActive = false)
        {
            // load the list of settings sets and start with the default set
            _rrm.SettingsSets = ListOfSettingsSets.Load();
            _rrm.Settings = _rrm.SettingsSets.GetSettings(preferLastActive: preferLastActive);
            teSettingsSet.Text = _rrm.Settings.SetName;
            BindUiToSettings();
            cbSettingsSet.SelectedValue = _rrm.Settings;
            Log($"Settings loaded; {_rrm.Settings.SetName} set selected");
        }

        private void BindUiToSettings()
        {
            bsG2RSettings.DataSource = _rrm.Settings;
            bsG2RSettings.ResetBindings(false);
            bsFullCitationParts.DataSource = _rrm.Settings.CitationPartsFull;
            bsFullCitationParts.ResetBindings(false);
            bsSeeNoteCitationParts.DataSource = _rrm.Settings.CitationPartsSeeNote;
            bsSeeNoteCitationParts.ResetBindings(false);
            DataGridViewComboBoxColumn cbc = dgShortCited.Columns["tagColumn"] as DataGridViewComboBoxColumn;
            cbc.DataSource = _rrm.Settings.CitationPartChoices;
            cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
            cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);

            cbc = dgFullCitations.Columns["tagColumnFull"] as DataGridViewComboBoxColumn;
            cbc.DataSource = _rrm.Settings.CitationPartChoices;
            cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
            cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);

            cbc = colStyleRole;
            cbc.DataSource = _rrm.Settings.StyleSlotChoices;
            cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
            cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);
            bsStyleAssignments.DataSource = _rrm.Settings.StyleMap;
            bsStyleAssignments.ResetBindings(false);

            cbc = colTitleRewriteContext;
            cbc.DataSource = _rrm.Settings.TextCleanerChoices;
            cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
            cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);
            bsTitleCleaners.DataSource = _rrm.Settings.TextCleaners;
            bsTitleCleaners.ResetBindings(false);

            cbSettingsSet.DataSource = bsSettingsSets;
            cbSettingsSet.ValueMember = "Value";
            cbSettingsSet.DisplayMember = "Name";
            _settingsSetsBound.Clear();
            foreach (G2RSettings set in _rrm.SettingsSets)
            {
                _settingsSetsBound.Add(new NamedValue<G2RSettings>(set.SetName, set));
            }
            bsSettingsSets.ResetBindings(false);
            //_settingsSetsBound.

            bsNameIndexSettings.DataSource = _rrm.Settings.NameIndexSettings;
            bsPlaceIndexSettings.DataSource = _rrm.Settings.PlaceIndexSettings;

            teSettingsSet.Text = _rrm.Settings.SetName;
        }

        private void PushReluctantBindings()
        {
            _rrm.Settings.Generations = (int) nudGenerations.Value;
            _rrm.Settings.LivingFence = (int) nudAgeGuess.Value;
            _rrm.Settings.NumberCitesToSumamrize = (int) nudCiteSummaryMax.Value;
            _rrm.Settings.NameIndexSettings.Columns = (int)nudColumnsNameIndex.Value;
            _rrm.Settings.PlaceIndexSettings.Columns = (int)nudColumnsPlaceIndex.Value;
        }

        private void ToggleUiEnable(bool ena)
        {
            tpAncestry.Enabled = tabPage2.Enabled = tabPage3.Enabled =
                tabPage4.Enabled = tabPage5.Enabled = tabPage6.Enabled = tabPage7.Enabled = ena;

            pnToggler.Enabled = pbGo.Enabled = ena;

            cbSettingsSet.Enabled = ena;

            loadSettingsToolStripMenuItem.Enabled = defaultSettingsToolStripMenuItem.Enabled =
                manageSettingsToolStripMenuItem.Enabled = ena;

            pbCancel.Enabled = !ena;
        }

        private char? _seeked;
        private void dgvStartPerson_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!char.IsLetter(c)) return;
            c = char.ToUpper(c);
            int ix = (_seeked.HasValue && c==_seeked.Value) 
                ? dgvStartPerson.FirstDisplayedScrollingRowIndex + 1
                : 0;
            _seeked = c;
            ix = _rrm.Individuals.Locate(c.ToString(), ix);
            dgvStartPerson.FirstDisplayedScrollingRowIndex = ix;
            dgvStartPerson.Rows[ix].Selected = true;
        }
       private void lvStyles_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            e.Graphics.DrawRectangle(Pens.Gray, e.Bounds);
        }

        private void lvStyles_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            e.Graphics.DrawRectangle(Pens.Gray, e.Bounds);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_forceClose) return;
                var dr = MessageBox.Show("Save Settings?", "Ged2Reg is Closing", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (dr == DialogResult.No)
                    return;

                _rrm.SettingsSets.Save();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void cbSettingsSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var sel = (cbSettingsSet.SelectedItem as NamedValue<G2RSettings>)?.Value;
                if (sel == null || sel == _rrm.Settings)
                    return;
                ApplySettingSetChoice(sel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        #endregion

        #region buttonActions
        private void pbInit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(teGedcom.Text))
                {
                    MessageBox.Show("GEDCOM (file name) is required");
                    return;
                }
                if (!File.Exists(teGedcom.Text))
                {
                    MessageBox.Show("GEDCOM file does not exist");
                    return;
                }
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                LoadGedData();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void pbOpenGedcom_Click(object sender, EventArgs e)
        {
            try
            {

                //UseWaitCursor = true;
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = "Select Gedcom File",
                    Multiselect = false,
                    Filter = "gedcom files (*.ged)|*.ged|All files (*.*)|*.*"
                };
                if (!string.IsNullOrEmpty(teGedcom.Text)
                    && !string.IsNullOrEmpty(Path.GetDirectoryName(teGedcom.Text)))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(teGedcom.Text);
                }

                if (ofd.ShowDialog() != DialogResult.OK) return;
                _rrm.Settings.GedcomFile = teGedcom.Text = ofd.FileName;
                LoadGedData();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                //UseWaitCursor = false;
                Cursor = Cursors.Default;
            }
        }

        private bool _debugging = false;
        private async void pbGo_Async(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_rrm.Settings.OutFile))
                {
                    if (MessageBox.Show("Output file exists: delete/replace it?", "Ged2Reg Warning", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.Cancel)
                    {
                        Log("Action canceled (output file already exists).");
                        return;
                    }
                    File.Delete(_rrm.Settings.OutFile);
                    Log($"Deleted prior version of output file {_rrm.Settings.OutFile}.");
                }
                _aad.CancelRequested = false;
                ToggleUiEnable(false);
                pbCancel.Focus();
                Application.DoEvents();
                PushReluctantBindings();
                Application.DoEvents();

                _rrm.Settings.ProgramName = AboutBox1.AssemblyTitle;
                _rrm.Settings.ProgramVer = AboutBox1.AssemblyVersion;
                
                // clear instances based on editable collections so they will be recreated with current contents
                _rrm.Settings.Reset(); 
                object o = dgvStartPerson.SelectedRows[0]?.DataBoundItem;

                // last chance:
                SetDocFactory();

                bool result = false;
                DateTime start = DateTime.Now;
                if (_debugging)
                    _rrm.Exec(o, _aad);
                else
                    await Task.Run(() => result = _rrm.Exec(o, _aad));

                // NB only save ONCE; save a second time RELOADS THE EFFING FILE FIRST!
                if (result)
                {
                    _rrm.Doc.Save();
                    _rrm.Doc.Dispose();
                    _rrm.Doc = null;
                    TimeSpan elapsed = DateTime.Now.Subtract(start);
                    Log(_rrm.Reporter.GetStatsSummary(), false);
                    Log($"Report created ({_rrm.Settings.OutFile} in {elapsed:g})");
                    Log("completed processing; see Log for details");
                    if (kbOpenAfter.Checked && File.Exists(_rrm.Settings.OutFile))
                    {
                        try
                        {
                            StartDefaultProcess(_rrm.Settings.OutFile);
                        }
                        catch (Exception ex)
                        {
                            string msg = $"Failed to open the output file ({ex.Message}).";
                            Log(msg);
                            MessageBox.Show(msg);
                        }
                    }
                }
                else
                {
                    try
                    {
                        _rrm.Doc.Dispose();
                    }
                    catch { }
                    finally{_rrm.Doc = null; }
                    Log(_rrm.Reporter.GetStatsSummary(), false);
                    Log("Report creation failed");
                }
                // do this here, hoping the property change events will propagate....
                // but that results in inaccurate reports
                //_rrm.Settings.LastRun = _rrm.Reporter.MyReportStats.EndTime = DateTime.Now;
                //_rrm.Settings.LastRunTimeSpan = _rrm.Reporter.MyReportStats.PrepTime.Add(_rrm.Reporter.MyReportStats.ReportTime);
                //_rrm.Settings.LastFileCreated = _rrm.Settings.OutFile;
                // so, let's jiggle the handle instead
                bsG2RSettings.ResetBindings(false);
            }
            catch (CanceledByUserException cbu)
            {
                MessageBox.Show("Canceled");
                Log("Canceled");
                if (_rrm.ActionDelegates != null) 
                    // don't leave cancel request lying around
                    _rrm.ActionDelegates.CancelRequested = false;
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                ToggleUiEnable(true);
                try
                {
                    _rrm.Doc?.Dispose();
                }
                catch { }
            }
        }

        private void PbOpenLastRunOutput_Click(object sender, EventArgs e)
        {
            try
            {
                string fn = _rrm.Settings.LastFileCreated;
                if (string.IsNullOrEmpty(fn)) 
                    throw new Exception("No file to open");
                if (!File.Exists(fn))
                    throw new Exception("File no longer exists");
                StartDefaultProcess(_rrm.Settings.OutFile);
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbOpenStylesDoc_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = "Select Gedcom File",
                    Multiselect = false,
                    Filter = "document files (*.docx)|*.docx|All files (*.*)|*.*"
                };
                if (!string.IsNullOrEmpty(teStylesFile.Text)
                    && !string.IsNullOrEmpty(Path.GetDirectoryName(teStylesFile.Text)))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(teStylesFile.Text);
                }
                if (ofd.ShowDialog() != DialogResult.OK) return;
                _rrm.Settings.StylesFile = teStylesFile.Text = ofd.FileName;
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbOutputFile_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Title = "Select Output File",
                    //CheckFileExists = true,
                    OverwritePrompt = true,
                    AddExtension = true,
                    DefaultExt = ".docx",
                    Filter = "Word files (*.docx)|*.docx|Open Document files (*.odt)|*.odt"
                };
                if (!string.IsNullOrEmpty(teOutFile.Text) 
                    && !string.IsNullOrEmpty(Path.GetDirectoryName(teOutFile.Text)))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(teOutFile.Text);
                }
                if (sfd.ShowDialog() != DialogResult.OK) return;
                _rrm.Settings.OutFile = teOutFile.Text = sfd.FileName;
                SetDocFactory();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void SetDocFactory()
        {
            string ext = Path.GetExtension(_rrm.Settings.OutFile);
            switch (ext)
            {
                case ".docx":
                    _rrm.DocFactory = new OoxDocFactory();
                    break;
                case ".odt":
                    _rrm.DocFactory = new OalDocFactory();
                    break;
            }
        }

        private void pbListStyles_Click(object sender, EventArgs e)
        {
            try
            {
                if (!RegisterReportModel.ValidateStylesDoc(_rrm.DocFactory, _rrm.Settings.GetStylesStream()))
                {
                    MessageBox.Show(
                        "Footnotes/Endnotes in the style document may result in invalid output; using an empty file (styles only) is recommended.",
                        "Problem With Styles File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                lvStyles.Clear();
                lvStyles.Columns.Add("Style Name");
                lvStyles.Columns.Add("Style Id");
                lvStyles.Columns.Add("Style Type");
                lvStyles.View = View.Details;
                
                foreach (string[] style in RegisterReportModel.ListAvailableStyles(_rrm.DocFactory, _rrm.Settings.GetStylesStream()))
                {
                    lvStyles.Items.Add(new ListViewItem(style));
                }

                foreach (ColumnHeader header in lvStyles.Columns)
                {
                    header.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbDefaultNameIndex_Click(object sender, EventArgs e)
        {
            try
            {
                _rrm.Settings.InitNameIndexSettings();
                bsNameIndexSettings.DataSource = _rrm.Settings.NameIndexSettings;
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbDefaultPlaceIndex_Click(object sender, EventArgs e)
        {
            try
            {
                _rrm.Settings.InitPlaceIndexSettings();
                bsPlaceIndexSettings.DataSource = _rrm.Settings.PlaceIndexSettings;
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _aad.CancelRequested = true;
                Log("Cancel pending...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }
        private void pbConform_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Conforming settings to conventions...");
                _rrm.Settings.ConformToRegister(sender == pbConform);
                Log(_rrm.Settings.ReportConformanceSettings(), false);
                Log("...best conforming settings applied, see Log for details");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }


        private void pbListAncestors_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                if (dgvStartPerson.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Must load GEDCOM and select a person", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                Log("Building list of ancestors...");
                Application.DoEvents();
                _rrm.ResetGedcom();
                var o = dgvStartPerson.SelectedRows[0]?.DataBoundItem as GedcomIndividual;
                var t = new ReportTreeBuilder
                {
                    AllFamilies = false,
                    AllowMultiple = true,
                    Cm = null,
                    Root = o,
                    GenerationsOverride = 999
                };
                t.Init().Exec();
                if (_rrm.Settings.FindDuplicates)
                {
                    t.IdentifyDuplicates();
                    Log($"Groups of (apparent) duplicates: {t.DuplicationGroups}");
                }
                // NB we need "natural" (GEDCOM) ids to save in settings, but
                // will only recognize duplicates based on the consolidated, 
                // ReportEntry.Id property, which will 'see' the optional override
                List<ReportEntry> choices = new List<ReportEntry>();
                for (int i = 1; i < t.LastSlotWithPeople; i++)
                {
                    choices.AddRange(t.Generations[i]);
                }
                
                // prepare them.  we want to be able to flag multi-line ancestors
                choices = choices.OrderBy(c => c.Id).ToList(); // here we need to 'see' the override Id
                List<NamedValue<ReportEntry>> nvChoices = new List<NamedValue<ReportEntry>>();
                int occurrences = 0;
                int multiLine = 0;
                for (int i = 0; i < choices.Count; i++)
                {
                    if (i < choices.Count - 1 
                        && choices[i].Id == choices[i + 1].Id)
                    {
                        occurrences++;
                        continue;
                    }
                    
                    string pn = occurrences > 1
                        ? $"{choices[i].Individual.PresentationName()} [{occurrences}]" : choices[i].Individual.PresentationName();
                    nvChoices.Add(new NamedValue<ReportEntry>(pn, choices[i]));
                    if (occurrences > 1) multiLine++;
                    occurrences = 1;
                }

                var nvcs = nvChoices
                    .Where(x => !x.Value.Individual.PresentationName().StartsWith("(Unknown),"))
                    .OrderBy(nv => nv.Name)
                    .ToList();
                List<string> choiceList = nvcs.Select(nv => nv.Name).ToList();
                _ancestorChoices = nvcs.Select(nv => nv.Value).ToList();

                // present them
                cbAncestorChoices.Items.Clear();
                cbAncestorChoices.Items.AddRange(choiceList.ToArray<object>());
                cbAncestorChoices.SelectedIndex = -1;
                Log($"Ancestor choices: {choiceList.Count}; with multiple lines: {multiLine}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void pbPickFocus_Click(object sender, EventArgs e)
        {
            try
            {
                int ix = cbAncestorChoices.SelectedIndex;
                var indi = _ancestorChoices[ix];
                _rrm.Settings.FocusName = cbAncestorChoices.SelectedItem.ToString();
                _rrm.Settings.FocusId = indi.NaturalId; // we need to 'see' the actual GEDCOM id here
                bsG2RSettings.ResetBindings(false);

                Log($"Focus ancestor selected: {_rrm.Settings.FocusName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbTestFocus_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvStartPerson.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Must load GEDCOM and select a person", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                Log("Building list of ancestors...");
                Application.DoEvents();
                _rrm.ResetGedcom();
                GedcomIndividual o = dgvStartPerson.SelectedRows[0]?.DataBoundItem as GedcomIndividual;
                ReportTreeBuilder t = new ReportTreeBuilder
                {
                    AllFamilies = true,
                    AllowMultiple = true,
                    Cm = null,
                    Root = o
                };
                t.Init().Exec();
                Log("Focusing list of ancestors...");

                t.ApplyReduction(_rrm.Settings.FocusId, true);

                Log($"Focus ancestor applied: {_rrm.Settings.FocusName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbNameRex_Click(object sender, EventArgs e)
        {
            frmRegexTester fr = null;
            try
            {
                fr = new frmRegexTester()
                {
                    TheRegex = teFinderNames.Text,
                    TheReplacer = teFixerNames.Text,
                    TheContext = "Rewriting Names"
                };
                if (fr.ShowDialog(this) != DialogResult.OK)
                    return;
                _rrm.Settings.FinderForNames = fr.TheRegex;
                _rrm.Settings.FixerForNames = fr.TheReplacer;
                Log("Rewriter for Names updated");
                fr.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                fr?.Dispose();
            }
        }

        private void pbEventRex_Click(object sender, EventArgs e)
        {
            frmRegexTester fr = null;
            try
            {
                fr = new frmRegexTester()
                {
                    TheRegex = teFinderEvents.Text,
                    TheReplacer = teFixerEvents.Text,
                    TheContext = "Rewriting Event Descriptions"
                };
                if (fr.ShowDialog(this) != DialogResult.OK)
                    return;
                _rrm.Settings.FinderForEvents = fr.TheRegex;
                _rrm.Settings.FixerForEvents = fr.TheReplacer;
                Log("Rewriter for Event Descriptions updated");
                fr.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                fr?.Dispose();
            }
        }
        #endregion

        #region menuActions
        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _rrm.SettingsSets.Save();
                Log("Settings saved");
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private G2RSettings _restoredSettings;

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadSettingsSets();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void defaultSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("Keep title rewrites?", "Ged2Reg Option",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel)
                    return;
                bool keepTitleRewrites = (DialogResult.Yes == dr);
                if (keepTitleRewrites)
                    _rrm.Settings = new G2RSettings { SetName = _rrm.Settings.SetName, GedcomFile = teGedcom.Text, OutFile = teOutFile.Text, TextCleaners = _rrm.Settings.TextCleaners }
                        .Defaults().Init();
                else
                    _rrm.Settings = new G2RSettings { SetName = _rrm.Settings.SetName, GedcomFile = teGedcom.Text, OutFile = teOutFile.Text }
                        .Defaults().Init();

                BindUiToSettings();
                _rrm.SettingsSets.Update(_rrm.Settings);
                Log($"Settings set {_rrm.Settings.SetName} reset to default values");
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog(this);
        }

        private void installedLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string applicationDirectory = Application.ExecutablePath;
                MessageBox.Show($"Program is run from {applicationDirectory}", "Ged2Reg - Location",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void manageSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ListOfSettingsSets tempListOfSettingsSets = new ListOfSettingsSets();
                tempListOfSettingsSets.AddRange(_rrm.SettingsSets);
                frmSettingsSets fss = new frmSettingsSets {SettingSets = tempListOfSettingsSets, SelectedSet = _rrm.Settings}.Init();
                if (fss.ShowDialog(this) != DialogResult.OK)
                    return;
                _rrm.SettingsSets = tempListOfSettingsSets;
                G2RSettings sel = fss.SelectedSet;
                ApplySettingSetChoice(sel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void ApplySettingSetChoice(G2RSettings sel)
        {
            //_rrm.Settings = sel.InitInternals();
            _rrm.Activate(sel);
            BindUiToSettings();
            bsIndi.DataSource = new ListOfGedcomIndividuals();
            bsIndi.ResetBindings(false);
            pbGo.Enabled = false;
            Log($"Settings set {_rrm.Settings.SetName} is active");
            cbSettingsSet.SelectedValue = sel;
            //_rrm.SettingsSets.LastActiveSet = _rrm.Settings.SetName;
        }

        private void miObfuscate_Click(object sender, EventArgs e)
        {
            void FailMsg()
            {
                string msg = null;
                if (_obfu.LastException != null)
                {
                    Log(_obfu.LastException);
                    msg = $" ({_obfu.LastException.Message})";
                }

                Log($"Failed to create Obfuscated (obscured) GEDCOM file{msg}");
            }

            try
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    Title = "Save obscured GEDCOM",
                    DefaultExt = ".ged",
                    AddExtension = true
                };
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
                Cursor = Cursors.WaitCursor;
                _obfu = new GedcomObfuscator()
                {
                    FileNameOut = sfd.FileName,
                };

                Log($"Creating obfuscated (obscured) GEDCOM file{sfd.FileName}");
                Application.DoEvents();

                if (_obfu.Init(_rrm))
                {
                    if (_obfu.Exec())
                    {
                        Log(_obfu.ResultsText.ToString());
                        Log("Obfuscated (obscured) GEDCOM file(s) written, see log for details");
                    }
                    else
                    {
                        FailMsg();
                    }
                }
                else
                {
                    FailMsg();
                }
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MiUserGuide_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                string ugFn = RegisterReportModel.PathToFileResource("Ged2Reg User Guide.pdf");
                StartDefaultProcess(ugFn);
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static void StartDefaultProcess(string filePath)
        {
            Process p = new Process()
            {
                StartInfo =
                {
                    FileName = filePath,
                    UseShellExecute = true
                }
            };
            p.Start();
        }

        private void MiUpdates_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                StartDefaultProcess(_githubUrl.ToString());
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MiSampleInput_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                teGedcom.Text = RegisterReportModel.PathToFileResource("sample.ged" );
                _rrm.ConfigureSampleFile(teGedcom.Text);
                pbInit_Click(sender, e);
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        //public void ConfigureSampleFile(string path)
        //{
        //    _rrm.Settings.LastPersonFile = _rrm.Settings.GedcomFile = path;
        //    if (_rrm.Settings.AncestorsReport)
        //        _rrm.Settings.LastPersonId = "@I376@";
        //    else
        //        _rrm.Settings.LastPersonId = "@I2392@";
        //    if (_rrm.Settings.TextCleaners != null && _rrm.Settings.TextCleaners.Count == 0)
        //    {
        //        _rrm.Settings.TextCleaners.Add(new TextCleanerEntry()
        //        {
        //            Context = TextCleanerContext.Everywhere,
        //            FirstUseUnchanged = true,
        //            Input = "(Brede, Sussex, England), Parish Registers",
        //            Output = "Brede Parish Registers",
        //            ReplaceEntire = true
        //        });

        //        _rrm.Settings.TextCleaners.Add(new TextCleanerEntry()
        //        {
        //            Context = TextCleanerContext.Everywhere,
        //            FirstUseUnchanged = false,
        //            Input = "United States Federal Census",
        //            Output = "US Census",
        //            ReplaceEntire = false
        //        });
        //    }
        //}

        private void MiLicenseInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                Agreement agreed = Agreement.Load();
                frmAgreement fa = new frmAgreement().Init(agreed);
                fa.ShowDialog();
            }
            catch (Exception ex)
            {
                Log(ex);
                MessageBox.Show($"Exception:{ex}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Implementation of ILocalLogger

        public delegate void UpdateLogDisplayDelegate(string[] lines);
        public delegate void LogOneLineDelegate(string txt);
        public delegate void UpdateStatusLineDelegate(string txt);

        public UpdateLogDisplayDelegate DisplayLog { get; set; }
        public UpdateStatusLineDelegate ShowStatus { get; set; }
        public LogOneLineDelegate LogOneLine{ get; set; }

        private List<string> _logLines = new List<string>();
        private int _logMax;
        private bool _logPaused;

        private void InitLogDelegates()
        {
            DisplayLog = lines =>
            {
                teLog.Lines = lines;
                teLog.SelectionStart = teLog.TextLength;
                teLog.ScrollToCaret();
                Application.DoEvents();
            };
            ShowStatus = txt => slStatusMessage.Text = txt;
            LogOneLine = txt => teLog.AppendText($"{txt}\r\n");
        }

        public void Log(Exception ex)
        {
            Log(ex.Message, true);
            Log(ex.StackTrace, false);
        }

        public void Log(string txt)
        {
            Log(txt, true);
        }
        public void Log(string txt, bool showOnStatusLine)
        {
            try
            {
                _logLines.Add($"{DateTime.Now:T} | {txt}");
                if (showOnStatusLine)
                    Invoke(ShowStatus, _logLines[_logLines.Count - 1]);
                if (_logMax > 10 && _logLines.Count > _logMax)
                {
                    int furst = _logLines.Count / 2;
                    _logLines = new List<string>(_logLines.GetRange(furst, _logLines.Count - furst));
                }

                if (!_logPaused)
                    Invoke(LogOneLine, _logLines[_logLines.Count-1]);
            }
            catch (Exception) { }

        }
        private void pbPauseResume_Click(object sender, EventArgs e)
        {
            _logPaused = !_logPaused;
            pbPauseResume.Text = _logPaused ? "Resume" : "Pause";
            if (!_logPaused)
                Invoke(DisplayLog, new object[] { _logLines.ToArray() });
        }

        private void pbClear_Click(object sender, EventArgs e)
        {
            _logLines.Clear();
            Invoke(DisplayLog, new object[] { _logLines.ToArray() });
            Log("log cleared");
        }

        private void nudLogLimit_ValueChanged(object sender, EventArgs e)
        {
            _logMax = (int) nudLogLimit.Value;
        }

        #endregion

        #region privateMethods
        private void LoadGedData()
        {
            Log("Loading file...");
            Application.DoEvents();
            _rrm.Init();
            Log($"Loaded file {_rrm.Settings.GedcomFile}");
            Application.DoEvents();

            bsIndi.DataSource = _rrm.Individuals;
            bsIndi.Sort = nameof(GedcomIndividual.Name);
            bsIndi.ResetBindings(false);
            dgvStartPerson.Focus();
            int toSelect = 0;
            if (_rrm.Settings.GedcomFile.Equals(_rrm.Settings.LastPersonFile))
            {
                string lpid = _rrm.Settings.LastPersonId;
                if (!string.IsNullOrEmpty(lpid))
                {
                    var tgt = _rrm.Individuals?.FirstOrDefault(ind => ind?.IndividualView?.Id?.Equals(lpid)??false);
                    if (tgt != null)
                        toSelect = _rrm.Individuals.IndexOf(tgt);
                }
            }

            dgvStartPerson.FirstDisplayedScrollingRowIndex = toSelect;
            dgvStartPerson.Rows[toSelect].Selected = true;
            pbGo.Enabled = miObfuscate.Enabled = true;
        }
        #endregion

        #region FormDataErrorHandlers
        private int _dataerrortries;
        private List<ReportEntry> _ancestorChoices;
        private GedcomObfuscator _obfu;

        private void dgTitleCleaners_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                if (_dataerrortries++ > 256)
                {
                    e.Cancel = true;
                    return;
                } 
                _rrm.Settings.InitTextCleanerChoices();
                var cbc = colTitleRewriteContext;
                cbc.DataSource = _rrm.Settings.TextCleanerChoices;
                cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
                cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);
            }
        }

        private void dgStyles_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                if (_dataerrortries++ > 256)
                {
                    e.Cancel = true;
                    return;
                }
                _rrm.Settings.InitStyleSlotChoices();
                var cbc = colStyleRole;
                cbc.DataSource = _rrm.Settings.StyleSlotChoices;
                cbc.DisplayMember = nameof(NamedValue<CitationPart>.Name);
                cbc.ValueMember = nameof(NamedValue<CitationPart>.Value);
            }
        }
        #endregion
    }
}
