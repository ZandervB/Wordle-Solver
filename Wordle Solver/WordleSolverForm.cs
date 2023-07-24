using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;

namespace Wordle_Solver
{
    public partial class WordleSolverForm : Form
    {
        private HashSet<string> wordList;

        public WordleSolverForm()
        {
            InitializeComponent();
            LoadWordList();
        }

        private void LoadWordList()
        {
            try
            {
                wordList = new HashSet<string>(System.IO.File.ReadAllLines("words.txt"));
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("The 'words.txt' file was not found. Attempting to download the file from the website...", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Attempt to download the file from your website
                if (DownloadWordListFromWebsite())
                {
                    // If the download was successful, try loading the file again
                    try
                    {
                        wordList = new HashSet<string>(System.IO.File.ReadAllLines("words.txt"));
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        MessageBox.Show("The 'words.txt' file could not be downloaded or found. Please make sure it exists and try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"An error occurred while loading the word list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
                else
                {
                    MessageBox.Show("The 'words.txt' file could not be downloaded. Please make sure your internet connection is active and try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the word list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private bool DownloadWordListFromWebsite()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    // Replace "YourWebsiteURL" with the actual URL of the "words.txt" file on your website
                    webClient.DownloadFile("https://raw.githubusercontent.com/ZandervB/Wordle-Solver/master/Wordle%20Solver/words.txt", "words.txt");
                }
                MessageBox.Show("The 'words.txt' file has been successfully downloaded from the website. The program can now start.", "Download Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;

            }
            catch (System.Exception)
            {
                return false;
            }
        }

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


        public List<string> findMatchingWords(HashSet<string> words, string green, string yellow, string grey)
        {
            List<string> matchingWords = new List<string>();
            foreach (string word in words)
            {
                bool isValidG = true;
                bool isValidY = true;
                bool isValidGrey = true;


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
                for (int i = 0; i < grey.Length; i++)
                {
                    if (word.Contains(grey[i]))
                    {
                        isValidGrey = false;
                        break;
                    }
                }
                if (isValidG && isValidY && isValidGrey)
                {
                    matchingWords.Add(word);
                }
            }
            return matchingWords;
        }




        public string GetColorText(string color)
        {
            StringBuilder result = new StringBuilder();
            if (color == "grey")
            {
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
            
            for (int i = 1; i <= 5; i++)
            {
                TextBox? textBox = this.Controls.Find(color + i, true).FirstOrDefault() as TextBox;

                // Check if the textbox is null or empty
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    result.Append("#"); // Add "#" to the result for an empty or null textbox
                }
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



        private void btnFindWords_Click(object sender, EventArgs e)
        {
            string greenPositions = GetColorText("green");
            string yellowPositions = GetColorText("yellow");
            string greyPositions = GetColorText("grey");

            List<string> matchingWords = findMatchingWords(wordList, greenPositions, yellowPositions, greyPositions);

            outputText.Text = string.Join("\r\n", matchingWords);
            outputText.Visible = true;
            lCount.Text = matchingWords.Count.ToString() + " Possible words found";
        }
    }



}