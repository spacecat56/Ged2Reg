// OdtBodyElement.cs
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