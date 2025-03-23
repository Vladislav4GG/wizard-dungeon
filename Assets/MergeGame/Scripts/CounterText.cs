using System;
using UnityEngine;

public static class CounterText
{
    public static string UpdateText(decimal value)
    {
        string number = value.ToString();
        int numDigits = number.Length;
        
        
        int pos = 0;//store digit grouping
        string place = "";//digit grouping name:hundres,thousand,etc...

        if (value < 1000)
            return number;
        
        switch (numDigits)
        {
            case 4://thousands' range
            case 5:
            case 6:
                pos = numDigits % 4 + 1;
                place = "k";
                break;
            case 7://millions' range
            case 8:
            case 9:
                pos = numDigits % 7 + 1;
                place = "m";
                break;
            case 10://Billions's range
            case 11:
            case 12:
                pos = numDigits % 10 + 1;
                place = "b";
                break;
            case 13://Trilions's range
            case 14:
            case 15:
                pos = numDigits % 16 + 1;
                place = "t";
                break;
            case 16://Quadrillion's range
            case 17:
            case 18:
                pos = numDigits % 19 + 1;
                place = "q";
                break;
            case 19://Quintillion's range
            case 20:
            case 21:
                pos = numDigits % 24 + 1;
                place = "Q";
                break;
            case 22://Sextillion's range
            case 23:
            case 24:
                pos = numDigits % 27 + 1;
                place = "s";
                break;
            case int n when n > 25:
                pos = numDigits % 30 + 1;
                place = "*";
                break;
        }
        
        string word = number.Substring(0, pos) + "," + number.Substring(pos, 1) + place;
        return word;
    }
}
