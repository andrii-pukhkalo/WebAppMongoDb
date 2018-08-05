using System.Collections.Generic;

namespace WebAppMongoDb.Models {

    public class ComputerList
    {
        public IEnumerable<Computer> Computers { get; set; }
        public ComputerFilter Filter { get; set; }
    }
}