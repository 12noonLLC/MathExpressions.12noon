using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Shared;

/// <summary>
/// This behavior will save and restore the following settings:
///	column widths
///	column order
/// </summary>
/// <example>
/// Create settings for widths and order.
///	Example: (string) MainWindow_Widths
///	Example: (string) MainWindow_Order
/// Add namespaces to the window:
///	xmlns:b="clr-namespace:Behaviors"
///	xmlns:props="clr-namespace:Typecast.Properties"
/// Add the behavior to the ListView to be saved:
///	<b:ListViewPersistBehavior MySettings="{x:Static props:Settings.Default}" Setting_Widths="MainWindow_Widths" Setting_Order="MainWindow_Order" />
/// </example>
public class ListViewPersistBehavior : Microsoft.Xaml.Behaviors.Behavior<ListView>
{
	public System.Configuration.ApplicationSettingsBase? MySettings { get; set; }
	public string? Setting_Widths { get; set; }
	public string? Setting_Order { get; set; }

	private const string List_DELIMITER = ",";

	private bool _isWidthsDirty;
	private bool _isOrderDirty;

	private List<int> _columnOrder = new();


	protected override void OnAttached()
	{
		if ((MySettings is null) || String.IsNullOrWhiteSpace(Setting_Widths))
		{
			Debug.WriteLine($"ERROR: Please specify {nameof(MySettings)} and {nameof(Setting_Widths)}. ({nameof(Setting_Order)} is optional.)");
			return;
		}

		AssociatedObject.Loaded += AssociatedObject_Loaded;
		Application.Current.Exit += Application_Exit;

		// We can't use the Unloaded event. It's raised when the control is destroyed, but the view is null by then.
		// We can't use the SizeChanged event. It's raised only when WPF sets the initial widths.
	}

	// This is never called, but just in case...
	protected override void OnDetaching()
	{
		GridView gridView = (GridView)AssociatedObject.View;
		gridView.Columns.CollectionChanged -= Columns_CollectionChanged;

		Application.Current.Exit -= Application_Exit;
		AssociatedObject.Loaded -= AssociatedObject_Loaded;
	}



	/// <summary>
	/// Restore the control's state.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(MySettings);
		if (AssociatedObject.View is not GridView gridView)
		{
			throw new ArgumentException($"This behavior is for {nameof(GridView)} only.");
		}

		GridViewColumnCollection columns = gridView.Columns;

		_columnOrder.AddRange(Enumerable.Range(0, columns.Count));

		// Ensure the requisite settings are present.
		try
		{
			LoadColumnOrder(columns);
			LoadColumnWidths(columns);
		}
		catch (System.Configuration.SettingsPropertyNotFoundException)
		{
			Debug.WriteLine($"ERROR: Please create {nameof(String)} settings named \"{nameof(Setting_Widths)}\" and \"{nameof(Setting_Order)}.\"");
		}

		// Subscribe to the necessary events.
		gridView.Columns.CollectionChanged += Columns_CollectionChanged;

		foreach (GridViewColumn column in gridView.Columns)
		{
			((INotifyPropertyChanged)column).PropertyChanged += GridViewColumn_PropertyChanged;
		}
	}

	/// <summary>
	/// Load column order (if any).
	/// </summary>
	/// <param name="columns"></param>
	private void LoadColumnOrder(GridViewColumnCollection columns)
	{
		ArgumentNullException.ThrowIfNull(MySettings);
		if (Setting_Order is null)
		{
			return;
		}

		string strOrder = (string)MySettings[Setting_Order];
		if (!String.IsNullOrWhiteSpace(strOrder))
		{
			List<int> newOrder = strOrder.Split(List_DELIMITER.ToCharArray()).Select(o => Convert.ToInt32(o)).ToList();
			if (newOrder.Count == columns.Count)
			{
				/*
				 * For each entry in the new order, add each column with THAT index to the column collection.
				 * columns	= 0,1,2,3,4,5,6,7,8,9
				 * newOrder	= 2,3,0,1,5,4,6,8,7,9
				 */
				// Move columns to a staging area
				List<GridViewColumn> staging = new(columns);
				columns.Clear();

				// Copy from staging area to live column collection
				foreach (int n in Enumerable.Range(0, staging.Count))
				{
					columns.Add(staging[newOrder[n]]);
				}

				// Record the new order
				_columnOrder = newOrder;
			}
		}
	}

	/// <summary>
	/// Load column widths (if any).
	/// Note: Load them AFTER the order because they were saved under this new order.
	/// </summary>
	/// <param name="columns"></param>
	private void LoadColumnWidths(GridViewColumnCollection columns)
	{
		ArgumentNullException.ThrowIfNull(MySettings);
		string strWidths = (string)MySettings[Setting_Widths];
		if (!String.IsNullOrWhiteSpace(strWidths))
		{
			string[] widths = strWidths.Split(List_DELIMITER.ToCharArray());
			if (widths.Length == columns.Count)
			{
				foreach (int n in Enumerable.Range(0, columns.Count))
				{
					columns[n].Width = Convert.ToDouble(widths[n]);
				}
			}
		}
	}


	/// <summary>
	/// We could subscribe to PropertyChanged to catch when ActualWidth changes,
	/// and save widths then, but it's raised while the user drags the divider--too often.
	/// So we only set a dirty flag.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void GridViewColumn_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(GridViewColumnHeader.ActualWidth))
		{
			_isWidthsDirty = true;
		}
	}

	/// <summary>
	/// Track the order of the columns.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Move)
		{
			int ixColumn = _columnOrder[e.OldStartingIndex];
			_columnOrder.RemoveAt(e.OldStartingIndex);
			_columnOrder.Insert(e.NewStartingIndex, ixColumn);
			_isOrderDirty = true;
		}
	}

	/// <summary>
	/// Save state on exit.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Application_Exit(object sender, ExitEventArgs e)
	{
		GridView gridView = (GridView)AssociatedObject.View;
		if (gridView is null)
		{
			return;
		}
		GridViewColumnCollection columns = gridView.Columns;

		SaveColumnWidthsAndOrder(columns);
	}

	private void SaveColumnWidthsAndOrder(GridViewColumnCollection columns)
	{
		ArgumentNullException.ThrowIfNull(MySettings);

		/*
		 * Save column widths
		 *	XYZZY_Widths = "80,250,40,100,70"
		 */
		if (_isWidthsDirty)
		{
			// If any Width is NaN, don't save.
			if (columns.Any(c => Double.IsNaN(c.ActualWidth)))
			{
				if (Debugger.IsAttached) Debugger.Break();   // Follows WER settings if debugger is not attached.
				return;
			}

			MySettings[Setting_Widths] = String.Join(List_DELIMITER, columns.Select(c => c.ActualWidth));
			_isWidthsDirty = false;
		}

		/*
		 * Save column order
		 * XYZZY_Order = "1,3,0,4,2"
		 */
		if (_isOrderDirty)
		{
			MySettings[Setting_Order] = String.Join(List_DELIMITER, _columnOrder);
			_isOrderDirty = false;
		}

		MySettings.Save();
	}
}
