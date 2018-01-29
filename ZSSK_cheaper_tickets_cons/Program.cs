using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

// TODO TODO TODO TODO
// remove that "2" from identifier
// Regex for detecting fast trains. Or should RR pass???


namespace ZSSK_cheaper_tickets_cons
{
	public static class GlobalVar
	{
		public const bool LOGS = true;
		public const string FROM = "Košice";
		public const string TO = "Bratislava hl.st.";
		public const string TIME = "19:00";
		public const string DATE = "29.1.2018";
	}

	class Program
	{
		private static readonly HttpClientHandler handler = new HttpClientHandler();
		private static readonly HttpClient client = new HttpClient(handler);
		static void Main(string[] args)
		{

			RunAsync().Wait();
			Console.ReadLine();
		}

		static async Task<Dictionary<string, string>> GetCartParams(string JFSViewState)
		{
			var values = new Dictionary<string, string>();
			values.Add("cartForm", "cartForm");
			values.Add("javax.faces.ViewState", JFSViewState);
			values.Add("cartForm:j_idt63", "cartForm:j_idt63");
			return values;
		}

		/*
		 * Calling succesfull GetCart method can mean 2 things:
		 * 1. There is free ticket/s available to buy (can be alternatively checked by separate function (yes do it))
		 * 2. You can now remove it from your cart
		 */
		static async Task<String> GetCart(string page) // 
		{
			var htmlDoc = new HtmlDocument(); // remove that "2" from identifier
			htmlDoc.LoadHtml(page); // Loading up HTML

			var isInCart = htmlDoc.DocumentNode
								.SelectNodes("//div[@class='tmp-shopping-cart-total']/h2[@class='tmp-shopping-header']"); // this node has to be part of the cart page

			if (isInCart == null) // If it is not (== null) then end function 
				return null;


			var JSFViewState = htmlDoc.DocumentNode
								.SelectSingleNode("//input[@name='javax.faces.ViewState']")
								.Attributes["value"].Value;

			var values = await GetCartParams(JSFViewState);

			var content = new FormUrlEncodedContent(values);

			var response = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/personalData.xhtml", content); // got the page

			return await response.Content.ReadAsStringAsync();
		}

		static async Task<Dictionary<string, string>> EmptyCartParams(string JFSViewState)
		{
			var values = new Dictionary<string, string>()
			{
				{"shoppingCart", "shoppingCart"},
				{"javax.faces.ViewState", JFSViewState},
				{"shoppingCart:j_idt96:0:j_idt103", "shoppingCart:j_idt96:0:j_idt103"}
			};

			return values;
		}

		/*
		 * EmptyCart method should always after GetCart
		 * Method is used for removing checked tickets.
		 * Method DOES NOT empty the whole cart!
		 */
		static async Task<String> EmptyCart(string page)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(page); // Loading up HTML

			var JSFViewState = htmlDoc.DocumentNode
								.SelectSingleNode("//input[@name='javax.faces.ViewState']")
								.Attributes["value"].Value;

			var values = await EmptyCartParams(JSFViewState);
			var content = new FormUrlEncodedContent(values);
			var response = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/shoppingCart.xhtml", content); // got the page
			return await response.Content.ReadAsStringAsync();
		}

		static async Task<String> GetZSSKInfo(string from, string to, string date, string time)
		{
			var values = new Dictionary<string, string> // getting list of available trains
				{
					{ "lang", "sk" },
					{ "portal", "" },
					{ "from", from },
					{ "to", to },
					{ "via", "" },
					{ "date", date },
					{ "time", time },
					{ "departure", "true" },
					{ "wlw-checkbox_key%3A%7BpageFlow.inputParam.paramItemParams.direct.valueBoolean%7DOldValue", "false" },
					{ "maxChangeTrainCount", "5" },
					{ "minChangeTrainTime", "2" },
					{ "bed", "0" }
				};

			var content = new FormUrlEncodedContent(values);

			HttpResponseMessage response = null;
			try
			{
				response = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/connection/portal.xhtml", content); // got the page
			}
			catch
			{
				throw new Exception("No internet connection or some other error.");
			}

			return await response.Content.ReadAsStringAsync();
		}

		static async Task<String> GetTrainInfo(Dictionary<string, string> values) // fetching specific train page
		{
			var content = new FormUrlEncodedContent(values);

			var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/connection/search.xhtml", content);

			var trainPage = trainResponse.Content.ReadAsStringAsync();

			return await trainPage;
		}

		static async Task<String> GetContigentCheckChangePage(Dictionary<string, string> values)
		{
			var content = new FormUrlEncodedContent(values);

			var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/ticketVCD.xhtml", content);

			var trainPage = trainResponse.Content.ReadAsStringAsync();

			return await trainPage;
		}

