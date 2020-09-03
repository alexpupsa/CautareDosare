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
using PortalWSClient.Model;
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
        private List<string> errors = new List<string>();
        private int countJson = 0;
        private string[] PartiExcluse = { "Contestator", "Intervenient în nume propriu", "Petent", "Reclamant" };

        public Form1()
        {
            InitializeComponent();
            LoadPartiExcluse();
        }


        private DosareResult FindDosareByName(string firstName, string lastName)
        {
            try
            {
                Query ws = new Query();
                Dosar[] dosare = ws.CautareDosare(null, null, lastName, null, null, null);

                List<Dosar> results = new List<Dosar>();

                foreach (var dosar in dosare)
                {
                    var found = dosar.parti.Any(x => !PartiExcluse.Contains(x.calitateParte) && x.nume.ToLower().Contains(lastName.ToLower()) && x.nume.ToLower().Contains(firstName.ToLower()));
                    if (found)
                    {
                        results.Add(dosar);
                    }
                }

                if (results.Any())
                {
                    return new DosareResult
                    {
                        Json = JsonConvert.SerializeObject(results, Formatting.Indented)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                return new DosareResult
                {
                    Error = ex.ToString()
                };
            }
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
                        textBoxFolder.Text = Path.GetDirectoryName(openFileDialog1.FileName);
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
                    var rezultat = FindDosareByName(prenume, nume);
                    if (rezultat != null)
                    {
                        if (rezultat.Error != null)
                        {
                            errors.Add($"Nume cautat: {name} -> Erori: {rezultat.Error}");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(textBoxFolder.Text, $"{prenume} {nume}.json"), rezultat.Json);
                            countJson++;
                        }
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
            var log = $"log-{DateTime.Now:ddMMyyyyHHmmss}.txt";
            if (errors.Count > 0)
            {
                File.WriteAllLines(Path.Combine(textBoxFolder.Text, log), errors.ToArray());
            }
            buttonStart.Enabled = true;
            var text = "Gata!";
            if (countJson > 0)
            {
                text += $" Am gasit {countJson} persoane cu dosare.";
            }
            if (errors.Count > 0)
            {
                text += $" {errors.Count} cautari au generat erori. Verifica log {log}";
            }
            if (countJson == 0 && errors.Count == 0)
            {
                text += " Nu am gasit nicio persoana cu dosare.";
            }
            MessageBox.Show(text);
            errors.Clear();
            countJson = 0;
        }

        private void LoadPartiExcluse()
        {
            try
            {
                var lines = File.ReadAllLines("parti-excluse.txt");
                if(lines.Any())
                {
                    PartiExcluse = lines.Select(x => x.Trim()).ToArray();
                }
            }
            catch
            {

            }
        }
    }
}
