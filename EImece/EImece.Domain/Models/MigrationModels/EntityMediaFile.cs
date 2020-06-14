namespace EImece.Domain.Models.MigrationModels
{
    public class EntityMediaFile
    {
        public string CategoryName { get; set; }
        public string File_Type { get; set; }
        public string Modul_Name { get; set; }
        public string Mod { get; set; }
        public string Name { get; set; }
        public string File_Path { get; set; }
        public string File_Desc { get; set; }
        public string File_Name { get; set; }
        public string File_Format { get; set; }

        public override string ToString()
        {
            return "Name:" + Name + " CategoryName:" + CategoryName + " File_Path:" + File_Path;
        }
    }
}