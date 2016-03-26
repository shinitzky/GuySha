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
        private Dictionary<string, double> m_userAvgs;
        //private Dictionary<int, MovieDetails> m_movies; // maybe not needed
        //private HashSet<string> m_movies;
        private Dictionary<string, int> m_movies; //movie id and amount of ratings


        //constructor
        public RecommenderSystem()
        {
            //m_users = new Dictionary<int, string>();
            m_ratings = new Dictionary<string, Dictionary<string, double>>();
            m_movies = new Dictionary<string, int>();
            m_userAvgs = new Dictionary<string, double>();
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
                    calcAvgs();
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
                    if (!m_movies.ContainsKey(movieId))
                        m_movies.Add(movieId, 1);
                    else
                        m_movies[movieId]++;
                    double rating = Double.Parse(l[2]);
                    if (!m_ratings.ContainsKey(userId))
                    {
                        m_ratings.Add(userId, new Dictionary<string, double>());
                        m_userAvgs.Add(userId, 0);
                    }
                    m_ratings[userId].Add(movieId, rating);
                    m_userAvgs[userId] += rating;
                }
                line = sr.ReadLine();
            }
            sr.Close();

        }
        private void calcAvgs() //for the initial calculations ufter loading ratings file
        {
            //calc avg ratings for each user - the sums foreach user has already calculated
            foreach(string userId in m_ratings.Keys)
            {
                int numOfRatings = m_ratings[userId].Count; //check!!
                double sumOfRatings = m_userAvgs[userId];
                m_userAvgs[userId] = sumOfRatings / numOfRatings;
            }
        }
        //return a list of the ids of all the users in the dataset
        public List<string> GetAllUsers()
        {
            return m_ratings.Keys.ToList();
        }

        //returns a list of all the items in the dataset
        public List<string> GetAllItems()
        {
            return m_movies.Keys.ToList();
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
            //check if sUID exist in m_users
            //check if sIID exist in m_movies

            double ra = m_userAvgs[sUID];
            Dictionary<string, double> raiDic = new Dictionary<string, double>();
            double pearsonDenominatorRight = 0;
            if (m == PredictionMethod.Pearson)
            {
                foreach (string mID in m_ratings[sUID].Keys)
                {
                    double val = m_ratings[sUID][mID] - ra;
                    raiDic.Add(mID, val);
                    pearsonDenominatorRight += Math.Pow(val,2);
                }
            }
            double numerator = 0;
            double denominator = m_movies[sIID];

            //calc sum of w
            foreach (string uID in m_ratings.Keys)
            {
                if (uID.Equals(sUID))
                    continue;
                if (m_ratings[uID].ContainsKey(sIID))
                {
                    double wau = 0;
                    if (m == PredictionMethod.Pearson)
                        wau = calcWPearson(sUID, uID, raiDic,pearsonDenominatorRight);
                    else if (m == PredictionMethod.Cosine)
                        wau = calcWCossim(sUID, uID);
                    //else random
                    numerator += (wau * m_ratings[uID][sIID]);
                }
                
            }
            return (numerator / denominator);
        }
        
        private double calcWPearson(string aID, string uID, Dictionary<string, double> raiDic, double denominatorRight)
        { 
            double numerator = 0;
            double denominatorLeft = 0;

            double ru = m_userAvgs[uID];
            foreach(string mId in m_ratings[uID].Keys)
            {
                if (!raiDic.ContainsKey(mId))
                    continue;
                double val = (m_ratings[uID][mId] - ru);
                numerator += (val * raiDic[mId]);
                denominatorLeft += Math.Pow(val, 2);               
            }

            double denominator = (Math.Sqrt(denominatorLeft)) * (Math.Sqrt(denominatorRight));
            return numerator/denominator;
        }

        private double calcWCossim(string aID, string uID)
        {
            double numerator = 0;
            double denominatorLeft = 0;
            double denominatorRight = 0;

            foreach (string mId in m_ratings[uID].Keys)
            {
                if (!m_ratings[aID].ContainsKey(mId))
                    continue;
                double rui = m_ratings[uID][mId];
                double rai = m_ratings[aID][mId];
                numerator += (rui * rai);
                denominatorLeft += Math.Pow(rui, 2);
                denominatorRight += Math.Pow(rai, 2);
            }

            double denominator = (Math.Sqrt(denominatorLeft)) * (Math.Sqrt(denominatorRight));
            return numerator / denominator;
        }
        //Compute MAE (mean absolute error) for a set of rating prediction methods over the same user-item pairs
        //cTrials specifies the number of user-item pairs to be tested
        public Dictionary<PredictionMethod, double> ComputeMAE(List<PredictionMethod> lMethods, int cTrials)
        {
            throw new NotImplementedException();
        }
    }
}
