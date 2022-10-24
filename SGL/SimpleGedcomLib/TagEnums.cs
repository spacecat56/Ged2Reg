// TagEnums.cs
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

namespace SimpleGedcomLib {
    public enum TagCode
    {
        UNK,
        ABBR,
        ADDR,
        ADR1,
        ADR2,
        ADOP,
        AFN,
        AGE,
        AGNC,
        ALIA,
        ANCE,
        ANCI,
        ANUL,
        ASSO,
        AUTH,
        BAPL,
        BAPM,
        BARM,
        BASM,
        BIRT,
        BLES,
        BLOB,
        BURI,
        CALN,
        CAST,
        CAUS,
        CENS,
        CHAN,
        CHAR,
        CHIL,
        CHR,
        CHRA,
        CITY,
        CONC,
        CONF,
        CONL,
        CONT,
        COPR,
        CORP,
        CREM,
        CTRY,
        DATA,
        DATE,
        DEAT,
        DESC,
        DESI,
        DEST,
        DIV,
        DIVF,
        DSCR,
        EDUC,
        EMAIL,
        EMIG,
        ENDL,
        ENGA,
        EVEN,
        FACT,
        FAM,
        FAMC,
        FAMF,
        FAMS,
        FAX,
        FCOM,
        FILE,
        FONE,
        FORM,
        GEDC,
        GIVN,
        GRAD,
        HEAD,
        HUSB,
        IDNO,
        IMMI,
        INDI,
        LANG,
        LATI,
        LEGA,
        LONG,
        MAP,
        MARB,
        MARC,
        MARL,
        MARR,
        MARS,
        MEDI,
        NAME,
        NATI,
        NATU,
        NCHI,
        NICK,
        NMR,
        NOTE,
        NPFX,
        NSFX,
        OBJE,
        OCCU,
        ORDI,
        ORDN,
        PAGE,
        PEDI,
        PHON,
        PLAC,
        POST,
        PROB,
        PROP,
        PUBL,
        QUAY,
        REFN,
        RELA,
        RELI,
        REPO,
        RESI,
        RESN,
        RETI,
        RFN,
        RIN,
        ROLE,
        ROMN,
        SEX,
        SLGC,
        SLGS,
        SOUR,
        SPFX,
        SSN,
        STAE,
        STAT,
        SUBM,
        SUBN,
        SURN,
        TEMP,
        TEXT,
        TIME,
        TITL,
        TRLR,
        TYPE,
        VERS,
        WIFE,
        WWW,
        WILL,
        _DATE,
        _PRIV,
        _TEXT,
        _LINK,
        _FREL,
        _MREL,
        _FOOT,   // FTM edited citation
    }

