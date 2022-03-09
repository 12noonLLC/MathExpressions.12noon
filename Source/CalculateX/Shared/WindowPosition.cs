using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml.Linq;

namespace Shared;

/*
 * REF: WPF 4 pp756-758
 * REF: WPF Unleashed (Listing 7.2)
 */
public class WindowPosition
{
	private Window _window;
	private readonly string _tag;

	private Rect _bounds;
	private bool _bMinimized;
	private bool _bMaximized;

	private const string KEY_ATTRIBUTE_LEFT = "left";
	private const string KEY_ATTRIBUTE_TOP = "top";
	private const string KEY_ATTRIBUTE_WIDTH = "width";
	private const string KEY_ATTRIBUTE_HEIGHT = "height";
	private const string KEY_ATTRIBUTE_MINIMIZED = "minimized";
	private const string KEY_ATTRIBUTE_MAXIMIZED = "maximized";


	/// <summary>
	/// This constructor will automatically save and restore the passed
	/// window's position in isolated storage.
	/// </summary>
	/// <remarks>
	/// Yes, we could use subclasses, but banana-monkey-jungle problem.
	/// </remarks>
	/// <param name="w">window whose position is to be saved and restored</param>
	/// <param name="tag">unique name for isolated storage</param>
	public WindowPosition(Window w, string tag)
	{
		_tag = tag;

		// Read the current position from storage
		XElement? position = MyStorage.ReadElement(_tag);
		if (position is null)
		{
			_bounds = w.RestoreBounds;
			_bMinimized = (w.WindowState == WindowState.Minimized);
			_bMaximized = (w.WindowState == WindowState.Maximized);
		}
		else
		{
			FromXML(position);
		}

		SetWindow(w);
	}

	/// <summary>
	/// This constructor loads the associated window's position from the passed XML.
	/// The caller must use <see cref="ToXML()"/> and <see cref="FromXML(XElement)"/>
	/// to form and extract the values for reading/writing to a <see cref="XDocument"/> object.
	/// </summary>
	/// <param name="w">window whose position is to be saved and restored</param>
	/// <param name="xml"></param>
	[Obsolete("We do not use the class this way. If we do, we'll have to allow _tag to be null.")]
	public WindowPosition(Window w, XElement xml)
	{
		_tag = xml.Name.LocalName;

		FromXML(xml);
		SetWindow(w);
		SetWindowPosition();
	}

	[MemberNotNull(nameof(_window))]
	public void SetWindow(Window w)
	{
		_window = w ?? throw new ArgumentNullException(nameof(w));

		_window.Loaded += (object sender, RoutedEventArgs e) =>
		{
			System.Diagnostics.Debug.Assert(sender as Window == _window);

			SetWindowPosition();
		};

		_window.Closing += (object? sender, CancelEventArgs e) =>
		{
			System.Diagnostics.Debug.Assert(sender as Window == _window);

			_bounds = _window.RestoreBounds;
			_bMinimized = (_window.WindowState == WindowState.Minimized);
			_bMaximized = (_window.WindowState == WindowState.Maximized);

			MyStorage.WriteElement(_tag, new XElement(nameof(WindowPosition), ToXML()));
		};
	}


	/// <summary>
	/// Format the current window position data into XML.
	/// </summary>
	/// <returns>Collection of attributes</returns>
	public IEnumerable<XAttribute> ToXML()
	{
		if (_bounds.IsEmpty || (_bounds.Width == 0) || (_bounds.Height == 0))
		{
			return System.Linq.Enumerable.Empty<XAttribute>();
		}

		return
			new XElement("parent",
				new XAttribute(KEY_ATTRIBUTE_LEFT, _bounds.Left),
				new XAttribute(KEY_ATTRIBUTE_TOP, _bounds.Top),
				new XAttribute(KEY_ATTRIBUTE_WIDTH, _bounds.Width),
				new XAttribute(KEY_ATTRIBUTE_HEIGHT, _bounds.Height),
				new XAttribute(KEY_ATTRIBUTE_MINIMIZED, _bMinimized),
				new XAttribute(KEY_ATTRIBUTE_MAXIMIZED, _bMaximized)
			)
			.Attributes();
	}



	/// <summary>
	/// Call when restoring the window's position from XML storage.
	/// </summary>
	/// <param name="w">Window whose position is to be restored</param>
	/// <param name="xml">XML node with position attributes (returned by SaveToXML())</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Argument is in passed XML")]
	private void FromXML(XElement? xml)
	{
		if (xml is null)
		{
			return;
		}

		XAttribute elt = xml.Attribute(KEY_ATTRIBUTE_LEFT) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_LEFT);
		if (elt != null)
		{
			_bounds.X = (double)elt;
		}

		elt = xml.Attribute(KEY_ATTRIBUTE_TOP) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_TOP);
		if (elt != null)
		{
			_bounds.Y = (double)elt;
		}

		elt = xml.Attribute(KEY_ATTRIBUTE_WIDTH) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_WIDTH);
		if (elt != null)
		{
			_bounds.Width = (double)elt;
		}

		elt = xml.Attribute(KEY_ATTRIBUTE_HEIGHT) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_HEIGHT);
		if (elt != null)
		{
			_bounds.Height = (double)elt;
		}

		elt = xml.Attribute(KEY_ATTRIBUTE_MINIMIZED) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_MINIMIZED);
		if (elt != null)
		{
			_bMinimized = (bool)elt;
		}

		elt = xml.Attribute(KEY_ATTRIBUTE_MAXIMIZED) ?? throw new ArgumentNullException(KEY_ATTRIBUTE_MAXIMIZED);
		if (elt != null)
		{
			_bMaximized = (bool)elt;
		}
	}


	/// <summary>
	/// Set the passed window to the passed location and size.
	/// It also ensures that the window is fully on the display.
	/// </summary>
	private void SetWindowPosition()
	{
		// We can't set a Setting to Rect.Empty
		if (!_bounds.IsEmpty && (_bounds.Width > 0) && (_bounds.Height > 0))
		{
			_window.WindowStartupLocation = WindowStartupLocation.Manual;
			_window.SizeToContent = SizeToContent.Manual;

			_window.Top = _bounds.Top;
			_window.Left = _bounds.Left;
			_window.Width = _bounds.Width;
			_window.Height = _bounds.Height;

			EnsureWindowOnScreen(_window);
		}

		if (_bMinimized)
		{
			_window.WindowState = WindowState.Minimized;
		}
		else if (_bMaximized)
		{
			_window.WindowState = WindowState.Maximized;
		}
	}

	/// <summary>
	/// Ensure that the passed window is fully displayed on the screen.
	/// This accounts for multiple monitors.
	/// </summary>
	/// <param name="w">Window to be on the screen</param>
	private static void EnsureWindowOnScreen(Window w)
	{
		double lScreen = SystemParameters.VirtualScreenLeft;
		double tScreen = SystemParameters.VirtualScreenTop;
		double cxScreen = SystemParameters.VirtualScreenWidth;
		double cyScreen = SystemParameters.VirtualScreenHeight;

		// ensure the window's not too big for the screen
		w.Width = Math.Min(w.Width, cxScreen);
		w.Height = Math.Min(w.Height, cyScreen);

		// if necessary, move the window right and down
		w.Left = Math.Max(w.Left, lScreen);
		w.Top = Math.Max(w.Top, tScreen);

		// if necessary, move the window left and up
		w.Left -= Math.Max(0, (w.Left + w.Width) - (lScreen + cxScreen));
		w.Top -= Math.Max(0, (w.Top + w.Height) - (tScreen + cyScreen));
	}
}
