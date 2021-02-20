using CommonClassesLib;

namespace Ged2Reg.Model
{
    public enum IndexRole
    {
        Names,
        Places
    }

   public class IndexSettings : AbstractBindableModel
    {
        private string _indexName = "names";
        private string _indexHeading = "Index of Names";
        private bool _enabled = true;
        private int _columns = 2;
        private string _sep = "\t";

        private IndexRole _role;

        public IndexRole Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged();}
        }

        // looks good this way:
        // \e "	" \c "2" \z "1033"


        public string Separator
        {
            get { return _sep; }
            set { _sep = value; }
        }

        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public string IndexHeading
        {
            get { return _indexHeading; }
            set { _indexHeading = value; OnPropertyChanged(); }
        }

        //private string x = "\\e \"\t\" \\c \"2\" \\z \"1033\"";

        public string IndexName
        {
            get { return _indexName; }
            set { _indexName = value; OnPropertyChanged(); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged(); }
        }
    }
}