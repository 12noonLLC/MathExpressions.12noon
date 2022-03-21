using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Shared;

/// <see cref="https://stackoverflow.com/a/48909764/4858"/>
/// <seealso cref="https://stackoverflow.com/questions/343468/richtextbox-wpf-binding/2989277#2989277"/>
/// <seealso cref="https://stackoverflow.com/questions/343468/richtextbox-wpf-binding/2641774#2641774"/>
/// <summary>
/// Add an attached property so a RichTextBox control can bind its content to a DataContext member.
/// The member can be either a FlowDocument or a string of XML representing a FlowDocument.
/// </summary>
/// <seealso cref="https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/how-to-use-flow-content-elements"/>
/// <example>
/// Example of FlowDocument data.
///	<RichTextBox shared:RichTextBoxBinding.DocumentFlow="{Binding ContentFlow}" />
///	
///	FlowDocument ContentFlow = new();
///	Paragraph para = new() { Margin = new Thickness(0), };
///	para.Inlines.Add(new Run("Lorem ipsum dolor."));
///	ContentFlow.Blocks.Add(para);
///	Paragraph para = new() { Margin = new Thickness(0), };
///	para.Inlines.Add(new Run("Lorem "));
///	para.Inlines.Add(new Bold(new Run("ipsum")) { Foreground = Brushes.Gray });
///	para.Inlines.Add(new Run("dolor."));
///	ContentFlow.Blocks.Add(para);
///	Paragraph para = new() { Margin = new Thickness(0), };
///	para.Inlines.Add(new Run("Lorem "));
///	para.Inlines.Add(new Run("ipsum") { Foreground = new SolidColorBrush(Color.Red) });
///	para.Inlines.Add(new Run("Lorem ipsum dolor."));
///	ContentFlow.Blocks.Add(para);
/// </example>
/// <example>
/// Example of XML data (representing FlowDocument).
///	<RichTextBox shared:RichTextBoxBinding.ContentXAML="{Binding ContentXaml}" />
///
///	// <FlowDocument xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///	// 	FontFamily="Segoe UI" FontSize="12" FontStyle="Normal" FontWeight="Normal" FontStretch="Normal"
///	// >
///	// 	<Paragraph>Lorem ipsum dolor.</Paragraph>
///	// 	<Paragraph>Lorem <Bold>ipsum</Bold> dolor.</Paragraph>
///	// 	<Paragraph>Lorem <Span Foreground="#FFFF0000>ipsum</Span> dolor.</Paragraph>
///	// </FlowDocument>
///	XNamespace XmlnsFlowDocument = @"http://schemas.microsoft.com/winfx/2006/xaml/presentation";
///	XDocument xdoc = new(
///		new XElement(nameof(FlowDocument),
///			new XElement(XmlnsFlowDocument + nameof(Paragraph),
///				"Lorem ipsum dolor."
///			),
///			new XElement(XmlnsFlowDocument + nameof(Paragraph),
///				"Lorem ",
///				new XElement(XmlnsFlowDocument + nameof(Bold), "ipsum"),
///				" dolor."
///			)
///		)
///	);
///	xdoc.Element(XmlnsFlowDocument + nameof(FlowDocument))!
///		.Add(new XElement(XmlnsFlowDocument + nameof(Paragraph),
///				"Lorem ",
///				new XElement(XmlnsFlowDocument + nameof(Span),
///					// OR Color.FromArgb(0xFF, 0x80, 0x12, 0x50);
///					new XAttribute(nameof(Foreground), Colors.Red.ToString()),
///					"ipsum"
///				),
///				" dolor."
///			)
///		)
///	);
///	ContentXaml = xdoc.ToString(SaveOptions.DisableFormatting);
/// </example>
public class RichTextBoxBinding
{
	public static bool GetScrollToEnd(DependencyObject obj) => (bool)obj.GetValue(ScrollToEndProperty);
	public static void SetScrollToEnd(DependencyObject obj, bool value) => obj.SetValue(ScrollToEndProperty, value);
	public static readonly DependencyProperty ScrollToEndProperty = DependencyProperty.RegisterAttached(
		"ScrollToEnd",
		typeof(bool),
		typeof(RichTextBoxBinding),
		new PropertyMetadata(defaultValue: false));


