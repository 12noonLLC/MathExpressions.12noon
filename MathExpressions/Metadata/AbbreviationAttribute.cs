using System;

namespace MathExpressions.Metadata;

/// <summary>
/// Specifies an abbreviation for a instance. 
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class AbbreviationAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AbbreviationAttribute"/> class.
	/// </summary>
	/// <param name="text">The abbreviation text.</param>
	public AbbreviationAttribute(string text)
	{
		_text = text;
	}

	private readonly string _text;

	/// <summary>
	/// Gets the abbreviation text.
	/// </summary>
	/// <value>The abbreviation text.</value>
	public string Text => _text;

	/// <summary>
	/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
	/// </returns>
	/// <filterPriority>2</filterPriority>
	public override string ToString() => _text;
}
