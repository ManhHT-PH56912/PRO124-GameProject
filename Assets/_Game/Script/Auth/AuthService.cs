using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AuthService : IAuthService
{
    private const string UserDataPrefix = "User_"; // Prefix for PlayerPrefs keys

    public static void CleanAllData()
    {
        try
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            // Check if PlayerPrefs are cleared
            if (PlayerPrefs.HasKey(UserDataPrefix + ""))
            {
                Debug.LogError("PlayerPrefs not cleared!");
            }
            else
            {
                Debug.Log("PlayerPrefs cleared successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to clear PlayerPrefs: " + ex.Message);
        }
    }

    public bool Authenticate(string email, string password)
    {
        try
        {
            // Retrieve stored hashed password for the email
            string storedHashedPassword = PlayerPrefs.GetString(UserDataPrefix + email, null);
            if (string.IsNullOrEmpty(storedHashedPassword))
            {
                return false; // User not found
            }

            // Hash the input password and compare
            string hashedInputPassword = HashPassword(password);
            return hashedInputPassword == storedHashedPassword;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Authentication failed for email {email}: {ex.Message}");
            return false;
        }
    }

    public bool Register(string email, string password)
    {
        try
        {
            // Check if user already exists
            if (PlayerPrefs.HasKey(UserDataPrefix + email))
            {
                return false; // User already registered
            }

            // Hash the password and store it
            string hashedPassword = HashPassword(password);
            PlayerPrefs.SetString(UserDataPrefix + email, hashedPassword);
            PlayerPrefs.Save(); // Ensure data is written to disk
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Registration failed for email {email}: {ex.Message}");
            return false;
        }
    }

    private string HashPassword(string password)
    {
        try
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Password hashing failed: {ex.Message}");
            return string.Empty; // Return empty string to indicate failure
        }
    }
}