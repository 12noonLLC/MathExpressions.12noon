using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspaces
{
	/// https://stackoverflow.com/questions/24249133/understanding-the-mstest-testcontext
	public required TestContext TestContext { get; set; }

	private const string StorageNameFormat1 = "CalculateX-Test{0}-{1}.xml";

	public static string StoragePath { get; private set; } = string.Empty;


	[ClassInitialize]
	public static void ClassSetup(TestContext _)
	{
		/*
				testContext.TestRunResultsDirectory
				"C:\\OneDrive\\Development\\12noon\\CalculateX\\TestResults\\Deploy_Stefan 2022-06-02 22_58_01"
		 */
		//StoragePath = Path.Combine(testContext.TestRunResultsDirectory!, string.Format(CultureInfo.InvariantCulture, StorageNameFormat1, testContext.FullyQualifiedTestClassName));

		// Clean up after a previous failed test.
		//File.Delete(StoragePath);
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
		File.Delete(StoragePath);
	}


	[TestInitialize]
	public void TestSetup()
	{
		/*
				testContext.TestRunResultsDirectory
				"C:\\OneDrive\\Development\\12noon\\CalculateX\\TestResults\\Deploy_Stefan 2022-06-02 22_58_01"
		 */
		StoragePath = Path.Combine(TestContext.TestRunResultsDirectory!, string.Format(CultureInfo.InvariantCulture, StorageNameFormat1, TestContext.FullyQualifiedTestClassName, TestContext.TestName));
	}

	[TestCleanup]
	public void TestTeardown()
	{
		// Clean up after a previous test.
		File.Delete(StoragePath);
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
		Assert.AreEqual(workspace2.ID, newWorkspaces.LoadedSelectedWorkspaceID);

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

	[TestMethod]
	public void TestConstructorXDocument()
	{
		// Arrange
		XDocument xdoc = new(
			new XElement("calculatex",
				new XAttribute("last-synchronized", "2024-12-16T08:20:44.0000000+00:00"),
				new XElement("workspaces",
					new XElement("workspace",
						new XAttribute("id", "id1"),
						new XAttribute("name", "Name 1"),
						new XAttribute("last-modified", "2024-11-12T16:07:04.0000000+00:00"),
						new XAttribute("selected", false),
						new XElement("inputs")
					),
					new XElement("workspace",
						new XAttribute("id", "id2"),
						new XAttribute("name", "Name 2"),
						new XAttribute("last-modified", "2024-08-04T05:12:09.0000000+00:00"),
						new XAttribute("selected", true),
						new XElement("inputs")
					)
				)
			)
		);
		string tempFilePath = Path.GetTempFileName();
		xdoc.Save(tempFilePath);

		// Act
		Models.Workspaces workspaces = new(tempFilePath);
		File.Delete(tempFilePath);
		Assert.AreEqual(new DateTimeOffset(2024, 12, 16, 08, 20, 44, TimeSpan.Zero), workspaces.LastSynchronizedUTC);

		// Assert
		Assert.AreEqual(2, workspaces.TheWorkspaces.Count);
		Models.Workspace? workspace1 = workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == "id1");
		Models.Workspace? workspace2 = workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == "id2");
		Assert.IsNotNull(workspace1);
		Assert.IsNotNull(workspace2);

		Assert.AreEqual("id2", workspaces.LoadedSelectedWorkspaceID);

		Assert.AreEqual("id1", workspace1.ID);
		Assert.AreEqual("Name 1", workspace1.Name);
		Assert.AreEqual(new DateTimeOffset(2024, 11, 12, 16, 07, 04, TimeSpan.Zero), workspace1.LastModifiedUTC);
		Assert.AreEqual(3, workspace1.Variables.Count);
		Assert.IsTrue(workspace1.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace1.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace1.Variables.ContainsKey("e"));

		Assert.AreEqual("id2", workspace2.ID);
		Assert.AreEqual("Name 2", workspace2.Name);
		Assert.AreEqual(new DateTimeOffset(2024, 08, 04, 05, 12, 09, TimeSpan.Zero), workspace2.LastModifiedUTC);
		Assert.AreEqual(3, workspace2.Variables.Count);
		Assert.IsTrue(workspace2.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace2.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace2.Variables.ContainsKey("e"));
	}

	[TestMethod]
	public void TestWorkspacesToXML()
	{
		// Arrange
		Models.Workspaces workspaces = new(StoragePath);
		Models.Workspace workspace1 = new("id1", "Name 1");
		Models.Workspace workspace2 = new("id2", "Name 2");
		workspaces.AddWorkspace(workspace1);
		workspaces.AddWorkspace(workspace2);

		// Act
		XDocument xdoc = workspaces.ToXML(selectedWorkspaceID: workspace1.ID);

		// Assert
		Assert.IsNotNull(xdoc);
		Assert.AreEqual(2, xdoc.Root!.Element("workspaces")!.Elements("workspace").Count());
		Assert.IsNotNull(xdoc.Root.Element("workspaces")!.Elements("workspace").SingleOrDefault(e => e.Attribute("id")?.Value == "id1"));
		Assert.IsNotNull(xdoc.Root.Element("workspaces")!.Elements("workspace").SingleOrDefault(e => e.Attribute("id")?.Value == "id2"));
	}

	[TestMethod]
	public void TestDeleteWorkspace()
	{
		// Arrange
		Models.Workspaces workspaces = new();
		Models.Workspace workspace1 = new("id1", "Name 1");
		Models.Workspace workspace2 = new("id2", "Name 2");
		workspaces.AddWorkspace(workspace1);
		workspaces.AddWorkspace(workspace2);

		// Act
		workspaces.DeleteWorkspace(workspace1.ID);

		// Assert
		Assert.AreEqual(1, workspaces.TheWorkspaces.Count);
		Assert.IsNull(workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace1.ID));
		Assert.IsNotNull(workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == workspace2.ID));
	}

	[TestMethod]
	public void TestConstructorXDocumentOldFormat()
	{
		const string SELECTED_ID = "id1";

		// Arrange
		XDocument xdoc = new(
			new XElement("workspaces",
				new XElement("workspace",
					new XAttribute("id", "id1"),
					new XAttribute("name", "Name 1"),
					new XAttribute("last-modified", "2024-11-12T16:07:04.0000000+00:00"),
					new XAttribute("selected", false),
					new XElement("inputs")
				),
				new XElement("workspace",
					new XAttribute("id", "id2"),
					new XAttribute("name", "Name 2"),
					new XAttribute("last-modified", "2024-08-04T05:12:09.0000000+00:00"),
					new XAttribute("selected", true),
					new XElement("inputs")
				)
			)
		);
		string tempFilePath = Path.GetTempFileName();
		xdoc.Save(tempFilePath);
		XDocument xdocTarget = new(
			new XElement("calculatex",
				new XAttribute("last-synchronized", DateTimeOffset.UtcNow.ToString("o")),
				new XElement("workspaces",
					new XElement("workspace",
						new XAttribute("id", "id1"),
						new XAttribute("name", "Name 1"),
						new XAttribute("last-modified", "2024-11-12T16:07:04.0000000+00:00"),
						new XAttribute("selected", false),
						new XElement("inputs")
					),
					new XElement("workspace",
						new XAttribute("id", "id2"),
						new XAttribute("name", "Name 2"),
						new XAttribute("last-modified", "2024-08-04T05:12:09.0000000+00:00"),
						new XAttribute("selected", true),
						new XElement("inputs")
					)
				)
			)
		);
		string tempFilePathTarget = Path.GetTempFileName();
		xdocTarget.Save(tempFilePathTarget);

		// Act
		Models.Workspaces workspaces = new(tempFilePath);
		File.Delete(tempFilePath);
		long timeDiff = Math.Abs(workspaces.LastSynchronizedUTC.Subtract(DateTimeOffset.UtcNow).Ticks);
		Assert.IsTrue((timeDiff >= 0) && (timeDiff <= TimeSpan.FromSeconds(1).Ticks));

		Models.Workspaces workspacesTarget = new(tempFilePathTarget)
		{
			LastSynchronizedUTC = workspaces.LastSynchronizedUTC,
		};
		File.Delete(tempFilePathTarget);
		timeDiff = Math.Abs(workspacesTarget.LastSynchronizedUTC.Subtract(DateTimeOffset.UtcNow).Ticks);
		Assert.IsTrue((timeDiff >= 0) && (timeDiff <= TimeSpan.FromSeconds(1).Ticks));

		// Assert
		Assert.AreEqual(2, workspaces.TheWorkspaces.Count);
		Models.Workspace? workspace1 = workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == "id1");
		Models.Workspace? workspace2 = workspaces.TheWorkspaces.SingleOrDefault(w => w.ID == "id2");
		Assert.IsNotNull(workspace1);
		Assert.IsNotNull(workspace2);

		Assert.AreEqual("id2", workspaces.LoadedSelectedWorkspaceID);

		Assert.AreEqual("id1", workspace1.ID);
		Assert.AreEqual("Name 1", workspace1.Name);
		Assert.AreEqual(new DateTimeOffset(2024, 11, 12, 16, 07, 04, TimeSpan.Zero), workspace1.LastModifiedUTC);
		Assert.AreEqual(3, workspace1.Variables.Count);
		Assert.IsTrue(workspace1.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace1.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace1.Variables.ContainsKey("e"));

		Assert.AreEqual("id2", workspace2.ID);
		Assert.AreEqual("Name 2", workspace2.Name);
		Assert.AreEqual(new DateTimeOffset(2024, 08, 04, 05, 12, 09, TimeSpan.Zero), workspace2.LastModifiedUTC);
		Assert.AreEqual(3, workspace2.Variables.Count);
		Assert.IsTrue(workspace2.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace2.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace2.Variables.ContainsKey("e"));

		Assert.AreEqual(workspacesTarget.ToXML(SELECTED_ID).ToString(), workspaces.ToXML(SELECTED_ID).ToString());
	}

	[TestMethod]
	public void TestLoadWorkspaces()
	{
		// Arrange
		XDocument xdoc = new(
			new XElement("workspaces",
				new XElement("workspace",
					new XAttribute("id", "id1"),
					new XAttribute("name", "Name 1"),
					new XAttribute("last-modified", "2024-11-12T16:07:04.0000000+00:00"),
					new XAttribute("selected", false),
					new XElement("inputs")
				),
				new XElement("workspace",
					new XAttribute("id", "id2"),
					new XAttribute("name", "Name 2"),
					new XAttribute("last-modified", "2024-08-04T05:12:09.0000000+00:00"),
					new XAttribute("selected", true),
					new XElement("inputs")
				)
			)
		);
		xdoc.Save(StoragePath);

		// Act
		Models.Workspaces workspaces = new(StoragePath);

		// Assert
		long timeDiff = Math.Abs(workspaces.LastSynchronizedUTC.Subtract(DateTimeOffset.UtcNow).Ticks);
		Assert.IsTrue((timeDiff >= 0) && (timeDiff <= TimeSpan.FromSeconds(1).Ticks));
	}

	[TestMethod]
	public void TestFileConversion()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";

		XDocument xdocThis = XDocument.Parse(
"""
<?xml version="1.0" encoding="utf-8"?>
<workspaces>
	<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
		<inputs>
		<key ordinal="1">34.5 (23 + 1.5) / 2</key>
		<key ordinal="2">sqrt(4)</key>
		<key ordinal="3">cos(45pi / 180)</key>
		</inputs>
	</workspace>
	<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
		<inputs>
		<key ordinal="1">4+5*3</key>
		<key ordinal="2">(4+5)*3</key>
		<key ordinal="3">sqrt((4+5)*3)</key>
		</inputs>
	</workspace>
</workspaces>
"""
		);
		string tempFilePathThis = Path.GetTempFileName();
		xdocThis.Save(tempFilePathThis);
		Models.Workspaces workspacesThis = new(tempFilePathThis)
		{
			LastSynchronizedUTC = new(2024, 12, 16, 08, 20, 44, TimeSpan.Zero),
		};
		File.Delete(tempFilePathThis);

		XDocument xdocTarget = XDocument.Parse(
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex last-synchronized="2024-12-16T08:20:44.0000000+00:00">
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
			<key ordinal="1">34.5 (23 + 1.5) / 2</key>
			<key ordinal="2">sqrt(4)</key>
			<key ordinal="3">cos(45pi / 180)</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
			<key ordinal="1">4+5*3</key>
			<key ordinal="2">(4+5)*3</key>
			<key ordinal="3">sqrt((4+5)*3)</key>
			</inputs>
		</workspace>
	</workspaces>
	<deleted-workspaces />
</calculatex>
"""
		);
		string tempFilePathTarget = Path.GetTempFileName();
		xdocTarget.Save(tempFilePathTarget);
		Models.Workspaces workspacesTarget = new(tempFilePathTarget)
		{
			LastSynchronizedUTC = workspacesThis.LastSynchronizedUTC,
		};
		File.Delete(tempFilePathTarget);

		Assert.AreEqual(xdocTarget.ToString(), workspacesThis.ToXML(SELECTED_ID).ToString());
		Assert.AreEqual(workspacesTarget.ToXML(SELECTED_ID).ToString(), workspacesThis.ToXML(SELECTED_ID).ToString());
	}
}
