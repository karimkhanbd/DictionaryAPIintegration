namespace Domain.Entities
{
    public  class Definition
    {
        public string PartOfSpeech { get; set; }
        public string Text { get; set; }        
        public string Example { get; set; }
        public IEnumerable<string> Synonyms { get; set; } = new List<string>();
        public IEnumerable<string> Antonyms { get; set; } = new List<string>();
    }
}
