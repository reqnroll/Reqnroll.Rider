using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;

namespace RE
{
	/// <summary>
	/// This is an internal class that helps the code serializer know how to serialize DFA entries
	/// </summary>
	class CharDfaEntryConverter : TypeConverter
	{
		// we only need to convert to an InstanceDescriptor
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (typeof(InstanceDescriptor) == destinationType)
				return true;
			return base.CanConvertTo(context, destinationType);
		}
		// we return an InstanceDescriptor so the serializer can read it to figure out what code to generate
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (typeof(InstanceDescriptor) == destinationType)
			{
				// basically what we're doing is reporting that the constructor contains all the necessary
				// parameters for initializing an instance of this object in the specified state
				var dte = (CharDfaEntry)value;
				return new InstanceDescriptor(typeof(CharDfaEntry).GetConstructor(new Type[] { typeof(int), typeof(CharDfaTransitionEntry[]) }), new object[] { dte.AcceptSymbolId, dte.Transitions });
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
	/// <summary>
	/// Represents an entry in a DFA state table
	/// </summary>
	[TypeConverter(typeof(CharDfaEntryConverter))]
	public struct CharDfaEntry
	{
		/// <summary>
		/// Constructs a new instance of the DFA state table with the specified parameters
		/// </summary>
		/// <param name="acceptSymbolId">The symbolId to accept or -1 for non-accepting</param>
		/// <param name="transitions">The transition entries</param>
		public CharDfaEntry(int acceptSymbolId, CharDfaTransitionEntry[] transitions)
		{
			AcceptSymbolId = acceptSymbolId;
			Transitions = transitions;
		}
		/// <summary>
		/// Indicates the accept symbol's id or -1 for non-accepting
		/// </summary>
		public int AcceptSymbolId;
		/// <summary>
		/// Indicates the transition entries
		/// </summary>
		public CharDfaTransitionEntry[] Transitions;
	}
	/// <summary>
	/// This is an internal class that helps the code serializer serialize a DfaTransitionEntry
	/// </summary>
	class CharDfaTransitionEntryConverter : TypeConverter
	{
		// we only need to convert to an InstanceDescriptor
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (typeof(InstanceDescriptor) == destinationType)
				return true;
			return base.CanConvertTo(context, destinationType);
		}
		// report the constructor of the class so the serializer knows which call to serialize
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (typeof(InstanceDescriptor) == destinationType)
			{
				var dte = (CharDfaTransitionEntry)value;
				return new InstanceDescriptor(typeof(CharDfaTransitionEntry).GetConstructor(new Type[] { typeof(char[]), typeof(int) }), new object[] { dte.PackedRanges, dte.Destination });
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
	/// <summary>
	/// Indicates a transition entry in the DFA state table
	/// </summary>
	[TypeConverter(typeof(CharDfaTransitionEntryConverter))]
	public struct CharDfaTransitionEntry
	{
		/// <summary>
		/// Constructs a DFA transition entry with the specified parameters
		/// </summary>
		/// <param name="transitions">Packed character range pairs as a flat array</param>
		/// <param name="destination">The destination state id</param>
		public CharDfaTransitionEntry(char[] transitions, int destination)
		{
			PackedRanges = transitions;
			Destination = destination;
		}
		/// <summary>
		/// Indicates the packed range characters. Each range is specified by two array entries, first and last in that order.
		/// </summary>
		public char[] PackedRanges;
		/// <summary>
		/// Indicates the destination state id
		/// </summary>
		public int Destination;
	}
}
