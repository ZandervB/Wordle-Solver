using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Wordle_Solver
{
    public partial class WordleSolverForm : Form
    {
        private HashSet<string> wordList = new HashSet<string>(System.IO.File.ReadAllLines("words.txt"));

        public WordleSolverForm()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            green1.Text = "";
            green2.Text = "";
            green3.Text = "";
            green4.Text = "";
            green5.Text = "";
            yellow1.Text = "";
            yellow2.Text = "";
            yellow3.Text = "";
            yellow4.Text = "";
            yellow5.Text = "";
            grey.Text = "";
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