
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;
using System;

namespace StoryCallouts
{
    public class CharacterData
    {
        public string Model;
        public string Forename;
        public string Surname;
        public DateTime Birthday;
        public Gender Gender;
        public Persona Persona;

        public CharacterData(string model, string forename, string surname, Gender gender, DateTime birthday)
        {
            Model = model;
            Forename = forename;
            Surname = surname;
            Gender = gender;
            Birthday = birthday;
            if (birthday == new DateTime(1, 1, 1))
                Persona = new Persona(Forename, Surname, Gender, Model);
            else
                Persona = new Persona(Forename, Surname, Gender, Birthday, Model);
        }
    }
    public class Characters
    {
        public static readonly CharacterData Micheal =
            new CharacterData("player_zero", "Michael", "De Santa", Gender.Male, new DateTime(1962, 3, 1));

        public static readonly CharacterData Franklin =
            new CharacterData("player_one", "Franklin", "Clinton", Gender.Male, new DateTime(1988, 7, 28));
        
        public static readonly CharacterData Trevor =
            new CharacterData("player_two", "Trevor", "Philips", Gender.Male, new DateTime(1, 1, 1));
        
        public static readonly CharacterData Lamar =
            new CharacterData("ig_lamardavis", "Lamar", "Davis", Gender.Male, new DateTime(1, 1, 1));
        
        public static readonly CharacterData Stretch =
            new CharacterData("ig_stretch", "Harold", "Joseph", Gender.Male, new DateTime(1, 1, 1));
    }

}
