using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollingService.Model
{
    public class APIList
    {
        public string ApiURL { get; set; }
        public string ApiTag { get; set; }
        public int ApiTimer { get; set; }
        public DateTime LastRequestTime { get; set; }
    }
}
