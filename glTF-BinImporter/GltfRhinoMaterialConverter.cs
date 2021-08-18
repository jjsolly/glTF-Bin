﻿using Rhino.Render;
using Rhino.Render.ChildSlotNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glTF_BinImporter
{
    class GltfRhinoMaterialConverter
    {
        public GltfRhinoMaterialConverter(glTFLoader.Schema.Gltf gltf, glTFLoader.Schema.Material material, Rhino.RhinoDoc doc, GltfRhinoConverter converter)
        {
            this.gltf = gltf;
            this.material = material;
            this.doc = doc;
            this.converter = converter;
        }

        glTFLoader.Schema.Gltf gltf = null;
        glTFLoader.Schema.Material material = null;
        Rhino.RhinoDoc doc = null;
        GltfRhinoConverter converter = null;

        public Rhino.Render.RenderMaterial Convert()
        {
            RenderMaterial pbr = RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, doc) as RenderMaterial;

            pbr.BeginChange(RenderContent.ChangeContexts.Program);

            pbr.Name = converter.GetUniqueName(material.Name);

            if (material.PbrMetallicRoughness != null)
            {
                Rhino.Display.Color4f baseColor = material.PbrMetallicRoughness.BaseColorFactor.ToColor4f();

                baseColor = GltfUtils.UnapplyGamma(baseColor);

                pbr.Fields.Set(PhysicallyBased.BaseColor, baseColor);

                if(material.PbrMetallicRoughness.BaseColorTexture != null)
                {
                    int index = material.PbrMetallicRoughness.BaseColorTexture.Index;

                    RenderTexture texture = converter.GetRenderTexture(index);

                    pbr.SetChild(texture, Rhino.Render.ParameterNames.PhysicallyBased.BaseColor);
                    pbr.SetChildSlotOn(Rhino.Render.ParameterNames.PhysicallyBased.BaseColor, true, RenderContent.ChangeContexts.Program);
                }

                double roughness = material.PbrMetallicRoughness.RoughnessFactor;

                pbr.Fields.Set(PhysicallyBased.Roughness, roughness);

                double metallic = material.PbrMetallicRoughness.MetallicFactor;

                pbr.Fields.Set(PhysicallyBased.Metallic, metallic);

                if(material.PbrMetallicRoughness.MetallicRoughnessTexture != null)
                {
                    int index = material.PbrMetallicRoughness.MetallicRoughnessTexture.Index;

                    RhinoGltfMetallicRoughnessConverter metallicRoughness = converter.GetMetallicRoughnessTexture(index);

                    if(metallicRoughness.HasMetalness)
                    {
                        pbr.SetChild(metallicRoughness.MetallicTexture, PhysicallyBased.Metallic);
                        pbr.SetChildSlotOn(PhysicallyBased.Metallic, true, RenderContent.ChangeContexts.Program);
                    }

                    if(metallicRoughness.HasRoughness)
                    {
                        pbr.SetChild(metallicRoughness.RoughnessTexture, PhysicallyBased.Roughness);
                        pbr.SetChildSlotOn(PhysicallyBased.Roughness, true, RenderContent.ChangeContexts.Program);
                    }
                }
            }

            Rhino.Display.Color4f emissionColor = material.EmissiveFactor.ToColor4f();

            emissionColor = GltfUtils.UnapplyGamma(emissionColor);

            pbr.Fields.Set(PhysicallyBased.Emission, emissionColor);

            if (material.EmissiveTexture != null)
            {
                RenderTexture emissiveTexture = converter.GetRenderTexture(material.EmissiveTexture.Index);

                pbr.SetChild(emissiveTexture, PhysicallyBased.Emission);
                pbr.SetChildSlotOn(PhysicallyBased.Emission, true, RenderContent.ChangeContexts.Program);
            }

            if(material.OcclusionTexture != null)
            {
                RenderTexture occlusionTexture = converter.GetRenderTexture(material.OcclusionTexture.Index);

                pbr.SetChild(occlusionTexture, PhysicallyBased.AmbientOcclusion);
                pbr.SetChildSlotOn(PhysicallyBased.AmbientOcclusion, true, RenderContent.ChangeContexts.Program);
            }

            if(material.NormalTexture != null)
            {
                RenderTexture normalTexture = converter.GetRenderTexture(material.NormalTexture.Index);

                pbr.SetChild(normalTexture, PhysicallyBased.Bump);
                pbr.SetChildSlotOn(PhysicallyBased.Bump, true, RenderContent.ChangeContexts.Program);
            }

            if(material.Extensions != null)
            {
                if(material.Extensions.TryGetValue("KHR_materials_clearcoat", out object clearcoatValue))
                {
                    HandleClearcoat(clearcoatValue.ToString(), pbr);
                }

                if(material.Extensions.TryGetValue("KHR_materials_transmission", out object transmissionValue))
                {
                    HandleTransmission(transmissionValue.ToString(), pbr);
                }
            }

            pbr.EndChange();

            doc.RenderMaterials.BeginChange(RenderContent.ChangeContexts.Program);

            doc.RenderMaterials.Add(pbr);

            doc.RenderMaterials.EndChange();

            return pbr;
        }

        void HandleClearcoat(string text, RenderMaterial pbr)
        {
            glTFExtensions.KHR_materials_clearcoat clearcoat = Newtonsoft.Json.JsonConvert.DeserializeObject<glTFExtensions.KHR_materials_clearcoat>(text);

            if (clearcoat == null)
            {
                return;
            }

            if(clearcoat.ClearcoatTexture != null)
            {
                RenderTexture clearcoatTexture = converter.GetRenderTexture(clearcoat.ClearcoatTexture.Index);

                pbr.SetChild(clearcoatTexture, PhysicallyBased.Clearcoat);
                pbr.SetChildSlotOn(PhysicallyBased.Clearcoat, true, RenderContent.ChangeContexts.Program);
            }

            pbr.Fields.Set(PhysicallyBased.Clearcoat, clearcoat.ClearcoatFactor);

            if(clearcoat.ClearcoatRoughnessTexture != null)
            {
                RenderTexture clearcoatRoughnessTexture = converter.GetRenderTexture(clearcoat.ClearcoatRoughnessTexture.Index);

                pbr.SetChild(clearcoatRoughnessTexture, PhysicallyBased.ClearcoatRoughness);
                pbr.SetChildSlotOn(PhysicallyBased.ClearcoatRoughness, true, RenderContent.ChangeContexts.Program);
            }

            pbr.Fields.Set(PhysicallyBased.ClearcoatRoughness, clearcoat.ClearcoatRoughnessFactor);

            if(clearcoat.ClearcoatNormalTexture != null)
            {
                RenderTexture clearcoatNormalTexture = converter.GetRenderTexture(clearcoat.ClearcoatNormalTexture.Index);

                pbr.SetChild(clearcoatNormalTexture, PhysicallyBased.ClearcoatBump);
                pbr.SetChildSlotOn(PhysicallyBased.ClearcoatBump, true, RenderContent.ChangeContexts.Program);
            }
        }

        void HandleTransmission(string text, RenderMaterial pbr)
        {
            glTFExtensions.KHR_materials_transmission transmission = Newtonsoft.Json.JsonConvert.DeserializeObject<glTFExtensions.KHR_materials_transmission>(text);

            if (transmission != null)
            {
                return;
            }

            if(transmission.TransmissionTexture != null)
            {
                RenderTexture transmissionTexture = converter.GetRenderTexture(transmission.TransmissionTexture.Index);

                pbr.SetChild(transmissionTexture, PhysicallyBased.Opacity);
                pbr.SetChildSlotOn(PhysicallyBased.Opacity, true, RenderContent.ChangeContexts.Program);
            }

            pbr.Fields.Set(PhysicallyBased.Opacity, transmission.TransmissionFactor);
        }

    }
}
