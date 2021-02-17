using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxDoc : IWpdDocument
    {
        private WordprocessingDocument _doc;
        private MainDocumentPart _mainPart;
        private StyleDefinitionsPart _stylesPart;
        private Body _body;
        internal FootnotesPart _footnotesPart;
        private EndnotesPart _endnotesPart;
        private OoxParagraph _para;
        
        private SectionProperties _sectionProperties;

        private bool SkipPageSettings { get; set; } = false;

        public static OoxDoc Create(string path)
        {
            return new OoxDoc().Init(path);
        }


        internal OoxDoc() { }

        private OoxDoc Init(string path)
        {
            bool isNew = !File.Exists(path);
            _doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            if (isNew || _doc.MainDocumentPart == null)
            {
                _mainPart = _doc.AddMainDocumentPart();
                _mainPart.Document = new Document();
                _body = _mainPart.Document.AppendChild(new Body());
                CreateStylePart();
            }
            else
            {
                _mainPart = _doc.MainDocumentPart;
                _body = _mainPart.Document.Body;
                _footnotesPart = _mainPart.GetPartsOfType<FootnotesPart>().FirstOrDefault() ?? CreateFootnotePart();
                _stylesPart = _mainPart.GetPartsOfType<StyleDefinitionsPart>().FirstOrDefault() ?? CreateStylePart();
            }
            
            return this;
        }

        private StyleDefinitionsPart CreateStylePart()
        {
            _stylesPart = _mainPart.AddNewPart<StyleDefinitionsPart>();
            _stylesPart.Styles = new Styles();
            return _stylesPart;
        }

        private FootnotesPart CreateFootnotePart()
        {
            _footnotesPart = _mainPart.AddNewPart<FootnotesPart>();
            _footnotesPart.Footnotes = new Footnotes();
            return _footnotesPart;
        }

        private EndnotesPart CreateEndnotesPart()
        {
            _endnotesPart = _mainPart.AddNewPart<EndnotesPart>();
            _endnotesPart.Endnotes = new Endnotes();
            return _endnotesPart;
        }

        public void Save()
        {
            if (_sectionProperties != null && !SkipPageSettings)
            {
                // final sectPr goes directly in the body
                // any earlier ones go in the final para
                _body.Append(_sectionProperties);
            }

            if (_corePropsState == CorePropsState.Open)
                CloseCoreProps();

            _doc.Save();
        }

        public void ApplyTemplateStyles(Stream stream, bool fonts)
        {
            using (WordprocessingDocument wpd = WordprocessingDocument.Open(stream, false))
            {
                var stylesIn = wpd.MainDocumentPart.StyleDefinitionsPart;
                var stylesOut = _mainPart.StyleDefinitionsPart?.Styles 
                    ?? (_mainPart.StyleDefinitionsPart.Styles = new Styles());

                stylesOut.InnerXml = "";

                foreach (var s in stylesIn.Styles.ChildElements)
                {
                    stylesOut.AppendChild((OpenXmlElement) s.Clone());
                }

                if (fonts)
                {
                    FontTablePart ftpi = wpd.MainDocumentPart.FontTablePart;
                    if (ftpi == null) return;
                    FontTablePart ftpo = _mainPart.FontTablePart ?? _mainPart.AddNewPart<FontTablePart>();
                    ftpo.Fonts = ftpi.Fonts.Clone() as Fonts;
                }
            }
        }

        #region CorePropertiesLashup
        internal enum CorePropsState
        {
            None,
            Open,
            Done
        }

        private CorePropsState _corePropsState;
        XDocument _corePropDoc;
        CoreFilePropertiesPart _corePropPart; // = package.GetPart(new Uri("/docProps/core.xml", UriKind.Relative));

        public bool OpenCoreProps()
        {
            switch (_corePropsState)
            {
                case CorePropsState.None:
                    if (_doc.CoreFilePropertiesPart == null)
                    {
                        _corePropPart = _doc.AddCoreFilePropertiesPart();
                        XDocument xd = XDocument.Parse(
                            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
                            <cp:coreProperties xmlns:cp='http://schemas.openxmlformats.org/package/2006/metadata/core-properties' xmlns:dc='http://purl.org/dc/elements/1.1/' xmlns:dcterms='http://purl.org/dc/terms/' xmlns:dcmitype='http://purl.org/dc/dcmitype/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'> 
                               <dc:title></dc:title>
                               <dc:subject></dc:subject>
                               <dc:creator></dc:creator>
                               <cp:keywords></cp:keywords>
                               <dc:description></dc:description>
                               <cp:lastModifiedBy></cp:lastModifiedBy>
                               <cp:revision>1</cp:revision>
                               <dcterms:created xsi:type='dcterms:W3CDTF'>" + DateTime.UtcNow.ToString("s") + "Z" + @"</dcterms:created>
                               <dcterms:modified xsi:type='dcterms:W3CDTF'>" + DateTime.UtcNow.ToString("s") + "Z" + @"</dcterms:modified>
                            </cp:coreProperties>");
                        _corePropPart.PutXDocument(xd);
                    }

                    _corePropDoc = _corePropPart.GetXDocument();
                    //using (TextReader tr = new StreamReader(_corePropPart.GetStream(FileMode.Open, FileAccess.Read)))
                    //{
                    //    _corePropDoc = XDocument.Load(tr);
                    //}
                    _corePropsState = CorePropsState.Open;
                    return true;
                case CorePropsState.Open:
                    return true;
                case CorePropsState.Done:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool SetCoreProperty(string propertyName, string propertyValue)
        {
            if (_corePropsState != CorePropsState.Open)
                OpenCoreProps();

            if (_corePropsState != CorePropsState.Open || _corePropDoc == null)
                return false;

            string propertyNamespacePrefix = propertyName.Contains(":") ? propertyName.Split(':')[0] : "cp";
            string propertyLocalName = propertyName.Contains(":") ? propertyName.Split(':')[1] : propertyName;

            XElement corePropElement =
                (from propElement in _corePropDoc.Root.Elements()
                 where (propElement.Name.LocalName.Equals(propertyLocalName))
                 select propElement).SingleOrDefault();
            if (corePropElement != null)
            {
                corePropElement.SetValue(propertyValue);
            }
            else
            {
                var propertyNamespace = _corePropDoc.Root.GetNamespaceOfPrefix(propertyNamespacePrefix);
                _corePropDoc.Root.Add(new XElement(XName.Get(propertyLocalName, propertyNamespace.NamespaceName), propertyValue));
            }

            return true;
        }

        public bool CloseCoreProps()
        {
            if (_corePropsState != CorePropsState.Open)
                return false;

            //using (TextWriter tw = new StreamWriter(/*new PackagePartStream*/(_corePropPart.GetStream(FileMode.Create, FileAccess.Write))))
            //{
            //    _corePropDoc.Save(tw);
            //}
            _corePropPart.PutXDocument(_corePropDoc);
            _corePropsState = CorePropsState.Done;
            
            return true;
        }

        #endregion


        public void Apply(WpdPageSettings ps)
        {
            _sectionProperties = new SectionProperties();
            PageMargin pageMargin = new PageMargin()
            {
                Top = (int)ps.MarginTop * 20, 
                Right = (UInt32Value) ps.MarginRight * 20, 
                Bottom = (int)ps.MarginBottom * 20, 
                Left = (UInt32Value) ps.MarginLeft * 20, 
                Header = (UInt32Value)1224U, 
                Footer = (UInt32Value)720U, 
                Gutter = (UInt32Value)0U
            };
            PageSize pz = new PageSize()
            {
                Height = (UInt32Value) (ps.PageHeight * 20), 
                Width = (UInt32Value) (ps.PageWidth * 20),
                Code = 1
            };

            _sectionProperties.Append(pz);
            _sectionProperties.Append(pageMargin);
            _sectionProperties.Append(new PageNumberType() {Start = 1});
            _sectionProperties.Append(new Columns() {Space = "720"});
            _sectionProperties.Append(new TitlePage());
        }

        public void InsertPageBreak()
        {
            if (SkipPageSettings)
                return;

            Paragraph brk = new Paragraph();
            brk.ParagraphProperties = new ParagraphProperties(){SectionProperties = _sectionProperties.Clone() as SectionProperties};
            _body.Append(brk);
        }

        public void ConfigureFootnotes(bool asEndnotes, string[] brackets)
        {
            // no-op in this stack
        }

        public IWpdParagraph InsertParagraph(string text = null)
        {
            _para = new OoxParagraph(_body.AppendChild(new Paragraph()));

            return _para.Append(text);
        }

        public WpdNoteRefField BuildNoteRef(WpdFootnoteBase fn)
        {
            return new OoxNoteRefField(this);
        }

        public WpdIndexField BuildIndexField()
        {
            return new OoxIndexField(this);
        }

        public WpdIndexEntry BuildIndexEntryField(string indexName, string indexValue)
        {
            return new OoxIndexEntry(this){IndexName = indexName, IndexValue = indexValue};
        }

        public WpdFootnoteBase BuildFootNote(string noteText = null, string[] brackets = null)
        {
            return new OoxFootnote(this, noteText, brackets);
        }

        public WpdFootnoteBase BuildEndNote(string noteText = null, string[] brackets = null)
        {
            return new OoxEndnote(this, noteText, brackets);
        }

        int maxFootnoteId = 0;
        int maxEndnoteId = 0;

        public int MaxFootnoteId()
        {
            return maxFootnoteId++;
        }

        public int MaxEndnoteId()
        {
            return maxEndnoteId++;
        }

        public void Dispose()
        {
            _doc?.Dispose();
            _doc = null;
        }

        public static OoxDoc Load(Stream docstream, bool editable = false)
        {
            OoxDoc rv = new OoxDoc() {_doc = WordprocessingDocument.Open(docstream, editable)};
            return rv;
        }

        public List<WpdStyleInfo> ListStyles()
        {
            var sxDoc = _doc.MainDocumentPart.StyleDefinitionsPart.GetXDocument();
            List<WpdStyleInfo> rvl = new List<WpdStyleInfo>();
            if ((sxDoc?.Root?.IsEmpty ?? true))
                return rvl;
            foreach (XElement styleElement in sxDoc.Root.Elements(W.style))
            {
                rvl.Add(new WpdStyleInfo()
                {
                    Id = (string)styleElement.Attribute(W.styleId),
                    Name = (string)styleElement.Elements(W.name).Attributes(W.val).FirstOrDefault(),
                    StyleType = (string)styleElement.Attribute(W.type)
                });
            }
            return rvl;
        }

        public bool HasNonDefaultEndnotes()
        {
            return _endnotesPart?.Endnotes.LongCount() > 2;
        }
        public bool HasNonDefaultFootnotes()
        {
            return _footnotesPart?.Footnotes.LongCount() > 2;
        }

        public void BreakForIndex()
        {
            InsertPageBreak();
        }

        public HyperlinkRelationship AddHyperlinkRelationship(Uri uri, bool external, OoxHyperlink.LinkLocation inFootnote)
        {
            switch (inFootnote)
            {
                case OoxHyperlink.LinkLocation.Main:
                    return _mainPart.AddHyperlinkRelationship(uri, external);
                case OoxHyperlink.LinkLocation.Footnote:
                    return (_footnotesPart ?? CreateFootnotePart()).AddHyperlinkRelationship(uri, external);
                case OoxHyperlink.LinkLocation.Endnote:
                    return (_endnotesPart ?? CreateEndnotesPart()).AddHyperlinkRelationship(uri, external);
                default:
                    throw new ArgumentOutOfRangeException(nameof(inFootnote), inFootnote, null);
            }
        }

        public void Apply(FootnoteEndnoteType fe)
        {
            if (fe is Footnote)
                (_footnotesPart ?? CreateFootnotePart()).Footnotes.AppendChild(fe);
            else
                (_endnotesPart ?? CreateEndnotesPart()).Endnotes.AppendChild(fe);
        }
    }
}
