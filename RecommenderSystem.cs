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
        public double PredictRating(PredictionMethod m,string sUID, string sIID )
        {
            if(!m_ratings.ContainsKey(sUID))
            {
                Console.WriteLine("invalid user ID");
                return -1;
            }
            if (!m_movies.ContainsKey(sIID))
            {
                Console.WriteLine("invalid Item ID");
                return -1;
            }
            if(m== PredictionMethod.Cosine || m == PredictionMethod.Pearson)
            {
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
                            wau = calcWCosine(sUID, uID);
                        //else random
                        numerator += (wau * m_ratings[uID][sIID]);
                    }
                
                }
                return (numerator / denominator);
            }
            else
            {
                return randomPredictRating(sUID,sIID);
            }

        }
        private double randomPredictRating(string sUID, string sIID)
        {
            Random r = new Random();
            double randomVal = r.NextDouble();
            int location = (int)randomVal * (m_ratings[sUID].Keys.Count-1);
            return m_ratings[sUID].ElementAt(location).Value;
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

        private double calcWCosine(string aID, string uID)
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
            Dictionary<PredictionMethod, double> ans = new Dictionary<PredictionMethod, double>();
            Dictionary<int, HashSet<int>> used = new Dictionary<int, HashSet<int>>();

            bool toContinue = true;
            int iterationNumber = 0;
            Random r = new Random();
            double pearsonMAE = 0;
            double cosineMAE = 0;
            double randomMAE = 0;
            while (toContinue)
            {
                iterationNumber++;
                bool foundNotUsed = false;
                string userID = "";
                string movieID = "";
                while (!foundNotUsed)
                {
                    double randomU = r.NextDouble();
                    double randomI = r.NextDouble();
                    int locationU = (int)randomU * (m_ratings.Keys.Count-1);
                    userID = m_ratings.Keys.ToList()[locationU]; //check if its better then ElementAt
                    int locationI = (int)randomI * (m_ratings[userID].Keys.Count-1);
                    if (!used.ContainsKey(locationU))                   
                        used.Add(locationU, new HashSet<int>());
                    else if(used[locationU].Contains(locationI))
                            continue;
                    used[locationU].Add(locationI);
                    movieID = m_ratings[userID].Keys.ToList()[locationI]; 
                    foundNotUsed = true;
                }
                double realRating = m_ratings[userID][movieID];
                //pearson
                double pearsonRating = PredictRating(PredictionMethod.Pearson, userID, movieID);
                double pearsonError = realRating - pearsonRating;
                //cossim
                double cosineRating = PredictRating(PredictionMethod.Cosine, userID, movieID);
                double cosineError = realRating - cosineRating;
                //random
                double randomRating = PredictRating(PredictionMethod.Pearson, userID, movieID);
                double randomError = realRating - randomRating;
                if(iterationNumber == 1)
                {
                    pearsonMAE = pearsonError;
                    cosineMAE = cosineError;
                    randomMAE = randomError;
                    continue;
                }
                double pearsonChange = ((pearsonMAE + pearsonError) / iterationNumber) - (pearsonMAE / iterationNumber - 1);
                double cosineChange = ((cosineMAE + cosineError) / iterationNumber) - (cosineMAE / iterationNumber - 1);
                double randomChange = ((randomMAE + randomError) / iterationNumber) - (randomMAE / iterationNumber - 1);
                pearsonMAE += pearsonError;
                cosineMAE += cosineError;
                randomMAE += randomError;
                if (pearsonChange < 0.1 && cosineChange < 0.1 && randomChange < 0.1) //halting condition
                    toContinue = false;

            }
            ans.Add(PredictionMethod.Cosine, cosineMAE / iterationNumber);
            ans.Add(PredictionMethod.Pearson, pearsonMAE / iterationNumber);
            ans.Add(PredictionMethod.Random, randomMAE / iterationNumber);
            return ans;
        }
    }
}
