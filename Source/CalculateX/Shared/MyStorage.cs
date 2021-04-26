using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Linq;

namespace Shared
{
	/// <summary>
	/// This class saves and restores data in isolated storage.
	/// </summary>
	public static class MyStorage
	{
		/// <summary>
		/// These two methods write and read a collection of strings.
		/// </summary>
		public static void WriteStrings(string tag, IEnumerable<string> strings)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			using IsolatedStorageFileStream stm = new(tag, FileMode.Create, isf);
			using StreamWriter stmWriter = new(stm);

			strings.ToList().ForEach(s => stmWriter.WriteLine(s));

			//This calls Dispose, so we don't need to. stmWriter.Close();
			//This calls Dispose, so we don't need to. stm.Close();
		}

		public static List<string> ReadStrings(string tag)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (!isf.FileExists(tag))
			{
				return Enumerable.Empty<string>().ToList();
			}

			using IsolatedStorageFileStream stm = new(tag, FileMode.Open, isf);
			if (stm is null)
			{
				return Enumerable.Empty<string>().ToList();
			}

			using StreamReader stmReader = new(stm);

			List<string> strings = new();

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


		/// <summary>
		/// These two methods write and read an XML element.
		/// </summary>
		/// <example>
		/// XDocument xdoc = ...
		/// WriteElement(xdoc.Root);
		/// xdoc = new XDocument(MyStorage.ReadElement("fish"));
		///
		/// MyStorage.WriteElement("fish", new XElement("parent", "some value"));
		/// XElement position = MyStorage.ReadElement("fish");
		/// </example>
		/// <param name="tag">unique name for isolated storage</param>
		/// <param name="xml">XML element to save to storage</param>
		public static void WriteElement(string tag, XElement xml)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			using IsolatedStorageFileStream stm = new(tag, FileMode.Create, isf);
			using StreamWriter stmWriter = new(stm);

			xml.Save(stmWriter);

			//This calls Dispose, so we don't need to. stmWriter.Close();
			//This calls Dispose, so we don't need to. stm.Close();
		}


		/// <param name="tag">unique name for isolated storage</param>
		/// <returns>XML element that was read from storage; null if nothing is found</returns>
		public static XElement? ReadElement(string tag)
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (!isf.FileExists(tag))
			{
				return null;
			}

			using IsolatedStorageFileStream stm = new(tag, FileMode.Open, isf);
			if (stm is null)
			{
				return null;
			}

			using StreamReader stmReader = new(stm);

			// If this hasn't been created yet, EOS is true.
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
