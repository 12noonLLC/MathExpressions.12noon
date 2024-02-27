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

		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count(vm => vm.CanCloseTab));
		Assert.AreEqual(1, vmWorkspaces.TheWorkspaceViewModels.Count(vm => !vm.CanCloseTab));
		Assert.IsTrue(vmWorkspaces.SelectedWorkspaceVM.CanCloseTab);
	}

	[TestMethod]
	public void TestDeleteWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
		/// +----+-----+
		/// |	0	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+-----+
		Assert.AreEqual(5, vmWorkspaces.TheWorkspaceViewModels.Count);

		/// Delete first
		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+
		/// |	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		/// Delete middle
		vmWorkspaces.SelectedWorkspaceVM = vmWorkspaces.TheWorkspaceViewModels[1];
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
		/// Delete last
		/// +----+-----+-----+
		/// |	1	|	3	|	+	|
		/// +----+-----+-----+
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
		/// Delete only
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);

		vmWorkspaces.TheWorkspaceViewModels.First().DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		// It should re-create one so there is at least one workspace.
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
	}

	[TestMethod]
	public void TestWorkspaceDefaultName()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);

		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		/// +----+-----+-----+
		/// |	1	|	2	|	+	|
		/// +----+-----+-----+
		Assert.AreEqual("Workspace2", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);

		// Delete Workspace2. Add new. Should be Workspace3.
		vmWorkspaces.TheWorkspaceViewModels[1].DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		/// +----+-----+-----+
		/// |	1	|	3	|	+	|
		/// +----+-----+-----+
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[1]);

		// Delete Workspace1.
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	3	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace3", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);

		// Delete Workspace3. Should automatically re-create one, and it should be Workspace1.
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);

		// Delete Workspace1. Should automatically re-create one, and it should be Workspace1.
		vmWorkspaces.TheWorkspaceViewModels[0].DeleteWorkspaceCommand.Execute(null);
		/// +----+-----+
		/// |	1	|	+	|
		/// +----+-----+
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		Assert.AreEqual("Workspace1", vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreSame(vmWorkspaces.SelectedWorkspaceVM, vmWorkspaces.TheWorkspaceViewModels[0]);
	}

	[TestMethod]
	public void TestNextPreviousWorkspace()
	{
		ViewModels.WorkspacesViewModel vmWorkspaces = new(StoragePath);
		Assert.AreEqual(2, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		Assert.AreEqual(3, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		Assert.AreEqual(4, vmWorkspaces.TheWorkspaceViewModels.Count);
		vmWorkspaces.NewWorkspaceCommand.Execute(null);
		/// +----+-----+-----+-----+-----+
		/// |	0	|	1	|	2	|	3	|	+	|
		/// +----+-----+-----+-----+-----+
		Assert.AreEqual(5, vmWorkspaces.TheWorkspaceViewModels.Count);
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
	}
}
