using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Data
{
    public class User
    {
        public int id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash{get;set;}= new byte[0];
        public byte[] PasswordSalt { get; set; }= new byte[0];
        public List<Character>? Characters {get;set;}
    }
}