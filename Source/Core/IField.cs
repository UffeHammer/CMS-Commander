using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    public interface IField
    {
        string Name { get; }
        string LanguageTitle { get; }
        string Key { get; }
        string Source { get; }
        string Section { get; }
        string Content { get; set; }
        string Type { get; }
        string SortOrder { get; }
        string TemplateFieldID { get; }
    }
}
