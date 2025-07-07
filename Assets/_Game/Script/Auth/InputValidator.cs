using System.Text.RegularExpressions;
using System;

public class InputValidator : IInputValidator
{
    // Updated regex to enforce email ending with exactly @gmail.com
    private const string EmailPattern = @"^[^@\s]+@gmail\.com$";

    public bool IsValidEmail(string email, out string errorMessage)
    {
        if (string.IsNullOrEmpty(email))
        {
            errorMessage = "Email cannot be empty.";
            return false;
        }

        try
        {
            if (!Regex.IsMatch(email, EmailPattern))
            {
                errorMessage = "Email must be a valid Gmail address (example@gmail.com).";
                return false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = "Error validating email format: " + ex.Message;
            return false;
        }

        errorMessage = "";
        return true;
    }

    public bool IsValidPassword(string password, out string errorMessage)
    {
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Password cannot be empty.";
            return false;
        }

        errorMessage = "";
        return true;
    }
}