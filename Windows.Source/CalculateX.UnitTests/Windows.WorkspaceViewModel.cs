using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspaceViewModel
{
	[ClassInitialize]
	public static void ClassSetup(TestContext _)
	{
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
	}


	[TestInitialize]
	public void TestSetup()
	{
	}

	[TestCleanup]
	public void TestTeardown()
	{
	}


	[TestMethod]
	public void TestConstructorDefault()
	{
		ViewModels.WorkspaceViewModel vm = new();
#if !MAUI_UNITTESTS
		Assert.AreEqual("Designer", vm.Name);
#else
		Assert.AreEqual("temporary", vm.Name);
#endif
	}

	[TestMethod]
	public void TestConstructorWorkspace()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);
		Assert.AreEqual("Test", vm.Name);
	}

	[TestMethod]
	public void TestCompareWorkspaceViewModelEquals()
	{
		Models.Workspace workspaceRandom1 = new("Test1");
		ViewModels.WorkspaceViewModel vmRandom1 = new(workspaceRandom1);

		Models.Workspace workspaceRandom2 = new("Test2");
		ViewModels.WorkspaceViewModel vmRandom2 = new(workspaceRandom2);

		Assert.AreNotEqual(vmRandom1, vmRandom2);
		Assert.IsFalse(vmRandom1 == vmRandom2);
		Assert.IsTrue(vmRandom1 != vmRandom2);


		Models.Workspace workspace1 = new("id1", "Test1");
		ViewModels.WorkspaceViewModel vm1 = new(workspace1);

		Models.Workspace workspace2 = new("id2", "Test2");
		ViewModels.WorkspaceViewModel vm2 = new(workspace2);

		Assert.AreNotEqual(vm1, vm2);
		Assert.IsFalse(vm1 == vm2);
		Assert.IsTrue(vm1 != vm2);


		Models.Workspace workspace3 = new("id1", "Test3");
		ViewModels.WorkspaceViewModel vm3 = new(workspace3);

		Assert.AreEqual(vm1, vm3);
		Assert.IsTrue(vm1 == vm3);
		Assert.IsFalse(vm1 != vm3);
	}

	[TestMethod]
	public void TestInput()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);

		Assert.IsFalse(vm.EvaluateCommand.CanExecute(null));

		vm.Input = "45 + 136";
		Assert.AreEqual("45 + 136", vm.Input);
		Assert.IsTrue(vm.EvaluateCommand.CanExecute(null));

		vm.EvaluateCommand.Execute(null);

#if !MAUI_UNITTESTS
		vm.ClearInput();
#else
		vm.Input = string.Empty;
#endif
		Assert.AreEqual(string.Empty, vm.Input);
	}

	[TestMethod]
	public void TestInsertTextAtCursor()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);

		const string insertText = "fish";
		vm.Input = "0123456789";
		Assert.AreEqual(7, vm.InsertTextAtCursor(insertText, 3, 0));
		vm.Input = "0123456789";
		Assert.AreEqual(7, vm.InsertTextAtCursor(insertText, 3, 5));
		vm.Input = "0123456789";
		Assert.AreEqual(4, vm.InsertTextAtCursor(insertText, 0, 0));
		vm.Input = "0123456789";
		Assert.AreEqual(14, vm.InsertTextAtCursor(insertText, vm.Input.Length, 0));
		vm.Input = "0123456789";
		Assert.AreEqual(10, vm.InsertTextAtCursor(insertText, vm.Input.Length - insertText.Length, insertText.Length));
	}

#if !MAUI_UNITTESTS
	[TestMethod]
	public void TestHelp()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);

		Assert.IsFalse(vm.ShowHelp);
		vm.ShowHelp = true;
		Assert.IsTrue(vm.ShowHelp);
		vm.ShowHelp = false;
		Assert.IsFalse(vm.ShowHelp);
	}

	[TestMethod]
	public void TestClearHistory()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);

		Assert.IsFalse(vm.ClearHistoryCommand.CanExecute(null));

		vm.Input = "2+3";
		vm.EvaluateCommand.Execute(null);
		vm.Input = "45*77";
		vm.EvaluateCommand.Execute(null);

		Assert.AreEqual(2, vm._workspace.History.Count);
		Assert.IsTrue(vm.ClearHistoryCommand.CanExecute(null));

		vm.ClearHistoryCommand.Execute(null);
		Assert.IsFalse(vm.ClearHistoryCommand.CanExecute(null));
	}
#endif
}
