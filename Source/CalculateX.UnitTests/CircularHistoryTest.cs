using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CalculateX.UnitTests;

[TestClass]
public class CircularHistoryTest
{
	private readonly CircularHistory History = new();

	[ClassInitialize]
	public static void ClassSetup(TestContext testContext)
	{
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
	}


	[TestInitialize]
	public void TestSetup()
	{
		Assert.IsTrue(History.IsEmpty);

		History.AddNewEntry("11");
		History.AddNewEntry("22");
		History.AddNewEntry("33");
		History.AddNewEntry("44");
	}

	[TestCleanup]
	public void TestTeardown()
	{
	}


	[TestMethod]
	public void TestReset()
	{
		Assert.IsFalse(History.IsEmpty);
		History.Reset();
		Assert.IsTrue(History.IsEmpty);
	}


	[TestMethod]
	public void TestPrevious()
	{
		string entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("11", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("44", entry);
	}

	[TestMethod]
	public void TestPreviousAndAdd()
	{
		string entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);

		History.AddNewEntry("55");

		entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("55", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("11", entry);
	}

	[TestMethod]
	public void TestPreviousAndSelect()
	{
		string entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);

		History.AddNewEntry(entry);

		entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("22", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("11", entry);
	}


	[TestMethod]
	public void TestNext()
	{
		string entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("11", entry);
	}

	[TestMethod]
	public void TestNextAndAdd()
	{
		string entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);

		History.AddNewEntry("55");

		entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("55", entry);
	}

	[TestMethod]
	public void TestNextAndSelect()
	{
		string entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);

		History.AddNewEntry(entry);

		entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);
	}


	[TestMethod]
	public void TestPreviousAndNext()
	{
		string entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("44", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);

		entry = History.NextEntry(entry);
		Assert.AreEqual("33", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("44", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("11", entry);
	}

	[TestMethod]
	public void TestNextAndPrevious()
	{
		string entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.NextEntry(entry);
		Assert.AreEqual("33", entry);

		entry = History.PreviousEntry(entry);
		Assert.AreEqual("22", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("11", entry);
		entry = History.PreviousEntry(entry);
		Assert.AreEqual("44", entry);
	}


	[TestMethod]
	public void TestNoEntry()
	{
		History.Reset();
		Assert.IsTrue(History.IsEmpty);

		string entry = History.NextEntry(String.Empty);
		Assert.IsTrue(String.IsNullOrEmpty(entry));
		entry = History.PreviousEntry(String.Empty);
		Assert.IsTrue(String.IsNullOrEmpty(entry));
	}

	[TestMethod]
	public void TestSingleEntry()
	{
		History.Reset();
		History.AddNewEntry("11");

		string entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.NextEntry(String.Empty);
		Assert.AreEqual("11", entry);

		entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("11", entry);
		entry = History.PreviousEntry(String.Empty);
		Assert.AreEqual("11", entry);
	}
}
