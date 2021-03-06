using System.Collections.Generic;
namespace RE
{
	/// <summary>
	/// Represents a single state in a character based finite state machine.
	/// </summary>
	/// <typeparam name="TAccept">The type of the accepting symbols</typeparam>
	public partial class CharFA<TAccept>
	{
		// we use a specialized dictionary class both for performance and
		// to preserve the order of the input transitions
		/// <summary>
		/// Indicates the input transitions. These are the states that will be transitioned to on the specified input key.
		/// </summary>
		public InputTransitionsDictionary InputTransitions { get; }
			= new InputTransitionsDictionary();
		/// <summary>
		/// Indicates the epsilon transitions. These are the states that are transitioned to without consuming input.
		/// </summary>
		public IList<CharFA<TAccept>> EpsilonTransitions { get; }
			= new List<CharFA<TAccept>>();
		/// <summary>
		/// Indicates whether or not this is an accepting state. When an accepting state is landed on, this indicates a potential match.
		/// </summary>
		public bool IsAccepting { get; set; } = false;
		/// <summary>
		/// The symbol to associate with this accepting state. Upon accepting a match, the specified symbol is returned which can identify it.
		/// </summary>
		public TAccept AcceptSymbol { get; set; } = default(TAccept);
		/// <summary>
		/// Indicates a user-defined value to associate with this state
		/// </summary>
		public object Tag { get; set; } = null;
		/// <summary>
		/// Constructs a new instance with the specified accepting value and accept symbol.
		/// </summary>
		/// <param name="isAccepting">Indicates whether or not the state is accepting</param>
		/// <param name="acceptSymbol">Indicates the associated symbol to be used when accepting.</param>
		public CharFA(bool isAccepting,TAccept acceptSymbol=default(TAccept))
		{
			IsAccepting = isAccepting;
			AcceptSymbol = acceptSymbol;
		}
		/// <summary>
		/// Constructs a new non-accepting state
		/// </summary>
		public CharFA()
		{

		}
	}
}
