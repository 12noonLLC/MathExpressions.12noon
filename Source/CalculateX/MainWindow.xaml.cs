using MathExpressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CalculateX
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, Shared.IRaisePropertyChanged
	{
		/// <summary>
		/// Commands for this window.
		/// </summary>
		public static RoutedUICommand ClearInput { get; } = new("Clear Input", nameof(ClearInput), typeof(MainWindow),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Escape, ModifierKeys.None),
			}
		);
		public static RoutedUICommand ClearHistory { get; } = new("Clear", nameof(ClearHistory), typeof(MainWindow),
			new InputGestureCollection()
			{
				// Note: Ctrl+Del and Ctrl+Backspace do not work.
				new KeyGesture(Key.D, ModifierKeys.Control),
			}
		);
		public static RoutedCommand HistoryPrevious { get; } = new(nameof(HistoryPrevious), typeof(MainWindow));
		public static RoutedCommand HistoryNext { get; } = new(nameof(HistoryNext), typeof(MainWindow));


		private readonly Shared.NotifyProperty<string> _input;
		public string Input { get => _input.Value; set => _input.Value = value; }


		// record history of inputs so we can save them and play them back on restart.
		private readonly Shared.StoreStringsOrdered InputRecord = new("inputs");

		// separate history because it is rearranged based on MRU entry.
		private readonly CircularHistory _entryHistory = new();


		// This is an instance variable to maintain the state of its variable dictionary.
		// No need to dispose it because it's present for the entire lifetime.
		private readonly MathEvaluator _eval = new();

		public VariableDictionary Variables { get => _eval.Variables; private set {} }

#pragma warning disable IDE0052 // Remove unread private members
		// This class provides features through event handling.
		private readonly Shared.WindowPosition _windowPosition;
#pragma warning restore IDE0052 // Remove unread private members

		private readonly Shared.NotifyProperty<bool> _showHelp;
		public bool ShowHelp { get => _showHelp.Value; set => _showHelp.Value = value; }


		public MainWindow()
		{
			_windowPosition = new(this, "MainWindowPosition");
			_input = new Shared.NotifyProperty<string>(this, nameof(Input), initialValue: String.Empty);
			_showHelp = new Shared.NotifyProperty<bool>(this, nameof(ShowHelp), initialValue: false);
			InputRecord.Load();

			InitializeComponent();
			DataContext = this;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Restore saved entries
			InputRecord.Get().ToList().ForEach(i => Evaluate(i));
			CollectionViewSource.GetDefaultView(Variables).Refresh();

			HistoryDisplay.ScrollToEnd();
		}


		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			ShowHelp = !ShowHelp;
		}


		private void ClearInput_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !String.IsNullOrEmpty(Input);
		}
		private void ClearInput_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Input = String.Empty;
		}


		private void ClearHistory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !InputRecord.IsEmpty();
		}
		private void ClearHistory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			InputRecord.Clear();
			InputRecord.Save();

			_ = new TextRange(HistoryDisplay.Document.ContentStart, HistoryDisplay.Document.ContentEnd)
			{
				Text = String.Empty
			};

			// Clear variables (because they will not exist when the app restarts).
			Variables.Initialize();
			CollectionViewSource.GetDefaultView(Variables).Refresh();
		}


		private void HistoryPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !_entryHistory.IsEmpty;
		}
		private void HistoryPrevious_Executed(object /*Window*/ sender, ExecutedRoutedEventArgs e)
		{
			Debug.Assert(!_entryHistory.IsEmpty);

			TextBox textBox = (TextBox)e.Source;

			string entry = _entryHistory.PreviousEntry(Input);
			SetInput(entry, textBox);

			e.Handled = true;
		}

		private void HistoryNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !_entryHistory.IsEmpty;
		}
		private void HistoryNext_Executed(object /*Window*/ sender, ExecutedRoutedEventArgs e)
		{
			Debug.Assert(!_entryHistory.IsEmpty);

			TextBox textBox = (TextBox)e.Source;

			string entry = _entryHistory.NextEntry(Input);
			SetInput(entry, textBox);

			e.Handled = true;
		}

		private void SetInput(string s, TextBox textBox)
		{
			Input = s;

			// Position cursor at the end of the text
			textBox.Select(textBox.Text.Length, 0);
		}


		/// <summary>
		/// If the first character the user enters is an operator symbol,
		/// prepend it with the name of the "answer" variable.
		/// </summary>
		/// <param name="sender">TextBox for input</param>
		/// <param name="e"></param>
		private void InputTextBox_TextChanged(object /*TextBox*/ sender, TextChangedEventArgs e)
		{
			if ((Input.Length == 1) && (e.Changes.First().AddedLength == 1) && (e.Changes.First().Offset == 0))
			{
				TextBox textBox = (TextBox)e.Source;
				char op = Input.First();
				if (OperatorExpression.IsSymbol(op))
				{
					Input = MathEvaluator.AnswerVariable + Input;

					// Position cursor at the end of the text
					textBox.Select(textBox.Text.Length, 0);
				}
			}
		}


		private void EvaluateButton_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(Input))
			{
				return;
			}

			// Save input in playback record.
			InputRecord.Add(Input);
			InputRecord.Save();

			Evaluate(Input);
			CollectionViewSource.GetDefaultView(Variables).Refresh();

			// Clear input when we're done
			Input = String.Empty;
		}

		private void Evaluate(string input)
		{
			try
			{
				double? d = _eval.Evaluate(input);
				// If a variable was deleted, modify the history entry.
				if (d is null)
				{
					AppendHistoryEntry(input, String.Empty, Brushes.Black);
				}
				else
				{
					AppendHistoryEntry(input, d.Value.ToString(), Brushes.Blue);
				}
			}
			catch (Exception ex)
			{
				AppendHistoryEntry(input, ex.Message, Brushes.Red);
			}

			_entryHistory.AddNewEntry(input);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="input">The user's typed expression</param>
		/// <param name="answer">The value of the expression. (Empty if variable was deleted.)</param>
		/// <param name="fgBrush"></param>
		private void AppendHistoryEntry(string input, string answer, Brush fgBrush)
		{
			input = input.Trim();

			if (String.IsNullOrEmpty(answer))
			{
				_ = new TextRange(HistoryDisplay.Document.ContentEnd, HistoryDisplay.Document.ContentEnd)
				{
					Text = $"{input} "
				};

				TextRange trange = new(HistoryDisplay.Document.ContentEnd, HistoryDisplay.Document.ContentEnd)
				{
					Text = "cleared" + Environment.NewLine
				};
				trange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
			}
			else
			{
				// "input = " in default color...
				_ = new TextRange(HistoryDisplay.Document.ContentEnd, HistoryDisplay.Document.ContentEnd)
				{
					Text = $"{input} = "
				};

				// followed by answer string.
				TextRange trange = new(HistoryDisplay.Document.ContentEnd, HistoryDisplay.Document.ContentEnd)
				{
					Text = answer + Environment.NewLine
				};
				trange.ApplyPropertyValue(TextElement.ForegroundProperty, fgBrush);
			}

			HistoryDisplay.ScrollToEnd();
		}

		#region Implement IRaisePropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion Implement IRaisePropertyChanged
	}
}
