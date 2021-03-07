namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Indicates whether the state machine is a loop or not
		/// </summary>
		public bool IsLoop {
			get {
				foreach (var fa in Descendants)
					if (fa == this)
						return true;
				return false;
			}
		}
	}
}
