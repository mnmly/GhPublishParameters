using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Text.Json;
using System.Text;
using MNML;
using System.Timers;

namespace GhPublishParameters
{

    public class UpdateMessage
    {
        public string guid { get; set; }
        public float value { get; set; }
    }


    public class GhUpdateSliderComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        ///
        Timer timer = null;
        bool isUpdating = false;
        string prevJSONString = null;

        public GhUpdateSliderComponent()
          : base("Update Slider Value", "UpdateSlider",
            "GhUpdateSliderComponent description",
            "MNML", "Communication")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "JSON Object containing the change", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Updated", "U", "Flag to indicate the slider updates", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jsonString = null;
            bool prevIsUpdating = isUpdating;

            if (!DA.GetData(0, ref jsonString)) return;

            if (prevJSONString == jsonString)
            {
                DA.SetData(0, isUpdating);
                return;
            }
            prevJSONString = jsonString;
            var data = JsonSerializer.Deserialize<UpdateMessage>(jsonString);
            var doc = OnPingDocument();
            var guid = new Guid(data.guid);
            GH_NumberSlider sliderToExpire = null;

            foreach (IGH_DocumentObject docObject in doc.Objects)
            {
                if (guid == docObject.InstanceGuid)
                {
                    sliderToExpire = docObject as GH_NumberSlider;
                    break;
                }
            }

            if (sliderToExpire != null)
            {
                isUpdating = true;
                doc.ScheduleSolution(5, (GH_Document _doc) =>
                {
                    sliderToExpire.SetSliderValue((decimal)data.value);
                    sliderToExpire.ExpireSolution(false);
                });


                if (timer != null)
                {
                    timer.Stop();
                    timer.Elapsed -= Timer_Elapsed;
                    timer.Dispose();
                }

                timer = new Timer() { Interval = 300 };
                timer.AutoReset = false;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }

            DA.SetData(0, isUpdating);

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnPingDocument()?.ScheduleSolution(5, (GH_Document _doc) =>
            {
                isUpdating = false;
                ExpireSolution(false);
            });
        }

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
        public override Guid ComponentGuid
        {
            get { return new Guid("4c607bd7-f498-4949-916a-9410b1b5cbbb"); }
        }
    }
}
