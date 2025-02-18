﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;

namespace glTF_BinExporter
{
    class ExportOptionsDialog : Dialog<DialogResult>
    {
        private const int DefaultPadding = 5;
        private static readonly Eto.Drawing.Size DefaultSpacing = new Eto.Drawing.Size(2, 2);

        private CheckBox mapZtoY = new CheckBox();
        private CheckBox exportMaterials = new CheckBox();
        private CheckBox useDisplayColorForUnsetMaterial = new CheckBox();
        private CheckBox useDoubleSidedMaterials = new CheckBox();
        private CheckBox exportLayers = new CheckBox();

        private GroupBox subdBox = new GroupBox();
        private CheckBox useSubdControlNet = new CheckBox();
        private Label subdLevelLabel = new Label();
        private Slider subdLevel = new Slider();

        private CheckBox exportTextureCoordinates = new CheckBox();
        private CheckBox exportVertexNormals = new CheckBox();
        private CheckBox exportOpenMeshes = new CheckBox();
        private CheckBox exportVertexColors = new CheckBox();

        private CheckBox useDracoCompressionCheck = new CheckBox();

        private Label dracoCompressionLabel = new Label();
        private NumericStepper dracoCompressionLevelInput = new NumericStepper();

        private Label dracoQuantizationBitsLabel = new Label();
        private NumericStepper dracoQuantizationBitsInputPosition = new NumericStepper() { DecimalPlaces = 0, MinValue = 8, MaxValue = 32 };
        private NumericStepper dracoQuantizationBitsInputNormal = new NumericStepper() { DecimalPlaces = 0, MinValue = 8, MaxValue = 32 };
        private NumericStepper dracoQuantizationBitsInputTexture = new NumericStepper() { DecimalPlaces = 0, MinValue = 8, MaxValue = 32 };
        
        private Button cancelButton = new Button();
        private Button okButton = new Button();

        private CheckBox useSettingsDontShowDialogCheck = new CheckBox();

