using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using WebSocketSharp;
using System.Text.Json;
using System.Text;

namespace MNML
{
    // @TODO may deprecate in favour of WebsocketReceive component.

    public class GhReceiveParameterComponent : GH_Component
    {
        
        string latestMessage = null;
        WebSocket lastSocket = null;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhReceiveParameterComponent()
          : base("Receive Update", "Receive Update",
            "Receive updates from GUI",
            "MNML", "Communication")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket", "S", "Socket to receive events", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            WebSocket socket = null;
            if (!DA.GetData(0, ref socket)) return;
            lastSocket = socket;
            lastSocket.OnMessage -= Socket_OnMessage;
            lastSocket.OnMessage += Socket_OnMessage;
        }


        private void ScheduleCallback(GH_Document document)
        {

            if (latestMessage == null) return;

            var data = JsonSerializer.Deserialize<UpdateMessage>(latestMessage);
            var doc = OnPingDocument();
            var guid = new Guid(data.guid);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, latestMessage);

            foreach (IGH_DocumentObject docObject in doc.Objects)
            {
                if (guid == docObject.InstanceGuid)
                {
                    var slider = docObject as Grasshopper.Kernel.Special.GH_NumberSlider;
                    slider.SetSliderValue((decimal)data.value);
                    slider.ExpireSolution(false);
                    break;
                }
            }

            latestMessage = null;

            //ExpireSolution(false);
        }


        class UpdateMessage
        {
            public string guid { get; set; }
            public float value { get; set; }
        }


        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            latestMessage = e.Data;
            OnPingDocument().ScheduleSolution(5, ScheduleCallback);
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
            get { return new Guid("4126b380-ebca-49dc-aa4b-adde4f9bfe69"); }
        }
    }
}
