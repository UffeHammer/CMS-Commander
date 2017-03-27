using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    public class Credentials
    {

        private string customDataField;

        private string passwordField;

        private string userNameField;

        /// <remarks/>
        public string CustomData
        {
            get
            {
                return this.customDataField;
            }
            set
            {
                this.customDataField = value;
            }
        }

        /// <remarks/>
        public string Password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }

        /// <remarks/>
        public string UserName
        {
            get
            {
                return this.userNameField;
            }
            set
            {
                this.userNameField = value;
            }
        }
    }

}