    public enum TagDescription
    {
        Unknown,
        Abbreviation,
        Address,
        Address1,
        Address2,
        Adoption,
        Afn,
        Age,
        Agency,
        Alias,
        Ancestors,
        Ances_Interest,
        Annulment,
        Associates,
        Author,
        Baptism_Lds,
        Baptism,
        Bar_Mitzvah,
        Bas_Mitzvah,
        Birth,
        Blessing,
        Binary_Object,
        Burial,
        Call_Number,
        Caste,
        Cause,
        Census,
        Change,
        Character,
        Child,
        Christening,
        Adult_Christening,
        City,
        Concatenation,
        Confirmation,
        Confirmation_L,
        Continued,
        Copyright,
        Corporate,
        Cremation,
        Country,
        Data,
        Date,
        Death,
        Descendants,
        Descendant_Int,
        Destination,
        Divorce,
        Divorce_Filed,
        Phy_Description,
        Education,
        Email,
        Emigration,
        Endowment,
        Engagement,
        Event,
        Fact,
        Family,
        Family_Child,
        Family_File,
        Family_Spouse,
        Fax,
        First_Communion,
        File,
        Phonetic,
        Format,
        Gedcom,
        Given_Name,
        Graduation,
        Header,
        Husband,
        Ident_Number,
        Immigration,
        Individual,
        Language,
        Latitude,
        Legatee,
        Longitude,
        Map,
        Marriage_Bann,
        Marr_Contract,
        Marr_License,
        Marriage,
        Marr_Settlement,
        Media,
        Name,
        Nationality,
        Naturalization,
        Children_Count,
        Nickname,
        Marriage_Count,
        Note,
        Name_Prefix,
        Name_Suffix,
        Object,
        Occupation,
        Ordinance,
        Ordination,
        Page,
        Pedigree,
        Phone,
        Place,
        Postal_Code,
        Probate,
        Property,
        Publication,
        Quality_Of_Data,
        Reference,
        Relationship,
        Religion,
        Repository,
        Residence,
        Restriction,
        Retirement,
        Rec_File_Number,
        Rec_Id_Number,
        Role,
        Romanized,
        Sex,
        Sealing_Child,
        Sealing_Spouse,
        Source,
        Surn_Prefix,
        Soc_Sec_Number,
        State,
        Status,
        Submitter,
        Submission,
        Surname,
        Temple,
        Text,
        Time,
        Title,
        Trailer,
        Type,
        Version,
        Wife,
        Web,
        Will,
        MediaComment,
        Private,
        MediaDate,
        URL,
        Relation_To_Father,
        Relation_To_Mother,
        EditedCitation
    }


