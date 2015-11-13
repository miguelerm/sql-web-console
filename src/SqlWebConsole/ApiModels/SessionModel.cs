using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqlWebConsole.ApiModels
{
    public class SessionModel
    {
        public string Id { get; set; }
        public UserProfile User { get; set; }
        public DateTime Date { get; set; }

        public SessionModel()
        {
            Date = DateTime.UtcNow;
        }
    }
}