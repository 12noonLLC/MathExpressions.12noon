using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Shared;


/// <summary>
/// </summary>
/// <see cref="https://docs.telerik.com/devtools/wpf/controls/radtabcontrol/howto/how-to-make-the-tab-headers-editable-wpf"/>
/// <see cref="https://docs.telerik.com/devtools/wpf/controls/radtabcontrol/howto/how-to-add-close-button-to-the-tab-headers"/>
/// <seealso cref="https://www.codeproject.com/Articles/138024/Header-Editable-Tab-Control-in-WPF"/>
/// <example>
/// 	<TabControl ItemsSource="{Binding PRODUCTS}">
/// 		<TabControl.Resources>
/// 			<ResourceDictionary>
/// 				<ResourceDictionary.MergedDictionaries>
/// 					<ResourceDictionary Source="Shared\EditableTabHeaderDictionary.xaml" />
/// 				</ResourceDictionary.MergedDictionaries>
/// 			</ResourceDictionary>
/// 		</TabControl.Resources>
/// 		<TabControl.ItemTemplate>
/// 			<DataTemplate>
/// 				<shared:EditableTabHeaderControl Content="{Binding NAME, Mode=TwoWay}" />
/// 			</DataTemplate>
/// 		</TabControl.ItemTemplate>
/// 	</TabControl>
///
/// 	public MainWindow()
/// 	{
/// 		InitializeComponent();
/// 		DataContext = this;
///
/// 		EventManager.RegisterClassHandler(typeof(TabItem), RoutedEventHelper.CloseTabEvent, new RoutedEventHandler(OnCloseTab));
/// 	}
/// 	public void OnCloseTab(object /*TabItem*/ sender, RoutedEventArgs e)
/// 	{
/// 		// close the tab
/// 	}
///
///	The TabItem's DataContext class can implement
///	Shared.EditableTabHeaderControl.IEditableTabHeaderControl
///	in order to override the value of the CanCloseTab property.
///	This is optional, but without it, a WPF binding error will be thrown
///	for the undefined 'CanCloseTab.'
///
///	public class Product : Shared.EditableTabHeaderControl.IEditableTabHeaderControl
///	{
///		public bool CanCloseTab { get; set; } = true;	// IEditableTabHeaderControl
///	}
/// </example>
[TemplatePart(Name="PART_EditArea", Type=typeof(TextBox))]
public class EditableTabHeaderControl : ContentControl
{
	public interface IEditableTabHeaderControl
	{
		public bool CanCloseTab { get; }
	}


	static EditableTabHeaderControl()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTabHeaderControl), new FrameworkPropertyMetadata(typeof(EditableTabHeaderControl)));
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private TextBox _headerTextBox;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private string _originalHeader = string.Empty;

	public bool IsInEditMode
	{
		get => (bool)GetValue(IsInEditModeProperty);
		set => SetValue(IsInEditModeProperty, value);
	}
	public static readonly DependencyProperty IsInEditModeProperty =
		DependencyProperty.Register(nameof(IsInEditMode), typeof(bool), typeof(EditableTabHeaderControl));

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		_headerTextBox = (TextBox)Template.FindName("PART_EditArea", this);
		_headerTextBox.LostFocus += TextBox_LostFocus;
		_headerTextBox.KeyDown += TextBox_KeyDown;

		MouseDoubleClick += EditableTabHeaderControl_MouseDoubleClick;
	}

	private void EditableTabHeaderControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			_originalHeader = _headerTextBox.Text;
			IsInEditMode = true;
			Dispatcher.BeginInvoke(() => _headerTextBox.Focus());
		}
	}

	private void TextBox_LostFocus(object /*TextBox*/ sender, RoutedEventArgs e)
	{
		IsInEditMode = false;
	}

	private void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		TextBox headerTextBox = (TextBox)sender;

		if (e.Key == Key.Enter)
		{
			IsInEditMode = false;
			headerTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
		else if (e.Key == Key.Escape)
		{
			// Restore original value
			IsInEditMode = false;
			if (_headerTextBox.Text != _originalHeader)
			{
				_headerTextBox.Text = _originalHeader;
			}
			headerTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
	}


	/*
	 * Close and Add buttons
	 */
	//public class TabViewModel : INotifyPropertyChanged, IDisposable
	//{
	//	private bool isSelected;
	//	private readonly MainViewModel mainViewModel;

	//	public TabViewModel(MainViewModel mainViewModel)
	//	{
	//		this.mainViewModel = mainViewModel;
	//		this.mainViewModel.Tabs.CollectionChanged += this.Tabs_CollectionChanged;

	//		this.AddItemCommand = new DelegateCommand(
	//			 delegate
	//			 {
	//				 this.mainViewModel.AddItem(this);
	//			 },
	//			 delegate
	//			 {
	//				 return this.mainViewModel.Tabs.Count < 5;
	//			 });

	//		this.RemoveItemCommand = new DelegateCommand(
	//			 delegate
	//			 {
	//				 this.mainViewModel.RemoveItem(this);
	//			 },
	//			 delegate
	//			 {
	//				 return this.mainViewModel.Tabs.Count > 1;
	//			 });
	//	}

	//	public void Dispose()
	//	{
	//		this.mainViewModel.Tabs.CollectionChanged -= this.Tabs_CollectionChanged;
	//	}

	//	void Tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	//	{
	//		this.AddItemCommand.InvalidateCanExecute();
	//		this.RemoveItemCommand.InvalidateCanExecute();
	//	}

	//	public string Header { get; set; }

	//	public bool IsSelected
	//	{
	//		get => this.isSelected;
	//		set
	//		{
	//			if (this.isSelected != value)
	//			{
	//				this.isSelected = value;
	//				this.OnPropertyChanged("IsSelected");
	//			}
	//		}
	//	}

	//	public DelegateCommand AddItemCommand { get; set; }
	//	public DelegateCommand RemoveItemCommand { get; set; }

	//	#region INotifyPropertyChanged
	//	public event PropertyChangedEventHandler PropertyChanged;

	//	private void OnPropertyChanged(string propertyName)
	//	{
	//		if (this.PropertyChanged != null)
	//		{
	//			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
	//		}
	//	}
	//}
}