		static async Task<Dictionary<string, string>> GetContigentCheckParams(string trainPage)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(trainPage);

			var JSFViewState = htmlDoc.DocumentNode
				.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				.Attributes["value"].Value;


			var ticketPassenger = htmlDoc.DocumentNode
				.SelectSingleNode("//select[@class='tmp-drl-passenger-type']")
				.Attributes["id"].Value;

			var changeParams = new Dictionary<string, string>()
			{
				{"ticketParam", "ticketParam" },
				{ticketPassenger, "2" },
				{"javax.faces.ViewState", JSFViewState },
				{"javax.faces.source", ticketPassenger },
				{"javax.faces.partial.event", "change" },
				{"javax.faces.partial.execute", ticketPassenger + " ticketParam:formWrap" },
				{"javax.faces.partial.render", "ticketParam:formWrap" },
				{"javax.faces.behavior.event", "change" },
				{"AJAX:EVENTS_COUNT", "1" },
				{"rfExt", "null" },
				{"javax.faces.partial.ajax", "true" }
			};

			var page = await GetContigentCheckChangePage(changeParams); // should internally change type of passenger to student (for free)
			htmlDoc.LoadHtml(page);

			JSFViewState = htmlDoc.DocumentNode
				.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				.Attributes["value"].Value;


			ticketPassenger = htmlDoc.DocumentNode
				.SelectSingleNode("//select[@class='tmp-drl-passenger-type']")
				.Attributes["id"].Value;

			var ticketParam = "ticketParam:j_idt637"; // this is duplicated in param in request

			Dictionary<string, string> values = new Dictionary<string, string>()
			{
				{ "ticketParam", "ticketParam"},
				{ ticketPassenger, "2"},
				{ "ticketParam:passenger:0:contingentCheck", "on"},
				{ "javax.faces.ViewState", JSFViewState},
				{ ticketParam, ticketParam}
			};

