using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPIClient;
public static class Prompt
{
    public static (string, string) ShowDialog(string text1, string text2, string caption)
    {
        Form prompt = new Form()
        {
            Width = 500,
            Height = 200,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };
        Label textLabel = new Label() { Left = 50, Top = 20, Text = text1, AutoSize = true };
        TextBox textBox1 = new TextBox() { Left = 50, Top = 50, Width = 400 };
        Label textLabel2 = new Label() { Left = 50, Top = 80, Text = text2, AutoSize = true};
        TextBox textBox2 = new TextBox() { Left = 50, Top = 110, Width = 400 };
        Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 140, DialogResult = DialogResult.OK };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox1);
        prompt.Controls.Add(textLabel2);
        prompt.Controls.Add(textBox2);
        prompt.Controls.Add(confirmation);
        prompt.AcceptButton = confirmation;

        if(prompt.ShowDialog() == DialogResult.OK)
        {
            return (textBox1.Text, textBox2.Text);
        }
        return ("", "");
    }
}