    public static class TagMapper
    {
        public static TagDescription Map(this TagCode t)
        {
            switch (t)
            {
                case TagCode.ABBR: return TagDescription.Abbreviation;
                case TagCode.ADDR: return TagDescription.Address;
                case TagCode.ADR1: return TagDescription.Address1;
                case TagCode.ADR2: return TagDescription.Address2;
                case TagCode.ADOP: return TagDescription.Adoption;
                case TagCode.AFN: return TagDescription.Afn;
                case TagCode.AGE: return TagDescription.Age;
                case TagCode.AGNC: return TagDescription.Agency;
                case TagCode.ALIA: return TagDescription.Alias;
                case TagCode.ANCE: return TagDescription.Ancestors;
                case TagCode.ANCI: return TagDescription.Ances_Interest;
                case TagCode.ANUL: return TagDescription.Annulment;
                case TagCode.ASSO: return TagDescription.Associates;
                case TagCode.AUTH: return TagDescription.Author;
                case TagCode.BAPL: return TagDescription.Baptism_Lds;
                case TagCode.BAPM: return TagDescription.Baptism;
                case TagCode.BARM: return TagDescription.Bar_Mitzvah;
                case TagCode.BASM: return TagDescription.Bas_Mitzvah;
                case TagCode.BIRT: return TagDescription.Birth;
                case TagCode.BLES: return TagDescription.Blessing;
                case TagCode.BLOB: return TagDescription.Binary_Object;
                case TagCode.BURI: return TagDescription.Burial;
                case TagCode.CALN: return TagDescription.Call_Number;
                case TagCode.CAST: return TagDescription.Caste;
                case TagCode.CAUS: return TagDescription.Cause;
                case TagCode.CENS: return TagDescription.Census;
                case TagCode.CHAN: return TagDescription.Change;
                case TagCode.CHAR: return TagDescription.Character;
                case TagCode.CHIL: return TagDescription.Child;
                case TagCode.CHR: return TagDescription.Christening;
                case TagCode.CHRA: return TagDescription.Adult_Christening;
                case TagCode.CITY: return TagDescription.City;
                case TagCode.CONC: return TagDescription.Concatenation;
                case TagCode.CONF: return TagDescription.Confirmation;
                case TagCode.CONL: return TagDescription.Confirmation_L;
                case TagCode.CONT: return TagDescription.Continued;
                case TagCode.COPR: return TagDescription.Copyright;
                case TagCode.CORP: return TagDescription.Corporate;
                case TagCode.CREM: return TagDescription.Cremation;
                case TagCode.CTRY: return TagDescription.Country;
                case TagCode.DATA: return TagDescription.Data;
                case TagCode.DATE: return TagDescription.Date;
                case TagCode.DEAT: return TagDescription.Death;
                case TagCode.DESC: return TagDescription.Descendants;
                case TagCode.DESI: return TagDescription.Descendant_Int;
                case TagCode.DEST: return TagDescription.Destination;
                case TagCode.DIV: return TagDescription.Divorce;
                case TagCode.DIVF: return TagDescription.Divorce_Filed;
                case TagCode.DSCR: return TagDescription.Phy_Description;
                case TagCode.EDUC: return TagDescription.Education;
                case TagCode.EMAIL: return TagDescription.Email;
                case TagCode.EMIG: return TagDescription.Emigration;
                case TagCode.ENDL: return TagDescription.Endowment;
                case TagCode.ENGA: return TagDescription.Engagement;
                case TagCode.EVEN: return TagDescription.Event;
                case TagCode.FACT: return TagDescription.Fact;
                case TagCode.FAM: return TagDescription.Family;
                case TagCode.FAMC: return TagDescription.Family_Child;
                case TagCode.FAMF: return TagDescription.Family_File;
                case TagCode.FAMS: return TagDescription.Family_Spouse;
                case TagCode.FAX: return TagDescription.Fax;
                case TagCode.FCOM: return TagDescription.First_Communion;
                case TagCode.FILE: return TagDescription.File;
                case TagCode.FONE: return TagDescription.Phonetic;
                case TagCode.FORM: return TagDescription.Format;
                case TagCode.GEDC: return TagDescription.Gedcom;
                case TagCode.GIVN: return TagDescription.Given_Name;
                case TagCode.GRAD: return TagDescription.Graduation;
                case TagCode.HEAD: return TagDescription.Header;
                case TagCode.HUSB: return TagDescription.Husband;
                case TagCode.IDNO: return TagDescription.Ident_Number;
                case TagCode.IMMI: return TagDescription.Immigration;
                case TagCode.INDI: return TagDescription.Individual;
                case TagCode.LANG: return TagDescription.Language;
                case TagCode.LATI: return TagDescription.Latitude;
                case TagCode.LEGA: return TagDescription.Legatee;
                case TagCode.LONG: return TagDescription.Longitude;
                case TagCode.MAP: return TagDescription.Map;
                case TagCode.MARB: return TagDescription.Marriage_Bann;
                case TagCode.MARC: return TagDescription.Marr_Contract;
                case TagCode.MARL: return TagDescription.Marr_License;
                case TagCode.MARR: return TagDescription.Marriage;
                case TagCode.MARS: return TagDescription.Marr_Settlement;
                case TagCode.MEDI: return TagDescription.Media;
                case TagCode.NAME: return TagDescription.Name;
                case TagCode.NATI: return TagDescription.Nationality;
                case TagCode.NATU: return TagDescription.Naturalization;
                case TagCode.NCHI: return TagDescription.Children_Count;
                case TagCode.NICK: return TagDescription.Nickname;
                case TagCode.NMR: return TagDescription.Marriage_Count;
                case TagCode.NOTE: return TagDescription.Note;
                case TagCode.NPFX: return TagDescription.Name_Prefix;
                case TagCode.NSFX: return TagDescription.Name_Suffix;
                case TagCode.OBJE: return TagDescription.Object;
                case TagCode.OCCU: return TagDescription.Occupation;
                case TagCode.ORDI: return TagDescription.Ordinance;
                case TagCode.ORDN: return TagDescription.Ordination;
                case TagCode.PAGE: return TagDescription.Page;
                case TagCode.PEDI: return TagDescription.Pedigree;
                case TagCode.PHON: return TagDescription.Phone;
                case TagCode.PLAC: return TagDescription.Place;
                case TagCode.POST: return TagDescription.Postal_Code;
                case TagCode.PROB: return TagDescription.Probate;
                case TagCode.PROP: return TagDescription.Property;
                case TagCode.PUBL: return TagDescription.Publication;
                case TagCode.QUAY: return TagDescription.Quality_Of_Data;
                case TagCode.REFN: return TagDescription.Reference;
                case TagCode.RELA: return TagDescription.Relationship;
                case TagCode.RELI: return TagDescription.Religion;
                case TagCode.REPO: return TagDescription.Repository;
                case TagCode.RESI: return TagDescription.Residence;
                case TagCode.RESN: return TagDescription.Restriction;
                case TagCode.RETI: return TagDescription.Retirement;
                case TagCode.RFN: return TagDescription.Rec_File_Number;
                case TagCode.RIN: return TagDescription.Rec_Id_Number;
                case TagCode.ROLE: return TagDescription.Role;
                case TagCode.ROMN: return TagDescription.Romanized;
                case TagCode.SEX: return TagDescription.Sex;
                case TagCode.SLGC: return TagDescription.Sealing_Child;
                case TagCode.SLGS: return TagDescription.Sealing_Spouse;
                case TagCode.SOUR: return TagDescription.Source;
                case TagCode.SPFX: return TagDescription.Surn_Prefix;
                case TagCode.SSN: return TagDescription.Soc_Sec_Number;
                case TagCode.STAE: return TagDescription.State;
                case TagCode.STAT: return TagDescription.Status;
                case TagCode.SUBM: return TagDescription.Submitter;
                case TagCode.SUBN: return TagDescription.Submission;
                case TagCode.SURN: return TagDescription.Surname;
                case TagCode.TEMP: return TagDescription.Temple;
                case TagCode.TEXT: return TagDescription.Text;
                case TagCode.TIME: return TagDescription.Time;
                case TagCode.TITL: return TagDescription.Title;
                case TagCode.TRLR: return TagDescription.Trailer;
                case TagCode.TYPE: return TagDescription.Type;
                case TagCode.VERS: return TagDescription.Version;
                case TagCode.WIFE: return TagDescription.Wife;
                case TagCode.WWW: return TagDescription.Web;
                case TagCode.WILL: return TagDescription.Will;
                case TagCode._PRIV: return TagDescription.Private;
                case TagCode._TEXT: return TagDescription.MediaComment;
                case TagCode._DATE: return TagDescription.MediaDate;
                case TagCode._LINK: return TagDescription.URL;
                case TagCode._FREL: return TagDescription.Relation_To_Father;
                case TagCode._MREL: return TagDescription.Relation_To_Mother;
                case TagCode._FOOT: return TagDescription.EditedCitation;
                default:
                    return TagDescription.Unknown;
            }
        }

