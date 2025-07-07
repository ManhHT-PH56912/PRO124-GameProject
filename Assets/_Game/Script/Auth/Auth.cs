using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [Header("Login UI")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private TextMeshProUGUI loginMessageText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton;

    [Header("Register UI")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TextMeshProUGUI registerMessageText;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button goToLoginButton;

    private IAuthService authService;
    private IInputValidator inputValidator;

    // Enum to define message states
    private enum MessageState
    {
        Success,
        Error
    }

    // Colors for different message states
    private readonly Color successColor = new Color(0.0f, 0.5f, 0.0f, 1f); // Dark green, fully opaque
    private readonly Color errorColor = Color.red;

    void Awake()
    {
        try
        {
            authService = new AuthService();
            inputValidator = new InputValidator();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize services: {ex.Message}");
        }
    }

    void Start()
    {
        try
        {
            AuthService.CleanAllData(); // Clear all data for testing purposes


            // Clear messages at start
            ClearMessage(loginMessageText);
            ClearMessage(registerMessageText);

            // Bind button click events
            loginButton.onClick.AddListener(OnLoginClicked);
            goToRegisterButton.onClick.AddListener(ShowRegisterPage);
            registerButton.onClick.AddListener(OnRegisterClicked);
            goToLoginButton.onClick.AddListener(ShowLoginPage);

            // Show the login page by default
            ShowLoginPage();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Start failed: {ex.Message}");
            SetMessage(loginMessageText, "Initialization error.", MessageState.Error);
        }
    }

    private void ShowLoginPage()
    {
        try
        {
            // Clear messages
            ClearMessage(loginMessageText);
            ClearMessage(registerMessageText);

            // Clear input fields
            if (loginEmailInput != null) loginEmailInput.text = "";
            if (loginPasswordInput != null) loginPasswordInput.text = "";

            // set title text
            if (titleText != null)
            {
                titleText.text = "Login Account";
            }
            // Show login panel, hide register panel
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to show login page: {ex.Message}");
        }
    }

    private void ShowRegisterPage()
    {
        try
        {
            // Clear messages
            ClearMessage(loginMessageText);
            ClearMessage(registerMessageText);

            // Clear input fields
            if (registerEmailInput != null) registerEmailInput.text = "";
            if (registerPasswordInput != null) registerPasswordInput.text = "";

            // set title text
            if (titleText != null)
            {
                titleText.text = "Create Account";
            }
            // Show register panel, hide login panel
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to show register page: {ex.Message}");
        }
    }

    private void OnLoginClicked()
    {
        try
        {
            string email = loginEmailInput.text;
            string password = loginPasswordInput.text;

            // Collect validation errors
            bool isEmailValid = inputValidator.IsValidEmail(email, out string emailError);
            bool isPasswordValid = inputValidator.IsValidPassword(password, out string passwordError);

            // Check if both email and password are empty
            if (!isEmailValid && !isPasswordValid &&
                emailError == "Email cannot be empty." &&
                passwordError == "Password cannot be empty.")
            {
                SetMessage(loginMessageText, "Email and password cannot be empty.", MessageState.Error);
                return;
            }

            // Handle individual validation errors
            if (!isEmailValid)
            {
                SetMessage(loginMessageText, emailError, MessageState.Error);
                return;
            }
            if (!isPasswordValid)
            {
                SetMessage(loginMessageText, passwordError, MessageState.Error);
                return;
            }

            // Proceed with authentication
            if (authService.Authenticate(email, password))
            {
                SetMessage(loginMessageText, "Login successful!", MessageState.Success);
                // Next Scene
                // SceneManager.LoadScene("NextSceneName"); // Uncomment and replace with your scene name
            }
            else
            {
                SetMessage(loginMessageText, "Invalid email or password.", MessageState.Error);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Login failed: {ex.Message}");
            SetMessage(loginMessageText, "Login error occurred.", MessageState.Error);
        }
    }

    private void OnRegisterClicked()
    {
        try
        {
            string email = registerEmailInput.text;
            string password = registerPasswordInput.text;

            // Collect validation errors
            bool isEmailValid = inputValidator.IsValidEmail(email, out string emailError);
            bool isPasswordValid = inputValidator.IsValidPassword(password, out string passwordError);

            // Check if both email and password are empty
            if (!isEmailValid && !isPasswordValid &&
                emailError == "Email cannot be empty." &&
                passwordError == "Password cannot be empty.")
            {
                SetMessage(registerMessageText, "Email and password cannot be empty.", MessageState.Error);
                return;
            }

            // Handle individual validation errors
            if (!isEmailValid)
            {
                SetMessage(registerMessageText, emailError, MessageState.Error);
                return;
            }
            if (!isPasswordValid)
            {
                SetMessage(registerMessageText, passwordError, MessageState.Error);
                return;
            }

            // Proceed with registration
            if (authService.Register(email, password))
            {
                SetMessage(registerMessageText, "Registration successful!", MessageState.Success);
                ShowLoginPage();
            }
            else
            {
                SetMessage(registerMessageText, "Registration failed.", MessageState.Error);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Registration failed: {ex.Message}");
            SetMessage(registerMessageText, "Registration error occurred.", MessageState.Error);
        }
    }

    // Reusable method to set and clear a message with color based on state
    private void SetMessage(TMP_Text textField, string message, MessageState state)
    {
        try
        {
            textField.text = message;
            textField.color = state == MessageState.Success ? successColor : errorColor;
            ClearMessage(textField);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to set message: {ex.Message}");
        }
    }

    // Reusable method to clear a message after a delay and reset color
    private void ClearMessage(TMP_Text textField)
    {
        try
        {
            StopCoroutine(nameof(ClearMessageAfterDelay));
            StartCoroutine(ClearMessageAfterDelay(textField));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to clear message: {ex.Message}");
        }
    }

    private IEnumerator ClearMessageAfterDelay(TMP_Text textField)
    {
        yield return new WaitForSeconds(3f);
        try
        {
            if (textField != null)
            {
                textField.text = "";
                textField.color = Color.white; // Reset to default color
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to clear message after delay: {ex.Message}");
        }
    }
}