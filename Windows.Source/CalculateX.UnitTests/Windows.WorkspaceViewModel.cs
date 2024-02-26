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
		Assert.IsTrue(vm.CanCloseTab);
		Assert.AreEqual("Designer", vm.Name);
	}

	[TestMethod]
	public void TestConstructorCanCloseTab()
	{
		Assert.ThrowsException<ArgumentException>(() => new ViewModels.WorkspaceViewModel(canCloseTab: true));

		ViewModels.WorkspaceViewModel vm = new(canCloseTab: false);
		Assert.IsFalse(vm.CanCloseTab);
		Assert.AreEqual("+", vm.Name);
	}

	[TestMethod]
	public void TestConstructorWorkspace()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);
		Assert.IsTrue(vm.CanCloseTab);
		Assert.AreEqual("Test", vm.Name);
	}

	[TestMethod]
	public void TestCanCloseTab()
	{
		Models.Workspace workspace = new("Test");
		ViewModels.WorkspaceViewModel vm = new(workspace);

		Assert.IsTrue(vm.CanCloseTab);
		vm.CanCloseTab = false;
		Assert.IsFalse(vm.CanCloseTab);
		vm.CanCloseTab = true;
		Assert.IsTrue(vm.CanCloseTab);
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

		vm.ClearInput();
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
}
