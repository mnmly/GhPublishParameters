using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;

using System.Text.Json;
using System.Text;

namespace MNML
{
    public class GhRetrieveGUIDComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhRetrieveGUIDComponent()
          : base("Retrieve GUID", "Retrieve GUID",
            "Retrieve GUID of Inputs",
            "MNML", "Primitive")
        {
            Params.ParameterSourcesChanged += ParamSourcesChanged;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input 1", "I1", "General Inputs", GH_ParamAccess.list);
            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddGenericParameter("GUIDs", "GUIDs", "List of GUID of inputs", GH_ParamAccess.list);
            pManager.AddTextParameter("JSON String", "JSON", "List of Parameters", GH_ParamAccess.list);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Guid> guidList = new List<Guid>();

            var options = new JsonWriterOptions
            {
                Indented = true
            };


            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("action", "mnml:parameters");
                    writer.WritePropertyName("data");

                    writer.WriteStartArray();
                    foreach (var input in Params.Input)
                    {
                        if (!input.Sources.Any()) continue;
                        foreach (var source in input.Sources)
                        {
                            if (source is GH_NumberSlider)
                            {
                                var slider = source as GH_NumberSlider;
                                
                                var min = slider.Slider.Minimum;
                                var max = slider.Slider.Maximum;
                                var step = slider.Slider.SnapDistance;
                                var name = slider.NickName == "" ? slider.Name : slider.NickName;
                                var guid = source.InstanceGuid;
                                writer.WriteStartObject();
                                writer.WriteNumber("step", step);
                                writer.WriteNumber("min", min);
                                writer.WriteNumber("max", max);
                                writer.WriteString("name", name);
                                writer.WriteNumber("value", slider.Slider.Value);
                                writer.WriteString("guid", guid.ToString());
                                writer.WriteEndObject();
                                guidList.Add(guid);
                            }
                        }
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();

                }
                string json = Encoding.UTF8.GetString(stream.ToArray());
                DA.SetData(1, json);
            }

            if (!Hidden)
            {
                DA.SetDataList(0, guidList);
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            //leave two inputs
            if (side == GH_ParameterSide.Input)
            {
                if (Params.Input.Count > 1)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject param = new Param_GenericObject();
            int num = index + 1;
            param.Name = "Input " + num;
            param.NickName = "I" + num;
            param.Description = "Input " + num;
            param.Access = GH_ParamAccess.list;
            param.Optional = true;
            for (int i = 0; i < Params.Input.Count; i++)
            {
                var n = (i + (index <= i ? 2 : 1));
                Params.Input[i].Name = "Input " + n;
                Params.Input[i].NickName = "I" + n;

            }
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                for (int i = 0; i < Params.Input.Count; i++)
                {
                    var n = (i + (index <= i ? 0 : 1));
                    Params.Input[i].Name = "Input " + n;
                    Params.Input[i].NickName = "I" + n;

                }
                return true;
            } else
            {
                return false;
            }
        }

        public void VariableParameterMaintenance()
        {
            //throw new NotImplementedException();
        }

        //This is for if any source connected, reconnected, removed, replacement 
        private void ParamSourcesChanged(Object sender, GH_ParamServerEventArgs e)
        {

            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input ? true : false;

            //check input side only
            if (!isInputSide) return;

            bool isLastSourceFull = Params.Input[Params.Input.Count - 1].Sources.Any();

            // add a new input param while the second last input is full
            if (isLastSourceFull)
            {
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count);
                Params.RegisterInputParam(newParam, Params.Input.Count);
                VariableParameterMaintenance();
                Params.OnParametersChanged();
            }
        }


        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5A42678A-3ADD-4A5A-AAE5-D8F0728D7344");


    }
}