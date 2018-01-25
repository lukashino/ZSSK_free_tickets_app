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


		public Train()
		{
			Name = "";
			ID = "";
			Stations = new List<Station>();
		}

		public void AddStation(Station station)
		{
			Stations.Add(station);
		}

	}
}
