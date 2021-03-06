using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace RE
{
	partial class CharFA<TAccept>
	{
		#region Type serialization
		static CodeExpression _SerializeArray(Array arr)
		{
			if (1 == arr.Rank && 0 == arr.GetLowerBound(0))
			{
				var result = new CodeArrayCreateExpression(arr.GetType());
				foreach (var elem in arr)
					result.Initializers.Add(_Serialize(elem));
				return result;
			}
			throw new NotSupportedException("Only SZArrays can be serialized to code.");
		}
		static CodeExpression _Serialize(object val)
		{
			if (null == val)
				return new CodePrimitiveExpression(null);
			if (val is char) // special case for unicode nonsense
			{
				// console likes to cook unicode characters
				// so we render them as ints cast to the character
				if (((char)val) > 0x7E)
					return new CodeCastExpression(typeof(char), new CodePrimitiveExpression((int)(char)val));
				return new CodePrimitiveExpression((char)val);
			}
			else
			if (val is bool ||
				val is string ||
				val is short ||
				val is ushort ||
				val is int ||
				val is uint ||
				val is ulong ||
				val is long ||
				val is byte ||
				val is sbyte ||
				val is float ||
				val is double ||
				val is decimal)
			{
				// TODO: mess with strings to make them console safe.
				return new CodePrimitiveExpression(val);
			}
			if (val is Array && 1 == ((Array)val).Rank && 0 == ((Array)val).GetLowerBound(0))
			{
				return _SerializeArray((Array)val);
			}
			var conv = TypeDescriptor.GetConverter(val);
			if (null != conv)
			{
				if (conv.CanConvertTo(typeof(InstanceDescriptor)))
				{
					var desc = conv.ConvertTo(val, typeof(InstanceDescriptor)) as InstanceDescriptor;
					if (!desc.IsComplete)
						throw new NotSupportedException(
							string.Format(
								"The type \"{0}\" could not be serialized.",
								val.GetType().FullName));
					var ctor = desc.MemberInfo as ConstructorInfo;
					if (null != ctor)
					{
						var result = new CodeObjectCreateExpression(ctor.DeclaringType);
						foreach (var arg in desc.Arguments)
							result.Parameters.Add(_Serialize(arg));
						return result;
					}
					throw new NotSupportedException(
						string.Format(
							"The instance descriptor for type \"{0}\" is not supported.",
							val.GetType().FullName));
				}
				else
				{
					// we special case for KeyValuePair types.
					var t = val.GetType();
					if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
					{
						// TODO: Find a workaround for the bug with VBCodeProvider
						// may need to modify the reference source
						var kvpType = new CodeTypeReference(typeof(KeyValuePair<,>));
						foreach (var arg in val.GetType().GetGenericArguments())
							kvpType.TypeArguments.Add(arg);
						var result = new CodeObjectCreateExpression(kvpType);
						for (int ic = kvpType.TypeArguments.Count, i = 0; i < ic; ++i)
						{
							var prop = val.GetType().GetProperty(0 == i ? "Key" : "Value");
							result.Parameters.Add(_Serialize(prop.GetValue(val)));
						}
						return result;
					}
					throw new NotSupportedException(
						string.Format("The type \"{0}\" could not be serialized.",
						val.GetType().FullName));
				}
			}
			else
				throw new NotSupportedException(
					string.Format(
						"The type \"{0}\" could not be serialized.",
						val.GetType().FullName));
		}
		#endregion
	}
}
