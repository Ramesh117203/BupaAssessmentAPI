using System.Collections.Generic;

namespace BookApi.Models
{
    public class BookOwner
    {
        public string name { get; set; }
        public int age { get; set; }
        public List<Book> books { get; set; }
    }


    public class Book
    {
        public string name { get; set; }
        public string type { get; set; }
    }
}
