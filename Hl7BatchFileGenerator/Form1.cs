using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Hl7BatchFileGenerator
{
    public partial class frmHl7Generator : Form
    {
        protected class Template
        {
            public int Index { get; set; }
            public string? Name { get; set; }
            public string? File { get; set; }
            public string? Diagnosis { get; set; }
            public string? Arguments { get; set; }
            public string? Seoc { get; set; }

            public override string ToString() => $"{Index + 1}: {Name ?? string.Empty} {File ?? string.Empty} {Diagnosis ?? string.Empty} {Arguments ?? string.Empty} {Seoc ?? string.Empty}";
        }

        public frmHl7Generator()
        {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            TryGenerate();
        }

        private bool TryGenerate()
        {
            string server = this.tbServer.Text;
            int port = (int)this.nudPort.Value;
            int count = (int)this.nudNumToGenerate.Value;
            var startIcn = this.nudIdsStart.Text; // RA Notes: ICN Number changed to text
            int startRefs = (int)this.nudRefsStart.Value;
            var siteId = this.nudSiteId.Text; // RA Notes: Consult Id Changed to text
            string outFile = this.tbOutputFile.Text;
            if (!ValidateCommonFields(server, outFile, out string? error))
                return SetOutputAndReturn(error);
            IEnumerable<string> lines;
            if (tcSubmitType.SelectedTab.Name == this.tpComplex.Name)
            {
                var templates = this.lbTemplates.Items.Cast<Template>();
                if (!ValidateTemplates(templates, out error))
                    return SetOutputAndReturn(error);
                lines = GetLinesForComplexFile(templates, server, port, count, siteId, startRefs, startIcn);
            }
            else
            {
                string name = this.tbSimpleName.Text;
                string file = this.tbSimpleFileName.Text;
                string args = this.tbSimpleArgs.Text;
                if (!ValidateSimpleFields(name, file, out error))
                    return SetOutputAndReturn(error);
                lines = GetLinesForSimpleFile(server, port, file, count, siteId, startRefs, startIcn, name, args);
            }
            if (!TrySaveLinesToFile(outFile, lines, out error))
                return SetOutputAndReturn($"Error encountered:{Environment.NewLine}{error}");
            return SetOutputAndReturn($"Completed generating file {outFile}, with {count} referral lines", true);
        }

        private bool SetOutputAndReturn(string output, bool response = false)
        {
            tbOutput.Text = output;
            return response;
        }

        private static bool ValidateCommonFields(string server, string outFile, out string error)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(server))
                errors.Add("Server is required.");
            if (string.IsNullOrWhiteSpace(outFile))
                errors.Add("Output File is required.");
            error = string.Join(Environment.NewLine, errors);
            return string.IsNullOrWhiteSpace(error);
        }

        private static bool ValidateTemplates(IEnumerable<Template> templates, out string error)
        {
            if (templates.Count() < 1)
                error = "At least one template is required";
            else
            {
                var errors = Enumerable.Range(0, templates.Count()).Select(i =>
                     ValidateTemplate(templates.ElementAt(i), out string? templateError) ? null : $"Errors on the template {i + 1}:{Environment.NewLine}{templateError}");
                error = string.Join(Environment.NewLine, errors.Where(e => !string.IsNullOrWhiteSpace(e)));
            }
            return string.IsNullOrWhiteSpace(error);
        }

        private static bool ValidateTemplate(Template template, out string? error)
        {
            error = null;
            List<string> innerErrors = new List<string>();
            if (!ValidateSimpleFields(template.Name, template.File, out string innerError))
                innerErrors.Add(innerError);
            if (string.IsNullOrWhiteSpace(template.Seoc))
                innerErrors.Add("Seoc is required.");
            if (string.IsNullOrWhiteSpace(template.File))
                innerErrors.Add("File is required.");
            if (innerErrors.Count > 0)
                error = string.Join(Environment.NewLine, innerErrors);
            return string.IsNullOrWhiteSpace(error);
        }

        private static bool ValidateSimpleFields(string? vetName, string? fileName, out string error)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(vetName))
                errors.Add("Veteran name is required");
            else if (vetName.Split(',').Length != 2)
                errors.Add($"Exactly one comma is expected in Veteran name, found {vetName}");
            if (string.IsNullOrWhiteSpace(fileName))
                errors.Add("File name is required");
            error = string.Join(Environment.NewLine, errors);
            return string.IsNullOrWhiteSpace(error);
        }

        private static bool TrySaveLinesToFile(string fileName, IEnumerable<string> lines, out string? error)
        {
            error = null;
            try
            {
                System.IO.File.WriteAllText(fileName, string.Join(Environment.NewLine, lines));
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            return error == null;
        }

        private static IEnumerable<string> GetLinesForSimpleFile(string server, int port, string file, int count, string siteId, int startConsultId,
            string startIcnId, string name, string additionalArgs) =>
            Enumerable.Range(0, count).Select(i => GetStringForLine(server, port, file, siteId + (i+1), startConsultId + i, startIcnId, name, additionalArgs));

        private static IEnumerable<string> GetLinesForComplexFile(IEnumerable<Template> templates, string server, int port, int count, string siteId,
            int startConsultId, string startIcnId)
        {
            return Enumerable.Range(0, count).Select(i =>
            {
                Template currentTemplate = templates.ElementAt(i % templates.Count());
                return GetStringForLine(server, port, currentTemplate.File, siteId, startConsultId + i, startIcnId, currentTemplate.Name, currentTemplate.Arguments, currentTemplate.Diagnosis, currentTemplate.Seoc);
            });
        }

        private static string GetStringForLine(string server, int port, string? file, string siteId, int consultIdEnd, string icnEnd, string? name, string? additionalArgs, string? diagnosis = null, string? seoc = null)
        {
            string args = string.IsNullOrWhiteSpace(additionalArgs) ? string.Empty : $" {additionalArgs}";
            if (!string.IsNullOrWhiteSpace(seoc))
                args = $" -se \"{seoc}\"" + args;
            if (!string.IsNullOrWhiteSpace(diagnosis))
                args = $" -d \"{diagnosis}\"" + args;
            //return $"process {server} {port} {file} -c {siteId:D3}_{consultIdEnd:D10} -i {siteId:D3}v{icnEnd:D10} -n \"{name}{icnEnd}\" -nt -rt -st -w 1{args}";
 //  return $"Hl7Generator.exe process {server} {port} {file} -c {siteId:D3}_{consultIdEnd:D10} -i {siteId:D3}v{icnEnd:D10} -n \"{name}{icnEnd}\" -nt -rt -st -w 1{args}";

            return $"Hl7Generator.exe process {server} {port} {file} -c {siteId:D3} -i {icnEnd:D10} -n \"{name}\" -nt -rt -st -w 1{args}";

        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Template newTemplate = (new Template
            {
                Arguments = this.tbComplexArgs.Text,
                Diagnosis = this.tbDiagnosis.Text,
                File = this.tbComplexFile.Text,
                Index = this.lbTemplates.SelectedIndex == -1 ? this.lbTemplates.Items.Count : this.lbTemplates.SelectedIndex,
                Name = this.tbName.Text,
                Seoc = this.tbSeoc.Text,
            });
            if (lbTemplates.SelectedIndex == -1)//adding new
                lbTemplates.Items.Add(newTemplate);
            else //editing
                lbTemplates.Items[lbTemplates.SelectedIndex] = newTemplate;
            ClearComplexSelection();
        }

        private void LbTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTemplates.SelectedIndex == -1)
            {
                ClearComplexSelection();
            }
            else
            {
                var selected = (Template)lbTemplates.SelectedItem;
                this.tbComplexArgs.Text = selected.Arguments;
                this.tbDiagnosis.Text = selected.Diagnosis;
                this.tbComplexFile.Text = selected.File;
                this.tbName.Text = selected.Name;
                this.tbSeoc.Text = selected.Seoc;
                this.btnAdd.Text = "Save";
            }
        }

        private void ClearComplexSelection()
        {
            lbTemplates.SelectedIndex = -1;
            this.tbComplexArgs.Text = this.tbDiagnosis.Text = this.tbComplexFile.Text = this.tbSeoc.Text = string.Empty;
            this.tbName.Text = "LastName,FirstName";
            this.btnAdd.Text = "Add";
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            lbTemplates.Items.Remove(this.lbTemplates.SelectedItem);
            ClearComplexSelection();
        }

        private void BtnCancel_Click(object sender, EventArgs e) => ClearComplexSelection();
    }
}
