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
        private Dictionary<string, Dictionary<string, double>> m_ratings; //users to movies
        private Dictionary<string, double> m_userAvgs;
        //private Dictionary<string, List<string>> movieToUser;
        private Dictionary<string, int> m_movies; //movie id and amount of ratings

        //constructor
        public RecommenderSystem()
        {
            m_ratings = new Dictionary<string, Dictionary<string, double>>(); //<User <Movie,Rating>>
            m_movies = new Dictionary<string, int>(); 
            m_userAvgs = new Dictionary<string, double>();
           // movieToUser = new Dictionary<string, List<string>>();
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

                else
                    Console.WriteLine("invalid file name");
            }
            catch(Exception e)
            {
                Console.WriteLine("Couldn't load file...sorry:)");
            }
        }
      
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

                    //Added Tomer //calculate Weights
                   /* if (!movieToUser.ContainsKey(movieId))
                    {
                        movieToUser.Add(movieId, new List<string>());

                    }
                    movieToUser[movieId].Add(userId); */
                }
                line = sr.ReadLine();
            }
            sr.Close();

        }
        private void calcAvgs() //for the initial calculations ufter loading ratings file
        {
            //calc avg ratings for each user - the sums foreach user has already calculated
            foreach(string userId in m_ratings.Keys) //tomer:  maybe it can be done in the function above
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
                Dictionary<string, double> raiDic = new Dictionary<string, double>();//<movieID,(activeUserRating-activeUserAverage)>
                double cosineDenominatorRight = 0;
                if (m == PredictionMethod.Pearson)
                {
                    foreach (string mID in m_ratings[sUID].Keys)
                    {
                        double val = m_ratings[sUID][mID] - ra;
                        raiDic.Add(mID, val);
                        cosineDenominatorRight += Math.Pow(m_ratings[sUID][mID], 2);
                    }
                }
                double numerator = 0; //?
                double denominator = m_movies[sIID]; //?

                //calc sum of w
                foreach (string uID in m_ratings.Keys)
                {
                    if (uID.Equals(sUID))
                        continue;
                    if (m_ratings[uID].ContainsKey(sIID)) //if the other user also rated the movie
                    {
                        double wau = 0;
                        if (m == PredictionMethod.Pearson)
                            wau = calcWPearson(sUID, uID, raiDic);
                        else if (m == PredictionMethod.Cosine)
                            wau = calcWCosine(sUID, uID, cosineDenominatorRight);                                              
                        double left = m_ratings[uID][sIID];
                        double right = wau;
                        if (wau>0.01) //change this number and check
                            numerator =numerator+ ( left * right);
                    }
                }
                return (numerator / denominator); //should be Ra + num/dem
            }
            else//else random
            {
                return randomPredictRating(sUID,sIID);
            }
        }   

                //predict a rating for a user item pair using the specified method
        /*public double PredictRating2(PredictionMethod m, string sUID, string sIID)
        {
            if (!m_ratings.ContainsKey(sUID))
            {
                Console.WriteLine("invalid user ID");
                return -1;
            }
            if (!m_movies.ContainsKey(sIID))
            {
                Console.WriteLine("invalid Item ID");
                return -1;
            }
            if (m == PredictionMethod.Pearson)
            {
                double ra = m_userAvgs[sUID];// user avarage
                                             //Dictionary<string, double> raiDic = new Dictionary<string, double>();//<movieID,(activeUserRating-activeUserAverage)>

                Dictionary<string, double> weights = new Dictionary<string, double>();
                foreach (string mID in m_ratings[sUID].Keys)//should be only on movies that they both rated. not all the movies of the active user
                {
                    List<string> users = movieToUser[mID]; //all the users that also rated this movie
                 }
                
                double numerator = 0; //?
                double denominator = m_movies[sIID]; //?

                //calc sum of w
                foreach (string uID in m_ratings.Keys)
                {
                    if (uID.Equals(sUID))
                        continue;
                    if (m_ratings[uID].ContainsKey(sIID)) //if the other user also rated the movie
                    {
                        double wau = 0;
                        if (m == PredictionMethod.Pearson)
                            wau = calcWPearson(sUID, uID, raiDic, pearsonDenominatorRight);
                        else if (m == PredictionMethod.Cosine)
                            wau = calcWCosine(sUID, uID);

                        //numerator += (wau * m_ratings[uID][sIID]);

                        double left = m_ratings[uID][sIID];
                        double right = wau;
                        if (wau > 0) //only if the calculations are not divided by zero in the functions below
                            numerator = numerator + (left * right);
                    }

                }
                return (numerator / denominator); //should be Ra + num/dem
            }
            else//else random
            {
                return randomPredictRating(sUID, sIID);
            }

        }*/

        private double randomPredictRating(string sUID, string sIID)
        {
            Random r = new Random();
            double randomVal = r.NextDouble();
            int location = (int)randomVal * (m_ratings[sUID].Keys.Count-1);
            return m_ratings[sUID].ElementAt(location).Value;
        }
        private double calcWPearson(string aID, string uID, Dictionary<string, double> raiDic)
        { 
            double numerator = 0;
            double denominatorLeft = 0;
            double denominatorRight = 0;
            double ru = m_userAvgs[uID]; //get the average of the other user
            foreach(string mId in m_ratings[uID].Keys) 
            {
                if (!raiDic.ContainsKey(mId)) //only movies that they both rated
                    continue;
                double ruval = (m_ratings[uID][mId] - ru);
                double raval = raiDic[mId];
                numerator += (ruval * raval);
                denominatorLeft += Math.Pow(ruval, 2);
                denominatorRight+= Math.Pow(raval, 2);
            }
            double denominator = (Math.Sqrt(denominatorLeft)) * (Math.Sqrt(denominatorRight));
            if (denominator == 0) //throw exception?
                return 0; //check this
            return numerator/denominator;
        }

        private double calcWCosine(string aID, string uID, double denominatorRight)
        {
            double numerator = 0;
            double denominatorLeft = 0;
            foreach (string mId in m_ratings[uID].Keys)
            {
                double rui = m_ratings[uID][mId];
                denominatorLeft += Math.Pow(rui, 2);
                if (!m_ratings[aID].ContainsKey(mId))
                    continue;
                double rai = m_ratings[aID][mId];
                numerator += (rui * rai);
            }
            double denominator = (Math.Sqrt(denominatorLeft)) * (Math.Sqrt(denominatorRight));
            if (denominator == 0)
                return 0;
            return numerator / denominator;
        }

        //Compute MAE (mean absolute error) for a set of rating prediction methods over the same user-item pairs
        //cTrials specifies the number of user-item pairs to be tested
        public Dictionary<PredictionMethod, double> ComputeMAE(List<PredictionMethod> lMethods, int cTrials)
        {
            Dictionary<PredictionMethod, double> ans = new Dictionary<PredictionMethod, double>();
            Dictionary<int, HashSet<int>> used = new Dictionary<int, HashSet<int>>();             
            int iterationNumber = 0;
            Random r = new Random();
            double pearsonMAE = 0;
            double cosineMAE = 0;
            double randomMAE = 0;
            while (iterationNumber<cTrials)//changes - hilla
            {
                iterationNumber++;
                bool foundNotUsed = false;
                string userID = "";
                string movieID = "";
                while (!foundNotUsed)
                {
                    double randomU = r.NextDouble();
                    double randomI = r.NextDouble();
                    int locationU = (int)(randomU * (m_ratings.Keys.Count-1)); 
                    userID = m_ratings.Keys.ToList()[locationU]; //check if its better then ElementAt
                    int locationI = (int)(randomI * (m_ratings[userID].Keys.Count-1));
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
                pearsonMAE += pearsonError;
                //cossim
                double cosineRating = PredictRating(PredictionMethod.Cosine, userID, movieID);
                double cosineError = realRating - cosineRating;
                cosineMAE += cosineError;
                //random
                double randomRating = PredictRating(PredictionMethod.Pearson, userID, movieID);
                double randomError = realRating - randomRating;
                randomMAE += randomError;
            }
            ans.Add(PredictionMethod.Cosine, cosineMAE / cTrials);
            ans.Add(PredictionMethod.Pearson, pearsonMAE / cTrials);
            ans.Add(PredictionMethod.Random, randomMAE / cTrials);
            return ans;
        }
    }
}
