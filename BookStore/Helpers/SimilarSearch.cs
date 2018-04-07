using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Helpers
{
    public class SimilarSearch
    {
        private String key;
        private List<String> list;

        public SimilarSearch(String key, List<String> list)
        {
            this.key = key.ToLower();
            this.list = list;
        }
        //tách từ
        private List<List<String>> tokenizer()
        {
            List<List<String>> docs = new List<List<String>>();
            docs.Add(key.Split(' ').ToList());
            foreach (String element in list)
            {
                string[] doc = element.ToLower().Split(' ');
                docs.Add(doc.ToList());
            }
            
            return docs;
        }
        //tính tần suất xuất hiện của từ
        private Double calcTF(String term, List<String> doc)
        {
            int count = 0;
            foreach(String element in doc)
            {
                if (element.Equals(term))
                {
                    count++;
                }
            }
            return count / (Double)doc.Count();
        }
        //chuyển từ thành vector
        private List<Dictionary<String, Double>> convertToVector(List<List<String>> docs)
        {
            List<Dictionary<String, Double>> docsDict = new List<Dictionary<string, double>>();
            foreach(List<String> doc in docs)
            {
                Dictionary<String, Double> docDict = new Dictionary<string, double>();
                foreach(String term in doc)
                {
                    docDict.Add(term, calcTF(term, doc));
                }
                docsDict.Add(docDict);
            }
            return docsDict;
        }
        //tính cosine giữa 2 vector
        private Double calcCosine(Dictionary<String, Double> leftVec, Dictionary<String, Double> rightVec)
        {
            List<String> intersection = new List<String>();
            Double product = 0.0;
            Double d1 = 0.0, d2 = 0.0;
            List<String> leftVal = leftVec.Keys.ToList();
            List<String> rightVal = rightVec.Keys.ToList();
            intersection = leftVal.Intersect(rightVal).ToList();
            
            foreach(String element in intersection)
            {
                product += leftVec[element] * rightVec[element];
            }
            foreach(Double element in leftVec.Values)
            {
                d1 += Math.Pow(element, 2.0);
            }
            foreach(Double element in rightVec.Values)
            {
                d2 += Math.Pow(element, 2.0);
            }
            return (Double)product / (Math.Sqrt(d1)*Math.Sqrt(d2));
        }
        //tính độ tương đồng của text
        public List<String> calcSimilarity()
        {
            List<String> returnList = new List<string>();
            SortedDictionary<String, Double> result = new SortedDictionary<string, double>();
            List<Dictionary<String, Double>> listVector = convertToVector(tokenizer());
            int index = 0;
            foreach(Dictionary<String, Double> leftVector in listVector)
            {
                foreach(Dictionary<String, Double> rightVector in listVector)
                {
                    if(index == 0)
                    {
                        index++;
                        continue;
                    }
                    else
                    {
                        if(calcCosine(leftVector, rightVector) > 0)
                        {
                            double l = calcCosine(leftVector, rightVector);
                            returnList.Add(list[index-1]);
                            
                        }
                    }
                    index++;
                }
                break;
            }
            return returnList;
        }

        //-------------------------------------------------------------------------------------------------
        //Deprecated
        //Giải thuật LevenshteinDistance tính độ sai khác giữa hai String
        //Input: 2 chuỗi
        //Output: số lượng kí tự sai khác
        private int LevenshteinDistance(String s, String t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
        //Deprecated
        //trả về list các chuỗi tương tự với từ khóa (key)
        //distance: số lượng kí tự sai khác
        public List<String> getResult(int distance)
        {
            //SortedDictionary<int, String> result = new SortedDictionary<int, string>();
            List<String> listStr = new List<String>();
            foreach(String element in list){
                int value = LevenshteinDistance(key, element);
                
                if(value <= distance)
                {
                    listStr.Add(element);
                }
               
            }
            //result.OrderBy(key => key.Key);
            //listStr.AddRange(result.Values);
            return listStr;
        }
    }
}