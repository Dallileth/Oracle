using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Shared.Models
{
    public enum IDType { User, Admin }
    public static class IDTypeExtensions
    {
        public static string[] GetAccess(this IDType type)
        {
            return type switch
            {                
                IDType.User => new string[] { "User" },
                IDType.Admin => new string[] { "Admin","User" },
                _ => new string[] {}
            };
        }
    }
    public class Individual : IEquatable<Individual>
    {
        public int ID { get;  set; }
        public IDType IDType { get;  set; }
        public string Name { get;  set; }
        public string Email { get;  set; }


        public string[] UIAccess { get; set; }

        //From Json
        public Individual() { }

        //From SQL
        public Individual(int id,IDType idtype,string email,params string[]roles)
        {
            ID = id;
            IDType = idtype;
            Email = email;
            UIAccess = roles;
        }

        public Claim[] GetClaims()
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, $"{(int)IDType}.{ID}")); //eg. 0.123 (Members.123)
            //claims.Add(new Claim(ClaimTypes.NameIdentifier,ID.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, Name)); //eg. Dennis Benson
            claims.Add(new Claim(ClaimTypes.Email, Email)); //eg. dbenson.software@gmail.com


            //claims.Add(new Claim(ClaimTypes.Role, $"['{string.Join("\',\'", UIAccess)}']"));
            foreach(var role in UIAccess)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var ret= claims.ToArray();
            return ret;
        }



        public bool Equals(Individual other)
        {
            return ID == other?.ID && IDType == other?.IDType;
        }
    }

    public static class IndividualExtensions
    {
        //From Client.MyAuth
        //From Server.Controllers (to figure out who is calling)
        public static Individual ToIndividual(this ClaimsPrincipal principal)
        {

            Individual individual = new Individual();
            foreach (var claim in principal.Claims)
            {
                switch (claim.Type)
                {
                    //note: clients detect 'nameid'
                    //servers detect nameidentifier
                    case "nameid":
                    case ClaimTypes.NameIdentifier:
                        var ids = claim.Value.Split('.');
                        individual.IDType = (IDType)int.Parse(ids[0]);
                        individual.ID = int.Parse(ids[1]);
                        //individual.ID = int.Parse(claim.Value);
                        break;
                    case ClaimTypes.Name:
                    case "unique_name":
                        individual.Name = claim.Value;
                        break;
                    case "email":
                    case ClaimTypes.Email:
                        individual.Email = claim.Value;
                        break;
                    case ClaimTypes.Role:
                    case "role":
                        string str = claim.Value;
                        if (str[0] == '[')
                        {
                            str = str.Substring(2, str.Length - 4);
                            individual.UIAccess = str.Split("\",\"");
                        }
                        else
                        {
                            individual.UIAccess = new string[] { str };
                        }
                        break;
                }
            }
            return individual;
        }




    }
}
