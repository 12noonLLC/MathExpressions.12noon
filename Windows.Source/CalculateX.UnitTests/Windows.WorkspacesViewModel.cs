using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspacesViewModel
{
	private const string StorageNameFormat1 = "CalculateX-Test{0}.xml";

	public static string StoragePath { get; private set; } = string.Empty;


	[ClassInitialize]
	public static void ClassSetup(TestContext testContext)
	{
		/*
				testContext.TestRunResultsDirectory
				"C:\\OneDrive\\Development\\12noon\\CalculateX\\TestResults\\Deploy_Stefan 2022-06-02 22_58_01"
		 */
		StoragePath = Path.Combine(testContext.TestRunResultsDirectory!, string.Format(CultureInfo.InvariantCulture, StorageNameFormat1, testContext.FullyQualifiedTestClassName));

		// Clean up after a previous failed test.
		File.Delete(StoragePath);

#if !MAUI_UNITTESTS
		App.MyAlertService = new AlertServiceUnitTests();
#endif
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
		File.Delete(StoragePath);
	}


	[TestInitialize]
	public void TestSetup()
	{
	}

	[TestCleanup]
	public void TestTeardown()
	{
		File.Delete(StoragePath);
	}


	[TestMethod]
	public void TestConstructor()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);

		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
	}

	[TestMethod]
	public void TestDeleteWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);
		/// +----+
		/// |	0	|
		/// +----+
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|
		/// +----+-----+-----+-----+
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);

		/// Delete first
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
#endif
		/// +----+-----+-----+
		/// |	1	|	2	|	3	|
		/// +----+-----+-----+
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		/// Delete middle
#if !MAUI_UNITTESTS
		vmWorkspaces.SelectedWorkspaceVM = vmWorkspaces.TheWorkspaceViewModels[1];
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[1]);
#endif
		/// Delete last
		/// +----+-----+
		/// |	1	|	3	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[1]);
#endif
		/// Delete only
		/// +----+
		/// |	1	|
		/// +----+
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);

#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
		// We have to delete one more for the extra one we added at the beginning of this test.
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
#endif
		/// +----+
		/// |	1	|
		/// +----+
		// It should re-create one so there is at least one workspace.
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
	}

	[TestMethod]
	public void TestWorkspaceDefaultName()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);
		/// +----+
		/// |	1	|
		/// +----+
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	2	|
		/// +----+-----+
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
#endif

		// Delete Workspace2. Add new. Should be Workspace3.
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[1]);
#endif
		/// +----+
		/// |	1	|
		/// +----+
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	3	|
		/// +----+-----+
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[1].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
#endif

		// Delete Workspace1.
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
		/// +----+
		/// |	3	|
		/// +----+
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif

		// Delete Workspace3. Should automatically re-create one, and it should be Workspace1.
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
		/// +----+
		/// |	1	|
		/// +----+
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif

		// Delete Workspace1. Should automatically re-create one, and it should be Workspace1.
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
		/// +----+
		/// |	1	|
		/// +----+
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
	}

	[TestMethod]
	public void TestNextPreviousWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|
		/// +----+-----+-----+-----+
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[3]);

		vmWorkspaces.SelectNextWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
		vmWorkspaces.SelectNextWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
		vmWorkspaces.SelectNextWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[2]);
		vmWorkspaces.SelectNextWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[3]);

		vmWorkspaces.SelectPreviousWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[2]);
		vmWorkspaces.SelectPreviousWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
		vmWorkspaces.SelectPreviousWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
		vmWorkspaces.SelectPreviousWorkspaceCommand.Execute(null);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[3]);
#endif
	}

	[TestMethod]
	public void TestSortAdd()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// Workspace1,2,3,4

		vmWorkspaces.SaveWorkspaces();

		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace4", vmWorkspaces.TheWorkspaceViewModels[3].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[0], "cat");
		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[1], "dog");
		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[2], "giraffe");
		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[3], "llama");
		// cat, dog, giraffe, llama
		Assert.AreEqual("cat", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama", vmWorkspaces.TheWorkspaceViewModels[3].Name);

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// cat, dog, giraffe, llama, Workspace5
		Assert.AreEqual("cat", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace5", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[4], "zebra");
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// cat, dog, giraffe, llama, Workspace6, zebra
		Assert.AreEqual("cat", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace6", vmWorkspaces.TheWorkspaceViewModels[4].Name);
		Assert.AreEqual("zebra", vmWorkspaces.TheWorkspaceViewModels[5].Name);
	}

	[TestMethod]
	public void TestSortRename()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = ViewModels.WorkspacesViewModel.ConstructFromFile(StoragePath);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// Workspace1,2,3,4

		vmWorkspaces.SaveWorkspaces();

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// Workspace1,2,3,4,5
		Assert.AreEqual("Workspace5", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[1], "Workspace8");
		// Workspace1,3,4,5,8
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace4", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace5", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[3], "Workspace2");
		// Workspace1,2,3,4,8
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace4", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[3], "Workspace6");
		// Workspace1,2,3,6,8
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace6", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[3], "Workspace5");
		// Workspace1,2,3,5,8
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace5", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[3], "Workspace55");
		// Workspace1,2,3,55,8
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace55", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[2], "Workspace9");
		// Workspace1,2,55,8,9
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace55", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace8", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace9", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		// Rename last workspace so it is still last.
		Assert.AreEqual(5, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[4], "Zebra");
		Assert.AreEqual("Zebra", vmWorkspaces.TheWorkspaceViewModels[4].Name);
	}
}
