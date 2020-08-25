/*
Autor:      indaco systems (www.indaco.ro)
Data:       21.12.2012
Versiunea:  1.0
Descriere:  Exemplu de apel al serviciului de acces programatic la date despre dosare http://portalquery.just.ro/query.asmx

Istoric modificari:
Data        21.12.2012
Descriere:  Prima versiune
 */
using Newtonsoft.Json;
using PortalWSClient.PortalWS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PortalWSClient
{
    public partial class Form1 : Form
    {
        private List<string> names = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }


        private string FindDosareByName(string firstName, string lastName)
        {
            Query ws = new Query();
            Dosar[] dosare = ws.CautareDosare(null, null, lastName, null, null, null);

            List<Dosar> results = new List<Dosar>();

            foreach (var dosar in dosare)
            {
                var found = dosar.parti.Any(x => x.nume.ToLower().Contains(lastName.ToLower()) && x.nume.ToLower().Contains(firstName.ToLower()));
                if (found)
                {
                    results.Add(dosar);
                }
            }

            if (results.Any())
            {
                return JsonConvert.SerializeObject(results, Formatting.Indented);
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(openFileDialog1.FileName))
                {
                    names = File.ReadAllLines(openFileDialog1.FileName).ToList();
                    if (names.Any())
                    {
                        textBoxInput.Text = openFileDialog1.FileName;
                        textBoxProgres.Text = $"0/{names.Count}";
                        checkRequirements();
                    }
                    else
                    {
                        MessageBox.Show("Fisierul nu contine niciun nume");
                    }

                }
                else
                {
                    MessageBox.Show("Eroare deschidere fisier");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxFolder.Text = folderBrowserDialog1.SelectedPath;
                checkRequirements();
            }
        }

        private void checkRequirements()
        {
            if (!string.IsNullOrEmpty(textBoxInput.Text) && !string.IsNullOrEmpty(textBoxFolder.Text) && names.Count > 0)
            {
                buttonStart.Enabled = true;
            }
            else
            {
                buttonStart.Enabled = false;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            var count = 0;

            foreach (var name in names)
            {
                var splits = name.Split();
                if (splits.Length > 1)
                {
                    var prenume = splits[0];
                    var nume = splits[1];
                    var rezultate = FindDosareByName(prenume, nume);
                    if (rezultate != null)
                    {
                        File.WriteAllText($@"C:\temp\{prenume} {nume}.json", rezultate);
                    }
                }
                count++;
                worker.ReportProgress(count);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            textBoxProgres.Text = $"{e.ProgressPercentage}/{names.Count}";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonStart.Enabled = true;
            MessageBox.Show("Gata! Verifica folder-ul!");
        }
    }
}
