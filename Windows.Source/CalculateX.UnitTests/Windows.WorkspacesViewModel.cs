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
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);

#if !MAUI_UNITTESTS
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count(vm => vm.CanCloseTab));
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count(vm => !vm.CanCloseTab));
		Assert.IsTrue(vmWorkspaces.SelectedWorkspaceVM.CanCloseTab);

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count(vm => vm.CanCloseTab));
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count(vm => !vm.CanCloseTab));
		Assert.IsTrue(vmWorkspaces.SelectedWorkspaceVM.CanCloseTab);
#else
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
#endif
	}

	[TestMethod]
	public void TestDeleteWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
#if MAUI_UNITTESTS
		// Add an extra workspace to account for the missing "+" tab on Windows.
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
#endif
		/// +----+-----+
		/// |	0	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+-----+
		Assert.AreEqual(5, vmWorkspaces.TheWorkspaceViewModels.Count);

		/// Delete first
#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
#endif
		/// +----+-----+-----+-----+
		/// |	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		/// Delete middle
#if !MAUI_UNITTESTS
		vmWorkspaces.SelectedWorkspaceVM = vmWorkspaces.TheWorkspaceViewModels[1];
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[1]);
#endif
		/// Delete last
		/// +----+-----+-----+
		/// |	1	|	3	|	+	|
		/// +----+-----+-----+
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels[1]);
#endif
		/// Delete only
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);

#if !MAUI_UNITTESTS
		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
#else
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
		// We have to delete one more for the extra one we added at the beginning of this test.
		vmWorkspaces.DeleteWorkspace(vmWorkspaces.TheWorkspaceViewModels.First());
#endif
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		// It should re-create one so there is at least one workspace.
#if !MAUI_UNITTESTS
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
#else
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
#endif
	}

	[TestMethod]
	public void TestWorkspaceDefaultName()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+
		/// |	1	|	2	|	+	|
		/// +----+-----+-----+
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
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+
		/// |	1	|	3	|	+	|
		/// +----+-----+-----+
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
		/// +----+-----+
		/// |	3	|	+	|
		/// +----+-----+
#if !MAUI_UNITTESTS
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
#else
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
#endif
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
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
#if !MAUI_UNITTESTS
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
#else
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
#endif
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
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
#if !MAUI_UNITTESTS
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
#else
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count);
#endif
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
#if !MAUI_UNITTESTS
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
#endif
	}

	[TestMethod]
	public void TestNextPreviousWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
#if MAUI_UNITTESTS
		// Add an extra workspace to account for the missing "+" tab on Windows.
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
#endif
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+-----+
		Assert.AreEqual(5, vmWorkspaces.TheWorkspaceViewModels.Count);
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
}
