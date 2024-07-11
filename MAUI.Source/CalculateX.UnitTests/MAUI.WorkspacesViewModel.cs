using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspacesMAUI
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
		Assert.AreEqual("cat",     vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog",     vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama",   vmWorkspaces.TheWorkspaceViewModels[3].Name);

		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// cat, dog, giraffe, llama, Workspace5
		Assert.AreEqual("cat",        vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog",        vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe",    vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama",      vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace5", vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[4], "zebra");
		vmWorkspaces.AddWorkspaceCommand.Execute(null);
		// cat, dog, giraffe, llama, Workspace6, zebra
		Assert.AreEqual("cat",         vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("dog",         vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("giraffe",     vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("llama",       vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace6",  vmWorkspaces.TheWorkspaceViewModels[4].Name);
		Assert.AreEqual("zebra",       vmWorkspaces.TheWorkspaceViewModels[5].Name);
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
		Assert.AreEqual("Workspace2",  vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace3",  vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace1",  vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace55", vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace8",  vmWorkspaces.TheWorkspaceViewModels[4].Name);

		vmWorkspaces.RenameWorkspace(vmWorkspaces.TheWorkspaceViewModels[2], "Workspace9");
		// Workspace1,2,55,8,9
		Assert.AreEqual("Workspace1",  vmWorkspaces.TheWorkspaceViewModels[0].Name);
		Assert.AreEqual("Workspace2",  vmWorkspaces.TheWorkspaceViewModels[1].Name);
		Assert.AreEqual("Workspace55", vmWorkspaces.TheWorkspaceViewModels[2].Name);
		Assert.AreEqual("Workspace8",  vmWorkspaces.TheWorkspaceViewModels[3].Name);
		Assert.AreEqual("Workspace9",  vmWorkspaces.TheWorkspaceViewModels[4].Name);
	}
}
