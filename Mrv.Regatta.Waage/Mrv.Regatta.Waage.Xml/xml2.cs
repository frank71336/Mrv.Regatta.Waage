using System.Xml.Serialization;
using System.IO;
using XmlBase;

// Erweiterung der automatisch erstellten Klasse "Xml" um Standard-Funktionen (Laden, Speichern, ...)

namespace Mrv.Regatta.Waage.Xml
{
    public partial class Messung : XmlBase.XmlBase, IXml
    {
    }

}
