using MathExpressions;
using MathExpressions.Metadata;
using MathExpressions.UnitConversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CalculateX
{
	public partial class CalculatorForm : Form
	{
		private readonly LinkedList<string> _history = new LinkedList<string>();
		// This is an instance variable to maintain the state of its variable dictionary.
		private readonly MathEvaluator _eval = new MathEvaluator();

		public CalculatorForm()
		{
			InitializeComponent();
			Application.Idle += OnApplicationIdle;
		}

		private void InitializeSettings()
		{
			SuspendLayout();

			bool upgradeRequired = Properties.Settings.Default.UpgradeRequired;
			if (upgradeRequired)
			{
				Properties.Settings.Default.UpgradeRequired = false;

				if (Properties.Settings.Default[nameof(Properties.Settings.Default.CalculatorLocation)] is null)
				{
					Properties.Settings.Default.CalculatorLocation = Location;
				}

				if (Properties.Settings.Default[nameof(Properties.Settings.Default.CalculatorSize)] is null)
				{
					Properties.Settings.Default.CalculatorSize = Size;
				}

				if (Properties.Settings.Default[nameof(Properties.Settings.Default.HistoryFont)] is null)
				{
					Properties.Settings.Default.HistoryFont = historyRichTextBox.Font;
				}

				if (Properties.Settings.Default[nameof(Properties.Settings.Default.InputFont)] is null)
				{
					Properties.Settings.Default.InputFont = inputTextBox.Font;
				}

				Properties.Settings.Default.Save();
			}

			// Settings must be assigned to a variable.
			// https://codedocu.com/Net-Framework/WinForms/Errors/C-_hash_,-Winforms_colon_-error-CS1061_colon_-PropertyStore-does-not-contain-a-definition-for-Settings
			WindowState = Properties.Settings.Default.CalculatorWindowState;

			var calculatorLocation = Properties.Settings.Default.CalculatorLocation;
			if (calculatorLocation != null)
			{
				StartPosition = FormStartPosition.Manual;
				Location = calculatorLocation;
			}

			var calculatorSize = Properties.Settings.Default.CalculatorSize;
			if (calculatorSize != null)
			{
				Size = calculatorSize;
			}

			var historyFont = Properties.Settings.Default.HistoryFont;
			if (historyFont != null)
			{
				historyRichTextBox.Font = historyFont;
			}

			var inputFont = Properties.Settings.Default.InputFont;
			if (inputFont != null)
			{
				inputTextBox.Font = inputFont;
			}

			replaceCalculatorToolStripMenuItem.Checked = Application.ExecutablePath.Equals(
				 ImageFileOptions.GetDebugger(CalculatorConstants.WindowsCalculatorName),
				 StringComparison.OrdinalIgnoreCase);

			allowOnlyOneInstanceToolStripMenuItem.Checked = Properties.Settings.Default.IsSingleInstance;

			ResumeLayout(true);
		}

		private void OnApplicationIdle(object sender, EventArgs e)
		{
			numLockToolStripStatusLabel.Text = NativeMethods.IsNumLockOn ? "NUM" : string.Empty;

			undoToolStripMenuItem.Enabled = inputTextBox.ContainsFocus && inputTextBox.CanUndo;
			undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled;
			undoContextStripMenuItem.Enabled = undoToolStripMenuItem.Enabled;

			cutToolStripMenuItem.Enabled = inputTextBox.ContainsFocus && inputTextBox.CanSelect;
			cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled;
			cutContextStripMenuItem.Enabled = cutToolStripMenuItem.Enabled;

			try
			{
				pasteToolStripMenuItem.Enabled = inputTextBox.ContainsFocus && Clipboard.ContainsText();
			}
			catch (Exception)
			{
				pasteToolStripMenuItem.Enabled = false;
			}
			pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled;
			pasteContextStripMenuItem.Enabled = pasteToolStripMenuItem.Enabled;
		}

		private void Eval(string input)
		{
			string answer;
			bool hasError = false;
			Stopwatch watch = Stopwatch.StartNew();
			try
			{
				answer = _eval.Evaluate(input).ToString();
			}
			catch (Exception ex)
			{
				answer = ex.Message;
				hasError = true;
			}
			watch.Stop();
			timerToolStripStatusLabel.Text = watch.Elapsed.TotalMilliseconds + " ms";
			answerToolStripStatusLabel.Text = "Answer: " + answer;

			// Prevent duplicate history entries
			if (!_history.Contains(input))
			{
				_history.AddFirst(input);
			}

			historyRichTextBox.SuspendLayout();
			historyRichTextBox.AppendText(input);
			historyRichTextBox.AppendText(Environment.NewLine);
			historyRichTextBox.AppendText("\t= ");
			if (hasError)
			{
				historyRichTextBox.SelectionColor = Color.Maroon;
			}
			else
			{
				historyRichTextBox.SelectionColor = Color.Blue;
			}
			historyRichTextBox.SelectionFont = new Font(historyRichTextBox.Font, FontStyle.Bold);
			historyRichTextBox.AppendText(answer);
			historyRichTextBox.AppendText(Environment.NewLine);
			historyRichTextBox.ScrollToCaret();
			historyRichTextBox.ResumeLayout();

			inputTextBox.ResetText();
			inputTextBox.Focus();
			inputTextBox.Select();
		}

		private void CalculatorForm_Load(object sender, EventArgs e)
		{
			InitializeSettings();

			inputTextBox.Focus();
			inputTextBox.Select();
		}

		private void CalculatorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.CalculatorLocation = Location;
			Properties.Settings.Default.CalculatorSize = Size;
			Properties.Settings.Default.CalculatorWindowState = WindowState;
			Properties.Settings.Default.HistoryFont = historyRichTextBox.Font;
			Properties.Settings.Default.InputFont = inputTextBox.Font;
			Properties.Settings.Default.Save();
		}

		private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox textBox = (TextBox)sender;

			if (e.KeyData == Keys.Enter)
			{
				if (textBox.TextLength > 0)
				{
					Eval(textBox.Text);
					e.Handled = true;
					// Prevent ding caused by Enter not having a default button.
					// https://stackoverflow.com/questions/6660772/avoid-windows-ding-when-enter-is-pressed-in-textbox-with-onkeyup
					e.SuppressKeyPress = true;
				}
			}
			else if (e.KeyData == Keys.Up)
			{
				if (_history.Any())
				{
					SetInputFromHistory(_history.First());

					_history.AddLast(_history.First());
					_history.RemoveFirst();

					e.Handled = true;
				}
			}
			else if (e.KeyData == Keys.Down)
			{
				if (_history.Any())
				{
					_history.AddFirst(_history.Last());
					_history.RemoveLast();

					SetInputFromHistory(_history.Last());

					e.Handled = true;
				}
			}
		}

		private void SetInputFromHistory(string s)
		{
			inputTextBox.Text = s;
			inputTextBox.Select(inputTextBox.TextLength, 0);
			inputTextBox.Focus();
		}

		private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			TextBox textBox = (TextBox)sender;

			if ((textBox.TextLength == 0) && OperatorExpression.IsSymbol(e.KeyChar))
			{
				textBox.Text = MathEvaluator.AnswerVariable + e.KeyChar;
				textBox.Select(textBox.TextLength, 0);
				e.Handled = true;
			}
		}

		private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/skst/CalculateX");
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm about = new AboutForm();
			about.ShowDialog(this);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult result = saveFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				File.WriteAllText(saveFileDialog.FileName, historyRichTextBox.Text);
			}
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			inputTextBox.Undo();
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			inputTextBox.Cut();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (historyRichTextBox.ContainsFocus)
			{
				historyRichTextBox.Copy();
			}
			else
			{
				inputTextBox.Copy();
			}
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			inputTextBox.Paste();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			inputTextBox.SelectAll();
		}

		private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			historyRichTextBox.ResetText();
		}

		private void historyFontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			fontDialog.Font = historyRichTextBox.Font;

			DialogResult result = fontDialog.ShowDialog(this);
			if (result != DialogResult.OK)
			{
				return;
			}

			historyRichTextBox.Font = fontDialog.Font;
		}

		private void inputFontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			fontDialog.Font = inputTextBox.Font;

			DialogResult result = fontDialog.ShowDialog(this);
			if (result != DialogResult.OK)
			{
				return;
			}

			inputTextBox.Font = fontDialog.Font;
		}

		private void replaceCalculatorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (replaceCalculatorToolStripMenuItem.Checked)
			{
				ImageFileOptions.SetDebugger(CalculatorConstants.WindowsCalculatorName, Application.ExecutablePath);
			}
			else
			{
				ImageFileOptions.ClearDebugger(CalculatorConstants.WindowsCalculatorName);
			}
		}

		private void function_Click(object sender, EventArgs e)
		{
			if (!(sender is ToolStripItem item) || (item.Tag is null))
			{
				return;
			}

			string insert = item.Tag.ToString();
			int start = inputTextBox.SelectionStart;
			int length = inputTextBox.SelectionLength;
			int pad = insert.IndexOf('|');

			if ((pad < 0) && (length == 0))
			{
				pad = insert.Length;
			}
			else if ((pad >= 0) && (length > 0))
			{
				pad = insert.Length;
			}

			inputTextBox.SuspendLayout();
			inputTextBox.Paste(insert.Replace("|", inputTextBox.SelectedText));
			inputTextBox.Select(start + pad + length, 0);
			inputTextBox.ResumeLayout();
		}

		private void AddToConvertMenuItem<T>(ToolStripMenuItem p)
			 where T : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(T);
			int[] a = (int[])Enum.GetValues(enumType);

			p.DropDownItems.Clear();
			foreach (int x in Enumerable.Range(0, a.Length))
			{
				MemberInfo parentInfo = GetMemberInfo(enumType, Enum.GetName(enumType, x));
				string parrentKey = AttributeReader.GetAbbreviation(parentInfo);
				string parrentName = AttributeReader.GetDescription(parentInfo);

				ToolStripMenuItem t = new ToolStripMenuItem(parrentName);
				p.DropDownItems.Add(t);

				foreach (int i in Enumerable.Range(0, a.Length))
				{
					if (x == i)
					{
						continue;
					}

					MemberInfo childInfo = GetMemberInfo(enumType, Enum.GetName(enumType, i));
					string childName = AttributeReader.GetDescription(childInfo);
					string childKey = AttributeReader.GetAbbreviation(childInfo);

					string key = string.Format(
						 CultureInfo.InvariantCulture,
						 ConvertExpression.KeyExpressionFormat2,
						 parrentKey,
						 childKey);

					ToolStripMenuItem s = new ToolStripMenuItem(childName);
					s.Click += convert_Click;
					s.Tag = key;

					t.DropDownItems.Add(s);
				}
			}
		}

		private static MemberInfo GetMemberInfo(Type type, string name)
		{
			MemberInfo[] info = type.GetMember(name) ?? new MemberInfo[0];
			if (info.Length == 0)
			{
				return null;
			}

			return info[0];
		}

		private void lengthToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (lengthToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<LengthUnit>(lengthToolStripMenuItem);
		}

		private void massToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (massToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<MassUnit>(massToolStripMenuItem);

		}

		private void speedToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (speedToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<SpeedUnit>(speedToolStripMenuItem);

		}

		private void temperatureToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (temperatureToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<TemperatureUnit>(temperatureToolStripMenuItem);
		}

		private void timeToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (timeToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<TimeUnit>(timeToolStripMenuItem);
		}

		private void volumeToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (volumeToolStripMenuItem.DropDownItems.Count > 1)
			{
				return;
			}

			AddToConvertMenuItem<VolumeUnit>(volumeToolStripMenuItem);
		}

		private void convert_Click(object sender, EventArgs e)
		{
			ToolStripItem item = sender as ToolStripItem;
			if (item?.Tag is null)
			{
				return;
			}

			string insert = item.Tag.ToString();
			int start = inputTextBox.SelectionStart;
			int length = inputTextBox.SelectionLength;
			int pad = insert.Length;

			inputTextBox.SuspendLayout();
			inputTextBox.Paste(insert);
			inputTextBox.Select(start + pad + length, 0);
			inputTextBox.ResumeLayout();
		}

		private void allowOnlyOneInstanceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.IsSingleInstance = allowOnlyOneInstanceToolStripMenuItem.Checked;
			Properties.Settings.Default.Save();
		}
	}
}