	public static FlowDocument GetDocumentFlow(DependencyObject obj) => (FlowDocument)obj.GetValue(DocumentFlowProperty);
	public static void SetDocumentFlow(DependencyObject obj, FlowDocument value) => obj.SetValue(DocumentFlowProperty, value);
	public static readonly DependencyProperty DocumentFlowProperty = DependencyProperty.RegisterAttached(
		"DocumentFlow",
		typeof(FlowDocument),
		typeof(RichTextBoxBinding),
		/// <see cref="https://stackoverflow.com/questions/1168648/why-would-a-dependency-property-implementation-crash-my-application-when-i-provid"/>
		/// <seealso cref="https://social.msdn.microsoft.com/Forums/en-US/2cb12481-ef86-40b7-8333-443598d89933/custom-type-dependency-property-which-has-dependency-properties-itself?forum=wpf"/>
		new FrameworkPropertyMetadata(
			defaultValue: null,	// Cannot specify default value. (See above.)
			FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
			OnDocumentFlowPropertyChanged
		)
	);

	private static void OnDocumentFlowPropertyChanged(DependencyObject /*RichTextBox*/ d, DependencyPropertyChangedEventArgs e)
	{
		RichTextBox richTextBox = (RichTextBox)d;

		/// Assigning a value to RichTextBox.Document raises the TextChanged
		/// event many times, so we remove it while we're in this handler.
		richTextBox.TextChanged -= OnFlowTextChanged;

		// Load the FlowDocument into the RichTextBox.
		try
		{
			FlowDocument docFlow = GetDocumentFlow(richTextBox) ?? new();

			// Set the document to the new value.
			richTextBox.Document = docFlow;
		}
		catch (Exception)
		{
			richTextBox.Document = new();
		}

		ScrollToEnd(d, richTextBox);

		// Update the source when the document changes.
		richTextBox.TextChanged += OnFlowTextChanged;
	}

	private static void OnFlowTextChanged(object obj, TextChangedEventArgs e)
	{
		RichTextBox rtb = (RichTextBox)obj;
		if (rtb is not null)
		{
			SetDocumentFlow(rtb, rtb.Document);
		}
	}


	public static string GetDocumentXaml(DependencyObject obj) => (string)obj.GetValue(DocumentXamlProperty);
	public static void SetDocumentXaml(DependencyObject obj, string value) => obj.SetValue(DocumentXamlProperty, value);
	public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
		"DocumentXaml",
		typeof(string),
		typeof(RichTextBoxBinding),
		new FrameworkPropertyMetadata(
			defaultValue: string.Empty,
			FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
			OnDocumentXamlPropertyChanged
		)
	);


	private static void OnDocumentXamlPropertyChanged(DependencyObject /*RichTextBox*/ d, DependencyPropertyChangedEventArgs e)
	{
		RichTextBox richTextBox = (RichTextBox)d;

		/// Assigning a value to RichTextBox.Document raises the TextChanged
		/// event many times, so we remove it while we're in this handler.
		richTextBox.TextChanged -= OnXamlTextChanged;

		// Parse the XAML to a document (or use XamlReader.Parse())
		try
		{
			string docXaml = GetDocumentXaml(richTextBox);
			MemoryStream stream = new(Encoding.UTF8.GetBytes(docXaml));
			FlowDocument doc = string.IsNullOrEmpty(docXaml) ? new() : (FlowDocument)XamlReader.Load(stream);

			// Set the document to the new value.
			richTextBox.Document = doc;
		}
		catch (Exception)
		{
			richTextBox.Document = new FlowDocument();
		}

		ScrollToEnd(d, richTextBox);

		// Update the source when the document changes.
		richTextBox.TextChanged += OnXamlTextChanged;
	}

	private static void OnXamlTextChanged(object obj, TextChangedEventArgs e)
	{
		RichTextBox rtb = (RichTextBox)obj;
		if (rtb is not null)
		{
			SetDocumentXaml(rtb, XamlWriter.Save(rtb.Document));
		}
	}


	private static void ScrollToEnd(DependencyObject d, RichTextBox richTextBox)
	{
		if (GetScrollToEnd(d))
		{
			d.Dispatcher.BeginInvoke(() => richTextBox.ScrollToEnd());
		}
	}
}
