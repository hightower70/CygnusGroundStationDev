///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Converts XML to Flowdoc with syntax highlighting
///////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

namespace CygnusControls
{
	public class XMLToFlowDoc
	{
		/// <summary>
		/// Converts the given 
		/// </summary>
		/// <param name="in_filename"></param>
		/// <returns></returns>
		public FlowDocument ConvertDocumentForomFile(string in_filename)
		{
			using (Stream file_stream = new FileStream(in_filename, FileMode.Open))
			{
				return ConvertDocumentFromStream(file_stream);
			}
		}

		/// <summary>
		/// Converts XML from resource to FlowDoc
		/// </summary>
		/// <param name="in_resource_name">Name of the resource</param>
		/// <returns></returns>
		public FlowDocument ConvertDocumentFromResource(string in_resource_name)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			using (Stream file_stream = assembly.GetManifestResourceStream(in_resource_name))
			{
				return ConvertDocumentFromStream(file_stream);
			}
		}

		/// <summary>
		/// Converts XML file stream to flowdoc
		/// </summary>
		/// <param name="in_file_stream">File stream to convert</param>
		/// <returns>Converted and syntax highlighted XML document</returns>
		public FlowDocument ConvertDocumentFromStream(Stream in_file_stream)
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
			XmlTextReader reader = new XmlTextReader(in_file_stream);
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

			// cfinishing document
			document.Blocks.Add(paragraph);

			// close text readres
			reader.Close();

			return document;
		}

		/// <summary>
		/// Copies FlowDoc ontent to clipboard
		/// </summary>
		/// <param name="in_flow_doc">FlowDoc to copy to the clipboard</param>
		public static void CopyDataToClipboard(FlowDocument in_flow_doc)
		{

			TextRange range = new TextRange(in_flow_doc.ContentStart, in_flow_doc.ContentEnd);
			Clipboard.SetData(DataFormats.Text, range.Text);
		}

		#region · private functions ·

		/// <summary>
		/// Adds SPAN element to a paragraph
		/// </summary>
		/// <param name="in_text"></param>
		/// <param name="in_style"></param>
		/// <param name="in_paragraph"></param>
		private void AddSpanToParagraph(string in_text, Style in_style, Paragraph in_paragraph)
		{
			Span span = new Span();

			span.Style = in_style;
			span.Inlines.Add(in_text);

			in_paragraph.Inlines.Add(span);
		}

		/// <summary>
		/// Adds spaces to a paragraph
		/// </summary>
		/// <param name="in_space_count"></param>
		/// <param name="in_style"></param>
		/// <param name="in_paragraph"></param>
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
		#endregion

	}
}
