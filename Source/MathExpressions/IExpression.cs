namespace MathExpressions
{
	/// <summary>
	/// The interface used when running expressions
	/// </summary>
	public interface IExpression
	{
		/// <summary>Gets the number of arguments this expression uses.</summary>
		int ArgumentCount { get; }

		/// <summary>Gets or sets the evaluate delegate.</summary>
		MathEvaluate Evaluate { get; set; }
	}
}
