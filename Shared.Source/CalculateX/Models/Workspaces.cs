using System;
using System.Collections.Generic;
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
	public ObservableCollection<Workspace> TheWorkspaces { get; private set; } = [];
	public readonly List<Workspace> _deletedWorkspaces = [];

	public DateTimeOffset LastSynchronizedUTC { get; set; }

	private readonly string _pathStorageFile;

	public const string NAME_ELEMENT_ROOT = "calculatex";
	public const string NAME_ATTRIBUTE_SYNCHRONIZED = "last-synchronized";
	public const string NAME_ELEMENT_WORKSPACES = "workspaces";
	public const string NAME_ELEMENT_DELETED_WORKSPACES = "deleted-workspaces";
	private const int MAX_DELETED_WORKSPACES = 100;


	public Workspaces(string pathFile)
	{
		_pathStorageFile = pathFile;
		LoadWorkspaces(pathFile);
	}

	//private Workspaces(StreamReader reader)
	//{
	//	_pathStorageFile = string.Empty;
	//	LoadWorkspaces(reader);
	//}

	public Workspaces(XDocument xdoc)
	{
		_pathStorageFile = string.Empty;
		LoadWorkspaces(xdoc);
	}

	public Workspaces()
	{
		_pathStorageFile = string.Empty;
	}


	public void AddWorkspace(Workspace addWorkspace)
	{
#if DEBUG
		Workspace? foundWorkspace = TheWorkspaces.FirstOrDefault(w => w == addWorkspace);
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

		deleteWorkspace.LastModifiedUTC = DateTimeOffset.UtcNow;
		deleteWorkspace.ClearHistory();

		Debug.Assert(!_deletedWorkspaces.Any(wExists => wExists == deleteWorkspace), "There is already a workspace with this ID in the deleted list.");
		_deletedWorkspaces.Add(deleteWorkspace);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="selectedWorkspaceID">ID of the selected workspace</param>
	public void SaveWorkspaces(string selectedWorkspaceID)
	{
		if (string.IsNullOrEmpty(_pathStorageFile))
		{
			return;
		}

		XDocument xdoc = ToXML(selectedWorkspaceID);
		xdoc.Save(_pathStorageFile);
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
	/// <returns>XML of workspaces</returns>
	public XDocument ToXML(string selectedWorkspaceID)
	{
		XDocument xdoc = new(
			new XElement(NAME_ELEMENT_ROOT,
				new XAttribute(NAME_ATTRIBUTE_SYNCHRONIZED, LastSynchronizedUTC.ToString("o")),
				new XElement(NAME_ELEMENT_WORKSPACES,
					TheWorkspaces
					.Select(w => w.ToXML(selectedWorkspaceID))
				),
				new XElement(NAME_ELEMENT_DELETED_WORKSPACES,
					_deletedWorkspaces
					.OrderByDescending(w => w.LastModifiedUTC)
					.Take(MAX_DELETED_WORKSPACES)
					.Select(w => w.ToXML(selectedWorkspaceID: string.Empty))
				)
			)
		);
		return xdoc;
	}

	/// <summary>
	/// Initializes this object from the passed XML file.
	/// </summary>
	private void LoadWorkspaces(string pathStorageFile)
	{
		LoadedSelectedWorkspaceID = null;

		// Try to load workspaces
		if (!File.Exists(pathStorageFile))
		{
			return;
		}

		try
		{
			using StreamReader reader = new(pathStorageFile);
			LoadWorkspaces(reader);
		}
		catch (Exception)
		{
		}
	}

	private void LoadWorkspaces(StreamReader reader)
	{
		LoadedSelectedWorkspaceID = null;

		try
		{
			XDocument xdoc = XDocument.Load(reader);
			LoadWorkspaces(xdoc);
		}
		catch (Exception)
		{
		}
	}

	private void LoadWorkspaces(XDocument xdoc)
	{
		LoadedSelectedWorkspaceID = null;

		try
		{
			Workspaces workspaces = FromXML(xdoc);
			TheWorkspaces = workspaces.TheWorkspaces;
			_deletedWorkspaces.AddRange(workspaces._deletedWorkspaces);
			LoadedSelectedWorkspaceID = workspaces.LoadedSelectedWorkspaceID;
			LastSynchronizedUTC = workspaces.LastSynchronizedUTC;
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Creates a Workspaces object from the passed XML.
	/// </summary>
	/// <param name="xdoc"></param>
	/// <returns>New <see cref="Workspaces"/> object.</returns>
	private static Workspaces FromXML(XDocument xdoc)
	{
		/*
		 * 11 Oct 2024
		 * If root is old "workspaces" tag, move it under new root tag.
		 *
		 * TODO: This can be removed after everyone has updated to the new version.
		 */
		if (xdoc.Root?.Name == "workspaces")
		{
			XElement newRoot = new(NAME_ELEMENT_ROOT);
			newRoot.Add(xdoc.Root);
			xdoc.Root.ReplaceWith(newRoot);
		}

		Workspaces workspaces = new()
		{
			LastSynchronizedUTC = DateTimeOffset.Parse(xdoc.Root!.Attribute(NAME_ATTRIBUTE_SYNCHRONIZED)?.Value ?? DateTimeOffset.UtcNow.ToString("o"))
		};
		string? selectedWorkspaceID = null;
		foreach (XElement xWorkspace in xdoc.Root!.Element(NAME_ELEMENT_WORKSPACES)?.Elements(Workspace.NAME_ELEMENT_WORKSPACE) ?? [])
		{
			Workspace workspace = Workspace.FromXML(xWorkspace);
			workspaces.TheWorkspaces.Add(workspace);

			bool selected = (bool?)xWorkspace.Attribute(Workspace.NAME_ATTRIBUTE_SELECTED) ?? false;
			if (selected)
			{
				Debug.Assert(selectedWorkspaceID is null);
				selectedWorkspaceID = workspace.ID;
			}
		}
		workspaces.LoadedSelectedWorkspaceID = selectedWorkspaceID;

		foreach (XElement xWorkspace in xdoc.Root.Element(NAME_ELEMENT_DELETED_WORKSPACES)?.Elements(Workspace.NAME_ELEMENT_WORKSPACE) ?? [])
		{
			Workspace workspace = Workspace.FromXML(xWorkspace);
			workspaces._deletedWorkspaces.Add(workspace);
		}

		return workspaces;
	}
}
