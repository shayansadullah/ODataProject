using Microsoft.OData.Client;
using Microsoft.OData.SampleService.Models.TripPin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ODataClientExample
{
    public class Trippin
    {
        static async Task Main(string[] args)
        {
            var serviceRoot = "https://services.odata.org/V4/TripPinServiceRW/";
            var context = new DefaultContainer(new Uri(serviceRoot));

            await GetAllByEntity(context);
            await GetUsingWithLinq(context, "Vincent");
            await GetBySimpleFilter(context, "Clyde");
            await GetByKey(context);
            await GetEntitySet(context);
            await GetByNavigation(context);
            await GetByBatchOperation();
            await GetCountOfEntity(context);
            await GetMoreLinq(context);
            await GetExpandExample(context);
            

        }

        static async Task GetAllByEntity(DefaultContainer context)
        {
            IEnumerable<Person> people = await context.People.ExecuteAsync();
            var order_by_first_name = people.OrderBy(c => c.FirstName);
            Console.WriteLine("ALL PEOPLE:");
            foreach (var person in order_by_first_name)
            {
                Console.WriteLine("{0} {1}", person.FirstName, person.LastName);
            }
            Console.WriteLine("------------------------------------------");
        }

        static async Task GetUsingWithLinq(DefaultContainer context, string name)
        {
            IEnumerable<Person> people = await context.People.ExecuteAsync();
            var people_vincent_only = people.Where(c => c.FirstName == name);

            foreach (var p in people_vincent_only)
            {
                Console.WriteLine($"SPECIFIC NAME: { name}");
                Console.WriteLine("{0} {1}", p.FirstName, p.LastName);
            }
            Console.WriteLine("------------------------------------------");
        }

        static async Task GetBySimpleFilter(DefaultContainer context, string lastname)
        {
            IEnumerable<Person> people = await context.People.ExecuteAsync();
            var people_with_last_name = people.Where(c => c.FirstName.EndsWith(lastname));
            foreach (var p in people_with_last_name)
            {
                Console.WriteLine($"SPECIFIC FIRST NAME ENDS WITH: { lastname }");
                Console.WriteLine("{0} {1}", p.FirstName, p.LastName);
            }
            Console.WriteLine("------------------------------------------");
        }

        static async Task GetByKey(DefaultContainer context)
        {
            Console.WriteLine("GET BY KEY:");
            var russell = await context.People.ByKey("russellwhyte").GetValueAsync();
            Console.WriteLine(russell.UserName);
            Console.WriteLine("------------------------------------------");
        }

        static async Task GetEntitySet(DefaultContainer context)
        {
            var response = await context.People.ExecuteAsync();
            foreach (var p in (response as QueryOperationResponse<Person>))
            {
                Console.WriteLine("GET BY USING BUILT IN QUERY OPERATION RESPONSE:");
                Console.WriteLine(p.UserName);
                Console.WriteLine("------------------------------------------");
            }
        }

        static async Task GetByNavigation(DefaultContainer context)
        {
            var me = await context.Me.GetValueAsync();
            await context.LoadPropertyAsync(me, "Trips");
            foreach (var t in me.Trips)
            {
                Console.WriteLine("GET BY USING LOAD PROPERTY OPERATOR??:");
                Console.WriteLine(t.Name);
                Console.WriteLine("------------------------------------------");
            }

        }

        static async Task GetByBatchOperation()
        {
            DefaultContainer context = new DefaultContainer(new Uri("https://services.odata.org/V4/(S(uvf1y321yx031rnxmcbqmlxw))/TripPinServiceRW/"));
            var peopleQuery = context.People;
            var airlinesQuery = context.Airlines;

            var batchResponse = await context.ExecuteBatchAsync(peopleQuery, airlinesQuery);

            Console.WriteLine("GET BY USING EXECUTING A BATCH AS ASYNC:");

            foreach (var r in batchResponse)
            {
                var people = r as QueryOperationResponse<Person>;
                if (people != null)
                {
                    Console.WriteLine("GET BY USING EXECUTING A BATCH AS ASYNC - PEOPLE ENTITY:");
                    foreach (Person p in people)
                    {
                        Console.WriteLine(p.UserName);
                    }
                }

                var airlines = r as QueryOperationResponse<Airline>;
                if (airlines != null)
                {
                    Console.WriteLine("GET BY USING EXECUTING A BATCH AS ASYNC - AIRLINE ENTITY:");
                    foreach (var airline in airlines)
                    {
                        Console.WriteLine(airline.Name);
                    }
                }
            }
            Console.WriteLine("------------------------------------------");
        }

        static async Task GetCountOfEntity(DefaultContainer context)
        {
            var peopleEntity = await context.People.ExecuteAsync();
            var airlineEntity = await context.Airlines.ExecuteAsync();
            Console.WriteLine($"GET TOTAL COUNT OF PEOPLE: {peopleEntity.Count()}");
            Console.WriteLine($"GET TOTAL COUNT OF AIRLINES: {airlineEntity.Count()}");
        }

        static async Task GetMoreLinq(DefaultContainer context)
        {
            IEnumerable<Person> people = await context.People.ExecuteAsync();

            var female_gender = people.Where(p => p.Gender == PersonGender.Male && p.FirstName == "Vincent")
                                      .OrderBy(p => p.UserName)
                                      .Select(p => new { p.FirstName, p.LastName, p.UserName, p.Gender });

            foreach (var person in female_gender)
            {
                Console.WriteLine($"Username: {person.UserName} First Name: {person.FirstName} Gender: {person.Gender}");
            }
        }

        static async Task GetExpandExample(DefaultContainer context)
        {
            IEnumerable<Person> people = await context.People.Expand(c => c.Trips)
                                                             .ExecuteAsync();

            var peoples = people.Where(c => c.FirstName == "Vincent")
                            .OrderBy(c => c.FirstName);


            foreach (var a in peoples)
            {
                var trips = a.Trips;
                Console.WriteLine("{0}, {1}", a.FirstName, a.LastName);
                
                foreach(var b in trips)
                {
                    Console.WriteLine("{0} {1}", b.TripId, b.Description);
                }
            }
        }
    }
}
