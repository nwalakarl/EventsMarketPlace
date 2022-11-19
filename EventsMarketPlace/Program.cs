using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace EventsMarketPlace
{
    internal class Program
    {
        static List<Customer> customers = new List<Customer>{
                new Customer{ Name = "Nathan", City = "New York"},
                new Customer{ Name = "Bob", City = "Boston"},
                new Customer{ Name = "Cindy", City = "Chicago"},
                new Customer{ Name = "Lisa", City = "Los Angeles"}
            };


        static List<Event> events = new List<Event>{
                new Event{ Name = "Phantom of the Opera", City = "New York"},
                new Event{ Name = "Metallica", City = "Los Angeles"},
                new Event{ Name = "Metallica", City = "New York"},
                new Event{ Name = "Metallica", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "New York"},
                new Event{ Name = "LadyGaGa", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "Chicago"},
                new Event{ Name = "LadyGaGa", City = "San Francisco"},
                new Event{ Name = "LadyGaGa", City = "Washington"}
            };

        static void Main(string[] args)
        {
            

            Console.WriteLine("Hello, World!");

            Console.WriteLine("Welcome to our Ticketing System");
            Console.WriteLine("-------------------------------\n");

            int option = -1;

            while (option != 0)
            {
                // Display ticketing system action options.
                Console.WriteLine("Choose an option from the following list:");
                Console.WriteLine("\t1 - View all customers");
                Console.WriteLine("\t2 - View all events");
                Console.WriteLine("\t3 - View all event locations at City");
                Console.WriteLine("\t4 - Email events at Customer Location");
                Console.WriteLine("\t5 - Email 5 closest events at Customer Location");

                Console.WriteLine("\t0 - Quit");

                Int32.TryParse(Console.ReadLine(), out option);

                switch (option)
                {
                    case 1:
                        DisplayUsers();
                        break;
                    case 2:
                        DisplayEvents(option);
                        break;
                    case 3:
                        DisplayEvents(option);
                        break;
                    case 4:
                        EmailEvents(option);
                        break;
                    case 5:
                        EmailEvents(option);
                        break;
                }
            }

        }

        static void EmailEvents(int? option)
        {
            if (option > 2)
            {

                // Prompt to enter customer name.
                Console.WriteLine("Enter customer name, and then press Enter");
                string customerName = Console.ReadLine();

                var customer = GetCustomerByName(customerName);

                if (customer == null)
                {
                    Console.WriteLine($"{customerName} not found.");
                    return;
                }
                

                if (option == 4)
                {
                    List<Event> eventsByCity = GetEventsByCity(customer.City, events);

                    events.Sort(new EventPriceComparer());

                    foreach (var item in eventsByCity)
                    {
                        int? price = GetPrice(item);
                        AddToEmail(customer, item, price);
                    }
                }
                else if (option == 5)
                {
                    

                    List<Event> closestEvents = GetNClosestEvents(customer.City, events);

                    foreach (var item in closestEvents)
                    {
                        var price = GetPrice(item);
                        AddToEmail(customer, item, price);
                    }
                }
            }
        }

        static void DisplayUsers()
        {           
            customers.Sort();
            int count = 0;

            foreach (var item in customers)
            {                        
                count++;
                Console.Out.WriteLine($"{count} {item.Name} in {item.City}");
            }               
        }

        static void DisplayEvents(int? option)
        {
           
            if (option == 2)
            {
                events.Sort(new EventPriceComparer());
                int count = 0;

                foreach (var item in events)
                {
                    int? price = GetPrice(item);
                    count++;
                    Console.Out.WriteLine($"{count} {item.Name} in {item.City}"
                    + (price.HasValue ? $" for ${price}" : ""));
                }
            }
            else if (option == 3)
            {

                // Promt to enter customer city.
                Console.WriteLine("Enter customer city, and then press Enter");
                string customerCity = Console.ReadLine();

                var eventsAtCity = GetEventsByCity(customerCity, events);

                eventsAtCity.Sort(new EventPriceComparer());
                int count = 0;
                foreach (var item in eventsAtCity)
                {
                    int distance = GetDistance(item.City, customerCity);

                    int? price = GetPrice(item);
                    count++;
                    Console.Out.WriteLine($"{count} {item.Name} in {item.City}"
                        + (distance > 0 ? $" ({distance} miles away)" : "")
                    + (price.HasValue ? $" for ${price}" : ""));
                }
            }
        }

        /** Utility/API Services or functions **/
        private static Dictionary<string, int> CachedDistances = new Dictionary<string, int>();

        public static List<Event> GetNClosestEvents(string customerCity, List<Event> events, int n = 5)
        {
            List<Event> result = new List<Event>();
                       
            PriorityQueue<Event, int> priorityQueue = new PriorityQueue<Event, int>();

            foreach (Event e in events)
            {
                var distance = GetDistance(e.City, customerCity);

                priorityQueue.Enqueue(e, distance);
            }


            int count = 0;

            while (count < n)
            {
                result.Add(priorityQueue.Dequeue());

                count++;
            }

            result.Sort(new EventDistanceComparer(customerCity));

            return result;
        }

        public static int GetDistance(string fromCity, string toCity)
        {
            try
            {
                if (fromCity == null || toCity == null)
                {
                    return 0;
                }

                if (fromCity.ToLower() == toCity.ToLower())
                {
                    return 0;
                }

                string[] citiesArray = { fromCity, toCity };

                Array.Sort(citiesArray, (x, y) => x.CompareTo(y));

                string distanceCacheKey = String.Join('-',citiesArray);

                if (CachedDistances.ContainsKey(distanceCacheKey))
                {
                    return CachedDistances[distanceCacheKey];
                }


                return AlphabeticalDistance(fromCity, toCity);
            }
            catch (Exception)
            {
                // Returns a zero for the distance
                return 0;
            }

        }

        public static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);

            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (price.HasValue ? $" for ${price}" : ""));
        }

        public static int AlphabeticalDistance(string s, string t)
        {
            var result = 0;
            for (int i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                result += Math.Abs(s[i] - t[i]);
            }

            for (int i = 0; i < Math.Max(s.Length, t.Length); i++)
            {
                result += s.Length > t.Length ? s[i] : t[i];
            }
            return result;
        }

        public static int GetPrice(Event e)
        {
            return (AlphabeticalDistance(e.City, "") + AlphabeticalDistance(e.Name, "")) / 10;
        }

        public static List<Event> GetEventsByCity(string city, List<Event> events)
        {           

            var queryResult = from result in events
                              where result.City.ToLower() == city.ToLower()
                              select result;


            return queryResult.ToList();
        }

        public static Customer? GetCustomerByName(string customerName)
        {

            Customer? customer = customers.FirstOrDefault(c => c.Name.ToLower() == customerName.ToLower());

            return customer;
        }

        /** Comparators **/
        public class EventPriceComparer : IComparer<Event>
        {
            public int Compare(Event? x, Event? y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                return GetPrice(x) - GetPrice(y);
            }
        }

        public class EventDistanceComparer : IComparer<Event>
        {
            private string _relativeEventCity;
            public EventDistanceComparer(string relativeEventCity)
            {
                _relativeEventCity = relativeEventCity;
            }


            public int Compare(Event? x, Event? y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                int eventXDistance = 0;
                int eventYDistance = 0;

                if (_relativeEventCity != null)
                {
                    eventXDistance = GetDistance(_relativeEventCity, x.City);
                    eventYDistance = GetDistance(_relativeEventCity, y.City);
                }

                if (eventXDistance == eventYDistance)
                {
                    int eventXPrice = GetPrice(x);
                    int eventYPrice = GetPrice(y);

                    return eventXPrice - eventYPrice;
                }


                return eventXDistance - eventYDistance;
            }
        }


    }

    public class Event
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class Customer: IComparable<Customer>
    {
        public string Name { get; set; }
        public string City { get; set; }

        public int CompareTo(Customer? other)
        {
            return Name.CompareTo(other.Name);
        }
    }

}
