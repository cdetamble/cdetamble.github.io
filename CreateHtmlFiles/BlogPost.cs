namespace CreateHtmlFiles
{
	internal class BlogPost
	{

		public BlogPost(string id, string title, string date, string description, string content)
		{
			Id = id;
			Title = title;
			Date = date;
			Description = description;
			Content = content;
		}

		public string Id { get ; internal set; }
		public string Title { get; internal set; }
		public string Date { get; internal set; }
		public string Description { get; internal set; }
		public string Content { get; internal set; }
	}
}