using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public virtual UserProfile UserProfile { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
