using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wordle_Solver
{
    public partial class WordleSolverForm : Form
    {
        private HashSet<string> wordList = new HashSet<string>();

        public WordleSolverForm()
        {
            InitializeComponent();
            LoadWordListAsync(); // Load the word list asynchronously when the form is initialized
        }

        // Asynchronously load the word list from a file or download it from a website if not found
        private async Task LoadWordListAsync()
        {
            try
            {
                // Try reading the word list from the file
                wordList = new HashSet<string>(System.IO.File.ReadAllLines("words.txt"));
            }
            catch (System.IO.FileNotFoundException)
            {
                // Show a message if the file is not found and attempt to download it
                MessageBox.Show("The 'words.txt' file was not found. Attempting to download the file from the website...", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Attempt to download the file from your website
                if (await DownloadWordListFromWebsiteAsync())
                {
                    // If the download was successful, try loading the file again
                    try
                    {
                        wordList = new HashSet<string>(System.IO.File.ReadAllLines("words.txt"));
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        ShowErrorAndExit("The 'words.txt' file could not be downloaded or found. Please make sure it exists and try again later.");
                    }
                    catch (System.Exception ex)
                    {
                        ShowErrorAndExit($"An error occurred while loading the word list: {ex.Message}");
                    }
                }
                else
                {
                    ShowErrorAndExit("The 'words.txt' file could not be downloaded. Please make sure your internet connection is active and try again later.");
                }
            }
            catch (System.Exception ex)
            {
                ShowErrorAndExit($"An error occurred while loading the word list: {ex.Message}");
            }
        }

        // Asynchronously download the word list from the website
        private async Task<bool> DownloadWordListFromWebsiteAsync()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/ZandervB/Wordle-Solver/master/Wordle%20Solver/words.txt");

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] content = await response.Content.ReadAsByteArrayAsync();
                        System.IO.File.WriteAllBytes("words.txt", content);

                        // Show a success message if the download was successful
                        MessageBox.Show("The 'words.txt' file has been successfully downloaded from the website. The program can now start.", "Download Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        // Show an error message if the download fails
                        MessageBox.Show($"Failed to download the 'words.txt' file from the website. Status code: {response.StatusCode}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Show an error message if download fails
                MessageBox.Show($"An error occurred while downloading the 'words.txt' file: {ex.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Show an error message and exit the application
        private void ShowErrorAndExit(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        // Clear input and output fields
        private void btnClear_Click(object sender, EventArgs e)
        {
            foreach (TextBox textBox in this.Controls.OfType<TextBox>())
            {
                textBox.Text = "";
            }

            outputText.Text = "";
            lCount.Text = "";
            outputText.Visible = false;
        }

        // Find matching words based on user input and display the results
        private void btnFindWords_Click(object sender, EventArgs e)
        {
            string greenPositions = GetColorText("green");
            string yellowPositions = GetColorText("yellow");
            string greyPositions = GetColorText("grey");
            if (!checkForConflicts(greenPositions, yellowPositions, greyPositions))
            {
                List<string> matchingWords = findMatchingWords(wordList, greenPositions, yellowPositions, greyPositions);

                outputText.Text = string.Join("\r\n", matchingWords);
                outputText.Visible = true;
                lCount.Text = matchingWords.Count.ToString() + " Possible words found";
            }
            else
            {
                outputText.Visible = false;
                lCount.Text = "Fix Conflict";
            }
        }

        // Find words that match the given input conditions
        public List<string> findMatchingWords(HashSet<string> words, string green, string yellow, string grey)
        {
            List<string> matchingWords = new List<string>();
            foreach (string word in words)
            {
                bool isValidG = true;
                bool isValidY = true;
                bool isValidGrey = true;

                // Check for matching letters in green, yellow, and gray positions
                for (int i = 0; i < 5; i++)
                {
                    if (green[i] != '#' && green[i] != word[i])
                    {
                        isValidG = false;
                        break;
                    }
                    if (yellow[i] != '#' && (yellow[i] == word[i] || (!word.Contains(yellow[i]) || word.IndexOf(yellow[i]) == i)))
                    {
                        isValidY = false;
                        break;
                    }
                }
                // Check for conflicting letters in gray positions
                for (int i = 0; i < grey.Length; i++)
                {
                    if (word.Contains(grey[i]))
                    {
                        isValidGrey = false;
                        break;
                    }
                }
                // If the word satisfies all conditions, add it to the matching words list
                if (isValidG && isValidY && isValidGrey)
                {
                    matchingWords.Add(word);
                }
            }
            return matchingWords;
        }

        // Get input text from color textboxes and handle validation
        public string GetColorText(string color)
        {
            StringBuilder result = new StringBuilder();
            if (color == "grey")
            {
                // Validate input in the gray textbox
                if (grey.Text.Any(c => !char.IsLetter(c)))
                {
                    MessageBox.Show("Invalid input in the grey textbox. Please enter only letters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }
                else
                {
                    return grey.Text.ToLower();
                }
            }

            // Loop through the color textboxes and construct the input string
            for (int i = 1; i <= 5; i++)
            {
                TextBox? textBox = this.Controls.Find(color + i, true).FirstOrDefault() as TextBox;

                // Check if the textbox is null or empty
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    result.Append("#"); // Add "#" to the result for an empty or null textbox
                }
                // Check for invalid input in the color textboxes
                else if (textBox.Text.All(c => !char.IsLetter(c)))
                {
                    MessageBox.Show($"Invalid input in {color} textbox {i}. Please enter only letters or leave the textbox empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "#####"; // Return a placeholder value to indicate an error
                }
                else
                {
                    result.Append(textBox.Text);
                }
            }
            return result.ToString().ToLower();
        }

        // Check for conflicting letters in color positions
        public bool checkForConflicts(string green, string yellow, string grey)
        {
            for (int i = 0; i < grey.Length; i++)
            {
                if (yellow.Contains(grey[i]))
                {
                    MessageBox.Show($"Conflict in grey and yellow text boxes, {grey[i]} is in more than one color textbox", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                if (green.Contains(grey[i]))
                {
                    MessageBox.Show($"Conflict in grey and green text boxes, {grey[i]} is in more than one color textbox", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                if (green[i] == yellow[i] && char.IsLetter(green[i]))
                {
                    MessageBox.Show($"Conflict in green and yellow text boxes, {green[i]} is in the same position in yellow and green textboxes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
            }
            return false;
        }
    }
}
