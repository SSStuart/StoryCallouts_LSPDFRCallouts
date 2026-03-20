
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using System;

namespace StoryCallouts
{
    public class CharacterDefinition
    {
        public string Model { get; }
        public string Forename { get; }
        public string Surname { get; }
        public Gender Gender { get; }
        public DateTime Birthdate { get; }

        public CharacterDefinition(string model, string forename, string surname, Gender gender, DateTime birthdate)
        {
            Model = model;
            Forename = forename;
            Surname = surname;
            Gender = gender;
            Birthdate = birthdate;
        }
        public Ped Create(Vector3 postion, int heading, string callout, bool blockPermanentEvents = true)
        {
            Ped character = new Ped(Model, postion, heading)
            {
                IsPersistent = true,
                BlockPermanentEvents = blockPermanentEvents,
            };
            Functions.SetPersonaForPed(character, new Persona(Forename, Surname, Gender, Birthdate, Model));

            // Set model variations
            character.ResetVariation();
            switch (callout, Forename)
            {
                // PROLOGUE
                case ("Prologue", "Michael") :
                    character.SetVariation(0, 0, 3);
                    character.SetVariation(2, 5, 0);
                    character.SetVariation(3, 31, 0);
                    character.SetVariation(4, 26, 0);
                    character.SetVariation(5, 5, 0);
                    character.SetVariation(6, 14, 0);
                    character.SetVariation(9, 12, 0);
                    break;
                case ("Prologue", "Trevor") :
                    character.SetVariation(0, 0, 5);
                    character.SetVariation(2, 1, 0);
                    character.SetVariation(3, 9, 0);
                    character.SetVariation(4, 9, 0);
                    character.SetVariation(5, 4, 0);
                    character.SetVariation(6, 12, 0);
                    character.SetVariation(8, 13, 0);
                    character.SetVariation(9, 1, 0);
                    break;
                case ("Prologue", "Brad"):
                    character.SetVariation(2, 1, 0);
                    character.SetVariation(6, 1, 0);
                    character.SetVariation(9, 1, 0);
                    break;

                // FRANKLIN AND LAMAR
                case ("FranklinAndLamar", "Franklin") :
                    character.SetVariation(0, 0, 5);
                    character.SetVariation(1, 3, 0);
                    character.SetVariation(2, 2, 0);
                    character.SetVariation(3, 7, 10);
                    character.SetVariation(4, 20, 8);
                    character.SetVariation(6, 6, 5);
                    character.SetVariation(8, 14, 0);
                    break;
                case ("FranklinAndLamar", "Lamar"):
                    character.SetVariation(5, 2, 0);
                    break;

                // THE LONG STRETCH
                case ("TheLongStretch", "Franklin"):
                    character.SetVariation(3, 17, 2);
                    character.SetVariation(4, 8, 0);
                    character.SetVariation(6, 11, 0);
                    character.SetVariation(8, 14, 0);
                    break;
                case ("TheLongStretch", "Lamar"):
                    character.SetVariation(1, 1, 0);
                    character.SetVariation(2, 2, 0);
                    character.SetVariation(3, 2, 2);
                    character.SetVariation(4, 5, 0);
                    character.SetVariation(6, 1, 0);
                    break;

                // CASING THE JEWEL STORE
                case ("CasingTheJewelStore", "Franklin"):
                    character.SetVariation(0, 0, 5);
                    character.SetVariation(1, 3, 0);
                    character.SetVariation(2, 2, 0);
                    character.SetVariation(3, 18, 0);
                    character.SetVariation(4, 15, 0);
                    character.SetVariation(5, 4, 0);
                    character.SetVariation(6, 8, 1);
                    character.SetVariation(8, 14, 0);
                    character.SetVariation(9, 6, 0);
                    character.SetVariation(11, 5, 0);
                    break;
                case ("CasingTheJewelStore", "Eddie"):
                    character.SetVariation(3, 4, 2);
                    character.SetVariation(4, 3, 2);
                    character.SetVariation(8, 1, 0);
                    character.SetVariation(9, 1, 0);
                    character.SetVariation(10, 1, 0);
                    break;
                case ("CasingTheJewelStore", "Karim"):
                    character.SetVariation(0, 1, 0);
                    character.SetVariation(3, 4, 2);
                    character.SetVariation(4, 3, 2);
                    character.SetVariation(8, 1, 0);
                    character.SetVariation(9, 1, 0);
                    character.SetVariation(10, 1, 0);
                    break;
                case ("CasingTheJewelStore", "Gustavo"):
                    character.SetVariation(0, 4, 0);
                    character.SetVariation(1, 1, 0);
                    character.SetVariation(2, 1, 0);
                    character.SetVariation(3, 5, 0);
                    character.SetVariation(4, 5, 0);
                    character.SetVariation(6, 1, 0);
                    character.SetVariation(9, 4, 0);
                    character.SetVariation(10, 1, 0);
                    break;
                case ("CasingTheJewelStore", "Patrick"):
                    character.SetVariation(0, 5, 0);
                    character.SetVariation(1, 1, 0);
                    character.SetVariation(2, 1, 0);
                    character.SetVariation(3, 5, 0);
                    character.SetVariation(4, 5, 0);
                    character.SetVariation(6, 1, 0);
                    character.SetVariation(9, 4, 0);
                    character.SetVariation(10, 1, 0);
                    break;
                case ("CasingTheJewelStore", "Michael"):
                    character.SetVariation(0, 0, 2);
                    character.SetVariation(1, 3, 0);
                    character.SetVariation(2, 3, 0);
                    character.SetVariation(3, 0, 3);
                    character.SetVariation(4, 0, 3);
                    character.SetVariation(5, 8, 0);
                    character.SetVariation(11, 1, 0);
                    break;
                case ("CasingTheJewelStore", "Norm"):
                    character.SetVariation(1, 1, 0);
                    character.SetVariation(2, 1, 0);
                    character.SetVariation(3, 5, 0);
                    character.SetVariation(4, 5, 0);
                    character.SetVariation(6, 1, 0);
                    character.SetVariation(10, 1, 0);
                    break;

                // DEAD MAN WALKING
                case ("DeadManWalking", "Michael"):
                    character.SetVariation(0, 0, 2);
                    character.SetVariation(1, 3, 0);
                    character.SetVariation(2, 3, 0);
                    character.SetVariation(3, 19, 0);
                    break;

                // PACK MAN
                case ("PackMan", "Franklin"):
                    // ...
                    break;
                case ("PackMan", "Trevor"):
                    character.SetVariation(4, 23, 0);
                    character.SetVariation(6, 10, 0);
                    break;
                case ("PackMan", "Lamar"):
                    character.SetVariation(5, 2, 0);
                    break;

                default:
                    break;
            }

            return character;
        }
    }

