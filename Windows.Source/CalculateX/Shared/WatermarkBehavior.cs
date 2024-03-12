using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Shared;

/// <summary>
///
/// </summary>
/// <example>
/// Install NuGet: Microsoft.Xaml.Behaviors.Wpf
/// xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
///
/// Two methods:
///	1. Use text properties of parent control.
///	<TextBox Margin="10">
///		<i:Interaction.Behaviors>
///			<local:WatermarkBehavior Text="Fish" />
///		</i:Interaction.Behaviors>
///
///	2. Specify exact watermark.
///	<TextBox Margin="10">
///		<i:Interaction.Behaviors>
///			<local:WatermarkBehavior>
///				<local:WatermarkBehavior.Watermark>
///					<TextBlock Text="BIG ol' FrameworkElement" VerticalAlignment="Center" Foreground="DarkGray" />
///				</local:WatermarkBehavior.Watermark>
///			</local:WatermarkBehavior>
///		</i:Interaction.Behaviors>
///	</TextBox>
///
/// Example:
/// <TextBlock Text="{Binding MyProperty, NotifyOnTargetUpdated=True}">
///	<i:Interaction.Behaviors>
///		<shared:WatermarkBehavior Text="This is the watermark" />
///	</i:Interaction.Behaviors>
/// </TextBlock>
/// </example>
/// <remarks>
/// The associated control must not set Handled to true if handling the
/// TextBox.TextChanged or ComboBox.SelectionChanged event.
///
/// TextBlock must set its Text.Binding.NotifyOnTargetUpdated to true.
/// </remarks>
/// <see cref="http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf"/>
/// <see cref="http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf#answer-836463"/>
/// <see cref="http://pwlodek.blogspot.com/2009/11/watermark-effect-for-wpfs-textbox.html"/>
/// <see cref="https://jasonkemp.ca/blog/the-missing-net-4-cue-banner-in-wpf-i-mean-watermark-in-wpf/"/>
/// Adorners Overview <see cref="https://msdn.microsoft.com/en-us/library/ms743737.aspx"/>
/// Adorner <see cref="https://msdn.microsoft.com/en-us/library/system.windows.documents.adorner.aspx"/>
/// ContentPresenter <see cref="https://msdn.microsoft.com/en-us/library/system.windows.controls.contentpresenter.aspx"/>
public class WatermarkBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
{
	#region Dependency Properties

	public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(nameof(Watermark), typeof(FrameworkElement), typeof(WatermarkBehavior));

