// TODO TODO TODO TODO
// remove that "2" from identifier
// Regex for detecting fast trains. Or should RR pass???


using System.Collections.Generic;

namespace ZSSK_cheaper_tickets_cons
{
	public class Train
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public List<Station> Stations { get; }
		public List<Passage> Passages { get; }

		public Train()
		{
			Name = "";
			ID = "";
			Stations = new List<Station>();
			Passages = new List<Passage>();
		}

		public void AddStation(Station station)
		{
			if (station == null)
			{
				throw new System.ArgumentNullException("Station is equal to null while trying to add to the train { 0}", Name);
			}

			Stations.Add(station);
		}
		
		public void AddPassage(Passage passage)
		{
			if (passage == null)
			{
				throw new System.ArgumentNullException("Passage is equal to null while trying to add to the train {0}", Name);
			}

			Passages.Add(passage);
		}
	}
}