    public static class Characters
    {
        public static readonly CharacterDefinition Michael =
            new CharacterDefinition("player_zero", "Michael", "De Santa", Gender.Male, new DateTime(1962, 3, 1));

        public static readonly CharacterDefinition Franklin =
            new CharacterDefinition("player_one", "Franklin", "Clinton", Gender.Male, new DateTime(1988, 7, 28));
        
        public static readonly CharacterDefinition Trevor =
            new CharacterDefinition("player_two", "Trevor", "Philips", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Brad =
            new CharacterDefinition("ig_brad", "Bradley", "Snider", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Lamar =
            new CharacterDefinition("ig_lamardavis", "Lamar", "Davis", Gender.Male, new DateTime(1, 1, 1));
        
        public static readonly CharacterDefinition Stretch =
            new CharacterDefinition("ig_stretch", "Harold", "Joseph", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Eddie =
            new CharacterDefinition("u_m_m_edtoh", "Eddie", "Toh", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Karim =
            new CharacterDefinition("hc_driver", "Karim", "Denz", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Gustavo =
            new CharacterDefinition("hc_gunman", "Gustavo", "Mota", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Patrick =
            new CharacterDefinition("hc_gunman", "Patrick", "McReary", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Norm =
            new CharacterDefinition("hc_gunman", "Norm", "Richards", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Paige =
            new CharacterDefinition("ig_paige", "Paige", "Harris", Gender.Female, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Christian =
            new CharacterDefinition("hc_hacker", "Christian", "Feltz", Gender.Male, new DateTime(1, 1, 1));

        public static readonly CharacterDefinition Rickie =
            new CharacterDefinition("ig_lifeinvad_01", "Rickie", "Lukens", Gender.Male, new DateTime(1, 1, 1));
    }

}
