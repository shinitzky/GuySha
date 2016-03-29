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
        private Dictionary<string, List<string>> movieToUser;
        private Dictionary<string, int> m_movies; //movie id and amount of ratings
        private Dictionary<string, double> cosineDenominator;


        private Dictionary<string,Dictionary<string,double>> raiDic_AllUsers; //KEY = USERID. VALUE = <MOVIE,VAL> SO THAT VALUE IS THE DIFFERENCE (RATING-AVARAGE)
        private Dictionary<string, double> currentAvarage;
            

        //constructor
        public RecommenderSystem()
        {
            m_ratings = new Dictionary<string, Dictionary<string, double>>(); //<User <Movie,Rating>>
            m_movies = new Dictionary<string, int>(); 
            m_userAvgs = new Dictionary<string, double>();
           movieToUser = new Dictionary<string, List<string>>();
            cosineDenominator = new Dictionary<string, double>();
            raiDic_AllUsers = new Dictionary<string, Dictionary<string, double>>();
            currentAvarage = new Dictionary<string, double>(); //not sure if we need it
            
            
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
                    calcRAI();
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

                    //added tomer
                    if (!cosineDenominator.ContainsKey(userId))
                    {
                        cosineDenominator.Add(userId, 0);
                    }
                    cosineDenominator[userId] = cosineDenominator[userId] + Math.Pow(rating,2);

                    //Added Tomer not sure needed
                   if (!movieToUser.ContainsKey(movieId))
                    {
                        movieToUser.Add(movieId, new List<string>());

                    }
                    movieToUser[movieId].Add(userId); 

                    //not sure if we need it
                    if (!currentAvarage.ContainsKey(userId))
                    {
                        currentAvarage.Add(userId, 0);
                    }
                    int currentRatedMoviesNum = m_ratings[userId].Keys.Count;
                    currentAvarage[userId] = currentAvarage[userId] + (rating / currentRatedMoviesNum);
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

        private void calcRAI()
        {
            foreach (string user in m_ratings.Keys)
            {
                double average = m_userAvgs[user];
                double value = 0;
                Dictionary<string, double> rai = new Dictionary<string, double>();
                foreach(string movie in m_ratings[user].Keys)
                {
                    value = m_ratings[user][movie]-average;
                    rai.Add(movie, value);
                    
                }
                raiDic_AllUsers.Add(user, rai);
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
        public double PredictRating(PredictionMethod m,string sUID, string sIID)
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
                /*
                double ra = m_userAvgs[sUID];
                Dictionary<string, double> raiDic = new Dictionary<string, double>();//<movieID,(activeUserRating-activeUserAverage)>
                //double cosineDenominatorRight = 0;
                if (m == PredictionMethod.Pearson) 
                {
                    foreach (string mID in m_ratings[sUID].Keys)
                    {
                        double val = m_ratings[sUID][mID] - ra;
                        raiDic.Add(mID, val);
                        //cosineDenominatorRight += Math.Pow(m_ratings[sUID][mID], 2); //why its in the if of pearson?? can be calculated in loading the dataset
                    }
                }
                */
                double numerator = 0; 
                double denominator = m_movies[sIID]; //check this!!

                //calc sum of w
                //foreach (string uID in m_ratings.Keys) //we can go here only on the users that also rated the movie!
                foreach (string uID in movieToUser[sIID])
                {
                    if (uID.Equals(sUID))
                        continue;
                    //if (m_ratings[uID].ContainsKey(sIID)) //if the other user also rated the movie
                    //{
                        double wau = 0;
                        if (m == PredictionMethod.Pearson)
                            wau = calcWPearson(sUID, uID);
                        else if (m == PredictionMethod.Cosine)
                            wau = calcWCosine(sUID, uID);                                              
                        double left = m_ratings[uID][sIID];
                        double right = wau;
                       // if (wau>0) //change this number and check
                            numerator =numerator+ ( left * right);
                   // }
                }
                return (numerator / denominator)+ m_userAvgs[sUID]; //should be Ra + num/dem
            }
            else//else random
            {
                return randomPredictRating(sUID,sIID);
            }
        }   

      

        private double randomPredictRating(string sUID, string sIID)
        {
            double sumOfRatings = m_ratings[sUID].Keys.Count * m_userAvgs[sUID];
            SortedSet<double> set = new SortedSet<double>();
            foreach(double rating in m_ratings[sUID].Values)
            {
                set.Add(rating / sumOfRatings);
            }
            Random r = new Random();
            double randomVal = r.NextDouble();
            double ans = 0;
            foreach(double d in set)
            {
                if(d>= randomVal)
                {
                    ans = d;
                    break;
                }
            }
            return ans;
        }
        private double calcWPearson(string aID, string uID)
        {
            Dictionary<string, double> raiDic = raiDic_AllUsers[aID];
            Dictionary<string, double> ruiDic = raiDic_AllUsers[uID];
            double numerator = 0;
            double denominatorLeft = 0;
            double denominatorRight = 0;
            //double ru = m_userAvgs[uID]; //get the average of the other user
            foreach(string mId in m_ratings[uID].Keys) 
            {
                if (!raiDic.ContainsKey(mId)) //only movies that they both rated
                    continue;
                double ruval = ruiDic[mId];
                double raval = raiDic[mId];
                numerator += (raiDic[mId] * ruiDic[mId]);
                denominatorLeft += Math.Pow(ruval, 2);
                denominatorRight+= Math.Pow(raval, 2);
            }
            double denominator = (Math.Sqrt(denominatorLeft)) * (Math.Sqrt(denominatorRight));
            if (denominator == 0) //throw exception?
                return 0; //check this
            return numerator/denominator;
        }

        private double calcWCosine(string aID, string uID) 
        {
            double numerator = 0;
            //double denominatorLeft = 0;
            foreach (string mId in m_ratings[uID].Keys)
            {
                double rui = m_ratings[uID][mId];
                //denominatorLeft += Math.Pow(rui, 2);
                if (!m_ratings[aID].ContainsKey(mId))
                    continue;
                double rai = m_ratings[aID][mId];
                numerator += (rui * rai);
            }
            double denominator = (cosineDenominator[uID] * cosineDenominator[aID]);
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
                double pearsonError = Math.Abs(realRating - pearsonRating); //should be absolute value
                pearsonMAE += pearsonError;
                //cossim
                double cosineRating = PredictRating(PredictionMethod.Cosine, userID, movieID);
                double cosineError = Math.Abs(realRating - cosineRating);
                cosineMAE += cosineError;
                //random
                double randomRating = PredictRating(PredictionMethod.Random , userID, movieID);
                double randomError = Math.Abs(realRating - randomRating);
                randomMAE += randomError;
            }
            ans.Add(PredictionMethod.Cosine, cosineMAE / cTrials);
            ans.Add(PredictionMethod.Pearson, pearsonMAE / cTrials);
            ans.Add(PredictionMethod.Random, randomMAE / cTrials);
            return ans;
        }
    }
}
