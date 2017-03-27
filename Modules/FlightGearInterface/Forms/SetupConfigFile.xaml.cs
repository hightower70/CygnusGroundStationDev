using CygnusControls;
using System.Windows.Controls;

namespace FlightGearInterface
{
	/// <summary>
	/// Interaction logic for SetupHelp.xaml
	/// </summary>
	public partial class SetupConfigFile : UserControl
	{
		public SetupConfigFile()
		{
			InitializeComponent();

			XMLToFlowDoc converter = new XMLToFlowDoc();

			//TODO: covert it to resource
			fdsvXMLCOnfig.Document = converter.ConvertDocumentForomFile(@"d:\Projects\CygnusGroundStation\Devel\devel\XMLHighlighter\XML Highlighter\cygnusuav.xml");

		}
	}
}