/// <summary>
/// Create routed events for the extended TabHeader class.
/// </summary>
public class RoutedEventHelper
{
	/// <summary>
	/// Set up routed event raised when the user clicks the Close Tab button.
	/// </summary>
	/// <example>
	/// EventManager.RegisterClassHandler(typeof(TabItem), Shared.RoutedEventHelper.CloseTabEvent, new RoutedEventHandler(OnCloseTab));
	/// </example>
	public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent(
		"CloseTab",
		RoutingStrategy.Bubble,
		typeof(RoutedEventHandler),
		typeof(RoutedEventHelper));

	public static bool GetEnableRoutedClick(DependencyObject obj) => (bool)obj.GetValue(EnableRoutedClickProperty);
	public static void SetEnableRoutedClick(DependencyObject obj, bool value) => obj.SetValue(EnableRoutedClickProperty, value);

	public static readonly DependencyProperty EnableRoutedClickProperty = DependencyProperty.RegisterAttached(
		"EnableRoutedClick",
		typeof(bool),
		typeof(RoutedEventHelper),
		new PropertyMetadata(OnEnableRoutedClickChanged));

	private static void OnEnableRoutedClickChanged(DependencyObject /*Button*/ sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is not Button button)
		{
			return;
		}
		var newValue = (bool)e.NewValue;
		if (newValue)
		{
			button.Click += OnButtonClick;
		}
	}

	private static void OnButtonClick(object /*Button*/ sender, RoutedEventArgs e)
	{
		if (sender is Control control)
		{
			control.RaiseEvent(new RoutedEventArgs(CloseTabEvent, control));
		}
	}


	/// <summary>
	/// Set up routed event raised when the user changes the header text of a tab item.
	/// </summary>
	/// <example>
	/// EventManager.RegisterClassHandler(typeof(TabItem), Shared.RoutedEventHelper.HeaderChangedEvent, new RoutedEventHandler(OnWorkspaceNameChanged));
	/// </example>
	public static readonly RoutedEvent HeaderChangedEvent = EventManager.RegisterRoutedEvent(
		"HeaderChanged",
		RoutingStrategy.Bubble,
		typeof(RoutedEventHandler),
		typeof(RoutedEventHelper));

	public static bool GetEnableRoutedHeaderChanged(DependencyObject obj) => (bool)obj.GetValue(EnableRoutedHeaderChangedProperty);
	public static void SetEnableRoutedHeaderChanged(DependencyObject obj, bool value) => obj.SetValue(EnableRoutedHeaderChangedProperty, value);

	public static readonly DependencyProperty EnableRoutedHeaderChangedProperty = DependencyProperty.RegisterAttached(
		"EnableRoutedHeaderChanged",
		typeof(bool),
		typeof(RoutedEventHelper),
		new PropertyMetadata(OnEnableRoutedHeaderChangedChanged));

	private static void OnEnableRoutedHeaderChangedChanged(DependencyObject /*TextBox*/ sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
		{
			return;
		}
		var newValue = (bool)e.NewValue;
		if (newValue)
		{
			textBox.SourceUpdated += TextBox_SourceUpdated;
		}
	}

	private static void TextBox_SourceUpdated(object? /*TextBox*/ sender, DataTransferEventArgs e)
	{
		if (sender is Control control)
		{
			control.RaiseEvent(new RoutedEventArgs(HeaderChangedEvent, control));
		}
	}
}