        public static TagCode Map(this TagDescription t)
        {
            switch (t)
            {
                case TagDescription.Abbreviation: return TagCode.ABBR;
                case TagDescription.Address: return TagCode.ADDR;
                case TagDescription.Address1: return TagCode.ADR1;
                case TagDescription.Address2: return TagCode.ADR2;
                case TagDescription.Adoption: return TagCode.ADOP;
                case TagDescription.Afn: return TagCode.AFN;
                case TagDescription.Age: return TagCode.AGE;
                case TagDescription.Agency: return TagCode.AGNC;
                case TagDescription.Alias: return TagCode.ALIA;
                case TagDescription.Ancestors: return TagCode.ANCE;
                case TagDescription.Ances_Interest: return TagCode.ANCI;
                case TagDescription.Annulment: return TagCode.ANUL;
                case TagDescription.Associates: return TagCode.ASSO;
                case TagDescription.Author: return TagCode.AUTH;
                case TagDescription.Baptism_Lds: return TagCode.BAPL;
                case TagDescription.Baptism: return TagCode.BAPM;
                case TagDescription.Bar_Mitzvah: return TagCode.BARM;
                case TagDescription.Bas_Mitzvah: return TagCode.BASM;
                case TagDescription.Birth: return TagCode.BIRT;
                case TagDescription.Blessing: return TagCode.BLES;
                case TagDescription.Binary_Object: return TagCode.BLOB;
                case TagDescription.Burial: return TagCode.BURI;
                case TagDescription.Call_Number: return TagCode.CALN;
                case TagDescription.Caste: return TagCode.CAST;
                case TagDescription.Cause: return TagCode.CAUS;
                case TagDescription.Census: return TagCode.CENS;
                case TagDescription.Change: return TagCode.CHAN;
                case TagDescription.Character: return TagCode.CHAR;
                case TagDescription.Child: return TagCode.CHIL;
                case TagDescription.Christening: return TagCode.CHR;
                case TagDescription.Adult_Christening: return TagCode.CHRA;
                case TagDescription.City: return TagCode.CITY;
                case TagDescription.Concatenation: return TagCode.CONC;
                case TagDescription.Confirmation: return TagCode.CONF;
                case TagDescription.Confirmation_L: return TagCode.CONL;
                case TagDescription.Continued: return TagCode.CONT;
                case TagDescription.Copyright: return TagCode.COPR;
                case TagDescription.Corporate: return TagCode.CORP;
                case TagDescription.Cremation: return TagCode.CREM;
                case TagDescription.Country: return TagCode.CTRY;
                case TagDescription.Data: return TagCode.DATA;
                case TagDescription.Date: return TagCode.DATE;
                case TagDescription.Death: return TagCode.DEAT;
                case TagDescription.Descendants: return TagCode.DESC;
                case TagDescription.Descendant_Int: return TagCode.DESI;
                case TagDescription.Destination: return TagCode.DEST;
                case TagDescription.Divorce: return TagCode.DIV;
                case TagDescription.Divorce_Filed: return TagCode.DIVF;
                case TagDescription.Phy_Description: return TagCode.DSCR;
                case TagDescription.Education: return TagCode.EDUC;
                case TagDescription.Email: return TagCode.EMAIL;
                case TagDescription.Emigration: return TagCode.EMIG;
                case TagDescription.Endowment: return TagCode.ENDL;
                case TagDescription.Engagement: return TagCode.ENGA;
                case TagDescription.Event: return TagCode.EVEN;
                case TagDescription.Fact: return TagCode.FACT;
                case TagDescription.Family: return TagCode.FAM;
                case TagDescription.Family_Child: return TagCode.FAMC;
                case TagDescription.Family_File: return TagCode.FAMF;
                case TagDescription.Family_Spouse: return TagCode.FAMS;
                case TagDescription.Fax: return TagCode.FAX;
                case TagDescription.First_Communion: return TagCode.FCOM;
                case TagDescription.File: return TagCode.FILE;
                case TagDescription.Phonetic: return TagCode.FONE;
                case TagDescription.Format: return TagCode.FORM;
                case TagDescription.Gedcom: return TagCode.GEDC;
                case TagDescription.Given_Name: return TagCode.GIVN;
                case TagDescription.Graduation: return TagCode.GRAD;
                case TagDescription.Header: return TagCode.HEAD;
                case TagDescription.Husband: return TagCode.HUSB;
                case TagDescription.Ident_Number: return TagCode.IDNO;
                case TagDescription.Immigration: return TagCode.IMMI;
                case TagDescription.Individual: return TagCode.INDI;
                case TagDescription.Language: return TagCode.LANG;
                case TagDescription.Latitude: return TagCode.LATI;
                case TagDescription.Legatee: return TagCode.LEGA;
                case TagDescription.Longitude: return TagCode.LONG;
                case TagDescription.Map: return TagCode.MAP;
                case TagDescription.Marriage_Bann: return TagCode.MARB;
                case TagDescription.Marr_Contract: return TagCode.MARC;
                case TagDescription.Marr_License: return TagCode.MARL;
                case TagDescription.Marriage: return TagCode.MARR;
                case TagDescription.Marr_Settlement: return TagCode.MARS;
                case TagDescription.Media: return TagCode.MEDI;
                case TagDescription.Name: return TagCode.NAME;
                case TagDescription.Nationality: return TagCode.NATI;
                case TagDescription.Naturalization: return TagCode.NATU;
                case TagDescription.Children_Count: return TagCode.NCHI;
                case TagDescription.Nickname: return TagCode.NICK;
                case TagDescription.Marriage_Count: return TagCode.NMR;
                case TagDescription.Note: return TagCode.NOTE;
                case TagDescription.Name_Prefix: return TagCode.NPFX;
                case TagDescription.Name_Suffix: return TagCode.NSFX;
                case TagDescription.Object: return TagCode.OBJE;
                case TagDescription.Occupation: return TagCode.OCCU;
                case TagDescription.Ordinance: return TagCode.ORDI;
                case TagDescription.Ordination: return TagCode.ORDN;
                case TagDescription.Page: return TagCode.PAGE;
                case TagDescription.Pedigree: return TagCode.PEDI;
                case TagDescription.Phone: return TagCode.PHON;
                case TagDescription.Place: return TagCode.PLAC;
                case TagDescription.Postal_Code: return TagCode.POST;
                case TagDescription.Probate: return TagCode.PROB;
                case TagDescription.Property: return TagCode.PROP;
                case TagDescription.Publication: return TagCode.PUBL;
                case TagDescription.Quality_Of_Data: return TagCode.QUAY;
                case TagDescription.Reference: return TagCode.REFN;
                case TagDescription.Relationship: return TagCode.RELA;
                case TagDescription.Religion: return TagCode.RELI;
                case TagDescription.Repository: return TagCode.REPO;
                case TagDescription.Residence: return TagCode.RESI;
                case TagDescription.Restriction: return TagCode.RESN;
                case TagDescription.Retirement: return TagCode.RETI;
                case TagDescription.Rec_File_Number: return TagCode.RFN;
                case TagDescription.Rec_Id_Number: return TagCode.RIN;
                case TagDescription.Role: return TagCode.ROLE;
                case TagDescription.Romanized: return TagCode.ROMN;
                case TagDescription.Sex: return TagCode.SEX;
                case TagDescription.Sealing_Child: return TagCode.SLGC;
                case TagDescription.Sealing_Spouse: return TagCode.SLGS;
                case TagDescription.Source: return TagCode.SOUR;
                case TagDescription.Surn_Prefix: return TagCode.SPFX;
                case TagDescription.Soc_Sec_Number: return TagCode.SSN;
                case TagDescription.State: return TagCode.STAE;
                case TagDescription.Status: return TagCode.STAT;
                case TagDescription.Submitter: return TagCode.SUBM;
                case TagDescription.Submission: return TagCode.SUBN;
                case TagDescription.Surname: return TagCode.SURN;
                case TagDescription.Temple: return TagCode.TEMP;
                case TagDescription.Text: return TagCode.TEXT;
                case TagDescription.Time: return TagCode.TIME;
                case TagDescription.Title: return TagCode.TITL;
                case TagDescription.Trailer: return TagCode.TRLR;
                case TagDescription.Type: return TagCode.TYPE;
                case TagDescription.Version: return TagCode.VERS;
                case TagDescription.Wife: return TagCode.WIFE;
                case TagDescription.Web: return TagCode.WWW;
                case TagDescription.Will: return TagCode.WILL;
                case TagDescription.Private: return TagCode._PRIV;
                case TagDescription.MediaComment: return TagCode._TEXT;
                case TagDescription.MediaDate: return TagCode._DATE;
                case TagDescription.URL: return TagCode._LINK;
                case TagDescription.EditedCitation: return TagCode._FOOT;
                default:
                    return TagCode.UNK;
            }
        }
    }
}