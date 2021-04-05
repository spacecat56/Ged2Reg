using System;
using System.Windows.Forms;
using SOL;

namespace SimpleTestApp
{
    public partial class Form1 : Form
    {
        private OdtDocument _doc;
        public Form1()
        {
            InitializeComponent();
        }

        private void pbLoad_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog() {InitialDirectory = teWorkingDir.Text};
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                _doc = OdtDocument.Load(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex}");
            }
        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog(){InitialDirectory = teWorkingDir.Text};
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                _doc.Save(sfd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex}");
            }
        }

        private void pbEdit_Click(object sender, EventArgs e)
        {
            try
            {
                //_doc.TheContentPart.Clear();
                OdtParagraph para = _doc.AppendParagraph("1. ", "MainPersonText")
                    .Append("Robert", "MainPerson")
                    .Append("1", "GenerationNumber")
                    .Append(" Jones", "MainPerson")
                    .Append(" was born...");
                var ie1 = new OdtIndexEntry(){DelimitedText = "Ives:Joseph"};
                para.Append(ie1.Build());
                var fn = new OdtFootnote(){Document = _doc}.Build() as OdtFootnote;
                fn.Append(new OdtSpan(){Text = " Make note of this: "}.Build());
                fn.Append(new OdtHyperlink() { Document = _doc, URL = "http://www.ancestry.com" }.Build());
                fn.ApplyTo(para);
                //var bm = new OdtBookMark().Build() as OdtBookMark;
                //bm.Wrap(fn);
                para.Append(" ...and then what happened?");
                OdtBodyElement h = new OdtHyperlink(){Document = _doc, URL = "http://www.google.com"}.Build();
                para.Append(h);
                new OdtSpan() {Document = _doc, Text = " LOOK IT UP!"}.Build().ApplyTo(para);

                _doc.AppendPageBreak();

                para = _doc.AppendParagraph("2. ", "MainPersonText")
                    .Append("Sean", "MainPerson")
                    .Append("1", "GenerationNumber")
                    .Append(" Jones", "MainPerson")
                    .Append(" was born...");
                var ie2 = new OdtIndexEntry() { DelimitedText = "Ives:William" };
                para.Append(ie2.Build());
                var fn2 = new OdtFootnote() { Document = _doc }.Build() as OdtFootnote;
                fn2.Append(new OdtSpan() { Text = " See note: " }.Build());
                fn2.Append(fn.GetNoteRef);
                fn2.Append(new OdtSpan(){Text = "."}.Build());
                fn2.ApplyTo(para);

                _doc.AppendHorizontalLine(OdtDocument.LineStyle.Double, para: para);
                
                para = _doc.AppendParagraph("And so on. The end.");

                
                _doc.AppendPageBreak();
                para = _doc.AppendParagraph("3. ", "MainPersonText").Append("Lisa");
                _doc.AppendPageBreak();
                para = _doc.AppendParagraph("4. ", "MainPersonText").Append("Roger");
                //para = _doc.AppendParagraph();
                _doc.AppendPageBreak();
                _doc.Append(new OdtIndex() {Heading = "Index of Names, yo", Placeholder = "Click here, ho"});

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex}");
            }
        }

        private void pbNew_Click(object sender, EventArgs e)
        {
            try
            {
                _doc = OdtDocument.New();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex}");
            }
        }
    }
}
