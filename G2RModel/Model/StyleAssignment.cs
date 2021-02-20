using CommonClassesLib;

namespace Ged2Reg.Model
{

    public enum StyleSlots
    {
        Normal,
        MainPerson,
        MainPersonText,
        BodyTextIndent, 
        KidsIntro,
        Kids,
        ChildName,
        KidMoreText,
        Grandkids,
        GrandkidName,
        GenerationNumber,
    }
    public class ListOfStyleAssignments : SortableBindingList<StyleAssignment> { }

    public class StyleAssignment  : AbstractBindableModel
    {
        private StyleSlots _slot;
        private string _styleName;

        public StyleAssignment() { }


        public StyleAssignment(StyleSlots slot)
        {
            _slot = slot;
            _styleName = slot.ToString();
        }

        public StyleSlots Style
        {
            get { return _slot; }
            set { _slot = value; OnPropertyChanged(); }
        }

        public string StyleName
        {
            get { return _styleName; }
            set { _styleName = value; OnPropertyChanged(); }
        }

    }
}