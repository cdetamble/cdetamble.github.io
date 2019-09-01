using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CreateHtmlFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new Builder("/mouthlessgames/");
            builder.CreateIndex();
            builder.CreateAbout();
			builder.CreateOurGames();
			builder.CreateGameTutorials();

			var blogposts = builder.CreateBlogPosts();
			builder.CreateBlog(blogposts);
		    
            //Console.ReadLine();
        }
    }

    class Builder
    {
        private readonly string _rootDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _fragments = new Dictionary<string, string>();

        public Builder(string baseUrl)
        {
            this._baseUrl = baseUrl;

            List<string> directories = new List<string>();
            var fragmentsPath = Path.Combine(_rootDir, "fragments");
            directories.Add(fragmentsPath);
            directories.Add(Path.Combine(fragmentsPath, "custom-js"));

            foreach (var directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    string key = file.Replace(fragmentsPath + "\\", "");
                    _fragments.Add(Path.GetFileNameWithoutExtension(key), File.ReadAllText(file));
                }
            }
        }

        public void CreateIndex()
        {
            WriteAllText(Path.Combine(_rootDir, "index.html"),
                ResolvePlaceholders(_fragments["header"] + _fragments["index"] + _fragments["footer"]));
        }

        public void CreateGameTutorials()
        {
            var folder = Path.Combine(_rootDir, "tutorials");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + _fragments["tutorials"] + _fragments["footer"];

            WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public void CreateOurGames()
        {
            var folder = Path.Combine(_rootDir, "games");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + _fragments["games"] + _fragments["footer"];

            WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

		public void CreateBlog(List<BlogPost> blogposts)
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
					.Replace("{description}", post.Description.Trim()));
			}

			string html = _fragments["header"] + sb.ToString() + _fragments["footer"];

			WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
		}

		public void CreateAbout()
        {
            var folder = Path.Combine(_rootDir, "about");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + _fragments["about"] + _fragments["footer"];

            WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public List<BlogPost> CreateBlogPosts()
        {
			var blogposts = new List<BlogPost>();
            string[] blogPostPaths = Directory.GetFiles(Path.Combine(_rootDir, "fragments", "posts"));
            foreach (var blogPostPath in blogPostPaths)
            {
                string blogPost = File.ReadAllText(blogPostPath);
                string[] lines = blogPost.Split('\n');
                string id = ExtractFrom("id", lines[0]);
                string title = ExtractFrom("title", lines[1]);
                string date = ExtractFrom("date", lines[2]);
				string description = ExtractFrom("description", lines[3]);
				lines[0] = "";
                lines[1] = "";
                lines[2] = "";
				lines[3] = "";
				string content = string.Join("", lines);

				var blogpost = new BlogPost(id, title, date, description, content);
				blogposts.Add(blogpost);

				var folder = Path.Combine(_rootDir, "blog", id);
                Directory.CreateDirectory(folder);

                string html = _fragments["header"]
                                    .Replace("{customJs}", _fragments["load-comments"])
                                    .Replace("{blogPostId}", id)
                    + _fragments["blog-post"]
                            .Replace("{id}", blogpost.Id)
                            .Replace("{title}", blogpost.Title)
                            .Replace("{date}", blogpost.Date)
                            .Replace("{content}", blogpost.Content)
                    + _fragments["comments"]
                    + _fragments["footer"];

                WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
            }

			return blogposts;
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
					Console.WriteLine("Untouched: " + path);
				}
			} else
			{
				Console.WriteLine("Written: " + path);
				File.WriteAllText(path, text);
			}
		}

		private string ExtractFrom(string property, string line)
        {
            return line
                .Replace("{", "")
                .Replace("}", "")
                .Replace(property, "")
                .Trim();
        }

        private string ResolvePlaceholders(string text)
        {
            return text
                .Replace("{baseUrl}", _baseUrl)

                // if the following placeholders have not been set until now,
                // assume that we dont want to use them and simply remove them
                .Replace("{blogPostId}", "")
                .Replace("{customJs}", ""); 
                                            
        }
    }
}
