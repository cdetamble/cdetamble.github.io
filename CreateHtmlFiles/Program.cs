using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CreateHtmlFiles
{
    class Program
    {
		private static bool isLocalhost = false;

        static void Main(string[] args)
        {
            var builder = new Builder(isLocalhost  ? "http://localhost:8080/mouthlessgames/" : "/", true);
            
            builder.CreateGeneric("about", "About");
			builder.CreateGeneric("legal", "Legal");

			var gamesHtml = builder.CreateGames();
			//builder.CreateTutorials();

			var blogposts = builder.CreateBlogPosts();
			var blogHtml = builder.CreateBlog(blogposts);
			builder.CreateIndex(gamesHtml);
			//Console.ReadLine();
		}
    }

    class Builder
    {
        private readonly string _rootDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _fragments = new Dictionary<string, string>();

		public Builder(string baseUrl, bool withGoogleAnalytics)
        {
            this._baseUrl = baseUrl;

            List<string> directories = new List<string>();
            var fragmentsPath = Path.Combine(_rootDir, "fragments");
            directories.Add(fragmentsPath);

            foreach (var directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
					if (Path.GetFileName(file).StartsWith(".")) continue;
                    string key = file.Replace(fragmentsPath + "\\", "");
                    _fragments.Add(Path.GetFileNameWithoutExtension(key), File.ReadAllText(file));
                }
            }

			if (withGoogleAnalytics)
			{
				_fragments["header"] = _fragments["header"].Replace("{analytics}", _fragments["analytics"]);
			}
        }

        public void CreateIndex(string html)
        {
			var html2 = ResolvePlaceholders(_fragments["header"]
					.Replace("{title}", "MouthlessGames - Official Website")
						+ html + _fragments["footer"]);

			WriteAllText(Path.Combine(_rootDir, "index.html"), html2);
			WriteAllText(Path.Combine(_rootDir, "games", "index.html"), html2);
		}

        public void CreateTutorials()
        {
            var folder = Path.Combine(_rootDir, "tutorials");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"]
							.Replace("{title}", "Tutorials - MouthlessGames") + _fragments["tutorials"] + _fragments["footer"];

            WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public string CreateGames()
        {
			var folder = Path.Combine(_rootDir, "games");
			Directory.CreateDirectory(folder);
			string html = "";

			var games = new List<Dictionary<string, string>>();

			string[] gamePaths = Directory.GetFiles(Path.Combine(_rootDir, "fragments", "games"));
			foreach (var gamePath in gamePaths)
			{
				if (Path.GetFileName(gamePath).StartsWith(".")) continue;

				string gameString = File.ReadAllText(gamePath);
				string[] lines = gameString.Split('\n');

				var game = new Dictionary<string, string>();
				foreach (var line in lines)
				{
					if (line.Trim().Length == 0 || !(line.Trim().StartsWith("{") && line.Trim().EndsWith("}")))
					{
						continue;
					}

					var key = line.Substring(1, line.IndexOf(" ")).Trim();
					game.Add(key, ExtractFrom(key, line));
				}
				//game.Add("name", FromBrowsableString(Path.GetFileNameWithoutExtension(gamePath)));
				games.Add(game);
			}

			foreach (var game in games)
			{
				var browsableGameName = ToBrowsableString(game["name"]);

				var gameHtml = _fragments["game"];
				foreach (var key in game.Keys)
				{
					gameHtml = gameHtml.Replace("{" + key + "}", game[key].Replace("{baseUrl}", _baseUrl));
				}

				string[] previews = Directory.GetFiles(Path.Combine(_rootDir, "img", browsableGameName, "previews"));
				var previewsHtml = new StringBuilder();
				string mainImage = null;
				foreach (var preview in previews)
				{
					var imageName = Path.GetFileName(preview);
					var imageUrl = _baseUrl + "img/" + browsableGameName + "/previews/" + imageName;
					if (mainImage == null) mainImage = imageUrl;
					var thumbnailUrl = _baseUrl + "img/" + browsableGameName + "/previews/thumbnails/" + Path.GetFileNameWithoutExtension(preview) + ".png";
					previewsHtml.Append($"<a href=\"{imageUrl}\" class=\"fancybox\" rel=\"gallery-{ browsableGameName }\">" +
						$"<img src=\"{thumbnailUrl}\"></a>");
				}

				if (game.ContainsKey("video-url"))
				{
					var videoHtml = $"<a class=\"fancybox-media\" href=\"{ game["video-url"] }\" rel=\"gallery-{ browsableGameName }\">" +
						$"<img src=\"{_baseUrl}img/youtube.png\"></a>";
					gameHtml = gameHtml.Replace("{video}", videoHtml);
				}

				if (game.ContainsKey("ldjam-url"))
				{
					gameHtml = gameHtml.Replace("{ldjam-url}", game["ldjam-url"]);
				}

				if (game.ContainsKey("stars"))
				{
					if (game["stars"] == "0")
					{
						gameHtml = gameHtml.Replace("{num-stars}", "");
					}
					else
					{
						var starsHtml = $"<img class=\"stars\" src=\"{ _baseUrl }img/stars/{ game["stars"] }stars.png\" title=\"Ranked with { game["stars"] } Stars on Google Play\">";
						gameHtml = gameHtml.Replace("{num-stars}", starsHtml);
					}
				}

				var quotesHtml = new StringBuilder();
				if (game.ContainsKey("quotes"))
				{
					string[] quotes = game["quotes"].Split(';');
					foreach (var quote in quotes)
					{
						quotesHtml.Append("<div class=\"quote\">\"" + quote + "\"</div>");
					}
				}

				gameHtml = gameHtml
					.Replace("{game-quotes}", quotesHtml.ToString())
					.Replace("{previews}", previewsHtml.ToString())
					.Replace("{main-image}", mainImage)
					.Replace("{browsable-name}", browsableGameName)
					.Replace("{name-image}", _baseUrl + "img/" + browsableGameName + "/name.png");

				gameHtml = gameHtml.Replace("{baseUrl}", _baseUrl);

				html += gameHtml;
			}

			var gamesHtml = _fragments["header"].Replace("{title}", "Games - MouthlessGames") + html + _fragments["footer"];
			WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(gamesHtml));

			return html;
		}

		public string CreateBlog(List<Blogpost> blogposts)
		{
			var folder = Path.Combine(_rootDir, "blog");
			Directory.CreateDirectory(folder);

			StringBuilder sb = new StringBuilder();
			foreach (var post in blogposts)
			{
				sb.Append(_fragments["blog"]
					.Replace("{title}", post.Title.Trim())
					.Replace("{id}", post.Id)
					.Replace("{date}", post.Date)
					.Replace("{image}", "{baseUrl}img/" + post.Image)
					.Replace("{topic}", post.Topic)
					.Replace("{baseUrl}", _baseUrl)
					.Replace("{description}", post.Description.Trim())
					.Replace("{readtime}", post.Readtime.Trim())
					);
			}

			string html = _fragments["header"]
						.Replace("{title}", "Blog - MouthlessGames") + sb.ToString() + _fragments["footer"];

			WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));

			return sb.ToString();
		}

		public void CreateGeneric(string id, string pageTitle)
        {
            var folder = Path.Combine(_rootDir, id);
            Directory.CreateDirectory(folder);

            string html = _fragments["header"]
				.Replace("{with-bg}", "with-bg")
				.Replace("{title}", pageTitle + " - MouthlessGames") + _fragments[id] + _fragments["footer"];

            WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

		public List<Blogpost> CreateBlogPosts()
        {
			var blogposts = new List<Blogpost>();
            string[] blogPostPaths = Directory.GetFiles(Path.Combine(_rootDir, "fragments", "posts"));
            foreach (var blogPostPath in blogPostPaths)
            {
				if (Path.GetFileName(blogPostPath).StartsWith(".")) continue;

                string blogPost = File.ReadAllText(blogPostPath);
                string[] lines = blogPost.Split('\n');
              
                string title = ExtractFrom("title", lines[0]);
				string id = ToBrowsableString(title);
				string date = ExtractFrom("date", lines[1]);
				string topic = ExtractFrom("topic", lines[2]);
				string image = ExtractFrom("image", lines[3]);
				string description = ExtractFrom("description", lines[4]);
				string readtime = ExtractFrom("readtime", lines[5]);

				lines[0] = "";
                lines[1] = "";
                lines[2] = "";
				lines[3] = "";
				lines[4] = "";
				lines[5] = "";
				string content = string.Join("", lines);

				var blogpost = new Blogpost(id, title, date, description, topic, image, content, readtime);
				blogposts.Add(blogpost);

				var folder = Path.Combine(_rootDir, "blog", id);
                Directory.CreateDirectory(folder);

                string html = _fragments["header"]
									.Replace("{with-bg}", "with-bg")
									.Replace("{title}", blogpost.Title + " - MouthlessGames")
                                    .Replace("{blogPostId}", blogpost.Id)
                    + _fragments["blog-post"]
                            .Replace("{id}", blogpost.Id)
                            .Replace("{title}", blogpost.Title)
                            .Replace("{date}", blogpost.Date)
							.Replace("{image}", "{baseUrl}img/" + blogpost.Image)
							.Replace("{topic}", blogpost.Topic)
							.Replace("{content}", blogpost.Content)
					+ _fragments["footer"];

                WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
            }

			return blogposts;
		}

		private string ToBrowsableString(string title)
		{
			return Regex.Replace(title, @"[^0-9a-zA-Z ]+", "").Replace(" ", "-").ToLower().Trim();
		}

		private string FromBrowsableString(string title)
		{
			CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
			TextInfo textInfo = cultureInfo.TextInfo;

			return textInfo.ToTitleCase(title.Replace("-", " "));
		}

		private void WriteAllText(string path, string text)
		{
			if (File.Exists(path))
			{
				var current = File.ReadAllText(path);
				if (current != text)
				{
					Console.WriteLine("Written: " + path);
					File.WriteAllText(path, text);
				}
				else
				{
					//Console.WriteLine("Untouched: " + path);
				}
			} else
			{
				Console.WriteLine("Written: " + path);
				File.WriteAllText(path, text);
			}
		}

		private string ExtractFrom(string property, string line)
        {
            var result = line.Substring(2 + property.Length).Trim();
			return result.Substring(0, result.Length - 1);

		}

        private string ResolvePlaceholders(string text)
        {
            return text
                .Replace("{baseUrl}", _baseUrl)

                // if the following placeholders have not been set until now,
                // assume that we dont want to use them and simply remove them
                .Replace("{previous-game}", "")
                .Replace("{next-game}", "")
                .Replace("{blogPostId}", "")
				.Replace("{title}", "")
				.Replace("{with-bg}", "")
				.Replace("{analytics}", "")
				.Replace("{customJs}", "");
                                            
        }
    }
}
