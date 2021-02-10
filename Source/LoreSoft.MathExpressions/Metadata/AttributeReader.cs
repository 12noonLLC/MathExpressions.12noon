using System;
using System.ComponentModel;
using System.Reflection;

namespace LoreSoft.MathExpressions.Metadata
{
	/// <summary>
	/// A class to read attributes from type members.
	/// </summary>
	public static class AttributeReader
	{
		/// <summary>
		/// Gets the description from the <see cref="DescriptionAttribute"/> on an enum.
		/// </summary>
		/// <typeparam name="T">An enum type.</typeparam>
		/// <param name="instance">The value to get the description from.</param>
		/// <returns>The <see cref="DescriptionAttribute.Description"/> or the name of the instance.</returns>
		/// <seealso cref="DescriptionAttribute"/>
		public static string GetDescription<T>(T instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			string result = instance.ToString();

			Type type = instance.GetType();
			MemberInfo[] members = type.GetMember(result) ?? new MemberInfo[0];
			if (members.Length == 0)
			{
				return result;
			}

			return GetDescription(members[0]);
		}

		/// <summary>
		/// Gets the description from the <see cref="DescriptionAttribute"/> on a MemberInfo.
		/// </summary>
		/// <param name="info">The member info to look for the description.</param>
		/// <returns>The <see cref="DescriptionAttribute.Description"/> or the name of the member.</returns>
		/// <seealso cref="DescriptionAttribute"/>
		public static string GetDescription(MemberInfo info)
		{
			if (info is null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			string result = info.Name;

			object[] attributes = info.GetCustomAttributes(typeof(DescriptionAttribute), false) ?? new object[0];
			if (attributes.Length == 0)
			{
				return result;
			}

			DescriptionAttribute description = attributes[0] as DescriptionAttribute;
			if (String.IsNullOrEmpty(description?.Description))
			{
				return result;
			}

			return description.Description;
		}

		/// <summary>
		/// Gets the abbreviation from the <see cref="AbbreviationAttribute"/> on an enum.
		/// </summary>
		/// <typeparam name="T">An enum type.</typeparam>
		/// <param name="instance">The enum to get the abbreviation from.</param>
		/// <returns>The <see cref="AbbreviationAttribute.Text"/> or the name of the memeber.</returns>
		/// <seealso cref="AbbreviationAttribute"/>
		public static string GetAbbreviation<T>(T instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			string result = instance.ToString();

			Type type = instance.GetType();
			MemberInfo[] members = type.GetMember(result) ?? new MemberInfo[0];
			if (members.Length == 0)
			{
				return result;
			}

			return GetAbbreviation(members[0]);
		}

		/// <summary>
		/// Gets the abbreviation from the <see cref="AbbreviationAttribute"/> on a instance.
		/// </summary>
		/// <param name="info">The instance info look for the abbreviation.</param>
		/// <returns>The <see cref="AbbreviationAttribute.Text"/> or the name of the instance.</returns>
		/// <seealso cref="AbbreviationAttribute"/>
		public static string GetAbbreviation(MemberInfo info)
		{
			if (info is null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			string result = info.Name;

			object[] attributes = info.GetCustomAttributes(typeof(AbbreviationAttribute), false) ?? new object[0];
			if (attributes.Length == 0)
			{
				return result;
			}

			AbbreviationAttribute abbreviation = attributes[0] as AbbreviationAttribute;
			if (String.IsNullOrEmpty(abbreviation?.Text))
			{
				return result;
			}

			return abbreviation.Text;
		}
	}
}
