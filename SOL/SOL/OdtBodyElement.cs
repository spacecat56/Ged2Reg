using System.Xml.Linq;

namespace SOL
{
    public abstract class OdtBodyElement
    {
        public OdtDocument Document { get; set; }
        public OdtPart Part { get; set; }
        public XElement Parent { get; set; }
        public OdtBookMark Bookmark { get; internal set; }
        internal XElement ContentElement { get; set; }
        public virtual XElement ContentInsertPoint => ContentElement;
        //public XElement InsertPointElement { get; set; }
        public abstract OdtBodyElement Build();

        public virtual OdtBodyElement ApplyTo(OdtBodyElement parent)
        {   
            parent.ContentInsertPoint.Add(ContentElement); 
            return this;
        }

        public virtual void Append(OdtBodyElement newchild)
        {
            ContentInsertPoint?.Add(newchild.ContentElement);
        }

    }
}