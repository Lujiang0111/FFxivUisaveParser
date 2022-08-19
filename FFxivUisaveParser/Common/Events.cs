using Prism.Events;
using System.Xml;

namespace FFxivUisaveParser.Common
{
    internal class FilePathEvent : PubSubEvent<string>
    {
    }

    internal class xmlEvent : PubSubEvent<XmlDocument>
    {
    }
}
