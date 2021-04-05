using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOL
{
    public class OdtBookMark : OdtBodyElement
    { // todo: this is not being used.... probably not needed...
        /*
        <text:bookmark-start text:name="_Ref64000640"/>
	        <text:span>
	        </text:span>
        <text:bookmark-end text:name="_Ref64000640"/>


        */

        private static int _bookmakNbr;
        public static int NextBookmarkNumber => ++_bookmakNbr;

        public static string BookmarkIdFormat = "_bm{0}";

        public string AssignId() => Name = string.Format(BookmarkIdFormat, _id = NextBookmarkNumber);

        public override XElement ContentInsertPoint => null;
        #region Overrides of OdtBodyElement


        #endregion

        public string Name { get; set; }

        internal int _id;
        private XElement _endElement;


        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
            AssignId();
            
            ContentElement = new XElement(XName.Get("bookmark-start", OdtDocument.text));
            ContentElement.SetAttributeValue(XName.Get("name", OdtDocument.text), Name);

            _endElement = new XElement(XName.Get("bookmark-end", OdtDocument.text));
            _endElement.SetAttributeValue(XName.Get("name", OdtDocument.text), Name);

            return this;
        }

        public override void Append(OdtBodyElement newchild)
        {
            (_endElement as XNode).AddBeforeSelf(newchild);
        }

        public override OdtBodyElement ApplyTo(OdtBodyElement parent)
        {
            XNode toWrap = parent.ContentInsertPoint.LastNode;
            if (toWrap == null)
            {
                parent.ContentInsertPoint.Add(ContentElement);
                parent.ContentInsertPoint.Add(_endElement);
                return this;
            }
            toWrap.AddBeforeSelf(ContentElement);
            toWrap.AddAfterSelf(_endElement);
            return this;
        }

        #endregion

        public OdtBookMark Wrap(OdtBodyElement toBeMarked)
        {
            toBeMarked.ContentElement.AddBeforeSelf(ContentElement);
            toBeMarked.ContentElement.AddAfterSelf(_endElement);
            toBeMarked.Bookmark = this;
            return this;
        }
    }
}
