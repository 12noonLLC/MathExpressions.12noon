using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspaces
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
	}


	[TestMethod]
	public void TestManagement()
	{
		Models.Workspaces workspaces = new(StoragePath);

		Models.Workspace workspace1 = new("id1", "Name 1");
		Models.Workspace workspace2 = new("id2", "Name 2");
		Models.Workspace workspace3 = new("Name 3");

		Assert.AreEqual("id1", workspace1.ID);
		Assert.AreEqual("id2", workspace2.ID);

		Assert.AreEqual("Name 1", workspace1.Name);
		Assert.AreEqual("Name 2", workspace2.Name);
		Assert.AreEqual("Name 3", workspace3.Name);

		Assert.IsFalse(workspace1.History.Any());
		Assert.IsFalse(workspace2.History.Any());
		Assert.IsFalse(workspace3.History.Any());

		Assert.AreEqual(3, workspace1.Variables.Count);
		Assert.IsTrue(workspace1.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace1.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace1.Variables.ContainsKey("e"));

		workspace1.Evaluate("45*77");
		workspace1.Evaluate("fish = answer");
		workspace1.Evaluate("fish + 102 * 23");
		workspace1.Evaluate("fish");					// display variable value
		workspace1.Evaluate("fish=");					// clear variable => IsCleared = true
		workspace1.Evaluate("45*77x");            // error => IsError => true

		Assert.IsFalse(workspace1.History[0].IsCleared);
		Assert.IsFalse(workspace1.History[1].IsCleared);
		Assert.IsFalse(workspace1.History[2].IsCleared);
		Assert.IsFalse(workspace1.History[3].IsCleared);
		Assert.IsTrue (workspace1.History[4].IsCleared);
		Assert.IsFalse(workspace1.History[5].IsCleared);
		Assert.IsFalse(workspace1.History[0].IsError);
		Assert.IsFalse(workspace1.History[1].IsError);
		Assert.IsFalse(workspace1.History[2].IsError);
		Assert.IsFalse(workspace1.History[3].IsError);
		Assert.IsFalse(workspace1.History[4].IsError);
		Assert.IsTrue (workspace1.History[5].IsError);

		Assert.AreEqual(3, workspace2.Variables.Count);
		Assert.IsTrue(workspace2.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace2.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace2.Variables.ContainsKey("e"));

		workspace1.Evaluate("5 ^ 10 + 102 * 23");

		Assert.AreEqual(3, workspace3.Variables.Count);
		Assert.IsTrue(workspace3.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace3.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace3.Variables.ContainsKey("e"));

		workspace1.Evaluate("5 / 101 + 102 * 23");

		Assert.IsFalse(workspaces.TheWorkspaces.Any());

		workspaces.AddWorkspace(workspace1);
		Assert.AreEqual(1, workspaces.TheWorkspaces.Count);

		workspaces.AddWorkspace(workspace2);
		Assert.AreEqual(2, workspaces.TheWorkspaces.Count);

		workspaces.AddWorkspace(workspace3);
		Assert.AreEqual(3, workspaces.TheWorkspaces.Count);


		// Save workspaces
		workspaces.SaveWorkspaces(selectedWorkspaceID: workspace2.ID);

		// Load workspaces into another instance
		Models.Workspaces newWorkspaces = new(StoragePath);

		Assert.AreEqual(3, newWorkspaces.TheWorkspaces.Count);
		Assert.AreEqual(3, workspace3.Variables.Count);
#if !MAUI_UNITTESTS
		Assert.AreEqual(workspace2.ID, newWorkspaces.LoadedSelectedWorkspaceID);
#else
		Assert.AreEqual(workspace2.ID, newWorkspaces.SelectedWorkspaceID);
#endif

		Assert.IsNotNull(newWorkspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace1.ID));
		Assert.IsNotNull(newWorkspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace2.ID));
		Assert.IsNotNull(newWorkspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace3.ID));

		Models.Workspace? w1 = newWorkspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace1.ID);
		Assert.IsNotNull(w1);
		Assert.IsFalse(w1.History[0].IsCleared);
		Assert.IsFalse(w1.History[1].IsCleared);
		Assert.IsFalse(w1.History[2].IsCleared);
		Assert.IsFalse(w1.History[3].IsCleared);
		Assert.IsTrue (w1.History[4].IsCleared);
		Assert.IsFalse(w1.History[5].IsCleared);
		Assert.IsFalse(w1.History[0].IsError);
		Assert.IsFalse(w1.History[1].IsError);
		Assert.IsFalse(w1.History[2].IsError);
		Assert.IsFalse(w1.History[3].IsError);
		Assert.IsFalse(w1.History[4].IsError);
		Assert.IsTrue (w1.History[5].IsError);


		// Delete workspace
		newWorkspaces.DeleteWorkspace(workspace2.ID);
		Assert.AreEqual(2, newWorkspaces.TheWorkspaces.Count);

		newWorkspaces.DeleteWorkspace(workspace1.ID);
		Assert.AreEqual(1, newWorkspaces.TheWorkspaces.Count);

		newWorkspaces.DeleteWorkspace(workspace3.ID);
		Assert.AreEqual(0, newWorkspaces.TheWorkspaces.Count);
	}
}
