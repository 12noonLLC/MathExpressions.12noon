using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CalculateX.Models;

public class Workspaces
{
	/// <summary>
	/// This is only valid after the workspaces are loaded from storage.
	/// It is not updated by the view-model.
	/// </summary>
	public string? LoadedSelectedWorkspaceID { get; private set; }
	public ObservableCollection<Workspace> TheWorkspaces { get; private set; } = new();

	private readonly string _pathStorageFile;

	private const string NAME_ELEMENT_WORKSPACES = "workspaces";
	private const string NAME_ATTRIBUTE_SELECTED = "selected";
	private const string NAME_ELEMENT_WORKSPACE = "workspace";
	private const string NAME_ATTRIBUTE_ID = "id";
	private const string NAME_ATTRIBUTE_NAME = "name";
	private const string NAME_ELEMENT_INPUTS = "inputs";
	private const string NAME_ELEMENT_KEY = "key";
	private const string NAME_ATTRIBUTE_ORDINAL = "ordinal";


	public Workspaces(string pathFile)
	{
		_pathStorageFile = pathFile;
		LoadWorkspaces();
	}

	public void AddWorkspace(Workspace addWorkspace)
	{
#if DEBUG
		Workspace? foundWorkspace = TheWorkspaces.FirstOrDefault(w => w.ID == addWorkspace.ID);
		Debug.Assert(foundWorkspace is null);
		if (foundWorkspace is not null)
		{
			return;
		}
#endif

		TheWorkspaces.Add(addWorkspace);
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

	/// <summary>
	///
	/// </summary>
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
	/// <param name="selectedWorkspaceID">ID of the selected workspace</param>
	public void SaveWorkspaces(string selectedWorkspaceID)
	{
		XDocument xdoc = new(
			new XElement(NAME_ELEMENT_WORKSPACES,
				TheWorkspaces
				.Select(w =>
					new XElement(NAME_ELEMENT_WORKSPACE,
						new XAttribute(NAME_ATTRIBUTE_ID, w.ID),
						new XAttribute(NAME_ATTRIBUTE_NAME, w.Name),
						new XAttribute(NAME_ATTRIBUTE_SELECTED, (w.ID == selectedWorkspaceID)),
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
										entry.GetInput()
									)
								);
								return t;
							}
						)
						.Item1
					)
				))
			);
		xdoc.Save(_pathStorageFile);
	}

	/// <summary>
	///
	/// </summary>
	private void LoadWorkspaces()
	{
		LoadedSelectedWorkspaceID = null;

		// Try to load workspaces
		if (!File.Exists(_pathStorageFile))
		{
			return;
		}

		XDocument? xdoc = null;
		try
		{
			xdoc = XDocument.Load(_pathStorageFile);
		}
		catch (Exception)
		{
			return;
		}

		string? selectedWorkspaceID = null;
		foreach (XElement xWorkspace in xdoc.Element(NAME_ELEMENT_WORKSPACES)?.Elements(NAME_ELEMENT_WORKSPACE) ?? Enumerable.Empty<XElement>())
		{
			string id = xWorkspace.Attribute(NAME_ATTRIBUTE_ID)!.Value;
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

		LoadedSelectedWorkspaceID = selectedWorkspaceID;
	}
}
