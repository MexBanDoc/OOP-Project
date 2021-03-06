﻿using System;
using System.Text;

namespace Mafia.Domain
{
    public class CrazyInteraction : IInteraction
    {
        public PersonState Interact(IPerson person)
        {
            var name = new StringBuilder();
            for (var i = 0; i < person.Name.Length; i++)
            {
                if (char.IsLetter(person.Name[i]))
                {
                    name.Append(i % 2 == 0 ? char.ToUpper(person.Name[i]) : char.ToLower(person.Name[i]));
                }
                else if (person.Name[i] == ' ')
                {
                    name.Append(" ");
                }
                else
                {
                    name.Append(person.Name[i]);
                }
            }
            
            person.Name = name.ToString();

            return PersonState.Immortal;
        }
    }
}