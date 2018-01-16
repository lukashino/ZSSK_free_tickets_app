using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ZSSK_cheaper_tickets_cons
{
	public class Trains
	{
		List<Train> TrainsList = new List<Train>();
		public string JSFViewState { get; set; }

		public Trains()
		{
			JSFViewState = "";
			TrainsList = null;
		}
	}

	public class Train
	{
		public string ID { get; set; }
		public string Name { get; set; }

		public Train()
		{
			Name = "";
			ID = "";
		}
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

		static async Task<String> GetCart(string JSFViewState)
		{
			var values = await GetCartParams(JSFViewState);

			var content = new FormUrlEncodedContent(values);

			var response = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/personalData.xhtml", content); // got the page

			return await response.Content.ReadAsStringAsync();
		}

		//static async void EmptyCart()
		//{

		//}

		static async Task<String> GetZSSKInfo()
		{
			
			//using (var client = new HttpClient())
			//{
				var values = new Dictionary<string, string> // getting list of available trains
				{
					{ "lang", "sk" },
					{ "portal", "" },
					{ "from", "Bratislava hl.st." },
					{ "to", "Margecany" },
					{ "via", "" },
					{ "date", "19.1.2018" },
					{ "time", "6:30" },
					{ "departure", "true" },
					{ "wlw-checkbox_key%3A%7BpageFlow.inputParam.paramItemParams.direct.valueBoolean%7DOldValue", "false" },
					{ "maxChangeTrainCount", "5" },
					{ "minChangeTrainTime", "2" },
					{ "bed", "0" }
				};

				var content = new FormUrlEncodedContent(values);

				var response = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/connection/portal.xhtml", content); // got the page


				return await response.Content.ReadAsStringAsync();
			//}



			//var url = new Url("https://ikvc.slovakrail.sk/inet-sales-web/pages/connection/portal.xhtml");
			//var values = new Dictionary<string, string>
			//	{
			//		{ "lang", "sk" },
			//		{ "portal", "" },
			//		{ "from", "Bratislava hl.st." },
			//		{ "to", "Margecany" },
			//		{ "via", "" },
			//		{ "date", "6.12.2017" },
			//		{ "time", "6:00" },
			//		{ "departure", "true" },
			//		{ "wlw-checkbox_key%3A%7BpageFlow.inputParam.paramItemParams.direct.valueBoolean%7DOldValue", "false" },
			//		{ "maxChangeTrainCount", "5" },
			//		{ "minChangeTrainTime", "2" },
			//		{ "bed", "0" }
			//	};
			//var response = await url


			//	//.WithHeaders(new {
			//	//	Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
			//	//	Accept_Encoding = "gzip, deflate, br",
			//	//	Accept_Language = "en-US,en; q=0.5",
			//	//	User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0",
			//	//	Referer = "http://www.slovakrail.sk/",
			//	//	Upgrade_Insecure_Requests = "1",
			//	//	Connection = "keep-alive",
			//	//	Host = "ikvc.slovakrail.sk",
			//	//	DNT ="1" })
			//	//	.WithCookies(new {
			//	//		WEBCOOKIE = "5RgpLmo2MTcNsYHUUNZOED63oDLYfXeADRthrA4RDd9biSeewSvg!1595625564",
			//	//		IAMCOOKIE = "1XopLmzntsmlGngDENb6Nw3hoWTFTcrkBuj229A_0kTGPMJMWfrk!1362272383",
			//	//		lang = "sk"}) 


			//	.PostUrlEncodedAsync(values).ReceiveString();
			//return response;

		}

		static async Task<String> GetTrainInfo(Dictionary<string, string> values)
		{

			//using (var client = new HttpClient())
			//{
				var content = new FormUrlEncodedContent(values);

				var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/connection/search.xhtml", content);

				var trainPage = trainResponse.Content.ReadAsStringAsync();

				return await trainPage;
			//}

		}

		static async Task<String> GetContigentCheckChangePage(Dictionary<string, string> values)
		{
			var content = new FormUrlEncodedContent(values);

			var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/ticketVCD.xhtml", content);

			return await trainResponse.Content.ReadAsStringAsync();
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

			var changeParams = new Dictionary<string, string>();
			changeParams.Add("ticketParam", "ticketParam");
			changeParams.Add(ticketPassenger, "2");
			changeParams.Add("javax.faces.ViewState", JSFViewState); // get it before values and do check for JSF again
			changeParams.Add("javax.faces.source", ticketPassenger);
			changeParams.Add("javax.faces.partial.event", "change");
			changeParams.Add("javax.faces.partial.execute", ticketPassenger + " ticketParam:formWrap");
			changeParams.Add("javax.faces.partial.render", "ticketParam:formWrap");
			changeParams.Add("javax.faces.behavior.event", "change");
			changeParams.Add("AJAX:EVENTS_COUNT", "1");
			changeParams.Add("rfExt", "null");
			changeParams.Add("javax.faces.partial.ajax", "true");

			var page = await GetContigentCheckChangePage(changeParams); // should internally change type of passenger to student (for free)
			htmlDoc.LoadHtml(page);

			JSFViewState = htmlDoc.DocumentNode
				.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				.Attributes["value"].Value;


			ticketPassenger = htmlDoc.DocumentNode
				.SelectSingleNode("//select[@class='tmp-drl-passenger-type']")
				.Attributes["id"].Value;

			var ticketParam = "ticketParam:j_idt637"; // this is duplicated in param in request

			Dictionary<string, string> values = new Dictionary<string, string>();
			values.Add("ticketParam", "ticketParam");
			values.Add(ticketPassenger, "2");
			values.Add("ticketParam:passenger:0:contingentCheck", "on");
			values.Add("javax.faces.ViewState", JSFViewState);
			values.Add(ticketParam, ticketParam);

			return values;
		}

		static async Task<String> GetContigentCheck(Dictionary<string, string> values)
		{
			
			

			var content = new FormUrlEncodedContent(values);

			var trainResponse = await client.PostAsync("https://ikvc.slovakrail.sk/inet-sales-web/pages/shopping/ticketVCD.xhtml", content);

			return await trainResponse.Content.ReadAsStringAsync();
		}




		static async Task RunAsync()
		{
			//var hostName = "https://www.bazos.sk/";
			//var _pageCode = await GetPageCode(hostName);
			//Console.WriteLine(_pageCode);
			var response = await GetZSSKInfo();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(response);

			Trains trains = new Trains();
			trains.JSFViewState = htmlDoc.DocumentNode
				.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				.Attributes["value"].Value;

			var trainsNodes = htmlDoc.DocumentNode
				.SelectNodes("//tr[@class='tmp-item-line ']/td[@class='tmp-valign-top' and @colspan='3']");
			if (trainsNodes == null)
			{
				throw new NullReferenceException("No trains were fetched.");
			}

			int i = 0;
			string patternForID = @"searchForm:inetConnection.*?:.*?:.*?'";
			foreach (var node in trainsNodes)
			{
				Train train = new Train();
				train.Name = node.InnerText;
				train.Name = Regex.Replace(train.Name, @"\s+", " ");

				if (!Regex.IsMatch(train.Name, @"R .*")) // Checks if train is a fast train
					continue;


				var trainNodes = node.ParentNode.SelectSingleNode("./td[3]/div").ChildNodes; // Getting all possible a href links (Like Listok a miestenka, Miestenka, Listok)
				HtmlNode trainNode = null;
				foreach (var nodeTrain in trainNodes) // getting last a element in train nodes (Get the attribute value for "Listok")
				{
					if (nodeTrain.Name == "a" && nodeTrain.InnerText == "Lístok")
					{
						trainNode = nodeTrain;
					}
				}


				train.ID = Regex.Match(trainNode.Attributes["onclick"].Value, patternForID).Value;
				train.ID = train.ID.Remove(train.ID.Length - 1);

				var values = new Dictionary<string, string> // getting list of available trains
				{
					{ "searchForm", "searchForm" },
				};

				values.Add("javax.faces.ViewState", trains.JSFViewState);
				values.Add(train.ID, train.ID);


				Console.WriteLine("Params of train: {0} with id: {1} is:", train.Name, train.ID);
				foreach (KeyValuePair<string, string> list in values)
				{
					Console.WriteLine(string.Format("Key = {0}, Value = {1}", list.Key, list.Value));
				}

				Dictionary<string, string> str = await GetContigentCheckParams(await GetTrainInfo(values)); // setting params for contigent page

				foreach (KeyValuePair<string, string> list in str)
				{
					Console.WriteLine(string.Format("Key = {0}, Value = {1}", list.Key, list.Value));
				}
				Console.WriteLine("");
				response = await GetContigentCheck(str); // Checking whether there are free tickets or not
				var htmlDoc2 = new HtmlDocument();
				htmlDoc2.LoadHtml(response); // Loading up HTML
				//var JSFViewState = htmlDoc.DocumentNode
				//.SelectSingleNode("//input[@name='javax.faces.ViewState']")
				//.Attributes["value"].Value;

				//response = await GetCart(JSFViewState);

				System.IO.File.WriteAllText(string.Format(@"C:\Users\Lukas\Desktop\scr\Train{0}.html", i), response);// 

				i++; // GETCART IS NOT WORKING!!!!!!!
			}

		}
	}
}
