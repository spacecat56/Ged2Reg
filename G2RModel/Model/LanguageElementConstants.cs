// LanguageElementConstants.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ged2Reg.Model
{
    public class LanguageElementConstants
    {
        public static HashSet<string> CommonPrepositions = new HashSet<string>() 
        {
            "aboard",
            "about",
            "above",
            "across",
            "after",
            "against",
            "along",
            "amid",
            "among",
            "anti",
            "around",
            "as",
            "at",
            "before",
            "behind",
            "below",
            "beneath",
            "beside",
            "besides",
            "between",
            "beyond",
            "but",
            "by",
            "concerning",
            "considering",
            "despite",
            "down",
            "during",
            "except",
            "excepting",
            "excluding",
            "following",
            "for",
            "from",
            "in",
            "inside",
            "into",
            "like",
            "minus",
            "near",
            "of",
            "off",
            "on",
            "onto",
            "opposite",
            "outside",
            "over",
            "past",
            "per",
            "plus",
            "regarding",
            "round",
            "save",
            "since",
            "than",
            "through",
            "to",
            "toward",
            "towards",
            "under",
            "underneath",
            "unlike",
            "until",
            "up",
            "upon",
            "versus",
            "via",
            "with",
            "within",
            "without",
        };

        public static HashSet<string> Determiners = new HashSet<string>()
        {
            "a",
            "an ",
            "the",
            "this",
            "that",
            "these",
            "those",
            "my",
            "your",
            "his",
            "her",
            "its",
            "our",
            "their",
            "all",
            "every",
            "most",
            "many",
            "much",
            "some",
            "few",
            "little",
            "any",
            "no",
            "whose",
            "what",
            "which",

        };

        public static HashSet<string> Pronouns = new HashSet<string>()
        {
            "all",
            "another",
            "any",
            "anybody",
            "anyone",
            "anything",
            "as",
            "aught",
            "both",
            "each",
            "each other",
            "either",
            "enough",
            "everybody",
            "everyone",
            "everything",
            "few",
            "he",
            "her",
            "hers",
            "herself",
            "him",
            "himself",
            "his",
            "I",
            "idem",
            "it",
            "its",
            "itself",
            "many",
            "me",
            "mine",
            "most",
            "my",
            "myself",
            "naught",
            "neither",
            "no one",
            "nobody",
            "none",
            "nothing",
            "nought",
            "one",
            "one another",
            "other",
            "others",
            "ought",
            "our",
            "ours",
            "ourself",
            "ourselves",
            "several",
            "she",
            "some",
            "somebody",
            "someone",
            "something",
            "somewhat",
            "such",
            "suchlike",
            "that",
            "thee",
            "their",
            "theirs",
            "theirself",
            "theirselves",
            "them",
            "themself",
            "themselves",
            "there",
            "these",
            "they",
            "thine",
            "this",
            "those",
            "thou",
            "thy",
            "thyself",
            "us",
            "we",
            "what",
            "whatever",
            "whatnot",
            "whatsoever",
            "whence",
            "where",
            "whereby",
            "wherefrom",
            "wherein",
            "whereinto",
            "whereof",
            "whereon",
            "wherever",
            "wheresoever",
            "whereto",
            "whereunto",
            "wherewith",
            "wherewithal",
            "whether",
            "which",
            "whichever",
            "whichsoever",
            "who",
            "whoever",
            "whom",
            "whomever",
            "whomso",
            "whomsoever",
            "whose",
            "whosever",
            "whosesoever",
            "whoso",
            "whosoever",
            "ye",
            "yon",
            "yonder",
            "you",
            "your",
            "yours",
            "yourself",
            "yourselves",

        };
        public static HashSet<string> Abbreviations = new HashSet<string>()
        {
            "a.", // about, age, acre, ante, aunt
            "a.a.r.", // against all risks
            "ab.", // about; abbey
            "abbr.", // abbreviation
            "abd.", // abdicated
            "Abp.", // Archbishop
            "abr.", // abridged; abridgment
            "abs.", // or abstr. - abstract
            "abt.", // about
            "a.c.", // attested copy; account current
            "acad.", // academy
            "acc.", // according to; account; accompanied
            "acco.", // account
            "accu.", // accurate
            "ackd.", // acknowledged
            "actg.", // acting
            "adj.", // adjoining; adjutant; adjourned
            "adm.", // admission; admitted
            "admin.", // administration; administrator
            "Admon.", // letters of administration
            "admr.", // administration
            "af.", // or afft. - affidavit
            "aft.", // after
            "a.k.a.", // also known as
            "ald.", // alderman
            "alleg.", // allegiance
            "a.l.s.", // autographed letter signed
            "als.", // alias
            "anc.", // ancestry; ancestor; ancient
            "annot.", // annotated
            "ano.", // another
            "anon.", // anonymous
            "ant.", // antiquary; antonym
            "antiq.", // antiquary; antiquities; antiquity; antiquarian
            "a.o.", // account of
            "app.", // apprentice; aprpoximately; appendix; appointed
            "appr.", // appraisment
            "apprd.", // apprised; appeared
            "approx.", // approximately
            "apptd.", // appointed
            "appx.", // appendix
            "arr.", // arrived
            "ascert.", // ascertain(ed)
            "asgd.", // assigned
            "asr.", // assessor
            "assn.", // or assoc. - association
            "asso.", // associated; associate
            "atty.", // attorney
            "aud.", // auditor
            "ave", // avenue
            "a.w.c.", // admon. (letters of administration) with will and codicil annexed
            "b.", // born; bondsman; banns; book; birth; bachelor; brother
            "ba.", // bachelor; baptized
            "bach.", // or batch. - bachelor
            "bapt.", // baptized; baptism
            "B.B.", // Bail Bond
            "b.d.", // birth date
            "bd.", // bound; buried
            "bec.", // because; became
            "bef.", //"- before
            "bet.", // between
            "b.i.l.", // or Bl - brother-in-law
            "biog.", // biography
            "bish.", // bishop
            "bks.", // books; barracks
            "bl.", // bibliography
            "B.M.", // Bench Mark; British Museum
            "bndsmn.", // bondsman
            "bot.", // bought; bottom
            "b.o.t.p.", // both of this parish
            "bp.", // baptized; birthplace
            "bpl.", // birthplace
            "bpt.", // baptized
            "Br.", // British
            "br.", // or bro - brother
            "B.S.", // in court records, Bill of Sale
            "B.T.", // Bishop's Transcripts
            "bur.", // or bu - buried
            "c.", // cousin; circa; codicil
            "ca.", // circa
            "capt.", // captain; captured; captivity
            "catal.", // catalogue
            "cath.", // cathedral
            "cem.", // cemetery
            "cen.", // census
            "cens.", // census
            "cent.", // century
            "cert.", // certificate
            "cf.", // confer
            "ch.", // child; children; church; chief, chaplain
            "chan.", // chancery
            "chldn.", // or chn. - children
            "chr.", // or chris.- christened
            "cir.", // circa
            "civ.", // civil
            "clk.", // clerk
            "cod.", // codicil
            "co.", // county; company
            "col.", // colony; colonel
            "coll.", // college; collections
            "com.", // commissioner; commander; commentary; committee; common; commoner; communicate, comm. - commissioners
            "comp.", // company
            "confer.", // conferred
            "conject.", // conjecture
            "cont.", // continued
            "contr.", // contract
            "corp.", // corporal
            "couns.", // counsellor
            "cous.", // cousin
            "coven.", // covenant
            "c.r.", // church report
            "crspd.", // correspond; correspondence
            "c.s.", // copy signed
            "csn.", // cousin; cousins
            "ct.", // court; citation; county
            "d.", // died; death; daughter
            "da.", // daughter; day
            "dau.", // daughter
            "daus.", // daughters
            "d'd.", //" - deceased
            "deac.", // deacon
            "dec.", // or dec'd - deceased
            "decis.", // decision
            "degr.", // degree
            "dep.", // deputy; depot
            "dept.", // department
            "desc.", // descendant
            "devis.", // devised
            "dil.", // daughter-in-law
            "dio.", // diocese
            "dis.", // discharge
            "discip.", // discipline
            "dist.", // district
            "div.", // division; divided; divorced; divinity
            "do.", // ditto
            "doc.", // document
            "dom.", // domestic
            "dpl.", // death place
            "dr.", // doctor; dram
            "Dr.", // doctor; dram
            "d.s.", // document signed; died single
            "ds.", // deaths; daughters
            "dsct.", // descendant
            "d.s.p.", // [Latin] descessit sine prole; died without issue
            "d.s.p.m.", // [Latin] descessit sine prole mascula; died without male issue
            "dt.", // date
            "dto.", // ditto
            "dtr.", // daughter
            "dt's.", // delirium tremens
            "dum.", // or d. um. - died unmarried
            "d.y.", // died young
            "E.", // East or eastern
            "easi.", // easily
            "ecux.", // executrix; a female executor
            "E.D.", // Enumeration District
            "ed.", // edited; edition; editor
            "educ.", // education; educated
            "Eng.", // England
            "eno.", // enough
            "ens.", // ensign
            "ensu.", // ensuing
            "est.", // estate;established
            "establ.", // establishment
            "estd.", // estimated
            "etc.", // [Latin] etcetera; and so forth
            "et.", // vir. - and
            "exc.", // except; excellency; excepted; exchange
            "exec.", // or exor. or exr. - executor
            "exox.", // executrix
            "exs.", // executors
            "exx.", // executrix
            "f.", // father; female; folio; feast; feet; farm; following
            "fa.", // father
            "F.A.", // Field Artillery
            "fam.", // family; families
            "F.B.", // Family Bible
            "f.e.", // for example
            "ff.", // following (pages), foster father
            "fidel.", // fidelity
            "fil.", // or f.inl or FL - father-in-law
            "f.m.", // free mulatto
            "f.n.", // free negro
            "foll.", // following; followed
            "freem.", // freeman; freemen
            "ft.", // foot; fort
            "g.", // grand; great
            "G.B.", // Great Britain
            "gch.", // or gcl - grandchildren
            "gdn.", // guardian
            "giv.", // given; giving
            "gm.", // grandmother
            "godf.", // godfather
            "godm.", // godmother
            "gp.", // grandparents
            "gr.", // grand; great; grant; graduate
            "g.r.", // grave record
            "grf.", // grandfather
            "grmo.", // grandmother
            "grs.", // or GS - grandson
            "g.s.", // grave stone
            "h.", // husband; heir; heiress
            "hdgrs.", // headquarters
            "her.", // heraldry
            "hers.", // herself
            "hims.", // himself
            "Hist.", // History
            "hist.", // historian
            "hon.", // honorable
            "honor.", // honorary
            "honora.", // honorably
            "hund.", // hundred
            "hus.", // husband
            "ibid.", // [Latin] ibidem; same (reference)
            "ign.", // ignorant
            "illus.", // illustrated
            "imp.", // importation
            "inc.", // incorporated; incomplete
            "incl.", // included; inclusive
            "Ind.", // Indians
            "IND.S.C.", // Indian Survivors' Certificates
            "inf.", // infant; infantry; informed
            "info.", // information
            "inh.", // inherited
            "inhab.", // inhabitant
            "inq.", // inquiry
            "ins.", // insert
            "inst.", // institute; institution
            "int.", // intentions; interested; interred
            "inv.", // orinvt. - inventory
            "Jr.", // Junior
            "judic.", //" - judicial; judicious
            "junr.", // junior
            "jur.", // [Latin] jurat; certification that a document was written by the person who signed it
            "k.", // killed; king
            "kn.", // known
            "knt.", // or kt. - knight
            "l.", // license; law, or lodger
            "lab.", //" - laborer
            "Lat.", // Latin
            "lb.", // pound
            "ld.", // land
            "ldr.", // leader
            "l.e.", // local elder in a church
            "lib.", // library
            "lic.", // license
            "lieut.", // lieutenant
            "li.", // or liv. - lived; living
            "ll.", // lines
            "lnd.", // land
            "l.p.", // local preacher
            "ltd.", // limited
            "m.", // month; male; mother; married; marriage
            "mag.", // magistrate
            "maj.", // major
            "mak.", // making
            "mat.", // maternal
            "m.bn.", // marriage banns
            "md.", // married
            "mem.", // member; membership; memorials; memoir
            "ment.", // mentioned
            "messrs.", // plural of mister
            "m.h.", // meeting house
            "m.i.", // monument inscription
            "mi.", // mile; miles
            "mil.", //"  - military
            "milit.", //" - military
            "min.", // minister
            "m.o.", // mustered out
            "mo.", // mother; month
            "mos.", // months
            "mors.", // death; corpse
            "mov.", // moved
            "Mr.", // Mister
            "Mrs.", // Mistress
            "ms.", // manuscript
            "mss.", // manuscripts
            "mtg.", // meeting; mortgage
            "mvd.", // moved
            "N.", // Negro; North
            "n.", // nephew; name
            "na.", // naturalized
            "nam.", // named
            "nat.", // [Latin] natus; birth; son; offspring
            "n.d.", // no date
            "N.E.", // New England; North Eeast
            "neph.", // nephew
            "nm.", // name
            "nmed.", // named
            "not.", // noted
            "n.p.", // no place
            "nr.", // none recorded; not recorded; naturalized
            "nunc.", // nuncupative
            "N.W.", // North West
            "n.x.n.", // no christian name
            "o.", // oath, officer
            "O.B.", // Order Book
            "ob.", // obiit
            "obit.", // obituary
            "o.c.", // only child
            "O.E.", // Old England; Old English
            "offic.", // official
            "oft.", // often
            "o.p.", // out of print
            "ord.", // ordained; ordinance; order; ordinary
            "org.", // organization
            "orig.", // origin; original
            "o.t.p.", // of this parish
            "p.", // page; per; populus; parentage; parents; pence, patient
            "p.a.", // power of attorney
            "pam.", // pamphlet
            "par.", // parish; parent; parents
            "pat.", // patent; patented; paternal
            "pchd.", // purchased
            "peo.", // people
            "perh.", // perhaps
            "petitn.", // or petn. - petition
            "petr.", // petitioner
            "ph.", // parish, physician
            "pion.", // pioneer
            "plt.", // plantiff
            "P.M.", // Post Meridiem; afternoon; Post Mortem; after death; Police magistrate
            "P.O.", // Post Office
            "pp.", // pages
            "pr.", // proved; probated, prisoner
            "p.r.", // parish record
            "preced.", // preceding
            "pro.", // probate; proved
            "prob.", // probable; probably
            "prop.", // property
            "propr.", // proprietor(s)
            "provis.", // provision
            "pt.", // point; port; petition; pint
            "ptf.", // plaintiff
            "pub.", // public; published; publisher; publication
            "pvt.", // private
            "pymt.", // payment
            "q.", // [Latin] quarto; oversize book
            "q.y.", // query
            "r.", // rector; rex; rejected; river; road
            "R.", // Range; Rabbi; River; Road
            "rat.", // rated
            "R.C.", // Roman Catholic
            "rcdr.", // recorder
            "rcpt.", // receipt
            "re.", // regarding
            "rec.", // record
            "reg.", // register
            "rel.", // relative
            "reld.", // relieved
            "ren.", // renunciation
            "rep.", // report; representative; reprint; reprinted
            "repl.", // replaced; replacement
            "repud.", // repudiated
            "res.", // research; residence; resides
            "respectiv.", // respectively
            "ret.", // retired
            "Rev.", // Reverend; Revolutionary War
            "rgstr.", // registrar
            "rinq.", // relinquished
            "Rom.", // Roman
            "s.", // son(s)/ soldier; survivor; spinster; successor; shilling
            "scatt.", // scattering; scattered
            "S.E.", // southeast
            "sec.", // second; secretary; section; sector; security
            "serg.", // sergeant
            "serv.", // service; servant
            "sett.", // settlers; settler
            "sev.", // several
            "sh.", // share; ship
            "sin.", // [Latin] sine; without
            "sis.", // sister
            "sn.", // [Latin] sine; without
            "soc.", // society; societies
            "s.p.", // [Latin] sine prole; without offspring
            "s.p.l.", // [Latin] sine prole legitima; without legitimate offspring
            "s.p.m.", // [Latin] sine prole mascula; without male offspring
            "spell.", // spelling; spelled
            "spr.", // sponsor
            "sr.", // senior
            "srnms.", // surnames
            "st.", // saint; street
            "St.", // saint; street
            "sup.", // supply; superior
            "supt.", // or Su - superintendant
            "surg.", // surgeon
            "sw.", // swear; sworn
            "syl.", //" - syllable
            "T.", // Township
            "tak.", // taken
            "terr.", // territory
            "test.", // testament
            "tho.", // though
            "thot.", // thought
            "thro.", // through
            "tn.", // town; township
            "top.", // topographical
            "Tp.", // Township
            "t.p.", // title page
            "t.p.m.", // title page mutilated
            "t.p.w.", // title page wanting
            "tr.", // troop; translated; translation
            "transcr.", // transcribed
            "transl.", // translation
            "treas.", // treasurer
            "twn.", // town
            "twp.", // township
            "ty.", // territory
            "U.K.", // United Kingdom
            "unasgd.", // unassigned
            "unc.", // uncle
            "unit.", // uniting; united
            "unk.", // unknown
            "unm.", // unmarried
            "unorg.", // unorganized
            "v.a.", // [Latin] vixit annos; (s)he lived (a certain number) years
            "var.", // various; variation; variant
            "Vis.", // or Visc. - Viscount; Viscountess
            "vit.", // vital
            "viz.", // [Latin] videlicet; namely
            "V.L.", // Vulgar Latin
            "vols.", // volunteers; volumes
            "v.r.", // vital records
            "vs.", // versus
            "v.s.", // vital statistics
            "w.", // wife; will; west; widow
            "wag.", // wagoner
            "W.B.", // Will Book
            "W.D.", // War Department
            "w.d.", // will dated
            "wd.", // widow; ward
            "wh.", // who; which
            "wit.", // witness
            "wk.", //  week(s)
            "wnt.", // wants
            "W.O.", // Widow's Originals
            "W.O.", // Warrant Officer
            "w.p.", // will probated; will proved
            "W.S.", // Writer to the Signet
            "wtn.", // witness
            "ww.", // widow
            "wwr.", // widower
            "xch.", // - exchange
            "Xn.", // Christian
            "Xnty.", // Christianity
            "Xped.", // Christened
            "Xr.", // Christian
            "Xt.", // Christ
            "Xtian.", // Christian
            "Xty.", // Christianity
            "y.", // year
            "yd.", // graveyard
            "yr.", // year; younger; your
        };

    }
}
