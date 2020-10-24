namespace Hl7BatchFileGenerator
{
    partial class frmHl7Generator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lIdsStart = new System.Windows.Forms.Label();
            this.lSiteId = new System.Windows.Forms.Label();
            this.lRefIds = new System.Windows.Forms.Label();
            this.lFile = new System.Windows.Forms.Label();
            this.tbSimpleFileName = new System.Windows.Forms.TextBox();
            this.lServer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbServer = new System.Windows.Forms.TextBox();
            this.lbNumToGen = new System.Windows.Forms.Label();
            this.nudNumToGenerate = new System.Windows.Forms.NumericUpDown();
            this.lAdditionalArgs = new System.Windows.Forms.Label();
            this.tbSimpleArgs = new System.Windows.Forms.TextBox();
            this.lOutputFile = new System.Windows.Forms.Label();
            this.tbOutputFile = new System.Windows.Forms.TextBox();
            this.nudIdsStart = new System.Windows.Forms.NumericUpDown();
            this.nudRefsStart = new System.Windows.Forms.NumericUpDown();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.nudSiteId = new System.Windows.Forms.NumericUpDown();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.tcSubmitType = new System.Windows.Forms.TabControl();
            this.tpSimple = new System.Windows.Forms.TabPage();
            this.tbSimpleName = new System.Windows.Forms.TextBox();
            this.lSimpleVetName = new System.Windows.Forms.Label();
            this.tpComplex = new System.Windows.Forms.TabPage();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lbTemplates = new System.Windows.Forms.ListBox();
            this.tbSeoc = new System.Windows.Forms.TextBox();
            this.tbDiagnosis = new System.Windows.Forms.TextBox();
            this.tbComplexArgs = new System.Windows.Forms.TextBox();
            this.tbComplexFile = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lSeoc = new System.Windows.Forms.Label();
            this.lDiagnosis = new System.Windows.Forms.Label();
            this.lComplexArgs = new System.Windows.Forms.Label();
            this.lComplexHl7 = new System.Windows.Forms.Label();
            this.lComplexVetName = new System.Windows.Forms.Label();
            this.lTemplates = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumToGenerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIdsStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefsStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSiteId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            this.tcSubmitType.SuspendLayout();
            this.tpSimple.SuspendLayout();
            this.tpComplex.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(9, 409);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(147, 28);
            this.btnGenerate.TabIndex = 10;
            this.btnGenerate.Text = "Generate Batch File";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.BtnGenerate_Click);
            // 
            // lIdsStart
            // 
            this.lIdsStart.AutoSize = true;
            this.lIdsStart.Location = new System.Drawing.Point(9, 90);
            this.lIdsStart.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lIdsStart.Name = "lIdsStart";
            this.lIdsStart.Size = new System.Drawing.Size(134, 17);
            this.lIdsStart.TabIndex = 999;
            this.lIdsStart.Text = "ICN-Vet IDs Start at:";
            // 
            // lSiteId
            // 
            this.lSiteId.AutoSize = true;
            this.lSiteId.Location = new System.Drawing.Point(9, 124);
            this.lSiteId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lSiteId.Name = "lSiteId";
            this.lSiteId.Size = new System.Drawing.Size(74, 17);
            this.lSiteId.TabIndex = 999;
            this.lSiteId.Text = "Consult Id:";
            // 
            // lRefIds
            // 
            this.lRefIds.AutoSize = true;
            this.lRefIds.Location = new System.Drawing.Point(9, 159);
            this.lRefIds.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lRefIds.Name = "lRefIds";
            this.lRefIds.Size = new System.Drawing.Size(106, 17);
            this.lRefIds.TabIndex = 999;
            this.lRefIds.Text = "Ref Ids Start at:";
            // 
            // lFile
            // 
            this.lFile.AutoSize = true;
            this.lFile.Location = new System.Drawing.Point(20, 15);
            this.lFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lFile.Name = "lFile";
            this.lFile.Size = new System.Drawing.Size(64, 17);
            this.lFile.TabIndex = 999;
            this.lFile.Text = "HL7 File:";
            // 
            // tbSimpleFileName
            // 
            this.tbSimpleFileName.Location = new System.Drawing.Point(181, 11);
            this.tbSimpleFileName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbSimpleFileName.MaxLength = 2000;
            this.tbSimpleFileName.Name = "tbSimpleFileName";
            this.tbSimpleFileName.Size = new System.Drawing.Size(320, 22);
            this.tbSimpleFileName.TabIndex = 30;
            // 
            // lServer
            // 
            this.lServer.AutoSize = true;
            this.lServer.Location = new System.Drawing.Point(9, 21);
            this.lServer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lServer.Name = "lServer";
            this.lServer.Size = new System.Drawing.Size(100, 17);
            this.lServer.TabIndex = 999;
            this.lServer.Text = "Target Server:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 55);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 999;
            this.label1.Text = "Target Port:";
            // 
            // tbServer
            // 
            this.tbServer.Location = new System.Drawing.Point(167, 17);
            this.tbServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbServer.MaxLength = 2000;
            this.tbServer.Name = "tbServer";
            this.tbServer.Size = new System.Drawing.Size(159, 22);
            this.tbServer.TabIndex = 2;
            // 
            // lbNumToGen
            // 
            this.lbNumToGen.AutoSize = true;
            this.lbNumToGen.Location = new System.Drawing.Point(9, 192);
            this.lbNumToGen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbNumToGen.Name = "lbNumToGen";
            this.lbNumToGen.Size = new System.Drawing.Size(147, 17);
            this.lbNumToGen.TabIndex = 999;
            this.lbNumToGen.Text = "Number To Generate:";
            // 
            // nudNumToGenerate
            // 
            this.nudNumToGenerate.Location = new System.Drawing.Point(167, 190);
            this.nudNumToGenerate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudNumToGenerate.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudNumToGenerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNumToGenerate.Name = "nudNumToGenerate";
            this.nudNumToGenerate.Size = new System.Drawing.Size(160, 22);
            this.nudNumToGenerate.TabIndex = 7;
            this.nudNumToGenerate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lAdditionalArgs
            // 
            this.lAdditionalArgs.AutoSize = true;
            this.lAdditionalArgs.Location = new System.Drawing.Point(20, 82);
            this.lAdditionalArgs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lAdditionalArgs.Name = "lAdditionalArgs";
            this.lAdditionalArgs.Size = new System.Drawing.Size(146, 17);
            this.lAdditionalArgs.TabIndex = 999;
            this.lAdditionalArgs.Text = "Additional Arguments:";
            // 
            // tbSimpleArgs
            // 
            this.tbSimpleArgs.Location = new System.Drawing.Point(181, 75);
            this.tbSimpleArgs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbSimpleArgs.Name = "tbSimpleArgs";
            this.tbSimpleArgs.Size = new System.Drawing.Size(320, 22);
            this.tbSimpleArgs.TabIndex = 32;
            // 
            // lOutputFile
            // 
            this.lOutputFile.AutoSize = true;
            this.lOutputFile.Location = new System.Drawing.Point(9, 225);
            this.lOutputFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lOutputFile.Name = "lOutputFile";
            this.lOutputFile.Size = new System.Drawing.Size(81, 17);
            this.lOutputFile.TabIndex = 999;
            this.lOutputFile.Text = "Output File:";
            // 
            // tbOutputFile
            // 
            this.tbOutputFile.Location = new System.Drawing.Point(167, 222);
            this.tbOutputFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbOutputFile.Name = "tbOutputFile";
            this.tbOutputFile.Size = new System.Drawing.Size(159, 22);
            this.tbOutputFile.TabIndex = 9;
            // 
            // nudIdsStart
            // 
            this.nudIdsStart.Location = new System.Drawing.Point(167, 87);
            this.nudIdsStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudIdsStart.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudIdsStart.Name = "nudIdsStart";
            this.nudIdsStart.Size = new System.Drawing.Size(160, 22);
            this.nudIdsStart.TabIndex = 4;
            // 
            // nudRefsStart
            // 
            this.nudRefsStart.Location = new System.Drawing.Point(167, 156);
            this.nudRefsStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudRefsStart.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudRefsStart.Name = "nudRefsStart";
            this.nudRefsStart.Size = new System.Drawing.Size(160, 22);
            this.nudRefsStart.TabIndex = 6;
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(13, 446);
            this.tbOutput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ReadOnly = true;
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbOutput.Size = new System.Drawing.Size(917, 93);
            this.tbOutput.TabIndex = 999;
            // 
            // nudSiteId
            // 
            this.nudSiteId.Location = new System.Drawing.Point(167, 122);
            this.nudSiteId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudSiteId.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudSiteId.Name = "nudSiteId";
            this.nudSiteId.Size = new System.Drawing.Size(160, 22);
            this.nudSiteId.TabIndex = 5;
            // 
            // nudPort
            // 
            this.nudPort.Location = new System.Drawing.Point(167, 53);
            this.nudPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(160, 22);
            this.nudPort.TabIndex = 3;
            // 
            // tcSubmitType
            // 
            this.tcSubmitType.Controls.Add(this.tpSimple);
            this.tcSubmitType.Controls.Add(this.tpComplex);
            this.tcSubmitType.Location = new System.Drawing.Point(353, 15);
            this.tcSubmitType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tcSubmitType.Name = "tcSubmitType";
            this.tcSubmitType.SelectedIndex = 0;
            this.tcSubmitType.Size = new System.Drawing.Size(584, 390);
            this.tcSubmitType.TabIndex = 1000;
            // 
            // tpSimple
            // 
            this.tpSimple.Controls.Add(this.tbSimpleName);
            this.tpSimple.Controls.Add(this.lSimpleVetName);
            this.tpSimple.Controls.Add(this.tbSimpleFileName);
            this.tpSimple.Controls.Add(this.lFile);
            this.tpSimple.Controls.Add(this.lAdditionalArgs);
            this.tpSimple.Controls.Add(this.tbSimpleArgs);
            this.tpSimple.Location = new System.Drawing.Point(4, 25);
            this.tpSimple.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpSimple.Name = "tpSimple";
            this.tpSimple.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpSimple.Size = new System.Drawing.Size(576, 361);
            this.tpSimple.TabIndex = 0;
            this.tpSimple.Text = "Simple";
            this.tpSimple.UseVisualStyleBackColor = true;
            // 
            // tbSimpleName
            // 
            this.tbSimpleName.Location = new System.Drawing.Point(181, 43);
            this.tbSimpleName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbSimpleName.Name = "tbSimpleName";
            this.tbSimpleName.Size = new System.Drawing.Size(320, 22);
            this.tbSimpleName.TabIndex = 31;
            this.tbSimpleName.Text = "LastName,FirstName";
            // 
            // lSimpleVetName
            // 
            this.lSimpleVetName.AutoSize = true;
            this.lSimpleVetName.Location = new System.Drawing.Point(20, 48);
            this.lSimpleVetName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lSimpleVetName.Name = "lSimpleVetName";
            this.lSimpleVetName.Size = new System.Drawing.Size(74, 17);
            this.lSimpleVetName.TabIndex = 999;
            this.lSimpleVetName.Text = "Vet Name:";
            // 
            // tpComplex
            // 
            this.tpComplex.Controls.Add(this.btnCancel);
            this.tpComplex.Controls.Add(this.btnRemove);
            this.tpComplex.Controls.Add(this.btnAdd);
            this.tpComplex.Controls.Add(this.lbTemplates);
            this.tpComplex.Controls.Add(this.tbSeoc);
            this.tpComplex.Controls.Add(this.tbDiagnosis);
            this.tpComplex.Controls.Add(this.tbComplexArgs);
            this.tpComplex.Controls.Add(this.tbComplexFile);
            this.tpComplex.Controls.Add(this.tbName);
            this.tpComplex.Controls.Add(this.lSeoc);
            this.tpComplex.Controls.Add(this.lDiagnosis);
            this.tpComplex.Controls.Add(this.lComplexArgs);
            this.tpComplex.Controls.Add(this.lComplexHl7);
            this.tpComplex.Controls.Add(this.lComplexVetName);
            this.tpComplex.Controls.Add(this.lTemplates);
            this.tpComplex.Location = new System.Drawing.Point(4, 25);
            this.tpComplex.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpComplex.Name = "tpComplex";
            this.tpComplex.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpComplex.Size = new System.Drawing.Size(576, 361);
            this.tpComplex.TabIndex = 1;
            this.tpComplex.Text = "Complex";
            this.tpComplex.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(441, 279);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 56;
            this.btnCancel.Text = "Cancel Edit";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(441, 34);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 28);
            this.btnRemove.TabIndex = 58;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(441, 318);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 28);
            this.btnAdd.TabIndex = 57;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // lbTemplates
            // 
            this.lbTemplates.FormattingEnabled = true;
            this.lbTemplates.HorizontalScrollbar = true;
            this.lbTemplates.ItemHeight = 16;
            this.lbTemplates.Location = new System.Drawing.Point(12, 32);
            this.lbTemplates.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lbTemplates.Name = "lbTemplates";
            this.lbTemplates.Size = new System.Drawing.Size(420, 132);
            this.lbTemplates.TabIndex = 50;
            this.lbTemplates.SelectedIndexChanged += new System.EventHandler(this.LbTemplates_SelectedIndexChanged);
            // 
            // tbSeoc
            // 
            this.tbSeoc.Location = new System.Drawing.Point(144, 279);
            this.tbSeoc.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbSeoc.Name = "tbSeoc";
            this.tbSeoc.Size = new System.Drawing.Size(288, 22);
            this.tbSeoc.TabIndex = 54;
            // 
            // tbDiagnosis
            // 
            this.tbDiagnosis.Location = new System.Drawing.Point(144, 247);
            this.tbDiagnosis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbDiagnosis.Name = "tbDiagnosis";
            this.tbDiagnosis.Size = new System.Drawing.Size(288, 22);
            this.tbDiagnosis.TabIndex = 53;
            // 
            // tbComplexArgs
            // 
            this.tbComplexArgs.Location = new System.Drawing.Point(144, 311);
            this.tbComplexArgs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbComplexArgs.Name = "tbComplexArgs";
            this.tbComplexArgs.Size = new System.Drawing.Size(288, 22);
            this.tbComplexArgs.TabIndex = 55;
            // 
            // tbComplexFile
            // 
            this.tbComplexFile.Location = new System.Drawing.Point(144, 215);
            this.tbComplexFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbComplexFile.Name = "tbComplexFile";
            this.tbComplexFile.Size = new System.Drawing.Size(288, 22);
            this.tbComplexFile.TabIndex = 52;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(144, 183);
            this.tbName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(288, 22);
            this.tbName.TabIndex = 51;
            this.tbName.Text = "LastName,FirstName";
            // 
            // lSeoc
            // 
            this.lSeoc.AutoSize = true;
            this.lSeoc.Location = new System.Drawing.Point(13, 283);
            this.lSeoc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lSeoc.Name = "lSeoc";
            this.lSeoc.Size = new System.Drawing.Size(50, 17);
            this.lSeoc.TabIndex = 999;
            this.lSeoc.Text = "SEOC:";
            // 
            // lDiagnosis
            // 
            this.lDiagnosis.AutoSize = true;
            this.lDiagnosis.Location = new System.Drawing.Point(13, 251);
            this.lDiagnosis.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lDiagnosis.Name = "lDiagnosis";
            this.lDiagnosis.Size = new System.Drawing.Size(74, 17);
            this.lDiagnosis.TabIndex = 999;
            this.lDiagnosis.Text = "Diagnosis:";
            // 
            // lComplexArgs
            // 
            this.lComplexArgs.AutoSize = true;
            this.lComplexArgs.Location = new System.Drawing.Point(13, 315);
            this.lComplexArgs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lComplexArgs.Name = "lComplexArgs";
            this.lComplexArgs.Size = new System.Drawing.Size(113, 17);
            this.lComplexArgs.TabIndex = 999;
            this.lComplexArgs.Text = "Add. Arguments:";
            // 
            // lComplexHl7
            // 
            this.lComplexHl7.AutoSize = true;
            this.lComplexHl7.Location = new System.Drawing.Point(12, 219);
            this.lComplexHl7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lComplexHl7.Name = "lComplexHl7";
            this.lComplexHl7.Size = new System.Drawing.Size(64, 17);
            this.lComplexHl7.TabIndex = 999;
            this.lComplexHl7.Text = "HL7 File:";
            // 
            // lComplexVetName
            // 
            this.lComplexVetName.AutoSize = true;
            this.lComplexVetName.Location = new System.Drawing.Point(13, 187);
            this.lComplexVetName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lComplexVetName.Name = "lComplexVetName";
            this.lComplexVetName.Size = new System.Drawing.Size(74, 17);
            this.lComplexVetName.TabIndex = 999;
            this.lComplexVetName.Text = "Vet Name:";
            // 
            // lTemplates
            // 
            this.lTemplates.AutoSize = true;
            this.lTemplates.Location = new System.Drawing.Point(8, 11);
            this.lTemplates.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lTemplates.Name = "lTemplates";
            this.lTemplates.Size = new System.Drawing.Size(78, 17);
            this.lTemplates.TabIndex = 999;
            this.lTemplates.Text = "Templates:";
            // 
            // frmHl7Generator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 554);
            this.Controls.Add(this.tcSubmitType);
            this.Controls.Add(this.nudPort);
            this.Controls.Add(this.nudSiteId);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.nudRefsStart);
            this.Controls.Add(this.nudIdsStart);
            this.Controls.Add(this.tbOutputFile);
            this.Controls.Add(this.lOutputFile);
            this.Controls.Add(this.nudNumToGenerate);
            this.Controls.Add(this.lbNumToGen);
            this.Controls.Add(this.tbServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lServer);
            this.Controls.Add(this.lRefIds);
            this.Controls.Add(this.lSiteId);
            this.Controls.Add(this.lIdsStart);
            this.Controls.Add(this.btnGenerate);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmHl7Generator";
            this.Text = "HL7 Batch File Generator";
            ((System.ComponentModel.ISupportInitialize)(this.nudNumToGenerate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIdsStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefsStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSiteId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            this.tcSubmitType.ResumeLayout(false);
            this.tpSimple.ResumeLayout(false);
            this.tpSimple.PerformLayout();
            this.tpComplex.ResumeLayout(false);
            this.tpComplex.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lIdsStart;
        private System.Windows.Forms.Label lSiteId;
        private System.Windows.Forms.Label lRefIds;
        private System.Windows.Forms.Label lFile;
        private System.Windows.Forms.TextBox tbSimpleFileName;
        private System.Windows.Forms.Label lServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbServer;
        private System.Windows.Forms.Label lbNumToGen;
        private System.Windows.Forms.NumericUpDown nudNumToGenerate;
        private System.Windows.Forms.Label lAdditionalArgs;
        private System.Windows.Forms.TextBox tbSimpleArgs;
        private System.Windows.Forms.Label lOutputFile;
        private System.Windows.Forms.TextBox tbOutputFile;
        private System.Windows.Forms.NumericUpDown nudIdsStart;
        private System.Windows.Forms.NumericUpDown nudRefsStart;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.NumericUpDown nudSiteId;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.TabControl tcSubmitType;
        private System.Windows.Forms.TabPage tpSimple;
        private System.Windows.Forms.TabPage tpComplex;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListBox lbTemplates;
        private System.Windows.Forms.TextBox tbSeoc;
        private System.Windows.Forms.TextBox tbDiagnosis;
        private System.Windows.Forms.TextBox tbComplexArgs;
        private System.Windows.Forms.TextBox tbComplexFile;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label lSeoc;
        private System.Windows.Forms.Label lDiagnosis;
        private System.Windows.Forms.Label lComplexArgs;
        private System.Windows.Forms.Label lComplexHl7;
        private System.Windows.Forms.Label lComplexVetName;
        private System.Windows.Forms.Label lTemplates;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbSimpleName;
        private System.Windows.Forms.Label lSimpleVetName;
    }
}

