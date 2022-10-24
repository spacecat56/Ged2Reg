// frmAgreement.cs
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
using System.Windows.Forms;

namespace Ged2Reg
{
    public partial class frmAgreement : Form
    {
        public Agreement TheAgreement { get; private set; }

        public frmAgreement()
        {
            InitializeComponent();
        }

        public frmAgreement Init(Agreement a)
        {
            TheAgreement = a;
            lbUser.Text = TheAgreement.AgreedUser;
            teAgreement.Text = TheAgreement.AgreedEul;
            if (TheAgreement.Status == StateOfPlay.Authorship)
            {
                pbCancel.Visible = false;
                pbAgree.Text = "Next";
                Text = $"Agreed on {TheAgreement.AgreedOn:d}";
            }

            return this;
        }

        private void pbAgree_Click(object sender, EventArgs e)
        {
            try
            {
                switch (TheAgreement.Status)
                {
                    case StateOfPlay.None:
                        TheAgreement.Status = StateOfPlay.EUL;
                        teAgreement.Text = TheAgreement.AgreedAuthorship;
                        break;
                    case StateOfPlay.EUL:
                        TheAgreement.Status = StateOfPlay.Authorship;
                        TheAgreement.AgreedOn = DateTime.Now;
                        DialogResult = DialogResult.OK;
                        Close();
                        break;
                    case StateOfPlay.Authorship:
                        switch (pbAgree.Text)
                        {
                            case "Next":
                                teAgreement.Text = TheAgreement.AgreedAuthorship;
                                pbAgree.Text = "Ok";
                                break;
                            case "Ok":
                                DialogResult = DialogResult.OK;
                                Close();
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:{ex}");
            }

        }
    }
}
