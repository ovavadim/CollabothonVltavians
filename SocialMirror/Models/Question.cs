namespace SocialMirror.Models
{
    public class Question
    {
        public int Id { get; set; }
        public bool IsText { get; set; }
        public string Text { get; set; }
        public string ImagePath { get; set; }

        public List<Answer> Answers { get; set; }
        public Question() 
        { 
            Answers = new List<Answer>();
        }


    }
}
