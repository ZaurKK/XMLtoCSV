using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLtoCSVconvertor
{
    public class pers
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public class dataroot
        {

            private datarootPACIENT[] pACIENTField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("PERS")]
            public datarootPACIENT[] PACIENT
            {
                get
                {
                    return this.pACIENTField;
                }
                set
                {
                    this.pACIENTField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class datarootPACIENT
        {

            private string iD_PACField;

            private string fAMField;

            private string iMField;

            private string oTField;

            private string dRField;

            /// <remarks/>
            public string ID_PAC
            {
                get
                {
                    return this.iD_PACField;
                }
                set
                {
                    this.iD_PACField = value;
                }
            }

            /// <remarks/>
            public string FAM
            {
                get
                {
                    return this.fAMField;
                }
                set
                {
                    this.fAMField = value;
                }
            }

            /// <remarks/>
            public string IM
            {
                get
                {
                    return this.iMField;
                }
                set
                {
                    this.iMField = value;
                }
            }

            /// <remarks/>
            public string OT
            {
                get
                {
                    return this.oTField;
                }
                set
                {
                    this.oTField = value;
                }
            }

            /// <remarks/>
            public string DR
            {
                get
                {
                    return this.dRField;
                }
                set
                {
                    this.dRField = value;
                }
            }
        }


    }
}
