using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blogifier.Shared.Domain
{
    public class Configuration
    {
        [Key]
        public string Name { get; set;}

        public bool Active { get; set; }
    }
}
