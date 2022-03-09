using System.Collections.Generic;
using System.Linq;

namespace CalculateX;

public class CircularHistory
{
	public bool IsEmpty => !Entries.Any();

	private List<string> Entries = new();  // oldest entries at beginning; most recent at the end
	private int IndexRecent = -1;          // index to previous history entry


	public CircularHistory()
	{

	}

	public void Reset()
	{
		Entries.Clear();
		IndexRecent = -1;
	}


	public void AddNewEntry(string entry)
	{
		// Prevent duplicate history entries.
		// If this entry is already in the history, move it to the end.
		if (Entries.Contains(entry))
		{
			Entries.Remove(entry);
		}

		Entries.Add(entry);

		// Reset history index to MRU entry
		IndexRecent = Entries.Count - 1;
	}


	public string PreviousEntry(string currentEntry)
	{
		if (IndexRecent == -1)
		{
			return string.Empty;
		}

		string entry = Entries[IndexRecent];

		// if the history entry is the same as the current entry, skip it.
		if (entry == currentEntry)
		{
			IndexRecent = (IndexRecent == 0) ? Entries.Count - 1 : IndexRecent - 1;
			entry = Entries[IndexRecent];
		}
		IndexRecent = (IndexRecent == 0) ? Entries.Count - 1 : IndexRecent - 1;

		return entry;
	}

	public string NextEntry(string currentEntry)
	{
		if (IndexRecent == -1)
		{
			return string.Empty;
		}

		// increment the index first because it points to the PREVIOUS entry.
		IndexRecent = (IndexRecent == Entries.Count - 1) ? 0 : IndexRecent + 1;
		string entry = Entries[IndexRecent];

		// if the history entry is the same as the current entry, skip it.
		if (entry == currentEntry)
		{
			IndexRecent = (IndexRecent == Entries.Count - 1) ? 0 : IndexRecent + 1;
			entry = Entries[IndexRecent];
		}

		return entry;
	}
}
