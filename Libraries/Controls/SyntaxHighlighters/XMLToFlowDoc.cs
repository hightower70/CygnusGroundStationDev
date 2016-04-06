using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

namespace CygnusControls
{
	public class XMLToFlowDoc
	{
		public FlowDocument ConvertDocument(string in_filename)
		{
			// create flowdocument class
			FlowDocument document = new FlowDocument();
			document.Style = (Style)Application.Current.FindResource("XMLDocumentStyle"); ;

			// cache styles
			Style xml_keyword_style = (Style)Application.Current.FindResource("XMLKeywordStyle");
			Style xml_comment_style = (Style)Application.Current.FindResource("XMLCommentStyle");
			Style xml_text_style = (Style)Application.Current.FindResource("XMLTextStyle");
			Style xml_attribute_style = (Style)Application.Current.FindResource("XMLAttributeStyle");
			Style xml_delimiter_style = (Style)Application.Current.FindResource("XMLDelimiterStyle");

			Paragraph paragraph = new Paragraph();
			paragraph.Margin = new Thickness(0);

			// load config file from resource
			using (Stream file_stream = new FileStream(in_filename, FileMode.Open))
			{
				XmlTextReader reader = new XmlTextReader(file_stream);
				while (reader.Read())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.Element: // The node is an element.
							{
								AddSpanToParagraph("<", xml_delimiter_style, paragraph);
								AddSpanToParagraph(reader.Name, xml_keyword_style, paragraph);
								AddSpanToParagraph(">", xml_delimiter_style, paragraph);
							}
							break;

						case XmlNodeType.Text: //Display the text in each element.
							{
								AddSpanToParagraph(reader.Value, xml_text_style, paragraph);
							}
							break;

						case XmlNodeType.EndElement: //Display the end of the element.
							AddSpanToParagraph("</", xml_delimiter_style, paragraph);
							AddSpanToParagraph(reader.Name, xml_keyword_style, paragraph);
							AddSpanToParagraph(">", xml_delimiter_style, paragraph);
							break;

						case XmlNodeType.Comment: // Display comment
							AddSpanToParagraph("<!-- ", xml_delimiter_style, paragraph);
							AddSpanToParagraph(reader.Value, xml_comment_style, paragraph);
							AddSpanToParagraph(" -->", xml_delimiter_style, paragraph);
							break;

						case XmlNodeType.Whitespace:  // Add whitespace
							{
								int spaces = 0;
								int i;

								i = 0;
								while (i < reader.Value.Length)
								{
									switch (reader.Value[i])
									{
										case ' ':
											spaces++;
											i++;
											break;

										case '\r':
											AddSpacesToParagraph(spaces, xml_delimiter_style, paragraph);
											spaces = 0;

											i += 2;

											paragraph.Inlines.Add(new LineBreak());
											break;

										default:
											i++;
											break;
									}
								}

								AddSpacesToParagraph(spaces, xml_delimiter_style, paragraph);
							}
							break;

						case XmlNodeType.XmlDeclaration:  // Display declaration
							AddSpanToParagraph("<?", xml_delimiter_style, paragraph);
							AddSpanToParagraph(reader.Name + " ", xml_keyword_style, paragraph);
							AddSpanToParagraph(reader.Value, xml_attribute_style, paragraph);
							AddSpanToParagraph("?>", xml_delimiter_style, paragraph);
							break;
					}
				}
				reader.Close();
			}

			document.Blocks.Add(paragraph);

			return document;
		}

		public static void CopyDataToClipboard(FlowDocument flowDoc)
		{

			TextRange range = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
			Clipboard.SetData(DataFormats.Text, range.Text);
		}

		private void AddSpanToParagraph(string in_text, Style in_style, Paragraph in_paragraph)
		{
			Span span = new Span();

			span.Style = in_style;
			span.Inlines.Add(in_text);

			in_paragraph.Inlines.Add(span);
		}

		private void AddSpacesToParagraph(int in_space_count, Style in_style, Paragraph in_paragraph)
		{
			if (in_space_count <= 0)
				return;

			string space_text = new string(' ', in_space_count);

			Span span = new Span();

			span.Style = in_style;
			span.Inlines.Add(space_text);

			in_paragraph.Inlines.Add(span);
		}

	}
}
