using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonClassesLib;
using G2RModel.Model;
using Ged2Reg.Model;
using DocxAdapterLib;
using OdtAdapterLib;

namespace Ged2Reg
{
    public partial class Form1 : Form, ILocalLogger
    {
 
        private RegisterReportModel _rrm;
        private CitationStrategyChoices _csss = new CitationStrategyChoices();
        private CitationStrategyChoices _csssFill = new CitationStrategyChoices();
        private BaptismOptionChoices _baptismOptionChoices  = new BaptismOptionChoices();
        private AsyncActionDelegates _aad = new AsyncActionDelegates();
        private frmSettingsSets.ListOfNamedSettingSets _settingsSetsBound = new frmSettingsSets.ListOfNamedSettingSets();


        #region formMechanics
        public Form1()
        {
            InitializeComponent();
            InitLogDelegates();
            Application.DoEvents();

            bsSettingsSets.DataSource = _settingsSetsBound;
            bsSettingsSets.ResetBindings(true);

            //bsSettingsSets.

            teSepNi.Height = teSepPi.Height = textBox1.Height; // Winforms can be SO STUPID

            citationStrategyChoicesBindingSource.DataSource = _csss;
            citationStrategyChoicesBindingSource1.DataSource = _csssFill;
            baptismOptionChoicesBindingSource.DataSource = _baptismOptionChoices;

            _rrm = new RegisterReportModel()
            {
                Logger = this, 
                DocFactory = new OoxDocFactory()
            };

            LoadSettingsSets();

            _aad.CancelEnable = ena => { pbCancel.Enabled = ena; };
            _aad.PostStatusReport = (txt) => { Log(txt); };

            teLog.Height = panel1.Height - (teLog.Top + 10);
        }

        private void LoadSettingsSets()
        {
            // load the list of settings sets and start with the default set
            _rrm.SettingsSets = ListOfSettingsSets.Load();
            _rrm.Settings = _rrm.SettingsSets.GetSettings();
            teSettingsSet.Text = _rrm.Settings.SetName;
            BindUiToSettings();
            Log("Settings loaded; DefaultSettings set selected");
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
            tabPage2.Enabled = tabPage3.Enabled =
                tabPage4.Enabled = tabPage5.Enabled = tabPage6.Enabled = tabPage7.Enabled = ena;

            pnToggler.Enabled = pbGo.Enabled = ena;

            loadSettingsToolStripMenuItem.Enabled = defaultSettingsToolStripMenuItem.Enabled =
                manageSettingsToolStripMenuItem.Enabled = ena;

            pbCancel.Enabled = !ena;
        }

        private char? _seeked = null;
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
                this.Cursor = Cursors.WaitCursor;
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
                this.Cursor = Cursors.Default;
            }
        }

        private void pbOpenGedcom_Click(object sender, EventArgs e)
        {
            try
            {
                //UseWaitCursor = true;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                OpenFileDialog ofd = new OpenFileDialog()
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
                this.Cursor = Cursors.Default;
            }
        }

        private bool _debugging = false;
        private async void pbGo_Async(object sender, EventArgs e)
        {
            try
            {
                _aad.CancelRequested = false;
                ToggleUiEnable(false);
                pbCancel.Focus();
                Application.DoEvents();
                PushReluctantBindings();
                Application.DoEvents();
                // clear instances based on editable collections so they will be recreated with current contents
                _rrm.Settings.Reset(); 
                object o = dgvStartPerson.SelectedRows[0]?.DataBoundItem;

                // last chance:
                SetDocFactory();
                
                DateTime start = DateTime.Now;
                if (_debugging)
                    _rrm.Exec(o, _aad);
                else
                    await Task.Run(() => _rrm.Exec(o, _aad));

                // NB only save ONCE; save a second time RELOADS THE EFFING FILE FIRST!
                _rrm.Doc.Save();
                _rrm.Doc.Dispose();
                _rrm.Doc = null;
                TimeSpan elapsed = DateTime.Now.Subtract(start);
                Log($"Report created ({_rrm.Settings.OutFile} in {elapsed:g})");
                Log(_rrm.Reporter.GetStatsSummary(), false);
                Log($"completed processing; see Log for details");
            }
            catch (CanceledByUserException cbu)
            {
                MessageBox.Show($"Canceled");
                Log("Canceled");
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
        private void pbOpenStylesDoc_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog()
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
                SaveFileDialog sfd = new SaveFileDialog()
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
                default:
                    // todo: complain?
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
                _rrm.Settings.DefaultNameIndex();
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
                _rrm.Settings.DefaultPlaceIndex();
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
                    _rrm.Settings = new G2RSettings() { SetName = _rrm.Settings.SetName, GedcomFile = teGedcom.Text, OutFile = teOutFile.Text, TextCleaners = _rrm.Settings.TextCleaners }.Init();
                else
                    _rrm.Settings = new G2RSettings() { SetName = _rrm.Settings.SetName, GedcomFile = teGedcom.Text, OutFile = teOutFile.Text }.Init();
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
                frmSettingsSets fss = new frmSettingsSets(){SettingSets = tempListOfSettingsSets, SelectedSet = _rrm.Settings}.Init();
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
            _rrm.Settings = sel.InitInternals();
            BindUiToSettings();
            bsIndi.DataSource = new ListOfGedcomIndividuals();
            bsIndi.ResetBindings(false);
            pbGo.Enabled = false;
            Log($"Settings set {_rrm.Settings.SetName} is active");
            cbSettingsSet.SelectedValue = sel;
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
        private int _logMax = 0;
        private bool _logPaused = false;

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
            pbGo.Enabled = true;
        }
        #endregion

        #region FormDataErrorHandlers
        private int _dataerrortries = 0;
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

        private void cbSettingsSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var sel = (cbSettingsSet.SelectedItem as NamedValue<G2RSettings>)?.Value;
                if (sel == null || sel == _rrm.Settings)
                    return;
                ApplySettingSetChoice(sel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }
    }
}
