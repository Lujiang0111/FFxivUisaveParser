using FFxivUisaveParser.Common;
using Prism.Events;
using Prism.Mvvm;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace FFxivUisaveParser.ViewModels
{
    public class XmlTreeViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;

        private string text = "";
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        public XmlTreeViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.GetEvent<xmlEvent>().Subscribe(xmlDoc =>
            {
                Text = XmltoStringFrendly(xmlDoc);
            });
        }

        private string XmltoStringFrendly(XmlDocument xmlDoc)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = " ",
                NewLineChars = " \n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                xmlDoc.Save(writer);
            }
            return stringBuilder.ToString();
        }

    }
}
