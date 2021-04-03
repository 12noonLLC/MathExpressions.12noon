using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CalculateX.UnitTests
{
	[TestClass]
	public class StoreStringsOrderedTest
	{
		private const string Tag = "store-test";
		private readonly Shared.StoreStringsOrdered Store = new(Tag);

		[ClassInitialize]
		public static void ClassSetup(TestContext testContext)
		{
		}

		[ClassCleanup]
		public static void ClassTeardown()
		{
			Shared.MyStorage.Delete(Tag);
		}


		[TestInitialize]
		public void TestSetup()
		{
			Assert.IsTrue(Store.IsEmpty());

			var l = Store.Get();
			Assert.AreEqual(0, l.Count);

			Store.Add("Value 1");
			Store.Add("Value 2");
			Store.Add("Value 3");

			l = Store.Get();
			Assert.AreEqual(3, l.Count);
			Assert.AreEqual(l.Skip(0).First(), "Value 1");
			Assert.AreEqual(l.Skip(1).First(), "Value 2");
			Assert.AreEqual(l.Skip(2).First(), "Value 3");
		}

		[TestCleanup]
		public void TestTeardown()
		{
		}


		[TestMethod]
		public void TestClear()
		{
			var l = Store.Get();
			Assert.AreEqual(3, l.Count);

			Store.Clear();
			Assert.IsTrue(Store.IsEmpty());

			l = Store.Get();
			Assert.AreEqual(0, l.Count);
		}

		[TestMethod]
		public void TestAdd()
		{
			var l = Store.Get();
			Assert.AreEqual(3, l.Count);

			Store.Add("Value 2");

			l = Store.Get();
			Assert.AreEqual(4, l.Count);
			Assert.AreEqual(l.Skip(3).First(), "Value 2");
		}

		[TestMethod]
		public void TestSaveAndLoad()
		{
			Assert.AreEqual(3, Store.Get().Count);
			Store.Save();
			Assert.AreEqual(3, Store.Get().Count);

			Store.Clear();
			Assert.IsTrue(Store.IsEmpty());
			Assert.AreEqual(0, Store.Get().Count);

			Store.Load();
			var l = Store.Get();
			Assert.AreEqual(3, l.Count);
			Assert.AreEqual(l.Skip(0).First(), "Value 1");
			Assert.AreEqual(l.Skip(1).First(), "Value 2");
			Assert.AreEqual(l.Skip(2).First(), "Value 3");
		}
	}
}
