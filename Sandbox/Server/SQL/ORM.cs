using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Server.Data
{
    /*public class Path
    {
        [Key]
        public int ID { get; set; } 

        [ForeignKey("people")] public Person Person { get; set; }

        //creates table: One Path ID will have several locations
        public Location[] Locations { get; set; }
    }


    public class Person
    {
        [Key]
        public int ID { get; set; }

        [Key]
        public int IDType { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }

        //creates column: HomeLocation_PersonID and HomeLocation_LocationID
        [ForeignKey("locations")] 
        public Location HomeLocation { get; set; }         

    }

    public class Location
    {
        //Person_ID, Person_IDType, LocationID,  Name,Latitude,Longitude

        [Key]
        public int LocationID { get; set; }


        [ForeignKey("people")]
        public Person Person { get; set; }

        public string Name { get; set; }
        public long Latitude { get; set; }
        public long Longitude { get; set; }
        
    }

    public class IndividualExtensions
    {
        public static ISQL _sql;

        public static Dictionary<string,object> GetKeys(this object o)
        {
            Dictionary<string, object> o = new Dictionary<string, object>();
            foreach(var prop in o.GetType().GetProperties())
            {
                foreach(var attr in prop.GetCustomAttributes(false).Where(attr=>attr is KeyAttribute))
                {
                    o.Add(prop.Name, prop.GetValue(o));
                }
            }
            return o;
        }

        public static T Get<T>(this T item)
        {
            string table = typeof(T).Name;//((TableAttribute)typeof(T).GetCustomAttributes(false).First(obj => obj is TableAttribute)).TableName;

            string left_joins = "";
            string inner_joins = "";
            foreach(var prop in typeof(T).GetProperties())
            {
                if (prop.PropertyType.IsArray)
                {
                }
            }
            var keys = item.GetKeys();
            return item;
        }
    }*/
}
