using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlBase
{

    public interface IXml
    {
        string File { get; set; }
        void Save();
        void Save(string file);
        void Save(List<string> prefixLines);
        void Save(string file, List<string> prefixLines);
    }

}
