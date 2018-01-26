// TODO TODO TODO TODO
// remove that "2" from identifier
// Regex for detecting fast trains. Or should RR pass???


namespace ZSSK_cheaper_tickets_cons
{
	public class Passage
	{
		public string From { get; }
		public string To { get; }
		public bool IsFree { get; }

		public Passage(string from, string to, bool isFree)
		{
			From = from;
			To = to;
			IsFree = isFree;
		}
	}
}