	public FrameworkElement Watermark
	{
		get => (FrameworkElement)GetValue(WatermarkProperty);
		set => SetValue(WatermarkProperty, value);
	}


	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(WatermarkBehavior));

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}


	public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register(nameof(HorizontalAlignment), typeof(HorizontalAlignment), typeof(WatermarkBehavior));

	public HorizontalAlignment HorizontalAlignment
	{
		get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
		set => SetValue(HorizontalAlignmentProperty, value);
	}

	#endregion Dependency Properties

	public bool Show
	{
		get
		{
			if ((AssociatedObject is ComboBox cb) && (cb.SelectedIndex == -1))
			{
				return true;
			}

			if ((AssociatedObject is TextBox tb) && string.IsNullOrEmpty(tb.Text))
			{
				return true;
			}

			if ((AssociatedObject is TextBlock tbl) && string.IsNullOrEmpty(tbl.Text))
			{
				return true;
			}

			return false;
		}
	}


	protected override void OnAttached()
	{
		if (AssociatedObject is not ComboBox and not TextBox and not TextBlock)
		{
			throw new NotSupportedException($"{nameof(WatermarkBehavior)} supports only {nameof(ComboBox)}, {nameof(TextBox)}, and {nameof(TextBlock)} controls.");
		}

		AssociatedObject.Loaded += (object sender, RoutedEventArgs e) =>
		{
			UpdateWatermark();
		};

		if (AssociatedObject is TextBox textBox)
		{
			textBox.TextChanged += (object sender, TextChangedEventArgs e) =>
			{
				UpdateWatermark();
			};
		}
		else if (AssociatedObject is ComboBox comboBox)
		{
			comboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
			{
				UpdateWatermark();
			};
		}
		else if (AssociatedObject is TextBlock textBlock)
		{
			// Ensure textBlock.Text.Binding.NotifyOnTargetUpdated = true
			Binding binding = BindingOperations.GetBinding(textBlock, TextBlock.TextProperty);
			if (!binding.NotifyOnTargetUpdated)
			{
				throw new NotSupportedException($"Watermark is not supported when {nameof(Text)} property's {nameof(Binding)}'s {nameof(Binding.NotifyOnTargetUpdated)} property is false;");
			}

			textBlock.TargetUpdated += (object? sender, DataTransferEventArgs e) =>
			{
				UpdateWatermark();
			};
		}
	}

	private void UpdateWatermark()
	{
		if (Show)
		{
			AddWatermark();
		}
		else
		{
			RemoveWatermark();
		}
	}

	private void AddWatermark()
	{
		var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
		if (layer is null)
		{
			return;
		}

		if (layer.GetAdorners(AssociatedObject) is not null)
		{
			// The adorner has already been added
			return;
		}

		WatermarkAdorner adorner;
		if (Watermark is null)
		{
			adorner = new WatermarkAdornerText(AssociatedObject, Text, HorizontalAlignment);
		}
		else
		{
			adorner = new WatermarkAdornerElement(AssociatedObject, Watermark, HorizontalAlignment);
		}
		layer.Add(adorner);
	}

	private void RemoveWatermark()
	{
		AdornerLayer layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
		if (layer is null)
		{
			return;
		}

		Adorner[] adorners = layer.GetAdorners(AssociatedObject);
		if (adorners is null)
		{
			return;
		}

		new List<Adorner>(adorners)
			.OfType<WatermarkAdorner>().ToList()
			.ForEach(a =>
			{
				a.Visibility = Visibility.Hidden;
				layer.Remove(a);
			});
	}

	public class WatermarkAdorner : Adorner
	{
		public WatermarkAdorner(UIElement element) : base(element)
		{
			// Setting IsHitTestVisible of the adorner to false, so when the user clicks on
			// the placeholder of a control (i.e. ComboBox), the click is passed to the
			// adorned control. Otherwise the click is not passed to the adorned control.
			// In the case of a ComboBox, the drop down box for a ComboBox won't be opened.
			IsHitTestVisible = false;

			VerticalAlignment = VerticalAlignment.Center;
			Opacity = 0.5;
		}
	}

	public class WatermarkAdornerText : WatermarkAdorner
	{
		private readonly string _placeholderText;
		private readonly HorizontalAlignment _horizontalAlignment;
		private readonly FontFamily _fontFamily;
		private readonly double _fontSize;
		private readonly FontStyle _fontStyle;
		private readonly FontWeight _fontWeight;
		private readonly Brush _foreground;
		private readonly Thickness _padding;

		/// <summary>
		///
		/// </summary>
		/// <param name="adornedElement"><see cref="UIElement" /> to be adorned</param>
		/// <param name="placeholderText">The text to display as the watermark</param>
		public WatermarkAdornerText(UIElement adornedElement, string placeholderText, HorizontalAlignment horizontalAlignment) : base(adornedElement)
		{
			ArgumentNullException.ThrowIfNull(placeholderText);

			_placeholderText = placeholderText;
			_horizontalAlignment = horizontalAlignment;

			dynamic? ctrl = ((adornedElement as TextBox as dynamic) ?? (adornedElement as ComboBox)) ?? (adornedElement as TextBlock);
			ArgumentNullException.ThrowIfNull(ctrl);
			_fontFamily = ctrl!.FontFamily;
			_fontSize = ctrl.FontSize;
			_fontStyle = ctrl.FontStyle;
			_fontWeight = ctrl.FontWeight;
			_foreground = ctrl.Foreground;
			_padding = ctrl.Padding;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Ensure the text does not go outside the control.
			drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight)));

			FormattedText formattedText = new(
					_placeholderText,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(_fontFamily, _fontStyle, _fontWeight, FontStretches.Normal, _fontFamily), _fontSize, _foreground,
					VisualTreeHelper.GetDpi(this).PixelsPerDip);

			double lx;
			if (_horizontalAlignment == HorizontalAlignment.Left)
			{
				/// To align left, use (_padding.Left, _padding.Top).
				/// But that needs about _padding.Left+4 to position it after the caret,
				/// and I'm not sure where that's coming from.
				lx = _padding.Left;
			}
			else if (_horizontalAlignment == HorizontalAlignment.Right)
			{
				lx = this.ActualWidth - formattedText.Width;
			}
			else // Center or Stretch
			{
				// Center watermark text
				lx = (this.ActualWidth - formattedText.Width) / 2;
			}

			var ty = (this.ActualHeight - formattedText.Height) / 2;
			drawingContext.DrawText(formattedText, new Point(lx, ty));
		}
	}

	/// <summary>
	/// This adorner specifies an entire element as the watermark.
	/// </summary>
	/// <see cref="http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf#answer-836463" />
	public class WatermarkAdornerElement : WatermarkAdorner
	{
		/// <summary>
		/// <see cref="ContentPresenter"/> that holds the watermark
		/// </summary>
		private readonly ContentPresenter _contentPresenter;

		/// <summary>
		/// Initializes a new instance of the <see cref="WatermarkAdorner"/> class
		/// </summary>
		/// <param name="adornedElement"><see cref="UIElement"/> to be adorned</param>
		/// <param name="watermark">The watermark</param>
		public WatermarkAdornerElement(UIElement adornedElement, UIElement watermark, HorizontalAlignment horizontalAlignment) : base(adornedElement)
		{
			_contentPresenter = new ContentPresenter()
			{
				Content = watermark,
				HorizontalAlignment = horizontalAlignment,
				VerticalAlignment = VerticalAlignment.Center,
			};

			//if (AdornedElement is ItemsControl && !(AdornedElement is ComboBox))
			//{
			//	_contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
			//}

			// Hide the control adorner when the adorned element is hidden
			Binding binding = new("IsVisible") { Source = adornedElement, Converter = new BooleanToVisibilityConverter() };
			SetBinding(VisibilityProperty, binding);
		}

		#region Protected Overrides

		protected override int VisualChildrenCount => 1;

		/// <summary>
		/// Returns a specified child <see cref="Visual"/> for the parent <see cref="ContainerVisual"/>.
		/// </summary>
		/// <param name="index">A 32-bit signed integer that represents the index value of the child <see cref="Visual"/>.
		/// The value of index must be between 0 and <see cref="VisualChildrenCount"/> - 1.</param>
		/// <returns>The child <see cref="Visual"/>.</returns>
		protected override Visual GetVisualChild(int index)
		{
			return _contentPresenter;
		}

		/// <summary>
		/// Implements any custom measuring behavior for the adorner.
		/// </summary>
		/// <param name="constraint">A size to constrain the adorner to.</param>
		/// <returns>A <see cref="Size"/> object representing the amount of layout space needed by the adorner.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			// Here's the secret to getting the adorner to cover the whole control
			_contentPresenter.Measure(AdornedElement.RenderSize);
			return AdornedElement.RenderSize;
		}

		/// <summary>
		/// When overridden in a derived class, positions child elements and determines a size for a <see cref="FrameworkElement"/> derived class.
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			_contentPresenter.Arrange(new Rect(finalSize));
			return finalSize;
		}

		#endregion Protected Overrides
	}
}