			return values;
		}

		static async Task<String> GetContigentCheck(Dictionary<string, string> values)
		{
			var content = new FormUrlEncodedContent(values);

			var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/ticketVCD.xhtml", content);

			var trainPage = trainResponse.Content.ReadAsStringAsync();

			return await trainPage;
		}




		static async Task RunAsync()
		{
			Trains trains = await GetStations(GlobalVar.FROM, GlobalVar.TO, GlobalVar.DATE, GlobalVar.TIME);




			foreach (var train in trains.GetTrains())
			{
				//var tasksPassages = new List<Task<bool>>();
				//for (var i = 0; i < train.Stations.Count - 1; i++)
				//{
				//	tasksPassages.Add(ContigentCheckPassage(train.Stations[i].Name, train.Stations[i + 1].Name,
				//		GlobalVar.DATE, train.Stations[i].DepartureTime, trains.JSFViewState, train.ID));
				//}
				//var listPassages = await Task.WhenAll(tasksPassages);


				for (var i = 0; i < train.Stations.Count - 1; i++)
				{

					var isFree = await ContigentCheckPassage(train.Stations[i].Name, train.Stations[i + 1].Name,
						GlobalVar.DATE, train.Stations[i].DepartureTime, trains.JSFViewState, train.ID);
					train.AddPassage(new Passage(train.Stations[i].Name, train.Stations[i + 1].Name, isFree));
					//train.AddPassage(new Passage(train.Stations[i].Name, train.Stations[i + 1].Name, listPassages[i]));
				}
			}

			foreach (var train in trains.GetTrains())
			{
				List<Passage> passages = new List<Passage>();
				int i = 0;
				while (i < train.Passages.Count)
				{
					int j = i + 1;
					while ((train.Passages[j].IsFree == train.Passages[i].IsFree) && (j < train.Passages.Count - 1))
					{
						j++;
					}
					Passage passage = new Passage(train.Passages[i].From, train.Passages[j].To, train.Passages[i].IsFree);
					i = j + 1;

					passages.Add(passage);
				}

				Console.WriteLine("Train {0} has following passages:", train.Name);
				foreach (var passage in passages)
				{
					Console.WriteLine("Passage from {0} to {1} is {2}", passage.From, passage.To, (passage.IsFree ? "free" : "paid"));
				}
			}
			//var response = await GetZSSKInfo(GlobalVar.FROM, GlobalVar.TO, GlobalVar.DATE, GlobalVar.TIME);

			//var htmlDoc = new HtmlDocument();
			//htmlDoc.LoadHtml(response);

			//if (GlobalVar.LOGS)
			//{
			//	System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\GetZSSKInfo.html"), response);
			//}

			//var JSFViewState = htmlDoc.DocumentNode
			//	.SelectSingleNode("//input[@name='javax.faces.ViewState']")
			//	.Attributes["value"].Value;

			////Trains trains = new Trains(JSFViewState);

			//var trainsNodes = htmlDoc.DocumentNode
			//	.SelectNodes("//tr[@class='tmp-item-line ']/td[@class='tmp-valign-top' and @colspan='3']");
			//if (trainsNodes == null)
			//{
			//	throw new NullReferenceException("No trains were fetched.");
			//}

			//int i = 0;
			//string patternForID = @"searchForm:inetConnection.*?:.*?:.*?'";
			//foreach (var node in trainsNodes)
			//{
			//	Train train = new Train();
			//	train.Name = node.InnerText; // get the train name
			//	train.Name = Regex.Replace(train.Name, @"\s+", " "); // exclude spaces from train name

			//	if (!Regex.IsMatch(train.Name, @"R .*")) // Checks if train is a fast train
			//		continue;


			//	var trainNodes = node.ParentNode.SelectSingleNode("./td[3]/div").ChildNodes; // Getting all possible *a* href links (Like Listok a miestenka, Miestenka, Listok)
			//	HtmlNode trainNode = null;
			//	foreach (var nodeTrain in trainNodes) // getting the last *a* element in train nodes (Get the attribute value for "Listok")    
			//	{
			//		if (nodeTrain.Name == "a" && nodeTrain.InnerText == "Lístok")
			//		{
			//			trainNode = nodeTrain;
			//		}
			//	}


			//	train.ID = Regex.Match(trainNode.Attributes["onclick"].Value, patternForID).Value;
			//	train.ID = train.ID.Remove(train.ID.Length - 1);

			//	var values = new Dictionary<string, string> // Clicking the specific train
			//	{
			//		{ "searchForm", "searchForm" },
			//		{ "javax.faces.ViewState", trains.JSFViewState }, 
			//		{ train.ID, train.ID }
			//	};


			//	if (GlobalVar.LOGS)
			//	{
			//		Console.WriteLine("Params of train: {0} with id: {1} is:", train.Name, train.ID);
			//		foreach (KeyValuePair<string, string> list in values)
			//		{
			//			Console.WriteLine(string.Format("Key = {0}, Value = {1}", list.Key, list.Value));
			//		}
			//	}


			//	Dictionary<string, string> str = await GetContigentCheckParams(await GetTrainInfo(values)); // setting params for contigent page

			//	if (GlobalVar.LOGS)
			//	{
			//		foreach (KeyValuePair<string, string> list in str)
			//		{
			//			Console.WriteLine(string.Format("Key = {0}, Value = {1}", list.Key, list.Value));
			//		}
			//		Console.WriteLine("");
			//	}

			//	response = await GetContigentCheck(str); // Checking whether there are free tickets or not

			//	if (GlobalVar.LOGS)
			//	{
			//		System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\GetContigentCheckTrain{0}.html", i), response);
			//	}
			//	response = await GetCart(response);

			//	if (response == null)
			//	{
			//		Console.WriteLine("Train tickets are not available for train {0}", train.Name);
			//		continue;
			//	}
			//	else
			//	{
			//		if (GlobalVar.LOGS)
			//		{
			//			System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\GetCartTrain{0}.html", i), response);
			//		}
			//		Console.WriteLine("Train tickets are available for train {0}", train.Name);
			//	}

			//	response = await EmptyCart(response);
			//	if (GlobalVar.LOGS)
			//	{
			//		System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\EmptyCartTrain{0}.html", i), response);
			//	}

			//	System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\Train{0}.html", i), response);// 
			//	i++;

			//}
			Console.WriteLine("END");
		}

		static async Task<Trains> GetStations(string from, string to, string date, string time)
		{
			var response = await GetZSSKInfo(from, to, date, time);

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(response);

			//trains.
			var JSFViewState = htmlDoc.DocumentNode
				.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				.Attributes["value"].Value;

			Trains trains = new Trains(JSFViewState);

			var trainsNodes = htmlDoc.DocumentNode
				.SelectNodes("//tr[@class='tmp-item-line ']/td[@class='tmp-valign-top' and @colspan='3']");

			if (trainsNodes == null)
			{
				throw new NullReferenceException("No trains were fetched.");
			}

			string patternForID = @"searchForm:inetConnection.*?:.*?:.*?'";

			// getting ready for scraping stations
			var nodesOfStations = htmlDoc.DocumentNode.SelectNodes("//*[@id='r0_train_R615']/table"); // loading all the tables where stations are preserved for each train

			HtmlNode stationNode = null;
			for (int i = 0; i < nodesOfStations.Count; i++)
			{
				stationNode = nodesOfStations[i];
				if (stationNode.ParentNode.ParentNode.ParentNode.Attributes["class"].Value.Contains("ic"))
					nodesOfStations.Remove(stationNode);
			}

			// going to scrape the whole structure
			for (var k = 0; k < trainsNodes.Count; k++)
			{
				var node = trainsNodes[k];
				Train train = new Train();

				train.Name = node.InnerText; // get the train name
				train.Name = Regex.Replace(train.Name, @"\s+", " "); // exclude spaces from train name

				if (!Regex.IsMatch(train.Name, @"R .*")) // Checks if train is a fast train
					continue;

				HtmlNodeCollection trainNodes = null;
				try
				{
					trainNodes = node.ParentNode.SelectSingleNode("./td[3]/div").ChildNodes; // Getting all possible *a* href links (Like Listok a miestenka, Miestenka, Listok)
				}
				catch
				{
					if (GlobalVar.LOGS)
					{
						Console.WriteLine("In train {0} href link for buying tickets couldn't be found", train.Name);
					}
					continue;
				}
				HtmlNode trainNode = null;
				foreach (var nodeTrain in trainNodes) // getting the last *a* element in train nodes (Get the attribute value for "Listok")    
				{
					if (nodeTrain.Name == "a" && nodeTrain.InnerText == "Lístok")
					{
						trainNode = nodeTrain;
					}
				}

				if (trainNode == null) // it means there was no button "Listok"
				{
					continue;
				}


				train.ID = Regex.Match(trainNode.Attributes["onclick"].Value, patternForID).Value;
				train.ID = train.ID.Remove(train.ID.Length - 1);

				// Scraping stations from now on

				var nodeHelp = nodesOfStations[k]; // same index as train If train is not a fast train it will not get here (because it skipped earlier)

				var stationNodes = nodeHelp.SelectNodes("./tbody"); // this can be minimized
				for (int i = 0; i < stationNodes.Count; i = i + 3)
				{
					string departureTime = stationNodes[i].SelectSingleNode("./tr[2]/td[4]/strong").InnerText; // scraping the first station
					string name = stationNodes[i].SelectSingleNode("./tr[2]/td[2]").InnerText;
					departureTime = Regex.Replace(departureTime, @"\s+", "");
					Station station = new Station(name, departureTime);
					train.AddStation(station); // end of scraping the first station

					var otherStations = stationNodes[i + 1].SelectNodes("./tr"); // scraping stations between the first and the last stations
					if (otherStations != null)
					{
						for (int j = 0; j < otherStations.Count; j++)
						{
							name = otherStations[j].SelectSingleNode("./td[2]").InnerText;
							departureTime = otherStations[j].SelectSingleNode("./td[4]/strong[2]").InnerText;
							departureTime = Regex.Replace(departureTime, @"\s+", "");
							station = new Station(name, departureTime);
							train.AddStation(station);
						} // end of scraping stations between the first and the last stations
					}

					if (i + 3 == stationNodes.Count) // scraping the last station
					{
						name = stationNodes[i + 2].SelectSingleNode("./tr[1]/td[2]").InnerText;
						departureTime = stationNodes[i + 2].SelectSingleNode("./tr[1]/td[4]/strong").InnerText;
						departureTime = Regex.Replace(departureTime, @"\s+", "");
						station = new Station(name, departureTime);

						train.AddStation(station);
					} // end of scraping the last station
				}
				trains.AddTrain(train); // adding final train to list of trains
			}
			return trains;
		}

		static async Task<bool> ContigentCheckPassage(string from, string to, string date, string time, string JSFViewState, string trainID)
		{
			var response = await GetZSSKInfo(from, to, date, time);
			var values = new Dictionary<string, string> // Clicking the specific train
				{
					{ "searchForm", "searchForm" },
					{ "javax.faces.ViewState", JSFViewState },
					{ trainID, trainID }
				};
			Dictionary<string, string> str = await GetContigentCheckParams(await GetTrainInfo(values)); // setting params for contigent page
			response = await GetContigentCheck(str); // Checking whether there are free tickets or not

			response = await GetCart(response); // now the final check comes, If null means page was unsuccessful to load so no free ticket.

			if (response == null)
			{
				Console.WriteLine("Train tickets are not available for train going on {0} at {1} from {2} to {3}", date, time, from, to);
				return false;
			}
			else
			{
				Console.WriteLine("Train tickets are available for train going on {0} at {1} from {2} to {3}", date, time, from, to);
			}

			response = await EmptyCart(response);
			return true;
		}
	}
}
