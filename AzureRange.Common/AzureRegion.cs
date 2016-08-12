using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureRange.Website
{
    public class AzureRegion
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Geography { get; set; }
    }
}