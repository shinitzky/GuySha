using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    public class MovieDetails //maybe not needed
    {
        public string Title { get; set; }
        public List<string> Genres { get; set; }

        public MovieDetails()
        {
            Title = "noTitle";
            Genres = new List<string>();
        }

        public MovieDetails(string title, List<string> genres)
        {
            Title = title;
            Genres = genres;
        }
    }
}
