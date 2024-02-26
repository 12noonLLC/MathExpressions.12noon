using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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
	public void TestConstructor()
	{
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
	public void TestEvaluateAndClear()
	{
		Models.Workspace workspace = new("Workspace 3");

		workspace.Evaluate("2+3");
		Assert.AreEqual(1, workspace.History.Count);
		Assert.AreEqual("2+3", workspace.History[workspace.History.Count - 1].Input);
		Assert.AreEqual("5", workspace.History[workspace.History.Count - 1].Result);

		workspace.Evaluate("45*77");
		Assert.AreEqual(2, workspace.History.Count);
		Assert.AreEqual("45*77", workspace.History[workspace.History.Count - 1].Input);
		Assert.AreEqual("3,465", workspace.History[workspace.History.Count - 1].Result);

		workspace.ClearHistory();
		Assert.IsFalse(workspace.History.Any());
	}


	[TestMethod]
	public void TestVariables()
	{
		Models.Workspace workspace = new("Test Variables");

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
