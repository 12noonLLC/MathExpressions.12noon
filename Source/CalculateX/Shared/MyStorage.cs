using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Linq;

namespace Shared
{
	/// <summary>
	/// This class saves and restores XML data in isolated storage.
	/// </summary>
	/// <example>
	///	MyStorage.Write("fish", new XElement("parent", "some value"));
	///	XElement position = MyStorage.Read("fish");
	/// </example>
	public static class MyStorage
	{
		/// <summary>
		/// These two methods write and read a collection of strings.
		/// </summary>
		public static void WriteStrings(string tag, IEnumerable<string> strings)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			using IsolatedStorageFileStream stm = new(tag, FileMode.OpenOrCreate, isf);
			using StreamWriter stmWriter = new(stm);

			strings.ToList().ForEach(s => stmWriter.WriteLine(s));

			//This calls Dispose, so we don't need to. stmWriter.Close();
			//This calls Dispose, so we don't need to. stm.Close();
		}

		public static List<string> ReadStrings(string tag)
		{
			List<string> strings = new();

			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (!isf.FileExists(tag))
			{
				return strings;
			}

			using IsolatedStorageFileStream stm = new(tag, FileMode.OpenOrCreate, isf);
			if (stm is null)
			{
				return strings;
			}

			using StreamReader stmReader = new(stm);

			// If this hasn't been created yet, EOS true.
			while (!stmReader.EndOfStream)
			{
				try
				{
					string? s = stmReader.ReadLine();
					if (s is not null)
					{
						strings.Add(s);
					}
				}
				catch (XmlException)
				{
					stm.SetLength(0);
					break;
				}
			}

			return strings;

			//This calls Dispose, so we don't need to. stmReader.Close();
			//This calls Dispose, so we don't need to. stm.Close();
			// REF: http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream
		}


		/// <example>
		/// XDocument xdoc = ...
		/// Write(xdoc.Root);
		/// xdoc = new XDocument(MyStorage.ReadElement("fish"));
		/// </example>
		public static void WriteElement(string tag, XElement xml)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			using IsolatedStorageFileStream stm = new(tag, FileMode.OpenOrCreate, isf);
			using StreamWriter stmWriter = new(stm);

			xml.Save(stmWriter);

			//This calls Dispose, so we don't need to. stmWriter.Close();
			//This calls Dispose, so we don't need to. stm.Close();
		}

		/// <summary>
		/// These two methods write and read an XML element.
		/// </summary>
		/// <example>
		/// MyStorage.Write("fish", new XElement("parent", "some value"));
		/// XElement position = MyStorage.Read("fish");
		/// </example>
		/// <param name="tag">unique name for isolated storage</param>
		/// <returns>XML element that was read from storage; null if nothing is found</returns>
		public static XElement? ReadElement(string tag)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (!isf.FileExists(tag))
			{
				return null;
			}

			using IsolatedStorageFileStream stm = new(tag, FileMode.OpenOrCreate, isf);
			if (stm is null)
			{
				return null;
			}

			using StreamReader stmReader = new(stm);

			// if this hasn't been created yet, just return
			if (stmReader.EndOfStream)
			{
				return null;
			}

			try
			{
				return XElement.Load(stmReader);
			}
			catch (XmlException)
			{
				stm.SetLength(0);
				return null;
			}

			//This calls Dispose, so we don't need to. stmReader.Close();
			//This calls Dispose, so we don't need to. stm.Close();
			// REF: http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream
		}


		public static void Delete(string tag)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (isf.FileExists(tag))
			{
				isf.DeleteFile(tag);
			}
		}
	}
}
