using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Shared
{
	/// <summary>
	/// Save an ordered list of strings in isolated storage.
	/// </summary>
	/// <example>
	///	<root>
	///		<key ordinal="1">value1</key>
	///		<key ordinal="2">value2</key>
	///		<key ordinal="3">value3</key>
	///	</root>
	/// </example>
	/// <seealso cref="MyStorage"/>
	public class StoreStringsOrdered
	{
		private readonly XDocument Record = new();
		private readonly string _tag;

		private const string NAME_ATTRIBUTE_KEY = "key";
		private const string NAME_ATTRIBUTE_ORDINAL = "ordinal";


		public StoreStringsOrdered(string tag)
		{
			Record.Add(new XElement(tag));
			_tag = tag;
		}


		public List<string> Get()
		{
			return 
				Record.Root!.Elements()
					.Select(e => (ordinal: (int)e.Attribute(NAME_ATTRIBUTE_ORDINAL)!, value: e.Value))
					.OrderBy(t => t.ordinal)
					.Select(t => t.value)
					.ToList();
		}


		public void Add(string value)
		{
			var n = Record.Root!.Elements().Count();
			Record.Root!.Add(new XElement(NAME_ATTRIBUTE_KEY, value, new XAttribute(NAME_ATTRIBUTE_ORDINAL, n + 1)));
		}


		public bool IsEmpty()
		{
			return !Record.Root!.Elements().Any();
		}


		public void Clear()
		{
			Record.Root!.RemoveAll();
		}


		public void Save()
		{
			Debug.Assert(Record.Root!.Name.LocalName == _tag);

			MyStorage.WriteElement(_tag, Record.Root);
		}


		public void Load()
		{
			XElement? store = MyStorage.ReadElement(_tag);
			if (store is null)
			{
				return;
			}

			Clear();
			Record.Root!.Add(store.Elements());
		}
	}
}
