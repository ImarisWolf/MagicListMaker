using MagicParser.CodeParsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagicParser
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Private fields

        private string fileName = "";
        private string originInput = "";
        private bool changed
        {
            get
            {
                return InputTextBox.Text != originInput;
            }
        }

        private Size startSize;
        private Size leftBoxStartSize;
        private Size rightBoxStartSize;
        private int rightStartPos;

        #endregion

        #region Private methods

        //в случае наличия изменений предлагает сохранить файл и возвращает true; если была нажата "отмена", возвращает false
        private bool CheckChanges() 
        {
            if (changed)
            {
                DialogResult answer = MessageBox.Show(this, "Желаете ли вы сохранить изменения?", "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                switch (answer)
                {
                    case DialogResult.Yes:
                        Save();
                        return true;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        //Сохранение
        private void Save()
        {
            if (fileName == "")
            {
                SaveAs();
            }
            else
            {
                Write();
            }
        }

        //Сохранение по выбранному пути
        private void SaveAs()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                Write();
            }
        }
        
        //Запись из инпута в файл
        private void Write()
        {
            StreamWriter sw = new StreamWriter(fileName, false);
            sw.Write(InputTextBox.Text);
            sw.Close();
        }
        
        //Открыть файл
        private void Open()
        {
            if (CheckChanges())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                    StreamReader sr = new StreamReader(fileName);
                    InputTextBox.Text = sr.ReadToEnd();
                    originInput = InputTextBox.Text;
                    sr.Close();
                }
            }
        }

        //Новый файл
        private void New()
        {
            if (CheckChanges())
            {
                fileName = "";
                InputTextBox.Text = "";
            }
        }

        #endregion


        #region Events

        private void InputTextBox_TextChanged(object sender, EventArgs e)
        {
            Analizer a = new Analizer(InputTextBox.Text);
            string parsedText = a.Parse();
            OutputTextBox.Text = parsedText;
            if (a.errorDescription != null)
            {
                OutputTextBox.ForeColor = Color.Gray;
                ErrorLogTextBox.Text = a.errorDescription + "\r\nDouble click to set position onto the error.";
            }
            else
            {
                OutputTextBox.ForeColor = Color.Black;
                ErrorLogTextBox.Text = "";
            }
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            New();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckChanges())
            {
                e.Cancel = true;
            }
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            //При изменении размеров окна равномерно меняются размеры двух текстбоксов.
            InputTextBox.Height = leftBoxStartSize.Height + (Size.Height - startSize.Height);
            InputTextBox.Width = leftBoxStartSize.Width + (Size.Width - startSize.Width) / 2;
            OutputTextBox.Left = rightStartPos + (Size.Width - startSize.Width) / 2;
            OutputTextBox.Width = rightBoxStartSize.Width + (Size.Width - startSize.Width) / 2;
            OutputTextBox.Height = rightBoxStartSize.Height + (Size.Height - startSize.Height);
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            //Необходимо записать стартовые значения размеров окна, чтобы при изменении размеров использовать записанные данные для корректировки размеров элементов.
            startSize = Size;
            leftBoxStartSize = InputTextBox.Size;
            rightBoxStartSize = OutputTextBox.Size;
            rightStartPos = OutputTextBox.Left;
    }

        private void ErrorLogTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (Analizer.tokenizerLastErrorPos != 0)
            {
                InputTextBox.Focus();
                InputTextBox.Select(Analizer.tokenizerLastErrorPos, 1);
            }
        }

        private void CopyRightPartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Analizer.tokenizerLastErrorPos == 0)
            {
                if (!string.IsNullOrEmpty(OutputTextBox.Text)) Clipboard.SetText(OutputTextBox.Text);
            }
            else MessageBox.Show("There are errors in the code. Fix them first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        #endregion

    }
}
