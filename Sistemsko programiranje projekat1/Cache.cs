using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_projekat1
{
    internal class Cache
    {
        public LinkedList<string> ListOfKeys;
        public static Dictionary<string, EuropeanaMapper> cache;
        public int sizeOfCache { get; set;}

        private static readonly object _lock = new object();

        public Cache(int sizeForCache)
        {
            sizeOfCache = sizeForCache;
            cache = new Dictionary<string, EuropeanaMapper>(sizeOfCache);
            ListOfKeys = new LinkedList<string>();
        }


        public bool checkForKey(string key)
        {
            lock (_lock)
            {

                if (ListOfKeys.Count == 0)
                    return false;


                foreach (string it in ListOfKeys)
                {
                    if (it == key)
                        return true;

                }

                return false;
            }
        }


        public EuropeanaMapper getDataFromCache(string key)
        {
            //Stavljamo na kraj liste posto je najskorije iskoriscen kljuc
            lock (_lock)
            {
                ListOfKeys.Remove(key);
                ListOfKeys.AddLast(key);

                return cache[key];
            }
        }


        public void addToCache(string key, EuropeanaMapper value)
        {
            //Ako je pun "kes" onda izbaci kljuc koji je najdavnije koriscen sto je na pocetak liste
            lock (_lock)
            {

                try
                {
                    if (sizeOfCache == ListOfKeys.Count)
                    {
                        Logger.Log("Cache is full, replacing the least recently used element");
                        cache.Remove(ListOfKeys.First());
                        ListOfKeys.RemoveFirst();
                    }

                    ListOfKeys.AddLast(key);
                    cache.Add(key, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while adding and element to cache" + e.Message);
                }
            }

        }

    }
}
