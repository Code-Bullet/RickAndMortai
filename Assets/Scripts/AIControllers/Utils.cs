﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AIControllers
{
    public static class Utils
    {
        // takes the chatgpt output and seperates it into a string array, each string in the array is a new dialog line or an action.
        // this also is where we remove a bunch of nono
        public static string[] ProcessOutputIntoStringArray(string chatgptOutputMessage, ref string message)
        {
            // message = messages[messages.Count - 1].Content;
            message = chatgptOutputMessage;
            Debug.Log(message);
            // textField.text = message;

            message = message.Replace("frick", "fuck");
            message = message.Replace("Frick", "Fuck");
            message = message.Replace("Freakin", "Fuckin");
            message = message.Replace("freakin", "fuckin");
            message = message.Replace("crap", "shit");
            message = message.Replace("Crap", "Shit");
            message = message.Replace("shoot", "shit");
            message = message.Replace("Shoot", "Shit");
            message = message.Replace("Nigger", "nope");
            message = message.Replace("Nigga", "nope");
            message = message.Replace("nigga", "nope");
            message = message.Replace("Niger", "nope");
            message = message.Replace("nigger", "nope");
            message = message.Replace("niger", "nope");
            message = message.Replace("negro", "nope");
            message = message.Replace("Negro", "nope");
            message = message.Replace("migger", "mope");
            message = message.Replace("migga", "mope");
            message = message.Replace("migga", "mope");
            message = message.Replace("miger", "mope");
            message = message.Replace("migger", "mope");
            message = message.Replace("miger", "mope");
            message = message.Replace("megro", "mope");
            message = message.Replace("megro", "mope");
            message = message.Replace("faggot", "fnope");
            message = message.Replace("Faggot", "fnope");
            message = message.Replace("feggot", "fnope");
            message = message.Replace("Feggot", "fnope");
            message = message.Replace("fagot", "fnope");
            message = message.Replace("Fagot", "fnope");
            message = message.Replace("Fogot", "fnope");
            message = message.Replace("fogot", "fnope");
            message = message.Replace("panigerism", "nope");
            message = message.Replace("Nick G", "nope");
            message = message.Replace("nick g", "nope");
            message = message.Replace("Nick g", "nope");
            message = message.Replace("nick g", "nope");

            char[] delims = new[] { '\r', '\n' };
            string[] outputLinesProcessed = message.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            return outputLinesProcessed;
        }
    }
}
