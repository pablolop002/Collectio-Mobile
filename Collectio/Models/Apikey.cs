using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using Xamarin.Essentials;

namespace Collectio.Models
{
    public class Apikey
    {
        [PrimaryKey] public string Token { get; set; }
        
        [ForeignKey(typeof(User))] public int UserId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UsedAt { get; set; }
        
        public string Device { get; set; }
        
        public string UserDeviceName { get; set; }

        [Ignore] public bool ThisDevice => Token.Equals(App.Token);
    }
}