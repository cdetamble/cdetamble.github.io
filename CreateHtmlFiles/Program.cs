using System;
using System.Collections.Generic;
using System.IO;

namespace CreateHtmlFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new Builder("/mouthlessgames/");
            builder.CreateIndex();
            builder.CreateAbout();
            builder.CreateBlogPosts();
            builder.CreateOurGames();
            builder.CreateGameTutorials();
            
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
            File.WriteAllText(Path.Combine(_rootDir, "index.html"),
                ResolvePlaceholders(_fragments["header"] + _fragments["index"] + _fragments["footer"]));
        }

        public void CreateGameTutorials()
        {
            var folder = Path.Combine(_rootDir, "game-tutorials");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + _fragments["game-tutorials"] + _fragments["footer"];

            File.WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public void CreateOurGames()
        {
            var folder = Path.Combine(_rootDir, "our-games");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + _fragments["our-games"] + _fragments["footer"];

            File.WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public void CreateAbout()
        {
            var about = Path.Combine(_rootDir, "fragments", "about.html");
            var folder = Path.Combine(_rootDir, "about");
            Directory.CreateDirectory(folder);

            string html = _fragments["header"] + File.ReadAllText(about) + _fragments["footer"];

            File.WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
        }

        public void CreateBlogPosts()
        {
            string[] blogPostPaths = Directory.GetFiles(Path.Combine(_rootDir, "fragments", "posts"));
            foreach (var blogPostPath in blogPostPaths)
            {
                string blogPost = File.ReadAllText(blogPostPath);
                string[] lines = blogPost.Split('\n');
                string id = ExtractFrom("id", lines[0]);
                string title = ExtractFrom("title", lines[1]);
                string date = ExtractFrom("date", lines[2]);
                lines[0] = "";
                lines[1] = "";
                lines[2] = "";
                string content = string.Join("", lines);

                var folder = Path.Combine(_rootDir, "blog", id);
                Directory.CreateDirectory(folder);

                string html = _fragments["header"]
                                    .Replace("{customJs}", _fragments["load-comments"])
                                    .Replace("{blogPostId}", id)
                    + _fragments["blog-post"]
                            .Replace("{id}", id)
                            .Replace("{title}", title)
                            .Replace("{date}", date)
                            .Replace("{content}", content)
                    + _fragments["comments"]
                    + _fragments["footer"];

                File.WriteAllText(Path.Combine(folder, "index.html"), ResolvePlaceholders(html));
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
