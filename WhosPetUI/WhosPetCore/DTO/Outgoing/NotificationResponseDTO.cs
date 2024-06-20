using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Outgoing
{
    public class NotificationResponseDTO
    {
        public int ReportId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
       
    }
}
