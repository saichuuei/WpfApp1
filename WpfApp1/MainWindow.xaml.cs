using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Enigma_Machine
{
    public partial class MainWindow : Window
    {
        // Constants for rotor wiring and reflector
        string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Standard alphabet for reference
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV"; // Rotor 1 wiring (Hours)
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; // Rotor 2 wiring (Minutes)
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; // Rotor 3 wiring (Seconds)
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // Reflector wiring

        // Rotor offset tracking
        int[] _keyOffset = { 0, 0, 0 }; // Current rotor offsets (H, M, S)
        int[] _initOffset = { 0, 0, 0 }; // Initial rotor offsets (H, M, S)

        // Rotor state flag
        bool _rotor = false;

        // Plugboard setup
        Dictionary<char, char> _plugboard = new Dictionary<char, char>(); // Plugboard dictionary
        private bool _plugboardSet = false; // Flag to indicate if plugboard is set

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            SetDefaults(); // Initialize default values

            _rotor = false; // Initially rotor is off
            btnRotor.Content = "Rotor On"; // Set button text
            btnRotor.IsEnabled = false; // Disable rotor button until plugboard is set
        }

        // Display rotor wiring in UI labels
        private void DisplayRing(Label displayLabel, string ring)
        {
            string temp = "";
            foreach (char r in ring)
                temp += r + "\t"; // Add tab for spacing
            displayLabel.Content = temp;
        }

        // Find the index of a character in a string
        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        // Handle keyboard input
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the focus is on any of the text boxes for rotor offsets or plugboard
            if (txtBRing1Init.IsFocused || txtBRing2Init.IsFocused || txtBRing3Init.IsFocused || txtPlugboard.IsFocused)
            {
                return; // Ignore key presses when these text boxes are focused
            }

            // Check for uppercase letters and message length
            if (e.Key.ToString().Length == 1 && lblInput.Content.ToString().Length < 128)
            {
                if ((int)e.Key.ToString()[0] >= 65 && (int)e.Key.ToString()[0] <= 90)
                {
                    lblInput.Content += e.Key.ToString(); // Append input character
                    lblEncrpyt.Content += Encrypt(e.Key.ToString()[0]) + ""; // Encrypt and append
                    lblEncrpytMirror.Content += Mirror(e.Key.ToString()[0]) + ""; // Mirror and append

                    // Rotate rotors if enabled
                    if (_rotor)
                    {
                        Rotate(true);
                        DisplayRing(lblRing1, _ring1); // Update rotor display
                        DisplayRing(lblRing2, _ring2);
                        DisplayRing(lblRing3, _ring3);
                    }
                }
            }
            // Handle space key
            else if (e.Key == Key.Space)
            {
                lblInput.Content += " ";
                lblEncrpyt.Content += " ";
                lblEncrpytMirror.Content += " ";
            }
            // Handle backspace key
            else if (e.Key == Key.Back)
            {
                Rotate(false); // Rotate rotors backward
                DisplayRing(lblRing1, _ring1);
                DisplayRing(lblRing2, _ring2);
                DisplayRing(lblRing3, _ring3);

                lblInput.Content = RemoveLastLetter(lblInput.Content.ToString()); // Remove last character
                lblEncrpyt.Content = RemoveLastLetter(lblEncrpyt.Content.ToString());
                lblEncrpytMirror.Content = RemoveLastLetter(lblEncrpytMirror.Content.ToString());
            }
        }

        // Encrypt a character
        private char Encrypt(char letter)
        {
            char newChar = letter;

            // Plugboard pass (before rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            // Rotor pass forward
            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            // Reflector pass
            newChar = _reflector[IndexSearch(_control, newChar)];

            // Rotor pass backward
            newChar = _control[IndexSearch(_ring3, newChar)];
            newChar = _control[IndexSearch(_ring2, newChar)];
            newChar = _control[IndexSearch(_ring1, newChar)];

            // Plugboard pass (after rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            return newChar;
        }

        // Mirror a character (encrypt and pass back through rotors)
        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _control[IndexSearch(_ring3, newChar)];
            newChar = _control[IndexSearch(_ring2, newChar)];
            newChar = _control[IndexSearch(_ring1, newChar)];

            return newChar;
        }

        // Set default values
        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
            _keyOffset = new int[] { 0, 0, 0 };

            lblInput.Content = "";
            lblEncrpyt.Content = "";
            lblEncrpytMirror.Content = "";

            DisplayRing(lblControlRing, _control);
            DisplayRing(lblRing1, _ring1);
            DisplayRing(lblRing2, _ring2);
            DisplayRing(lblRing3, _ring3);
        }

        // Rotate rotors
        private void Rotate(bool forward)
        {
            if (forward)
            {
                _keyOffset[0]++;  // Ring 1 (H) moves first
                _ring1 = MoveValues(forward, _ring1);

                if (_keyOffset[0] / _control.Length >= 1)
                {
                    _keyOffset[0] = 0;
                    _keyOffset[1]++;  // Then Ring 2 (M)
                    _ring2 = MoveValues(forward, _ring2);
                    if (_keyOffset[1] / _control.Length >= 1)
                    {
                        _keyOffset[1] = 0;
                        _keyOffset[2]++;  // Finally Ring 3 (S)
                        _ring3 = MoveValues(forward, _ring3);
                    }
                }
            }
            else
            {
                if (_keyOffset[0] > 0 || _keyOffset[1] > 0 || _keyOffset[2] > 0)
                {
                    _keyOffset[0]--;  // Reverse rotation starts with Ring 1 (H)
                    _ring1 = MoveValues(forward, _ring1);
                    if (_keyOffset[0] < 0)
                    {
                        _keyOffset[0] = 25;
                        _keyOffset[1]--;  // Then Ring 2 (M)
                        _ring2 = MoveValues(forward, _ring2);
                        if (_keyOffset[1] < 0)
                        {
                            _keyOffset[1] = 25;
                            _keyOffset[2]--;  // Finally Ring 3 (S)
                            _ring3 = MoveValues(forward, _ring3);
                            if (_keyOffset[2] < 0)
                                _keyOffset[2] = 25;
                        }
                    }
                }
            }

            DisplayOffset();
        }

        // Move rotor values
        private string MoveValues(bool forward, string ring)
        {
            char movingValue = ' ';
            string newRing = "";

            if (forward)
            {
                movingValue = ring[0];
                for (int x = 1; x < ring.Length; x++)
                    newRing += ring[x];
                newRing += movingValue;
            }
            else
            {
                movingValue = ring[25];
                for (int x = 0; x < ring.Length - 1; x++)
                    newRing += ring[x];
                newRing = movingValue + newRing;
            }

            return newRing;
        }

        // Handle rotor button click
        private void btnRotor_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();

            if (int.TryParse(txtBRing1Init.Text, out _initOffset[0]) &&
                int.TryParse(txtBRing2Init.Text, out _initOffset[1]) &&
                int.TryParse(txtBRing3Init.Text, out _initOffset[2]))
            {
                if (_initOffset[0] >= 0 && _initOffset[0] <= 25 &&
                    _initOffset[1] >= 0 && _initOffset[1] <= 25 &&
                    _initOffset[2] >= 0 && _initOffset[2] <= 25)
                {
                    txtBRing1Init.IsEnabled = false;
                    txtBRing2Init.IsEnabled = false;
                    txtBRing3Init.IsEnabled = false;

                    _rotor = true;
                    btnRotor.Content = "Settings Lock";

                    // Initialize rotors in correct order (H, M, S)
                    _ring1 = InitializeRotors(_initOffset[0], _ring1);
                    _ring2 = InitializeRotors(_initOffset[1], _ring2);
                    _ring3 = InitializeRotors(_initOffset[2], _ring3);

                    DisplayRing(lblRing1, _ring1);
                    DisplayRing(lblRing2, _ring2);
                    DisplayRing(lblRing3, _ring3);
                    DisplayOffset();
                }
            }
        }

        // Initialize rotors with initial offset
        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        // Remove last letter from a string
        private string RemoveLastLetter(string word)
        {
            string newWord = "";
            for (int x = 0; x < word.Length - 1; x++)
                newWord += word[x];
            return newWord;
        }

        // Handle text box focus
        private void txtBRing1Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing1Init.Text = "";
        }

        private void txtBRing2Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing2Init.Text = "";
        }

        private void txtBRing3Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing3Init.Text = "";
        }

        // Display rotor offsets
        private void DisplayOffset()
        {
            txtBRing1Init.Text = _keyOffset[0] + "";
            txtBRing2Init.Text = _keyOffset[1] + "";
            txtBRing3Init.Text = _keyOffset[2] + "";
        }

        // Setup plugboard
        private void SetupPlugboard(string plugboardSettings)
        {
            _plugboard.Clear();
            string[] pairs = plugboardSettings.ToUpper().Split(' ');
            foreach (string pair in pairs)
            {
                if (pair.Length == 2)
                {
                    _plugboard[pair[0]] = pair[1];
                    _plugboard[pair[1]] = pair[0];
                }
            }
        }

        // Handle plugboard button click
        private void btnSetPlugboard_Click(object sender, RoutedEventArgs e)
        {
            if (_plugboardSet)
            {
                MessageBox.Show("Plugboard is already set.");
                return;
            }

            SetupPlugboard(txtPlugboard.Text);
            _plugboardSet = true;
            btnRotor.IsEnabled = true; // Enable the rotor button.

            // Disable the plugboard text box after setting
            txtPlugboard.IsEnabled = false;

            // Clear the existing input and encrypted messages
            lblInput.Content = "";
            lblEncrpyt.Content = "";
            lblEncrpytMirror.Content = "";
        }

        // Handle plugboard text change
        private void txtPlugboard_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Reset the input and encrypted message labels
            lblInput.Content = "";
            lblEncrpyt.Content = "";
            lblEncrpytMirror.Content = "";

            // Clear the plugboard dictionary if the text box is empty
            if (string.IsNullOrEmpty(txtPlugboard.Text))
            {
                _plugboard.Clear();
                _plugboardSet = false;
                btnRotor.IsEnabled = false; // Disable the rotor button if plugboard is not set
            }
        }
    }
}
