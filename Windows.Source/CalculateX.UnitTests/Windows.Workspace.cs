using CalculateX.Models;
using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CalculateX.UnitTests;

[TestClass]
public class TestWorkspace
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
	public void TestConstructorIdName()
	{
		Workspace workspace1 = new("id1", "Name 1");
		Workspace workspace2 = new("id2", "Name 2");
		Workspace workspace3 = new("Name 3");

		Assert.AreEqual("id1", workspace1.ID);
		Assert.AreEqual("id2", workspace2.ID);

		Assert.AreEqual("Name 1", workspace1.Name);
		Assert.AreEqual("Name 2", workspace2.Name);
		Assert.AreEqual("Name 3", workspace3.Name);

		Assert.AreNotEqual(new DateTimeOffset(), workspace1.LastModifiedUTC);
		Assert.AreNotEqual(new DateTimeOffset(), workspace2.LastModifiedUTC);
		Assert.AreNotEqual(new DateTimeOffset(), workspace3.LastModifiedUTC);

		Assert.IsFalse(workspace1.History.Any());
		Assert.IsFalse(workspace2.History.Any());
		Assert.IsFalse(workspace3.History.Any());

		Assert.AreEqual(3, workspace1.Variables.Count);
		Assert.IsTrue(workspace1.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace1.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace1.Variables.ContainsKey("e"));

		Assert.AreEqual(3, workspace2.Variables.Count);
		Assert.IsTrue(workspace2.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace2.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace2.Variables.ContainsKey("e"));

		Assert.AreEqual(3, workspace3.Variables.Count);
		Assert.IsTrue(workspace3.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace3.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace3.Variables.ContainsKey("e"));
	}

	[TestMethod]
	public void TestConstructorName()
	{
		Workspace workspace = new("Name 1");

		Assert.AreNotEqual(Guid.Empty.ToString(), workspace.ID);
		Assert.AreEqual("Name 1", workspace.Name);

		Assert.IsFalse(workspace.History.Any());

		Assert.AreEqual(3, workspace.Variables.Count);
		Assert.IsTrue(workspace.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace.Variables.ContainsKey("e"));
	}

	[TestMethod]
	public void TestCompareWorkspaceEquals()
	{
		Workspace wRandom1 = new("Test1");
		Workspace wRandom2 = new("Test2");

		Assert.AreNotEqual(wRandom1, wRandom2);
		Assert.IsFalse(wRandom1 == wRandom2);
		Assert.IsTrue(wRandom1 != wRandom2);


		Workspace w1 = new("id1", "Test1");
		Workspace w2 = new("id2", "Test2");

		Assert.AreNotEqual(w1, w2);
		Assert.IsFalse(w1 == w2);
		Assert.IsTrue(w1 != w2);


		Workspace w3 = new("id1", "Test3");

		Assert.AreEqual(w1, w3);
		Assert.IsTrue(w1 == w3);
		Assert.IsFalse(w1 != w3);
	}

	[TestMethod]
	public void TestClone()
	{
		// Arrange
		Workspace workspace = new("id1", "Name 1");
		workspace.Evaluate("2+3");
		workspace.Evaluate("45*77");

		// Act
		Workspace clonedWorkspace = (Workspace)workspace.Clone();

		// Assert
		Assert.AreEqual(workspace.ID, clonedWorkspace.ID);
		Assert.IsTrue(workspace.Equals(clonedWorkspace));
		Assert.IsTrue(workspace == clonedWorkspace);
		Assert.IsFalse(workspace != clonedWorkspace);
		Assert.AreEqual(workspace.Name, clonedWorkspace.Name);
		Assert.AreEqual(workspace.History.Count, clonedWorkspace.History.Count);
		for (int i = 0; i < workspace.History.Count; i++)
		{
			Assert.AreEqual(workspace.History[i].Input, clonedWorkspace.History[i].Input);
			Assert.AreEqual(workspace.History[i].Result, clonedWorkspace.History[i].Result);
		}
		Assert.AreEqual(workspace.Variables.Count, clonedWorkspace.Variables.Count);
		foreach (var variable in workspace.Variables)
		{
			Assert.IsTrue(clonedWorkspace.Variables.ContainsKey(variable.Key));
			Assert.AreEqual(variable.Value, clonedWorkspace.Variables[variable.Key]);
		}
	}

	[TestMethod]
	public void TestCloneWorkspaceSameID1()
	{
		// Arrange
		Workspace workspace = new("123", "Test Workspace")
		{
			LastModifiedUTC = new(2024, 6, 14, 09, 24, 55, TimeSpan.Zero),
		};
		workspace.Evaluate("1 + 2");
		workspace.Evaluate("3 * 15");
		workspace.Evaluate("15 ^ 6");

		// Act
		Workspace clonedWorkspace = workspace.CloneWorkspace(idNew: null);

		// Assert
		Assert.AreEqual(workspace.ID, clonedWorkspace.ID, "Clone should have the same ID.");
		Assert.IsTrue(workspace.Equals(clonedWorkspace));
		Assert.IsTrue(workspace == clonedWorkspace);
		Assert.IsFalse(workspace != clonedWorkspace);
		Assert.AreEqual(workspace.Name, clonedWorkspace.Name);
		Assert.AreEqual(workspace.LastModifiedUTC, clonedWorkspace.LastModifiedUTC);
		CollectionAssert.AreEqual(workspace.History, clonedWorkspace.History);
		CollectionAssert.AreEqual(workspace.Variables, clonedWorkspace.Variables);
	}

	[TestMethod]
	public void TestCloneWorkspaceSameID2()
	{
		// Arrange
		Workspace workspace = new("id1", "Name 1");
		workspace.Evaluate("2+3");
		workspace.Evaluate("45*77");

		// Act
		Workspace clonedWorkspace = workspace.CloneWorkspace(idNew: null);

		// Assert
		Assert.AreEqual(workspace.ID, clonedWorkspace.ID);
		Assert.IsTrue(workspace.Equals(clonedWorkspace));
		Assert.IsTrue(workspace == clonedWorkspace);
		Assert.IsFalse(workspace != clonedWorkspace);
		Assert.AreEqual(workspace.Name, clonedWorkspace.Name);
		Assert.AreEqual(workspace.History.Count, clonedWorkspace.History.Count);
		for (int i = 0; i < workspace.History.Count; i++)
		{
			Assert.AreEqual(workspace.History[i].Input, clonedWorkspace.History[i].Input);
			Assert.AreEqual(workspace.History[i].Result, clonedWorkspace.History[i].Result);
		}
		Assert.AreEqual(workspace.Variables.Count, clonedWorkspace.Variables.Count);
		foreach (var variable in workspace.Variables)
		{
			Assert.IsTrue(clonedWorkspace.Variables.ContainsKey(variable.Key));
			Assert.AreEqual(variable.Value, clonedWorkspace.Variables[variable.Key]);
		}
	}

	[TestMethod]
	public void TestCloneWorkspaceNewID()
	{
		// Arrange
		Workspace workspace = new("id1", "Name 1");
		workspace.Evaluate("2+3");
		workspace.Evaluate("45*77");

		// Act
		Workspace clonedWorkspace = workspace.CloneWorkspace(Guid.NewGuid());

		// Assert
		Assert.AreNotEqual(workspace.ID, clonedWorkspace.ID);
		Assert.IsFalse(workspace.Equals(clonedWorkspace));
		Assert.IsFalse(workspace == clonedWorkspace);
		Assert.IsTrue(workspace != clonedWorkspace);
		Assert.AreEqual(workspace.Name, clonedWorkspace.Name);
		Assert.AreEqual(workspace.History.Count, clonedWorkspace.History.Count);
		for (int i = 0; i < workspace.History.Count; i++)
		{
			Assert.AreEqual(workspace.History[i].Input, clonedWorkspace.History[i].Input);
			Assert.AreEqual(workspace.History[i].Result, clonedWorkspace.History[i].Result);
		}
		Assert.AreEqual(workspace.Variables.Count, clonedWorkspace.Variables.Count);
		foreach (var variable in workspace.Variables)
		{
			Assert.IsTrue(clonedWorkspace.Variables.ContainsKey(variable.Key));
			Assert.AreEqual(variable.Value, clonedWorkspace.Variables[variable.Key]);
		}
	}

	[TestMethod]
	public void TestFromXML()
	{
		// Arrange
		XElement element = XElement.Parse("<workspace id=\"id1\" name=\"Name 1\" selected=\"false\"><inputs/></workspace>");

		// Act
		Workspace workspace = Workspace.FromXML(element);

		// Assert
		Assert.AreEqual("id1", workspace.ID);
		Assert.AreEqual("Name 1", workspace.Name);

		Assert.IsFalse(workspace.History.Any());

		Assert.AreEqual(3, workspace.Variables.Count);
		Assert.IsTrue(workspace.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace.Variables.ContainsKey("e"));
	}

	[TestMethod]
	public void TestToXML()
	{
		// Arrange
		Workspace workspace = new("id1", "Name 1");

		// Act
		XElement element = workspace.ToXML("id1");

		// Assert
		Assert.AreEqual("id1", element.Attribute("id")!.Value);
		Assert.AreEqual("Name 1", element.Attribute("name")!.Value);
		Assert.IsTrue((bool)element.Attribute("selected")!);

		Assert.IsFalse(element.Element("inputs")!.Descendants().Any());

		Assert.AreEqual(3, workspace.Variables.Count);
		Assert.IsTrue(workspace.Variables.ContainsKey(MathEvaluator.AnswerVariable));
		Assert.IsTrue(workspace.Variables.ContainsKey("pi"));
		Assert.IsTrue(workspace.Variables.ContainsKey("e"));
	}


	[TestMethod]
	public void TestEvaluateAndClear()
	{
		Workspace workspace = new("Workspace 3");

		workspace.Evaluate("2+3");
		Assert.AreEqual(1, workspace.History.Count);
		Assert.AreEqual("2+3", workspace.History[^1].Input);
		Assert.AreEqual("5", workspace.History[^1].Result);

		workspace.Evaluate("45*77");
		Assert.AreEqual(2, workspace.History.Count);
		Assert.AreEqual("45*77", workspace.History[^1].Input);
		Assert.AreEqual("3,465", workspace.History[^1].Result);

		workspace.ClearHistory();
		Assert.IsFalse(workspace.History.Any());
	}


	[TestMethod]
	public void TestVariables()
	{
		Workspace workspace = new("Test Variables");

		workspace.Evaluate("fish=52");
		Assert.AreEqual(1, workspace.History.Count);
		Assert.AreEqual("fish=52", workspace.History[0].Input);
		Assert.AreEqual("52", workspace.History[0].Result);
		Assert.IsFalse(workspace.History[0].IsCleared);
		Assert.IsFalse(workspace.History[0].IsError);
		Assert.IsTrue(workspace.Variables.ContainsKey("fish"));
		Assert.AreEqual(52, workspace.Variables["fish"]);

		workspace.Evaluate("fish");
		Assert.AreEqual(2, workspace.History.Count);
		Assert.AreEqual("fish", workspace.History[1].Input);
		Assert.AreEqual("52", workspace.History[1].Result);
		Assert.IsFalse(workspace.History[1].IsCleared);
		Assert.IsFalse(workspace.History[1].IsError);
		Assert.IsTrue(workspace.Variables.ContainsKey("fish"));
		Assert.AreEqual(52, workspace.Variables["fish"]);

		workspace.Evaluate("fish=");
		Assert.AreEqual(3, workspace.History.Count);
		Assert.AreEqual("fish", workspace.History[2].Input);
		Assert.AreEqual("fish=", workspace.History[2].GetInput());
		Assert.IsNull(workspace.History[2].Result);
		Assert.IsTrue(workspace.History[2].IsCleared);
		Assert.IsFalse(workspace.History[2].IsError);
		Assert.IsFalse(workspace.Variables.ContainsKey("fish"));
		Assert.ThrowsException<KeyNotFoundException>(() => workspace.Variables["fish"]);
	}
}