        public ExportOptionsDialog()
        {
            Resizable = false;

            Title = "glTF Export Options";

            mapZtoY.Text = "Map Rhino Z to glTF Y";

            exportMaterials.Text = "Export materials";

            useDisplayColorForUnsetMaterial.Text = "Use display color for objects with no material set";

            useDoubleSidedMaterials.Text = "Use double sided materials";

            exportLayers.Text = "Export Layers";

            subdBox.Text = "SubD Meshing";

            useSubdControlNet.Text = "Use control net";

            subdLevelLabel.Text = "Subdivision level";

            subdLevel.SnapToTick = true;
            subdLevel.TickFrequency = 1;
            subdLevel.MinValue = 1;
            subdLevel.MaxValue = 5;
            
            subdBox.Content = new TableLayout()
            {
                Padding = DefaultPadding,
                Spacing = DefaultSpacing,
                Rows =
                {
                    new TableRow(useSubdControlNet, null),
                    new TableRow(subdLevel, subdLevelLabel),
                }
            };

            exportTextureCoordinates.Text = "Export texture coordinates";

            exportVertexNormals.Text = "Export vertex normals";

            exportOpenMeshes.Text = "Export open meshes";

            exportVertexColors.Text = "Export vertex colors";

            useDracoCompressionCheck.Text = "Use Draco compression";

            dracoCompressionLabel.Text = "Draco compression Level";
            dracoCompressionLevelInput.DecimalPlaces = 0;
            dracoCompressionLevelInput.MinValue = 1;
            dracoCompressionLevelInput.MaxValue = 10;

            dracoQuantizationBitsLabel.Text = "Quantization";

            cancelButton.Text = "Cancel";

            okButton.Text = "Ok";

            useSettingsDontShowDialogCheck.Text = "Always use these settings. Do not show this dialog again.";

            OptionsToDialog();

            useDracoCompressionCheck.CheckedChanged += UseDracoCompressionCheck_CheckedChanged;
            exportMaterials.CheckedChanged += ExportMaterials_CheckedChanged;

            useSubdControlNet.CheckedChanged += UseSubdControlNet_CheckedChanged;

            cancelButton.Click += CancelButton_Click;
            okButton.Click += OkButton_Click;

            var dracoGroupBox = new GroupBox() { Text = "Draco Quantization Bits" };
            dracoGroupBox.Content = new TableLayout()
            {
                Padding = DefaultPadding,
                Spacing = DefaultSpacing,
                Rows = {
                    new TableRow("Position", "Normal", "Texture"),
                    new TableRow(dracoQuantizationBitsInputPosition, dracoQuantizationBitsInputNormal, dracoQuantizationBitsInputTexture)
                }
            };

            var layout = new DynamicLayout()
            {
                Padding = DefaultPadding,
                Spacing = DefaultSpacing,
            };

            layout.AddSeparateRow(useDracoCompressionCheck, null);
            layout.AddSeparateRow(dracoCompressionLabel, dracoCompressionLevelInput, null);
            layout.AddSeparateRow(dracoGroupBox, null);

            TabControl tabControl = new TabControl();

            TabPage formattingPage = new TabPage()
            {
                Text = "Formatting",
                Content = new TableLayout()
                {
                    Padding = DefaultPadding,
                    Spacing = DefaultSpacing,
                    Rows =
                    {
                        new TableRow(mapZtoY),
                        new TableRow(exportMaterials),
                        new TableRow(useDisplayColorForUnsetMaterial),
                        new TableRow(useDoubleSidedMaterials),
                        new TableRow(exportLayers)
                    },
                },
            };

            tabControl.Pages.Add(formattingPage);

            TabPage meshPage = new TabPage()
            {
                Text = "Mesh",
                Content = new TableLayout()
                {
                    Padding = DefaultPadding,
                    Spacing = DefaultSpacing,
                    Rows =
                    {
                        new TableRow(subdBox),
                        new TableRow(exportTextureCoordinates),
                        new TableRow(exportVertexNormals),
                        new TableRow(exportOpenMeshes),
                        new TableRow(exportVertexColors),
                    },
                },
            };

            tabControl.Pages.Add(meshPage);

            TabPage compressionPage = new TabPage()
            {
                Text = "Compression",
                Content = layout,
            };
            
            tabControl.Pages.Add(compressionPage);

            this.Content = new TableLayout()
            {
                Padding = DefaultPadding,
                Spacing = DefaultSpacing,
                Rows =
                {
                    new TableRow(tabControl),
                    null,
                    new TableRow(useSettingsDontShowDialogCheck),
                    new TableRow(new TableLayout()
                    {
                        Padding = DefaultPadding,
                        Spacing = DefaultSpacing,
                        Rows =
                        {
                            new TableRow(new TableCell(cancelButton, true), new TableCell(okButton, true)),
                        }
                    }),
                }
            };
        }

        private void OptionsToDialog()
        {
            useSettingsDontShowDialogCheck.Checked = glTFBinExporterPlugin.UseSavedSettingsDontShowDialog;

            mapZtoY.Checked = glTFBinExporterPlugin.MapRhinoZToGltfY;
            exportMaterials.Checked = glTFBinExporterPlugin.ExportMaterials;
            EnableDisableMaterialControls(glTFBinExporterPlugin.ExportMaterials);
            exportLayers.Checked = glTFBinExporterPlugin.ExportLayers;

            useDisplayColorForUnsetMaterial.Checked = glTFBinExporterPlugin.UseDisplayColorForUnsetMaterials;
            useDoubleSidedMaterials.Checked = glTFBinExporterPlugin.UseDoubleSidedMaterials;

            bool controlNet = glTFBinExporterPlugin.SubDExportMode == SubDMode.ControlNet;
            useSubdControlNet.Checked = controlNet;
            EnabledDisableSubDLevel(!controlNet);

            subdLevel.Value = glTFBinExporterPlugin.SubDLevel;

            exportTextureCoordinates.Checked = glTFBinExporterPlugin.ExportTextureCoordinates;
            exportVertexNormals.Checked = glTFBinExporterPlugin.ExportVertexNormals;
            exportOpenMeshes.Checked = glTFBinExporterPlugin.ExportOpenMeshes;
            exportVertexColors.Checked = glTFBinExporterPlugin.ExportVertexColors;

            useDracoCompressionCheck.Checked = glTFBinExporterPlugin.UseDracoCompression;
            EnableDisableDracoControls(glTFBinExporterPlugin.UseDracoCompression);

            dracoCompressionLevelInput.Value = glTFBinExporterPlugin.DracoCompressionLevel;
            dracoQuantizationBitsInputPosition.Value = glTFBinExporterPlugin.DracoQuantizationBitsPosition;
            dracoQuantizationBitsInputNormal.Value = glTFBinExporterPlugin.DracoQuantizationBitsNormal;
            dracoQuantizationBitsInputTexture.Value = glTFBinExporterPlugin.DracoQuantizationBitsTexture;
        }

