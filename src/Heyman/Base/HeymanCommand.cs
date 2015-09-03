namespace Heyman
{
    public class HeymanCommand
    {
        public HeymanCommand()
        {

        }
        public string Title { get; set; }
        public string Regex { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; }
        public string Arguments { get; set; }
        public string EndLine { get; set; }
        public string WorkingDirectory { get; set; }
    }
}