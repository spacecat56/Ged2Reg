using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Runtime.InteropServices;
using G2RModel.Model;
using DocxAdapterLib;
using SimpleGedcomLib;
using WpdInterfaceLib;


namespace Ged2Reg.Model
{
    public class RegisterReportModel
    {
        public AsyncActionDelegates ActionDelegates { get; private set; }
        public ListOfSettingsSets SettingsSets { get; set; }
        public G2RSettings Settings { get; set; }
        public IWpdDocument Doc { get; set; }

        public GedcomFile GedcomFile { get; set; }

        public ListOfGedcomIndividuals Individuals { get; set; }

        // these two finders get VERY expensive 
        // when we are using them we will make maps 
        // NB be sure to delete the maps on re-init!
        private Dictionary<IndividualView, GedcomIndividual> _ivgiMap;
        // can't do this because the list is dynamic
        //private Dictionary<FamilyView, GedcomFamily> _fvgiMap;


        public IWpdFactory DocFactory { get; set; }

        //public List<GedcomFamily> AllFamilies { get; set; }


        public ILocalLogger Logger { get; set; }

        public RegisterReporter Reporter { get; private set; }

        public bool Init()
        {
            //
            bool rv = true;
            if (!string.IsNullOrEmpty(Settings.OutFile) && string.IsNullOrEmpty(Path.GetDirectoryName(Settings.OutFile)))
            {
                Settings.OutFile = Path.Combine(Path.GetTempPath(), Settings.OutFile);
            }
            //Doc.Save();
            if (!string.IsNullOrEmpty(Settings.GedcomFile))
                OpenGedcom(Settings.GedcomFile);

            ReportContext.Init(Settings).Model = this;

            _ivgiMap = null;
            //_fvgiMap = null;

            return rv;
        }

        public static List<string[]> ListAvailableStyles(IWpdFactory docFactory, Stream docstream)
        {
            List<string[]> rvl = new List<string[]>();

            // TODO: convert this
            using (IWpdDocument dt = docFactory.Load(docstream))
            {
                foreach (var styleInfo in dt.ListStyles().OrderBy(si => si.Name))
                {
                    rvl.Add(new []{styleInfo.Name??styleInfo.Id, styleInfo.Id, styleInfo.StyleType});
                }
            }
            docstream.Dispose();
            return rvl;
        }

        public static bool ValidateStylesDoc(IWpdFactory docFactory, Stream docstream)
        {
            IWpdDocument dt = null;
            try
            {
                dt = docFactory.Load(docstream);
                return !(dt.HasNonDefaultEndnotes() || dt.HasNonDefaultFootnotes());
            }
            finally
            {
                dt?.Dispose(); 
                docstream?.Dispose();
            }
        }

        public bool CheckCancel()
        {
            return ActionDelegates?.CancelRequested ?? false;
        }

        public void PostProgress(string s)
        {
            ActionDelegates?.PostStatusReport(s);
        }

