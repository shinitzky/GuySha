using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RecommenderSystem
{
    class RecommenderSystem
    {
        public enum PredictionMethod { Pearson, Cosine, Random };

        //class members here

        //private Dictionary<int, string> m_users; //maybe not needed
        private Dictionary<string, Dictionary<string, double>> m_ratings;
        //private Dictionary<int, MovieDetails> m_movies; // maybe not needed
        private HashSet<string> m_movies;
        
        //constructor
        public RecommenderSystem()
        {
            //m_users = new Dictionary<int, string>();
            m_ratings = new Dictionary<string, Dictionary<string, double>>();
            m_movies = new HashSet<string>();
            //m_movies = new Dictionary<int, MovieDetails>();

        }

        //load a datatset 
        //The file contains one row for each u,i rating, in the following format:
        //userid::itemid::rating::timestamp
        //More at http://recsyswiki.com/wiki/Movietweetings
        //Download at https://github.com/sidooms/MovieTweetings/tree/master/latest
        //Do all precomputations here if needed
        public void Load(string sFileName)
        {
            try
            {
                StreamReader sr = new StreamReader(sFileName);
                if(sr == null)
                    Console.WriteLine("Couldn't load file...sorry:)");
                if (sFileName.Contains("ratings"))
                {
                    parseRatings(sr);
                    calc();
                }
               /* else if (sFileName.Contains("users"))
                {
                    parseUsers(sr);
                }
                else if (sFileName.Contains("movies"))
                {
                    parseMovies(sr);
                }*/
                else
                    Console.WriteLine("invalid file name");
               
            }
            catch(Exception e)
            {
                Console.WriteLine("Couldn't load file...sorry:)");
            }

        }
       /* private void parseUsers(StreamReader sr) //maybe not needed
        {
            string line = sr.ReadLine();
            while (line != null)
            {
                char[] sep = new char[1];
                sep[0] = ':';
                string[] l = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (l.Length == 2)
                {
                    int userId = Int32.Parse(l[0]);
                    string twitterID = l[1];
                    m_users.Add(userId, twitterID);
                }
                line = sr.ReadLine();
            }
            sr.Close();
        }
        private void parseMovies(StreamReader sr) //maybe not needed
        {
            string line = sr.ReadLine();
            while (line != null)
            {
                char[] sep = new char[1];
                sep[0] = ':';
                string[] l = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (l.Length >= 2)
                {
                    int movieId = Int32.Parse(l[0]);
                    string movieTitle = l[1];
                    List<String> genres = new List<string>();
                    if (l.Length > 2)
                    {
                        char[] sep2 = new char[1];
                        sep2[0] = '|';
                        genres = l[2].Split(sep2, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    MovieDetails movieDet = new MovieDetails(movieTitle, genres);
                    m_movies.Add(movieId, movieDet);
                }
                line = sr.ReadLine();
            }
            sr.Close();

        }*/
        private void parseRatings(StreamReader sr) //not saving time stamp
        {
            string line = sr.ReadLine();
            while (line != null)
            {
                char[] sep = new char[1];
                sep[0] = ':';
                string[] l = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (l.Length == 4)
                {
                    //int userId = Int32.Parse(l[0]);
                    //int movieId = Int32.Parse(l[1]);
                    string userId = l[0];
                    string movieId = l[1];
                    if (!m_movies.Contains(movieId))
                        m_movies.Add(movieId);
                    double rating = Double.Parse(l[2]);
                    if (!m_ratings.ContainsKey(userId))
                    {
                        m_ratings.Add(userId, new Dictionary<string, double>());
                    }
                    m_ratings[userId].Add(movieId, rating);
                }
                line = sr.ReadLine();
            }
            sr.Close();

        }
        private void calc() //for the initial calculations ufter loading ratings file
        {

        }
        //return a list of the ids of all the users in the dataset
        public List<string> GetAllUsers()
        {
            return m_ratings.Keys.ToList();
        }

        //returns a list of all the items in the dataset
        public List<string> GetAllItems()
        {
            return m_movies.ToList();
        }

        //returns the list of all items that the given user has rated in the dataset
        public List<string> GetRatedItems(string sUID)
        {
            if (m_ratings.ContainsKey(sUID))
            {
                return m_ratings[sUID].Keys.ToList();
            }
            else
                return null;//!!
        }

        //Returns a user-item rating that appears in the dataset (not predicted)
        public double GetRating(string sUID, string sIID)
        {
            if (m_ratings.ContainsKey(sUID))
            {
                double rating = -1;
                m_ratings[sUID].TryGetValue(sIID, out rating);
                return rating;
            }
            else
            {
                Console.WriteLine("User not found");
                return -1;
            }
        }

        //predict a rating for a user item pair using the specified method
        public double PredictRating(PredictionMethod m, string sUID, string sIID)
        {
            throw new NotImplementedException();
        }

        //Compute MAE (mean absolute error) for a set of rating prediction methods over the same user-item pairs
        //cTrials specifies the number of user-item pairs to be tested
        public Dictionary<PredictionMethod, double> ComputeMAE(List<PredictionMethod> lMethods, int cTrials)
        {
            throw new NotImplementedException();
        }
    }
}
