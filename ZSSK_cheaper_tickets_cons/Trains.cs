using System.Collections.Generic;

// TODO TODO TODO TODO
// remove that "2" from identifier
// Regex for detecting fast trains. Or should RR pass???


namespace ZSSK_cheaper_tickets_cons
{
	public class Trains
	{
		List<Train> _trainsList { get; set; }
		public string JSFViewState { get; }

		public Trains(string jsfViewState)
		{
			JSFViewState = jsfViewState;
			_trainsList = new List<Train>();
		}

		public void AddTrain(Train train)
		{
			_trainsList.Add(train);
		}

		public List<Train> GetTrains()
		{
			return _trainsList;
		}
	}
}
