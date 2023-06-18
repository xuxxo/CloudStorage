namespace FilesAPI.Contexts
{ 
    public class UserFile
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long UserId { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastTimeChanged { get; set; }
    }

}
