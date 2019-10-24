namespace CreateHtmlFiles
{
	internal class Blogpost
	{

		public Blogpost(string id, string title, string date, string description, string topic, string image, string content, string readtime)
		{
			Id = id;
			Title = title;
			Date = date;
			Description = description;
			Content = content;
			Image = image;
			Topic = topic;
			Readtime = readtime;
		}

		public string Id { get ; internal set; }
		public string Title { get; internal set; }
		public string Date { get; internal set; }
		public string Description { get; internal set; }
		public string Image { get; internal set; }
		public string Topic { get; internal set; }
		public string Content { get; internal set; }
		public string Readtime { get; internal set; }
	}
}