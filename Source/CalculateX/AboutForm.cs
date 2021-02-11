using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace CalculateX
{
	partial class AboutForm : Form
	{

		public AboutForm()
		{
			InitializeComponent();

			titleLabel.Text = ThisAssembly.AssemblyTitle;
			versionLabel.Text = "Version " + ThisAssembly.AssemblyVersion;
			descriptionLabel.Text = ThisAssembly.AssemblyDescription;
			copyrightLabel.Text = ThisAssembly.AssemblyCopyright;

			// We need to do this to guarantee that the assembly is in the domain.
			var _ = new MathExpressions.MathEvaluator();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					string name = assembly.FullName;
					bool isIncluded = !String.IsNullOrEmpty(assembly.Location);
					if (isIncluded)
						isIncluded = !(name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
									 || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
									 || name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase));

					if (isIncluded)
					{
						ListViewItem item = new ListViewItem();

						AssemblyName assemblyName = assembly.GetName();
						item.Text = assemblyName.Name;

						item.SubItems.Add(assemblyName.Version.ToString());

						DateTime date = File.GetLastAccessTime(assembly.Location);
						item.SubItems.Add(date.ToShortDateString());

						assembliesListView.Items.Add(item);
					}
				}
				catch (NotSupportedException) { }
			}
		}

		private void loresoftLinkLabel_LinkClicked(object /*LinkLabel*/ sender, LinkLabelLinkClickedEventArgs e)
		{
			LinkLabel label = (LinkLabel)sender;
			System.Diagnostics.Process.Start(label.Text);
		}
	}
}
