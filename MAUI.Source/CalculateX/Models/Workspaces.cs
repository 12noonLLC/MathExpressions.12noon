using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;

namespace CalculateX.Models;

internal class Workspaces
{
	public string? CurrentWorkspaceID { get; set; }
	public ObservableCollection<Workspace> TheWorkspaces { get; private set; } = new();

	private const string CalculateX_FileName = "CalculateX.xml";
	private static string GetWorkspacesFilePath() => Path.Combine(FileSystem.AppDataDirectory, CalculateX_FileName);

	private const string NAME_ELEMENT_WORKSPACES = "workspaces";
	private const string NAME_ATTRIBUTE_SELECTED = "selected";
	private const string NAME_ELEMENT_WORKSPACE = "workspace";
	private const string NAME_ATTRIBUTE_ID = "id";
	private const string NAME_ATTRIBUTE_NAME = "name";
	private const string NAME_ELEMENT_INPUTS = "inputs";
	private const string NAME_ELEMENT_KEY = "key";
	private const string NAME_ATTRIBUTE_ORDINAL = "ordinal";


	public Workspaces()
	{
		LoadWorkspaces();
	}

	public void DeleteWorkspace(string workspaceID)
	{
		Workspace? deleteWorkspace = TheWorkspaces.FirstOrDefault(w => w.ID == workspaceID);
		Debug.Assert(deleteWorkspace is not null);
		if (deleteWorkspace is null)
		{
			return;
		}

		TheWorkspaces.Remove(deleteWorkspace);
	}

	/// <example>
	///	<calculatex>
	///		<workspaces>
	///			<workspace id="..." name="..." selected="true/false">
	///				<inputs>
	///					<key ordinal="1">cat</key>
	///					<key ordinal="2">dog</key>
	///				</inputs>
	///			</workspace>
	///			<workspace>
	///				...
	///			</workspace>
	///		</workspaces>
	///	</calculatex>
	/// </example>
	public void SaveWorkspaces()
	{
		XDocument xdoc = new(
			new XElement(NAME_ELEMENT_WORKSPACES,
				TheWorkspaces
				.Select(w =>
					new XElement(NAME_ELEMENT_WORKSPACE,
						new XAttribute(NAME_ATTRIBUTE_ID, w.ID),
						new XAttribute(NAME_ATTRIBUTE_NAME, w.Name),
						new XAttribute(NAME_ATTRIBUTE_SELECTED, (w.ID == CurrentWorkspaceID)),
						w.History
						.Aggregate(
							seed: (new XElement(NAME_ELEMENT_INPUTS), 0),
							func:
							((XElement root, int n) t, Workspace.HistoryEntry entry) =>
							{
								++t.n;   // advance ordinal
								t.root.Add(
									new XElement(NAME_ELEMENT_KEY,
										new XAttribute(NAME_ATTRIBUTE_ORDINAL, t.n),
										// If necessary, modify entry to clear variable when we evaluate it again.
										entry.IsCleared ? entry.Input + '=' : entry.Input
									)
								);
								return t;
							}
						)
						.Item1
					)
				))
			);
		xdoc.Save(GetWorkspacesFilePath());
	}

	public void LoadWorkspaces()
	{
		// Try to load workspaces
		string filePath = GetWorkspacesFilePath();
		if (!File.Exists(filePath))
		{
			CurrentWorkspaceID = null;
			return;
		}

		XDocument? xdoc = null;
		try
		{
			xdoc = XDocument.Load(filePath);
		}
		catch (Exception)
		{
			CurrentWorkspaceID = null;
			return;
		}

		string? selectedWorkspaceID = null;
		foreach (XElement xWorkspace in xdoc.Element(NAME_ELEMENT_WORKSPACES)?.Elements(NAME_ELEMENT_WORKSPACE) ?? Enumerable.Empty<XElement>())
		{
			string id = xWorkspace.Attribute(NAME_ATTRIBUTE_ID)!.Value!;
			string name = xWorkspace.Attribute(NAME_ATTRIBUTE_NAME)!.Value!;
			Workspace workspace = new(id, name);
			bool selected = (bool?)xWorkspace.Attribute(NAME_ATTRIBUTE_SELECTED) ?? false;
			if (selected)
			{
				Debug.Assert(selectedWorkspaceID is null);
				selectedWorkspaceID = workspace.ID;
			}
			foreach (string input in
											xWorkspace
											.Element(NAME_ELEMENT_INPUTS)!
											.Elements(NAME_ELEMENT_KEY)
											.Select(e => (ordinal: (int)e.Attribute(NAME_ATTRIBUTE_ORDINAL)!, value: e.Value))
											.OrderBy(t => t.ordinal)
											.Select(t => t.value)
			)
			{
				// Evaluate loaded input and add to history
				workspace.Evaluate(input);
			}

			TheWorkspaces.Add(workspace);
		}

		CurrentWorkspaceID = selectedWorkspaceID;
	}
}