        public bool Exec(object oRoot, AsyncActionDelegates aad = null, bool testing = false)
        {
            DateTime started = DateTime.Now;
            GedcomIndividual root = oRoot as GedcomIndividual;
            if (root == null) 
                throw new ArgumentException("Must select a person with some descendant(s)");

            Settings.LastPersonId = root.IndividualView.Id;
            Settings.LastPersonFile = Settings.GedcomFile;
            Settings.BracketArray = Settings.Brackets ? new[] { "[", "]" } : null;

            ActionDelegates = aad;

            ActionDelegates?.PostStatusReport($"begin processing; starting person: {root.NameForward}");

            // reset the output
            Doc = DocFactory.Create(Settings.OutFile);
            //Doc.ApplyTemplate(Settings.GetStylesStream(), false);
            // todo: this has to be factory-type-aware; or otherwise reconciled
            Doc.ApplyTemplateStyles(Settings.GetStylesStream(), true);

            Doc.ConfigureFootnotes(Settings.AsEndnotes, Settings.BracketArray);

            // clear author and title - may be copied from template
            //if (Doc.OpenCoreProps())
            {
                 Doc.SetCoreProperty("dc:title", Settings.Title??"");
                 Doc.SetCoreProperty("dc:creator", Settings.Author??"");
                 //Doc.CloseCoreProps();
            }

            const float pointsPerInch = 72f;

            if (Settings.PageH == 0)
                Settings.InitPageMetrics();

            //Doc.PageHeight = Settings.PageH * pointsPerInch;
            //Doc.PageWidth = Settings.PageW * pointsPerInch;
            //Doc.MarginBottom = Settings.MarginB * pointsPerInch;
            //Doc.MarginLeft = Settings.MarginL * pointsPerInch;
            //Doc.MarginRight = Settings.MarginR * pointsPerInch;
            //Doc.MarginTop = Settings.MarginT * pointsPerInch;
            var ps = new WpdPageSettings()
            {
                PageHeight = Settings.PageH * pointsPerInch,
                PageWidth = Settings.PageW * pointsPerInch,
                MarginBottom = Settings.MarginB * pointsPerInch,
                MarginLeft = Settings.MarginL * pointsPerInch,
                MarginRight = Settings.MarginR * pointsPerInch,
                MarginTop = Settings.MarginT * pointsPerInch,
            };
            Doc.Apply(ps);

            // reset state
            _ivgiMap = null;
            //_fvgiMap = null;
            ResetGedcom();

            // trigger rebuild of title cleaner, and place it where it will be used
            Settings.CitationTitleCleaner = null;
            TextCleaner.TitleCleaner = Settings.CitationTitleCleaner;

            // build the tree from the root person
            if (CheckCancel()) throw new CanceledByUserException();
            if (Settings.AncestorsReport)
            {
                PostProgress("building ancestors tree");
                //root.Ascend(false);
                // this seems not really to work
                // root.FindFamilies(true, true);
                // so just do one step, and go stepwise in the numbering
                root.FindFamilies(true);
                if (root.ChildhoodFamily == null)
                    throw new ArgumentException("Must select a person with some ancestor(s)");
            }
            else
            {
                PostProgress("building descendants tree");
                root.Expand();
                if (!root.HasDescendants)
                    throw new ArgumentException("Must select a person with some descendant(s)");
            }

            // process all spouses, especially to evaluate whether they and their parents are maybe living
            //BuildFamiliesList();
            List<GedcomIndividual> allSpouses = new List<GedcomIndividual>();
            foreach (GedcomIndividual individual in Individuals)
            {
                if (individual.Spouses == null) continue;
                allSpouses.AddRange(individual.Spouses);
            }
            foreach (GedcomIndividual spouse in allSpouses)
            {
                spouse.FindFamilies(true);
            }
            
            //if (Settings.ObscureLiving)
            //    PostProgress($"obscure names of (possibly) living persons");
            // now everybody we care about for this report, is linked to the family
            // so we can evaluate the "maybe living" status of everyone
            //foreach (GedcomIndividual individual in Individuals)
            //{
            //    if (CheckCancel()) throw new CanceledByUserException();
            //    if (Settings.ObscureLiving)
            //    {
            //        individual.EvalLivingStatus();
            //    }
            //    else
            //    {
            //        individual.PresumedDeceased = true;
            //    }
            //}

            //BuildFamiliesList();

            if (CheckCancel()) throw new CanceledByUserException();
            PostProgress($"initializing report model");
            Reporter = new RegisterReporter(){Model = this}.Init(root, DateTime.Now.Subtract(started));

            if (CheckCancel()) throw new CanceledByUserException();
            Reporter.ProcessLivingStatus();

            if (CheckCancel()) throw new CanceledByUserException();
            PostProgress($"processing report");
            Reporter.Exec(Doc);

            //PostProgress(Reporter.GetStatsSummary());

            return true;
        }

        public void ResetGedcom()
        {
            GedcomFamily.ClearFamilies(); // .AllFamilies.Clear();
            foreach (GedcomIndividual individual in Individuals ?? new ListOfGedcomIndividuals())
            {
                individual.Reset();
            }
        }

        public void OpenGedcom(string fn)
        {
            Settings.GedcomFile = fn ?? Settings.GedcomFile;
            GedcomFile = new GedcomFile().Parse(fn);

            Individuals = new ListOfGedcomIndividuals(GedcomFile.IndividualViews);

        }

        private Dictionary<IndividualView, GedcomIndividual> GetIndiMap()
        {
            if (_ivgiMap != null) return _ivgiMap;

            _ivgiMap = Individuals.ToDictionary(i => i.IndividualView, i => i);

            return _ivgiMap;
        }

        //private Dictionary<FamilyView, GedcomFamily> GetFamMap()
        //{
        //    if (_fvgiMap != null) return _fvgiMap;

        //    _fvgiMap = GedcomFamily.AllFamilies.ToDictionary(f => f.FamilyView, f => f);

        //    return _fvgiMap;
        //}

        public GedcomIndividual FindIndividual(IndividualView iv)
        {
            if (iv == null) return null; // why does this happen?
            //return Individuals.FirstOrDefault(v => v.IndividualView.Equals(iv));
            GetIndiMap().TryGetValue(iv, out GedcomIndividual rv);
            return rv;
        }


        public GedcomFamily FindAsChildInFamily(GedcomIndividual indi)
        {
            if (indi == null) return null;

            // todo: optimize this
            // bug? there could be more than one result, eh?
            FamilyView fv = GedcomFile.FamilyViews.Find(f => f.Chiildren.Contains(indi.IndividualView));
            if (fv == null) return null;

            // GedcomFamily rvf = GedcomFamily.AllFamilies.FirstOrDefault(f => f.FamilyView.Equals(fv));

            GedcomFamily.GetFamMap().TryGetValue(fv, out GedcomFamily rvf);
            rvf ??= GedcomFamily.Add(new GedcomFamily(fv));

            return rvf;
        }

        public void Activate(G2RSettings settings)
        {
            Settings = settings.InitInternals();
            foreach (G2RSettings settingsSet in SettingsSets)
            {
                settingsSet.LastActive = false;
            }

            Settings.LastActive = true;
        }
    }
}
