using System;
using System.Runtime.Serialization;

namespace MathExpressions;

/// <summary>
/// The exception that is thrown when there is an error parsing a math expression.
/// </summary>
[Serializable]
public class ParseException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
	public ParseException()
		 : base()
	{ }

	/// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
	/// <param name="message">The message.</param>
	public ParseException(string message)
		 : base(message)
	{ }

	/// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
	/// <param name="message">The message.</param>
	/// <param name="innerException">The inner exception.</param>
	public ParseException(string message, Exception innerException)
		 : base(message, innerException)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ParseException"/> class with serialized data.
	/// </summary>
	/// <remarks>
	/// .\MathExpressions\ParseException.cs(36,4,36,25):
	/// warning SYSLIB0051: 'Exception.Exception(SerializationInfo, StreamingContext)' is obsolete:
	/// 'This API supports obsolete formatter-based serialization. It should not be called or extended by application code.'
	/// (https://aka.ms/dotnet-warnings/SYSLIB0051)
	/// 
	/// </remarks>
	/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
	//protected ParseException(SerializationInfo info, StreamingContext context)
	//	 : base(info, context)
	//{ }
}
