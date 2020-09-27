﻿namespace Iwentys.Models
{
    public class GroupName
    {
        public int Course { get; }
        public int Number { get; }
        public string Name { get; }
        
        public GroupName(string name)
        {
            //FYI: russian letter
            Name = name
                .ToUpper()
                .Substring(0, 5)
                .Replace("М", "M");

            Course = int.Parse(Name.Substring(2, 1));
            Number = int.Parse(Name.Substring(3, 2));
        }
    }
}