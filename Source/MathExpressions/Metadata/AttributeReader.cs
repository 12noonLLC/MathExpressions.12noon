using System;
using System.ComponentModel;
using System.Reflection;

namespace MathExpressions.Metadata
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
			string result = instance?.ToString() ?? throw new ArgumentNullException(nameof(instance));

			Type type = instance.GetType();
			MemberInfo[] members = type.GetMember(result) ?? Array.Empty<MemberInfo>();
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
			DescriptionAttribute? attr = GetAttribute<DescriptionAttribute>(info);
			return (attr is null) ? info.Name : attr.Description;
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
			string result = instance?.ToString() ?? throw new ArgumentNullException(nameof(instance));

			Type type = instance.GetType();
			MemberInfo[] members = type.GetMember(result) ?? Array.Empty<MemberInfo>();
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
			AbbreviationAttribute? attr = GetAttribute<AbbreviationAttribute>(info);
			return (attr is null) ? info.Name : attr.Text;
		}


		/// <summary>
		/// Return the instance of the attribute with the type T on the passed member.
		/// </summary>
		/// <typeparam name="T">The type of the attribute to return.</typeparam>
		/// <param name="info">The instance info look for the attribute.</param>
		/// <returns>The instance of the member's attribute or null.</returns>
		private static T? GetAttribute<T>(MemberInfo info) where T : class
		{
			if (info is null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			object[] attributes = info.GetCustomAttributes(typeof(T), false) ?? Array.Empty<object>();
			if (attributes.Length == 0)
			{
				return null;
			}

			T? attribute = attributes[0] as T;
			return attribute;
		}
	}
}
