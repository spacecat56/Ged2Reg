﻿// frmSettingsSets.cs
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommonClassesLib;
using Ged2Reg.Model;

namespace Ged2Reg
{
    public partial class frmSettingsSets : Form
    {
        private G2RSettings _defaultValues;

        private ListOfNamedSettingSets _setSelect;
        private ListOfNamedSettingSets _setModelOn;
        public G2RSettings SelectedSet { get; set; }
 
        public ListOfSettingsSets SettingSets { get; set; }

        private bool _nameEdited;
        public frmSettingsSets()
        {
            InitializeComponent();
        }

        public frmSettingsSets Init()
        {
            _defaultValues = new G2RSettings() {SetName = "(default values)"}.Defaults().Init();
            _setSelect = new ListOfNamedSettingSets();
            _setModelOn = new ListOfNamedSettingSets();
            _setModelOn.Add(new NamedValue<G2RSettings>(_defaultValues.SetName, _defaultValues));
            int current = 0;
            foreach (G2RSettings set in SettingSets)
            {
                if (set == SelectedSet)
                    current = _setSelect.Count; // index = Count - 1
                AddToLists(set);
            }

            bsBasedOnSet.DataSource = _setModelOn;
            bsSelectedSet.DataSource = _setSelect;
            RefreshBindings();
            cbChooseSet.SelectedIndex = current;

            teNewName.TextChanged += teNewName_TextChanged;
            
            return this;
        }

        private void RefreshBindings()
        {
            bsBasedOnSet.ResetBindings(false);
            bsSelectedSet.ResetBindings(false);
        }

        private void AddToLists(G2RSettings set, bool addToCollection = false)
        {
            _setSelect.Add(new NamedValue<G2RSettings>(set.SetName, set));
            _setModelOn.Add(new NamedValue<G2RSettings>(set.SetName, set));
            if (addToCollection)
                SettingSets.Add(set);
        }

        private bool CheckUnfinishedBiz()
        {
            if (!_nameEdited || string.IsNullOrEmpty(teNewName.Text)) 
                return true;
            DialogResult dr = MessageBox.Show(
                "You edited the name but have not applied it.  Are you really finished here?",
                "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dr == DialogResult.Yes;
        }

        private void pbOk_Click(object sender, EventArgs e)
        {
            if (!CheckUnfinishedBiz())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            if (!CheckUnfinishedBiz())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public class ListOfNamedSettingSets : SortableBindingList<NamedValue<G2RSettings>> { }

        private void listOfNamedSettingSetsBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void teNewName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _nameEdited = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbDelete_Click(object sender, EventArgs e)
        {
            
            try
            {
                if (!ValidateNoUndo("delete")) return;
                G2RSettings victim = cbChooseSet.SelectedValue as G2RSettings;
                if (victim==null) //huh?
                    return;
                SettingSets.Remove(victim);
                _setModelOn.Remove(_setModelOn.FirstOrDefault(nv => nv.Name == victim.SetName));
                _setSelect.Remove(_setSelect.FirstOrDefault(nv => nv.Name == victim.SetName));
                RefreshBindings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateNewName()) return;

                G2RSettings newSettings = null;
                if (cbSelectCopyFrom.SelectedIndex == 0)
                {
                    newSettings = new G2RSettings(){SetName = teNewName.Text}.Defaults().Init();
                }
                else
                {
                    G2RSettings src = cbSelectCopyFrom.SelectedValue as G2RSettings;
                    string tfn = Path.GetTempFileName();
                    src.Save(tfn);
                    newSettings = G2RSettings.Load(tfn);
                    newSettings.SetName = teNewName.Text;
                    File.Delete(tfn);
                }

                Apply(newSettings);
                cbChooseSet.SelectedIndex = _setSelect.Count - 1;
                _nameEdited = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private bool ValidateNewName()
        {
            if (string.IsNullOrEmpty(teNewName.Text) || _setSelect.FirstOrDefault(s => s.Name == teNewName.Text) != null)
            {
                MessageBox.Show($"Must provide a unique New Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }

            return true;
        }

        private bool ValidateNoUndo(string what)
        {
            if (ListOfSettingsSets.DefaultSetName.Equals((cbChooseSet.SelectedValue as G2RSettings)?.SetName))
            {
                MessageBox.Show($"Cannot {what} the selected set", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (MessageBox.Show($"'Cancel' later will NOT undo this {what} action. Proceed?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                return true;

            return false;
        }

        private void Apply(G2RSettings ns)
        {
            if (ns == null) return;
            AddToLists(ns, true);
        }

        private void cbChooseSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSet = cbChooseSet.SelectedValue as G2RSettings;
        }

        private void pbRename_Click(object sender, EventArgs e)
        {
            if (!ValidateNewName()) return;
            if (!ValidateNoUndo("rename")) return;

            SelectedSet.SetName = teNewName.Text;
            _setSelect.First(s => s.Value == SelectedSet).Name = teNewName.Text;
            _setModelOn.First(s => s.Value == SelectedSet).Name = teNewName.Text;
            RefreshBindings();
            _nameEdited = false;
        }
    }
}
