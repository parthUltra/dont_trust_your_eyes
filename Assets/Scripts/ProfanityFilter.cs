using System.Collections.Generic;
using System.Linq;

public static class ProfanityFilter
{
    // List of bad words to filter (add more as needed)
    private static readonly HashSet<string> badWords = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "fuck", "shit", "ass", "bitch", "damn", "hell", "crap", "bastard",
        "dick", "cock", "pussy", "cunt", "fag", "nigger", "retard",
        "asshole", "motherfucker", "whore", "slut", "piss", "douche"
    };

    /// <summary>
    /// Checks if the text contains any profanity
    /// </summary>
    public static bool ContainsProfanity(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string lowerText = text.ToLower();
        
        // Check for exact word matches
        foreach (string badWord in badWords)
        {
            if (lowerText.Contains(badWord))
                return true;
        }

        // Check for variations with numbers/symbols (l33t speak)
        string normalized = NormalizeText(lowerText);
        foreach (string badWord in badWords)
        {
            if (normalized.Contains(badWord))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Cleans the text by replacing profanity with asterisks
    /// </summary>
    public static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        string cleanedText = text;
        
        foreach (string badWord in badWords)
        {
            string replacement = new string('*', badWord.Length);
            cleanedText = System.Text.RegularExpressions.Regex.Replace(
                cleanedText,
                badWord,
                replacement,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        return cleanedText;
    }

    /// <summary>
    /// Normalizes text by converting common l33t speak patterns to regular letters
    /// </summary>
    private static string NormalizeText(string text)
    {
        return text
            .Replace("@", "a")
            .Replace("4", "a")
            .Replace("3", "e")
            .Replace("1", "i")
            .Replace("!", "i")
            .Replace("0", "o")
            .Replace("$", "s")
            .Replace("5", "s")
            .Replace("7", "t")
            .Replace("+", "t");
    }

    /// <summary>
    /// Validates username for submission (checks length and profanity)
    /// </summary>
    public static bool IsValidUsername(string username, out string errorMessage)
    {
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = "Username cannot be empty";
            return false;
        }

        if (username.Length < 2)
        {
            errorMessage = "Username must be at least 2 characters";
            return false;
        }

        if (username.Length > 20)
        {
            errorMessage = "Username must be less than 20 characters";
            return false;
        }

        if (ContainsProfanity(username))
        {
            errorMessage = "Username contains inappropriate language";
            return false;
        }

        return true;
    }
}