        private void DialogToOptions()
        {
            glTFBinExporterPlugin.UseSavedSettingsDontShowDialog = GetCheckboxValue(useSettingsDontShowDialogCheck);

            glTFBinExporterPlugin.MapRhinoZToGltfY = GetCheckboxValue(mapZtoY);
            glTFBinExporterPlugin.ExportMaterials = GetCheckboxValue(exportMaterials);
            glTFBinExporterPlugin.UseDisplayColorForUnsetMaterials = GetCheckboxValue(useDisplayColorForUnsetMaterial);
            glTFBinExporterPlugin.UseDoubleSidedMaterials = GetCheckboxValue(useDoubleSidedMaterials);
            glTFBinExporterPlugin.ExportLayers = GetCheckboxValue(exportLayers);

            bool controlNet = GetCheckboxValue(useSubdControlNet);
            glTFBinExporterPlugin.SubDExportMode = controlNet ? SubDMode.ControlNet : SubDMode.Surface;

            glTFBinExporterPlugin.SubDLevel = subdLevel.Value;

            glTFBinExporterPlugin.ExportTextureCoordinates = GetCheckboxValue(exportTextureCoordinates);
            glTFBinExporterPlugin.ExportVertexNormals = GetCheckboxValue(exportVertexNormals);
            glTFBinExporterPlugin.ExportOpenMeshes = GetCheckboxValue(exportOpenMeshes);
            glTFBinExporterPlugin.ExportVertexColors = GetCheckboxValue(exportVertexColors);

            glTFBinExporterPlugin.UseDracoCompression = GetCheckboxValue(useDracoCompressionCheck);
            glTFBinExporterPlugin.DracoCompressionLevel = (int)dracoCompressionLevelInput.Value;
            glTFBinExporterPlugin.DracoQuantizationBitsPosition = (int)dracoQuantizationBitsInputPosition.Value;
            glTFBinExporterPlugin.DracoQuantizationBitsNormal = (int)dracoQuantizationBitsInputNormal.Value;
            glTFBinExporterPlugin.DracoQuantizationBitsTexture = (int)dracoQuantizationBitsInputTexture.Value;
        }

        private bool GetCheckboxValue(CheckBox checkBox)
        {
            return checkBox.Checked.HasValue ? checkBox.Checked.Value : false;
        }

        private void EnabledDisableSubDLevel(bool enable)
        {
            subdLevel.Enabled = enable;
        }

        private void EnableDisableDracoControls(bool enable)
        {
            dracoCompressionLevelInput.Enabled = enable;
            dracoQuantizationBitsInputPosition.Enabled = enable;
            dracoQuantizationBitsInputNormal.Enabled = enable;
            dracoQuantizationBitsInputTexture.Enabled = enable;
        }

        private void UseDracoCompressionCheck_CheckedChanged(object sender, EventArgs e)
        {
            bool useDraco = GetCheckboxValue(useDracoCompressionCheck);

            EnableDisableDracoControls(useDraco);
        }

        private void ExportMaterials_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = GetCheckboxValue(exportMaterials);

            EnableDisableMaterialControls(enabled);
        }

        private void EnableDisableMaterialControls(bool enabled)
        {
            useDisplayColorForUnsetMaterial.Enabled = enabled;
            useDoubleSidedMaterials.Enabled = enabled;
        }

        private void UseSubdControlNet_CheckedChanged(object sender, EventArgs e)
        {
            bool controlNet = GetCheckboxValue(useSubdControlNet);
            EnabledDisableSubDLevel(!controlNet);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogToOptions();

            this.Close(DialogResult.Ok);
        }
    }
}
